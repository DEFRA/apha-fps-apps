using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class BudgetBidsRepository : BaseRepository, IBudgetBidsRepository
    {
        private readonly IFpsRequestContext _requestContext;

        public BudgetBidsRepository(FpsDbContext context, IFpsRequestContext requestContext) : base(context)
        {
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        }

        public async Task<bool> IsAuthorizedAsync(string WorkGroupName)
        {
            return await _context.WorkGroupViews
                .AnyAsync(w => w.WorkGroupName == WorkGroupName
                            && w.UserEmail != null
                            && w.UserEmail.ToLower() == _requestContext.UserEmailId);
        }

        public async Task<List<BidView>> GetBidViewAsync(string workgroup)
        {
            var rows = await _context.BidViews
                .AsNoTracking()
                .Where(b => b.WorkGroupName == workgroup
                         && b.UserEmail != null && b.UserEmail.ToLower() == _requestContext.UserEmailId)
                .OrderBy(b => b.Account)
                .ToListAsync();

            return rows.DistinctBy(b => b.Account).ToList();
        }

        public async Task<Bid?> GetBidByIdAsync(string WorkGroupName, string account)
        {
            return await _context.Bids
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.WorkGroupName == WorkGroupName
                    && b.Account == account);
        }

        public async Task<Bid> AddBidAsync(Bid bid)
        {
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
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var existing = await _context.Bids
                        .FirstOrDefaultAsync(b => b.WorkGroupName == bid.WorkGroupName && b.Account == bid.Account);

                    existing!.GenBid = bid.GenBid;

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

        public async Task<bool> DeleteBidAsync(string WorkGroupName, string account)
        {
            var entity = await _context.Bids
                .FirstOrDefaultAsync(b => b.WorkGroupName == WorkGroupName && b.Account == account);

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

        public async Task<List<AccountCategory>> GetAccountCategoriesAsync()
        {
            return await _context.AccountCategories
                .AsNoTracking()
                .Where(a => a.RcSpecific == -1)
                .OrderBy(a => a.AccShortName)
                .ToListAsync();
        }
    }
}
