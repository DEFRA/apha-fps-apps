/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeTotalsDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New application-layer DTO created for the DepartmentIncomeTotals entity
 *   - Mirrors all 7 properties of DepartmentIncomeTotals entity for service-layer contracts
 *   - Mapped to/from DepartmentIncomeTotals via EntityMapper CreateMap<DepartmentIncomeTotals, DepartmentIncomeTotalsDto>().ReverseMap()
 *   - Mirrors DepartmentIncomeTotalsRes response contract shape for clean API handoff
 *   - PIVOT columns (Time, Tests, Animals, Project-specifics) → nullable decimal properties
 *
 * PRESERVED:
 *   - All 7 property names and types from DepartmentIncomeTotals entity
 *   - Nullable semantics for individual area cost columns: null when no data for that area
 *   - TotalCosts is non-nullable grand total across all areas
 *
 * DEFERRED: none — fully automated.
 */

namespace Apha.FPS.Application.Dtos
{
    // TRANSFORMENGINE: Application DTO for qryDeptIncomeTotals PIVOT output — service-layer contract for per-project totals
    public class DepartmentIncomeTotalsDto
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
