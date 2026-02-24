namespace Apha.FPSApps.Web.Handler
{
    public interface IFPSYearContext
    {
        int Year { get; set; }
        bool IsReadOnly { get; }
    }


    public class FPSYearContext : IFPSYearContext
    {
        public int Year { get; set; }
        public bool IsReadOnly => Year < DateTime.UtcNow.Year;
    }
}
