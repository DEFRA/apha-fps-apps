using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ProjectProfitabilityVlaController : Controller
    {
        // TRANSFORMENGINE [Phase 14 security fix]: upper bound on page size used when fetching
        //   all rows for summary aggregation. Replaces int.MaxValue to prevent unbounded
        //   memory allocation and excessive DB load. Increase if VLA dataset exceeds this limit.
        private const int SummaryMaxPageSize = 5000;

        private readonly IMapper _mapper;

        // TRANSFORMENGINE: main CRUD service — delegates to GET /api/v1/project/profitability-vla
        private readonly IProjectService _projectService;

        // TRANSFORMENGINE: lookup service — used only for Program dropdown (separate from CRUD flow)
        private readonly IProgramService _programService;

        public ProjectProfitabilityVlaController(
            IMapper mapper,
            IProjectService projectService,
            IProgramService programService)
        {
            _mapper = mapper;
            _projectService = projectService;
            _programService = programService;
        }

        /// <summary>
        /// GET /FPS/ProjectProfitabilityVla — renders the VLA profitability page.
        /// Builds explicit DataGridConfig and populates all 4 filter dropdowns.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var viewModel = new ProjectProfitabilityVlaViewModel();
            await PopulateDropdownsAsync(viewModel);

            // TRANSFORMENGINE: DataGridConfig built explicitly — never left as new().
            //   AllowAdd/Edit/Delete = false (JS showAddButton:false; no edit/delete buttons).
            //   KeyProperty = "Id" — hidden row discriminator; Id is not a visible grid column.
            viewModel.ProfitabilityVlaGrid = new DataGridConfig<ProjectProfitabilityVlaItem>
            {
                GridId             = "projectProfitabilityVlaGrid",
                Title              = "Project Profitability for VLA",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "Id",
                AllowAdd           = false,
                AllowEdit          = false,
                AllowDelete        = false,
                // TRANSFORMENGINE: ExtraFilterMethod wires the 4 filter dropdowns into the
                //   DataGrid AJAX reload; implemented in the Razor view (Phase 12).
                ExtraFilterMethod  = "getProjectProfitabilityVlaExtraFilters",
                BindGridUrl        = "/FPS/ProjectProfitabilityVla/LoadProjectProfitabilityVlaGrid",
                Data               = new List<ProjectProfitabilityVlaItem>(),
                Columns            = GridDataProvider.GetColumnsDefination<ProjectProfitabilityVlaItem>(),
                Pagination         = new PaginationModel()
            };

            return View(viewModel);
        }

        /// <summary>
        /// POST /FPS/ProjectProfitabilityVla/LoadProjectProfitabilityVlaGrid
        /// AJAX DataGrid reload endpoint — called by the _DataGrid gridManager.
        /// Four optional filter params are merged in by getProjectProfitabilityVlaExtraFilters().
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadProjectProfitabilityVlaGrid(
            PaginationFilter<string> request,
            string? projectStatus = null,
            string? programNo = null,
            string? manager = null,
            string? customer = null)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var gridConfig = await GetProjectProfitabilityVlaGridConfigAsync(
                request, projectStatus, programNo, manager, customer);

            return PartialView("_DataGrid", gridConfig);
        }

        /// <summary>
        /// GET /FPS/ProjectProfitabilityVla/GetProjectProfitabilityVlaSummary
        /// Returns JSON summary totals for the 9 ppf-total-* readonly inputs in the HTML prototype.
        /// Called by the Razor view after the grid reloads to update the summary bar.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProjectProfitabilityVlaSummary(
            [FromQuery] string? projectStatus = null,
            [FromQuery] string? programNo = null,
            [FromQuery] string? manager = null,
            [FromQuery] string? customer = null)
        {
            // TRANSFORMENGINE: fetch all rows (no pagination) for aggregate calculation;
            //   mirrors projectprofitability_vla.js updateSummary behaviour.
            // TRANSFORMENGINE [Phase 14 security fix]: SummaryMaxPageSize (5000) replaces
            //   int.MaxValue to bound memory allocation; see class-level constant declaration.
            var query = new QueryParameters<string> { Page = 1, PageSize = SummaryMaxPageSize };

            var response = await _projectService.GetProjectProfitabilityVlaAsync(
                query,
                projectStatus: string.IsNullOrWhiteSpace(projectStatus) ? null : projectStatus,
                programNo: string.IsNullOrWhiteSpace(programNo) ? null : programNo,
                manager: string.IsNullOrWhiteSpace(manager) ? null : manager,
                customer: string.IsNullOrWhiteSpace(customer) ? null : customer);

            if (!response.Success)
                return StatusCode(500, response.Errors);

            var items = response.Data ?? new List<ProjectProfitabilityVlaDto>();

            // TRANSFORMENGINE: aggregate 9 financial fields — matches JS updateSummary totals object
            return Ok(new
            {
                totalStaffCosts      = items.Sum(i => i.StaffCosts),
                totalTestCost        = items.Sum(i => i.TestCost),
                totalAnimalCosts     = items.Sum(i => i.AnimalCosts),
                totalAdditionalCosts = items.Sum(i => i.AdditionalCosts),
                totalTotalCosts      = items.Sum(i => i.TotalCosts),
                totalBudget          = items.Sum(i => i.Budget ?? 0m),
                totalProfit          = items.Sum(i => i.Profit),
                totalTargetProfit    = items.Sum(i => i.TargetProfit),
                totalOffTarget       = items.Sum(i => i.OffTarget)
            });
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private async Task<DataGridConfig<ProjectProfitabilityVlaItem>> GetProjectProfitabilityVlaGridConfigAsync(
            PaginationFilter<string> request,
            string? projectStatus,
            string? programNo,
            string? manager,
            string? customer)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);

            // TRANSFORMENGINE: delegates to backend GET /api/v1/project/profitability-vla
            //   via IProjectService; all 4 filter params optional — no placeholder defaults.
            var response = await _projectService.GetProjectProfitabilityVlaAsync(
                queryParameters,
                projectStatus: string.IsNullOrWhiteSpace(projectStatus) ? null : projectStatus,
                programNo: string.IsNullOrWhiteSpace(programNo) ? null : programNo,
                manager: string.IsNullOrWhiteSpace(manager) ? null : manager,
                customer: string.IsNullOrWhiteSpace(customer) ? null : customer);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<ProjectProfitabilityVlaItem>>(response.Data.ToList())
                : new List<ProjectProfitabilityVlaItem>();

            var paginationModel = response.Pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<ProjectProfitabilityVlaItem>
            {
                GridId             = "projectProfitabilityVlaGrid",
                Title              = "Project Profitability for VLA",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "Id",
                AllowAdd           = false,
                AllowEdit          = false,
                AllowDelete        = false,
                ExtraFilterMethod  = "getProjectProfitabilityVlaExtraFilters",
                BindGridUrl        = "/FPS/ProjectProfitabilityVla/LoadProjectProfitabilityVlaGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<ProjectProfitabilityVlaItem>(null),
                Pagination         = paginationModel,
                CurrentFilters     = filterDict
            };
        }

        private async Task PopulateDropdownsAsync(ProjectProfitabilityVlaViewModel model)
        {
            // TRANSFORMENGINE: static status options — matches HTML prototype filterProjectStatus
            //   options: Approved, Completed, Not Approved.
            model.StatusList = new List<SelectListItem>
            {
                new SelectListItem { Value = "",             Text = "All statuses" },
                new SelectListItem { Value = "Approved",     Text = "Approved" },
                new SelectListItem { Value = "Completed",    Text = "Completed" },
                new SelectListItem { Value = "Not Approved", Text = "Not Approved" }
            };

            // TRANSFORMENGINE: dynamic program dropdown — lookup flow via IProgramService;
            //   separate from CRUD resource per layer boundary rule.
            var programResult = await _programService.GetAllProgramsAsync();
            if (programResult.Success && programResult.Data != null)
            {
                model.ProgramList = programResult.Data
                    .OrderBy(p => p.ProgramNo)
                    .Select(p => new SelectListItem
                    {
                        Value    = p.ProgramNo,
                        Text     = string.IsNullOrWhiteSpace(p.ProgramName)
                                      ? p.ProgramNo
                                      : $"{p.ProgramNo} — {p.ProgramName}",
                        Selected = string.Equals(model.SelectedProgram, p.ProgramNo,
                                      StringComparison.OrdinalIgnoreCase)
                    })
                    .ToList();
            }

            // TRANSFORMENGINE: dynamic manager dropdown — reuses IProjectService.GetManagersAsync()
            //   (existing /api/v1/employee lookup); ManagerDto.Name used as both Value and Text.
            //   TRANSFORMENGINE TODO: verify Name matches backend 'manager' filter semantics.
            var managerResult = await _projectService.GetManagersAsync();
            if (managerResult.Success && managerResult.Data != null)
            {
                model.ManagerList = managerResult.Data
                    .Where(m => !string.IsNullOrWhiteSpace(m.Name))
                    .OrderBy(m => m.Name)
                    .Select(m => new SelectListItem
                    {
                        Value    = m.Name,
                        Text     = m.Name,
                        Selected = string.Equals(model.SelectedManager, m.Name,
                                      StringComparison.OrdinalIgnoreCase)
                    })
                    .ToList();
            }

            // TRANSFORMENGINE: dynamic customer dropdown — reuses IProjectService.GetAllCustomersAsync()
            //   (existing /api/v1/customer lookup); CustomerDto.Customer used as both Value and Text.
            //   TRANSFORMENGINE TODO: verify Customer field matches backend 'customer' filter semantics.
            var customerResult = await _projectService.GetAllCustomersAsync();
            if (customerResult.Success && customerResult.Data != null)
            {
                model.CustomerList = customerResult.Data
                    .Where(c => !string.IsNullOrWhiteSpace(c.Customer))
                    .OrderBy(c => c.Customer)
                    .Select(c => new SelectListItem
                    {
                        Value    = c.Customer,
                        Text     = c.Customer,
                        Selected = string.Equals(model.SelectedCustomer, c.Customer,
                                      StringComparison.OrdinalIgnoreCase)
                    })
                    .ToList();
            }
        }
    }
}
