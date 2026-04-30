using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
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
    public class ProjectTestPlanActualController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectTestPlanActualService _projTestPlanActualService;
        private readonly IProjectService _projectService;
        private readonly ITestRequirementService _testRequirementService;

        public ProjectTestPlanActualController(
            IMapper mapper,
            IProjectTestPlanActualService projTestPlanActualService,
            IProjectService projectService,
            ITestRequirementService testRequirementService)
        {
            _mapper = mapper;
            _projTestPlanActualService = projTestPlanActualService;
            _projectService = projectService;
            _testRequirementService = testRequirementService;
        }

        public async Task<IActionResult> Index(string? projectCode = null)
        {
            var projectList = await GetProjectListAsync();
            var selectedProjectCode = !string.IsNullOrWhiteSpace(projectCode)
                && projectList.Any(p => p.Value == projectCode)
                ? projectCode
                : projectList.FirstOrDefault()?.Value ?? string.Empty;

            var projectInfo = await GetProjectInfoAsync(selectedProjectCode);

            var testPlanGrid = new DataGridConfig<TestPlanActualItem>
            {
                GridId = "testPlanGrid",
                Title = "Planned Time (FPS)",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = true,
                KeyProperty = "RowKey",
                DeleteFunction = "deleteTestPlanJob",
                ExtraFilterMethod = "getTestPlanExtraFilters",
                BindGridUrl = "/FPS/ProjectTestPlanActual/LoadTestPlanGrid",
                Data = new List<TestPlanActualItem>(),
                Columns = GridDataProvider.GetColumnsDefination<TestPlanActualItem>(),
                Pagination = new PaginationModel()
            };

            var compareTests2Grid = new DataGridConfig<CompareTests2Item>
            {
                GridId = "compareTests2Grid",
                Title = "Actual Tests (PACT)",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = true,
                KeyProperty = "RowKey",
                DeleteFunction = "deleteCompareTests2",
                ExtraFilterMethod = "getCompareTests2ExtraFilters",
                BindGridUrl = "/FPS/ProjectTestPlanActual/LoadCompareTests2Grid",
                Data = new List<CompareTests2Item>(),
                Columns = GridDataProvider.GetColumnsDefination<CompareTests2Item>(),
                Pagination = new PaginationModel()
            };

            var totalPlannedCost = selectedProjectCode != string.Empty
                ? (await _projTestPlanActualService.GetTotalPlannedCostAsync(selectedProjectCode)).Data
                : 0m;

            var model = new ProjectTestPlanActualViewModel
            {
                SelectedProjectCode = selectedProjectCode,
                ProjectTitle = projectInfo?.ProjectTitle ?? string.Empty,
                Program = projectInfo?.Program ?? string.Empty,
                Contract = projectInfo?.Contract ?? string.Empty,
                TotalPlannedCost = totalPlannedCost,
                ProjectList = projectList,
                TestPlanGrid = testPlanGrid,
                CompareTests2Grid = compareTests2Grid
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> LoadTestPlanGrid(PaginationFilter<string> request, string? jobCode = null)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request data", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new Dictionary<string, string>();
            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _testRequirementService.GetPagedTestReqmtbyProjectAsync(query, jobCode ?? string.Empty);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<TestPlanActualItem>>(response.Data)
                : new List<TestPlanActualItem>();

            var paginationModel = response.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            var grid = new DataGridConfig<TestPlanActualItem>
            {
                GridId = "testPlanGrid",
                Title = "Planned Time (FPS)",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = true,
                KeyProperty = "TestCode",
                DeleteFunction = "deleteTestPlan",
                ExtraFilterMethod = "getTestPlanExtraFilters",
                BindGridUrl = "/FPS/ProjectTestPlanActual/LoadTestPlanGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<TestPlanActualItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };

            return PartialView("_DataGrid", grid);
        }

        [HttpPost]
        public async Task<IActionResult> LoadCompareTests2Grid(PaginationFilter<string> request, string? projectCode = null)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request data", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new Dictionary<string, string>();
            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var pagedData = await _projTestPlanActualService.GetMonthlyOutputCalcsByProjectAsync(queryParameters, projectCode ?? string.Empty);

            var items = pagedData.Data != null ? _mapper.Map<List<CompareTests2Item>>(pagedData.Data) : new List<CompareTests2Item>();
            var paginationModel = _mapper.Map<PaginationModel>(pagedData.Pagination) ?? new PaginationModel();
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            var gridConfig = new DataGridConfig<CompareTests2Item>
            {
                GridId = "compareTests2Grid",
                Title = "Actual Tests (FPS)",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = true,
                KeyProperty = "RowKey",
                DeleteFunction = "deleteCompareTests2",
                ExtraFilterMethod = "getCompareTests2ExtraFilters",
                BindGridUrl = "/FPS/ProjectTestPlanActual/LoadCompareTests2Grid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<CompareTests2Item>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };

            return PartialView("_DataGrid", gridConfig);
        }

        [HttpGet]
        public async Task<IActionResult> GetProjectInfo(string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                return Json(new { success = false, message = "Project code is required." });

            var result = await _projectService.GetProjectByIdAsync(projectCode);
            if (result.Success && result.Data != null)
                return Json(new { success = true, projectTitle = result.Data.ProjectTitle, program = result.Data.Program, contract = result.Data.Contract });

            return Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Project not found.", errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." }) });
        }

        [HttpGet]
        public async Task<IActionResult> GetTotalPlannedCost(string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                return Json(new { success = false, message = "Project code is required." });

            var result = await _projTestPlanActualService.GetTotalPlannedCostAsync(projectCode);
            if (result.Success)
                return Json(new { success = true, totalPlannedCost = result.Data });

            return Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Could not retrieve planned cost.", totalPlannedCost = 0, errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." }) });
        }

        [HttpGet]
        public async Task<IActionResult> GetTotalActualCost(string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                return Json(new { success = false, message = "Project code is required." });

            var result = await _projTestPlanActualService.GetTotalActualByProjectAsync(projectCode);
            if (result.Success && result.Data != null)
                return Json(new { success = true, totalVolume = result.Data.TotalVolume, totalCost = result.Data.TotalCost });

            return Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Could not retrieve actual totals.", totalVolume = 0, totalCost = 0, errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." }) });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMonthlyOutputCalcs(string rowKey)
        {
            if (string.IsNullOrWhiteSpace(rowKey))
                return Json(new { success = false, message = "Row key is required." });

            var parts = rowKey.Split('|');
            if (parts.Length != 4)
                return Json(new { success = false, message = "Invalid row key format." });

            var testCode  = parts[0];
            var buyer     = parts[1];
            var month     = double.TryParse(parts[2], out var m) ? m : 0;
            var workGroup = parts[3];

            var result = await _projTestPlanActualService.DeleteMonthlyOutputCalcsAsync(buyer, testCode, month, workGroup);
            if (result.Success)
                return Json(new { success = true, message = "Record deleted successfully" });

            return Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Delete failed.", errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." }) });
        }

        private async Task<List<SelectListItem>> GetProjectListAsync()
        {
            var result = await _projectService.GetAllProjectsAsync();
            if (result.Success && result.Data != null)
                return result.Data.Select(p => new SelectListItem { Value = p.ParentProject, Text = p.ParentProject }).ToList();
            return new List<SelectListItem>();
        }

        private async Task<ProjectDto?> GetProjectInfoAsync(string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode)) return null;
            var result = await _projectService.GetProjectByIdAsync(projectCode);
            return result.Success ? result.Data : null;
        }
    }
}