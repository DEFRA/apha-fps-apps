/*
 * TRANSFORMENGINE MIGRATION — SetUpStaffResourcesItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-frontend  Phase 5 — ViewModels + MVC Controller
 * Migrated : 2026-07-07
 *
 * CHANGED:
 *   - New grid item model created for the staff data-grid in Set Up Staff Resources
 *   - Maps JS DataGridComponent columns: spNo, name, hrsPaid, leave, sickSp, atWork, planable
 *   - Grid Operations Profile: Edit-only (no Add, no Delete — JS showAddButton:false, no delete button)
 *   - WorkGroupGrade + SpNumber: ReadOnly (read-only in .frm and locked in HTML modal prototype)
 *   - HrsAvail (AtWork): ReadOnly + computed (HrsPaid - Leave - SickSpecial)
 *   - MakeAvailable (Planable): Checkbox
 *   - Name, HrsPaid, Leave, SickSpecial: Editable in Edit modal
 *   - PactId: hidden key field for edit operations
 *
 * PRESERVED:
 *   - Property names match WorkGroupEmployeeStaffDto (PactId, SpNumber, WorkGroupGrade, Name,
 *     HrsPaid, Leave, SickSpecial, HrsAvail, MakeAvailable)
 *   - JS grid column order preserved: spNo, name, hrsPaid, leave, sickSp, atWork, planable
 *   - GridColumn widths match JS column widths exactly
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Add CreateMap<SetUpStaffResourcesItem, WorkGroupEmployeeStaffDto>().ReverseMap()
 *     to FpsViewModelMapper.cs once this file is registered in the build.
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class SetUpStaffResourcesItem
    {
        // TRANSFORMENGINE: PactId — hidden key, not visible in JS columns array (used for Edit routing only)
        [GridColumn(IsVisible = false)]
        public string PactId { get; set; } = null!;

        // TRANSFORMENGINE: SpNumber — read-only in modal (ssrEditSpNo readonly), maps to JS field 'spNo'
        [Display(Name = "SP No")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string SpNumber { get; set; } = null!;

        // TRANSFORMENGINE: WorkGroupGrade — not visible in JS columns array; used in modal GradeCode (ssrEditGradeCode, readonly)
        [GridColumn(IsVisible = false)]
        public string WorkGroupGrade { get; set; } = null!;

        // TRANSFORMENGINE: Name — editable in modal (ssrEditName), maps to JS field 'name'
        [Display(Name = "Name")]
        [GridColumn(Width = 200, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Name { get; set; }

        // TRANSFORMENGINE: HrsPaid — editable number in modal (ssrEditHrsPaid), maps to JS field 'hrsPaid'
        [Display(Name = "Hrs Paid")]
        [GridColumn(Width = 90, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double HrsPaid { get; set; }

        // TRANSFORMENGINE: Leave — editable number in modal (ssrEditLeave), maps to JS field 'leave'
        [Display(Name = "Leave")]
        [GridColumn(Width = 70, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double Leave { get; set; }

        // TRANSFORMENGINE: SickSpecial — editable number in modal (ssrEditSickSp), maps to JS field 'sickSp'
        [Display(Name = "Sick Sp")]
        [GridColumn(Width = 80, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double SickSpecial { get; set; }

        // TRANSFORMENGINE: HrsAvail — computed readonly (HrsPaid - Leave - SickSpecial), maps to JS field 'atWork'
        //   Displayed as readonly in modal (ssrEditAtWork); JS recalculates on input change
        [Display(Name = "At Work")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly, IsFilterable = false)]
        public double HrsAvail { get; set; }

        // TRANSFORMENGINE: MakeAvailable — checkbox in modal (ssrEditPlanable), maps to JS field 'planable'
        //   int type matches WorkGroupEmployeeStaffDto.MakeAvailable (0/1 from backend)
        [Display(Name = "Planable")]
        [GridColumn(Width = 80, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public int MakeAvailable { get; set; }
    }
}
