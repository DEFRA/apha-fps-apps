using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Dtos.BulkRates;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Mapster;

namespace Apha.FPS.Api.Mappings
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

            config.NewConfig<StaffJobViewDto, StaffJobViewRes>().TwoWays();
            config.NewConfig<StaffJobZtViewDto, StaffJobZtViewRes>().TwoWays();
            config.NewConfig<StaffWorkgroupLookupDto, StaffWorkgroupLookupRes>().TwoWays();
            config.NewConfig<StaffJobDto, StaffJobReq>().TwoWays();
            config.NewConfig<StaffJobDto, StaffJobRes>().TwoWays();

            config.NewConfig<FpsSettingRes, FpsSettingDto>().TwoWays();
            config.NewConfig<FpsSettingReq, FpsSettingDto>().TwoWays();
            config.NewConfig<FpsYearEndSettingRes, YearEndFpsSettingDto>().TwoWays();

            config.NewConfig<AnimalCostViewDto, AnimalCostViewRes>().TwoWays();
            config.NewConfig<AnimalSnapshotViewDto, AnimalSnapshotViewRes>().TwoWays();
            config.NewConfig<AnimalDto, AnimalRes>().TwoWays();
            config.NewConfig<AnimalReq, AnimalDto>().TwoWays();
            config.NewConfig<AnimalRequestDto, AnimalRequestReq>().TwoWays();
            config.NewConfig<AnimalRequestDto, AnimalRequestRes>().TwoWays();
            config.NewConfig<EmployeeDto, EmployeeReq>().TwoWays();
            config.NewConfig<EmployeeDto, EmployeeRes>().TwoWays();
            config.NewConfig<ManagerDto, ManagerRes>().TwoWays();
            config.NewConfig<ProgramReq, ProgramDto>().TwoWays();
            config.NewConfig<ProgramRes, ProgramDto>().TwoWays();
            config.NewConfig<ProgramPlanCostRes, ProgramPlanCostDto>().TwoWays();
            config.NewConfig<ProjectDto, ProjectReq>()
                .Map(d => d.BudgetExt, s => s.CustIncome);
            config.NewConfig<ProjectReq, ProjectDto>()
                .Map(d => d.CustIncome, s => s.BudgetExt);
            config.NewConfig<ProjectDto, ProjectRes>()
                .Map(d => d.BudgetExt, s => s.CustIncome);
            config.NewConfig<ProjectRes, ProjectDto>()
                .Map(d => d.CustIncome, s => s.BudgetExt);

            //   JobCode (DTO natural key) -> Project (response display column per HTML prototype)
            //   Id is int? in DTO (nullable ROW_NUMBER) -> int in Res (non-nullable contract property)
            config.NewConfig<ProjectProfitabilityVlaDto, ProjectProfitabilityVlaRes>()
                .Map(d => d.Id, s => s.Id.GetValueOrDefault(0))
                .Map(d => d.Project, s => s.JobCode);
            config.NewConfig<PaginatedResult<ProjectProfitabilityVlaDto>, PaginationRes<ProjectProfitabilityVlaRes>>();

            config.NewConfig<ProjectSpecificQueryDto, ProjectSpecificQueryRes>().TwoWays();
            config.NewConfig<PaginatedResult<ProjectSpecificQueryDto>, PaginationRes<ProjectSpecificQueryRes>>();

            config.NewConfig<ContractDto, ContractRes>()
                .Map(d => d.ContractNo, s => s.Contractno)
                .Map(d => d.Category, s => s.Category);
            config.NewConfig<YearMasterRes, YearMasterDto>().TwoWays();
            config.NewConfig<DivisionReq, DivisionDto>().TwoWays();
            config.NewConfig<DivisionRes, DivisionDto>().TwoWays();
            config.NewConfig<GradeDto, GradeRes>().TwoWays();
            config.NewConfig<GradeReq, GradeDto>().TwoWays();
            config.NewConfig<DivisionGradeReq, DivisionGradeDto>().TwoWays();
            config.NewConfig<DivisionGradeRes, DivisionGradeDto>().TwoWays();
            config.NewConfig<AgencyRes, AgencyDto>().TwoWays();

            // ProgrammeNewProject mappings
            config.NewConfig<AccountCodeDto, AccountCodeRes>().TwoWays();
            config.NewConfig<SubAccountDto, SubAccountRes>()
                .Map(d => d.SubAccount, s => s.SubAccountName);
            config.NewConfig<SubAccountRes, SubAccountDto>()
                .Map(d => d.SubAccountName, s => s.SubAccount);
            config.NewConfig<ProjectGroupDto, ProjectGroupRes>().TwoWays();
            config.NewConfig<TimeCostCalcsViewDto, TimeCostCalcsViewRes>().TwoWays();
            config.NewConfig<TimeCostCalcsTotalsDto, TimeCostCalcsTotalsRes>().TwoWays();
            config.NewConfig<AdditionalCostDto, AdditionalCostReq>().TwoWays();
            config.NewConfig<AdditionalCostDto, AdditionalCostRes>().TwoWays();
            config.NewConfig<AccountCategoryDto, AccountCategoryReq>().TwoWays();
            config.NewConfig<AccountCategoryDto, AccountCategoryRes>().TwoWays();
            config.NewConfig<MonthlyOutputDto, MonthlyOutputRes>().TwoWays();
            config.NewConfig<CostCentreWorkgroup, CostCentreWorkgroupRes>().TwoWays();
            //   CostCentreReq ? CostCentreDto (POST create, PUT update request binding; FpsYear excluded from Req — set server-side)
            //   CostCentreDto ? CostCentreRes (GET paged, GET by id, POST, PUT response)
            config.NewConfig<CostCentreReq, CostCentreDto>().TwoWays();
            config.NewConfig<CostCentreDto, CostCentreRes>().TwoWays();
            config.NewConfig<WorkGroupPersonDto, WorkGroupPersonRes>().TwoWays();

            // ResourceSetUp
            config.NewConfig<ProfitCentreDto, ProfitCentreRes>().TwoWays();
            config.NewConfig<ProfitCentreReq, ProfitCentreDto>().TwoWays();
            config.NewConfig<ProfitCentreCostDto, ProfitCentreCostRes>().TwoWays();
            config.NewConfig<ProfitCentreGradeDto, ProfitCentreGradeRes>().TwoWays();
            config.NewConfig<ProfitCentreGradeReq, ProfitCentreGradeDto>().TwoWays();
            config.NewConfig<WorkgroupGradeDto, WorkgroupGradeRes>().TwoWays();

            // POST CreateWorkGroupEmployeeAsync added in Phase 5. New fields (TimeRecorder, StartDate,
            // EndDate, HoursPerWeek) are resolved by Mapster name convention — no mapping override needed.
            config.NewConfig<WorkGroupEmployeeDto, WorkGroupEmployeeReq>().TwoWays();
            config.NewConfig<WorkGroupEmployeeDto, WorkGroupEmployeeRes>().TwoWays();

            config.NewConfig<ProjectProfitabilityDto, ProjectProfitabilityRes>().TwoWays();

            config.NewConfig<ProjectStaffPlanViewDto, ProjectStaffPlanViewRes>().TwoWays();
            config.NewConfig<PaginatedResult<ProjectStaffPlanViewDto>, PaginationRes<ProjectStaffPlanViewRes>>();

            config.NewConfig<ProjectStaffPlanDetailsViewDto, ProjectStaffPlanDetailsViewRes>().TwoWays();
            config.NewConfig<PaginatedResult<ProjectStaffPlanDetailsViewDto>, PaginationRes<ProjectStaffPlanDetailsViewRes>>();

            config.NewConfig<ProjectGroupStaffPlanViewDto, ProjectGroupStaffPlanViewRes>().TwoWays();
            config.NewConfig<PaginatedResult<ProjectGroupStaffPlanViewDto>, PaginationRes<ProjectGroupStaffPlanViewRes>>();

            // Workgroup Staff Plan view
            config.NewConfig<WgStaffPlanViewDto, WgStaffPlanViewRes>().TwoWays();
            config.NewConfig<PaginatedResult<WgStaffPlanViewDto>, PaginationRes<WgStaffPlanViewRes>>();

            config.NewConfig<PactStaffDto, PactStaffRes>().TwoWays();
            config.NewConfig<WorkgroupGradeDto, WorkgroupGradeReq>().TwoWays();


            // UserPermission
            config.NewConfig<UserDto, UserRes>().TwoWays();
            config.NewConfig<UserReq, UserDto>().TwoWays();
            config.NewConfig<UserPermissionDto, UserPermissionRes>().TwoWays();
            config.NewConfig<UserPermissionReq, UserPermissionDto>().TwoWays();
            config.NewConfig<PermissionOptionsDto, PermissionOptionsRes>().TwoWays();

            // BudgetResourceLevel
            config.NewConfig<BidDto, BidReq>().TwoWays();
            config.NewConfig<BidDto, BidRes>().TwoWays();
            config.NewConfig<BidViewDto, BidViewRes>().TwoWays();
            config.NewConfig<TestsRequiredByWgDto, TestsRequiredByWgRes>().TwoWays();
            config.NewConfig<TestsRequiredByRcDto, TestsRequiredByRcRes>().TwoWays();
            config.NewConfig<GenericBidViewDto, GenericBidViewRes>().TwoWays();
            config.NewConfig<ProjectExceptionalCostViewDto, ProjectExceptionalCostViewRes>().TwoWays();
            config.NewConfig<PurchaseDto, PurchaseReq>().TwoWays();
            config.NewConfig<PurchaseDto, PurchaseRes>().TwoWays();

            // TimeSellerPC - frmTimeSellerPC
            config.NewConfig<ContributionSummaryRowDto, ContributionSummaryRowRes>().TwoWays();
            config.NewConfig<ContributionSummaryTotalsDto, ContributionSummaryTotalsRes>().TwoWays();


            //   TestRCCostReq and TestRCCostRes both map bidirectionally to TestRCCostDto.
            //   PaginatedResult<TestRCCostDto> -> PaginationRes<TestRCCostRes> for paged list endpoint.
            config.NewConfig<TestRCCostReq, TestRCCostDto>().TwoWays();
            config.NewConfig<TestRCCostRes, TestRCCostDto>().TwoWays();
            config.NewConfig<PaginatedResult<TestRCCostDto>, PaginationRes<TestRCCostRes>>();

            //   TestRequirementRCCostReq and TestRequirementRCCostRes both map bidirectionally to TestRequirementRCCostDto.
            //   PaginatedResult<TestRequirementRCCostDto> -> PaginationRes<TestRequirementRCCostRes> for paged list endpoint.
            config.NewConfig<TestRequirementRCCostReq, TestRequirementRCCostDto>().TwoWays();
            config.NewConfig<TestRequirementRCCostRes, TestRequirementRCCostDto>().TwoWays();
            config.NewConfig<PaginatedResult<TestRequirementRCCostDto>, PaginationRes<TestRequirementRCCostRes>>();

            // 5 log tables: project_log, staffjob_log, testreq_log, animalreq_log, additionalcosts_log
            config.NewConfig<ProjectLogDto, ProjectLogRes>().TwoWays();
            config.NewConfig<PaginatedResult<ProjectLogDto>, PaginationRes<ProjectLogRes>>();

            config.NewConfig<StaffJobLogDto, StaffJobLogRes>().TwoWays();
            config.NewConfig<PaginatedResult<StaffJobLogDto>, PaginationRes<StaffJobLogRes>>();

            config.NewConfig<TestRequirementLogDto, TestRequirementLogRes>().TwoWays();
            config.NewConfig<PaginatedResult<TestRequirementLogDto>, PaginationRes<TestRequirementLogRes>>();

            config.NewConfig<AnimalRequestLogDto, AnimalRequestLogRes>().TwoWays();
            config.NewConfig<PaginatedResult<AnimalRequestLogDto>, PaginationRes<AnimalRequestLogRes>>();

            config.NewConfig<AdditionalCostLogDto, AdditionalCostLogRes>().TwoWays();
            config.NewConfig<PaginatedResult<AdditionalCostLogDto>, PaginationRes<AdditionalCostLogRes>>();

            // MaintTotalBusinessOverheads
            config.NewConfig<TotalBusinessOverheadsDto, TotalBusinessOverheadsReq>().TwoWays();
            config.NewConfig<TotalBusinessOverheadsDto, TotalBusinessOverheadsRes>().TwoWays();
            // StaffResourceUtilisation
            config.NewConfig<StaffResourceUtilisationDto, StaffResourceUtilisationRes>().TwoWays();



            // ResourceAllocation — Stage 2 Check Resource Allocation
            config.NewConfig<ResourceStaffAllocationDto, ResourceStaffAllocationRes>().TwoWays();
            config.NewConfig<ResourceStaffJobDto, ResourceStaffJobRes>().TwoWays();
            config.NewConfig<ResourceStaffJobDetailDto, ResourceStaffJobDetailRes>().TwoWays();

            // Resource Replan — project staff replan
            config.NewConfig<ProjectStaffReplanDto, ProjectStaffReplanRes>().TwoWays();
            config.NewConfig<BatchJobHistoryRes, BatchJobHistoryDto>().TwoWays();
            config.NewConfig<BatchJobQueueRes, BatchJobQueueDto>().TwoWays();
            config.NewConfig<BatchJobEventTriggerRes, BatchJobEventTriggerDto>().TwoWays();
            config.NewConfig<MonthHourRes, MonthHourDto>().TwoWays();
            config.NewConfig<MonthHourReq, MonthHourDto>().TwoWays();
            config.NewConfig<YearEndMonthHourRes, YearEndMonthHourDto>().TwoWays();

            // Bulk Rates — Application DTO -> Common Res (API response contract). One-directional:
            // these DTOs are never built from a Res, so a reverse config here would just declare an
            // unused, misleading equivalence. The three mutation Req bodies (Create/Reject/Cancel)
            // are small enough to unpack directly in the controller instead of mapping through
            // Mapster, matching the YearEndDataSetupReq precedent.
            config.NewConfig<BulkRatesQueueEntryDto, BulkRatesQueueEntryRes>();
            config.NewConfig<BulkRatesUploadMetadataDto, BulkRatesUploadMetadataRes>();
            config.NewConfig<BulkRatesRowCountsDto, BulkRatesRowCountsRes>();
            config.NewConfig<BulkRatesQueueLogDto, BulkRatesQueueLogRes>();
            config.NewConfig<BulkRatesValidationErrorDto, BulkRatesValidationErrorRes>();
            config.NewConfig<BulkRatesFecStagingRowDto, BulkRatesFecStagingRowRes>();
            config.NewConfig<BulkRatesAgrupStagingRowDto, BulkRatesAgrupStagingRowRes>();
            config.NewConfig<BulkRatesAnimalStagingRowDto, BulkRatesAnimalStagingRowRes>();
            config.NewConfig<BulkRatesStaffStagingRowDto, BulkRatesStaffStagingRowRes>();
            config.NewConfig<BulkRatesRequestDto, BulkRatesRequestDetailRes>();
            config.NewConfig<BulkRatesUploadResultDto, BulkRatesUploadResultRes>();
            config.NewConfig<BulkRatesStagingDataDto, BulkRatesStagingDataRes>();

            // All property names are identical across Dto and Res; .TwoWays() covers both directions.
            config.NewConfig<DepartmentIncomeTimeDto, DepartmentIncomeTimeRes>().TwoWays();
            config.NewConfig<DepartmentIncomeTestDto, DepartmentIncomeTestRes>().TwoWays();
            config.NewConfig<DepartmentIncomeAnimalDto, DepartmentIncomeAnimalRes>().TwoWays();
            config.NewConfig<DepartmentIncomeAdditionalDto, DepartmentIncomeAdditionalRes>().TwoWays();
            config.NewConfig<DepartmentIncomeTotalsDto, DepartmentIncomeTotalsRes>().TwoWays();
            config.NewConfig<PeriodLookupDto, PeriodLookupRes>().TwoWays();
            config.NewConfig<PeriodSnapshotDto, PeriodSnapshotRes>().TwoWays();
        }
    }
}
