using Apha.BatchJobs.Domain.Constants;
using Serilog;
using Serilog.Core;

namespace Apha.BatchJobs.Worker.Bootstrap;

/// <summary>
/// Reports a failure that escaped the worker runner with the General marker so the BatchJobs alarm fires.
/// Falls back to the plain error stream when Serilog was never configured (failure before or during
/// logging setup); the metric filter matches raw text, so the fallback line still alerts.
/// </summary>
public static class StartupFailureReporter
{
    public static void Report(Exception exception, bool startupCompleted, ILogger logger, TextWriter fallback)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(fallback);

        var summary = startupCompleted
            ? "Batch worker failed with an unhandled exception"
            : "Batch worker failed during startup";

        if (ReferenceEquals(logger, Logger.None))
        {
            fallback.WriteLine(
                $"[{BatchExceptionMarkers.General}] {summary} | ExceptionType={exception.GetType().FullName} | Message={exception.Message}");
            fallback.WriteLine(exception.ToString());
            fallback.Flush();
            return;
        }

        logger.Fatal(
            exception,
            "[{ErrorType:l}] {Summary:l} | ExceptionType={ExceptionType:l}",
            BatchExceptionMarkers.General,
            summary,
            exception.GetType().FullName);
    }
}
