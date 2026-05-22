namespace Apha.Common.Contracts.FPS
{
    public class DivisionGradeReq
    {
        public string DivisionGradeCode { get; set; } = null!;
        public string GradeCode { get; set; } = null!;
        public string Division { get; set; } = null!;

        public decimal? ChargeRate { get; set; }
        public decimal? DirectRate { get; set; }
        public decimal? PayRate { get; set; }
        public decimal? Npr { get; set; }
        public decimal? Ohr { get; set; }
    }
}
