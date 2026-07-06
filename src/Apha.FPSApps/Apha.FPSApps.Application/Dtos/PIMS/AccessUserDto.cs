/*
 * TRANSFORMENGINE MIGRATION — AccessUserDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend DTO mirroring Apha.PIMS.Application.Dtos.AccessUserDto
 *   - Placed in Apha.FPSApps.Application.Dtos.PIMS namespace for frontend consumption
 *   - Composite PK (Systemid, Ntlogin) — both are required fields
 *
 * PRESERVED:
 *   - All property names match backend DTO exactly (case-sensitive)
 *   - Nullable reference types match backend DTO nullability
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Systemid must be sourced from session/route context (fnSystemName() maps "PIMS" → SystemId) — verify frontend supplies this correctly
 */

namespace Apha.FPSApps.Application.Dtos.PIMS
{
    // TRANSFORMENGINE: Frontend DTO mirroring Apha.PIMS.Application.Dtos.AccessUserDto — composite PK (Systemid, Ntlogin)
    public class AccessUserDto
    {
        public int Systemid { get; set; }
        public string Ntlogin { get; set; } = null!;
        public string? Username { get; set; }
        public string? Dt2login { get; set; }
    }
}
