using Apha.Common.Utilities.GenericExcelExport;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPSApps.Web.Extensions
{
    /// <summary>
    /// Helpers that let any grid controller add the centralised DataGrid "Excel Export"
    /// feature with only a couple of lines. Works together with
    /// <c>DataGridConfig.AllowExcelExport</c> and the export button in <c>_DataGrid.cshtml</c>.
    /// </summary>
    public static class GridExcelExportExtensions
    {
        public const string ExcelContentType =
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        /// <summary>
        /// True when the current request was issued by the DataGrid Excel Export button
        /// (query contains <c>export=true</c> or <c>format=excel</c>).
        /// </summary>
        public static bool IsExcelExportRequest(this Controller controller)
        {
            var query = controller.HttpContext?.Request?.Query;
            if (query == null)
            {
                return false;
            }

            return string.Equals(query["export"], "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(query["format"], "excel", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Produces an .xlsx <see cref="FileResult"/> from the supplied collection using the
        /// shared <see cref="IGenericExcelExporter"/>. The file name is suffixed with a
        /// timestamp and the .xlsx extension is ensured.
        /// </summary>
        public static FileResult ExcelFile<T>(
            this Controller controller,
            IGenericExcelExporter exporter,
            IEnumerable<T> data,
            string fileName,
            string sheetName = "Sheet1")
        {
            ArgumentNullException.ThrowIfNull(exporter);

            byte[] fileContent = exporter.Export(data, sheetName);

            var baseName = string.IsNullOrWhiteSpace(fileName) ? "Export" : fileName.Trim();
            if (baseName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                baseName = baseName[..^5];
            }

            var downloadName = $"{baseName}_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";

            return controller.File(fileContent, ExcelContentType, downloadName);
        }
    }
}
