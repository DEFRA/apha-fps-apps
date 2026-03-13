namespace Apha.FPSApps.Web.Models.Components.DataGrid
{
    public class DataGridConfig<T> where T : class
    {
        public string GridId { get; set; }
        public string Title { get; set; }
        public List<DataGridColumn> Columns { get; set; }
        public List<T> Data { get; set; }
        public bool ShowCheckboxColumn { get; set; }
        public bool ShowPagination { get; set; }
        public string KeyProperty { get; set; }
        public bool AllowAdd { get; set; }
        public bool AllowDelete { get; set; }
        public string AddFunction { get; set; }
        public string EditFunction { get; set; }
        public string DeleteFunction { get; set; }
        public string BindGridUrl { get; set; }        
        public PaginationModel Pagination { get; set; }
        public string? CurrentSearch { get; set; }
        public Dictionary<string, string>? CurrentFilters { get; set; } = null;

        public DataGridConfig()
        {
            Columns = new List<DataGridColumn>();
            Data = new List<T>();
            ShowCheckboxColumn = true;
            ShowPagination = true;
            AllowAdd = true;
            AllowDelete = true;
        }
    }
}
