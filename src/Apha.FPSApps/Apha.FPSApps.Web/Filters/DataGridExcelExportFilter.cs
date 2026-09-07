using Apha.Common.Utilities.GenericExcelExport;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections;
using System.Reflection;

namespace Apha.FPSApps.Web.Filters
{
    public sealed class DataGridExcelExportFilter : IAsyncActionFilter
    {
        private const string ExcelContentType =
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private readonly IGenericExcelExporter _excelExporter;

        public DataGridExcelExportFilter(IGenericExcelExporter excelExporter)
        {
            _excelExporter = excelExporter ?? throw new ArgumentNullException(nameof(excelExporter));
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!IsExcelExportRequest(context))
            {
                await next();
                return;
            }

            // Ask the existing grid action for the full result set instead of a single page.
            ExpandPagingToFullResultSet(context);

            // Model validation errors from the oversized page size must not short-circuit the action.
            context.ModelState.Clear();

            var executedContext = await next();

            var config = ExtractGridConfig(executedContext.Result);
            if (config == null || !config.AllowExcelExport)
            {
                return;
            }

            executedContext.Result = BuildExcelResult(config);
        }

        private static bool IsExcelExportRequest(ActionExecutingContext context)
        {
            var request = context.HttpContext?.Request;
            if (request == null)
            {
                return false;
            }

            if (MatchesExcelFlags(request.Query["export"], request.Query["format"]))
            {
                return true;
            }

            if (request.HasFormContentType
                && MatchesExcelFlags(request.Form["export"], request.Form["format"]))
            {
                return true;
            }

            return false;
        }

