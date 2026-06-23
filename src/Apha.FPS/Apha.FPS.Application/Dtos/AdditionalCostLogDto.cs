/*
 * TRANSFORMENGINE MIGRATION — AdditionalCostLogDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — DTO mirroring AdditionalCostLog entity for service-layer contracts
 *   - All 11 fields from AdditionalCostLog entity exposed as DTO properties
 *   - Used as output contract from IProjectAuditTrailService.GetAdditionalCostLogsAsync
 *
 * PRESERVED:
 *   - All property names, types, and nullability exactly matching AdditionalCostLog entity
 *   - JobCode, Account, Description as string = null! (NOT NULL in entity, matching DDL NOT NULL constraints)
 *   - ItemCost as decimal (NOT NULL in entity, matching DDL NOT NULL constraint)
 *   - Freq?, Supplier?, DateTime?, UserId?, InsertDelete? nullable as per entity definition
 *   - FpsYear as int (NOT NULL in entity, matching DDL NOT NULL constraint)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated
 */
namespace Apha.FPS.Application.Dtos
{
    // TRANSFORMENGINE: DTO mirroring fps.additionalcosts_log table — all 11 columns surfaced for Exceptional Cost audit trail tab
    public class AdditionalCostLogDto
    {
        public int SequenceNo { get; set; }
        public string JobCode { get; set; } = null!;
        public string Account { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal ItemCost { get; set; }
        public string? Freq { get; set; }
        public string? Supplier { get; set; }
        public DateTime? DateTime { get; set; }
        public string? UserId { get; set; }
        public string? InsertDelete { get; set; }
        public int FpsYear { get; set; }
    }
}
