using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class ContractRepository : IContractRepository
    {
        private readonly FpsDbContext _dbContext;
        public ContractRepository(FpsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Contract>> GetAllContractsAsync()
        {
            return await _dbContext.Contracts
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
