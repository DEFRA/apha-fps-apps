using ClosedXML.Excel;

namespace Apha.Common.Utilities.ExcelExport
{
    public partial class ExcelExportService : IExcelExportService
    {
        public byte[] BuildBudgetBidsCrosstabExcel(
            IEnumerable<string> accounts,
            IEnumerable<string> workgroups,
            Dictionary<string, Dictionary<string, decimal>> bidLookup)
        {
            const string currencyFormat = "\u00A3#,##0.00";
            var accountList   = accounts.ToList();
            var workgroupList = workgroups.ToList();

            using var workbook = new XLWorkbook();
            var ws             = workbook.Worksheets.Add("BudgetBidsCrosstab");

            // Header row: A=AccShortName | B=Row Summary | C=<> | D+=workgroups
            ws.Cell(1, 1).Value = "AccShortName";
            ws.Cell(1, 2).Value = "Row Summary";
            ws.Cell(1, 3).Value = "<>";
            for (int col = 0; col < workgroupList.Count; col++)
                ws.Cell(1, col + 4).Value = workgroupList[col];

            // Data rows
            int row = 2;
            foreach (var account in accountList)
            {
                ws.Cell(row, 1).Value = account;

                decimal rowTotal = 0;
                for (int col = 0; col < workgroupList.Count; col++)
                {
                    if (bidLookup.TryGetValue(account, out var wgBids) &&
                        wgBids.TryGetValue(workgroupList[col], out var amount))
                    {
                        var cell = ws.Cell(row, col + 4);
                        cell.Value = (double)amount;
                        cell.Style.NumberFormat.Format = currencyFormat;
                        rowTotal += amount;
                    }
                }

                ws.Cell(row, 2).Value                     = (double)rowTotal;
                ws.Cell(row, 2).Style.NumberFormat.Format = currencyFormat;
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
