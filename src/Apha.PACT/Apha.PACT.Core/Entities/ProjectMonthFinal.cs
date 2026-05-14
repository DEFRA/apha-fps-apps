namespace Apha.PACT.Core.Entities;

public partial class ProjectMonthFinal
{
    public string Project { get; set; } = null!;

    public double MonthNo { get; set; }

    public string? PeriodName { get; set; }

    public double? CumFlag { get; set; }

    public decimal? CostProfile { get; set; }

    public decimal? Subcontracts { get; set; }

    public decimal? Animals { get; set; }

    public decimal? NonAnimals { get; set; }

    public decimal? TimeCosts { get; set; }

    public decimal? TransferCosts { get; set; }

    public decimal? TotalCost { get; set; }

    public decimal? Invoices { get; set; }

    public decimal? Coiw { get; set; }

    public decimal? PortSales { get; set; }

    public decimal? CumCost { get; set; }

    public decimal? CumProfile { get; set; }

    public decimal? SumOfCostProfile { get; set; }

    public decimal? CumInvoices { get; set; }

    public decimal? CumCoiw { get; set; }

    public decimal? CumPortSales { get; set; }

    public int? MstoneDue { get; set; }

    public double? DueDone { get; set; }

    public double? OnTime { get; set; }

    public double? SumOfMstoneDue { get; set; }

    public double? SumOfDueDone { get; set; }

    public double? SumOfOnTime { get; set; }

    public decimal? CwDebit { get; set; }

    public decimal? CwCredit { get; set; }

    public decimal? CumCwDebit { get; set; }

    public decimal? CumCwCredit { get; set; }

    public double? TotalHours { get; set; }

    public double? CumTotalHours { get; set; }

    public double? CumSubcontracts { get; set; }

    public int? X { get; set; }

    public double? CumTestCosts { get; set; }

    public double? PayCosts { get; set; }

    public double? CumPayCosts { get; set; }

    public int? FpsYear { get; set; }
}