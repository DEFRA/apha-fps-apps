using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Mapster;

namespace Apha.FPSApps.Web.Mappings
{
    public class PactViewModelMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<WorkGroupDto, WorkGroup>().TwoWays();
            config.NewConfig<ProjectDto, Project>().TwoWays();
            config.NewConfig<ProfitCentreDto, ProfitCentre>().TwoWays();
            config.NewConfig<PactProjectViewModel, ProjectDto>().TwoWays();
            config.NewConfig<ProjectJobCodeViewModel, JobCodeDto>().TwoWays();
            config.NewConfig<JobCodeViewModel, JobCodeDto>().TwoWays();
            config.NewConfig<PortfolioJobCodeViewModel, JobCodeDto>().TwoWays();
            config.NewConfig<TimeCodeValidDto, TimeCodeViewModel>().TwoWays();
            config.NewConfig<TimeCodeValidDto, ValidTimeCodeViewModel>()
                .Map(dest => dest.Project, src => src.ParentProject)
                .Ignore(dest => dest.OriginalWorkGroup);
            config.NewConfig<ValidTimeCodeViewModel, TimeCodeValidDto>()
                .Map(dest => dest.ParentProject, src => src.ParentProject);
            config.NewConfig<ProjectInvoiceItem, ProjectInvoiceDto>().TwoWays();
            config.NewConfig<InvoiceItem, ProjectInvoiceDto>().TwoWays();
            config.NewConfig<ProjectSubContractItem, ProjectSubContractDto>().TwoWays();
            // Mapping for standalone SubContract page
            config.NewConfig<SubContractItem, ProjectSubContractDto>()
                .Ignore(dest => dest.DailyRate)
                .Ignore(dest => dest.AnimalDays);
            config.NewConfig<ProjectSubContractDto, SubContractItem>()
                .Map(dest => dest.Counter, src => src.SubContCounter);

            config.NewConfig<SubContractRmsItem, ProjectSubContractDto>().TwoWays();
            config.NewConfig<SubContractRmsImportRowDto, SubContractRmsFailedItem>().TwoWays();
            config.NewConfig<InvoiceImportRowDto, InvoiceImportFailedItem>().TwoWays();

            config.NewConfig<TestCapabilityItem, TestCapabilityDto>().TwoWays();
            config.NewConfig<ConstituentTestItem, TestCapabilityDto>().TwoWays();

            // Mapping for WorkGroup-focused Test Capability view
            config.NewConfig<WorkGroupTestCapabilityItem, TestCapabilityDto>().TwoWays();

            config.NewConfig<PortfolioTimeCodeViewModel, TimeCodeValidDto>().TwoWays();

            config.NewConfig<TestRequirementItem, TestRequirementDto>().TwoWays();
            config.NewConfig<TestPurchaseRequirementItem, TestRequirementDto>().TwoWays();

            config.NewConfig<ProgramViewModel, ProgramDto>().TwoWays();
            config.NewConfig<ProgramProjectItem, ProjectDto>().TwoWays();
            config.NewConfig<TestorProductDto, TestOrProductViewModel>().TwoWays();

