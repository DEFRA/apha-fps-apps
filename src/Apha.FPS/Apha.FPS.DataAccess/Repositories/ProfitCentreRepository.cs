using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class ProfitCentreRepository : BaseRepository, IProfitCentreRepository
    {
        private readonly FpsDbContext _dbContext;

        public ProfitCentreRepository(FpsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Returns all profit centres ordered by ProfitCentreId.
        /// </summary>
        public async Task<List<ProfitCentre>> GetProfitCentresAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.ProfitCentres
                .AsNoTracking()
                .OrderBy(x => x.ProfitCentreId)
                .ToListAsync(cancellationToken);
        }
    }
}