        private static bool MatchesExcelFlags(string? export, string? format)
        {
            return string.Equals(export, "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(format, "excel", StringComparison.OrdinalIgnoreCase);
        }

        private static void ExpandPagingToFullResultSet(ActionExecutingContext context)
        {
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument == null)
                {
                    continue;
                }

                var type = argument.GetType();
                var pageProperty = type.GetProperty("Page");
                var pageSizeProperty = type.GetProperty("PageSize");

                if (pageProperty?.CanWrite == true && pageProperty.PropertyType == typeof(int))
                {
                    pageProperty.SetValue(argument, 1);
                }

                if (pageSizeProperty?.CanWrite == true && pageSizeProperty.PropertyType == typeof(int))
                {
                    pageSizeProperty.SetValue(argument, int.MaxValue);
                }
            }
        }

        private static IDataGridExportConfig? ExtractGridConfig(IActionResult? result)
        {
            var model = result switch
            {
                PartialViewResult partial => partial.Model,
                ViewResult view => view.Model,
                ObjectResult obj => obj.Value,
                _ => null
            };

            if (model == null)
            {
                return null;
            }

            var modelType = model.GetType();
            if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(DataGridConfig<>))
            {
                return new DataGridExportConfig(model, modelType);
            }

            return null;
        }

        private FileContentResult BuildExcelResult(IDataGridExportConfig config)
        {
            var data = config.Data ?? (IEnumerable)Array.CreateInstance(config.RowType, 0);

            // Rows may be a runtime-generated subclass of the declared grid row type (e.g. cross-tab
            // pivots whose dynamic Y1..Yn columns are emitted on a derived type). Reflect over the
            // actual runtime row type so those extra properties are discovered and exported; fall
            // back to the declared type when there is no data or the row is the declared type itself.
            var rowType = ResolveRowType(config.RowType, data);

            // The strongly-typed Export<T>(IEnumerable<T>, ...) requires the argument to be an
            // IEnumerable<rowType>. When rowType is a runtime-derived type, config.Data is still
            // typed as IEnumerable<declaredType>, which cannot bind to IEnumerable<rowType> via
            // reflection, so project it to the resolved row type first.
            var typedData = CastData(rowType, data);

            // Invoke IGenericExcelExporter.Export<T>(IEnumerable<T>, string) for the discovered row type.
            var exportMethod = typeof(IGenericExcelExporter)
                .GetMethod(nameof(IGenericExcelExporter.Export))!
                .MakeGenericMethod(rowType);

            var sheetName = string.IsNullOrWhiteSpace(config.SheetName) ? "Sheet1" : config.SheetName;
            var includeProperties = config.VisibleColumnNames;
            var columnHeaders = config.ColumnHeaders;

            var fileContent = (byte[])exportMethod.Invoke(
                _excelExporter,
                new object?[] { typedData, sheetName, includeProperties, columnHeaders })!;

            var baseName = config.FileName;
            if (baseName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                baseName = baseName[..^5];
            }

            var downloadName = $"{baseName}_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
            return new FileContentResult(fileContent, ExcelContentType) { FileDownloadName = downloadName };
        }

        // Projects the untyped row sequence to a strongly-typed IEnumerable<rowType> so it can bind
        // to the generic Export<T> parameter. Uses Enumerable.Cast<T> for the resolved row type.
        private static object CastData(Type rowType, IEnumerable data)
        {
            var castMethod = typeof(Enumerable)
                .GetMethod(nameof(Enumerable.Cast), BindingFlags.Public | BindingFlags.Static)!
                .MakeGenericMethod(rowType);

            return castMethod.Invoke(null, new object?[] { data })!;
        }

        // Returns the most derived common type of the rows so runtime-emitted properties (e.g. the
        // dynamic Y1..Yn pivot columns) are exported. Uses the first non-null element's runtime type
        // when it derives from the declared type; otherwise keeps the declared type unchanged so
        // existing exports are completely unaffected.
        private static Type ResolveRowType(Type declaredRowType, IEnumerable data)
        {
            foreach (var item in data)
            {
                if (item == null)
                {
                    continue;
                }

                var runtimeType = item.GetType();
                return declaredRowType.IsAssignableFrom(runtimeType) ? runtimeType : declaredRowType;
            }

            return declaredRowType;
        }

        private interface IDataGridExportConfig
        {
            bool AllowExcelExport { get; }
            Type RowType { get; }
            IEnumerable? Data { get; }
            string FileName { get; }
            string SheetName { get; }
            IReadOnlyList<string>? VisibleColumnNames { get; }
            IReadOnlyDictionary<string, string>? ColumnHeaders { get; }
        }

        private sealed class DataGridExportConfig : IDataGridExportConfig
        {
            private readonly object _model;
            private readonly Type _modelType;

            public DataGridExportConfig(object model, Type modelType)
            {
                _model = model;
                _modelType = modelType;
                RowType = modelType.GetGenericArguments()[0];
            }

            public Type RowType { get; }

            public bool AllowExcelExport =>
                (bool)(_modelType.GetProperty("AllowExcelExport")?.GetValue(_model) ?? false);

            public IEnumerable? Data =>
                _modelType.GetProperty("Data")?.GetValue(_model) as IEnumerable;

            public string FileName
            {
                get
                {
                    var configured = _modelType.GetProperty("ExcelExportFileName")?.GetValue(_model) as string;
                    if (!string.IsNullOrWhiteSpace(configured))
                    {
                        return configured.Trim();
                    }

                    var title = _modelType.GetProperty("Title")?.GetValue(_model) as string;
                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        return title.Trim();
                    }

                    var gridId = _modelType.GetProperty("GridId")?.GetValue(_model) as string;
                    return string.IsNullOrWhiteSpace(gridId) ? "Export" : gridId.Trim();
                }
            }

            public string SheetName
            {
                get
                {
                    var title = _modelType.GetProperty("Title")?.GetValue(_model) as string;
                    return string.IsNullOrWhiteSpace(title) ? "Sheet1" : title.Trim();
                }
            }

            // Restrict the export to the grid's VISIBLE columns, in their configured order. This
            // automatically excludes [GridColumn(IsVisible = false)] members (e.g. DivisionId) and
            // non-tabular helpers such as List<SelectListItem> ManagerList.
            public IReadOnlyList<string>? VisibleColumnNames
            {
                get
                {
                    if (_modelType.GetProperty("Columns")?.GetValue(_model) is not IEnumerable columns)
                    {
                        return null;
                    }

                    var names = new List<string>();
                    foreach (var column in columns)
                    {
                        if (column == null)
                        {
                            continue;
                        }

                        var columnType = column.GetType();
                        var isVisible = (bool)(columnType.GetProperty("IsVisible")?.GetValue(column) ?? true);
                        if (!isVisible)
                        {
                            continue;
                        }

                        if (columnType.GetProperty("PropertyName")?.GetValue(column) is string propertyName
                            && !string.IsNullOrWhiteSpace(propertyName))
                        {
                            names.Add(propertyName);
                        }
                    }

                    return names.Count > 0 ? names : null;
                }
            }

            // Maps each visible column's PropertyName to its DisplayName so dictionary-backed grids
            // (cross-tabs) can show friendly headers instead of the raw dictionary keys.
            public IReadOnlyDictionary<string, string>? ColumnHeaders
            {
                get
                {
                    if (_modelType.GetProperty("Columns")?.GetValue(_model) is not IEnumerable columns)
                    {
                        return null;
                    }

                    var headers = new Dictionary<string, string>(StringComparer.Ordinal);
                    foreach (var column in columns)
                    {
                        if (column == null)
                        {
                            continue;
                        }

                        var columnType = column.GetType();
                        if (columnType.GetProperty("PropertyName")?.GetValue(column) is string propertyName
                            && !string.IsNullOrWhiteSpace(propertyName)
                            && columnType.GetProperty("DisplayName")?.GetValue(column) is string displayName
                            && !string.IsNullOrWhiteSpace(displayName))
                        {
                            headers[propertyName] = displayName;
                        }
                    }

                    return headers.Count > 0 ? headers : null;
                }
            }
        }
    }
}
