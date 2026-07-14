/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeTest.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination (Steps 2-3)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New keyless entity created from qryDeptIncomeTests MS Access SELECT query
 *   - Maps projection columns: Project, OracleProjectCode, SubAccountCode, DefraProject,
 *     OPC, OCC, Month, SPC, WorkGroup, SCC, TestCode, Volume, TestPrice, TotalCost
 *   - IIf([IsDefraProject],"Yes","No") → DefraProject string property
 *   - [TestPrice]*[Volume] AS TotalCost → TotalCost decimal (computed value from query)
 *   - Marked for HasNoKey EF Core mapping (view / LINQ projection — no PK)
 *
 * PRESERVED:
 *   - All 14 output columns from qryDeptIncomeTests SELECT list
 *   - Column ordering matches qryDeptIncomeTests: OPC appears before OCC (differs from Time query)
 *   - Nullable semantics: OCC/OPC nullable from LEFT JOIN on CostCentre
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: verify Volume type — Access MonthlyOutput.Volume may be integer or decimal
 *   - TRANSFORMENGINE TODO: verify TestPrice precision matches tblTestRequ_TM.TestPrice column type
 *   - TRANSFORMENGINE TODO: DepartmentIncomeTestMap.cs must call .HasNoKey() — confirmed keyless in Phase 4
 */

namespace Apha.FPS.Core.Entities
{
    // TRANSFORMENGINE: Keyless view entity — maps qryDeptIncomeTests projection for GET /api/v1/department-income/tests
    public class DepartmentIncomeTest
    {
        // TRANSFORMENGINE: tlkpProject_MAP.ParentProject AS Project
        public string Project { get; set; } = null!;

        // TRANSFORMENGINE: tlkpProject_MAP.OracleProjectCode (nullable)
        public string? OracleProjectCode { get; set; }

        // TRANSFORMENGINE: tlkpProject_MAP.SubAccountCode (nullable)
        public string? SubAccountCode { get; set; }

        // TRANSFORMENGINE: IIf([IsDefraProject],"Yes","No") AS DefraProject
        public string? DefraProject { get; set; }

        // TRANSFORMENGINE: CostCentre.ProfitCentre AS OPC (note: OPC before OCC in this query — LEFT JOIN)
        public string? OPC { get; set; }

        // TRANSFORMENGINE: CostCentre.CostCentre AS OCC (nullable from LEFT JOIN)
        public string? OCC { get; set; }

        // TRANSFORMENGINE: MonthlyOutput.Month
        public int Month { get; set; }

        // TRANSFORMENGINE: WorkGroup_MAP.ProfitCentre AS SPC
        public string? SPC { get; set; }

        // TRANSFORMENGINE: MonthlyOutput.WorkGroup
        public string? WorkGroup { get; set; }

        // TRANSFORMENGINE: WorkGroup_MAP.CostCentre AS SCC
        public string? SCC { get; set; }

        // TRANSFORMENGINE: MonthlyOutput.TestCode
        public string? TestCode { get; set; }

        // TRANSFORMENGINE: MonthlyOutput.Volume
        public decimal Volume { get; set; }

        // TRANSFORMENGINE: tblTestRequ_TM.TestPrice
        public decimal TestPrice { get; set; }

        // TRANSFORMENGINE: [TestPrice]*[Volume] AS TotalCost — computed by query, stored as flat value
        public decimal TotalCost { get; set; }
    }
}
