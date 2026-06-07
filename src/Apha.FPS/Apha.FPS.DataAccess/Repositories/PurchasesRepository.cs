using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
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

        public async Task<bool> IsAuthorizedAsync(string workgroupName, string userEmail)
        {
            return await _context.BidViews
                .AnyAsync(b => b.WorkgroupName == workgroupName
                            && b.FpsYear == _requestContext.FpsYear
                            && b.UserEmail != null
                            && b.UserEmail.ToLower() == userEmail);
        }

        public async Task<List<Purchase>> GetPurchasesAsync(string workgroupName, string account)
        {
            return await _context.Purchases
                .AsNoTracking()
                .Where(p => p.WorkgroupName == workgroupName
                         && p.Account == account
                         && p.FpsYear == _requestContext.FpsYear)
                .OrderBy(p => p.ItemDescription)
                .ToListAsync();
        }

        public async Task<Purchase?> GetPurchaseByIdAsync(string workgroupName, string account, string itemDescription)
        {
            return await _context.Purchases
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.WorkgroupName == workgroupName
                    && p.Account == account
                    && p.ItemDescription == itemDescription
                    && p.FpsYear == _requestContext.FpsYear);
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

        public async Task<Purchase> UpdatePurchaseAsync(string workgroupName, string account, string itemDescriptionOld, string itemDescriptionNew, decimal amount)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var existing = await _context.Purchases
                        .FirstOrDefaultAsync(p => p.WorkgroupName == workgroupName
                            && p.Account == account
                            && p.ItemDescription == itemDescriptionOld
                            && p.FpsYear == _requestContext.FpsYear);

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

        public async Task<bool> DeletePurchaseAsync(string workgroupName, string account, string itemDescription)
        {
            var entity = await _context.Purchases
                .FirstOrDefaultAsync(p => p.WorkgroupName == workgroupName
                    && p.Account == account
                    && p.ItemDescription == itemDescription
                    && p.FpsYear == _requestContext.FpsYear);

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
    }
}
