using Apha.Common.Contracts.Email;
using Apha.Common.Contracts.FPS.Email;
using Apha.Common.Utilities.Email;
using Apha.FPS.Application.Common.BulkRates;
using Apha.FPS.Application.Enums;
using Apha.FPS.Application.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Application.UnitTests.Services.BulkRatesServiceTest;

public class GraphEmailBulkRatesNotificationServiceTests
{
    private static GraphEmailBulkRatesNotificationService CreateSut(
        IGraphEmailService? emailService = null,
        BulkRatesEmailSettings? settings = null)
    {
        var s = settings ?? DefaultSettings();
        return new GraphEmailBulkRatesNotificationService(
            emailService ?? Substitute.For<IGraphEmailService>(),
            Options.Create(s),
            NullLogger<GraphEmailBulkRatesNotificationService>.Instance);
    }

    private static BulkRatesEmailSettings DefaultSettings() => new()
    {
        ReleasedForApprovalRecipients = "approver@test.com,approver2@test.com",
        ReleasedForApprovalSubject    = "Released {JobName} {FpsYear}",
        ReleasedForApprovalBody       = "Job={JobName} Year={FpsYear} By={RequestedBy} Id={JobQueueId}",
        ApprovedRecipients            = "initiator@test.com",
        ApprovedSubject               = "Approved {JobName} {FpsYear}",
        ApprovedBody                  = "Approved by {ApprovedBy}",
        RejectedSubject               = "Rejected {JobName}",
        RejectedBody                  = "Reason={Reason}",
        CancelledSubject              = "Cancelled {JobName}",
        CancelledBody                 = "Reason={Reason}"
    };

    private static BulkRatesNotificationContext Context(string requestedBy = "alice@test.com", string? reason = null) => new()
    {
        JobQueueId  = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        JobName     = "BulkTestRatesUpdate",
        FpsYear     = 2027,
        RequestedBy = requestedBy,
        Reason      = reason
    };

