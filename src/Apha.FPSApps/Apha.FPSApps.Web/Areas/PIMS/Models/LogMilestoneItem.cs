using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class LogMilestoneItem
    {
        [Display(Name = "Project")]
        [GridColumn(Order = 1, Width = 100, Type = GridColumnType.ReadOnly)]
        public string? Project { get; set; }

        [Display(Name = "Number")]
        [GridColumn(Order = 2, Width = 80, Type = GridColumnType.ReadOnly)]
        public string? Number { get; set; }

        [Display(Name = "Description")]
        [GridColumn(Order = 3, Width = 200, Type = GridColumnType.ReadOnly)]
        public string? Description { get; set; }

        [Display(Name = "Date Due")]
        [GridColumn(Order = 4, Width = 100, Type = GridColumnType.Date)]
        public DateTime? DateDue { get; set; }

        [Display(Name = "Date Completed")]
        [GridColumn(Order = 5, Width = 120, Type = GridColumnType.Date)]
        public DateTime? DateCompleted { get; set; }

        [Display(Name = "Under Review?")]
        [GridColumn(Order = 6, Width = 100, Type = GridColumnType.Checkbox)]
        public short UnderSdReview { get; set; }

        [Display(Name = "On Target")]
        [GridColumn(Order = 7, Width = 90, Type = GridColumnType.Checkbox)]
        public short OnTarget { get; set; }

        [Display(Name = "Project Leader Comment")]
        [GridColumn(Order = 8, Width = 180, Type = GridColumnType.ReadOnly)]
        public string? ProjectLeaderComment { get; set; }

        [Display(Name = "CAPS Comment")]
        [GridColumn(Order = 9, Width = 180, Type = GridColumnType.ReadOnly)]
        public string? CapsComment { get; set; }

        [Display(Name = "ID Type")]
        [GridColumn(Order = 10, Width = 80, Type = GridColumnType.ReadOnly)]
        public string? IdType { get; set; }

        [GridColumn(IsVisible = false)]
        public DateTime? DateChanged { get; set; }

        [Display(Name = "Date Changed")]
        [GridColumn(Order = 11, Width = 150, Type = GridColumnType.ReadOnly)]
        public string? DateChangedDisplay => DateChanged?.ToString("dd/MM/yyyy HH:mm:ss");

        [Display(Name = "Changed By")]
        [GridColumn(Order = 12, Width = 110, Type = GridColumnType.ReadOnly)]
        public string? ChangedBy { get; set; }

        [Display(Name = "Update Type")]
        [GridColumn(Order = 13, Width = 90, Type = GridColumnType.ReadOnly)]
        public string? UpdateType { get; set; }
    }
}
