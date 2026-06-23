/*
 * TRANSFORMENGINE MIGRATION — WorkgroupMaintenanceDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - NEW FILE: Frontend DTO mirroring Apha.FPS.Application.Dtos.WorkgroupDto in the FPSApps
 *     Application layer (different namespace, same shape)
 *   - Source form: frmMaintWorkGroup2 (RecordSource: WorkGroup_MAP → fps.workgroup)
 *   - All properties align exactly with backend WorkgroupDto property names and nullability
 *   - WorkGroupName and ProfitCentre are required (null!) — matches VBA form validation and
 *     WorkgroupMaintenanceReq required fields
 *   - CostCentre typed as double? matching fps.workgroup.costcentre (double precision, nullable)
 *   - CentralOverhead typed as decimal? matching fps.workgroup.centraloverhead (money → decimal)
 *   - SendEmail, Cos90 carried as short? matching DB smallint columns
 *   - FpsYear carried as int? for audit display; auto-resolved server-side by FpsRequestContext
 *
 * PRESERVED:
 *   - All nullable annotations aligned with backend WorkgroupDto and WorkgroupMaintenanceRes
 *   - Property order follows backend DTO (WorkGroupName, ProfitCentre, then remaining fields)
 *   - No Lk_ prefixes used — clean property names from entity surface
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether SendEmail and Cos90 (short?) should be surfaced as
 *     bool on the ViewModel/frontend binding layer (see backend WorkgroupDto same TODO)
 *   - TRANSFORMENGINE TODO: CostCentre is double precision in the DB; verify whether the frontend
 *     DataGrid display requires string formatting at the MVC controller level
 */

namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>
    /// Frontend DTO for WorkGroup maintenance operations.
    /// Mirrors <c>Apha.FPS.Application.Dtos.WorkgroupDto</c> — same shape, FPSApps Application namespace.
    /// Used by <see cref="Apha.FPSApps.Application.Interfaces.FpsApiClients.IFpsWorkgroupApiClient"/> for
    /// CRUD operations and by the frontend MVC layer via ViewModelMapper.
    /// </summary>
    public class WorkgroupMaintenanceDto
    {
        // TRANSFORMENGINE: WorkGroupName — required PK component; matches WorkgroupDto.WorkGroupName exactly
        /// <summary>
        /// WorkGroup name. Natural primary key component (composite PK: WorkGroupName + FpsYear).
        /// Required — validated as mandatory in the modal form.
        /// </summary>
        public string WorkGroupName { get; set; } = null!;

        // TRANSFORMENGINE: ProfitCentre — required FK; HTML label "ResourceCentre"; drives cascading CostCentre dropdown
        /// <summary>
        /// Profit centre code. Required.
        /// HTML modal label: "ResourceCentre"; backing DB column: fps.workgroup.profitcentre.
        /// Used to filter the cascading CostCentre dropdown via GET api/v1/workgroup/costcentres.
        /// </summary>
        public string ProfitCentre { get; set; } = null!;

        // TRANSFORMENGINE: CostCentre — optional; double? matches fps.workgroup.costcentre (double precision, nullable)
        /// <summary>
        /// Cost centre identifier. Optional.
        /// Filtered by ProfitCentre selection via cascading dropdown.
        /// Maps to fps.workgroup.costcentre (double precision, nullable).
        /// </summary>
        public double? CostCentre { get; set; }

        // TRANSFORMENGINE: Owner — optional; from qryManager lookup (GET api/v1/workgroup/owners → ManagerRes)
        /// <summary>
        /// Owner display name. Optional.
        /// Populated from the Owner dropdown (qryManager source).
        /// Maps to fps.workgroup.owner (varchar 50, nullable).
        /// </summary>
        public string? Owner { get; set; }

        /// <summary>
        /// Free-text description of the workgroup. Optional.
        /// Maps to fps.workgroup.description (varchar 45, nullable).
        /// </summary>
        public string? Description { get; set; }

        // TRANSFORMENGINE: CentralOverhead — money/decimal; displayed with £ prefix in JS prototype
        /// <summary>
        /// Central overhead allocation amount (GBP). Optional; defaults to 0 in DB.
        /// Maps to fps.workgroup.centraloverhead (money type; decimal here).
        /// </summary>
        public decimal? CentralOverhead { get; set; }

        /// <summary>
        /// Send-email flag (0 = no, 1 = yes). Not shown in the current DataGrid but present in DB.
        /// Maps to fps.workgroup.sendemail (smallint, nullable).
        /// </summary>
        public short? SendEmail { get; set; }

        /// <summary>
        /// COS90 flag. Not shown in the current DataGrid but present in DB.
        /// Maps to fps.workgroup.cos90 (smallint, nullable).
        /// </summary>
        public short? Cos90 { get; set; }

        /// <summary>
        /// Previous cost centre value — retained for historical reference.
        /// Maps to fps.workgroup.costcentreold (double precision, nullable).
        /// </summary>
        public double? CostCentreOld { get; set; }

        /// <summary>
        /// Email recipient address for workgroup notifications. Optional.
        /// Maps to fps.workgroup.email_recipient (varchar 50, nullable).
        /// </summary>
        public string? EmailRecipient { get; set; }

        // TRANSFORMENGINE: FpsYear — partition key; resolved by FpsRequestContext server-side; carried for audit display
        /// <summary>
        /// FPS financial year — informational; auto-resolved by server-side query filter.
        /// Second component of the composite primary key (workgroup, fpsyear).
        /// </summary>
        public int? FpsYear { get; set; }
    }
}
