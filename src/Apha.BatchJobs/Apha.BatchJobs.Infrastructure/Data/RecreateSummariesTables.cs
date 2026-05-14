namespace Apha.BatchJobs.Infrastructure.Data;

// RecreateSummaries source/target table and view models used by LINQ implementation.

internal sealed class RsFpsYearTotalsTable
{
    public required string ParentProject { get; set; }
    public string? Program { get; set; }
    public decimal? TotalAdditionalCosts { get; set; }
    public double? TotalAnimalCosts { get; set; }
    public double? TotalStaffCosts { get; set; }
    public double? TotalTestCosts { get; set; }
    public double? TotalCosts { get; set; }
    public decimal? CustIncome { get; set; }
    public decimal? TransferIncome { get; set; }
    public decimal? TotalIncome { get; set; }
    public decimal? BudgetCvl { get; set; }
    public decimal? RequiredProfit { get; set; }
    public string? Manager { get; set; }
    public string? Customer { get; set; }
    public string? ProjectStatus { get; set; }
    public decimal? PvsIncome { get; set; }
    public decimal? PlanCaseworkDebit { get; set; }
    public double? TotalPayCosts { get; set; }
    public int FpsYear { get; set; }
}

internal sealed class RsTlkpProjectTable
{
    public required string ParentProject { get; set; }
    public string? Program { get; set; }
    public decimal? PlanCaseworkDebit { get; set; }
    public decimal? CustIncome { get; set; }
    public decimal? TransferIncome { get; set; }
    public decimal? BudgetCvl { get; set; }
    public decimal? Profit { get; set; }
    public string? Manager { get; set; }
    public string? Customer { get; set; }
    public string? ProjectStatus { get; set; }
    public decimal? PvsIncome { get; set; }
    public int FpsYear { get; set; }
    public double? CostCentre { get; set; }
    public short? IsDefraProject { get; set; }
    public string? OracleProjectCode { get; set; }
    public string? SubAccountCode { get; set; }
}

internal sealed class RsTlkpProgramTable
{
    public required string ProgramNo { get; set; }
    public string? SectorName { get; set; }
}

internal sealed class RsQryTotalAdditionalCostsView
{
    public required string JobCode { get; set; }
    public decimal? TotalAdditionalCosts { get; set; }
}

internal sealed class RsQryTotalAnimalCostsView
{
    public required string JobCode { get; set; }
    public decimal? TotalAnimalCosts { get; set; }
}

internal sealed class RsQryTotalStaffCostsView
{
    public required string JobCode { get; set; }
    public decimal? TotalStaffCosts { get; set; }
    public decimal? TotalPayCosts { get; set; }
}

internal sealed class RsQryTotalTestCostsView
{
    public required string JobCode { get; set; }
    public decimal? TotalTestCosts { get; set; }
}

internal sealed class RsProjectMonthTable
{
    public required string Project { get; set; }
    public int MonthNo { get; set; }
    public decimal? CostProfile { get; set; }
}

internal sealed class RsTimeCostCalcsTable
{
    public required string WorkGroup { get; set; }
    public required string JobCode { get; set; }
    public required string Project { get; set; }
    public int Month { get; set; }
    public required string StaffId { get; set; }
    public string? GradeCode { get; set; }
    public string? Name { get; set; }
    public decimal? ChargeRate { get; set; }
    public string? Class { get; set; }
    public double? Time { get; set; }
    public double? Cost { get; set; }
    public string? Division { get; set; }
    public double? Pay { get; set; }
    public double? NonPay { get; set; }
    public double? Overhead { get; set; }
    public int FpsYear { get; set; }
}

internal sealed class RsProjectMonthCaseworkTable
{
    public required string Project { get; set; }
    public int MonthNo { get; set; }
    public double? CwDebit { get; set; }
    public double? CwCredit { get; set; }
}

internal sealed class RsQryProjectMonthCwView
{
    public required string Project { get; set; }
    public int MonthNo { get; set; }
    public decimal? CwDebit { get; set; }
    public decimal? CwCredit { get; set; }
}

internal sealed class RsTblkpProfitCentreTable
{
    public required string ProfitCentre { get; set; }
    public string? Division { get; set; }
}

internal sealed class RsProfitCentreGradeTable
{
    public required string PcGrade { get; set; }
    public string? ProfitCentre { get; set; }
    public decimal? ChargeRate { get; set; }
    public decimal? DefraChargeRate { get; set; }
    public decimal? PayRate { get; set; }
    public decimal? Npr { get; set; }
    public decimal? Ohr { get; set; }
}

