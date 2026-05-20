namespace Apha.FPS.Application.Dtos
{
    public class DivisionGradeMaintenanceDto
    {
        public string DivisionGradeCode { get; set; } = null!;
        public int FpsYear { get; set; }
        public decimal? ChargeRate { get; set; }
        public decimal? DirectRate { get; set; }
        public string Division { get; set; } = null!;
        public string GradeCode { get; set; } = null!;
        public decimal? Npr { get; set; }
        public decimal? Ohr { get; set; }
        public decimal? PayRate { get; set; }
    }
}
