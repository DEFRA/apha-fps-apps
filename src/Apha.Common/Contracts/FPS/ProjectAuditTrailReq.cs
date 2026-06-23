/*
 * TRANSFORMENGINE MIGRATION — ProjectAuditTrailReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - MS Access frmProjectChangesLog filter section → typed C# request contract
 *   - HTML filter-project (select) → ParentProject string? filter
 *   - HTML filter-from / filter-to (type="date" inputs) → DateOnly? FromDate / ToDate
 *   - All three fields are nullable: an empty filter is valid (returns no rows per JS behaviour)
 *   - Phase 6 Backend Readiness Gate — VERIFIED: contract matches controller query params
 *     (project, fromDate, toDate); ParentProject string? aligns with controller string project param;
 *     DateOnly? FromDate/ToDate align with controller DateOnly? fromDate/toDate params;
 *     no structural changes required; ParentProject requiredness resolved at controller layer
 *     (ArgumentException guard in action body) so nullable here is correct per the frontend
 *     binding pattern where the HTML select can be unpopulated on first page load
 *
 * PRESERVED:
 *   - Field semantics from projectaudit_trail.html filter section (Project, From, To)
 *   - Nullable contract — all three fields are optional per the legacy filter UX
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm whether the API endpoint enforces ParentProject as required
 *     (legacy JS clears all grids when no project is selected — the backend may want to mirror this)
 */

namespace Apha.Common.Contracts.FPS
{
    // TRANSFORMENGINE: request contract for GET /fps/project-audit-trail
    // Binds the three HTML filter inputs from projectaudit_trail.html:
    //   #filter-project (select), #filter-from (date), #filter-to (date)
    public class ProjectAuditTrailReq
    {
        // TRANSFORMENGINE: maps to HTML #filter-project select; nullable — omitting returns empty result set per legacy UX
        public string? ParentProject { get; set; }

        // TRANSFORMENGINE: maps to HTML #filter-from type="date"; DateOnly because the input carries no time component
        public DateOnly? FromDate { get; set; }

        // TRANSFORMENGINE: maps to HTML #filter-to type="date"; DateOnly because the input carries no time component
        public DateOnly? ToDate { get; set; }
    }
}
