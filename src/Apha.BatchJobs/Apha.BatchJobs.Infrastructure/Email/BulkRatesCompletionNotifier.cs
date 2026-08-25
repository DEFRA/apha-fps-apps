using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Orchestration;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Infrastructure.Email;

/// <summary>
/// Sends a completion email for all three Bulk Rates jobs after their durable Completed transition.
/// A no-op for any other job name. Email failures are logged and swallowed — the job remains Completed.
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
                "Bulk Rates completion notification suppressed: CompletionRecipients not configured | JobName={JobName} | JobQueueId={JobQueueId}",
                context.JobName,
                context.JobQueueId);
            return;
        }

        var recipients = _settings.CompletionRecipients
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var subject = ReplacePlaceholders(_settings.CompletionSubject, context);
        var body = ReplacePlaceholders(_settings.CompletionBody, context);

        try
        {
            await _emailService.SendAsync(new EmailMessage(recipients, subject, body), cancellationToken);

            _logger.LogInformation(
                "Bulk Rates completion notification sent | JobName={JobName} | JobQueueId={JobQueueId} | FpsYear={FpsYear}",
                context.JobName,
                context.JobQueueId,
                context.FpsYear);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send Bulk Rates completion notification | JobName={JobName} | JobQueueId={JobQueueId} | JobExecutionId={JobExecutionId} | FpsYear={FpsYear} | RequestedBy={RequestedBy}",
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
        template
            .Replace("{JobName}", context.JobName, StringComparison.Ordinal)
            .Replace("{JobQueueId}", context.JobQueueId.ToString("D"), StringComparison.Ordinal)
            .Replace("{FpsYear}", context.FpsYear?.ToString() ?? string.Empty, StringComparison.Ordinal)
            .Replace("{RequestedBy}", context.RequestedBy, StringComparison.Ordinal);
}
