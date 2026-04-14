using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Repository
{
    public class TestorProductRepository : BaseRepository, ITestorProductRepository
    {
        public TestorProductRepository(FpsDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TestorProduct>> GetAllAsync()
        {
            return await _context.TestorProducts
                .AsNoTracking()
                .OrderBy(t => t.ItemCode)
                .ToListAsync();
        }
    }
}
