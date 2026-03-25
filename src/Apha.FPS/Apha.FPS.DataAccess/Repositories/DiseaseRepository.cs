using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class DiseaseRepository : IDiseaseRepository
    {
        private readonly FpsDbContext _dbContext;

        public DiseaseRepository(FpsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Disease>> GetAllDiseasesAsync()
        {
            return await _dbContext.Diseases
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
