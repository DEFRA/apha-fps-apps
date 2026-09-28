using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;
using Mapster;

namespace Apha.PIMS.Application.Mappings
{
    public class EntityMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig(typeof(PaginationParameters<>), typeof(QueryParameters<>));
            config.NewConfig(typeof(QueryParameters<>), typeof(PaginationParameters<>));
            config.NewConfig(typeof(PagedData<>), typeof(PaginatedResult<>));
            config.NewConfig(typeof(PaginatedResult<>), typeof(PagedData<>));
            config.NewConfig<PaginationData, PaginationDto>().TwoWays();
            config.NewConfig<ProjectListView, ProjectListViewDto>().TwoWays();
            config.NewConfig<ProjectListMilestone, ProjectListMilestoneDto>().TwoWays();
            config.NewConfig<ProjectDetailsMilestone, ProjectDetailsMilestoneDto>().TwoWays();
            config.NewConfig<Project, ProjectDto>().TwoWays();
            config.NewConfig<ProposedProject, ProposedProjectDto>().TwoWays();
            config.NewConfig<Projects, ProjectsDto>().TwoWays();
            config.NewConfig<Comment, CommentDto>().TwoWays();
            config.NewConfig<ProjectDetail, ProjectDetailDto>().TwoWays();
            config.NewConfig<Risk, RiskDto>().TwoWays();
            config.NewConfig<Year, YearDto>().TwoWays();
            config.NewConfig<CommentTopic, CommentTopicDto>().TwoWays();
            config.NewConfig<ProjSubContract, AdditionalCostDto>().TwoWays();
            config.NewConfig<AdditionalCosts, AdditionalCostDto>().TwoWays();
            config.NewConfig<ProjSubContract, AnimalCostDto>().TwoWays();
            config.NewConfig<ProjectAnimalPlan, AnimalCostDto>().TwoWays();
            config.NewConfig<ProjectStaffPlan, StaffCostDto>().TwoWays();
            config.NewConfig<TimeCostCalcs, StaffCostDto>().TwoWays();
            config.NewConfig<Projects, ProjectYearDetailsDto>().TwoWays();
            config.NewConfig<PactPayCalc, PactPayDto>().TwoWays();
            config.NewConfig<ProjectMonthFinal, MonthlyPactDto>().TwoWays();
            config.NewConfig<FpsYearTotal, FpsYearTotalsDto>().TwoWays();
            config.NewConfig<Milestone, MilestoneDto>()
                .Ignore(dest => dest.IsLate);
            config.NewConfig<MilestoneDto, Milestone>();
            config.NewConfig<MilestoneType, MilestoneTypeDto>().TwoWays();
            config.NewConfig<MilestoneFormDates, MilestoneFormDatesDto>().TwoWays();
            config.NewConfig<LogMilestone, LogMilestoneDto>().TwoWays();
            config.NewConfig<RadTrackInvoice, RadTrackInvoiceDto>().TwoWays();
            config.NewConfig<RadTrackInvoiceTotals, RadTrackInvoiceTotalsDto>().TwoWays();

            config.NewConfig<Report, ReportDto>().TwoWays();
            config.NewConfig<ReportGroup, ReportGroupDto>().TwoWays();
            config.NewConfig<ReportGroupLink, ReportGroupLinkDto>().TwoWays();
            config.NewConfig<ProjectManager, ProjectManagerDto>().TwoWays();
            config.NewConfig<ProgramManagerLink, ProgramManagerLinkDto>().TwoWays();
            config.NewConfig<ProgramLookup, ProgramLookupDto>().TwoWays();
            config.NewConfig<ProfitCentreLookup, ProfitCentreLookupDto>().TwoWays();
            config.NewConfig<ProfitCentreManagerLink, ProfitCentreManagerLinkDto>().TwoWays();
            config.NewConfig<Settings, SettingDto>().TwoWays();
            config.NewConfig<AccessUser, AccessUserDto>().TwoWays();
            config.NewConfig<AccessLevel, AccessLevelDto>().TwoWays();
            config.NewConfig<AccessUserLevel, AccessUserLevelDto>().TwoWays();
            config.NewConfig<AccessSystem, AccessSystemDto>().TwoWays();
            config.NewConfig<Frequency, FrequencyDto>().TwoWays();
            config.NewConfig<ReviewItem, ReviewItemDto>().TwoWays();
            config.NewConfig<PublicationType, PublicationTypeDto>().TwoWays();

            config.NewConfig<RadtrackProg, RadTrackProgDto>().TwoWays();
            config.NewConfig<StagingMilestone, StagingMilestoneDto>().TwoWays();
            config.NewConfig<YearlyFinancialData, YearlyFinancialDataDto>().TwoWays();
            config.NewConfig<PactProjectYearCosts, PactProjectYearCostsDto>().TwoWays();

            config.NewConfig<ProjectYearManager, ProjectYearManagerDto>().TwoWays();

            config.NewConfig<QueryReportItem, QueryReportDto>().TwoWays();
        }
    }
}

