using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;
using Mapster;

namespace Apha.FPSApps.Infrastructure.Mappings
{
    public class PimsApiDtoMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig(typeof(ApiResponseDto<>), typeof(ApiResponse<>));
            config.NewConfig(typeof(ApiResponse<>), typeof(ApiResponseDto<>));
            config.NewConfig<ApiErrorDto, ApiError>().TwoWays();
            config.NewConfig<ApiMetaDto, ApiMeta>().TwoWays();
            config.NewConfig(typeof(PaginationRes<>), typeof(PaginatedResult<>));
            config.NewConfig(typeof(PaginatedResult<>), typeof(PaginationRes<>));
            config.NewConfig<PaginationDto, Pagination>().TwoWays();

            // Project List
            config.NewConfig<ProfitCentreLookupRes, ProfitCentreLookupDto>().TwoWays();
            config.NewConfig<ProgramLookupRes, ProgramLookupDto>().TwoWays();
            config.NewConfig<ProjectListRes, ProjectListViewDto>().TwoWays();
            config.NewConfig<ProjectListMilestoneRes, ProjectListMilestoneDto>().TwoWays();
            config.NewConfig<ProjectDetailsMilestoneRes, ProjectDetailsMilestoneDto>().TwoWays();

            // FPS Project Details (read-only)
            config.NewConfig<ProjectRes, ProjectDto>().TwoWays();

            // Proposed Project
            config.NewConfig<ProposedProjectRes, ProposedProjectDto>().TwoWays();
            config.NewConfig<ProposedProjectDto, ProposedProjectReq>().TwoWays();

            // FPS Yearly Details
            config.NewConfig<ProjectsRes, ProjectsDto>().TwoWays();

            // Comments
            config.NewConfig<CommentRes, CommentDto>()
                .Map(dest => dest.CommentText, src => src.Comment);
            config.NewConfig<CommentDto, CommentRes>()
                .Map(dest => dest.Comment, src => src.CommentText);
            config.NewConfig<CommentDto, CommentReq>()
                .Map(dest => dest.Comment, src => src.CommentText);
            config.NewConfig<CommentReq, CommentDto>()
                .Map(dest => dest.CommentText, src => src.Comment);

            // PIMS Project Detail
            config.NewConfig<ProjectDetailRes, ProjectDetailDto>().TwoWays();
            config.NewConfig<ProjectDetailDto, ProjectDetailReq>().TwoWays();

            // Comment Topics
            config.NewConfig<CommentTopicRes, CommentTopicDto>().TwoWays();
            config.NewConfig<ProjectCommentForecastSpendRes, ProjectCommentForecastSpendDto>().TwoWays();

            // Risk
            config.NewConfig<RiskRes, RiskDto>().TwoWays();
            config.NewConfig<RiskDto, RiskReq>().TwoWays();

            // Publication Type
            config.NewConfig<PublicationTypeRes, PublicationTypeDto>().TwoWays();
            config.NewConfig<PublicationTypeDto, PublicationTypeReq>().TwoWays();

            // Year
            config.NewConfig<YearRes, YearDto>().TwoWays();

            // Additional Cost
            config.NewConfig<AdditionalCostRes, AdditionalCostDto>().TwoWays();

            // Animal Cost
            config.NewConfig<AnimalCostRes, AnimalCostDto>().TwoWays();

            // Test Cost
            config.NewConfig<TestCostRes, TestCostDto>().TwoWays();

            // Staff Cost
            config.NewConfig<StaffCostRes, StaffCostDto>().TwoWays();

            // Project Year Details
            config.NewConfig<ProjectYearDetailsRes, ProjectYearDetailsDto>().TwoWays();

            // Pact Pay
            config.NewConfig<PactPayRes, PactPayDto>().TwoWays();

            // Monthly Pact Data
            config.NewConfig<MonthlyPactRes, MonthlyPactDto>().TwoWays();

            // FPS Year Totals
            config.NewConfig<FpsYearTotalsRes, FpsYearTotalsDto>().TwoWays();

            // Milestones
            config.NewConfig<MilestoneRes, MilestoneDto>().TwoWays();
            config.NewConfig<MilestoneDto, MilestoneReq>().TwoWays();
            config.NewConfig<MilestoneTypeRes, MilestoneTypeDto>().TwoWays();

            config.NewConfig<MilestoneFormDatesRes, MilestoneFormDatesDto>().TwoWays();
            config.NewConfig<MilestoneFormDatesDto, MilestoneFormDatesReq>().TwoWays();

            config.NewConfig<LogMilestoneRes, LogMilestoneDto>().TwoWays();
            config.NewConfig<RadTrackInvoiceRes, RadTrackInvoiceDto>().TwoWays();
            config.NewConfig<RadTrackInvoiceDto, RadTrackInvoiceReq>().TwoWays();
            config.NewConfig<MonitoringReportDataRes, MonitoringReportDataDto>().TwoWays();
            config.NewConfig<ProgramCustomerMonitoringReportDataRes, ProgramCustomerMonitoringReportDataDto>().TwoWays();

            // Staging Milestone
            config.NewConfig<StagingMilestoneRes, StagingMilestoneDto>().TwoWays();
            config.NewConfig<StagingMilestoneDto, StagingMilestoneReq>().TwoWays();

            config.NewConfig<YearlyFinancialDataRes, YearlyFinancialDataDto>().TwoWays();
            config.NewConfig<YearlyFinancialDataDto, YearlyFinancialDataReq>().TwoWays();
            config.NewConfig<PactProjectYearCostsRes, PactProjectYearCostsDto>().TwoWays();

            // Project Year Manager
            config.NewConfig<ProjectYearManagerRes, ProjectYearManagerDto>().TwoWays();
        }
    }
}
