using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    /// <summary>
    /// Represents one row in the Work Group Time By Job Code view for a person / job-code combination,
    /// showing hours recorded against each of the 12 fiscal-year months (April – March),
    /// along with totals and percentages of standard hours. Mirrors the legacy frmCluedo1 form.
    /// </summary>
    public class WgSummarisedStaffTimeUsageRow
    {
        //[Display(Name = "Work Group")]
        //[GridColumn(Order = 1, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        //public string? WorkGroup { get; set; }

        //[Display(Name = "Name")]
        //[GridColumn(Order = 2, Width = 160, Type = GridColumnType.Text, IsFilterable = true)]
        //public string? Name { get; set; }

        //[Display(Name = "Hrs Paid")]
        //[GridColumn(Order = 3, Width = 80, Type = GridColumnType.DecimalNumber)]
        //public double? HrsPaid { get; set; }

        [Display(Name = "Project")]
        [GridColumn(Order = 4, Width = 130, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ParentProject { get; set; }

        [Display(Name = "Job Code")]
        [GridColumn(Order = 5, Width = 90, Type = GridColumnType.Text, IsFilterable = true)]
        public string? JobCode { get; set; }

        //[Display(Name = "Job Title")]
        //[GridColumn(Order = 6, Width = 160, Type = GridColumnType.Text, IsFilterable = true)]
        //public string? JobTitle { get; set; }

        [Display(Name = "Apr")]
        [GridColumn(Order = 7, Width = 55, Type = GridColumnType.DecimalNumber)]
        public double April { get; set; }

        [Display(Name = "May")]
        [GridColumn(Order = 8, Width = 55, Type = GridColumnType.DecimalNumber)]
        public double May { get; set; }

        [Display(Name = "Jun")]
        [GridColumn(Order = 9, Width = 55, Type = GridColumnType.DecimalNumber)]
        public double June { get; set; }

        [Display(Name = "Jul")]
        [GridColumn(Order = 10, Width = 55, Type = GridColumnType.DecimalNumber)]
        public double July { get; set; }

        [Display(Name = "Aug")]
        [GridColumn(Order = 11, Width = 55, Type = GridColumnType.DecimalNumber)]
        public double August { get; set; }

        [Display(Name = "Sep")]
        [GridColumn(Order = 12, Width = 55, Type = GridColumnType.DecimalNumber)]
        public double September { get; set; }

        [Display(Name = "Oct")]
        [GridColumn(Order = 13, Width = 55, Type = GridColumnType.DecimalNumber)]
        public double October { get; set; }

        [Display(Name = "Nov")]
        [GridColumn(Order = 14, Width = 55, Type = GridColumnType.DecimalNumber)]
        public double November { get; set; }

        [Display(Name = "Dec")]
        [GridColumn(Order = 15, Width = 55, Type = GridColumnType.DecimalNumber)]
        public double December { get; set; }

        [Display(Name = "Jan")]
        [GridColumn(Order = 16, Width = 55, Type = GridColumnType.DecimalNumber)]
        public double January { get; set; }

        [Display(Name = "Feb")]
        [GridColumn(Order = 17, Width = 55, Type = GridColumnType.DecimalNumber)]
        public double February { get; set; }

        [Display(Name = "Mar")]
        [GridColumn(Order = 18, Width = 55, Type = GridColumnType.DecimalNumber)]
        public double March { get; set; }

        [Display(Name = "Total Time")]
        [GridColumn(Order = 19, Width = 80, Type = GridColumnType.DecimalNumber)]
        public double TotalTime { get; set; }

        [Display(Name = "Total Cost")]
        [GridColumn(Order = 20, Width = 90, Type = GridColumnType.GbpValue)]
        public double TotalCost { get; set; }
    }
}