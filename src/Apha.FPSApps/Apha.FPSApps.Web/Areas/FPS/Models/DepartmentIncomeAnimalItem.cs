/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeAnimalItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - Upgraded from Phase 10 stub — added [GridColumn] and [Display] attributes to all 13 properties
 *   - Column widths derived from JS getQueryColumns() in fps_department_income.js (shared 18-col modal)
 *     matched to DepartmentIncomeAnimalDto property names
 *   - All columns are ReadOnly / GbpValue / DecimalNumber — read-only report grid (showAddButton: false)
 *   - GbpValue used for Rate, TotalCost (monetary values)
 *   - DecimalNumber used for AnimalDays (fractional animal-day count)
 *   - Number used for Month (integer fiscal period number)
 *   - Property names match DepartmentIncomeAnimalDto exactly for AutoMapper convention mapping
 *   - Note: OPC appears before OCC in this query (preserved from DTO / Access qryDeptIncomeAnimal)
 *
 * PRESERVED:
 *   - All 13 property names from DepartmentIncomeAnimalDto / DepartmentIncomeAnimalRes (Phase 7)
 *   - Nullable semantics matching DTO (SPC/SCC are literal constant strings from Access query)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    // TRANSFORMENGINE: Read-only grid row model for GET /api/v1/department-income/animals
    // JS getQueryColumns() modal shared grid — 13 of 18 columns present for this query type
    public class DepartmentIncomeAnimalItem
    {
        // TRANSFORMENGINE: JS field: 'project', header: 'Project', width: 110
        [Display(Name = "Project")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string Project { get; set; } = null!;

        // TRANSFORMENGINE: JS field: 'oracleProject', header: 'OracleProject', width: 130
        [Display(Name = "OracleProject")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? OracleProjectCode { get; set; }

        // TRANSFORMENGINE: JS field: 'subAccount', header: 'SubAccount', width: 120
        [Display(Name = "SubAccount")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? SubAccountCode { get; set; }

        // TRANSFORMENGINE: JS field: 'defraProject', header: 'DefraProject', width: 120
        [Display(Name = "DefraProject")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? DefraProject { get; set; }

        // TRANSFORMENGINE: JS field: 'opc', header: 'OPC', width: 80 — Owning Profit Centre (before OCC for animals query)
        [Display(Name = "OPC")]
        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? OPC { get; set; }

        // TRANSFORMENGINE: JS field: 'occ', header: 'OCC', width: 100 — Owning Cost Centre
        [Display(Name = "OCC")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? OCC { get; set; }

        // TRANSFORMENGINE: JS field: 'month', header: 'Month', width: 80
        [Display(Name = "Month")]
        [GridColumn(Width = 80, Type = GridColumnType.Number, IsFilterable = true)]
        public int Month { get; set; }

        // TRANSFORMENGINE: SPC literal constant "SSSD" from Access query — maps to 'spc' column proxy (width: 80)
        [Display(Name = "SPC")]
        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? SPC { get; set; }

        // TRANSFORMENGINE: SCC literal constant "35227" from Access query — maps to 'scc' column proxy (width: 100)
        [Display(Name = "SCC")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? SCC { get; set; }

        // TRANSFORMENGINE: AnimalType from fnAnimalDesc VBA function — maps to 'name' column proxy (width: 170)
        [Display(Name = "AnimalType")]
        [GridColumn(Width = 170, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? AnimalType { get; set; }

        // TRANSFORMENGINE: AnimalDays from fnAnimalDays VBA function — maps to 'time' column proxy (width: 90)
        [Display(Name = "AnimalDays")]
        [GridColumn(Width = 90, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal AnimalDays { get; set; }

        // TRANSFORMENGINE: Rate from DLookUp tblAnimals — maps to 'chargeRate' column proxy (width: 110)
        [Display(Name = "Rate")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal Rate { get; set; }

        // TRANSFORMENGINE: TotalCost = Proj_SubContract.Amount — maps to 'totalCost' column (width: 110)
        [Display(Name = "TotalCost")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal TotalCost { get; set; }
    }
}
