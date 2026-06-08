using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PIMS.Controllers;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PIMS.Controllers.ProjectYearCostsControllerTest
{
    public class ProjectYearCostsControllerTests
    {
        private readonly IProjectYearCostsService _yearCostsServiceMock;
        private readonly IProjectListService _projectListServiceMock;
        private readonly IProjectDetailsService _projectDetailsServiceMock;
        private readonly IMapper _mapperMock;
        private readonly ProjectYearCostsController _controller;

        public ProjectYearCostsControllerTests()
        {
            _yearCostsServiceMock   = Substitute.For<IProjectYearCostsService>();
            _projectListServiceMock = Substitute.For<IProjectListService>();
            _projectDetailsServiceMock = Substitute.For<IProjectDetailsService>();
            _mapperMock             = Substitute.For<IMapper>();

            _controller = new ProjectYearCostsController(
                _mapperMock,
                _yearCostsServiceMock,
                _projectListServiceMock,
                _projectDetailsServiceMock);

            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Substitute.For<ITempDataProvider>());
        }

        // ── helpers ──────────────────────────────────────────────────────

        private static PaginationFilter<string> DefaultRequest()
            => new() { Filter = "{}", Page = 1, PageSize = 10 };

        private static ApiResponseDto<List<T>> OkListResponse<T>(List<T>? data = null)
            => new() { Success = true, Data = data ?? [] };

        private void SetupDefaultIndexMocks(
            List<ProjectListViewDto>? projects     = null,
            List<YearDto>?            years        = null,
            List<MonthlyPactDto>?     monthlyPact  = null)
        {
            _projectListServiceMock.GetAllProjectsListAsync()
                .Returns(new ApiResponseDto<List<ProjectListViewDto>>
                {
                    Success = true,
                    Data    = projects ?? [new ProjectListViewDto { Parentproject = "PP001" }]
                });

            _projectDetailsServiceMock.GetAllYearAsync()
                .Returns(new ApiResponseDto<List<YearDto>>
                {
                    Success = true,
                    Data    = years ?? [new YearDto { Value = 2024 }]
                });

            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());

            _yearCostsServiceMock
                .GetMonthlyPactDataAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<MonthlyPactDto>> { Success = true, Data = monthlyPact ?? [] });

            _mapperMock.Map<List<MonthlyPactItem>>(Arg.Any<List<MonthlyPactDto>>()).Returns([]);
            _mapperMock.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());
        }

        private void SetupAllPlanVsActualsMocks()
        {
            _yearCostsServiceMock.GetStaffPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<StaffCostDto>());
            _yearCostsServiceMock.GetStaffActualsAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<StaffCostDto>());
            _yearCostsServiceMock.GetTestPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<TestCostDto>());
            _yearCostsServiceMock.GetTestActualsAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<TestCostDto>());
            _yearCostsServiceMock.GetAnimalPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<AnimalCostDto>());
            _yearCostsServiceMock.GetAnimalActualsAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<AnimalCostDto>());
            _yearCostsServiceMock.GetAdditionalPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<AdditionalCostDto>());
            _yearCostsServiceMock.GetAdditionalActualsAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<AdditionalCostDto>());
        }

        private T? GetJsonProperty<T>(JsonResult result, string propertyName)
        {
            string serialized = System.Text.Json.JsonSerializer.Serialize(result.Value);
            using System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(serialized);
            return doc.RootElement.TryGetProperty(propertyName, out System.Text.Json.JsonElement element)
                ? System.Text.Json.JsonSerializer.Deserialize<T>(element.GetRawText())
                : default;
        }

        // ── Constructor ───────────────────────────────────────────────────

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_InitializesController()
        {
            var controller = new ProjectYearCostsController(
                _mapperMock, _yearCostsServiceMock, _projectListServiceMock, _projectDetailsServiceMock);
            Assert.NotNull(controller);
        }

        #endregion

        // ── Index ─────────────────────────────────────────────────────────

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewResult()
        {
            SetupDefaultIndexMocks();
            var result = await _controller.Index(null, null);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_ReturnsProjectYearCostsViewModel()
        {
            SetupDefaultIndexMocks();
            var result = await _controller.Index(null, null);
            Assert.IsType<ProjectYearCostsViewModel>(Assert.IsType<ViewResult>(result).Model);
        }

        [Fact]
        public async Task Index_CallsGetAllProjectsListAsync_Once()
        {
            SetupDefaultIndexMocks();
            await _controller.Index(null, null);
            await _projectListServiceMock.Received(1).GetAllProjectsListAsync();
        }

        [Fact]
        public async Task Index_CallsGetAllYearAsync_Once()
        {
            SetupDefaultIndexMocks();
            await _controller.Index(null, null);
            await _projectDetailsServiceMock.Received(1).GetAllYearAsync();
        }

        [Fact]
        public async Task Index_CallsGetMonthlyPactDataAsync_Once()
        {
            SetupDefaultIndexMocks();
            await _controller.Index(null, null);
            await _yearCostsServiceMock.Received(1).GetMonthlyPactDataAsync(
                Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task Index_WithNoParameters_SelectsFirstProjectFromOptions()
        {
            SetupDefaultIndexMocks(projects: [new ProjectListViewDto { Parentproject = "PP001" }]);
            var result = await _controller.Index(null, null);
            var model  = Assert.IsType<ProjectYearCostsViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("PP001", model.Parentproject);
        }

        [Fact]
        public async Task Index_WithNoParameters_SelectsMaxYear()
        {
            SetupDefaultIndexMocks(years:
            [
                new YearDto { Value = 2022 },
                new YearDto { Value = 2024 },
                new YearDto { Value = 2023 }
            ]);
            var result = await _controller.Index(null, null);
            var model  = Assert.IsType<ProjectYearCostsViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal((short)2024, model.SelectedYear);
        }

        [Fact]
        public async Task Index_WithExplicitParameters_UsesProvidedProjectAndYear()
        {
            SetupDefaultIndexMocks();
            var result = await _controller.Index("PP999", 2022);
            var model  = Assert.IsType<ProjectYearCostsViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("PP999",    model.Parentproject);
            Assert.Equal((short)2022, model.SelectedYear);
        }

        [Fact]
        public async Task Index_ProjectOptions_ContainsServiceReturnedProjects()
        {
            SetupDefaultIndexMocks(projects:
            [
                new ProjectListViewDto { Parentproject = "PP001" },
                new ProjectListViewDto { Parentproject = "PP002" }
            ]);
            var result = await _controller.Index(null, null);
            var model  = Assert.IsType<ProjectYearCostsViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Contains(model.ProjectOptions, o => o.Value == "PP001");
            Assert.Contains(model.ProjectOptions, o => o.Value == "PP002");
        }

        [Fact]
        public async Task Index_YearOptions_AreOrderedByValue()
        {
            SetupDefaultIndexMocks(years:
            [
                new YearDto { Value = 2024 },
                new YearDto { Value = 2022 },
                new YearDto { Value = 2023 }
            ]);
            var result = await _controller.Index(null, null);
            var model  = Assert.IsType<ProjectYearCostsViewModel>(Assert.IsType<ViewResult>(result).Model);
            List<int> values = model.YearOptions.Select(o => int.Parse(o.Value!)).ToList();
            Assert.Equal(values.OrderBy(v => v).ToList(), values);
        }

        [Fact]
        public async Task Index_WhenProjectsDataIsNull_ProjectOptionsIsEmpty()
        {
            _projectListServiceMock.GetAllProjectsListAsync()
                .Returns(new ApiResponseDto<List<ProjectListViewDto>> { Success = true, Data = null });
            _projectDetailsServiceMock.GetAllYearAsync()
                .Returns(new ApiResponseDto<List<YearDto>> { Success = true, Data = [new YearDto { Value = 2024 }] });
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetMonthlyPactDataAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<MonthlyPactDto>> { Success = true, Data = [] });
            _mapperMock.Map<List<MonthlyPactItem>>(Arg.Any<List<MonthlyPactDto>>()).Returns([]);

            var result = await _controller.Index(null, null);
            var model  = Assert.IsType<ProjectYearCostsViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Empty(model.ProjectOptions);
        }

        [Fact]
        public async Task Index_WhenYearsDataIsNull_YearOptionsIsEmpty()
        {
            _projectListServiceMock.GetAllProjectsListAsync()
                .Returns(new ApiResponseDto<List<ProjectListViewDto>>
                {
                    Success = true,
                    Data    = [new ProjectListViewDto { Parentproject = "PP001" }]
                });
            _projectDetailsServiceMock.GetAllYearAsync()
                .Returns(new ApiResponseDto<List<YearDto>> { Success = true, Data = null });
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetMonthlyPactDataAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<MonthlyPactDto>> { Success = true, Data = [] });
            _mapperMock.Map<List<MonthlyPactItem>>(Arg.Any<List<MonthlyPactDto>>()).Returns([]);

            var result = await _controller.Index(null, null);
            var model  = Assert.IsType<ProjectYearCostsViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Empty(model.YearOptions);
        }

        [Fact]
        public async Task Index_MonthlyPactGrid_IsIncludedInViewModel()
        {
            SetupDefaultIndexMocks();
            var result = await _controller.Index(null, null);
            var model  = Assert.IsType<ProjectYearCostsViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.NotNull(model.MonthlyPactGrid);
        }

        [Fact]
        public async Task Index_WhenProjectListServiceThrowsException_PropagatesException()
        {
            _projectListServiceMock.GetAllProjectsListAsync()
                .ThrowsAsync(new Exception("Service unavailable"));
            _projectDetailsServiceMock.GetAllYearAsync()
                .Returns(new ApiResponseDto<List<YearDto>> { Success = true, Data = [] });

            await Assert.ThrowsAsync<Exception>(() => _controller.Index(null, null));
        }

        #endregion

        // ── LoadAdditionalPlansGrid ──────────────────────────────────────

        #region LoadAdditionalPlansGrid Tests

        [Fact]
        public async Task LoadAdditionalPlansGrid_WithInvalidModelState_ReturnsJsonResult()
        {
            _controller.ModelState.AddModelError("key", "error");
            var result = await _controller.LoadAdditionalPlansGrid(DefaultRequest(), "PP001", 2024);
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task LoadAdditionalPlansGrid_WithInvalidModelState_ReturnsFalseSuccess()
        {
            _controller.ModelState.AddModelError("key", "error");
            JsonResult result = Assert.IsType<JsonResult>(
                await _controller.LoadAdditionalPlansGrid(DefaultRequest(), "PP001", 2024));
            Assert.False(GetJsonProperty<bool>(result, "success"));
        }

        [Fact]
        public async Task LoadAdditionalPlansGrid_WithInvalidModelState_DoesNotCallService()
        {
            _controller.ModelState.AddModelError("key", "error");
            await _controller.LoadAdditionalPlansGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.DidNotReceive().GetAdditionalPlansAsync(
                Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadAdditionalPlansGrid_WithValidModelState_ReturnsDataGridPartialView()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetAdditionalPlansAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<AdditionalCostDto>());
            _mapperMock.Map<List<AdditionalCostPlanItem>>(Arg.Any<List<AdditionalCostDto>>()).Returns([]);

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadAdditionalPlansGrid(DefaultRequest(), "PP001", 2024));
            Assert.Equal("_DataGrid", result.ViewName);
        }

        [Fact]
        public async Task LoadAdditionalPlansGrid_CallsGetAdditionalPlansAsync_Once()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetAdditionalPlansAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<AdditionalCostDto>());
            _mapperMock.Map<List<AdditionalCostPlanItem>>(Arg.Any<List<AdditionalCostDto>>()).Returns([]);

            await _controller.LoadAdditionalPlansGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.Received(1).GetAdditionalPlansAsync(
                Arg.Is("PP001"), Arg.Is((short)2024), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadAdditionalPlansGrid_WhenServiceReturnsNullData_ReturnsEmptyGrid()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetAdditionalPlansAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<AdditionalCostDto>> { Success = true, Data = null });

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadAdditionalPlansGrid(DefaultRequest(), "PP001", 2024));
            DataGridConfig<AdditionalCostPlanItem> grid =
                Assert.IsType<DataGridConfig<AdditionalCostPlanItem>>(result.Model);
            Assert.Empty(grid.Data);
        }

        #endregion

        // ── LoadAdditionalActualsGrid ────────────────────────────────────

        #region LoadAdditionalActualsGrid Tests

        [Fact]
        public async Task LoadAdditionalActualsGrid_WithInvalidModelState_ReturnsJsonResult()
        {
            _controller.ModelState.AddModelError("key", "error");
            var result = await _controller.LoadAdditionalActualsGrid(DefaultRequest(), "PP001", 2024);
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task LoadAdditionalActualsGrid_WithInvalidModelState_ReturnsFalseSuccess()
        {
            _controller.ModelState.AddModelError("key", "error");
            JsonResult result = Assert.IsType<JsonResult>(
                await _controller.LoadAdditionalActualsGrid(DefaultRequest(), "PP001", 2024));
            Assert.False(GetJsonProperty<bool>(result, "success"));
        }

        [Fact]
        public async Task LoadAdditionalActualsGrid_WithInvalidModelState_DoesNotCallService()
        {
            _controller.ModelState.AddModelError("key", "error");
            await _controller.LoadAdditionalActualsGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.DidNotReceive().GetAdditionalActualsAsync(
                Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadAdditionalActualsGrid_WithValidModelState_ReturnsDataGridPartialView()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetAdditionalActualsAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<AdditionalCostDto>());
            _mapperMock.Map<List<AdditionalCostActualItem>>(Arg.Any<List<AdditionalCostDto>>()).Returns([]);

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadAdditionalActualsGrid(DefaultRequest(), "PP001", 2024));
            Assert.Equal("_DataGrid", result.ViewName);
        }

        [Fact]
        public async Task LoadAdditionalActualsGrid_CallsGetAdditionalActualsAsync_Once()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetAdditionalActualsAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<AdditionalCostDto>());
            _mapperMock.Map<List<AdditionalCostActualItem>>(Arg.Any<List<AdditionalCostDto>>()).Returns([]);

            await _controller.LoadAdditionalActualsGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.Received(1).GetAdditionalActualsAsync(
                Arg.Is("PP001"), Arg.Is((short)2024), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadAdditionalActualsGrid_WhenServiceReturnsNullData_ReturnsEmptyGrid()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetAdditionalActualsAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<AdditionalCostDto>> { Success = true, Data = null });

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadAdditionalActualsGrid(DefaultRequest(), "PP001", 2024));
            DataGridConfig<AdditionalCostActualItem> grid =
                Assert.IsType<DataGridConfig<AdditionalCostActualItem>>(result.Model);
            Assert.Empty(grid.Data);
        }

        #endregion

        // ── LoadAnimalPlansGrid ──────────────────────────────────────────

        #region LoadAnimalPlansGrid Tests

        [Fact]
        public async Task LoadAnimalPlansGrid_WithInvalidModelState_ReturnsJsonResult()
        {
            _controller.ModelState.AddModelError("key", "error");
            var result = await _controller.LoadAnimalPlansGrid(DefaultRequest(), "PP001", 2024);
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task LoadAnimalPlansGrid_WithInvalidModelState_DoesNotCallService()
        {
            _controller.ModelState.AddModelError("key", "error");
            await _controller.LoadAnimalPlansGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.DidNotReceive().GetAnimalPlansAsync(
                Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadAnimalPlansGrid_WithValidModelState_ReturnsDataGridPartialView()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetAnimalPlansAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<AnimalCostDto>());
            _mapperMock.Map<List<AnimalCostPlanItem>>(Arg.Any<List<AnimalCostDto>>()).Returns([]);

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadAnimalPlansGrid(DefaultRequest(), "PP001", 2024));
            Assert.Equal("_DataGrid", result.ViewName);
        }

        [Fact]
        public async Task LoadAnimalPlansGrid_CallsGetAnimalPlansAsync_Once()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetAnimalPlansAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<AnimalCostDto>());
            _mapperMock.Map<List<AnimalCostPlanItem>>(Arg.Any<List<AnimalCostDto>>()).Returns([]);

            await _controller.LoadAnimalPlansGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.Received(1).GetAnimalPlansAsync(
                Arg.Is("PP001"), Arg.Is((short)2024), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadAnimalPlansGrid_WhenServiceReturnsNullData_ReturnsEmptyGrid()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetAnimalPlansAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<AnimalCostDto>> { Success = true, Data = null });

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadAnimalPlansGrid(DefaultRequest(), "PP001", 2024));
            DataGridConfig<AnimalCostPlanItem> grid =
                Assert.IsType<DataGridConfig<AnimalCostPlanItem>>(result.Model);
            Assert.Empty(grid.Data);
        }

        #endregion

        // ── LoadAnimalActualsGrid ────────────────────────────────────────

        #region LoadAnimalActualsGrid Tests

        [Fact]
        public async Task LoadAnimalActualsGrid_WithInvalidModelState_ReturnsJsonResult()
        {
            _controller.ModelState.AddModelError("key", "error");
            var result = await _controller.LoadAnimalActualsGrid(DefaultRequest(), "PP001", 2024);
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task LoadAnimalActualsGrid_WithInvalidModelState_DoesNotCallService()
        {
            _controller.ModelState.AddModelError("key", "error");
            await _controller.LoadAnimalActualsGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.DidNotReceive().GetAnimalActualsAsync(
                Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadAnimalActualsGrid_WithValidModelState_ReturnsDataGridPartialView()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetAnimalActualsAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<AnimalCostDto>());
            _mapperMock.Map<List<AnimalCostActualItem>>(Arg.Any<List<AnimalCostDto>>()).Returns([]);

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadAnimalActualsGrid(DefaultRequest(), "PP001", 2024));
            Assert.Equal("_DataGrid", result.ViewName);
        }

        [Fact]
        public async Task LoadAnimalActualsGrid_CallsGetAnimalActualsAsync_Once()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetAnimalActualsAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<AnimalCostDto>());
            _mapperMock.Map<List<AnimalCostActualItem>>(Arg.Any<List<AnimalCostDto>>()).Returns([]);

            await _controller.LoadAnimalActualsGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.Received(1).GetAnimalActualsAsync(
                Arg.Is("PP001"), Arg.Is((short)2024), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadAnimalActualsGrid_WhenServiceReturnsNullData_ReturnsEmptyGrid()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetAnimalActualsAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<AnimalCostDto>> { Success = true, Data = null });

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadAnimalActualsGrid(DefaultRequest(), "PP001", 2024));
            DataGridConfig<AnimalCostActualItem> grid =
                Assert.IsType<DataGridConfig<AnimalCostActualItem>>(result.Model);
            Assert.Empty(grid.Data);
        }

        #endregion

        // ── LoadTestPlansGrid ────────────────────────────────────────────

        #region LoadTestPlansGrid Tests

        [Fact]
        public async Task LoadTestPlansGrid_WithInvalidModelState_ReturnsJsonResult()
        {
            _controller.ModelState.AddModelError("key", "error");
            var result = await _controller.LoadTestPlansGrid(DefaultRequest(), "PP001", 2024);
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task LoadTestPlansGrid_WithInvalidModelState_DoesNotCallService()
        {
            _controller.ModelState.AddModelError("key", "error");
            await _controller.LoadTestPlansGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.DidNotReceive().GetTestPlansAsync(
                Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadTestPlansGrid_WithValidModelState_ReturnsDataGridPartialView()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetTestPlansAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<TestCostDto>());
            _mapperMock.Map<List<TestCostPlanItem>>(Arg.Any<List<TestCostDto>>()).Returns([]);

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadTestPlansGrid(DefaultRequest(), "PP001", 2024));
            Assert.Equal("_DataGrid", result.ViewName);
        }

        [Fact]
        public async Task LoadTestPlansGrid_CallsGetTestPlansAsync_Once()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetTestPlansAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<TestCostDto>());
            _mapperMock.Map<List<TestCostPlanItem>>(Arg.Any<List<TestCostDto>>()).Returns([]);

            await _controller.LoadTestPlansGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.Received(1).GetTestPlansAsync(
                Arg.Is("PP001"), Arg.Is((short)2024), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadTestPlansGrid_WhenServiceReturnsNullData_ReturnsEmptyGrid()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetTestPlansAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<TestCostDto>> { Success = true, Data = null });

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadTestPlansGrid(DefaultRequest(), "PP001", 2024));
            DataGridConfig<TestCostPlanItem> grid =
                Assert.IsType<DataGridConfig<TestCostPlanItem>>(result.Model);
            Assert.Empty(grid.Data);
        }

        #endregion

        // ── LoadTestActualsGrid ──────────────────────────────────────────

        #region LoadTestActualsGrid Tests

        [Fact]
        public async Task LoadTestActualsGrid_WithInvalidModelState_ReturnsJsonResult()
        {
            _controller.ModelState.AddModelError("key", "error");
            var result = await _controller.LoadTestActualsGrid(DefaultRequest(), "PP001", 2024);
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task LoadTestActualsGrid_WithInvalidModelState_DoesNotCallService()
        {
            _controller.ModelState.AddModelError("key", "error");
            await _controller.LoadTestActualsGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.DidNotReceive().GetTestActualsAsync(
                Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadTestActualsGrid_WithValidModelState_ReturnsDataGridPartialView()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetTestActualsAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<TestCostDto>());
            _mapperMock.Map<List<TestCostActualItem>>(Arg.Any<List<TestCostDto>>()).Returns([]);

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadTestActualsGrid(DefaultRequest(), "PP001", 2024));
            Assert.Equal("_DataGrid", result.ViewName);
        }

        [Fact]
        public async Task LoadTestActualsGrid_CallsGetTestActualsAsync_Once()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetTestActualsAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<TestCostDto>());
            _mapperMock.Map<List<TestCostActualItem>>(Arg.Any<List<TestCostDto>>()).Returns([]);

            await _controller.LoadTestActualsGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.Received(1).GetTestActualsAsync(
                Arg.Is("PP001"), Arg.Is((short)2024), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadTestActualsGrid_WhenServiceReturnsNullData_ReturnsEmptyGrid()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetTestActualsAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<TestCostDto>> { Success = true, Data = null });

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadTestActualsGrid(DefaultRequest(), "PP001", 2024));
            DataGridConfig<TestCostActualItem> grid =
                Assert.IsType<DataGridConfig<TestCostActualItem>>(result.Model);
            Assert.Empty(grid.Data);
        }

        #endregion

        // ── LoadStaffPlansGrid ───────────────────────────────────────────

        #region LoadStaffPlansGrid Tests

        [Fact]
        public async Task LoadStaffPlansGrid_WithInvalidModelState_ReturnsJsonResult()
        {
            _controller.ModelState.AddModelError("key", "error");
            var result = await _controller.LoadStaffPlansGrid(DefaultRequest(), "PP001", 2024);
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task LoadStaffPlansGrid_WithInvalidModelState_DoesNotCallService()
        {
            _controller.ModelState.AddModelError("key", "error");
            await _controller.LoadStaffPlansGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.DidNotReceive().GetStaffPlansAsync(
                Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadStaffPlansGrid_WithValidModelState_ReturnsDataGridPartialView()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetStaffPlansAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<StaffCostDto>());
            _mapperMock.Map<List<StaffCostPlanItem>>(Arg.Any<List<StaffCostDto>>()).Returns([]);

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadStaffPlansGrid(DefaultRequest(), "PP001", 2024));
            Assert.Equal("_DataGrid", result.ViewName);
        }

        [Fact]
        public async Task LoadStaffPlansGrid_CallsGetStaffPlansAsync_Once()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetStaffPlansAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<StaffCostDto>());
            _mapperMock.Map<List<StaffCostPlanItem>>(Arg.Any<List<StaffCostDto>>()).Returns([]);

            await _controller.LoadStaffPlansGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.Received(1).GetStaffPlansAsync(
                Arg.Is("PP001"), Arg.Is((short)2024), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadStaffPlansGrid_WhenServiceReturnsNullData_ReturnsEmptyGrid()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetStaffPlansAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<StaffCostDto>> { Success = true, Data = null });

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadStaffPlansGrid(DefaultRequest(), "PP001", 2024));
            DataGridConfig<StaffCostPlanItem> grid =
                Assert.IsType<DataGridConfig<StaffCostPlanItem>>(result.Model);
            Assert.Empty(grid.Data);
        }

        #endregion

        // ── LoadStaffActualsGrid ─────────────────────────────────────────

        #region LoadStaffActualsGrid Tests

        [Fact]
        public async Task LoadStaffActualsGrid_WithInvalidModelState_ReturnsJsonResult()
        {
            _controller.ModelState.AddModelError("key", "error");
            var result = await _controller.LoadStaffActualsGrid(DefaultRequest(), "PP001", 2024);
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task LoadStaffActualsGrid_WithInvalidModelState_DoesNotCallService()
        {
            _controller.ModelState.AddModelError("key", "error");
            await _controller.LoadStaffActualsGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.DidNotReceive().GetStaffActualsAsync(
                Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadStaffActualsGrid_WithValidModelState_ReturnsDataGridPartialView()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetStaffActualsAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<StaffCostDto>());
            _mapperMock.Map<List<StaffCostActualItem>>(Arg.Any<List<StaffCostDto>>()).Returns([]);

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadStaffActualsGrid(DefaultRequest(), "PP001", 2024));
            Assert.Equal("_DataGrid", result.ViewName);
        }

        [Fact]
        public async Task LoadStaffActualsGrid_CallsGetStaffActualsAsync_Once()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetStaffActualsAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<StaffCostDto>());
            _mapperMock.Map<List<StaffCostActualItem>>(Arg.Any<List<StaffCostDto>>()).Returns([]);

            await _controller.LoadStaffActualsGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.Received(1).GetStaffActualsAsync(
                Arg.Is("PP001"), Arg.Is((short)2024), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadStaffActualsGrid_WhenServiceReturnsNullData_ReturnsEmptyGrid()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetStaffActualsAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<StaffCostDto>> { Success = true, Data = null });

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadStaffActualsGrid(DefaultRequest(), "PP001", 2024));
            DataGridConfig<StaffCostActualItem> grid =
                Assert.IsType<DataGridConfig<StaffCostActualItem>>(result.Model);
            Assert.Empty(grid.Data);
        }

        #endregion

        // ── GetPlanTotals ────────────────────────────────────────────────

        #region GetPlanTotals Tests

        [Fact]
        public async Task GetPlanTotals_ReturnsJsonResult()
        {
            _yearCostsServiceMock.GetStaffPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>()).Returns(OkListResponse<StaffCostDto>());
            _yearCostsServiceMock.GetTestPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>()).Returns(OkListResponse<TestCostDto>());
            _yearCostsServiceMock.GetAnimalPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>()).Returns(OkListResponse<AnimalCostDto>());
            _yearCostsServiceMock.GetAdditionalPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>()).Returns(OkListResponse<AdditionalCostDto>());

            var result = await _controller.GetPlanTotals("PP001", 2024);
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task GetPlanTotals_CallsAllFourPlanServices()
        {
            _yearCostsServiceMock.GetStaffPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>()).Returns(OkListResponse<StaffCostDto>());
            _yearCostsServiceMock.GetTestPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>()).Returns(OkListResponse<TestCostDto>());
            _yearCostsServiceMock.GetAnimalPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>()).Returns(OkListResponse<AnimalCostDto>());
            _yearCostsServiceMock.GetAdditionalPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>()).Returns(OkListResponse<AdditionalCostDto>());

            await _controller.GetPlanTotals("PP001", 2024);

            await _yearCostsServiceMock.Received(1).GetStaffPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
            await _yearCostsServiceMock.Received(1).GetTestPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
            await _yearCostsServiceMock.Received(1).GetAnimalPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
            await _yearCostsServiceMock.Received(1).GetAdditionalPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task GetPlanTotals_WithData_ReturnsCurrencyFormattedTotals()
        {
            _yearCostsServiceMock.GetStaffPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse([new StaffCostDto { Cost = 100m }]));
            _yearCostsServiceMock.GetTestPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse([new TestCostDto { Cost = 200m }]));
            _yearCostsServiceMock.GetAnimalPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse([new AnimalCostDto { Cost = 300d }]));
            _yearCostsServiceMock.GetAdditionalPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse([new AdditionalCostDto { ItemCost = 400m }]));

            JsonResult result = Assert.IsType<JsonResult>(await _controller.GetPlanTotals("PP001", 2024));
            Assert.Equal(100m.ToString("C"), GetJsonProperty<string>(result, "staffTotal"));
            Assert.Equal(200m.ToString("C"), GetJsonProperty<string>(result, "testTotal"));
            Assert.Equal(400m.ToString("C"), GetJsonProperty<string>(result, "additionalTotal"));
        }

        [Fact]
        public async Task GetPlanTotals_WhenAllDataIsNull_ReturnsZeroCurrencyTotals()
        {
            _yearCostsServiceMock.GetStaffPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<StaffCostDto>> { Success = true, Data = null });
            _yearCostsServiceMock.GetTestPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<TestCostDto>> { Success = true, Data = null });
            _yearCostsServiceMock.GetAnimalPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<AnimalCostDto>> { Success = true, Data = null });
            _yearCostsServiceMock.GetAdditionalPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<AdditionalCostDto>> { Success = true, Data = null });

            JsonResult result = Assert.IsType<JsonResult>(await _controller.GetPlanTotals("PP001", 2024));
            Assert.Equal(0m.ToString("C"), GetJsonProperty<string>(result, "staffTotal"));
            Assert.Equal(0m.ToString("C"), GetJsonProperty<string>(result, "additionalTotal"));
        }

        #endregion

        // ── LoadPlanStaffGrid ────────────────────────────────────────────

        #region LoadPlanStaffGrid Tests

        [Fact]
        public async Task LoadPlanStaffGrid_WithInvalidModelState_ReturnsJsonResult()
        {
            _controller.ModelState.AddModelError("key", "error");
            var result = await _controller.LoadPlanStaffGrid(DefaultRequest(), "PP001", 2024);
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task LoadPlanStaffGrid_WithInvalidModelState_DoesNotCallService()
        {
            _controller.ModelState.AddModelError("key", "error");
            await _controller.LoadPlanStaffGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.DidNotReceive().GetStaffPlansAsync(
                Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadPlanStaffGrid_WithValidModelState_ReturnsDataGridPartialView()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetStaffPlansAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<StaffCostDto>());
            _mapperMock.Map<List<StaffCostPlanItem>>(Arg.Any<List<StaffCostDto>>()).Returns([]);

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadPlanStaffGrid(DefaultRequest(), "PP001", 2024));
            Assert.Equal("_DataGrid", result.ViewName);
        }

        [Fact]
        public async Task LoadPlanStaffGrid_CallsGetStaffPlansAsync_Once()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetStaffPlansAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<StaffCostDto>());
            _mapperMock.Map<List<StaffCostPlanItem>>(Arg.Any<List<StaffCostDto>>()).Returns([]);

            await _controller.LoadPlanStaffGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.Received(1).GetStaffPlansAsync(
                Arg.Is("PP001"), Arg.Is((short)2024), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadPlanStaffGrid_WhenServiceReturnsNullData_ReturnsEmptyGrid()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetStaffPlansAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<StaffCostDto>> { Success = true, Data = null });

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadPlanStaffGrid(DefaultRequest(), "PP001", 2024));
            DataGridConfig<StaffCostPlanItem> grid =
                Assert.IsType<DataGridConfig<StaffCostPlanItem>>(result.Model);
            Assert.Empty(grid.Data);
        }

        #endregion

        // ── LoadPlanTestGrid ─────────────────────────────────────────────

        #region LoadPlanTestGrid Tests

        [Fact]
        public async Task LoadPlanTestGrid_WithInvalidModelState_ReturnsJsonResult()
        {
            _controller.ModelState.AddModelError("key", "error");
            var result = await _controller.LoadPlanTestGrid(DefaultRequest(), "PP001", 2024);
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task LoadPlanTestGrid_WithInvalidModelState_DoesNotCallService()
        {
            _controller.ModelState.AddModelError("key", "error");
            await _controller.LoadPlanTestGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.DidNotReceive().GetTestPlansAsync(
                Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadPlanTestGrid_WithValidModelState_ReturnsDataGridPartialView()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetTestPlansAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<TestCostDto>());
            _mapperMock.Map<List<TestCostPlanItem>>(Arg.Any<List<TestCostDto>>()).Returns([]);

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadPlanTestGrid(DefaultRequest(), "PP001", 2024));
            Assert.Equal("_DataGrid", result.ViewName);
        }

        [Fact]
        public async Task LoadPlanTestGrid_CallsGetTestPlansAsync_Once()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetTestPlansAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<TestCostDto>());
            _mapperMock.Map<List<TestCostPlanItem>>(Arg.Any<List<TestCostDto>>()).Returns([]);

            await _controller.LoadPlanTestGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.Received(1).GetTestPlansAsync(
                Arg.Is("PP001"), Arg.Is((short)2024), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadPlanTestGrid_WhenServiceReturnsNullData_ReturnsEmptyGrid()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetTestPlansAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<TestCostDto>> { Success = true, Data = null });

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadPlanTestGrid(DefaultRequest(), "PP001", 2024));
            DataGridConfig<TestCostPlanItem> grid =
                Assert.IsType<DataGridConfig<TestCostPlanItem>>(result.Model);
            Assert.Empty(grid.Data);
        }

        #endregion

        // ── LoadPlanAnimalGrid ───────────────────────────────────────────

        #region LoadPlanAnimalGrid Tests

        [Fact]
        public async Task LoadPlanAnimalGrid_WithInvalidModelState_ReturnsJsonResult()
        {
            _controller.ModelState.AddModelError("key", "error");
            var result = await _controller.LoadPlanAnimalGrid(DefaultRequest(), "PP001", 2024);
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task LoadPlanAnimalGrid_WithInvalidModelState_DoesNotCallService()
        {
            _controller.ModelState.AddModelError("key", "error");
            await _controller.LoadPlanAnimalGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.DidNotReceive().GetAnimalPlansAsync(
                Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadPlanAnimalGrid_WithValidModelState_ReturnsDataGridPartialView()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetAnimalPlansAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<AnimalCostDto>());
            _mapperMock.Map<List<AnimalCostPlanItem>>(Arg.Any<List<AnimalCostDto>>()).Returns([]);

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadPlanAnimalGrid(DefaultRequest(), "PP001", 2024));
            Assert.Equal("_DataGrid", result.ViewName);
        }

        [Fact]
        public async Task LoadPlanAnimalGrid_CallsGetAnimalPlansAsync_Once()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetAnimalPlansAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<AnimalCostDto>());
            _mapperMock.Map<List<AnimalCostPlanItem>>(Arg.Any<List<AnimalCostDto>>()).Returns([]);

            await _controller.LoadPlanAnimalGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.Received(1).GetAnimalPlansAsync(
                Arg.Is("PP001"), Arg.Is((short)2024), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadPlanAnimalGrid_WhenServiceReturnsNullData_ReturnsEmptyGrid()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetAnimalPlansAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<AnimalCostDto>> { Success = true, Data = null });

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadPlanAnimalGrid(DefaultRequest(), "PP001", 2024));
            DataGridConfig<AnimalCostPlanItem> grid =
                Assert.IsType<DataGridConfig<AnimalCostPlanItem>>(result.Model);
            Assert.Empty(grid.Data);
        }

        #endregion

        // ── LoadPlanAdditionalGrid ───────────────────────────────────────

        #region LoadPlanAdditionalGrid Tests

        [Fact]
        public async Task LoadPlanAdditionalGrid_WithInvalidModelState_ReturnsJsonResult()
        {
            _controller.ModelState.AddModelError("key", "error");
            var result = await _controller.LoadPlanAdditionalGrid(DefaultRequest(), "PP001", 2024);
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task LoadPlanAdditionalGrid_WithInvalidModelState_DoesNotCallService()
        {
            _controller.ModelState.AddModelError("key", "error");
            await _controller.LoadPlanAdditionalGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.DidNotReceive().GetAdditionalPlansAsync(
                Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadPlanAdditionalGrid_WithValidModelState_ReturnsDataGridPartialView()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetAdditionalPlansAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<AdditionalCostDto>());
            _mapperMock.Map<List<AdditionalCostPlanItem>>(Arg.Any<List<AdditionalCostDto>>()).Returns([]);

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadPlanAdditionalGrid(DefaultRequest(), "PP001", 2024));
            Assert.Equal("_DataGrid", result.ViewName);
        }

        [Fact]
        public async Task LoadPlanAdditionalGrid_CallsGetAdditionalPlansAsync_Once()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetAdditionalPlansAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<AdditionalCostDto>());
            _mapperMock.Map<List<AdditionalCostPlanItem>>(Arg.Any<List<AdditionalCostDto>>()).Returns([]);

            await _controller.LoadPlanAdditionalGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.Received(1).GetAdditionalPlansAsync(
                Arg.Is("PP001"), Arg.Is((short)2024), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadPlanAdditionalGrid_WhenServiceReturnsNullData_ReturnsEmptyGrid()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetAdditionalPlansAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<AdditionalCostDto>> { Success = true, Data = null });

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadPlanAdditionalGrid(DefaultRequest(), "PP001", 2024));
            DataGridConfig<AdditionalCostPlanItem> grid =
                Assert.IsType<DataGridConfig<AdditionalCostPlanItem>>(result.Model);
            Assert.Empty(grid.Data);
        }

        #endregion

        // ── LoadPactPayGrid ──────────────────────────────────────────────

        #region LoadPactPayGrid Tests

        [Fact]
        public async Task LoadPactPayGrid_WithInvalidModelState_ReturnsJsonResult()
        {
            _controller.ModelState.AddModelError("key", "error");
            var result = await _controller.LoadPactPayGrid(DefaultRequest(), "PP001", 2024);
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task LoadPactPayGrid_WithInvalidModelState_ReturnsFalseSuccess()
        {
            _controller.ModelState.AddModelError("key", "error");
            JsonResult result = Assert.IsType<JsonResult>(
                await _controller.LoadPactPayGrid(DefaultRequest(), "PP001", 2024));
            Assert.False(GetJsonProperty<bool>(result, "success"));
        }

        [Fact]
        public async Task LoadPactPayGrid_WithInvalidModelState_DoesNotCallService()
        {
            _controller.ModelState.AddModelError("key", "error");
            await _controller.LoadPactPayGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.DidNotReceive().GetPactPayAsync(
                Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadPactPayGrid_WithValidModelState_ReturnsDataGridPartialView()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetPactPayAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<PactPayDto>());
            _mapperMock.Map<List<PactPayItem>>(Arg.Any<List<PactPayDto>>()).Returns([]);

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadPactPayGrid(DefaultRequest(), "PP001", 2024));
            Assert.Equal("_DataGrid", result.ViewName);
        }

        [Fact]
        public async Task LoadPactPayGrid_CallsGetPactPayAsync_Once()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetPactPayAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<PactPayDto>());
            _mapperMock.Map<List<PactPayItem>>(Arg.Any<List<PactPayDto>>()).Returns([]);

            await _controller.LoadPactPayGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.Received(1).GetPactPayAsync(
                Arg.Is("PP001"), Arg.Is((short)2024), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadPactPayGrid_WhenServiceReturnsNullData_ReturnsEmptyGrid()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetPactPayAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<PactPayDto>> { Success = true, Data = null });

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadPactPayGrid(DefaultRequest(), "PP001", 2024));
            DataGridConfig<PactPayItem> grid = Assert.IsType<DataGridConfig<PactPayItem>>(result.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadPactPayGrid_WithData_SetsMonthNameOnItems()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetPactPayAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse([new PactPayDto { Month = 1d }]));
            _mapperMock.Map<List<PactPayItem>>(Arg.Any<List<PactPayDto>>())
                .Returns([new PactPayItem { Month = 1d }]);

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadPactPayGrid(DefaultRequest(), "PP001", 2024));
            DataGridConfig<PactPayItem> grid = Assert.IsType<DataGridConfig<PactPayItem>>(result.Model);
            Assert.NotNull(grid.Data[0].MonthName);
        }

        #endregion

        // ── GetPactPayTotals ─────────────────────────────────────────────

        #region GetPactPayTotals Tests

        [Fact]
        public async Task GetPactPayTotals_ReturnsJsonResult()
        {
            _yearCostsServiceMock.GetPactPayAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<PactPayDto>());
            var result = await _controller.GetPactPayTotals("PP001", 2024);
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task GetPactPayTotals_CallsGetPactPayAsync_Once()
        {
            _yearCostsServiceMock.GetPactPayAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<PactPayDto>());
            await _controller.GetPactPayTotals("PP001", 2024);
            await _yearCostsServiceMock.Received(1).GetPactPayAsync(
                Arg.Is("PP001"), Arg.Is((short)2024), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task GetPactPayTotals_WithData_ReturnsSummedCurrencyTotals()
        {
            _yearCostsServiceMock.GetPactPayAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse([
                    new PactPayDto { Pay = 100m, NonPay = 200m, Overhead = 300m, StaffCosts = 600m }
                ]));

            JsonResult result = Assert.IsType<JsonResult>(await _controller.GetPactPayTotals("PP001", 2024));
            Assert.Equal(100m.ToString("C"), GetJsonProperty<string>(result, "payTotal"));
            Assert.Equal(200m.ToString("C"), GetJsonProperty<string>(result, "nonPayTotal"));
            Assert.Equal(300m.ToString("C"), GetJsonProperty<string>(result, "overheadTotal"));
            Assert.Equal(600m.ToString("C"), GetJsonProperty<string>(result, "staffCostsTotal"));
        }

        [Fact]
        public async Task GetPactPayTotals_WhenDataIsNull_ReturnsZeroTotals()
        {
            _yearCostsServiceMock.GetPactPayAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<PactPayDto>> { Success = true, Data = null });

            JsonResult result = Assert.IsType<JsonResult>(await _controller.GetPactPayTotals("PP001", 2024));
            Assert.Equal(0m.ToString("C"), GetJsonProperty<string>(result, "payTotal"));
            Assert.Equal(0m.ToString("C"), GetJsonProperty<string>(result, "staffCostsTotal"));
        }

        #endregion

        // ── GetProjectYearDetails ────────────────────────────────────────

        #region GetProjectYearDetails Tests

        [Fact]
        public async Task GetProjectYearDetails_ReturnsJsonResult()
        {
            _yearCostsServiceMock.GetProjectYearDetailsAsync(Arg.Any<string>(), Arg.Any<short>())
                .Returns(new ApiResponseDto<ProjectYearDetailsDto> { Success = true, Data = new ProjectYearDetailsDto() });
            var result = await _controller.GetProjectYearDetails("PP001", 2024);
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task GetProjectYearDetails_CallsGetProjectYearDetailsAsync_Once()
        {
            _yearCostsServiceMock.GetProjectYearDetailsAsync(Arg.Any<string>(), Arg.Any<short>())
                .Returns(new ApiResponseDto<ProjectYearDetailsDto> { Success = true, Data = new ProjectYearDetailsDto() });
            await _controller.GetProjectYearDetails("PP001", 2024);
            await _yearCostsServiceMock.Received(1).GetProjectYearDetailsAsync(
                Arg.Is("PP001"), Arg.Is((short)2024));
        }

        [Fact]
        public async Task GetProjectYearDetails_WithData_ReturnsMappedFields()
        {
            var dto = new ProjectYearDetailsDto
            {
                Parentproject = "PP001",
                Manager       = "Manager A",
                Disease       = "Disease A",
                Contract      = "Contract A"
            };
            _yearCostsServiceMock.GetProjectYearDetailsAsync(Arg.Any<string>(), Arg.Any<short>())
                .Returns(new ApiResponseDto<ProjectYearDetailsDto> { Success = true, Data = dto });

            JsonResult result = Assert.IsType<JsonResult>(await _controller.GetProjectYearDetails("PP001", 2024));
            Assert.Equal("PP001",      GetJsonProperty<string>(result, "parentproject"));
            Assert.Equal("Manager A",  GetJsonProperty<string>(result, "manager"));
            Assert.Equal("Disease A",  GetJsonProperty<string>(result, "disease"));
            Assert.Equal("Contract A", GetJsonProperty<string>(result, "contract"));
        }

        [Fact]
        public async Task GetProjectYearDetails_WhenDataIsNull_ReturnsDefaultValues()
        {
            _yearCostsServiceMock.GetProjectYearDetailsAsync(Arg.Any<string>(), Arg.Any<short>())
                .Returns(new ApiResponseDto<ProjectYearDetailsDto> { Success = true, Data = null });

            JsonResult result = Assert.IsType<JsonResult>(await _controller.GetProjectYearDetails("PP001", 2024));
            Assert.Null(GetJsonProperty<string>(result, "parentproject"));
            Assert.Null(GetJsonProperty<string>(result, "manager"));
        }

        #endregion

        // ── LoadMonthlyPactGrid ──────────────────────────────────────────

        #region LoadMonthlyPactGrid Tests

        [Fact]
        public async Task LoadMonthlyPactGrid_WithInvalidModelState_ReturnsJsonResult()
        {
            _controller.ModelState.AddModelError("key", "error");
            var result = await _controller.LoadMonthlyPactGrid(DefaultRequest(), "PP001", 2024);
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task LoadMonthlyPactGrid_WithInvalidModelState_ReturnsFalseSuccess()
        {
            _controller.ModelState.AddModelError("key", "error");
            JsonResult result = Assert.IsType<JsonResult>(
                await _controller.LoadMonthlyPactGrid(DefaultRequest(), "PP001", 2024));
            Assert.False(GetJsonProperty<bool>(result, "success"));
        }

        [Fact]
        public async Task LoadMonthlyPactGrid_WithInvalidModelState_DoesNotCallService()
        {
            _controller.ModelState.AddModelError("key", "error");
            await _controller.LoadMonthlyPactGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.DidNotReceive().GetMonthlyPactDataAsync(
                Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadMonthlyPactGrid_WithValidModelState_ReturnsDataGridPartialView()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetMonthlyPactDataAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<MonthlyPactDto>());
            _mapperMock.Map<List<MonthlyPactItem>>(Arg.Any<List<MonthlyPactDto>>()).Returns([]);

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadMonthlyPactGrid(DefaultRequest(), "PP001", 2024));
            Assert.Equal("_DataGrid", result.ViewName);
        }

        [Fact]
        public async Task LoadMonthlyPactGrid_CallsGetMonthlyPactDataAsync_Once()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetMonthlyPactDataAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<MonthlyPactDto>());
            _mapperMock.Map<List<MonthlyPactItem>>(Arg.Any<List<MonthlyPactDto>>()).Returns([]);

            await _controller.LoadMonthlyPactGrid(DefaultRequest(), "PP001", 2024);
            await _yearCostsServiceMock.Received(1).GetMonthlyPactDataAsync(
                Arg.Is("PP001"), Arg.Is((short)2024), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadMonthlyPactGrid_WhenServiceReturnsNullData_ReturnsEmptyGrid()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetMonthlyPactDataAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<MonthlyPactDto>> { Success = true, Data = null });

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadMonthlyPactGrid(DefaultRequest(), "PP001", 2024));
            DataGridConfig<MonthlyPactItem> grid =
                Assert.IsType<DataGridConfig<MonthlyPactItem>>(result.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadMonthlyPactGrid_WithData_SetsPeriodnameOnItems()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetMonthlyPactDataAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse([new MonthlyPactDto { Monthno = 3d }]));
            _mapperMock.Map<List<MonthlyPactItem>>(Arg.Any<List<MonthlyPactDto>>())
                .Returns([new MonthlyPactItem { Monthno = 3d }]);

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadMonthlyPactGrid(DefaultRequest(), "PP001", 2024));
            DataGridConfig<MonthlyPactItem> grid =
                Assert.IsType<DataGridConfig<MonthlyPactItem>>(result.Model);
            Assert.Equal("Mar", grid.Data[0].Periodname);
        }

        [Fact]
        public async Task LoadMonthlyPactGrid_WithInvalidMonthno_SetsMonthnoAsString()
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _yearCostsServiceMock.GetMonthlyPactDataAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse([new MonthlyPactDto { Monthno = 99d }]));
            _mapperMock.Map<List<MonthlyPactItem>>(Arg.Any<List<MonthlyPactDto>>())
                .Returns([new MonthlyPactItem { Monthno = 99d }]);

            PartialViewResult result = Assert.IsType<PartialViewResult>(
                await _controller.LoadMonthlyPactGrid(DefaultRequest(), "PP001", 2024));
            DataGridConfig<MonthlyPactItem> grid =
                Assert.IsType<DataGridConfig<MonthlyPactItem>>(result.Model);
            Assert.Equal("99", grid.Data[0].Periodname);
        }

        #endregion

        // ── GetMonthlyPactTotals ─────────────────────────────────────────

        #region GetMonthlyPactTotals Tests

        [Fact]
        public async Task GetMonthlyPactTotals_ReturnsJsonResult()
        {
            _yearCostsServiceMock.GetMonthlyPactDataAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<MonthlyPactDto>());
            var result = await _controller.GetMonthlyPactTotals("PP001", 2024);
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task GetMonthlyPactTotals_CallsGetMonthlyPactDataAsync_Once()
        {
            _yearCostsServiceMock.GetMonthlyPactDataAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<MonthlyPactDto>());
            await _controller.GetMonthlyPactTotals("PP001", 2024);
            await _yearCostsServiceMock.Received(1).GetMonthlyPactDataAsync(
                Arg.Is("PP001"), Arg.Is((short)2024), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task GetMonthlyPactTotals_WithData_ReturnsSummedCurrencyTotals()
        {
            _yearCostsServiceMock.GetMonthlyPactDataAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse([
                    new MonthlyPactDto
                    {
                        Nonanimals    = 100m,
                        Animals       = 200m,
                        Timecosts     = 300m,
                        Transfercosts = 400m,
                        Totalcost     = 500m,
                        Totalhours    = 10d,
                        Invoices      = 600m,
                        Coiw          = 700m
                    }
                ]));

            JsonResult result = Assert.IsType<JsonResult>(await _controller.GetMonthlyPactTotals("PP001", 2024));
            Assert.Equal(100m.ToString("C"), GetJsonProperty<string>(result, "totalNonAnimals"));
            Assert.Equal(200m.ToString("C"), GetJsonProperty<string>(result, "totalAnimals"));
            Assert.Equal(300m.ToString("C"), GetJsonProperty<string>(result, "totalTimeCosts"));
            Assert.Equal(400m.ToString("C"), GetJsonProperty<string>(result, "totalTransferCosts"));
            Assert.Equal(500m.ToString("C"), GetJsonProperty<string>(result, "totalCost"));
            Assert.Equal(10d.ToString("N2"), GetJsonProperty<string>(result, "totalHours"));
            Assert.Equal(600m.ToString("C"), GetJsonProperty<string>(result, "totalInvoices"));
            Assert.Equal(700m.ToString("C"), GetJsonProperty<string>(result, "totalCoiw"));
        }

        [Fact]
        public async Task GetMonthlyPactTotals_WithMultipleRows_SumsAllRows()
        {
            _yearCostsServiceMock.GetMonthlyPactDataAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse([
                    new MonthlyPactDto { Totalcost = 100m, Totalhours = 5d },
                    new MonthlyPactDto { Totalcost = 200m, Totalhours = 3d }
                ]));

            JsonResult result = Assert.IsType<JsonResult>(await _controller.GetMonthlyPactTotals("PP001", 2024));
            Assert.Equal(300m.ToString("C"), GetJsonProperty<string>(result, "totalCost"));
            Assert.Equal(8d.ToString("N2"),  GetJsonProperty<string>(result, "totalHours"));
        }

        [Fact]
        public async Task GetMonthlyPactTotals_WhenDataIsNull_ReturnsZeroTotals()
        {
            _yearCostsServiceMock.GetMonthlyPactDataAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<MonthlyPactDto>> { Success = true, Data = null });

            JsonResult result = Assert.IsType<JsonResult>(await _controller.GetMonthlyPactTotals("PP001", 2024));
            Assert.Equal(0m.ToString("C"),  GetJsonProperty<string>(result, "totalNonAnimals"));
            Assert.Equal(0m.ToString("C"),  GetJsonProperty<string>(result, "totalCost"));
            Assert.Equal(0d.ToString("N2"), GetJsonProperty<string>(result, "totalHours"));
        }

        [Fact]
        public async Task GetMonthlyPactTotals_WhenNullableFieldsAreNull_TreatsAsZero()
        {
            _yearCostsServiceMock.GetMonthlyPactDataAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse([new MonthlyPactDto()]));

            JsonResult result = Assert.IsType<JsonResult>(await _controller.GetMonthlyPactTotals("PP001", 2024));
            Assert.Equal(0m.ToString("C"), GetJsonProperty<string>(result, "totalNonAnimals"));
            Assert.Equal(0m.ToString("C"), GetJsonProperty<string>(result, "totalAnimals"));
            Assert.Equal(0m.ToString("C"), GetJsonProperty<string>(result, "totalTimeCosts"));
        }

        #endregion

        // ── GetMonthlyPactFpsPlanned ─────────────────────────────────────

        #region GetMonthlyPactFpsPlanned Tests

        [Fact]
        public async Task GetMonthlyPactFpsPlanned_ReturnsJsonResult()
        {
            _yearCostsServiceMock.GetFpsYearTotalsAsync(Arg.Any<string>(), Arg.Any<short>())
                .Returns(new ApiResponseDto<FpsYearTotalsDto> { Success = true, Data = new FpsYearTotalsDto() });
            _yearCostsServiceMock.GetMonthlyPactDataAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<MonthlyPactDto>());

            var result = await _controller.GetMonthlyPactFpsPlanned("PP001", 2024);
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task GetMonthlyPactFpsPlanned_CallsGetFpsYearTotalsAsync_Once()
        {
            _yearCostsServiceMock.GetFpsYearTotalsAsync(Arg.Any<string>(), Arg.Any<short>())
                .Returns(new ApiResponseDto<FpsYearTotalsDto> { Success = true, Data = new FpsYearTotalsDto() });
            _yearCostsServiceMock.GetMonthlyPactDataAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<MonthlyPactDto>());

            await _controller.GetMonthlyPactFpsPlanned("PP001", 2024);
            await _yearCostsServiceMock.Received(1).GetFpsYearTotalsAsync(
                Arg.Is("PP001"), Arg.Is((short)2024));
        }

        [Fact]
        public async Task GetMonthlyPactFpsPlanned_CallsGetMonthlyPactDataAsync_Once()
        {
            _yearCostsServiceMock.GetFpsYearTotalsAsync(Arg.Any<string>(), Arg.Any<short>())
                .Returns(new ApiResponseDto<FpsYearTotalsDto> { Success = true, Data = new FpsYearTotalsDto() });
            _yearCostsServiceMock.GetMonthlyPactDataAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<MonthlyPactDto>());

            await _controller.GetMonthlyPactFpsPlanned("PP001", 2024);
            await _yearCostsServiceMock.Received(1).GetMonthlyPactDataAsync(
                Arg.Is("PP001"), Arg.Is((short)2024), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task GetMonthlyPactFpsPlanned_WithData_ReturnsCurrencyFormattedFields()
        {
            var fps = new FpsYearTotalsDto
            {
                Totalstaffcosts      = 100d,
                Totaltestcosts       = 200d,
                Totalanimalcosts     = 300d,
                Totaladditionalcosts = 400m,
                Totalcosts           = 1000d,
                Custincome           = 500m,
                Totalincome          = 600m,
                BudgetCvl            = 800m
            };
            _yearCostsServiceMock.GetFpsYearTotalsAsync(Arg.Any<string>(), Arg.Any<short>())
                .Returns(new ApiResponseDto<FpsYearTotalsDto> { Success = true, Data = fps });
            _yearCostsServiceMock.GetMonthlyPactDataAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<MonthlyPactDto>());

            JsonResult result = Assert.IsType<JsonResult>(
                await _controller.GetMonthlyPactFpsPlanned("PP001", 2024));
            Assert.Equal(100m.ToString("C"), GetJsonProperty<string>(result, "staffCosts"));
            Assert.Equal(200m.ToString("C"), GetJsonProperty<string>(result, "testCosts"));
            Assert.Equal(400m.ToString("C"), GetJsonProperty<string>(result, "exceptionalCosts"));
            Assert.Equal(500m.ToString("C"), GetJsonProperty<string>(result, "custIncome"));
            Assert.Equal(600m.ToString("C"), GetJsonProperty<string>(result, "totalIncome"));
            Assert.Equal(800m.ToString("C"), GetJsonProperty<string>(result, "budgetVla"));
        }

        [Fact]
        public async Task GetMonthlyPactFpsPlanned_WhenBudgetVlaIsZero_TotalCostPctIsEmpty()
        {
            _yearCostsServiceMock.GetFpsYearTotalsAsync(Arg.Any<string>(), Arg.Any<short>())
                .Returns(new ApiResponseDto<FpsYearTotalsDto>
                {
                    Success = true,
                    Data    = new FpsYearTotalsDto { BudgetCvl = 0m }
                });
            _yearCostsServiceMock.GetMonthlyPactDataAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse([new MonthlyPactDto { Totalcost = 500m }]));

            JsonResult result = Assert.IsType<JsonResult>(
                await _controller.GetMonthlyPactFpsPlanned("PP001", 2024));
            Assert.Equal(string.Empty, GetJsonProperty<string>(result, "totalCostPct"));
        }

        [Fact]
        public async Task GetMonthlyPactFpsPlanned_WhenBudgetVlaIsPositive_TotalCostPctIsCalculated()
        {
            _yearCostsServiceMock.GetFpsYearTotalsAsync(Arg.Any<string>(), Arg.Any<short>())
                .Returns(new ApiResponseDto<FpsYearTotalsDto>
                {
                    Success = true,
                    Data    = new FpsYearTotalsDto { BudgetCvl = 1000m }
                });
            _yearCostsServiceMock.GetMonthlyPactDataAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse([new MonthlyPactDto { Totalcost = 500m }]));

            JsonResult result = Assert.IsType<JsonResult>(
                await _controller.GetMonthlyPactFpsPlanned("PP001", 2024));
            Assert.Equal("50.00%", GetJsonProperty<string>(result, "totalCostPct"));
        }

        [Fact]
        public async Task GetMonthlyPactFpsPlanned_WhenFpsDataIsNull_ReturnsZeroDefaults()
        {
            _yearCostsServiceMock.GetFpsYearTotalsAsync(Arg.Any<string>(), Arg.Any<short>())
                .Returns(new ApiResponseDto<FpsYearTotalsDto> { Success = true, Data = null });
            _yearCostsServiceMock.GetMonthlyPactDataAsync(
                    Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse<MonthlyPactDto>());

            JsonResult result = Assert.IsType<JsonResult>(
                await _controller.GetMonthlyPactFpsPlanned("PP001", 2024));
            Assert.Equal(0m.ToString("C"), GetJsonProperty<string>(result, "staffCosts"));
            Assert.Equal(0m.ToString("C"), GetJsonProperty<string>(result, "testCosts"));
            Assert.Equal(string.Empty,     GetJsonProperty<string>(result, "totalCostPct"));
        }

        #endregion

        // ── GetPlanVsActualsTotals ───────────────────────────────────────

        #region GetPlanVsActualsTotals Tests

        [Fact]
        public async Task GetPlanVsActualsTotals_ReturnsJsonResult()
        {
            SetupAllPlanVsActualsMocks();
            var result = await _controller.GetPlanVsActualsTotals("PP001", 2024);
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task GetPlanVsActualsTotals_CallsAllEightServices()
        {
            SetupAllPlanVsActualsMocks();
            await _controller.GetPlanVsActualsTotals("PP001", 2024);

            await _yearCostsServiceMock.Received(1).GetStaffPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
            await _yearCostsServiceMock.Received(1).GetStaffActualsAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
            await _yearCostsServiceMock.Received(1).GetTestPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
            await _yearCostsServiceMock.Received(1).GetTestActualsAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
            await _yearCostsServiceMock.Received(1).GetAnimalPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
            await _yearCostsServiceMock.Received(1).GetAnimalActualsAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
            await _yearCostsServiceMock.Received(1).GetAdditionalPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
            await _yearCostsServiceMock.Received(1).GetAdditionalActualsAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task GetPlanVsActualsTotals_WithData_ReturnsCurrencyFormattedTotals()
        {
            _yearCostsServiceMock.GetStaffPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse([new StaffCostDto { Cost = 100m }]));
            _yearCostsServiceMock.GetStaffActualsAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse([new StaffCostDto { ActualCost = 110m }]));
            _yearCostsServiceMock.GetTestPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse([new TestCostDto { Cost = 200m }]));
            _yearCostsServiceMock.GetTestActualsAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse([new TestCostDto { Charge = 210m }]));
            _yearCostsServiceMock.GetAnimalPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse([new AnimalCostDto { Cost = 300d }]));
            _yearCostsServiceMock.GetAnimalActualsAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse([new AnimalCostDto { Amount = 310m }]));
            _yearCostsServiceMock.GetAdditionalPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse([new AdditionalCostDto { ItemCost = 400m }]));
            _yearCostsServiceMock.GetAdditionalActualsAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse([new AdditionalCostDto { Amount = 410m }]));

            JsonResult result = Assert.IsType<JsonResult>(
                await _controller.GetPlanVsActualsTotals("PP001", 2024));
            Assert.Equal(100m.ToString("C"), GetJsonProperty<string>(result, "staffPlansTotal"));
            Assert.Equal(110m.ToString("C"), GetJsonProperty<string>(result, "staffActualsTotal"));
            Assert.Equal(200m.ToString("C"), GetJsonProperty<string>(result, "testPlansTotal"));
            Assert.Equal(210m.ToString("C"), GetJsonProperty<string>(result, "testActualsTotal"));
            Assert.Equal(400m.ToString("C"), GetJsonProperty<string>(result, "additionalPlansTotal"));
            Assert.Equal(410m.ToString("C"), GetJsonProperty<string>(result, "additionalActualsTotal"));
        }

        [Fact]
        public async Task GetPlanVsActualsTotals_WhenAllDataIsNull_ReturnsZeroTotals()
        {
            _yearCostsServiceMock.GetStaffPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<StaffCostDto>> { Success = true, Data = null });
            _yearCostsServiceMock.GetStaffActualsAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<StaffCostDto>> { Success = true, Data = null });
            _yearCostsServiceMock.GetTestPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<TestCostDto>> { Success = true, Data = null });
            _yearCostsServiceMock.GetTestActualsAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<TestCostDto>> { Success = true, Data = null });
            _yearCostsServiceMock.GetAnimalPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<AnimalCostDto>> { Success = true, Data = null });
            _yearCostsServiceMock.GetAnimalActualsAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<AnimalCostDto>> { Success = true, Data = null });
            _yearCostsServiceMock.GetAdditionalPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<AdditionalCostDto>> { Success = true, Data = null });
            _yearCostsServiceMock.GetAdditionalActualsAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<AdditionalCostDto>> { Success = true, Data = null });

            JsonResult result = Assert.IsType<JsonResult>(
                await _controller.GetPlanVsActualsTotals("PP001", 2024));
            Assert.Equal(0m.ToString("C"), GetJsonProperty<string>(result, "staffPlansTotal"));
            Assert.Equal(0m.ToString("C"), GetJsonProperty<string>(result, "staffActualsTotal"));
            Assert.Equal(0m.ToString("C"), GetJsonProperty<string>(result, "testPlansTotal"));
            Assert.Equal(0m.ToString("C"), GetJsonProperty<string>(result, "testActualsTotal"));
            Assert.Equal(0m.ToString("C"), GetJsonProperty<string>(result, "animalPlansTotal"));
            Assert.Equal(0m.ToString("C"), GetJsonProperty<string>(result, "animalActualsTotal"));
            Assert.Equal(0m.ToString("C"), GetJsonProperty<string>(result, "additionalPlansTotal"));
            Assert.Equal(0m.ToString("C"), GetJsonProperty<string>(result, "additionalActualsTotal"));
        }

        [Fact]
        public async Task GetPlanVsActualsTotals_WithMultipleRows_SumsAllRows()
        {
            _yearCostsServiceMock.GetStaffPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .Returns(OkListResponse([new StaffCostDto { Cost = 100m }, new StaffCostDto { Cost = 200m }]));
            _yearCostsServiceMock.GetStaffActualsAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>()).Returns(OkListResponse<StaffCostDto>());
            _yearCostsServiceMock.GetTestPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>()).Returns(OkListResponse<TestCostDto>());
            _yearCostsServiceMock.GetTestActualsAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>()).Returns(OkListResponse<TestCostDto>());
            _yearCostsServiceMock.GetAnimalPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>()).Returns(OkListResponse<AnimalCostDto>());
            _yearCostsServiceMock.GetAnimalActualsAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>()).Returns(OkListResponse<AnimalCostDto>());
            _yearCostsServiceMock.GetAdditionalPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>()).Returns(OkListResponse<AdditionalCostDto>());
            _yearCostsServiceMock.GetAdditionalActualsAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>()).Returns(OkListResponse<AdditionalCostDto>());

            JsonResult result = Assert.IsType<JsonResult>(
                await _controller.GetPlanVsActualsTotals("PP001", 2024));
            Assert.Equal(300m.ToString("C"), GetJsonProperty<string>(result, "staffPlansTotal"));
        }

        [Fact]
        public async Task GetPlanVsActualsTotals_WhenServiceThrowsException_PropagatesException()
        {
            _yearCostsServiceMock.GetStaffPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>())
                .ThrowsAsync(new Exception("Service unavailable"));
            _yearCostsServiceMock.GetStaffActualsAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>()).Returns(OkListResponse<StaffCostDto>());
            _yearCostsServiceMock.GetTestPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>()).Returns(OkListResponse<TestCostDto>());
            _yearCostsServiceMock.GetTestActualsAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>()).Returns(OkListResponse<TestCostDto>());
            _yearCostsServiceMock.GetAnimalPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>()).Returns(OkListResponse<AnimalCostDto>());
            _yearCostsServiceMock.GetAnimalActualsAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>()).Returns(OkListResponse<AnimalCostDto>());
            _yearCostsServiceMock.GetAdditionalPlansAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>()).Returns(OkListResponse<AdditionalCostDto>());
            _yearCostsServiceMock.GetAdditionalActualsAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<QueryParameters<string>>()).Returns(OkListResponse<AdditionalCostDto>());

            await Assert.ThrowsAsync<Exception>(() => _controller.GetPlanVsActualsTotals("PP001", 2024));
        }

        #endregion

        // ── ExportToExcel ────────────────────────────────────────────────

        #region ExportToExcel Tests

        [Fact]
        public async Task ExportToExcel_ReturnsFileContentResult()
        {
            _yearCostsServiceMock.ExportProjectYearCostsToExcelAsync(Arg.Any<string>(), Arg.Any<short>())
                .Returns([0x50, 0x4B]);
            var result = await _controller.ExportToExcel("PP001", 2024);
            Assert.IsType<FileContentResult>(result);
        }

        [Fact]
        public async Task ExportToExcel_CallsExportProjectYearCostsToExcelAsync_Once()
        {
            _yearCostsServiceMock.ExportProjectYearCostsToExcelAsync(Arg.Any<string>(), Arg.Any<short>())
                .Returns([0x50, 0x4B]);
            await _controller.ExportToExcel("PP001", 2024);
            await _yearCostsServiceMock.Received(1).ExportProjectYearCostsToExcelAsync(
                Arg.Is("PP001"), Arg.Is((short)2024));
        }

        [Fact]
        public async Task ExportToExcel_ReturnsCorrectContentType()
        {
            _yearCostsServiceMock.ExportProjectYearCostsToExcelAsync(Arg.Any<string>(), Arg.Any<short>())
                .Returns([0x50, 0x4B]);
            FileContentResult result = Assert.IsType<FileContentResult>(
                await _controller.ExportToExcel("PP001", 2024));
            Assert.Equal(
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                result.ContentType);
        }

        [Fact]
        public async Task ExportToExcel_ReturnsCorrectFileName()
        {
            _yearCostsServiceMock.ExportProjectYearCostsToExcelAsync(Arg.Any<string>(), Arg.Any<short>())
                .Returns([0x50, 0x4B]);
            FileContentResult result = Assert.IsType<FileContentResult>(
                await _controller.ExportToExcel("PP001", 2024));
            Assert.Equal("ProjectYearCosts_PP001_2024.xlsx", result.FileDownloadName);
        }

        [Fact]
        public async Task ExportToExcel_ReturnsServiceBytes()
        {
            byte[] expected = [0x50, 0x4B, 0x03, 0x04];
            _yearCostsServiceMock.ExportProjectYearCostsToExcelAsync(Arg.Any<string>(), Arg.Any<short>())
                .Returns(expected);
            FileContentResult result = Assert.IsType<FileContentResult>(
                await _controller.ExportToExcel("PP001", 2024));
            Assert.Equal(expected, result.FileContents);
        }

        [Fact]
        public async Task ExportToExcel_WhenServiceThrowsException_PropagatesException()
        {
            _yearCostsServiceMock.ExportProjectYearCostsToExcelAsync(Arg.Any<string>(), Arg.Any<short>())
                .ThrowsAsync(new Exception("Export failed"));
            await Assert.ThrowsAsync<Exception>(() => _controller.ExportToExcel("PP001", 2024));
        }

        #endregion
    }
}