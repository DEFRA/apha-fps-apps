using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.Costbook.DataAccess.Repositories
{
    public class YearMasterRepository:IYearMasterRepository
    {
        private readonly CostbookDbContext _context;

        public YearMasterRepository(CostbookDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetOpenYearAsync()
        {
            var yearMaster = await _context.YearMasters
                .AsNoTracking()
                .Where(y => y.YearStatus.ToLower() == "open" && y.Active)
                .OrderByDescending(y => y.FpsYear)
                .FirstOrDefaultAsync();

            return yearMaster?.FpsYear ?? 0;
        }
    }
}
