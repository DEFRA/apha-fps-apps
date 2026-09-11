using Apha.Common.Contracts.PACT;


using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;
using Mapster;

namespace Apha.FPS.Application.Mappings
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
            config.NewConfig<StaffJobView, StaffJobViewDto>().TwoWays();
            config.NewConfig<StaffJobZtView, StaffJobZtViewDto>()
                .Map(dest => dest.ZtDescription, src => src.Name)
                .TwoWays();
            config.NewConfig<StaffWorkgroupLookup, StaffWorkgroupLookupDto>().TwoWays();
            config.NewConfig<StaffJob, StaffJobDto>().TwoWays();
            config.NewConfig<FpsSetting, FpsSettingDto>().TwoWays();
            config.NewConfig<Program, ProgramDto>().TwoWays();
            config.NewConfig<ProgramPlanCostView, ProgramPlanCostDto>().TwoWays();
            config.NewConfig<Project, ProjectDto>().TwoWays();
            config.NewConfig<ProjectSpecificQueryItem, ProjectSpecificQueryDto>().TwoWays();
            config.NewConfig<ProjectView, Project>().TwoWays();
            config.NewConfig<Contract, ContractDto>().TwoWays();
            config.NewConfig<AnimalCostView, AnimalCostViewDto>().TwoWays();
            config.NewConfig<AnimalSnapshotView, AnimalSnapshotViewDto>().TwoWays();
            config.NewConfig<Animal, AnimalDto>().TwoWays();
            config.NewConfig<AnimalRequest, AnimalRequestDto>().TwoWays();
            config.NewConfig<AccountCode, AccountCodeDto>().TwoWays();
            config.NewConfig<SubAccount, SubAccountDto>().TwoWays();
            config.NewConfig<ProjectGroup, ProjectGroupDto>().TwoWays();
            config.NewConfig<Employee, EmployeeDto>().TwoWays();
            config.NewConfig<Manager, ManagerDto>().TwoWays();
            config.NewConfig<ProjectView, ProjectDto>().TwoWays();
            config.NewConfig<PactProjectView, ProjectDto>()
                .Map(d => d.FpsCalYear, s => s.FpsYear);
            config.NewConfig<ProjectDto, PactProjectView>()
                .Map(d => d.FpsYear, s => s.FpsCalYear);
            config.NewConfig<YearMaster, YearMasterDto>().TwoWays();
            config.NewConfig<Division, DivisionDto>().TwoWays();
            config.NewConfig<DivisionGrade, DivisionGradeDto>().TwoWays();
            config.NewConfig<Grade, GradeDto>()
                .Map(d => d.Description, s => s.DescLong);
            config.NewConfig<GradeDto, Grade>()
                .Map(d => d.DescLong, s => s.Description);

            config.NewConfig<Agency, AgencyDto>().TwoWays();
            config.NewConfig<TimeCostCalcsView, TimeCostCalcsViewDto>().TwoWays();
            config.NewConfig<ProjectStaffPlanView, ProjectStaffPlanViewDto>().TwoWays();
            config.NewConfig<ProjectStaffPlanDetailsView, ProjectStaffPlanDetailsViewDto>().TwoWays();
            config.NewConfig<ProjectGroupStaffPlanView, ProjectGroupStaffPlanViewDto>().TwoWays();
            config.NewConfig<AdditionalCost, AdditionalCostDto>().TwoWays();
            config.NewConfig<AccountCategory, AccountCategoryDto>().TwoWays();
            config.NewConfig<WorkGroupPerson, WorkGroupPersonDto>().TwoWays();


            // ResourceSetUp
            config.NewConfig<ProfitCentre, ProfitCentreDto>().TwoWays();
            config.NewConfig<ProfitCentreView, ProfitCentreDto>()
                .Map(d => d.ProfitCentreId, s => s.ProfitCentreId)
                .Map(d => d.ProfitCentreName, s => s.ProfitCentreName)
                .Map(d => d.Division, s => s.Division)
                .Map(d => d.ContTarget, s => s.ContTarget)
                .Map(d => d.ProfitCentreHead, s => s.ProfitCentreHead)
                .Map(d => d.DivisionId, s => s.DivisionId)
                .Map(d => d.EmailRecipient, s => s.EmailRecipient);
            config.NewConfig<ProfitCentreCostSummary, ProfitCentreCostDto>().TwoWays();
            config.NewConfig<ProfitCentreGrade, ProfitCentreGradeDto>().TwoWays();

            config.NewConfig<WorkgroupGrade, WorkgroupGradeDto>().TwoWays();
            config.NewConfig<WorkGroupGradeView, WorkgroupGradeDto>().TwoWays();

            config.NewConfig<WorkGroupEmployee, WorkGroupEmployeeDto>().TwoWays();
            config.NewConfig<WorkGroupEmployeeView, WorkGroupEmployeeDto>().TwoWays();
            config.NewConfig<PactStaff, PactStaffDto>().TwoWays();
            config.NewConfig<ProjectProfitabilityView, ProjectProfitabilityDto>().TwoWays();
            config.NewConfig<MonthlyOutput, MonthlyOutputDto>().TwoWays();
            // ProjectProfitabilityVlaView
            //   Property names are aligned between entity and DTO; no mapping overrides needed.
            //   Covers: Id, JobCode, Program, Customer, Manager, Status, StaffCosts, TestCost,
            //   AnimalCosts, AdditionalCosts, TotalCosts, Budget, Profit, TargetProfit, OffTarget.
            config.NewConfig<ProjectProfitabilityVlaView, ProjectProfitabilityVlaDto>().TwoWays();

            config.NewConfig<User, UserDto>().TwoWays();

            //   Property names are aligned between entity and DTO; no mapping overrides needed.
            //   Covers: CostCentreNo (double), ProfitCentre (string), FpsYear (int).
            config.NewConfig<CostCentre, CostCentreDto>().TwoWays();

            // BudgetResourceLevel
            config.NewConfig<Bid, BidDto>().TwoWays();
            config.NewConfig<BidView, BidViewDto>().TwoWays();
            config.NewConfig<TestsRequiredByWgView, TestsRequiredByWgDto>().TwoWays();
            config.NewConfig<TestsRequiredByRcView, TestsRequiredByRcDto>().TwoWays();
            config.NewConfig<GenericBidView, GenericBidViewDto>().TwoWays();
            config.NewConfig<ProjectExceptionalCostView, ProjectExceptionalCostViewDto>().TwoWays();
            config.NewConfig<Purchase, PurchaseDto>().TwoWays();
            //   All 5 log entities from fps schema partitioned tables.
            //   Property names are fully aligned between entity and DTO; no mapping overrides needed.
            //   Covers all columns: sequenceno, parentproject/jobcode/testcode, date range, user tracking fields, fpsyear.
            config.NewConfig<ProjectLog, ProjectLogDto>().TwoWays();
            config.NewConfig<StaffJobLog, StaffJobLogDto>().TwoWays();
            config.NewConfig<TestRequirementLog, TestRequirementLogDto>().TwoWays();
            config.NewConfig<AnimalRequestLog, AnimalRequestLogDto>().TwoWays();
            config.NewConfig<AdditionalCostLog, AdditionalCostLogDto>().TwoWays();
            // MaintTotalBusinessOverheads
            config.NewConfig<TotalBusinessOverheads, TotalBusinessOverheadsDto>()
                .Map(d => d.TotalBusinessOverheads, s => s.BusinessOverheads);
            config.NewConfig<TotalBusinessOverheadsDto, TotalBusinessOverheads>()
                .Map(d => d.BusinessOverheads, s => s.TotalBusinessOverheads);

            // StaffResourceUtilisation
            config.NewConfig<StaffResourceUtilisationView, StaffResourceUtilisationDto>().TwoWays();
            //TestListVLA
            config.NewConfig<TestRCCost, TestRCCostDto>().TwoWays();
            config.NewConfig<TestRequirementRCCost, TestRequirementRCCostDto>().TwoWays();

            // ResourceAllocation — Stage 2 Check Resource Allocation
            config.NewConfig<ResourceStaffGeneralSummaryRow, ResourceStaffAllocationDto>().TwoWays();
            config.NewConfig<ResourceStaffJobView, ResourceStaffJobDto>().TwoWays();
            config.NewConfig<ResourceStaffJobDetailRow, ResourceStaffJobDetailDto>().TwoWays();

            // ResourceMgmtReplan — Resource Re-allocation Screen (frmRM_RePlan)
            config.NewConfig<ResourceMgmtReplanView, ResourceMgmtReplanViewDto>().TwoWays();
            config.NewConfig<ProjectStaffReplanView, ProjectStaffReplanDto>().TwoWays();
            config.NewConfig<StaffJobRmView, ResourceMgmtReplanDto>()
                .Map(d => d.StaffId, s => s.StaffId)
                .Map(d => d.JobCode, s => s.JobCode)
                .Map(d => d.PlannedHours, s => s.PlannedHours ?? 0);
            config.NewConfig<ResourceMgmtReplanDto, StaffJobRmView>();
            config.NewConfig<ResourceMgmtReplanDto, ResourceMgmtReplanRow>().TwoWays();

            // Workgroup Staff Plan view
            config.NewConfig<WgStaffPlanView, WgStaffPlanViewDto>().TwoWays();
            config.NewConfig<BatchJobHistory, BatchJobHistoryDto>().TwoWays();
            config.NewConfig<BatchJobQueue, BatchJobQueueDto>().TwoWays();
            config.NewConfig<BatchJobQueue, BatchJobQueueRes>().TwoWays();
            config.NewConfig<BatchJobQueue, BatchJobEventTriggerDto>()
                .Map(dest => dest.Jobqueue, src => src)
                .Ignore(dest => dest.EventId);

            config.NewConfig<FpsSetting, FpsSettingDto>().TwoWays();
            config.NewConfig<YearEndFpsSetting, YearEndFpsSettingDto>().TwoWays();
            config.NewConfig<MonthHour, MonthHourDto>().TwoWays();
            config.NewConfig<YearEndMonthHour, YearEndMonthHourDto>().TwoWays();


            // All six pairs are property-name aligned between entity and DTO; no mapping overrides required.
            // Covers all query types from frmDeptIncome: Time, Tests, Animals, Additional, Totals, plus PeriodLookup.
            config.NewConfig<DepartmentIncomeTime, DepartmentIncomeTimeDto>().TwoWays();
            config.NewConfig<DepartmentIncomeTest, DepartmentIncomeTestDto>().TwoWays();
            config.NewConfig<DepartmentIncomeAnimal, DepartmentIncomeAnimalDto>().TwoWays();
            config.NewConfig<DepartmentIncomeAdditional, DepartmentIncomeAdditionalDto>().TwoWays();
            config.NewConfig<DepartmentIncomeTotals, DepartmentIncomeTotalsDto>().TwoWays();
            config.NewConfig<PeriodLookup, PeriodLookupDto>()
                .Map(d => d.AccntsPeriod, s => (int)s.AccntsPeriod)
                .Map(d => d.MonthNumber, s => (int)s.MonthNumber);
            config.NewConfig<PeriodLookupDto, PeriodLookup>();
            config.NewConfig<Period, PeriodSnapshotDto>()
                .Map(d => d.FinalSummariesRun, s => s.FinalSummariesRun == -1)
                .Map(d => d.PeriodLocked, s => s.PeriodLocked == -1);
            config.NewConfig<PeriodSnapshotDto, Period>();
        }
    }
}
