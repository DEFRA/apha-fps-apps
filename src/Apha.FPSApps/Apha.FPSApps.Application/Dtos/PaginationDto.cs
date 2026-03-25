namespace Apha.FPSApps.Application.Dtos
{
    public class PaginationDto
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }
}
