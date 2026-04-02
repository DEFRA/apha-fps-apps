namespace Apha.FPSApps.Web.Handler
{
    public interface IFPSYearContext
    {
        int Year { get; set; }
        bool IsReadOnly { get; set; }
    }


    public class FPSYearContext : IFPSYearContext
    {
        public int Year { get; set; }
        public bool IsReadOnly { get; set; }
    }
}
