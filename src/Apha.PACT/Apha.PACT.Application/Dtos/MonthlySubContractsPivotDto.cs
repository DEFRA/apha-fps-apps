using Apha.PACT.Application.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PACT.Application.Dtos
{
    public class MonthlySubContractsPivotDto
    {
        public List<int> Months { get; set; } = [];
        public List<MonthlySubContractsSummaryDto> Rows { get; set; } = [];
        public PaginationDto Pagination { get; set; } = new();
    }
}
