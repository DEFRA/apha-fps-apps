using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;
using AutoMapper;
using NSubstitute;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactSummarisedWgTimeApiClientTest
{
    public class PactSummarisedWgTimeApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactSummarisedWgTimeApiClient _client;

        public PactSummarisedWgTimeApiClientTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PactSummarisedWgTimeApiClient(_http, _mapper);
        }

        #region GetSummarisedWorkgroupTimeSummaryAsync Tests

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithValidWorkGroup_IncludesWorkGroupInUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string workGroup = "WG1";
            var pivotRes = new SummarisedWgTimePivotRes
            {
                Months = [1, 2, 3],
                Rows = new List<SummarisedWgTimeRes>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ1",
                        ProjectTitle = "Project 1",
                        April = 100,
                        May = 150,
                        SumOfTime = 250,
                        SumOfCost = 2500
                    }
                },
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var apiResponse = new ApiResponse<SummarisedWgTimePivotRes>
            {
                Success = true,
                Data = pivotRes
            };
            var expectedDto = ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(
                new SummarisedWgTimeViewDto
                {
                    Months = [1, 2, 3],
                    Rows = new List<SummarisedWgTimeDto>
                    {
                        new()
                        {
                            WorkGroup = "WG1",
                            ParentProject = "PRJ1",
                            ProjectTitle = "Project 1",
                            SumOfTime = 250,
                            SumOfCost = 2500
                        }
                    }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            );

            _http.GetAsync<SummarisedWgTimePivotRes>(Arg.Is<string>(url =>
                url.Contains(PactApiEndpoints.GetPagedSummarisedWorkgroupTime) &&
                url.Contains($"workGroup={Uri.EscapeDataString(workGroup)}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SummarisedWgTimeViewDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.Rows);
            Assert.Equal("WG1", result.Data.Rows[0].WorkGroup);
            await _http.Received(1).GetAsync<SummarisedWgTimePivotRes>(
                Arg.Is<string>(url => url.Contains($"workGroup={Uri.EscapeDataString(workGroup)}")));
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithNullWorkGroup_UsesBaseUrlWithoutWorkGroupParameter()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pivotRes = new SummarisedWgTimePivotRes
            {
                Months = [1, 2, 3],
                Rows = new List<SummarisedWgTimeRes>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ1",
                        ProjectTitle = "Project 1",
                        SumOfTime = 100,
                        SumOfCost = 1000
                    },
                    new()
                    {
                        WorkGroup = "WG2",
                        ParentProject = "PRJ2",
                        ProjectTitle = "Project 2",
                        SumOfTime = 200,
                        SumOfCost = 2000
                    }
                },
                Pagination = new Pagination()
            };
            var apiResponse = new ApiResponse<SummarisedWgTimePivotRes>
            {
                Success = true,
                Data = pivotRes
            };
            var expectedDto = ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(
                new SummarisedWgTimeViewDto
                {
                    Months = [1, 2, 3],
                    Rows = new List<SummarisedWgTimeDto>()
                },
                new PaginationDto()
            );

            _http.GetAsync<SummarisedWgTimePivotRes>(Arg.Is<string>(url =>
                url.Contains(PactApiEndpoints.GetPagedSummarisedWorkgroupTime)))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SummarisedWgTimeViewDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetSummarisedWorkgroupTimeSummaryAsync(query, "");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<SummarisedWgTimePivotRes>(
                Arg.Is<string>(url => 
                    url.Contains(PactApiEndpoints.GetPagedSummarisedWorkgroupTime) &&
                    !url.Contains("workGroup=")));
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithEmptyWorkGroup_UsesBaseUrlWithoutWorkGroupParameter()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string workGroup = "";
            var apiResponse = new ApiResponse<SummarisedWgTimePivotRes>
            {
                Success = true,
                Data = new SummarisedWgTimePivotRes
                {
                    Months = [],
                    Rows = [],
                    Pagination = new Pagination()
                }
            };
            var expectedDto = ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(
                new SummarisedWgTimeViewDto(),
                new PaginationDto()
            );

            _http.GetAsync<SummarisedWgTimePivotRes>(Arg.Is<string>(url =>
                url.Contains(PactApiEndpoints.GetPagedSummarisedWorkgroupTime)))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SummarisedWgTimeViewDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<SummarisedWgTimePivotRes>(
                Arg.Is<string>(url => !url.Contains("workGroup=")));
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithWhitespaceWorkGroup_UsesBaseUrlWithoutWorkGroupParameter()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string workGroup = "   ";
            var apiResponse = new ApiResponse<SummarisedWgTimePivotRes>
            {
                Success = true,
                Data = new SummarisedWgTimePivotRes()
            };
            var expectedDto = ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(
                new SummarisedWgTimeViewDto(),
                new PaginationDto()
            );

            _http.GetAsync<SummarisedWgTimePivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SummarisedWgTimeViewDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<SummarisedWgTimePivotRes>(
                Arg.Is<string>(url => !url.Contains("workGroup=")));
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string workGroup = "WG1";
            var errors = new List<ApiError>
            {
                new() { Message = "API Error", Code = "API_ERROR" }
            };
            var apiResponse = new ApiResponse<SummarisedWgTimePivotRes>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<SummarisedWgTimeViewDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto>
                {
                    new() { Message = "API Error", Code = "API_ERROR" }
                },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<SummarisedWgTimePivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SummarisedWgTimeViewDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithPaginationParameters_IncludesParametersInUrl()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 2,
                PageSize = 20,
                SortBy = "SumOfTime",
                Descending = true
            };
            const string workGroup = "WG1";
            var apiResponse = new ApiResponse<SummarisedWgTimePivotRes>
            {
                Success = true,
                Data = new SummarisedWgTimePivotRes
                {
                    Months = [1],
                    Rows = [],
                    Pagination = new Pagination
                    {
                        PageNumber = 2,
                        PageSize = 20
                    }
                }
            };
            var expectedDto = ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(
                new SummarisedWgTimeViewDto(),
                new PaginationDto()
            );

            _http.GetAsync<SummarisedWgTimePivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SummarisedWgTimeViewDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<SummarisedWgTimePivotRes>(
                Arg.Is<string>(url =>
                    url.Contains("Page=2") &&
                    url.Contains("PageSize=20")));
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithSearchParameter_IncludesSearchInUrl()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Search = "PRJ1"
            };
            const string workGroup = "WG1";
            var apiResponse = new ApiResponse<SummarisedWgTimePivotRes>
            {
                Success = true,
                Data = new SummarisedWgTimePivotRes
                {
                    Months = [1],
                    Rows = new List<SummarisedWgTimeRes>
                    {
                        new()
                        {
                            WorkGroup = "WG1",
                            ParentProject = "PRJ1",
                            ProjectTitle = "Project 1",
                            SumOfTime = 100,
                            SumOfCost = 1000
                        }
                    },
                    Pagination = new Pagination()
                }
            };
            var expectedDto = ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(
                new SummarisedWgTimeViewDto(),
                new PaginationDto()
            );

            _http.GetAsync<SummarisedWgTimePivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SummarisedWgTimeViewDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<SummarisedWgTimePivotRes>(
                Arg.Is<string>(url => url.Contains("Search=PRJ1")));
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithEmptyResult_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string workGroup = "WG_NONEXISTENT";
            var apiResponse = new ApiResponse<SummarisedWgTimePivotRes>
            {
                Success = true,
                Data = new SummarisedWgTimePivotRes
                {
                    Months = [],
                    Rows = [],
                    Pagination = new Pagination
                    {
                        PageNumber = 1,
                        PageSize = 10,
                        TotalRecords = 0,
                        TotalPages = 0
                    }
                }
            };
            var expectedDto = ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(
                new SummarisedWgTimeViewDto
                {
                    Months = [],
                    Rows = []
                },
                new PaginationDto
                {
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 0,
                    TotalPages = 0
                }
            );

            _http.GetAsync<SummarisedWgTimePivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SummarisedWgTimeViewDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data.Rows);
            Assert.Empty(result.Data.Months);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithAllTwelveMonths_ReturnsPivotWithAllMonths()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string workGroup = "WG1";
            var pivotRes = new SummarisedWgTimePivotRes
            {
                Months = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
                Rows = new List<SummarisedWgTimeRes>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ1",
                        ProjectTitle = "Project 1",
                        April = 10, May = 20, June = 30, July = 40,
                        August = 50, September = 60, October = 70, November = 80,
                        December = 90, January = 100, February = 110, March = 120,
                        SumOfTime = 780,
                        SumOfCost = 7800
                    }
                },
                Pagination = new Pagination()
            };
            var apiResponse = new ApiResponse<SummarisedWgTimePivotRes>
            {
                Success = true,
                Data = pivotRes
            };
            var expectedDto = ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(
                new SummarisedWgTimeViewDto
                {
                    Months = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
                    Rows = []
                },
                new PaginationDto()
            );

            _http.GetAsync<SummarisedWgTimePivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SummarisedWgTimeViewDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(12, result.Data.Months.Count);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithSpecialCharactersInWorkGroup_EscapesCharactersInUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string workGroup = "WG Test & Special";
            var apiResponse = new ApiResponse<SummarisedWgTimePivotRes>
            {
                Success = true,
                Data = new SummarisedWgTimePivotRes
                {
                    Months = [],
                    Rows = [],
                    Pagination = new Pagination()
                }
            };
            var expectedDto = ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(
                new SummarisedWgTimeViewDto(),
                new PaginationDto()
            );

            _http.GetAsync<SummarisedWgTimePivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SummarisedWgTimeViewDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<SummarisedWgTimePivotRes>(
                Arg.Is<string>(url => url.Contains(Uri.EscapeDataString(workGroup))));
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_UsesCorrectEndpoint()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string workGroup = "WG1";
            var apiResponse = new ApiResponse<SummarisedWgTimePivotRes>
            {
                Success = true,
                Data = new SummarisedWgTimePivotRes()
            };
            var expectedDto = ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(
                new SummarisedWgTimeViewDto(),
                new PaginationDto()
            );

            _http.GetAsync<SummarisedWgTimePivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SummarisedWgTimeViewDto>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);

            // Assert
            await _http.Received(1).GetAsync<SummarisedWgTimePivotRes>(
                Arg.Is<string>(url => url.StartsWith(PactApiEndpoints.GetPagedSummarisedWorkgroupTime)));
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WhenApiReturnsServerError_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string workGroup = "WG1";
            var errors = new List<ApiError>
            {
                new() { Message = "Internal Server Error", Code = "SERVER_ERROR" }
            };
            var apiResponse = new ApiResponse<SummarisedWgTimePivotRes>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<SummarisedWgTimeViewDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto>
                {
                    new() { Message = "Internal Server Error", Code = "SERVER_ERROR" }
                },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<SummarisedWgTimePivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SummarisedWgTimeViewDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("SERVER_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WhenApiReturnsMultipleErrors_ReturnsAllErrors()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string workGroup = "WG1";
            var errors = new List<ApiError>
            {
                new() { Message = "Error 1", Code = "ERROR_1" },
                new() { Message = "Error 2", Code = "ERROR_2" }
            };
            var apiResponse = new ApiResponse<SummarisedWgTimePivotRes>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<SummarisedWgTimeViewDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto>
                {
                    new() { Message = "Error 1", Code = "ERROR_1" },
                    new() { Message = "Error 2", Code = "ERROR_2" }
                },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<SummarisedWgTimePivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SummarisedWgTimeViewDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal(2, result.Errors.Count);
        }

        #endregion
    }
}
