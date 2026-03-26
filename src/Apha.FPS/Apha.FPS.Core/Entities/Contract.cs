namespace Apha.FPS.Core.Entities
{
    public partial class Contract
    {
        public string ContractNo { get; set; } = null!;

        public string Category { get; set; } = null!;

        public string? Manager { get; set; }

        public string? Customer { get; set; }

        public string? Title { get; set; }

        public DateOnly? RegisteredDate { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public byte[]? ContractDoc { get; set; }

        public int? Duration { get; set; }

        public int? FpsYear { get; set; }
    }
}
