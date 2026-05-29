namespace Apha.FPS.Application.Dtos
{
    public class BidDto
    {
        public string WorkgroupName { get; set; } = null!;

        public string Account { get; set; } = null!;

        public decimal GenBid { get; set; }

        public int FpsYear { get; set; }
    }
}
