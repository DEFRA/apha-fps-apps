using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PIMS.DataAccess.Repository
{
    public class SettingRepository : BaseRepository, ISettingRepository
    {
        private readonly PimsDbContext _dbContext;

        public SettingRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<Setting>> GetAllSettingsAsync()
        {
            return await _dbContext.Settings
                .AsNoTracking()
                .OrderBy(s => s.Id)
                .ToListAsync();
        }
        public async Task<List<Setting>> GetAllUserUpdateableSettingsAsync()
        {
            return await _dbContext.Settings
                .AsNoTracking()
                .Where(s => s.Userupdateable)
                .OrderBy(s => s.Id)
                .ToListAsync();
        }
        public async Task<Setting?> GetSettingByIdAsync(string id)
        {
            return await _dbContext.Settings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        public async Task<Setting> UpdateSettingAsync(Setting entity)
        {
            _dbContext.Settings.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task<bool> SettingExistsAsync(string id)
        {
            return await _dbContext.Settings
                .AnyAsync(s => s.Id == id);
        }
    }
}
