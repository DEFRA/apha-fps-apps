namespace Apha.PACT.Application.Dtos
{
    public class CopyInvoicesDto
    {
        public int SourceMonth { get; set; }
        public int TargetMonth { get; set; }
        public List<int>? InvoiceIds { get; set; }
        public List<ProjectInvoiceDto>? InvoiceRecords { get; set; }
    }
}
