using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.FPSApps.Application.Dtos.CostBook
{
    public class ProjectDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public string? PlanCategory { get; set; }
        public string? ProjectTitle { get; set; }
        public string? Programme { get; set; }
        public string? ProjectWorkgroup { get; set; }
        public double? ContractPrice { get; set; }
        public DateTime? StartDate { get; set; }
        public string? Disease { get; set; }
        public double? StartFYear { get; set; }
        public string? CustomerName { get; set; }
        public string? ContractNumber { get; set; }
        public string? SubmittedByFName { get; set; }
        public string? SubmittedByLName { get; set; }
        public DateTime? DateOfSubmission { get; set; }
        public string? PreparedBy { get; set; }
        public int? Inflation { get; set; }
        public int? FinancialYears { get; set; }
        public string? Notes { get; set; }
        public double? EuroConvRate { get; set; }
        public short? IsDefraProject { get; set; }
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
