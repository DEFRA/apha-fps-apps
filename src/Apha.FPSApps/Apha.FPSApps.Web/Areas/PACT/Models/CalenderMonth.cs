namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    /// <summary>
    /// View model for a single item in the calendar month selection dropdown.
    /// </summary>
    public class CalenderMonth
    {
        public short MonthNumber { get; set; }
        public string MonthName { get; set; } = null!;
        public short? AccntsPeriod { get; set; }
    }
}