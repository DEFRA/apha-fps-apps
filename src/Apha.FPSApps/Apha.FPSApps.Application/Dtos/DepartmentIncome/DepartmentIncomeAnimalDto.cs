/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeAnimalDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New frontend DTO mirroring backend Apha.Common.Contracts.FPS.DepartmentIncomeAnimalRes
 *   - Placed in Apha.FPSApps.Application.Dtos.DepartmentIncome namespace for frontend consumption
 *   - All 13 properties match backend DepartmentIncomeAnimalRes exactly (case-sensitive)
 *
 * PRESERVED:
 *   - Property names: Project, OracleProjectCode, SubAccountCode, DefraProject, OPC, OCC, Month,
 *     SPC, SCC, AnimalType, AnimalDays, Rate, TotalCost
 *   - SPC and SCC preserved as string? for consistency with backend contract
 *   - Nullable semantics matching backend contract
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: ApiDtoMapper (FpsDepartmentIncomeApiDtoMapper) must map DepartmentIncomeAnimalRes → this DTO
 */

namespace Apha.FPSApps.Application.Dtos.DepartmentIncome
{
    // TRANSFORMENGINE: Frontend DTO — mirrors backend DepartmentIncomeAnimalRes for GET /api/v1/department-income/animals
    public class DepartmentIncomeAnimalDto
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

        // TRANSFORMENGINE: "SSSD" AS SPC — literal constant from Access query
        public string? SPC { get; set; }

        // TRANSFORMENGINE: 35227 AS SCC — literal numeric constant from Access query; returned as string for API consistency
        public string? SCC { get; set; }

        // TRANSFORMENGINE: fnAnimalDesc([description]) AS AnimalType — VBA function result
        public string? AnimalType { get; set; }

        // TRANSFORMENGINE: fnAnimalDays([description]) AS AnimalDays — VBA function result
        public decimal AnimalDays { get; set; }

        // TRANSFORMENGINE: DLookUp("[DailyRate]",...) AS Rate — resolved via EF join on tblAnimals in repository
        public decimal Rate { get; set; }

        // TRANSFORMENGINE: Proj_SubContract.Amount AS TotalCost
        public decimal TotalCost { get; set; }
    }
}
