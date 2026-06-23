/*
 * TRANSFORMENGINE MIGRATION — TestRequirementLogDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — frontend DTO mirroring Apha.FPS.Application.Dtos.TestRequirementLogDto (backend Phase 3 artefact)
 *   - Namespace changed to Apha.FPSApps.Application.Dtos.FPS for frontend application layer consumption
 *   - All 13 properties copied verbatim to preserve exact name/type/nullability parity with backend DTO
 *
 * PRESERVED:
 *   - All property names, types, and nullability exactly matching backend TestRequirementLogDto
 *   - UnitPrice as decimal? (preserving entity type; EF handles PostgreSQL double precision conversion)
 *   - NoRequired as double? (preserving entity type; widening from DDL integer)
 *   - FpsYear as int (NOT NULL in backend entity, matching DDL NOT NULL constraint)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: UnitPrice (decimal?) — entity has DDL double precision; verify no precision loss at service boundary
 *   - TRANSFORMENGINE TODO: NoRequired (double?) — entity has DDL integer; verify intent vs int? at API boundary
 *   - TRANSFORMENGINE TODO: JobCode (string?) — DDL-level generated column; verify computed column handling in EF map
 */
namespace Apha.FPSApps.Application.Dtos.FPS
{
    // TRANSFORMENGINE: Frontend DTO mirroring backend Apha.FPS.Application.Dtos.TestRequirementLogDto
    // Same shape as backend DTO — all 13 columns from fps.testreq_log audit trail table
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
