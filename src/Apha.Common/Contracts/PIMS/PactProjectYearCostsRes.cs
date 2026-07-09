/*
 * TRANSFORMENGINE MIGRATION — PactProjectYearCostsRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - New file: no prior C# equivalent existed
 *   - Response contract for the vpactprojectyearcosts aggregation view, used by the
 *     "Update Costing" button (btnUpdateCosting) in frmProjectRadTrackData_Update
 *   - Source view: vpactprojectyearcosts (mab_archive schema) — aggregates actuals from
 *     my_projectmonthfinal joined to g_tlkpproject_radtrackdata and vtcc_summary
 *   - Joined lookup via qryProjectYearTotals_frm also surfaces CustIncome and Budget_CVL
 *     from MY_tlkpProject — included here as optional output fields
 *   - All sum() aggregate columns mapped to decimal? (money-equivalent totals)
 *   - Hours mapped to double? (totalcost/timecost are monetary; hours are float)
 *   - Project and Year are grouping/filter keys always populated in responses
 *
 * PRESERVED:
 *   - Column names aligned with vpactprojectyearcosts view and qryProjectYearTotals_frm output
 *   - SubContracts, Animals, Tests, Pay, NonPayOH column semantics preserved from query
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify whether the backend endpoint returns per-month rows
 *     (include MonthNo) or year-level aggregates only (omit MonthNo) — the Access subform
 *     sf_PactProjectYearCosts groups by project+year for the update button but the
 *     underlying view includes monthno; current contract omits MonthNo assuming year-level
 */

namespace Apha.Common.Contracts.PIMS
{
    public class PactProjectYearCostsRes
    {
        // TRANSFORMENGINE: Grouping key fields from vpactprojectyearcosts
        public string? Project { get; set; }
        public short Year { get; set; }

        // TRANSFORMENGINE: Aggregated cost columns from vpactprojectyearcosts (sum() results)
        public decimal? SubContracts { get; set; }          // sum(subcontracts)
        public decimal? Animals { get; set; }               // sum(animals) — maps to AnimalCosts in main record
        public decimal? Tests { get; set; }                 // sum(transfercosts) — maps to TestCosts in main record
        public decimal? Pay { get; set; }                   // sum(vtcc_summary.pay) — maps to PayCosts in main record
        public decimal? NonPayOH { get; set; }              // sum(nonpay+overhead) — maps to NonPayOhCosts in main record
        public decimal? TotalCosts { get; set; }            // sum(totalcost) — used by btnFixCosting to set ActualExpenditure
        public decimal? TimeCost { get; set; }              // sum(timecosts) — display only

        // TRANSFORMENGINE: Hours (float/double precision in source view)
        public double? Hours { get; set; }                  // sum(totalhours) — maps to ManHours; ManDays/ManYears derived

        // TRANSFORMENGINE: Joined lookup fields from MY_tlkpProject via qryProjectYearTotals_frm
        public decimal? CustIncome { get; set; }            // MY_tlkpProject.CustIncome — display label: "Customer Income"
        public decimal? BudgetCvl { get; set; }             // MY_tlkpProject.Budget_CVL — display label: "VLA Budget"
    }
}
