/*
 * TRANSFORMENGINE MIGRATION — AccountCategoryMaintenanceDto.cs (Frontend)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New frontend DTO created in Apha.FPSApps.Application, mirroring backend Apha.Costbook.Application.Dtos.AccountCategoryMaintenanceDto
 *   - Same shape as backend DTO — 4 properties: AccShortName (PK), AccountDescription, Csg7Group (maintained field), FpsYear
 *   - Used by ICostBookMaintenanceApiClient to deserialise GET /api/v1/maintenance/account-categories responses
 *     and serialise PUT /api/v1/maintenance/account-categories/{accShortName} requests
 *   - Namespace: Apha.FPSApps.Application.Dtos.CostBook (frontend application layer)
 *
 * PRESERVED:
 *   - All property names exactly match backend DTO (case-sensitive): AccShortName, AccountDescription, Csg7Group, FpsYear
 *   - Nullability preserved: AccShortName non-null; AccountDescription and Csg7Group nullable; FpsYear is int
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether FpsYear is needed on the frontend (currently server-side derived from CurrentFinancialYear)
 *   - TRANSFORMENGINE TODO: Confirm whether ConstituentAccountCodes or other fields are needed in the maintenance grid view
 */

namespace Apha.FPSApps.Application.Dtos.CostBook;

// TRANSFORMENGINE: Frontend mirror of Apha.Costbook.Application.Dtos.AccountCategoryMaintenanceDto
//   Covers frmMaintainance Tab 2 (Account Categories) grid — only Csg7Group is writable via PUT
public class AccountCategoryMaintenanceDto
{
    // TRANSFORMENGINE: Maps to backend AccountCategoryMaintenanceDto.AccShortName (PK, varchar 50)
    /// <summary>Account short name. Part of the composite primary key.</summary>
    public string AccShortName { get; set; } = null!;

    // TRANSFORMENGINE: Maps to backend AccountCategoryMaintenanceDto.AccountDescription (varchar 50, nullable)
    /// <summary>Human-readable description of the account category.</summary>
    public string? AccountDescription { get; set; }

    // TRANSFORMENGINE: Maps to backend AccountCategoryMaintenanceDto.Csg7Group (char 15, nullable) — the maintained field
    /// <summary>Assigned CSG7 group for this account category. Nullable — may be unassigned.</summary>
    public string? Csg7Group { get; set; }

    // TRANSFORMENGINE: Maps to backend AccountCategoryMaintenanceDto.FpsYear (int, PK component)
    /// <summary>FPS financial year. Part of the composite primary key (partitioned table).</summary>
    public int FpsYear { get; set; }
}
