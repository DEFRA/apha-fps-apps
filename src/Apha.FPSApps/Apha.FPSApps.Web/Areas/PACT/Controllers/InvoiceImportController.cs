using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using Apha.Common.Utilities.ExcelExport;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]
    public class InvoiceImportController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectInvoiceService _invoiceService;
        private readonly IProjectService _projectService;
        private readonly IMonthService _monthService;
        private readonly IExcelExportService _excelExportService;

        public InvoiceImportController(
            IMapper mapper,
            IProjectInvoiceService invoiceService,
            IProjectService projectService,
            IMonthService monthService,
            IExcelExportService excelExportService)
        {
            _mapper = mapper;
            _invoiceService = invoiceService;
            _projectService = projectService;
            _monthService = monthService;
            _excelExportService = excelExportService;
        }

        /// <summary>
        /// Displays the invoice management page with a paginated data grid, project dropdown, and month filter.
        /// </summary>
        /// <param name="parentProject">Optional parent project code to pre-filter the invoice grid.</param>
        /// <param name="month">Optional month number to pre-filter the invoice grid.</param>
        /// <returns>The Invoice Index view populated with grid configuration and filter options.</returns>
        public async Task<IActionResult> Index(string? parentProject, int? month)
        {
            var defaultRequest = new PaginationFilter<string>{};

            // Apply month filter via the Filter property
            if (month.HasValue)
            {
                defaultRequest.Filter = $"{{\"Month\":\"{month.Value}\"}}";
            }

            var gridConfig = await BuildInvoiceManualGridAsync(defaultRequest, parentProject, month);

            // Populate project dropdown for filter panel
            var projectsList = await GetProjectsListAsync();

            // Populate months dropdown
            var monthsList = await GetMonthsListAsync();

            // Also set ViewBag for modal form compatibility
            ViewBag.Projects = projectsList;
            ViewBag.FilterProjects = projectsList;

            var failedRequest = new PaginationFilter<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "Id",
                Descending = false
            };

            return View(new InvoiceViewModel
            {
                ParentProject = parentProject ?? string.Empty,
                Month = month,
                InvoicesGrid = gridConfig,
                FailedInvoicesGrid = await BuildFailedInvoiceImportGridAsync(failedRequest),
                FilterProjects = projectsList,
                FilterMonths = monthsList
            });
        }

        /// <summary>
        /// Reloads the invoice data grid partial view based on the supplied pagination, sort, and filter parameters.
        /// </summary>
        /// <param name="request">Pagination and filter parameters submitted from the data grid.</param>
        /// <param name="parentProject">Optional parent project code to filter invoices.</param>
        /// <param name="month">Optional month number to filter invoices.</param>
        /// <returns>A partial view containing the refreshed data grid, or <see cref="BadRequestResult"/> if the model state is invalid.</returns>
        [HttpPost]
        public async Task<IActionResult> LoadInvoicesGrid(PaginationFilter<string> request, string? parentProject, int? month)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Merge month filter into request filter
            if (month.HasValue)
            {
                var filterDict = string.IsNullOrEmpty(request.Filter)
                    ? new Dictionary<string, string>()
                    : JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter) ?? new Dictionary<string, string>();

                filterDict["Month"] = month.Value.ToString();
                request.Filter = JsonConvert.SerializeObject(filterDict);
            }

            var gridConfig = await BuildInvoiceManualGridAsync(request, parentProject, month);
            return PartialView("_DataGrid", gridConfig);
        }

        /// <summary>
        /// Returns the add/edit invoice modal partial view for a new or existing invoice.
        /// </summary>
        /// <param name="id">The invoice counter of the invoice to edit, or <c>0</c> to create a new invoice.</param>
        /// <param name="parentProject">Optional parent project code pre-populated on the new invoice form.</param>
        /// <returns>
        /// A partial view pre-populated with the invoice data, or <see cref="NotFoundResult"/> if the invoice does not exist.
        /// Returns <see cref="BadRequestResult"/> if the model state is invalid.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetInvoice(int id, string? parentProject)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await PopulateProjectsViewBagAsync();

            if (id == 0)
            {
                return PartialView("_AddEditInvoice", new InvoiceItem
                {
                    ProjectParent = parentProject ?? string.Empty
                });
            }

            var result = await _invoiceService.GetByIdAsync(id);
            if (!result.Success || result.Data == null) return NotFound();

            var item = _mapper.Map<InvoiceItem>(result.Data);
            return PartialView("_AddEditInvoice", item);
        }

        /// <summary>
        /// Creates or updates an invoice record based on the submitted model.
        /// A new invoice is created when <see cref="InvoiceItem.InvoiceCounter"/> is <c>0</c>; otherwise the existing record is updated.
        /// </summary>
        /// <param name="model">The invoice data submitted from the add/edit modal form.</param>
        /// <returns>
        /// A JSON response indicating success or failure. On validation failure, field-level error details are included.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> SaveInvoice([FromBody] InvoiceItem model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any() && kvp.Key != "$")
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key.StartsWith("$.") ? kvp.Key[2..] : kvp.Key,
                            message = e.ErrorMessage
                        }))
                });

            var dto = _mapper.Map<ProjectInvoiceDto>(model);
            ApiResponseDto<ProjectInvoiceDto> result;
            string successMsg;

            if (model.InvoiceCounter == 0)
            {
                result = await _invoiceService.CreateAsync(dto);
                successMsg = "Invoice saved successfully.";
            }
            else
            {
                result = await _invoiceService.UpdateAsync(model.InvoiceCounter, dto);
                successMsg = "Invoice updated successfully.";
            }

            if (result.Success)
                return Json(new { success = true, message = successMsg });

            return Json(new
            {
                success = false,
                message = "Failed to save invoice.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        /// <summary>
        /// Deletes the invoice with the specified identifier.
        /// </summary>
        /// <param name="id">The invoice counter of the invoice to delete.</param>
        /// <returns>
        /// A JSON response indicating success or failure.
        /// Returns <see cref="BadRequestResult"/> if the model state is invalid.
        /// </returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _invoiceService.DeleteAsync(id);
            if (result.Success)
                return Json(new { success = true });

            return Json(new { success = false, message = "Failed to delete invoice." });
        }

        [HttpGet]
        public IActionResult DownloadTemplate()
        {
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "PACT", "Invoice-Template.xlsx");
            if (!System.IO.File.Exists(templatePath))
                return NotFound();

            var bytes = System.IO.File.ReadAllBytes(templatePath);
            var fileName = $"Invoice_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> Import([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Please select an Excel file to import." });
            }

            var result = await _invoiceService.ImportInvoiceAsync(file);
            if (!result.Success || result.Data == null)
            {
                return Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Import failed."
                });
            }

            return Json(new
            {
                success = true,
                passedCount = result.Data.PassedCount,
                failedCount = result.Data.FailedCount,
                message = result.Data.Message
            });
        }

        [HttpPost]
        public async Task<IActionResult> LoadFailedInvoiceImportGrid(PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var grid = await BuildFailedInvoiceImportGridAsync(request);
            return PartialView("_DataGrid", grid);
        }

        [HttpGet]
        public async Task<IActionResult> ExportFailedInvoiceImport()
        {
            var exportQuery = new QueryParameters<string>
            {
                Page = 1,
                PageSize = int.MaxValue,
                SortBy = "Id",
                Descending = false
            };

            var response = await _invoiceService.GetFailedInvoiceImportAsync(exportQuery);
            var items = response.Success && response.Data != null
                ? _mapper.Map<List<InvoiceImportFailedItem>>(response.Data)
                : new List<InvoiceImportFailedItem>();

            var bytes = _excelExportService.ExportToExcel(items, "InvoiceImport", new Dictionary<string, string>
            {
                [nameof(InvoiceImportFailedItem.Amount)] = "#,##0.00;-#,##0.00",
                [nameof(InvoiceImportFailedItem.CostOfWork)] = "#,##0.00;-#,##0.00",
                [nameof(InvoiceImportFailedItem.Wip)] = "#,##0.00;-#,##0.00",
                [nameof(InvoiceImportFailedItem.ProfitLoss)] = "#,##0.00;-#,##0.00"
            });
            var fileName = $"ExportedInvoice_{DateTime.Now:ddMMyyyy}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAllFailedInvoiceImport()
        {
            var result = await _invoiceService.DeleteFailedInvoiceImportByUserAsync();
            return Json(new
            {
                success = result.Success && result.Data,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete failed imported records."
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetFailedInvoiceImport(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _invoiceService.GetFailedInvoiceImportByIdAsync(id);
            if (!result.Success || result.Data == null)
                return NotFound();

            var item = _mapper.Map<InvoiceImportFailedItem>(result.Data);
            return PartialView("_EditFailedInvoiceImport", item);
        }

        [HttpPost]
        public async Task<IActionResult> SaveFailedInvoiceImport([FromBody] InvoiceImportFailedItem model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any() && kvp.Key != "$")
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key.StartsWith("$.") ? kvp.Key[2..] : kvp.Key,
                            message = e.ErrorMessage
                        }))
                });

            var dto = _mapper.Map<InvoiceImportRowDto>(model);
            var result = await _invoiceService.SaveFailedInvoiceImportAsync(model.Id, dto);

            if (result.Success)
            {
                var movedToInvoice = result.Data;
                var message = movedToInvoice
                    ? "Record successfully validated and is now live."
                    : "Failed record updated successfully.";
                return Json(new { success = true, message, movedToInvoice });
            }

            return Json(new
            {
                success = false,
                message = "Validation failed. Please correct the errors below.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteFailedInvoiceImport(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _invoiceService.DeleteFailedInvoiceImportByIdAsync(id);
            if (result.Success)
                return Json(new { success = true });

            return Json(new { success = false, message = "Failed to delete failed record." });
        }

        /// <summary>
        /// Builds the invoice data grid configuration by fetching paged invoice data from the service
        /// and applying any active pagination, sort, and filter state.
        /// </summary>
        /// <param name="request">Pagination and filter parameters for the grid query.</param>
        /// <param name="parentProject">Optional parent project code used to scope the invoice query.</param>
        /// <param name="month">Optional month number injected into the filter when not already present.</param>
        /// <returns>A fully configured <see cref="DataGridConfig{T}"/> ready for rendering.</returns>
        private async Task<DataGridConfig<InvoiceItem>> BuildInvoiceManualGridAsync(
            PaginationFilter<string> request, string? parentProject, int? month = null)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            // Add month filter if specified
            if (month.HasValue && !filterDict.ContainsKey("Month"))
            {
                filterDict["Month"] = month.Value.ToString();
                request.Filter = JsonConvert.SerializeObject(filterDict);
            }

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _invoiceService.GetPagedProjectInvoiceManualAsync(query, parentProject);

            var items = response.Data != null
                ? _mapper.Map<List<InvoiceItem>>(response.Data)
                : new List<InvoiceItem>();

            var pagination = response.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(parentProject))
                queryParams.Add($"parentProject={Uri.EscapeDataString(parentProject)}");
            if (month.HasValue)
                queryParams.Add($"month={month.Value}");

            string queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;

            return new DataGridConfig<InvoiceItem>
            {
                GridId = "invoicesGrid",
                Title = "Invoice Record",
                KeyProperty = "InvoiceCounter",
                AddFunction = "addInvoice",
                EditFunction = "editInvoice",
                DeleteFunction = "deleteInvoice",
                BindGridUrl = $"/PACT/Invoice/LoadInvoicesGrid{queryString}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<InvoiceItem>(),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

        private async Task<DataGridConfig<InvoiceImportFailedItem>> BuildFailedInvoiceImportGridAsync(PaginationFilter<string> request)
        {
            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _invoiceService.GetFailedInvoiceImportAsync(query);

            var items = response.Data != null
                ? _mapper.Map<List<InvoiceImportFailedItem>>(response.Data)
                : new List<InvoiceImportFailedItem>();

            var pagination = response.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            return new DataGridConfig<InvoiceImportFailedItem>
            {
                GridId = "failedInvoicesGrid",
                Title = "Failed records",
                KeyProperty = "Id",
                EditFunction = "editFailedInvoiceImport",
                DeleteFunction = "deleteFailedInvoiceImport",
                BindGridUrl = "/PACT/InvoiceImport/LoadFailedInvoiceImportGrid",
                AllowAdd = false,
                AllowEdit = true,
                AllowDelete = true,
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<InvoiceImportFailedItem>(),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

        /// <summary>
        /// Retrieves an ordered list of all PACT projects formatted as <see cref="SelectListItem"/> entries
        /// for use in project filter dropdowns.
        /// </summary>
        /// <returns>An ordered list of project select items, or an empty list if none are available.</returns>
        private async Task<List<SelectListItem>> GetProjectsListAsync()
        {

            var result = await _projectService.GetAllPactProjectsAsync();

            if (result != null && result.Success && result.Data != null && result.Data.Count > 0)
            {
                var projectList = result.Data
                    .OrderBy(p => p.ParentProject)
                    .Select(p => new SelectListItem
                    {
                        Value = p.ParentProject,
                        Text = p.ParentProject
                    })
                    .ToList();

                return projectList;
            }
            else
            {
                return new List<SelectListItem>();
            }

        }

        /// <summary>
        /// Retrieves an ordered list of all months formatted as <see cref="SelectListItem"/> entries
        /// for use in month filter dropdowns.
        /// </summary>
        /// <returns>An ordered list of month select items in the format "number - name", or an empty list if none are available.</returns>
        private async Task<List<SelectListItem>> GetMonthsListAsync()
        {
            var result = await _monthService.GetAllMonthsAsync();

            if (result != null && result.Success && result.Data != null && result.Data.Count > 0)
            {
                var monthList = result.Data
                    .OrderBy(m => m.Monthnumber)
                    .Select(m => new SelectListItem
                    {
                        Value = m.Monthnumber.ToString(),
                        Text = $"{m.Monthnumber} - {m.Monthname}"
                    })
                    .ToList();

                return monthList;
            }
            else
            {
                return new List<SelectListItem>();
            }
        }

        /// <summary>
        /// Populates <c>ViewBag.Projects</c> with the list of PACT projects for use in modal form dropdowns.
        /// </summary>
        private async Task PopulateProjectsViewBagAsync()
        {
            ViewBag.Projects = await GetProjectsListAsync();
            ViewBag.Months = await GetMonthsListAsync();
        }
    }
}
