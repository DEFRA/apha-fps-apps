namespace Apha.Common.Contracts.PIMS
{
    public class ProgramCustomerMonitoringReportDataRes
    {
        public short? Year { get; set; }
        public string? Project { get; set; }
        public string? ParentProject { get; set; }
        public string? Program { get; set; }
        public string? ProjectTitle { get; set; }
        public string? Manager { get; set; }
        public string? ProjectStatus { get; set; }
        public string? Customer { get; set; }
        public string? Contract { get; set; }
        public decimal? PlannedCosts { get; set; }
        public decimal? BudgetCvl { get; set; }
        public decimal? CustIncome { get; set; }
        public decimal? ActualCostsYt { get; set; }
        public decimal? PercentOfBudget { get; set; }
        public double? PcForecastSpend { get; set; }
        public decimal? EstimateSpend { get; set; }
        public decimal? LinearSpend { get; set; }
        public decimal? BfBudget { get; set; }
        public decimal? TotalIncome { get; set; }
        public decimal? CumInvoice { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? MonitoringComment { get; set; }
    }
}
