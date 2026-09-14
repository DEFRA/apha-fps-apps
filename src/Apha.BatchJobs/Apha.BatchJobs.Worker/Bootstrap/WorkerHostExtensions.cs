using Apha.BatchJobs.Application.DependencyInjection;
using Apha.BatchJobs.Application.FailureHandling;
using Apha.BatchJobs.Infrastructure.DependencyInjection;
using Apha.BatchJobs.Worker.Configuration;
using Apha.BatchJobs.Worker.Execution;
using Apha.BatchJobs.Worker.Reporting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace Apha.BatchJobs.Worker.Bootstrap;

/// <summary>
/// Configuration, options, and service registration for the batch worker host.
/// Serilog setup lives separately in <c>Logging/SerilogConfigurationExtensions.cs</c>.
/// </summary>
public static class WorkerHostExtensions
{
    /// <summary>
    /// Layers <c>appsettings.Local.json</c> over the defaults, then re-adds environment
    /// variables so they still win.
    /// </summary>
    public static void ConfigureWorkerConfiguration(this HostApplicationBuilder builder)
    {
        builder.Configuration
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.Local.json"), optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();
    }

    public static void ConfigureWorkerServices(this HostApplicationBuilder builder)
    {
        builder.Services.AddBatchInfrastructure(builder.Configuration);
        builder.Services.AddBatchJobs(builder.Configuration);

        builder.Services
            .AddOptions<BatchRuntimeOptions>()
            .Bind(builder.Configuration.GetSection(BatchRuntimeOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddSingleton<BatchFailureClassifier>();
        builder.Services.AddSingleton<IBatchRunSummaryWriter, BatchRunSummaryWriter>();
        builder.Services.AddSingleton<BatchExecutionRequestResolver>();
        builder.Services.AddSingleton<IBatchWorkerRunner, BatchWorkerRunner>();
    }

    /// <summary>
    /// Stops the host within <see cref="BatchRuntimeOptions.GracefulShutdownWindowSeconds"/>,
    /// ahead of ECS's SIGTERM deadline. Reads config directly (not DI) so it still works if the
    /// host is in a partially-failed state; a shutdown failure never overrides the exit code.
    /// </summary>
    public static async Task StopSafelyAsync(this IHost host, IConfiguration configuration)
    {
        var gracefulShutdownWindowSeconds = configuration.GetValue<int?>(
            $"{BatchRuntimeOptions.SectionName}:GracefulShutdownWindowSeconds");

        if (gracefulShutdownWindowSeconds is not > 0)
            gracefulShutdownWindowSeconds = 25;

        using var stopCts = new CancellationTokenSource(TimeSpan.FromSeconds(gracefulShutdownWindowSeconds.Value));

        try
        {
            await host.StopAsync(stopCts.Token);
            Log.Information(
                "Host stopped | GracefulShutdownWindowSeconds={GracefulShutdownWindowSeconds}",
                gracefulShutdownWindowSeconds);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Host stop encountered an issue during shutdown");
        }
    }
}
