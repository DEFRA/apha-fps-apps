/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeTimeDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New application-layer DTO created for the DepartmentIncomeTime entity
 *   - Mirrors all 18 properties of DepartmentIncomeTime entity for service-layer contracts
 *   - Mapped to/from DepartmentIncomeTime via EntityMapper CreateMap<DepartmentIncomeTime, DepartmentIncomeTimeDto>().ReverseMap()
 *   - Mirrors DepartmentIncomeTimeRes response contract shape for clean API handoff
 *
 * PRESERVED:
 *   - All 18 property names and types from DepartmentIncomeTime entity
 *   - Nullable semantics: OCC, OPC, SPC, SCC, Name, GradeCode, SpNumber, DefraProject, OracleProjectCode, SubAccountCode are nullable
 *   - Project is non-nullable (required GROUP BY key)
 *
 * DEFERRED: none — fully automated.
 */

namespace Apha.FPS.Application.Dtos
{
    // TRANSFORMENGINE: Application DTO for qryDeptIncomeTime projection — service-layer contract for time-based income data
    public class DepartmentIncomeTimeDto
    {
        // TRANSFORMENGINE: tlkpProject_MAP.ParentProject AS Project
        public string Project { get; set; } = null!;

        // TRANSFORMENGINE: tlkpProject_MAP.OracleProjectCode (nullable — LEFT JOIN on CostCentre)
        public string? OracleProjectCode { get; set; }

        // TRANSFORMENGINE: tlkpProject_MAP.SubAccountCode (nullable)
        public string? SubAccountCode { get; set; }

        // TRANSFORMENGINE: TimeCostCalcsMAP.Month
        public int Month { get; set; }

        // TRANSFORMENGINE: IIf([IsDefraProject],"Yes","No") AS DefraProject
        public string? DefraProject { get; set; }

        // TRANSFORMENGINE: CostCentre.CostCentre AS OCC (Owning Cost Centre — nullable from LEFT JOIN)
        public string? OCC { get; set; }

        // TRANSFORMENGINE: CostCentre.ProfitCentre AS OPC (Owning Profit Centre — nullable from LEFT JOIN)
        public string? OPC { get; set; }

        // TRANSFORMENGINE: WorkGroup_MAP.ProfitCentre AS SPC (Staff Profit Centre)
        public string? SPC { get; set; }

        // TRANSFORMENGINE: WorkGroup_MAP.CostCentre AS SCC (Staff Cost Centre)
        public string? SCC { get; set; }

        // TRANSFORMENGINE: TimeCostCalcsMAP.Name
        public string? Name { get; set; }

        // TRANSFORMENGINE: TimeCostCalcsMAP.GradeCode
        public string? GradeCode { get; set; }

        // TRANSFORMENGINE: tblWGEmployeeMAB.SPNumber → SpNumber per .NET conventions
        public string? SpNumber { get; set; }

        // TRANSFORMENGINE: TimeCostCalcsMAP.ChargeRate
        public decimal ChargeRate { get; set; }

        // TRANSFORMENGINE: TimeCostCalcsMAP.Pay
        public decimal Pay { get; set; }

        // TRANSFORMENGINE: TimeCostCalcsMAP.NonPay
        public decimal NonPay { get; set; }

        // TRANSFORMENGINE: TimeCostCalcsMAP.Overhead
        public decimal Overhead { get; set; }

        // TRANSFORMENGINE: TimeCostCalcsMAP.Time
        public decimal Time { get; set; }

        // TRANSFORMENGINE: TimeCostCalcsMAP.Cost AS TotalCost
        public decimal TotalCost { get; set; }
    }
}
