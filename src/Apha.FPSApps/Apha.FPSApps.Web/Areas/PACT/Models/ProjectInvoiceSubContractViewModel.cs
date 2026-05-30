using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class ProjectInvoiceSubContractViewModel
    {
        public string ParentProject { get; set; } = string.Empty;
        public bool FromPortfolio { get; set; }
        public DataGridConfig<ProjectInvoiceItem> InvoicesGrid { get; set; } = new DataGridConfig<ProjectInvoiceItem>();
        public DataGridConfig<ProjectSubContractItem> SubContractsGrid { get; set; } = new DataGridConfig<ProjectSubContractItem>();
        public decimal TotalInvoiceAmount { get; set; }
        public decimal TotalSubContractAmount { get; set; }
    }
}
