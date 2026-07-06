/*
 * TRANSFORMENGINE MIGRATION — RadTrackProgDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend DTO mirroring Apha.PIMS.Application.Dtos.RadTrackProgDto (different namespace)
 *   - String PK (Program) — natural varchar(10) PK matching backend DTO
 *   - Radtrackprog boolean flag preserved
 *   - Publicationprefix nullable string preserved
 *
 * PRESERVED:
 *   - All property names exactly match backend DTO (case-sensitive)
 *   - Nullability matches backend DTO
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm Programme Tab maps solely to tblradtrackprog (see backend controller deferred note)
 */

namespace Apha.FPSApps.Application.Dtos.PIMS
{
    // TRANSFORMENGINE: mirrors Apha.PIMS.Application.Dtos.RadTrackProgDto; string PK (Program); Programme Tab
    public class RadTrackProgDto
    {
        public string Program { get; set; } = null!;

        // TRANSFORMENGINE: boolean flag — true if this programme is a RadTrack programme
        public bool Radtrackprog { get; set; }

        public string? Publicationprefix { get; set; }
    }
}
