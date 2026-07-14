/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeTotals.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination (Steps 2-3)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New keyless entity created from qryDeptIncomeTotals MS Access TRANSFORM/PIVOT query
 *   - Source PIVOT query unions Time/Tests/Animals/Project-specifics from qryDeptIncomeTotals_sub
 *   - TRANSFORM SUM pivot columns mapped to named properties:
 *       "Time" pivot column              → TimeCost decimal?
 *       "Tests" pivot column             → TestsCost decimal?
 *       "Animals" pivot column           → AnimalsCost decimal?
 *       "Project-specifics" pivot column → ProjectSpecificsCost decimal?
 *   - Sum(TotalCost) AS TotalCosts = grand total across all areas per project row
 *   - GROUP BY keys: Project, OracleProjectCode → entity identity fields
 *   - Marked for HasNoKey EF Core mapping (LINQ aggregation projection — no PK)
 *
 * PRESERVED:
 *   - All 7 output fields described in transform-plan and DepartmentIncomeTotalsRes
 *   - Nullable cost columns: individual area totals are null when no data exists for that area
 *   - Column ordering: Project, OracleProjectCode, TotalCosts, TimeCost, TestsCost, AnimalsCost, ProjectSpecificsCost
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: PIVOT cannot be expressed as a single LINQ GroupBy — repository must use
 *     conditional Sum (GroupBy + Sum with filter per area) or raw SQL to replicate pivot behaviour
 *   - TRANSFORMENGINE TODO: confirm TotalCosts is sum of all four area costs or the DB-computed grand total
 *   - TRANSFORMENGINE TODO: DepartmentIncomeTotalsMap.cs must call .HasNoKey() — confirmed keyless in Phase 4
 */

namespace Apha.FPS.Core.Entities
{
    // TRANSFORMENGINE: Keyless aggregation entity — maps qryDeptIncomeTotals PIVOT output for GET /api/v1/department-income/totals
    public class DepartmentIncomeTotals
    {
        // TRANSFORMENGINE: qryDeptIncomeTotals_sub.Project (GROUP BY key)
        public string Project { get; set; } = null!;

        // TRANSFORMENGINE: qryDeptIncomeTotals_sub.OracleProjectCode (GROUP BY key, nullable)
        public string? OracleProjectCode { get; set; }

        // TRANSFORMENGINE: Sum(TotalCost) AS TotalCosts — grand total across all areas for the project
        public decimal TotalCosts { get; set; }

        // TRANSFORMENGINE: PIVOT "Time" column — Sum of TotalCost where Area = "Time"; nullable when no time data
        public decimal? TimeCost { get; set; }

        // TRANSFORMENGINE: PIVOT "Tests" column — Sum of TotalCost where Area = "Tests"; nullable when no test data
        public decimal? TestsCost { get; set; }

        // TRANSFORMENGINE: PIVOT "Animals" column — Sum of TotalCost where Area = "Animals"; nullable when no animal data
        public decimal? AnimalsCost { get; set; }

        // TRANSFORMENGINE: PIVOT "Project-specifics" column — Sum of TotalCost where Area = "Project-specifics"; nullable when absent
        public decimal? ProjectSpecificsCost { get; set; }
    }
}
