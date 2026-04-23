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
    public class SettingsRepository : ISettingsRepository
    {
        private readonly CostbookDbContext _context;

        public SettingsRepository(CostbookDbContext context)
        {
            _context = context;
        }

        public async Task<string?> GetSettingValueByIdAsync(string id)
        {
           
            var allSettings = await _context.DatabaseSettings.ToListAsync();           

            var result = await _context.DatabaseSettings
                .Where(s => s.Id == id)
                .Select(s => s.Setting)
                .FirstOrDefaultAsync();           

            return result;
        }
    }
}
