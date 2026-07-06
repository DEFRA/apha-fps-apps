/*
 * TRANSFORMENGINE MIGRATION — SettingDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend DTO mirroring Apha.PIMS.Application.Dtos.SettingDto
 *   - Placed in Apha.FPSApps.Application.Dtos.PIMS namespace for frontend consumption
 *   - Natural varchar(50) PK (Id); Userupdateable guards edit access on Time Tab
 *
 * PRESERVED:
 *   - All property names match backend DTO exactly (case-sensitive)
 *   - Nullable reference types match backend DTO nullability
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Userupdateable flag must be respected in frontend controller/view — admin-only edit guard required
 */

namespace Apha.FPSApps.Application.Dtos.PIMS
{
    // TRANSFORMENGINE: Frontend DTO mirroring Apha.PIMS.Application.Dtos.SettingDto — natural varchar(50) PK (Id); read/update only
    public class SettingDto
    {
        public string Id { get; set; } = null!;
        public string? SettingValue { get; set; }
        public string? Notes { get; set; }
        public string? Testsetting { get; set; }
        public bool Userupdateable { get; set; }
    }
}
