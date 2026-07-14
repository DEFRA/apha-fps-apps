/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeTime.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination (Steps 2-3)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New keyless entity created from qryDeptIncomeTime MS Access SELECT query
 *   - Maps projection columns: Project, OracleProjectCode, SubAccountCode, Month,
 *     DefraProject, OCC, OPC, SPC, SCC, Name, GradeCode, SpNumber,
 *     ChargeRate, Pay, NonPay, Overhead, Time, TotalCost
 *   - IIf([IsDefraProject],"Yes","No") → DefraProject string property
 *   - tblWGEmployeeMAB.SPNumber → SpNumber (camel-cased per .NET conventions)
 *   - TimeCostCalcsMAP.Cost AS TotalCost → TotalCost decimal
 *   - Marked for HasNoKey EF Core mapping (view / LINQ projection — no PK)
 *
 * PRESERVED:
 *   - All 18 output columns from qryDeptIncomeTime SELECT list
 *   - Nullable semantics: code fields from LEFT JOINs (OCC, OPC, etc.) are nullable
 *   - Column ordering mirrors qryDeptIncomeTime SELECT clause
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: verify decimal precision for ChargeRate, Pay, NonPay, Overhead, TotalCost matches DB column types
 *   - TRANSFORMENGINE TODO: Time field uses decimal; confirm double vs decimal for fractional time values
 *   - TRANSFORMENGINE TODO: DepartmentIncomeTimeMap.cs must call .ToView("...") or .HasNoKey() — confirmed keyless in Phase 4
 */

namespace Apha.FPS.Core.Entities
{
    // TRANSFORMENGINE: Keyless view entity — maps qryDeptIncomeTime projection for GET /api/v1/department-income/time
    public class DepartmentIncomeTime
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
