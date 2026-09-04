using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq.Expressions;

namespace Apha.PACT.DataAccess.Repository
{
    public class ProjectInvoiceRepository : BaseRepository, IProjectInvoiceRepository
    {
        private readonly IFpsRequestContext _fpsRequestContext;

        public ProjectInvoiceRepository(FpsDbContext context, IFpsRequestContext fpsRequestContext) : base(context)
        {
            _fpsRequestContext = fpsRequestContext;
        }

        public async Task<PagedData<ProjectInvoice>> GetPagedProjectInvoicesAsync(PaginationParameters<string> query, string? parentProject)
        {
            IQueryable<ProjectInvoice> queryInvoices = _context.ProjectInvoices.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(parentProject))
            {
                queryInvoices = queryInvoices.Where(x => x.ProjectParent != null && x.ProjectParent.ToLower() == parentProject.ToLower());
            }

            queryInvoices = ApplyInvoiceFilter(queryInvoices, query.Filter);
            queryInvoices = (IQueryable<ProjectInvoice>)ApplySorting(queryInvoices, query.SortBy, query.Descending);

            return await ApplyPaging(queryInvoices, query.Page, query.PageSize);
        }

        public async Task<decimal> GetTotalAmountAsync(string? parentProject)
        {
            IQueryable<ProjectInvoice> query = _context.ProjectInvoices.AsNoTracking();
            if (!string.IsNullOrEmpty(parentProject))
                query = query.Where(i => i.ProjectParent == parentProject);
            return (await query.SumAsync(i => i.Amount)) ?? 0m;
        }

        public async Task<ProjectInvoice?> GetByIdAsync(int invoiceCounter)
        {
            return await _context.ProjectInvoices
                .AsNoTracking()
                      .FirstOrDefaultAsync(i => i.InvoiceCounter == invoiceCounter);
        }

        public async Task<ProjectInvoice> CreateAsync(ProjectInvoice entity)
        {
            entity.FpsYear = _fpsRequestContext.FpsYear;

            await _context.ProjectInvoices.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<ProjectInvoice> UpdateAsync(ProjectInvoice entity)
        {
            entity.FpsYear = _fpsRequestContext.FpsYear;
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int invoiceCounter)
        {
            ProjectInvoice? entity = await _context.ProjectInvoices
                .FirstOrDefaultAsync(i => i.InvoiceCounter == invoiceCounter && i.FpsYear == _fpsRequestContext.FpsYear);
            if (entity == null) return false;
            _context.ProjectInvoices.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        private static IQueryable<ProjectInvoice> ApplyInvoiceFilter(IQueryable<ProjectInvoice> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter)) return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null) return query;

