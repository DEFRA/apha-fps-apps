// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — PimsViewModelMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 10 — AutoMapper Profiles + DI Registration (Step 15)
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - Added RadTrack Invoice view-model mappings (Step 15b):
 *       CreateMap<InvoiceItem, RadTrackInvoiceDto>().ReverseMap()
 *       CreateMap<InvoiceViewModel, RadTrackInvoiceDto>().ReverseMap()
 *   - InvoiceItem and InvoiceViewModel are created in Phase 11
 *     (src/Apha.FPSApps/Apha.FPSApps.Web/Areas/PIMS/Models/InvoiceItem.cs and InvoiceViewModel.cs).
 *     The using directive Apha.FPSApps.Web.Areas.PIMS.Models already covers these types.
 *
 * PRESERVED:
 *   - All 19 existing CreateMap entries (PaginationFilter, ProjectList, ProposedProject,
 *     ProjectDetails, ProjectComment, costs plan/actuals, PactPay, MonthlyPact,
 *     Milestone, MilestoneFormDates, LogMilestone) unchanged.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: InvoiceItem and InvoiceViewModel must be created in Phase 11
 *     before this mapper compiles. Property names must match RadTrackInvoiceDto exactly
 *     for convention-based mapping; add .ForMember() overrides if they diverge.
 *   - TRANSFORMENGINE TODO: InvoiceTotalsItem (totals footer) maps to RadTrackInvoiceTotalsDto —
 *     add CreateMap<InvoiceTotalsItem, RadTrackInvoiceTotalsDto>().ReverseMap() in Phase 11
 *     once InvoiceTotalsItem is defined.
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;

namespace Apha.FPSApps.Web.Mappings
{
    public class PimsViewModelMapper : Profile
    {
        public PimsViewModelMapper()
        {
            CreateMap(typeof(PaginationFilter<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap<PaginationModel, PaginationDto>().ReverseMap();

            CreateMap<ProjectListItem, ProjectListViewDto>().ReverseMap();
            CreateMap<ProjectListViewModel, ProposedProjectDto>().ReverseMap();
            CreateMap<ProposedProjectViewModel, ProposedProjectDto>().ReverseMap();
            CreateMap<ProjectDetailsViewModel, ProjectDetailDto>().ReverseMap();
            CreateMap<ProjectDetailsViewModel, ProposedProjectDto>().ReverseMap();
            CreateMap<ProjectCommentItem, CommentDto>().ReverseMap();
            // Plan grid item — maps from plan fields on the shared DTO
            CreateMap<AdditionalCostDto, AdditionalCostPlanItem>().ReverseMap();

            // Actuals grid item — maps from actuals fields on the shared DTO
            CreateMap<AdditionalCostDto, AdditionalCostActualItem>().ReverseMap();

            // Animal Plan grid item
            CreateMap<AnimalCostDto, AnimalCostPlanItem>().ReverseMap();

            // Animal Actuals grid item
            CreateMap<AnimalCostDto, AnimalCostActualItem>().ReverseMap();

            // Test Plan grid item
            CreateMap<TestCostDto, TestCostPlanItem>().ReverseMap();

            // Test Actuals grid item
            CreateMap<TestCostDto, TestCostActualItem>().ReverseMap();

            // Staff Plan grid item
            CreateMap<StaffCostDto, StaffCostPlanItem>().ReverseMap();

            // Staff Actuals grid item
            CreateMap<StaffCostDto, StaffCostActualItem>().ReverseMap();

            // Pact Pay grid item
            CreateMap<PactPayDto, PactPayItem>().ReverseMap();

            // Monthly Pact Data grid item
            CreateMap<MonthlyPactDto, MonthlyPactItem>().ReverseMap();
            CreateMap<MilestoneItem, MilestoneDto>().ReverseMap();
            CreateMap<MilestoneFormDatesItem, MilestoneFormDatesDto>().ReverseMap();
            CreateMap<LogMilestoneItem, LogMilestoneDto>().ReverseMap();

            // TRANSFORMENGINE: RadTrack Invoice — Step 15b (Phase 10)
            // InvoiceItem: grid row ↔ RadTrackInvoiceDto (list display and inline edit).
            // InvoiceViewModel: page-level form ↔ RadTrackInvoiceDto (add/edit modal binding).
            // NOTE: InvoiceItem and InvoiceViewModel are created in Phase 11; this mapper
            //       will not compile until those types exist in Apha.FPSApps.Web.Areas.PIMS.Models.
            CreateMap<InvoiceItem, RadTrackInvoiceDto>().ReverseMap();
            CreateMap<InvoiceViewModel, RadTrackInvoiceDto>().ReverseMap();
        }
    }
}
