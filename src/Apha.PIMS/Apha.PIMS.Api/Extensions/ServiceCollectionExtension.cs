using Apha.Common.Utilities.ExcelExport;
using Apha.Common.Utilities.StateManagement;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Services;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.DataAccess.Context;
using Apha.PIMS.DataAccess.Repository;
namespace Apha.PIMS.Api.Extensions
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
            services.AddScoped<IProjectListService, ProjectListService>();
            services.AddScoped<IProposedProjectService, ProposedProjectService>();
            services.AddScoped<IProjectDetailsService, ProjectDetailsService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IProjectYearCostsService, ProjectYearCostsService>();
            services.AddScoped<IMilestoneService, MilestoneService>();
            services.AddScoped<IRadTrackInvoiceService, RadTrackInvoiceService>();       
            services.AddScoped<IYearlyFinancialDataService, YearlyFinancialDataService>();
            services.AddScoped<IRadTrackInvoiceService, RadTrackInvoiceService>();

            // TRANSFORMENGINE: Phase 5 additions — Report, ReportGroup, ReportGroupLink
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IReportGroupService, ReportGroupService>();
            services.AddScoped<IReportGroupLinkService, ReportGroupLinkService>();

            // TRANSFORMENGINE: Phase 5 additions — ProjectManager, ProgramManagerLink, ProfitCentreManagerLink
            services.AddScoped<IProjectManagerService, ProjectManagerService>();
            services.AddScoped<IProgramManagerLinkService, ProgramManagerLinkService>();
            services.AddScoped<IProfitCentreManagerLinkService, ProfitCentreManagerLinkService>();

            // TRANSFORMENGINE: Phase 5 additions — Setting
            services.AddScoped<ISettingService, SettingService>();

            // TRANSFORMENGINE: Phase 5 additions — Access* services
            services.AddScoped<IAccessUserService, AccessUserService>();
            services.AddScoped<IAccessLevelService, AccessLevelService>();
            services.AddScoped<IAccessUserLevelService, AccessUserLevelService>();
            services.AddScoped<IAccessSystemService, AccessSystemService>();

            // TRANSFORMENGINE: Phase 5 additions — Frequency, ReviewItem
            services.AddScoped<IFrequencyService, FrequencyService>();
            services.AddScoped<IReviewItemService, ReviewItemService>();

            // TRANSFORMENGINE: Phase 5 additions — RadTrackProg (Programme Tab); natural string PK (program varchar(10))
            services.AddScoped<IRadTrackProgService, RadTrackProgService>();

            // Risk rating lookup maintenance
            services.AddScoped<IRiskService, RiskService>();

            // Publication type lookup maintenance
            services.AddScoped<IPublicationTypeService, PublicationTypeService>();

            return services;
        }
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Add your data access services here
            services.AddScoped<IFPSYearContext, FPSYearContext>();
            services.AddScoped<IProjectListRepository, ProjectListRepository>();
            services.AddScoped<IProposedProjectRepository, ProposedProjectRepository>();
            services.AddScoped<IProjectDetailsRepository, ProjectDetailsRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IProjectYearCostsRepository, ProjectYearCostsRepository>();
            services.AddScoped<IMilestoneRepository, MilestoneRepository>();
            services.AddScoped<IRadTrackInvoiceRepository, RadTrackInvoiceRepository>();
            services.AddScoped<IYearlyFinancialDataRepository, YearlyFinancialDataRepository>();

            // TRANSFORMENGINE: Phase 5 additions — Report, ReportGroup, ReportGroupLink
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IReportGroupRepository, ReportGroupRepository>();
            services.AddScoped<IReportGroupLinkRepository, ReportGroupLinkRepository>();

            // TRANSFORMENGINE: Phase 5 additions — ProjectManager, ProgramManagerLink, ProfitCentreManagerLink
            services.AddScoped<IProjectManagerRepository, ProjectManagerRepository>();
            services.AddScoped<IProgramManagerLinkRepository, ProgramManagerLinkRepository>();
            services.AddScoped<IProfitCentreManagerLinkRepository, ProfitCentreManagerLinkRepository>();

            // TRANSFORMENGINE: Phase 5 additions — Setting
            services.AddScoped<ISettingRepository, SettingRepository>();

            // TRANSFORMENGINE: Phase 5 additions — Access* repositories
            services.AddScoped<IAccessUserRepository, AccessUserRepository>();
            services.AddScoped<IAccessLevelRepository, AccessLevelRepository>();
            services.AddScoped<IAccessUserLevelRepository, AccessUserLevelRepository>();
            services.AddScoped<IAccessSystemRepository, AccessSystemRepository>();

            // TRANSFORMENGINE: Phase 5 additions — Frequency, ReviewItem
            services.AddScoped<IFrequencyRepository, FrequencyRepository>();
            services.AddScoped<IReviewItemRepository, ReviewItemRepository>();

            // TRANSFORMENGINE: Phase 5 additions — RadTrackProg (Programme Tab); natural string PK (program varchar(10))
            services.AddScoped<IRadTrackProgRepository, RadTrackProgRepository>();

            // Risk rating lookup maintenance
            services.AddScoped<IRiskRepository, RiskRepository>();

            // Publication type lookup maintenance
            services.AddScoped<IPublicationTypeRepository, PublicationTypeRepository>();

            return services;
        }
    }
}
