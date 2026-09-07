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

    [Fact]
    public async Task NotifyAsync_WhenFailed_UsesFailureTemplatesWithFpsYearAndErrorMessageTokens()
    {
        var email = Substitute.For<IEmailService>();
        email.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
             .Returns(new EmailSendResult(true, null));

        EmailMessage? captured = null;
        await email.SendAsync(
            Arg.Do<EmailMessage>(m => captured = m),
            Arg.Any<CancellationToken>());

        var settings = DefaultSettings(
            subject: "should not appear",
            failureSubject: "{JobName} FAILED - {FpsYear}",
            failureBody: "Reason: {ErrorMessage}");

        await CreateNotifier(email, settings)
            .NotifyAsync(
                MakeContext(BatchJobNames.YearEndCutover, JobStatus.Failed, fpsYear: 2028, errorMessage: "Lock timeout"),
                CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal("YearEnd-CutOver FAILED - 2028", captured!.Subject);
        Assert.Contains("Lock timeout", captured.HtmlBody);
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
