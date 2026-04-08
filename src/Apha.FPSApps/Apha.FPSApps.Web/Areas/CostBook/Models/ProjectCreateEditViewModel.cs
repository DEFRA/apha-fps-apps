using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models
{
    public class ProjectCreateEditViewModel
    {
        public string ProjectId { get; set; } = string.Empty;
        public string? Plancat { get; set; }
        public string? Projecttitle { get; set; }
        public string? Programme { get; set; }
        public string? Projectworkgroup { get; set; }
        public double? Contractprice { get; set; }
        public DateOnly? Startdate { get; set; }
        public string? Disease { get; set; }
        public double? Startfyear { get; set; }
        public string? CustomerName { get; set; }
        public string? ContractNumber { get; set; }
        public string? Submittedbyfname { get; set; }
        public string? Submittedbylname { get; set; }
        public DateOnly? DateOfSubmission { get; set; }
        public string? PreparedBy { get; set; }
        public int? Inflation { get; set; }
        public int? Financialyears { get; set; }
        public string? Notes { get; set; }
        public double? Euroconvrate { get; set; }
        public short? Isdefraproject { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? BudgetAmount { get; set; }
        public decimal? ActualCost { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }

        [BindNever] public List<SelectListItem> AvailablePrograms { get; set; } = new();
        [BindNever] public List<SelectListItem> AvailableCustomers { get; set; } = new();
        [BindNever] public List<SelectListItem> AvailableDiseases { get; set; } = new();
        [BindNever] public List<SelectListItem> AvailableStaff { get; set; } = new();
        [BindNever] public List<SelectListItem> AvailableContracts { get; set; } = new();       
    }
}
