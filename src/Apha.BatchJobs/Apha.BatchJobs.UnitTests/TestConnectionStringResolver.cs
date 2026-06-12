using Microsoft.Extensions.Configuration;

namespace Apha.BatchJobs.UnitTests;

internal static class TestConnectionStringResolver
{
    public static string ResolveForTests(string fallbackConnectionString)
    {
        var batchJobsRoot = FindBatchJobsRoot();

        var configBuilder = new ConfigurationBuilder();
        if (batchJobsRoot is not null)
        {
            configBuilder
                .SetBasePath(batchJobsRoot)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.Local.json", optional: true)
                .AddJsonFile(Path.Combine("Apha.BatchJobs.Worker", "appsettings.json"), optional: true)
                .AddJsonFile(Path.Combine("Apha.BatchJobs.Worker", "appsettings.Development.json"), optional: true)
                .AddJsonFile(Path.Combine("Apha.BatchJobs.Worker", "appsettings.Local.json"), optional: true);
        }

        configBuilder.AddEnvironmentVariables();
        var config = configBuilder.Build();

        return config.GetConnectionString("FPSConnectionString")
            ?? config.GetConnectionString("BatchJobsConnectionString")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__FPSConnectionString")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__BatchJobsConnectionString")
            ?? fallbackConnectionString;
    }

    private static string? FindBatchJobsRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var hasSln = File.Exists(Path.Combine(current.FullName, "Apha.BatchJobs.sln"));
            var hasWorkerFolder = Directory.Exists(Path.Combine(current.FullName, "Apha.BatchJobs.Worker"));
            if (hasSln || hasWorkerFolder)
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return null;
    }
}