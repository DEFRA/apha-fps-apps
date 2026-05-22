namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class DivisionGradeDto
    {
        public string DivisionGradeCode { get; set; } = null!;
        public string? GradeCode { get; set; }
        public string? Division { get; set; }
        public decimal? ChargeRate { get; set; }
        public decimal? DirectRate { get; set; }
        public decimal? PayRate { get; set; }
        public decimal? Npr { get; set; }
        public decimal? Ohr { get; set; }
        public int FpsYear { get; set; }
    }
}
