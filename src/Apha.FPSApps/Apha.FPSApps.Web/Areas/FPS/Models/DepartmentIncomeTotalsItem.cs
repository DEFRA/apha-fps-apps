/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeTotalsItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - Upgraded from Phase 10 stub — added [GridColumn] and [Display] attributes to all 7 properties
 *   - Column widths derived from JS getQueryColumns() in fps_department_income.js (shared 18-col modal)
 *     matched to DepartmentIncomeTotalsDto property names
 *   - All columns are ReadOnly / GbpValue — read-only report grid (showAddButton: false)
 *   - GbpValue used for all cost columns (TotalCosts, TimeCost, TestsCost, AnimalsCost, ProjectSpecificsCost)
 *   - Nullable decimal? preserved for pivot columns (may be null when no data for that cost area)
 *   - Property names match DepartmentIncomeTotalsDto exactly for AutoMapper convention mapping
 *   - qryDeptIncomeTotals is a PIVOT query — one row per project with area subtotals as columns
 *
 * PRESERVED:
 *   - All 7 property names from DepartmentIncomeTotalsDto / DepartmentIncomeTotalsRes (Phase 7)
 *   - Nullable decimal? for TimeCost, TestsCost, AnimalsCost, ProjectSpecificsCost
 *   - TotalCosts non-nullable (grand total always present for the project)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    // TRANSFORMENGINE: Read-only grid row model for GET /api/v1/department-income/totals
    // qryDeptIncomeTotals — PIVOT query, one row per project with area cost subtotals
    public class DepartmentIncomeTotalsItem
    {
        // TRANSFORMENGINE: JS field: 'project', header: 'Project', width: 110 — group-by key
        [Display(Name = "Project")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string Project { get; set; } = null!;

        // TRANSFORMENGINE: JS field: 'oracleProject', header: 'OracleProject', width: 130 — group-by key
        [Display(Name = "OracleProject")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? OracleProjectCode { get; set; }

        // TRANSFORMENGINE: Sum(TotalCost) AS TotalCosts — grand total across all areas
        // Maps to JS 'totalCost' column (width: 110)
        [Display(Name = "TotalCosts")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal TotalCosts { get; set; }

        // TRANSFORMENGINE: PIVOT "Time" column — Sum TotalCost where Area = "Time"; nullable when no time costs
        // Maps to JS 'pay' column proxy (width: 100) for totals display
        [Display(Name = "TimeCost")]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? TimeCost { get; set; }

        // TRANSFORMENGINE: PIVOT "Tests" column — Sum TotalCost where Area = "Tests"; nullable when no test costs
        // Maps to JS 'nonPay' column proxy (width: 100) for totals display
        [Display(Name = "TestsCost")]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? TestsCost { get; set; }

        // TRANSFORMENGINE: PIVOT "Animals" column — Sum TotalCost where Area = "Animals"; nullable when absent
        // Maps to JS 'overhead' column proxy (width: 100) for totals display
        [Display(Name = "AnimalsCost")]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? AnimalsCost { get; set; }

        // TRANSFORMENGINE: PIVOT "Project-specifics" column — Sum TotalCost where Area = "Project-specifics"; nullable
        // Maps to JS 'chargeRate' column proxy (width: 110) for totals display
        [Display(Name = "ProjectSpecificsCost")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? ProjectSpecificsCost { get; set; }
    }
}
