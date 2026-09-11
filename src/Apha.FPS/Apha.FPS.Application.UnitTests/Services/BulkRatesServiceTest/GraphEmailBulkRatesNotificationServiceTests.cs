using Apha.Common.Constants;
using Apha.Common.Contracts.Email;
using Apha.Common.Utilities.Email;
using Apha.FPS.Application.Common.BulkRates;
using Apha.FPS.Application.Email;
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

    // Mirrors the real appsettings.json BulkRatesEmailSettings content exactly, so this
    // fixture and production config can never silently diverge.
    private static BulkRatesEmailSettings DefaultSettings() => new()
    {
        ReleasedForApprovalRecipients = "approver@test.com,approver2@test.com",
        ReleasedForApprovalSubject    = "Approval Required – Bulk {RateType} Rates Update",
        ReleasedForApprovalBody       = "The Bulk {RateType} Rates Update has been submitted for approval.\n\nYour timely action is appreciated.",
        ApprovedRecipients            = "initiator@test.com",
        ApprovedSubject               = "Bulk {RateType} Rates Update Approved Successfully",
        ApprovedBody                  = "The Bulk {RateType} Rates Update has been approved successfully.\n\nThank you for your support.",
        RejectedSubject               = "Bulk {RateType} Rates Update Rejected",
        RejectedBody                  = "The Bulk {RateType} Rates Update has been rejected.\n\nPlease review the details and take necessary action.\n\nThank you for your support.",
        CancelledSubject              = "Bulk {RateType} Rates Update Cancelled",
        CancelledBody                 = "The Bulk {RateType} Rates Update has been cancelled.\n\nNo further action is required.\n\nThank you for your support."
    };

    private static BulkRatesNotificationContext Context(
        string jobName = BulkRatesJobNames.Fec,
        string requestedBy = "alice@test.com",
        string? reason = null,
        string? approvedBy = null) => new()
    {
        JobQueueId  = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        JobName     = jobName,
        FpsYear     = 2027,
        RequestedBy = requestedBy,
        Reason      = reason,
        ApprovedBy  = approvedBy
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
                m.Subject == "Bulk Test Rates Update Approved Successfully" &&
                m.Body == "The Bulk Test Rates Update has been approved successfully.\n\nThank you for your support."),
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
    public async Task ReleasedForApproval_RendersRateTypeIntoSubjectAndBody()
    {
        var email = Substitute.For<IGraphEmailService>();
        var sut   = CreateSut(email);
        var ctx   = Context();

        await sut.NotifyAsync(BulkRatesNotificationEvent.ReleasedForApproval, ctx);

        await email.Received(1).SendEmailAsync(
            Arg.Is<EmailMessageModel>(m =>
                m.Subject == "Approval Required – Bulk Test Rates Update" &&
                m.Body == "The Bulk Test Rates Update has been submitted for approval.\n\nYour timely action is appreciated."),
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
                m.Subject == "Bulk Test Rates Update Rejected" &&
                m.Body == "The Bulk Test Rates Update has been rejected.\n\nPlease review the details and take necessary action.\n\nThank you for your support." &&
                !m.Body.Contains("Wrong rates")), // deliberately no rejection reason in the body
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
            Arg.Is<EmailMessageModel>(m =>
                m.To.SequenceEqual(new[] { "alice@test.com" }) &&
                m.Subject == "Bulk Test Rates Update Cancelled" &&
                m.Body == "The Bulk Test Rates Update has been cancelled.\n\nNo further action is required.\n\nThank you for your support." &&
                !m.Body.Contains("Mistake")), // reason is not part of the Cancelled body either
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

    // ── Rate-type × event rendering matrix (3 rate types × 4 events) ─────────
    // Locks the exact agreed business copy down, per rate type, rather than only
    // checking that {RateType} was substituted with *something*.

    public static IEnumerable<object[]> RateTypeEventMatrix()
    {
        (string JobName, string RateType)[] rateTypes =
        [
            (BulkRatesJobNames.Fec, "Test"),
            (BulkRatesJobNames.Staff, "Staff"),
            (BulkRatesJobNames.Animal, "Animal")
        ];

        foreach (var (jobName, rateType) in rateTypes)
        {
            yield return new object[]
            {
                BulkRatesNotificationEvent.ReleasedForApproval, jobName,
                $"Approval Required – Bulk {rateType} Rates Update",
                $"The Bulk {rateType} Rates Update has been submitted for approval.\n\nYour timely action is appreciated."
            };
            yield return new object[]
            {
                BulkRatesNotificationEvent.Approved, jobName,
                $"Bulk {rateType} Rates Update Approved Successfully",
                $"The Bulk {rateType} Rates Update has been approved successfully.\n\nThank you for your support."
            };
            yield return new object[]
            {
                BulkRatesNotificationEvent.Rejected, jobName,
                $"Bulk {rateType} Rates Update Rejected",
                $"The Bulk {rateType} Rates Update has been rejected.\n\nPlease review the details and take necessary action.\n\nThank you for your support."
            };
            yield return new object[]
            {
                BulkRatesNotificationEvent.Cancelled, jobName,
                $"Bulk {rateType} Rates Update Cancelled",
                $"The Bulk {rateType} Rates Update has been cancelled.\n\nNo further action is required.\n\nThank you for your support."
            };
        }
    }

    [Theory]
    [MemberData(nameof(RateTypeEventMatrix))]
    public async Task NotifyAsync_RendersExactSubjectAndBody_AsPlainText(
        BulkRatesNotificationEvent notificationEvent, string jobName, string expectedSubject, string expectedBody)
    {
        var email = Substitute.For<IGraphEmailService>();
        var sut   = CreateSut(email);
        var ctx   = Context(jobName: jobName);

        await sut.NotifyAsync(notificationEvent, ctx);

        await email.Received(1).SendEmailAsync(
            Arg.Is<EmailMessageModel>(m =>
                m.Subject == expectedSubject &&
                m.Body == expectedBody &&
                m.IsBodyHtml == false),
            Arg.Any<CancellationToken>());
    }

    // ── Unknown job name ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(BulkRatesNotificationEvent.ReleasedForApproval)]
    [InlineData(BulkRatesNotificationEvent.Approved)]
    [InlineData(BulkRatesNotificationEvent.Rejected)]
    [InlineData(BulkRatesNotificationEvent.Cancelled)]
    public async Task NotifyAsync_UnknownJobName_ThrowsAndSendsNoEmail(BulkRatesNotificationEvent notificationEvent)
    {
        var email = Substitute.For<IGraphEmailService>();
        var sut   = CreateSut(email);
        var ctx   = Context(jobName: "SomeFutureBulkRatesJob");

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => sut.NotifyAsync(notificationEvent, ctx));

        await email.DidNotReceive().SendEmailAsync(Arg.Any<EmailMessageModel>(), Arg.Any<CancellationToken>());
    }

    // ── No technical content leaks into Subject/Body ─────────────────────────
    // Asserts actual rendered values (not just token names) never appear — stronger than
    // checking for literal "{JobQueueId}" etc., since a renderer could substitute the
    // token and still leak the value. Recipient (`To`) is intentionally excluded: for
    // Rejected/Cancelled the recipient is legitimately ctx.RequestedBy.

    public static IEnumerable<object[]> AllEventsAndJobNames()
    {
        foreach (var ev in new[]
                 {
                     BulkRatesNotificationEvent.ReleasedForApproval,
                     BulkRatesNotificationEvent.Approved,
                     BulkRatesNotificationEvent.Rejected,
                     BulkRatesNotificationEvent.Cancelled
                 })
        foreach (var jobName in new[] { BulkRatesJobNames.Fec, BulkRatesJobNames.Staff, BulkRatesJobNames.Animal })
            yield return new object[] { ev, jobName };
    }

    [Theory]
    [MemberData(nameof(AllEventsAndJobNames))]
    public async Task NotifyAsync_DoesNotLeakTechnicalContentIntoSubjectOrBody(
        BulkRatesNotificationEvent notificationEvent, string jobName)
    {
        var email = Substitute.For<IGraphEmailService>();
        var sut   = CreateSut(email);
        var ctx   = new BulkRatesNotificationContext
        {
            JobQueueId  = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            JobName     = jobName,
            FpsYear     = 2099,
            RequestedBy = "leak-requestedby@test.com",
            ApprovedBy  = "leak-approvedby@test.com",
            Reason      = "Leak-Reason-Marker"
        };

        await sut.NotifyAsync(notificationEvent, ctx);

        await email.Received(1).SendEmailAsync(
            Arg.Is<EmailMessageModel>(m =>
                !m.Subject.Contains(ctx.JobQueueId.ToString()) && !m.Body.Contains(ctx.JobQueueId.ToString()) &&
                !m.Subject.Contains(ctx.FpsYear.ToString()) && !m.Body.Contains(ctx.FpsYear.ToString()) &&
                !m.Subject.Contains(ctx.RequestedBy) && !m.Body.Contains(ctx.RequestedBy) &&
                !m.Subject.Contains(ctx.ApprovedBy!) && !m.Body.Contains(ctx.ApprovedBy!) &&
                !m.Subject.Contains(ctx.Reason!) && !m.Body.Contains(ctx.Reason!) &&
                !m.Subject.Contains(jobName) && !m.Body.Contains(jobName)),
            Arg.Any<CancellationToken>());
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
