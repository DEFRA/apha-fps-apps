using Apha.Common.Utilities.StateManagement;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Constants;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    /// <summary>
    /// MVC controller for the Resource Management Re-plan screen (frmRM_RePlan).
    /// Allows staff hours to be re-planned across workgroups and projects.
    /// </summary>
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ResourceMgmtReplanController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProfitCentreService _profitCentreService;
        private readonly IWorkGroupService _workGroupService;
        private readonly IProjectService _projectService;
        private readonly IPlanStaffZTCodeService _planStaffZTCodeService;
        private readonly IAppStateService _appStateService;

        public ResourceMgmtReplanController(
            IMapper mapper,
            IProfitCentreService profitCentreService,
            IWorkGroupService workGroupService,
            IProjectService projectService,
            IPlanStaffZTCodeService planStaffZTCodeService,
            IAppStateService appStateService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _profitCentreService = profitCentreService ?? throw new ArgumentNullException(nameof(profitCentreService));
            _workGroupService = workGroupService ?? throw new ArgumentNullException(nameof(workGroupService));
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _planStaffZTCodeService = planStaffZTCodeService ?? throw new ArgumentNullException(nameof(planStaffZTCodeService));
            _appStateService = appStateService ?? throw new ArgumentNullException(nameof(appStateService));
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? resourceCentre = null)
        {
            var resourceCentres = await PopulateResourceCentresAsync();

            if (resourceCentre == null)
                resourceCentre = await _appStateService.GetSessionAsync<string>(SessionKeys.SelectedProfitCentre);

            var selected = !string.IsNullOrWhiteSpace(resourceCentre)
                && resourceCentres.Any(r => r.Value == resourceCentre) ? resourceCentre : string.Empty;

            await _appStateService.SetSessionAsync(SessionKeys.SelectedProfitCentre, selected);

            if (selected != string.Empty)
            {
                var selectedItem = resourceCentres.FirstOrDefault(r => r.Value == selected);
                if (selectedItem != null) selectedItem.Selected = true;
            }

            var viewModel = new ResourceMgmtReplanViewModel
            {
                ResourceCentres = resourceCentres,
                SelectedResourceCentre = selected,
                RePlanGrid = BuildRePlanGridConfig([]),
                AllTimeGrid = BuildAllTimeGridConfig([])
            };

            return View(viewModel);
        }

        /// <summary>
        /// Saves the selected resource centre to session (called from JS when the dropdown changes).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveProfitCentreSession([FromBody] string resourceCentre)
        {
            await _appStateService.SetSessionAsync(SessionKeys.SelectedProfitCentre, resourceCentre ?? string.Empty);
            return Ok();
        }

        // ─────────────── DROPDOWN DATA ───────────────

        /// <summary>
        /// Returns distinct workgroups for a given resource centre (used to populate the workgroup list).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetWorkGroups(string resourceCentre)
        {
            if (string.IsNullOrWhiteSpace(resourceCentre))
                return Json(new { success = false, message = "Resource centre is required." });

            var response = await _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync(resourceCentre);
            if (!response.Success)
                return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to load workgroups." });

            var workgroups = (response.Data ?? [])
                .Select(w => w.WorkGroupName)
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .Distinct()
                .OrderBy(w => w)
                .ToList();

            return Json(new { success = true, data = workgroups });
        }

        // ─────────────── GRID LOAD ENDPOINTS ───────────────

        /// <summary>
        /// Loads the re-plan staff grid (Section 2) for the given workgroup.
        /// </summary>
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LoadRePlanGrid(
            PaginationFilter<string> request, [FromForm] string? workGroup)
        {
            if (string.IsNullOrWhiteSpace(workGroup))
                return PartialView("_DataGrid", BuildRePlanGridConfig([]));

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _projectService.GetProjectGroupStaffReplanAsync(query, workGroup);
            if (!response.Success)
                return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to load re-plan grid." });

            var items = (response.Data ?? []).Select(d => _mapper.Map<ResourceMgmtReplanGridItem>(d)).ToList();
            var pagination = BuildPaginationModel(response.Pagination, request);
            var filters = ParseFilters(request.Filter);

            return PartialView("_DataGrid", BuildRePlanGridConfig(items, pagination, filters));
        }

        /// <summary>
        /// Loads the all-time staff jobs grid (Section 3) for the given job code and workgroup grade.
        /// </summary>
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LoadAllTimeGrid(
            PaginationFilter<string> request, [FromForm] string? jobCode, [FromForm] string? wgGrade)
        {
            if (string.IsNullOrWhiteSpace(jobCode) || string.IsNullOrWhiteSpace(wgGrade))
                return PartialView("_DataGrid", BuildAllTimeGridConfig([]));

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _planStaffZTCodeService.GetStaffJobsAllocationByJobCodeWgGradePagedAsync(query, jobCode, wgGrade);
            if (!response.Success)
                return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to load all-time grid." });

            var items = (response.Data ?? []).Select(d => _mapper.Map<ResourceMgmtReplanAllTimeItem>(d)).ToList();
            var pagination = BuildPaginationModel(response.Pagination, request);
            var filters = ParseFilters(request.Filter);

            return PartialView("_DataGrid", BuildAllTimeGridConfig(items, pagination, filters));
        }

        // ─────────────── STAGED ROWS ───────────────

        /// <summary>
        /// Returns the currently staged re-plan rows for the given job code and workgroup grade.
        /// </summary>
        //[HttpGet]
        //public async Task<IActionResult> GetStagedRows(string jobCode, string wgGrade)
        //{
        //    if (string.IsNullOrWhiteSpace(jobCode) || string.IsNullOrWhiteSpace(wgGrade))
        //        return Json(new { success = false, message = "Job code and workgroup grade are required." });

        //    var response = await _resourceMgmtReplanService.GetStagedRowsAsync(jobCode, wgGrade);
        //    if (!response.Success)
        //        return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to load staged rows." });

        //    var data = (response.Data ?? []).Select(d => _mapper.Map<ResourceMgmtReplanStagedItem>(d)).ToList();
        //    return Json(new { success = true, data });
        //}

        // ─────────────── PRIVATE HELPERS ───────────────

        private PaginationModel BuildPaginationModel(
            object? pagination, PaginationFilter<string> request)
        {
            var model = pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagination);
            model.SortColumn = request.SortBy;
            model.SortDirection = request.Descending;
            return model;
        }

        private static Dictionary<string, string>? ParseFilters(string? filter) =>
            !string.IsNullOrEmpty(filter)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(filter)
                : null;

        private static DataGridConfig<ResourceMgmtReplanGridItem> BuildRePlanGridConfig(
            List<ResourceMgmtReplanGridItem> data,
            PaginationModel? pagination = null,
            Dictionary<string, string>? filters = null) =>
            new()
            {
                GridId = "RePlanGrid",
                Title = "",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "StaffRowKey",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowRowSelection = true,
                RowSelectFunction = "rraOnStaffRowSelect",
                BindGridUrl = "/FPS/ResourceMgmtReplan/LoadRePlanGrid",
                ExtraFilterMethod = "rraGetRePlanExtraFilters",
                Data = data,
                Columns = GridDataProvider.GetColumnsDefination<ResourceMgmtReplanGridItem>(),
                Pagination = pagination ?? new PaginationModel(),
                CurrentFilters = filters ?? new Dictionary<string, string>()
            };

        private static DataGridConfig<ResourceMgmtReplanAllTimeItem> BuildAllTimeGridConfig(
            List<ResourceMgmtReplanAllTimeItem> data,
            PaginationModel? pagination = null,
            Dictionary<string, string>? filters = null) =>
            new()
            {
                GridId = "AllTimeGrid",
                Title = "",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "StaffId",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowRowSelection = false,
                BindGridUrl = "/FPS/ResourceMgmtReplan/LoadAllTimeGrid",
                ExtraFilterMethod = "rraGetAllTimeExtraFilters",
                Data = data,
                Columns = GridDataProvider.GetColumnsDefination<ResourceMgmtReplanAllTimeItem>(),
                Pagination = pagination ?? new PaginationModel(),
                CurrentFilters = filters ?? new Dictionary<string, string>()
            };

        private async Task<List<SelectListItem>> PopulateResourceCentresAsync()
        {
            var result = await _profitCentreService.GetProfitCentresAsync();
            return result.Success && result.Data != null
                ? result.Data.Select(p => new SelectListItem { Value = p.ProfitCentreId, Text = p.ProfitCentreId }).ToList()
                : new List<SelectListItem>();
        }
    }
}
