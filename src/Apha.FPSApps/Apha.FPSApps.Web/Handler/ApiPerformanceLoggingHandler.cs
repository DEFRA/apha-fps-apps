using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Apha.FPSApps.Web.Handler
{
    /// <summary>
    /// Centralised performance logger for all outbound API calls (FPS, PACT, PIMS, CostBook).
    /// Attached as a <see cref="DelegatingHandler"/> to every named HttpClient, it records the
    /// target API, method, URL, status code, start/end time and total duration to a single CSV
    /// file so cross-application performance issues can be identified from one place.
    /// </summary>
    public sealed class ApiPerformanceLoggingHandler : DelegatingHandler
    {
        private static readonly object FileLock = new();
        private readonly string _csvFilePath;
        private readonly long _slowCallThresholdMs;
        private readonly bool _enabled;

        public ApiPerformanceLoggingHandler(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _enabled = configuration.GetValue("ApiPerformanceLogging:Enabled", false);

            var csvPath = configuration["ApiPerformanceLogging:CsvFilePath"];
            if (string.IsNullOrWhiteSpace(csvPath))
            {
                csvPath = Path.Combine("Logs", "api-performance.csv");
            }

            if (!Path.IsPathRooted(csvPath))
            {
                csvPath = Path.Combine(environment.ContentRootPath, csvPath);
            }

            _csvFilePath = csvPath;
            _slowCallThresholdMs = configuration.GetValue("ApiPerformanceLogging:SlowCallThresholdMs", 0L);

            if (_enabled)
            {
                EnsureHeader();
            }
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!_enabled)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            var startTimeUtc = DateTime.UtcNow;
            var stopwatch = Stopwatch.StartNew();
            HttpResponseMessage? response = null;
            string outcome = "OK";

            try
            {
                response = await base.SendAsync(request, cancellationToken);
                return response;
            }
            catch (Exception ex)
            {
                outcome = ex.GetType().Name;
                throw;
            }
            finally
            {
                stopwatch.Stop();
                var durationMs = stopwatch.ElapsedMilliseconds;

                if (durationMs >= _slowCallThresholdMs)
                {
                    var endTimeUtc = DateTime.UtcNow;
                    var statusCode = response is null
                        ? string.Empty
                        : ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture);

                    WriteEntry(request, startTimeUtc, endTimeUtc, durationMs, statusCode, outcome);
                }
            }
        }

        private void WriteEntry(
            HttpRequestMessage request,
            DateTime startTimeUtc,
            DateTime endTimeUtc,
            long durationMs,
            string statusCode,
            string outcome)
        {
            var api = ResolveApiName(request.RequestUri);
            var correlationId = request.Headers.TryGetValues("X-Correlation-ID", out var cid)
                ? string.Join("", cid)
                : string.Empty;

            var row = string.Join(",",
                Csv(startTimeUtc.ToString("o", CultureInfo.InvariantCulture)),
                Csv(endTimeUtc.ToString("o", CultureInfo.InvariantCulture)),
                Csv(durationMs.ToString(CultureInfo.InvariantCulture)),
                Csv(api),
                Csv(request.Method.Method),
                Csv(statusCode),
                Csv(outcome),
                Csv(request.RequestUri?.ToString() ?? string.Empty),
                Csv(correlationId));

            lock (FileLock)
            {
                File.AppendAllText(_csvFilePath, row + Environment.NewLine, Encoding.UTF8);
            }
        }

        private static string ResolveApiName(Uri? uri)
        {
            var host = uri?.Host ?? string.Empty;
            if (host.Contains("pact", StringComparison.OrdinalIgnoreCase)) return "PACT";
            if (host.Contains("pims", StringComparison.OrdinalIgnoreCase)) return "PIMS";
            if (host.Contains("costbook", StringComparison.OrdinalIgnoreCase)) return "CostBook";
            if (host.Contains("fps", StringComparison.OrdinalIgnoreCase)) return "FPS";
            return host;
        }

        private void EnsureHeader()
        {
            lock (FileLock)
            {
                var directory = Path.GetDirectoryName(_csvFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (!File.Exists(_csvFilePath))
                {
                    var header = string.Join(",",
                        "StartTimeUtc",
                        "EndTimeUtc",
                        "DurationMs",
                        "Api",
                        "Method",
                        "StatusCode",
                        "Outcome",
                        "Url",
                        "CorrelationId");

                    File.AppendAllText(_csvFilePath, header + Environment.NewLine, Encoding.UTF8);
                }
            }
        }

        // Escapes a value for safe inclusion in a CSV field.
        private static string Csv(string? value)
        {
            value ??= string.Empty;
            value = value.Replace("\"", "\"\"", StringComparison.Ordinal);
            return $"\"{value}\"";
        }
    }
}
