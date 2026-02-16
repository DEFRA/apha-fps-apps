using Amazon.CloudWatchLogs;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.AwsCloudWatch;


namespace Apha.FPSApps.Web.Extensions
{
    public static class SerilogExtensions
    {
        public static LoggerConfiguration UseStructuredConsoleLogging(this LoggerConfiguration loggerConfiguration)
        {
            return loggerConfiguration
               .Enrich.FromLogContext()
               .WriteTo.Console(new RenderedCompactJsonFormatter()); // Structured JSON to stdout               

        }
    }
}
