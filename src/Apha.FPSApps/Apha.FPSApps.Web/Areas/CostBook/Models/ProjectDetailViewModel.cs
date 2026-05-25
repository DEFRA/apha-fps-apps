using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models
{
    public class ProjectDetailViewModel
    {
        public string ProjectId { get; set; } = string.Empty;
        public string? Plancat { get; set; }
        public string? Projecttitle { get; set; }
        public string? Programme { get; set; }
        public string? Projectworkgroup { get; set; }
        public double? Contractprice { get; set; }
        public DateTime?  Startdate { get; set; }
        public string? Disease { get; set; }
        public double? Startfyear { get; set; }
        public string? CustomerName { get; set; }
        public string? ContractNumber { get; set; }
        public string? Submittedbyfname { get; set; }
        public string? Submittedbylname { get; set; }
        public DateTime? DateOfSubmission { get; set; }
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
        public DateTime? ModifiedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
