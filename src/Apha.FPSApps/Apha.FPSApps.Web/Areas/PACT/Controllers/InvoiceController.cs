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

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "PACTApiSettings:Scope")]
    public class InvoiceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectInvoiceService _invoiceService;
        private readonly IProjectService _projectService;
        private readonly IMonthService _monthService;

        public InvoiceController(
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
        public async Task<IActionResult> Index(string? parentProject, int? month)
        {
            var defaultRequest = new PaginationFilter<string>
            {
                Page = 1,
                PageSize = 50,
                SortBy = "Month",
                Descending = false
            };

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

            return View(new InvoiceViewModel
            {
                ParentProject = parentProject ?? string.Empty,
                Month = month,
                InvoicesGrid = gridConfig,
                FilterProjects = projectsList,
                FilterMonths = monthsList
            });
        }

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

        [HttpGet]
        public async Task<IActionResult> GetInvoice(int id, string? parentProject)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await PopulateProjectsViewBagAsync();

            if (id == 0)
            {
                return PartialView("_AddEditInvoice", new ProjectInvoiceItem
                {
                    ProjectParent = parentProject ?? string.Empty
                });
            }

            var result = await _invoiceService.GetByIdAsync(id);
            if (!result.Success || result.Data == null) return NotFound();

            var item = _mapper.Map<ProjectInvoiceItem>(result.Data);
            return PartialView("_AddEditInvoice", item);
        }

        [HttpPost]
        public async Task<IActionResult> SaveInvoice([FromBody] ProjectInvoiceItem model)
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

        // ── PRIVATE GRID BUILDERS ─────────────────────────────────────────────

        private async Task<DataGridConfig<ProjectInvoiceItem>> BuildInvoiceManualGridAsync(
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
                ? _mapper.Map<List<ProjectInvoiceItem>>(response.Data)
                : new List<ProjectInvoiceItem>();

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

            return new DataGridConfig<ProjectInvoiceItem>
            {
                GridId = "invoicesGrid",
                Title = "Invoice Record",
                KeyProperty = "InvoiceCounter",
                AddFunction = "addInvoice",
                EditFunction = "editInvoice",
                DeleteFunction = "deleteInvoice",
                BindGridUrl = $"/PACT/Invoice/LoadInvoicesGrid{queryString}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ProjectInvoiceItem>(),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

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

        private async Task PopulateProjectsViewBagAsync()
        {
            ViewBag.Projects = await GetProjectsListAsync();
        }
    }
}
