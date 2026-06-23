/*
 * TRANSFORMENGINE MIGRATION — AdditionalCostLogDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — frontend DTO mirroring Apha.FPS.Application.Dtos.AdditionalCostLogDto (backend Phase 3 artefact)
 *   - Namespace changed to Apha.FPSApps.Application.Dtos.FPS for frontend application layer consumption
 *   - All 11 properties copied verbatim to preserve exact name/type/nullability parity with backend DTO
 *
 * PRESERVED:
 *   - All property names, types, and nullability exactly matching backend AdditionalCostLogDto
 *   - JobCode, Account, Description as string = null! (NOT NULL in backend entity, matching DDL NOT NULL constraints)
 *   - ItemCost as decimal (NOT NULL in backend entity, matching DDL NOT NULL constraint)
 *   - Freq?, Supplier?, DateTime?, UserId?, InsertDelete? nullable as per entity definition
 *   - FpsYear as int (NOT NULL in backend entity, matching DDL NOT NULL constraint)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
 */
namespace Apha.FPSApps.Application.Dtos.FPS
{
    // TRANSFORMENGINE: Frontend DTO mirroring backend Apha.FPS.Application.Dtos.AdditionalCostLogDto
    // Same shape as backend DTO — all 11 columns from fps.additionalcosts_log audit trail table
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
