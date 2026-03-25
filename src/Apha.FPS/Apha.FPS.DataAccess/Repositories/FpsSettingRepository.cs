using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class FpsSettingRepository : IFpsSettingRepository
    {
        private readonly FpsDbContext _dbContext;

        public FpsSettingRepository(FpsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<FpsSetting>> GetAllAsync()
        {
            return await _dbContext.TblSettings.ToListAsync();
        }
    }
}
