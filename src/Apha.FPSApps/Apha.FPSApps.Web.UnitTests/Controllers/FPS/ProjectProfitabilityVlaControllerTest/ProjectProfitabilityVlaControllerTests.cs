using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NSubstitute;
using System.Text.Json;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ProjectProfitabilityVlaControllerTest
{
    public class ProjectProfitabilityVlaControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly IProgramService _programService;
        private readonly ProjectProfitabilityVlaController _controller;

        public ProjectProfitabilityVlaControllerTests()
        {
            _mapper         = Substitute.For<IMapper>();
            _projectService = Substitute.For<IProjectService>();
            _programService = Substitute.For<IProgramService>();
            _controller     = new ProjectProfitabilityVlaController(_mapper, _projectService, _programService);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static ApiResponseDto<IEnumerable<ProgramDto>> MakeProgramResponse(
            params (string no, string name)[] programmes)
        {
            var dtos = programmes
                .Select(p => new ProgramDto { ProgramNo = p.no, ProgramName = p.name })
                .Cast<ProgramDto>();
            return ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(dtos);
        }

        private static ApiResponseDto<List<ManagerDto>> MakeManagerResponse(params string[] names) =>
            ApiResponseDto<List<ManagerDto>>.SuccessResponse(
                names.Select(n => new ManagerDto { Name = n }).ToList());

        private static ApiResponseDto<List<CustomerDto>> MakeCustomerResponse(params string[] customers) =>
            ApiResponseDto<List<CustomerDto>>.SuccessResponse(
                customers.Select(c => new CustomerDto { Customer = c }).ToList());

        private static ApiResponseDto<List<ProjectProfitabilityVlaDto>> MakeVlaResponse(
            params ProjectProfitabilityVlaDto[] items) =>
            ApiResponseDto<List<ProjectProfitabilityVlaDto>>.SuccessResponse(
                items.ToList(),
                new PaginationDto { PageNumber = 1, PageSize = 100, TotalRecords = items.Length });

        private static PaginationFilter<string> MakeGridRequest(int page = 1, int pageSize = 15) =>
            new() { Page = page, PageSize = pageSize };

        private static ProjectProfitabilityVlaDto MakeVlaItem(
            string jobCode,
            decimal staffCosts = 1000m,
            decimal budget = 5000m,
            decimal targetProfit = 500m) =>
            new()
            {
                JobCode      = jobCode,
                StaffCosts   = staffCosts,
                Budget       = budget,
                Profit       = budget - staffCosts,
                TargetProfit = targetProfit,
                OffTarget    = (budget - staffCosts) - targetProfit
            };

        private static JsonElement GetJsonElement(OkObjectResult okResult)
        {
            var json = JsonSerializer.Serialize(okResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private void SetupDropdowns()
        {
            _programService.GetAllProgramsAsync()
                .Returns(MakeProgramResponse(("P001", "Programme One"), ("P002", "Programme Two")));
            _projectService.GetManagersAsync()
                .Returns(MakeManagerResponse("John Smith", "Jane Doe"));
            _projectService.GetAllCustomersAsync()
                .Returns(MakeCustomerResponse("ACME Ltd", "Beta Corp"));
        }

        // ── Index ─────────────────────────────────────────────────────────────

        #region Index

        [Fact]
        public async Task Index_WithAllDropdownsPopulated_ReturnsViewResultWithViewModel()
        {
            // Arrange
            SetupDropdowns();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ProjectProfitabilityVlaViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Index_StatusListIsStaticWith4Options()
        {
            // Arrange
            SetupDropdowns();

            // Act
            var result = await _controller.Index();

            // Assert — StatusList: "" (All), Approved, Completed, Not Approved
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfitabilityVlaViewModel>(viewResult.Model);
            Assert.Equal(4, model.StatusList.Count);
            Assert.Contains(model.StatusList, s => s.Value == "" && s.Text == "All statuses");
            Assert.Contains(model.StatusList, s => s.Value == "Approved");
            Assert.Contains(model.StatusList, s => s.Value == "Completed");
            Assert.Contains(model.StatusList, s => s.Value == "Not Approved");
        }

        [Fact]
        public async Task Index_ProgramListIsPopulatedFromProgramService()
        {
            // Arrange
            SetupDropdowns();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfitabilityVlaViewModel>(viewResult.Model);
            Assert.Equal(2, model.ProgramList.Count);
            Assert.Contains(model.ProgramList, p => p.Value == "P001");
            Assert.Contains(model.ProgramList, p => p.Value == "P002");
        }

        [Fact]
        public async Task Index_ManagerListIsPopulatedFromProjectService()
        {
            // Arrange
            SetupDropdowns();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfitabilityVlaViewModel>(viewResult.Model);
            Assert.Equal(2, model.ManagerList.Count);
            Assert.Contains(model.ManagerList, m => m.Value == "John Smith");
            Assert.Contains(model.ManagerList, m => m.Value == "Jane Doe");
        }

        [Fact]
        public async Task Index_CustomerListIsPopulatedFromProjectService()
        {
            // Arrange
            SetupDropdowns();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfitabilityVlaViewModel>(viewResult.Model);
            Assert.Equal(2, model.CustomerList.Count);
            Assert.Contains(model.CustomerList, c => c.Value == "ACME Ltd");
            Assert.Contains(model.CustomerList, c => c.Value == "Beta Corp");
        }

        [Fact]
        public async Task Index_WhenProgramServiceFails_ProgramListIsEmpty()
        {
            // Arrange
            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Service error" } },
                    new ApiMetaDto()));
            _projectService.GetManagersAsync()
                .Returns(MakeManagerResponse("John Smith"));
            _projectService.GetAllCustomersAsync()
                .Returns(MakeCustomerResponse("ACME Ltd"));

            // Act
            var result = await _controller.Index();

            // Assert — program list empty; page still renders
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfitabilityVlaViewModel>(viewResult.Model);
            Assert.Empty(model.ProgramList);
        }

        [Fact]
        public async Task Index_WhenManagerServiceFails_ManagerListIsEmpty()
        {
            // Arrange
            _programService.GetAllProgramsAsync()
                .Returns(MakeProgramResponse(("P001", "Programme One")));
            _projectService.GetManagersAsync()
                .Returns(ApiResponseDto<List<ManagerDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Service error" } },
                    new ApiMetaDto()));
            _projectService.GetAllCustomersAsync()
                .Returns(MakeCustomerResponse("ACME Ltd"));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfitabilityVlaViewModel>(viewResult.Model);
            Assert.Empty(model.ManagerList);
        }

        [Fact]
        public async Task Index_WhenCustomerServiceFails_CustomerListIsEmpty()
        {
            // Arrange
            _programService.GetAllProgramsAsync()
                .Returns(MakeProgramResponse(("P001", "Programme One")));
            _projectService.GetManagersAsync()
                .Returns(MakeManagerResponse("John Smith"));
            _projectService.GetAllCustomersAsync()
                .Returns(ApiResponseDto<List<CustomerDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Service error" } },
                    new ApiMetaDto()));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfitabilityVlaViewModel>(viewResult.Model);
            Assert.Empty(model.CustomerList);
        }

        [Fact]
        public async Task Index_ProfitabilityVlaGrid_HasCorrectGridId()
        {
            // Arrange
            SetupDropdowns();

            // Act
            var result = await _controller.Index();

            // Assert — DataGridConfig built explicitly in Index()
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfitabilityVlaViewModel>(viewResult.Model);
            Assert.Equal("projectProfitabilityVlaGrid", model.ProfitabilityVlaGrid.GridId);
        }

        [Fact]
        public async Task Index_ProfitabilityVlaGrid_AllowAddEditDeleteAreFalse()
        {
            // Arrange
            SetupDropdowns();

            // Act
            var result = await _controller.Index();

            // Assert — read-only grid: showAddButton:false in JS; no edit/delete actions
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfitabilityVlaViewModel>(viewResult.Model);
            Assert.False(model.ProfitabilityVlaGrid.AllowAdd);
            Assert.False(model.ProfitabilityVlaGrid.AllowEdit);
            Assert.False(model.ProfitabilityVlaGrid.AllowDelete);
        }

        #endregion

        // ── LoadProjectProfitabilityVlaGrid ───────────────────────────────────

        #region LoadProjectProfitabilityVlaGrid

        [Fact]
        public async Task LoadProjectProfitabilityVlaGrid_WithInvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("request", "Required");
            var request = MakeGridRequest();

            // Act
            var result = await _controller.LoadProjectProfitabilityVlaGrid(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            var element = JsonSerializer.Deserialize<JsonElement>(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadProjectProfitabilityVlaGrid_WithValidRequest_ReturnsPartialViewWithDataGridConfig()
        {
            // Arrange
            var request = MakeGridRequest();
            var items = new List<ProjectProfitabilityVlaDto> { MakeVlaItem("PP001"), MakeVlaItem("PP002") };
            var apiResponse = MakeVlaResponse(items.ToArray());
            var mappedItems = new List<ProjectProfitabilityVlaItem>
            {
                new() { JobCode = "PP001" },
                new() { JobCode = "PP002" }
            };

            _mapper.Map<QueryParameters<string>>(request)
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 15 });
            _projectService.GetProjectProfitabilityVlaAsync(
                    Arg.Any<QueryParameters<string>>(),
                    null, null, null, null)
                .Returns(apiResponse);
            _mapper.Map<List<ProjectProfitabilityVlaItem>>(Arg.Any<List<ProjectProfitabilityVlaDto>>())
                .Returns(mappedItems);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadProjectProfitabilityVlaGrid(request);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
            Assert.IsType<DataGridConfig<ProjectProfitabilityVlaItem>>(partialResult.Model);
        }

        [Fact]
        public async Task LoadProjectProfitabilityVlaGrid_WithValidRequest_DataGridContainsItems()
        {
            // Arrange
            var request = MakeGridRequest();
            var items = new List<ProjectProfitabilityVlaDto> { MakeVlaItem("PP001") };
            var apiResponse = MakeVlaResponse(items.ToArray());
            var mappedItems = new List<ProjectProfitabilityVlaItem> { new() { JobCode = "PP001" } };

            _mapper.Map<QueryParameters<string>>(request)
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 15 });
            _projectService.GetProjectProfitabilityVlaAsync(
                    Arg.Any<QueryParameters<string>>(),
                    null, null, null, null)
                .Returns(apiResponse);
            _mapper.Map<List<ProjectProfitabilityVlaItem>>(Arg.Any<List<ProjectProfitabilityVlaDto>>())
                .Returns(mappedItems);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadProjectProfitabilityVlaGrid(request);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var config = Assert.IsType<DataGridConfig<ProjectProfitabilityVlaItem>>(partialResult.Model);
            Assert.Single(config.Data!);
            Assert.Equal("PP001", config.Data![0].JobCode);
        }

        [Fact]
        public async Task LoadProjectProfitabilityVlaGrid_WhenServiceReturnsFailure_GridDataIsEmpty()
        {
            // Arrange
            var request = MakeGridRequest();
            var failureResponse = ApiResponseDto<List<ProjectProfitabilityVlaDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Error" } },
                new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request)
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 15 });
            _projectService.GetProjectProfitabilityVlaAsync(
                    Arg.Any<QueryParameters<string>>(),
                    null, null, null, null)
                .Returns(failureResponse);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto?>())
                .Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadProjectProfitabilityVlaGrid(request);

            // Assert — partial view still returned but with empty data
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var config = Assert.IsType<DataGridConfig<ProjectProfitabilityVlaItem>>(partialResult.Model);
            Assert.Empty(config.Data!);
        }

        [Fact]
        public async Task LoadProjectProfitabilityVlaGrid_WithFilterParams_PassesFiltersToService()
        {
            // Arrange
            var request = MakeGridRequest();
            var apiResponse = MakeVlaResponse();

            _mapper.Map<QueryParameters<string>>(request)
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 15 });
            _projectService.GetProjectProfitabilityVlaAsync(
                    Arg.Any<QueryParameters<string>>(),
                    "Approved", "P001", "John Smith", "ACME Ltd")
                .Returns(apiResponse);
            _mapper.Map<List<ProjectProfitabilityVlaItem>>(Arg.Any<List<ProjectProfitabilityVlaDto>>())
                .Returns(new List<ProjectProfitabilityVlaItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadProjectProfitabilityVlaGrid(
                request,
                projectStatus: "Approved",
                programNo: "P001",
                manager: "John Smith",
                customer: "ACME Ltd");

            // Assert
            Assert.IsType<PartialViewResult>(result);
            await _projectService.Received(1).GetProjectProfitabilityVlaAsync(
                Arg.Any<QueryParameters<string>>(),
                "Approved", "P001", "John Smith", "ACME Ltd");
        }

        [Fact]
        public async Task LoadProjectProfitabilityVlaGrid_WithEmptyStringFilters_TreatsAsNull()
        {
            // Arrange — empty-string filters are normalised to null inside GetProjectProfitabilityVlaGridConfigAsync
            var request = MakeGridRequest();
            var apiResponse = MakeVlaResponse();

            _mapper.Map<QueryParameters<string>>(request)
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 15 });
            _projectService.GetProjectProfitabilityVlaAsync(
                    Arg.Any<QueryParameters<string>>(),
                    null, null, null, null)
                .Returns(apiResponse);
            _mapper.Map<List<ProjectProfitabilityVlaItem>>(Arg.Any<List<ProjectProfitabilityVlaDto>>())
                .Returns(new List<ProjectProfitabilityVlaItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadProjectProfitabilityVlaGrid(
                request, projectStatus: "", programNo: "", manager: "", customer: "");

            // Assert — empty strings normalised to null; same service call as no-filter
            Assert.IsType<PartialViewResult>(result);
            await _projectService.Received(1).GetProjectProfitabilityVlaAsync(
                Arg.Any<QueryParameters<string>>(),
                null, null, null, null);
        }

        #endregion

        // ── GetProjectProfitabilityVlaSummary ─────────────────────────────────

        #region GetProjectProfitabilityVlaSummary

        [Fact]
        public async Task GetProjectProfitabilityVlaSummary_WithData_ReturnsOkWithAggregatedTotals()
        {
            // Arrange
            var items = new List<ProjectProfitabilityVlaDto>
            {
                MakeVlaItem("PP001", staffCosts: 1000m, budget: 5000m, targetProfit: 3500m),
                MakeVlaItem("PP002", staffCosts: 2000m, budget: 6000m, targetProfit: 3000m)
            };
            // Set financial fields explicitly so aggregation is predictable
            items[0].StaffCosts = 1000m; items[0].TestCost = 200m; items[0].AnimalCosts = 100m;
            items[0].AdditionalCosts = 50m; items[0].TotalCosts = 1350m; items[0].Budget = 5000m;
            items[0].Profit = 3650m; items[0].TargetProfit = 3500m; items[0].OffTarget = 150m;
            items[1].StaffCosts = 2000m; items[1].TestCost = 300m; items[1].AnimalCosts = 200m;
            items[1].AdditionalCosts = 100m; items[1].TotalCosts = 2600m; items[1].Budget = 6000m;
            items[1].Profit = 3400m; items[1].TargetProfit = 3000m; items[1].OffTarget = 400m;

            var apiResponse = ApiResponseDto<List<ProjectProfitabilityVlaDto>>.SuccessResponse(
                items, new PaginationDto { TotalRecords = 2 });

            _projectService.GetProjectProfitabilityVlaAsync(
                    Arg.Any<QueryParameters<string>>(),
                    null, null, null, null)
                .Returns(apiResponse);

            // Act
            var result = await _controller.GetProjectProfitabilityVlaSummary();

            // Assert — OkObjectResult with 9 aggregated totals
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = GetJsonElement(okResult);
            Assert.Equal(3000m, json.GetProperty("totalStaffCosts").GetDecimal());
            Assert.Equal(500m, json.GetProperty("totalTestCost").GetDecimal());
            Assert.Equal(300m, json.GetProperty("totalAnimalCosts").GetDecimal());
            Assert.Equal(150m, json.GetProperty("totalAdditionalCosts").GetDecimal());
            Assert.Equal(3950m, json.GetProperty("totalTotalCosts").GetDecimal());
            Assert.Equal(11000m, json.GetProperty("totalBudget").GetDecimal());
            Assert.Equal(7050m, json.GetProperty("totalProfit").GetDecimal());
            Assert.Equal(6500m, json.GetProperty("totalTargetProfit").GetDecimal());
            Assert.Equal(550m, json.GetProperty("totalOffTarget").GetDecimal());
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaSummary_WithEmptyData_ReturnsAllZeroTotals()
        {
            // Arrange
            var apiResponse = ApiResponseDto<List<ProjectProfitabilityVlaDto>>.SuccessResponse(
                new List<ProjectProfitabilityVlaDto>(), new PaginationDto { TotalRecords = 0 });

            _projectService.GetProjectProfitabilityVlaAsync(
                    Arg.Any<QueryParameters<string>>(),
                    null, null, null, null)
                .Returns(apiResponse);

            // Act
            var result = await _controller.GetProjectProfitabilityVlaSummary();

            // Assert — all totals are zero when no data
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = GetJsonElement(okResult);
            Assert.Equal(0m, json.GetProperty("totalStaffCosts").GetDecimal());
            Assert.Equal(0m, json.GetProperty("totalProfit").GetDecimal());
            Assert.Equal(0m, json.GetProperty("totalOffTarget").GetDecimal());
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaSummary_WhenServiceFails_Returns500WithErrors()
        {
            // Arrange
            var failureResponse = ApiResponseDto<List<ProjectProfitabilityVlaDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Backend unavailable", Code = "SERVICE_ERROR" } },
                new ApiMetaDto());

            _projectService.GetProjectProfitabilityVlaAsync(
                    Arg.Any<QueryParameters<string>>(),
                    null, null, null, null)
                .Returns(failureResponse);

            // Act
            var result = await _controller.GetProjectProfitabilityVlaSummary();

            // Assert — StatusCode 500 when service reports failure
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaSummary_WithFilterParams_PassesFiltersToService()
        {
            // Arrange
            var apiResponse = ApiResponseDto<List<ProjectProfitabilityVlaDto>>.SuccessResponse(
                new List<ProjectProfitabilityVlaDto>(), new PaginationDto { TotalRecords = 0 });

            _projectService.GetProjectProfitabilityVlaAsync(
                    Arg.Any<QueryParameters<string>>(),
                    "Approved", "P001", "John Smith", "ACME Ltd")
                .Returns(apiResponse);

            // Act
            var result = await _controller.GetProjectProfitabilityVlaSummary(
                projectStatus: "Approved",
                programNo: "P001",
                manager: "John Smith",
                customer: "ACME Ltd");

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _projectService.Received(1).GetProjectProfitabilityVlaAsync(
                Arg.Any<QueryParameters<string>>(),
                "Approved", "P001", "John Smith", "ACME Ltd");
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaSummary_WithWhitespaceFilters_TreatsAsNull()
        {
            // Arrange — whitespace-only strings are normalised to null inside the controller
            var apiResponse = ApiResponseDto<List<ProjectProfitabilityVlaDto>>.SuccessResponse(
                new List<ProjectProfitabilityVlaDto>(), new PaginationDto { TotalRecords = 0 });

            _projectService.GetProjectProfitabilityVlaAsync(
                    Arg.Any<QueryParameters<string>>(),
                    null, null, null, null)
                .Returns(apiResponse);

            // Act
            var result = await _controller.GetProjectProfitabilityVlaSummary(
                projectStatus: "  ", programNo: "  ", manager: "  ", customer: "  ");

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _projectService.Received(1).GetProjectProfitabilityVlaAsync(
                Arg.Any<QueryParameters<string>>(),
                null, null, null, null);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaSummary_FetchesAllRowsWithMaxIntPageSize()
        {
            // Arrange — summary requires all rows (no pagination) for aggregation
            var apiResponse = ApiResponseDto<List<ProjectProfitabilityVlaDto>>.SuccessResponse(
                new List<ProjectProfitabilityVlaDto>(), new PaginationDto());

            _projectService.GetProjectProfitabilityVlaAsync(
                    Arg.Is<QueryParameters<string>>(q => q.PageSize == int.MaxValue),
                    null, null, null, null)
                .Returns(apiResponse);

            // Act
            await _controller.GetProjectProfitabilityVlaSummary();

            // Assert — PageSize == int.MaxValue ensures all rows fetched for aggregation
            await _projectService.Received(1).GetProjectProfitabilityVlaAsync(
                Arg.Is<QueryParameters<string>>(q => q.PageSize == int.MaxValue),
                null, null, null, null);
        }

        #endregion
    }
}
