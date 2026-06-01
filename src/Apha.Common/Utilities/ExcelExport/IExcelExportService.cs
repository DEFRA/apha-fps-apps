namespace Apha.Common.Utilities.ExcelExport
{
    public interface IExcelExportService
    {
        byte[] ExportToExcel<T>(
            IEnumerable<T> data,
            string sheetName = "Sheet1");

        byte[] BuildTimeSheetExcel(
            string workGroupName,
            short monthNumber,
            IEnumerable<WorkGroupTimeSheetRow> rows,
            short layout);

        byte[] BuildOutputSheetExcel(
            string workGroupName,
            short monthNumber,
            IEnumerable<WorkGroupOutputSheetRow> rows);        

        byte[] ExportToExcelMultiSheet(IEnumerable<ExcelSheetDefinition> sheets);
    }
}
