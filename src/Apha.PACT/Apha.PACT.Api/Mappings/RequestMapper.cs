using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;
using Mapster;

namespace Apha.PACT.Api.Mappings
{
    public class RequestMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig(typeof(PaginationReq<>), typeof(QueryParameters<>));
            config.NewConfig(typeof(QueryParameters<>), typeof(PaginationReq<>));
            config.NewConfig(typeof(PaginationRes<>), typeof(PaginatedResult<>));
            config.NewConfig(typeof(PaginatedResult<>), typeof(PaginationRes<>));

            config.NewConfig<Pagination, PaginationDto>().TwoWays();

            config.NewConfig<JobCodeReq, JobCodeDto>().TwoWays();
            config.NewConfig<JobCodeRes, JobCodeDto>().TwoWays();
            config.NewConfig<JobCodeZtRes, JobCodeZtDto>().TwoWays();
            config.NewConfig<TimeCodeValidReq, TimeCodeValidDto>().TwoWays();
            config.NewConfig<TimeCodeValidRes, TimeCodeValidDto>().TwoWays();
            config.NewConfig<WorkGroupRes, WorkGroupDto>().TwoWays();
            config.NewConfig<WorkGroupMaintenanceReq, WorkGroupDto>();
            config.NewConfig<WorkGroupDto, WorkGroupMaintenanceRes>()
                .Ignore(dest => dest.Id);
            config.NewConfig<OwnerDto, OwnerRes>().TwoWays();
            config.NewConfig<WorkGroupViewRes, WorkGroupViewDto>().TwoWays();
            config.NewConfig<CalenderMonthRes, CalenderMonthDto>().TwoWays();
            config.NewConfig<ProjectInvoiceReq, ProjectInvoiceDto>().TwoWays();
            config.NewConfig<ProjectInvoiceRes, ProjectInvoiceDto>().TwoWays();
            config.NewConfig<InvoiceImportRowReq, InvoiceImportRowDto>().TwoWays();
            config.NewConfig<InvoiceImportRowRes, InvoiceImportRowDto>().TwoWays();
            config.NewConfig<InvoiceImportReq, InvoiceImportDto>().TwoWays();
            config.NewConfig<InvoiceImportRes, InvoiceImportResultDto>().TwoWays();
            config.NewConfig<ProjectSubContractReq, ProjectSubContractDto>().TwoWays();
            config.NewConfig<ProjectSubContractRes, ProjectSubContractDto>().TwoWays();
            config.NewConfig<SubContractRmsImportRowReq, SubContractRmsImportRowDto>().TwoWays();
            config.NewConfig<SubContractRmsImportRowRes, SubContractRmsImportRowDto>().TwoWays();
            config.NewConfig<SubContractRmsImportReq, SubContractRmsImportDto>().TwoWays();
            config.NewConfig<SubContractRmsImportRes, SubContractRmsImportResultDto>().TwoWays();
            config.NewConfig<TestCapabilityReq, TestCapabilityDto>().TwoWays();
            config.NewConfig<TestCapabilityRes, TestCapabilityDto>().TwoWays();
            config.NewConfig<TestRequirementReq, TestRequirementtDto>().TwoWays();
            config.NewConfig<TestRequirementtRes, TestRequirementtDto>().TwoWays();
            config.NewConfig<TestorProductReq, TestorProductDto>().TwoWays();
            config.NewConfig<TestorProductRes, TestorProductDto>().TwoWays();
            config.NewConfig<MonthlyInvoicesSummaryDto, MonthlyInvoicesSummaryItemRes>().TwoWays();
            config.NewConfig<MonthlyInvoicesPivotDto, MonthlyInvoicesPivotRes>().TwoWays();
            config.NewConfig<MonthlySubContractsSummaryDto, MonthlySubContractsSummaryItemRes>().TwoWays();
            config.NewConfig<MonthlySubContractsPivotDto, MonthlySubContractsPivotRes>().TwoWays();
            config.NewConfig<ProjectMonthReq, ProjectMonthDto>().TwoWays();
            config.NewConfig<ProjectMonthRes, ProjectMonthDto>().TwoWays();
            config.NewConfig<ProjectProfileDto, ProjectProfileRes>().TwoWays();
            config.NewConfig<ProjectProfileCumulativeDto, ProjectProfileCumulativeRes>().TwoWays();
            config.NewConfig<MonthlyOutputLogDto, MonthlyOutputLogRes>().TwoWays();
            config.NewConfig<MonthlyOutputDto, MonthlyOutputReq>().TwoWays();
            config.NewConfig<MonthlyOutputDto, MonthlyOutputRes>().TwoWays();
            config.NewConfig<StagingMonthlyOutputDto, StagingMonthlyOutputReq>().TwoWays();
            config.NewConfig<StagingMonthlyOutputDto, StagingMonthlyOutputRes>().TwoWays();
            config.NewConfig<MonthlyOutputImportRowDto, MonthlyOutputImportRowReq>().TwoWays();
            config.NewConfig<MonthlyOutputImportRowDto, MonthlyOutputImportRowRes>().TwoWays();
            config.NewConfig<MonthlyOutputImportDto, MonthlyOutputImportReq>().TwoWays();
            config.NewConfig<MonthlyOutputImportResultDto, MonthlyOutputImportRes>().TwoWays();
            config.NewConfig<MonthlyOutputValidateResultDto, MonthlyOutputValidateRes>().TwoWays();
            config.NewConfig<MonthlyOutputMakeLiveResultDto, MonthlyOutputMakeLiveRes>().TwoWays();
            config.NewConfig<MonthlyTimeDto, MonthlyTimeReq>().TwoWays();
            config.NewConfig<MonthlyTimeDto, MonthlyTimeRes>().TwoWays();
            config.NewConfig<StagingMonthlyTimeDto, StagingMonthlyTimeReq>().TwoWays();
            config.NewConfig<StagingMonthlyTimeDto, StagingMonthlyTimeRes>().TwoWays();
            config.NewConfig<BulkUpdateStagingMonthlyTimeNamesDto, BulkUpdateStagingMonthlyTimeNamesReq>().TwoWays();
            config.NewConfig<BulkUpdateStagingMonthlyTimeNamesResultDto, BulkUpdateStagingMonthlyTimeNamesRes>().TwoWays();
            config.NewConfig<MonthlyTimeImportRowDto, MonthlyTimeImportRowReq>().TwoWays();
            config.NewConfig<MonthlyTimeImportRowDto, MonthlyTimeImportRowRes>().TwoWays();
            config.NewConfig<MonthlyTimeImportDto, MonthlyTimeImportReq>().TwoWays();
            config.NewConfig<MonthlyTimeImportResultDto, MonthlyTimeImportRes>().TwoWays();
            config.NewConfig<MonthlyTimeValidateResultDto, MonthlyTimeValidateRes>().TwoWays();
            config.NewConfig<MonthlyTimeMakeLiveResultDto, MonthlyTimeMakeLiveRes>().TwoWays();
            config.NewConfig<MonthlyTimeLogDto, MonthlyTimeLogRes>().TwoWays();
            config.NewConfig<WorkGroupTimeCodeRes, WorkGroupTimeCodeDto>().TwoWays();
            config.NewConfig<WorkGroupValidTimeCodeRes, WorkGroupValidTimeCodeDto>().TwoWays();
            config.NewConfig<WgSummarisedStaffTimeUsageRowRes, WgSummarisedStaffTimeUsageRowDto>().TwoWays();
            config.NewConfig<WgSummarisedStaffTimeUsageSummaryRes, WgSummarisedStaffTimeUsageSummaryDto>().TwoWays();
            config.NewConfig<JobTitleLookupItemRes, JobTitleLookupItem>().TwoWays();
            config.NewConfig<WgSummarisedStaffTimeUsageDto, WgSummarisedStaffTimeUsageRes>().TwoWays();
            config.NewConfig<SummarisedWgTimeRes, SummarisedWgTimeDto>().TwoWays();
            config.NewConfig<SummarisedWgTimeSummaryRes, SummarisedWgTimeSummaryDto>().TwoWays();
            config.NewConfig<SummarisedWgTimePivotRes, SummarisedWgTimeViewDto>().TwoWays();
            config.NewConfig<ProjectTitleLookupRes, ProjectTitleLookupItem>().TwoWays();
            config.NewConfig<SummarisedWgTimeRowDto, SummarisedWgTimeRes>()
                .Map(dest => dest.SumOfTime, src => src.TotalTime)
                .Map(dest => dest.SumOfCost, src => src.TotalCost);
            config.NewConfig<WorkGroupReportEmailResultDto, WorkGroupReportEmailResultRes>().TwoWays();
            config.NewConfig<TestSupplierViewRes, TestSupplierViewDto>().TwoWays();
            config.NewConfig<RecreateSummaryLogRes, RecreateSummaryLogDto>().TwoWays();
            config.NewConfig<ReleasePeriodRes, ReleasePeriodDto>().TwoWays();
            config.NewConfig<ReleaseSummaryDto, ReleaseSummaryRes>();
            config.NewConfig<TestPriceCheckDto, TestPriceCheckRes>().TwoWays();
            config.NewConfig<TestPriceCheckReq, TestPriceCheckDto>().TwoWays();
            config.NewConfig<TestFeePlanDto, TestFeePlanRes>().TwoWays();
            config.NewConfig<TimePurchaseProjectDto, TimePurchaseProjectRes>();
            config.NewConfig<TimeSaleProfitCentreDto, TimeSaleProfitCentreRes>();
            config.NewConfig<TimeSaleWorkGroupReq, TimeSaleWorkGroupDto>();
            config.NewConfig<TimeSaleWorkGroupDto, TimeSaleWorkGroupRes>();
            config.NewConfig<TestSaleSellingWorkgroupDto, TestSaleSellingWorkgroupRes>();
            config.NewConfig<TestSaleBuyingProjectDto, TestSaleBuyingProjectRes>();
            config.NewConfig<WgTestCapabilitiesWithDescriptionDto, WgTestCapabilitiesWithDescriptionRes>();
            config.NewConfig<TestReqBreakdownDto, TestReqBreakdownRes>().TwoWays();
            config.NewConfig<BatchJobHistoryRes, BatchJobHistoryDto>().TwoWays();
            config.NewConfig<BatchJobQueueRes, BatchJobQueueDto>().TwoWays();
            config.NewConfig<BatchJobEventTriggerRes, BatchJobEventTriggerDto>().TwoWays();
            config.NewConfig<TestActualBreakdownDto, TestActualBreakdownRes>().TwoWays();
            config.NewConfig<TestPlanCostBreakdownDto, TestPlanCostBreakdownRes>().TwoWays();
        }
    }
}