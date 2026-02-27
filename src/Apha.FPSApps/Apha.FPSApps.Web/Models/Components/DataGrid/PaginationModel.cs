namespace Apha.FPSApps.Web.Models.Components.DataGrid
{
    public class PaginationModel
    {
        public int TotalCount { get; set; } = 0;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public string? SortColumn { get; set; }
        public bool SortDirection { get; set; } = false;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public int WindowSize { get; set; } = 10;
        public int StartPage
        {
            get
            {
                var blockIndex = (PageNumber - 1) / WindowSize;
                var start = blockIndex * WindowSize + 1;

                if (TotalPages - start + 1 < WindowSize)
                {
                    start = Math.Max(1, TotalPages - WindowSize + 1);
                }
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
