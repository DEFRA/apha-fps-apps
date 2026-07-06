/*
 * TRANSFORMENGINE MIGRATION — AccessSystemDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend DTO mirroring Apha.PIMS.Application.Dtos.AccessSystemDto
 *   - Placed in Apha.FPSApps.Application.Dtos.PIMS namespace for frontend consumption
 *   - Read-only lookup (integer PK Systemid); no Req contract — GET only
 *
 * PRESERVED:
 *   - All property names match backend DTO exactly (case-sensitive)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
 */

namespace Apha.FPSApps.Application.Dtos.PIMS
{
    // TRANSFORMENGINE: Frontend DTO mirroring Apha.PIMS.Application.Dtos.AccessSystemDto — read-only lookup; integer PK (Systemid)
    public class AccessSystemDto
    {
        public int Systemid { get; set; }
        public string Systemname { get; set; } = null!;
    }
}
