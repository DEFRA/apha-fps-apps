using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class PurchasesRepository : BaseRepository, IPurchasesRepository
    {
        private readonly IFpsRequestContext _requestContext;

        public PurchasesRepository(FpsDbContext context, IFpsRequestContext requestContext) : base(context)
        {
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        }

        public async Task<List<Purchase>> GetPurchasesAsync(string WorkGroupName, string account)
        {
            var userEmail = _requestContext.UserEmailId.ToLower();

            var authorisedAccounts = await _context.BidViews
                .AsNoTracking()
                .Where(b => b.WorkGroupName == WorkGroupName
                         && b.UserEmail != null
                         && b.UserEmail.ToLower() == userEmail)
                .Select(b => b.Account)
                .ToListAsync();

            return await _context.Purchases
                .AsNoTracking()
                .Where(p => p.WorkGroupName == WorkGroupName
                         && p.Account == account
                         && authorisedAccounts.Contains(p.Account))
                .OrderBy(p => p.ItemDescription)
                .ToListAsync();
        }

        public async Task<PagedData<Purchase>> GetPurchasesPagedAsync(
            PaginationParameters<string> query, string WorkGroupName, string account)
        {
            var userEmail = _requestContext.UserEmailId.ToLower();

            var authorisedAccounts = await _context.BidViews
                .AsNoTracking()
                .Where(b => b.WorkGroupName == WorkGroupName
                         && b.UserEmail != null
                         && b.UserEmail.ToLower() == userEmail)
                .Select(b => b.Account)
                .ToListAsync();

            var q = _context.Purchases
                .AsNoTracking()
                .Where(p => p.WorkGroupName == WorkGroupName
                         && p.Account == account
                         && authorisedAccounts.Contains(p.Account))
                .AsQueryable();

            q = ApplyPurchaseFilter(q, query.Filter);
            q = ApplyPurchaseSort(q, query.SortBy, query.Descending);

            var result = await q.ToListAsync();
            return base.ApplyPaging(result, query.Page > 0 ? query.Page : 1, query.PageSize > 0 ? query.PageSize : 10);
        }

        public async Task<Purchase?> GetPurchaseByIdAsync(string WorkGroupName, string account, string itemDescription)
        {
            return await _context.Purchases
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.WorkGroupName == WorkGroupName
                    && p.Account == account
                    && p.ItemDescription == itemDescription);
        }

        public async Task<Purchase> AddPurchaseAsync(Purchase purchase)
        {
            purchase.FpsYear = _requestContext.FpsYear;

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Purchases.Add(purchase);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return purchase;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<Purchase> UpdatePurchaseAsync(string WorkGroupName, string account, string itemDescriptionOld, string itemDescriptionNew, decimal amount)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var existing = await _context.Purchases
                        .FirstOrDefaultAsync(p => p.WorkGroupName == WorkGroupName
                            && p.Account == account
                            && p.ItemDescription == itemDescriptionOld);

                    existing!.ItemDescription = itemDescriptionNew;
                    existing.Amount = amount;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return existing;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<bool> DeletePurchaseAsync(string WorkGroupName, string account, string itemDescription)
        {
            var entity = await _context.Purchases
                .FirstOrDefaultAsync(p => p.WorkGroupName == WorkGroupName
                    && p.Account == account
                    && p.ItemDescription == itemDescription);

            if (entity == null)
                return false;

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Purchases.Remove(entity);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        private static IQueryable<Purchase> ApplyPurchaseFilter(IQueryable<Purchase> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return query;

            if (filter.TrimStart().StartsWith('{'))
            {
                try
                {
                    var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(filter,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (dict != null)
                    {
                        if (dict.TryGetValue("WorkGroupName", out var wg) && !string.IsNullOrWhiteSpace(wg))
                            query = query.Where(p => EF.Functions.ILike(p.WorkGroupName, $"%{wg}%"));
                        if (dict.TryGetValue("Account", out var acc) && !string.IsNullOrWhiteSpace(acc))
                            query = query.Where(p => EF.Functions.ILike(p.Account, $"%{acc}%"));
                        if (dict.TryGetValue("ItemDescription", out var item) && !string.IsNullOrWhiteSpace(item))
                            query = query.Where(p => EF.Functions.ILike(p.ItemDescription, $"%{item}%"));
                    }
                }
                catch { }
            }
            return query;
        }

        private static IQueryable<Purchase> ApplyPurchaseSort(IQueryable<Purchase> query, string? sortBy, bool descending)
        {
            return sortBy?.ToLower() switch
            {
                "workgroupname"   => descending ? query.OrderByDescending(p => p.WorkGroupName)   : query.OrderBy(p => p.WorkGroupName),
                "account"         => descending ? query.OrderByDescending(p => p.Account)         : query.OrderBy(p => p.Account),
                "itemdescription" => descending ? query.OrderByDescending(p => p.ItemDescription) : query.OrderBy(p => p.ItemDescription),
                "amount"          => descending ? query.OrderByDescending(p => p.Amount)          : query.OrderBy(p => p.Amount),
                _                 => query.OrderBy(p => p.ItemDescription)
            };
        }
    }
}
