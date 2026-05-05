namespace Apha.Common.Contracts.FPS
{
    public class MonthlyOutputReq
    {
        public string  Buyer     { get; set; } = null!;
        public string  TestCode  { get; set; } = null!;
        public double  Month     { get; set; }
        public string  WorkGroup { get; set; } = null!;
    }
}
