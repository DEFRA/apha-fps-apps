/*
 * TRANSFORMENGINE MIGRATION — EntityMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - AutoMapper Profile extended with CreateMap entries for all Phase 3 frmMaintainance-derived entities:
 *     Report, ReportGroup, ReportGroupLink, ProjectManager, ProgramManagerLink,
 *     ProfitCentreManagerLink, Setting, AccessUser, AccessLevel, AccessUserLevel,
 *     AccessSystem, Frequency, ReviewItem, RadtrackProg
 *   - All new mappings use .ReverseMap() for bidirectional DTO <-> Entity conversion
 *   - Existing pre-Phase 3 mappings (pagination, Project family, Milestone, RadTrackInvoice, etc.) preserved verbatim
 *
 * PRESERVED:
 *   - All pagination generic mappings (PaginationParameters<>, PagedData<>, PaginationData)
 *   - All pre-existing entity-to-DTO mappings (ProjectListView, Project, Milestone, RadTrackInvoice families)
 *   - Milestone custom ForMember(IsLate, Ignore) configuration
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
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

            // TRANSFORMENGINE: Phase 3 — new CreateMap entries for frmMaintainance-derived entities
            CreateMap<Report, ReportDto>().ReverseMap();
            CreateMap<ReportGroup, ReportGroupDto>().ReverseMap();
            CreateMap<ReportGroupLink, ReportGroupLinkDto>().ReverseMap();
            CreateMap<ProjectManager, ProjectManagerDto>().ReverseMap();
            CreateMap<ProgramManagerLink, ProgramManagerLinkDto>().ReverseMap();
            CreateMap<ProfitCentreManagerLink, ProfitCentreManagerLinkDto>().ReverseMap();
            CreateMap<Setting, SettingDto>().ReverseMap();
            CreateMap<AccessUser, AccessUserDto>().ReverseMap();
            CreateMap<AccessLevel, AccessLevelDto>().ReverseMap();
            CreateMap<AccessUserLevel, AccessUserLevelDto>().ReverseMap();
            CreateMap<AccessSystem, AccessSystemDto>().ReverseMap();
            CreateMap<Frequency, FrequencyDto>().ReverseMap();
            CreateMap<ReviewItem, ReviewItemDto>().ReverseMap();

            // TRANSFORMENGINE: Phase 3 — RadTrackProg DTO mapping (Programme Tab)
            CreateMap<RadtrackProg, RadTrackProgDto>().ReverseMap();
            CreateMap<StagingMilestone, StagingMilestoneDto>().ReverseMap();
        }
    }
}
