/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeTimeRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New response contract created from qryDeptIncomeTime MS Access SELECT query
 *   - Field names mapped from SQL aliases: Project, OracleProjectCode, SubAccountCode, Month,
 *     DefraProject, OCC, OPC, SPC, SCC, Name, GradeCode, SpNumber, ChargeRate,
 *     Pay, NonPay, Overhead, Time, TotalCost
 *   - IIf([IsDefraProject],"Yes","No") → DefraProject as string
 *   - tblWGEmployeeMAB.SPNumber → SpNumber (camel-cased to .NET conventions)
 *   - TimeCostCalcsMAP.Cost AS TotalCost → TotalCost decimal
 *
 * PRESERVED:
 *   - All 18 output columns from qryDeptIncomeTime SELECT list
 *   - Nullable semantics: code fields that may be absent in left joins are nullable
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: verify decimal precision for ChargeRate, Pay, NonPay, Overhead, TotalCost matches DB column types
 *   - TRANSFORMENGINE TODO: Time field type — confirm double/decimal (VBA uses numeric time fractions)
 */

namespace Apha.Common.Contracts.FPS
{
    // TRANSFORMENGINE: Output surface for GET /api/v1/department-income/time — maps qryDeptIncomeTime columns
    public class DepartmentIncomeTimeRes
    {
        // TRANSFORMENGINE: tlkpProject_MAP.ParentProject AS Project
        public string Project { get; set; } = null!;

        // TRANSFORMENGINE: tlkpProject_MAP.OracleProjectCode
        public string? OracleProjectCode { get; set; }

        // TRANSFORMENGINE: tlkpProject_MAP.SubAccountCode
        public string? SubAccountCode { get; set; }

        // TRANSFORMENGINE: TimeCostCalcsMAP.Month
        public int Month { get; set; }

        // TRANSFORMENGINE: IIf([IsDefraProject],"Yes","No") AS DefraProject
        public string? DefraProject { get; set; }

        // TRANSFORMENGINE: CostCentre.CostCentre AS OCC (Owning Cost Centre)
        public string? OCC { get; set; }

        // TRANSFORMENGINE: CostCentre.ProfitCentre AS OPC (Owning Profit Centre)
        public string? OPC { get; set; }

        // TRANSFORMENGINE: WorkGroup_MAP.ProfitCentre AS SPC (Staff Profit Centre)
        public string? SPC { get; set; }

        // TRANSFORMENGINE: WorkGroup_MAP.CostCentre AS SCC (Staff Cost Centre)
        public string? SCC { get; set; }

        // TRANSFORMENGINE: TimeCostCalcsMAP.Name
        public string? Name { get; set; }

        // TRANSFORMENGINE: TimeCostCalcsMAP.GradeCode
        public string? GradeCode { get; set; }

        // TRANSFORMENGINE: tblWGEmployeeMAB.SPNumber — renamed SpNumber per .NET conventions
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
