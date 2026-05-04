using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Controllers;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.InvoiceByMonthControllerTests
{
    public class InvoiceByMonthControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectInvoiceService _invoiceService;
        private readonly InvoiceByMonthController _controller;

        public InvoiceByMonthControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _invoiceService = Substitute.For<IProjectInvoiceService>();
            _controller = new InvoiceByMonthController(_mapper, _invoiceService);
        }

        private void SetupGridMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
        }

        private static MonthlyInvoicesPivotDto BuildPivotDto(bool withRows = false)
        {
            var dto = new MonthlyInvoicesPivotDto
            {
                Months = [1, 2, 3],
                Rows = [],
                Pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 0 }
            };

            if (withRows)
            {
                dto.Rows =
                [
                    new MonthlyInvoicesSummaryRowDto
                    {
                        Program = "PROG1",
                        ParentProject = "PRJ001",
                        MonthlyAmounts = new Dictionary<int, decimal> { { 1, 100m }, { 2, 200m }, { 3, 300m } }
                    }
                ];
                dto.Pagination.TotalRecords = 1;
            }

            return dto;
        }

        #region Index

        [Fact]
        public async Task Index_ServiceReturnsSuccess_ReturnsViewWithViewModel()
        {
            // Arrange
            var pivotDto = BuildPivotDto();
            _invoiceService.GetMonthlyInvoicesSummaryAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<MonthlyInvoicesPivotDto>.SuccessResponse(pivotDto));
            SetupGridMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<InvoiceByMonthViewModel>(viewResult.Model);
            Assert.NotNull(model.Grid);
        }

        [Fact]
        public async Task Index_ServiceReturnsFailure_ReturnsViewWithOnlyStaticColumns()
        {
            // Arrange
            _invoiceService.GetMonthlyInvoicesSummaryAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<MonthlyInvoicesPivotDto>.FailureResponse([], new ApiMetaDto()));
            SetupGridMapper();

            // Act
            var result = await _controller.Index();

            // Assert — failure path: no month columns; only the 2 static columns (Program, ParentProject)
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<InvoiceByMonthViewModel>(viewResult.Model);
            Assert.Equal(2, model.Grid.Columns.Count);
            Assert.All(model.Grid.Data, r => Assert.Equal(string.Empty, r.Program));
        }

        [Fact]
        public async Task Index_ServiceReturnsNullData_ReturnsViewWithEmptyGrid()
        {
            // Arrange
            var response = ApiResponseDto<MonthlyInvoicesPivotDto>.SuccessResponse(null!);
            _invoiceService.GetMonthlyInvoicesSummaryAsync(Arg.Any<QueryParameters<string>>())
                .Returns(response);
            SetupGridMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<InvoiceByMonthViewModel>(viewResult.Model);
        }

        #endregion

        #region LoadGrid

        [Fact]
        public async Task LoadGrid_ValidRequest_ReturnsPartialViewWithDataGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            var pivotDto = BuildPivotDto(withRows: true);
            _invoiceService.GetMonthlyInvoicesSummaryAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<MonthlyInvoicesPivotDto>.SuccessResponse(pivotDto));
            SetupGridMapper();

            // Act
            var result = await _controller.LoadGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<MonthlyInvoicePivotRow>>(partial.Model);
        }

        [Fact]
        public async Task LoadGrid_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "error");

            // Act
            var result = await _controller.LoadGrid(new PaginationFilter<string>());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadGrid_WithRowData_MapsMonthlyAmountsToCorrectProperties()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            var pivotDto = BuildPivotDto(withRows: true);
            _invoiceService.GetMonthlyInvoicesSummaryAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<MonthlyInvoicesPivotDto>.SuccessResponse(pivotDto));
            SetupGridMapper();

            // Act
            var result = await _controller.LoadGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<MonthlyInvoicePivotRow>>(partial.Model);
            Assert.Single(grid.Data);
            Assert.Equal(100m, grid.Data[0].M1);
            Assert.Equal(200m, grid.Data[0].M2);
            Assert.Equal(300m, grid.Data[0].M3);
        }

        [Fact]
        public async Task LoadGrid_WithMonths_GeneratesCorrectFinancialYearColumnLabels()
        {
            // Arrange  
            var request = new PaginationFilter<string> { Filter = "{}" };
            // Month 1 (April in financial year) → calendarMonth = ((1+2)%12)+1 = 4 → "Apr"
            var pivotDto = new MonthlyInvoicesPivotDto
            {
                Months = [1, 9, 10],   // Apr, Dec, Jan
                Rows = [],
                Pagination = new PaginationDto()
            };
            _invoiceService.GetMonthlyInvoicesSummaryAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<MonthlyInvoicesPivotDto>.SuccessResponse(pivotDto));
            SetupGridMapper();

            // Act
            var result = await _controller.LoadGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<MonthlyInvoicePivotRow>>(partial.Model);
            // First 2 columns are Program and ParentProject; month columns follow
            var monthColumns = grid.Columns.Skip(2).ToList();
            Assert.Equal("1-Apr", monthColumns[0].DisplayName);
            Assert.Equal("9-Dec", monthColumns[1].DisplayName);
            Assert.Equal("10-Jan", monthColumns[2].DisplayName);
        }

        [Fact]
        public async Task LoadGrid_WithNullFilter_TreatsAsEmptyFilter()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = null };
            var pivotDto = BuildPivotDto();
            _invoiceService.GetMonthlyInvoicesSummaryAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<MonthlyInvoicesPivotDto>.SuccessResponse(pivotDto));
            SetupGridMapper();

            // Act
            var result = await _controller.LoadGrid(request);

            // Assert
            Assert.IsType<PartialViewResult>(result);
        }

        [Fact]
        public async Task LoadGrid_GridConfig_HasCorrectStaticProperties()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            var pivotDto = BuildPivotDto();
            _invoiceService.GetMonthlyInvoicesSummaryAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<MonthlyInvoicesPivotDto>.SuccessResponse(pivotDto));
            SetupGridMapper();

            // Act
            var result = await _controller.LoadGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<MonthlyInvoicePivotRow>>(partial.Model);
            Assert.Equal("invoiceByMonthGrid", grid.GridId);
            Assert.Equal("/PACT/InvoiceByMonth/LoadGrid", grid.BindGridUrl);
            Assert.False(grid.AllowAdd);
            Assert.False(grid.AllowEdit);
            Assert.False(grid.AllowDelete);
            Assert.True(grid.ShowPagination);
        }

        #endregion
    }
}
