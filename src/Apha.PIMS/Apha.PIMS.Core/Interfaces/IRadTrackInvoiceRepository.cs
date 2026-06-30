using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apha.PIMS.Core.Interfaces
{
    public class RadTrackInvoiceFilter
    {
        public string? Project { get; set; }

        public string? Contract { get; set; }
               
        public int? Year { get; set; }

        public string? Program { get; set; }
    }

    public interface IRadTrackInvoiceRepository
    {
        Task<PagedData<RadTrackInvoice>> GetAllAsync(PaginationParameters<RadTrackInvoiceFilter> query);
        Task<RadTrackInvoice?> GetByIdAsync(int invoiceCounter);
        Task<RadTrackInvoice> CreateAsync(RadTrackInvoice entity);
        Task<RadTrackInvoice> UpdateAsync(RadTrackInvoice entity);
        Task<bool> DeleteAsync(int invoiceCounter);
        Task<RadTrackInvoiceTotals> GetTotalsAsync(RadTrackInvoiceFilter? filter);
        Task<bool> ExistsAsync(string? project, string? contract, string? invoiceRef, int? excludeInvoiceCounter = null);

        // Lookup queries matching Access qryShowWhichProjects_GRadtrack, tlkpYear, tblRadtrackContract
        Task<List<string>> GetProjectsAsync();
        Task<List<int>> GetYearsAsync();
        Task<List<string>> GetContractsAsync();
        Task<List<string>> GetProgramsAsync();
    }
}
