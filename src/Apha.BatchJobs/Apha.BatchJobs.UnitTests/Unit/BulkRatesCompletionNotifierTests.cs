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

    // ── Canonical wording — exact Subject/Body render, {RateType} only ────────

    private static BulkRatesEmailSettings CanonicalSettings(string recipients = "dl@test.com") => new()
    {
        CompletionRecipients = recipients,
        CompletionSubject = "Bulk {RateType} Rates Update Completed Successfully",
        CompletionBody = "The Bulk {RateType} Rates Update has completed successfully.\n\nThank you for your support.",
        FailureSubject = "Bulk {RateType} Rates Update Failed",
        FailureBody = "The Bulk {RateType} Rates Update did not complete successfully.\n\nPlease review the details and take necessary action.\n\nThank you for your support."
    };

    public static IEnumerable<object[]> RateTypeJobs() =>
    [
        [BatchJobNames.BulkTestRatesUpdate, "Test"],
        [BatchJobNames.BulkStaffRatesUpdate, "Staff"],
        [BatchJobNames.BulkAnimalRatesUpdate, "Animal"],
    ];

    [Theory]
    [MemberData(nameof(RateTypeJobs))]
    public async Task NotifyAsync_WhenCompleted_SendsExactCanonicalSubjectAndBody(string jobName, string rateType)
    {
        var email = Substitute.For<IEmailService>();
        email.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
             .Returns(new EmailSendResult(true, null));

        EmailMessage? captured = null;
        await email.SendAsync(
            Arg.Do<EmailMessage>(m => captured = m),
            Arg.Any<CancellationToken>());

        await CreateNotifier(email, CanonicalSettings())
            .NotifyAsync(MakeContext(jobName, status: JobStatus.Completed), CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal($"Bulk {rateType} Rates Update Completed Successfully", captured!.Subject);
        Assert.Equal(
            $"The Bulk {rateType} Rates Update has completed successfully.\n\nThank you for your support.",
            captured.HtmlBody);
        Assert.False(captured.IsBodyHtml);
    }

    [Theory]
    [MemberData(nameof(RateTypeJobs))]
    public async Task NotifyAsync_WhenFailed_SendsExactCanonicalSubjectAndBody(string jobName, string rateType)
    {
        var email = Substitute.For<IEmailService>();
        email.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
             .Returns(new EmailSendResult(true, null));

        EmailMessage? captured = null;
        await email.SendAsync(
            Arg.Do<EmailMessage>(m => captured = m),
            Arg.Any<CancellationToken>());

        await CreateNotifier(email, CanonicalSettings())
            .NotifyAsync(MakeContext(jobName, status: JobStatus.Failed), CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal($"Bulk {rateType} Rates Update Failed", captured!.Subject);
        Assert.Equal(
            $"The Bulk {rateType} Rates Update did not complete successfully.\n\nPlease review the details and take necessary action.\n\nThank you for your support.",
            captured.HtmlBody);
        Assert.False(captured.IsBodyHtml);
    }

    [Fact]
    public async Task NotifyAsync_ShouldNotLeakTechnicalDiagnosticsIntoSubjectOrBody()
    {
        var email = Substitute.For<IEmailService>();
        email.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
             .Returns(new EmailSendResult(true, null));

        EmailMessage? captured = null;
        await email.SendAsync(
            Arg.Do<EmailMessage>(m => captured = m),
            Arg.Any<CancellationToken>());

        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();
        const int distinctiveFpsYear = 4242;
        const string distinctiveRequestedBy = "distinctive.requester@example.com";
        const string distinctiveErrorMessage = "Distinctive simulated failure: connection pool exhausted";

        await CreateNotifier(email, CanonicalSettings())
            .NotifyAsync(
                MakeContext(
                    BatchJobNames.BulkStaffRatesUpdate,
                    fpsYear: distinctiveFpsYear,
                    requestedBy: distinctiveRequestedBy,
                    jobQueueId: jobQueueId,
                    jobExecutionId: jobExecutionId,
                    status: JobStatus.Failed,
                    errorMessage: distinctiveErrorMessage),
                CancellationToken.None);

        Assert.NotNull(captured);
        var combined = captured!.Subject + captured.HtmlBody;
        Assert.DoesNotContain(distinctiveFpsYear.ToString(), combined);
        Assert.DoesNotContain(jobQueueId.ToString(), combined);
        Assert.DoesNotContain(jobExecutionId.ToString(), combined);
        Assert.DoesNotContain(distinctiveRequestedBy, combined);
        Assert.DoesNotContain(distinctiveErrorMessage, combined);
        Assert.DoesNotContain(BatchJobNames.BulkStaffRatesUpdate, combined);
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

}
