namespace Apha.FPS.Core.Entities
{
    public class WorkGroupGradeView
    {
        public string? WgGrade { get; set; }
        public string? ProfitCentreGrade { get; set; }
        public string? GradeCode { get; set; }
        public string? WorkGroup { get; set; }
        public decimal? ChargeRateWg { get; set; }
        public decimal? DirectRateWg { get; set; }
        public decimal? PayRateWg { get; set; }
        public decimal? NprWg { get; set; }
        public decimal? OhrWg { get; set; }
        public decimal? AvSalary { get; set; }
        public string? HrsChangedBy { get; set; }
        public int? FpsYear { get; set; }
        public int? UserId { get; set; }
        public string? Dt2Username { get; set; }
        public string? UserEmail { get; set; }
    }
}