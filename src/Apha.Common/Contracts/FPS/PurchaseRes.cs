namespace Apha.Common.Contracts.FPS
{
    public class PurchaseRes
    {
        public string WorkGroupName { get; set; } = null!;

        public string Account { get; set; } = null!;

        public string ItemDescription { get; set; } = null!;

        public decimal Amount { get; set; }

        public int FpsYear { get; set; }
    }
}
