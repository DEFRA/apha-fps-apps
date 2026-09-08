using Apha.Common.Constants;
using Apha.Common.Utilities.Email;
using Apha.FPS.Application.Common.BulkRates;
using Apha.FPS.Application.Email;
using Apha.FPS.Application.Enums;
using Apha.FPS.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Sends Bulk Rates lifecycle notification emails via <see cref="IGraphEmailService"/>.
    /// Events owned by the Worker (Approved, Completed, Failed) are logged but not emailed.
    /// </summary>
    public class GraphEmailBulkRatesNotificationService : IBulkRatesNotificationService
    {
        private readonly IGraphEmailService _emailService;
        private readonly BulkRatesEmailSettings _settings;
        private readonly ILogger<GraphEmailBulkRatesNotificationService> _logger;

        public GraphEmailBulkRatesNotificationService(
            IGraphEmailService emailService,
            IOptions<BulkRatesEmailSettings> settings,
            ILogger<GraphEmailBulkRatesNotificationService> logger)
        {
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _settings     = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger       = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task NotifyAsync(
            BulkRatesNotificationEvent notificationEvent,
            BulkRatesNotificationContext context,
            CancellationToken ct = default)
        {
            switch (notificationEvent)
            {
                case BulkRatesNotificationEvent.ReleasedForApproval:
                    await SendReleasedForApprovalAsync(context, ct);
                    break;

                case BulkRatesNotificationEvent.Approved:
                    await SendApprovedAsync(context, ct);
                    break;

                case BulkRatesNotificationEvent.Rejected:
                    await SendRejectedAsync(context, ct);
                    break;

                case BulkRatesNotificationEvent.Cancelled:
                    await SendCancelledAsync(context, ct);
                    break;

                default:
                    _logger.LogInformation(
                        "[BulkRatesNotification] Event {Event} is not handled by this service (Worker-owned). JobQueueId={JobQueueId}",
                        notificationEvent, context.JobQueueId);
                    break;
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private async Task SendApprovedAsync(BulkRatesNotificationContext ctx, CancellationToken ct)
        {
            var recipients = ParseRecipients(_settings.ApprovedRecipients);
            if (recipients.Count == 0)
            {
                _logger.LogWarning(
                    "[BulkRatesNotification] Approved email skipped: no recipients configured. JobQueueId={JobQueueId}",
                    ctx.JobQueueId);
                return;
            }

            var (subject, body) = BuildBulkRatesEmail(ctx.JobName, BulkRatesNotificationEvent.Approved);

            await _emailService.SendEmailAsync(new()
            {
                To         = recipients,
                Subject    = subject,
                Body       = body,
                IsBodyHtml = false
            }, ct);

            _logger.LogInformation(
                "[BulkRatesNotification] Approved email sent. JobQueueId={JobQueueId} Recipients={Count}",
                ctx.JobQueueId, recipients.Count);
        }

        private async Task SendReleasedForApprovalAsync(BulkRatesNotificationContext ctx, CancellationToken ct)
        {
            var recipients = ParseRecipients(_settings.ReleasedForApprovalRecipients);
            if (recipients.Count == 0)
            {
                _logger.LogWarning(
                    "[BulkRatesNotification] ReleasedForApproval email skipped: no recipients configured. JobQueueId={JobQueueId}",
                    ctx.JobQueueId);
                return;
            }

            var (subject, body) = BuildBulkRatesEmail(ctx.JobName, BulkRatesNotificationEvent.ReleasedForApproval);

            await _emailService.SendEmailAsync(new()
            {
                To         = recipients,
                Subject    = subject,
                Body       = body,
                IsBodyHtml = false
            }, ct);

            _logger.LogInformation(
                "[BulkRatesNotification] ReleasedForApproval email sent. JobQueueId={JobQueueId} Recipients={Count}",
                ctx.JobQueueId, recipients.Count);
        }

        private async Task SendRejectedAsync(BulkRatesNotificationContext ctx, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(ctx.RequestedBy))
            {
                _logger.LogWarning(
                    "[BulkRatesNotification] Rejected email skipped: RequestedBy is empty. JobQueueId={JobQueueId}",
                    ctx.JobQueueId);
                return;
            }

            var (subject, body) = BuildBulkRatesEmail(ctx.JobName, BulkRatesNotificationEvent.Rejected);

            await _emailService.SendEmailAsync(new()
            {
                To         = [ctx.RequestedBy],
                Subject    = subject,
                Body       = body,
                IsBodyHtml = false
            }, ct);

            _logger.LogInformation(
                "[BulkRatesNotification] Rejected email sent. JobQueueId={JobQueueId} To={To}",
                ctx.JobQueueId, ctx.RequestedBy);
        }

        private async Task SendCancelledAsync(BulkRatesNotificationContext ctx, CancellationToken ct)
        {
            // Email is optional for Cancelled; suppress if subject is not configured.
            if (string.IsNullOrWhiteSpace(_settings.CancelledSubject))
            {
                _logger.LogInformation(
                    "[BulkRatesNotification] Cancelled email suppressed (no subject configured). JobQueueId={JobQueueId}",
                    ctx.JobQueueId);
                return;
            }

            if (string.IsNullOrWhiteSpace(ctx.RequestedBy))
            {
                _logger.LogWarning(
                    "[BulkRatesNotification] Cancelled email skipped: RequestedBy is empty. JobQueueId={JobQueueId}",
                    ctx.JobQueueId);
                return;
            }

            var (subject, body) = BuildBulkRatesEmail(ctx.JobName, BulkRatesNotificationEvent.Cancelled);

            await _emailService.SendEmailAsync(new()
            {
                To         = [ctx.RequestedBy],
                Subject    = subject,
                Body       = body,
                IsBodyHtml = false
            }, ct);

            _logger.LogInformation(
                "[BulkRatesNotification] Cancelled email sent. JobQueueId={JobQueueId} To={To}",
                ctx.JobQueueId, ctx.RequestedBy);
        }

        /// <summary>Splits a comma-separated recipients string into a trimmed, non-empty list.</summary>
        private static List<string> ParseRecipients(string? value)
            => (value ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

        /// <summary>
        /// Business-facing rate-type wording for the {RateType} email token. Deliberately throws on an
        /// unrecognized job name rather than falling back to it — the raw job constant must never appear
        /// in a business email, and the caller already swallows-and-logs notification failures.
        /// </summary>
        private static string GetRateTypeDisplayName(string jobName) => jobName switch
        {
            BulkRatesJobNames.Fec    => "Test",
            BulkRatesJobNames.Staff  => "Staff",
            BulkRatesJobNames.Animal => "Animal",
            _ => throw new ArgumentOutOfRangeException(nameof(jobName), jobName, "Unknown Bulk Rates job name.")
        };

        /// <summary>Selects the configured subject/body template for the event and renders {RateType}.</summary>
        private (string Subject, string Body) BuildBulkRatesEmail(string jobName, BulkRatesNotificationEvent notificationEvent)
        {
            var (subjectTemplate, bodyTemplate) = notificationEvent switch
            {
                BulkRatesNotificationEvent.ReleasedForApproval =>
                    (_settings.ReleasedForApprovalSubject, _settings.ReleasedForApprovalBody),
                BulkRatesNotificationEvent.Approved =>
                    (_settings.ApprovedSubject, _settings.ApprovedBody),
                BulkRatesNotificationEvent.Rejected =>
                    (_settings.RejectedSubject, _settings.RejectedBody),
                BulkRatesNotificationEvent.Cancelled =>
                    (_settings.CancelledSubject, _settings.CancelledBody),
                _ => throw new ArgumentOutOfRangeException(nameof(notificationEvent), notificationEvent, "No email template for this event.")
            };

            var rateType = GetRateTypeDisplayName(jobName);
            return (subjectTemplate.Replace("{RateType}", rateType), bodyTemplate.Replace("{RateType}", rateType));
        }
    }
}
