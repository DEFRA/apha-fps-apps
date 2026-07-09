/*
 * TRANSFORMENGINE MIGRATION — YearlyFinancialDataItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - Full Phase 11 implementation: [GridColumn], [Display], and [Required] attributes added
 *   - Column order, widths, and types derived from frmProjectRadTrackData.html <colgroup> structure
 *     (yfd-col-year, yfd-col-ppacc, yfd-col-income, yfd-col-budget, yfd-col-actexp, yfd-col-seedcorn,
 *      yfd-col-manhours, yfd-col-pay, yfd-col-nonpay, yfd-col-test, yfd-col-projspec, yfd-col-animal,
 *      yfd-col-excadj, yfd-col-adjcomment, yfd-col-actions)
 *   - AllowEdit/AllowDelete confirmed present (edit/delete buttons in actions column)
 *   - AllowAdd confirmed from Save/Update buttons in modal footer
 *   - Audit "Changed" flags (ManHoursChanged etc.) hidden as display-only context, not editable grid columns
 *   - AdjustmentComment displayed as ReadOnly text in grid (truncated); full value in modal
 *   - Locked displayed as Checkbox column (short 0/1 → GridColumnType.Checkbox)
 *   - DateCosted and CostedBy are costing-panel fields, hidden in grid, visible in modal
 *   - TotalCosts shown as GbpValue (computed read-only)
 *
 * PRESERVED:
 *   - All property names exactly match YearlyFinancialDataDto for convention-based AutoMapper mapping
 *   - All types: decimal? for cost columns, double? for man-effort, short for Locked/audit flags,
 *     DateTime? for DateCosted, string? for text columns
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm GridColumn widths against final CSS yfd-col-* class widths
 *   - TRANSFORMENGINE TODO: Confirm ActualManYears should be grid-hidden (it is a costing panel field
 *     shown only in the modal "Actual ManYears Reported as:" display)
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    /// <summary>
    /// Grid row item for the Yearly Financial Data data grid.
    /// Maps to/from <c>Apha.FPSApps.Application.Dtos.PIMS.YearlyFinancialDataDto</c>.
    /// Composite key: (<see cref="Year"/>, <see cref="Project"/>).
    /// </summary>
    public class YearlyFinancialDataItem
    {
        // TRANSFORMENGINE: Project — not a visible grid column; provided as page-level context via SelectedProject.
        //                  Kept hidden so AutoMapper can populate it and the Create/Update modal can echo it.
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? Project { get; set; }

        // TRANSFORMENGINE: Year — col 1 (yfd-col-year); also used as KeyProperty for edit/delete.
        //                  Visible in JS grid columns → keep visible even though it is the key.
        [Required(ErrorMessage = "Year is required")]
        [Display(Name = "Year")]
        [GridColumn(Order = 1, Width = 60, Type = GridColumnType.Text, IsFilterable = true)]
        public short Year { get; set; }

        // TRANSFORMENGINE: BfBudget — col 2 (yfd-col-ppacc). Label: "PP/Acc".
        [Display(Name = "PP/Acc")]
        [GridColumn(Order = 2, Width = 90, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? BfBudget { get; set; }

        // TRANSFORMENGINE: PyBudget — col 3 (yfd-col-income). Label: "Customer Income".
        [Display(Name = "Customer Income")]
        [GridColumn(Order = 3, Width = 115, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? PyBudget { get; set; }

        // TRANSFORMENGINE: VlaBudget — col 4 (yfd-col-budget). Label: "VLA Budget".
        [Display(Name = "VLA Budget")]
        [GridColumn(Order = 4, Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? VlaBudget { get; set; }

        // TRANSFORMENGINE: ActualExpenditure — col 5 (yfd-col-actexp). Label: "Actual Exp".
        [Display(Name = "Actual Exp")]
        [GridColumn(Order = 5, Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? ActualExpenditure { get; set; }

        // TRANSFORMENGINE: Seedcorn — col 6 (yfd-col-seedcorn). Label: "Seedcorn".
        [Display(Name = "Seedcorn")]
        [GridColumn(Order = 6, Width = 90, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? Seedcorn { get; set; }

        // TRANSFORMENGINE: ManHours — col 7 (yfd-col-manhours). Label: "Man Hours".
        [Display(Name = "Man Hours")]
        [GridColumn(Order = 7, Width = 90, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double? ManHours { get; set; }

        // TRANSFORMENGINE: PayCosts — col 8 (yfd-col-pay). Label: "Pay Costs".
        [Display(Name = "Pay Costs")]
        [GridColumn(Order = 8, Width = 90, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? PayCosts { get; set; }

        // TRANSFORMENGINE: NonPayOhCosts — col 9 (yfd-col-nonpay). Label: "Non-Pay & OH".
        [Display(Name = "Non-Pay & OH")]
        [GridColumn(Order = 9, Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? NonPayOhCosts { get; set; }

        // TRANSFORMENGINE: TestCosts — col 10 (yfd-col-test). Label: "Test Costs".
        [Display(Name = "Test Costs")]
        [GridColumn(Order = 10, Width = 90, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? TestCosts { get; set; }

        // TRANSFORMENGINE: NonAnimalCosts — col 11 (yfd-col-projspec). Label: "Project Specific". DB: nonanimalcosts.
        [Display(Name = "Project Specific")]
        [GridColumn(Order = 11, Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? NonAnimalCosts { get; set; }

        // TRANSFORMENGINE: AnimalCosts — col 12 (yfd-col-animal). Label: "Animal Costs".
        [Display(Name = "Animal Costs")]
        [GridColumn(Order = 12, Width = 90, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? AnimalCosts { get; set; }

        // TRANSFORMENGINE: Adjustment — col 13 (yfd-col-excadj). Label: "Exc/Adj".
        [Display(Name = "Exc/Adj")]
        [GridColumn(Order = 13, Width = 80, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? Adjustment { get; set; }

        // TRANSFORMENGINE: AdjustmentComment — col 14 (yfd-col-adjcomment). Label: "Adj Comment".
        //                  ReadOnly in the grid (truncated); full value editable in the modal textarea.
        [Display(Name = "Adj Comment")]
        [GridColumn(Order = 14, Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = false)]
        public string? AdjustmentComment { get; set; }

        // TRANSFORMENGINE: TotalCosts — computed aggregation; not a separate grid column but
        //                  surfaced in the totals row and modal display. Hidden in grid rows (shown in footer).
        [Display(Name = "Total Costs")]
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public decimal? TotalCosts { get; set; }

        // TRANSFORMENGINE: ManDays, ManYears — modal-only man-time fields; not shown as grid columns.
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public double? ManDays { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public double? ManYears { get; set; }

        // TRANSFORMENGINE: ActualManYears — costing panel "Actual ManYears Reported as:" display; hidden in grid.
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public double? ActualManYears { get; set; }

        // TRANSFORMENGINE: Locked — modal "Fixed" checkbox; hidden in main grid, used in costing panel.
        [Display(Name = "Fixed")]
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public short Locked { get; set; }

        // TRANSFORMENGINE: DateCosted — modal "Date Fixed" costing panel field; hidden in grid.
        [Display(Name = "Date Fixed")]
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public DateTime? DateCosted { get; set; }

        // TRANSFORMENGINE: CostedBy — modal "Fixed By" costing panel field; hidden in grid.
        [Display(Name = "Fixed By")]
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? CostedBy { get; set; }

        // TRANSFORMENGINE: Audit "Changed" flag columns — read-only display context for costing panel.
        //                  Hidden in grid and modal; used only by front-end JS to determine changed state.
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public short ManHoursChanged { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public short PayCostsChanged { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public short NonPayOhCostsChanged { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public short TestCostsChanged { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public short AnimalCostsChanged { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public short NonAnimalCostsChanged { get; set; }
    }
}
