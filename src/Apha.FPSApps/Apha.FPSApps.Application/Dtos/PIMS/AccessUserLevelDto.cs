/*
 * TRANSFORMENGINE MIGRATION — AccessUserLevelDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend DTO mirroring Apha.PIMS.Application.Dtos.AccessUserLevelDto
 *   - Placed in Apha.FPSApps.Application.Dtos.PIMS namespace for frontend consumption
 *   - Three-column composite PK (Systemid, Ntlogin, Accesslevelid) — pure junction table
 *
 * PRESERVED:
 *   - All property names match backend DTO exactly (case-sensitive)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
 */

namespace Apha.FPSApps.Application.Dtos.PIMS
{
    // TRANSFORMENGINE: Frontend DTO mirroring Apha.PIMS.Application.Dtos.AccessUserLevelDto — three-column composite PK junction (Systemid, Ntlogin, Accesslevelid)
    public class AccessUserLevelDto
    {
        public int Systemid { get; set; }
        public string Ntlogin { get; set; } = null!;
        public int Accesslevelid { get; set; }
    }
}
