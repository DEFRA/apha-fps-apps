using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Repository
{
    public class CalenderMonthRepository : BaseRepository, ICalenderMonthRepository
    {
        public CalenderMonthRepository(FpsDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CalenderMonth>> GetCalenderMonthsAsync()
        {
            return await _context.CalenderMonths
                .AsNoTracking()
                .OrderBy(m => m.AccntsPeriod)
                .ToListAsync();
        }
    }
}