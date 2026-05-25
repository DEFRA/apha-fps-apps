namespace Apha.FPS.Core.Entities
{
    public class ProfitCentreGradeView
    {
        public string? PcGrade { get; set; }
        public string? DivisionGrade { get; set; }
        public string? GradeCode { get; set; }
        public string? ProfitCentre { get; set; }
        public decimal? ChargeRate { get; set; }
        public decimal? DirectRate { get; set; }
        public decimal? PayRate { get; set; }
        public decimal? Npr { get; set; }
        public decimal? Ohr { get; set; }
        public double? HrsAvailable { get; set; }
        public decimal? OldChargeRate { get; set; }
        public decimal? DefraChargeRate { get; set; }
        public int? FpsYear { get; set; }
        public int? UserId { get; set; }
        public string? Dt2Username { get; set; }
        public string? UserEmail { get; set; }
    }
}