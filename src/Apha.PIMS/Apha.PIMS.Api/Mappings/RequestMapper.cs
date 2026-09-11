using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;
using Mapster;

namespace Apha.PIMS.Api.Mappings
{
    public class RequestMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig(typeof(PaginationReq<>), typeof(QueryParameters<>));
            config.NewConfig(typeof(QueryParameters<>), typeof(PaginationReq<>));
            config.NewConfig(typeof(PaginationReq<>), typeof(PaginationParameters<>));
            config.NewConfig(typeof(PaginationParameters<>), typeof(PaginationReq<>));
            config.NewConfig(typeof(PaginationRes<>), typeof(PaginatedResult<>));
            config.NewConfig(typeof(PaginatedResult<>), typeof(PaginationRes<>));
            config.NewConfig<Pagination, PaginationDto>().TwoWays();
            config.NewConfig<Pagination, PaginationData>().TwoWays();

            config.NewConfig<ProjectListViewDto, ProjectListRes>().TwoWays();
            config.NewConfig<ProjectListMilestoneDto, ProjectListMilestoneRes>().TwoWays();
            config.NewConfig<ProjectDetailsMilestoneDto, ProjectDetailsMilestoneRes>().TwoWays();
            config.NewConfig<ProjectDto, ProjectRes>().TwoWays();
            config.NewConfig<ProposedProjectDto, ProposedProjectReq>().TwoWays();
            config.NewConfig<ProposedProjectDto, ProposedProjectRes>().TwoWays();
            config.NewConfig<ProjectsDto, ProjectsRes>().TwoWays();

            config.NewConfig<CommentDto, CommentReq>()
                .Map(dest => dest.Comment, src => src.CommentText);
            config.NewConfig<CommentReq, CommentDto>()
                .Map(dest => dest.CommentText, src => src.Comment);

            config.NewConfig<CommentDto, CommentRes>()
                .Map(dest => dest.Comment, src => src.CommentText);
            config.NewConfig<CommentRes, CommentDto>()
                .Map(dest => dest.CommentText, src => src.Comment);

            config.NewConfig<ProjectDetailDto, ProjectDetailReq>().TwoWays();
            config.NewConfig<ProjectDetailDto, ProjectDetailRes>().TwoWays();
            // Risk rating lookup maintenance
            config.NewConfig<RiskDto, RiskReq>().TwoWays();
            config.NewConfig<RiskDto, RiskRes>().TwoWays();

            // Publication type lookup maintenance
            config.NewConfig<PublicationTypeDto, PublicationTypeReq>().TwoWays();
            config.NewConfig<PublicationTypeDto, PublicationTypeRes>().TwoWays();
            config.NewConfig<YearDto, YearRes>().TwoWays();

            config.NewConfig<CommentTopicDto, CommentTopicRes>().TwoWays();

            config.NewConfig<AdditionalCostDto, AdditionalCostRes>().TwoWays();
            config.NewConfig<AnimalCostDto, AnimalCostRes>().TwoWays();
            config.NewConfig<TestCostDto, TestCostRes>().TwoWays();
            config.NewConfig<StaffCostDto, StaffCostRes>().TwoWays();
            config.NewConfig<ProjectYearDetailsDto, ProjectYearDetailsRes>().TwoWays();
            config.NewConfig<PactPayDto, PactPayRes>().TwoWays();
            config.NewConfig<MonthlyPactDto, MonthlyPactRes>().TwoWays();
            config.NewConfig<FpsYearTotalsDto, FpsYearTotalsRes>().TwoWays();

            config.NewConfig<MilestoneDto, MilestoneRes>().TwoWays();
            config.NewConfig<MilestoneDto, MilestoneReq>().TwoWays();
            config.NewConfig<MilestoneTypeDto, MilestoneTypeRes>().TwoWays();

            config.NewConfig<MilestoneFormDatesDto, MilestoneFormDatesReq>().TwoWays();
            config.NewConfig<MilestoneFormDatesDto, MilestoneFormDatesRes>().TwoWays();

