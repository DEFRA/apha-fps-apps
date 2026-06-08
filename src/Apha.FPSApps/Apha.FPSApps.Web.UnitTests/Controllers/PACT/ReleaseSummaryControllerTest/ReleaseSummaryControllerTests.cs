using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Web.Areas.PACT.Controllers;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.ReleaseSummaryControllerTest
{
    public class ReleaseSummaryControllerTests
    {
        private readonly IMapper _mockMapper;
        private readonly IReleaseSummaryService _mockService;
        private readonly ReleaseSummaryController _controller;

        private const string TestPeriodName       = "TestPeriod";
        private const short  TestFinalSummariesRun = 1;

        public ReleaseSummaryControllerTests()
        {
            _mockMapper  = Substitute.For<IMapper>();
            _mockService = Substitute.For<IReleaseSummaryService>();
            _controller  = new ReleaseSummaryController(_mockMapper, _mockService);
        }

        // ── helpers ──────────────────────────────────────────────────────────

        private static ApiResponseDto<ReleaseSummaryDto> SuccessResponse(
            IReadOnlyList<ReleasePeriodDto>? data = null) =>
            new()
            {
                Success = true,
                Data    = new ReleaseSummaryDto { ReleasePeriods = data ?? new List<ReleasePeriodDto>().AsReadOnly() }
            };

        private static ApiResponseDto<ReleaseSummaryDto> FailureResponse() =>
            new()
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Code = "ERR001", Message = "API Error" } }
            };

        private static List<ReleasePeriodDto> TwoPeriods() =>
        [
            new() { PeriodName = "Period1", StartPeriod = 0.5, EndPeriod = 1.0, FinalSummariesRun = 0 },
            new() { PeriodName = "Period2", StartPeriod = 1.5, EndPeriod = 2.0, FinalSummariesRun = 1 }
        ];

        // ─────────────────────────────────────────────────────────────────────

        #region Index

        [Fact]
        public async Task Index_WithSuccessfulResponseAndData_ReturnsViewWithPopulatedGrid()
        {
            // Arrange
            var periods = TwoPeriods();
            _mockService.GetReleaseSummariesAsync().Returns(SuccessResponse(periods.AsReadOnly()));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ReleaseSummaryViewModel>(viewResult.Model);
            Assert.NotNull(model.ReleaseSummaryGrid);
            Assert.Equal(2, model.ReleaseSummaryGrid.Data.Count);
            Assert.Equal("Period1", model.ReleaseSummaryGrid.Data[0].PeriodName);
            Assert.Equal("Period2", model.ReleaseSummaryGrid.Data[1].PeriodName);

            await _mockService.Received(1).GetReleaseSummariesAsync();
        }

        [Fact]
        public async Task Index_WithEmptyData_ReturnsViewWithEmptyGrid()
        {
            // Arrange
            _mockService.GetReleaseSummariesAsync().Returns(SuccessResponse());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ReleaseSummaryViewModel>(viewResult.Model);
            Assert.NotNull(model.ReleaseSummaryGrid);
            Assert.Empty(model.ReleaseSummaryGrid.Data);

            await _mockService.Received(1).GetReleaseSummariesAsync();
        }

        [Fact]
        public async Task Index_WithNullData_ReturnsViewWithEmptyGrid()
        {
            // Arrange
            _mockService.GetReleaseSummariesAsync().Returns(new ApiResponseDto<ReleaseSummaryDto>
            {
                Success = true,
                Data    = null
            });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ReleaseSummaryViewModel>(viewResult.Model);
            Assert.NotNull(model.ReleaseSummaryGrid);
            Assert.Empty(model.ReleaseSummaryGrid.Data);

            await _mockService.Received(1).GetReleaseSummariesAsync();
        }

        [Fact]
        public async Task Index_WithFailedResponse_ReturnsViewWithEmptyGrid()
        {
            // Arrange
            _mockService.GetReleaseSummariesAsync().Returns(FailureResponse());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ReleaseSummaryViewModel>(viewResult.Model);
            Assert.NotNull(model.ReleaseSummaryGrid);
            Assert.Empty(model.ReleaseSummaryGrid.Data);

            await _mockService.Received(1).GetReleaseSummariesAsync();
        }

        [Fact]
        public async Task Index_GridConfig_HasCorrectStaticProperties()
        {
            // Arrange
            _mockService.GetReleaseSummariesAsync().Returns(SuccessResponse());

            // Act
            var result = await _controller.Index();

            // Assert
            var model = Assert.IsType<ReleaseSummaryViewModel>(((ViewResult)result).Model);
            var grid = model.ReleaseSummaryGrid;
            Assert.Equal("releaseSummariesGrid",                    grid.GridId);
            Assert.Equal("/PACT/ReleaseSummary/LoadReleaseSummaryGrid", grid.BindGridUrl);
            Assert.Equal(nameof(ReleasePeriodItem.PeriodName),      grid.KeyProperty);
            Assert.False(grid.AllowAdd);
            Assert.False(grid.AllowEdit);
            Assert.False(grid.AllowDelete);
            Assert.False(grid.AllowExport);
            Assert.False(grid.ShowPagination);
        }

        [Fact]
        public async Task Index_MapsAllPeriodFieldsToGridItems()
        {
            // Arrange
            var periods = new List<ReleasePeriodDto>
            {
                new()
                {
                    PeriodName        = "P1",
                    StartPeriod       = 1.5,
                    EndPeriod         = 2.5,
                    FinalSummariesRun = 1
                }
            };

            _mockService.GetReleaseSummariesAsync().Returns(SuccessResponse(periods.AsReadOnly()));

            // Act
            var result = await _controller.Index();

            // Assert
            var model = Assert.IsType<ReleaseSummaryViewModel>(((ViewResult)result).Model);
            var item  = Assert.Single(model.ReleaseSummaryGrid.Data);
            Assert.Equal("P1",     item.PeriodName);
            Assert.Equal(1.5,      item.StartPeriod);
            Assert.Equal(2.5,      item.EndPeriod);
            Assert.Equal((short)1, item.FinalSummariesRun);
        }

        [Fact]
        public async Task Index_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _mockService.GetReleaseSummariesAsync()
                .Returns(Task.FromException<ApiResponseDto<ReleaseSummaryDto>>(
                    new InvalidOperationException("Service error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Index());
        }

        #endregion

        #region LoadReleaseSummaryGrid

        [Fact]
        public async Task LoadReleaseSummaryGrid_WithSuccessfulResponseAndData_ReturnsPartialViewWithPopulatedGrid()
        {
            // Arrange
            var periods = TwoPeriods();
            _mockService.GetReleaseSummariesAsync().Returns(SuccessResponse(periods.AsReadOnly()));

            // Act
            var result = await _controller.LoadReleaseSummaryGrid();

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
            var grid = Assert.IsType<DataGridConfig<ReleasePeriodItem>>(partialResult.Model);
            Assert.Equal(2, grid.Data.Count);
            Assert.Equal("Period1", grid.Data[0].PeriodName);
            Assert.Equal("Period2", grid.Data[1].PeriodName);

            await _mockService.Received(1).GetReleaseSummariesAsync();
        }

        [Fact]
        public async Task LoadReleaseSummaryGrid_WithEmptyData_ReturnsPartialViewWithEmptyGrid()
        {
            // Arrange
            _mockService.GetReleaseSummariesAsync().Returns(SuccessResponse());

            // Act
            var result = await _controller.LoadReleaseSummaryGrid();

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
            var grid = Assert.IsType<DataGridConfig<ReleasePeriodItem>>(partialResult.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadReleaseSummaryGrid_WithNullData_ReturnsPartialViewWithEmptyGrid()
        {
            // Arrange
            _mockService.GetReleaseSummariesAsync().Returns(new ApiResponseDto<ReleaseSummaryDto>
            {
                Success = true,
                Data    = null
            });

            // Act
            var result = await _controller.LoadReleaseSummaryGrid();

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<ReleasePeriodItem>>(partialResult.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadReleaseSummaryGrid_WithFailedResponse_ReturnsPartialViewWithEmptyGrid()
        {
            // Arrange
            _mockService.GetReleaseSummariesAsync().Returns(FailureResponse());

            // Act
            var result = await _controller.LoadReleaseSummaryGrid();

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<ReleasePeriodItem>>(partialResult.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadReleaseSummaryGrid_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _mockService.GetReleaseSummariesAsync()
                .Returns(Task.FromException<ApiResponseDto<ReleaseSummaryDto>>(
                    new InvalidOperationException("Service error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.LoadReleaseSummaryGrid());
        }

        #endregion

        #region SetFinalSummaryRun

        [Fact]
        public async Task SetFinalSummaryRun_WithSuccessfulResponse_ReturnsOkWithFinalSummariesRunValue()
        {
            // Arrange
            var dto = new ReleasePeriodDto
            {
                PeriodName        = TestPeriodName,
                FinalSummariesRun = TestFinalSummariesRun
            };

            _mockService.SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun, Arg.Any<string>())
                .Returns(new ApiResponseDto<ReleasePeriodDto> { Success = true, Data = dto });

            // Act
            var result = await _controller.SetFinalSummaryRun(TestPeriodName, TestFinalSummariesRun, "1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal((short?)TestFinalSummariesRun, okResult.Value);

            await _mockService.Received(1).SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun, Arg.Any<string>());
        }

        [Fact]
        public async Task SetFinalSummaryRun_WithSuccessAndNullData_ReturnsOkWithNullValue()
        {
            // Arrange — service succeeds but returns null DTO (period not found on API side)
            _mockService.SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun, Arg.Any<string>())
                .Returns(new ApiResponseDto<ReleasePeriodDto> { Success = true, Data = null });

            // Act
            var result = await _controller.SetFinalSummaryRun(TestPeriodName, TestFinalSummariesRun, "1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }

        [Fact]
        public async Task SetFinalSummaryRun_WithFailedResponse_ReturnsBadRequestWithErrors()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "ERR002", Message = "Period not found" } };

            _mockService.SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun, Arg.Any<string>())
                .Returns(new ApiResponseDto<ReleasePeriodDto> { Success = false, Errors = errors });

            // Act
            var result = await _controller.SetFinalSummaryRun(TestPeriodName, TestFinalSummariesRun, "0");

            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnedErrors = Assert.IsAssignableFrom<IEnumerable<ApiErrorDto>>(badResult.Value);
            Assert.Single(returnedErrors);
            Assert.Equal("ERR002",           returnedErrors.First().Code);
            Assert.Equal("Period not found", returnedErrors.First().Message);

            await _mockService.Received(1).SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun, Arg.Any<string>());
        }

        [Fact]
        public async Task SetFinalSummaryRun_PassesCorrectArgumentsToService()
        {
            // Arrange
            const string periodName        = "ArgCheckPeriod";
            const short  finalSummariesRun = 3;

            _mockService.SetFinalSummaryRunAsync(periodName, finalSummariesRun, Arg.Any<string>())
                .Returns(new ApiResponseDto<ReleasePeriodDto>
                {
                    Success = true,
                    Data    = new ReleasePeriodDto { PeriodName = periodName, FinalSummariesRun = finalSummariesRun }
                });

            // Act
            await _controller.SetFinalSummaryRun(periodName, finalSummariesRun, "1");

            // Assert
            await _mockService.Received(1).SetFinalSummaryRunAsync(
                Arg.Is<string>(p => p  == periodName),
                Arg.Is<short>(f  => f  == finalSummariesRun),
                Arg.Any<string>());
        }

        [Fact]
        public async Task SetFinalSummaryRun_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _mockService.SetFinalSummaryRunAsync(TestPeriodName, TestFinalSummariesRun, Arg.Any<string>())
                .Returns(Task.FromException<ApiResponseDto<ReleasePeriodDto>>(
                    new InvalidOperationException("Service error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.SetFinalSummaryRun(TestPeriodName, TestFinalSummariesRun, "1"));
        }

        #endregion
    }
}
