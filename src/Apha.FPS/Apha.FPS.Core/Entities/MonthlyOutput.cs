namespace Apha.FPS.Core.Entities
{
    public class MonthlyOutput
    {
        public string  TestCode  { get; set; } = null!;
        public string  Buyer     { get; set; } = null!;
        public double  Month     { get; set; }
        public string  WorkGroup { get; set; } = null!;
        public int     FpsYear   { get; set; }
        public double? Volume    { get; set; }
        public string? WgBuyer   { get; set; }
    }
}
