using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Application.Configuration;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities.Email;
using Apha.BatchJobs.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.BatchJobs.UnitTests;

public sealed class EmailNotificationServiceTests
{
    // Used wherever the send path must never be reached — proves EmailNotificationService
    // resolves IEmailService lazily (only once a notification is actually about to send), not
    // eagerly on construction. If this ever fires, that laziness guarantee has regressed.
    private static Func<IEmailService> ThrowingEmailServiceFactory =>
        () => throw new InvalidOperationException("IEmailService should not have been resolved");

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new EmailNotificationService(
                null!,
                Options.Create(new BatchAlertingSettings()),
                ThrowingEmailServiceFactory));

        Assert.Equal("logger", ex.ParamName);
    }

    // ─────────────────────────────────────────────────────────────
    // SendExecutionNotificationAsync — Completed/Failed execution notifications, standardised
    // per batchjobs-worker-email-notifications-final-spec-2026-09-09.md §2/§5. The service does
    // not decide whether to send: it never checks
    // EnableEmailNotifications/NotifyOnSuccess/NotifyOnFailure — that policy decision belongs to
    // the caller (JobOrchestrator). The service only handles delivery: missing-recipient check,
    // subject/body construction, sending, and logging the outcome.
    // ─────────────────────────────────────────────────────────────

    private static BatchExecutionNotification CreateNotification(
        JobStatus finalStatus,
        string? failureMessage = null,
        string jobName = BatchJobNames.MabArchive,
        Guid? jobExecutionId = null,
        Guid? jobQueueId = null,
        RunMode runMode = RunMode.Scheduled,
        string? requestedBy = "test-user",
        DateTime? requestedAtUtc = null,
        DateTime? finishedAtUtc = null,
        TimeSpan? duration = null) => new(
            jobName,
            jobExecutionId ?? Guid.NewGuid(),
            jobQueueId ?? Guid.NewGuid(),
            runMode,
            requestedBy,
            requestedAtUtc,
            finalStatus,
            finishedAtUtc ?? DateTime.UtcNow,
            duration,
            failureMessage);

    [Fact]
    public async Task SendExecutionNotificationAsync_WhenNotificationIsNull_ShouldThrowArgumentNullException()
    {
        var service = new EmailNotificationService(
            NullLogger<EmailNotificationService>.Instance,
            Options.Create(new BatchAlertingSettings { AdminNotificationEmail = "alerts@example.com" }),
            ThrowingEmailServiceFactory);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.SendExecutionNotificationAsync(null!, CancellationToken.None));
    }

    public static IEnumerable<object[]> AllKnownJobs() =>
    [
        [BatchJobNames.BulkTestRatesUpdate, "Bulk Test Rates Update"],
        [BatchJobNames.BulkStaffRatesUpdate, "Bulk Staff Rates Update"],
        [BatchJobNames.BulkAnimalRatesUpdate, "Bulk Animal Rates Update"],
        [BatchJobNames.MabArchive, "MABArchive"],
        [BatchJobNames.RecreateSummary, "Recreate Summary"],
        [BatchJobNames.YearEndDataSetup, "Year End DataSetup"],
        [BatchJobNames.YearEndCutover, "Year End CutOver"],
        [BatchJobNames.HealthCheck, "Health Check"],
        [BatchJobNames.MilestoneUpdateNotifications, "Milestone Update Notifications"],
    ];

    [Theory]
    [MemberData(nameof(AllKnownJobs))]
    public async Task SendExecutionNotificationAsync_WhenCompleted_SendsExactCanonicalSubjectAndBody(string jobName, string displayName)
    {
        var settings = Options.Create(new BatchAlertingSettings { AdminNotificationEmail = "alerts@example.com" });
        var emailService = Substitute.For<IEmailService>();
        emailService.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>()).Returns(EmailSendResult.Sent());
        var service = new EmailNotificationService(NullLogger<EmailNotificationService>.Instance, settings, () => emailService);

        await service.SendExecutionNotificationAsync(CreateNotification(JobStatus.Completed, jobName: jobName), CancellationToken.None);

        await emailService.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m =>
                m.To.Single() == "alerts@example.com" &&
                m.Subject == $"FPS Batch Job Completed Successfully – {displayName}" &&
                m.HtmlBody == $"The {displayName} process has completed successfully.\n\nThank you for your support." &&
                !m.IsBodyHtml),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [MemberData(nameof(AllKnownJobs))]
    public async Task SendExecutionNotificationAsync_WhenFailed_SendsExactCanonicalSubjectAndBody(string jobName, string displayName)
    {
        var settings = Options.Create(new BatchAlertingSettings { AdminNotificationEmail = "alerts@example.com" });
        var emailService = Substitute.For<IEmailService>();
        emailService.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>()).Returns(EmailSendResult.Sent());
        var service = new EmailNotificationService(NullLogger<EmailNotificationService>.Instance, settings, () => emailService);

        await service.SendExecutionNotificationAsync(
            CreateNotification(JobStatus.Failed, failureMessage: "Simulated failure", jobName: jobName),
            CancellationToken.None);

        await emailService.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m =>
                m.To.Single() == "alerts@example.com" &&
                m.Subject == $"FPS Batch Job Failed – {displayName}" &&
                m.HtmlBody == $"The {displayName} process did not complete successfully.\n\nPlease review the details and take necessary action.\n\nThank you for your support." &&
                !m.IsBodyHtml),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendExecutionNotificationAsync_ShouldNotLeakTechnicalDiagnosticsIntoSubjectOrBody()
    {
        var settings = Options.Create(new BatchAlertingSettings { AdminNotificationEmail = "alerts@example.com" });
        string? capturedSubject = null;
        string? capturedBody = null;
        var emailService = Substitute.For<IEmailService>();
        emailService.SendAsync(Arg.Do<EmailMessage>(m => { capturedSubject = m.Subject; capturedBody = m.HtmlBody; }), Arg.Any<CancellationToken>())
            .Returns(EmailSendResult.Sent());
        var service = new EmailNotificationService(NullLogger<EmailNotificationService>.Instance, settings, () => emailService);

        var jobExecutionId = Guid.NewGuid();
        var jobQueueId = Guid.NewGuid();
        var notification = CreateNotification(
            JobStatus.Failed,
            failureMessage: "Database connection timed out",
            jobName: BatchJobNames.YearEndDataSetup,
            jobExecutionId: jobExecutionId,
            jobQueueId: jobQueueId,
            runMode: RunMode.Manual,
            requestedBy: "arihant.jain@atos.net",
            requestedAtUtc: DateTime.UtcNow,
            duration: TimeSpan.FromMinutes(3));

        await service.SendExecutionNotificationAsync(notification, CancellationToken.None);

        var combined = capturedSubject + capturedBody;
        Assert.DoesNotContain(jobExecutionId.ToString(), combined);
        Assert.DoesNotContain(jobQueueId.ToString(), combined);
        Assert.DoesNotContain("Manual", combined);
        Assert.DoesNotContain("arihant.jain@atos.net", combined);
        Assert.DoesNotContain("Database connection timed out", combined);
        Assert.DoesNotContain(BatchJobNames.YearEndDataSetup, combined);
        Assert.DoesNotContain("Run Mode", combined);
        Assert.DoesNotContain("Requested By", combined);
        Assert.DoesNotContain("Duration", combined);
    }

    [Fact]
    public async Task SendExecutionNotificationAsync_WhenJobNameIsUnrecognised_SwallowsAndDoesNotSend()
    {
        // BatchAlerting applies to every job with no allow-list, so a genuinely new/unmapped job
        // must not crash the caller — it just can't produce a business email until it's added to
        // BatchJobDisplayNames. The lookup throws internally; this proves that's caught, not
        // propagated, and no send is attempted.
        var settings = Options.Create(new BatchAlertingSettings { AdminNotificationEmail = "alerts@example.com" });
        var emailService = Substitute.For<IEmailService>();
        var service = new EmailNotificationService(NullLogger<EmailNotificationService>.Instance, settings, () => emailService);

        await service.SendExecutionNotificationAsync(
            CreateNotification(JobStatus.Completed, jobName: "SomeFutureJob"),
            CancellationToken.None);

        await emailService.DidNotReceive().SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendExecutionNotificationAsync_WhenAdminEmailBlank_SkipsSendWithoutResolvingEmailService()
    {
        var settings = Options.Create(new BatchAlertingSettings { AdminNotificationEmail = "   " });
        var emailServiceResolved = false;
        var service = new EmailNotificationService(
            NullLogger<EmailNotificationService>.Instance, settings,
            () => { emailServiceResolved = true; return Substitute.For<IEmailService>(); });

        await service.SendExecutionNotificationAsync(CreateNotification(JobStatus.Completed), CancellationToken.None);

        Assert.False(emailServiceResolved);
    }

    [Fact]
    public async Task SendExecutionNotificationAsync_WhenEmailServiceReportsFailure_LogsWarningWithoutThrowing()
    {
        var settings = Options.Create(new BatchAlertingSettings { AdminNotificationEmail = "alerts@example.com" });
        var emailService = Substitute.For<IEmailService>();
        emailService.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>()).Returns(EmailSendResult.Failed("graph unavailable"));
        var service = new EmailNotificationService(NullLogger<EmailNotificationService>.Instance, settings, () => emailService);

        await service.SendExecutionNotificationAsync(CreateNotification(JobStatus.Failed, failureMessage: "boom"), CancellationToken.None);

        await emailService.Received(1).SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendExecutionNotificationAsync_WhenEmailServiceThrows_SwallowsWithoutThrowing()
    {
        // Fully self-contained best-effort per the spec: a delivery failure must never propagate
        // to threaten the already-persisted job status.
        var settings = Options.Create(new BatchAlertingSettings { AdminNotificationEmail = "alerts@example.com" });
        var emailService = Substitute.For<IEmailService>();
        emailService.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Notification transport down"));
        var service = new EmailNotificationService(NullLogger<EmailNotificationService>.Instance, settings, () => emailService);

        await service.SendExecutionNotificationAsync(CreateNotification(JobStatus.Completed), CancellationToken.None);
    }

    [Fact]
    public async Task SendExecutionNotificationAsync_WhenCancelled_PropagatesOperationCanceledException()
    {
        var settings = Options.Create(new BatchAlertingSettings { AdminNotificationEmail = "alerts@example.com" });
        var emailService = Substitute.For<IEmailService>();
        emailService.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Throws(new OperationCanceledException());
        var service = new EmailNotificationService(NullLogger<EmailNotificationService>.Instance, settings, () => emailService);

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => service.SendExecutionNotificationAsync(CreateNotification(JobStatus.Completed), CancellationToken.None));
    }

    [Fact]
    public async Task SendExecutionNotificationAsync_DoesNotGateOnEnableOrNotifyFlags_CallerOwnsThatPolicyDecision()
    {
        // Proves the responsibility split: EnableEmailNotifications/NotifyOnSuccess/NotifyOnFailure
        // are JobOrchestrator's decision, not re-interpreted here — the service sends whenever
        // it's called, as long as a recipient is configured.
        var settings = Options.Create(new BatchAlertingSettings
        {
            EnableEmailNotifications = false,
            NotifyOnSuccess = false,
            NotifyOnFailure = false,
            AdminNotificationEmail = "alerts@example.com"
        });
        var emailService = Substitute.For<IEmailService>();
        emailService.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>()).Returns(EmailSendResult.Sent());
        var service = new EmailNotificationService(NullLogger<EmailNotificationService>.Instance, settings, () => emailService);

        await service.SendExecutionNotificationAsync(CreateNotification(JobStatus.Completed), CancellationToken.None);

        await emailService.Received(1).SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }
}
