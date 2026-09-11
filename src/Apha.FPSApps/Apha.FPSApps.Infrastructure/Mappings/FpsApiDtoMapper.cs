using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Mapster;
namespace Apha.FPSApps.Infrastructure.Mappings
{
    public class FpsApiDtoMapper : IRegister
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

            config.NewConfig<StaffJobViewDto, StaffJobViewRes>().TwoWays();
            config.NewConfig<StaffJobZtViewDto, StaffJobZtViewRes>().TwoWays();
            config.NewConfig<StaffWorkgroupLookupDto, StaffWorkgroupLookupRes>().TwoWays();
            config.NewConfig<StaffJobDto, StaffJobReq>().TwoWays();
            config.NewConfig<StaffJobDto, StaffJobRes>().TwoWays();
            config.NewConfig<ProgramDto, ProgramReq>().TwoWays();
            config.NewConfig<ProgramDto, ProgramRes>().TwoWays();
            config.NewConfig<ManagerDto, ManagerRes>().TwoWays();
            config.NewConfig<EmployeeDto, EmployeeReq>().TwoWays();
            config.NewConfig<EmployeeDto, EmployeeRes>().TwoWays();

            // FPS Project
            // CustIncome in the FPS API wire format lives in ProjectReq.BudgetExt (see FPS RequestMapper)
            config.NewConfig<ProjectDto, ProjectReq>()
                .Map(d => d.BudgetExt, s => s.CustIncome);
            config.NewConfig<ProjectReq, ProjectDto>()
                .Map(d => d.CustIncome, s => s.BudgetExt);
            config.NewConfig<ProjectDto, ProjectRes>().TwoWays();
            config.NewConfig<ProjectSpecificQueryDto, ProjectSpecificQueryRes>().TwoWays();

            // FPS Lookups
            config.NewConfig<StatusDto, StatusRes>().TwoWays();
            config.NewConfig<DiseaseDto, DiseaseRes>().TwoWays();
            config.NewConfig<CustomerDto, CustomerRes>().TwoWays();
            config.NewConfig<ContractDto, ContractRes>().TwoWays();
            config.NewConfig<ProjectGroupDto, ProjectGroupRes>().TwoWays();
            
            // FPS Animal Plan
            config.NewConfig<AnimalCostViewDto, AnimalCostViewRes>().TwoWays();
            config.NewConfig<AnimalSnapshotViewDto, AnimalSnapshotViewRes>().TwoWays();
            config.NewConfig<AnimalDto, AnimalRes>().TwoWays();
            config.NewConfig<AnimalRequestDto, AnimalRequestReq>().TwoWays();
            config.NewConfig<AnimalRequestDto, AnimalRequestRes>().TwoWays();
            
            // FPS Animal Master
            config.NewConfig<AnimalDto, AnimalReq>().TwoWays();
            
            // YEar Master
            config.NewConfig<YearMasterDto, YearMasterRes>().TwoWays();
            config.NewConfig<YearMasterDto, YearMasterReq>().TwoWays();

            // Testor Product
            config.NewConfig<Apha.FPSApps.Application.Dtos.PACT.TestorProductDto, Apha.Common.Contracts.FPS.TestorProductRes>().TwoWays();
            
            // View Project Plan vs Actual Staff
            config.NewConfig<TimeCostCalcsViewDto, TimeCostCalcsViewRes>().TwoWays();
            config.NewConfig<TimeCostCalcsTotalsDto, TimeCostCalcsTotalsRes>().TwoWays();
            
            // Division
            config.NewConfig<DivisionDto, DivisionRes>().TwoWays();
            config.NewConfig<DivisionDto, DivisionReq>().TwoWays();

            // Division Grade
            config.NewConfig<DivisionGradeDto, DivisionGradeRes>().TwoWays();
            config.NewConfig<DivisionGradeDto, DivisionGradeReq>().TwoWays();

            // Grade CRUD: maps frontend GradeDto to/from backend GradeReq (POST/PUT) and GradeRes (GET/POST/PUT responses)
            config.NewConfig<GradeDto, GradeReq>().TwoWays();
            config.NewConfig<GradeDto, GradeRes>().TwoWays();

            // Agency
            config.NewConfig<AgencyDto, AgencyRes>().TwoWays();

            // Additional Cost
            config.NewConfig<AdditionalCostDto, AdditionalCostReq>().TwoWays();
            config.NewConfig<AdditionalCostDto, AdditionalCostRes>().TwoWays();
            config.NewConfig<AccountCategoryDto, AccountCategoryRes>().TwoWays();
            config.NewConfig<AccountCategoryDto, AccountCategoryReq>().TwoWays();

            // View Project Plan vs Actual Tests
            config.NewConfig<MonthlyOutputDto, MonthlyOutputRes>().TwoWays();

            // ProgrammeNewProject (merged into ProjectDto - mappings above)
            config.NewConfig<AccountCodeDto, AccountCodeRes>().TwoWays();
            config.NewConfig<SubAccountDto, SubAccountRes>()
                .Map(d => d.SubAccount, s => s.SubAccount);
            config.NewConfig<SubAccountRes, SubAccountDto>()
                .Map(d => d.SubAccount, s => s.SubAccount);
            config.NewConfig<CostCentreWorkgroupDto, CostCentreWorkgroupRes>().TwoWays();

