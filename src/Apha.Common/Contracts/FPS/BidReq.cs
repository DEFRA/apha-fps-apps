namespace Apha.Common.Contracts.FPS
{
    public class BidReq
    {
        public string WorkGroupName { get; set; } = null!;

        public string Account { get; set; } = null!;

        public decimal GenBid { get; set; }
    }
}
