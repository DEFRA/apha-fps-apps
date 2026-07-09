/*
 * TRANSFORMENGINE MIGRATION — RequestMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - Added YearlyFinancialData request/response mappings:
 *       YearlyFinancialDataDto <-> YearlyFinancialDataReq
 *       YearlyFinancialDataDto <-> YearlyFinancialDataRes
 *       PactProjectYearCostsDto <-> PactProjectYearCostsRes
 *
 * PRESERVED:
 *   - All pre-existing mapping registrations untouched
 *   - Existing ReverseMap chains preserved
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify YearlyFinancialDataRes.TotalCosts (set property) is
 *     populated correctly from YearlyFinancialDataDto.TotalCosts (computed getter).
 *     AutoMapper maps by name so the value will be read from the computed getter and
 *     set onto the flat response property — should work as-is.
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
            CreateMap<StagingMilestoneDto, StagingMilestoneReq>().ReverseMap();
            CreateMap<StagingMilestoneDto, StagingMilestoneRes>().ReverseMap();

            // TRANSFORMENGINE: YearlyFinancialData mappings added in Phase 5
            //   Dto <-> Req: inbound create/update requests map to service-layer DTO
            //   Dto <-> Res: outbound service DTO maps to API response contract
            //   TotalCosts in Res is a flat settable property; AutoMapper reads the
            //   computed getter from YearlyFinancialDataDto and writes it to the Res property
            CreateMap<YearlyFinancialDataDto, YearlyFinancialDataReq>().ReverseMap();
            CreateMap<YearlyFinancialDataDto, YearlyFinancialDataRes>().ReverseMap();

            // TRANSFORMENGINE: PactProjectYearCosts response mapping (read-only PACT actuals)
            //   MonthNo is in the DTO but not in PactProjectYearCostsRes (suppressed at contract layer)
            //   Year in DTO is double; Year in Res is short — explicit cast required
            CreateMap<PactProjectYearCostsDto, PactProjectYearCostsRes>()
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => (short)src.Year))
                .ReverseMap()
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => (double)src.Year));
        }
    }
}
