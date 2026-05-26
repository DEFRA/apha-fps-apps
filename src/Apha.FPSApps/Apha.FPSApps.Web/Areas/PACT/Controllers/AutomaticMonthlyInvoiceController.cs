using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    /// <summary>
    /// Controller for Automatic Monthly Invoice Creation functionality.
    /// Allows copying invoices from previous months to create new monthly invoices automatically.
    /// </summary>
    [Area("PACT")]
    [Authorize(Roles = "PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "PACTApiSettings:Scope")]
    public class AutomaticMonthlyInvoiceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectInvoiceService _invoiceService;
        private readonly IProjectService _projectService;
        private readonly IMonthService _monthService;

        public AutomaticMonthlyInvoiceController(
            IMapper mapper,
            IProjectInvoiceService invoiceService,
            IProjectService projectService,
            IMonthService monthService)
        {
            _mapper = mapper;
            _invoiceService = invoiceService;
            _projectService = projectService;
            _monthService = monthService;
        }

        /// <summary>
        /// Displays the Automatic Monthly Invoice Creation page with a month selector and data grid.
        /// This page is designed for creating monthly invoices for projects that receive 1/12th of the Contract value each month.
        /// </summary>
        /// <param name="month">Optional month number to pre-filter the invoice grid.</param>
        /// <returns>The AutomaticMonthlyInvoice Index view populated with grid configuration and month options.</returns>
        public async Task<IActionResult> Index(int? month)
        {
            var defaultRequest = new PaginationFilter<string> { Filter = "{}" };

            var gridConfig = await BuildAutomaticInvoiceGridAsync(defaultRequest, month);

            // Populate months dropdown
            var monthsList = await GetMonthsListAsync();

            return View(new AutomaticMonthlyInvoiceViewModel
            {
                SelectedMonth = month,
                InvoicesGrid = gridConfig,
                Months = monthsList
            });
        }

        /// <summary>
        /// Reloads the invoice data grid partial view based on the supplied pagination, sort, filter, and month parameters.
        /// </summary>
        /// <param name="request">Pagination and filter parameters submitted from the data grid.</param>
        /// <param name="month">Optional month number to filter invoices (can be null, empty string, or a valid integer).</param>
        /// <returns>A partial view containing the refreshed data grid, or BadRequest if the model state is invalid.</returns>
        [HttpPost]
        public async Task<IActionResult> LoadInvoicesGrid(PaginationFilter<string> request, string? month)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Parse month string to int?
            int? monthValue = null;
            if (!string.IsNullOrEmpty(month) && int.TryParse(month, out int parsedMonth))
            {
                monthValue = parsedMonth;
            }

            // Merge month filter into request filter
            if (monthValue.HasValue)
            {
                var filterDict = string.IsNullOrEmpty(request.Filter)
                    ? new Dictionary<string, string>()
                    : JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter) ?? new Dictionary<string, string>();

                filterDict["Month"] = monthValue.Value.ToString();
                request.Filter = JsonConvert.SerializeObject(filterDict);
            }

            var gridConfig = await BuildAutomaticInvoiceGridAsync(request, monthValue);
            return PartialView("_DataGrid", gridConfig);
        }

        /// <summary>
        /// Returns the add/edit invoice modal partial view for a new or existing invoice.
        /// </summary>
        /// <param name="id">The invoice counter of the invoice to edit, or 0 to create a new invoice.</param>
        /// <param name="selectedMonth">Optional month number to pre-populate on the new invoice form.</param>
        /// <returns>A partial view pre-populated with the invoice data, or NotFound if the invoice does not exist.</returns>
        [HttpGet]
        public async Task<IActionResult> GetInvoice(int id, int? selectedMonth)
        {
            // Populate dropdowns for the modal
            await PopulateProjectsAndMonthsViewBagAsync();

            if (id == 0)
            {
                return PartialView("_AddEditAutomaticInvoice", new AutomaticInvoiceItem
                {
                    Month = selectedMonth
                });
            }

            var result = await _invoiceService.GetByIdAsync(id);
            if (!result.Success || result.Data == null) return NotFound();

            var item = _mapper.Map<AutomaticInvoiceItem>(result.Data);
            return PartialView("_AddEditAutomaticInvoice", item);
        }

        /// <summary>
        /// Creates or updates an invoice record based on the submitted model.
        /// A new invoice is created when InvoiceCounter is 0; otherwise the existing record is updated.
        /// </summary>
        /// <param name="model">The invoice data submitted from the add/edit modal form.</param>
        /// <returns>A JSON response indicating success or failure with field-level error details on validation failure.</returns>
        [HttpPost]
        public async Task<IActionResult> SaveInvoice([FromBody] AutomaticInvoiceItem model)
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
        /// <returns>A JSON response indicating success or failure.</returns>
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

        /// <summary>
        /// Copies invoices from the source month to the target month.
        /// This allows automatic creation of monthly invoices by copying previous month's data.
        /// Supports both bulk copy (all invoices) and selective copy (specific invoice records).
        /// </summary>
        /// <param name="request">The copy request containing source month, target month, and optional invoice records.</param>
        /// <returns>A JSON response indicating success or failure with the number of invoices copied.</returns>
        [HttpPost]
        public async Task<IActionResult> CopyInvoices([FromBody] CopyInvoicesRequest request)
        {

            // Determine if this is a bulk copy (no records or empty list) or selective copy
            bool isBulkCopy = request.InvoiceRecords == null || request.InvoiceRecords.Count == 0;

            // Map invoice records to DTOs if selective copy
            List<ProjectInvoiceDto>? invoiceDtos = null;
            if (!isBulkCopy)
            {
                invoiceDtos = request.InvoiceRecords!.Select(item =>
                {
                    var dto = _mapper.Map<ProjectInvoiceDto>(item);
                    dto.Month = request.TargetMonth;
                    dto.InvoiceCounter = 0; // Reset counter for new invoices
                    return dto;
                }).ToList();
            }

            // Call unified API method for both bulk and selective copy
            ApiResponseDto<CopyInvoicesResultDto> response = await _invoiceService.CopyInvoicesAsync(
                request.SourceMonth,
                request.TargetMonth,
                invoiceDtos);

            if (response == null)
            {
                // Handle null response gracefully - no invoices copied
                return Json(new
                {
                    success = true,
                    message = "No invoices to copy",
                    copiedCount = 0,
                    errors = new List<string>(),
                    isBulkCopy = isBulkCopy
                });
            }

            if (!response.Success || response.Data == null)
            {
                var errorMessages = response?.Errors?.Select(e => e.Message ?? e.Code ?? "Unknown error").ToList() ?? new List<string>();
                return Json(new
                {
                    success = false,
                    message = errorMessages.Count > 0 ? string.Join(", ", errorMessages) : "Failed to copy invoices",
                    errors = errorMessages
                });
            }

            var result = response.Data;
            return Json(new
            {
                success = result.Success,
                message = result.Message,
                copiedCount = result.CopiedCount,
                errors = result.Errors,
                isBulkCopy = isBulkCopy
            });
        }

        /// <summary>
        /// Builds the invoice data grid configuration by fetching paged invoice data from the service.
        /// </summary>
        private async Task<DataGridConfig<AutomaticInvoiceItem>> BuildAutomaticInvoiceGridAsync(
            PaginationFilter<string> request, int? month = null)
        {
            List<AutomaticInvoiceItem> items = new List<AutomaticInvoiceItem>();
            PaginationModel pagination = new PaginationModel();
            Dictionary<string, string> filterDict = new Dictionary<string, string>();

            // Only fetch data if a month is selected
            if (month.HasValue)
            {
                // Map pagination filter to query parameters
                var query = _mapper.Map<QueryParameters<string>>(request);

                // Call the new API method that handles month filtering
                var response = await _invoiceService.GetPagedProjectInvoicesByMonthAsync(query, month);

                items = response.Data != null
                    ? _mapper.Map<List<AutomaticInvoiceItem>>(response.Data)
                    : new List<AutomaticInvoiceItem>();

                pagination = response.Pagination != null
                    ? _mapper.Map<PaginationModel>(response.Pagination)
                    : new PaginationModel();
                pagination.SortColumn = request.SortBy;
                pagination.SortDirection = request.Descending;

                // Parse existing filters from request if present
                if (!string.IsNullOrEmpty(request.Filter))
                {
                    filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter)
                                     ?? new Dictionary<string, string>();
                }
            }
            else
            {
                // Return empty pagination when no month is selected
                pagination.SortColumn = request.SortBy;
                pagination.SortDirection = request.Descending;
            }

            var queryParams = new List<string>();
            if (month.HasValue)
                queryParams.Add($"month={month.Value}");

            string queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;

            return new DataGridConfig<AutomaticInvoiceItem>
            {
                ShowCheckboxColumn = true,
                GridId = "automaticInvoiceGrid",
                Title = "(for CoreCapcity, Production & Test Activity Projects/Portfolios)",
                KeyProperty = "InvoiceCounter",
                AddFunction = "addAutomaticInvoice",
                EditFunction = "editAutomaticInvoice",
                DeleteFunction = "deleteAutomaticInvoice",
                ExtraFilterMethod = "getAutomaticInvoiceFilters",
                BindGridUrl = $"/PACT/AutomaticMonthlyInvoice/LoadInvoicesGrid{queryString}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<AutomaticInvoiceItem>(),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

        /// <summary>
        /// Retrieves the list of months for dropdown selection.
        /// </summary>
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
        /// Retrieves the list of projects for dropdown selection.
        /// </summary>
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
        /// Populates ViewBag with Projects and Months for use in modal form dropdowns.
        /// </summary>
        private async Task PopulateProjectsAndMonthsViewBagAsync()
        {
            ViewBag.Projects = await GetProjectsListAsync();
            ViewBag.Months = await GetMonthsListAsync();
        }
    }
}
