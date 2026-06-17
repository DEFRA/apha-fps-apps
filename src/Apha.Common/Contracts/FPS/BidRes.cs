namespace Apha.Common.Contracts.FPS
{
    public class BidRes
    {
        public string WorkGroupName { get; set; } = null!;

        public string Account { get; set; } = null!;

        public decimal GenBid { get; set; }

        public int FpsYear { get; set; }
    }
}
