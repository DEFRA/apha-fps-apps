/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeAdditionalRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New response contract created from qryDeptIncomeExceptional MS Access SELECT/GROUP BY query
 *   - Field names mapped from SQL aliases: Project, OracleProjectCode, SubAccountCode, DefraProject,
 *     OPC, OCC, Month, TotalCost
 *   - IIf([IsDefraProject],"Yes","No") → DefraProject as string
 *   - Sum(Proj_SubContract.Amount) AS TotalCost → TotalCost decimal (aggregated per project/month/group)
 *   - Named "Additional" in API route and contract per plan (source query is named "Exceptional")
 *
 * PRESERVED:
 *   - All 8 output columns from qryDeptIncomeExceptional GROUP BY SELECT list
 *   - Aggregated TotalCost semantics (Sum of exceptional/project-specific costs)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: "Additional" vs "Exceptional" naming — confirm API consumers use /additional route
 *   - TRANSFORMENGINE TODO: WHERE clause excludes LargeAnimals/SmallAnimals/Mice — must be enforced in repository filter
 */

namespace Apha.Common.Contracts.FPS
{
    // TRANSFORMENGINE: Output surface for GET /api/v1/department-income/additional — maps qryDeptIncomeExceptional columns
    public class DepartmentIncomeAdditionalRes
    {
        // TRANSFORMENGINE: tlkpProject_MAP.ParentProject AS Project
        public string Project { get; set; } = null!;

        // TRANSFORMENGINE: tlkpProject_MAP.OracleProjectCode
        public string? OracleProjectCode { get; set; }

        // TRANSFORMENGINE: tlkpProject_MAP.SubAccountCode
        public string? SubAccountCode { get; set; }

        // TRANSFORMENGINE: IIf([IsDefraProject],"Yes","No") AS DefraProject
        public string? DefraProject { get; set; }

        // TRANSFORMENGINE: CostCentre.ProfitCentre AS OPC
        public string? OPC { get; set; }

        // TRANSFORMENGINE: CostCentre.CostCentre AS OCC
        public string? OCC { get; set; }

        // TRANSFORMENGINE: Proj_SubContract.Month
        public int Month { get; set; }

        // TRANSFORMENGINE: Sum(Proj_SubContract.Amount) AS TotalCost — aggregated exceptional/additional costs
        public decimal TotalCost { get; set; }
    }
}
