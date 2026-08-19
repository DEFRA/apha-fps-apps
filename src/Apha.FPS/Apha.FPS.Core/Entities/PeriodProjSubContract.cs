namespace Apha.FPS.Core.Entities
{
    public class PeriodProjSubContract
    {
        public short Period { get; set; }
        public int SubContCounter { get; set; }
        public string? Project { get; set; }
        public string? OracleProjectCode { get; set; }
        public string? SubAccountCode { get; set; }
        public string IsDefraProject { get; set; } = null!;
        public string? Opc { get; set; }
        public double? Occ { get; set; }
        public double? Month { get; set; }
        public decimal? Amount { get; set; }
        public string? AcctCode { get; set; }
    }
}
