namespace Apha.BatchJobs.Infrastructure.Email
{
    public interface IGraphEmailService
    {
        Task SendEmailAsync(EmailMessageModel message, CancellationToken cancellationToken = default);
    }
}
