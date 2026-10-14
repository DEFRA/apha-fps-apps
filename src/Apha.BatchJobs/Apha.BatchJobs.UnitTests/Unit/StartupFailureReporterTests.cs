using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Worker.Bootstrap;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Apha.BatchJobs.UnitTests.Unit;

public sealed class StartupFailureReporterTests
{
    [Fact]
    public void Report_WhenSerilogNotConfigured_WritesGeneralMarkerToFallback()
    {
        var fallback = new StringWriter();
        var exception = new InvalidOperationException("Connection string 'FPSConnectionString' not found.");

        StartupFailureReporter.Report(exception, startupCompleted: false, Logger.None, fallback);

        var firstLine = fallback.ToString().Split(Environment.NewLine)[0];
        Assert.StartsWith($"[{BatchExceptionMarkers.General}] Batch worker failed during startup", firstLine);
        Assert.Contains("ExceptionType=System.InvalidOperationException", firstLine);
        Assert.Contains("Connection string 'FPSConnectionString' not found.", firstLine);
    }

    [Fact]
    public void Report_WhenSerilogConfigured_LogsFatalWithGeneralMarker_AndSkipsFallback()
    {
        var sink = new CollectingSink();
        using var logger = new LoggerConfiguration().WriteTo.Sink(sink).CreateLogger();
        var fallback = new StringWriter();
        var exception = new InvalidOperationException("boom");

        StartupFailureReporter.Report(exception, startupCompleted: false, logger, fallback);

        var logEvent = Assert.Single(sink.Events);
        Assert.Equal(LogEventLevel.Fatal, logEvent.Level);
        Assert.Same(exception, logEvent.Exception);
        Assert.Equal(BatchExceptionMarkers.General, ((ScalarValue)logEvent.Properties["ErrorType"]).Value);
        Assert.Equal("Batch worker failed during startup", ((ScalarValue)logEvent.Properties["Summary"]).Value);
        Assert.Empty(fallback.ToString());
    }

    [Fact]
    public void Report_AfterStartupCompleted_UsesUnhandledExceptionWording()
    {
        var fallback = new StringWriter();

        StartupFailureReporter.Report(new InvalidOperationException("late"), startupCompleted: true, Logger.None, fallback);

        Assert.StartsWith(
            $"[{BatchExceptionMarkers.General}] Batch worker failed with an unhandled exception",
            fallback.ToString());
    }

    private sealed class CollectingSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = [];

        public void Emit(LogEvent logEvent) => Events.Add(logEvent);
    }
}
