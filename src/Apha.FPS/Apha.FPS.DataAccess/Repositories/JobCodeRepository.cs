using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class JobCodeRepository : IJobCodeRepository
    {
        private readonly FpsDbContext _dbContext;
        public JobCodeRepository(FpsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<JobCode>> GetAllJobCodesAsync()
        {
            return await _dbContext.JobCodes
                .AsNoTracking()
                .OrderBy(j => j.JobCodeId)
                .ToListAsync();
        }


    }
}
