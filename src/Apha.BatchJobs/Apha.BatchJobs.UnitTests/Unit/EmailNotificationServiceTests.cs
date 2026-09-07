using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Application.Configuration;
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
    // SendExecutionNotificationAsync — Completed/Failed execution notifications (Worker-Wide
    // Batch Execution Notifications spec). The service does not decide whether to send: it
    // never checks EnableEmailNotifications/NotifyOnSuccess/NotifyOnFailure — that policy
    // decision belongs to the caller (JobOrchestrator). The service only handles delivery:
    // missing-recipient check, subject/body construction, sending, and logging the outcome.
    // ─────────────────────────────────────────────────────────────

    private static BatchExecutionNotification CreateNotification(
        JobStatus finalStatus,
        string? failureMessage = null,
        string jobName = "SampleJob",
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

    [Fact]
    public async Task SendExecutionNotificationAsync_WhenCompleted_SendsSuccessEmailWithExpectedSubjectAndRecipient()
    {
        var settings = Options.Create(new BatchAlertingSettings { AdminNotificationEmail = "alerts@example.com" });
        var emailService = Substitute.For<IEmailService>();
        emailService.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>()).Returns(EmailSendResult.Sent());
        var service = new EmailNotificationService(NullLogger<EmailNotificationService>.Instance, settings, () => emailService);

        await service.SendExecutionNotificationAsync(CreateNotification(JobStatus.Completed, jobName: "MABArchive"), CancellationToken.None);

        await emailService.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m =>
                m.To.Single() == "alerts@example.com" &&
                m.Subject == "FPS Batch Job Completed Successfully - MABArchive"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendExecutionNotificationAsync_WhenFailed_SendsFailureEmailWithExpectedSubjectAndRecipient()
    {
        var settings = Options.Create(new BatchAlertingSettings { AdminNotificationEmail = "alerts@example.com" });
        var emailService = Substitute.For<IEmailService>();
        emailService.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>()).Returns(EmailSendResult.Sent());
        var service = new EmailNotificationService(NullLogger<EmailNotificationService>.Instance, settings, () => emailService);

        await service.SendExecutionNotificationAsync(
            CreateNotification(JobStatus.Failed, failureMessage: "Simulated failure", jobName: "BulkStaffRatesUpdate"),
            CancellationToken.None);

        await emailService.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m =>
                m.To.Single() == "alerts@example.com" &&
                m.Subject == "FPS Batch Job Failed - BulkStaffRatesUpdate"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendExecutionNotificationAsync_BodyIncludesExecutionIdentifiers()
    {
        var settings = Options.Create(new BatchAlertingSettings { AdminNotificationEmail = "alerts@example.com" });
        var emailService = Substitute.For<IEmailService>();
        emailService.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>()).Returns(EmailSendResult.Sent());
        var service = new EmailNotificationService(NullLogger<EmailNotificationService>.Instance, settings, () => emailService);

        var jobExecutionId = Guid.NewGuid();
        var jobQueueId = Guid.NewGuid();
        var notification = CreateNotification(
            JobStatus.Completed,
            jobName: "YearEnd-DataSetup",
            jobExecutionId: jobExecutionId,
            jobQueueId: jobQueueId,
            runMode: RunMode.Manual,
            requestedBy: "arihant.jain@atos.net");

        await service.SendExecutionNotificationAsync(notification, CancellationToken.None);

        await emailService.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m =>
                m.HtmlBody.Contains("Job Name: YearEnd-DataSetup") &&
                m.HtmlBody.Contains($"Job Execution ID: {jobExecutionId}") &&
                m.HtmlBody.Contains($"Job Queue ID: {jobQueueId}") &&
                m.HtmlBody.Contains("Run Mode: Manual") &&
                m.HtmlBody.Contains("Requested By: arihant.jain@atos.net") &&
                m.HtmlBody.Contains("Final Status: Completed")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendExecutionNotificationAsync_WhenFailed_BodyIncludesConciseFailureReason_ButNotWhenCompleted()
    {
        var settings = Options.Create(new BatchAlertingSettings { AdminNotificationEmail = "alerts@example.com" });
        var emailService = Substitute.For<IEmailService>();
        emailService.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>()).Returns(EmailSendResult.Sent());
        var service = new EmailNotificationService(NullLogger<EmailNotificationService>.Instance, settings, () => emailService);

        await service.SendExecutionNotificationAsync(
            CreateNotification(JobStatus.Failed, failureMessage: "Database connection timed out"),
            CancellationToken.None);
        await service.SendExecutionNotificationAsync(CreateNotification(JobStatus.Completed), CancellationToken.None);

        await emailService.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m => m.HtmlBody.Contains("Failure:") && m.HtmlBody.Contains("Database connection timed out")),
            Arg.Any<CancellationToken>());
        await emailService.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m => !m.HtmlBody.Contains("Failure:")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendExecutionNotificationAsync_WhenFailed_BodyContainsOnlyProvidedMessage_NoStackTraceOrExtraDetail()
    {
        var settings = Options.Create(new BatchAlertingSettings { AdminNotificationEmail = "alerts@example.com" });
        string? capturedBody = null;
        var emailService = Substitute.For<IEmailService>();
        emailService.SendAsync(Arg.Do<EmailMessage>(m => capturedBody = m.HtmlBody), Arg.Any<CancellationToken>())
            .Returns(EmailSendResult.Sent());
        var service = new EmailNotificationService(NullLogger<EmailNotificationService>.Instance, settings, () => emailService);

        // A realistic .Message-only failure reason — the model has no Exception/StackTrace
        // property at all, so there is structurally nothing else the service could append.
        const string concise = "Simulated failure";
        await service.SendExecutionNotificationAsync(CreateNotification(JobStatus.Failed, failureMessage: concise), CancellationToken.None);

        Assert.NotNull(capturedBody);
        Assert.DoesNotContain("at Apha.BatchJobs", capturedBody);
        Assert.DoesNotContain("StackTrace", capturedBody);
        var failureLine = capturedBody!.Split(Environment.NewLine)
            .SkipWhile(line => line != "Failure:")
            .Skip(1)
            .First();
        Assert.Equal(concise, failureLine);
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
