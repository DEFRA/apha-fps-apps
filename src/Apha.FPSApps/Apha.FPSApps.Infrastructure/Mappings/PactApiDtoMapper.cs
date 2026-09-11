using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Mapster;
namespace Apha.FPSApps.Infrastructure.Mappings
{
    public class PactApiDtoMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // PACT
            config.NewConfig<JobCodeDto, JobCodeReq>().TwoWays();
            config.NewConfig<JobCodeDto, JobCodeRes>().TwoWays();
            config.NewConfig<TimeCodeValidDto, TimeCodeValidReq>().TwoWays();
            config.NewConfig<TimeCodeValidDto, TimeCodeValidRes>().TwoWays();
            config.NewConfig<WorkGroupDto, WorkGroupRes>().TwoWays();
            config.NewConfig<WorkGroupViewDto, WorkGroupViewRes>().TwoWays();

            // WorkGroup Maintenance (CRUD + lookups)
            config.NewConfig<WorkGroupDto, WorkGroupMaintenanceRes>().TwoWays();
            config.NewConfig<WorkGroupDto, WorkGroupMaintenanceReq>().TwoWays();
            config.NewConfig<OwnerDto, OwnerRes>().TwoWays();

            config.NewConfig<MonthDto, MonthRes>().TwoWays();
            config.NewConfig<CalenderMonthDto, CalenderMonthRes>().TwoWays();
            config.NewConfig<ProjectInvoiceDto, ProjectInvoiceReq>().TwoWays();
            config.NewConfig<ProjectInvoiceDto, ProjectInvoiceRes>().TwoWays();
            config.NewConfig<MonthlyInvoicesSummaryItemDto, MonthlyInvoicesSummaryItemRes>().TwoWays();
            config.NewConfig<PaginationDto, Pagination>().TwoWays();
            config.NewConfig<MonthlyInvoicesPivotDto, MonthlyInvoicesPivotRes>().TwoWays();
            config.NewConfig<ProjectSubContractDto, ProjectSubContractReq>().TwoWays();
            config.NewConfig<ProjectSubContractDto, ProjectSubContractRes>().TwoWays();
            config.NewConfig<SubContractRmsImportRowDto, SubContractRmsImportRowReq>().TwoWays();
            config.NewConfig<SubContractRmsImportRowDto, SubContractRmsImportRowRes>().TwoWays();
            config.NewConfig<SubContractRmsImportReqDto, SubContractRmsImportReq>().TwoWays();
            config.NewConfig<SubContractRmsImportResultDto, SubContractRmsImportRes>().TwoWays();
            config.NewConfig<TestCapabilityDto, TestCapabilityReq>().TwoWays();
            config.NewConfig<TestCapabilityDto, TestCapabilityRes>().TwoWays();
            config.NewConfig<TestRequirementDto, TestRequirementReq>().TwoWays();
            config.NewConfig<TestRequirementDto, TestRequirementtRes>().TwoWays();
            config.NewConfig<TestorProductDto, TestorProductReq>().TwoWays();
            config.NewConfig<TestorProductDto, TestorProductRes>().TwoWays();
            config.NewConfig<MonthlySubContractsSummaryItemDto, MonthlySubContractsSummaryItemRes>().TwoWays();
            config.NewConfig<MonthlySubContractsPivotDto, MonthlySubContractsPivotRes>().TwoWays();     

