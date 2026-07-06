/*
 * TRANSFORMENGINE MIGRATION — AccessLevelDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application DTO mirroring Apha.PIMS.Core.Entities.AccessLevel
 *   - Composite PK (Systemid, Accesslevelid) carried in DTO for Add/Update/Delete
 *
 * PRESERVED:
 *   - All field names consistent with entity naming convention
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

namespace Apha.PIMS.Application.Dtos
{
    // TRANSFORMENGINE: DTO maps to/from Apha.PIMS.Core.Entities.AccessLevel via EntityMapper; composite PK (Systemid, Accesslevelid)
    public class AccessLevelDto
    {
        public int Systemid { get; set; }

        public int Accesslevelid { get; set; }

        public string? Accesslevel { get; set; }
    }
}