            IDictionary<string, object> dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("ProjectParent", out object? projectParent) && projectParent != null)
                query = query.Where(x => EF.Functions.ILike(x.ProjectParent, $"%{projectParent}%"));

            if (dict.TryGetValue("Month", out object? month) && month != null && int.TryParse(month.ToString(), out int monthVal))
                query = query.Where(x => x.Month == monthVal);

            if (dict.TryGetValue("Detail", out object? detail) && detail != null)
                query = query.Where(x => x.Detail != null && EF.Functions.ILike(x.Detail, $"%{detail}%"));

            return query;
        }

        private static IQueryable ApplySorting(IQueryable<ProjectInvoice> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(e => e.InvoiceCounter);

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplySortingByProperty(IQueryable<ProjectInvoice> query, string property, bool descending)
        {
            return property switch
            {
                "projectparent" => ApplyOrder(query, i => i.ProjectParent, descending),
                "month" => ApplyOrder(query, i => i.Month, descending),
                "amount" => ApplyOrder(query, i => i.Amount, descending),
                "costofwork" => ApplyOrder(query, i => i.CostOfWork, descending),
                "wip" => ApplyOrder(query, i => i.Wip, descending),
                "profitloss" => ApplyOrder(query, i => i.ProfitLoss, descending),
                "detail" => ApplyOrder(query, i => i.Detail, descending),
                "invoicecounter" => ApplyOrder(query, i => i.InvoiceCounter, descending),
                _ => query.OrderBy(e => e.InvoiceCounter)
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<ProjectInvoice> query, Expression<Func<ProjectInvoice, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        public async Task<List<MonthlyInvoicesSummary>> GetMonthlyInvoicesSummaryAsync(PaginationParameters<string> parameters)
        {
            IQueryable<MonthlyInvoicesSummary> query = _context.MonthlyInvoicesSummary.AsNoTracking();

            // Parse filter JSON from DataGrid: {"Program":"ADMIN","ParentProject":"AH"}
            if (!string.IsNullOrWhiteSpace(parameters.Filter))
            {
                dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(parameters.Filter);
                if (filterModel != null)
                {
                    IDictionary<string, object> dict = (IDictionary<string, object>)filterModel;

                    if (dict.TryGetValue("Program", out object? program) && program != null)
                        query = query.Where(x => EF.Functions.ILike(x.Program, $"%{program}%"));

                    if (dict.TryGetValue("ParentProject", out object? parentProject) && parentProject != null)
                        query = query.Where(x => EF.Functions.ILike(x.ParentProject, $"%{parentProject}%"));
                }
            }

            // Always order raw rows by Program, Project, Month so grouping is stable
            return await query
                .OrderBy(x => x.Program)
                .ThenBy(x => x.ParentProject)
                .ThenBy(x => x.Month)
                .ToListAsync();
        }

        public async Task<HashSet<string>> GetValidProjectsAsync()
        {
            var fpsYear = _fpsRequestContext.FpsYear;
            return await _context.Projects
                .AsNoTracking()
                .Where(p => p.FpsYear == fpsYear)
                .Select(p => p.ParentProject)
                .ToHashSetAsync(StringComparer.OrdinalIgnoreCase);
        }

        public int GetCurrentFpsYear() => _fpsRequestContext.FpsYear;

        public async Task<PagedData<InvoiceImportRow>> GetFailedInvoiceImportAsync(PaginationParameters<string> query, string importedBy)
        {
            IQueryable<ProjectInvoiceStaging> failedQuery = _context.ProjectInvoiceStagings
                .AsNoTracking()
                .Where(x => x.ImportedBy == importedBy && x.IsPassed == false);

            failedQuery = ApplyFailedInvoiceFilter(failedQuery, query.Filter);
            failedQuery = ApplyFailedInvoiceSorting(failedQuery, query.SortBy, query.Descending);

            IQueryable<InvoiceImportRow> rows = failedQuery
                .Select(x => new InvoiceImportRow
                {
                    Id = x.Id,
                    ProjectParent = x.ProjectParent,
                    Month = x.Month,
                    Amount = x.Amount,
                    CostOfWork = x.CostOfWork,
                    Wip = x.Wip,
                    ProfitLoss = x.ProfitLoss,
                    Detail = x.Detail,
                    Type = x.Type,
                    ValidationFailure = x.ValidationFailure,
                    ImportedDate = x.ImportedDate
                });

            return await ApplyPaging(rows, query.Page, query.PageSize);
        }

        public async Task<ProjectInvoiceStaging?> GetFailedInvoiceImportByIdAsync(int id, string importedBy)
        {
            return await _context.ProjectInvoiceStagings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.ImportedBy == importedBy);
        }

        public async Task<bool> DeleteFailedInvoiceImportByIdAsync(int id, string importedBy)
        {
            var entity = await _context.ProjectInvoiceStagings
                .FirstOrDefaultAsync(s => s.Id == id && s.ImportedBy == importedBy);
            if (entity == null) return false;
            _context.ProjectInvoiceStagings.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> DeleteFailedInvoiceImportByUserAsync(string importedBy)
        {
            var rows = await _context.ProjectInvoiceStagings
                .Where(x => x.ImportedBy == importedBy)
                .ToListAsync();

            if (rows.Count == 0)
                return 0;

            _context.ProjectInvoiceStagings.RemoveRange(rows);
            return await _context.SaveChangesAsync();
        }

        public async Task<InvoiceImportResult> ImportInvoiceAsync(List<ProjectInvoice> passedRows, List<ProjectInvoiceStaging> failedRows)
        {
            if (passedRows.Count == 0 && failedRows.Count == 0)
                return new InvoiceImportResult { PassedCount = 0, FailedCount = 0 };

            if (passedRows.Count > 0)
                await _context.ProjectInvoices.AddRangeAsync(passedRows);

            if (failedRows.Count > 0)
                await _context.ProjectInvoiceStagings.AddRangeAsync(failedRows);

            await _context.SaveChangesAsync();

            return new InvoiceImportResult
            {
                PassedCount = passedRows.Count,
                FailedCount = failedRows.Count
            };
        }

        public async Task UpdateFailedInvoiceImportRecordsAsync(List<ProjectInvoiceStaging> records)
        {
            foreach (var record in records)
            {
                _context.Entry(record).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteFailedInvoiceImportByIdsAsync(List<int> ids, string importedBy)
        {
            var rows = await _context.ProjectInvoiceStagings
                .Where(s => ids.Contains(s.Id) && s.ImportedBy == importedBy)
                .ToListAsync();

            if (rows.Count > 0)
            {
                _context.ProjectInvoiceStagings.RemoveRange(rows);
                await _context.SaveChangesAsync();
            }
        }

        private static IQueryable<ProjectInvoiceStaging> ApplyFailedInvoiceFilter(IQueryable<ProjectInvoiceStaging> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter)) return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null) return query;

            IDictionary<string, object> dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("ProjectParent", out object? projectParent) && projectParent != null)
                query = query.Where(x => x.ProjectParent != null && EF.Functions.ILike(x.ProjectParent, $"%{projectParent}%"));

            if (dict.TryGetValue("Month", out object? month) && month != null)
                query = query.Where(x => x.Month != null && EF.Functions.ILike(x.Month, $"%{month}%"));

            return query;
        }

        private static IQueryable<ProjectInvoiceStaging> ApplyFailedInvoiceSorting(IQueryable<ProjectInvoiceStaging> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(e => e.Id);

            return sortBy.ToLower() switch
            {
                "id" => descending ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id),
                "projectparent" => descending ? query.OrderByDescending(e => e.ProjectParent) : query.OrderBy(e => e.ProjectParent),
                "month" => descending ? query.OrderByDescending(e => e.Month) : query.OrderBy(e => e.Month),
                "amount" => descending ? query.OrderByDescending(e => e.Amount) : query.OrderBy(e => e.Amount),
                "costofwork" => descending ? query.OrderByDescending(e => e.CostOfWork) : query.OrderBy(e => e.CostOfWork),
                "wip" => descending ? query.OrderByDescending(e => e.Wip) : query.OrderBy(e => e.Wip),
                "profitloss" => descending ? query.OrderByDescending(e => e.ProfitLoss) : query.OrderBy(e => e.ProfitLoss),
                "detail" => descending ? query.OrderByDescending(e => e.Detail) : query.OrderBy(e => e.Detail),
                "type" => descending ? query.OrderByDescending(e => e.Type) : query.OrderBy(e => e.Type),
                "validationfailure" => descending ? query.OrderByDescending(e => e.ValidationFailure) : query.OrderBy(e => e.ValidationFailure),
                _ => query.OrderBy(e => e.Id)
            };
        }
    }
}
