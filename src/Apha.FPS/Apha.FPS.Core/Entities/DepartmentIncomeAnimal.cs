/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeAnimal.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination (Steps 2-3)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New keyless entity created from qryDeptIncomeAnimals MS Access SELECT query
 *   - Maps projection columns: Project, OracleProjectCode, SubAccountCode, DefraProject,
 *     OPC, OCC, Month, SPC, SCC, AnimalType, AnimalDays, Rate, TotalCost
 *   - IIf([IsDefraProject],"Yes","No") → DefraProject string property
 *   - fnAnimalDesc([description]) → AnimalType string (VBA function result resolved by service/repo)
 *   - fnAnimalDays([description]) → AnimalDays decimal (VBA function result resolved by service/repo)
 *   - DLookUp("[DailyRate]","tblAnimals","[AnimalType]=...") → Rate decimal (EF join in repository)
 *   - "SSSD" AS SPC (literal constant in query) → SPC string
 *   - 35227 AS SCC (literal numeric constant in query) → SCC string (API consistency)
 *   - Proj_SubContract.Amount AS TotalCost
 *   - Marked for HasNoKey EF Core mapping (view / LINQ projection — no PK)
 *
 * PRESERVED:
 *   - All 13 output columns from qryDeptIncomeAnimals SELECT list
 *   - WHERE clause semantics: AcctCode IN ("LargeAnimals","SmallAnimals","Mice") enforced in repository
 *   - Nullable semantics: OCC/OPC nullable from LEFT JOIN (RIGHT JOIN) on CostCentre
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: SCC is literal 35227 in the query — repository must project this as string "35227"
 *   - TRANSFORMENGINE TODO: fnAnimalDesc and fnAnimalDays VBA functions must be re-implemented in DepartmentIncomeRepository
 *   - TRANSFORMENGINE TODO: DLookUp rate lookup must be replaced with EF Core join on Animals entity
 *   - TRANSFORMENGINE TODO: DepartmentIncomeAnimalMap.cs must call .HasNoKey() — confirmed keyless in Phase 4
 */

namespace Apha.FPS.Core.Entities
{
    // TRANSFORMENGINE: Keyless view entity — maps qryDeptIncomeAnimals projection for GET /api/v1/department-income/animals
    public class DepartmentIncomeAnimal
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
