using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    /// <summary>
    /// Repository for Agency operations.
    /// </summary>
    public class AgencyRepository : IAgencyRepository
    {
        private readonly FpsDbContext _context;

        public AgencyRepository(FpsDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Agency>> GetAllAsync()
        {
            return await _context.Agencies
                .OrderBy(a => a.AgencyId)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Agency?> GetByIdAsync(int agencyId)
        {
            return await _context.Agencies
                .FirstOrDefaultAsync(a => a.AgencyId == agencyId);
        }
    }
}
