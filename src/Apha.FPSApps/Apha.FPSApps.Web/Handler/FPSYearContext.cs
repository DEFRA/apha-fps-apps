namespace Apha.FPSApps.Web.Handler
{
    public interface IFpsYearContext
    {
        int Year { get; set; }
        bool IsReadOnly { get; set; }
        // yearstatus from fps.tblyearmaster — used by BulkRates to gate uploads by year phase
        string? YearStatus { get; set; }
    }


    public class FpsYearContext : IFpsYearContext
    {
        public int Year { get; set; }
        public bool IsReadOnly { get; set; }
        public string? YearStatus { get; set; }
    }
}
