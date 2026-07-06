/*
 * TRANSFORMENGINE MIGRATION — AccessUserDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application DTO mirroring Apha.PIMS.Core.Entities.AccessUser
 *   - Composite PK (Systemid, Ntlogin) carried in DTO for Add/Update/Delete
 *
 * PRESERVED:
 *   - All field names consistent with entity naming convention
 *   - Systemid included — service enforces PIMS-system-only access guard
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: verify AccessUserService filters by PIMS SystemId at all list/get operations to prevent cross-system data exposure
 */

namespace Apha.PIMS.Application.Dtos
{
    // TRANSFORMENGINE: DTO maps to/from Apha.PIMS.Core.Entities.AccessUser via EntityMapper; composite PK (Systemid, Ntlogin)
    public class AccessUserDto
    {
        // TRANSFORMENGINE: Systemid is part of composite PK and PIMS system isolation guard
        public int Systemid { get; set; }

        public string Ntlogin { get; set; } = null!;

        public string? Username { get; set; }

        public string? Dt2login { get; set; }
    }
}
