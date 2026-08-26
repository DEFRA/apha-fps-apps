using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace Apha.BatchJobs.Infrastructure.Email
{
    public class GraphEmailService : IGraphEmailService
    {
        private readonly GraphServiceClient _graphClient;
        private readonly string _sharedMailbox;

        public GraphEmailService(GraphServiceClient graphClient, IConfiguration config)
        {
            _graphClient = graphClient;
            _sharedMailbox = config["GraphEmailSettings:SharedMailbox"]
                ?? throw new ArgumentNullException(nameof(config), "SharedMailbox config missing");
        }

        public async Task SendEmailAsync(EmailMessageModel message, CancellationToken cancellationToken = default)
        {
            var mail = new Message
            {
                Subject = message.Subject,
                Body = new ItemBody
                {
                    ContentType = message.IsBodyHtml ? BodyType.Html : BodyType.Text,
                    Content = message.Body
                },
                ToRecipients = message.To
                    .Select(e => new Recipient { EmailAddress = new EmailAddress { Address = e } })
                    .ToList(),
            };

            if (message.Cc is { Count: > 0 })
            {
                mail.CcRecipients = message.Cc
                    .Select(e => new Recipient { EmailAddress = new EmailAddress { Address = e } })
                    .ToList();
            }

            if (message.Bcc is { Count: > 0 })
            {
                mail.BccRecipients = message.Bcc
                    .Select(e => new Recipient { EmailAddress = new EmailAddress { Address = e } })
                    .ToList();
            }

            if (message.Attachments is { Count: > 0 })
            {
                mail.Attachments = message.Attachments
                    .Select(a => new FileAttachment
                    {
                        Name = a.FileName,
                        ContentBytes = a.ContentBytes,
                        ContentType = a.ContentType
                    })
                    .Cast<Attachment>()
                    .ToList();
            }

            await _graphClient
                .Users[_sharedMailbox]
                .SendMail
                .PostAsync(new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
                {
                    Message = mail,
                    SaveToSentItems = true
                }, cancellationToken: cancellationToken);
        }
    }
}
