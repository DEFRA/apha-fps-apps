/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeTotalsRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New response contract created from qryDeptIncomeTotals MS Access TRANSFORM/PIVOT query
 *   - Source query uses PIVOT on qryDeptIncomeTotals_sub.Area IN ("Time","Tests","Animals","Project-specifics")
 *   - TRANSFORM SUM pivoted columns mapped to named properties:
 *     "Time" pivot column → TimeCost decimal
 *     "Tests" pivot column → TestsCost decimal
 *     "Animals" pivot column → AnimalsCost decimal
 *     "Project-specifics" pivot column → ProjectSpecificsCost decimal
 *   - Sum(TotalCost) AS TotalCosts = grand total across all areas per project
 *   - "Project-specifics" Access pivot column name → ProjectSpecificsCost (.NET-safe naming)
 *
 * PRESERVED:
 *   - All 7 output fields described in transform-plan: Project, OracleProjectCode, TotalCosts,
 *     TimeCost, TestsCost, AnimalsCost, ProjectSpecificsCost
 *   - Nullable cost columns: individual area totals are null when no data exists for that area
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: PIVOT query cannot be expressed as a single LINQ GroupBy — repository must use
 *     conditional Sum (GroupBy + Sum with filter per area) or raw SQL to replicate pivot behaviour
 *   - TRANSFORMENGINE TODO: confirm TotalCosts is the sum of all four area costs or the DB-computed grand total
 */

namespace Apha.Common.Contracts.FPS
{
    // TRANSFORMENGINE: Output surface for GET /api/v1/department-income/totals — maps qryDeptIncomeTotals PIVOT output
    public class DepartmentIncomeTotalsRes
    {
        // TRANSFORMENGINE: qryDeptIncomeTotals_sub.Project (GROUP BY key)
        public string Project { get; set; } = null!;

        // TRANSFORMENGINE: qryDeptIncomeTotals_sub.OracleProjectCode (GROUP BY key)
        public string? OracleProjectCode { get; set; }

        // TRANSFORMENGINE: Sum(TotalCost) AS TotalCosts — grand total across all areas for the project
        public decimal TotalCosts { get; set; }

        // TRANSFORMENGINE: PIVOT "Time" column — Sum of TotalCost where Area = "Time"; nullable when no time costs
        public decimal? TimeCost { get; set; }

        // TRANSFORMENGINE: PIVOT "Tests" column — Sum of TotalCost where Area = "Tests"; nullable when no test costs
        public decimal? TestsCost { get; set; }

        // TRANSFORMENGINE: PIVOT "Animals" column — Sum of TotalCost where Area = "Animals"; nullable when no animal costs
        public decimal? AnimalsCost { get; set; }

        // TRANSFORMENGINE: PIVOT "Project-specifics" column — Sum of TotalCost where Area = "Project-specifics"; nullable when absent
        public decimal? ProjectSpecificsCost { get; set; }
    }
}
