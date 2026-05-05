namespace Apha.Common.Contracts.FPS
{
    public class TimeCostCalcsReq
    {
        public string WorkGroup { get; set; } = null!;
        public string JobCode { get; set; } = null!;
        public string Project { get; set; } = null!;
        public double Month { get; set; }
        public string StaffId { get; set; } = null!;
    }
}
