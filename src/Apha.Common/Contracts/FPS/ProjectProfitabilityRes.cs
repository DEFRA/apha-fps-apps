namespace Apha.Common.Contracts.FPS
{
    public class ProjectProfitabilityRes
    {
        public string JobCode { get; set; } = null!;
        public decimal JcTotalStaffCosts { get; set; }
        public decimal JcTotalTestCosts { get; set; }
        public decimal JcTotalAnimalCosts { get; set; }
        public decimal JcTotalAdditionalCosts { get; set; }
        public decimal TotalCosts { get; set; }
        public decimal? BudgetCvl { get; set; }
        public decimal JcProfit { get; set; }
        public decimal TargetProfit { get; set; }
        public decimal OffTarget { get; set; }
        public string? ProgramNo { get; set; }
        public decimal? ProgrammeTarget { get; set; }
        public string? ProjectStatus { get; set; }
    }
}
