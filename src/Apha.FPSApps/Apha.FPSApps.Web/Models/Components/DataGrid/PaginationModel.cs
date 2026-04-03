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
                var blockIndex = (PageNumber - 1) / WindowSize;
                var start = blockIndex * WindowSize + 1;

                // If we’re in the last block and it’s smaller than WindowSize, shift it
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
