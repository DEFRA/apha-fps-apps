/*
 * TRANSFORMENGINE MIGRATION — WorkgroupMaintenanceReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-06-23
 * Phase 6 verified : 2026-06-23
 *
 * CHANGED:
 *   - NEW FILE: no prior C# equivalent existed; replaces MS Access form write-path for fps_workgroup_maintenance
 *   - Modal form fields (fps_workgroup_maintenance.html #formTblWG) mapped to request contract properties
 *   - HTML field name="workGroup" -> WorkGroupName (aligns with DB workgroup PK column)
 *   - HTML field name="resourceCentre" -> ProfitCentre (HTML select populates from fps.tblkpprofitcentre; DB FK profitcentre)
 *   - HTML field name="costCentre"     -> CostCentre? (optional; double precision in fps.workgroup.costcentre)
 *   - HTML field name="owner"          -> Owner? (optional; varchar 50 in fps.workgroup.owner)
 *   - HTML field name="description"    -> Description? (optional; varchar 45 in fps.workgroup.description)
 *   - HTML field name="centralOverhead"-> CentralOverhead? (optional; money type in fps.workgroup.centraloverhead; decimal here)
 *
 * PHASE 6 GATE — CONTRACT FIELD COVERAGE CONFIRMATION:
 *   - All 6 HTML modal form fields accounted for:
 *       WorkGroupName (required, aria-required="true") — VERIFIED
 *       ProfitCentre  (required, aria-required="true") — VERIFIED
 *       CostCentre?   (optional, cascading dropdown)   — VERIFIED
 *       Owner?        (optional, qryManager dropdown)  — VERIFIED
 *       Description?  (optional, free text)            — VERIFIED
 *       CentralOverhead? (optional, currency input)    — VERIFIED
 *   - FpsYear correctly excluded (resolved server-side via FpsRequestContext; not a form surface field)
 *   - Frontend will bind WorkGroupName as required business context when calling POST /api/v1/workgroup
 *     and PUT /api/v1/workgroup/{workGroupName}
 *
 * PRESERVED:
 *   - Required/optional distinctions match HTML aria-required attributes and JS wgValidationFields
 *   - Field ordering follows top-to-bottom, left-to-right modal layout
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: FpsYear is a PK component in fps.workgroup (composite PK workgroup+fpsyear).
 *     The API controller must supply FpsYear via a route parameter or app-context header — it is NOT
 *     included in this request body to keep the contract narrow per the form surface.
 *   - TRANSFORMENGINE TODO: CostCentre is double precision in the DB; confirm whether the API should
 *     accept it as double or as a string code and parse server-side.
 */

namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Request contract for creating or updating a WorkGroup maintenance record.
    /// Accepted by POST /api/v1/workgroup (Create) and PUT /api/v1/workgroup/{workGroupName} (Update).
    /// Contains only the writable fields exposed in the fps_workgroup_maintenance modal form.
    /// </summary>
    public class WorkgroupMaintenanceReq
    {
        // TRANSFORMENGINE: modal-wg-name (required input, aria-required="true") -> WorkGroupName
        /// <summary>
        /// WorkGroup name. Natural primary key component in fps.workgroup.
        /// Required — validated as mandatory in wgValidationFields.
        /// </summary>
        public string WorkGroupName { get; set; } = null!;

        // TRANSFORMENGINE: modal-wg-rc ResourceCentre select (required, aria-required="true") -> ProfitCentre
        // HTML label says "ResourceCentre" but the backing DB column is profitcentre (FK to fps.tblkpprofitcentre)
        /// <summary>
        /// Profit Centre code selected via the ResourceCentre dropdown.
        /// Required — validated as mandatory in wgValidationFields.
        /// Maps to fps.workgroup.profitcentre (FK: fps.tblkpprofitcentre).
        /// </summary>
        public string ProfitCentre { get; set; } = null!;

        // TRANSFORMENGINE: modal-wg-cc CostCentre select (optional) -> CostCentre?
        /// <summary>
        /// Cost Centre identifier. Optional.
        /// Maps to fps.workgroup.costcentre (double precision; numeric cost centre code).
        /// </summary>
        public double? CostCentre { get; set; }

        // TRANSFORMENGINE: modal-wg-owner Owner select (optional) -> Owner?
        /// <summary>
        /// Owner display name selected from the Owner dropdown. Optional.
        /// Maps to fps.workgroup.owner (varchar 50).
        /// </summary>
        public string? Owner { get; set; }

        // TRANSFORMENGINE: modal-wg-desc Description input (optional) -> Description?
        /// <summary>
        /// Free-text description of the workgroup. Optional.
        /// Maps to fps.workgroup.description (varchar 45).
        /// </summary>
        public string? Description { get; set; }

        // TRANSFORMENGINE: modal-wg-overhead CentralOverhead currency input (optional) -> CentralOverhead?
        // HTML renders £-prefixed formatted string; API receives the numeric value after client-side stripping
        /// <summary>
        /// Central overhead allocation amount (GBP). Optional.
        /// Maps to fps.workgroup.centraloverhead (money type; stored as decimal here).
        /// </summary>
        public decimal? CentralOverhead { get; set; }
    }
}
