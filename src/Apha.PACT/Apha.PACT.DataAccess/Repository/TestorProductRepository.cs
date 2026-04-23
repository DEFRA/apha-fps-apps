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

        public async Task<Dictionary<string, string?>> GetDescriptionsByCodesAsync(IEnumerable<string> itemCodes)
        {
            var codes = itemCodes.ToList();
            return await _context.TestorProducts
                .AsNoTracking()
                .Where(t => codes.Contains(t.ItemCode))
                .ToDictionaryAsync(t => t.ItemCode, t => t.ItemDescription);
        }
    }
}
