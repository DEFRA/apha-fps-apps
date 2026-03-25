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
            return await (from contract in _dbContext.Contracts
                        join userCategory in _dbContext.UserCategories
                            on contract.Category equals userCategory.Category
                        join user in _dbContext.Users
                            on userCategory.UserId equals user.UserId
                          where user.Username == "dbo"
                          select contract).AsNoTracking()
                .ToListAsync(); 
        }
    }
}
