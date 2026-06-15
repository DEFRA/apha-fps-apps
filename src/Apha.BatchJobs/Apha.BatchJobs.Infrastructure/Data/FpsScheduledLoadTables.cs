namespace Apha.BatchJobs.Infrastructure.Data;

/// <summary>
/// EF entity for fps.fps_source_project_year.
/// </summary>
internal sealed class FpsSourceProjectYearTable
{
    public short Year { get; set; }
    public required string ParentProject { get; set; }
    public required string Program { get; set; }
    public decimal? TotalAdditionalCosts { get; set; }
    public double? TotalAnimalCosts { get; set; }
    public double? TotalStaffCosts { get; set; }
    public double? TotalTestCosts { get; set; }
    public double? TotalCosts { get; set; }
    public decimal CustIncome { get; set; }
    public decimal TransferIncome { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal? BudgetCvl { get; set; }
    public decimal? RequiredProfit { get; set; }
    public string? Manager { get; set; }
    public string? Customer { get; set; }
    public string? ProjectStatus { get; set; }
    public decimal? PvsIncome { get; set; }
    public decimal? PlanCaseworkDebit { get; set; }
    public double? TotalPayCosts { get; set; }
}

/// <summary>
/// EF entity for fps.fps_year_totals.
/// </summary>
internal sealed class FpsYearTotalsTable
{
    public short Year { get; set; }
    public required string ParentProject { get; set; }
    public required string Program { get; set; }
    public decimal? TotalAdditionalCosts { get; set; }
    public double? TotalAnimalCosts { get; set; }
    public double? TotalStaffCosts { get; set; }
    public double? TotalTestCosts { get; set; }
    public double? TotalCosts { get; set; }
    public decimal CustIncome { get; set; }
    public decimal TransferIncome { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal? BudgetCvl { get; set; }
    public decimal? RequiredProfit { get; set; }
    public string? Manager { get; set; }
    public string? Customer { get; set; }
    public required string ProjectStatus { get; set; }
    public decimal? PvsIncome { get; set; }
    public decimal? PlanCaseworkDebit { get; set; }
    public double? TotalPayCosts { get; set; }
}

/// <summary>
/// EF entity for fps.fps_year_archive.
/// </summary>
internal sealed class FpsYearArchiveTable
{
    public short Year { get; set; }
    public required string ParentProject { get; set; }
    public required string Program { get; set; }
    public decimal? TotalAdditionalCosts { get; set; }
    public double? TotalAnimalCosts { get; set; }
    public double? TotalStaffCosts { get; set; }
    public double? TotalTestCosts { get; set; }
    public double? TotalCosts { get; set; }
    public decimal CustIncome { get; set; }
    public decimal TransferIncome { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal? BudgetCvl { get; set; }
    public decimal? RequiredProfit { get; set; }
    public string? Manager { get; set; }
    public string? Customer { get; set; }
    public required string ProjectStatus { get; set; }
    public decimal? PvsIncome { get; set; }
    public decimal? PlanCaseworkDebit { get; set; }
    public double? TotalPayCosts { get; set; }
    public DateTime ArchivedAt { get; set; }
    public required string ArchiveReason { get; set; }
}

/// <summary>
/// EF entity for fps.fps_project_all_current_year.
/// </summary>
internal sealed class FpsProjectAllCurrentYearTable
{
    public short Year { get; set; }
    public required string ParentProject { get; set; }
    public string? Program { get; set; }
    public string? Customer { get; set; }
    public string? Manager { get; set; }
    public decimal? TransferIncome { get; set; }
    public decimal? CustIncome { get; set; }
    public decimal? WipEoy { get; set; }
    public decimal? WipLimit { get; set; }
    public decimal? WipCurrent { get; set; }
    public string? ProjectStatus { get; set; }
    public DateTime? DateCreated { get; set; }
    public decimal? FecCost { get; set; }
    public decimal? Profit { get; set; }
    public decimal? BudgetCvl { get; set; }
    public decimal? CaseworkSub { get; set; }
    public decimal? PvsIncome { get; set; }
    public decimal? PlanCaseworkDebit { get; set; }
    public string? Source { get; set; }
    public string? Disease { get; set; }
    public string? Contract { get; set; }
    public short? Finished { get; set; }
    public string? Comments { get; set; }
    public decimal? CarryOver { get; set; }
    public short? IsDefraProject { get; set; }
    public double? CostCentre { get; set; }
    public string? OracleProjectCode { get; set; }
    public string? SubAccountCode { get; set; }
    public string? ProjectGroup { get; set; }
    public string? IncomeAccountCode { get; set; }
    public DateTime RefreshedAt { get; set; }
}
