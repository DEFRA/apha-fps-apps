using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;

namespace Apha.PACT.DataAccess.Repository
{
    public abstract class BaseRepository
    {
        protected readonly FpsDbContext _context;

        protected BaseRepository(FpsDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        protected PagedData<T> ApplyPaging<T>(IEnumerable<T> source, int page, int pageSize)
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

            return new PagedData<T>(result.AsReadOnly(), pagination);
        }
    }
}
