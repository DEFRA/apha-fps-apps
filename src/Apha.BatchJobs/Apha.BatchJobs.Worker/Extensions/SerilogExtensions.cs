using Serilog;
using Serilog.Formatting.Compact;

namespace Apha.BatchJobs.Worker.Extensions;

/// <summary>
/// Serilog bootstrap helpers for the batch jobs worker.
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// Adds structured console logging for non-local environments.
    /// </summary>
    public static LoggerConfiguration UseStructuredConsoleLogging(this LoggerConfiguration loggerConfiguration)
    {
        return loggerConfiguration
            .Enrich.FromLogContext()
            .WriteTo.Console(new RenderedCompactJsonFormatter());
    }
}
