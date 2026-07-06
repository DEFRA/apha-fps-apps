/*
 * TRANSFORMENGINE MIGRATION — SettingDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application DTO mirroring Apha.PIMS.Core.Entities.Setting
 *   - String PK (Id) — pre-seeded configuration records, not user-created
 *   - Userupdateable boolean guards which settings users may edit
 *
 * PRESERVED:
 *   - All field names consistent with entity naming convention
 *   - Userupdateable flag preserved — service enforces this guard on update
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Testsetting field should only be editable in non-production environments — verify SettingService enforces this
 */

namespace Apha.PIMS.Application.Dtos
{
    // TRANSFORMENGINE: DTO maps to/from Apha.PIMS.Core.Entities.Setting via EntityMapper; string PK; Userupdateable flag enforced in service
    public class SettingDto
    {
        public string Id { get; set; } = null!;

        public string? SettingValue { get; set; }

        public string? Notes { get; set; }

        // TRANSFORMENGINE: Testsetting is environment-conditional — service guards edits to this field
        public string? Testsetting { get; set; }

        public bool Userupdateable { get; set; }
    }
}
