using Apha.Common.Contracts.Email;

namespace Apha.Common.Utilities.Email
{
    public interface IGraphEmailService
    {
        Task SendEmailAsync(EmailMessageModel message, CancellationToken cancellationToken = default);
    }
}
