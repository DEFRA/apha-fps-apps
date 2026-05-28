namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class CopyInvoicesRequest
    {
        public int SourceMonth { get; set; }
        public int TargetMonth { get; set; }

        public List<int>? InvoiceIds { get; set; }

        public List<AutomaticInvoiceItem>? InvoiceRecords { get; set; }
    }
}
