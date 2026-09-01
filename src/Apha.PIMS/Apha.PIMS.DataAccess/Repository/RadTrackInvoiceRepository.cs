using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq.Expressions;

namespace Apha.PIMS.DataAccess.Repository
{
    public class RadTrackInvoiceRepository : BaseRepository, IRadTrackInvoiceRepository
    {
        private readonly PimsDbContext _dbContext;

        public RadTrackInvoiceRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<PagedData<RadTrackInvoice>> GetAllAsync(PaginationParameters<RadTrackInvoiceFilter> parameters)
        {
            IQueryable<RadTrackInvoice> query = _dbContext.RadTrackInvoices.AsNoTracking();

            query = BuildFilterQuery(query, parameters.Filter);

            query = BuildProgramFilterQuery(query, parameters.Filter?.Program);
           
            query = ApplyColumnFilter(query, parameters.Search);

            query = ApplySorting(query, parameters.SortBy, parameters.Descending);

            return await ApplyPaging(query, parameters.Page, parameters.PageSize);
        }

        public async Task<RadTrackInvoice?> GetByIdAsync(int invoiceCounter)
            => await _dbContext.RadTrackInvoices
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.InvoiceCounter == invoiceCounter);

        public async Task<RadTrackInvoice> CreateAsync(RadTrackInvoice entity)
        {
            entity.InvoiceCounter = 0;
            _dbContext.RadTrackInvoices.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<RadTrackInvoice> UpdateAsync(RadTrackInvoice entity)
        {
            _dbContext.RadTrackInvoices.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int invoiceCounter)
        {
            int rows = await _dbContext.RadTrackInvoices
                .Where(i => i.InvoiceCounter == invoiceCounter)
                .ExecuteDeleteAsync();
            return rows > 0;
        }

        public async Task<RadTrackInvoiceTotals> GetTotalsAsync(RadTrackInvoiceFilter? filter, string? search = null)
        {
            IQueryable<RadTrackInvoice> query = _dbContext.RadTrackInvoices.AsNoTracking();

            query = BuildFilterQuery(query, filter);
            query = BuildProgramFilterQuery(query, filter?.Program);
            query = ApplyColumnFilter(query, search);
            var totals = await query
                .GroupBy(_ => 1)
                .Select(g => new RadTrackInvoiceTotals
                {
                    TotalPlannedAmount = g.Sum(i => i.PlannedAmount ?? 0.0),
                    TotalDueAmount     = g.Sum(i => i.DueAmount ?? 0.0),
                    TotalActualAmount  = g.Sum(i => i.ActualAmount ?? 0.0)
                })
                .FirstOrDefaultAsync();
            return totals ?? new RadTrackInvoiceTotals
            {
                TotalPlannedAmount = 0.0,
                TotalDueAmount     = 0.0,
                TotalActualAmount  = 0.0
            };
        }

        public async Task<bool> ExistsAsync(
            string? project,
            string? contract,
            string? invoiceRef,
            int? excludeInvoiceCounter = null)
        {
            IQueryable<RadTrackInvoice> query = _dbContext.RadTrackInvoices.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(project))
                query = query.Where(i => i.Project == project);

            if (!string.IsNullOrWhiteSpace(contract))
                query = query.Where(i => i.Contract == contract);

            if (!string.IsNullOrWhiteSpace(invoiceRef))
                query = query.Where(i => i.InvoiceRef == invoiceRef);

            if (excludeInvoiceCounter.HasValue)
                query = query.Where(i => i.InvoiceCounter != excludeInvoiceCounter.Value);

            return await query.AnyAsync();
        }


        public async Task<List<string>> GetProjectsAsync()
            => await _dbContext.ProjectRadTrackData.AsNoTracking()
                .Select(rd => rd.Parentproject)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();

        
        public async Task<List<int>> GetYearsAsync()
        {
            var years = await _dbContext.Years.AsNoTracking()
                .Select(y => y.Value)
                .ToListAsync();
            if (years.Count == 0)
                return years;
            int max = years.Max();
            years.Add(max + 1);
            years.Add(max + 2);
            years.Add(max + 3);
            return years.Distinct().OrderBy(y => y).ToList();
        }

