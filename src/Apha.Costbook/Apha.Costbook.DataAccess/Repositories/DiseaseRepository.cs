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
    public class DiseaseRepository : IDiseaseRepository
    {
        private readonly CostbookDbContext _context;
        public DiseaseRepository(CostbookDbContext context) => _context = context;

        public async Task<List<Disease>> GetAllDiseasesAsync()
        {
            return await _context.Diseases.ToListAsync();
        }
    }
}
