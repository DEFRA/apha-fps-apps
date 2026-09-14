using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Compact;

namespace Apha.BatchJobs.Worker.Logging;

/// <summary>
/// Serilog setup for the batch worker host. Sinks are hardcoded per environment (matching the
/// sibling Apha.PACT/Apha.PIMS/Apha.FPS APIs) instead of a <c>Serilog:WriteTo</c> array in
/// appsettings.json. <c>ReadFrom.Configuration</c> is still used for MinimumLevel/overrides.
/// </summary>
public static class SerilogConfigurationExtensions
{
    /// <summary>
    /// Creates the Serilog logger and wires it into the host pipeline. Must run after
    /// <c>ConfigureWorkerConfiguration</c>, which resolves the legacy
    /// <c>BATCH_LOG_STREAM_PREFIX</c> variable into <c>Logging:LogStreamPrefix</c>.
    /// </summary>
    public static void ConfigureWorkerLogging(this HostApplicationBuilder builder)
    {
        // Diagnostic noise in production; only enable locally or when explicitly opted in.
        var selfLogEnabled = builder.Environment.IsDevelopment()
            || builder.Configuration.GetValue<bool>("Serilog:SelfLogEnabled");

        if (selfLogEnabled)
        {
            Serilog.Debugging.SelfLog.Enable(Console.Error);
        }

        var configuredLogStreamPrefix = builder.Configuration["Logging:LogStreamPrefix"] ?? "apha-batch";

        var loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "FPSBatchJobs")
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
            // "Configured" because this is intent metadata, not the actual ECS-assigned log stream id.
            .Enrich.WithProperty("ConfiguredLogStreamPrefix", configuredLogStreamPrefix);

        if (builder.Environment.IsEnvironment("local"))
        {
            var logPath = builder.Configuration.GetValue<string>("LogsPath") is { Length: > 0 } configuredPath
                ? Path.Combine(configuredPath, "Logsample.log")
                : Path.Combine("Logs", "Logsample.log");

            loggerConfiguration
                .WriteTo.Console()
                .WriteTo.File(logPath, Serilog.Events.LogEventLevel.Verbose, rollingInterval: RollingInterval.Day);
        }
        else
        {
            loggerConfiguration.WriteTo.Console(new RenderedCompactJsonFormatter());
        }

        Log.Logger = loggerConfiguration.CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger);
    }
}
