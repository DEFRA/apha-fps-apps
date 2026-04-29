using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class TestorProductRepository : ITestorProductRepository
    {
        private readonly FpsDbContext _dbContext;

        public TestorProductRepository(FpsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<TestorProduct>> GetAllTestorProductsAsync()
        {
            return await _dbContext.TestorProducts
                .AsNoTracking()
                .OrderBy(t => t.ItemCode)
                .ToListAsync();
        }
    }
}
