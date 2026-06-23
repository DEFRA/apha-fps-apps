/*
 * TRANSFORMENGINE MIGRATION — StaffJobLogDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — DTO mirroring StaffJobLog entity for service-layer contracts
 *   - All 8 fields from StaffJobLog entity exposed as DTO properties
 *   - Used as output contract from IProjectAuditTrailService.GetStaffJobLogsAsync
 *
 * PRESERVED:
 *   - All property names, types, and nullability exactly matching StaffJobLog entity
 *   - FpsYear as int (NOT NULL in entity, matching DDL NOT NULL constraint)
 *   - DateTime?, UserId?, InsertDelete? nullable as per entity definition
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: PlannedHours (double) may need rounding/formatting at API boundary for display
 */
namespace Apha.FPS.Application.Dtos
{
    // TRANSFORMENGINE: DTO mirroring fps.staffjob_log table — all 8 columns surfaced for Staff Plan audit trail tab
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
