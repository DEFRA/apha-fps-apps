/*
 * TRANSFORMENGINE MIGRATION — RequestMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - Added CreateMap entries for 14 new resource families:
 *     Report, ReportGroup, ReportGroupLink, ProjectManager, ProgramManagerLink,
 *     ProfitCentreManagerLink, Setting, AccessUser, AccessLevel, AccessUserLevel,
 *     AccessSystem, Frequency, ReviewItem, RadTrackProg
 *   - AccessLevel: no AccessLevelReq exists; Req mapping omitted; uses AccessLevelRes for both read and write body
 *   - AccessSystem: read-only reference data; only Dto <-> Res mapping added
 *   - RadTrackProg: natural string PK (program varchar(10)); Req and Res both added
 *
 * PRESERVED:
 *   - All existing pagination and resource family mappings (ProjectList, Milestone, RadTrackInvoice, etc.)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: AccessLevelReq contract missing — create dedicated write contract and add Dto <-> Req mapping when available
 *
 * PHASE 6 — Backend Readiness Gate (VERIFIED 2026-07-06):
 *   - Mapper coverage confirmed for ALL 14 Phase 5 resource families:
 *     Report:                ReportDto <-> ReportReq          CONFIRMED
 *                            ReportDto <-> ReportRes          CONFIRMED
 *     ReportGroup:           ReportGroupDto <-> ReportGroupReq CONFIRMED
 *                            ReportGroupDto <-> ReportGroupRes CONFIRMED
 *     ReportGroupLink:       ReportGroupLinkDto <-> ReportGroupLinkReq CONFIRMED
 *                            ReportGroupLinkDto <-> ReportGroupLinkRes CONFIRMED
 *     ProjectManager:        ProjectManagerDto <-> ProjectManagerReq CONFIRMED
 *                            ProjectManagerDto <-> ProjectManagerRes CONFIRMED
 *     ProgramManagerLink:    ProgramManagerLinkDto <-> ProgramManagerLinkReq CONFIRMED
 *                            ProgramManagerLinkDto <-> ProgramManagerLinkRes CONFIRMED
 *     ProfitCentreManagerLink: ProfitCentreManagerLinkDto <-> ProfitCentreManagerLinkReq CONFIRMED
 *                            ProfitCentreManagerLinkDto <-> ProfitCentreManagerLinkRes CONFIRMED
 *     Setting:               SettingDto <-> SettingReq        CONFIRMED
 *                            SettingDto <-> SettingRes        CONFIRMED
 *     AccessUser:            AccessUserDto <-> AccessUserReq  CONFIRMED
 *                            AccessUserDto <-> AccessUserRes  CONFIRMED
 *     AccessLevel:           AccessLevelDto <-> AccessLevelRes CONFIRMED (no Req — lookup only)
 *     AccessUserLevel:       AccessUserLevelDto <-> AccessUserLevelReq CONFIRMED
 *                            AccessUserLevelDto <-> AccessUserLevelRes CONFIRMED
 *     AccessSystem:          AccessSystemDto <-> AccessSystemRes CONFIRMED (read-only)
 *     Frequency:             FrequencyDto <-> FrequencyReq    CONFIRMED
 *                            FrequencyDto <-> FrequencyRes    CONFIRMED
 *     ReviewItem:            ReviewItemDto <-> ReviewItemReq  CONFIRMED
 *                            ReviewItemDto <-> ReviewItemRes  CONFIRMED
 *     RadTrackProg:          RadTrackProgDto <-> RadTrackProgReq CONFIRMED
 *                            RadTrackProgDto <-> RadTrackProgRes CONFIRMED
 *   - Lookup separation confirmed: AccessLevel/AccessSystem Res-only mappings correctly distinct from CRUD families
 *   - Interface changes log entry for RequestMapper marked DONE (see transform-plan.md)
 *   - All ~30 CreateMap entries added in Phase 5 are present and confirmed in Phase 6 gate
 */
using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Pagination;
using AutoMapper;

