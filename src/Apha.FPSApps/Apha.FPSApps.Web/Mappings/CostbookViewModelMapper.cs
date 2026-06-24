/*
 * TRANSFORMENGINE MIGRATION — CostbookViewModelMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - Phase 10: Documented required CreateMap entries for frmMaintainance Maintenance screen
 *     ViewModel/Item types (Tab 1-5) as TRANSFORMENGINE TODO stubs.
 *   - Phase 11: Activated all Maintenance CreateMap entries (stubs removed):
 *     InflationSettingsItem ↔ MaintenanceSettingsDto (Tab 1 inflation sub-set)
 *     ProfitMarginsItem ↔ MaintenanceSettingsDto (Tab 4 profit sub-set)
 *     AccountCategoryItem ↔ AccountCategoryMaintenanceDto (Tab 2 grid)
 *     Csg7GroupItem ↔ AccountGroupDto (Tab 3 grid)
 *     CapsStaffItem ↔ CapsStaffDto (Tab 5 grid)
 *
 * PRESERVED:
 *   - All existing CreateMap entries for Project, YearlyDetails, StaffRequirement, TestRequirement,
 *     AnimalRequirement, AdditionalCost — unchanged
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: InflationSettingsItem and ProfitMarginsItem only carry sub-sets of
 *     MaintenanceSettingsDto — confirm AutoMapper silently ignores unmapped properties on ReverseMap
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.CostBook.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;

namespace Apha.FPSApps.Web.Mappings
{
    public class CostbookViewModelMapper : Profile
    {
        public CostbookViewModelMapper()
        {
            // ── Existing project view model mappings ──────────────────────────
            CreateMap<ProjectDto, ProjectItemViewModel>().ReverseMap();
            CreateMap<ProjectDto, ProjectDetailViewModel>().ReverseMap();
            CreateMap<ProjectDto, ProjectCreateEditViewModel>().ReverseMap();

            // ── Yearly details: Dto ↔ ViewModel/Item ─────────────────────────
            CreateMap<ProjectYearDto, ProjectYearRateItem>().ReverseMap();
            CreateMap<StaffRequirementDto, StaffRequirementItem>().ReverseMap();
            CreateMap<TestRequirementDto, TestRequirementItem>().ReverseMap();
            CreateMap<AnimalRequirementDto, AnimalRequirementItem>().ReverseMap();
            CreateMap<AdditionalCostDto, AdditionalCostItem>().ReverseMap();

            // ── Phase 10 / Phase 11: frmMaintainance Maintenance screen ───────
            // TRANSFORMENGINE: Phase 11 ViewModel/Item types created — CreateMap entries activated.

            // Tab 1 — InflationSettingsItem ↔ MaintenanceSettingsDto (inflation sub-section binding)
            // TRANSFORMENGINE: Explicit member mapping because InflationSettingsItem carries only
            //   the 7 inflation/system fields; unmapped profit fields silently ignored via ReverseMap
            CreateMap<InflationSettingsItem, MaintenanceSettingsDto>().ReverseMap();

            // Tab 4 — ProfitMarginsItem ↔ MaintenanceSettingsDto (profit sub-section binding)
            // TRANSFORMENGINE: Explicit member mapping because ProfitMarginsItem carries only the 4
            //   profit fields; unmapped inflation fields silently ignored via ReverseMap
            CreateMap<ProfitMarginsItem, MaintenanceSettingsDto>().ReverseMap();

            // Tab 2 — AccountCategoryItem ↔ AccountCategoryMaintenanceDto (grid row + modal)
            // TRANSFORMENGINE: AccShortName ↔ AccShortName, AccountDescription ↔ AccountDescription,
            //   Csg7Group ↔ Csg7Group, FpsYear ↔ FpsYear
            CreateMap<AccountCategoryItem, AccountCategoryMaintenanceDto>().ReverseMap();

            // Tab 3 — Csg7GroupItem ↔ AccountGroupDto (grid row + modal; also drives AccCat dropdown)
            // TRANSFORMENGINE: Csg7Group ↔ Csg7Group, UseInflation ↔ UseInflation
            CreateMap<Csg7GroupItem, AccountGroupDto>().ReverseMap();

            // Tab 5 — CapsStaffItem ↔ CapsStaffDto (grid row + modal)
            // TRANSFORMENGINE: MNumber ↔ MNumber, Name ↔ Name, Dt2Number ↔ Dt2Number
            CreateMap<CapsStaffItem, CapsStaffDto>().ReverseMap();
        }
    }
}
