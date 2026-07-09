/*
 * TRANSFORMENGINE MIGRATION — PimsViewModelMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 10 — AutoMapper Profiles + DI Registration (Step 15)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - Added YearlyFinancialDataItem <-> YearlyFinancialDataDto (.ReverseMap)
 *       Grid row item ↔ frontend application DTO for the Yearly Financial Data grid
 *   - Added PactCostsItem <-> PactProjectYearCostsDto (.ReverseMap)
 *       PACT costs panel item ↔ frontend application DTO for "Update Costing" panel display
 *
 * PRESERVED:
 *   - All pre-existing PIMS view-model mapper entries (ProjectList, ProjectDetails, Comments,
 *     AdditionalCost Plan/Actual, AnimalCost Plan/Actual, TestCost Plan/Actual, StaffCost Plan/Actual,
 *     PactPay, MonthlyPact, Milestone, MilestoneFormDates, LogMilestone, Invoice, StagingMilestone)
 *   - Pagination/filter generic maps (PaginationFilter<>, PaginationModel, PaginationDto)
 *   - No duplicate entries introduced
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Phase 11 — verify YearlyFinancialDataItem property names match
 *     YearlyFinancialDataDto exactly once Phase 11 adds [GridColumn] attributes; no ForMember
 *     should be needed as stubs use identical names
 *   - TRANSFORMENGINE TODO: Phase 11 — verify PactCostsItem property names match
 *     PactProjectYearCostsDto exactly once Phase 11 refines the panel model
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

            CreateMap<InvoiceItem, RadTrackInvoiceDto>().ReverseMap();
            CreateMap<InvoiceViewModel, RadTrackInvoiceDto>().ReverseMap();
            CreateMap<InvoiceTotalsItem, RadTrackInvoiceTotalsDto>().ReverseMap();
            CreateMap<StagingMilestoneItem, StagingMilestoneDto>().ReverseMap();

            // TRANSFORMENGINE: YearlyFinancialData grid item ↔ DTO (Phase 10 — Step 15b)
            //   Grid row: YearlyFinancialDataItem ↔ YearlyFinancialDataDto
            //   Convention mapping — all property names identical between item and Dto
            CreateMap<YearlyFinancialDataItem, YearlyFinancialDataDto>().ReverseMap();

            // TRANSFORMENGINE: PactCosts panel item ↔ DTO (Phase 10 — Step 15b)
            //   Panel display: PactCostsItem ↔ PactProjectYearCostsDto
            //   Used by "Update Costing" button modal pre-population flow
            CreateMap<PactCostsItem, PactProjectYearCostsDto>().ReverseMap();
        }
    }
}
