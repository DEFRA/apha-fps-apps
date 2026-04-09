using Apha.Common.Utilities.ExcelExport;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
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
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class WorkGroupTestCapabilityController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IWorkGroupTestCapabilityService _service;
        private readonly IProjectService _projectService;
        private readonly IExcelExportService _excelExportService;

        public WorkGroupTestCapabilityController(
            IMapper mapper,
            IWorkGroupTestCapabilityService service,
            IProjectService projectService,
            IExcelExportService excelExportService)
        {
            _mapper = mapper;
            _service = service;
            _projectService = projectService;
            _excelExportService = excelExportService;
        }

        // ── INDEX ─────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var defaultRequest = new PaginationFilter<string> { Filter = "{}" };
            var testCapabilityGrid = await BuildTestCapabilityGridAsync(defaultRequest, viewBy: 1, filterValue: null);
            var testReqmtGrid = BuildEmptyTestReqmtGrid();

            var workGroupsResponse = await _service.GetAllWorkGroupsAsync();
            var testsResponse = await _service.GetAllTestorProductsAsync();

            var viewModel = new WorkGroupTestCapabilityViewModel
            {
                TestCapabilityGrid = testCapabilityGrid,
                TestReqmtGrid = testReqmtGrid,
                WorkGroupOptions = workGroupsResponse.Success && workGroupsResponse.Data != null
                    ? workGroupsResponse.Data
                        .Select(w => new SelectListItem(w.WorkGroupName, w.WorkGroupName))
                        .ToList()
                    : new List<SelectListItem>(),
                TestorProductOptions = testsResponse.Success && testsResponse.Data != null
                    ? testsResponse.Data
                        .Select(t => new SelectListItem(
                            string.IsNullOrWhiteSpace(t.ItemDescription)
                                ? t.ItemCode
                                : $"{t.ItemCode}",
                            t.ItemCode))
                        .ToList()
                    : new List<SelectListItem>()
            };

            return View(viewModel);
        }

        // ── GRID 1: TEST CAPABILITY ───────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadTestCapabilityGrid(
            PaginationFilter<string> request, int viewBy, string? filterValue)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var gridConfig = await BuildTestCapabilityGridAsync(request, viewBy, filterValue);
            return PartialView("_DataGrid", gridConfig);
        }

        // ── GRID 2: TEST REQMT ────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadTestReqmtGrid(
            PaginationFilter<string> request, string testCode)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var gridConfig = await BuildTestReqmtGridAsync(request, testCode);
            return PartialView("_DataGrid", gridConfig);
        }

        // ── TEST CAPABILITY CRUD ──────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> CreateTestCapability()
        {
            ViewBag.WorkGroupOptions = await GetWorkGroupSelectListAsync();
            ViewBag.TestorProductOptions = await GetTestorProductSelectListAsync();
            ViewBag.Projects = await GetProjectsAsync();
            return PartialView("_AddEditTestCapability", new WorkGroupTestCapabilityItem());
        }

        [HttpPost]
        public async Task<IActionResult> CreateTestCapability([FromBody] WorkGroupTestCapabilityItem model)
        {
            if (!ModelState.IsValid)
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

            var dto = _mapper.Map<TestCapabilityDto>(model);
            var result = await _service.CreateTestCapabilityAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "Test Capability created successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create Test Capability.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        [HttpGet]
        public async Task<IActionResult> EditTestCapability(string testCode, string workGroup)
        {
            var result = await _service.GetTestCapabilityByIdAsync(testCode, workGroup);
            if (!result.Success)
                return NotFound($"Test Capability with TestCode '{testCode}' and WorkGroup '{workGroup}' not found.");

            ViewBag.WorkGroupOptions = await GetWorkGroupSelectListAsync();
            ViewBag.TestorProductOptions = await GetTestorProductSelectListAsync();
            ViewBag.Projects = await GetProjectsAsync();
            var item = _mapper.Map<WorkGroupTestCapabilityItem>(result.Data);
            return PartialView("_AddEditTestCapability", item);
        }

        [HttpPost]
        public async Task<IActionResult> EditTestCapability([FromBody] WorkGroupTestCapabilityItem model)
        {
            if (!ModelState.IsValid)
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

            var dto = _mapper.Map<TestCapabilityDto>(model);
            var result = await _service.UpdateTestCapabilityAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "Test Capability updated successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update Test Capability.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTestCapability(string testCode, string workGroup)
        {
            var result = await _service.DeleteTestCapabilityAsync(testCode, workGroup);
            return result.Success
                ? Json(new { success = true, message = "Test Capability deleted successfully" })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete Test Capability.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        // ── TEST REQMT CRUD ───────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> CreateTestReqmt(string testCode)
        {
            ViewBag.Projects = await GetProjectsAsync();
            ViewBag.TestorProductOptions = await GetTestorProductSelectListAsync();

            var model = new TestReqmtItem { TestCode = testCode, Active = 1, NoRequired = 0 };

            if (!string.IsNullOrWhiteSpace(testCode))
            {
                var pricing = await _service.GetTestReqmtPricingAsync(testCode, null);
                if (pricing.Success && pricing.Data is not null)
                {
                    model.RecUnitPrice = pricing.Data.RecUnitPrice;
                    model.UnitPrice = pricing.Data.RecUnitPrice;
                }
            }

            return PartialView("_AddEditTestReqmt", model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTestReqmt([FromBody] TestReqmtItem model)
        {
            if (!ModelState.IsValid)
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

            var dto = _mapper.Map<TestReqmtDto>(model);
            var result = await _service.CreateTestReqmtAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "Test Requirement created successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create Test Requirement.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        [HttpGet]
        public async Task<IActionResult> EditTestReqmt(string testCode, string buyer)
        {
            var result = await _service.GetTestReqmtByIdAsync(testCode, buyer);
            if (!result.Success)
                return NotFound($"Test Requirement with TestCode '{testCode}' and Buyer '{buyer}' not found.");

            ViewBag.Projects = await GetProjectsAsync();
            ViewBag.TestorProductOptions = await GetTestorProductSelectListAsync();
            var item = _mapper.Map<TestReqmtItem>(result.Data);
            return PartialView("_AddEditTestReqmt", item);
        }

        [HttpPost]
        public async Task<IActionResult> EditTestReqmt([FromBody] TestReqmtItem model)
        {
            if (!ModelState.IsValid)
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

            var dto = _mapper.Map<TestReqmtDto>(model);
            var result = await _service.UpdateTestReqmtAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "Test Requirement updated successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update Test Requirement.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTestReqmt(string testCode, string buyer)
        {
            var result = await _service.DeleteTestReqmtAsync(testCode, buyer);
            return result.Success
                ? Json(new { success = true, message = "Test Requirement deleted successfully" })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete Test Requirement.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        [HttpGet]
        public async Task<IActionResult> ExportTestReqmt(string testCode, string? filter = null)
        {
            var response = await _service.GetAllTestReqmtForExportAsync(testCode, filter);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<TestReqmtItem>>(response.Data)
                : new List<TestReqmtItem>();

            var bytes = _excelExportService.ExportToExcel(items, "Test Requirements");
            var fileName = $"TestRequirements_{testCode}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> GetTestReqmtPricing(string testCode, string? projectCode = null)
        {
            if (string.IsNullOrWhiteSpace(testCode))
                return Json(new { success = false });

            var result = await _service.GetTestReqmtPricingAsync(testCode, projectCode);
            if (!result.Success || result.Data is null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                recUnitPrice = result.Data.RecUnitPrice,
                isDefraProject = string.IsNullOrWhiteSpace(projectCode) ? (short?)null : result.Data.IsDefraProject
            });
        }

        // ── PRIVATE HELPERS ───────────────────────────────────────────────────

        private async Task<DataGridConfig<WorkGroupTestCapabilityItem>> BuildTestCapabilityGridAsync(
            PaginationFilter<string> request, int viewBy, string? filterValue)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var query = _mapper.Map<QueryParameters<string>>(request);

            var response = viewBy == 2
                ? await _service.GetPagedByTestCodeAsync(query, filterValue)
                : await _service.GetPagedByWorkGroupAsync(query, filterValue);

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
                GridId = "testCapabilityGrid",
                Title = "Test Capabilities",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowRowSelection = true,
                KeyProperty = "TestCode",
                AddFunction = "addTestCapability",
                EditFunction = "editTestCapability",
                DeleteFunction = "deleteTestCapability",
                RowSelectFunction = "onTestCapabilityRowSelect",
                ExtraFilterMethod = "getTestCapabilityExtraFilters",
                BindGridUrl = "/PACT/WorkGroupTestCapability/LoadTestCapabilityGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<WorkGroupTestCapabilityItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        private async Task<DataGridConfig<TestReqmtItem>> BuildTestReqmtGridAsync(
            PaginationFilter<string> request, string testCode)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _service.GetPagedTestReqmtAsync(query, testCode);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<TestReqmtItem>>(response.Data)
                : new List<TestReqmtItem>();

            var paginationModel = response.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<TestReqmtItem>
            {
                GridId = "testReqmtGrid",
                Title = "Test Requirement Records for Test",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Buyer",
                AllowExport = true,
                ExportUrl = "/PACT/WorkGroupTestCapability/ExportTestReqmt",
                AddFunction = "addTestReqmt",
                EditFunction = "editTestReqmt",
                DeleteFunction = "deleteTestReqmt",
                ExtraFilterMethod = "getTestReqmtExtraFilters",
                BindGridUrl = "/PACT/WorkGroupTestCapability/LoadTestReqmtGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<TestReqmtItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        private static DataGridConfig<TestReqmtItem> BuildEmptyTestReqmtGrid()
        {
            return new DataGridConfig<TestReqmtItem>
            {
                GridId = "testReqmtGrid",
                Title = "Test Requirement Records for Test",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Buyer",
                AllowExport = true,
                ExportUrl = "/PACT/WorkGroupTestCapability/ExportTestReqmt",
                AddFunction = "addTestReqmt",
                EditFunction = "editTestReqmt",
                DeleteFunction = "deleteTestReqmt",
                ExtraFilterMethod = "getTestReqmtExtraFilters",
                BindGridUrl = "/PACT/WorkGroupTestCapability/LoadTestReqmtGrid",
                Data = new List<TestReqmtItem>(),
                Columns = GridDataProvider.GetColumnsDefination<TestReqmtItem>(null),
                Pagination = new PaginationModel()
            };
        }

        private async Task<List<SelectListItem>> GetWorkGroupSelectListAsync()
        {
            var response = await _service.GetAllWorkGroupsAsync();
            return response.Success && response.Data != null
                ? response.Data
                    .Select(w => new SelectListItem(w.WorkGroupName, w.WorkGroupName))
                    .ToList()
                : new List<SelectListItem>();
        }

        private async Task<List<ProjectDto>> GetProjectsAsync()
        {
            var response = await _projectService.GetAllProjectsAsync();
            return response.Success && response.Data != null
                ? response.Data
                : new List<ProjectDto>();
        }

        private async Task<List<SelectListItem>> GetTestorProductSelectListAsync()
        {
            var response = await _service.GetAllTestorProductsAsync();
            return response.Success && response.Data != null
                ? response.Data
                    .Select(t => new SelectListItem(
                        string.IsNullOrWhiteSpace(t.ItemDescription)
                            ? t.ItemCode
                            : $"{t.ItemCode}",
                        t.ItemCode))
                    .ToList()
                : new List<SelectListItem>();
        }
    }
}
