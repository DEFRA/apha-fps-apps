namespace Apha.BatchJobs.Infrastructure.Email;

public sealed class YearEndEmailSettings
{
    public const string SectionName = "YearEndEmail";

    public string Recipients { get; set; } = string.Empty;
    public string CompletionSubject { get; set; } = string.Empty;
    public string CompletionBody { get; set; } = string.Empty;
    public string FailureSubject { get; set; } = string.Empty;
    public string FailureBody { get; set; } = string.Empty;
}
