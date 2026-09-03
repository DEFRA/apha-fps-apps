using Apha.FPSApps.Application.Common;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Apha.FPSApps.Web.Validation;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class WorkGroupEmployeeStaffItem
    {
        [Display(Name = "PACTId")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? PactId { get; set; }

        [Display(Name = "Staff Name")]
        [GridColumn(Width = 220, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Name { get; set; }

        [Display(Name = "WG Grade")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? WorkGroupGrade { get; set; }

        [Display(Name = "Status")]
        [Required(ErrorMessage = "Status is required")]
        [AllowedValues(StaffStatus.Active, StaffStatus.Inactive, ErrorMessage = "Status must be either A or I")]
        [GridColumn(Width = 70, Type = GridColumnType.Text, IsFilterable = false)]
        public string PersonStatus { get; set; } = null!;

        [Display(Name = "Class")]
        [GridColumn(Width = 70, Type = GridColumnType.Text, IsFilterable = false)]
        public string? PersonClass { get; set; }

        [Display(Name = "HrsPaid")]
        [Required(ErrorMessage = "HrsPaid is required")]
        [NonFinancialRange]
        [GridColumn(Width = 80, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double HrsPaid { get; set; }

        [Display(Name = "Leave")]
        [Required(ErrorMessage = "Leave is required")]
        [NonFinancialRange]
        [GridColumn(Width = 70, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double Leave { get; set; }

        [Display(Name = "Sick Special")]
        [Required(ErrorMessage = "Sick Special is required")]
        [NonFinancialRange]
        [GridColumn(Width = 95, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double SickSpecial { get; set; }

        [Display(Name = "HrsAvail")]
        [Required(ErrorMessage = "HrsAvail is required")]
        [NonFinancialRange]
        [GridColumn(Width = 90, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double HrsAvail { get; set; }

        [Display(Name = "Available?")]
        [GridColumn(Width = 90, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public bool MakeAvailable { get; set; }

        [Display(Name = "Time recorder?")]
        [GridColumn(Width = 105, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public bool TimeRecorder { get; set; }

        [Display(Name = "Start Date")]
        [GridColumn(Width = 110, Type = GridColumnType.Date, IsFilterable = false)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [GridColumn(Width = 110, Type = GridColumnType.Date, IsFilterable = false)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Hours per week")]
        [NonFinancialRange]
        [GridColumn(Width = 115, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double? HoursPerWeek { get; set; }

        [Display(Name = "SP No")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsVisible = false, IsFilterable = false)]
        public string? SpNumber { get; set; }

        [GridColumn(IsVisible = false)]
        public List<WorkGroupStaffLookupItem> StaffLookupOptions { get; set; } = [];

        [GridColumn(IsVisible = false)]
        public List<string> WgGradeOptions { get; set; } = [];
    }

    public class WorkGroupStaffLookupItem
    {
        public string PactId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string SpNumber { get; set; } = string.Empty;
    }
}
