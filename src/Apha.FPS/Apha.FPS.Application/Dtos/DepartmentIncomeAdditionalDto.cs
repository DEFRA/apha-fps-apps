/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeAdditionalDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New application-layer DTO created for the DepartmentIncomeAdditional entity
 *   - Mirrors all 8 properties of DepartmentIncomeAdditional entity for service-layer contracts
 *   - Mapped to/from DepartmentIncomeAdditional via EntityMapper CreateMap<DepartmentIncomeAdditional, DepartmentIncomeAdditionalDto>().ReverseMap()
 *   - Mirrors DepartmentIncomeAdditionalRes response contract shape for clean API handoff
 *   - Named "Additional" per API route plan (source Access query is named "Exceptional")
 *
 * PRESERVED:
 *   - All 8 property names and types from DepartmentIncomeAdditional entity
 *   - Nullable semantics: OPC/OCC nullable from RIGHT JOIN; OracleProjectCode, SubAccountCode, DefraProject nullable
 *   - TotalCost is aggregated Sum(Proj_SubContract.Amount) per project+month group
 *
 * DEFERRED: none — fully automated.
 */

namespace Apha.FPS.Application.Dtos
{
    // TRANSFORMENGINE: Application DTO for qryDeptIncomeExceptional projection — service-layer contract for additional/exceptional income data
    public class DepartmentIncomeAdditionalDto
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
