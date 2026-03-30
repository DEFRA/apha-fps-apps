namespace Apha.FPSApps.Web.Models.Components.DataGrid
{
    public class DataGridConfig<T> where T : class
    {
        public string GridId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public List<DataGridColumn> Columns { get; set; }
        public List<T> Data { get; set; }
        public bool ShowCheckboxColumn { get; set; }
        public bool ShowPagination { get; set; }
        public string KeyProperty { get; set; } = string.Empty;
        public bool AllowAdd { get; set; }
        public bool AllowEdit { get; set; }
        public bool AllowDelete { get; set; }
        public bool AllowRowSelection { get; set; }
        public string RowSelectFunction { get; set; } = string.Empty;
        public string AddFunction { get; set; } = string.Empty;
        public string EditFunction { get; set; } = string.Empty;
        public string DeleteFunction { get; set; } = string.Empty;
        public string BindGridUrl { get; set; } = string.Empty;
        public string ExtraFilterMethod { get; set; } = string.Empty;
        public PaginationModel Pagination { get; set; } = new PaginationModel();
        public string? CurrentSearch { get; set; }
        public Dictionary<string, string>? CurrentFilters { get; set; } = null;

        public DataGridConfig()
        {
            Columns = new List<DataGridColumn>();
            Data = new List<T>();
            ShowCheckboxColumn = false;
            ShowPagination = true;
            AllowAdd = true;
            AllowEdit = true;
            AllowDelete = true;
            AllowRowSelection = false;
        }
    }
}
