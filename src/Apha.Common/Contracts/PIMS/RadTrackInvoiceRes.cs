using System;

namespace Apha.Common.Contracts.PIMS
{
    public class RadTrackInvoiceRes
    {        
        public int InvoiceCounter { get; set; }
        public string? Project { get; set; }
        public string? Contract { get; set; }
        public double? PlannedAmount { get; set; }
        public double? DueAmount { get; set; }
        public DateTime? DueDate { get; set; }
        public double? ActualAmount { get; set; }
        public DateTime? DateJobsheetRaised { get; set; }
        public string? InvoiceRef { get; set; }
        public short InvoicePaid { get; set; }
        public DateTime? DateInvoiced { get; set; }
    }
}
