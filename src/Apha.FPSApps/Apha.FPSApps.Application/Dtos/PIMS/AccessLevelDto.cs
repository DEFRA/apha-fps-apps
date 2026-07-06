/*
 * TRANSFORMENGINE MIGRATION — AccessLevelDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend DTO mirroring Apha.PIMS.Application.Dtos.AccessLevelDto
 *   - Placed in Apha.FPSApps.Application.Dtos.PIMS namespace for frontend consumption
 *   - Composite PK (Systemid, Accesslevelid); lookup/read-only on Admin Tab
 *
 * PRESERVED:
 *   - All property names match backend DTO exactly (case-sensitive)
 *   - Nullable reference types match backend DTO nullability
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
 */

namespace Apha.FPSApps.Application.Dtos.PIMS
{
    // TRANSFORMENGINE: Frontend DTO mirroring Apha.PIMS.Application.Dtos.AccessLevelDto — lookup dropdown for Admin Tab; composite PK (Systemid, Accesslevelid)
    public class AccessLevelDto
    {
        public int Systemid { get; set; }
        public int Accesslevelid { get; set; }
        public string? Accesslevel { get; set; }
    }
}