            // CostCentre CRUD: maps frontend CostCentreDto to/from backend CostCentreReq (POST/PUT)
            //   and CostCentreRes (GET/GET-paged/POST/PUT responses)
            config.NewConfig<CostCentreDto, CostCentreReq>().TwoWays();
            config.NewConfig<CostCentreDto, CostCentreRes>().TwoWays();

            config.NewConfig<PactStaffDto, PactStaffRes>().TwoWays();
            config.NewConfig<WorkGroupPersonDto, WorkGroupPersonRes>().TwoWays();

            // Resource Set-Up
            config.NewConfig<ProfitCentreDto, ProfitCentreRes>().TwoWays();
            config.NewConfig<ProfitCentreDto, ProfitCentreReq>().TwoWays();
            config.NewConfig<ProfitCentreCostDto, ProfitCentreCostRes>().TwoWays();
            config.NewConfig<ProfitCentreGradeDto, ProfitCentreGradeRes>().TwoWays();
            config.NewConfig<ProfitCentreGradeDto, ProfitCentreGradeReq>().TwoWays();
            config.NewConfig<WorkgroupGradeDto, WorkgroupGradeRes>().TwoWays();
            config.NewConfig<WorkGroupEmployeeDto, WorkGroupEmployeeReq>().TwoWays();
            config.NewConfig<WorkGroupEmployeeDto, WorkGroupEmployeeRes>().TwoWays();
            config.NewConfig<WorkGroupEmployeeStaffDto, WorkGroupEmployeeReq>().TwoWays();
            config.NewConfig<WorkGroupEmployeeStaffDto, WorkGroupEmployeeRes>().TwoWays();

            // ProjectProfitability
            config.NewConfig<ProjectProfitabilityDto, ProjectProfitabilityRes>().TwoWays();            // ProjectProfitabilityVla
            //   ForMember(Id) handles int->int? coercion: Id=GetValueOrDefault(0) on reverse.
            //   TotalCount is on Res only; silently ignored in Res->Dto direction (see DEFERRED note above).
            config.NewConfig<ProjectProfitabilityVlaDto, ProjectProfitabilityVlaRes>()
                .Map(d => d.Project, s => s.JobCode)
                .Map(d => d.Id, s => s.Id.GetValueOrDefault(0));
            config.NewConfig<ProjectProfitabilityVlaRes, ProjectProfitabilityVlaDto>()
                .Map(d => d.JobCode, s => s.Project)
                .Map(d => d.Id, s => (int?)s.Id);

            // Staff Plan view
            config.NewConfig<ProjectStaffPlanViewDto, ProjectStaffPlanViewRes>().TwoWays();

            // Staff Plan Details view
            config.NewConfig<ProjectStaffPlanDetailsViewDto, ProjectStaffPlanDetailsViewRes>().TwoWays();

            // Project Group Staff Plan view
            config.NewConfig<ProjectGroupStaffPlanViewDto, ProjectGroupStaffPlanViewRes>().TwoWays();

            // Workgroup Staff Plan view
            config.NewConfig<WgStaffPlanViewDto, WgStaffPlanViewRes>().TwoWays();

            config.NewConfig<PactStaffDto, PactStaffRes>().TwoWays();

            // WorkgroupGrade
            config.NewConfig<WorkgroupGradeDto, WorkgroupGradeReq>().TwoWays();

            // Job Code (ZT lookup) - now served from PACT API
            config.NewConfig<FpsJobCodeZtDto, Apha.Common.Contracts.PACT.JobCodeZtRes>().TwoWays();


            // Income/Contribution from Time Sales (frmTimeSellerPC)
            config.NewConfig<ContributionSummaryRowDto, ContributionSummaryRowRes>().TwoWays();
            config.NewConfig<ContributionSummaryTotalsDto, ContributionSummaryTotalsRes>().TwoWays();

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

            // Audit logs are read-only so no .ReverseMap() — frontend never writes back to backend audit tables.
            config.NewConfig<ProjectLogRes, ProjectLogDto>()
                .Map(d => d.CaseWorkSub, s => s.CaseworkSub)
                .Map(d => d.PlanCaseWorkDebit, s => s.PlanCaseworkDebit);

            // StaffJobLog: Res.Name (staff display name resolved server-side) maps to Dto.Name by convention.
            config.NewConfig<StaffJobLogRes, StaffJobLogDto>();

            // TestRequirementLog: type-coercion — Res.UnitPrice is double? but Dto.UnitPrice is decimal?;
            //   Res.NoRequired is int? but Dto.NoRequired is double?. Explicit ForMember casts applied.
            config.NewConfig<TestRequirementLogRes, TestRequirementLogDto>()
                .Map(d => d.UnitPrice, s => s.UnitPrice.HasValue ? (decimal?)Convert.ToDecimal(s.UnitPrice.Value) : null)
                .Map(d => d.NoRequired, s => s.NoRequired.HasValue ? (double?)Convert.ToDouble(s.NoRequired.Value) : null);

