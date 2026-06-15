// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — RequestMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - Added RadTrackInvoiceDto <-> RadTrackInvoiceReq mapping (Req -> Dto for Create/Update body deserialization).
 *   - Added RadTrackInvoiceDto <-> RadTrackInvoiceRes mapping (Dto -> Res for API response serialization).
 *   - Convention mapping applies for all properties; field names on DTO, Req, and Res are identical
 *     (Project, Contract, PlannedAmount, DueAmount, DueDate, ActualAmount, DateInvoiced,
 *     DateJobsheetRaised, InvoiceRef, InvoicePaid). No ForMember overrides required.
 *   - InvoiceCounter is present on RadTrackInvoiceDto and RadTrackInvoiceRes but excluded from
 *     RadTrackInvoiceReq (PK/IDENTITY not writable); convention map correctly omits it in the
 *     Dto->Req direction and includes it in the Dto->Res direction.
 *
 * PRESERVED:
 *   - All existing mappings (Pagination, Project, Milestone, Comment, Cost etc.) unchanged.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: If RadTrackInvoiceTotalsRes is added to Apha.Common.Contracts.PIMS
 *     in a future phase, add RadTrackInvoiceTotalsDto <-> RadTrackInvoiceTotalsRes mapping here.
 *   - TRANSFORMENGINE TODO: Confirm InvoicePaid short convention mapping is acceptable —
 *     if bool is preferred on the API surface, a ForMember override will be needed.
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

            // TRANSFORMENGINE: RadTrackInvoice mappings added Phase 5.
            // Req -> Dto: used in Create (POST) and Update (PUT) body deserialization.
            // Convention mapping covers all shared writable fields; InvoiceCounter is absent
            // from RadTrackInvoiceReq and will default to 0 — the controller overwrites it
            // with the route id before calling UpdateAsync.
            CreateMap<RadTrackInvoiceDto, RadTrackInvoiceReq>().ReverseMap();
            // Dto -> Res: used in GetAll, GetById, Create, and Update response serialization.
            // InvoiceCounter is present on both Dto and Res — convention mapping includes it.
            CreateMap<RadTrackInvoiceDto, RadTrackInvoiceRes>().ReverseMap();
        }
    }
}