            config.NewConfig<ProjectMonthItem, ProjectMonthDto>().TwoWays();
            config.NewConfig<PactStaffDto, WorkGroupPeopleItem>().TwoWays();
            config.NewConfig<WorkGroupPersonDto, WorkGroupPerson>().TwoWays();
            config.NewConfig<MonthlyOutputLogDto, MonthlyOutputLogItem>().TwoWays();
            config.NewConfig<MonthlyTimeDto, MonthlyTimeLiveItem>()
                .Map(dest => dest.CompositeKey, src => $"{src.PactStaffId}|{src.TimeCode}|{src.Month}|{src.ParentProject}");
            config.NewConfig<StagingMonthlyTimeDto, StagingMonthlyTimeItem>()
                .Map(dest => dest.Passed, src => src.Passed ?? false);
            config.NewConfig<MonthlyTimeLiveItem, MonthlyTimeDto>()
                .Map(dest => dest.FpsYear, src => src.FpsYear ?? 0);
            config.NewConfig<StagingMonthlyTimeItem, StagingMonthlyTimeDto>()
                .Map(dest => dest.Passed, src => (bool?)src.Passed);
            config.NewConfig<StagingMonthlyTimeDto, StagingMonthlyTimeExportItem>()
                .Map(dest => dest.Name, src => string.IsNullOrWhiteSpace(src.Name) ? src.PactStaffId : src.Name)
                .Map(dest => dest.Passed, src => src.Passed ?? false);
            config.NewConfig<MonthlyTimeLogDto, MonthlyTimeLogItem>().TwoWays();
            config.NewConfig<CalenderMonthDto, CalenderMonth>().TwoWays();
            config.NewConfig<WorkGroupTimeCodeDto, WorkGroupTimeCodeItem>().TwoWays();
            config.NewConfig<WorkGroupValidTimeCodeDto, WorkGroupValidTimeCodeItem>().TwoWays();
            config.NewConfig<WgSummarisedStaffTimeUsageRowDto, WgSummarisedStaffTimeUsageRow>()
                .Map(dest => dest.April, src => Math.Round(src.April, 2))
                .Map(dest => dest.May, src => Math.Round(src.May, 2))
                .Map(dest => dest.June, src => Math.Round(src.June, 2))
                .Map(dest => dest.July, src => Math.Round(src.July, 2))
                .Map(dest => dest.August, src => Math.Round(src.August, 2))
                .Map(dest => dest.September, src => Math.Round(src.September, 2))
                .Map(dest => dest.October, src => Math.Round(src.October, 2))
                .Map(dest => dest.November, src => Math.Round(src.November, 2))
                .Map(dest => dest.December, src => Math.Round(src.December, 2))
                .Map(dest => dest.January, src => Math.Round(src.January, 2))
                .Map(dest => dest.February, src => Math.Round(src.February, 2))
                .Map(dest => dest.March, src => Math.Round(src.March, 2))
                .Map(dest => dest.TotalTime, src => Math.Round(src.TotalTime, 2))
                .Map(dest => dest.TotalCost, src => Math.Round(src.TotalCost, 2));
            config.NewConfig<WgSummarisedStaffTimeUsageRow, WgSummarisedStaffTimeUsageRowDto>();
            config.NewConfig<WgSummarisedStaffTimeUsageSummaryDto, WgSummarisedStaffTimeUsageSummary>()
                .Map(dest => dest.TotalApril, src => Math.Round(src.TotalApril, 2))
                .Map(dest => dest.TotalMay, src => Math.Round(src.TotalMay, 2))
                .Map(dest => dest.TotalJune, src => Math.Round(src.TotalJune, 2))
                .Map(dest => dest.TotalJuly, src => Math.Round(src.TotalJuly, 2))
                .Map(dest => dest.TotalAugust, src => Math.Round(src.TotalAugust, 2))
                .Map(dest => dest.TotalSeptember, src => Math.Round(src.TotalSeptember, 2))
                .Map(dest => dest.TotalOctober, src => Math.Round(src.TotalOctober, 2))
                .Map(dest => dest.TotalNovember, src => Math.Round(src.TotalNovember, 2))
                .Map(dest => dest.TotalDecember, src => Math.Round(src.TotalDecember, 2))
                .Map(dest => dest.TotalJanuary, src => Math.Round(src.TotalJanuary, 2))
                .Map(dest => dest.TotalFebruary, src => Math.Round(src.TotalFebruary, 2))
                .Map(dest => dest.TotalMarch, src => Math.Round(src.TotalMarch, 2))
                .Map(dest => dest.GrandTotalTime, src => Math.Round(src.GrandTotalTime, 2))
                .Map(dest => dest.GrandTotalCost, src => Math.Round(src.GrandTotalCost, 2))
                .Map(dest => dest.StandardHoursPerMonth, src => Math.Round(src.StandardHoursPerMonth, 2))
                .Map(dest => dest.TotalStandardHours, src => Math.Round(src.TotalStandardHours, 2))
                .Map(dest => dest.GrandTotalPercentAllocated, src => Math.Round(src.GrandTotalPercentAllocated, 2))
                .Map(dest => dest.PercentAllocatedApril, src => Math.Round(src.PercentAllocatedApril, 2))
                .Map(dest => dest.PercentAllocatedMay, src => Math.Round(src.PercentAllocatedMay, 2))
                .Map(dest => dest.PercentAllocatedJune, src => Math.Round(src.PercentAllocatedJune, 2))
                .Map(dest => dest.PercentAllocatedJuly, src => Math.Round(src.PercentAllocatedJuly, 2))
                .Map(dest => dest.PercentAllocatedAugust, src => Math.Round(src.PercentAllocatedAugust, 2))
                .Map(dest => dest.PercentAllocatedSeptember, src => Math.Round(src.PercentAllocatedSeptember, 2))
                .Map(dest => dest.PercentAllocatedOctober, src => Math.Round(src.PercentAllocatedOctober, 2))
                .Map(dest => dest.PercentAllocatedNovember, src => Math.Round(src.PercentAllocatedNovember, 2))
                .Map(dest => dest.PercentAllocatedDecember, src => Math.Round(src.PercentAllocatedDecember, 2))
                .Map(dest => dest.PercentAllocatedJanuary, src => Math.Round(src.PercentAllocatedJanuary, 2))
                .Map(dest => dest.PercentAllocatedFebruary, src => Math.Round(src.PercentAllocatedFebruary, 2))
                .Map(dest => dest.PercentAllocatedMarch, src => Math.Round(src.PercentAllocatedMarch, 2));
            config.NewConfig<WgSummarisedStaffTimeUsageSummary, WgSummarisedStaffTimeUsageSummaryDto>();

