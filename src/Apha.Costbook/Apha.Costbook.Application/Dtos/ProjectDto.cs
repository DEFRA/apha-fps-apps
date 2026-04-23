using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.Costbook.Application.Dtos
{
    public class ProjectDto
    {
        public string ProjectId { get; set; } = null!;

        public string? PlanCategory { get; set; }

        public string? ProjectTitle { get; set; }

        public string? Programme { get; set; }

        public string? ProjectWorkgroup { get; set; }   
        public double? Contractprice { get; set; }

        public DateOnly? Startdate { get; set; }

        public string? Disease { get; set; }

        public double? StartFYear { get; set; }

        public string? CustomerName { get; set; }

        public string? ContractNumber { get; set; }

        public string? SubmittedByFName { get; set; }

        public string? SubmittedByLName { get; set; }
        public DateOnly? DateOfSubmission { get; set; }

        public string? PreparedBy { get; set; }

        public int? Inflation { get; set; }

        public int? FinancialYears { get; set; }

        public string? Notes { get; set; }
            
        public double? Euroconvrate { get; set; }

        public short? IsDefraProject { get; set; }
    }
}
