/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeTimeDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New frontend DTO mirroring backend Apha.Common.Contracts.FPS.DepartmentIncomeTimeRes
 *   - Placed in Apha.FPSApps.Application.Dtos.DepartmentIncome namespace for frontend consumption
 *   - All 18 properties match backend DepartmentIncomeTimeRes exactly (case-sensitive)
 *
 * PRESERVED:
 *   - Property names: Project, OracleProjectCode, SubAccountCode, Month, DefraProject, OCC, OPC,
 *     SPC, SCC, Name, GradeCode, SpNumber, ChargeRate, Pay, NonPay, Overhead, Time, TotalCost
 *   - Nullable semantics matching backend contract (nullable string? for optional fields)
 *   - decimal types for all cost/rate/time fields
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: ApiDtoMapper (FpsDepartmentIncomeApiDtoMapper) must map DepartmentIncomeTimeRes → this DTO
 */

namespace Apha.FPSApps.Application.Dtos.DepartmentIncome
{
    // TRANSFORMENGINE: Frontend DTO — mirrors backend DepartmentIncomeTimeRes for GET /api/v1/department-income/time
    public class DepartmentIncomeTimeDto
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
