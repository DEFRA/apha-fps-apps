namespace Apha.BatchJobs.Infrastructure.Email;

public sealed class MabArchiveEmailSettings
{
    public const string SectionName = "MabArchiveEmail";

    public string Recipients { get; set; } = string.Empty;
    public string CompletionSubject { get; set; } = string.Empty;
    public string CompletionBody { get; set; } = string.Empty;
    public string FailureSubject { get; set; } = string.Empty;
    public string FailureBody { get; set; } = string.Empty;
}
