using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class BudgetResourceLevelRepository : BaseRepository, IBudgetResourceLevelRepository
    {
        private readonly IFpsRequestContext _requestContext;

        public BudgetResourceLevelRepository(FpsDbContext context, IFpsRequestContext requestContext) : base(context)
        {
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        }

        public async Task<List<WorkGroupView>> GetWorkGroupsAsync(string profitCentre)
        {
            return await _context.WorkGroupViews
                .AsNoTracking()
                .Where(w => w.ProfitCentre == profitCentre)
                .OrderBy(w => w.WorkgroupName)
                .ToListAsync();
        }

        public async Task<List<BidView>> GetBidViewAsync(string workgroup)
        {
            return await _context.BidViews
                .AsNoTracking()
                .Where(b => b.WorkgroupName == workgroup)
                .OrderBy(b => b.Account)
                .ToListAsync();
        }

        public async Task<Bid?> GetBidByIdAsync(string workgroupName, string account)
        {
            return await _context.Bids
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.WorkgroupName == workgroupName && b.Account == account);
        }

        public async Task<Bid> AddBidAsync(Bid bid)
        {
            ArgumentNullException.ThrowIfNull(bid);
            bid.FpsYear = _requestContext.FpsYear;

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Bids.Add(bid);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return bid;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<Bid> UpdateBidAsync(Bid bid)
        {
            ArgumentNullException.ThrowIfNull(bid);

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var existing = await _context.Bids
                        .FirstOrDefaultAsync(b => b.WorkgroupName == bid.WorkgroupName && b.Account == bid.Account);

                    if (existing == null)
                        throw new InvalidOperationException(
                            $"Bid with Workgroup '{bid.WorkgroupName}' and Account '{bid.Account}' not found.");

                    existing.GenBid = bid.GenBid;

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

        public async Task<bool> DeleteBidAsync(string workgroupName, string account)
        {
            var entity = await _context.Bids
                .FirstOrDefaultAsync(b => b.WorkgroupName == workgroupName && b.Account == account);

            if (entity == null)
                return false;

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Bids.Remove(entity);
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

        public async Task<List<Purchase>> GetPurchasesAsync(string workgroupName, string account)
        {
            return await _context.Purchases
                .AsNoTracking()
                .Where(p => p.WorkgroupName == workgroupName && p.Account == account)
                .OrderBy(p => p.ItemDescription)
                .ToListAsync();
        }

        public async Task<Purchase?> GetPurchaseByIdAsync(string workgroupName, string account, string itemDescription)
        {
            return await _context.Purchases
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.WorkgroupName == workgroupName
                    && p.Account == account
                    && p.ItemDescription == itemDescription);
        }

        public async Task<Purchase> AddPurchaseAsync(Purchase purchase)
        {
            ArgumentNullException.ThrowIfNull(purchase);

            var isAuthorized = await _context.BidViews
                .AnyAsync(b => b.WorkgroupName == purchase.WorkgroupName && b.UserEmail == _requestContext.UserEmailId);

            if (!isAuthorized)
                throw new UnauthorizedAccessException(
                    $"User does not have access to workgroup '{purchase.WorkgroupName}'.");

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

        public async Task<Purchase> UpdatePurchaseAsync(string workgroupName, string account, string itemDescriptionOld, string itemDescriptionNew, decimal amount)
        {
            var isAuthorized = await _context.BidViews
                .AnyAsync(b => b.WorkgroupName == workgroupName && b.UserEmail == _requestContext.UserEmailId);

            if (!isAuthorized)
                throw new UnauthorizedAccessException(
                    $"User does not have access to workgroup '{workgroupName}'.");

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var existing = await _context.Purchases
                        .FirstOrDefaultAsync(p => p.WorkgroupName == workgroupName
                            && p.Account == account
                            && p.ItemDescription == itemDescriptionOld);

                    if (existing == null)
                        throw new InvalidOperationException(
                            $"Purchase with Workgroup '{workgroupName}', Account '{account}' and Item Description '{itemDescriptionOld}' not found.");

                    existing.ItemDescription = itemDescriptionNew;
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

        public async Task<bool> DeletePurchaseAsync(string workgroupName, string account, string itemDescription)
        {
            var isAuthorized = await _context.BidViews
                .AnyAsync(b => b.WorkgroupName == workgroupName && b.UserEmail == _requestContext.UserEmailId);

            if (!isAuthorized)
                throw new UnauthorizedAccessException(
                    $"User does not have access to workgroup '{workgroupName}'.");

            var entity = await _context.Purchases
                .FirstOrDefaultAsync(p => p.WorkgroupName == workgroupName
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

        public async Task<List<AccountCategory>> GetAccountCategoriesAsync()
        {
            return await _context.AccountCategories
                .AsNoTracking()
                .Where(a => a.RcSpecific == -1)
                .OrderBy(a => a.AccShortName)
                .ToListAsync();
        }

        public async Task<List<ProfitCentreView>> GetProfitCentresAsync()
        {
            return await _context.ProfitCentreViews
                .AsNoTracking()
                .OrderBy(p => p.ProfitCentreId)
                .ToListAsync();
        }
    }
}
