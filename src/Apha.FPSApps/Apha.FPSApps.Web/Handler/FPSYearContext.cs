namespace Apha.FPSApps.Web.Handler
{
    public interface IFpsYearContext
    {
        int Year { get; set; }
        bool IsReadOnly { get; set; }
    }


    public class FpsYearContext : IFpsYearContext
    {
        public int Year { get; set; }
        public bool IsReadOnly { get; set; }
    }
}
