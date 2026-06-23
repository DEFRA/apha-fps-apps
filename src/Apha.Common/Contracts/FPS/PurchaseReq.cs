namespace Apha.Common.Contracts.FPS
{
    public class PurchaseReq
    {
        public string WorkGroupName { get; set; } = null!;

        public string Account { get; set; } = null!;

        public string ItemDescription { get; set; } = null!;

        public decimal Amount { get; set; }

        public string? OldItemDescription { get; set; }
    }
}
