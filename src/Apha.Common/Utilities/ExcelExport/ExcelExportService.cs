using ClosedXML.Excel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace Apha.Common.Utilities.ExcelExport
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ExcelHiddenColumnAttribute : Attribute
    {
    }

    public partial class ExcelExportService : IExcelExportService
    {
        public byte[] ExportToExcel<T>(
        IEnumerable<T> data,
        string sheetName = "Sheet1",
        Dictionary<string, string>? columnFormats = null)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var normalizedFormats = columnFormats != null
                ? new Dictionary<string, string>(columnFormats, StringComparer.OrdinalIgnoreCase)
                : null;
            var formatByColumn = new string?[properties.Length];

            for (int i = 0; i < properties.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = GetColumnHeader(properties[i]);
                formatByColumn[i] = GetColumnFormat(normalizedFormats, properties[i].Name);

                var column = worksheet.Column(i + 1);
                if (!string.IsNullOrWhiteSpace(formatByColumn[i]))
                {
                    column.Style.NumberFormat.Format = formatByColumn[i]!;
                }

                if (IsHiddenColumn(properties[i]))
                {
                    column.Hide();
                }
            }

            int row = 2;

            foreach (var item in data)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    var rawValue = ConvertExcelValue(properties[col].GetValue(item));
                    var valueForExport = ConvertValueForColumnFormat(rawValue, formatByColumn[col]);
                    worksheet.Cell(row, col + 1).Value = XLCellValue.FromObject(valueForExport);
                }

                row++;
            }

            int lastDataRow = Math.Max(1, row - 1);
            int lastDataColumn = Math.Max(1, properties.Length);

            var headerRange = worksheet.Range(1, 1, 1, lastDataColumn);
            headerRange.Style.Font.Bold = true;

            var allCellsRange = worksheet.Range(1, 1, lastDataRow, lastDataColumn);
            allCellsRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            allCellsRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Auto-fit all used columns
            worksheet.Columns().AdjustToContents();

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
                    if (IsHiddenColumn(properties[i]))
                    {
                        worksheet.Column(i + 1).Hide();
                    }
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

        private static bool IsHiddenColumn(PropertyInfo property)
        {
            return property.GetCustomAttribute<ExcelHiddenColumnAttribute>() != null;
        }

        private static string? GetColumnFormat(Dictionary<string, string>? columnFormats, string propertyName)
        {
            if (columnFormats == null)
            {
                return null;
            }

            return columnFormats.TryGetValue(propertyName, out var format)
                ? format
                : null;
        }

        private static object? ConvertValueForColumnFormat(object? value, string? format)
        {
            if (string.IsNullOrWhiteSpace(format) || value is not string text || string.IsNullOrWhiteSpace(text))
            {
                return value;
            }

            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out var currentCultureValue))
            {
                return currentCultureValue;
            }

            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var invariantCultureValue))
            {
                return invariantCultureValue;
            }

            return value;
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
