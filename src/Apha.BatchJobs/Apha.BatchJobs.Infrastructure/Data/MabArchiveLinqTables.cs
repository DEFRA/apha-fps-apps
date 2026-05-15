namespace Apha.BatchJobs.Infrastructure.Data;

// MABArchive source/target models for LINQ migration (phase 2 incremental mapping).

internal sealed class MaSrcTlkpProgram
{
    public int FpsYear { get; set; }
    public required string ProgramNo { get; set; }
    public string? ProgramName { get; set; }
    public string? Directorate { get; set; }
    public string? Minim { get; set; }
    public string? SectorName { get; set; }
    public string? Customer { get; set; }
    public string? Target { get; set; }
    public string? Manager { get; set; }
}

internal sealed class MaSrcTlkpProject
{
    public int FpsYear { get; set; }
    public required string ParentProject { get; set; }
    public string? ProjectTitle { get; set; }
    public string? CostBookNo { get; set; }
    public string? Disease { get; set; }
    public string? Contract { get; set; }
    public string? ShortTitle { get; set; }
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
    public double? CaseworkSub { get; set; }
    public decimal? PvsIncome { get; set; }
    public decimal? PlanCaseworkDebit { get; set; }
    public short? Finished { get; set; }
    public string? Comments { get; set; }
    public decimal? CarryOver { get; set; }
    public short? IsDefraProject { get; set; }
    public double? CostCentre { get; set; }
    public string? OracleProjectCode { get; set; }
    public string? SubAccountCode { get; set; }
    public string? ProjectGroup { get; set; }
    public string? IncomeAccountCode { get; set; }
}

internal sealed class MaSrcFpsYearTotals
{
    public int FpsYear { get; set; }
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
}

internal sealed class MaDstMyTlkpProgram
{
    public int Year { get; set; }
    public required string ProgramNo { get; set; }
    public string? ProgramName { get; set; }
    public string? Directorate { get; set; }
    public string? Minim { get; set; }
    public string? SectorName { get; set; }
    public string? Customer { get; set; }
    public string? Target { get; set; }
    public string? Manager { get; set; }
}

internal sealed class MaDstGTlkpProject
{
    public required string ParentProject { get; set; }
    public string? ProjectTitle { get; set; }
    public string? CostBookNo { get; set; }
    public string? Disease { get; set; }
    public string? Contract { get; set; }
    public string? ShortTitle { get; set; }
    public string? ProjectStatus { get; set; }
}

internal sealed class MaDstMyTlkpProject
{
    public int Year { get; set; }
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
    public double? CaseworkSub { get; set; }
    public decimal? PvsIncome { get; set; }
    public decimal? PlanCaseworkDebit { get; set; }
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
}

internal sealed class MaDstMyFpsYearTotals
{
    public int Year { get; set; }
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
}

internal sealed class MaSrcMonthlyOutput
{
    public int FpsYear { get; set; }
    public required string TestCode { get; set; }
    public required string Buyer { get; set; }
    public int Month { get; set; }
    public required string WorkGroup { get; set; }
    public double? Volume { get; set; }
    public string? WgBuyer { get; set; }
}

internal sealed class MaDstMyMonthlyOutput
{
    public int Year { get; set; }
    public required string TestCode { get; set; }
    public required string Buyer { get; set; }
    public int Month { get; set; }
    public required string WorkGroup { get; set; }
    public double? Volume { get; set; }
    public string? WgBuyer { get; set; }
}

internal sealed class MaSrcMonthlyTime
{
    public int FpsYear { get; set; }
    public required string PactStaffId { get; set; }
    public required string TimeCode { get; set; }
    public int Month { get; set; }
    public required string ParentProject { get; set; }
    public required string WorkGroup { get; set; }
    public double? Hours { get; set; }
}

internal sealed class MaDstMyMonthlyTime
{
    public int Year { get; set; }
    public required string PactStaffId { get; set; }
    public required string TimeCode { get; set; }
    public int Month { get; set; }
    public required string ParentProject { get; set; }
    public required string WorkGroup { get; set; }
    public double? Hours { get; set; }
}

internal sealed class MaSrcProjInvoice
{
    public int FpsYear { get; set; }
    public required string ProjectParent { get; set; }
    public int Month { get; set; }
    public decimal? Amount { get; set; }
    public decimal? CostOfWork { get; set; }
    public decimal? Wip { get; set; }
    public decimal? ProfitLoss { get; set; }
    public string? Detail { get; set; }
    public int InvoiceCounter { get; set; }
    public string? Type { get; set; }
}