            config.NewConfig<ProjectMonthDto, ProjectMonthReq>().TwoWays();
            config.NewConfig<ProjectMonthDto, ProjectMonthRes>().TwoWays();
            config.NewConfig<MonthDto, MonthRes>().TwoWays();
            config.NewConfig<ProjectProfileDto, ProjectProfileRes>().TwoWays();
            config.NewConfig<ProjectProfileCumulativeDto, ProjectProfileCumulativeRes>().TwoWays();
            config.NewConfig<MonthlyOutputLogDto, MonthlyOutputLogRes>().TwoWays();
            config.NewConfig<MonthlyOutputRes, PactMonthlyOutputDto>().TwoWays();
            config.NewConfig<PactMonthlyOutputDto, MonthlyOutputReq>();
            config.NewConfig<MonthlyOutputImportReqDto, MonthlyOutputImportReq>();
            config.NewConfig<MonthlyOutputImportRowDto, MonthlyOutputImportRowReq>();
            config.NewConfig<MonthlyOutputImportRes, MonthlyOutputImportResultDto>();
            config.NewConfig<StagingMonthlyOutputRes, StagingMonthlyOutputDto>().TwoWays();
            config.NewConfig<StagingMonthlyOutputDto, StagingMonthlyOutputReq>().TwoWays();
            config.NewConfig<MonthlyOutputValidateRes, MonthlyOutputValidateResultDto>().TwoWays();
            config.NewConfig<MonthlyOutputMakeLiveRes, MonthlyOutputMakeLiveResultDto>().TwoWays();
            config.NewConfig<MonthlyTimeDto, MonthlyTimeReq>().TwoWays();
            config.NewConfig<MonthlyTimeDto, MonthlyTimeRes>().TwoWays();
            config.NewConfig<StagingMonthlyTimeDto, StagingMonthlyTimeReq>().TwoWays();
            config.NewConfig<StagingMonthlyTimeDto, StagingMonthlyTimeRes>().TwoWays();
            config.NewConfig<BulkUpdateStagingMonthlyTimeNamesDto, BulkUpdateStagingMonthlyTimeNamesReq>().TwoWays();
            config.NewConfig<BulkUpdateStagingMonthlyTimeNamesResultDto, BulkUpdateStagingMonthlyTimeNamesRes>().TwoWays();
            config.NewConfig<MonthlyTimeImportRowDto, MonthlyTimeImportRowReq>().TwoWays();
            config.NewConfig<MonthlyTimeImportRowDto, MonthlyTimeImportRowRes>().TwoWays();
            config.NewConfig<MonthlyTimeImportReqDto, MonthlyTimeImportReq>().TwoWays();
            config.NewConfig<MonthlyTimeImportResultDto, MonthlyTimeImportRes>().TwoWays();
            config.NewConfig<MonthlyTimeValidateResultDto, MonthlyTimeValidateRes>().TwoWays();
            config.NewConfig<MonthlyTimeMakeLiveResultDto, MonthlyTimeMakeLiveRes>().TwoWays();
            config.NewConfig<MonthlyTimeLogDto, MonthlyTimeLogRes>().TwoWays();
            config.NewConfig<CalenderMonthDto, CalenderMonthRes>().TwoWays();
            config.NewConfig<WorkGroupTimeCodeDto, WorkGroupTimeCodeRes>().TwoWays();
            config.NewConfig<WorkGroupValidTimeCodeDto, WorkGroupValidTimeCodeRes>().TwoWays();
            config.NewConfig<WgSummarisedStaffTimeUsageRowDto, WgSummarisedStaffTimeUsageRowRes>().TwoWays();
            config.NewConfig<WgSummarisedStaffTimeUsageSummaryDto, WgSummarisedStaffTimeUsageSummaryRes>().TwoWays();
            config.NewConfig<JobTitleLookupItemDto, JobTitleLookupItemRes>().TwoWays();
            config.NewConfig<WgSummarisedStaffTimeUsageDto, WgSummarisedStaffTimeUsageRes>().TwoWays();
            config.NewConfig<SummarisedWgTimeDto, SummarisedWgTimeRes>().TwoWays();
            config.NewConfig<SummarisedWgTimeSummaryDto, SummarisedWgTimeSummaryRes>().TwoWays();
            config.NewConfig<SummarisedWgTimeViewDto, SummarisedWgTimePivotRes>().TwoWays();
            config.NewConfig<ProjectTitleLookupRes, SummarisedWgTimeProjectTitleLookupItem>().TwoWays();
            config.NewConfig<ApiResponse<SummarisedWgTimePivotRes>, ApiResponseDto<SummarisedWgTimeViewDto>>();
            config.NewConfig<WorkGroupReportEmailResultDto, WorkGroupReportEmailResultRes>().TwoWays();
            config.NewConfig<TestSupplierViewDto, TestSupplierViewRes>().TwoWays();
            config.NewConfig<RecreateSummaryLogDto, RecreateSummaryLogRes>().TwoWays();
            config.NewConfig<ReleasePeriodDto, ReleasePeriodRes>().TwoWays();
            config.NewConfig<ReleaseSummaryRes, ReleaseSummaryDto>();
            config.NewConfig<TestPriceCheckDto, TestPriceCheckRes>().TwoWays();
            config.NewConfig<TestPriceCheckDto, TestPriceCheckReq>();

            // Invoice Import
            config.NewConfig<InvoiceImportRowDto, InvoiceImportRowReq>().TwoWays();
            config.NewConfig<InvoiceImportRowDto, InvoiceImportRowRes>().TwoWays();
            config.NewConfig<InvoiceImportReqDto, InvoiceImportReq>();
            config.NewConfig<InvoiceImportRes, InvoiceImportResultDto>();

            config.NewConfig<InvoiceImportResultDto, InvoiceImportRes>().TwoWays();

            config.NewConfig<InvoiceImportRowDto, InvoiceImportRowReq>();
            config.NewConfig<InvoiceImportRes, InvoiceImportResultDto>();
            config.NewConfig<ApiResponse<InvoiceImportRes>, ApiResponseDto<InvoiceImportResultDto>>();
            config.NewConfig<TimePurchaseProjectDto, TimePurchaseProjectRes>().TwoWays();
            config.NewConfig<TimeSaleProfitCentreDto, TimeSaleProfitCentreRes>().TwoWays();
            config.NewConfig<TimeSaleWorkGroupDto, TimeSaleWorkGroupRes>().TwoWays();
            config.NewConfig<TestSaleSellingWorkgroupDto, TestSaleSellingWorkgroupRes>().TwoWays();
            config.NewConfig<TestSaleBuyingProjectDto, TestSaleBuyingProjectRes>().TwoWays();
            config.NewConfig<WgTestCapabilitiesWithDescriptionDto, WgTestCapabilitiesWithDescriptionRes>().TwoWays();
            config.NewConfig<TestReqBreakdownRes, TestReqBreakdownDto>().TwoWays();
            config.NewConfig<BatchJobQueueDto, BatchJobQueueRes>().TwoWays();
            config.NewConfig<BatchJobHistoryDto, BatchJobHistoryRes>().TwoWays();
            config.NewConfig<BatchJobEventTriggerDto, BatchJobEventTriggerRes>().TwoWays();
            config.NewConfig<TestActualBreakdownRes, TestActualBreakdownDto>().TwoWays();
            config.NewConfig<TestPlanCostBreakdownRes, TestPlanCostBreakdownDto>().TwoWays();
        }
    }
}