        public async Task<List<string>> GetContractsAsync()
            => await _dbContext.RadTrackContracts.AsNoTracking()
                .Select(c => c.Contract)
                .OrderBy(c => c)
                .ToListAsync();

        public async Task<List<string>> GetProgramsAsync()
            => await (from rd in _dbContext.ProjectRadTrackData.AsNoTracking()
                      join vp in _dbContext.ProjectLatestDetails.AsNoTracking()
                          on rd.Parentproject equals vp.ParentProject
                      where vp.Program != null
                      orderby vp.Program
                      select vp.Program!)
                     .Distinct()
                     .ToListAsync();

        // ── Private helpers ──────────────────────────────────────────────────────────────

        private static IQueryable<RadTrackInvoice> BuildFilterQuery(
            IQueryable<RadTrackInvoice> query,
            RadTrackInvoiceFilter? filter)
        {
            if (filter == null)
                return query;

            if (!string.IsNullOrWhiteSpace(filter.Project))
                query = query.Where(i => i.Project == filter.Project);

            if (!string.IsNullOrWhiteSpace(filter.Contract))
                query = query.Where(i => i.Contract == filter.Contract);

            if (filter.Year.HasValue && filter.Year.Value > 0)
                query = query.Where(i => i.DueDate != null && i.DueDate.Value.Year == filter.Year.Value);

            return query;
        }

        private IQueryable<RadTrackInvoice> BuildProgramFilterQuery(
            IQueryable<RadTrackInvoice> query,
            string? program)
        {
            if (string.IsNullOrWhiteSpace(program))
                return query;

            var matchingProjects = _dbContext.MyTlkpProjects
                .AsNoTracking()
                .Where(p => p.Program == program)
                .Select(p => p.Parentproject);

            return query.Where(i => matchingProjects.Contains(i.Project));
        }

        
        private static IQueryable<RadTrackInvoice> ApplySorting(
            IQueryable<RadTrackInvoice> query,
            string? sortBy,
            bool descending)
        {
            return sortBy?.ToLowerInvariant() switch
            {
                "project"            => ApplyOrder(query, i => i.Project,            descending),
                "contract"           => ApplyOrder(query, i => i.Contract,           descending),
                "plannedamount"      => ApplyOrder(query, i => i.PlannedAmount,      descending),
                "dueamount"          => ApplyOrder(query, i => i.DueAmount,          descending),
                "duedate"            => ApplyOrder(query, i => i.DueDate,            descending),
                "actualamount"       => ApplyOrder(query, i => i.ActualAmount,       descending),
                "dateinvoiced"       => ApplyOrder(query, i => i.DateInvoiced,       descending),
                "datejobsheetraised" => ApplyOrder(query, i => i.DateJobsheetRaised, descending),
                "invoiceref"         => ApplyOrder(query, i => i.InvoiceRef,         descending),
                "invoicepaid"        => ApplyOrder(query, i => i.InvoicePaid,        descending),
                _                    => ApplyOrder(query, i => i.InvoiceCounter,     descending: true)
            };
        }

        private static IQueryable<RadTrackInvoice> ApplyOrder<T>(
            IQueryable<RadTrackInvoice> query,
            Expression<Func<RadTrackInvoice, T>> keySelector,
            bool descending)
            => descending
                ? query.OrderByDescending(keySelector)
                : query.OrderBy(keySelector);

        private static IQueryable<RadTrackInvoice> ApplyColumnFilter(
            IQueryable<RadTrackInvoice> query,
            string? filterJson)
        {
            if (string.IsNullOrWhiteSpace(filterJson) || filterJson == "{}")
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filterJson);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("Project", out var project) && project != null)
            {
                string val = project.ToString()!;
                query = query.Where(x => EF.Functions.ILike(x.Project!, $"%{val}%"));
            }

            if (dict.TryGetValue("Contract", out var contract) && contract != null)
            {
                string val = contract.ToString()!;
                query = query.Where(x => EF.Functions.ILike(x.Contract!, $"%{val}%"));
            }

            if (dict.TryGetValue("InvoiceRef", out var invoiceRef) && invoiceRef != null)
            {
                string val = invoiceRef.ToString()!;
                query = query.Where(x => EF.Functions.ILike(x.InvoiceRef!, $"%{val}%"));
            }

            return query;
        }
    }
}
