using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class ProjectCommentItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int CommentNo { get; set; }

        [Display(Name = "Year")]
        [GridColumn(Width = 80, Type = GridColumnType.Number)]
        public int? Year { get; set; }

        [Display(Name = "Topic")]
        [GridColumn(Width = 200, Type = GridColumnType.Text)]
        public string? Topic { get; set; }

        [Display(Name = "Comments")]
        [GridColumn(Width = 400, Type = GridColumnType.Text)]
        public string? Comment { get; set; }
    }
}
