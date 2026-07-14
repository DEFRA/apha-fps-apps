/*
 * TRANSFORMENGINE MIGRATION — PeriodItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New model for the period-table-dropdown custom control (Period from / Period to pickers)
 *   - Mirrors PeriodLookupDto (AccntsPeriod, MonthName, MonthNumber) for direct use in Razor views
 *   - The HTML prototype renders a custom searchable table-in-dropdown with 3 columns:
 *     AccntsPeriod, MonthName, MonthNumber (from the HTML <thead> in department_income.html)
 *   - Not a DataGrid grid row — rendered as a plain list for the custom period picker partial
 *
 * PRESERVED:
 *   - Property names match PeriodLookupDto exactly (AccntsPeriod, MonthName, MonthNumber)
 *   - AccntsPeriod: fiscal/accounts period number (1–12, maps to accounting fiscal year)
 *   - MonthName: human-readable display name for the period (e.g. "April")
 *   - MonthNumber: calendar month number corresponding to AccntsPeriod
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm fiscal-to-calendar month mapping is handled correctly in the
 *     period picker Razor partial (toCalendarMonthNumber VBA logic: ((period + 2) % 12) + 1)
 */

using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    // TRANSFORMENGINE: Model for the period-table-dropdown custom control (Period from / Period to pickers)
    // HTML prototype table headers: AccntsPeriod | MonthName | MonthNumber (department_income.html lines 66-69, 97-100)
    // Populated from IDepartmentIncomeService.GetPeriodsAsync() → PeriodLookupDto → PeriodItem
    public class PeriodItem
    {
        // TRANSFORMENGINE: HTML column "AccntsPeriod" — fiscal/accounting period number (1–12)
        // Maps to PeriodLookupDto.AccntsPeriod; used as the backing value for monthFromSelect / monthToSelect
        [Display(Name = "AccntsPeriod")]
        public int AccntsPeriod { get; set; }

        // TRANSFORMENGINE: HTML column "MonthName" — display label shown in the custom period picker
        // Maps to PeriodLookupDto.MonthName (e.g. "April", "May", ...)
        [Display(Name = "MonthName")]
        public string MonthName { get; set; } = null!;

        // TRANSFORMENGINE: HTML column "MonthNumber" — calendar month number (1=Jan … 12=Dec)
        // Maps to PeriodLookupDto.MonthNumber; JS toCalendarMonthNumber converts AccntsPeriod → MonthNumber
        [Display(Name = "MonthNumber")]
        public int MonthNumber { get; set; }
    }
}
