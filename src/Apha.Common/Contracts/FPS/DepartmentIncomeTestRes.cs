/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeTestRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New response contract created from qryDeptIncomeTests MS Access SELECT query
 *   - Field names mapped from SQL aliases: Project, OracleProjectCode, SubAccountCode, DefraProject,
 *     OPC, OCC, Month, SPC, WorkGroup, SCC, TestCode, Volume, TestPrice, TotalCost
 *   - IIf([IsDefraProject],"Yes","No") → DefraProject as string
 *   - [TestPrice]*[Volume] AS TotalCost → computed decimal in response (not computed here, comes from query)
 *
 * PRESERVED:
 *   - All 14 output columns from qryDeptIncomeTests SELECT list
 *   - Column order matches query: OPC before OCC (note: differs from Time query order)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: verify Volume type — Access MonthlyOutput.Volume may be integer or decimal
 *   - TRANSFORMENGINE TODO: verify TestPrice precision matches tblTestRequ_TM.TestPrice column type
 */

namespace Apha.Common.Contracts.FPS
{
    // TRANSFORMENGINE: Output surface for GET /api/v1/department-income/tests — maps qryDeptIncomeTests columns
    public class DepartmentIncomeTestRes
    {
        // TRANSFORMENGINE: tlkpProject_MAP.ParentProject AS Project
        public string Project { get; set; } = null!;

        // TRANSFORMENGINE: tlkpProject_MAP.OracleProjectCode
        public string? OracleProjectCode { get; set; }

        // TRANSFORMENGINE: tlkpProject_MAP.SubAccountCode
        public string? SubAccountCode { get; set; }

        // TRANSFORMENGINE: IIf([IsDefraProject],"Yes","No") AS DefraProject
        public string? DefraProject { get; set; }

        // TRANSFORMENGINE: CostCentre.ProfitCentre AS OPC (note: OPC listed before OCC in this query)
        public string? OPC { get; set; }

        // TRANSFORMENGINE: CostCentre.CostCentre AS OCC
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

        // TRANSFORMENGINE: [TestPrice]*[Volume] AS TotalCost — computed by query, returned as flat value
        public decimal TotalCost { get; set; }
    }
}
