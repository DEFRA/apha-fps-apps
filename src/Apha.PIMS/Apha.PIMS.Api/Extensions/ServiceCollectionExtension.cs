// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — ServiceCollectionExtension.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - Added IRadTrackInvoiceService / RadTrackInvoiceService scoped registration in AddServices().
 *   - Added IRadTrackInvoiceRepository / RadTrackInvoiceRepository scoped registration in AddRepositories().
 *   - Both registrations use AddScoped lifetime consistent with all existing service and repository
 *     pairs in this extension (EF Core DbContext is also scoped per-request).
 *
 * PRESERVED:
 *   - All existing service and repository registrations unchanged.
 *   - AddApplicationServices() orchestration method unchanged.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm RadTrackInvoiceRepository has no constructor dependencies beyond
 *     PimsDbContext (which is already registered via EF Core AddDbContext). If additional
 *     IFPSYearContext or other injection is required, verify the DI chain resolves correctly.
 */

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
            // TRANSFORMENGINE: RadTrackInvoice service registration added Phase 5.
            // Scoped lifetime matches all other PIMS service registrations.
            services.AddScoped<IRadTrackInvoiceService, RadTrackInvoiceService>();
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
            // TRANSFORMENGINE: RadTrackInvoice repository registration added Phase 5.
            // Scoped lifetime consistent with all other PIMS repository registrations.
            services.AddScoped<IRadTrackInvoiceRepository, RadTrackInvoiceRepository>();
            return services;
        }
    }
}
