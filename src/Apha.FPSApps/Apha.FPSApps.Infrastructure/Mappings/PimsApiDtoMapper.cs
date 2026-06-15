// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — PimsApiDtoMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 10 — AutoMapper Profiles + DI Registration (Step 15)
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - Added RadTrackInvoice mappings (Step 15a):
 *       CreateMap<RadTrackInvoiceRes, RadTrackInvoiceDto>().ReverseMap()
 *       CreateMap<RadTrackInvoiceDto, RadTrackInvoiceReq>().ReverseMap()
 *   - Note: RadTrackInvoiceTotalsDto has no Res contract (backend returns DTO directly);
 *     no totals Res→Dto mapping is required here.
 *
 * PRESERVED:
 *   - All 44 existing CreateMap entries (GenericResponse, ProjectList, FpsProjectDetails,
 *     ProposedProject, Comments, ProjectDetail, Risk, Year, costs, PactPay, MonthlyPact,
 *     FpsYearTotals, Milestone, MilestoneFormDates, LogMilestone) unchanged.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: If RadTrackInvoiceTotalsRes is added to Apha.Common.Contracts.PIMS,
 *     add CreateMap<RadTrackInvoiceTotalsRes, RadTrackInvoiceTotalsDto>().ReverseMap() here.
 *   - TRANSFORMENGINE TODO: If InvoicePaid is changed from short to bool on Res/Dto, update mappings.
 */

using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Mappings
{
    public class PimsApiDtoMapper : Profile
    {
        public PimsApiDtoMapper()
        {
            CreateMap(typeof(ApiResponseDto<>), typeof(ApiResponse<>)).ReverseMap();
            CreateMap<ApiErrorDto, ApiError>().ReverseMap();
            CreateMap<ApiMetaDto, ApiMeta>().ReverseMap();
            CreateMap(typeof(PaginationRes<>), typeof(PaginatedResult<>)).ReverseMap();
            CreateMap<PaginationDto, Pagination>().ReverseMap();

            // Project List
            CreateMap<ProjectListRes, ProjectListViewDto>().ReverseMap();
            CreateMap<ProjectListMilestoneRes, ProjectListMilestoneDto>().ReverseMap();

            // FPS Project Details (read-only)
            CreateMap<ProjectRes, ProjectDto>().ReverseMap();

            // Proposed Project
            CreateMap<ProposedProjectRes, ProposedProjectDto>().ReverseMap();
            CreateMap<ProposedProjectDto, ProposedProjectReq>().ReverseMap();

            // FPS Yearly Details
            CreateMap<ProjectsRes, ProjectsDto>().ReverseMap();

            // Comments
            CreateMap<CommentRes, CommentDto>().ReverseMap();
            CreateMap<CommentDto, CommentReq>().ReverseMap();

            // PIMS Project Detail
            CreateMap<ProjectDetailRes, ProjectDetailDto>().ReverseMap();
            CreateMap<ProjectDetailDto, ProjectDetailReq>().ReverseMap();

            // Comment Topics
            CreateMap<CommentTopicRes, CommentTopicDto>().ReverseMap();

            // Risk
            CreateMap<RiskRes, RiskDto>().ReverseMap();

            // Year
            CreateMap<YearRes, YearDto>().ReverseMap();

            // Additional Cost
            CreateMap<AdditionalCostRes, AdditionalCostDto>().ReverseMap();

            // Animal Cost
            CreateMap<AnimalCostRes, AnimalCostDto>().ReverseMap();

            // Test Cost
            CreateMap<TestCostRes, TestCostDto>().ReverseMap();

            // Staff Cost
            CreateMap<StaffCostRes, StaffCostDto>().ReverseMap();

            // Project Year Details
            CreateMap<ProjectYearDetailsRes, ProjectYearDetailsDto>().ReverseMap();

            // Pact Pay
            CreateMap<PactPayRes, PactPayDto>().ReverseMap();

            // Monthly Pact Data
            CreateMap<MonthlyPactRes, MonthlyPactDto>().ReverseMap();

            // FPS Year Totals
            CreateMap<FpsYearTotalsRes, FpsYearTotalsDto>().ReverseMap();

            // Milestones
            CreateMap<MilestoneRes, MilestoneDto>().ReverseMap();
            CreateMap<MilestoneDto, MilestoneReq>().ReverseMap();
            CreateMap<MilestoneTypeRes, MilestoneTypeDto>().ReverseMap();

            CreateMap<MilestoneFormDatesRes, MilestoneFormDatesDto>().ReverseMap();
            CreateMap<MilestoneFormDatesDto, MilestoneFormDatesReq>().ReverseMap();

            CreateMap<LogMilestoneRes, LogMilestoneDto>().ReverseMap();

            // TRANSFORMENGINE: RadTrack Invoice — Step 15a (Phase 10)
            // RadTrackInvoiceRes → RadTrackInvoiceDto: list/get-by-id response → frontend DTO.
            // RadTrackInvoiceDto → RadTrackInvoiceReq: frontend DTO → create/update request body.
            // No Res contract exists for RadTrackInvoiceTotalsDto (backend returns DTO directly).
            CreateMap<RadTrackInvoiceRes, RadTrackInvoiceDto>().ReverseMap();
            CreateMap<RadTrackInvoiceDto, RadTrackInvoiceReq>().ReverseMap();
        }
    }
}
