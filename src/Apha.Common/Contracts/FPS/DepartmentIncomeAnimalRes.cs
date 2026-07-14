/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeAnimalRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New response contract created from qryDeptIncomeAnimals MS Access SELECT query
 *   - Field names mapped from SQL aliases: Project, OracleProjectCode, SubAccountCode, DefraProject,
 *     OPC, OCC, Month, SPC, SCC, AnimalType, AnimalDays, Rate, TotalCost
 *   - IIf([IsDefraProject],"Yes","No") → DefraProject as string
 *   - fnAnimalDesc([description]) → AnimalType string (VBA function result, returned by service/repo)
 *   - fnAnimalDays([description]) → AnimalDays decimal (VBA function result, returned by service/repo)
 *   - DLookUp("[DailyRate]","tblAnimals","[AnimalType]=...") → Rate decimal
 *   - "SSSD" AS SPC (literal constant from query) → SPC string
 *   - 35227 AS SCC (literal numeric constant from query) → SCC string (kept as string for API consistency)
 *   - Proj_SubContract.Amount AS TotalCost
 *
 * PRESERVED:
 *   - All 13 output columns from qryDeptIncomeAnimals SELECT list
 *   - SPC and SCC preserved as strings for consistency with other response types
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: SCC is a literal 35227 in the query — repository must project this as string "35227"
 *   - TRANSFORMENGINE TODO: fnAnimalDesc and fnAnimalDays VBA functions must be re-implemented in DepartmentIncomeRepository
 *   - TRANSFORMENGINE TODO: DLookUp rate lookup must be replaced with EF Core join on tblAnimals/Animals entity
 */

namespace Apha.Common.Contracts.FPS
{
    // TRANSFORMENGINE: Output surface for GET /api/v1/department-income/animals — maps qryDeptIncomeAnimals columns
    public class DepartmentIncomeAnimalRes
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

        // TRANSFORMENGINE: DLookUp("[DailyRate]",...) AS Rate — must be resolved via EF join in repository
        public decimal Rate { get; set; }

        // TRANSFORMENGINE: Proj_SubContract.Amount AS TotalCost
        public decimal TotalCost { get; set; }
    }
}
