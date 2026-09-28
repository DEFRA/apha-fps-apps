using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Pagination;
using Mapster;

namespace Apha.FPSApps.Infrastructure.Mappings
{
    public class CostbookApiDtoMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig(typeof(ApiResponse<>), typeof(ApiResponseDto<>));
            config.NewConfig(typeof(ApiResponseDto<>), typeof(ApiResponse<>));
            config.NewConfig<ApiError, ApiErrorDto>().TwoWays();
            config.NewConfig<ApiMeta, ApiMetaDto>().TwoWays();

            // -- Existing project mappings -------------------------------------
            config.NewConfig<ProjectDto, ProjectRes>().TwoWays();
            config.NewConfig<ProjectDto, ProjectReq>().TwoWays();
            config.NewConfig<CustomerDto, CustomerRes>().TwoWays();
            config.NewConfig<DiseaseDto, DiseaseRes>().TwoWays();
            config.NewConfig<Application.Dtos.CostBook.ProgramDto, Common.Contracts.Costbook.ProgramRes>().TwoWays();
            config.NewConfig<StaffDto, StaffRes>().TwoWays();
            config.NewConfig<ContractDto, ContractRes>().TwoWays();
            config.NewConfig<ProjectEditDataDto, ProjectEditRes>().TwoWays();

            // -- Yearly details: Res/Req ? Dto (used by CostBookYearlyDetailsApiClient) --
            config.NewConfig<ProjectHeaderRes, ProjectHeaderDto>().TwoWays();
            config.NewConfig<ProjectYearRes, ProjectYearDto>().TwoWays();
            config.NewConfig<ProjectYearDto, ProjectYearReq>().TwoWays();
            config.NewConfig<StaffRequirementRes, StaffRequirementDto>().TwoWays();
            config.NewConfig<StaffRequirementDto, StaffRequirementReq>().TwoWays();
            config.NewConfig<TestRequirementRes, TestRequirementDto>().TwoWays();
            config.NewConfig<TestRequirementDto, TestRequirementReq>().TwoWays();
            config.NewConfig<AnimalRequirementRes, AnimalRequirementDto>().TwoWays();
            config.NewConfig<AnimalRequirementDto, AnimalRequirementReq>().TwoWays();
            config.NewConfig<AdditionalCostRes, AdditionalCostDto>().TwoWays();
            config.NewConfig<AdditionalCostDto, AdditionalCostReq>().TwoWays();
            config.NewConfig<PayRateRes, PayRateDto>().TwoWays();
            config.NewConfig<AnimalRateRes, AnimalRateDto>().TwoWays();
            config.NewConfig<AccountCategoryRes, AccountCategoryDto>().TwoWays();
            config.NewConfig<TestCodeLookupRes, TestCodeLookupDto>().TwoWays();
            config.NewConfig<AnimalLookupRes, AnimalLookupDto>().TwoWays();

            config.NewConfig<StaffYearsRowRes, StaffYearsRowDto>().TwoWays();
            config.NewConfig<StaffYearsPivotRes, StaffYearsPivotDto>().TwoWays();
            config.NewConfig<StaffEffortRowRes, StaffEffortRowDto>().TwoWays();
            config.NewConfig<StaffEffortPivotRes, StaffEffortPivotDto>().TwoWays();
            config.NewConfig<ProjectCostsRowRes, ProjectCostsRowDto>().TwoWays();
            config.NewConfig<ProjectCostsPivotRes, ProjectCostsPivotDto>().TwoWays();
            config.NewConfig<ProjectYearCostSummaryRes, ProjectYearCostSummaryDto>().TwoWays();

            
            config.NewConfig<MaintenanceSettingsRes, MaintenanceSettingsDto>().TwoWays();
            config.NewConfig<MaintenanceSettingsDto, MaintenanceSettingsReq>().TwoWays();
            config.NewConfig<StaffRes, StaffDto>().TwoWays();
            config.NewConfig<StaffDto, StaffReq>().TwoWays();
            config.NewConfig<AccountGroupRes, AccountGroupDto>().TwoWays();
            config.NewConfig<AccountGroupDto, AccountGroupReq>().TwoWays();
            config.NewConfig<AccountCategoryMaintenanceRes, AccountCategoryMaintenanceDto>().TwoWays();
            config.NewConfig<AccountCategoryMaintenanceDto, AccountCategoryMaintenanceReq>().TwoWays();
        }
    }
}
