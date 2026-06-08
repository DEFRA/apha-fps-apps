using Apha.Common.Utilities.ExcelExport;
using Apha.Common.Utilities.StateManagement;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Services.Costbook;
using Apha.FPSApps.Application.Services.FPS;
using Apha.FPSApps.Application.Services.PACT;
using Apha.FPSApps.Application.Services.PIMS;
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
            // CostBook services - Following FPS pattern
            services.AddScoped<ICostBookProjectService, CostBookProjectService>();
            services.AddScoped<ICostBookCustomerService, CostBookCustomerService>();
            services.AddScoped<ICostBookDiseaseService, CostBookDiseaseService>();
            services.AddScoped<ICostBookProgramService, CostBookProgramService>();
            services.AddScoped<ICostBookStaffService, CostBookStaffService>();
            services.AddScoped<ICostBookContractService, CostBookContractService>();
            services.AddScoped<ICostBookYearlyDetailsService, CostBookYearlyDetailsService>();
            services.AddScoped<ICostBookProjectSummaryService, CostBookProjectSummaryService>();
            services.AddScoped<ICostBookSettingsService, CostBookSettingsService>();
            services.AddScoped<IYearMasterService, YearMasterService>();
            services.AddScoped<IDivisionService, DivisionService>();
            services.AddScoped<IWorkGroupGradeService, WorkGroupGradeService>();
            services.AddScoped<IProjectInvoiceService, ProjectInvoiceService>();
            services.AddScoped<IProjectSubContractService, ProjectSubContractService>();
            services.AddScoped<IMonthService, MonthService>();
            services.AddScoped<ICalenderMonthService, CalenderMonthService>();
            services.AddScoped<ITestCapabilityService, TestCapabilityService>();
            services.AddScoped<ITestRequirementService, TestRequirementService>();
            services.AddScoped<ITimeCostCalcsService, TimeCostCalcsService>();
            services.AddScoped<IExcelExportService, ExcelExportService>();
            services.AddScoped<IAppStateService, AppStateService>();
            services.AddScoped<IAdditionalCostService, AdditionalCostService>();
            // PIMS
            services.AddScoped<IProjectListService, ProjectListService>();
            services.AddScoped<IProjectDetailsService, ProjectDetailsService>();
            services.AddScoped<IProjectCommentService, ProjectCommentService>();
            services.AddScoped<IProposedProjectService, ProposedProjectService>();
            services.AddScoped<IProjectYearCostsService, ProjectYearCostsService>();

            services.AddScoped<IProfitCentreService, ProfitCentreService>();
            services.AddScoped<IProfitCentreGradeService, ProfitCentreGradeService>();
            services.AddScoped<IWorkGroupGradeService, WorkGroupGradeService>();
            services.AddScoped<IWorkGroupEmployeeService, WorkGroupEmployeeService>();
            services.AddScoped<IWorkgroupService, WorkgroupService>();

            services.AddScoped<IWorkGroupService, WorkGroupService>();
            services.AddScoped<ITestorProductService, TestorProductService>();
            services.AddScoped<IMonthlyOutputService, MonthlyOutputService>();
            services.AddScoped<IPactMonthlyOutputService, PactMonthlyOutputService>();
            services.AddScoped<IPactMonthlyTimeService, PactMonthlyTimeService>();
            services.AddScoped<IProjectProfileService, ProjectProfileService>();
            services.AddScoped<IProjectMonthService, ProjectMonthService>();
            services.AddScoped<IWorkGroupService, WorkGroupService>();
            services.AddScoped<IWorkGroupReportEmailService, WorkGroupReportEmailService>();
            services.AddScoped<IWorkGroupService, WorkGroupService>();
            services.AddScoped<IDivisionGradeService, DivisionGradeService>();
            services.AddScoped<IProjectStaffPlanService, ProjectStaffPlanService>();
            services.AddScoped<ISummarisedWorkgroupTimeService, SummarisedWgTimeService>();
            services.AddScoped<IAnimalService, AnimalService>();
            services.AddScoped<IRecreateAndReleaseSummaryService, RecreateAndReleaseSummaryService>();
            services.AddScoped<IReleaseSummaryService, ReleaseSummaryService>();
            return services;
        }
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services;
        }
    }
}