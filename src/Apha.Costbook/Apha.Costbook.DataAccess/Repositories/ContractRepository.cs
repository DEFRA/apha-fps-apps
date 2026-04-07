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
    public class ContractRepository : IContractRepository
    {
        private readonly CostbookDbContext _context;
        public ContractRepository(CostbookDbContext context) => _context = context;

        public async Task<List<string>> GetAllContractNumbersAsync()
        {
            return await _context.Projects
                .Where(p => p.ContractNumber != null)
                .Select(p => p.ContractNumber!)
                .Distinct()
                .ToListAsync();
        }
    }
}
