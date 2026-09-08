using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Entities.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Infrastructure.Email;

/// <summary>
/// Implementation of <see cref="IEmailService"/> that wraps another <see cref="IEmailService"/>
/// and, when explicitly configured, redirects every recipient to one fixed test mailbox instead
/// of the real recipient list. Always registered — behavior is governed entirely by
/// <see cref="EmailDeliverySettings.RedirectEnabled"/>, never by environment name or any other
/// implicit signal. The flag is the sole authority.
/// </summary>
public sealed class EmailRedirectDecorator : IEmailService
{
    private readonly IEmailService _inner;
    private readonly EmailDeliverySettings _settings;
    private readonly ILogger<EmailRedirectDecorator> _logger;

    public EmailRedirectDecorator(
        IEmailService inner,
        IOptions<EmailDeliverySettings> settings,
        ILogger<EmailRedirectDecorator> logger)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!_settings.RedirectEnabled)
            return _inner.SendAsync(message, cancellationToken);

        if (string.IsNullOrWhiteSpace(_settings.RedirectTo))
        {
            throw new InvalidOperationException(
                "EmailDelivery:RedirectEnabled is true but EmailDelivery:RedirectTo is empty — " +
                "refusing to send to the real recipient list.");
        }

        var originalRecipients = string.Join(", ", message.To);
        var redirected = message with
        {
            To = [_settings.RedirectTo],
            Subject = $"[REDIRECTED - would send to: {originalRecipients}] {message.Subject}"
        };

        _logger.LogInformation(
            "EmailDelivery:RedirectEnabled is true — redirecting email from {OriginalRecipientCount} " +
            "real recipient(s) to the configured redirect address",
            message.To.Count);

        return _inner.SendAsync(redirected, cancellationToken);
    }
}
