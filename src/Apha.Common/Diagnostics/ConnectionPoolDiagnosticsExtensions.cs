using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Apha.Common.Diagnostics
{
    /// <summary>
    /// Registration helpers for centralised connection-pool diagnostics logging.
    /// Call <see cref="AddConnectionPoolDiagnostics"/> from each app's service configuration
    /// (FPS, PIMS, PACT, Costbook APIs and FPSApps.Web) to log DB and API pool health.
    /// </summary>
    public static class ConnectionPoolDiagnosticsExtensions
    {
        public static IServiceCollection AddConnectionPoolDiagnostics(
            this IServiceCollection services, IConfiguration configuration)
        {
            var options = new ConnectionPoolDiagnosticsOptions();
            configuration.GetSection(ConnectionPoolDiagnosticsOptions.SectionName).Bind(options);

            services.AddSingleton(options);
            services.AddSingleton(new ConnectionPoolDiagnosticsStore(options.MaxInMemoryRows));
            services.AddHostedService<ConnectionPoolDiagnosticsService>();

            return services;
        }
    }
}
