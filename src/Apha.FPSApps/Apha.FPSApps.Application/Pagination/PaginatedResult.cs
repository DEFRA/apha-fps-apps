namespace Apha.FPSApps.Application.Pagination
{
    public class PaginatedResult<T>
    {
        public IEnumerable<T> data { get; set; } = Enumerable.Empty<T>();
        public int TotalCount { get; set; }

        public PaginatedResult() { }

        public PaginatedResult(IEnumerable<T> items, int totalCount)
        {
            data = items;
            TotalCount = totalCount;
        }
    }
}
