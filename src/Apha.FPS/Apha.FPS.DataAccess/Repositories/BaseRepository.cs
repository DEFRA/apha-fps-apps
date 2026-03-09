using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using System.Collections.ObjectModel;

namespace Apha.FPS.DataAccess.Repositories
{
    public class BaseRepository
    {
        protected readonly FpsDbContext _context;
        public BaseRepository(FpsDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public PagedData<T> ApplyPaging<T>(
                    IEnumerable<T> source,
                    int page,
                    int pageSize)
        {
            var list = source.ToList();
            var totalRecords = list.Count;

            var result = list
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

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
