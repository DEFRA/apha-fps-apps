namespace Apha.Common.Utilities.GenericExcelExport
{
    /// <summary>
    /// Generic, reflection-based Excel exporter. Accepts any collection and produces
    /// a .xlsx byte array. Column headers are resolved from, in order of precedence:
    ///   1. [ExcelColumn(Name = "...")]
    ///   2. [Display(Name = "...")]
    ///   3. the property name.
    /// Properties decorated with [ExcelIgnore] are skipped.
    /// </summary>
    public interface IGenericExcelExporter
    {
        /// <summary>
        /// Exports a collection to a single-sheet Excel workbook.
        /// </summary>
        /// <typeparam name="T">Row type. Any class/record with public instance properties.</typeparam>
        /// <param name="data">The rows to export. A null value is treated as empty.</param>
        /// <param name="sheetName">The worksheet name.</param>
        /// <param name="includeProperties">
        /// Optional allow-list of property names to export, in the desired column order. When null or
        /// empty, every eligible property is exported. Property names not found on <typeparamref name="T"/>
        /// are ignored. Use this to restrict output to a known set of columns (e.g. only the grid's
        /// visible columns).
        /// </param>
        /// <param name="columnHeaders">
        /// Optional map of column key/property name to the header text to display. Primarily used for
        /// dictionary-backed row types (e.g. cross-tab grids) so friendly display names can be shown
        /// instead of the raw dictionary keys. Keys not present fall back to the property name/key.
        /// </param>
        /// <returns>The generated .xlsx file as a byte array.</returns>
        byte[] Export<T>(IEnumerable<T> data, string sheetName = "Sheet1", IReadOnlyList<string>? includeProperties = null, IReadOnlyDictionary<string, string>? columnHeaders = null);
    }
}
