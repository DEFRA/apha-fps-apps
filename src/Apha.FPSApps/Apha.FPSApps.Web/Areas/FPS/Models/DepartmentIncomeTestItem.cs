/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeTestItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - Upgraded from Phase 10 stub — added [GridColumn] and [Display] attributes to all 14 properties
 *   - Column widths derived from JS getQueryColumns() in fps_department_income.js (shared 18-col modal)
 *     matched to DepartmentIncomeTestDto property names
 *   - All columns are ReadOnly / GbpValue / DecimalNumber — read-only report grid (showAddButton: false)
 *   - GbpValue used for TestPrice, TotalCost (cost/price values)
 *   - DecimalNumber used for Volume (fractional test count)
 *   - Number used for Month (integer fiscal period number)
 *   - Property names match DepartmentIncomeTestDto exactly for AutoMapper convention mapping
 *   - Note: OPC appears before OCC in this query (qryDeptIncomeTests column order) — preserved from DTO
 *
 * PRESERVED:
 *   - All 14 property names from DepartmentIncomeTestDto / DepartmentIncomeTestRes (Phase 7)
 *   - Column order: Project, OracleProjectCode, SubAccountCode, DefraProject, OPC, OCC, Month,
 *     SPC, WorkGroup, SCC, TestCode, Volume, TestPrice, TotalCost
 *   - Nullable semantics matching DTO
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    // TRANSFORMENGINE: Read-only grid row model for GET /api/v1/department-income/tests
    // JS getQueryColumns() modal shared grid — 14 of 18 columns present for this query type
    public class DepartmentIncomeTestItem
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

        // TRANSFORMENGINE: JS field: 'opc', header: 'OPC', width: 80 — listed before OCC in tests query
        [Display(Name = "OPC")]
        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? OPC { get; set; }

        // TRANSFORMENGINE: JS field: 'occ', header: 'OCC', width: 100
        [Display(Name = "OCC")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? OCC { get; set; }

        // TRANSFORMENGINE: JS field: 'month', header: 'Month', width: 80
        [Display(Name = "Month")]
        [GridColumn(Width = 80, Type = GridColumnType.Number, IsFilterable = true)]
        public int Month { get; set; }

        // TRANSFORMENGINE: JS field: 'spc', header: 'SPC', width: 80 — Staff Profit Centre
        [Display(Name = "SPC")]
        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? SPC { get; set; }

        // TRANSFORMENGINE: WorkGroup — maps to 'name' column proxy (width: 170) for tests query
        [Display(Name = "WorkGroup")]
        [GridColumn(Width = 170, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? WorkGroup { get; set; }

        // TRANSFORMENGINE: JS field: 'scc', header: 'SCC', width: 100 — Staff Cost Centre
        [Display(Name = "SCC")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? SCC { get; set; }

        // TRANSFORMENGINE: TestCode — maps to 'gradeCode' column proxy (width: 100) for tests query
        [Display(Name = "TestCode")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? TestCode { get; set; }

        // TRANSFORMENGINE: Volume — fractional test count; maps to 'time' column proxy (width: 90)
        [Display(Name = "Volume")]
        [GridColumn(Width = 90, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal Volume { get; set; }

        // TRANSFORMENGINE: TestPrice — unit price per test; maps to 'chargeRate' column proxy (width: 110)
        [Display(Name = "TestPrice")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal TestPrice { get; set; }

        // TRANSFORMENGINE: TotalCost = TestPrice * Volume; maps to 'totalCost' column (width: 110)
        [Display(Name = "TotalCost")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal TotalCost { get; set; }
    }
}
