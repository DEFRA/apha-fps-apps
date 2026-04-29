using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PACT.Core.Entities
{
    public class MonthlyInvoicesSummary
    {
        public required int Fpsyear { get; set; }

        public required string Program { get; set; }

        public required string Parentproject { get; set; }

        public decimal? Monthlyamount { get; set; }

        public required int Month { get; set; }
    }
}
