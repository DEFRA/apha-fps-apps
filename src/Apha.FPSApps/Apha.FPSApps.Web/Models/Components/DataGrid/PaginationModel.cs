namespace Apha.FPSApps.Web.Models.Components.DataGrid
{
    public class PaginationModel
    {
        public int TotalRecords { get; set; } = 0;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortColumn { get; set; }
        public bool SortDirection { get; set; } = false;
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
        public int WindowSize { get; set; } = 5;
        public int StartPage
        {
            get
            {
                int half = WindowSize / 2;
                int start = PageNumber - half;

                if (start < 1)
                    start = 1;

                if (start + WindowSize - 1 > TotalPages)
                    start = Math.Max(1, TotalPages - WindowSize + 1);

                return start;
            }
        }
        public int EndPage
        {
            get
            {
                var end = StartPage + WindowSize - 1;
                return end > TotalPages ? TotalPages : end;
            }
        }
    }
}
