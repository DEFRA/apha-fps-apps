/*
 * TRANSFORMENGINE MIGRATION — PactProjectYearCosts.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination (Steps 2-3)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - New file: no prior C# entity existed for this view
 *   - Keyless projection entity derived from PostgreSQL view:
 *     mabarchive.vpactprojectyearcosts
 *   - Source view aggregates my_projectmonthfinal joined to g_tlkpproject_radtrackdata
 *     and vtcc_summary; groups by project, year (derived), monthno
 *   - All sum() aggregate columns (subcontracts, animals, tests, pay, nonpayoh,
 *     totalcosts, timecost) mapped to decimal? — monetary totals
 *   - Hours column (sum(totalhours)) mapped to double? — float precision source
 *   - MonthNo included to support per-month granularity (view includes monthno
 *     in GROUP BY; year-level aggregation is done in the repository/service layer)
 *   - Marked HasNoKey in EF configuration (PactProjectYearCostsMap.cs) — read-only view
 *
 * PRESERVED:
 *   - Column aliases from view DDL (subcontracts, animals, tests, pay, nonpayoh,
 *     totalcosts, timecost, hours) normalised to PascalCase
 *   - Year is a CASE-derived double in the view; stored as double here and cast
 *     to short in the repository where needed
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: EF map must call .HasNoKey() and .ToView("vpactprojectyearcosts", "mabarchive")
 *   - TRANSFORMENGINE TODO: Confirm whether MonthNo should be exposed in the API response
 *     or suppressed at the service layer — see PactProjectYearCostsRes.cs note
 */

namespace Apha.PIMS.Core.Entities
{
    /// <summary>
    /// Keyless projection entity representing an aggregated row from the
    /// <c>mabarchive.vpactprojectyearcosts</c> PostgreSQL view.
    /// Groups actuals by project, year, and month from <c>my_projectmonthfinal</c>,
    /// joined to <c>g_tlkpproject_radtrackdata</c> and <c>vtcc_summary</c>.
    /// This entity must be registered with <c>HasNoKey()</c> in the EF configuration.
    /// </summary>
    public class PactProjectYearCosts
    {
        // TRANSFORMENGINE: Grouping key columns from vpactprojectyearcosts
        /// <summary>Project code — DB column: project (varchar(20)).</summary>
        public string Project { get; set; } = null!;

        /// <summary>
        /// Financial year — derived in view as a CASE expression on g_tlkpproject_radtrackdata.useprojectyear
        /// (double precision in the view SELECT; typically treated as a year integer).
        /// </summary>
        public double Year { get; set; }

        /// <summary>Month number within the year — DB column: monthno (double precision in source).</summary>
        public double MonthNo { get; set; }

        // TRANSFORMENGINE: sum() aggregate columns → decimal? (monetary sums from view)
        /// <summary>Total subcontract costs for the month — sum(subcontracts).</summary>
        public decimal? SubContracts { get; set; }

        /// <summary>Total animal costs for the month — sum(animals).</summary>
        public decimal? Animals { get; set; }

        /// <summary>Total test/transfer costs for the month — sum(transfercosts) aliased as "tests".</summary>
        public decimal? Tests { get; set; }

        /// <summary>Total pay costs for the month — sum(vtcc_summary.pay).</summary>
        public decimal? Pay { get; set; }

        /// <summary>Total non-pay + overhead costs for the month — sum(nonpay + overhead).</summary>
        public decimal? NonPayOH { get; set; }

        /// <summary>Total all-in costs for the month — sum(totalcost).</summary>
        public decimal? TotalCosts { get; set; }

        /// <summary>Total time costs for the month — sum(timecosts).</summary>
        public decimal? TimeCost { get; set; }

        // TRANSFORMENGINE: hours column → double? (float/double precision in source view)
        /// <summary>Total hours for the month — sum(totalhours).</summary>
        public double? Hours { get; set; }
    }
}
