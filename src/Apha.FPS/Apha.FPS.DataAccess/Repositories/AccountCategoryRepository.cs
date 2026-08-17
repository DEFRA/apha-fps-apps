using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq.Expressions;

namespace Apha.FPS.DataAccess.Repositories
{
    public class AccountCategoryRepository : BaseRepository, IAccountCategoryRepository
    {
        private readonly IFpsRequestContext _requestContext;

        public AccountCategoryRepository(FpsDbContext context, IFpsRequestContext requestContext) : base(context)
        {
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        }

        public async Task<PagedData<AccountCategory>> GetAllAsync(PaginationParameters<string> query, string? filterType = null)
        {
            var queryable = BuildAccountCategoryQuery(filterType);

            queryable = ApplySorting(queryable, query.SortBy, query.Descending);
            queryable = ApplyAccountCategoryFilter(queryable, query.Filter);

            var records = await queryable.ToListAsync();

            return ApplyPaging(records, query.Page, query.PageSize);
        }

        public async Task<AccountCategory?> GetByIdAsync(string accShortName)
        {
            return await _context.AccountCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AccShortName == accShortName && a.FpsYear == _requestContext.FpsYear);
        }

        public async Task<bool> ExistsByAccShortNameAsync(string accShortName)
        {
            return await _context.AccountCategories
                .AsNoTracking()
                .AnyAsync(a => a.FpsYear == _requestContext.FpsYear
                            && EF.Functions.ILike(a.AccShortName, accShortName));
        }

        public async Task<AccountCategory> AddAsync(AccountCategory accountCategory)
        {
            ArgumentNullException.ThrowIfNull(accountCategory);
            accountCategory.FpsYear = _requestContext.FpsYear;

            _context.AccountCategories.Add(accountCategory);
            await _context.SaveChangesAsync();

            return accountCategory;
        }

        public async Task<AccountCategory> UpdateAsync(AccountCategory accountCategory)
        {
            ArgumentNullException.ThrowIfNull(accountCategory);

            var existing = await _context.AccountCategories
                .FirstOrDefaultAsync(a => a.AccShortName == accountCategory.AccShortName && a.FpsYear == _requestContext.FpsYear);

            if (existing == null)
                throw new InvalidOperationException(
                    $"Account category with AccShortName {accountCategory.AccShortName} not found");

            existing.AccountDescription = accountCategory.AccountDescription;
            existing.AccountType = accountCategory.AccountType;
            existing.ConstituentAccountCodes = accountCategory.ConstituentAccountCodes;
            existing.ProjectSpecific = accountCategory.ProjectSpecific;
            existing.RcSpecific = accountCategory.RcSpecific;

            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> DeleteAsync(string accShortName)
        {
            var entity = await _context.AccountCategories
                .FirstOrDefaultAsync(a => a.AccShortName == accShortName && a.FpsYear == _requestContext.FpsYear);

            if (entity == null)
                return false;

            _context.AccountCategories.Remove(entity);
            await _context.SaveChangesAsync();

            return true;
        }

        private IQueryable<AccountCategory> BuildAccountCategoryQuery(string? filterType)
        {
            var query = _context.AccountCategories
                .AsNoTracking()
                .Where(a => a.FpsYear == _requestContext.FpsYear);

            if (!string.IsNullOrEmpty(filterType))
            {
                query = filterType.ToLower() switch
                {
                    "rc" => query.Where(a => a.RcSpecific == -1),
                    "ps" => query.Where(a => a.ProjectSpecific == -1),
                    _ => query
                };
            }

            return query.OrderBy(a => a.AccShortName);
        }

        private static IQueryable<AccountCategory> ApplySorting(IQueryable<AccountCategory> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query;
            }

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable<AccountCategory> ApplySortingByProperty(IQueryable<AccountCategory> query, string property, bool descending)
        {
            return property switch
            {
                "accshortname" => ApplyOrder(query, i => i.AccShortName, descending),
                "accountdescription" => ApplyOrder(query, i => i.AccountDescription, descending),
                "accounttype" => ApplyOrder(query, i => i.AccountType, descending),
                "constituentaccountcodes" => ApplyOrder(query, i => i.ConstituentAccountCodes, descending),
                _ => query
            };
        }

        private static IQueryable<AccountCategory> ApplyOrder<T>(IQueryable<AccountCategory> query, Expression<Func<AccountCategory, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        private static IQueryable<AccountCategory> ApplyAccountCategoryFilter(IQueryable<AccountCategory> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return query;
            }

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
            {
                return query;
            }

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("AccShortName", out var accShortName) && accShortName != null)
                query = query.Where(x => EF.Functions.ILike(x.AccShortName, $"%{accShortName}%"));

            if (dict.TryGetValue("AccountDescription", out var accountDescription) && accountDescription != null)
                query = query.Where(x => EF.Functions.ILike(x.AccountDescription!, $"%{accountDescription}%"));

            if (dict.TryGetValue("ConstituentAccountCodes", out var constituentAccountCodes) && constituentAccountCodes != null)
                query = query.Where(x => EF.Functions.ILike(x.ConstituentAccountCodes!, $"%{constituentAccountCodes}%"));

            if (dict.TryGetValue("AccountType", out var accountType) && accountType != null)
                query = query.Where(x => EF.Functions.ILike(x.AccountType!, $"%{accountType}%"));

            return query;
        }
    }
}
