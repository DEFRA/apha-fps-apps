namespace Apha.Common.Contracts.PACT
{
    public class CopyInvoicesReq
    {
        public int SourceMonth { get; set; }
        public int TargetMonth { get; set; }
        public List<int>? InvoiceIds { get; set; }
    }
}