internal sealed class MaDstMyProjInvoice
{
    public int Year { get; set; }
    public required string ProjectParent { get; set; }
    public int Month { get; set; }
    public decimal? Amount { get; set; }
    public decimal? CostOfWork { get; set; }
    public decimal? Wip { get; set; }
    public decimal? ProfitLoss { get; set; }
    public string? Detail { get; set; }
    public int InvoiceCounter { get; set; }
    public string? Type { get; set; }
}

internal sealed class MaSrcProjSubContract
{
    public int FpsYear { get; set; }
    public int SubContCounter { get; set; }
    public required string Project { get; set; }
    public string? TestJob { get; set; }
    public int Month { get; set; }
    public decimal? Amount { get; set; }
    public string? WorkGroup { get; set; }
    public string? AcctCode { get; set; }
    public string? Supplier { get; set; }
    public string? Description { get; set; }
    public string? SupplierNumber { get; set; }
    public decimal? DailyRate { get; set; }
    public double? AnimalDays { get; set; }
}

internal sealed class MaDstMyProjSubContract
{
    public int Year { get; set; }
    public int SubContCounter { get; set; }
    public required string Project { get; set; }
    public string? TestJob { get; set; }
    public int Month { get; set; }
    public decimal? Amount { get; set; }
    public string? WorkGroup { get; set; }
    public string? AcctCode { get; set; }
    public string? Supplier { get; set; }
    public string? Description { get; set; }
    public string? SupplierNumber { get; set; }
    public decimal? DailyRate { get; set; }
    public double? AnimalDays { get; set; }
}

internal sealed class MaSrcProjectMonthFinal
{
    public int FpsYear { get; set; }
    public required string Project { get; set; }
    public int MonthNo { get; set; }
    public string? PeriodName { get; set; }
    public int? CumFlag { get; set; }
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
    public decimal? SumOfCostProfile { get; set; }
    public decimal? CumInvoices { get; set; }
    public decimal? CumCoiw { get; set; }
    public decimal? CumPortSales { get; set; }
    public double? MstoneDue { get; set; }
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
    public double? CumSubContracts { get; set; }
    public double? CumTestCosts { get; set; }
    public double? PayCosts { get; set; }
    public double? CumPayCosts { get; set; }
}

internal sealed class MaDstMyProjectMonthFinal
{
    public int Year { get; set; }
    public required string Project { get; set; }
    public int MonthNo { get; set; }
    public string? PeriodName { get; set; }
    public int? CumFlag { get; set; }
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
    public decimal? SumOfCostProfile { get; set; }
    public decimal? CumInvoices { get; set; }
    public decimal? CumCoiw { get; set; }
    public decimal? CumPortSales { get; set; }
    public double? MstoneDue { get; set; }
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
    public double? CumSubContracts { get; set; }
    public double? CumTestCosts { get; set; }
    public double? PayCosts { get; set; }
    public double? CumPayCosts { get; set; }
}

internal sealed class MaSrcTblAdditionalCosts
{
    public int FpsYear { get; set; }
    public required string JobCode { get; set; }
    public string? Account { get; set; }
    public string? Description { get; set; }
    public decimal? ItemCost { get; set; }
    public string? Freq { get; set; }
    public string? Supplier { get; set; }
    public int? AcCounter { get; set; }
}

internal sealed class MaDstMyTblAdditionalCosts
{
    public int Year { get; set; }
    public required string JobCode { get; set; }
    public string? Account { get; set; }
    public string? Description { get; set; }
    public decimal? ItemCost { get; set; }
    public string? Freq { get; set; }
    public string? Supplier { get; set; }
    public int? AcCounter { get; set; }
}

internal sealed class MaSrcTblAnimalReq
{
    public int FpsYear { get; set; }
    public required string JobCode { get; set; }
    public string? AnimalType { get; set; }
    public double? NumberOfDays { get; set; }
    public int? NumberOfAnimals { get; set; }
}

internal sealed class MaDstMyTblAnimalReq
{
    public int Year { get; set; }
    public required string JobCode { get; set; }
    public string? AnimalType { get; set; }
    public double? NumberOfDays { get; set; }
    public int? NumberOfAnimals { get; set; }
}

