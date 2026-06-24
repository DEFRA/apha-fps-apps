/*
 * TRANSFORMENGINE MIGRATION — WorkgroupMaintenanceRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-06-23
 * Phase 6 verified : 2026-06-23
 *
 * CHANGED:
 *   - NEW FILE: no prior C# equivalent existed; supersedes the incomplete WorkGroupRes (which only had
 *     WorkGroupName and ProfitCentre) for CRUD maintenance response operations
 *   - Exposes the full fps.workgroup column surface plus a synthetic Id for grid key binding
 *   - JS DataGrid columns (fps_workgroup_maintenance.js initializeWGTable) mapped to response properties:
 *       workGroup         -> WorkGroupName
 *       resourceCentre    -> ProfitCentre  (HTML label alias; DB column profitcentre)
 *       costCentre        -> CostCentre?
 *       owner             -> Owner?
 *       description       -> Description?
 *       centralOverhead   -> CentralOverhead?
 *   - Additional DB columns (sendemail, cos90, emailrecipient, fpsyear) included for full CRUD support
 *   - Synthetic Id property added for grid row identity (row.id referenced in onclick handlers)
 *
 * PHASE 6 GATE — RESPONSE FIELD COVERAGE CONFIRMATION:
 *   - All 6 JS DataGrid columns confirmed in response contract:
 *       workGroup       -> WorkGroupName  — VERIFIED
 *       resourceCentre  -> ProfitCentre   — VERIFIED (HTML alias; DB column: profitcentre)
 *       costCentre      -> CostCentre?    — VERIFIED
 *       owner           -> Owner?         — VERIFIED
 *       description     -> Description?   — VERIFIED
 *       centralOverhead -> CentralOverhead? — VERIFIED
 *   - Synthetic Id included for DataGrid row.id key binding — VERIFIED
 *   - Additional DB columns (SendEmail, Cos90, EmailRecipient, FpsYear) present for full CRUD round-trip — VERIFIED
 *   - WorkgroupDto <-> WorkgroupMaintenanceRes mapping in RequestMapper confirmed BIDIRECTIONAL
 *   - WorkgroupMaintenanceRes.Id NOT in WorkgroupDto — AutoMapper ignores it on reverse map (synthetic only)
 *   - FpsYear in WorkgroupDto is int?; FpsYear in Res is int — AutoMapper maps nullable->non-nullable
 *     (null resolves to 0 default); acceptable since FpsYear is always set by FpsRequestContext
 *
 * PRESERVED:
 *   - Property names aligned with existing WorkGroupRes naming convention (WorkGroupName, ProfitCentre)
 *   - Nullable annotations match fps.workgroup DDL nullability
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether WorkGroupName should be returned as-is or
 *     normalised (DB stores as varchar 50, no case-folding defined in DDL).
 *   - TRANSFORMENGINE TODO: CostCentre is double precision in fps.workgroup; verify decimal conversion
 *     is acceptable for the frontend DataGrid display (may need string formatting at controller level).
 *   - TRANSFORMENGINE TODO: Confirm CentralOverhead money->decimal mapping precision with finance team.
 *   - TRANSFORMENGINE TODO: WorkgroupMaintenanceRes.Id is a synthetic int assigned by service layer;
 *     confirm service implementation populates it (e.g., row index or hash) before grid renders.
 */

namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Response contract for WorkGroup maintenance CRUD operations.
    /// Returned by GET /api/v1/workgroup/paged, GET /api/v1/workgroup/{workGroupName},
    /// POST /api/v1/workgroup (Create), and PUT /api/v1/workgroup/{workGroupName} (Update).
    /// Covers the full fps.workgroup column surface plus a synthetic Id for DataGrid row binding.
    /// </summary>
    public class WorkgroupMaintenanceRes
    {
        // TRANSFORMENGINE: synthetic surrogate for DataGrid row.id (no integer PK in fps.workgroup)
        /// <summary>
        /// Synthetic row identifier for DataGrid key binding and edit/delete action routing.
        /// Assigned by the backend service layer; not persisted in fps.workgroup.
        /// </summary>
        public int Id { get; set; }

        // TRANSFORMENGINE: DB fps.workgroup.workgroup (varchar 50, NOT NULL, PK component) -> WorkGroupName
        /// <summary>
        /// WorkGroup name. Natural primary key component.
        /// Maps to fps.workgroup.workgroup (varchar 50, NOT NULL).
        /// </summary>
        public string WorkGroupName { get; set; } = null!;

        // TRANSFORMENGINE: DB fps.workgroup.profitcentre -> ProfitCentre (JS grid field: resourceCentre)
        // HTML prototype uses "ResourceCentre" as the user-facing label; the underlying DB FK and API surface use profitcentre
        /// <summary>
        /// Profit Centre code. Displayed as "ResourceCentre" in the HTML prototype DataGrid.
        /// Maps to fps.workgroup.profitcentre (varchar 50, NOT NULL; FK: fps.tblkpprofitcentre).
        /// </summary>
        public string ProfitCentre { get; set; } = null!;

        // TRANSFORMENGINE: DB fps.workgroup.costcentre (double precision, nullable) -> CostCentre?
        /// <summary>
        /// Cost Centre identifier. Optional.
        /// Maps to fps.workgroup.costcentre (double precision, nullable).
        /// </summary>
        public double? CostCentre { get; set; }

        // TRANSFORMENGINE: DB fps.workgroup.owner (varchar 50, nullable) -> Owner?
        /// <summary>
        /// Owner display name. Optional.
        /// Maps to fps.workgroup.owner (varchar 50, nullable).
        /// </summary>
        public string? Owner { get; set; }

        // TRANSFORMENGINE: DB fps.workgroup.description (varchar 45, nullable) -> Description?
        /// <summary>
        /// Free-text description of the workgroup. Optional.
        /// Maps to fps.workgroup.description (varchar 45, nullable).
        /// </summary>
        public string? Description { get; set; }

        // TRANSFORMENGINE: DB fps.workgroup.centraloverhead (money, DEFAULT 0, nullable) -> CentralOverhead?
        /// <summary>
        /// Central overhead allocation amount (GBP). Defaults to 0 in DB.
        /// Maps to fps.workgroup.centraloverhead (money type; decimal here).
        /// </summary>
        public decimal? CentralOverhead { get; set; }

        // TRANSFORMENGINE: DB fps.workgroup.sendemail (smallint, nullable) -> SendEmail?
        /// <summary>
        /// Send-email flag (0 = no, 1 = yes). Not shown in current HTML prototype grid but present in DB.
        /// Maps to fps.workgroup.sendemail (smallint, nullable).
        /// </summary>
        public short? SendEmail { get; set; }

        // TRANSFORMENGINE: DB fps.workgroup.cos90 (smallint, nullable) -> Cos90?
        /// <summary>
        /// COS90 flag. Not shown in current HTML prototype grid but present in DB.
        /// Maps to fps.workgroup.cos90 (smallint, nullable).
        /// </summary>
        public short? Cos90 { get; set; }

        // TRANSFORMENGINE: DB fps.workgroup.email_recipient (varchar 50, nullable) -> EmailRecipient?
        /// <summary>
        /// Email recipient address. Not shown in current HTML prototype grid but present in DB.
        /// Maps to fps.workgroup.email_recipient (varchar 50, nullable).
        /// </summary>
        public string? EmailRecipient { get; set; }

        // TRANSFORMENGINE: DB fps.workgroup.fpsyear (integer, NOT NULL, PK component) -> FpsYear
        /// <summary>
        /// FPS financial year. Second component of the composite primary key (workgroup, fpsyear).
        /// Maps to fps.workgroup.fpsyear (integer, NOT NULL; FK: fps.tblyearmaster).
        /// </summary>
        public int FpsYear { get; set; }
    }
}
