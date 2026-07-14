/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeAdditionalDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New frontend DTO mirroring backend Apha.Common.Contracts.FPS.DepartmentIncomeAdditionalRes
 *   - Placed in Apha.FPSApps.Application.Dtos.DepartmentIncome namespace for frontend consumption
 *   - All 8 properties match backend DepartmentIncomeAdditionalRes exactly (case-sensitive)
 *
 * PRESERVED:
 *   - Property names: Project, OracleProjectCode, SubAccountCode, DefraProject, OPC, OCC, Month, TotalCost
 *   - TotalCost is aggregated (Sum of exceptional/additional costs per project/month/group)
 *   - Nullable semantics matching backend contract
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: ApiDtoMapper (FpsDepartmentIncomeApiDtoMapper) must map DepartmentIncomeAdditionalRes → this DTO
 */

namespace Apha.FPSApps.Application.Dtos.DepartmentIncome
{
    // TRANSFORMENGINE: Frontend DTO — mirrors backend DepartmentIncomeAdditionalRes for GET /api/v1/department-income/additional
    public class DepartmentIncomeAdditionalDto
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
