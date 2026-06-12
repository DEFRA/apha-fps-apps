// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — GradeMaintenanceViewModel.cs (new file)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet8-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-10
 *
 * CHANGED:
 *   - New ViewModel and Item classes created for the Grade maintenance DataGrid form (frmMaintGrade)
 *   - GradeMaintenanceViewModel: holds DataGridConfig<GradeItem>; no page-level dropdowns
 *     (HTML prototype frmMaintGrade.html has no <select> element outside the grid container)
 *   - GradeItem: three visible grid columns derived from JS DataGridComponent columns array
 *     in fps_grade_maintenance.js (gradeCode width:140, description width:280, avSalary width:170)
 *   - FpsYear added as hidden/IsVisible=false field (composite PK component; partition enforced
 *     server-side via HasQueryFilter on FpsDbContext — not displayed or editable in the UI)
 *   - AllowAdd=true from JS showAddButton:true
 *   - AllowEdit=true and AllowDelete=true from edit+delete buttons in JS actions column render
 *   - GradeItem.GradeCode is [Required] — JS modal validation marks only modal-grade-code as required
 *     (requiredFields = [gradeValidationFields[0]])
 *
 * PRESERVED:
 *   - GradeItem property names match GradeDto exactly (GradeCode, Description, AvSalary, FpsYear)
 *     as required by FpsViewModelMapper CreateMap<GradeItem, GradeDto>().ReverseMap() (Phase 10)
 *   - GridColumn widths verbatim from JS column width values (140, 280, 170)
 *   - GradeCode is the sole Required field, matching JS modal validation behaviour
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: FpsViewModelMapper.cs (Phase 10) already has
 *     CreateMap<GradeItem, GradeDto>().ReverseMap() and
 *     CreateMap<GradeMaintenanceViewModel, GradeDto>().ReverseMap(); verify AutoMapper
 *     profile compiles correctly after dotnet restore with these new types resolved.
 *   - TRANSFORMENGINE TODO: Confirm GridColumnType.GbpValue renders the £ currency prefix
 *     in the shared DataGrid component, matching the HTML prototype AvSalary £ display.
 *   - TRANSFORMENGINE TODO: GradeItem is also the modal partial model for _AddEditGrade.cshtml
 *     (Phase 12). Confirm GradeCode input is set read-only on edit and editable on add in the
 *     Razor partial.
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// ViewModel for the Grade Maintenance DataGrid form (frmMaintGrade).
    /// No page-level dropdowns — HTML prototype contains no &lt;select&gt; outside the grid container.
    /// Grid config is built explicitly in GradeMaintenanceController.Index() — never left as new().
    /// </summary>
    public class GradeMaintenanceViewModel
    {
        // TRANSFORMENGINE: DataGridConfig built explicitly in GradeMaintenanceController.Index()
        // Leaving as new() would render an empty grid with default Add button regardless of profile
        public DataGridConfig<GradeItem> GradeGrid { get; set; } = new DataGridConfig<GradeItem>();
    }

    /// <summary>
    /// DataGrid row model and Add/Edit modal partial model for Grade Maintenance.
    /// Properties derived from the JS DataGridComponent columns array in fps_grade_maintenance.js.
    /// Property names must match GradeDto exactly for FpsViewModelMapper AutoMapper profiles.
    /// </summary>
    public class GradeItem
    {
        // TRANSFORMENGINE: PK — maps to JS column { field:'gradeCode', header:'GradeCode', width:140 }
        // Visible in grid (JS column present); used as KeyProperty in DataGridConfig.
        // ReadOnly type in grid display; editable in Add modal, read-only on Edit modal (Phase 12).
        // [Required] matches JS modal validation: requiredFields = [gradeValidationFields[0]] (GradeCode only).
        [Required(ErrorMessage = "GradeCode is required")]
        [Display(Name = "GradeCode")]
        [GridColumn(Width = 140, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string GradeCode { get; set; } = null!;

        // TRANSFORMENGINE: maps to JS column { field:'description', header:'Description', width:280 }
        // Not required — JS modal validation does NOT include description in requiredFields array.
        // Maps to Grade.DescLong via backend EntityMapper (DescLong → Description rename).
        [Display(Name = "Description")]
        [GridColumn(Width = 280, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Description { get; set; }

        // TRANSFORMENGINE: maps to JS column { field:'avSalary', header:'AvSalary', width:170 }
        // Currency field — HTML prototype shows £ prefix; GbpValue type renders currency format.
        // Not required — JS modal validation only checks numeric format if a value is provided.
        [Display(Name = "AvSalary")]
        [GridColumn(Width = 170, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? AvSalary { get; set; }

        // TRANSFORMENGINE: FpsYear — composite PK component; NOT a visible JS column.
        // Hidden field; FPS financial year partition managed server-side via HasQueryFilter on FpsDbContext.
        // Included to support full AutoMapper round-trip with GradeDto.FpsYear.
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int? FpsYear { get; set; }
    }
}
