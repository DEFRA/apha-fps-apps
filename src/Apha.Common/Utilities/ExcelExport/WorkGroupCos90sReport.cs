using ClosedXML.Excel;

namespace Apha.Common.Utilities.ExcelExport
{
    public partial class ExcelExportService : IExcelExportService
    {
        public byte[] BuildWorkGroupCos90sExcel(
            IEnumerable<WorkGroupCos90sExportRow> rows,
            short monthNumber,
            short year,
            string? profitCentre,
            string? pactId)
        {
            using var workbook = new XLWorkbook();
            var data = rows.ToList();

            var groupedByPerson = data.GroupBy(r => r.PactId).ToList();
            if (!groupedByPerson.Any())
                groupedByPerson = new List<IGrouping<string, WorkGroupCos90sExportRow>>
                {
                    new[]
                    {
                        new WorkGroupCos90sExportRow
                        {
                            PactId = pactId ?? string.Empty,
                            ProfitCentre = profitCentre ?? string.Empty,
                            Month = monthNumber,
                            Year = year
                        }
                    }.GroupBy(r => r.PactId).First()
                };

            var sheetIndex = 0;
            foreach (var personGroup in groupedByPerson)
            {
                sheetIndex++;
                var first = personGroup.FirstOrDefault();
                var sheetName = sheetIndex == 1 ? "Cos90" : $"Cos90_{sheetIndex}";

                var worksheet = workbook.Worksheets.Add(sheetName);

                var userName = first?.StaffName ?? string.Empty;
                var spNumber = first?.SpNumber ?? string.Empty;
                var workGroup = first?.WorkGroupName ?? string.Empty;
                var gradeCode = first?.GradeCode ?? string.Empty;
                var monthName = new DateTime(year, monthNumber, 1).ToString("MMMM");
                var daysInMonth = DateTime.DaysInMonth(year, monthNumber);
                var accntsPeriod = (short)(((monthNumber + 8) % 12) + 1);

                worksheet.Column(1).Width = 23.5703125;
                worksheet.Column(2).Width = 16.140625;
                worksheet.Column(3).Width = 50.7109375;
                worksheet.Column(3).Style.Alignment.WrapText = true;

                worksheet.Cell(2, 1).Value = "Name:";
                worksheet.Cell(2, 2).Value = userName;
                worksheet.Cell(3, 1).Value = "SP Number:";
                worksheet.Cell(3, 2).Value = spNumber;

                worksheet.Cell(2, 4).Value = "Workgroup:";
                worksheet.Cell(2, 7).Value = workGroup;
                worksheet.Cell(3, 4).Value = "Grade:";
                worksheet.Cell(3, 7).Value = gradeCode;
                worksheet.Cell(2, 11).Value = "Month:";
                worksheet.Cell(3, 11).Value = "Period:";
                worksheet.Cell(2, 14).Value = monthName;
                worksheet.Cell(3, 14).Value = accntsPeriod;

                worksheet.Cell(6, 1).Value = "Time Code:";
                worksheet.Cell(6, 2).Value = "Project Code:";

                for (var day = 1; day <= daysInMonth; day++)
                {
                    var dayCol = 3 + day;
                    var dayDate = new DateTime(year, monthNumber, day);
                    var dayName = dayDate.DayOfWeek switch
                    {
                        DayOfWeek.Sunday => "Sun",
                        DayOfWeek.Monday => "Mon",
                        DayOfWeek.Tuesday => "Tues",
                        DayOfWeek.Wednesday => "Wed",
                        DayOfWeek.Thursday => "Thur",
                        DayOfWeek.Friday => "Fri",
                        _ => "Sat"
                    };

                    worksheet.Column(dayCol).Width = 3.7109375;
                    worksheet.Cell(6, dayCol).Value = day;
                    worksheet.Cell(5, dayCol).Value = dayName;
                    worksheet.Cell(5, dayCol).Style.Alignment.TextRotation = 90;
                    worksheet.Cell(5, dayCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(5, dayCol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;
                    worksheet.Cell(6, dayCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(6, dayCol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;

                    if (dayDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                    {
                        worksheet.Column(dayCol).Style.Fill.BackgroundColor = XLColor.LightGray;
                    }
                }

                var totalCol = 4 + daysInMonth;
                worksheet.Cell(6, totalCol).Value = "Total";

                var templateRows = personGroup
                    .GroupBy(r => new { r.TimeCode, r.ParentProject, r.Description })
                    .Select(g => g.First())
                    .OrderBy(r => r.TimeCode)
                    .ThenBy(r => r.ParentProject)
                    .ToList();

                var currentRow = 6;
                foreach (var row in templateRows)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = row.TimeCode;
                    worksheet.Cell(currentRow, 2).Value = row.ParentProject;
                    worksheet.Cell(currentRow, 3).Value = row.Description ?? string.Empty;
                    worksheet.Cell(currentRow, totalCol).FormulaA1 = $"SUM(D{currentRow}:{XLHelper.GetColumnLetterFromNumber(totalCol - 1)}{currentRow})";
                }

                var workingDays = Enumerable.Range(1, daysInMonth)
                    .Select(d => new DateTime(year, monthNumber, d).DayOfWeek)
                    .Count(d => d is not DayOfWeek.Saturday and not DayOfWeek.Sunday);
                const int workingHours = 0;

                worksheet.Cell(2, 18).Value = $"There are {workingDays} working days in {monthName}. Working hours in the month: {workingHours}";
                worksheet.Cell(3, 18).Value = "NB. Time is to be recorded in hours to the nearest half hour.";
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
