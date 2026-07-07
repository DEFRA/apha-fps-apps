using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class TotalBusinessOverheadsRepository : ITotalBusinessOverheadsRepository
    {
        private readonly FpsDbContext _dbContext;

        public TotalBusinessOverheadsRepository(FpsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TotalBusinessOverheads?> GetByYearAsync(int fpsYear)
        {
            return await _dbContext.TotalBusinessOverheads
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.FpsYear == fpsYear);
        }

        public async Task<TotalBusinessOverheads> UpdateAsync(TotalBusinessOverheads entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var existing = await _dbContext.TotalBusinessOverheads
                .FirstOrDefaultAsync(e => e.FpsYear == entity.FpsYear);

            if (existing == null)
                throw new InvalidOperationException(
                    $"Total Business Overheads record for year {entity.FpsYear} not found.");

            existing.BusinessOverheads = entity.BusinessOverheads;
            await _dbContext.SaveChangesAsync();

            return existing;
        }
    }
}
