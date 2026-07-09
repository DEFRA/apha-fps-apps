/*
 * TRANSFORMENGINE MIGRATION — PimsApiDtoMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 10 — AutoMapper Profiles + DI Registration (Step 15)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - Added Phase 10 YearlyFinancialData + PactProjectYearCosts mapper entries:
 *       YearlyFinancialDataRes <-> YearlyFinancialDataDto (.ReverseMap — covers list + single-record GET)
 *       YearlyFinancialDataDto <-> YearlyFinancialDataReq (.ReverseMap — covers POST/PUT request body)
 *       PactProjectYearCostsRes <-> PactProjectYearCostsDto (.ReverseMap — covers GetPactCosts response)
 *   - Entries were written during the Phase 9 API client build pass and confirmed correct here
 *
 * PRESERVED:
 *   - All pre-existing PIMS mapper entries (ProjectList, ProjectDetails, Comments, Risk, Year,
 *     AdditionalCost, AnimalCost, TestCost, StaffCost, ProjectYearDetails, PactPay, MonthlyPact,
 *     FpsYearTotals, Milestones, RadTrackInvoice, StagingMilestone)
 *   - Pagination/envelope generic maps (ApiResponseDto<>, PaginationRes<>, PaginationDto, ApiError, ApiMeta)
 *   - No duplicate entries introduced
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify PactProjectYearCostsRes <-> PactProjectYearCostsDto ReverseMap is
 *     safe — Year type is short on frontend Dto, short on Res (both confirmed identical); no ForMember needed
 *   - TRANSFORMENGINE TODO: Verify TotalCosts on YearlyFinancialDataDto is settable (it is) so
 *     ReverseMap from Res → Dto correctly populates TotalCosts field
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
            CreateMap<ProjectDetailsMilestoneRes, ProjectDetailsMilestoneDto>().ReverseMap();

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
            CreateMap<RadTrackInvoiceRes, RadTrackInvoiceDto>().ReverseMap();
            CreateMap<RadTrackInvoiceDto, RadTrackInvoiceReq>().ReverseMap();

            // Staging Milestone
            CreateMap<StagingMilestoneRes, StagingMilestoneDto>().ReverseMap();
            CreateMap<StagingMilestoneDto, StagingMilestoneReq>().ReverseMap();

            // TRANSFORMENGINE: YearlyFinancialData CRUD + pactcosts mappings (Phase 9 — Step 14)
            //   YearlyFinancialDataRes ↔ YearlyFinancialDataDto (list + single-record endpoints)
            //   YearlyFinancialDataDto → YearlyFinancialDataReq (create / update request body mapping)
            //   PactProjectYearCostsRes → PactProjectYearCostsDto (GetPactCosts "Update Costing" button)
            CreateMap<YearlyFinancialDataRes, YearlyFinancialDataDto>().ReverseMap();
            CreateMap<YearlyFinancialDataDto, YearlyFinancialDataReq>().ReverseMap();
            CreateMap<PactProjectYearCostsRes, PactProjectYearCostsDto>().ReverseMap();
        }
    }
}
