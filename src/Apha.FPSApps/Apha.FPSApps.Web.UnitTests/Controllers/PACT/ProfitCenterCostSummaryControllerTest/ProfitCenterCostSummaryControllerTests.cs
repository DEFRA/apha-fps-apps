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
        private readonly IReleaseSummaryService _releaseSummaryService;
        private readonly ProfitCenterCostSummaryController _controller;

        public ProfitCenterCostSummaryControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _profitCentreService = Substitute.For<IProfitCentreService>();
            _releaseSummaryService = Substitute.For<IReleaseSummaryService>();
            _controller = new ProfitCenterCostSummaryController(_mapper, _profitCentreService, _releaseSummaryService);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private void SetupDefaultMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ProfitCenterCostItem>>(Arg.Any<List<ProfitCentreCostDto>>())
                .Returns([]);
            _mapper.Map<List<PeriodMonth>>(Arg.Any<List<ReleasePeriodDto>>())
                .Returns(callInfo =>
                {
                    var periods = callInfo.Arg<List<ReleasePeriodDto>>();
                    return periods.Select(p => new PeriodMonth
                    {
                        PeriodName = p.PeriodName,
                        EndPeriod = p.EndPeriod.ToString()
                    }).ToList();
                });
        }

        private void SetupDefaultReleaseSummaryResponse()
        {
            var periods = new List<ReleasePeriodDto>
            {
                new() { PeriodName = "Period 1", StartPeriod = 1, EndPeriod = 1 },
                new() { PeriodName = "Period 2", StartPeriod = 2, EndPeriod = 2 }
            };

            _releaseSummaryService.GetReleasePeriodsAsync()
                .Returns(ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>.SuccessResponse(periods.AsReadOnly()));
        }

        private void SetupDefaultCostSummaryResponse(double? monthNumber = null)
        {
            var costData = new List<ProfitCentreCostDto>
            {
                new() { ProfitCentre = TestProfitCentre1, Cost = 1000m },
                new() { ProfitCentre = TestProfitCentre2, Cost = 2000m }
            };
            _profitCentreService.GetPagedProfitCenterCostSummaryAsync(Arg.Any<QueryParameters<string>>(), monthNumber ?? 0.0)
                .Returns(ApiResponseDto<List<ProfitCentreCostDto>>.SuccessResponse(costData));

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
        public async Task Index_ReturnsViewResultWithNullGrid()
        {
            // Arrange
            SetupDefaultReleaseSummaryResponse();
            SetupDefaultMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProfitCenterCostSummaryViewModel>(viewResult.Model);
            Assert.NotNull(model);
            Assert.Null(model.CostGrid);
            Assert.Null(model.SelectedMonthNumber);
        }

        [Fact]
        public async Task Index_Always_PopulatesPeriodMonths()
        {
            // Arrange
            var periods = new List<ReleasePeriodDto>
            {
                new() { PeriodName = "Period 1", StartPeriod = 1, EndPeriod = 1 },
                new() { PeriodName = "Period 2", StartPeriod = 2, EndPeriod = 2 },
                new() { PeriodName = "Period 3", StartPeriod = 3, EndPeriod = 3 }
            };
            _releaseSummaryService.GetReleasePeriodsAsync()
                .Returns(ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>.SuccessResponse(periods.AsReadOnly()));
            SetupDefaultMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProfitCenterCostSummaryViewModel>(viewResult.Model);
            Assert.Equal(3, model.PeriodMonths.Count);
        }

        [Fact]
        public async Task Index_WhenReleaseSummaryServiceReturnsEmpty_ReturnsEmptyPeriodMonths()
        {
            // Arrange
            _releaseSummaryService.GetReleasePeriodsAsync()
                .Returns(ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>.SuccessResponse(new List<ReleasePeriodDto>().AsReadOnly()));
            SetupDefaultMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProfitCenterCostSummaryViewModel>(viewResult.Model);
            Assert.Empty(model.PeriodMonths);
        }

        [Fact]
        public async Task Index_WhenReleaseSummaryServiceFails_ReturnsEmptyPeriodMonths()
        {
            // Arrange
            _releaseSummaryService.GetReleasePeriodsAsync()
                .Returns(ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>.FailureResponse(null!, new ApiMetaDto()));
            SetupDefaultMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProfitCenterCostSummaryViewModel>(viewResult.Model);
            Assert.Empty(model.PeriodMonths);
        }

        [Fact]
        public async Task Index_WhenReleaseSummaryServiceReturnsNull_ReturnsEmptyPeriodMonths()
        {
            // Arrange
            _releaseSummaryService.GetReleasePeriodsAsync()
                .Returns(ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>.SuccessResponse(null!));
            SetupDefaultMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProfitCenterCostSummaryViewModel>(viewResult.Model);
            Assert.Empty(model.PeriodMonths);
        }

        [Fact]
        public async Task Index_OrdersPeriodMonthsByEndPeriod()
        {
            // Arrange
            var periods = new List<ReleasePeriodDto>
            {
                new() { PeriodName = "Period 3", StartPeriod = 3, EndPeriod = 3 },
                new() { PeriodName = "Period 1", StartPeriod = 1, EndPeriod = 1 },
                new() { PeriodName = "Period 2", StartPeriod = 2, EndPeriod = 2 }
            };
            _releaseSummaryService.GetReleasePeriodsAsync()
                .Returns(ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>.SuccessResponse(periods.AsReadOnly()));

            SetupDefaultMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProfitCenterCostSummaryViewModel>(viewResult.Model);
            Assert.Equal("Period 1", model.PeriodMonths[0].PeriodName);
            Assert.Equal("1", model.PeriodMonths[0].EndPeriod);
            Assert.Equal("Period 2", model.PeriodMonths[1].PeriodName);
            Assert.Equal("2", model.PeriodMonths[1].EndPeriod);
            Assert.Equal("Period 3", model.PeriodMonths[2].PeriodName);
            Assert.Equal("3", model.PeriodMonths[2].EndPeriod);
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
            var result = await _controller.LoadProfitCenterCostGrid(request, 0.0);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
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
        public async Task LoadProfitCenterCostGrid_WithZeroMonthNumber_PassesZeroToService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = DefaultFilterJson };
            SetupDefaultCostSummaryResponse(0.0);
            SetupDefaultMapper();

            // Act
            await _controller.LoadProfitCenterCostGrid(request, 0.0);

            // Assert
            await _profitCentreService.Received(1).GetPagedProfitCenterCostSummaryAsync(
                Arg.Any<QueryParameters<string>>(),
                0.0);
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
            await _controller.LoadProfitCenterCostGrid(request, 0.0);

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
            _profitCentreService.GetPagedProfitCenterCostSummaryAsync(Arg.Any<QueryParameters<string>>(), 0.0)
                .Returns(ApiResponseDto<List<ProfitCentreCostDto>>.SuccessResponse(costData));

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ProfitCenterCostItem>>(Arg.Any<List<ProfitCentreCostDto>>())
                .Returns([new ProfitCenterCostItem { ProfitCentre = TestProfitCentre1, Cost = 1500m }]);

            // Act
            var result = await _controller.LoadProfitCenterCostGrid(request, 0.0);

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
                Page = 2,
                PageSize = 5,
                SortBy = "Cost",
                Descending = true
            };
            var costData = new List<ProfitCentreCostDto>
            {
                new() { ProfitCentre = TestProfitCentre1, Cost = 1000m }
            };
            var pagination = new PaginationDto { TotalRecords = 15, PageNumber = 2, PageSize = 5 };
            _profitCentreService.GetPagedProfitCenterCostSummaryAsync(Arg.Any<QueryParameters<string>>(), 0.0)
                .Returns(ApiResponseDto<List<ProfitCentreCostDto>>.SuccessResponse(costData, pagination));

            SetupDefaultMapper();
            _mapper.Map<List<ProfitCenterCostItem>>(Arg.Any<List<ProfitCentreCostDto>>())
                .Returns([new ProfitCenterCostItem { ProfitCentre = TestProfitCentre1, Cost = 1000m }]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel { TotalRecords = 15, PageNumber = 2, PageSize = 5 });

            // Act
            var result = await _controller.LoadProfitCenterCostGrid(request, 0.0);

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
            _profitCentreService.GetPagedProfitCenterCostSummaryAsync(Arg.Any<QueryParameters<string>>(), 0.0)
                .Returns(ApiResponseDto<List<ProfitCentreCostDto>>.SuccessResponse(null!));
            SetupDefaultMapper();

            // Act
            var result = await _controller.LoadProfitCenterCostGrid(request, 0.0);

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
            SetupDefaultCostSummaryResponse(0.0);
            SetupDefaultMapper();

            // Act
            var result = await _controller.LoadProfitCenterCostGrid(request, 0.0);

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
            _profitCentreService.GetPagedProfitCenterCostSummaryAsync(Arg.Any<QueryParameters<string>>(), 0.0)
                .Returns(ApiResponseDto<List<ProfitCentreCostDto>>.SuccessResponse(null!));
            SetupDefaultMapper();

            // Act
            var result = await _controller.LoadProfitCenterCostGrid(request, 0.0);

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
