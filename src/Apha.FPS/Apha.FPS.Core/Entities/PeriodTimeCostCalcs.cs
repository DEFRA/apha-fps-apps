namespace Apha.FPS.Core.Entities
{
    // Maps fps.period_timecostcalcs — per-period snapshot of staff time costs.
    // Used by the Snapshot Data tab Time query (SQL Server fPeriodTime equivalent):
    // end-period snapshot minus start-period snapshot.
    public class PeriodTimeCostCalcs
    {
        public int Id { get; set; }
        public int Period { get; set; }
        public string Project { get; set; } = null!;
        public string? OracleProjectCode { get; set; }
        public string? SubAccountCode { get; set; }
        public double Month { get; set; }
        public string DefraProject { get; set; } = null!;
        public double? Occ { get; set; }
        public string? Opc { get; set; }
        public string Spc { get; set; } = null!;
        public double? Scc { get; set; }
        public string? Name { get; set; }
        public string? GradeCode { get; set; }
        public string SpNumber { get; set; } = null!;
        public decimal? ChargeRate { get; set; }
        public decimal? Pay { get; set; }
        public decimal? NonPay { get; set; }
        public decimal? Overhead { get; set; }
        public double? Time { get; set; }
        public decimal? TotalCost { get; set; }
    }
}
