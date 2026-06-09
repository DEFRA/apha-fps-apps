using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Controllers;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.ProfitCenterCostSummaryControllerTest
{
    public class ProfitCenterCostSummaryControllerTests
    {
        private const string DefaultFilterJson = "{}";
        private const string TestProfitCentre1 = "PC01";
        private const string TestProfitCentre2 = "PC02";
        private const short TestMonthNumber = 3;
        private const string ExpectedGridId = "profitCenterCostGrid";

        private readonly IMapper _mapper;
        private readonly IProfitCentreService _profitCentreService;
        private readonly ICalenderMonthService _calenderMonthService;
        private readonly ProfitCenterCostSummaryController _controller;

        public ProfitCenterCostSummaryControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _profitCentreService = Substitute.For<IProfitCentreService>();
            _calenderMonthService = Substitute.For<ICalenderMonthService>();
            _controller = new ProfitCenterCostSummaryController(_mapper, _profitCentreService, _calenderMonthService);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private void SetupDefaultMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ProfitCenterCostItem>>(Arg.Any<List<ProfitCentreCostDto>>())
                .Returns([]);
        }

        private void SetupPeriodMonthsMapper(List<CalenderMonthDto> periods)
        {
            _mapper.Map<List<PeriodMonth>>(Arg.Any<List<CalenderMonthDto>>())
                .Returns(periods.Select(p => new PeriodMonth
                {
                    Period = p.MonthName,
                    MonthNumber = p.MonthNumber.ToString()
                }).ToList());
        }

        private void SetupDefaultCalenderMonthsResponse()
        {
            var periods = new List<CalenderMonthDto>
            {
                new() { MonthName = "Period 1", MonthNumber = 1 },
                new() { MonthName = "Period 2", MonthNumber = 2 }
            };
            _calenderMonthService.GetCalenderMonthsAsync()
                .Returns(ApiResponseDto<List<CalenderMonthDto>>.SuccessResponse(periods));
            SetupPeriodMonthsMapper(periods);
        }

        private void SetupDefaultCostSummaryResponse(short? monthNumber = null)
        {
            var costData = new List<ProfitCentreCostDto>
            {
                new() { ProfitCentre = TestProfitCentre1, Cost = 1000m },
                new() { ProfitCentre = TestProfitCentre2, Cost = 2000m }
            };
            var paginatedResult = new PaginatedResult<ProfitCentreCostDto>(costData, 2, 1, 10);
            _profitCentreService.GetPagedProfitCenterCostSummaryAsync(Arg.Any<QueryParameters<string>>(), monthNumber)
                .Returns(ApiResponseDto<PaginatedResult<ProfitCentreCostDto>>.SuccessResponse(paginatedResult));

            _mapper.Map<List<ProfitCenterCostItem>>(Arg.Any<List<ProfitCentreCostDto>>())
                .Returns(costData.Select(c => new ProfitCenterCostItem
                {
                    ProfitCentre = c.ProfitCentre,
                    Cost = c.Cost
                }).ToList());
        }

        // ── Index ──────────────────────────────────────────────────────────────

        #region Index

        [Fact]
        public async Task Index_WithoutMonthNumber_ReturnsViewResultWithViewModel()
        {
            // Arrange
            SetupDefaultCalenderMonthsResponse();
            SetupDefaultCostSummaryResponse();
            SetupDefaultMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProfitCenterCostSummaryViewModel>(viewResult.Model);
            Assert.NotNull(model);
            Assert.NotNull(model.CostGrid);
            Assert.Equal(ExpectedGridId, model.CostGrid.GridId);
        }

        [Fact]
        public async Task Index_WithMonthNumber_PassesMonthNumberToService()
        {
            // Arrange
            SetupDefaultCalenderMonthsResponse();
            SetupDefaultCostSummaryResponse(TestMonthNumber);
            SetupDefaultMapper();

            // Act
            var result = await _controller.Index(TestMonthNumber);

            // Assert
            await _profitCentreService.Received(1).GetPagedProfitCenterCostSummaryAsync(
                Arg.Any<QueryParameters<string>>(),
                TestMonthNumber);
        }

        [Fact]
        public async Task Index_WithMonthNumber_SetsSelectedMonthNumberInViewModel()
        {
            // Arrange
            SetupDefaultCalenderMonthsResponse();
            SetupDefaultCostSummaryResponse(TestMonthNumber);
            SetupDefaultMapper();

            // Act
            var result = await _controller.Index(TestMonthNumber);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProfitCenterCostSummaryViewModel>(viewResult.Model);
            Assert.Equal(TestMonthNumber, model.SelectedMonthNumber);
        }

        [Fact]
        public async Task Index_Always_PopulatesPeriodMonths()
        {
            // Arrange
            var periods = new List<CalenderMonthDto>
            {
                new() { MonthName = "Period 1", MonthNumber = 1 },
                new() { MonthName = "Period 2", MonthNumber = 2 },
                new() { MonthName = "Period 3", MonthNumber = 3 }
            };
            _calenderMonthService.GetCalenderMonthsAsync()
                .Returns(ApiResponseDto<List<CalenderMonthDto>>.SuccessResponse(periods));
            SetupPeriodMonthsMapper(periods);
            SetupDefaultCostSummaryResponse();
            SetupDefaultMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProfitCenterCostSummaryViewModel>(viewResult.Model);
            Assert.Equal(3, model.PeriodMonths.Count);
        }

        [Fact]
        public async Task Index_WhenCalenderMonthServiceReturnsEmpty_ReturnsEmptyPeriodMonths()
        {
            // Arrange
            _calenderMonthService.GetCalenderMonthsAsync()
                .Returns(ApiResponseDto<List<CalenderMonthDto>>.SuccessResponse([]));
            SetupDefaultCostSummaryResponse();
            SetupDefaultMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProfitCenterCostSummaryViewModel>(viewResult.Model);
            Assert.Empty(model.PeriodMonths);
        }

        [Fact]
        public async Task Index_WhenCalenderMonthServiceFails_ReturnsEmptyPeriodMonths()
        {
            // Arrange
            _calenderMonthService.GetCalenderMonthsAsync()
                .Returns(ApiResponseDto<List<CalenderMonthDto>>.FailureResponse(null!, new ApiMetaDto()));
            SetupDefaultCostSummaryResponse();
            SetupDefaultMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProfitCenterCostSummaryViewModel>(viewResult.Model);
            Assert.Empty(model.PeriodMonths);
        }

        [Fact]
        public async Task Index_WhenCalenderMonthServiceReturnsNull_ReturnsEmptyPeriodMonths()
        {
            // Arrange
            _calenderMonthService.GetCalenderMonthsAsync()
                .Returns(ApiResponseDto<List<CalenderMonthDto>>.SuccessResponse(null!));
            SetupDefaultCostSummaryResponse();
            SetupDefaultMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProfitCenterCostSummaryViewModel>(viewResult.Model);
            Assert.Empty(model.PeriodMonths);
        }

        [Fact]
        public async Task Index_OrdersPeriodMonthsByMonthNumber()
        {
            // Arrange
            var periods = new List<CalenderMonthDto>
            {
                new() { MonthName = "Period 3", MonthNumber = 3 },
                new() { MonthName = "Period 1", MonthNumber = 1 },
                new() { MonthName = "Period 2", MonthNumber = 2 }
            };
            _calenderMonthService.GetCalenderMonthsAsync()
                .Returns(ApiResponseDto<List<CalenderMonthDto>>.SuccessResponse(periods));

            // Setup mapper to preserve order
            _mapper.Map<List<PeriodMonth>>(Arg.Any<List<CalenderMonthDto>>())
                .Returns(callInfo =>
                {
                    var input = callInfo.Arg<List<CalenderMonthDto>>();
                    return input.Select(p => new PeriodMonth
                    {
                        Period = p.MonthName,
                        MonthNumber = p.MonthNumber.ToString()
                    }).ToList();
                });

            SetupDefaultCostSummaryResponse();
            SetupDefaultMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProfitCenterCostSummaryViewModel>(viewResult.Model);
            Assert.Equal("Period 1", model.PeriodMonths[0].Period);
            Assert.Equal("Period 2", model.PeriodMonths[1].Period);
            Assert.Equal("Period 3", model.PeriodMonths[2].Period);
        }

        [Fact]
        public async Task Index_PopulatesCostGridWithData()
        {
            // Arrange
            SetupDefaultCalenderMonthsResponse();
            SetupDefaultCostSummaryResponse();
            SetupDefaultMapper();

            var costData = new List<ProfitCentreCostDto>
            {
                new() { ProfitCentre = TestProfitCentre1, Cost = 1000m },
                new() { ProfitCentre = TestProfitCentre2, Cost = 2000m }
            };
            _mapper.Map<List<ProfitCenterCostItem>>(Arg.Any<List<ProfitCentreCostDto>>())
                .Returns(costData.Select(c => new ProfitCenterCostItem
                {
                    ProfitCentre = c.ProfitCentre,
                    Cost = c.Cost
                }).ToList());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProfitCenterCostSummaryViewModel>(viewResult.Model);
            Assert.NotNull(model.CostGrid);
            Assert.Equal(2, model.CostGrid.Data.Count);
        }

        [Fact]
        public async Task Index_ConfiguresGridProperties()
        {
            // Arrange
            SetupDefaultCalenderMonthsResponse();
            SetupDefaultCostSummaryResponse();
            SetupDefaultMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProfitCenterCostSummaryViewModel>(viewResult.Model);
            var grid = model.CostGrid;

            Assert.NotNull(grid);
            Assert.Equal(ExpectedGridId, grid.GridId);
            Assert.Equal("ProfitCentre", grid.KeyProperty);
            Assert.False(grid.AllowAdd);
            Assert.False(grid.AllowEdit);
            Assert.False(grid.AllowDelete);
            Assert.False(grid.AllowExport);
            Assert.False(grid.AllowRowSelection);
            Assert.True(grid.ShowPagination);
            Assert.Equal("getProfitCenterCostGridExtraFilters", grid.ExtraFilterMethod);
        }

        #endregion

        // ── LoadProfitCenterCostGrid ───────────────────────────────────────────

        #region LoadProfitCenterCostGrid

        [Fact]
        public async Task LoadProfitCenterCostGrid_WithValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = DefaultFilterJson };
            SetupDefaultCostSummaryResponse();
            SetupDefaultMapper();

            // Act
            var result = await _controller.LoadProfitCenterCostGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
        }

        [Fact]
        public async Task LoadProfitCenterCostGrid_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = DefaultFilterJson };
            _controller.ModelState.AddModelError("TestError", "Test error message");

            // Act
            var result = await _controller.LoadProfitCenterCostGrid(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
        }

        [Fact]
        public async Task LoadProfitCenterCostGrid_WithMonthNumber_PassesMonthNumberToService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = DefaultFilterJson };
            SetupDefaultCostSummaryResponse(TestMonthNumber);
            SetupDefaultMapper();

            // Act
            await _controller.LoadProfitCenterCostGrid(request, TestMonthNumber);

            // Assert
            await _profitCentreService.Received(1).GetPagedProfitCenterCostSummaryAsync(
                Arg.Any<QueryParameters<string>>(),
                TestMonthNumber);
        }

        [Fact]
        public async Task LoadProfitCenterCostGrid_WithoutMonthNumber_PassesNullToService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = DefaultFilterJson };
            SetupDefaultCostSummaryResponse(null);
            SetupDefaultMapper();

            // Act
            await _controller.LoadProfitCenterCostGrid(request, null);

            // Assert
            await _profitCentreService.Received(1).GetPagedProfitCenterCostSummaryAsync(
                Arg.Any<QueryParameters<string>>(),
                null);
        }

        [Fact]
        public async Task LoadProfitCenterCostGrid_MapsFilterToQueryParameters()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Filter = DefaultFilterJson,
                SortBy = "ProfitCentre",
                Descending = true
            };
            SetupDefaultCostSummaryResponse();
            SetupDefaultMapper();

            // Act
            await _controller.LoadProfitCenterCostGrid(request);

            // Assert
            _mapper.Received(1).Map<QueryParameters<string>>(Arg.Is<PaginationFilter<string>>(
                f => f.SortBy == "ProfitCentre" && f.Descending));
        }

        [Fact]
        public async Task LoadProfitCenterCostGrid_ReturnsGridConfigWithData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = DefaultFilterJson };
            var costData = new List<ProfitCentreCostDto>
            {
                new() { ProfitCentre = TestProfitCentre1, Cost = 1500m }
            };
            var paginatedResult = new PaginatedResult<ProfitCentreCostDto>(costData, 1, 1, 10);
            _profitCentreService.GetPagedProfitCenterCostSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<PaginatedResult<ProfitCentreCostDto>>.SuccessResponse(paginatedResult));

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ProfitCenterCostItem>>(Arg.Any<List<ProfitCentreCostDto>>())
                .Returns([new ProfitCenterCostItem { ProfitCentre = TestProfitCentre1, Cost = 1500m }]);

            // Act
            var result = await _controller.LoadProfitCenterCostGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProfitCenterCostItem>>(partialViewResult.Model);
            Assert.Single(gridConfig.Data);
        }

        [Fact]
        public async Task LoadProfitCenterCostGrid_ConfiguresPaginationFromResponse()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Filter = DefaultFilterJson,
                SortBy = "Cost",
                Descending = true
            };
            var costData = new List<ProfitCentreCostDto>
            {
                new() { ProfitCentre = TestProfitCentre1, Cost = 1000m }
            };
            var paginatedResult = new PaginatedResult<ProfitCentreCostDto>(costData, 15, 2, 5);
            _profitCentreService.GetPagedProfitCenterCostSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<PaginatedResult<ProfitCentreCostDto>>.SuccessResponse(paginatedResult));

            SetupDefaultMapper();
            _mapper.Map<List<ProfitCenterCostItem>>(Arg.Any<List<ProfitCentreCostDto>>())
                .Returns([new ProfitCenterCostItem { ProfitCentre = TestProfitCentre1, Cost = 1000m }]);

            // Act
            var result = await _controller.LoadProfitCenterCostGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProfitCenterCostItem>>(partialViewResult.Model);
            Assert.NotNull(gridConfig.Pagination);
            Assert.Equal(2, gridConfig.Pagination.PageNumber);
            Assert.Equal(5, gridConfig.Pagination.PageSize);
            Assert.Equal(15, gridConfig.Pagination.TotalRecords);
            Assert.Equal("Cost", gridConfig.Pagination.SortColumn);
            Assert.True(gridConfig.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadProfitCenterCostGrid_WhenResponseDataIsNull_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = DefaultFilterJson };
            _profitCentreService.GetPagedProfitCenterCostSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<PaginatedResult<ProfitCentreCostDto>>.SuccessResponse(null!));
            SetupDefaultMapper();

            // Act
            var result = await _controller.LoadProfitCenterCostGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProfitCenterCostItem>>(partialViewResult.Model);
            Assert.Empty(gridConfig.Data);
        }

        [Fact]
        public async Task LoadProfitCenterCostGrid_WithMonthNumber_UpdatesBindGridUrl()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = DefaultFilterJson };
            SetupDefaultCostSummaryResponse(TestMonthNumber);
            SetupDefaultMapper();

            // Act
            var result = await _controller.LoadProfitCenterCostGrid(request, TestMonthNumber);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProfitCenterCostItem>>(partialViewResult.Model);
            Assert.Contains($"monthNumber={TestMonthNumber}", gridConfig.BindGridUrl);
        }

        [Fact]
        public async Task LoadProfitCenterCostGrid_WithoutMonthNumber_UsesDefaultBindGridUrl()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = DefaultFilterJson };
            SetupDefaultCostSummaryResponse(null);
            SetupDefaultMapper();

            // Act
            var result = await _controller.LoadProfitCenterCostGrid(request, null);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProfitCenterCostItem>>(partialViewResult.Model);
            Assert.DoesNotContain("monthNumber=", gridConfig.BindGridUrl);
        }

        [Fact]
        public async Task LoadProfitCenterCostGrid_WhenPaginatedResultIsNull_CreatesPaginationWithSortOnly()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Filter = DefaultFilterJson,
                SortBy = "ProfitCentre",
                Descending = false
            };
            _profitCentreService.GetPagedProfitCenterCostSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<PaginatedResult<ProfitCentreCostDto>>.SuccessResponse(null!));
            SetupDefaultMapper();

            // Act
            var result = await _controller.LoadProfitCenterCostGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProfitCenterCostItem>>(partialViewResult.Model);
            Assert.NotNull(gridConfig.Pagination);
            Assert.Equal("ProfitCentre", gridConfig.Pagination.SortColumn);
            Assert.False(gridConfig.Pagination.SortDirection);
        }

        #endregion
    }
}
