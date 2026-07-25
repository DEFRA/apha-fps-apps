using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PIMS.DataAccess.Repository
{
    public class AccessUserRepository : BaseRepository, IAccessUserRepository
    {
        private readonly PimsDbContext _dbContext;

        public AccessUserRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<AccessUser>> GetAllAsync()
        {
            return await _dbContext.AccessUsers
                .AsNoTracking()
                .OrderBy(u => u.SystemId)
                .ThenBy(u => u.NtLogin)
                .ToListAsync();
        }
        public async Task<List<AccessUser>> GetBySystemIdAsync(int systemid)
        {
            return await _dbContext.AccessUsers
                .AsNoTracking()
                .Where(u => u.SystemId == systemid)
                .OrderBy(u => u.NtLogin)
                .ToListAsync();
        }
        public async Task<List<AccessUser>> GetByNtLoginAsync(string ntlogin)
        {
            return await _dbContext.AccessUsers
                .AsNoTracking()
                .Where(u => u.NtLogin == ntlogin)
                .OrderBy(u => u.SystemId)
                .ToListAsync();
        }
        public async Task<AccessUser?> GetByIdAsync(int systemid, string ntlogin)
        {
            return await _dbContext.AccessUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.SystemId == systemid && u.NtLogin == ntlogin);
        }
        public async Task<AccessUser> AddAsync(AccessUser entity)
        {
            _dbContext.AccessUsers.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task<AccessUser> UpdateAsync(AccessUser entity)
        {
            _dbContext.AccessUsers.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task<bool> DeleteAsync(int systemid, string ntlogin)
        {
            int rowsAffected = await _dbContext.AccessUsers
                .Where(u => u.SystemId == systemid && u.NtLogin == ntlogin)
                .ExecuteDeleteAsync();

            return rowsAffected > 0;
        }
        public async Task<bool> ExistsAsync(int systemid, string ntlogin)
        {
            return await _dbContext.AccessUsers
                .AnyAsync(u => u.SystemId == systemid && u.NtLogin == ntlogin);
        }
    }
}
