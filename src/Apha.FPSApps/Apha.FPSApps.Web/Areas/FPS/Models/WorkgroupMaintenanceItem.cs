/*
 * TRANSFORMENGINE MIGRATION — WorkgroupMaintenanceItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - NEW FILE: Extracted from Phase 10 stub WorkgroupMaintenanceViewModel.cs into dedicated file
 *   - Source form: frmMaintWorkGroup2 (RecordSource: WorkGroup_MAP -> fps.workgroup)
 *   - [GridColumn] attributes derived from JS fps_workgroup_maintenance.js initializeWGTable() columns array:
 *       workGroup (150), resourceCentre (170), costCentre (150), owner (180), description (260),
 *       centralOverhead (170) — actions column is rendered by DataGrid infrastructure, not a C# property
 *   - AllowAdd = true  (showAddButton: true in JS DataGridComponent)
 *   - AllowEdit = true  (edit button present in JS actions column render)
 *   - AllowDelete = true  (delete button present in JS actions column render)
 *   - KeyProperty = "WorkGroupName" — maps to JS grid row.id (workGroup is the natural PK; WorkGroupName
 *     is visible in the grid per JS columns[0] { field:'workGroup' }, so it stays visible — NOT hidden)
 *   - Required validation on WorkGroupName and ProfitCentre only (matches JS wgValidationFields)
 *   - CentralOverhead typed as decimal? (GBP money column in DB; £ prefix shown in JS prototype)
 *   - CostCentre typed as double? (fps.workgroup.costcentre double precision, nullable)
 *   - Audit-only fields (SendEmail, Cos90, CostCentreOld, EmailRecipient, FpsYear) carried for modal
 *     round-trip but not shown in the main grid (IsVisible = false)
 *
 * PRESERVED:
 *   - Property names mirror WorkgroupMaintenanceDto exactly for AutoMapper convention-based mapping
 *   - Nullable annotations aligned with WorkgroupMaintenanceDto and WorkgroupMaintenanceRes
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: CostCentre is double? — verify whether the DataGrid display requires string
 *     formatting or a dedicated display-only string field at the MVC controller level
 *   - TRANSFORMENGINE TODO: SendEmail and Cos90 are short? — confirm if bool binding is needed in view layer
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// DataGrid row model and Add/Edit modal partial model for WorkGroup Maintenance.
    /// Properties mirror <see cref="Apha.FPSApps.Application.Dtos.FPS.WorkgroupMaintenanceDto"/>
    /// for AutoMapper convention-based mapping in FpsViewModelMapper.
    /// Derived from JS <c>initializeWGTable()</c> columns array in fps_workgroup_maintenance.js.
    /// </summary>
    public class WorkgroupMaintenanceItem
    {
        // TRANSFORMENGINE: WorkGroupName — PK component; visible grid column per JS columns[0] { field:'workGroup', header:'WorkGroup', width:150 }
        // Also used as KeyProperty in DataGridConfig. Visible because it appears in JS columns array.
        [Display(Name = "WorkGroup")]
        [Required(ErrorMessage = "WorkGroup is required")]
        [GridColumn(Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string WorkGroupName { get; set; } = null!;

        // TRANSFORMENGINE: ProfitCentre — JS columns[1] { field:'resourceCentre', header:'ResourceCentre', width:170 }
        // HTML label "ResourceCentre"; modal uses AJAX GET /FPS/WorkgroupMaintenance/GetProfitCentres
        [Display(Name = "ResourceCentre")]
        [Required(ErrorMessage = "ResourceCentre is required")]
        [GridColumn(Width = 170, Type = GridColumnType.Text, IsFilterable = true)]
        public string ProfitCentre { get; set; } = null!;

        // TRANSFORMENGINE: CostCentre — JS columns[2] { field:'costCentre', header:'CostCentre', width:150 }
        // Optional; cascading dropdown in modal filtered by ProfitCentre via AJAX GET /FPS/WorkgroupMaintenance/GetCostCentres
        [Display(Name = "CostCentre")]
        [GridColumn(Width = 150, Type = GridColumnType.DecimalNumber, IsFilterable = true)]
        public double? CostCentre { get; set; }

        // TRANSFORMENGINE: Owner — JS columns[3] { field:'owner', header:'Owner', width:180 }
        // Optional; modal uses AJAX GET /FPS/WorkgroupMaintenance/GetOwners → ManagerDto.Name
        [Display(Name = "Owner")]
        [GridColumn(Width = 180, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Owner { get; set; }

        // TRANSFORMENGINE: Description — JS columns[4] { field:'description', header:'Description', width:260 }
        [Display(Name = "Description")]
        [GridColumn(Width = 260, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Description { get; set; }

        // TRANSFORMENGINE: CentralOverhead — JS columns[5] { field:'centralOverhead', header:'CentralOverhead', width:170 }
        // JS prototype formats this as '£N.NN'; GBP value column
        [Display(Name = "CentralOverhead")]
        [GridColumn(Width = 170, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? CentralOverhead { get; set; }

        // ── Audit / non-grid fields — carried for modal round-trip, not displayed in main grid ─────

        // TRANSFORMENGINE: SendEmail — not in JS grid columns; DB smallint; hidden from grid
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public short? SendEmail { get; set; }

        // TRANSFORMENGINE: Cos90 — not in JS grid columns; DB smallint; hidden from grid
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public short? Cos90 { get; set; }

        // TRANSFORMENGINE: CostCentreOld — not in JS grid columns; historical reference; hidden from grid
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public double? CostCentreOld { get; set; }

        // TRANSFORMENGINE: EmailRecipient — not in JS grid columns; notification address; hidden from grid
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? EmailRecipient { get; set; }

        // TRANSFORMENGINE: FpsYear — partition key; auto-resolved server-side; hidden from grid
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int? FpsYear { get; set; }
    }
}
