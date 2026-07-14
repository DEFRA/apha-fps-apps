/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeAdditionalItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - Upgraded from Phase 10 stub — added [GridColumn] and [Display] attributes to all 8 properties
 *   - Column widths derived from JS getQueryColumns() in fps_department_income.js (shared 18-col modal)
 *     matched to DepartmentIncomeAdditionalDto property names
 *   - All columns are ReadOnly / GbpValue — read-only report grid (showAddButton: false)
 *   - GbpValue used for TotalCost (Sum of exceptional/additional costs)
 *   - Number used for Month (integer fiscal period number)
 *   - Property names match DepartmentIncomeAdditionalDto exactly for AutoMapper convention mapping
 *   - Note: qryDeptIncomeAdditional (qryDeptIncomeExceptional in VBA) has 8 columns — fewer than other queries
 *
 * PRESERVED:
 *   - All 8 property names from DepartmentIncomeAdditionalDto / DepartmentIncomeAdditionalRes (Phase 7)
 *   - Nullable semantics matching DTO
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    // TRANSFORMENGINE: Read-only grid row model for GET /api/v1/department-income/additional
    // qryDeptIncomeAdditional (VBA: qryDeptIncomeExceptional) — 8 columns (aggregated exceptional costs)
    public class DepartmentIncomeAdditionalItem
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

        // TRANSFORMENGINE: JS field: 'opc', header: 'OPC', width: 80 — Owning Profit Centre
        [Display(Name = "OPC")]
        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? OPC { get; set; }

        // TRANSFORMENGINE: JS field: 'occ', header: 'OCC', width: 100 — Owning Cost Centre
        [Display(Name = "OCC")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? OCC { get; set; }

        // TRANSFORMENGINE: JS field: 'month', header: 'Month', width: 80 — fiscal period number
        [Display(Name = "Month")]
        [GridColumn(Width = 80, Type = GridColumnType.Number, IsFilterable = true)]
        public int Month { get; set; }

        // TRANSFORMENGINE: TotalCost = Sum(Proj_SubContract.Amount) — aggregated exceptional costs
        // Maps to JS 'totalCost' column (width: 110)
        [Display(Name = "TotalCost")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal TotalCost { get; set; }
    }
}
