namespace Apha.FPS.Core.Entities
{
    public partial class DivisionGrade
    {
        public string DivisionGradeCode { get; set; } = null!;
        public int FpsYear { get; set; }
        public decimal? ChargeRate { get; set; }
        public decimal? DirectRate { get; set; }
        public string? Division { get; set; }
        public string? GradeCode { get; set; }
        public decimal? Npr { get; set; }
        public decimal? Ohr { get; set; }
        public decimal? PayRate { get; set; }
    }
}
