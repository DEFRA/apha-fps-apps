using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Orchestration;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities.Email;
using Apha.BatchJobs.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Infrastructure.Email;

/// <summary>
/// Sends a failure email to the MABArchive process owner when the job durably reaches Failed.
/// Deliberately failure-only, mirroring the scope of the legacy MABArchive-only alert this
/// worker-wide notification model replaced — a success channel for this process owner is a
/// separate decision, not yet in scope. A no-op for any other job name. Email failures are
/// logged and swallowed — never alters the job's own durable outcome.
/// </summary>
public sealed class MabArchiveCompletionNotifier : IPostCompletionNotifier
{
    private readonly IEmailService _emailService;
    private readonly MabArchiveEmailSettings _settings;
    private readonly ILogger<MabArchiveCompletionNotifier> _logger;

    public MabArchiveCompletionNotifier(
        IEmailService emailService,
        IOptions<MabArchiveEmailSettings> settings,
        ILogger<MabArchiveCompletionNotifier> logger)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task NotifyAsync(BatchJobCompletionContext context, CancellationToken cancellationToken)
    {
        if (context.JobName != BatchJobNames.MabArchive)
            return;

        if (context.Status != JobStatus.Failed)
            return;

        if (string.IsNullOrWhiteSpace(_settings.Recipients))
        {
            _logger.LogInformation(
                "MABArchive failure notification suppressed: Recipients not configured | JobQueueId={JobQueueId}",
                context.JobQueueId);
            return;
        }

        var recipients = _settings.Recipients
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var subject = ReplacePlaceholders(_settings.FailureSubject, context);
        var body = ReplacePlaceholders(_settings.FailureBody, context);

        try
        {
            await _emailService.SendAsync(new EmailMessage(recipients, subject, body), cancellationToken);

            _logger.LogInformation(
                "MABArchive failure notification sent | JobQueueId={JobQueueId}",
                context.JobQueueId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send MABArchive failure notification | JobQueueId={JobQueueId} | JobExecutionId={JobExecutionId} | RequestedBy={RequestedBy}",
                context.JobQueueId,
                context.JobExecutionId,
                context.RequestedBy);
        }
    }

    private static string ReplacePlaceholders(string template, BatchJobCompletionContext context) =>
        template
            .Replace("{JobName}", context.JobName, StringComparison.Ordinal)
            .Replace("{JobQueueId}", context.JobQueueId.ToString("D"), StringComparison.Ordinal)
            .Replace("{RequestedBy}", context.RequestedBy, StringComparison.Ordinal)
            .Replace("{ErrorMessage}", context.ErrorMessage ?? string.Empty, StringComparison.Ordinal);
}
