namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class CopyInvoicesDto
    {
        public int SourceMonth { get; set; }

        public int TargetMonth { get; set; }

        public List<ProjectInvoiceDto>? InvoiceRecords { get; set; }
    }
}
