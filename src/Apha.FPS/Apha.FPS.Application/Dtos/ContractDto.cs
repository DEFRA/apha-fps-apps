namespace Apha.FPS.Application.Dtos
{
    public class ContractDto
    {
        public string Contractno { get; set; } = null!;

        public string Category { get; set; } = null!;

        public string? Manager { get; set; }

        public string? Customer { get; set; }

        public string? Title { get; set; }

        public DateTime? Registereddate { get; set; }

        public DateTime? Startdate { get; set; }

        public DateTime? Enddate { get; set; }

        public byte[]? Contractdoc { get; set; }

        public int? Duration { get; set; }

        public int? Fpscalyear { get; set; }
    }
}
