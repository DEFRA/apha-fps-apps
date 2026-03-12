namespace Apha.FPS.Core.Entities
{
    public partial class Contract
    {
        public string Contractno { get; set; } = null!;

        public string Category { get; set; } = null!;

        public string? Manager { get; set; }

        public string? Customer { get; set; }

        public string? Title { get; set; }

        public DateOnly? Registereddate { get; set; }

        public DateOnly? Startdate { get; set; }

        public DateOnly? Enddate { get; set; }

        public byte[]? Contractdoc { get; set; }

        public int? Duration { get; set; }

        public int? Fpscalyear { get; set; }
    }
}
