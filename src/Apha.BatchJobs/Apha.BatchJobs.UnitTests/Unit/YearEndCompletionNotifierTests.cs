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

public sealed class YearEndCompletionNotifierTests
{
    private static BatchJobCompletionContext MakeContext(
        string jobName,
        JobStatus status,
        int fpsYear = 2027,
        string requestedBy = "requester@test",
        string? errorMessage = null,
        Guid? jobQueueId = null,
        Guid? jobExecutionId = null)
        => new(
            jobQueueId ?? Guid.NewGuid(),
            jobExecutionId ?? Guid.NewGuid(),
            jobName,
            fpsYear,
            requestedBy,
            status,
            errorMessage);

    private static YearEndCompletionNotifier CreateNotifier(
        IEmailService? email = null,
        YearEndEmailSettings? settings = null)
        => new(
            email ?? Substitute.For<IEmailService>(),
            Options.Create(settings ?? DefaultSettings()),
            NullLogger<YearEndCompletionNotifier>.Instance);

    private static YearEndEmailSettings DefaultSettings(
        string recipients = "dl@test.com",
        string subject = "Completed",
        string body = "Done",
        string failureSubject = "Failed",
        string failureBody = "Failed body") => new()
    {
        Recipients = recipients,
        CompletionSubject = subject,
        CompletionBody = body,
        FailureSubject = failureSubject,
        FailureBody = failureBody
    };

    [Theory]
    [InlineData(BatchJobNames.YearEndDataSetup)]
    [InlineData(BatchJobNames.YearEndCutover)]
    public async Task NotifyAsync_WhenYearEndJobSucceeded_SendsEmail(string jobName)
    {
        var email = Substitute.For<IEmailService>();
        email.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
             .Returns(new EmailSendResult(true, null));

        await CreateNotifier(email)
            .NotifyAsync(MakeContext(jobName, JobStatus.Completed), CancellationToken.None);

        await email.Received(1).SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(BatchJobNames.YearEndDataSetup)]
    [InlineData(BatchJobNames.YearEndCutover)]
    public async Task NotifyAsync_WhenYearEndJobFailed_SendsEmail(string jobName)
    {
        var email = Substitute.For<IEmailService>();
        email.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
             .Returns(new EmailSendResult(true, null));

        await CreateNotifier(email)
            .NotifyAsync(MakeContext(jobName, JobStatus.Failed), CancellationToken.None);

        await email.Received(1).SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(BatchJobNames.MabArchive)]
    [InlineData(BatchJobNames.RecreateSummary)]
    [InlineData("SomeOtherJob")]
    public async Task NotifyAsync_WhenNonYearEndJob_DoesNotSendEmail(string jobName)
    {
        var email = Substitute.For<IEmailService>();

        await CreateNotifier(email)
            .NotifyAsync(MakeContext(jobName, JobStatus.Completed), CancellationToken.None);

        await email.DidNotReceiveWithAnyArgs().SendAsync(default!, default);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task NotifyAsync_WhenRecipientsBlank_DoesNotSendEmail(string recipients)
    {
        var email = Substitute.For<IEmailService>();
        var settings = DefaultSettings(recipients: recipients);

        await CreateNotifier(email, settings)
            .NotifyAsync(MakeContext(BatchJobNames.YearEndDataSetup, JobStatus.Completed), CancellationToken.None);

        await email.DidNotReceiveWithAnyArgs().SendAsync(default!, default);
    }

    // ── Canonical wording — exact Subject/Body render, {YearEndStep} only ─────

    private static YearEndEmailSettings CanonicalSettings(string recipients = "dl@test.com") => new()
    {
        Recipients = recipients,
        CompletionSubject = "Year End {YearEndStep} Process Completed Successfully",
        CompletionBody = "The Year End {YearEndStep} process has completed successfully.\n\nThank you for your support.",
        FailureSubject = "Year End {YearEndStep} Process Failed",
        FailureBody = "The Year End {YearEndStep} process did not complete successfully.\n\nPlease review the details and take necessary action.\n\nThank you for your support."
    };

    public static IEnumerable<object[]> YearEndSteps() =>
    [
        [BatchJobNames.YearEndDataSetup, "DataSetup"],
        [BatchJobNames.YearEndCutover, "CutOver"],
    ];

    [Theory]
    [MemberData(nameof(YearEndSteps))]
    public async Task NotifyAsync_WhenCompleted_SendsExactCanonicalSubjectAndBody(string jobName, string step)
    {
        var email = Substitute.For<IEmailService>();
        email.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
             .Returns(new EmailSendResult(true, null));

        EmailMessage? captured = null;
        await email.SendAsync(
            Arg.Do<EmailMessage>(m => captured = m),
            Arg.Any<CancellationToken>());

        await CreateNotifier(email, CanonicalSettings())
            .NotifyAsync(MakeContext(jobName, JobStatus.Completed), CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal($"Year End {step} Process Completed Successfully", captured!.Subject);
        Assert.Equal(
            $"The Year End {step} process has completed successfully.\n\nThank you for your support.",
            captured.HtmlBody);
        Assert.False(captured.IsBodyHtml);
    }

    [Theory]
    [MemberData(nameof(YearEndSteps))]
    public async Task NotifyAsync_WhenFailed_SendsExactCanonicalSubjectAndBody(string jobName, string step)
    {
        var email = Substitute.For<IEmailService>();
        email.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
             .Returns(new EmailSendResult(true, null));

        EmailMessage? captured = null;
        await email.SendAsync(
            Arg.Do<EmailMessage>(m => captured = m),
            Arg.Any<CancellationToken>());

        await CreateNotifier(email, CanonicalSettings())
            .NotifyAsync(MakeContext(jobName, JobStatus.Failed), CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal($"Year End {step} Process Failed", captured!.Subject);
        Assert.Equal(
            $"The Year End {step} process did not complete successfully.\n\nPlease review the details and take necessary action.\n\nThank you for your support.",
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
        const string distinctiveErrorMessage = "Distinctive simulated failure: lock acquisition timed out";

        await CreateNotifier(email, CanonicalSettings())
            .NotifyAsync(
                MakeContext(
                    BatchJobNames.YearEndCutover,
                    JobStatus.Failed,
                    fpsYear: distinctiveFpsYear,
                    requestedBy: distinctiveRequestedBy,
                    errorMessage: distinctiveErrorMessage,
                    jobQueueId: jobQueueId,
                    jobExecutionId: jobExecutionId),
                CancellationToken.None);

        Assert.NotNull(captured);
        var combined = captured!.Subject + captured.HtmlBody;
        Assert.DoesNotContain(distinctiveFpsYear.ToString(), combined);
        Assert.DoesNotContain(jobQueueId.ToString(), combined);
        Assert.DoesNotContain(jobExecutionId.ToString(), combined);
        Assert.DoesNotContain(distinctiveRequestedBy, combined);
        Assert.DoesNotContain(distinctiveErrorMessage, combined);
        Assert.DoesNotContain(BatchJobNames.YearEndCutover, combined);
    }

    [Fact]
    public async Task NotifyAsync_WhenEmailServiceThrows_DoesNotPropagate()
    {
        var email = Substitute.For<IEmailService>();
        email.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
             .ThrowsAsync(new InvalidOperationException("Transport failure"));

        await CreateNotifier(email)
            .NotifyAsync(MakeContext(BatchJobNames.YearEndDataSetup, JobStatus.Completed), CancellationToken.None);

        await email.Received(1).SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }
}
