using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Handler;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class TestListVlaController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ITestListVlaService _testListVlaService;
        private readonly ITestRequirementService _testRequirementService;
        private readonly ITestCapabilityService _testCapabilityService;
        private readonly IFpsApiClient _fpsApiClient;
        private readonly IFpsYearContext _fpsYearContext;

        public TestListVlaController(
            IMapper mapper,
            ITestListVlaService testListVlaService,
            ITestRequirementService testRequirementService,
            ITestCapabilityService testCapabilityService,
            IFpsApiClient fpsApiClient,
            IFpsYearContext fpsYearContext)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _testListVlaService = testListVlaService ?? throw new ArgumentNullException(nameof(testListVlaService));
            _testRequirementService = testRequirementService ?? throw new ArgumentNullException(nameof(testRequirementService));
            _testCapabilityService = testCapabilityService ?? throw new ArgumentNullException(nameof(testCapabilityService));
            _fpsApiClient = fpsApiClient ?? throw new ArgumentNullException(nameof(fpsApiClient));
            _fpsYearContext = fpsYearContext ?? throw new ArgumentNullException(nameof(fpsYearContext));
        }

        // ── INDEX ─────────────────────────────────────────────────────────────

        public IActionResult Index()
        {
            var fpsYear = _fpsYearContext.Year;

            var viewModel = new TestListVlaViewModel
            {
                FpsYear = fpsYear,

                // AllowAdd/Edit/Delete = true — CRUD modals exist in HTML prototype (vlaTestListModal, vlaDeleteModal)
                TestListGrid = new DataGridConfig<TestListVlaItem>
                {
                    GridId              = "testListVlaGrid",
                    Title               = "Test List for VLA",
                    ShowCheckboxColumn  = false,
                    ShowPagination      = true,
                    KeyProperty         = "ItemCode",
                    AllowAdd            = true,
                    AddFunction         = "addTestListVla",
                    AllowEdit           = true,
                    EditFunction        = "editTestListVla",
                    AllowDelete         = true,
                    DeleteFunction      = "deleteTestListVla",
                    ExtraFilterMethod   = "getTestListVlaExtraFilters",
                    BindGridUrl         = "/FPS/TestListVla/LoadTestListVlaGrid",
                    Data                = new List<TestListVlaItem>(),
                    Columns             = GridDataProvider.GetColumnsDefination<TestListVlaItem>(),
                    Pagination          = new PaginationModel()
                },

                // AllowAdd/Edit/Delete = true — tabGridModal + tabDeleteModal exist in HTML prototype
                TestRequirementsGrid = new DataGridConfig<TestRequirementItem>
                {
                    GridId              = "testRequirementsGrid",
                    Title               = "Test Requirements",
                    ShowCheckboxColumn  = false,
                    ShowPagination      = true,
                    KeyProperty         = "Buyer",
                    AllowAdd            = true,
                    AddFunction         = "addTestRequirement",
                    AllowEdit           = true,
                    EditFunction        = "editTestRequirement",
                    AllowDelete         = true,
                    DeleteFunction      = "deleteTestRequirement",
                    ExtraFilterMethod   = "getTestRequirementExtraFilters",
                    BindGridUrl         = "/FPS/TestListVla/LoadTestRequirementsGrid",
                    Data                = new List<TestRequirementItem>(),
                    Columns             = GridDataProvider.GetColumnsDefination<TestRequirementItem>(),
                    Pagination          = new PaginationModel()
                },

                // AllowAdd/Edit/Delete = true — tabGridModal + tabDeleteModal in HTML prototype
                ComponentChargesGeneralGrid = new DataGridConfig<TestRCCostItem>
                {
                    GridId              = "componentChargesGeneralGrid",
                    Title               = "Component Charges",
                    ShowCheckboxColumn  = false,
                    ShowPagination      = true,
                    KeyProperty         = "ProfitCentre",
                    AllowAdd            = true,
                    AddFunction         = "addComponentChargeGeneral",
                    AllowEdit           = true,
                    EditFunction        = "editComponentChargeGeneral",
                    AllowDelete         = true,
                    DeleteFunction      = "deleteComponentChargeGeneral",
                    ExtraFilterMethod   = "getComponentChargesExtraFilters",
                    BindGridUrl         = "/FPS/TestListVla/LoadComponentChargesGeneralGrid",
                    Data                = new List<TestRCCostItem>(),
                    Columns             = GridDataProvider.GetColumnsDefination<TestRCCostItem>(),
                    Pagination          = new PaginationModel()
                },

                // AllowAdd/Edit/Delete = true — tabGridModal + tabDeleteModal in HTML prototype
                ComponentChargesProjectGrid = new DataGridConfig<TestRequirementRCCostItem>
                {
                    GridId              = "componentChargesProjectGrid",
                    Title               = "Component Charges for Individual Projects",
                    ShowCheckboxColumn  = false,
                    ShowPagination      = true,
                    KeyProperty         = "ProfitCentre",
                    AllowAdd            = true,
                    AddFunction         = "addComponentChargeProject",
                    AllowEdit           = true,
                    EditFunction        = "editComponentChargeProject",
                    AllowDelete         = true,
                    DeleteFunction      = "deleteComponentChargeProject",
                    ExtraFilterMethod   = "getComponentChargesProjectExtraFilters",
                    BindGridUrl         = "/FPS/TestListVla/LoadComponentChargesProjectGrid",
                    Data                = new List<TestRequirementRCCostItem>(),
                    Columns             = GridDataProvider.GetColumnsDefination<TestRequirementRCCostItem>(),
                    Pagination          = new PaginationModel()
                },

                // Read-only listing of WorkGroups able to supply the selected test item
                SuppliersGrid = new DataGridConfig<TestCapabilityItem>
                {
                    GridId              = "suppliersGrid",
                    Title               = "WorkGroups able to Supply",
                    ShowCheckboxColumn  = false,
                    ShowPagination      = true,
                    KeyProperty         = "TestCode",
                    AllowAdd            = false,
                    AllowEdit           = false,
                    AllowDelete         = false,
                    BindGridUrl         = "/FPS/TestListVla/LoadSuppliersGrid",
                    Data                = new List<TestCapabilityItem>(),
                    Columns             = GridDataProvider.GetColumnsDefination<TestCapabilityItem>(),
                    Pagination          = new PaginationModel()
                }
            };

            return View(viewModel);
        }

        // ── MAIN TEST LIST GRID ───────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadTestListVlaGrid(PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var gridConfig = await BuildTestListVlaGridAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<TestListVlaItem>> BuildTestListVlaGridAsync(PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();
            var fpsYear = _fpsYearContext.Year;

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _testListVlaService.GetAllAsync(query, fpsYear);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<TestListVlaItem>>(response.Data)
                : new List<TestListVlaItem>();

            var paginationModel = response.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<TestListVlaItem>
            {
                GridId              = "testListVlaGrid",
                Title               = "Test List for VLA",
                ShowCheckboxColumn  = false,
                ShowPagination      = true,
                KeyProperty         = "ItemCode",
                AllowAdd            = true,
                AddFunction         = "addTestListVla",
                AllowEdit           = true,
                EditFunction        = "editTestListVla",
                AllowDelete         = true,
                DeleteFunction      = "deleteTestListVla",
                ExtraFilterMethod   = "getTestListVlaExtraFilters",
                BindGridUrl         = "/FPS/TestListVla/LoadTestListVlaGrid",
                Data                = items,
                Columns             = GridDataProvider.GetColumnsDefination<TestListVlaItem>(),
                Pagination          = paginationModel,
                CurrentFilters      = filterDict
            };
        }

        // ── MAIN TEST LIST CRUD ───────────────────────────────────────────────

        [HttpGet]
        public IActionResult CreateTestListVla()
        {
            return PartialView("_AddEditTestListVla", new TestListVlaItem { FpsYear = _fpsYearContext.Year });
        }

        [HttpPost]
        public async Task<IActionResult> CreateTestListVla([FromBody] TestListVlaItem model)
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

            var dto = _mapper.Map<TestListVlaDto>(model);
            dto.FpsYear = _fpsYearContext.Year;
            var result = await _testListVlaService.CreateAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "Test created successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create test.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        [HttpGet]
        public async Task<IActionResult> EditTestListVla(string itemCode)
        {
            var fpsYear = _fpsYearContext.Year;
            var result = await _testListVlaService.GetByIdAsync(itemCode, fpsYear);
            if (!result.Success)
                return NotFound();

            var item = _mapper.Map<TestListVlaItem>(result.Data);
            return PartialView("_AddEditTestListVla", item);
        }

        [HttpPost]
        public async Task<IActionResult> EditTestListVla([FromBody] TestListVlaItem model)
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

            var fpsYear = _fpsYearContext.Year;
            var dto = _mapper.Map<TestListVlaDto>(model);
            var result = await _testListVlaService.UpdateAsync(model.ItemCode, fpsYear, dto);

            return result.Success
                ? Json(new { success = true, message = "Test updated successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update test.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTestListVla(string itemCode)
        {
            var fpsYear = _fpsYearContext.Year;
            var result = await _testListVlaService.DeleteAsync(itemCode, fpsYear);
            return result.Success
                ? Json(new { success = true, message = "Test deleted successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete test.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        // ── TEST REQUIREMENTS TAB GRID ────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadTestRequirementsGrid(
            PaginationFilter<string> request, string? testCode = null)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var gridConfig = await BuildTestRequirementsGridAsync(request, testCode);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<TestRequirementItem>> BuildTestRequirementsGridAsync(
            PaginationFilter<string> request, string? testCode)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var query = _mapper.Map<QueryParameters<string>>(request);
            var items = new List<TestRequirementItem>();
            var paginationModel = new PaginationModel();

            if (!string.IsNullOrEmpty(testCode))
            {
                var response = await _testRequirementService.GetPagedTestReqmtAsync(query, testCode);
                if (response.Success && response.Data != null)
                    items = _mapper.Map<List<TestRequirementItem>>(response.Data);
                if (response.Pagination is not null)
                    paginationModel = _mapper.Map<PaginationModel>(response.Pagination);
            }

            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<TestRequirementItem>
            {
                GridId              = "testRequirementsGrid",
                Title               = string.IsNullOrEmpty(testCode)
                    ? "Test Requirements"
                    : $"Test Requirements for {testCode}",
                ShowCheckboxColumn  = false,
                ShowPagination      = true,
                KeyProperty         = "Buyer",
                AllowAdd            = true,
                AddFunction         = "addTestRequirement",
                AllowEdit           = true,
                EditFunction        = "editTestRequirement",
                AllowDelete         = true,
                DeleteFunction      = "deleteTestRequirement",
                ExtraFilterMethod   = "getTestRequirementExtraFilters",
                BindGridUrl         = "/FPS/TestListVla/LoadTestRequirementsGrid",
                Data                = items,
                Columns             = GridDataProvider.GetColumnsDefination<TestRequirementItem>(),
                Pagination          = paginationModel,
                CurrentFilters      = filterDict
            };
        }

        // ── TEST REQUIREMENT CRUD ─────────────────────────────────────────────

        [HttpGet]
        public IActionResult CreateTestRequirement(string testCode)
        {
            return PartialView("_AddEditTestRequirement",
                new TestRequirementItem { TestCode = testCode, FpsYear = _fpsYearContext.Year });
        }

        [HttpPost]
        public async Task<IActionResult> CreateTestRequirement([FromBody] TestRequirementItem model)
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

            var dto = _mapper.Map<TestRequirementDto>(model);
            dto.FpsYear = _fpsYearContext.Year;
            var result = await _testRequirementService.CreateTestReqmtAsync(dto);

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
        public async Task<IActionResult> EditTestRequirement(string testCode, string buyer)
        {
            var result = await _testRequirementService.GetTestReqmtByIdAsync(testCode, buyer);
            if (!result.Success)
                return NotFound();

            var item = _mapper.Map<TestRequirementItem>(result.Data);
            return PartialView("_AddEditTestRequirement", item);
        }

        [HttpPost]
        public async Task<IActionResult> EditTestRequirement([FromBody] TestRequirementItem model)
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

            var dto = _mapper.Map<TestRequirementDto>(model);
            var result = await _testRequirementService.UpdateTestReqmtAsync(dto);

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
        public async Task<IActionResult> DeleteTestRequirement(string testCode, string buyer)
        {
            var result = await _testRequirementService.DeleteTestReqmtAsync(testCode, buyer);
            return result.Success
                ? Json(new { success = true, message = "Test Requirement deleted successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete Test Requirement.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        // ── COMPONENT CHARGES GENERAL TAB GRID ───────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadComponentChargesGeneralGrid(
            PaginationFilter<string> request, string? testCode = null)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var gridConfig = await BuildComponentChargesGeneralGridAsync(request, testCode);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<TestRCCostItem>> BuildComponentChargesGeneralGridAsync(
            PaginationFilter<string> request, string? testCode)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();
            var fpsYear = _fpsYearContext.Year;

            var items = new List<TestRCCostItem>();
            var paginationModel = new PaginationModel();

            if (!string.IsNullOrEmpty(testCode))
            {
                var response = await _fpsApiClient.FpsTestRCCost.GetByTestCodeAsync(testCode, fpsYear);
                if (response.Success && response.Data != null)
                    items = _mapper.Map<List<TestRCCostItem>>(response.Data);
            }

            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<TestRCCostItem>
            {
                GridId              = "componentChargesGeneralGrid",
                Title               = string.IsNullOrEmpty(testCode)
                    ? "Component Charges"
                    : $"Component charges {testCode}",
                ShowCheckboxColumn  = false,
                ShowPagination      = true,
                KeyProperty         = "ProfitCentre",
                AllowAdd            = true,
                AddFunction         = "addComponentChargeGeneral",
                AllowEdit           = true,
                EditFunction        = "editComponentChargeGeneral",
                AllowDelete         = true,
                DeleteFunction      = "deleteComponentChargeGeneral",
                ExtraFilterMethod   = "getComponentChargesExtraFilters",
                BindGridUrl         = "/FPS/TestListVla/LoadComponentChargesGeneralGrid",
                Data                = items,
                Columns             = GridDataProvider.GetColumnsDefination<TestRCCostItem>(),
                Pagination          = paginationModel,
                CurrentFilters      = filterDict
            };
        }

        // ── COMPONENT CHARGE (GENERAL) CRUD ──────────────────────────────────

        [HttpGet]
        public IActionResult CreateComponentChargeGeneral(string testCode)
        {
            return PartialView("_AddEditComponentCharge",
                new TestRCCostItem { TestCode = testCode, FpsYear = _fpsYearContext.Year });
        }

        [HttpPost]
        public async Task<IActionResult> CreateComponentChargeGeneral([FromBody] TestRCCostItem model)
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

            var dto = _mapper.Map<TestRCCostDto>(model);
            dto.FpsYear = _fpsYearContext.Year;
            var result = await _fpsApiClient.FpsTestRCCost.CreateAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "Component charge created successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create component charge.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        [HttpGet]
        public async Task<IActionResult> EditComponentChargeGeneral(string testCode, string profitCentre)
        {
            var fpsYear = _fpsYearContext.Year;
            var result = await _fpsApiClient.FpsTestRCCost.GetByKeyAsync(testCode, profitCentre, fpsYear);
            if (!result.Success)
                return NotFound();

            var item = _mapper.Map<TestRCCostItem>(result.Data);
            return PartialView("_AddEditComponentCharge", item);
        }

        [HttpPost]
        public async Task<IActionResult> EditComponentChargeGeneral([FromBody] TestRCCostItem model)
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

            var fpsYear = _fpsYearContext.Year;
            var dto = _mapper.Map<TestRCCostDto>(model);
            var result = await _fpsApiClient.FpsTestRCCost.UpdateAsync(model.TestCode, model.ProfitCentre, fpsYear, dto);

            return result.Success
                ? Json(new { success = true, message = "Component charge updated successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update component charge.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteComponentChargeGeneral(string testCode, string profitCentre)
        {
            var fpsYear = _fpsYearContext.Year;
            var result = await _fpsApiClient.FpsTestRCCost.DeleteAsync(testCode, profitCentre, fpsYear);
            return result.Success
                ? Json(new { success = true, message = "Component charge deleted successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete component charge.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        // ── COMPONENT CHARGES PROJECT TAB GRID ───────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadComponentChargesProjectGrid(
            PaginationFilter<string> request, string? testCode = null)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var gridConfig = await BuildComponentChargesProjectGridAsync(request, testCode);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<TestRequirementRCCostItem>> BuildComponentChargesProjectGridAsync(
            PaginationFilter<string> request, string? testCode)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();
            var fpsYear = _fpsYearContext.Year;

            var items = new List<TestRequirementRCCostItem>();
            var paginationModel = new PaginationModel();

            if (!string.IsNullOrEmpty(testCode))
            {
                var response = await _fpsApiClient.FpsTestRequirementRCCost.GetByTestCodeAsync(testCode, fpsYear);
                if (response.Success && response.Data != null)
                    items = _mapper.Map<List<TestRequirementRCCostItem>>(response.Data);
            }

            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<TestRequirementRCCostItem>
            {
                GridId              = "componentChargesProjectGrid",
                Title               = string.IsNullOrEmpty(testCode)
                    ? "Component Charges for Individual Projects"
                    : $"Component charges {testCode}",
                ShowCheckboxColumn  = false,
                ShowPagination      = true,
                KeyProperty         = "ProfitCentre",
                AllowAdd            = true,
                AddFunction         = "addComponentChargeProject",
                AllowEdit           = true,
                EditFunction        = "editComponentChargeProject",
                AllowDelete         = true,
                DeleteFunction      = "deleteComponentChargeProject",
                ExtraFilterMethod   = "getComponentChargesProjectExtraFilters",
                BindGridUrl         = "/FPS/TestListVla/LoadComponentChargesProjectGrid",
                Data                = items,
                Columns             = GridDataProvider.GetColumnsDefination<TestRequirementRCCostItem>(),
                Pagination          = paginationModel,
                CurrentFilters      = filterDict
            };
        }

        // ── COMPONENT CHARGE (PROJECT-SPECIFIC) CRUD ─────────────────────────

        [HttpGet]
        public IActionResult CreateComponentChargeProject(string testCode)
        {
            return PartialView("_AddEditComponentChargeProject",
                new TestRequirementRCCostItem { TestCode = testCode, FpsYear = _fpsYearContext.Year });
        }

        [HttpPost]
        public async Task<IActionResult> CreateComponentChargeProject([FromBody] TestRequirementRCCostItem model)
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

            var dto = _mapper.Map<TestRequirementRCCostDto>(model);
            dto.FpsYear = _fpsYearContext.Year;
            var result = await _fpsApiClient.FpsTestRequirementRCCost.CreateAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "Project component charge created successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create project component charge.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        [HttpGet]
        public async Task<IActionResult> EditComponentChargeProject(
            string testCode, string buyer, string profitCentre)
        {
            var fpsYear = _fpsYearContext.Year;
            var result = await _fpsApiClient.FpsTestRequirementRCCost.GetByKeyAsync(testCode, buyer, profitCentre, fpsYear);
            if (!result.Success)
                return NotFound();

            var item = _mapper.Map<TestRequirementRCCostItem>(result.Data);
            return PartialView("_AddEditComponentChargeProject", item);
        }

        [HttpPost]
        public async Task<IActionResult> EditComponentChargeProject([FromBody] TestRequirementRCCostItem model)
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

            var fpsYear = _fpsYearContext.Year;
            var dto = _mapper.Map<TestRequirementRCCostDto>(model);
            var result = await _fpsApiClient.FpsTestRequirementRCCost.UpdateAsync(
                model.TestCode, model.Buyer, model.ProfitCentre, fpsYear, dto);

            return result.Success
                ? Json(new { success = true, message = "Project component charge updated successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update project component charge.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteComponentChargeProject(
            string testCode, string buyer, string profitCentre)
        {
            var fpsYear = _fpsYearContext.Year;
            var result = await _fpsApiClient.FpsTestRequirementRCCost.DeleteAsync(testCode, buyer, profitCentre, fpsYear);
            return result.Success
                ? Json(new { success = true, message = "Project component charge deleted successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete project component charge.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        // ── SUPPLIERS / WORKGROUPS TAB GRID ───────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadSuppliersGrid(
            PaginationFilter<string> request, string? testCode = null)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var gridConfig = await BuildSuppliersGridAsync(request, testCode);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<TestCapabilityItem>> BuildSuppliersGridAsync(
            PaginationFilter<string> request, string? testCode)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var query = _mapper.Map<QueryParameters<string>>(request);
            var items = new List<TestCapabilityItem>();
            var paginationModel = new PaginationModel();

            if (!string.IsNullOrEmpty(testCode))
            {
                // maps to testCode here (capability items keyed by TestCode)
                var response = await _testCapabilityService.GetPagedTestCapabilityByPortfolioAsync(query, testCode);
                if (response.Success && response.Data != null)
                    items = _mapper.Map<List<TestCapabilityItem>>(response.Data);
                if (response.Pagination is not null)
                    paginationModel = _mapper.Map<PaginationModel>(response.Pagination);
            }

            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<TestCapabilityItem>
            {
                GridId              = "suppliersGrid",
                Title               = string.IsNullOrEmpty(testCode)
                    ? "WorkGroups able to Supply"
                    : $"WorkGroups able to Supply {testCode}",
                ShowCheckboxColumn  = false,
                ShowPagination      = true,
                KeyProperty         = "TestCode",
                AllowAdd            = false,
                AllowEdit           = false,
                AllowDelete         = false,
                BindGridUrl         = "/FPS/TestListVla/LoadSuppliersGrid",
                Data                = items,
                Columns             = GridDataProvider.GetColumnsDefination<TestCapabilityItem>(),
                Pagination          = paginationModel,
                CurrentFilters      = filterDict
            };
        }
    }
}
