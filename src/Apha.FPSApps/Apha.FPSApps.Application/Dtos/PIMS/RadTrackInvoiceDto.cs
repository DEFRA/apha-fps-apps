
using System;

namespace Apha.FPSApps.Application.Dtos.PIMS
{    
    public class RadTrackInvoiceDto
    {
        public int InvoiceCounter { get; set; }

        public string? Project { get; set; }

        public double? PlannedAmount { get; set; }

        public double? DueAmount { get; set; }

        public DateTime? DueDate { get; set; }

        public double? ActualAmount { get; set; }

        public DateTime? DateInvoiced { get; set; }

        public string? Contract { get; set; }

        public DateTime? DateJobsheetRaised { get; set; }

        public string? InvoiceRef { get; set; }
        public short InvoicePaid { get; set; }
    }
}
