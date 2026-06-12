using Apha.BatchJobs.Domain.Interfaces;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Apha.BatchJobs.Infrastructure.Context;

/// <summary>
/// Scoped holder for RecreateSummaries run parameters.
/// </summary>
public sealed class RecreateSummariesContext : IRecreateSummariesContext
{
    /// <summary>
    /// Initializes context from optional environment overrides.
    /// Defaults remain month=1 and triggeredBy=system for backward compatibility.
    /// </summary>
    public RecreateSummariesContext()
    {
        var parametersJson = Environment.GetEnvironmentVariable("BATCH_JOB_PARAMETERS_JSON");
        if (!string.IsNullOrWhiteSpace(parametersJson))
        {
            ApplyParametersJson(parametersJson);
        }

        var monthOverride = Environment.GetEnvironmentVariable("BATCH_RECREATE_SUMMARIES_MONTH");
        if (string.IsNullOrWhiteSpace(parametersJson)
            && int.TryParse(monthOverride, out var parsedMonth)
            && parsedMonth is >= 1 and <= 12)
        {
            Month = parsedMonth;
        }

        var yearOverride = Environment.GetEnvironmentVariable("BATCH_RECREATE_SUMMARIES_YEAR");
        if (string.IsNullOrWhiteSpace(parametersJson)
            && int.TryParse(yearOverride, out var parsedYear)
            && parsedYear is >= 2000 and <= 9999)
        {
            Year = parsedYear;
        }

        var triggeredByOverride = Environment.GetEnvironmentVariable("BATCH_REQUESTED_BY");

        if (!string.IsNullOrWhiteSpace(triggeredByOverride))
        {
            TriggeredBy = triggeredByOverride.Trim();
        }
    }

    /// <inheritdoc />
    public int Month { get; set; } = 1;

    /// <inheritdoc />
    public int Year { get; set; } = DateTime.UtcNow.Year;

    /// <inheritdoc />
    public string TriggeredBy { get; set; } = "system";

    private void ApplyParametersJson(string parametersJson)
    {
        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(parametersJson);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("BATCH_JOB_PARAMETERS_JSON must be valid JSON.", ex);
        }

        using (doc)
        {
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException("BATCH_JOB_PARAMETERS_JSON root type must be a JSON object.");
            }

            if (!doc.RootElement.TryGetProperty("month", out var monthElement)
                || monthElement.ValueKind != JsonValueKind.String)
            {
                throw new InvalidOperationException("RecreateSummaries requires month in parametersJson with format YYYY-MM.");
            }

            var monthValue = monthElement.GetString();
            if (string.IsNullOrWhiteSpace(monthValue)
                || !Regex.IsMatch(monthValue, "^\\d{4}-(0[1-9]|1[0-2])$"))
            {
                throw new InvalidOperationException("RecreateSummaries month must use format YYYY-MM.");
            }

            var parts = monthValue.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            Year = int.Parse(parts[0]);
            Month = int.Parse(parts[1]);
        }
    }
}
