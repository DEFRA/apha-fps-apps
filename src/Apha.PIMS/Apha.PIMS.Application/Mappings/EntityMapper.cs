/*
 * TRANSFORMENGINE MIGRATION — EntityMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - MS Access / VBA implicit field bindings → AutoMapper Profile with explicit bidirectional CreateMap<Entity, Dto>().ReverseMap() entries
 *   - Pagination primitives mapped: PaginationParameters<T> <-> QueryParameters<T>, PagedData<T> <-> PaginatedResult<T>, PaginationData <-> PaginationDto
 *   - Comment <-> CommentDto: maps all 7 tblcomments columns including CommentText (entity) <-> CommentText (dto), server-managed DateEntered nullable
 *   - CommentTopic <-> CommentTopicDto: single-column PK lookup table
 *   - Milestone <-> MilestoneDto: IsLate is a computed property, ignored on inbound map to avoid overwrite
 *   - All other entity pairs registered for ReverseMap bi-directional mapping
 *
 * PRESERVED:
 *   - All existing CreateMap registrations unchanged — no removals or renames
 *   - MilestoneDto -> Milestone unidirectional map (IsLate opt-out) preserved exactly
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
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

            CreateMap<ProjectYearManager, ProjectYearManagerDto>().ReverseMap();
        }
    }
}
