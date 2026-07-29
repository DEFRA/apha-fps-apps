using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    // HTML prototype table headers: AccntsPeriod | MonthName | MonthNumber (department_income.html lines 66-69, 97-100)
    // Populated from IDepartmentIncomeService.GetPeriodsAsync() → PeriodLookupDto → PeriodItem
    public class PeriodItem
    {
        // Maps to PeriodLookupDto.AccntsPeriod; used as the backing value for monthFromSelect / monthToSelect
        [Display(Name = "AccntsPeriod")]
        public int AccntsPeriod { get; set; }

        // Maps to PeriodLookupDto.MonthName (e.g. "April", "May", ...)
        [Display(Name = "MonthName")]
        public string MonthName { get; set; } = null!;

        // Maps to PeriodLookupDto.MonthNumber; JS toCalendarMonthNumber converts AccntsPeriod → MonthNumber
        [Display(Name = "MonthNumber")]
        public int MonthNumber { get; set; }
    }
}
