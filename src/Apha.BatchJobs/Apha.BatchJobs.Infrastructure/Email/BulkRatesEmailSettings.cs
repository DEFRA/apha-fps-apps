namespace Apha.BatchJobs.Infrastructure.Email;

public sealed class BulkRatesEmailSettings
{
    public const string SectionName = "BulkRatesEmail";

    public string CompletionRecipients { get; set; } = string.Empty;
    public string CompletionSubject { get; set; } = string.Empty;
    public string CompletionBody { get; set; } = string.Empty;
}
