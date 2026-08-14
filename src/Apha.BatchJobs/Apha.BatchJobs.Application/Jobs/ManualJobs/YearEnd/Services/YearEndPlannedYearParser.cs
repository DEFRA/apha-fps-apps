using System.Text.Json;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Parses the Year End target-year field from a batch job parameters payload.
/// </summary>
public static class YearEndPlannedYearParser
{
    /// <summary>
    /// Parses <c>plannedYear</c> (the production event field), falling back to the legacy
    /// <c>targetFpsYear</c> alias when <c>plannedYear</c> is absent. Throws when both are present
    /// and disagree, so a malformed/ambiguous payload fails fast rather than silently picking one.
    /// </summary>
    public static int? Parse(string? parametersJson)
    {
        var plannedYear = TryReadInt(parametersJson, "plannedYear");
        var legacyTargetFpsYear = TryReadInt(parametersJson, "targetFpsYear");

        if (plannedYear.HasValue && legacyTargetFpsYear.HasValue && plannedYear.Value != legacyTargetFpsYear.Value)
        {
            throw new InvalidOperationException(
                $"Year End payload contains conflicting year values: plannedYear={plannedYear.Value}, " +
                $"targetFpsYear={legacyTargetFpsYear.Value}. These must agree or only one should be supplied.");
        }

        return plannedYear ?? legacyTargetFpsYear;
    }

    private static int? TryReadInt(string? json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (!doc.RootElement.TryGetProperty(propertyName, out var value))
            {
                return null;
            }

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var parsedInt))
            {
                return parsedInt;
            }

            if (value.ValueKind == JsonValueKind.String
                && int.TryParse(value.GetString(), out parsedInt))
            {
                return parsedInt;
            }
        }
        catch (JsonException)
        {
            // Parameters are validated in worker contract pre-check.
        }

        return null;
    }
}
