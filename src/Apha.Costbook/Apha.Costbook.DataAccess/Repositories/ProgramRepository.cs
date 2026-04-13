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
    public class ProgramRepository : IProgramRepository
    {
        private readonly CostbookDbContext _context;
        public ProgramRepository(CostbookDbContext context) => _context = context;

        public async Task<List<Program>> GetAllProgramsAsync()
        {
            return await _context.Programs
                .OrderBy(p => p.ProgramNo)
                .ToListAsync();
        }
    }
}
