namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class BidDto
    {
        public string WorkgroupName { get; set; } = null!;

        public string Account { get; set; } = null!;

        public decimal GenBid { get; set; }

        public int FpsYear { get; set; }

        public string? OldItemDescription { get; set; }
    }
}
