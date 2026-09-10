using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Apha.Common.Diagnostics
{
    /// <summary>
    /// Centralised connection-pool telemetry logger.
    /// Listens to the built-in .NET metrics for the database connection pool (Npgsql)
    /// and the outbound HTTP/API connection pool (System.Net.Http), then writes a periodic
    /// summary to the configured logger (Serilog). This helps identify whether performance
    /// issues are caused by DB pool exhaustion, HTTP connection saturation or load-balancer
    /// behaviour (e.g. long-lived idle connections, pending/queued requests).
    ///
    /// It reads existing instruments only - it does not create load or change pool sizes.
    /// </summary>
    public sealed class ConnectionPoolDiagnosticsService : BackgroundService
    {
        // Npgsql instrument names (System.Diagnostics.Metrics meter "Npgsql").
        private const string NpgsqlMeter = "Npgsql";

        // System.Net.Http instrument names (meter "System.Net.Http").
        private const string HttpMeter = "System.Net.Http";

        private readonly ILogger<ConnectionPoolDiagnosticsService> _logger;
        private readonly ConnectionPoolDiagnosticsOptions _options;
        private readonly ConnectionPoolDiagnosticsStore _store;
        private readonly ConnectionPoolDiagnosticsDbSink? _dbSink;
        private readonly MeterListener _meterListener;
        private readonly object _fileLock = new();
        private string? _csvFilePath;
        private bool _fileWritingDisabled;

        // Latest observed value per (instrument, tag-set). Thread-safe because the
        // MeterListener callbacks may run on the sampling thread while ExecuteAsync reads.
        private readonly ConcurrentDictionary<string, double> _latest = new();

        public ConnectionPoolDiagnosticsService(
            ILogger<ConnectionPoolDiagnosticsService> logger,
            IHostEnvironment environment,
            IConfiguration configuration,
            ConnectionPoolDiagnosticsStore store,
            ConnectionPoolDiagnosticsOptions? options = null)
        {
            _logger = logger;
            _options = options ?? new ConnectionPoolDiagnosticsOptions();
            _store = store;
            _meterListener = new MeterListener();

            if (_options.WriteToDatabase)
            {
                var connectionString = configuration.GetConnectionString(_options.ConnectionStringName);
                _dbSink = new ConnectionPoolDiagnosticsDbSink(
                    logger,
                    connectionString,
                    _options.TableName,
                    environment.ApplicationName ?? "App");
            }

            if (_options.WriteToCsv)
            {
                var csvPath = string.IsNullOrWhiteSpace(_options.CsvFilePath)
                    ? Path.Combine("Logs", "connection-pool-diagnostics.csv")
                    : _options.CsvFilePath;

                if (!Path.IsPathRooted(csvPath))
                {
                    csvPath = Path.Combine(environment.ContentRootPath, csvPath);
                }

                _csvFilePath = ResolveWritablePath(csvPath, environment);
            }
        }

        /// <summary>
        /// Verifies the target directory is writable. On a locked-down web server the app-pool
        /// account often cannot write to the app folder, so we fall back to the OS temp folder.
        /// If even that fails, file writing is disabled and snapshots are kept in memory only.
        /// </summary>
        private string? ResolveWritablePath(string preferredPath, IHostEnvironment environment)
        {
            if (TryEnsureWritable(preferredPath))
            {
                return preferredPath;
            }

            var fileName = Path.GetFileName(preferredPath);
            var fallback = Path.Combine(
                Path.GetTempPath(),
                "FpsDiagnostics",
                environment.ApplicationName ?? "App",
                fileName);

            if (TryEnsureWritable(fallback))
            {
                _logger.LogWarning(
                    "ConnectionPoolDiagnostics could not write to '{Preferred}'. Falling back to '{Fallback}'.",
                    preferredPath, fallback);
                return fallback;
            }

            _fileWritingDisabled = true;
            _logger.LogWarning(
                "ConnectionPoolDiagnostics has no writable location. Snapshots will be kept in memory only and are downloadable from the diagnostics endpoint.");
            return null;
        }

        private static bool TryEnsureWritable(string filePath)
        {
            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if (string.IsNullOrEmpty(directory))
                {
                    return false;
                }

                Directory.CreateDirectory(directory);

                // Probe with a throwaway file to confirm write permission.
                var probe = Path.Combine(directory, $".write-probe-{Guid.NewGuid():N}.tmp");
                File.WriteAllText(probe, "ok");
                File.Delete(probe);
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("ConnectionPoolDiagnostics is disabled.");
                return;
            }

            _meterListener.InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name is NpgsqlMeter or HttpMeter)
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            };

            _meterListener.SetMeasurementEventCallback<double>(OnMeasurement);
            _meterListener.SetMeasurementEventCallback<long>((inst, value, tags, state) =>
                OnMeasurement(inst, value, tags, state));
            _meterListener.SetMeasurementEventCallback<int>((inst, value, tags, state) =>
                OnMeasurement(inst, value, tags, state));

            _meterListener.Start();

            var interval = TimeSpan.FromSeconds(Math.Max(5, _options.SampleIntervalSeconds));

            _logger.LogInformation(
                "ConnectionPoolDiagnostics started. Sampling every {IntervalSeconds}s.",
                interval.TotalSeconds);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Force the observable instruments (pool gauges) to publish current values.
                    _meterListener.RecordObservableInstruments();
                    await LogSnapshotAsync(stoppingToken);

                    await Task.Delay(interval, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown.
            }
        }

        private void OnMeasurement<T>(
            Instrument instrument,
            T measurement,
            ReadOnlySpan<KeyValuePair<string, object?>> tags,
            object? state) where T : struct
        {
            var key = BuildKey(instrument, tags);
            _latest[key] = Convert.ToDouble(measurement);
        }

        private static string BuildKey(
            Instrument instrument,
            ReadOnlySpan<KeyValuePair<string, object?>> tags)
        {
            if (tags.Length == 0)
            {
                return instrument.Name;
            }

            var tagText = string.Empty;
            foreach (var tag in tags)
            {
                // Only keep low-cardinality tags relevant to pooling (state, host, pool name).
                if (tag.Key is "state" or "server.address" or "pool.name" or "db.name" or "http.connection.state")
                {
                    tagText += $"{tag.Key}={tag.Value};";
                }
            }

            return tagText.Length == 0 ? instrument.Name : $"{instrument.Name}[{tagText}]";
        }

        private async Task LogSnapshotAsync(CancellationToken cancellationToken)
        {
            if (_latest.IsEmpty)
            {
                _logger.LogInformation(
                    "ConnectionPoolDiagnostics: no pool metrics captured yet (no DB/HTTP activity or instruments not available).");
                return;
            }

            var dbMetrics = _latest
                .Where(kv => kv.Key.StartsWith("db.", StringComparison.OrdinalIgnoreCase))
                .OrderBy(kv => kv.Key)
                .Select(kv => $"{kv.Key}={kv.Value:0.##}")
                .ToList();

            var httpMetrics = _latest
                .Where(kv => kv.Key.StartsWith("http.", StringComparison.OrdinalIgnoreCase))
                .OrderBy(kv => kv.Key)
                .Select(kv => $"{kv.Key}={kv.Value:0.##}")
                .ToList();

            _logger.LogInformation(
                "ConnectionPoolDiagnostics snapshot | DB_POOL: {DbPool} | API_POOL: {ApiPool}",
                dbMetrics.Count > 0 ? string.Join(", ", dbMetrics) : "n/a",
                httpMetrics.Count > 0 ? string.Join(", ", httpMetrics) : "n/a");

            var timestampUtc = DateTime.UtcNow;
            var dbText = dbMetrics.Count > 0 ? string.Join(" | ", dbMetrics) : string.Empty;
            var httpText = httpMetrics.Count > 0 ? string.Join(" | ", httpMetrics) : string.Empty;

            // Always keep snapshots in memory so they remain downloadable even when the
            // server denies file-write permission.
            _store.Add(timestampUtc, dbText, httpText);

            if (_dbSink is { IsEnabled: true })
            {
                await _dbSink.WriteAsync(timestampUtc, dbText, httpText, cancellationToken);
            }

            WriteCsvRow(timestampUtc, dbText, httpText);
        }

        private void WriteCsvRow(DateTime timestampUtc, string dbText, string httpText)
        {
            if (_csvFilePath is null || _fileWritingDisabled)
            {
                return;
            }

            try
            {
                var row = string.Join(",",
                    Csv(timestampUtc.ToString("o", CultureInfo.InvariantCulture)),
                    Csv(dbText),
                    Csv(httpText));

                lock (_fileLock)
                {
                    EnsureHeader();
                    File.AppendAllText(_csvFilePath, row + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                // Permissions may change at runtime; stop retrying disk writes but keep
                // collecting in memory so downloads still work.
                _fileWritingDisabled = true;
                _logger.LogWarning(ex,
                    "ConnectionPoolDiagnostics failed to write CSV snapshot to '{Path}'. Disk logging disabled; snapshots remain available in memory.",
                    _csvFilePath);
            }
        }

        private void EnsureHeader()
        {
            var directory = Path.GetDirectoryName(_csvFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(_csvFilePath))
            {
                var header = string.Join(",", "TimestampUtc", "DbPool", "ApiPool");
                File.AppendAllText(_csvFilePath!, header + Environment.NewLine, Encoding.UTF8);
            }
        }

        // Escapes a value for safe inclusion in a CSV field.
        private static string Csv(string? value)
        {
            value ??= string.Empty;
            value = value.Replace("\"", "\"\"", StringComparison.Ordinal);
            return $"\"{value}\"";
        }

        public override void Dispose()
        {
            _meterListener.Dispose();
            base.Dispose();
        }
    }
}
