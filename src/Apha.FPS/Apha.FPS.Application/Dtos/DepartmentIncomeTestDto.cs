/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeTestDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New application-layer DTO created for the DepartmentIncomeTest entity
 *   - Mirrors all 14 properties of DepartmentIncomeTest entity for service-layer contracts
 *   - Mapped to/from DepartmentIncomeTest via EntityMapper CreateMap<DepartmentIncomeTest, DepartmentIncomeTestDto>().ReverseMap()
 *   - Mirrors DepartmentIncomeTestRes response contract shape for clean API handoff
 *
 * PRESERVED:
 *   - All 14 property names and types from DepartmentIncomeTest entity
 *   - Nullable semantics preserved: OPC/OCC nullable from LEFT JOIN; WorkGroup, TestCode, SPC, SCC nullable
 *   - Column order mirrors qryDeptIncomeTests: OPC before OCC (differs from Time query order)
 *
 * DEFERRED: none — fully automated.
 */

namespace Apha.FPS.Application.Dtos
{
    // TRANSFORMENGINE: Application DTO for qryDeptIncomeTests projection — service-layer contract for test-based income data
    public class DepartmentIncomeTestDto
    {
        // TRANSFORMENGINE: tlkpProject_MAP.ParentProject AS Project
        public string Project { get; set; } = null!;

        // TRANSFORMENGINE: tlkpProject_MAP.OracleProjectCode (nullable)
        public string? OracleProjectCode { get; set; }

        // TRANSFORMENGINE: tlkpProject_MAP.SubAccountCode (nullable)
        public string? SubAccountCode { get; set; }

        // TRANSFORMENGINE: IIf([IsDefraProject],"Yes","No") AS DefraProject
        public string? DefraProject { get; set; }

        // TRANSFORMENGINE: CostCentre.ProfitCentre AS OPC (note: OPC listed before OCC in this query — LEFT JOIN)
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
