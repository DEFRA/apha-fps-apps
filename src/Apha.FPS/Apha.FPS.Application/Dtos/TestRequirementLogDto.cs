/*
 * TRANSFORMENGINE MIGRATION — TestRequirementLogDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — DTO mirroring TestRequirementLog entity for service-layer contracts
 *   - All 13 fields from TestRequirementLog entity exposed as DTO properties
 *   - Used as output contract from IProjectAuditTrailService.GetTestRequirementLogsAsync
 *
 * PRESERVED:
 *   - All property names, types, and nullability exactly matching TestRequirementLog entity
 *   - UnitPrice as decimal? (preserving entity type; EF handles PostgreSQL double precision conversion)
 *   - NoRequired as double? (preserving entity type; widening from DDL integer)
 *   - FpsYear as int (NOT NULL in entity, matching DDL NOT NULL constraint)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: UnitPrice (decimal?) — entity has DDL double precision; verify no precision loss at service boundary
 *   - TRANSFORMENGINE TODO: NoRequired (double?) — entity has DDL integer; verify intent vs int? at API boundary
 *   - TRANSFORMENGINE TODO: JobCode (string?) — DDL-level generated column; verify computed column handling in EF map
 */
namespace Apha.FPS.Application.Dtos
{
    // TRANSFORMENGINE: DTO mirroring fps.testreq_log table — all 13 columns surfaced for Test Requirement audit trail tab
    public class TestRequirementLogDto
    {
        public int SequenceNo { get; set; }
        public string? TestCode { get; set; }
        public string? Buyer { get; set; }
        // TRANSFORMENGINE TODO: UnitPrice DDL type is double precision — decimal? maps via EF but verify no precision loss
        public decimal? UnitPrice { get; set; }
        // TRANSFORMENGINE TODO: NoRequired DDL type is integer — double? is widening, verify intent vs int?
        public double? NoRequired { get; set; }
        public string? ProjectBuyerCode { get; set; }
        public string? TestBuyerCode { get; set; }
        public short? Active { get; set; }
        public DateTime? DateTime { get; set; }
        public string? UserId { get; set; }
        public string? InsertDelete { get; set; }
        // TRANSFORMENGINE TODO: JobCode is a DDL-level generated column — verify EF map and display-only at API boundary
        public string? JobCode { get; set; }
        public int FpsYear { get; set; }
    }
}
