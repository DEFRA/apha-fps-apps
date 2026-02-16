using Apha.FPSApps.Application.Interfaces;
using Apha.FPSApps.Application.Services;
using Apha.FPSApps.Web.Handler;

namespace Apha.FPSApps.Web.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddServices();
            services.AddRepositories();
            return services;
        }
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            // Add your application services here
            services.AddScoped<IWeatherForecastService, WeatherForecastService>();
            services.AddTransient<FinancialYearHeaderHandler>();
            return services;
        }
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Add your data access services here
            return services;
        }
    }
}
