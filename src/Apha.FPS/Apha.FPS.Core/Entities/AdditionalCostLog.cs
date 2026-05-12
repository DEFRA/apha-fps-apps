namespace Apha.FPS.Core.Entities
{
    public class AdditionalCostLog
    {
        public int SequenceNo { get; set; }
        public string JobCode { get; set; } = null!;
        public string Account { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal ItemCost { get; set; }
        public string? Freq { get; set; }
        public string? Supplier { get; set; }
        public DateTime? DateTime { get; set; }
        public string? UserId { get; set; }
        public string? InsertDelete { get; set; }
        public int? FpsYear { get; set; }
    }
}
