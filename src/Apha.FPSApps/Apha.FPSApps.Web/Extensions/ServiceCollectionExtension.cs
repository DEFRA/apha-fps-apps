using Apha.Common.Utilities.ExcelExport;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Services.FPS;
using Apha.FPSApps.Application.Services.PACT;
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
            services.AddScoped<IStaffJobService, StaffJobService>();
            services.AddTransient<RequestHeadersHandler>();
            services.AddScoped<IFpsYearContext, FpsYearContext>();
            services.AddScoped<IProgramService, ProgramService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IProjectJobCodeService, ProjectJobCodeService>();
            services.AddScoped<IPactTimeCodeValidService, PactTimeCodeValidService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IAnimalPlanService, AnimalPlanService>();
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<IWorkGroupTestCapabilityService, WorkGroupTestCapabilityService>();
            services.AddScoped<IExcelExportService, ExcelExportService>();
            return services;
        }
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services;
        }
    }
}
