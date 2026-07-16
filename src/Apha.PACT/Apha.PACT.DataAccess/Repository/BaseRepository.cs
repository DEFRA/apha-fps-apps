using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Repository
{
    public abstract class BaseRepository
    {
        protected readonly FpsDbContext _context;

        protected BaseRepository(FpsDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        protected static async Task<PagedData<T>> ApplyPaging<T>(IQueryable<T> source, int page, int pageSize)
        {
            var totalRecords = await source.CountAsync();
            
            var result = page == -1
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

            return new PagedData<T>(result.AsReadOnly(), pagination);
        }
    }
}
