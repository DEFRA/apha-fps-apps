using Apha.Common.Utilities.StateManagement;
using Apha.Common.Utilities.ExcelExport;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Context;
using Apha.FPS.DataAccess.Repositories;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;

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
            services.AddScoped<IAppStateService, AppStateService>();
            services.AddScoped<IExcelExportService, ExcelExportService>();
            services.AddScoped<IStaffJobService, StaffJobService>();
            services.AddScoped<IFpsSettingService, FpsSettingService>();
            services.AddScoped<IAnimalService, AnimalService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IProgramService, ProgramService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IStatusService, StatusService>();
            services.AddScoped<IDiseaseService, DiseaseService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<IYearMasterService, YearMasterService>();
            services.AddScoped<IProjectGroupService, ProjectGroupService>();
            services.AddScoped<ITimeCostCalcsService, TimeCostCalcsService>();
            return services;
        }
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Add your data access services here
            services.AddScoped<IFpsRequestContext, FpsRequestContext>();
            services.AddScoped<IProgramRepository, ProgramRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IStaffJobRepository, StaffJobRepository>();
            services.AddScoped<IFpsSettingRepository, FpsSettingRepository>(); 
            services.AddScoped<IAnimalRepository, AnimalRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IStatusRepository, StatusRepository>();
            services.AddScoped<IDiseaseRepository, DiseaseRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IContractRepository, ContractRepository>();
            services.AddScoped<IYearMasterRepository, YearMasterRepository>();
            services.AddScoped<IProjectGroupRepository, ProjectGroupRepository>();
            services.AddScoped<ITimeCostCalcsRepository, TimeCostCalcsRepository>();
            return services;
        }
    }
}
