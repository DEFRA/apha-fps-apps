using Apha.Common.Utilities.StateManagement;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ContributionSummaryControllerTest
{
    public class ContributionSummaryControllerTests
    {
        private readonly IMapper                     _mapper;
        private readonly IContributionSummaryService _service;
        private readonly IProfitCentreService        _profitCentreService;
        private readonly IAppStateService            _appStateService;
        private readonly ContributionSummaryController _controller;

        public ContributionSummaryControllerTests()
        {
            _mapper              = Substitute.For<IMapper>();
            _service             = Substitute.For<IContributionSummaryService>();
            _profitCentreService = Substitute.For<IProfitCentreService>();
            _appStateService     = Substitute.For<IAppStateService>();
            _controller          = new ContributionSummaryController(_mapper, _service, _profitCentreService, _appStateService);
        }

        private static List<ProfitCentreDto> MakeProfitCentres()
            =>
            [
                new() { ProfitCentreId = "ENV", ProfitCentreName = "Environment" },
                new() { ProfitCentreId = "ASU", ProfitCentreName = "Animal Science" }
            ];

        private static List<ContributionSummaryRowDto> MakeRowDtos(int count = 2)
            => Enumerable.Range(1, count)
                .Select(i => new ContributionSummaryRowDto { WorkGroup = $"WG{i}", WgGrade = $"G{i}", Fec = 100m * i })
                .ToList();

        private static ContributionSummaryTotalsDto MakeTotalsDto(string sellingPc = "ENV")
            => new() { SellingPc = sellingPc, TotalFec = 500m, TotalToRecover = 1200m, Surplus = -700m };

        private static PaginationFilter<string> MakeRequest(
            int page = 1, int pageSize = 10,
            string? filter  = null,
            string? sortBy  = null,
            bool descending = false)
            => new() { Page = page, PageSize = pageSize, Filter = filter, SortBy = sortBy, Descending = descending };

        private void SetupRowsSuccess(string sellingPc, List<ContributionSummaryRowDto>? dtos = null)
        {
            dtos ??= MakeRowDtos();
            _service.GetRowsAsync(sellingPc, Arg.Any<string?>(), Arg.Any<bool>())
                .Returns(ApiResponseDto<List<ContributionSummaryRowDto>>.SuccessResponse(dtos));
        }

        private void SetupMapper(List<ContributionSummaryRowDto>? dtos = null, List<ContributionSummaryRowItem>? items = null)
        {
            items ??= (dtos ?? MakeRowDtos())
                .Select(d => new ContributionSummaryRowItem { WorkGroup = d.WorkGroup, WgGrade = d.WgGrade })
                .ToList();
            _mapper.Map<List<ContributionSummaryRowItem>>(Arg.Any<List<ContributionSummaryRowDto>>()).Returns(items);
        }

        #region Index

        [Fact]
        public async Task Index_PopulatesSellingProfitCentresFromService()
        {
            // Arrange
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(MakeProfitCentres()));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<ContributionSummaryViewModel>(viewResult.Model);
            Assert.Equal(2, model.SellingProfitCentres.Count);
            Assert.Equal("ENV", model.SellingProfitCentres[0].Value);
            Assert.Equal("ENV - Environment", model.SellingProfitCentres[0].Text);
        }

        [Fact]
        public async Task Index_WhenServiceFails_ReturnsEmptyProfitCentreList()
        {
            // Arrange
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Error" } }, new ApiMetaDto()));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<ContributionSummaryViewModel>(viewResult.Model);
            Assert.Empty(model.SellingProfitCentres);
        }

        [Fact]
        public async Task Index_WhenServiceReturnsNullData_ReturnsEmptyProfitCentreList()
        {
            // Arrange
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(null!));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<ContributionSummaryViewModel>(viewResult.Model);
            Assert.Empty(model.SellingProfitCentres);
        }

        #endregion

        #region LoadData — Validation

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task LoadData_WhenSellingPcIsNullOrWhitespace_ReturnsBadRequest(string? sellingPc)
        {
            // Act
            var result = await _controller.LoadData(MakeRequest(), sellingPc!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Selling PC is required.", badRequest.Value);
            await _service.DidNotReceive().GetRowsAsync(Arg.Any<string>());
        }

        #endregion

        #region LoadData — Happy path

        [Fact]
        public async Task LoadData_WithValidSellingPc_ReturnsDataGridPartialView()
        {
            // Arrange
            var sellingPc = "ENV";
            SetupRowsSuccess(sellingPc);
            SetupMapper();

            // Act
            var result = await _controller.LoadData(MakeRequest(), sellingPc);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<ContributionSummaryRowItem>>(partial.Model);
            await _service.Received(1).GetRowsAsync(sellingPc);
        }

        [Fact]
        public async Task LoadData_WhenServiceFails_ReturnsEmptyGrid()
        {
            // Arrange
            var sellingPc = "ENV";
            _service.GetRowsAsync(sellingPc, Arg.Any<string?>(), Arg.Any<bool>())
                .Returns(ApiResponseDto<List<ContributionSummaryRowDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Error" } }, new ApiMetaDto()));
            _mapper.Map<List<ContributionSummaryRowItem>>(Arg.Any<List<ContributionSummaryRowDto>>())
                .Returns(new List<ContributionSummaryRowItem>());

            // Act
            var result = await _controller.LoadData(MakeRequest(), sellingPc);

            // Assert
            var partial    = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ContributionSummaryRowItem>>(partial.Model);
            Assert.Empty(gridConfig.Data);
        }

        [Fact]
        public async Task LoadData_AppliesPagination()
        {
            // Arrange
            var sellingPc = "ENV";
            var dtos  = Enumerable.Range(1, 15).Select(i => new ContributionSummaryRowDto { WorkGroup = $"WG{i:D2}", WgGrade = $"G{i}" }).ToList();
            var items = dtos.Select(d => new ContributionSummaryRowItem { WorkGroup = d.WorkGroup, WgGrade = d.WgGrade }).ToList();

            SetupRowsSuccess(sellingPc, dtos);
            _mapper.Map<List<ContributionSummaryRowItem>>(Arg.Any<List<ContributionSummaryRowDto>>()).Returns(items);

            // Act — page 2, size 5
            var result = await _controller.LoadData(MakeRequest(page: 2, pageSize: 5), sellingPc);

            // Assert
            var gridConfig = Assert.IsType<DataGridConfig<ContributionSummaryRowItem>>(
                Assert.IsType<PartialViewResult>(result).Model);
            Assert.Equal(5, gridConfig.Data.Count);
            Assert.Equal(15, gridConfig.Pagination.TotalRecords);
        }

        #endregion

        #region LoadData — Filtering

        [Fact]
        public async Task LoadData_WhenWgGradeFilterApplied_ReturnsOnlyMatchingRows()
        {
            // Arrange
            var sellingPc = "ENV";
            var dtos  = MakeRowDtos(3);
            var items = new List<ContributionSummaryRowItem>
            {
                new() { WgGrade = "GradeA", WorkGroup = "WG1" },
                new() { WgGrade = "GradeB", WorkGroup = "WG2" },
                new() { WgGrade = "GradeA", WorkGroup = "WG3" }
            };

            SetupRowsSuccess(sellingPc, dtos);
            _mapper.Map<List<ContributionSummaryRowItem>>(Arg.Any<List<ContributionSummaryRowDto>>()).Returns(items);

            // Act
            var result = await _controller.LoadData(
                MakeRequest(filter: "{\"WgGrade\":\"GradeA\"}"), sellingPc);

            // Assert
            var gridConfig = Assert.IsType<DataGridConfig<ContributionSummaryRowItem>>(
                Assert.IsType<PartialViewResult>(result).Model);
            Assert.Equal(2, gridConfig.Data.Count);
            Assert.All(gridConfig.Data, r => Assert.Equal("GradeA", r.WgGrade));
        }

        [Fact]
        public async Task LoadData_WhenWorkGroupFilterApplied_ReturnsOnlyMatchingRows()
        {
            // Arrange
            var sellingPc = "ENV";
            var dtos  = MakeRowDtos(2);
            var items = new List<ContributionSummaryRowItem>
            {
                new() { WorkGroup = "AlphaTeam", WgGrade = "G1" },
                new() { WorkGroup = "BetaTeam",  WgGrade = "G2" }
            };

            SetupRowsSuccess(sellingPc, dtos);
            _mapper.Map<List<ContributionSummaryRowItem>>(Arg.Any<List<ContributionSummaryRowDto>>()).Returns(items);

            // Act
            var result = await _controller.LoadData(
                MakeRequest(filter: "{\"WorkGroup\":\"Alpha\"}"), sellingPc);

            // Assert
            var gridConfig = Assert.IsType<DataGridConfig<ContributionSummaryRowItem>>(
                Assert.IsType<PartialViewResult>(result).Model);
            Assert.Single(gridConfig.Data);
            Assert.Equal("AlphaTeam", gridConfig.Data[0].WorkGroup);
        }

        #endregion

        #region LoadData — Sorting

        [Fact]
        public async Task LoadData_WhenSortByWgGradeAscending_DelegatesSortingToService()
        {
            // Arrange — sorting is applied in the repository, so the controller must
            // forward the sort request and preserve the order it receives back.
            var sellingPc = "ENV";
            var dtos  = MakeRowDtos(3);
            var items = new List<ContributionSummaryRowItem>
            {
                new() { WgGrade = "GA" },
                new() { WgGrade = "GB" },
                new() { WgGrade = "GC" }
            };

            SetupRowsSuccess(sellingPc, dtos);
            _mapper.Map<List<ContributionSummaryRowItem>>(Arg.Any<List<ContributionSummaryRowDto>>()).Returns(items);

            // Act
            var result = await _controller.LoadData(
                MakeRequest(sortBy: "WgGrade", descending: false), sellingPc);

            // Assert
            await _service.Received(1).GetRowsAsync(sellingPc, "WgGrade", false);
            var gridConfig = Assert.IsType<DataGridConfig<ContributionSummaryRowItem>>(
                Assert.IsType<PartialViewResult>(result).Model);
            Assert.Equal("GA", gridConfig.Data[0].WgGrade);
            Assert.Equal("GC", gridConfig.Data[2].WgGrade);
        }

        [Fact]
        public async Task LoadData_WhenSortByWorkGroupDescending_DelegatesSortingToService()
        {
            // Arrange
            var sellingPc = "ENV";
            var dtos  = MakeRowDtos(3);
            var items = new List<ContributionSummaryRowItem>
            {
                new() { WorkGroup = "Charlie" },
                new() { WorkGroup = "Bravo" },
                new() { WorkGroup = "Alpha" }
            };

            SetupRowsSuccess(sellingPc, dtos);
            _mapper.Map<List<ContributionSummaryRowItem>>(Arg.Any<List<ContributionSummaryRowDto>>()).Returns(items);

            // Act
            var result = await _controller.LoadData(
                MakeRequest(sortBy: "WorkGroup", descending: true), sellingPc);

            // Assert
            await _service.Received(1).GetRowsAsync(sellingPc, "WorkGroup", true);
            var gridConfig = Assert.IsType<DataGridConfig<ContributionSummaryRowItem>>(
                Assert.IsType<PartialViewResult>(result).Model);
            Assert.Equal("Charlie", gridConfig.Data[0].WorkGroup);
            Assert.Equal("Alpha",   gridConfig.Data[2].WorkGroup);
        }

        #endregion

        #region LoadTotals

        [Fact]
        public async Task LoadTotals_WithValidSellingPc_ReturnsPartialViewWithTotals()
        {
            // Arrange
            var sellingPc = "ENV";
            var dto       = MakeTotalsDto(sellingPc);
            _service.GetTotalsAsync(sellingPc)
                .Returns(ApiResponseDto<ContributionSummaryTotalsDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.LoadTotals(sellingPc);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_ContributionSummaryTotals", partial.ViewName);
            Assert.Equal(dto, partial.Model);
            await _service.Received(1).GetTotalsAsync(sellingPc);
        }

        [Fact]
        public async Task LoadTotals_WhenServiceFails_ReturnsPartialViewWithNullModel()
        {
            // Arrange
            var sellingPc = "ENV";
            _service.GetTotalsAsync(sellingPc)
                .Returns(ApiResponseDto<ContributionSummaryTotalsDto>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Error" } }, new ApiMetaDto()));

            // Act
            var result = await _controller.LoadTotals(sellingPc);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Null(partial.Model);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task LoadTotals_WhenSellingPcIsNullOrWhitespace_ReturnsBadRequest(string? sellingPc)
        {
            // Act
            var result = await _controller.LoadTotals(sellingPc!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Selling PC is required.", badRequest.Value);
            await _service.DidNotReceive().GetTotalsAsync(Arg.Any<string>());
        }

        #endregion
    }
}
