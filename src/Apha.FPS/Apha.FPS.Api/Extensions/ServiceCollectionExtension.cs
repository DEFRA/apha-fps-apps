using Apha.Common.Utilities.Cache;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Context;
using Apha.FPS.DataAccess.Repositories;

namespace Apha.FPS.Api.Extensions
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
            services.AddScoped<ICacheService, CacheService>();
            return services;
        }
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Add your data access services here
            services.AddScoped<IYearContext, YearContext>();
            services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();
            return services;
        }
    }
}
