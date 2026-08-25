using Apha.Common.Contracts.Email;
using Apha.Common.Utilities.Email;
using Apha.FPS.Application.Enums;
using Apha.FPS.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apha.FPS.Application.Services.BulkRates
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

            await _emailService.SendEmailAsync(new()
            {
                To      = recipients,
                Subject = ApplyTokens(_settings.ApprovedSubject, ctx),
                Body    = ApplyTokens(_settings.ApprovedBody, ctx)
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

            await _emailService.SendEmailAsync(new()
            {
                To      = recipients,
                Subject = ApplyTokens(_settings.ReleasedForApprovalSubject, ctx),
                Body    = ApplyTokens(_settings.ReleasedForApprovalBody, ctx)
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

            await _emailService.SendEmailAsync(new()
            {
                To      = [ctx.RequestedBy],
                Subject = ApplyTokens(_settings.RejectedSubject, ctx),
                Body    = ApplyTokens(_settings.RejectedBody, ctx)
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

            await _emailService.SendEmailAsync(new()
            {
                To      = [ctx.RequestedBy],
                Subject = ApplyTokens(_settings.CancelledSubject, ctx),
                Body    = ApplyTokens(_settings.CancelledBody, ctx)
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

        /// <summary>Replaces well-known tokens in subject/body templates.</summary>
        private static string ApplyTokens(string template, BulkRatesNotificationContext ctx)
            => template
                .Replace("{JobQueueId}",  ctx.JobQueueId.ToString())
                .Replace("{JobName}",     ctx.JobName)
                .Replace("{FpsYear}",     ctx.FpsYear.ToString())
                .Replace("{RequestedBy}", ctx.RequestedBy)
                .Replace("{ApprovedBy}",  ctx.ApprovedBy ?? string.Empty)
                .Replace("{Reason}",      ctx.Reason ?? string.Empty);
    }
}