            config.NewConfig<LogMilestoneDto, LogMilestoneRes>().TwoWays();
            config.NewConfig<RadTrackInvoiceDto, RadTrackInvoiceReq>().TwoWays();
            config.NewConfig<RadTrackInvoiceDto, RadTrackInvoiceRes>().TwoWays();

            config.NewConfig<ReportDto, ReportReq>().TwoWays();
            config.NewConfig<ReportDto, ReportRes>().TwoWays();
            config.NewConfig<ReportGroupDto, ReportGroupReq>().TwoWays();
            config.NewConfig<ReportGroupDto, ReportGroupRes>().TwoWays();
            config.NewConfig<ReportGroupLinkDto, ReportGroupLinkReq>().TwoWays();
            config.NewConfig<ReportGroupLinkDto, ReportGroupLinkRes>().TwoWays();

            config.NewConfig<ProjectManagerDto, ProjectManagerReq>().TwoWays();
            config.NewConfig<ProjectManagerDto, ProjectManagerRes>().TwoWays();
            config.NewConfig<ProgramManagerLinkDto, ProgramManagerLinkReq>().TwoWays();
            config.NewConfig<ProgramManagerLinkDto, ProgramManagerLinkRes>().TwoWays();
            config.NewConfig<ProgramLookupDto, ProgramLookupRes>().TwoWays();
            config.NewConfig<ProfitCentreLookupDto, ProfitCentreLookupRes>().TwoWays();
            config.NewConfig<ProfitCentreManagerLinkDto, ProfitCentreManagerLinkReq>().TwoWays();
            config.NewConfig<ProfitCentreManagerLinkDto, ProfitCentreManagerLinkRes>().TwoWays();

            config.NewConfig<SettingDto, SettingReq>().TwoWays();
            config.NewConfig<SettingDto, SettingRes>().TwoWays();

            config.NewConfig<AccessUserDto, AccessUserReq>().TwoWays();
            config.NewConfig<AccessUserDto, AccessUserRes>().TwoWays();

            config.NewConfig<AccessLevelDto, AccessLevelRes>().TwoWays();
            config.NewConfig<AccessUserLevelDto, AccessUserLevelReq>().TwoWays();
            config.NewConfig<AccessUserLevelDto, AccessUserLevelRes>().TwoWays();
            config.NewConfig<AccessSystemDto, AccessSystemRes>().TwoWays();

            config.NewConfig<FrequencyDto, FrequencyReq>().TwoWays();
            config.NewConfig<FrequencyDto, FrequencyRes>().TwoWays();
            config.NewConfig<ReviewItemDto, ReviewItemReq>().TwoWays();
            config.NewConfig<ReviewItemDto, ReviewItemRes>().TwoWays();

            config.NewConfig<RadTrackProgDto, RadTrackProgReq>().TwoWays();
            config.NewConfig<RadTrackProgDto, RadTrackProgRes>().TwoWays();
            config.NewConfig<StagingMilestoneDto, StagingMilestoneReq>().TwoWays();
            config.NewConfig<StagingMilestoneDto, StagingMilestoneRes>().TwoWays();

            config.NewConfig<YearlyFinancialDataDto, YearlyFinancialDataReq>().TwoWays();
            config.NewConfig<YearlyFinancialDataDto, YearlyFinancialDataRes>().TwoWays();

            config.NewConfig<PactProjectYearCostsDto, PactProjectYearCostsRes>()
                .Map(dest => dest.Year, src => (short)src.Year);
            config.NewConfig<PactProjectYearCostsRes, PactProjectYearCostsDto>()
                .Map(dest => dest.Year, src => (double)src.Year);

            config.NewConfig<ProjectYearManagerDto, ProjectYearManagerRes>().TwoWays();
            config.NewConfig<QueryReportDto, QueryReportRes>().TwoWays();
            config.NewConfig<MonitoringReportData, MonitoringReportDataRes>().TwoWays();
            config.NewConfig<ProgramCustomerMonitoringReportData, ProgramCustomerMonitoringReportDataRes>().TwoWays();
        }
    }
}