            // AnimalRequestLog: all property names and types align — convention mapping suffices.
            config.NewConfig<AnimalRequestLogRes, AnimalRequestLogDto>();

            // AdditionalCostLog: all property names and types align — convention mapping suffices.
            config.NewConfig<AdditionalCostLogRes, AdditionalCostLogDto>();
            // UserPermission
            config.NewConfig<UserDto, UserRes>().TwoWays();
            config.NewConfig<UserDto, UserReq>().TwoWays();
            config.NewConfig<UserPermissionDataDto, UserPermissionRes>().TwoWays();
            config.NewConfig<UserPermissionDataDto, UserPermissionReq>().TwoWays();
            config.NewConfig<PermissionOptionsDto, PermissionOptionsRes>().TwoWays();
            
            // Total Business Overheads
            config.NewConfig<TotalBusinessOverheadsDto, TotalBusinessOverheadsReq>().TwoWays();
            config.NewConfig<TotalBusinessOverheadsDto, TotalBusinessOverheadsRes>().TwoWays();

            // Setting
            config.NewConfig<SettingDto, FpsSettingRes>().TwoWays();
            config.NewConfig<SettingDto, FpsSettingReq>().TwoWays();
            config.NewConfig<YearEndSettingDto, FpsYearEndSettingRes>().TwoWays();

            // MonthHour
            config.NewConfig<MonthHourDto, MonthHourRes>().TwoWays();
            config.NewConfig<MonthHourDto, MonthHourReq>().TwoWays();
            config.NewConfig<YearEndMonthHourDto, YearEndMonthHourRes>().TwoWays();
            // StaffResourceUtilisation
            config.NewConfig<StaffResourceUtilisationDto, StaffResourceUtilisationRes>().TwoWays();

            //  TestListVLA
            config.NewConfig<TestRCCostDto, TestRCCostRes>().TwoWays();
            config.NewConfig<TestRCCostDto, TestRCCostReq>().TwoWays();
            config.NewConfig<TestRequirementRCCostDto, TestRequirementRCCostRes>().TwoWays();
            config.NewConfig<TestRequirementRCCostDto, TestRequirementRCCostReq>().TwoWays();

            // ResourceAllocation — Stage 2 Check Resource Allocation
            config.NewConfig<ResourceStaffAllocationDto, ResourceStaffAllocationRes>().TwoWays();
            config.NewConfig<ResourceStaffJobDto, ResourceStaffJobRes>().TwoWays();
            config.NewConfig<ResourceStaffJobDetailDto, ResourceStaffJobDetailRes>().TwoWays();

            // ResourceMgmtReplan — Resource Re-allocation Screen (frmRM_RePlan)
            config.NewConfig<ResourceMgmtReplanViewDto, ResourceMgmtReplanViewRes>().TwoWays();
            config.NewConfig<ResourceMgmtReplanStaffJobDto, ResourceMgmtReplanStaffJobRes>().TwoWays();

            // Resource Replan — project staff replan
            config.NewConfig<ProjectStaffReplanDto, ProjectStaffReplanRes>().TwoWays();
            // Year End batch job
            config.NewConfig<BatchJobQueueDto, BatchJobQueueRes>().TwoWays();
            config.NewConfig<BatchJobHistoryDto, BatchJobHistoryRes>().TwoWays();
            config.NewConfig<BatchJobEventTriggerDto, BatchJobEventTriggerRes>().TwoWays();

            // Bulk Rates — Common Res -> Web DTO. One-directional: the Web app never builds a
            // Res from its own Dto (nothing serializes these back out), so a ReverseMap would
            // declare an unused equivalence. BulkRatesValidationErrorRes deliberately maps onto
            // a narrower BulkRatesValidationErrorDto (no Id/JobQueueId/UploadVersion) — a valid,
            // ordinary AutoMapper mapping since only unmapped *destination* members are an error.
            config.NewConfig<BulkRatesQueueEntryRes, BulkRatesQueueEntryDto>();
            config.NewConfig<BulkRatesUploadMetadataRes, BulkRatesUploadMetadataDto>();
            config.NewConfig<BulkRatesRowCountsRes, BulkRatesRowCountsDto>();
            config.NewConfig<BulkRatesQueueLogRes, BulkRatesQueueLogDto>();
            config.NewConfig<BulkRatesValidationErrorRes, BulkRatesValidationErrorDto>();
            config.NewConfig<BulkRatesFecStagingRowRes, BulkRatesFecStagingRowDto>();
            config.NewConfig<BulkRatesAgrupStagingRowRes, BulkRatesAgrupStagingRowDto>();
            config.NewConfig<BulkRatesAnimalStagingRowRes, BulkRatesAnimalStagingRowDto>();
            config.NewConfig<BulkRatesStaffStagingRowRes, BulkRatesStaffStagingRowDto>();
            config.NewConfig<BulkRatesRequestDetailRes, BulkRatesRequestDetailDto>();
            config.NewConfig<BulkRatesUploadResultRes, BulkRatesUploadResultDto>();
            config.NewConfig<BulkRatesStagingDataRes, BulkRatesStagingDataDto>();
        }
    }
}