internal sealed class RsWorkGroupGradeTable
{
    public required string WgGrade { get; set; }
    public string? ProfitCentreGrade { get; set; }
    public string? WorkGroup { get; set; }
    public string? GradeCode { get; set; }
}

internal sealed class RsTimeCodeValidTable
{
    public required string WorkGroup { get; set; }
    public required string TimeCode { get; set; }
    public required string ParentProject { get; set; }
}

internal sealed class RsVpactTblStaffView
{
    public required string PactId { get; set; }
    public string? Name { get; set; }
    public string? WorkGroupGrade { get; set; }
}

internal sealed class RsMonthlyTimeTable
{
    public required string PactStaffId { get; set; }
    public required string WorkGroup { get; set; }
    public required string TimeCode { get; set; }
    public required string ParentProject { get; set; }
    public int Month { get; set; }
    public double? Hours { get; set; }
}

internal sealed class RsProjectMonth2Table
{
    public required string Project { get; set; }
    public int MonthNo { get; set; }
    public decimal? CostProfile { get; set; }
    public decimal? SubContracts { get; set; }
    public decimal? Animals { get; set; }
    public decimal? NonAnimal { get; set; }
    public double? TimeCosts { get; set; }
    public double? TransferCosts { get; set; }
    public decimal? TotalCost { get; set; }
    public decimal? Invoices { get; set; }
    public decimal? Coiw { get; set; }
    public double? SumOfCostProfile { get; set; }
    public double? PortSales { get; set; }
    public double? MstoneDue { get; set; }
    public double? DueDone { get; set; }
    public double? OnTime { get; set; }
    public double? TotalHours { get; set; }
    public double? PayCosts { get; set; }
}

internal sealed class RsProjectMonth3Table
{
    public int EndPeriod { get; set; }
    public string? PeriodName { get; set; }
    public required string Project { get; set; }
    public decimal? CumCost { get; set; }
    public decimal? CumInvoices { get; set; }
    public decimal? CumCoiw { get; set; }
    public decimal? CumPortSales { get; set; }
    public decimal? CumProfile { get; set; }
    public double? SumOfCostProfile { get; set; }
    public double? SumOfMstoneDue { get; set; }
    public double? SumOfDueDone { get; set; }
    public double? SumOfOnTime { get; set; }
    public decimal? CumCwDebit { get; set; }
    public decimal? CumCwCredit { get; set; }
    public double? CumTotalHours { get; set; }
    public double? CumSubContracts { get; set; }
    public double? CumTestCosts { get; set; }
    public double? CumPayCosts { get; set; }
}

internal sealed class RsProjectMonthFinalTable
{
    public required string Project { get; set; }
    public int MonthNo { get; set; }
    public decimal? CostProfile { get; set; }
    public decimal? SubContracts { get; set; }
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
    public string? PeriodName { get; set; }
    public double? SumOfCostProfile { get; set; }
    public decimal? CumInvoices { get; set; }
    public decimal? CumCoiw { get; set; }
    public decimal? CumPortSales { get; set; }
    public double? MstoneDue { get; set; }
    public double? DueDone { get; set; }
    public double? OnTime { get; set; }
    public double? SumOfMstoneDue { get; set; }
    public double? SumOfDueDone { get; set; }
    public double? SumOfOnTime { get; set; }
    public int? CumFlag { get; set; }
    public decimal? CwDebit { get; set; }
    public decimal? CwCredit { get; set; }
    public decimal? CumCwDebit { get; set; }
    public decimal? CumCwCredit { get; set; }
    public double? TotalHours { get; set; }
    public double? CumTotalHours { get; set; }
    public double? CumSubContracts { get; set; }
    public double? CumTestCosts { get; set; }
    public double? PayCosts { get; set; }
    public double? CumPayCosts { get; set; }
}

internal sealed class RsTblPeriodTable
{
    public int EndPeriod { get; set; }
    public string? PeriodName { get; set; }
    public int? PeriodLocked { get; set; }
}

internal sealed class RsTblkPeriodMonthTable
{
    public required string PeriodName { get; set; }
    public int MonthNo { get; set; }
}

internal sealed class RsQryJobMonthSubContractsView
{
    public required string Project { get; set; }
    public int Month { get; set; }
    public decimal? Total { get; set; }
    public decimal? Animals { get; set; }
    public decimal? Other { get; set; }
}

