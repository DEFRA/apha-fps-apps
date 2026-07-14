/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeTimeItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - Upgraded from Phase 10 stub — added [GridColumn] and [Display] attributes to all 18 properties
 *   - Column widths derived from JS getQueryColumns() in fps_department_income.js
 *   - All columns are ReadOnly / GbpValue / DecimalNumber — this is a read-only report grid (showAddButton: false)
 *   - No AllowEdit/AllowDelete — the modal grid is read-only (no action column in getQueryColumns)
 *   - GbpValue used for ChargeRate, Pay, NonPay, Overhead, TotalCost (seed data shows £ values)
 *   - DecimalNumber used for Time (floating-point hours)
 *   - Number used for Month (integer period number)
 *   - Property names match DepartmentIncomeTimeDto exactly for AutoMapper convention mapping
 *
 * PRESERVED:
 *   - All 18 property names from DepartmentIncomeTimeDto / DepartmentIncomeTimeRes (Phase 7)
 *   - Nullable semantics matching DTO (string? for optional string fields)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    // TRANSFORMENGINE: Read-only grid row model for GET /api/v1/department-income/time
    // JS getQueryColumns(): 18 columns, all read-only (no edit/delete/add buttons in modal grid)
    public class DepartmentIncomeTimeItem
    {
        // TRANSFORMENGINE: JS field: 'project', header: 'Project', width: 110
        [Display(Name = "Project")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string Project { get; set; } = null!;

        // TRANSFORMENGINE: JS field: 'oracleProject', header: 'OracleProject', width: 130
        // DTO field: OracleProjectCode (AP prefix + project code)
        [Display(Name = "OracleProject")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? OracleProjectCode { get; set; }

        // TRANSFORMENGINE: JS field: 'subAccount', header: 'SubAccount', width: 120
        // DTO field: SubAccountCode
        [Display(Name = "SubAccount")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? SubAccountCode { get; set; }

        // TRANSFORMENGINE: JS field: 'month', header: 'Month', width: 80 — fiscal period number (integer)
        [Display(Name = "Month")]
        [GridColumn(Width = 80, Type = GridColumnType.Number, IsFilterable = true)]
        public int Month { get; set; }

        // TRANSFORMENGINE: JS field: 'defraProject', header: 'DefraProject', width: 120
        [Display(Name = "DefraProject")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? DefraProject { get; set; }

        // TRANSFORMENGINE: JS field: 'occ', header: 'OCC', width: 100 — Owning Cost Centre
        [Display(Name = "OCC")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? OCC { get; set; }

        // TRANSFORMENGINE: JS field: 'opc', header: 'OPC', width: 80 — Owning Profit Centre
        [Display(Name = "OPC")]
        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? OPC { get; set; }

        // TRANSFORMENGINE: JS field: 'spc', header: 'SPC', width: 80 — Staff Profit Centre
        [Display(Name = "SPC")]
        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? SPC { get; set; }

        // TRANSFORMENGINE: JS field: 'scc', header: 'SCC', width: 100 — Staff Cost Centre
        [Display(Name = "SCC")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? SCC { get; set; }

        // TRANSFORMENGINE: JS field: 'name', header: 'Name', width: 170 — staff member name
        [Display(Name = "Name")]
        [GridColumn(Width = 170, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Name { get; set; }

        // TRANSFORMENGINE: JS field: 'gradeCode', header: 'GradeCode', width: 100
        [Display(Name = "GradeCode")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? GradeCode { get; set; }

        // TRANSFORMENGINE: JS field: 'spNumber', header: 'SPNumber', width: 100
        [Display(Name = "SPNumber")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? SpNumber { get; set; }

        // TRANSFORMENGINE: JS field: 'chargeRate', header: 'ChargeRate', width: 110 — seed data: £84.73 → GbpValue
        [Display(Name = "ChargeRate")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal ChargeRate { get; set; }

        // TRANSFORMENGINE: JS field: 'pay', header: 'Pay', width: 100 — seed data: £1,040.99 → GbpValue
        [Display(Name = "Pay")]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal Pay { get; set; }

        // TRANSFORMENGINE: JS field: 'nonPay', header: 'NonPay', width: 100 — seed data: £357.06 → GbpValue
        [Display(Name = "NonPay")]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal NonPay { get; set; }

        // TRANSFORMENGINE: JS field: 'overhead', header: 'Overhead', width: 100 — seed data: £506.55 → GbpValue
        [Display(Name = "Overhead")]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal Overhead { get; set; }

        // TRANSFORMENGINE: JS field: 'time', header: 'Time', width: 90 — hours (decimal, not currency)
        [Display(Name = "Time")]
        [GridColumn(Width = 90, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal Time { get; set; }

        // TRANSFORMENGINE: JS field: 'totalCost', header: 'TotalCost', width: 110 — seed data: £1,398.05 → GbpValue
        [Display(Name = "TotalCost")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal TotalCost { get; set; }
    }
}
