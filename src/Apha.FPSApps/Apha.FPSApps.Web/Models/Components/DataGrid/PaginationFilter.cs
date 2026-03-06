namespace Apha.FPSApps.Web.Models.Components.DataGrid
{
    public class PaginationFilter<TFilter>
    {        
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool Descending { get; set; }
        public TFilter? Filter { get; set; }
    }
}
