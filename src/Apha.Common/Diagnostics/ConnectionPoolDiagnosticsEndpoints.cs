using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Apha.Common.Diagnostics
{
    /// <summary>
    /// Endpoint mapping helpers that expose connection-pool diagnostics for download
    /// straight from the in-memory store. This works even when the web server denies
    /// file-write permission, because no disk access is required to serve the data.
    /// </summary>
    public static class ConnectionPoolDiagnosticsEndpoints
    {
        /// <summary>
        /// Maps GET endpoints:
        ///   {routePrefix}            -> CSV download of the in-memory snapshots
        ///   {routePrefix}/view       -> plain-text view in the browser
        /// Default prefix: /connection-pool-logs
        /// </summary>
        public static IEndpointRouteBuilder MapConnectionPoolDiagnostics(
            this IEndpointRouteBuilder endpoints,
            string routePrefix = "/connection-pool-logs")
        {
            endpoints.MapGet(routePrefix, (ConnectionPoolDiagnosticsStore store) =>
            {
                var csv = store.ToCsv();
                var bytes = Encoding.UTF8.GetBytes(csv);
                var fileName = $"connection-pool-diagnostics-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";
                return Results.File(bytes, "text/csv", fileName);
            });

            endpoints.MapGet($"{routePrefix}/view", (ConnectionPoolDiagnosticsStore store) =>
                Results.Text(store.ToCsv(), "text/plain"));

            return endpoints;
        }
    }
}
