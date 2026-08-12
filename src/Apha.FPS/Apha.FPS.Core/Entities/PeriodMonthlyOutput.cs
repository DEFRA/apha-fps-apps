namespace Apha.FPS.Core.Entities
{
    public class PeriodMonthlyOutput
    {
        public int Id { get; set; }
        public int Period { get; set; }
        public string Project { get; set; } = null!;
        public string? OracleProjectCode { get; set; }
        public string? SubAccountCode { get; set; }
        public string IsDefraProject { get; set; } = null!;
        public string? Opc { get; set; }
        public double? Occ { get; set; }
        public double Month { get; set; }
        public string Spc { get; set; } = null!;
        public string WorkGroup { get; set; } = null!;
        public double? Scc { get; set; }
        public string TestCode { get; set; } = null!;
        public double? Volume { get; set; }
        public decimal? TestPrice { get; set; }
        public decimal? TotalCost { get; set; }
    }
}
