// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — ProjectProfitabilityVlaReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-06-15
 *
 * CHANGED:
 *   - New file: no legacy C# equivalent existed.
 *   - Source artefact: HTML prototype frmJobcodeTotalsVLA.html + projectprofitability_vla.js
 *   - Four filter controls from HTML prototype (filterProjectStatus, filterProgram,
 *     filterManager, filterCustomer) mapped to typed request properties.
 *   - Pagination parameters (Page, PageSize) added per plan note; align with
 *     existing FPS paged-list contracts convention.
 *   - This is a query/filter request — all fields are optional; no Required
 *     annotations applied because the list endpoint returns all rows when no
 *     filter is selected (HTML prototype shows "All statuses / All programs /
 *     All managers / All customers" default options).
 *
 * PRESERVED:
 *   - Field semantics and naming aligned with projectprofitability_vla.js
 *     normalizeRow() property names and HTML filter element ids.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm MaxLength values match database column widths
 *     once the vprojectprofitability view schema is finalised in Phase 2.
 *   - TRANSFORMENGINE TODO: confirm default PageSize (15) matches DataGridComponent
 *     pageSize configured in projectprofitability_vla.js (currently 15).
 */

using System.ComponentModel.DataAnnotations;

namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Filter request for the Project Profitability VLA list endpoint
    /// (<c>GET /api/v1/project/profitability-vla</c>).
    /// All filter fields are optional; omitting a field returns all rows for that dimension.
    /// </summary>
    public class ProjectProfitabilityVlaReq
    {
        // TRANSFORMENGINE: maps HTML filterProjectStatus — static options: Approved, Completed, Not Approved
        /// <summary>
        /// Optional filter by project status (e.g. "Approved", "Completed", "Not Approved").
        /// </summary>
        [MaxLength(50)]
        public string? ProjectStatus { get; set; }

        // TRANSFORMENGINE: maps HTML filterProgram — dynamically populated from data
        /// <summary>
        /// Optional filter by program number / name.
        /// </summary>
        [MaxLength(50)]
        public string? ProgramNo { get; set; }

        // TRANSFORMENGINE: maps HTML filterManager — dynamically populated from data
        /// <summary>
        /// Optional filter by manager name.
        /// </summary>
        [MaxLength(100)]
        public string? Manager { get; set; }

        // TRANSFORMENGINE: maps HTML filterCustomer — dynamically populated from data
        /// <summary>
        /// Optional filter by customer name.
        /// </summary>
        [MaxLength(100)]
        public string? Customer { get; set; }

        // TRANSFORMENGINE: pagination — aligns with DataGridComponent pageSizeOptions [5,10,15,20,25,30]
        /// <summary>
        /// 1-based page number. Defaults to 1 if not supplied.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        /// <summary>
        /// Number of rows per page. Defaults to 15 (DataGrid default). Valid options: 5, 10, 15, 20, 25, 30.
        /// </summary>
        [Range(1, 100)]
        public int PageSize { get; set; } = 15;
    }
}
