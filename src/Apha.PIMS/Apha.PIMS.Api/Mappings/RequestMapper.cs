/*
 * TRANSFORMENGINE MIGRATION — RequestMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - Verified Phase 5 Comment mappings complete; no new map entries required
 *   - TransformEngine header added
 *
 * PRESERVED:
 *   - All existing AutoMapper Profile CreateMap entries (pagination, project, comment, milestone, costs, invoice)
 *   - CommentDto <-> CommentReq: ForMember Comment/CommentText custom field projection preserved
 *   - CommentDto <-> CommentRes: ForMember Comment/CommentText custom field projection preserved
 *   - CommentTopicDto <-> CommentTopicRes: simple bidirectional map preserved
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
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
            CreateMap<ProjectDetailsMilestoneDto, ProjectDetailsMilestoneRes>().ReverseMap();
            CreateMap<ProjectDto, ProjectRes>().ReverseMap();
            CreateMap<ProposedProjectDto, ProposedProjectReq>().ReverseMap();
            CreateMap<ProposedProjectDto, ProposedProjectRes>().ReverseMap();
            CreateMap<ProjectsDto, ProjectsRes>().ReverseMap();

            // TRANSFORMENGINE: CommentDto <-> CommentReq — Comment field in contract maps to CommentText in Dto
            CreateMap<CommentDto, CommentReq>()
                .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.CommentText))
                .ReverseMap()
                .ForMember(dest => dest.CommentText, opt => opt.MapFrom(src => src.Comment));

            // TRANSFORMENGINE: CommentDto <-> CommentRes — Comment field in contract maps to CommentText in Dto
            CreateMap<CommentDto, CommentRes>()
                .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.CommentText))
                .ReverseMap()
                .ForMember(dest => dest.CommentText, opt => opt.MapFrom(src => src.Comment));

            CreateMap<ProjectDetailDto, ProjectDetailReq>().ReverseMap();
            CreateMap<ProjectDetailDto, ProjectDetailRes>().ReverseMap();
            CreateMap<RiskDto, RiskRes>().ReverseMap();
            CreateMap<YearDto, YearRes>().ReverseMap();

            // TRANSFORMENGINE: CommentTopicDto <-> CommentTopicRes — lookup topic for filter dropdown
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
            CreateMap<StagingMilestoneDto, StagingMilestoneReq>().ReverseMap();
            CreateMap<StagingMilestoneDto, StagingMilestoneRes>().ReverseMap();

            CreateMap<ProjectYearManagerDto, ProjectYearManagerRes>().ReverseMap();
        }
    }
}
