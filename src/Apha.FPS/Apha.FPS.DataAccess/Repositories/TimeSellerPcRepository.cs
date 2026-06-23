using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    /// <summary>
    /// Repository for the Income/Contribution from Time Sales summary view (frmTimeSellerPC).
    /// </summary>
    public class TimeSellerPcRepository : ITimeSellerPcRepository
    {
        private readonly FpsDbContext _dbContext;

        public TimeSellerPcRepository(FpsDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <inheritdoc/>
        public async Task<List<TimeSellerPcView>> GetBySellingPcAsync(
            string sellingPc,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sellingPc);

            return await _dbContext.VQryFrmTimeSellerPcViews
                .AsNoTracking()
                .Where(x => x.SellingPc == sellingPc)
                .OrderBy(x => x.WorkGroup)
                .ThenBy(x => x.WgGrade)
                .ToListAsync(cancellationToken);
        }
    }
}
