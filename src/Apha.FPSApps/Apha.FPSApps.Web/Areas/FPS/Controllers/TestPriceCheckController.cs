using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Handler;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class TestPriceCheckController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ITestorProductService _testListService;
        private readonly IFpsYearContext _fpsYearContext;

        public TestPriceCheckController(
            IMapper mapper,
            ITestorProductService testListService,
            IFpsYearContext fpsYearContext)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _testListService = testListService ?? throw new ArgumentNullException(nameof(testListService));
            _fpsYearContext = fpsYearContext ?? throw new ArgumentNullException(nameof(fpsYearContext));
        }

        public async Task<IActionResult> Index()
        {
            var defaultRequest = new PaginationFilter<string> { Filter = "{}" };
            var owners = await GetOwnersListAsync();

            var gridConfig = await GetTestPriceCheckGridConfigAsync(defaultRequest, "all", null);

            var viewModel = new TestPriceCheckViewModel
            {
                PriceCheckGrid = gridConfig,
                Owners = owners,
                SelectedPriceFilter = "all",
                SelectedOwner = null
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LoadTestPriceCheckGrid(
            PaginationFilter<string> request,
            string priceFilter = "all",
            string? owner = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var gridConfig = await GetTestPriceCheckGridConfigAsync(request, priceFilter, owner);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string testCode, string jobCode)
        {
            var response = await _testListService.GetTestPriceCheckByKeyAsync(testCode, jobCode);

            if (!response.Success || response.Data == null)
                return NotFound();

            var model = _mapper.Map<TestPriceCheckItem>(response.Data);

            model.IsDefraProjectList = new List<SelectListItem>
            {               
                new("Yes", "-1", model.IsDefraProject == -1),
                new("No",  "0",  model.IsDefraProject == 0)
            };

            return PartialView("_EditTestPriceCheck", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] TestPriceCheckItem model)
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

            var dto = _mapper.Map<TestPriceCheckDto>(model);

            var response = await _testListService.UpdateTestPriceCheckByKeyAsync(model.TestCode, model.JobCode, dto);

            if (response.Success)
                return Json(new { success = true, message = "Test price updated successfully." });

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to update test price.",
                errors = (response.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        private const string DefraProjectSortColumn = nameof(TestPriceCheckItem.IsDefraProject);

        private async Task<DataGridConfig<TestPriceCheckItem>> GetTestPriceCheckGridConfigAsync(
            PaginationFilter<string> request,
            string priceFilter,
            string? owner)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            var isSortingByDefraProject = string.Equals(
                request.SortBy,
                DefraProjectSortColumn,
                StringComparison.OrdinalIgnoreCase);

            List<TestPriceCheckItem> items;
            int totalRecords;

            if (isSortingByDefraProject)
            {
                // Step 1: fetch a first page (page 1, size 1) just to get TotalRecords.
                var probeQuery = _mapper.Map<QueryParameters<string>>(request);
                probeQuery.Filter  = request.Filter;
                probeQuery.Page     = 1;
                probeQuery.PageSize = 1;
                probeQuery.SortBy   = null; // don't ask the API to sort by this column

                var probeResponse = await _testListService.GetTestPriceCheckPagedAsync(probeQuery, priceFilter, owner);
                totalRecords = probeResponse.Pagination?.TotalRecords ?? 0;

                if (totalRecords == 0 || !probeResponse.Success)
                {
                    items = new List<TestPriceCheckItem>();
                }
                else
                {
                    // Step 2: fetch all records in one call, unsorted by the API.
                    var allQuery = _mapper.Map<QueryParameters<string>>(request);
                    allQuery.Filter   = request.Filter;
                    allQuery.Page     = 1;
                    allQuery.PageSize = totalRecords;
                    allQuery.SortBy   = null;

                    var allResponse = await _testListService.GetTestPriceCheckPagedAsync(allQuery, priceFilter, owner);

                    var allItems = allResponse.Success && allResponse.Data != null
                        ? _mapper.Map<List<TestPriceCheckItem>>(allResponse.Data)
                        : new List<TestPriceCheckItem>();

                    // Step 3: sort the full dataset in memory so Defra projects group together.
                    var sorted = request.Descending
                        ? allItems.OrderByDescending(x => x.IsDefraProject)
                        : allItems.OrderBy(x => x.IsDefraProject);

                    // Step 4: slice the requested page.
                    items = sorted
                        .Skip((request.Page - 1) * request.PageSize)
                        .Take(request.PageSize)
                        .ToList();
                }
            }
            else
            {
                // Normal path — let the API handle sorting and paging.
                var queryParameters = _mapper.Map<QueryParameters<string>>(request);
                queryParameters.Filter = request.Filter;

                var response = await _testListService.GetTestPriceCheckPagedAsync(queryParameters, priceFilter, owner);

                totalRecords = response.Pagination?.TotalRecords ?? 0;
                items = response.Success && response.Data != null
                    ? _mapper.Map<List<TestPriceCheckItem>>(response.Data)
                    : new List<TestPriceCheckItem>();
            }

            var paginationModel = new PaginationModel
            {
                TotalRecords = totalRecords,
                PageSize     = request.PageSize,
                PageNumber   = request.Page,
                SortColumn   = request.SortBy,
                SortDirection = request.Descending
            };

            return new DataGridConfig<TestPriceCheckItem>
            {
                GridId           = "testPriceCheckGrid",
                Title            = "Test Price Check",
                ShowCheckboxColumn = false,
                ShowPagination   = true,
                KeyProperty      = "TestCode",
                AllowAdd         = false,
                AllowEdit        = true,
                AllowDelete      = false,
                EditFunction     = "editTestPriceCheck",
                ExtraFilterMethod = "getTestPriceCheckExtraFilters",
                BindGridUrl      = "/FPS/TestPriceCheck/LoadTestPriceCheckGrid",
                Data             = items,
                Columns          = GridDataProvider.GetColumnsDefination<TestPriceCheckItem>(null),
                Pagination       = paginationModel,
                CurrentFilters   = filterDict
            };
        }

        private async Task<List<string>> GetOwnersListAsync()
        {
            var response = await _testListService.GetOwnersAsync();
            return response.Success && response.Data != null ? response.Data : new List<string>();
        }
    }
}
