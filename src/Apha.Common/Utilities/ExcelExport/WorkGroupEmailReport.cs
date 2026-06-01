using ClosedXML.Excel;

namespace Apha.Common.Utilities.ExcelExport
{
    public partial class ExcelExportService : IExcelExportService
    {
        public byte[] BuildTimeSheetExcel(
            string workGroupName,
            short monthNumber,
            IEnumerable<WorkGroupTimeSheetRow> rows,
            short layout)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("TimeSheet");
            var data = rows.ToList();

            if (layout == 2)
            {
                // Cross-tab: fixed cols + one column per staff name (PIVOT tblStaff.Name)
                var staffNames = data
                    .SelectMany(r => r.StaffName.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    .Distinct()
                    .OrderBy(n => n)
                    .ToList();

                ws.Cell(1, 1).Value = "Time Code";
                ws.Cell(1, 2).Value = "Description";
                ws.Cell(1, 3).Value = "Parent Project";
                ws.Cell(1, 4).Value = "Month";
                for (int c = 0; c < staffNames.Count; c++)
                    ws.Cell(1, 5 + c).Value = staffNames[c];

                int row = 2;
                foreach (var item in data)
                {
                    ws.Cell(row, 1).Value = item.TimeCode;
                    ws.Cell(row, 2).Value = item.Description ?? string.Empty;
                    ws.Cell(row, 3).Value = item.ParentProject;
                    ws.Cell(row, 4).Value = item.Month;
                    row++;
                }
            }
            else
            {
                // Flat-file: WorkGroup | Name | TimeCode | ParentProject | Month | Hours
                ws.Cell(1, 1).Value = "Work Group";
                ws.Cell(1, 2).Value = "Name";
                ws.Cell(1, 3).Value = "Time Code";
                ws.Cell(1, 4).Value = "Parent Project";
                ws.Cell(1, 5).Value = "Month";
                ws.Cell(1, 6).Value = "Hours";

                int row = 2;
                foreach (var item in data)
                {
                    ws.Cell(row, 1).Value = workGroupName;
                    ws.Cell(row, 2).Value = item.StaffName;
                    ws.Cell(row, 3).Value = item.TimeCode;
                    ws.Cell(row, 4).Value = item.ParentProject;
                    ws.Cell(row, 5).Value = item.Month;
                    ws.Cell(row, 6).Value = string.Empty; // blank — recipient fills in
                    row++;
                }
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] BuildOutputSheetExcel(
            string workGroupName,
            short monthNumber,
            IEnumerable<WorkGroupOutputSheetRow> rows)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("OutputSheet");

            ws.Cell(1, 1).Value = "Work Group";
            ws.Cell(1, 2).Value = "Test Code";
            ws.Cell(1, 3).Value = "Item Description";
            ws.Cell(1, 4).Value = "Buyer";
            ws.Cell(1, 5).Value = "Month";
            ws.Cell(1, 6).Value = "Volume";

            int row = 2;
            foreach (var item in rows)
            {
                ws.Cell(row, 1).Value = workGroupName;
                ws.Cell(row, 2).Value = item.TestCode;
                ws.Cell(row, 3).Value = item.ItemDescription ?? string.Empty;
                ws.Cell(row, 4).Value = item.Buyer;
                ws.Cell(row, 5).Value = item.Month;
                ws.Cell(row, 6).Value = string.Empty; // blank — recipient fills in
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
