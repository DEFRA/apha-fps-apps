using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.CostBook.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Mapster;

namespace Apha.FPSApps.Web.Mappings
{
    public class CostbookViewModelMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {

            config.NewConfig<PaginationDto, PaginationModel>().TwoWays();

            // -- Existing project view model mappings --------------------------
            config.NewConfig<ProjectDto, ProjectItemViewModel>().TwoWays();
            config.NewConfig<ProjectDto, ProjectDetailViewModel>().TwoWays();
            config.NewConfig<ProjectDto, ProjectCreateEditViewModel>().TwoWays();

            // -- Yearly details: Dto ? ViewModel/Item -------------------------
            config.NewConfig<ProjectYearDto, ProjectYearRateItem>().TwoWays();
            config.NewConfig<StaffRequirementDto, StaffRequirementItem>().TwoWays();
            config.NewConfig<StaffRequirementDto, StaffRequirementFormItem>().TwoWays();
            config.NewConfig<TestRequirementDto, TestRequirementItem>().TwoWays();
            config.NewConfig<AnimalRequirementDto, AnimalRequirementItem>().TwoWays();
            config.NewConfig<AdditionalCostDto, AdditionalCostItem>().TwoWays();
            
            config.NewConfig<InflationSettingsItem, MaintenanceSettingsDto>().TwoWays();            
            config.NewConfig<ProfitMarginsItem, MaintenanceSettingsDto>().TwoWays();            
            config.NewConfig<AccountCategoryItem, AccountCategoryMaintenanceDto>().TwoWays();
            config.NewConfig<Csg7GroupItem, AccountGroupDto>().TwoWays();
            config.NewConfig<CapsStaffItem, StaffDto>().TwoWays();
           
        }
    }
}
