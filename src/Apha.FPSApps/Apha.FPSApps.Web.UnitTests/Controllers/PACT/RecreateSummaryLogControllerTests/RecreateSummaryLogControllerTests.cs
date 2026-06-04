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

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.RecreateSummariesLogControllerTest
{
    public class RecreateSummaryLogControllerTests
    {
        private readonly IMapper _mockMapper;
        private readonly IRecreateAndReleaseSummaryService _mockLogService;
        private readonly RecreateSummaryLogController _controller;

        private const string TestUserId = "TestUser1";
        private const string TestUserName = "Test User";
        private const short TestPeriod = 1;
        private const int TestPage = 1;
        private const int TestPageSize = 20;
        private const int TestTotalRecords = 50;

        public RecreateSummaryLogControllerTests()
        {
            _mockMapper = Substitute.For<IMapper>();
            _mockLogService = Substitute.For<IRecreateAndReleaseSummaryService>();
            _controller = new RecreateSummaryLogController(_mockMapper, _mockLogService);
        }

        #region Index

        [Fact]
        public async Task Index_WithSuccessfulResponse_ReturnsViewWithViewModel()
        {
            // Arrange
            var paginationFilter = new PaginationFilter<string> { Filter = "{}" };
            var query = new QueryParameters<string> { Page = 1, PageSize = 20 };

            var apiResponse = new ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>>
            {
                Success = true,
                Data = new PaginatedResult<RecreateSummaryLogDto>(
                    new List<RecreateSummaryLogDto>
                    {
                        new() { Id = 1, UserId = TestUserId, Comments = TestUserName, Period = TestPeriod, DateDone = DateTime.UtcNow }
                    },
                    TestTotalRecords,
                    TestPage,
                    TestPageSize)
            };

            _mockMapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()).Returns(query);
            _mockLogService.GetRecreateSummaryLogAsync(query).Returns(apiResponse);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RecreateSummaryLogViewModel>(viewResult.Model);
            Assert.NotNull(model.LogsGrid);
            await _mockLogService.Received(1).GetRecreateSummaryLogAsync(Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task Index_WithFailedResponse_ReturnsViewWithEmptyGrid()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 20 };

            var apiResponse = new ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "ERR001" } }
            };

            _mockMapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()).Returns(query);
            _mockLogService.GetRecreateSummaryLogAsync(query).Returns(apiResponse);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RecreateSummaryLogViewModel>(viewResult.Model);
            Assert.NotNull(model.LogsGrid);
            Assert.Empty(model.LogsGrid.Data);
        }

        [Fact]
        public async Task Index_WithNullData_ReturnsViewWithEmptyGrid()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 20 };

            var apiResponse = new ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>>
            {
                Success = true,
                Data = null
            };

            _mockMapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()).Returns(query);
            _mockLogService.GetRecreateSummaryLogAsync(query).Returns(apiResponse);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RecreateSummaryLogViewModel>(viewResult.Model);
            Assert.NotNull(model.LogsGrid);
            Assert.Empty(model.LogsGrid.Data);
        }

        #endregion

        #region LoadRecreateSummariesLogGrid

        [Fact]
        public async Task LoadRecreateSummariesLogGrid_WithValidRequest_ReturnsPartialViewWithGrid()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = TestPage,
                PageSize = TestPageSize,
                SortBy = "DateDone",
                Descending = true,
                Filter = "{}"
            };

            var query = new QueryParameters<string> { Page = TestPage, PageSize = TestPageSize };

            var apiResponse = new ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>>
            {
                Success = true,
                Data = new PaginatedResult<RecreateSummaryLogDto>(
                    new List<RecreateSummaryLogDto>
                    {
                        new() { Id = 1, UserId = TestUserId, Comments = TestUserName, Period = TestPeriod, DateDone = DateTime.UtcNow }
                    },
                    TestTotalRecords,
                    TestPage,
                    TestPageSize)
            };

            _mockMapper.Map<QueryParameters<string>>(request).Returns(query);
            _mockLogService.GetRecreateSummaryLogAsync(query).Returns(apiResponse);

            // Act
            var result = await _controller.LoadRecreateSummariesLogGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
            var model = Assert.IsType<DataGridConfig<RecreateSummaryLogItem>>(partialViewResult.Model);
            Assert.NotNull(model);
            await _mockLogService.Received(1).GetRecreateSummaryLogAsync(query);
        }

        [Fact]
        public async Task LoadRecreateSummariesLogGrid_WithFailedResponse_ReturnsPartialViewWithEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = TestPage,
                PageSize = TestPageSize,
                Filter = "{}"
            };

            var query = new QueryParameters<string> { Page = TestPage, PageSize = TestPageSize };

            var apiResponse = new ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "ERR001" } }
            };

            _mockMapper.Map<QueryParameters<string>>(request).Returns(query);
            _mockLogService.GetRecreateSummaryLogAsync(query).Returns(apiResponse);

            // Act
            var result = await _controller.LoadRecreateSummariesLogGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<RecreateSummaryLogItem>>(partialViewResult.Model);
            Assert.Empty(model.Data);
        }

        [Fact]
        public async Task LoadRecreateSummariesLogGrid_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = TestPage,
                PageSize = TestPageSize,
                Filter = "{}"
            };

            var query = new QueryParameters<string> { Page = TestPage, PageSize = TestPageSize };

            _mockMapper.Map<QueryParameters<string>>(request).Returns(query);
            _mockLogService.GetRecreateSummaryLogAsync(query)
                .Returns(Task.FromException<ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>>>(
                    new InvalidOperationException("Service error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.LoadRecreateSummariesLogGrid(request));
        }

        #endregion
    }
}
