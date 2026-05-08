using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PACT.Core.Entities
{
    public class MonthlyInvoicesSummary
    {
        public required int FpsYear { get; set; }

        public required string Program { get; set; }

        public required string ParentProject { get; set; }

        public decimal? MonthlyAmount { get; set; }

        public required int Month { get; set; }
    }
}
