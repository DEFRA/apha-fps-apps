using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Apha.Common.Contracts;
using Microsoft.Extensions.Logging;

namespace Apha.FPSApps.Web.Handler
{
    /// <summary>
    /// Centralised performance logger for all outbound API calls (FPS, PACT, PIMS, CostBook).
    /// Attached as a <see cref="DelegatingHandler"/> to every named HttpClient, it records the
    /// target API, method, URL, status code, start/end time and total duration and posts the
    /// entry to the target API's <c>/performance-logs</c> endpoint, which persists it to the
    /// centralised <c>performance_log</c> table. Posting is fire-and-forget and uses a
    /// non-instrumented HttpClient so it never recurses through this handler.
    /// </summary>
    public sealed class ApiPerformanceLoggingHandler : DelegatingHandler
    {
        private const string LogClientName = "PerformanceLogClient";
        private const string LogTypeApi = "Api";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ApiPerformanceLoggingHandler> _logger;
        private readonly long _slowCallThresholdMs;
        private readonly bool _enabled;
        // Configured API base URLs (name + host + optional path base) used to build the log
        // endpoint and resolve the API name for whichever API is being called, so it works in
        // every environment (local, dev, UAT, prod), including when APIs share a gateway host.
        private readonly IReadOnlyList<(string Name, Uri BaseUrl)> _apiEndpoints;

        public ApiPerformanceLoggingHandler(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<ApiPerformanceLoggingHandler> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _enabled = configuration.GetValue("ApiPerformanceLogging:Enabled", false);
            _slowCallThresholdMs = configuration.GetValue("ApiPerformanceLogging:SlowCallThresholdMs", 0L);

            var endpoints = new List<(string Name, Uri BaseUrl)>();
            foreach (var (name, key) in new[]
            {
                ("FPS", "FPSApiSettings:BaseUrl"),
                ("PACT", "PACTApiSettings:BaseUrl"),
                ("PIMS", "PIMSApiSettings:BaseUrl"),
                ("CostBook", "CostBookApiSettings:BaseUrl")
            })
            {
                var value = configuration[key];
                if (!string.IsNullOrWhiteSpace(value)
                    && Uri.TryCreate(value, UriKind.Absolute, out var baseUri))
                {
                    endpoints.Add((name, baseUri));
                }
            }

            _apiEndpoints = endpoints;
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
                        : ((int)response.StatusCode).ToString();

                    PostLog(request, startTimeUtc, endTimeUtc, durationMs, statusCode, outcome);
                }
            }
        }

        private void PostLog(
            HttpRequestMessage request,
            DateTime startTimeUtc,
            DateTime endTimeUtc,
            long durationMs,
            string statusCode,
            string outcome)
        {
            var uri = request.RequestUri;
            if (uri is null)
            {
                return;
            }

            var correlationId = request.Headers.TryGetValues("X-Correlation-ID", out var cid)
                ? string.Join("", cid)
                : string.Empty;

            var payload = new PerformanceLogReq
            {
                LogType = LogTypeApi,
                StartTimeUtc = startTimeUtc,
                EndTimeUtc = endTimeUtc,
                DurationMs = durationMs,
                ApiName = ResolveApiName(uri),
                HttpMethod = request.Method.Method,
                StatusCode = statusCode,
                Outcome = outcome,
                Url = uri.ToString(),
                CorrelationId = correlationId
            };

            // Build the log endpoint relative to the matching configured API base URL so any
            // host and path base (e.g. https://gateway/fps-api/) is preserved in every
            // environment. Falls back to the request's own authority if no base matches.
            var logEndpoint = BuildLogEndpoint(uri);

            // Fire-and-forget: never block or fail the original call because of diagnostics.
            _ = Task.Run(async () =>
            {
                try
                {
                    var client = _httpClientFactory.CreateClient(LogClientName);
                    var json = JsonSerializer.Serialize(payload);
                    using var content = new StringContent(json, Encoding.UTF8, "application/json");
                    using var response = await client.PostAsync(logEndpoint, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning(
                            "Performance-log POST to {Endpoint} returned {StatusCode}.",
                            logEndpoint, (int)response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    // Diagnostics logging must never surface errors to the app, but log so it is visible.
                    _logger.LogWarning(ex, "Performance-log POST to {Endpoint} failed.", logEndpoint);
                }
            });
        }

        private Uri BuildLogEndpoint(Uri requestUri)
        {
            var match = MatchEndpoint(requestUri);
            if (match is { } baseUrl)
            {
                var normalizedBase = baseUrl.AbsoluteUri.EndsWith('/')
                    ? baseUrl
                    : new Uri(baseUrl.AbsoluteUri + "/");

                return new Uri(normalizedBase, "performance-logs");
            }

            // Fallback: same scheme/host as the request.
            return new Uri($"{requestUri.Scheme}://{requestUri.Authority}/performance-logs");
        }

        private string ResolveApiName(Uri requestUri)
        {
            foreach (var (name, baseUrl) in _apiEndpoints)
            {
                if (IsMatch(requestUri, baseUrl))
                {
                    return name;
                }
            }

            return requestUri.Host;
        }

        private Uri? MatchEndpoint(Uri requestUri)
        {
            foreach (var (_, baseUrl) in _apiEndpoints)
            {
                if (IsMatch(requestUri, baseUrl))
                {
                    return baseUrl;
                }
            }

            return null;
        }

        private static bool IsMatch(Uri requestUri, Uri baseUrl)
        {
            // Match by authority (host + port) and, when the base URL has a path segment,
            // also require the request path to start with it. This distinguishes APIs that
            // share a gateway host but differ by path base (e.g. /fps-api, /pact-api).
            if (!requestUri.Authority.Equals(baseUrl.Authority, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var basePath = baseUrl.AbsolutePath.TrimEnd('/');
            return basePath.Length == 0
                || requestUri.AbsolutePath.StartsWith(basePath + "/", StringComparison.OrdinalIgnoreCase)
                || requestUri.AbsolutePath.Equals(basePath, StringComparison.OrdinalIgnoreCase);
        }
    }
}
