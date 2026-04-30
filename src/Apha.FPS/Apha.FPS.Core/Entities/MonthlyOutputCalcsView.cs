namespace Apha.FPS.Core.Entities
{
    public class MonthlyOutputCalcsView
    {
        public string? Buyer     { get; set; }
        public string? TestCode  { get; set; }
        public double? Month     { get; set; }
        public double? Volume    { get; set; }
        public double? TestPrice { get; set; }
        public double? Charge    { get; set; }
        public string? WorkGroup { get; set; }
        public int     FpsYear   { get; set; }
    }
}
