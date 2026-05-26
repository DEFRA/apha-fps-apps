using ClosedXML.Excel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Apha.Common.Utilities.ExcelExport
{
    public class ExcelExportService : IExcelExportService
    {
        public byte[] ExportToExcel<T>(
        IEnumerable<T> data,
        string sheetName = "Sheet1")
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < properties.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = properties[i].Name;
            }

            int row = 2;

            foreach (var item in data)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    var rawValue = ConvertExcelValue(properties[col].GetValue(item));
                    worksheet.Cell(row, col + 1).Value = XLCellValue.FromObject(rawValue);                     
                }

                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return stream.ToArray();
        }

        public byte[] ExportToExcelMultiSheet(IEnumerable<ExcelSheetDefinition> sheets)
        {
            using var workbook = new XLWorkbook();

            foreach (var sheet in sheets)
            {
                var worksheet = workbook.Worksheets.Add(sheet.SheetName);
                var allProperties = sheet.DataType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                var properties = sheet.IncludedProperties != null
                    ? allProperties.Where(p => sheet.IncludedProperties.Contains(p.Name)).ToArray()
                    : allProperties;

                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = GetColumnHeader(properties[i]);
                }

                int row = 2;
                foreach (var item in sheet.Data)
                {
                    for (int col = 0; col < properties.Length; col++)
                    {
                        var rawValue = ConvertExcelValue(properties[col].GetValue(item));
                        worksheet.Cell(row, col + 1).Value = XLCellValue.FromObject(rawValue);
                    }
                    row++;
                }
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static string GetColumnHeader(PropertyInfo property)
        {
            var display = property.GetCustomAttribute<DisplayAttribute>();
            return display?.Name ?? property.Name;
        }

        private object? ConvertExcelValue(object? value)
        {
            return value switch
            {
                null => null,
                DateOnly d => d.ToDateTime(TimeOnly.MinValue),
                TimeOnly t => t.ToTimeSpan(),
                _ => value
            };
        }
    }
}
