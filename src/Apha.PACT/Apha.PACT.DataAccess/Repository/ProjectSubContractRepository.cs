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
    public class ProjectSubContractRepository : BaseRepository, IProjectSubContractRepository
    {
        private readonly IFpsRequestContext _fpsRequestContext;

        public ProjectSubContractRepository(FpsDbContext context, IFpsRequestContext fpsRequestContext) : base(context)
        {
            _fpsRequestContext = fpsRequestContext;
        }

        public async Task<PagedData<ProjectSubContract>> GetPagedProjectSubContractsAsync(PaginationParameters<string> query, string? project)
        {
            IQueryable<ProjectSubContract> querySubContracts = _context.ProjectSubContracts.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(project))
            {
                querySubContracts = querySubContracts.Where(s => s.Project == project);
            }

            querySubContracts = ApplySubContractFilter(querySubContracts, query.Filter);
            querySubContracts = (IQueryable<ProjectSubContract>)ApplySorting(querySubContracts, query.SortBy, query.Descending);

            List<ProjectSubContract> result = await querySubContracts.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<decimal> GetTotalAmountAsync(string? project)
        {
            IQueryable<ProjectSubContract> query = _context.ProjectSubContracts.AsNoTracking();
            if (!string.IsNullOrEmpty(project))
                query = query.Where(s => s.Project == project);
            return (await query.SumAsync(s => s.Amount)) ?? 0m;
        }

        private static readonly IReadOnlyList<string> AnimalAcctCodes =
            new[] { "LargeAnimals", "SmallAnimals", "Mice" };

        public async Task<PagedData<ProjectSubContract>> GetFpsProjectSubContractsAsync(PaginationParameters<string> query, string? project)
        {
            IQueryable<ProjectSubContract> q = _context.ProjectSubContracts
                .AsNoTracking()
                .Where(s => AnimalAcctCodes.Contains(s.AcctCode));

            if (!string.IsNullOrEmpty(project))
                q = q.Where(s => s.Project == project);

            q = ApplySubContractFilter(q, query.Filter);
            q = (IQueryable<ProjectSubContract>)ApplySorting(q, query.SortBy, query.Descending);

            List<ProjectSubContract> result = await q.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<decimal> GetFpsProjectSubContractTotalAmountAsync(string? project)
        {
            IQueryable<ProjectSubContract> q = _context.ProjectSubContracts
                .AsNoTracking()
                .Where(s => AnimalAcctCodes.Contains(s.AcctCode));

            if (!string.IsNullOrEmpty(project))
                q = q.Where(s => s.Project == project);

            return (await q.SumAsync(s => s.Amount)) ?? 0m;
        }

        public async Task<ProjectSubContract?> GetByIdAsync(int subContCounter)
        {
            return await _context.ProjectSubContracts
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SubContCounter == subContCounter);
        }

        public async Task<ProjectSubContract> CreateAsync(ProjectSubContract entity)
        {
            entity.FpsYear = _fpsRequestContext.FpsYear;
            await _context.ProjectSubContracts.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<ProjectSubContract> UpdateAsync(ProjectSubContract entity)
        {
            entity.FpsYear = _fpsRequestContext.FpsYear;
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int subContCounter)
        {
            ProjectSubContract? entity = await _context.ProjectSubContracts
                .FirstOrDefaultAsync(s => s.SubContCounter == subContCounter && s.FpsYear == _fpsRequestContext.FpsYear);
            if (entity == null) return false;
            _context.ProjectSubContracts.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<MonthlySubContractsSummary>> GetMonthlySubContractsSummaryAsync(PaginationParameters<string> parameters)
        {
            IQueryable<MonthlySubContractsSummary> query = _context.MonthlySubContractsSummary.AsNoTracking();

            // Parse filter JSON from DataGrid: {"Program":"ADMIN","ParentProject":"AH"}
            if (!string.IsNullOrWhiteSpace(parameters.Filter))
            {
                dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(parameters.Filter);
                if (filterModel != null)
                {
                    IDictionary<string, object> dict = (IDictionary<string, object>)filterModel;

                    if (dict.TryGetValue("Program", out object? program) && program != null)
                        query = query.Where(x => x.Program.Contains(program.ToString()!));

                    if (dict.TryGetValue("ParentProject", out object? parentProject) && parentProject != null)
                        query = query.Where(x => x.ParentProject.Contains(parentProject.ToString()!));
                }
            }

            // Always order raw rows by Program, Project, Month so grouping is stable
            return await query
                .OrderBy(x => x.Program)
                .ThenBy(x => x.ParentProject)
                .ThenBy(x => x.Month)
                .ToListAsync();
        }

        private static IQueryable<ProjectSubContract> ApplySubContractFilter(IQueryable<ProjectSubContract> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter)) return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null) return query;

            IDictionary<string, object> dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("Project", out object? project) && project != null)
                query = query.Where(x => x.Project != null && x.Project.Contains(project.ToString()!));

            if (dict.TryGetValue("AcctCode", out object? acctCode) && acctCode != null)
                query = query.Where(x => x.AcctCode != null && x.AcctCode.Contains(acctCode.ToString()!));

            if (dict.TryGetValue("TestJob", out object? testJob) && testJob != null)
                query = query.Where(x => x.TestJob != null && x.TestJob.Contains(testJob.ToString()!));

            return query;
        }

        private static IQueryable ApplySorting(IQueryable<ProjectSubContract> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(e => e.SubContCounter);

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplySortingByProperty(IQueryable<ProjectSubContract> query, string property, bool descending)
        {
            return property switch
            {
                "project" => ApplyOrder(query, s => s.Project, descending),
                "month" => ApplyOrder(query, s => s.Month, descending),
                "amount" => ApplyOrder(query, s => s.Amount, descending),
                "acctcode" => ApplyOrder(query, s => s.AcctCode, descending),
                "testjob" => ApplyOrder(query, s => s.TestJob, descending),
                "subcontcounter" => ApplyOrder(query, s => s.SubContCounter, descending),
                _ => query.OrderBy(e => e.SubContCounter)
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<ProjectSubContract> query, Expression<Func<ProjectSubContract, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
    }
}
