namespace Apha.FPSApps.Web.Models.Components.DataGrid
{
    public static class GridHelpers
    {
        private const string GridReadonlyCssClass = "grid-readonly";

        public static object? GetPropertyValue(object obj, string propertyName)
        {
            try
            {
                if (obj == null) return null;
                var type = obj.GetType();
                var prop = type.GetProperty(propertyName);
                return prop?.GetValue(obj);
            }
            catch
            {
                return null;
            }
        }

        public static string GetTypeCssClass(DataGridColumn column)
        {
            return column.ColumnType switch
            {
                GridColumnType.Date => GridReadonlyCssClass,
                GridColumnType.DateTime => GridReadonlyCssClass,
                //GridColumnType.Decimal => GridReadonlyCssClass,
                GridColumnType.Number => GridReadonlyCssClass,
                GridColumnType.Text => GridReadonlyCssClass,
                GridColumnType.Dropdown => GridReadonlyCssClass,
                GridColumnType.Checkbox => "grid-input grid-checkbox",
                _ => ""
            };
        }

        public static string FormatValue(object value, DataGridColumn column)
        {
            if (value == null) return "";

            switch (column.ColumnType)
            {
                case GridColumnType.DecimalNumber:
                    if (value is decimal decValue)
                        return decValue.ToString("F2");                    
                    break;
                case GridColumnType.Date:
                    if (value is DateTime dateValue)
                        return dateValue.ToString(column.DateFormat ?? "yyyy-MM-dd");
                    break;
                case GridColumnType.DateTime:
                    if (value is DateTime dateTimeValue)
                        return dateTimeValue.ToString(column.DateFormat ?? "yyyy-MM-dd HH:mm");
                    break;
                case GridColumnType.UsdValue:
                    if (value is decimal usdValue)
                        return usdValue.ToString("C", new System.Globalization.CultureInfo("en-US"));
                    break;
                case GridColumnType.GbpValue:
                    if (value is decimal gbpValue)
                        return gbpValue.ToString("£#,##0;-£#,##0");
                    break;
                // case GridColumnType.Decimal:
                //     if (value is decimal decValue)
                //         return decValue.ToString(column.DecimalFormat ?? "F2");
                //     break;
                // case GridColumnType.Number:
                //     if (value is double dblValue)
                //         return dblValue.ToString(column.DecimalFormat ?? "F2");
                //     else if (value is float fltValue)
                //         return fltValue.ToString(column.DecimalFormat ?? "F2");
                //     else if (value is int intValue)
                //         return intValue.ToString();
                //     break;
                // case GridColumnType.Checkbox:
                //     return Convert.ToBoolean(value) ? "☑" : "☐";
                default:
                    return value?.ToString() ?? string.Empty;
            }
            return value?.ToString() ?? string.Empty;
        }
    }
}
