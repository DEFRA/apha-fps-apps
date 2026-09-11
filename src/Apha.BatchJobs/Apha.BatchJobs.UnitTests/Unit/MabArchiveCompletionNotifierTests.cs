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

public sealed class MabArchiveCompletionNotifierTests
{
    private static BatchJobCompletionContext MakeContext(
        string jobName,
        JobStatus status,
        string requestedBy = "requester@test",
        string? errorMessage = null,
        Guid? jobQueueId = null,
        Guid? jobExecutionId = null)
        => new(
            jobQueueId ?? Guid.NewGuid(),
            jobExecutionId ?? Guid.NewGuid(),
            jobName,
            null,
            requestedBy,
            status,
            errorMessage);

    private static MabArchiveCompletionNotifier CreateNotifier(
        IEmailService? email = null,
        MabArchiveEmailSettings? settings = null)
        => new(
            email ?? Substitute.For<IEmailService>(),
            Options.Create(settings ?? DefaultSettings()),
            NullLogger<MabArchiveCompletionNotifier>.Instance);

    private static MabArchiveEmailSettings DefaultSettings(
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

    [Fact]
    public async Task NotifyAsync_WhenMabArchiveFailed_SendsEmail()
    {
        var email = Substitute.For<IEmailService>();
        email.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
             .Returns(new EmailSendResult(true, null));

        await CreateNotifier(email)
            .NotifyAsync(MakeContext(BatchJobNames.MabArchive, JobStatus.Failed), CancellationToken.None);

        await email.Received(1).SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NotifyAsync_WhenMabArchiveSucceeded_SendsEmail()
    {
        var email = Substitute.For<IEmailService>();
        email.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
             .Returns(new EmailSendResult(true, null));

        await CreateNotifier(email)
            .NotifyAsync(MakeContext(BatchJobNames.MabArchive, JobStatus.Completed), CancellationToken.None);

        await email.Received(1).SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(BatchJobNames.RecreateSummary)]
    [InlineData(BatchJobNames.YearEndDataSetup)]
    [InlineData("SomeOtherJob")]
    public async Task NotifyAsync_WhenNonMabArchiveJob_DoesNotSendEmail(string jobName)
    {
        var email = Substitute.For<IEmailService>();

        await CreateNotifier(email)
            .NotifyAsync(MakeContext(jobName, JobStatus.Failed), CancellationToken.None);

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
            .NotifyAsync(MakeContext(BatchJobNames.MabArchive, JobStatus.Failed), CancellationToken.None);

        await email.DidNotReceiveWithAnyArgs().SendAsync(default!, default);
    }

    // ── Canonical wording — exact Subject/Body render, no tokens ──────────────

    private static MabArchiveEmailSettings CanonicalSettings(string recipients = "dl@test.com") => new()
    {
        Recipients = recipients,
        CompletionSubject = "MABArchive Process Completed Successfully",
        CompletionBody = "The MABArchive process has completed successfully.\n\nThank you for your support.",
        FailureSubject = "MABArchive Process Failed",
        FailureBody = "The MABArchive process did not complete successfully.\n\nPlease review the details and take necessary action.\n\nThank you for your support."
    };

    [Fact]
    public async Task NotifyAsync_WhenSucceeded_SendsExactCanonicalSubjectAndBody()
    {
        var email = Substitute.For<IEmailService>();
        email.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
             .Returns(new EmailSendResult(true, null));

        EmailMessage? captured = null;
        await email.SendAsync(
            Arg.Do<EmailMessage>(m => captured = m),
            Arg.Any<CancellationToken>());

        await CreateNotifier(email, CanonicalSettings())
            .NotifyAsync(MakeContext(BatchJobNames.MabArchive, JobStatus.Completed), CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal("MABArchive Process Completed Successfully", captured!.Subject);
        Assert.Equal(
            "The MABArchive process has completed successfully.\n\nThank you for your support.",
            captured.HtmlBody);
        Assert.False(captured.IsBodyHtml);
    }

    [Fact]
    public async Task NotifyAsync_WhenFailed_SendsExactCanonicalSubjectAndBody()
    {
        var email = Substitute.For<IEmailService>();
        email.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
             .Returns(new EmailSendResult(true, null));

        EmailMessage? captured = null;
        await email.SendAsync(
            Arg.Do<EmailMessage>(m => captured = m),
            Arg.Any<CancellationToken>());

        await CreateNotifier(email, CanonicalSettings())
            .NotifyAsync(MakeContext(BatchJobNames.MabArchive, JobStatus.Failed), CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal("MABArchive Process Failed", captured!.Subject);
        Assert.Equal(
            "The MABArchive process did not complete successfully.\n\nPlease review the details and take necessary action.\n\nThank you for your support.",
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
        const string distinctiveRequestedBy = "distinctive.requester@example.com";
        const string distinctiveErrorMessage = "Distinctive simulated failure: loader connection reset";

        await CreateNotifier(email, CanonicalSettings())
            .NotifyAsync(
                MakeContext(
                    BatchJobNames.MabArchive,
                    JobStatus.Failed,
                    requestedBy: distinctiveRequestedBy,
                    errorMessage: distinctiveErrorMessage,
                    jobQueueId: jobQueueId,
                    jobExecutionId: jobExecutionId),
                CancellationToken.None);

        Assert.NotNull(captured);
        var combined = captured!.Subject + captured.HtmlBody;
        Assert.DoesNotContain(jobQueueId.ToString(), combined);
        Assert.DoesNotContain(jobExecutionId.ToString(), combined);
        Assert.DoesNotContain(distinctiveRequestedBy, combined);
        Assert.DoesNotContain(distinctiveErrorMessage, combined);
    }

    [Fact]
    public async Task NotifyAsync_WhenEmailServiceThrows_DoesNotPropagate()
    {
        var email = Substitute.For<IEmailService>();
        email.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
             .ThrowsAsync(new InvalidOperationException("Transport failure"));

        await CreateNotifier(email)
            .NotifyAsync(MakeContext(BatchJobNames.MabArchive, JobStatus.Failed), CancellationToken.None);

        await email.Received(1).SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }
}
