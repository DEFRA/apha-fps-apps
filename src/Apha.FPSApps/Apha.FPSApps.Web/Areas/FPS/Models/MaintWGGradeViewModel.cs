using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Page ViewModel for the WorkgroupGrade maintenance page.
    /// </summary>
    public class MaintWGGradeViewModel
    {
        /// <summary>DataGrid configuration for the WorkgroupGrade grid.</summary>
        public DataGridConfig<MaintWGGradeItem> WGGradeGrid { get; set; } = new DataGridConfig<MaintWGGradeItem>();
    }
}
