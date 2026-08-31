namespace Apha.Common.Utilities.ExcelExport
{
    public interface IExcelExportService
    {
        byte[] ExportToExcel<T>(
            IEnumerable<T> data,
            string sheetName = "Sheet1",
            Dictionary<string, string>? columnFormats = null);

        byte[] BuildTimeSheetExcel(
            string WorkGroupName,
            short monthNumber,
            IEnumerable<WorkGroupTimeSheetRow> rows,
            short layout);

        byte[] BuildOutputSheetExcel(
            string WorkGroupName,
            short monthNumber,
            IEnumerable<WorkGroupOutputSheetRow> rows);        

        byte[] ExportToExcelMultiSheet(IEnumerable<ExcelSheetDefinition> sheets);

        byte[] BuildBudgetBidsCrosstabExcel(
            IEnumerable<string> accounts,
            IEnumerable<string> workgroups,
            Dictionary<string, Dictionary<string, decimal>> bidLookup);
    }
}
