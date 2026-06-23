/*
 * TRANSFORMENGINE MIGRATION — StaffJobLogDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — frontend DTO mirroring Apha.FPS.Application.Dtos.StaffJobLogDto (backend Phase 3 artefact)
 *   - Namespace changed to Apha.FPSApps.Application.Dtos.FPS for frontend application layer consumption
 *   - All 8 properties copied verbatim to preserve exact name/type/nullability parity with backend DTO
 *
 * PRESERVED:
 *   - All property names, types, and nullability exactly matching backend StaffJobLogDto
 *   - PlannedHours as double (NOT NULL in backend entity)
 *   - FpsYear as int (NOT NULL in backend entity, matching DDL NOT NULL constraint)
 *   - DateTime?, UserId?, InsertDelete? nullable as per entity definition
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: PlannedHours (double) may need rounding/formatting at API boundary for display
 */
namespace Apha.FPSApps.Application.Dtos.FPS
{
    // TRANSFORMENGINE: Frontend DTO mirroring backend Apha.FPS.Application.Dtos.StaffJobLogDto
    // Same shape as backend DTO — all 8 columns from fps.staffjob_log audit trail table
    public class StaffJobLogDto
    {
        public int SequenceNo { get; set; }
        public string StaffId { get; set; } = null!;
        public string JobCode { get; set; } = null!;
        public double PlannedHours { get; set; }
        public DateTime? DateTime { get; set; }
        public string? UserId { get; set; }
        public string? InsertDelete { get; set; }
        public int FpsYear { get; set; }
    }
}
