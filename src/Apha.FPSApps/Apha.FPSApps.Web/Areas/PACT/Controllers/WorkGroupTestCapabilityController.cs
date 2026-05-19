using Apha.Common.Utilities.ExcelExport;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
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
    /// <summary>
    /// Controller for WorkGroup-focused Test Capability management.
    /// This is a new controller separate from TestCapabilityController.
    /// </summary>
    [Area("PACT")]
    [Authorize(Roles = "FPSAdmin,FPSUser,PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]
    public class WorkGroupTestCapabilityController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ITestCapabilityService _service;
        private readonly IExcelExportService _excelExportService;

        public WorkGroupTestCapabilityController(
            IMapper mapper,
            ITestCapabilityService service,
            IExcelExportService excelExportService)
        {
            _mapper = mapper;
            _service = service;
            _excelExportService = excelExportService;
        }

        // ── INDEX (Main View) ─────────────────────────────────────────────────

        /// <summary>
        /// Displays the WorkGroup-focused Test Capability view.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var workGroupsResponse = await _service.GetAllWorkGroupsAsync();

            var viewModel = new WorkGroupTestCapabilityViewModel
            {
                TestCapabilityGrid = BuildEmptyTestCapabilityGrid(),
                WorkGroupOptions = workGroupsResponse.Success && workGroupsResponse.Data != null
                    ? workGroupsResponse.Data
                        .Select(w => new SelectListItem(w.WorkGroupName, w.WorkGroupName))
                        .ToList()
                    : new List<SelectListItem>()
            };

            return View(viewModel);
        }

        // ── GRID OPERATIONS ───────────────────────────────────────────────────

        /// <summary>
        /// Loads the Test Capability grid filtered by WorkGroup.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadTestCapabilityGrid(
            PaginationFilter<string> request, string? workGroup)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var gridConfig = await BuildTestCapabilityGridAsync(request, workGroup);
            return PartialView("_DataGrid", gridConfig);
        }

        /// <summary>
        /// Builds the Test Capability grid configuration with data.
        /// </summary>
        private async Task<DataGridConfig<WorkGroupTestCapabilityItem>> BuildTestCapabilityGridAsync(
            PaginationFilter<string> request, string? workGroup)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var query = _mapper.Map<QueryParameters<string>>(request);

            // Use the existing API endpoint that filters by WorkGroup
            var response = await _service.GetPagedByWorkGroupAsync(query, workGroup);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<WorkGroupTestCapabilityItem>>(response.Data)
                : new List<WorkGroupTestCapabilityItem>();

            var paginationModel = response.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<WorkGroupTestCapabilityItem>
            {
                GridId = "testCapabilitiesWGGrid",
                Title = "",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowRowSelection = true,
                KeyProperty = "TestCode",
                AllowExport = false,
                AllowEdit = false,
                AllowDelete = false,
                RowSelectFunction = "onTestCapabilityRowSelect",
                ExtraFilterMethod = "getTestCapabilityExtraFilters",
                BindGridUrl = "/PACT/WorkGroupTestCapability/LoadTestCapabilityGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<WorkGroupTestCapabilityItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        /// <summary>
        /// Builds an empty Test Capability grid configuration.
        /// </summary>
        private static DataGridConfig<WorkGroupTestCapabilityItem> BuildEmptyTestCapabilityGrid()
        {
            return new DataGridConfig<WorkGroupTestCapabilityItem>
            {
                GridId = "testCapabilitiesWGGrid",
                Title = "",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowRowSelection = true,
                KeyProperty = "TestCode",
                AllowExport = false,
                AllowEdit = false,
                AllowDelete = false,
                RowSelectFunction = "onTestCapabilityRowSelect",
                ExtraFilterMethod = "getTestCapabilityExtraFilters",
                BindGridUrl = "/PACT/WorkGroupTestCapability/LoadTestCapabilityGrid",
                Data = new List<WorkGroupTestCapabilityItem>(),
                Columns = GridDataProvider.GetColumnsDefination<WorkGroupTestCapabilityItem>(null),
                Pagination = new PaginationModel()
            };
        }
    }
}
