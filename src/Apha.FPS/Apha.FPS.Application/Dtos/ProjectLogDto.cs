/*
 * TRANSFORMENGINE MIGRATION — ProjectLogDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — DTO mirroring ProjectLog entity for service-layer contracts
 *   - All 41 fields from ProjectLog entity exposed as DTO properties for API surface
 *   - Used as input/output contract between service layer and API controller
 *
 * PRESERVED:
 *   - All property names, types, and nullability exactly matching ProjectLog entity
 *   - decimal fields preserved as decimal (no lossy conversion)
 *   - FpsYear as int (NOT NULL in entity, matching DDL NOT NULL constraint)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify that CostCentre (double?) is acceptable in DTO surface or should be decimal?
 *   - TRANSFORMENGINE TODO: Finished and IsDefraProject (short?) may need bool representation at API boundary
 */
namespace Apha.FPS.Application.Dtos
{
    // TRANSFORMENGINE: DTO mirroring fps.project_log table — all 41 columns surfaced for audit trail display
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
