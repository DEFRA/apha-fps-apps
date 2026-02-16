namespace Apha.FPS.Application.Pagination
{
    public class PaginatedResult<T>
    {
        public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
        public PaginationDto PaginationData { get; set; } = new PaginationDto(); // Initialize to avoid nullability issue

        public PaginatedResult() { }       

        public PaginatedResult(IEnumerable<T> items, PaginationDto paginationData)
        {
            Data = items;
            PaginationData = paginationData;
        }
    }

    public class PaginationDto
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }
}
