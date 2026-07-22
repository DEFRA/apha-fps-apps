/*
 * TRANSFORMENGINE MIGRATION — ServiceCollectionExtension.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - Verified Phase 5 DI registrations complete; no new registrations required
 *   - TransformEngine header added
 *
 * PRESERVED:
 *   - All existing scoped service registrations (IAppStateService, IProjectListService,
 *     IProposedProjectService, IProjectDetailsService, ICommentService,
 *     IProjectYearCostsService, IMilestoneService, IRadTrackInvoiceService, IExcelExportService)
 *   - All existing scoped repository registrations (IFPSYearContext, IProjectListRepository,
 *     IProposedProjectRepository, IProjectDetailsRepository, ICommentRepository,
 *     IProjectYearCostsRepository, IMilestoneRepository, IRadTrackInvoiceRepository)
 *   - AddApplicationServices() orchestration method calling AddServices() + AddRepositories()
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
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
            // TRANSFORMENGINE: ICommentService registered — supports ProjectCommentController comment CRUD + topic filter
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IProjectYearCostsService, ProjectYearCostsService>();
            services.AddScoped<IMilestoneService, MilestoneService>();
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
            // TRANSFORMENGINE: ICommentRepository registered — backing store for ICommentService; topic filter support added in Phase 4
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IProjectYearCostsRepository, ProjectYearCostsRepository>();
            services.AddScoped<IMilestoneRepository, MilestoneRepository>();
            services.AddScoped<IRadTrackInvoiceRepository, RadTrackInvoiceRepository>();
            return services;
        }
    }
}
