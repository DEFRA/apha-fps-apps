using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class SubAccountRepository : ISubAccountRepository
    {
        private readonly FpsDbContext _dbContext;

        public SubAccountRepository(FpsDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IEnumerable<SubAccount>> GetAllSubAccountsAsync()
        {
            return await _dbContext.SubAccounts
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
