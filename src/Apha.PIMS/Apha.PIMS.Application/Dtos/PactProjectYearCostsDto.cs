/*
 * TRANSFORMENGINE MIGRATION — PactProjectYearCostsDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - New file: no prior C# DTO existed for this entity
 *   - Application-layer DTO mirroring PactProjectYearCosts keyless projection entity
 *     derived from mabarchive.vpactprojectyearcosts view
 *   - Source view aggregates my_projectmonthfinal joined to g_tlkpproject_radtrackdata
 *     and vtcc_summary; groups by project, year (derived), monthno
 *   - All sum() aggregate cost columns (SubContracts, Animals, Tests, Pay, NonPayOH,
 *     TotalCosts, TimeCost) mapped to decimal?
 *   - Hours column mapped to double? — float precision in source view
 *   - MonthNo preserved as double to match entity definition
 *   - Year preserved as double to match entity definition (CASE-derived in view)
 *   - CustIncome and BudgetCvl: optional joined fields from MY_tlkpProject via
 *     qryProjectYearTotals_frm (carried from PactProjectYearCostsRes contract)
 *
 * PRESERVED:
 *   - All column aliases from vpactprojectyearcosts view normalised to PascalCase
 *   - Column semantics aligned with PactProjectYearCostsRes response contract
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether MonthNo should be exposed at the API response
 *     layer or suppressed at the service layer (current: included in DTO, suppressed in
 *     PactProjectYearCostsRes per that contract's design decision)
 *   - TRANSFORMENGINE TODO: Verify EF map uses HasNoKey() for PactProjectYearCosts entity
 */

namespace Apha.PIMS.Application.Dtos
{
    /// <summary>
    /// Application-layer DTO for PACT actuals aggregation.
    /// Maps to/from <see cref="Apha.PIMS.Core.Entities.PactProjectYearCosts"/>.
    /// Derived from the <c>mabarchive.vpactprojectyearcosts</c> read-only view.
    /// </summary>
    public class PactProjectYearCostsDto
    {
        // TRANSFORMENGINE: Grouping key columns from vpactprojectyearcosts
        /// <summary>Project code — DB column: project (varchar(20)).</summary>
        public string? Project { get; set; }

        /// <summary>
        /// Financial year — CASE-derived double precision value in view SELECT.
        /// Stored as double to match entity definition; cast to short at API/frontend layer when needed.
        /// </summary>
        public double Year { get; set; }

        /// <summary>Month number within the year — DB column: monthno (double precision in source).</summary>
        public double MonthNo { get; set; }

        // TRANSFORMENGINE: sum() aggregate columns → decimal? (monetary sums from view)
        /// <summary>Total subcontract costs — sum(subcontracts).</summary>
        public decimal? SubContracts { get; set; }

        /// <summary>Total animal costs — sum(animals). Maps to AnimalCosts in main yearly record.</summary>
        public decimal? Animals { get; set; }

        /// <summary>Total test/transfer costs — sum(transfercosts) aliased as "tests". Maps to TestCosts in main yearly record.</summary>
        public decimal? Tests { get; set; }

        /// <summary>Total pay costs — sum(vtcc_summary.pay). Maps to PayCosts in main yearly record.</summary>
        public decimal? Pay { get; set; }

        /// <summary>Total non-pay + overhead costs — sum(nonpay + overhead). Maps to NonPayOhCosts in main yearly record.</summary>
        public decimal? NonPayOH { get; set; }

        /// <summary>Total all-in costs — sum(totalcost). Used by btnFixCosting to set ActualExpenditure.</summary>
        public decimal? TotalCosts { get; set; }

        /// <summary>Total time costs — sum(timecosts). Display only.</summary>
        public decimal? TimeCost { get; set; }

        // TRANSFORMENGINE: hours column → double? (float/double precision in source view)
        /// <summary>Total hours — sum(totalhours). Maps to ManHours in main yearly record; ManDays/ManYears derived.</summary>
        public double? Hours { get; set; }

        // TRANSFORMENGINE: Optional joined lookup fields from MY_tlkpProject via qryProjectYearTotals_frm
        //   These are not in the base vpactprojectyearcosts view; populated from a secondary lookup when available
        /// <summary>Customer income from MY_tlkpProject. Display label: "Customer Income".</summary>
        public decimal? CustIncome { get; set; }

        /// <summary>VLA budget from MY_tlkpProject.Budget_CVL. Display label: "VLA Budget".</summary>
        public decimal? BudgetCvl { get; set; }
    }
}
