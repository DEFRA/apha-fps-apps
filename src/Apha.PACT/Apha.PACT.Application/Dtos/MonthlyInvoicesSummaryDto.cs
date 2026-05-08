using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Dtos
{
    public class MonthlyInvoicesSummaryDto
    {
        public string Program { get; set; } = null!;
        public string ParentProject { get; set; } = null!;
        public Dictionary<int, decimal> MonthlyAmounts { get; set; } = [];
    }
    
}
