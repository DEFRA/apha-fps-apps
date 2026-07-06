/*
 * TRANSFORMENGINE MIGRATION — RadTrackProgDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application DTO mirroring Apha.PIMS.Core.Entities.RadtrackProg
 *   - String PK (Program) — matches entity naming convention
 *   - Radtrackprog boolean flag preserved (tracks whether programme is a RadTrack programme)
 *   - Publicationprefix nullable string preserved
 *
 * PRESERVED:
 *   - All field names consistent with entity naming convention (lowercase-based properties)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

namespace Apha.PIMS.Application.Dtos
{
    // TRANSFORMENGINE: DTO maps to/from Apha.PIMS.Core.Entities.RadtrackProg via EntityMapper; string PK (Program); Programme Tab
    public class RadTrackProgDto
    {
        public string Program { get; set; } = null!;

        // TRANSFORMENGINE: boolean flag — true if this programme is a RadTrack programme
        public bool Radtrackprog { get; set; }

        public string? Publicationprefix { get; set; }
    }
}
