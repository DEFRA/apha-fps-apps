
namespace Apha.Common.Contracts
{
    public class PaginatedApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public Pagination? Pagination { get; set; }
        public List<ApiError>? Errors { get; set; } = new();
        public ApiMeta Meta { get; set; } = new();

    }
    public class Pagination
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }
}
