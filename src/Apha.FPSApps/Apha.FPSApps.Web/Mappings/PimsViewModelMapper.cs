using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Mapster;

namespace Apha.FPSApps.Web.Mappings
{
    public class PimsViewModelMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig(typeof(PaginationFilter<>), typeof(QueryParameters<>));
            config.NewConfig(typeof(QueryParameters<>), typeof(PaginationFilter<>));
            config.NewConfig<PaginationModel, PaginationDto>().TwoWays();

            config.NewConfig<ProjectListItem, ProjectListViewDto>().TwoWays();
            config.NewConfig<ProjectListViewModel, ProposedProjectDto>().TwoWays();
            config.NewConfig<ProposedProjectViewModel, ProposedProjectDto>().TwoWays();
            config.NewConfig<ProjectDetailsViewModel, ProjectDetailDto>().TwoWays();
            config.NewConfig<ProjectDetailsViewModel, ProposedProjectDto>().TwoWays();
            config.NewConfig<ProjectCommentItem, CommentDto>()
                .Map(dest => dest.CommentText, src => src.Comment)
                .Map(dest => dest.MadeBy, src => src.MadeBy)
                .Map(dest => dest.DateEntered, src => src.DateEntered);
            config.NewConfig<CommentDto, ProjectCommentItem>()
                .Map(dest => dest.Comment, src => src.CommentText)
                .Map(dest => dest.MadeBy, src => src.MadeBy)
                .Map(dest => dest.DateEntered, src => src.DateEntered);
            
            // Maps CommentNo, Project, Year, Topic, CommentText ? CommentDto fields; ReverseMap for pre-population on edit
            config.NewConfig<AddEditCommentViewModel, CommentDto>().TwoWays();
            // Plan grid item — maps from plan fields on the shared DTO
            config.NewConfig<AdditionalCostDto, AdditionalCostPlanItem>().TwoWays();
            config.NewConfig<AdditionalCostDto, AdditionalCostActualItem>().TwoWays();
            config.NewConfig<AnimalCostDto, AnimalCostPlanItem>().TwoWays();
            config.NewConfig<AnimalCostDto, AnimalCostActualItem>().TwoWays();
            config.NewConfig<TestCostDto, TestCostPlanItem>().TwoWays();
            config.NewConfig<TestCostDto, TestCostActualItem>().TwoWays();
            config.NewConfig<StaffCostDto, StaffCostPlanItem>().TwoWays();
            config.NewConfig<StaffCostDto, StaffCostActualItem>().TwoWays();
            config.NewConfig<PactPayDto, PactPayItem>().TwoWays();
            config.NewConfig<MonthlyPactDto, MonthlyPactItem>().TwoWays();
            config.NewConfig<MilestoneItem, MilestoneDto>().TwoWays();
            config.NewConfig<PMDMilestoneItem, MilestoneDto>().TwoWays();
            config.NewConfig<MilestoneDto, PMDMilestoneItem>().TwoWays();
            config.NewConfig<MilestoneFormDatesItem, MilestoneFormDatesDto>().TwoWays();
            config.NewConfig<LogMilestoneItem, LogMilestoneDto>().TwoWays();
            config.NewConfig<InvoiceItem, RadTrackInvoiceDto>().TwoWays();
            config.NewConfig<InvoiceViewModel, RadTrackInvoiceDto>().TwoWays();
            config.NewConfig<InvoiceTotalsItem, RadTrackInvoiceTotalsDto>().TwoWays();
            config.NewConfig<QueryResultItem, MonitoringReportDataDto>().TwoWays();
            config.NewConfig<ProgramCustomerMonitoringResultItem, ProgramCustomerMonitoringReportDataDto>().TwoWays();
            config.NewConfig<StagingMilestoneItem, StagingMilestoneDto>().TwoWays();
            config.NewConfig<YearlyFinancialDataItem, YearlyFinancialDataDto>().TwoWays();
            config.NewConfig<PactCostsItem, PactProjectYearCostsDto>().TwoWays();
        }
    }
}
