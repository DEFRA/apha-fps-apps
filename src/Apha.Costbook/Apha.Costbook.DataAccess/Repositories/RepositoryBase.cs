using Apha.Costbook.Core.Pagination;
using Apha.Costbook.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.Costbook.DataAccess.Repositories
{
    public class RepositoryBase
    {
        protected readonly CostbookDbContext _context;

        public RepositoryBase(CostbookDbContext context)
        {
            _context = context;
        }

        protected async Task<PagedData<T>> ApplyPaging<T>(IQueryable<T> source, int page, int pageSize)
        {
          
            var totalRecords = await source.CountAsync();

            var result =  page == -1
            ? await source.ToListAsync() : await source.Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

            var pagination = new PaginationData
            {
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                TotalRecords = totalRecords
            };

            return new PagedData<T>(result, pagination);
        }
    }
}
