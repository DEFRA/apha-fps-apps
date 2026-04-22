using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Handler;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "PACTApiSettings:Scope")]
    public class TestListController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ITestListService _testListService;
        private readonly IFpsYearContext _fpsYearContext;

        public TestListController(IMapper mapper, ITestListService testListService, IFpsYearContext fpsYearContext)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _testListService = testListService ?? throw new ArgumentNullException(nameof(testListService));
            _fpsYearContext = fpsYearContext ?? throw new ArgumentNullException(nameof(fpsYearContext));
        }

        public async Task<IActionResult> Index()
        {
            var defaultRequest = new PaginationFilter<string>
            {
                Filter = "{}"
            };

            var testGridConfig = await GetTestOrProductGridConfigAsync(defaultRequest);

            var viewModel = new TestListViewModel
            {
                TestGrid = testGridConfig
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LoadTestGrid(PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var testGridConfig = await GetTestOrProductGridConfigAsync(request);
            return PartialView("_DataGrid", testGridConfig);
        }

        [HttpGet]
        public async Task<IActionResult> GetOwners()
        {
            try
            {
                var response = await _testListService.GetOwnersAsync();
                if (response.Success)
                {
                    return Json(new { success = true, data = response.Data });
                }
                return Json(new { success = false, message = "Failed to retrieve owners" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTestOrProduct([Required] string itemCode)
        {
            if (string.IsNullOrWhiteSpace(itemCode))
            {
                return Json(new { success = false, message = "Item Code cannot be null or empty." });
            }

            try
            {
                var response = await _testListService.GetTestOrProductByIdAsync(itemCode);
                if (response.Success && response.Data != null)
                {
                    var viewModel = _mapper.Map<TestOrProductViewModel>(response.Data);
                    return Json(new { success = true, data = viewModel });
                }
                return Json(new { success = false, message = $"Test/Product with Item Code '{itemCode}' not found." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error retrieving test/product: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTestOrProduct([FromBody] TestOrProductViewModel model)
        {
            if (model == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Request body cannot be null.",
                    errors = new[] { new { field = "", message = "Invalid request data." } }
                });
            }

            try
            {
                if (!ModelState.IsValid)
                {
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
                }

                model.FpsYear = _fpsYearContext.Year;
                var dto = _mapper.Map<TestOrProductDto>(model);
                var response = await _testListService.CreateTestOrProductAsync(dto);

                if (response.Success)
                {
                    return Json(new { success = true, message = "Test/Product created successfully", data = response.Data });
                }

                return Json(new
                {
                    success = false,
                    message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to create Test/Product",
                    errors = (response.Errors ?? new List<ApiErrorDto>()).Select(e => new
                    {
                        field = e.Code ?? string.Empty,
                        message = e.Message ?? "An unexpected error occurred."
                    })
                });
            }
            catch (Exception ex)
            {
                return Json(new 
                { 
                    success = false, 
                    message = $"Error creating test/product: {ex.Message}",
                    errors = new[] { new { field = "", message = ex.Message } }
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTestOrProduct([Required] string itemCode, [FromBody] TestOrProductViewModel model)
        {
            if (string.IsNullOrWhiteSpace(itemCode))
            {
                return Json(new
                {
                    success = false,
                    message = "Item Code cannot be null or empty.",
                    errors = new[] { new { field = "ItemCode", message = "Item Code is required." } }
                });
            }

            if (model == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Request body cannot be null.",
                    errors = new[] { new { field = "", message = "Invalid request data." } }
                });
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Validation failed",
                        errors = ModelState
                            .Where(kvp => kvp.Value!.Errors.Any())
                            .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                            {
                                field = kvp.Key,
                                message = e.ErrorMessage
                            }))
                    });
                }

                model.FpsYear = _fpsYearContext.Year;
                var dto = _mapper.Map<TestOrProductDto>(model);
                var response = await _testListService.UpdateTestOrProductAsync(itemCode, dto);

                if (response.Success)
                {
                    return Json(new { success = true, message = "Test/Product updated successfully", data = response.Data });
                }

                return Json(new 
                { 
                    success = false, 
                    message = response.Errors?.FirstOrDefault()?.Message ?? $"Failed to update Test/Product with Item Code '{itemCode}'.",
                    errors = (response.Errors ?? new List<ApiErrorDto>()).Select(e => new
                    {
                        field = e.Code ?? string.Empty,
                        message = e.Message ?? "An unexpected error occurred."
                    })
                });
            }
            catch (Exception ex)
            {
                return Json(new 
                { 
                    success = false, 
                    message = $"Error updating test/product: {ex.Message}",
                    errors = new[] { new { field = "", message = ex.Message } }
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTestOrProduct([Required] string itemCode)
        {
            if (string.IsNullOrWhiteSpace(itemCode))
            {
                return Json(new { success = false, message = "Item Code cannot be null or empty." });
            }

            try
            {
                var response = await _testListService.DeleteTestOrProductAsync(itemCode);
                if (response.Success && response.Data)
                {
                    return Json(new { success = true, message = $"Test/Product with Item Code '{itemCode}' deleted successfully." });
                }

                return Json(new { success = false, message = $"Test/Product with Item Code '{itemCode}' not found or could not be deleted." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error deleting test/product: {ex.Message}" });
            }
        }

        private async Task<DataGridConfig<TestOrProductViewModel>>  GetTestOrProductGridConfigAsync(PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            queryParameters.Filter = request.Filter;

            var response = await _testListService.GetPagedTestOrProductsAsync(queryParameters);

            List<TestOrProductViewModel> testItems = new List<TestOrProductViewModel>();
            if (response.Success && response.Data != null)
            {
                testItems = _mapper.Map<List<TestOrProductViewModel>>(response.Data);
            }

            // Map pagination properties
            PaginationModel paginationModel = new PaginationModel
            {
                TotalRecords = testItems.Count,
                PageSize = request.PageSize,
                PageNumber = request.Page
            };
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<TestOrProductViewModel>
            {
                GridId = "testGrid",
                Title = "Test/Product Maintenance",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "ItemCode",
                AllowAdd = true,
                AllowEdit = true,
                AllowDelete = true,
                AddFunction = "addTestOrProduct",
                EditFunction = "editTestOrProduct",
                DeleteFunction = "deleteTestOrProduct",
                BindGridUrl = "/PACT/TestList/LoadTestGrid",
                Data = testItems,
                Columns = GridDataProvider.GetColumnsDefination<TestOrProductViewModel>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }
    }
}