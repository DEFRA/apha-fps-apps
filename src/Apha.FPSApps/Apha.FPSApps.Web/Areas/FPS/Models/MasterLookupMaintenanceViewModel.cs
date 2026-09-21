using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// ViewModel for the generic Master Lookup maintenance page.
    /// </summary>
    public class MasterLookupMaintenanceViewModel
    {
        /// <summary>
        /// List of registered lookup table names shown in the left-side navigation.
        /// </summary>
        public List<string> TableNames { get; set; } = new List<string>();

        /// <summary>
        /// The currently selected lookup table (null when none is selected yet).
        /// </summary>
        public string? SelectedTable { get; set; }

        /// <summary>
        /// DataGrid configuration for the selected lookup table's values.
        /// </summary>
        public DataGridConfig<LookupItemViewModel>? ItemGrid { get; set; }
    }

    /// <summary>
    /// ViewModel for a single value within a lookup table grid.
    /// </summary>
    public class LookupItemViewModel
    {
        /// <summary>
        /// The lookup value (primary key of the underlying single-column table).
        /// </summary>
        [Display(Name = "Value")]
        [GridColumn(Width = 400, Type = GridColumnType.Text, IsFilterable = true)]
        [Required(ErrorMessage = "Value is required")]
        [StringLength(100, ErrorMessage = "Value cannot exceed 100 characters")]
        public string Value { get; set; } = string.Empty;
    }
}
