/*
 * TRANSFORMENGINE MIGRATION — WorkgroupDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New DTO created; no prior WorkgroupDto existed for the maintenance surface
 *   - Fields derived from Workgroup entity (fps.workgroup DDL) and frmMaintWorkGroup2 form fields:
 *     WorkGroupName, ProfitCentre, CostCentre (double?), Owner, Description,
 *     CentralOverhead (decimal?), SendEmail (short?), Cos90 (short?),
 *     CostCentreOld (double?), EmailRecipient, FpsYear (partition key)
 *   - SysTimestamp intentionally excluded from public DTO surface (server-managed column)
 *
 * PRESERVED:
 *   - All nullable annotations aligned exactly with Workgroup entity nullability
 *   - WorkGroupName is required (null!), ProfitCentre is required — matches VBA form validation
 *   - FpsYear carried through for display/audit; service layer does not use it for filtering
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: SysTimestamp is on the entity but has no confirmed DDL column —
 *     if it is needed for optimistic-concurrency in the API, add it here and update the mapper
 *   - TRANSFORMENGINE TODO: Confirm whether SendEmail and Cos90 (short?) should be exposed
 *     as bool on the DTO surface to simplify the MVC binding layer
 */

namespace Apha.FPS.Application.Dtos
{
    /// <summary>
    /// Data Transfer Object for Workgroup maintenance operations.
    /// Maps to/from the <c>fps.workgroup</c> table entity via EntityMapper.
    /// </summary>
    public class WorkgroupDto
    {
        // TRANSFORMENGINE: WorkGroupName — required PK component (composite PK WorkGroupName + FpsYear)
        /// <summary>Workgroup name — primary key component.</summary>
        public string WorkGroupName { get; set; } = null!;

        // TRANSFORMENGINE: ProfitCentre — required FK; drives cascading CostCentre dropdown
        /// <summary>Profit centre — required; used to filter CostCentre dropdown.</summary>
        public string ProfitCentre { get; set; } = null!;

        /// <summary>Cost centre — nullable double; filtered by ProfitCentre selection.</summary>
        public double? CostCentre { get; set; }

        /// <summary>Owner (manager name) — nullable; from qryManager lookup.</summary>
        public string? Owner { get; set; }

        /// <summary>Free-text description of the workgroup.</summary>
        public string? Description { get; set; }

        // TRANSFORMENGINE: CentralOverhead — money/decimal type from DDL; displayed with £ prefix in JS prototype
        /// <summary>Central overhead amount — decimal precision (money column).</summary>
        public decimal? CentralOverhead { get; set; }

        /// <summary>Send email flag — short? maps to bit/flag in legacy DDL.</summary>
        public short? SendEmail { get; set; }

        /// <summary>Cos90 flag — short? maps to bit/flag in legacy DDL.</summary>
        public short? Cos90 { get; set; }

        /// <summary>Previous cost centre value — retained for historical reference.</summary>
        public double? CostCentreOld { get; set; }

        /// <summary>Email recipient address for workgroup notifications.</summary>
        public string? EmailRecipient { get; set; }

        // TRANSFORMENGINE: FpsYear — partition key; resolved by FpsRequestContext, carried on DTO for audit display
        /// <summary>FPS financial year — informational; auto-resolved by server-side query filter.</summary>
        public int? FpsYear { get; set; }
    }
}
