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
    public class StaffRepository : IStaffRepository
    {
        private readonly CostbookDbContext _context;
        public StaffRepository(CostbookDbContext context) => _context = context;

        public async Task<List<Staff>> GetAllStaffAsync()
        {
            return await _context.Staffs.ToListAsync();
        }
    }
}
