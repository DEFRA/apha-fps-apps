namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class PurchaseDto
    {
        public string WorkgroupName { get; set; } = null!;

        public string Account { get; set; } = null!;

        public string ItemDescription { get; set; } = null!;

        public decimal Amount { get; set; }

        public int FpsYear { get; set; }

        public string? OldItemDescription { get; set; }
    }
}
