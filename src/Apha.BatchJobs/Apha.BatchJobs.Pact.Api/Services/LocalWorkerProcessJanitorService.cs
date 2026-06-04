using System.Diagnostics;
using System.Text.Json;
using Apha.BatchJobs.Pact.Api.Options;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Pact.Api.Services;

public sealed class LocalWorkerProcessJanitorService : BackgroundService
{
    private readonly TriggerDispatchOptions _options;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalWorkerProcessJanitorService> _logger;

    public LocalWorkerProcessJanitorService(
        IOptions<TriggerDispatchOptions> options,
        IWebHostEnvironment environment,
        ILogger<LocalWorkerProcessJanitorService> logger)
    {
        _options = options.Value;
        _environment = environment;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.LocalWorker.EnableOrphanProcessReaper)
        {
            return;
        }

        if (!(_environment.IsDevelopment() || _environment.IsEnvironment("Local")))
        {
            return;
        }

        var intervalSeconds = Math.Max(5, _options.LocalWorker.ReaperIntervalSeconds);
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalSeconds));

        await ReapStaleProcessesAsync(stoppingToken);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ReapStaleProcessesAsync(stoppingToken);
        }
    }

    private Task ReapStaleProcessesAsync(CancellationToken cancellationToken)
    {
        var metadataDirectory = LocalWorkerProcessRegistry.GetMetadataDirectory(_environment.ContentRootPath);
        if (!Directory.Exists(metadataDirectory))
        {
            return Task.CompletedTask;
        }

        var maxLifetime = TimeSpan.FromMinutes(Math.Max(1, _options.LocalWorker.MaxWorkerLifetimeMinutes));
        var utcNow = DateTime.UtcNow;

        foreach (var metadataPath in Directory.GetFiles(metadataDirectory, "pid-*.json"))
        {
            cancellationToken.ThrowIfCancellationRequested();

            LocalWorkerProcessRecord? record = null;
            try
            {
                var json = File.ReadAllText(metadataPath);
                record = JsonSerializer.Deserialize<LocalWorkerProcessRecord>(json);
            }
            catch
            {
                // Corrupt metadata should not block reaper; delete and continue.
                TryDeleteMetadata(metadataPath);
                continue;
            }

            if (record is null || record.Pid <= 0)
            {
                TryDeleteMetadata(metadataPath);
                continue;
            }

            Process? process = null;
            try
            {
                process = Process.GetProcessById(record.Pid);
            }
            catch
            {
                TryDeleteMetadata(metadataPath);
                continue;
            }

            try
            {
                if (process.HasExited)
                {
                    TryDeleteMetadata(metadataPath);
                    continue;
                }

                var age = utcNow - record.StartedAtUtc;
                if (age <= maxLifetime)
                {
                    continue;
                }

                process.Kill(entireProcessTree: true);
                _logger.LogWarning(
                    "Killed stale local worker process | WorkerPid={WorkerPid} | JobName={JobName} | JobExecutionId={JobExecutionId} | AgeMinutes={AgeMinutes}",
                    record.Pid,
                    record.JobName,
                    record.JobExecutionId,
                    age.TotalMinutes);
                TryDeleteMetadata(metadataPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to inspect or kill stale local worker process | WorkerPid={WorkerPid} | MetadataPath={MetadataPath}",
                    record.Pid,
                    metadataPath);
            }
            finally
            {
                process.Dispose();
            }
        }

        return Task.CompletedTask;
    }

    private static void TryDeleteMetadata(string metadataPath)
    {
        try
        {
            if (File.Exists(metadataPath))
            {
                File.Delete(metadataPath);
            }
        }
        catch
        {
            // Ignore cleanup failure; next cycle can retry.
        }
    }
}
