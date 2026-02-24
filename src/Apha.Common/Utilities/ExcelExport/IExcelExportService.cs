namespace Apha.Common.Utilities.ExcelExport
{
    public interface IExcelExportService
    {
        byte[] ExportToExcel<T>(
        IEnumerable<T> data,
        string sheetName = "Sheet1");
    }
}
