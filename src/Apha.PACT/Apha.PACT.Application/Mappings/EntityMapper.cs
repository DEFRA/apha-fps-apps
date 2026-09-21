using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;
using Mapster;

namespace Apha.PACT.Application.Mappings
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

            config.NewConfig<JobCode, JobCodeDto>().TwoWays();
            config.NewConfig<TimeCodeValid, TimeCodeValidDto>().TwoWays();
            config.NewConfig<WorkGroup, WorkGroupDto>().TwoWays();
            config.NewConfig<Owner, OwnerDto>().TwoWays();
            config.NewConfig<WorkGroupView, WorkGroupViewDto>();
            config.NewConfig<Month, MonthDto>().TwoWays();
            config.NewConfig<MonthDto, MonthRes>().TwoWays();
            config.NewConfig<ProjectInvoice, ProjectInvoiceDto>().TwoWays();
            config.NewConfig<InvoiceImportRow, InvoiceImportRowDto>().TwoWays();
            config.NewConfig<ProjectInvoiceStaging, InvoiceImportRowDto>();
            config.NewConfig<ProjectSubContract, ProjectSubContractDto>().TwoWays();
            config.NewConfig<SubContractRmsImportRow, SubContractRmsImportRowDto>().TwoWays();
            config.NewConfig<ProjectSubcontractStaging, SubContractRmsImportRowDto>();
            config.NewConfig<SubContractRmsImport, SubContractRmsImportDto>().TwoWays();
            config.NewConfig<SubContractRmsImportResult, SubContractRmsImportResultDto>().TwoWays();
            config.NewConfig<TestCapability, TestCapabilityDto>().TwoWays();
            config.NewConfig<TestCapabilityWithDescription, TestCapabilityDto>();
            config.NewConfig<TestRequirement, TestRequirementtDto>().TwoWays();
            config.NewConfig<TestRequirementDetail, TestRequirementtDto>();
            config.NewConfig<TestSupplierView, TestSupplierViewDto>().TwoWays();
            config.NewConfig<TestorProduct, TestorProductDto>().TwoWays();
            config.NewConfig<CalenderMonth, CalenderMonthDto>().TwoWays();
            config.NewConfig<ProjectMonth, ProjectMonthDto>().TwoWays();
            config.NewConfig<ProjectMonthFinal, ProjectMonthFinalDto>().TwoWays();
            config.NewConfig<MonthlyOutputLog, MonthlyOutputLogDto>().TwoWays();
            config.NewConfig<MonthlyOutput, MonthlyOutputDto>().TwoWays();
            config.NewConfig<StagingMonthlyOutput, StagingMonthlyOutputDto>().TwoWays();
            config.NewConfig<MonthlyOutputImportRowDto, StagingMonthlyOutputDto>().TwoWays();
            config.NewConfig<MonthlyTime, MonthlyTimeDto>().TwoWays();
            config.NewConfig<MonthlyTimeStaff, MonthlyTimeDto>();
            config.NewConfig<StagingMonthlyTime, StagingMonthlyTimeDto>().TwoWays();
            config.NewConfig<MonthlyTimeImportRowDto, StagingMonthlyTimeDto>().TwoWays();
            config.NewConfig<MonthlyTimeLog, MonthlyTimeLogDto>().TwoWays();
            config.NewConfig<MonthlyTimeLogFilter, MonthlyTimeLogFilterDto>().TwoWays();
            config.NewConfig<WorkGroupTimeCode, WorkGroupTimeCodeDto>().TwoWays();
            config.NewConfig<WorkGroupValidTimeCode, WorkGroupValidTimeCodeDto>().TwoWays();
            config.NewConfig<WgSummarisedStaffTimeUsageView, WgSummarisedStaffTimeUsageEntryDto>();
            config.NewConfig<SummarisedWgTimeView, SummarisedWgTimeDto>().TwoWays();
            config.NewConfig<SummarisedWgTimeView, SummarisedWgTimeEntryDto>();
            config.NewConfig<SummarisedWgTimeDto, SummarisedWgTimeRes>().TwoWays();
            config.NewConfig<RecreateSummaryLog, RecreateSummaryLogDto>().TwoWays();
            config.NewConfig<RecreateSummaryLogWithComment, RecreateSummaryLogDto>().TwoWays();
            config.NewConfig<ReleasePeriod, ReleasePeriodDto>().TwoWays();
            config.NewConfig<ReleaseSummary, ReleaseSummaryDto>();
            config.NewConfig<JobCodeZtLookup, JobCodeZtDto>().TwoWays();
            config.NewConfig<TestPriceCheckView, TestPriceCheckDto>().TwoWays();
            config.NewConfig<TestFeePlanView, TestFeePlanDto>().TwoWays();
            config.NewConfig<TimePurchaseProject, TimePurchaseProjectDto>();
            config.NewConfig<TimeSaleProfitCentre, TimeSaleProfitCentreDto>();
            config.NewConfig<TimeSaleWorkGroup, TimeSaleWorkGroupDto>();
            config.NewConfig<TestSaleSellingWorkgroup, TestSaleSellingWorkgroupDto>();
            config.NewConfig<TestSaleBuyingProject, TestSaleBuyingProjectDto>();
            config.NewConfig<WgTestCapabilitiesWithDescription, WgTestCapabilitiesWithDescriptionDto>();
            config.NewConfig<TestReqBreakdownView, TestReqBreakdownDto>().TwoWays();
            config.NewConfig<BatchJobHistory, BatchJobHistoryDto>().TwoWays();
            config.NewConfig<BatchJobQueue, BatchJobQueueDto>().TwoWays();
            config.NewConfig<BatchJobQueue, BatchJobQueueRes>().TwoWays();
            config.NewConfig<BatchJobQueue, BatchJobEventTriggerDto>()
                .Map(dest => dest.Jobqueue, src => src)
                .Ignore(dest => dest.EventId);
            config.NewConfig<TestActualBreakdownView, TestActualBreakdownDto>().TwoWays();

        }
    }
}