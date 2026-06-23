/*
 * TRANSFORMENGINE MIGRATION — AnimalRequestLogDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — DTO mirroring AnimalRequestLog entity for service-layer contracts
 *   - All 9 fields from AnimalRequestLog entity exposed as DTO properties
 *   - Used as output contract from IProjectAuditTrailService.GetAnimalRequestLogsAsync
 *
 * PRESERVED:
 *   - All property names, types, and nullability exactly matching AnimalRequestLog entity
 *   - JobCode, AnimalType as string = null! (NOT NULL in entity, matching DDL NOT NULL constraints)
 *   - NumberOfDays and NumberOfAnimals as double (NOT NULL in entity)
 *   - FpsYear as int (NOT NULL in entity, matching DDL NOT NULL constraint)
 *   - DateTime?, UserId?, InsertDelete? nullable as per entity definition
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: NumberOfDays and NumberOfAnimals (double) may need rounding at API boundary for display
 */
namespace Apha.FPS.Application.Dtos
{
    // TRANSFORMENGINE: DTO mirroring fps.animalreq_log table — all 9 columns surfaced for Animal Requirement audit trail tab
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
