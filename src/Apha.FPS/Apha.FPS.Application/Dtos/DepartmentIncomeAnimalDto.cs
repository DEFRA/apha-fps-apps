/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeAnimalDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New application-layer DTO created for the DepartmentIncomeAnimal entity
 *   - Mirrors all 13 properties of DepartmentIncomeAnimal entity for service-layer contracts
 *   - Mapped to/from DepartmentIncomeAnimal via EntityMapper CreateMap<DepartmentIncomeAnimal, DepartmentIncomeAnimalDto>().ReverseMap()
 *   - Mirrors DepartmentIncomeAnimalRes response contract shape for clean API handoff
 *   - SPC and SCC kept as string types (SPC = "SSSD" literal, SCC = "35227" literal from Access query)
 *
 * PRESERVED:
 *   - All 13 property names and types from DepartmentIncomeAnimal entity
 *   - Nullable semantics: OPC/OCC nullable from LEFT/RIGHT JOIN; AnimalType nullable
 *   - SPC/SCC as nullable strings for API consistency with other query types
 *
 * DEFERRED: none — fully automated.
 */

namespace Apha.FPS.Application.Dtos
{
    // TRANSFORMENGINE: Application DTO for qryDeptIncomeAnimals projection — service-layer contract for animal-based income data
    public class DepartmentIncomeAnimalDto
    {
        // TRANSFORMENGINE: tlkpProject_MAP.ParentProject AS Project
        public string Project { get; set; } = null!;

        // TRANSFORMENGINE: tlkpProject_MAP.OracleProjectCode (nullable)
        public string? OracleProjectCode { get; set; }

        // TRANSFORMENGINE: tlkpProject_MAP.SubAccountCode (nullable)
        public string? SubAccountCode { get; set; }

        // TRANSFORMENGINE: IIf([IsDefraProject],"Yes","No") AS DefraProject
        public string? DefraProject { get; set; }

        // TRANSFORMENGINE: CostCentre.ProfitCentre AS OPC (nullable from LEFT/RIGHT JOIN on CostCentre)
        public string? OPC { get; set; }

        // TRANSFORMENGINE: CostCentre.CostCentre AS OCC (nullable from LEFT/RIGHT JOIN on CostCentre)
        public string? OCC { get; set; }

        // TRANSFORMENGINE: Proj_SubContract.Month
        public int Month { get; set; }

        // TRANSFORMENGINE: "SSSD" AS SPC — literal constant from Access query
        public string? SPC { get; set; }

        // TRANSFORMENGINE: 35227 AS SCC — literal numeric constant from Access query; stored as string for API consistency
        public string? SCC { get; set; }

        // TRANSFORMENGINE: fnAnimalDesc([description]) AS AnimalType — VBA function result, resolved by repository
        public string? AnimalType { get; set; }

        // TRANSFORMENGINE: fnAnimalDays([description]) AS AnimalDays — VBA function result, resolved by repository
        public decimal AnimalDays { get; set; }

        // TRANSFORMENGINE: DLookUp("[DailyRate]","tblAnimals","[AnimalType]=...") AS Rate — resolved via EF join in repository
        public decimal Rate { get; set; }

        // TRANSFORMENGINE: Proj_SubContract.Amount AS TotalCost
        public decimal TotalCost { get; set; }
    }
}
