namespace Apha.FPS.Core.Entities
{
    public partial class Purchase
    {
        public string WorkgroupName { get; set; } = null!;

        public string Account { get; set; } = null!;

        public string ItemDescription { get; set; } = null!;

        public decimal Amount { get; set; }

        public int FpsYear { get; set; }
    }
}
