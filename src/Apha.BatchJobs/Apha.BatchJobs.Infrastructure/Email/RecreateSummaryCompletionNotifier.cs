using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Orchestration;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities.Email;
using Apha.BatchJobs.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Infrastructure.Email;

/// <summary>
/// Sends a completion email to the Recreate Summary process owner after the job durably reaches
/// Completed or Failed. A no-op for any other job name. Email failures are logged and swallowed
/// — never alters the job's own durable outcome.
/// </summary>
public sealed class RecreateSummaryCompletionNotifier : IPostCompletionNotifier
{
    private readonly IEmailService _emailService;
    private readonly RecreateSummaryEmailSettings _settings;
    private readonly ILogger<RecreateSummaryCompletionNotifier> _logger;

    public RecreateSummaryCompletionNotifier(
        IEmailService emailService,
        IOptions<RecreateSummaryEmailSettings> settings,
        ILogger<RecreateSummaryCompletionNotifier> logger)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task NotifyAsync(BatchJobCompletionContext context, CancellationToken cancellationToken)
    {
        if (context.JobName != BatchJobNames.RecreateSummary)
            return;

        if (string.IsNullOrWhiteSpace(_settings.Recipients))
        {
            _logger.LogInformation(
                "Recreate Summary {Status} notification suppressed: Recipients not configured | JobQueueId={JobQueueId}",
                context.Status,
                context.JobQueueId);
            return;
        }

        var recipients = _settings.Recipients
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var isSuccess = context.Status == JobStatus.Completed;
        var subject = isSuccess ? _settings.CompletionSubject : _settings.FailureSubject;
        var body = isSuccess ? _settings.CompletionBody : _settings.FailureBody;

        try
        {
            await _emailService.SendAsync(new EmailMessage(recipients, subject, body, IsBodyHtml: false), cancellationToken);

            _logger.LogInformation(
                "Recreate Summary {Status} notification sent | JobQueueId={JobQueueId}",
                context.Status,
                context.JobQueueId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send Recreate Summary {Status} notification | JobQueueId={JobQueueId} | JobExecutionId={JobExecutionId} | RequestedBy={RequestedBy}",
                context.Status,
                context.JobQueueId,
                context.JobExecutionId,
                context.RequestedBy);
        }
    }
}