internal sealed class MaSrcTblContract
{
    public int FpsYear { get; set; }
    public required string ContractNo { get; set; }
    public string? Category { get; set; }
    public string? Manager { get; set; }
    public string? Customer { get; set; }
    public string? Title { get; set; }
    public DateTime? RegisteredDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ContractDoc { get; set; }
    public double? Duration { get; set; }
}

internal sealed class MaDstMyTblContract
{
    public int Year { get; set; }
    public required string ContractNo { get; set; }
    public string? Category { get; set; }
    public string? Manager { get; set; }
    public string? Customer { get; set; }
    public string? Title { get; set; }
    public DateTime? RegisteredDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ContractDoc { get; set; }
    public double? Duration { get; set; }
}

internal sealed class MaSrcTblStaffJob
{
    public int FpsYear { get; set; }
    public required string StaffId { get; set; }
    public required string JobCode { get; set; }
    public double? PlannedHours { get; set; }
}

internal sealed class MaDstMyTblStaffJob
{
    public int Year { get; set; }
    public required string StaffId { get; set; }
    public required string JobCode { get; set; }
    public double? PlannedHours { get; set; }
}

internal sealed class MaSrcTimeCostCalcs
{
    public int FpsYear { get; set; }
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
    public string? JobCodeOld { get; set; }
    public decimal? Pay { get; set; }
    public decimal? NonPay { get; set; }
    public decimal? Overhead { get; set; }
}

internal sealed class MaDstMyTimeCostCalcs
{
    public int Year { get; set; }
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
    public string? JobCodeOld { get; set; }
    public decimal? Pay { get; set; }
    public decimal? NonPay { get; set; }
    public decimal? Overhead { get; set; }
}

internal sealed class MaSrcTlkpTestReqmt
{
    public int FpsYear { get; set; }
    public required string TestCode { get; set; }
    public string? Buyer { get; set; }
    public decimal? UnitPrice { get; set; }
    public int? NoRequired { get; set; }
    public required string ProjectBuyerCode { get; set; }
    public string? TestBuyerCode { get; set; }
}

internal sealed class MaDstMyTlkpTestReqmt
{
    public int Year { get; set; }
    public required string TestCode { get; set; }
    public string? Buyer { get; set; }
    public decimal? UnitPrice { get; set; }
    public int? NoRequired { get; set; }
    public required string ProjectBuyerCode { get; set; }
    public string? TestBuyerCode { get; set; }
}

internal sealed class MaSrcTblDbVariable
{
    public required string DbVarName { get; set; }
    public string? DbVarValue { get; set; }
}

internal sealed class MaDstTlkpYear
{
    public int Year { get; set; }
    public int? LatestMonthReleased { get; set; }
}

internal sealed class MaSrcWorkGroupGrade
{
    public int FpsYear { get; set; }
    public required string WgGrade { get; set; }
    public string? ProfitCentreGrade { get; set; }
    public string? GradeCode { get; set; }
    public string? WorkGroup { get; set; }
}

internal sealed class MaDstMyWorkGroupGrade
{
    public int Year { get; set; }
    public required string WgGrade { get; set; }
    public string? ProfitCentreGrade { get; set; }
    public string? GradeCode { get; set; }
    public string? WorkGroup { get; set; }
}

internal sealed class MaSrcProfitCentreGrade
{
    public int FpsYear { get; set; }
    public required string PcGrade { get; set; }
    public string? DivisionGrade { get; set; }
    public string? GradeCode { get; set; }
    public string? ProfitCentre { get; set; }
    public decimal? ChargeRate { get; set; }
    public decimal? DirectRate { get; set; }
    public decimal? PayRate { get; set; }
    public decimal? Npr { get; set; }
    public decimal? Ohr { get; set; }
}

internal sealed class MaDstMyProfitCentreGrade
{
    public int Year { get; set; }
    public required string PcGrade { get; set; }
    public string? DivisionGrade { get; set; }
    public string? GradeCode { get; set; }
    public string? ProfitCentre { get; set; }
    public decimal? ChargeRate { get; set; }
    public decimal? DirectRate { get; set; }
    public decimal? PayRate { get; set; }
    public decimal? Npr { get; set; }
    public decimal? Ohr { get; set; }
}

internal sealed class MaSrcTblkpProfitCentre
{
    public required string ProfitCentre { get; set; }
    public string? ProfitCentreName { get; set; }
    public string? Division { get; set; }
    public decimal? ContTarget { get; set; }
    public string? ProfitCentreHead { get; set; }
    public string? DivisionId { get; set; }
}

