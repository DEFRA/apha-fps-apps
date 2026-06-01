namespace Apha.Common.Contracts.FPS
{
    public class ProfitCentreGradeRes
    {
        public string PcGrade { get; set; } = null!;
        public string DivisionGrade { get; set; } = null!;
        public string GradeCode { get; set; } = null!;
        public string ProfitCentre { get; set; } = null!;
        public decimal? ChargeRate { get; set; }
        public decimal? DirectRate { get; set; }
        public decimal? PayRate { get; set; }
        public decimal? NPR { get; set; }
        public decimal? OHR { get; set; }
        public double? HrsAvailable { get; set; }
    }
}
