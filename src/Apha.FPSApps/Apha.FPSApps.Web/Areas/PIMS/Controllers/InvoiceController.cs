using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.PIMS.Controllers
{
    [Area("PIMS")]
    [Authorize(Roles = "PIMSAdmin,PIMSUser")]
    [AuthorizeForScopes(ScopeKeySection = "PIMSApiSettings:Scope")]
    public class InvoiceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IRadTrackInvoiceService _invoiceService;

        public InvoiceController(IMapper mapper, IRadTrackInvoiceService invoiceService)
        {
            _mapper = mapper;
            _invoiceService = invoiceService;
        }

        // ── Index ────────────────────────────────────────────────────────────────
       
        public async Task<IActionResult> Index(
            string? project = null,
            string? contract = null,
            int? year = null,
            string? program = null)
        {
            year = NormalizeYear(year);
            // Populate dropdowns first so we can resolve defaults from the live data.
            InvoiceViewModel viewModel = new()
            {
                FilterContract = contract,
                FilterProgram  = program
            };
            await PopulateDropdownsAsync(viewModel);
            string? resolvedProject = !string.IsNullOrWhiteSpace(project) &&
                                      viewModel.ProjectList.Any(p => p.Value == project)
                ? project
                : viewModel.ProjectList.FirstOrDefault()?.Value;
            int currentCalendarYear = DateTime.Now.Year;
            int? resolvedYear = year ?? (viewModel.YearList.Count > 0
                ? viewModel.YearList
                    .Select(y => int.Parse(y.Value))
                    .Where(y => y <= currentCalendarYear)
                    .DefaultIfEmpty(currentCalendarYear)
                    .Max()
                : (int?)null);

            viewModel.FilterProject = resolvedProject;
            viewModel.FilterYear    = resolvedYear;
            foreach (var item in viewModel.ProjectList)
                item.Selected = item.Value == resolvedProject;
            foreach (var item in viewModel.YearList)
                item.Selected = item.Value == resolvedYear?.ToString();

            PaginationFilter<string> defaultRequest = new() { Filter = "{}" };
            viewModel.InvoicesGrid = await BuildInvoiceGridAsync(
                defaultRequest, resolvedProject, contract, resolvedYear, program);

            return View(viewModel);
        }

        // ── DataGrid AJAX reload ─────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> LoadInvoiceGrid(
            PaginationFilter<string> request,
            string? project = null,
            string? contract = null,
            int? year = null,
            string? program = null)
        {
            year = NormalizeYear(year);

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            DataGridConfig<InvoiceItem> gridConfig =
                await BuildInvoiceGridAsync(request, project, contract, year, program);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<InvoiceItem>> BuildInvoiceGridAsync(
            PaginationFilter<string> request,
            string? project,
            string? contract,
            int? year,
            string? program)
        {
            Dictionary<string, string> filterDict =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new();

            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
            queryParameters.Search = request.Filter;
            queryParameters.Filter = null;
            ApiResponseDto<List<RadTrackInvoiceDto>> pagedData =
                await _invoiceService.GetAllAsync(queryParameters, project, contract, year, program);

            List<InvoiceItem> items = new();
            if (pagedData.Success && pagedData.Data != null)
            {
                items = _mapper.Map<List<InvoiceItem>>(pagedData.Data);
            }
            else if (pagedData.Errors != null)
            {
                foreach (var error in pagedData.Errors)
                    Console.WriteLine($"Invoice grid error: {error.Message}");
            }

            PaginationModel paginationModel = pagedData.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;
            return new DataGridConfig<InvoiceItem>
            {
                GridId = "invoicesGrid",
                Title = "Invoices",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "InvoiceCounter",
                AllowAdd = true,
                AddFunction = "addInvoice",
                AllowEdit = true,
                EditFunction = "editInvoice",
                AllowDelete = true,
                DeleteFunction = "deleteInvoice",
                ExtraFilterMethod = "getInvoiceExtraFilters",
                BindGridUrl = "/PIMS/Invoice/LoadInvoiceGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<InvoiceItem>(),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        // ── Totals footer AJAX ───────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetInvoiceTotals(
            string? project = null,
            string? contract = null,
            int? year = null,
            string? program = null,
            string? search = null)
        {
            year = NormalizeYear(year);

            ApiResponseDto<RadTrackInvoiceTotalsDto> result =
                await _invoiceService.GetTotalsAsync(project, contract, year, program, search);

            if (!result.Success || result.Data == null)
            {
                return Json(new
                {
                    success = false,
                    errors = result.Errors
                });
            }

            InvoiceTotalsItem totalsItem = _mapper.Map<InvoiceTotalsItem>(result.Data);
            return Json(new
            {
                totalPlanned  = totalsItem.TotalPlannedAmount < 0
                    ? $"-£{Math.Abs(totalsItem.TotalPlannedAmount):N2}"
                    : totalsItem.TotalPlannedAmount  == 0 ? "" : $"£{totalsItem.TotalPlannedAmount:N2}",
                totalDue      = totalsItem.TotalDueAmount < 0
                    ? $"-£{Math.Abs(totalsItem.TotalDueAmount):N2}"
                    : totalsItem.TotalDueAmount      == 0 ? "" : $"£{totalsItem.TotalDueAmount:N2}",
                totalInvoiced = totalsItem.TotalActualAmount < 0
                    ? $"-£{Math.Abs(totalsItem.TotalActualAmount):N2}"
                    : totalsItem.TotalActualAmount   == 0 ? "" : $"£{totalsItem.TotalActualAmount:N2}"
            });
        }

        // ── Add / Edit modal partial ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAddEditInvoicePartial(int? id = null, string? project = null, string? contract = null)
        {
            InvoiceItem model = new();

            if (id.HasValue && id.Value > 0)
            {
                ApiResponseDto<RadTrackInvoiceDto> result = await _invoiceService.GetByIdAsync(id.Value);
                if (result is { Success: true, Data: not null })
                    model = _mapper.Map<InvoiceItem>(result.Data);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(project))
                    model.Project = project;
                if (!string.IsNullOrWhiteSpace(contract))
                    model.Contract = contract;
            }
            ViewBag.ProjectList  = await GetProjectSelectListAsync();
            ViewBag.ContractList = await GetContractSelectListAsync();
            ViewBag.IsAddingNew  = !id.HasValue || id.Value == 0;
            return PartialView("_AddEditInvoice", model);
        }

        // ── Save (Create + Update) ────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveInvoice(InvoiceItem item)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key,
                            message = e.ErrorMessage
                        }))
                });
            }

            RadTrackInvoiceDto dto = _mapper.Map<RadTrackInvoiceDto>(item);

            ApiResponseDto<RadTrackInvoiceDto> result;

            if (item.InvoiceCounter == 0)
            {
                result = await _invoiceService.CreateAsync(dto);
                return result.Success
                    ? Json(new { success = true, data = result.Data, message = "Invoice created successfully." })
                    : Json(new { success = false, errors = result.Errors });
            }
            else
            {
                result = await _invoiceService.UpdateAsync(item.InvoiceCounter, dto);
                return result.Success
                    ? Json(new { success = true, data = result.Data, message = "Invoice updated successfully." })
                    : Json(new { success = false, errors = result.Errors });
            }
        }

        // ── Delete ────────────────────────────────────────────────────────────────
        [HttpDelete]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            ApiResponseDto<object> result = await _invoiceService.DeleteAsync(id);
            return result.Success
                ? Json(new { success = true, message = "Invoice deleted successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // ── Private helpers ───────────────────────────────────────────────────────
        private static int? NormalizeYear(int? year) => year.HasValue && year.Value > 0 ? year : null;
        private async Task PopulateDropdownsAsync(InvoiceViewModel viewModel)
        {
            var projectsTask  = _invoiceService.GetProjectsAsync();
            var yearsTask     = _invoiceService.GetYearsAsync();
            var contractsTask = _invoiceService.GetContractsAsync();
            var programsTask  = _invoiceService.GetProgramsAsync();
            await Task.WhenAll(projectsTask, yearsTask, contractsTask, programsTask);

            viewModel.ProjectList = (projectsTask.Result.Data ?? [])
                .Select(p => new SelectListItem { Value = p, Text = p,
                    Selected = p == viewModel.FilterProject })
                .ToList();

            viewModel.YearList = (yearsTask.Result.Data ?? [])
                .Select(y => new SelectListItem { Value = y.ToString(), Text = y.ToString(),
                    Selected = y == viewModel.FilterYear })
                .ToList();

            viewModel.ContractList = (contractsTask.Result.Data ?? [])
                .Select(c => new SelectListItem { Value = c, Text = c,
                    Selected = c == viewModel.FilterContract })
                .ToList();

            viewModel.ProgramList = (programsTask.Result.Data ?? [])
                .Select(p => new SelectListItem { Value = p, Text = p,
                    Selected = p == viewModel.FilterProgram })
                .ToList();
        }

        private async Task<List<SelectListItem>> GetProjectSelectListAsync()
        {
            var result = await _invoiceService.GetProjectsAsync();
            return (result.Data ?? [])
                .Select(p => new SelectListItem { Value = p, Text = p })
                .ToList();
        }

        private async Task<List<SelectListItem>> GetContractSelectListAsync()
        {
            var result = await _invoiceService.GetContractsAsync();
            return (result.Data ?? [])
                .Select(c => new SelectListItem { Value = c, Text = c })
                .ToList();
        }
    }
}
