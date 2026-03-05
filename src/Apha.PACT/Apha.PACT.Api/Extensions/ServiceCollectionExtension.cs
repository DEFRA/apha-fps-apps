using Apha.Common.Utilities.StateManagement;
using Apha.Common.Utilities.ExcelExport;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Services;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Context;
using Apha.PACT.DataAccess.Repositories;

namespace Apha.PACT.Api.Extensions
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
            services.AddScoped<IAppStateService, AppStateService>();
            services.AddScoped<IExcelExportService, ExcelExportService>();
            return services;
        }
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Add your data access services here
            services.AddScoped<IFPSYearContext, FPSYearContext>();
            services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();
            return services;
        }
    }
}
