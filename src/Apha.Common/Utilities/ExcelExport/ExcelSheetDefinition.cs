namespace Apha.Common.Utilities.ExcelExport
{
    public class ExcelSheetDefinition
    {
        public string SheetName { get; set; } = "Sheet";
        public IEnumerable<object> Data { get; set; } = Enumerable.Empty<object>();
        public Type DataType { get; set; } = typeof(object);

        /// <summary>
        /// When set, only properties whose names are in this list will be included as columns.
        /// When null, all public instance properties are included.
        /// </summary>
        public IEnumerable<string>? IncludedProperties { get; set; }
    }
}
