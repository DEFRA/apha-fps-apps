using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Entities.Email;
using Apha.BatchJobs.Infrastructure.Email;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

public sealed class EmailRedirectDecoratorTests
{
    [Fact]
    public void Constructor_WhenInnerIsNull_ShouldThrowArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new EmailRedirectDecorator(
                null!,
                Options.Create(new EmailDeliverySettings()),
                NullLogger<EmailRedirectDecorator>.Instance));

        Assert.Equal("inner", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenSettingsIsNull_ShouldThrowArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new EmailRedirectDecorator(
                Substitute.For<IEmailService>(),
                null!,
                NullLogger<EmailRedirectDecorator>.Instance));

        Assert.Equal("settings", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new EmailRedirectDecorator(
                Substitute.For<IEmailService>(),
                Options.Create(new EmailDeliverySettings()),
                null!));

        Assert.Equal("logger", ex.ParamName);
    }

    [Fact]
    public async Task SendAsync_WhenNullMessage_ShouldThrowArgumentNullException()
    {
        var decorator = CreateDecorator(out _, RedirectSettings());

        await Assert.ThrowsAsync<ArgumentNullException>(() => decorator.SendAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task SendAsync_WhenRedirectDisabled_ShouldPassThroughUnchanged()
    {
        // Default settings: RedirectEnabled is false with no other configuration at all —
        // must send to the real recipient regardless of anything else.
        var decorator = CreateDecorator(out var inner, new EmailDeliverySettings());
        var message = new EmailMessage(["real.manager@example.com"], "Subject", "<p>body</p>", IsBodyHtml: true);

        await decorator.SendAsync(message, CancellationToken.None);

        await inner.Received(1).SendAsync(message, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_WhenRedirectEnabled_ShouldRedirectToConfiguredAddress()
    {
        var decorator = CreateDecorator(out var inner, RedirectSettings());
        var message = new EmailMessage(["real.manager@example.com"], "Milestone and Deliverable Update Request", "<p>body</p>", IsBodyHtml: true);

        await decorator.SendAsync(message, CancellationToken.None);

        await inner.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m =>
                m.To.SequenceEqual(new[] { "test.mailbox@example.com" }) &&
                m.Subject.Contains("real.manager@example.com") &&
                m.Subject.Contains("Milestone and Deliverable Update Request")),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SendAsync_WhenRedirectEnabled_ShouldPreserveIsBodyHtml(bool isBodyHtml)
    {
        var decorator = CreateDecorator(out var inner, RedirectSettings());
        var message = new EmailMessage(["real.manager@example.com"], "Subject", "body", isBodyHtml);

        await decorator.SendAsync(message, CancellationToken.None);

        await inner.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m => m.IsBodyHtml == isBodyHtml),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_WhenRedirectEnabled_ButRedirectToEmpty_ShouldThrow_AndNeverCallInner()
    {
        var settings = new EmailDeliverySettings { RedirectEnabled = true, RedirectTo = "" };
        var decorator = CreateDecorator(out var inner, settings);
        var message = new EmailMessage(["real.manager@example.com"], "Subject", "<p>body</p>", IsBodyHtml: true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => decorator.SendAsync(message, CancellationToken.None));

        await inner.DidNotReceive().SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_WhenRedirectEnabled_ButRedirectToWhitespace_ShouldThrow_AndNeverCallInner()
    {
        var settings = new EmailDeliverySettings { RedirectEnabled = true, RedirectTo = "   " };
        var decorator = CreateDecorator(out var inner, settings);
        var message = new EmailMessage(["real.manager@example.com"], "Subject", "<p>body</p>", IsBodyHtml: true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => decorator.SendAsync(message, CancellationToken.None));

        await inner.DidNotReceive().SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }

    private static EmailDeliverySettings RedirectSettings() => new()
    {
        RedirectEnabled = true,
        RedirectTo = "test.mailbox@example.com"
    };

    private static EmailRedirectDecorator CreateDecorator(out IEmailService inner, EmailDeliverySettings settings)
    {
        inner = Substitute.For<IEmailService>();
        inner.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(EmailSendResult.Sent()));

        return new EmailRedirectDecorator(inner, Options.Create(settings), NullLogger<EmailRedirectDecorator>.Instance);
    }
}
