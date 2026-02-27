namespace Apha.FPSApps.Web.Models.Components.DataGrid
{
    public class DataGridColumn
    {
        public string PropertyName { get; set; }
        public string DisplayName { get; set; }
        public GridColumnType ColumnType { get; set; }
        public bool IsEditable { get; set; }
        public bool IsVisible { get; set; }
        public int Width { get; set; }
        public string CssClass { get; set; }
        public string DateFormat { get; set; }
        public bool IsFilterable { get; set; }
        public GridColumnType? FilterType { get; set; }
        public Dictionary<string, string>? FilterOptions { get; set; }

        public DataGridColumn()
        {
            IsVisible = true;
            IsEditable = true;
            Width = 100;
            ColumnType = GridColumnType.Text;
            DateFormat = "dd/MM/yyyy";
        }
    }
}
