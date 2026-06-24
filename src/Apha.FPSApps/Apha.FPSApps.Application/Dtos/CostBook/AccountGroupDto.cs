/*
 * TRANSFORMENGINE MIGRATION — AccountGroupDto.cs (Frontend)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New frontend DTO created in Apha.FPSApps.Application, mirroring backend Apha.Costbook.Application.Dtos.AccountGroupDto
 *   - Same shape as backend DTO — 2 properties: Csg7Group (PK, varchar 15), UseInflation (bool)
 *   - Used by ICostBookAccountGroupApiClient and frontend CostBookAccountGroupService to serialise/deserialise API calls
 *   - Also used as lookup DTO for CSG7 group dropdown in AccountCategory modal (GET /api/v1/accountgroup returns this list)
 *   - Namespace: Apha.FPSApps.Application.Dtos.CostBook (frontend application layer)
 *
 * PRESERVED:
 *   - All property names exactly match backend DTO (case-sensitive): Csg7Group, UseInflation
 *   - Csg7Group is non-null (PK); UseInflation is bool (defaults false)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm Csg7Group max length (varchar 15) validation is enforced at service/controller level
 */

namespace Apha.FPSApps.Application.Dtos.CostBook;

// TRANSFORMENGINE: Frontend mirror of Apha.Costbook.Application.Dtos.AccountGroupDto
//   Covers frmMaintainance Tab 3 (CSG7 Inflation Options) CRUD surface
//   Also used as the lookup DTO for the CSG7 group dropdown in AccountCategory maintenance modal
public class AccountGroupDto
{
    // TRANSFORMENGINE: Maps to backend AccountGroupDto.Csg7Group (PK, varchar 15)
    /// <summary>CSG7 group name. Primary key (varchar 15).</summary>
    public string Csg7Group { get; set; } = null!;

    // TRANSFORMENGINE: Maps to backend AccountGroupDto.UseInflation (bool)
    /// <summary>Whether inflation is applied to costs in this CSG7 group.</summary>
    public bool UseInflation { get; set; }
}
