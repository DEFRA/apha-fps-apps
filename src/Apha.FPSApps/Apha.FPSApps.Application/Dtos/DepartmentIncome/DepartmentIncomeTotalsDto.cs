/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeTotalsDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New frontend DTO mirroring backend Apha.Common.Contracts.FPS.DepartmentIncomeTotalsRes
 *   - Placed in Apha.FPSApps.Application.Dtos.DepartmentIncome namespace for frontend consumption
 *   - All 7 properties match backend DepartmentIncomeTotalsRes exactly (case-sensitive)
 *
 * PRESERVED:
 *   - Property names: Project, OracleProjectCode, TotalCosts, TimeCost, TestsCost, AnimalsCost, ProjectSpecificsCost
 *   - Nullable decimal? for individual area cost columns (null when no data for that area)
 *   - TotalCosts non-nullable (grand total always present)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: ApiDtoMapper (FpsDepartmentIncomeApiDtoMapper) must map DepartmentIncomeTotalsRes → this DTO
 */

namespace Apha.FPSApps.Application.Dtos.DepartmentIncome
{
    // TRANSFORMENGINE: Frontend DTO — mirrors backend DepartmentIncomeTotalsRes for GET /api/v1/department-income/totals
    public class DepartmentIncomeTotalsDto
    {
        // TRANSFORMENGINE: qryDeptIncomeTotals_sub.Project (GROUP BY key)
        public string Project { get; set; } = null!;

        // TRANSFORMENGINE: qryDeptIncomeTotals_sub.OracleProjectCode (GROUP BY key)
        public string? OracleProjectCode { get; set; }

        // TRANSFORMENGINE: Sum(TotalCost) AS TotalCosts — grand total across all areas for the project
        public decimal TotalCosts { get; set; }

        // TRANSFORMENGINE: PIVOT "Time" column — Sum of TotalCost where Area = "Time"; nullable when no time costs
        public decimal? TimeCost { get; set; }

        // TRANSFORMENGINE: PIVOT "Tests" column — Sum of TotalCost where Area = "Tests"; nullable when no test costs
        public decimal? TestsCost { get; set; }

        // TRANSFORMENGINE: PIVOT "Animals" column — Sum of TotalCost where Area = "Animals"; nullable when no animal costs
        public decimal? AnimalsCost { get; set; }

        // TRANSFORMENGINE: PIVOT "Project-specifics" column — Sum of TotalCost where Area = "Project-specifics"; nullable when absent
        public decimal? ProjectSpecificsCost { get; set; }
    }
}
