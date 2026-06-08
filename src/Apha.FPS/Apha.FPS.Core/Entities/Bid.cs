namespace Apha.FPS.Core.Entities
{
    public partial class Bid
    {
        public string WorkGroupName { get; set; } = null!;

        public string Account { get; set; } = null!;

        public decimal GenBid { get; set; }

        public int FpsYear { get; set; }
    }
}
