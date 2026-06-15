// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — EntityMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - Added RadTrackInvoice <-> RadTrackInvoiceDto CreateMap with ReverseMap.
 *   - Added RadTrackInvoiceTotals <-> RadTrackInvoiceTotalsDto CreateMap with ReverseMap.
 *   - No existing mappings were modified.
 *
 * PRESERVED:
 *   - All 28 existing entity-to-DTO mappings unchanged.
 *   - Generic pagination mappings (PaginationParameters<>, PagedData<>, PaginationData) unchanged.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: If InvoicePaid is later changed to bool on RadTrackInvoiceDto,
 *     add a ForMember override here to handle the short -> bool conversion.
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

            // TRANSFORMENGINE: RadTrackInvoice <-> RadTrackInvoiceDto — added Phase 3.
            // Convention mapping covers all 11 properties; no ForMember overrides needed while
            // InvoicePaid remains short on both sides.
            CreateMap<RadTrackInvoice, RadTrackInvoiceDto>().ReverseMap();

            // TRANSFORMENGINE: RadTrackInvoiceTotals <-> RadTrackInvoiceTotalsDto — added Phase 3.
            // All three aggregate-sum properties map by name convention.
            CreateMap<RadTrackInvoiceTotals, RadTrackInvoiceTotalsDto>().ReverseMap();
        }
    }
}
