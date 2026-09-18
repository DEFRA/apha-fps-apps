namespace Apha.BatchJobs.Application.FailureHandling;

/// <summary>
/// Builds a bounded, best-effort diagnostic summary for the Failed-transition <c>job_queue_log</c>
/// row. Not sanitization — a "best-effort diagnostic summary", not a guarantee the text is scrubbed
/// of internal detail. <c>job_queue.errormessage</c> gets the friendly message from
/// <see cref="BatchFailureMessageProvider"/> instead, never this; full exception detail (including
/// stack trace) always remains in application logs, correlatable by JobExecutionId. Must never throw
/// while describing the original failure — a diagnostic-summary bug must not mask or replace the
/// real failure being recorded.
/// </summary>
public static class DiagnosticSummaryBuilder
{
    /// <summary><c>job_queue_log.note</c> is <c>varchar(500)</c> — the persistence constraint this truncates to.</summary>
    public const int MaxLength = 500;

    private const string FallbackMessage = "Execution failed with no further detail available.";

    public static string Build(Exception exception, int maxLength = MaxLength)
    {
        try
        {
            var message = exception.Message;
            if (string.IsNullOrWhiteSpace(message))
                message = exception.GetBaseException().Message;

            if (string.IsNullOrWhiteSpace(message))
                return FallbackMessage;

            var normalized = NormalizeWhitespace(message);

            return normalized.Length <= maxLength
                ? normalized
                : normalized[..maxLength];
        }
        catch
        {
            return FallbackMessage;
        }
    }

    /// <summary>Collapses embedded newlines/indentation so one audit-log row stays a single readable line.</summary>
    private static string NormalizeWhitespace(string text)
    {
        var lines = text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var trimmedLines = lines.Select(line => line.Trim()).Where(line => line.Length > 0);
        var joined = string.Join(' ', trimmedLines);
        return string.IsNullOrWhiteSpace(joined) ? text.Trim() : joined;
    }
}
