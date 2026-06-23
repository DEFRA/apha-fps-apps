/*
 * TRANSFORMENGINE MIGRATION — AnimalRequestLogDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — frontend DTO mirroring Apha.FPS.Application.Dtos.AnimalRequestLogDto (backend Phase 3 artefact)
 *   - Namespace changed to Apha.FPSApps.Application.Dtos.FPS for frontend application layer consumption
 *   - All 9 properties copied verbatim to preserve exact name/type/nullability parity with backend DTO
 *
 * PRESERVED:
 *   - All property names, types, and nullability exactly matching backend AnimalRequestLogDto
 *   - JobCode and AnimalType as string = null! (NOT NULL in backend entity, matching DDL NOT NULL constraints)
 *   - NumberOfDays and NumberOfAnimals as double (NOT NULL in backend entity)
 *   - FpsYear as int (NOT NULL in backend entity, matching DDL NOT NULL constraint)
 *   - DateTime?, UserId?, InsertDelete? nullable as per entity definition
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: NumberOfDays and NumberOfAnimals (double) may need rounding at API boundary for display
 */
namespace Apha.FPSApps.Application.Dtos.FPS
{
    // TRANSFORMENGINE: Frontend DTO mirroring backend Apha.FPS.Application.Dtos.AnimalRequestLogDto
    // Same shape as backend DTO — all 9 columns from fps.animalreq_log audit trail table
    public class AnimalRequestLogDto
    {
        public int SequenceNo { get; set; }
        public string JobCode { get; set; } = null!;
        public string AnimalType { get; set; } = null!;
        public double NumberOfDays { get; set; }
        public double NumberOfAnimals { get; set; }
        public DateTime? DateTime { get; set; }
        public string? UserId { get; set; }
        public string? InsertDelete { get; set; }
        public int FpsYear { get; set; }
    }
}
