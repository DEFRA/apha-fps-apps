using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class RecreateSummariesLogViewModel
    {
        public required DataGridConfig<RecreateSummariesLogItem> LogsGrid { get; set; }
    }

    public class RecreateSummariesLogItem
    {
        [Display(Name = "ID")]
        [GridColumn(Order = 1, Width = 80, Type = GridColumnType.Text)]
        public int Id { get; set; }

        [Display(Name = "DateDone")]
        [GridColumn(Order = 2, Width = 180, Type = GridColumnType.Text)]
        public string? DateDone { get; set; }

        [Display(Name = "UserID")]
        [GridColumn(Order = 3, Width = 120, Type = GridColumnType.Text)]
        public string? UserId { get; set; }

        [Display(Name = "User")]
        [GridColumn(Order = 4, Width = 200, Type = GridColumnType.Text)]
        public string? User { get; set; }

        [Display(Name = "Period")]
        [GridColumn(Order = 5, Width = 100, Type = GridColumnType.Text)]
        public short? Period { get; set; }
    }
}
