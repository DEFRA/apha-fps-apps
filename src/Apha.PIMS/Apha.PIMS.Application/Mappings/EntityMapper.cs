/*
 * TRANSFORMENGINE MIGRATION — EntityMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - Added CreateMap<YearlyFinancialData, YearlyFinancialDataDto>().ReverseMap()
 *   - Added CreateMap<PactProjectYearCosts, PactProjectYearCostsDto>().ReverseMap()
 *   - Note: YearlyFinancialDataDto.TotalCosts is a computed get-only property;
 *     AutoMapper populates it on map-from-entity via the DTO's own getter.
 *     The ReverseMap (DTO -> Entity) ignores TotalCosts (no corresponding entity property).
 *
 * PRESERVED:
 *   - All existing CreateMap entries unchanged
 *   - Profile registration order preserved
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify that AutoMapper handles TotalCosts as a computed
 *     read-only property correctly when mapping DTO -> Entity (ReverseMap should skip it)
 */

using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;
using AutoMapper;

namespace Apha.PIMS.Application.Mappings
{
    public class EntityMapper : Profile
    {
        public EntityMapper()
        {
            CreateMap(typeof(PaginationParameters<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap(typeof(PagedData<>), typeof(PaginatedResult<>)).ReverseMap();
            CreateMap<PaginationData, PaginationDto>().ReverseMap();

            CreateMap<ProjectListView, ProjectListViewDto>().ReverseMap();
            CreateMap<ProjectListMilestone, ProjectListMilestoneDto>().ReverseMap();
            CreateMap<ProjectDetailsMilestone, ProjectDetailsMilestoneDto>().ReverseMap();
            CreateMap<Project, ProjectDto>().ReverseMap();
            CreateMap<ProposedProject, ProposedProjectDto>().ReverseMap();
            CreateMap<Projects, ProjectsDto>().ReverseMap();
            CreateMap<Comment, CommentDto>().ReverseMap();
            CreateMap<ProjectDetail, ProjectDetailDto>().ReverseMap();
            CreateMap<Risk, RiskDto>().ReverseMap();
            CreateMap<Year, YearDto>().ReverseMap();
            CreateMap<CommentTopic, CommentTopicDto>().ReverseMap();
            CreateMap<ProjSubContract, AdditionalCostDto>().ReverseMap();
            CreateMap<AdditionalCosts, AdditionalCostDto>().ReverseMap();
            CreateMap<ProjSubContract, AnimalCostDto>().ReverseMap();
            CreateMap<ProjectAnimalPlan, AnimalCostDto>().ReverseMap();
            CreateMap<ProjectStaffPlan, StaffCostDto>().ReverseMap();
            CreateMap<TimeCostCalcs, StaffCostDto>().ReverseMap();
            CreateMap<Projects, ProjectYearDetailsDto>().ReverseMap();
            CreateMap<PactPayCalc, PactPayDto>().ReverseMap();
            CreateMap<ProjectMonthFinal, MonthlyPactDto>().ReverseMap();
            CreateMap<FpsYearTotal, FpsYearTotalsDto>().ReverseMap();

            CreateMap<Milestone, MilestoneDto>()
               .ForMember(dest => dest.IsLate, opt => opt.Ignore());
            CreateMap<MilestoneDto, Milestone>();



            CreateMap<MilestoneType, MilestoneTypeDto>().ReverseMap();

            CreateMap<MilestoneFormDates, MilestoneFormDatesDto>().ReverseMap();

            CreateMap<LogMilestone, LogMilestoneDto>().ReverseMap();
            CreateMap<RadTrackInvoice, RadTrackInvoiceDto>().ReverseMap();
            CreateMap<RadTrackInvoiceTotals, RadTrackInvoiceTotalsDto>().ReverseMap();
            CreateMap<StagingMilestone, StagingMilestoneDto>().ReverseMap();

            // TRANSFORMENGINE: YearlyFinancialData <-> YearlyFinancialDataDto mapping
            //   TotalCosts is a computed property on the DTO; AutoMapper handles it via the getter.
            //   ReverseMap skips TotalCosts silently (no matching entity property).
            CreateMap<YearlyFinancialData, YearlyFinancialDataDto>().ReverseMap();

            // TRANSFORMENGINE: PactProjectYearCosts <-> PactProjectYearCostsDto mapping
            //   CustIncome and BudgetCvl are optional joined fields — null when not populated.
            //   HasNoKey entity (vpactprojectyearcosts view) — read-only mapping only needed in practice.
            CreateMap<PactProjectYearCosts, PactProjectYearCostsDto>().ReverseMap();
        }
    }
}
