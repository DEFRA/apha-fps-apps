namespace Apha.FPSApps.Web.Models.Components.DataGrid
{
    public class PaginationModel
    {
        public int TotalRecords { get; set; } = 0;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public string? SortColumn { get; set; }
        public bool SortDirection { get; set; } = false;
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);        
    }
}