internal sealed class MaDstMyTblProfitCentre
{
    public int Year { get; set; }
    public required string ProfitCentre { get; set; }
    public string? ProfitCentreName { get; set; }
    public string? Division { get; set; }
    public decimal? ContTarget { get; set; }
    public string? ProfitCentreHead { get; set; }
    public string? DivisionId { get; set; }
}

internal sealed class MaSrcTestOrProduct
{
    public int FpsYear { get; set; }
    public required string ItemCode { get; set; }
    public string? ItemDescription { get; set; }
    public string? TestManager { get; set; }
    public string? JobStatus { get; set; }
    public decimal? UnitPriceVla { get; set; }
    public decimal? PriceAhvg { get; set; }
    public string? Owner { get; set; }
    public string? ChargeMethod { get; set; }
    public string? ShortDescription { get; set; }
    public decimal? DefraUnitPrice { get; set; }
}

internal sealed class MaDstMyTestOrProduct
{
    public int Year { get; set; }
    public required string ItemCode { get; set; }
    public string? ItemDescription { get; set; }
    public string? TestManager { get; set; }
    public string? JobStatus { get; set; }
    public decimal? UnitPriceVla { get; set; }
    public decimal? PriceAhvg { get; set; }
    public string? Owner { get; set; }
    public string? ChargeMethod { get; set; }
    public string? ShortDescription { get; set; }
    public decimal? DefraUnitPrice { get; set; }
}

internal sealed class MaSrcTblWgEmployee
{
    public int FpsYear { get; set; }
    public required string PactId { get; set; }
    public required string SpNumber { get; set; }
    public string? WorkGroupGrade { get; set; }
    public string? PersonStatus { get; set; }
    public string? PersonClass { get; set; }
    public double? HrsPaid { get; set; }
    public double? LeaveHours { get; set; }
    public double? SickSpecial { get; set; }
    public double? HrsAvail { get; set; }
}

internal sealed class MaSrcTblEmployee
{
    public required string SpNumber { get; set; }
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public string? Title { get; set; }
}

internal sealed class MaDstMyStaff
{
    public int Year { get; set; }
    public required string StaffId { get; set; }
    public string? Name { get; set; }
    public string? WorkGroupGrade { get; set; }
    public string? Title { get; set; }
    public string? PersonStatus { get; set; }
    public string? PersonClass { get; set; }
    public double? HrsPaid { get; set; }
    public double? LeaveHours { get; set; }
    public double? SickSpecial { get; set; }
    public double? HrsAvail { get; set; }
}

internal sealed class MaSrcWorkGroup
{
    public int FpsYear { get; set; }
    public required string WorkGroup { get; set; }
    public string? ProfitCentre { get; set; }
    public double? CostCentre { get; set; }
    public string? Owner { get; set; }
    public string? Description { get; set; }
    public decimal? CentralOverhead { get; set; }
    public string? SendEmail { get; set; }
    public decimal? Cos90 { get; set; }
    public string? CostCentreOld { get; set; }
    public string? EmailRecipient { get; set; }
}

internal sealed class MaDstMyWorkGroup
{
    public int Year { get; set; }
    public required string WorkGroup { get; set; }
    public string? ProfitCentre { get; set; }
    public double? CostCentre { get; set; }
    public string? Owner { get; set; }
    public string? Description { get; set; }
    public decimal? CentralOverhead { get; set; }
    public string? SendEmail { get; set; }
    public decimal? Cos90 { get; set; }
    public string? CostCentreOld { get; set; }
    public string? EmailRecipient { get; set; }
}

internal sealed class MaSrcTblAnimals
{
    public int FpsYear { get; set; }
    public required string AnimalType { get; set; }
    public string? Species { get; set; }
    public string? SecurityLevel { get; set; }
    public decimal? DailyRate { get; set; }
    public string? PlanByWeek { get; set; }
    public decimal? DefraDailyRate { get; set; }
}

internal sealed class MaDstMyTblAnimals
{
    public int Year { get; set; }
    public required string AnimalType { get; set; }
    public string? Species { get; set; }
    public string? SecurityLevel { get; set; }
    public decimal? DailyRate { get; set; }
    public string? PlanByWeek { get; set; }
    public decimal? DefraDailyRate { get; set; }
}

internal sealed class MaDstMyTlkpProjectAll
{
    public int Year { get; set; }
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
    public double? CaseworkSub { get; set; }
    public decimal? PvsIncome { get; set; }
    public decimal? PlanCaseworkDebit { get; set; }
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
}
