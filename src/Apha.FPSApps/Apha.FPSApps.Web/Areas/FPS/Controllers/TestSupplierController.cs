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
    public class TestSupplierController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ITestSupplierService _testSupplierService;
        private readonly ITestorProductService _testorProductService;

        public TestSupplierController(
            IMapper mapper,
            ITestSupplierService testSupplierService,
            ITestorProductService testorProductService)
        {
            _mapper = mapper;
            _testSupplierService = testSupplierService;
            _testorProductService = testorProductService;
        }

        // ── GRID ──────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var testCodeOptions = await PopulateTestCodeDropdownAsync();

            var defaultRequest = new PaginationFilter<string> { Filter = "{}" };
            var gridConfig = BuildTestSupplierGridConfig(defaultRequest, null, false, new List<TestSupplierItem>(), new PaginationModel(), new Dictionary<string, string>());

            var viewModel = new TestSupplierViewModel
            {
                TestCodeOptions = testCodeOptions,
                TestSupplierGrid = gridConfig
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LoadTestSupplierGrid(
            PaginationFilter<string> request,
            string? testCode = null,
            bool showRejected = false)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var gridConfig = await GetTestSupplierGridConfigAsync(request, testCode ?? string.Empty, showRejected);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<TestSupplierItem>> GetTestSupplierGridConfigAsync(
            PaginationFilter<string> request,
            string testCode,
            bool showRejected)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _testSupplierService.GetPagedAsync(query, testCode, showRejected);

            List<TestSupplierItem> items = response.Success && response.Data != null
                ? _mapper.Map<List<TestSupplierItem>>(response.Data)
                : new List<TestSupplierItem>();

            var pagination = response.Pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return BuildTestSupplierGridConfig(request, testCode, showRejected, items, pagination, filterDict);
        }

        private static DataGridConfig<TestSupplierItem> BuildTestSupplierGridConfig(
            PaginationFilter<string> request,
            string? testCode,
            bool showRejected,
            List<TestSupplierItem> items,
            PaginationModel pagination,
            Dictionary<string, string> filterDict)
        {
            return new DataGridConfig<TestSupplierItem>
            {
                GridId = "testSupplierGrid",
                Title = "Test Suppliers",
                KeyProperty = "Buyer",
                AddFunction = "addTestSupplier",
                EditFunction = "editTestSupplier",
                DeleteFunction = "deleteTestSupplier",
                ExtraFilterMethod = "getTestSupplierExtraFilters",
                BindGridUrl = "/FPS/TestSupplier/LoadTestSupplierGrid",
                AllowAdd = false,
                AllowEdit = true,
                AllowDelete = true,
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<TestSupplierItem>(null),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

        // ── CRUD ──────────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_AddEditTestSupplier", new TestSupplierItem
            {
                TestCode = string.Empty,
                Buyer = string.Empty,
                ProjectStatusOptions = GetProjectStatusOptions()
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FpsTestRequirementDto dto)
        {
            if (!ModelState.IsValid)
                return BuildModelStateErrorResponse();

            var result = await _testSupplierService.CreateAsync(dto);

            if (result.Success)
                return Json(new { success = true, data = result.Data, message = "Test supplier created successfully." });

            return BuildServiceErrorResponse(result.Errors, "Failed to create test supplier.");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string testCode, string buyer)
        {
            if (string.IsNullOrWhiteSpace(testCode) || string.IsNullOrWhiteSpace(buyer))
                return Json(new { success = false, message = "TestCode and Buyer are required." });

            var result = await _testSupplierService.GetViewByIdAsync(testCode, buyer);

            if (!result.Success)
                return Json(new { success = false, message = $"Record for TestCode '{testCode}' and Buyer '{buyer}' not found." });

            var item = _mapper.Map<TestSupplierItem>(result.Data);
            item.ProjectStatusOptions = GetProjectStatusOptions();

            var detailResult = await _testSupplierService.GetByIdAsync(testCode, buyer);
            if (detailResult.Success && detailResult.Data != null)
            {
                item.ProjectBuyerCode = detailResult.Data.ProjectBuyerCode;
                item.TestBuyerCode = detailResult.Data.TestBuyerCode;
            }

            return PartialView("_AddEditTestSupplier", item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] FpsTestRequirementDto dto)
        {
            if (!ModelState.IsValid)
                return BuildModelStateErrorResponse();

            var result = await _testSupplierService.UpdateAsync(dto);

            if (result.Success)
                return Json(new { success = true, data = result.Data, message = "Test supplier updated successfully." });

            return BuildServiceErrorResponse(result.Errors, "Failed to update test supplier.");
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string testCode, string buyer)
        {
            if (string.IsNullOrWhiteSpace(testCode) || string.IsNullOrWhiteSpace(buyer))
                return Json(new { success = false, message = "TestCode and Buyer are required." });

            var result = await _testSupplierService.DeleteAsync(testCode, buyer);

            if (result.Success)
                return Json(new { success = true, message = "Test supplier deleted successfully." });

            return BuildServiceErrorResponse(result.Errors, "Failed to delete test supplier.");
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private JsonResult BuildModelStateErrorResponse()
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

        private static JsonResult BuildServiceErrorResponse(List<ApiErrorDto>? errors, string fallbackMessage)
        {
            return new JsonResult(new
            {
                success = false,
                message = errors?.FirstOrDefault()?.Message ?? fallbackMessage,
                errors = (errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        private async Task<List<SelectListItem>> PopulateTestCodeDropdownAsync()
        {
            var result = await _testorProductService.GetAllTestorProductsAsync();
            if (!result.Success || result.Data == null)
                return new List<SelectListItem>();

            return result.Data
                .OrderBy(t => t.ItemCode)
                .Select(t => new SelectListItem
                {
                    Value = t.ItemCode,
                    Text = $"{t.ItemCode} - {t.ItemDescription}"
                })
                .ToList();
        }

        private static List<SelectListItem> GetProjectStatusOptions()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Approved", Text = "Approved" },
                new SelectListItem { Value = "Rejected", Text = "Rejected" }
            };
        }
    }
}
