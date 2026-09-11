using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Orchestration;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities.Email;
using Apha.BatchJobs.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Infrastructure.Email;

/// <summary>
/// Sends a completion email for all three Bulk Rates jobs after they durably reach Completed or
/// Failed. Same recipients both times — they're the approvers for the update, so they need
/// failure visibility, not just a success receipt. A no-op for any other job name. Email failures
/// are logged and swallowed — never alters the job's own durable outcome.
/// </summary>
public sealed class BulkRatesCompletionNotifier : IPostCompletionNotifier
{
    private readonly IEmailService _emailService;
    private readonly BulkRatesEmailSettings _settings;
    private readonly ILogger<BulkRatesCompletionNotifier> _logger;

    public BulkRatesCompletionNotifier(
        IEmailService emailService,
        IOptions<BulkRatesEmailSettings> settings,
        ILogger<BulkRatesCompletionNotifier> logger)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task NotifyAsync(BatchJobCompletionContext context, CancellationToken cancellationToken)
    {
        if (!IsBulkRatesJob(context.JobName))
            return;

        if (string.IsNullOrWhiteSpace(_settings.CompletionRecipients))
        {
            _logger.LogInformation(
                "Bulk Rates {Status} notification suppressed: CompletionRecipients not configured | JobName={JobName} | JobQueueId={JobQueueId}",
                context.Status,
                context.JobName,
                context.JobQueueId);
            return;
        }

        var recipients = _settings.CompletionRecipients
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var isSuccess = context.Status == JobStatus.Completed;
        var subject = ReplacePlaceholders(isSuccess ? _settings.CompletionSubject : _settings.FailureSubject, context);
        var body = ReplacePlaceholders(isSuccess ? _settings.CompletionBody : _settings.FailureBody, context);

        try
        {
            await _emailService.SendAsync(new EmailMessage(recipients, subject, body, IsBodyHtml: false), cancellationToken);

            _logger.LogInformation(
                "Bulk Rates {Status} notification sent | JobName={JobName} | JobQueueId={JobQueueId} | FpsYear={FpsYear}",
                context.Status,
                context.JobName,
                context.JobQueueId,
                context.FpsYear);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send Bulk Rates {Status} notification | JobName={JobName} | JobQueueId={JobQueueId} | JobExecutionId={JobExecutionId} | FpsYear={FpsYear} | RequestedBy={RequestedBy}",
                context.Status,
                context.JobName,
                context.JobQueueId,
                context.JobExecutionId,
                context.FpsYear,
                context.RequestedBy);
        }
    }

    private static bool IsBulkRatesJob(string jobName) =>
        jobName is
            BatchJobNames.BulkTestRatesUpdate or
            BatchJobNames.BulkStaffRatesUpdate or
            BatchJobNames.BulkAnimalRatesUpdate;

    private static string ReplacePlaceholders(string template, BatchJobCompletionContext context) =>
        template.Replace("{RateType}", GetRateType(context.JobName), StringComparison.Ordinal);

    /// <summary>
    /// Only ever called after <see cref="IsBulkRatesJob"/> has already confirmed one of the three
    /// known job names — the default branch is unreachable in practice, kept defensive rather
    /// than silently returning something.
    /// </summary>
    private static string GetRateType(string jobName) => jobName switch
    {
        BatchJobNames.BulkTestRatesUpdate => "Test",
        BatchJobNames.BulkStaffRatesUpdate => "Staff",
        BatchJobNames.BulkAnimalRatesUpdate => "Animal",
        _ => throw new ArgumentOutOfRangeException(nameof(jobName), jobName, "Not a Bulk Rates job.")
    };
}
