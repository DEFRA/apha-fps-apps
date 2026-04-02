namespace Apha.FPSApps.Web.Handler
{
    public interface IFpsYearContext
    {
        int Year { get; set; }
        bool IsReadOnly { get; }
    }


    public class FpsYearContext : IFpsYearContext
    {
        public int Year { get; set; }
        public bool IsReadOnly => Year < DateTime.UtcNow.Year;
    }
}
