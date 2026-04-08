using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.FPSApps.Application.Dtos.CostBook
{
    public class ProjectFilterDto
    {
        public string? ContractFilter { get; set; }
        public string? SubmittedByFilter { get; set; }
        public string? SearchTerm { get; set; }
        public int Year { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 5;
    }
}
