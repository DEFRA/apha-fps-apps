using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Worker.Bootstrap;
using Apha.BatchJobs.Worker.Execution;
using Apha.BatchJobs.Worker.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

CliArgumentAdapter.Apply(args);

var requestedJobName = Environment.GetEnvironmentVariable("BATCH_JOB_NAME");

// A null/empty BATCH_JOB_NAME means the ECS container override did not inject the env var —
// most likely an EventBridge input transformer misconfiguration (e.g. PascalCase JSON keys
// not matching the camelCase paths $.detail.jobName expected by the transformer). Fails before
// the host is built, since there is nothing a fully-built host could add to this diagnosis.
if (string.IsNullOrWhiteSpace(requestedJobName))
{
    Console.Error.WriteLine(
        $"[{BatchExceptionMarkers.General}] BATCH_JOB_NAME is not set. Cannot determine which job to run. " +
        "Verify the EventBridge input transformer maps $.detail.jobName → BATCH_JOB_NAME.");
    return BatchExitCodes.ConfigurationFailure;
}

// HealthCheck is a host-startup smoke check: it proves the container can build and start the
// real worker host (config, logging, DI graph) without touching the database, job_queue, or
// job_lock. It short-circuits after host.StartAsync() succeeds, before IBatchWorkerRunner (and
// therefore IJobOrchestrator) is ever resolved.
var isHealthCheck = string.Equals(requestedJobName, BatchJobNames.HealthCheck, StringComparison.OrdinalIgnoreCase);

HostApplicationBuilder? builder = null;
IHost? host = null;
var exitCode = BatchExitCodes.UnhandledFailure;
var startupCompleted = false;

try
{
    // Configuration, logging and DI setup stay inside the try so a failure here (e.g. a missing
    // connection string) is reported with the General marker instead of crashing silently.
    builder = Host.CreateApplicationBuilder(args);
    builder.ConfigureWorkerConfiguration();
    builder.ConfigureWorkerLogging();
    builder.ConfigureWorkerServices();

    host = builder.Build();
    await host.StartAsync();
    startupCompleted = true;

    if (isHealthCheck)
    {
        Log.Information(
            "HealthCheck OK | WorkerHostBuilt={WorkerHostBuilt} | WorkerHostStarted={WorkerHostStarted} | DatabaseChecked={DatabaseChecked} | OrchestratorResolved={OrchestratorResolved}",
            true, true, false, false);
        exitCode = BatchExitCodes.Success;
    }
    else
    {
        var runner = host.Services.GetRequiredService<IBatchWorkerRunner>();
        exitCode = await runner.RunAsync();
    }
}
catch (Exception ex)
{
    StartupFailureReporter.Report(ex, startupCompleted, Log.Logger, Console.Error);
}
finally
{
    if (host is not null)
    {
        await host.StopSafelyAsync(builder!.Configuration);
        host.Dispose();
    }

    await Log.CloseAndFlushAsync();
}

return exitCode;

