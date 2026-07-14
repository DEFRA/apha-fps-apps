/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeAdditional.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination (Steps 2-3)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New keyless entity created from qryDeptIncomeExceptional MS Access SELECT/GROUP BY query
 *   - Named "Additional" per API route plan (source Access query is named "Exceptional")
 *   - Maps projection columns: Project, OracleProjectCode, SubAccountCode, DefraProject,
 *     OPC, OCC, Month, TotalCost
 *   - IIf([IsDefraProject],"Yes","No") → DefraProject string property
 *   - Sum(Proj_SubContract.Amount) AS TotalCost → TotalCost decimal (aggregated per project/month)
 *   - Marked for HasNoKey EF Core mapping (view / LINQ projection — no PK)
 *
 * PRESERVED:
 *   - All 8 output columns from qryDeptIncomeExceptional GROUP BY SELECT list
 *   - WHERE filter semantics: AcctCode NOT IN ("LargeAnimals","SmallAnimals","Mice") enforced in repository
 *   - Aggregated TotalCost semantics (Sum of exceptional / project-specific subcontract costs)
 *   - Nullable semantics: OCC/OPC nullable from RIGHT JOIN on CostCentre
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: "Additional" vs "Exceptional" naming — confirm API consumers use /additional route
 *   - TRANSFORMENGINE TODO: NOT IN ("LargeAnimals","SmallAnimals","Mice") filter must be enforced in repository LINQ query
 *   - TRANSFORMENGINE TODO: DepartmentIncomeAdditionalMap.cs must call .HasNoKey() — confirmed keyless in Phase 4
 */

namespace Apha.FPS.Core.Entities
{
    // TRANSFORMENGINE: Keyless view entity — maps qryDeptIncomeExceptional projection for GET /api/v1/department-income/additional
    public class DepartmentIncomeAdditional
    {
        // TRANSFORMENGINE: tlkpProject_MAP.ParentProject AS Project
        public string Project { get; set; } = null!;

        // TRANSFORMENGINE: tlkpProject_MAP.OracleProjectCode (nullable)
        public string? OracleProjectCode { get; set; }

        // TRANSFORMENGINE: tlkpProject_MAP.SubAccountCode (nullable)
        public string? SubAccountCode { get; set; }

        // TRANSFORMENGINE: IIf([IsDefraProject],"Yes","No") AS DefraProject
        public string? DefraProject { get; set; }

        // TRANSFORMENGINE: CostCentre.ProfitCentre AS OPC (nullable from RIGHT JOIN on CostCentre)
        public string? OPC { get; set; }

        // TRANSFORMENGINE: CostCentre.CostCentre AS OCC (nullable from RIGHT JOIN on CostCentre)
        public string? OCC { get; set; }

        // TRANSFORMENGINE: Proj_SubContract.Month
        public int Month { get; set; }

        // TRANSFORMENGINE: Sum(Proj_SubContract.Amount) AS TotalCost — aggregated exceptional/additional costs per project+month group
        public decimal TotalCost { get; set; }
    }
}
