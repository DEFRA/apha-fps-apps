/*
 * TRANSFORMENGINE MIGRATION — ProjectLogDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — frontend DTO mirroring Apha.FPS.Application.Dtos.ProjectLogDto (backend Phase 3 artefact)
 *   - Namespace changed to Apha.FPSApps.Application.Dtos.FPS for frontend application layer consumption
 *   - All 41 properties copied verbatim to preserve exact name/type/nullability parity with backend DTO
 *
 * PRESERVED:
 *   - All property names, types, and nullability exactly matching backend ProjectLogDto
 *   - decimal fields preserved as decimal (no lossy conversion)
 *   - FpsYear as int (NOT NULL in backend entity, matching DDL NOT NULL constraint)
 *   - short? for Finished and IsDefraProject (pending backend decision on bool representation)
 *   - double? for CostCentre (pending backend decision on decimal representation)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: CostCentre (double?) — verify acceptable in API surface or should be decimal?
 *   - TRANSFORMENGINE TODO: Finished and IsDefraProject (short?) may need bool representation at API boundary
 */
namespace Apha.FPSApps.Application.Dtos.FPS
{
    // TRANSFORMENGINE: Frontend DTO mirroring backend Apha.FPS.Application.Dtos.ProjectLogDto
    // Same shape as backend DTO — all 41 columns from fps.project_log audit trail table
    public class ProjectLogDto
    {
        public int SequenceNo { get; set; }
        public string ParentProject { get; set; } = null!;
        public string ProjectTitle { get; set; } = null!;
        public string Program { get; set; } = null!;
        public string Customer { get; set; } = null!;
        public string? Manager { get; set; }
        public decimal TransferIncome { get; set; }
        public decimal CustIncome { get; set; }
        public decimal? WipEoy { get; set; }
        public decimal? WipLimit { get; set; }
        public decimal? WipCurrent { get; set; }
        public string ProjectStatus { get; set; } = null!;
        public string? CostBookNo { get; set; }
        public DateTime? DateCreated { get; set; }
        public decimal? FecCost { get; set; }
        public decimal? Profit { get; set; }
        public decimal? BudgetCvl { get; set; }
        public DateTime? DateCosted { get; set; }
        public string Disease { get; set; } = null!;
        public string Contract { get; set; } = null!;
        public string? ProjectParent { get; set; }
        public string? ShortTitle { get; set; }
        public decimal? CaseWorkSub { get; set; }
        public decimal? PvsIncome { get; set; }
        public decimal? PlanCaseWorkDebit { get; set; }
        public short? Finished { get; set; }
        public string? OwningRc { get; set; }
        public string? Comments { get; set; }
        public decimal? CarryOver { get; set; }
        public decimal? CarryOverSeed { get; set; }
        public DateTime? DateTime { get; set; }
        public string? UserId { get; set; }
        public string? InsertDelete { get; set; }
        public string JobCode { get; set; } = null!;
        public short? IsDefraProject { get; set; }
        public double? CostCentre { get; set; }
        public string? OracleProjectCode { get; set; }
        public string? SubAccountCode { get; set; }
        public string? ProjectGroup { get; set; }
        public string? IncomeAccountCode { get; set; }
        public int FpsYear { get; set; }
    }
}
