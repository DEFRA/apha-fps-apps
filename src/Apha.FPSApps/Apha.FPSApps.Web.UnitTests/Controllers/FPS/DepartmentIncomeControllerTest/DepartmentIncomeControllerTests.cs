using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.DepartmentIncomeControllerTest
{
    public class DepartmentIncomeControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IDepartmentIncomeService _departmentIncomeService;
        private readonly IProjectService _projectService;
        private readonly IMonthService _monthService;
        private readonly DepartmentIncomeController _controller;

        private const string TestProject = "AH0033";

        public DepartmentIncomeControllerTests()
        {
            _mapper                  = Substitute.For<IMapper>();
            _departmentIncomeService = Substitute.For<IDepartmentIncomeService>();
            _projectService          = Substitute.For<IProjectService>();
            _monthService            = Substitute.For<IMonthService>();
            _controller              = new DepartmentIncomeController(_mapper, _departmentIncomeService, _projectService, _monthService);
        }

        // ── JSON helper ─────────────────────────────────────────────────────────

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        // ── Index ────────────────────────────────────────────────────────────────

        #region Index

        [Fact]
        public async Task Index_ServiceReturnsProjectsAndPeriods_ReturnsViewWithPopulatedViewModel()
        {
            // Arrange
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "AH0033", ProjectTitle = "Project A" },
                new() { ParentProject = "AH0034", ProjectTitle = "Project B" },
            };
            var periods = new List<PeriodLookupDto>
            {
                new() { AccntsPeriod = 1, MonthName = "April",   MonthNumber = 4 },
                new() { AccntsPeriod = 2, MonthName = "May",     MonthNumber = 5 },
            };

            _projectService.GetAllProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects));
            _departmentIncomeService.GetPeriodsAsync()
                .Returns(ApiResponseDto<List<PeriodLookupDto>>.SuccessResponse(periods));
            _monthService.GetAllMonthsAsync()
                .Returns(ApiResponseDto<List<Apha.FPSApps.Application.Dtos.PACT.MonthDto>>.SuccessResponse(new List<Apha.FPSApps.Application.Dtos.PACT.MonthDto>()));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel  = Assert.IsType<DepartmentIncomeViewModel>(viewResult.Model);
            Assert.Equal(2, viewModel.ProjectList.Count);
            Assert.Equal(2, viewModel.PeriodList.Count);
            Assert.NotNull(viewModel.SnapshotGrid);
            Assert.Equal("departmentIncomeSnapshotGrid", viewModel.SnapshotGrid.GridId);
        }

        [Fact]
        public async Task Index_ProjectServiceReturnsFailure_ReturnsViewWithEmptyProjectList()
        {
            // Arrange
            _projectService.GetAllProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "API down", Code = "ERROR" } },
                    new ApiMetaDto()));
            _departmentIncomeService.GetPeriodsAsync()
                .Returns(ApiResponseDto<List<PeriodLookupDto>>.SuccessResponse(new List<PeriodLookupDto>()));
            _monthService.GetAllMonthsAsync()
                .Returns(ApiResponseDto<List<Apha.FPSApps.Application.Dtos.PACT.MonthDto>>.SuccessResponse(new List<Apha.FPSApps.Application.Dtos.PACT.MonthDto>()));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel  = Assert.IsType<DepartmentIncomeViewModel>(viewResult.Model);
            Assert.Empty(viewModel.ProjectList);
        }

        [Fact]
        public async Task Index_PeriodsServiceReturnsFailure_ReturnsViewWithEmptyPeriodList()
        {
            // Arrange
            _projectService.GetAllProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>()));
            _departmentIncomeService.GetPeriodsAsync()
                .Returns(ApiResponseDto<List<PeriodLookupDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Period API down", Code = "ERROR" } },
                    new ApiMetaDto()));
            _monthService.GetAllMonthsAsync()
                .Returns(ApiResponseDto<List<Apha.FPSApps.Application.Dtos.PACT.MonthDto>>.SuccessResponse(new List<Apha.FPSApps.Application.Dtos.PACT.MonthDto>()));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel  = Assert.IsType<DepartmentIncomeViewModel>(viewResult.Model);
            Assert.Empty(viewModel.PeriodList);
        }

        [Fact]
        public async Task Index_SnapshotGridConfig_HasCorrectProperties()
        {
            // Arrange
            _projectService.GetAllProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>()));
            _departmentIncomeService.GetPeriodsAsync()
                .Returns(ApiResponseDto<List<PeriodLookupDto>>.SuccessResponse(new List<PeriodLookupDto>()));
            _monthService.GetAllMonthsAsync()
                .Returns(ApiResponseDto<List<Apha.FPSApps.Application.Dtos.PACT.MonthDto>>.SuccessResponse(new List<Apha.FPSApps.Application.Dtos.PACT.MonthDto>()));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel  = Assert.IsType<DepartmentIncomeViewModel>(viewResult.Model);
            var grid       = viewModel.SnapshotGrid;

            Assert.Equal("departmentIncomeSnapshotGrid",                        grid.GridId);
            Assert.Equal("Snapshot data",                                       grid.Title);
            Assert.False(grid.ShowCheckboxColumn);
            Assert.True(grid.ShowPagination);
            Assert.False(grid.AllowAdd);
            Assert.True(grid.AllowEdit);
            Assert.False(grid.AllowDelete);
            Assert.Equal("PeriodName",                                          grid.KeyProperty);
            Assert.Equal("getDepartmentIncomeSnapshotExtraFilters",             grid.ExtraFilterMethod);
            Assert.Equal("/FPS/DepartmentIncome/LoadSnapshotGrid",              grid.BindGridUrl);
        }

        #endregion

        // ── LoadSnapshotGrid ─────────────────────────────────────────────────────

        #region LoadSnapshotGrid

        [Fact]
        public async Task LoadSnapshotGrid_ValidRequest_ReturnsPartialViewWithEmptySnapshotGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            _departmentIncomeService.GetSnapshotPeriodsAsync()
                .Returns(ApiResponseDto<List<PeriodSnapshotDto>>.SuccessResponse(new List<PeriodSnapshotDto>()));
            _mapper.Map<List<DepartmentIncomeSnapshotItem>>(Arg.Any<List<PeriodSnapshotDto>>())
                .Returns(new List<DepartmentIncomeSnapshotItem>());

            // Act
            var result = await _controller.LoadSnapshotGrid(request, TestProject, 1, 12);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
        }

        [Fact]
        public async Task LoadSnapshotGrid_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Page", "Page is required");
            var request = new PaginationFilter<string>();

            // Act
            var result = await _controller.LoadSnapshotGrid(request, null, null, null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element    = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── LoadGrid (unified cross-tab endpoint) ────────────────────────────────

        #region LoadGrid

        [Fact]
        public async Task LoadGrid_QueryTypeTime_ServiceReturnsData_ReturnsPartialViewWithRows()
        {
            // Arrange
            var dtos  = new List<DepartmentIncomeTimeDto> { new() { Project = TestProject, Month = 1, TotalCost = 1000m } };
            var items = new List<DepartmentIncomeTimeItem> { new() { Project = TestProject, Month = 1, TotalCost = 1000m } };
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            _departmentIncomeService.GetTimeIncomeAsync(TestProject, 1, 6)
                .Returns(ApiResponseDto<List<DepartmentIncomeTimeDto>>.SuccessResponse(dtos));
            _mapper.Map<List<DepartmentIncomeTimeItem>>(dtos).Returns(items);

            // Act
            var result = await _controller.LoadGrid(request, "qryDeptIncomeTime", TestProject, 1, 6, "snapshot");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadGrid_SourceCurrent_GridIdIsCurrentGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            _departmentIncomeService.GetTimeIncomeCurrentAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .Returns(ApiResponseDto<List<DepartmentIncomeTimeDto>>.SuccessResponse(new List<DepartmentIncomeTimeDto>()));
            _mapper.Map<List<DepartmentIncomeTimeItem>>(Arg.Any<List<DepartmentIncomeTimeDto>>())
                .Returns(new List<DepartmentIncomeTimeItem>());

            // Act
            var result = await _controller.LoadGrid(request, "qryDeptIncomeTime", null, null, null, "current");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(partial.Model);
            Assert.Equal("departmentIncomeCurrentGrid",           grid.GridId);
            Assert.Equal("getDeptIncomeCurrentExtraFilters",      grid.ExtraFilterMethod);
        }

        [Fact]
        public async Task LoadGrid_SourceSnapshot_GridIdIsSnapshotQueryGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            _departmentIncomeService.GetTimeIncomeAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .Returns(ApiResponseDto<List<DepartmentIncomeTimeDto>>.SuccessResponse(new List<DepartmentIncomeTimeDto>()));
            _mapper.Map<List<DepartmentIncomeTimeItem>>(Arg.Any<List<DepartmentIncomeTimeDto>>())
                .Returns(new List<DepartmentIncomeTimeItem>());

            // Act
            var result = await _controller.LoadGrid(request, "qryDeptIncomeTime", null, null, null, "snapshot");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(partial.Model);
            Assert.Equal("departmentIncomeSnapshotQueryGrid",          grid.GridId);
            Assert.Equal("getDeptIncomeSnapshotQueryExtraFilters",     grid.ExtraFilterMethod);
        }

        [Fact]
        public async Task LoadGrid_QueryTypeTime_ServiceReturnsFailure_ReturnsPartialViewWithEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            _departmentIncomeService.GetTimeIncomeAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .Returns(ApiResponseDto<List<DepartmentIncomeTimeDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } }, new ApiMetaDto()));

            // Act
            var result = await _controller.LoadGrid(request, "qryDeptIncomeTime", TestProject, 1, 6, "snapshot");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadGrid_QueryTypeTest_ServiceReturnsData_ReturnsPartialViewWithRows()
        {
            // Arrange
            var dtos  = new List<DepartmentIncomeTestDto> { new() { Project = TestProject, Month = 1, TotalCost = 500m } };
            var items = new List<DepartmentIncomeTestItem> { new() { Project = TestProject, Month = 1, TotalCost = 500m } };
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            _departmentIncomeService.GetTestSnapshotIncomeAsync(TestProject, 1, 6)
                .Returns(ApiResponseDto<List<DepartmentIncomeTestDto>>.SuccessResponse(dtos));
            _mapper.Map<List<DepartmentIncomeTestItem>>(dtos).Returns(items);

            // Act
            var result = await _controller.LoadGrid(request, "qryDeptIncomeTest", TestProject, 1, 6, "snapshot");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadGrid_QueryTypeTest_ServiceReturnsFailure_ReturnsPartialViewWithEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            _departmentIncomeService.GetTestSnapshotIncomeAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .Returns(ApiResponseDto<List<DepartmentIncomeTestDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } }, new ApiMetaDto()));

            // Act
            var result = await _controller.LoadGrid(request, "qryDeptIncomeTest", TestProject, 1, 6, "snapshot");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadGrid_QueryTypeTest_SourceCurrent_ServiceReturnsData_ReturnsPartialViewWithRows()
        {
            // Arrange
            var dtos  = new List<DepartmentIncomeTestDto> { new() { Project = TestProject, Month = 1, TotalCost = 500m } };
            var items = new List<DepartmentIncomeTestItem> { new() { Project = TestProject, Month = 1, TotalCost = 500m } };
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            _departmentIncomeService.GetTestIncomeCurrentAsync(TestProject, 1, 6)
                .Returns(ApiResponseDto<List<DepartmentIncomeTestDto>>.SuccessResponse(dtos));
            _mapper.Map<List<DepartmentIncomeTestItem>>(dtos).Returns(items);

            // Act
            var result = await _controller.LoadGrid(request, "qryDeptIncomeTest", TestProject, 1, 6, "current");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadGrid_QueryTypeTest_SourceCurrent_ServiceReturnsFailure_ReturnsPartialViewWithEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            _departmentIncomeService.GetTestIncomeCurrentAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .Returns(ApiResponseDto<List<DepartmentIncomeTestDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } }, new ApiMetaDto()));

            // Act
            var result = await _controller.LoadGrid(request, "qryDeptIncomeTest", TestProject, 1, 6, "current");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadGrid_QueryTypeAnimal_ServiceReturnsData_ReturnsPartialViewWithRows()
        {
            // Arrange
            var dtos  = new List<DepartmentIncomeAnimalDto> { new() { Project = TestProject, Month = 2, TotalCost = 750m } };
            var items = new List<DepartmentIncomeAnimalItem> { new() { Project = TestProject, Month = 2, TotalCost = 750m } };
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            _departmentIncomeService.GetAnimalIncomeAsync(TestProject, 1, 12)
                .Returns(ApiResponseDto<List<DepartmentIncomeAnimalDto>>.SuccessResponse(dtos));
            _mapper.Map<List<DepartmentIncomeAnimalItem>>(dtos).Returns(items);

            // Act
            var result = await _controller.LoadGrid(request, "qryDeptIncomeAnimal", TestProject, 1, 12, "snapshot");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadGrid_QueryTypeAnimal_ServiceReturnsFailure_ReturnsPartialViewWithEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            _departmentIncomeService.GetAnimalIncomeAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .Returns(ApiResponseDto<List<DepartmentIncomeAnimalDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } }, new ApiMetaDto()));

            // Act
            var result = await _controller.LoadGrid(request, "qryDeptIncomeAnimal", TestProject, 1, 12, "snapshot");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadGrid_QueryTypeAdditional_ServiceReturnsData_ReturnsPartialViewWithRows()
        {
            // Arrange
            var dtos  = new List<DepartmentIncomeAdditionalDto> { new() { Project = TestProject, Month = 3, TotalCost = 300m } };
            var items = new List<DepartmentIncomeAdditionalItem> { new() { Project = TestProject, Month = 3, TotalCost = 300m } };
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            _departmentIncomeService.GetAdditionalIncomeAsync(TestProject, 1, 12)
                .Returns(ApiResponseDto<List<DepartmentIncomeAdditionalDto>>.SuccessResponse(dtos));
            _mapper.Map<List<DepartmentIncomeAdditionalItem>>(dtos).Returns(items);

            // Act
            var result = await _controller.LoadGrid(request, "qryDeptIncomeExceptional", TestProject, 1, 12, "snapshot");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadGrid_QueryTypeAdditional_ServiceReturnsFailure_ReturnsPartialViewWithEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            _departmentIncomeService.GetAdditionalIncomeAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .Returns(ApiResponseDto<List<DepartmentIncomeAdditionalDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } }, new ApiMetaDto()));

            // Act
            var result = await _controller.LoadGrid(request, "qryDeptIncomeExceptional", TestProject, 1, 12, "snapshot");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadGrid_QueryTypeTotals_ServiceReturnsData_ReturnsPartialViewWithRows()
        {
            // Arrange
            var dtos  = new List<DepartmentIncomeTotalsDto> { new() { Project = TestProject, TotalCosts = 2500m } };
            var items = new List<DepartmentIncomeTotalsItem> { new() { Project = TestProject, TotalCosts = 2500m } };
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            _departmentIncomeService.GetTotalsAsync(TestProject, 1, 12)
                .Returns(ApiResponseDto<List<DepartmentIncomeTotalsDto>>.SuccessResponse(dtos));
            _mapper.Map<List<DepartmentIncomeTotalsItem>>(dtos).Returns(items);

            // Act
            var result = await _controller.LoadGrid(request, "qryDeptIncomeTotals", TestProject, 1, 12, "snapshot");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadGrid_QueryTypeTotals_ServiceReturnsFailure_ReturnsPartialViewWithEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            _departmentIncomeService.GetTotalsAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .Returns(ApiResponseDto<List<DepartmentIncomeTotalsDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } }, new ApiMetaDto()));

            // Act
            var result = await _controller.LoadGrid(request, "qryDeptIncomeTotals", TestProject, 1, 12, "snapshot");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        #endregion

        // ── Constructor Tests ────────────────────────────────────────────────────

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DepartmentIncomeController(null!, _departmentIncomeService, _projectService, _monthService));
        }

        [Fact]
        public void Constructor_WithNullDepartmentIncomeService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DepartmentIncomeController(_mapper, null!, _projectService, _monthService));
        }

        [Fact]
        public void Constructor_WithNullProjectService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DepartmentIncomeController(_mapper, _departmentIncomeService, null!, _monthService));
        }

        [Fact]
        public void Constructor_WithNullMonthService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DepartmentIncomeController(_mapper, _departmentIncomeService, _projectService, null!));
        }

        #endregion

        // ── EditSnapshotPeriod ────────────────────────────────────────────────────

        #region EditSnapshotPeriod

        [Fact]
        public async Task EditSnapshotPeriod_PeriodExists_ReturnsPartialViewWithModel()
        {
            // Arrange
            const string periodName = "April 2025 Only";
            var snapshotPeriods = new List<PeriodSnapshotDto>
            {
                new() { PeriodName = periodName, EndPeriod = 4, PeriodLocked = false, FinalSummariesRun = true }
            };
            _departmentIncomeService.GetSnapshotPeriodsAsync()
                .Returns(ApiResponseDto<List<PeriodSnapshotDto>>.SuccessResponse(snapshotPeriods));

            var snapshotItem = new DepartmentIncomeSnapshotItem
            {
                PeriodName       = periodName,
                Month            = 4,
                PeriodLocked     = false,
                FinalSummariesRun = true
            };
            _mapper.Map<DepartmentIncomeSnapshotItem>(snapshotPeriods[0]).Returns(snapshotItem);

            // Act
            var result = await _controller.EditSnapshotPeriod(periodName);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Contains("_AddEditDepartmentIncome", partial.ViewName);
            var model = Assert.IsType<DepartmentIncomeSnapshotItem>(partial.Model);
            Assert.Equal(periodName, model.PeriodName);
            await _departmentIncomeService.Received(1).GetSnapshotPeriodsAsync();
        }

        [Fact]
        public async Task EditSnapshotPeriod_PeriodNotFound_ReturnsNotFound()
        {
            // Arrange
            _departmentIncomeService.GetSnapshotPeriodsAsync()
                .Returns(ApiResponseDto<List<PeriodSnapshotDto>>.SuccessResponse(new List<PeriodSnapshotDto>
                {
                    new() { PeriodName = "April - May 2025", EndPeriod = 5 }
                }));

            // Act
            var result = await _controller.EditSnapshotPeriod("NonExistentPeriod");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditSnapshotPeriod_ServiceReturnsNullData_ReturnsNotFound()
        {
            // Arrange
            _departmentIncomeService.GetSnapshotPeriodsAsync()
                .Returns(ApiResponseDto<List<PeriodSnapshotDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } }, new ApiMetaDto()));

            // Act
            var result = await _controller.EditSnapshotPeriod("April 2025 Only");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        // ── UpdateSnapshotPeriod ──────────────────────────────────────────────────

        #region UpdateSnapshotPeriod

        [Fact]
        public async Task UpdateSnapshotPeriod_ValidModel_ServiceSucceeds_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var model = new DepartmentIncomeSnapshotUpdateDto { PeriodName = "April 2025 Only", PeriodLocked = true };
            _departmentIncomeService.UpdatePeriodLockedAsync("April 2025 Only", true)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.UpdateSnapshotPeriod(model);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal("Period locked updated successfully.", element.GetProperty("message").GetString());
            await _departmentIncomeService.Received(1).UpdatePeriodLockedAsync("April 2025 Only", true);
        }

        [Fact]
        public async Task UpdateSnapshotPeriod_ValidModel_ServiceFails_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var model = new DepartmentIncomeSnapshotUpdateDto { PeriodName = "April 2025 Only", PeriodLocked = false };
            _departmentIncomeService.UpdatePeriodLockedAsync("April 2025 Only", false)
                .Returns(ApiResponseDto<bool>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Update failed.", Code = "ERROR" } },
                    new ApiMetaDto()));

            // Act
            var result = await _controller.UpdateSnapshotPeriod(model);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task UpdateSnapshotPeriod_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("PeriodName", "PeriodName is required");
            var model = new DepartmentIncomeSnapshotUpdateDto();

            // Act
            var result = await _controller.UpdateSnapshotPeriod(model);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
            await _departmentIncomeService.DidNotReceive()
                .UpdatePeriodLockedAsync(Arg.Any<string>(), Arg.Any<bool>());
        }

        [Fact]
        public async Task UpdateSnapshotPeriod_PeriodNameWithSlash_PassedToServiceUnchanged()
        {
            // Arrange
            const string slashPeriod = "April - August 2025/25";
            var model = new DepartmentIncomeSnapshotUpdateDto { PeriodName = slashPeriod, PeriodLocked = true };
            _departmentIncomeService.UpdatePeriodLockedAsync(slashPeriod, true)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.UpdateSnapshotPeriod(model);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
            await _departmentIncomeService.Received(1).UpdatePeriodLockedAsync(slashPeriod, true);
        }

        #endregion
    }
}