    // ── Approved ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Approved_SendsToConfiguredRecipients()
    {
        var email = Substitute.For<IGraphEmailService>();
        var sut   = CreateSut(email);
        var ctx   = Context();
        ctx.ApprovedBy = "bob@test.com";

        await sut.NotifyAsync(BulkRatesNotificationEvent.Approved, ctx);

        await email.Received(1).SendEmailAsync(
            Arg.Is<EmailMessageModel>(m =>
                m.To.Contains("initiator@test.com") &&
                m.Subject.Contains("BulkTestRatesUpdate") &&
                m.Subject.Contains("2027") &&
                m.Body.Contains("bob@test.com")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Approved_SkipsEmail_WhenRecipientsNotConfigured()
    {
        var email    = Substitute.For<IGraphEmailService>();
        var settings = DefaultSettings();
        settings.ApprovedRecipients = string.Empty;
        var sut = CreateSut(email, settings);

        await sut.NotifyAsync(BulkRatesNotificationEvent.Approved, Context());

        await email.DidNotReceive().SendEmailAsync(Arg.Any<EmailMessageModel>(), Arg.Any<CancellationToken>());
    }

    // ── ReleasedForApproval ───────────────────────────────────────────────────

    [Fact]
    public async Task ReleasedForApproval_SendsToAllConfiguredRecipients()
    {
        var email = Substitute.For<IGraphEmailService>();
        var sut   = CreateSut(email);

        await sut.NotifyAsync(BulkRatesNotificationEvent.ReleasedForApproval, Context());

        await email.Received(1).SendEmailAsync(
            Arg.Is<EmailMessageModel>(m =>
                m.To.Contains("approver@test.com") &&
                m.To.Contains("approver2@test.com")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReleasedForApproval_AppliesTokensToSubjectAndBody()
    {
        var email = Substitute.For<IGraphEmailService>();
        var sut   = CreateSut(email);
        var ctx   = Context();

        await sut.NotifyAsync(BulkRatesNotificationEvent.ReleasedForApproval, ctx);

        await email.Received(1).SendEmailAsync(
            Arg.Is<EmailMessageModel>(m =>
                m.Subject.Contains("BulkTestRatesUpdate") &&
                m.Subject.Contains("2027") &&
                m.Body.Contains("alice@test.com") &&
                m.Body.Contains("11111111-1111-1111-1111-111111111111")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReleasedForApproval_SkipsEmail_WhenRecipientsNotConfigured()
    {
        var email    = Substitute.For<IGraphEmailService>();
        var settings = DefaultSettings();
        settings.ReleasedForApprovalRecipients = string.Empty;
        var sut = CreateSut(email, settings);

        await sut.NotifyAsync(BulkRatesNotificationEvent.ReleasedForApproval, Context());

        await email.DidNotReceive().SendEmailAsync(Arg.Any<EmailMessageModel>(), Arg.Any<CancellationToken>());
    }

    // ── Rejected ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Rejected_SendsToRequestedBy()
    {
        var email = Substitute.For<IGraphEmailService>();
        var sut   = CreateSut(email);
        var ctx   = Context(requestedBy: "alice@test.com", reason: "Wrong rates");

        await sut.NotifyAsync(BulkRatesNotificationEvent.Rejected, ctx);

        await email.Received(1).SendEmailAsync(
            Arg.Is<EmailMessageModel>(m =>
                m.To.SequenceEqual(new[] { "alice@test.com" }) &&
                m.Body.Contains("Wrong rates")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Rejected_SkipsEmail_WhenRequestedByIsEmpty()
    {
        var email = Substitute.For<IGraphEmailService>();
        var sut   = CreateSut(email);

        await sut.NotifyAsync(BulkRatesNotificationEvent.Rejected, Context(requestedBy: string.Empty));

        await email.DidNotReceive().SendEmailAsync(Arg.Any<EmailMessageModel>(), Arg.Any<CancellationToken>());
    }

    // ── Cancelled ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Cancelled_SendsToRequestedBy_WhenSubjectConfigured()
    {
        var email = Substitute.For<IGraphEmailService>();
        var sut   = CreateSut(email);
        var ctx   = Context(requestedBy: "alice@test.com", reason: "Mistake");

        await sut.NotifyAsync(BulkRatesNotificationEvent.Cancelled, ctx);

        await email.Received(1).SendEmailAsync(
            Arg.Is<EmailMessageModel>(m => m.To.SequenceEqual(new[] { "alice@test.com" })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cancelled_SkipsEmail_WhenSubjectNotConfigured()
    {
        var email    = Substitute.For<IGraphEmailService>();
        var settings = DefaultSettings();
        settings.CancelledSubject = string.Empty;
        var sut = CreateSut(email, settings);

        await sut.NotifyAsync(BulkRatesNotificationEvent.Cancelled, Context());

        await email.DidNotReceive().SendEmailAsync(Arg.Any<EmailMessageModel>(), Arg.Any<CancellationToken>());
    }

    // ── Worker-owned events ───────────────────────────────────────────────────

    [Theory]
    [InlineData(BulkRatesNotificationEvent.Completed)]
    [InlineData(BulkRatesNotificationEvent.Failed)]
    public async Task WorkerOwnedEvents_DoNotSendEmail(BulkRatesNotificationEvent ev)
    {
        var email = Substitute.For<IGraphEmailService>();
        var sut   = CreateSut(email);

        await sut.NotifyAsync(ev, Context());

        await email.DidNotReceive().SendEmailAsync(Arg.Any<EmailMessageModel>(), Arg.Any<CancellationToken>());
    }

    // ── Propagation ──────────────────────────────────────────────────────────

    [Fact]
    public async Task NotifyAsync_PropagatesEmailServiceException()
    {
        // The failure policy (swallow) is applied at the BulkRatesRequestService call site,
        // not here. The notification service itself must propagate to allow the caller to log.
        var email = Substitute.For<IGraphEmailService>();
        email.SendEmailAsync(Arg.Any<EmailMessageModel>(), Arg.Any<CancellationToken>())
             .ThrowsAsync(new InvalidOperationException("Graph API unavailable"));
        var sut = CreateSut(email);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.NotifyAsync(BulkRatesNotificationEvent.Rejected, Context()));
    }
}
