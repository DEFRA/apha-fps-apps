/*
 * TRANSFORMENGINE MIGRATION — AccessSystemDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application DTO mirroring Apha.PIMS.Core.Entities.AccessSystem
 *   - Single integer PK (Systemid) — lookup/reference table; read-only list usage
 *
 * PRESERVED:
 *   - All field names consistent with entity naming convention
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

namespace Apha.PIMS.Application.Dtos
{
    // TRANSFORMENGINE: DTO maps to/from Apha.PIMS.Core.Entities.AccessSystem via EntityMapper; single integer PK (Systemid); lookup/reference
    public class AccessSystemDto
    {
        public int Systemid { get; set; }

        public string Systemname { get; set; } = null!;
    }
}
