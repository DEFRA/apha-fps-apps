using System.Data.Common;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Resolves the current FPS year live from <c>fps.tblyearmaster</c>, shared by both Data Setup
/// (<see cref="ValidateYearEndContextStep"/>) and CutOver (<see cref="YearEndCutoverService"/>) so
/// neither depends on a <c>currentFpsYear</c> value carried in the trigger payload.
/// </summary>
internal static class YearEndYearContextResolver
{
    /// <summary>
    /// Requires exactly one <c>Open</c> row in <c>fps.tblyearmaster</c> — not the latest of several
    /// via <c>ORDER BY ... LIMIT 1</c>. No <c>active</c> filter: an Open-but-inactive row is bad
    /// year-master state that must surface as an ambiguity error, not be silently filtered out.
    /// </summary>
    public static async Task<int> ResolveCurrentFpsYearAsync(
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction,
            "SELECT fpsyear FROM fps.tblyearmaster WHERE yearstatus = 'Open';");

        var openYears = new List<int>();
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                openYears.Add(reader.GetInt32(0));
            }
        }

        if (openYears.Count == 0)
        {
            throw new InvalidOperationException(
                "No Open year found in fps.tblyearmaster. Year End cannot determine the current year.");
        }

        if (openYears.Count > 1)
        {
            throw new InvalidOperationException(
                $"Multiple Open years found in fps.tblyearmaster ({string.Join(", ", openYears)}); " +
                "ambiguous current year state.");
        }

        return openYears[0];
    }
}
