/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeTestDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New frontend DTO mirroring backend Apha.Common.Contracts.FPS.DepartmentIncomeTestRes
 *   - Placed in Apha.FPSApps.Application.Dtos.DepartmentIncome namespace for frontend consumption
 *   - All 14 properties match backend DepartmentIncomeTestRes exactly (case-sensitive)
 *
 * PRESERVED:
 *   - Property names: Project, OracleProjectCode, SubAccountCode, DefraProject, OPC, OCC, Month,
 *     SPC, WorkGroup, SCC, TestCode, Volume, TestPrice, TotalCost
 *   - Column order matches qryDeptIncomeTests (OPC before OCC — differs from Time query)
 *   - Nullable semantics matching backend contract
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: ApiDtoMapper (FpsDepartmentIncomeApiDtoMapper) must map DepartmentIncomeTestRes → this DTO
 */

namespace Apha.FPSApps.Application.Dtos.DepartmentIncome
{
    // TRANSFORMENGINE: Frontend DTO — mirrors backend DepartmentIncomeTestRes for GET /api/v1/department-income/tests
    public class DepartmentIncomeTestDto
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
