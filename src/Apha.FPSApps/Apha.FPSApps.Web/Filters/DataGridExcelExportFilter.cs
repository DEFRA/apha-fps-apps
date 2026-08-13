using System.Collections;
using System.Reflection;
using Apha.Common.Utilities.GenericExcelExport;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Apha.FPSApps.Web.Filters
{
    /// <summary>
    /// Centralises DataGrid "Excel Export" for every grid controller. When a request targets a
    /// grid's <c>BindGridUrl</c> with <c>export=true</c> (or <c>format=excel</c>), this filter:
    ///   1. Expands any bound paging argument so the action returns the full result set.
    ///   2. Inspects the resulting <see cref="DataGridConfig{T}"/> model and, when
    ///      <see cref="DataGridConfig{T}.AllowExcelExport"/> is true, converts its <c>Data</c>
    ///      collection into an .xlsx download using the shared <see cref="IGenericExcelExporter"/>.
    ///
    /// Controllers therefore only need to set <c>AllowExcelExport = true</c> in their grid config —
    /// no export-specific action code is required.
    /// </summary>
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
            var query = context.HttpContext?.Request?.Query;
            if (query == null)
            {
                return false;
            }

            return string.Equals(query["export"], "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(query["format"], "excel", StringComparison.OrdinalIgnoreCase);
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
            var rowType = config.RowType;
            var data = config.Data ?? (IEnumerable)Array.CreateInstance(rowType, 0);

            // Invoke IGenericExcelExporter.Export<T>(IEnumerable<T>, string) for the discovered row type.
            var exportMethod = typeof(IGenericExcelExporter)
                .GetMethod(nameof(IGenericExcelExporter.Export))!
                .MakeGenericMethod(rowType);

            var sheetName = string.IsNullOrWhiteSpace(config.SheetName) ? "Sheet1" : config.SheetName;
            var fileContent = (byte[])exportMethod.Invoke(_excelExporter, new object[] { data, sheetName })!;

            var baseName = config.FileName;
            if (baseName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                baseName = baseName[..^5];
            }

            var downloadName = $"{baseName}_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
            return new FileContentResult(fileContent, ExcelContentType) { FileDownloadName = downloadName };
        }

        private interface IDataGridExportConfig
        {
            bool AllowExcelExport { get; }
            Type RowType { get; }
            IEnumerable? Data { get; }
            string FileName { get; }
            string SheetName { get; }
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
        }
    }
}
