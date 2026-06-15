using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PIMS.DataAccess.Repository
{
    public class BaseRepository
    {
        protected readonly PimsDbContext _context;
        public BaseRepository(PimsDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        protected async Task<PagedData<T>> ApplyPaging<T>(IQueryable<T> source, int page, int pageSize)
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

            return new PagedData<T>(result, pagination);
        }
    }
}
