using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Orchestration;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities.Email;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Infrastructure.Email;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.BatchJobs.UnitTests;

public sealed class BulkRatesCompletionNotifierTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────────

    private static BatchJobCompletionContext MakeContext(
        string jobName,
        int fpsYear = 2027,
        string requestedBy = "requester@test",
        Guid? jobQueueId = null,
        Guid? jobExecutionId = null,
        JobStatus status = JobStatus.Completed,
        string? errorMessage = null)
        => new(
            jobQueueId ?? Guid.NewGuid(),
            jobExecutionId ?? Guid.NewGuid(),
            jobName,
            fpsYear,
            requestedBy,
            status,
            errorMessage);

    private static BulkRatesCompletionNotifier CreateNotifier(
        IEmailService? email = null,
        BulkRatesEmailSettings? settings = null)
        => new(
            email ?? Substitute.For<IEmailService>(),
            Options.Create(settings ?? DefaultSettings()),
            NullLogger<BulkRatesCompletionNotifier>.Instance);

    private static BulkRatesEmailSettings DefaultSettings(
        string recipients = "dl@test.com",
        string subject = "Completed",
        string body = "Done",
        string failureSubject = "Failed",
        string failureBody = "Failed body") => new()
    {
        CompletionRecipients = recipients,
        CompletionSubject = subject,
        CompletionBody = body,
        FailureSubject = failureSubject,
        FailureBody = failureBody
    };

    // ── Job recognition: Bulk Rates jobs trigger send ─────────────────────────

    [Theory]
    [InlineData(BatchJobNames.BulkTestRatesUpdate)]
    [InlineData(BatchJobNames.BulkStaffRatesUpdate)]
    [InlineData(BatchJobNames.BulkAnimalRatesUpdate)]
    public async Task NotifyAsync_WhenBulkRatesJob_SendsEmail(string jobName)
    {
        var email = Substitute.For<IEmailService>();
        email.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
             .Returns(new EmailSendResult(true, null));

        await CreateNotifier(email).NotifyAsync(MakeContext(jobName), CancellationToken.None);

        await email.Received(1).SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }

    // ── Job recognition: non-Bulk-Rates job is a no-op ───────────────────────

    [Theory]
    [InlineData(BatchJobNames.MabArchive)]
    [InlineData(BatchJobNames.RecreateSummary)]
    [InlineData(BatchJobNames.YearEndDataSetup)]
    [InlineData("SomeOtherJob")]
    public async Task NotifyAsync_WhenNonBulkRatesJob_DoesNotSendEmail(string jobName)
    {
        var email = Substitute.For<IEmailService>();

        await CreateNotifier(email).NotifyAsync(MakeContext(jobName), CancellationToken.None);

        await email.DidNotReceiveWithAnyArgs().SendAsync(default!, default);
    }

    // ── Blank recipients suppresses send ─────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task NotifyAsync_WhenRecipientsBlank_DoesNotSendEmail(string recipients)
    {
        var email = Substitute.For<IEmailService>();
        var settings = DefaultSettings(recipients: recipients);

        await CreateNotifier(email, settings)
            .NotifyAsync(MakeContext(BatchJobNames.BulkStaffRatesUpdate), CancellationToken.None);

        await email.DidNotReceiveWithAnyArgs().SendAsync(default!, default);
    }

    // ── Multiple recipients parsed correctly ─────────────────────────────────

    [Fact]
    public async Task NotifyAsync_WhenMultipleRecipients_PassesAllToEmailService()
    {
        var email = Substitute.For<IEmailService>();
        email.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
             .Returns(new EmailSendResult(true, null));

        IReadOnlyList<string>? capturedTo = null;
        await email.SendAsync(
            Arg.Do<EmailMessage>(m => capturedTo = m.To),
            Arg.Any<CancellationToken>());

        var settings = DefaultSettings(recipients: "a@test.com, b@test.com , c@test.com");

        await CreateNotifier(email, settings)
            .NotifyAsync(MakeContext(BatchJobNames.BulkAnimalRatesUpdate), CancellationToken.None);

        await email.Received(1).SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
        Assert.NotNull(capturedTo);
        Assert.Equal(3, capturedTo!.Count);
        Assert.Contains("a@test.com", capturedTo);
        Assert.Contains("b@test.com", capturedTo);
        Assert.Contains("c@test.com", capturedTo);
    }

    // ── Token replacement in subject / body ──────────────────────────────────

    [Fact]
    public async Task NotifyAsync_ReplacesContextTokensInSubjectAndBody()
    {
        var email = Substitute.For<IEmailService>();
        email.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
             .Returns(new EmailSendResult(true, null));

        EmailMessage? captured = null;
        await email.SendAsync(
            Arg.Do<EmailMessage>(m => captured = m),
            Arg.Any<CancellationToken>());

        var jqid = Guid.NewGuid();
        var settings = DefaultSettings(
            subject: "{JobName} {FpsYear} completed",
            body: "Job {JobName} / {JobQueueId} / by {RequestedBy}");

        await CreateNotifier(email, settings)
            .NotifyAsync(
                MakeContext(BatchJobNames.BulkStaffRatesUpdate, fpsYear: 2028,
                    requestedBy: "user@test", jobQueueId: jqid),
                CancellationToken.None);

        await email.Received(1).SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
        Assert.NotNull(captured);
        Assert.Equal("BulkStaffRatesUpdate 2028 completed", captured!.Subject);
        Assert.Contains("BulkStaffRatesUpdate", captured.HtmlBody);
        Assert.Contains(jqid.ToString("D"), captured.HtmlBody);
        Assert.Contains("user@test", captured.HtmlBody);
    }

    // ── Email exception is logged and swallowed ───────────────────────────────

    [Fact]
    public async Task NotifyAsync_WhenEmailServiceThrows_DoesNotPropagate()
    {
        var email = Substitute.For<IEmailService>();
        email.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
             .ThrowsAsync(new InvalidOperationException("Transport failure"));

        // Must complete without throwing.
        await CreateNotifier(email)
            .NotifyAsync(MakeContext(BatchJobNames.BulkTestRatesUpdate), CancellationToken.None);

        // Email send was attempted; the exception was swallowed by the notifier.
        await email.Received(1).SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }

    // ── Failed jobs notify the same recipients, using the failure templates ───

    [Theory]
    [InlineData(BatchJobNames.BulkTestRatesUpdate)]
    [InlineData(BatchJobNames.BulkStaffRatesUpdate)]
    [InlineData(BatchJobNames.BulkAnimalRatesUpdate)]
    public async Task NotifyAsync_WhenBulkRatesJobFailed_SendsEmail(string jobName)
    {
        var email = Substitute.For<IEmailService>();
        email.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
             .Returns(new EmailSendResult(true, null));

        await CreateNotifier(email)
            .NotifyAsync(MakeContext(jobName, status: JobStatus.Failed), CancellationToken.None);

        await email.Received(1).SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NotifyAsync_WhenFailed_RecipientsBlank_DoesNotSendEmail()
    {
        var email = Substitute.For<IEmailService>();
        var settings = DefaultSettings(recipients: "");

        await CreateNotifier(email, settings)
            .NotifyAsync(
                MakeContext(BatchJobNames.BulkStaffRatesUpdate, status: JobStatus.Failed),
                CancellationToken.None);

        await email.DidNotReceiveWithAnyArgs().SendAsync(default!, default);
    }

    [Fact]
    public async Task NotifyAsync_WhenFailed_UsesFailureTemplatesAndErrorMessageToken()
    {
        var email = Substitute.For<IEmailService>();
        email.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
             .Returns(new EmailSendResult(true, null));

        EmailMessage? captured = null;
        await email.SendAsync(
            Arg.Do<EmailMessage>(m => captured = m),
            Arg.Any<CancellationToken>());

        var settings = DefaultSettings(
            subject: "{JobName} {FpsYear} completed",
            body: "should not appear",
            failureSubject: "{JobName} FAILED",
            failureBody: "Reason: {ErrorMessage}");

        await CreateNotifier(email, settings)
            .NotifyAsync(
                MakeContext(
                    BatchJobNames.BulkAnimalRatesUpdate,
                    status: JobStatus.Failed,
                    errorMessage: "DB connection timed out"),
                CancellationToken.None);

        await email.Received(1).SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
        Assert.NotNull(captured);
        Assert.Equal("BulkAnimalRatesUpdate FAILED", captured!.Subject);
        Assert.Contains("DB connection timed out", captured.HtmlBody);
    }
}
