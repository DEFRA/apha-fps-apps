namespace Apha.BatchJobs.Infrastructure.Email
{
    public class EmailMessageModel
    {
        public List<string> To { get; set; } = new();
        public List<string>? Cc { get; set; }
        public List<string>? Bcc { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsBodyHtml { get; set; } = true;
        public List<EmailAttachmentModel>? Attachments { get; set; }
    }

    public class EmailAttachmentModel
    {
        public string FileName { get; set; } = string.Empty;
        public byte[] ContentBytes { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "application/octet-stream";
    }
}
