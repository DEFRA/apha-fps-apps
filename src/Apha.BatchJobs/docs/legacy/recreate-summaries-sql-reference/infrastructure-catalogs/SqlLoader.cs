namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

/// <summary>
/// Loads RecreateSummaries SQL files from the
/// <c>Infrastructure/Sql/RecreateSummaries/</c> directory at runtime.
/// Files must be set as <c>CopyToOutputDirectory = Always</c> or
/// <c>EmbeddedResource</c> in the project file.
/// </summary>
internal static class SqlLoader
{
    private static readonly string SqlDirectory = Path.Combine(
        AppContext.BaseDirectory, "Apha.BatchJobs.Infrastructure", "Sql", "RecreateSummaries");

    /// <summary>
    /// Reads and returns the content of the named SQL file.
    /// Throws <see cref="FileNotFoundException"/> if the file is missing.
    /// </summary>
    internal static string Load(string fileName)
    {
        var path = Path.Combine(SqlDirectory, fileName);
        if (!File.Exists(path))
            throw new FileNotFoundException(
                $"RecreateSummaries SQL file not found: {path}", path);

        return File.ReadAllText(path);
    }
}
