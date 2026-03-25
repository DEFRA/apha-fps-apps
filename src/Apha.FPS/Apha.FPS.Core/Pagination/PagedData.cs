namespace Apha.FPS.Core.Pagination
{
    public class PagedData<T>
    {
        public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
        public PaginationData PaginationData { get; set; } = new PaginationData();

        public PagedData() { }

        public PagedData(IEnumerable<T> items, PaginationData paginationData)
        {
            Data = items;           
            PaginationData = paginationData;
        }
    }

    public class PaginationData
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; } 
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }
}
