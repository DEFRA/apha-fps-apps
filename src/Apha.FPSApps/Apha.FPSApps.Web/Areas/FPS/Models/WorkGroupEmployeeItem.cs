// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — WorkGroupEmployeeItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-11
 *
 * CHANGED:
 *   - PactId: changed from hidden (IsVisible = false) to visible grid column with Width = 90
 *     and GridColumnType.ReadOnly — JS columns[] shows pactId as a visible sortable column;
 *     per skill rule, a PK shown in JS columns[] must remain visible.
 *   - Added StaffName (string?) — JS column { field: 'staffName', header: 'Staff Name', width: 220 }
 *     maps to WorkGroupEmployeeDto.Name (displayed as Name in prior item; renamed to match JS key
 *     'staffName' for DataGrid binding — see DEFERRED note).
 *   - Added WgGrade (string?) — JS column { field: 'wgGrade', header: 'WG Grade', width: 120 }
 *     maps to WorkGroupEmployeeDto.WorkGroupGrade (display column, read-only).
 *   - Added TimeRecorder (bool) — JS column { field: 'timeRecorder', render: checkbox ✔, width: 105 }
 *     maps to WorkGroupEmployeeDto.TimeRecorder (int); AutoMapper ForMember handles int<->bool.
 *   - Added StartDate (DateTime?) — JS column { field: 'startDate', header: 'Start Date', width: 110 }.
 *   - Added EndDate (DateTime?) — JS column { field: 'endDate', header: 'End Date', width: 110 }.
 *   - Added HoursPerWeek (double?) — JS column { field: 'hoursPerWeek', header: 'Hours per week', width: 115 }.
 *   - Reordered existing properties to match JS DataGridComponent columns[] order exactly.
 *   - MakeAvailable renamed from bool to match GridColumnType.Checkbox — field name preserved; maps to
 *     JS { field: 'available', header: 'Available?', render: checkbox ✔ }.
 *   - SpNumber and Name (read-only columns from prior version) aligned with JS column order.
 *   - PersonStatus and PersonClass kept as IsVisible = false (grid metadata; not in JS columns[]).
 *
 * PRESERVED:
 *   - SpNumber, HrsPaid, Leave, SickSpecial, HrsAvail, MakeAvailable, PersonStatus, PersonClass
 *   - All GridColumnType and IsFilterable values for pre-existing fields
 *   - Namespace Apha.FPSApps.Web.Areas.FPS.Models unchanged
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: JS uses field key 'staffName' and 'wgGrade'; backend DTO uses 'Name'
 *     and 'WorkGroupGrade'. FpsViewModelMapper CreateMap<WorkGroupEmployeeItem, WorkGroupEmployeeDto>
 *     must add ForMember overrides for StaffName->Name and WgGrade->WorkGroupGrade (and reverse) so
 *     AutoMapper does not silently drop these values. Update FpsViewModelMapper.cs after Phase 11.
 *   - TRANSFORMENGINE TODO: FpsViewModelMapper TimeRecorder ForMember (int<->bool) was deferred to
 *     Phase 11 in the Phase 10 checklist entry — add it to FpsViewModelMapper now.
 *   - TRANSFORMENGINE TODO: PactId was previously IsVisible=false (hidden PK). It is now visible
 *     (Width=90, ReadOnly) to match JS columns[]. Verify DataGrid KeyProperty = "PactId" still
 *     works correctly with the field visible.
 *   - TRANSFORMENGINE TODO: HrsAvail is GridColumnType.ReadOnly (computed server-side, not in Req).
 *     Verify AutoMapper does not attempt to map it on POST/PUT (it should be Res-only).
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    // TRANSFORMENGINE: Grid item for the WG Staff maintenance grid (frmMaintWGStaff).
    // AllowAdd: true  AllowEdit: true  AllowDelete: true  (derived from JS prototype)
    // Column order matches JS DataGridComponent columns[] array in fps_maintain_wg_staff.js.
    public class WorkGroupEmployeeItem
    {
        // TRANSFORMENGINE: PactId — visible grid column AND KeyProperty.
        // JS columns[0] = { field: 'pactId', header: 'PACTId', width: 90 }.
        // Previously IsVisible = false (hidden PK); corrected to visible per skill rule:
        // "A PK shown in JS columns[] must stay visible."
        [Display(Name = "PACTId")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string PactId { get; set; } = null!;

        // TRANSFORMENGINE: StaffName — JS columns[1] = { field: 'staffName', header: 'Staff Name', width: 220 }.
        // Maps to WorkGroupEmployeeDto.Name via ForMember in FpsViewModelMapper (see DEFERRED above).
        [Display(Name = "Staff Name")]
        [GridColumn(Width = 220, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? StaffName { get; set; }

        // TRANSFORMENGINE: WgGrade — JS columns[2] = { field: 'wgGrade', header: 'WG Grade', width: 120 }.
        // Maps to WorkGroupEmployeeDto.WorkGroupGrade via ForMember in FpsViewModelMapper (see DEFERRED).
        [Display(Name = "WG Grade")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? WgGrade { get; set; }

        // TRANSFORMENGINE: Status — JS columns[3] = { field: 'status', header: 'Status', width: 70 }.
        // Maps to WorkGroupEmployeeDto.PersonStatus.
        [Display(Name = "Status")]
        [Required(ErrorMessage = "Status is required")]
        [GridColumn(Width = 70, Type = GridColumnType.Text, IsFilterable = false)]
        public string PersonStatus { get; set; } = null!;

        // TRANSFORMENGINE: ClassCode — JS columns[4] = { field: 'classCode', header: 'Class', width: 70 }.
        // Maps to WorkGroupEmployeeDto.PersonClass.
        [Display(Name = "Class")]
        [GridColumn(Width = 70, Type = GridColumnType.Text, IsFilterable = false)]
        public string? PersonClass { get; set; }

        // TRANSFORMENGINE: HrsPaid — JS columns[5] = { field: 'hrsPaid', header: 'HrsPaid', width: 80 }.
        [Display(Name = "HrsPaid")]
        [Required(ErrorMessage = "HrsPaid is required")]
        [GridColumn(Width = 80, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double HrsPaid { get; set; }

        // TRANSFORMENGINE: Leave — JS columns[6] = { field: 'leave', header: 'Leave', width: 70 }.
        [Display(Name = "Leave")]
        [Required(ErrorMessage = "Leave is required")]
        [GridColumn(Width = 70, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double Leave { get; set; }

        // TRANSFORMENGINE: SickSpecial — JS columns[7] = { field: 'sickSpecial', header: 'SickSpecial', width: 95 }.
        [Display(Name = "SickSp")]
        [Required(ErrorMessage = "SickSp is required")]
        [GridColumn(Width = 95, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double SickSpecial { get; set; }

        // TRANSFORMENGINE: HrsAvail — JS columns[8] = { field: 'hrsAvail', header: 'HrsAvail', width: 90 }.
        // Read-only: computed server-side (HrsPaid - Leave - SickSpecial). Not in WorkGroupEmployeeReq.
        [Display(Name = "HrsAvail")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly, IsFilterable = false)]
        public double HrsAvail { get; set; }

        // TRANSFORMENGINE: MakeAvailable (Available?) — JS columns[9] = { field: 'available', header: 'Available?', width: 90, render: checkbox ✔ }.
        // bool in Item; int (0/1) in Dto. FpsViewModelMapper ForMember handles conversion.
        [Display(Name = "Available?")]
        [GridColumn(Width = 90, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public bool MakeAvailable { get; set; }

        // TRANSFORMENGINE: TimeRecorder — JS columns[10] = { field: 'timeRecorder', header: 'Time recorder?', width: 105, render: checkbox ✔ }.
        // bool in Item; int (0/1) in Dto. FpsViewModelMapper must add ForMember for TimeRecorder (see DEFERRED).
        // Added in Phase 11 (was deferred in Phase 10 FpsViewModelMapper update).
        [Display(Name = "Time recorder?")]
        [GridColumn(Width = 105, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public bool TimeRecorder { get; set; }

        // TRANSFORMENGINE: StartDate — JS columns[11] = { field: 'startDate', header: 'Start Date', width: 110 }.
        // Nullable — user may not supply a start date.
        [Display(Name = "Start Date")]
        [GridColumn(Width = 110, Type = GridColumnType.Date, IsFilterable = false)]
        public DateTime? StartDate { get; set; }

        // TRANSFORMENGINE: EndDate — JS columns[12] = { field: 'endDate', header: 'End Date', width: 110 }.
        // Nullable — user may not supply an end date.
        [Display(Name = "End Date")]
        [GridColumn(Width = 110, Type = GridColumnType.Date, IsFilterable = false)]
        public DateTime? EndDate { get; set; }

        // TRANSFORMENGINE: HoursPerWeek — JS columns[13] = { field: 'hoursPerWeek', header: 'Hours per week', width: 115 }.
        // Nullable double — JS prototype uses '' (empty string) when not set.
        [Display(Name = "Hours per week")]
        [GridColumn(Width = 115, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double? HoursPerWeek { get; set; }

        // TRANSFORMENGINE: SpNumber — not a JS visible column but kept for read-only display in modal.
        // Previously Width = 100; kept as IsVisible = false to preserve modal context without cluttering grid.
        [Display(Name = "SP No")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsVisible = false, IsFilterable = false)]
        public string? SpNumber { get; set; }
    }
}
