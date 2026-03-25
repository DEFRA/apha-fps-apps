using Apha.FPS.Core.Enities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class AccountCodeRepository : IAccountCodeRepository
    {
        private readonly FpsDbContext _dbContext;
        public AccountCodeRepository(FpsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<AccountCode>> GetAllAccountCodeAsync()
        {
            return await _dbContext.AccountCodes
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
