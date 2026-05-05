namespace Apha.Common.Contracts.FPS
{
    public class AdditionalCostRes
    {
        public string JobCode { get; set; } = null!;

        public string Account { get; set; } = null!;

        public string Description { get; set; } = null!;

        public decimal ItemCost { get; set; }

        public string? Freq { get; set; }

        public string? Supplier { get; set; }

        public int? FpsYear { get; set; }
    }
}
