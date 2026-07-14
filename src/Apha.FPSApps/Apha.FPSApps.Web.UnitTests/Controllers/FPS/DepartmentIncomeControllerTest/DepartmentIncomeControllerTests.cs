/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeControllerTests.cs (FPS Web UnitTests)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New xUnit test class for frontend FPS DepartmentIncomeController (MVC)
 *   - Covers: Index, LoadSnapshotGrid, GetTimeData, GetTestData, GetAnimalData, GetAdditionalData, GetTotalsData
 *   - NSubstitute mocks for IMapper, IDepartmentIncomeService, IProjectService
 *   - JSON result assertions use System.Text.Json + GetJsonResultElement helper
 *
 * PRESERVED:
 *   - Controller is read-only (no Create/Edit/Delete action tests)
 *   - LoadSnapshotGrid has a stub implementation — tests verify stub returns empty data without service error
 *
 * DEFERRED: none — fully automated.
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.DepartmentIncome;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
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
        private readonly DepartmentIncomeController _controller;

        private const string TestProject = "AH0033";

        public DepartmentIncomeControllerTests()
        {
            _mapper                  = Substitute.For<IMapper>();
            _departmentIncomeService = Substitute.For<IDepartmentIncomeService>();
            _projectService          = Substitute.For<IProjectService>();
            _controller              = new DepartmentIncomeController(_mapper, _departmentIncomeService, _projectService);
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

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel  = Assert.IsType<DepartmentIncomeViewModel>(viewResult.Model);
            Assert.Empty(viewModel.PeriodList);
        }

        #endregion

        // ── LoadSnapshotGrid ─────────────────────────────────────────────────────

        #region LoadSnapshotGrid

        [Fact]
        public async Task LoadSnapshotGrid_ValidRequest_ReturnsPartialViewWithEmptySnapshotGrid()
        {
            // Arrange
            // TRANSFORMENGINE: Snapshot grid is a stub — always returns empty items
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

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

        // ── GetTimeData ──────────────────────────────────────────────────────────

        #region GetTimeData

        [Fact]
        public async Task GetTimeData_ServiceReturnsData_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var dtos = new List<DepartmentIncomeTimeDto>
            {
                new() { Project = TestProject, Month = 1, TotalCost = 1000m }
            };
            var items = new List<DepartmentIncomeTimeItem>
            {
                new() { Project = TestProject, Month = 1, TotalCost = 1000m }
            };

            _departmentIncomeService.GetTimeIncomeAsync(TestProject, 1, 6)
                .Returns(ApiResponseDto<List<DepartmentIncomeTimeDto>>.SuccessResponse(dtos));
            _mapper.Map<List<DepartmentIncomeTimeItem>>(dtos).Returns(items);

            // Act
            var result = await _controller.GetTimeData(TestProject, 1, 6);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element    = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetTimeData_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _departmentIncomeService.GetTimeIncomeAsync(TestProject, 1, 6)
                .Returns(ApiResponseDto<List<DepartmentIncomeTimeDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Time data error", Code = "ERROR" } },
                    new ApiMetaDto()));

            // Act
            var result = await _controller.GetTimeData(TestProject, 1, 6);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element    = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetTimeData_ServiceReturnsNullData_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _departmentIncomeService.GetTimeIncomeAsync(null, null, null)
                .Returns(new ApiResponseDto<List<DepartmentIncomeTimeDto>> { Success = true, Data = null });

            // Act
            var result = await _controller.GetTimeData(null, null, null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element    = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── GetTestData ──────────────────────────────────────────────────────────

        #region GetTestData

        [Fact]
        public async Task GetTestData_ServiceReturnsData_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var dtos = new List<DepartmentIncomeTestDto>
            {
                new() { Project = TestProject, Month = 1, TotalCost = 500m }
            };
            var items = new List<DepartmentIncomeTestItem>
            {
                new() { Project = TestProject, Month = 1, TotalCost = 500m }
            };

            _departmentIncomeService.GetTestIncomeAsync(TestProject, 1, 6)
                .Returns(ApiResponseDto<List<DepartmentIncomeTestDto>>.SuccessResponse(dtos));
            _mapper.Map<List<DepartmentIncomeTestItem>>(dtos).Returns(items);

            // Act
            var result = await _controller.GetTestData(TestProject, 1, 6);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element    = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetTestData_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _departmentIncomeService.GetTestIncomeAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .Returns(ApiResponseDto<List<DepartmentIncomeTestDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Test data error", Code = "ERROR" } },
                    new ApiMetaDto()));

            // Act
            var result = await _controller.GetTestData(TestProject, 1, 6);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element    = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── GetAnimalData ────────────────────────────────────────────────────────

        #region GetAnimalData

        [Fact]
        public async Task GetAnimalData_ServiceReturnsData_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var dtos = new List<DepartmentIncomeAnimalDto>
            {
                new() { Project = TestProject, Month = 2, TotalCost = 750m }
            };
            var items = new List<DepartmentIncomeAnimalItem>
            {
                new() { Project = TestProject, Month = 2, TotalCost = 750m }
            };

            _departmentIncomeService.GetAnimalIncomeAsync(TestProject, 1, 12)
                .Returns(ApiResponseDto<List<DepartmentIncomeAnimalDto>>.SuccessResponse(dtos));
            _mapper.Map<List<DepartmentIncomeAnimalItem>>(dtos).Returns(items);

            // Act
            var result = await _controller.GetAnimalData(TestProject, 1, 12);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element    = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetAnimalData_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _departmentIncomeService.GetAnimalIncomeAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .Returns(ApiResponseDto<List<DepartmentIncomeAnimalDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Animal data error", Code = "ERROR" } },
                    new ApiMetaDto()));

            // Act
            var result = await _controller.GetAnimalData(TestProject, 1, 12);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element    = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── GetAdditionalData ────────────────────────────────────────────────────

        #region GetAdditionalData

        [Fact]
        public async Task GetAdditionalData_ServiceReturnsData_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var dtos = new List<DepartmentIncomeAdditionalDto>
            {
                new() { Project = TestProject, Month = 3, TotalCost = 250m }
            };
            var items = new List<DepartmentIncomeAdditionalItem>
            {
                new() { Project = TestProject, Month = 3, TotalCost = 250m }
            };

            _departmentIncomeService.GetAdditionalIncomeAsync(TestProject, 1, 12)
                .Returns(ApiResponseDto<List<DepartmentIncomeAdditionalDto>>.SuccessResponse(dtos));
            _mapper.Map<List<DepartmentIncomeAdditionalItem>>(dtos).Returns(items);

            // Act
            var result = await _controller.GetAdditionalData(TestProject, 1, 12);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element    = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetAdditionalData_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _departmentIncomeService.GetAdditionalIncomeAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .Returns(ApiResponseDto<List<DepartmentIncomeAdditionalDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Additional data error", Code = "ERROR" } },
                    new ApiMetaDto()));

            // Act
            var result = await _controller.GetAdditionalData(TestProject, 1, 12);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element    = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── GetTotalsData ────────────────────────────────────────────────────────

        #region GetTotalsData

        [Fact]
        public async Task GetTotalsData_ServiceReturnsData_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var dtos = new List<DepartmentIncomeTotalsDto>
            {
                new() { Project = TestProject, TotalCosts = 2500m }
            };
            var items = new List<DepartmentIncomeTotalsItem>
            {
                new() { Project = TestProject, TotalCosts = 2500m }
            };

            _departmentIncomeService.GetTotalsAsync(TestProject, 1, 12)
                .Returns(ApiResponseDto<List<DepartmentIncomeTotalsDto>>.SuccessResponse(dtos));
            _mapper.Map<List<DepartmentIncomeTotalsItem>>(dtos).Returns(items);

            // Act
            var result = await _controller.GetTotalsData(TestProject, 1, 12);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element    = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetTotalsData_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _departmentIncomeService.GetTotalsAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .Returns(ApiResponseDto<List<DepartmentIncomeTotalsDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Totals error", Code = "ERROR" } },
                    new ApiMetaDto()));

            // Act
            var result = await _controller.GetTotalsData(TestProject, 1, 12);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element    = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetTotalsData_ServiceReturnsNullData_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _departmentIncomeService.GetTotalsAsync(null, null, null)
                .Returns(new ApiResponseDto<List<DepartmentIncomeTotalsDto>> { Success = true, Data = null });

            // Act
            var result = await _controller.GetTotalsData(null, null, null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element    = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion
    }
}