internal sealed class RsQryJobMonthTimeView
{
    public required string Project { get; set; }
    public int Month { get; set; }
    public double? SumOfCost { get; set; }
    public double? SumOfHours { get; set; }
    public decimal? SumOfPayRate { get; set; }
    public decimal? WorkCost { get; set; }
}

internal sealed class RsQryJobMonthMilestoneView
{
    public required string Project { get; set; }
    public int DueMonth { get; set; }
    public double? MstoneDue { get; set; }
    public double? DueDone { get; set; }
    public double? OnTime { get; set; }
}

internal sealed class RsQryJobMonthTransfersTotalView
{
    public required string Project { get; set; }
    public int Month { get; set; }
    public decimal? SumOfTransferCost { get; set; }
}

internal sealed class RsQryJobMonthInvoicesView
{
    public required string ProjectParent { get; set; }
    public int Month { get; set; }
    public decimal? SumOfAmount1 { get; set; }
}

internal sealed class RsQryJobMonthPortfolioSalesView
{
    public required string PlanPortfolio { get; set; }
    public int Month { get; set; }
    public decimal? Fee { get; set; }
}

internal sealed class RsQryJobMonthTotProfileView
{
    public required string Project { get; set; }
    public double? SumOfCostProfile { get; set; }
}

internal sealed class RsRecreateSummariesLogTable
{
    public required string UserId { get; set; }
    public int Period { get; set; }
    public DateTime DateDone { get; set; }
}

internal sealed class RsCostCentreTable
{
    public double CostCentre { get; set; }
    public string? ProfitCentre { get; set; }
}

internal sealed class RsMonthlyOutputTable
{
    public required string Buyer { get; set; }
    public required string WorkGroup { get; set; }
    public required string TestCode { get; set; }
    public int Month { get; set; }
    public double? Volume { get; set; }
}

internal sealed class RsWorkGroupTable
{
    public required string WorkGroup { get; set; }
    public string? ProfitCentre { get; set; }
    public double? CostCentre { get; set; }
}

internal sealed class RsTlkpTestReqmtTable
{
    public required string ProjectBuyerCode { get; set; }
    public required string TestCode { get; set; }
    public decimal? UnitPrice { get; set; }
}

internal sealed class RsPeriodMonthlyOutputTable
{
    public int Period { get; set; }
    public required string Project { get; set; }
    public string? OracleProjectCode { get; set; }
    public string? SubAccountCode { get; set; }
    public string? IsDefraProject { get; set; }
    public string? Opc { get; set; }
    public double? Occ { get; set; }
    public int Month { get; set; }
    public string? Spc { get; set; }
    public string? WorkGroup { get; set; }
    public double? Scc { get; set; }
    public string? TestCode { get; set; }
    public double? Volume { get; set; }
    public decimal? TestPrice { get; set; }
    public decimal? TotalCost { get; set; }
}

internal sealed class RsProjSubContractTable
{
    public int SubContCounter { get; set; }
    public required string Project { get; set; }
    public int Month { get; set; }
    public decimal? Amount { get; set; }
    public string? AcctCode { get; set; }
}

internal sealed class RsPeriodProjSubContractTable
{
    public int Period { get; set; }
    public int SubContCounter { get; set; }
    public required string Project { get; set; }
    public string? OracleProjectCode { get; set; }
    public string? SubAccountCode { get; set; }
    public string? IsDefraProject { get; set; }
    public string? Opc { get; set; }
    public double? Occ { get; set; }
    public int Month { get; set; }
    public decimal? Amount { get; set; }
    public string? AcctCode { get; set; }
}

internal sealed class RsTblWgEmployeeTable
{
    public required string PactId { get; set; }
    public string? SpNumber { get; set; }
}

internal sealed class RsPeriodTimeCostCalcsTable
{
    public int Period { get; set; }
    public required string Project { get; set; }
    public string? OracleProjectCode { get; set; }
    public string? SubAccountCode { get; set; }
    public int Month { get; set; }
    public string? DefraProject { get; set; }
    public double? Occ { get; set; }
    public string? Opc { get; set; }
    public string? Spc { get; set; }
    public double? Scc { get; set; }
    public required string Name { get; set; }
    public string? GradeCode { get; set; }
    public string? SpNumber { get; set; }
    public decimal? ChargeRate { get; set; }
    public double? Pay { get; set; }
    public double? NonPay { get; set; }
    public double? Overhead { get; set; }
    public double? Time { get; set; }
    public double? TotalCost { get; set; }
}
