using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.SummarisedWgTimeServiceTest
{
    public class SummarisedWgTimeServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactSummarisedWgTimeApiClient _pactSummarisedWgTimeApiClient;
        private readonly SummarisedWgTimeService _service;

        public SummarisedWgTimeServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _pactSummarisedWgTimeApiClient = Substitute.For<IPactSummarisedWgTimeApiClient>();
            _pactClient.PactSummarisedWgTime.Returns(_pactSummarisedWgTimeApiClient);
            _service = new SummarisedWgTimeService(_pactClient);
        }

        #region GetSummarisedWorkgroupTimeSummaryAsync Tests

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithValidQueryAndWorkGroup_ReturnsPivotData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var workGroup = "WG001";
            var pivotDto = new SummarisedWgTimeViewDto
            {
                Months = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
                Rows =
                [
                    new SummarisedWgTimeDto
                    {
                        WorkGroup = workGroup,
                        ProfitCentre = "PC001",
                        ParentProject = "PP001",
                        ProjectTitle = "Test Project",
                        April = 10.5m,
                        May = 15.0m,
                        SumOfTime = 25.5m,
                        SumOfCost = 1500.00m,
                        Budget = 10000.00m,
                        PercentSpent = 15.0m
                    },
                    new SummarisedWgTimeDto
                    {
                        WorkGroup = workGroup,
                        ProfitCentre = "PC002",
                        ParentProject = "PP002",
                        ProjectTitle = "Another Project",
                        June = 20.0m,
                        July = 18.5m,
                        SumOfTime = 38.5m,
                        SumOfCost = 2200.00m,
                        Budget = 15000.00m,
                        PercentSpent = 14.67m
                    }
                ],
                Pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };
            var expectedResponse = ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(
                pivotDto,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );
            _pactSummarisedWgTimeApiClient.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup)
                .Returns(expectedResponse);

            // Act
            var result = await _service.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(12, result.Data.Months.Count);
            Assert.Equal(2, result.Data.Rows.Count);
            Assert.Equal(workGroup, result.Data.Rows[0].WorkGroup);
            Assert.Equal(25.5m, result.Data.Rows[0].SumOfTime);
            await _pactSummarisedWgTimeApiClient.Received(1)
                .GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithNullWorkGroup_ReturnsAllWorkGroups()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pivotDto = new SummarisedWgTimeViewDto
            {
                Months = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
                Rows =
                [
                    new SummarisedWgTimeDto
                    {
                        WorkGroup = "WG001",
                        ParentProject = "PP001",
                        ProjectTitle = "Project 1",
                        SumOfTime = 50.0m,
                        SumOfCost = 3000.00m
                    },
                    new SummarisedWgTimeDto
                    {
                        WorkGroup = "WG002",
                        ParentProject = "PP002",
                        ProjectTitle = "Project 2",
                        SumOfTime = 75.0m,
                        SumOfCost = 4500.00m
                    }
                ],
                Pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };
            var expectedResponse = ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(pivotDto);
            _pactSummarisedWgTimeApiClient.GetSummarisedWorkgroupTimeSummaryAsync(query, "")
                .Returns(expectedResponse);

            // Act
            var result = await _service.GetSummarisedWorkgroupTimeSummaryAsync(query, "");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Rows.Count);
            Assert.Equal("WG001", result.Data.Rows[0].WorkGroup);
            Assert.Equal("WG002", result.Data.Rows[1].WorkGroup);
            await _pactSummarisedWgTimeApiClient.Received(1)
                .GetSummarisedWorkgroupTimeSummaryAsync(query, "");
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithEmptyResults_ReturnsSuccessWithEmptyRows()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var workGroup = "NONEXISTENT";
            var pivotDto = new SummarisedWgTimeViewDto
            {
                Months = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
                Rows = [],
                Pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            };
            var expectedResponse = ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(pivotDto);
            _pactSummarisedWgTimeApiClient.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup)
                .Returns(expectedResponse);

            // Act
            var result = await _service.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data.Rows);
            Assert.Equal(12, result.Data.Months.Count);
            Assert.Equal(0, result.Data.Pagination.TotalRecords);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithPaginationParameters_ReturnsCorrectPage()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 5 };
            var workGroup = "WG001";
            var pivotDto = new SummarisedWgTimeViewDto
            {
                Months = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
                Rows =
                [
                    new SummarisedWgTimeDto
                    {
                        WorkGroup = workGroup,
                        ParentProject = "PP006",
                        ProjectTitle = "Project 6",
                        SumOfTime = 30.0m,
                        SumOfCost = 1800.00m
                    }
                ],
                Pagination = new PaginationDto { PageNumber = 2, PageSize = 5, TotalRecords = 15 }
            };
            var expectedResponse = ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(pivotDto);
            _pactSummarisedWgTimeApiClient.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup)
                .Returns(expectedResponse);

            // Act
            var result = await _service.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.NotNull(result.Data.Pagination);
            Assert.Equal(2, result.Data.Pagination.PageNumber);
            Assert.Equal(5, result.Data.Pagination.PageSize);
            Assert.Equal(15, result.Data.Pagination.TotalRecords);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithAllMonthlyData_ReturnsCompleteFinancialYear()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var workGroup = "WG001";
            var pivotDto = new SummarisedWgTimeViewDto
            {
                Months = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
                Rows =
                [
                    new SummarisedWgTimeDto
                    {
                        WorkGroup = workGroup,
                        ParentProject = "PP001",
                        ProjectTitle = "Full Year Project",
                        April = 10.0m,
                        May = 12.0m,
                        June = 15.0m,
                        July = 18.0m,
                        August = 20.0m,
                        September = 22.0m,
                        October = 25.0m,
                        November = 28.0m,
                        December = 30.0m,
                        January = 32.0m,
                        February = 35.0m,
                        March = 38.0m,
                        SumOfTime = 285.0m,
                        SumOfCost = 17100.00m,
                        Budget = 20000.00m,
                        PercentSpent = 85.5m
                    }
                ],
                Pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var expectedResponse = ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(pivotDto);
            _pactSummarisedWgTimeApiClient.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup)
                .Returns(expectedResponse);

            // Act
            var result = await _service.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.Rows);
            var row = result.Data.Rows[0];
            Assert.Equal(10.0m, row.April);
            Assert.Equal(38.0m, row.March);
            Assert.Equal(285.0m, row.SumOfTime);
            Assert.Equal(17100.00m, row.SumOfCost);
            Assert.Equal(85.5m, row.PercentSpent);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var workGroup = "WG001";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Database connection failed", Code = "DB_ERROR" }
            };
            var expectedResponse = ApiResponseDto<SummarisedWgTimeViewDto>.FailureResponse(errors, new ApiMetaDto());
            _pactSummarisedWgTimeApiClient.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup)
                .Returns(expectedResponse);

            // Act
            var result = await _service.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Database connection failed", result.Errors[0].Message);
            Assert.Equal("DB_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WhenApiReturnsMultipleErrors_ReturnsAllErrors()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Invalid workgroup", Code = "INVALID_WG" },
                new ApiErrorDto { Message = "Unauthorized access", Code = "UNAUTHORIZED" }
            };
            var expectedResponse = ApiResponseDto<SummarisedWgTimeViewDto>.FailureResponse(errors, new ApiMetaDto());
            _pactSummarisedWgTimeApiClient.GetSummarisedWorkgroupTimeSummaryAsync(query, "")
                .Returns(expectedResponse);

            // Act
            var result = await _service.GetSummarisedWorkgroupTimeSummaryAsync(query, "");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal(2, result.Errors.Count);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithFilterParameters_PassesFiltersToApiClient()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 20,
                SortBy = "ProjectTitle",
                Descending = true
            };
            var workGroup = "WG001";
            var pivotDto = new SummarisedWgTimeViewDto
            {
                Months = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
                Rows = [],
                Pagination = new PaginationDto { PageNumber = 1, PageSize = 20, TotalRecords = 0 }
            };
            var expectedResponse = ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(pivotDto);
            _pactSummarisedWgTimeApiClient.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup)
                .Returns(expectedResponse);

            // Act
            var result = await _service.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _pactSummarisedWgTimeApiClient.Received(1)
                .GetSummarisedWorkgroupTimeSummaryAsync(
                    Arg.Is<QueryParameters<string>>(q =>
                        q.Page == 1 &&
                        q.PageSize == 20 &&
                        q.SortBy == "ProjectTitle" &&
                        q.Descending == true),
                    workGroup);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithNullBudgetAndPercentSpent_HandlesNullableFields()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pivotDto = new SummarisedWgTimeViewDto
            {
                Months = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
                Rows =
                [
                    new SummarisedWgTimeDto
                    {
                        WorkGroup = "WG001",
                        ParentProject = "PP001",
                        ProjectTitle = "Project Without Budget",
                        April = 10.0m,
                        SumOfTime = 10.0m,
                        SumOfCost = 600.00m,
                        Budget = null,
                        PercentSpent = null
                    }
                ],
                Pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var expectedResponse = ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(pivotDto);
            _pactSummarisedWgTimeApiClient.GetSummarisedWorkgroupTimeSummaryAsync(query, "")
                .Returns(expectedResponse);

            // Act
            var result = await _service.GetSummarisedWorkgroupTimeSummaryAsync(query, "");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            var row = result.Data.Rows[0];
            Assert.Null(row.Budget);
            Assert.Null(row.PercentSpent);
            Assert.Equal(600.00m, row.SumOfCost);
        }

        #endregion
    }
}