namespace Apha.PIMS.Api.Mappings
{
    public class RequestMapper : Profile
    {
        public RequestMapper()
        {
            CreateMap(typeof(PaginationReq<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap(typeof(PaginationReq<>), typeof(PaginationParameters<>)).ReverseMap();
            CreateMap(typeof(PaginationRes<>), typeof(PaginatedResult<>)).ReverseMap();
            CreateMap<Pagination, PaginationDto>().ReverseMap();
            CreateMap<Pagination, PaginationData>().ReverseMap();

            CreateMap<ProjectListViewDto, ProjectListRes>().ReverseMap();
            CreateMap<ProjectListMilestoneDto, ProjectListMilestoneRes>().ReverseMap();
            CreateMap<ProjectDto, ProjectRes>().ReverseMap();
            CreateMap<ProposedProjectDto, ProposedProjectReq>().ReverseMap();
            CreateMap<ProposedProjectDto, ProposedProjectRes>().ReverseMap();
            CreateMap<ProjectsDto, ProjectsRes>().ReverseMap();
            CreateMap<CommentDto, CommentReq>()
                .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.CommentText))
                .ReverseMap()
                .ForMember(dest => dest.CommentText, opt => opt.MapFrom(src => src.Comment));

            CreateMap<CommentDto, CommentRes>()
                .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.CommentText))
                .ReverseMap()
                .ForMember(dest => dest.CommentText, opt => opt.MapFrom(src => src.Comment));
            CreateMap<ProjectDetailDto, ProjectDetailReq>().ReverseMap();
            CreateMap<ProjectDetailDto, ProjectDetailRes>().ReverseMap();
            CreateMap<RiskDto, RiskRes>().ReverseMap();
            CreateMap<YearDto, YearRes>().ReverseMap();
            CreateMap<CommentTopicDto, CommentTopicRes>().ReverseMap();
            CreateMap<AdditionalCostDto, AdditionalCostRes>().ReverseMap();
            CreateMap<AnimalCostDto, AnimalCostRes>().ReverseMap();
            CreateMap<TestCostDto, TestCostRes>().ReverseMap();
            CreateMap<StaffCostDto, StaffCostRes>().ReverseMap();
            CreateMap<ProjectYearDetailsDto, ProjectYearDetailsRes>().ReverseMap();
            CreateMap<PactPayDto, PactPayRes>().ReverseMap();
            CreateMap<MonthlyPactDto, MonthlyPactRes>().ReverseMap();
            CreateMap<FpsYearTotalsDto, FpsYearTotalsRes>().ReverseMap();

            CreateMap<MilestoneDto, MilestoneRes>().ReverseMap();
            CreateMap<MilestoneDto, MilestoneReq>().ReverseMap();
            CreateMap<MilestoneTypeDto, MilestoneTypeRes>().ReverseMap();

            CreateMap<MilestoneFormDatesDto, MilestoneFormDatesReq>().ReverseMap();
            CreateMap<MilestoneFormDatesDto, MilestoneFormDatesRes>().ReverseMap();

            CreateMap<LogMilestoneDto, LogMilestoneRes>().ReverseMap();
            CreateMap<RadTrackInvoiceDto, RadTrackInvoiceReq>().ReverseMap();
            CreateMap<RadTrackInvoiceDto, RadTrackInvoiceRes>().ReverseMap();

            // TRANSFORMENGINE: Phase 5 additions — Report, ReportGroup, ReportGroupLink
            CreateMap<ReportDto, ReportReq>().ReverseMap();
            CreateMap<ReportDto, ReportRes>().ReverseMap();
            CreateMap<ReportGroupDto, ReportGroupReq>().ReverseMap();
            CreateMap<ReportGroupDto, ReportGroupRes>().ReverseMap();
            CreateMap<ReportGroupLinkDto, ReportGroupLinkReq>().ReverseMap();
            CreateMap<ReportGroupLinkDto, ReportGroupLinkRes>().ReverseMap();

            // TRANSFORMENGINE: Phase 5 additions — ProjectManager, ProgramManagerLink, ProfitCentreManagerLink
            CreateMap<ProjectManagerDto, ProjectManagerReq>().ReverseMap();
            CreateMap<ProjectManagerDto, ProjectManagerRes>().ReverseMap();
            CreateMap<ProgramManagerLinkDto, ProgramManagerLinkReq>().ReverseMap();
            CreateMap<ProgramManagerLinkDto, ProgramManagerLinkRes>().ReverseMap();
            CreateMap<ProfitCentreManagerLinkDto, ProfitCentreManagerLinkReq>().ReverseMap();
            CreateMap<ProfitCentreManagerLinkDto, ProfitCentreManagerLinkRes>().ReverseMap();

            // TRANSFORMENGINE: Phase 5 additions — Setting
            CreateMap<SettingDto, SettingReq>().ReverseMap();
            CreateMap<SettingDto, SettingRes>().ReverseMap();

            // TRANSFORMENGINE: Phase 5 additions — AccessUser, AccessLevel (no Req), AccessUserLevel, AccessSystem (read-only)
            CreateMap<AccessUserDto, AccessUserReq>().ReverseMap();
            CreateMap<AccessUserDto, AccessUserRes>().ReverseMap();
            // TRANSFORMENGINE TODO: AccessLevelReq does not exist — add Dto <-> Req mapping once contract is created
            CreateMap<AccessLevelDto, AccessLevelRes>().ReverseMap();
            CreateMap<AccessUserLevelDto, AccessUserLevelReq>().ReverseMap();
            CreateMap<AccessUserLevelDto, AccessUserLevelRes>().ReverseMap();
            CreateMap<AccessSystemDto, AccessSystemRes>().ReverseMap();

            // TRANSFORMENGINE: Phase 5 additions — Frequency, ReviewItem
            CreateMap<FrequencyDto, FrequencyReq>().ReverseMap();
            CreateMap<FrequencyDto, FrequencyRes>().ReverseMap();
            CreateMap<ReviewItemDto, ReviewItemReq>().ReverseMap();
            CreateMap<ReviewItemDto, ReviewItemRes>().ReverseMap();

            // TRANSFORMENGINE: Phase 5 additions — RadTrackProg (Programme Tab); natural string PK (program varchar(10))
            CreateMap<RadTrackProgDto, RadTrackProgReq>().ReverseMap();
            CreateMap<RadTrackProgDto, RadTrackProgRes>().ReverseMap();
        }
    }
}