            config.NewConfig<SummarisedWgTimeDto, SummarisedWgTimePivotRow>()
                .Map(dest => dest.April, src => Math.Round(src.April ?? 0, 2))
                .Map(dest => dest.May, src => Math.Round(src.May ?? 0, 2))
                .Map(dest => dest.June, src => Math.Round(src.June ?? 0, 2))
                .Map(dest => dest.July, src => Math.Round(src.July ?? 0, 2))
                .Map(dest => dest.August, src => Math.Round(src.August ?? 0, 2))
                .Map(dest => dest.September, src => Math.Round(src.September ?? 0, 2))
                .Map(dest => dest.October, src => Math.Round(src.October ?? 0, 2))
                .Map(dest => dest.November, src => Math.Round(src.November ?? 0, 2))
                .Map(dest => dest.December, src => Math.Round(src.December ?? 0, 2))
                .Map(dest => dest.January, src => Math.Round(src.January ?? 0, 2))
                .Map(dest => dest.February, src => Math.Round(src.February ?? 0, 2))
                .Map(dest => dest.March, src => Math.Round(src.March ?? 0, 2))
                .Map(dest => dest.SumOfTime, src => Math.Round(src.SumOfTime, 2))
                .Map(dest => dest.SumOfCost, src => Math.Round(src.SumOfCost, 2))
                .Map(dest => dest.Budget, src => src.Budget.HasValue ? Math.Round(src.Budget.Value, 2) : (decimal?)null)
                .Map(dest => dest.PercentSpent, src => src.PercentSpent.HasValue ? Math.Round(src.PercentSpent.Value, 2) : (decimal?)null);
            config.NewConfig<SummarisedWgTimeSummaryDto, SummarisedWgTimeSummary>()
                .Map(dest => dest.TotalApril, src => Math.Round(src.TotalApril, 2))
                .Map(dest => dest.TotalMay, src => Math.Round(src.TotalMay, 2))
                .Map(dest => dest.TotalJune, src => Math.Round(src.TotalJune, 2))
                .Map(dest => dest.TotalJuly, src => Math.Round(src.TotalJuly, 2))
                .Map(dest => dest.TotalAugust, src => Math.Round(src.TotalAugust, 2))
                .Map(dest => dest.TotalSeptember, src => Math.Round(src.TotalSeptember, 2))
                .Map(dest => dest.TotalOctober, src => Math.Round(src.TotalOctober, 2))
                .Map(dest => dest.TotalNovember, src => Math.Round(src.TotalNovember, 2))
                .Map(dest => dest.TotalDecember, src => Math.Round(src.TotalDecember, 2))
                .Map(dest => dest.TotalJanuary, src => Math.Round(src.TotalJanuary, 2))
                .Map(dest => dest.TotalFebruary, src => Math.Round(src.TotalFebruary, 2))
                .Map(dest => dest.TotalMarch, src => Math.Round(src.TotalMarch, 2))
                .Map(dest => dest.GrandTotalTime, src => Math.Round(src.GrandTotalTime, 2))
                .Map(dest => dest.GrandTotalCost, src => Math.Round(src.GrandTotalCost, 2));

            config.NewConfig<RecreateSummaryLogDto, RecreateSummaryLogItem>()
                .Map(dest => dest.User, src => src.Comments);

            config.NewConfig<Apha.FPSApps.Application.Dtos.PACT.BatchJobHistoryDto, BatchJobHistoryItem>();

            config.NewConfig<ProfitCentreCostDto, ProfitCenterCostItem>().TwoWays();

            config.NewConfig<ReleasePeriodDto, PeriodMonth>().TwoWays();

            config.NewConfig<ReleasePeriodDto, ReleasePeriodItem>().TwoWays();

            config.NewConfig<WgTestCapabilitiesWithDescriptionDto, WgTestCapabilitiesWithDescriptionItem>().TwoWays();

            // Monthly Output
            config.NewConfig<Apha.FPSApps.Application.Dtos.PACT.PactMonthlyOutputDto, MonthlyOutputLiveItem>()
                .Map(dest => dest.CompositeKey, src => $"{src.TestCode}|{src.Buyer}|{src.Month}|{src.WorkGroup}");
            config.NewConfig<MonthlyOutputLiveItem, Apha.FPSApps.Application.Dtos.PACT.PactMonthlyOutputDto>()
                .Map(dest => dest.FpsYear, src => src.FpsYear ?? 0)
                .Ignore(dest => dest.OriginalTestCode)
                .Ignore(dest => dest.OriginalBuyer)
                .Ignore(dest => dest.OriginalMonth)
                .Ignore(dest => dest.OriginalWorkGroup)
                .Ignore(dest => dest.WgBuyer);
            config.NewConfig<StagingMonthlyOutputDto, StagingMonthlyOutputItem>()
                .Map(dest => dest.Passed, src => src.Passed ?? false);
            config.NewConfig<StagingMonthlyOutputItem, StagingMonthlyOutputDto>()
                .Map(dest => dest.Passed, src => (bool?)src.Passed);
            config.NewConfig<StagingMonthlyOutputDto, StagingMonthlyOutputExportItem>();
        }
    }
}
