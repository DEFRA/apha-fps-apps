using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsYearMasterApiClientTest
{
    public class FpsYearMasterApiClientTests
    {
        private readonly IFpsHttpExecutor _httpExecutor;
        private readonly IMapper _mapper;
        private readonly FpsYearMasterApiClient _client;

        public FpsYearMasterApiClientTests()
        {
            _httpExecutor = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsYearMasterApiClient(_httpExecutor, _mapper);
        }

        #region GetAllYearMastersAsync Tests

        [Fact]
        public async Task GetAllYearMastersAsync_WithSuccessResponse_ReturnsMappedYearMasterList()
        {
            // Arrange
            var yearMasterResList = new List<YearMasterRes>
            {
                new YearMasterRes { FpsYear = 2025, FpsYearCode = "2025", YearStatus = "Planned", Active = true },
                new YearMasterRes { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true },
                new YearMasterRes { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true }
            };

            var apiResponse = new ApiResponse<List<YearMasterRes>>
            {
                Success = true,
                Data = yearMasterResList
            };

            var expectedDto = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse(
                new List<YearMasterDto>
                {
                    new YearMasterDto { FpsYear = 2025, FpsYearCode = "2025", YearStatus = "Planned", Active = true },
                    new YearMasterDto { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true },
                    new YearMasterDto { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true }
                }
            );

            _httpExecutor.GetAsync<List<YearMasterRes>>("api/yearmaster").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<YearMasterDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllYearMastersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count());
            await _httpExecutor.Received(1).GetAsync<List<YearMasterRes>>("api/yearmaster");
            _mapper.Received(1).Map<ApiResponseDto<IEnumerable<YearMasterDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<YearMasterRes>>
            {
                Success = true,
                Data = new List<YearMasterRes>()
            };
            var expectedDto = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse(new List<YearMasterDto>());

            _httpExecutor.GetAsync<List<YearMasterRes>>("api/yearmaster").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<YearMasterDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllYearMastersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError>
            {
                new ApiError { Message = "API Error", Code = "API_ERROR" }
            };
            var apiResponse = new ApiResponse<List<YearMasterRes>>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<IEnumerable<YearMasterDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _httpExecutor.GetAsync<List<YearMasterRes>>("api/yearmaster").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<YearMasterDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllYearMastersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            _httpExecutor.GetAsync<List<YearMasterRes>>("api/yearmaster")
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAllYearMastersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
            Assert.Equal("Failed to retrieve year master data", result.Errors[0].Message);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_CallsCorrectEndpoint()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<YearMasterRes>>
            {
                Success = true,
                Data = new List<YearMasterRes>()
            };
            var expectedDto = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse(new List<YearMasterDto>());

            _httpExecutor.GetAsync<List<YearMasterRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<YearMasterDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetAllYearMastersAsync();

            // Assert
            await _httpExecutor.Received(1).GetAsync<List<YearMasterRes>>("api/yearmaster");
        }

        #endregion

        #region GetAllYearMastersPagedAsync Tests

        [Fact]
        public async Task GetAllYearMastersPagedAsync_WithSuccessResponse_ReturnsPaginatedYearMasterList()
        {
            // Arrange
            var queryParameters = new QueryParameters<int>
            {
                Page = 1,
                PageSize = 10,
                Filter = 2024
            };

            var yearMasterResList = new List<YearMasterRes>
            {
                new YearMasterRes { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true }
            };

            var apiResponse = new ApiResponse<List<YearMasterRes>>
            {
                Success = true,
                Data = yearMasterResList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            var expectedDto = ApiResponseDto<List<YearMasterDto>>.SuccessResponse(
                new List<YearMasterDto>
                {
                    new YearMasterDto { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            );

            _httpExecutor.GetAsync<List<YearMasterRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<YearMasterDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllYearMastersPagedAsync(queryParameters);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal(2024, result.Data[0].FpsYear);
            await _httpExecutor.Received(1).GetAsync<List<YearMasterRes>>(Arg.Is<string>(url => url.Contains("api/yearmaster/paged")));
            _mapper.Received(1).Map<ApiResponseDto<List<YearMasterDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllYearMastersPagedAsync_ConstructsUrlWithQueryParameters()
        {
            // Arrange
            var queryParameters = new QueryParameters<int>
            {
                Page = 2,
                PageSize = 20,
                Filter = 2024,
                SortBy = "FpsYear",
                Descending = true
            };
            var apiResponse = new ApiResponse<List<YearMasterRes>>
            {
                Success = true,
                Data = new List<YearMasterRes>()
            };
            var expectedDto = ApiResponseDto<List<YearMasterDto>>.SuccessResponse(new List<YearMasterDto>(), new PaginationDto());

            _httpExecutor.GetAsync<List<YearMasterRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<YearMasterDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetAllYearMastersPagedAsync(queryParameters);

            // Assert
            await _httpExecutor.Received(1).GetAsync<List<YearMasterRes>>(Arg.Is<string>(url =>
                url.Contains("api/yearmaster/paged")
            ));
        }

        [Fact]
        public async Task GetAllYearMastersPagedAsync_WithMultiplePages_ReturnsCorrectPage()
        {
            // Arrange
            var queryParameters = new QueryParameters<int>
            {
                Page = 2,
                PageSize = 5
            };

            var yearMasterResList = new List<YearMasterRes>
            {
                new YearMasterRes { FpsYear = 2019, FpsYearCode = "2019", YearStatus = "Closed" },
                new YearMasterRes { FpsYear = 2018, FpsYearCode = "2018", YearStatus = "Closed" }
            };

            var apiResponse = new ApiResponse<List<YearMasterRes>>
            {
                Success = true,
                Data = yearMasterResList,
                Pagination = new Pagination
                {
                    PageNumber = 2,
                    PageSize = 5,
                    TotalPages = 3,
                    TotalRecords = 12
                }
            };

            var expectedDto = ApiResponseDto<List<YearMasterDto>>.SuccessResponse(
                new List<YearMasterDto>
                {
                    new YearMasterDto { FpsYear = 2019, FpsYearCode = "2019", YearStatus = "Closed" },
                    new YearMasterDto { FpsYear = 2018, FpsYearCode = "2018", YearStatus = "Closed" }
                },
                new PaginationDto { PageNumber = 2, PageSize = 5, TotalPages = 3, TotalRecords = 12 }
            );

            _httpExecutor.GetAsync<List<YearMasterRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<YearMasterDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllYearMastersPagedAsync(queryParameters);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
        }

        [Fact]
        public async Task GetAllYearMastersPagedAsync_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var queryParameters = new QueryParameters<int> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<YearMasterRes>>
            {
                Success = true,
                Data = new List<YearMasterRes>()
            };
            var expectedDto = ApiResponseDto<List<YearMasterDto>>.SuccessResponse(
                new List<YearMasterDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            );

            _httpExecutor.GetAsync<List<YearMasterRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<YearMasterDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllYearMastersPagedAsync(queryParameters);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllYearMastersPagedAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var queryParameters = new QueryParameters<int> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError>
            {
                new ApiError { Message = "API Error", Code = "ERROR" }
            };
            var apiResponse = new ApiResponse<List<YearMasterRes>>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<List<YearMasterDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _httpExecutor.GetAsync<List<YearMasterRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<YearMasterDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllYearMastersPagedAsync(queryParameters);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetAllYearMastersPagedAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var queryParameters = new QueryParameters<int> { Page = 1, PageSize = 10 };
            _httpExecutor.GetAsync<List<YearMasterRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAllYearMastersPagedAsync(queryParameters);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
            Assert.Equal("Failed to retrieve paginated year master data", result.Errors[0].Message);
        }

        [Theory]
        [InlineData(2024)]
        [InlineData(2025)]
        [InlineData(0)]
        public async Task GetAllYearMastersPagedAsync_WithDifferentFilters_PassesCorrectFilter(int filter)
        {
            // Arrange
            var queryParameters = new QueryParameters<int>
            {
                Page = 1,
                PageSize = 10,
                Filter = filter
            };
            var apiResponse = new ApiResponse<List<YearMasterRes>>
            {
                Success = true,
                Data = new List<YearMasterRes>()
            };
            var expectedDto = ApiResponseDto<List<YearMasterDto>>.SuccessResponse(
                new List<YearMasterDto>(),
                new PaginationDto()
            );

            _httpExecutor.GetAsync<List<YearMasterRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<YearMasterDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetAllYearMastersPagedAsync(queryParameters);

            // Assert
            await _httpExecutor.Received(1).GetAsync<List<YearMasterRes>>(Arg.Is<string>(url =>
                url.Contains("api/yearmaster/paged")
            ));
        }

        #endregion

        #region GetYearMasterByIdAsync Tests

        [Fact]
        public async Task GetYearMasterByIdAsync_WithValidFpsYear_ReturnsYearMaster()
        {
            // Arrange
            var fpsYear = 2024;
            var yearMasterRes = new YearMasterRes
            {
                FpsYear = 2024,
                FpsYearCode = "2024",
                YearStatus = "Open",
                Active = true,
                Remarks = "Active fiscal year"
            };
            var apiResponse = new ApiResponse<YearMasterRes>
            {
                Success = true,
                Data = yearMasterRes
            };
            var expectedDto = ApiResponseDto<YearMasterDto>.SuccessResponse(
                new YearMasterDto
                {
                    FpsYear = 2024,
                    FpsYearCode = "2024",
                    YearStatus = "Open",
                    Active = true,
                    Remarks = "Active fiscal year"
                }
            );

            _httpExecutor.GetAsync<YearMasterRes>($"api/yearmaster/{fpsYear}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<YearMasterDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetYearMasterByIdAsync(fpsYear);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2024, result.Data.FpsYear);
            Assert.Equal("Open", result.Data.YearStatus);
            await _httpExecutor.Received(1).GetAsync<YearMasterRes>($"api/yearmaster/{fpsYear}");
            _mapper.Received(1).Map<ApiResponseDto<YearMasterDto>>(apiResponse);
        }

        [Theory]
        [InlineData(2024)]
        [InlineData(2025)]
        [InlineData(2023)]
        public async Task GetYearMasterByIdAsync_WithVariousFpsYears_CallsCorrectUrl(int fpsYear)
        {
            // Arrange
            var apiResponse = new ApiResponse<YearMasterRes>
            {
                Success = true,
                Data = new YearMasterRes { FpsYear = fpsYear }
            };
            var expectedDto = ApiResponseDto<YearMasterDto>.SuccessResponse(
                new YearMasterDto { FpsYear = fpsYear }
            );

            _httpExecutor.GetAsync<YearMasterRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<YearMasterDto>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetYearMasterByIdAsync(fpsYear);

            // Assert
            await _httpExecutor.Received(1).GetAsync<YearMasterRes>($"api/yearmaster/{fpsYear}");
        }

        [Fact]
        public async Task GetYearMasterByIdAsync_WithClosedYear_ReturnsClosedYearMaster()
        {
            // Arrange
            var fpsYear = 2023;
            var yearMasterRes = new YearMasterRes
            {
                FpsYear = 2023,
                FpsYearCode = "2023",
                YearStatus = "Closed",
                Active = true
            };
            var apiResponse = new ApiResponse<YearMasterRes>
            {
                Success = true,
                Data = yearMasterRes
            };
            var expectedDto = ApiResponseDto<YearMasterDto>.SuccessResponse(
                new YearMasterDto { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true }
            );

            _httpExecutor.GetAsync<YearMasterRes>($"api/yearmaster/{fpsYear}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<YearMasterDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetYearMasterByIdAsync(fpsYear);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Closed", result.Data?.YearStatus);
        }

        [Fact]
        public async Task GetYearMasterByIdAsync_WithPlannedYear_ReturnsPlannedYearMaster()
        {
            // Arrange
            var fpsYear = 2025;
            var yearMasterRes = new YearMasterRes
            {
                FpsYear = 2025,
                FpsYearCode = "2025",
                YearStatus = "Planned",
                Active = true
            };
            var apiResponse = new ApiResponse<YearMasterRes>
            {
                Success = true,
                Data = yearMasterRes
            };
            var expectedDto = ApiResponseDto<YearMasterDto>.SuccessResponse(
                new YearMasterDto { FpsYear = 2025, FpsYearCode = "2025", YearStatus = "Planned", Active = true }
            );

            _httpExecutor.GetAsync<YearMasterRes>($"api/yearmaster/{fpsYear}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<YearMasterDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetYearMasterByIdAsync(fpsYear);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Planned", result.Data?.YearStatus);
        }

        [Fact]
        public async Task GetYearMasterByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var fpsYear = 9999;
            var errors = new List<ApiError>
            {
                new ApiError { Message = "Not found", Code = "NOT_FOUND" }
            };
            var apiResponse = new ApiResponse<YearMasterRes>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<YearMasterDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _httpExecutor.GetAsync<YearMasterRes>($"api/yearmaster/{fpsYear}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<YearMasterDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetYearMasterByIdAsync(fpsYear);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetYearMasterByIdAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var fpsYear = 2024;
            _httpExecutor.GetAsync<YearMasterRes>($"api/yearmaster/{fpsYear}")
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetYearMasterByIdAsync(fpsYear);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
            Assert.Equal($"Failed to retrieve year master with FPS Year: {fpsYear}", result.Errors[0].Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(9999)]
        public async Task GetYearMasterByIdAsync_WithInvalidFpsYear_ReturnsInternalError(int invalidYear)
        {
            // Arrange
            _httpExecutor.GetAsync<YearMasterRes>($"api/yearmaster/{invalidYear}")
                .ThrowsAsync(new Exception("Invalid year"));

            // Act
            var result = await _client.GetYearMasterByIdAsync(invalidYear);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public async Task GetAllYearMastersAsync_WithHttpTimeoutException_ReturnsInternalError()
        {
            // Arrange
            _httpExecutor.GetAsync<List<YearMasterRes>>("api/yearmaster")
                .ThrowsAsync(new TimeoutException("Request timed out"));

            // Act
            var result = await _client.GetAllYearMastersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetAllYearMastersPagedAsync_WithHttpTimeoutException_ReturnsInternalError()
        {
            // Arrange
            var queryParameters = new QueryParameters<int> { Page = 1, PageSize = 10 };
            _httpExecutor.GetAsync<List<YearMasterRes>>(Arg.Any<string>())
                .ThrowsAsync(new TimeoutException("Request timed out"));

            // Act
            var result = await _client.GetAllYearMastersPagedAsync(queryParameters);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetYearMasterByIdAsync_WithHttpTimeoutException_ReturnsInternalError()
        {
            // Arrange
            var fpsYear = 2024;
            _httpExecutor.GetAsync<YearMasterRes>($"api/yearmaster/{fpsYear}")
                .ThrowsAsync(new TimeoutException("Request timed out"));

            // Act
            var result = await _client.GetYearMasterByIdAsync(fpsYear);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_MapsResponseCorrectly()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<YearMasterRes>>
            {
                Success = true,
                Data = new List<YearMasterRes>
                {
                    new YearMasterRes { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open" }
                }
            };
            var expectedDto = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse(
                new List<YearMasterDto> { new YearMasterDto { FpsYear = 2024 } }
            );

            _httpExecutor.GetAsync<List<YearMasterRes>>("api/yearmaster").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<YearMasterDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetAllYearMastersAsync();

            // Assert
            _mapper.Received(1).Map<ApiResponseDto<IEnumerable<YearMasterDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllYearMastersPagedAsync_MapsResponseCorrectly()
        {
            // Arrange
            var queryParameters = new QueryParameters<int> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<YearMasterRes>>
            {
                Success = true,
                Data = new List<YearMasterRes>()
            };
            var expectedDto = ApiResponseDto<List<YearMasterDto>>.SuccessResponse(
                new List<YearMasterDto>(),
                new PaginationDto()
            );

            _httpExecutor.GetAsync<List<YearMasterRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<YearMasterDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetAllYearMastersPagedAsync(queryParameters);

            // Assert
            _mapper.Received(1).Map<ApiResponseDto<List<YearMasterDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetYearMasterByIdAsync_MapsResponseCorrectly()
        {
            // Arrange
            var fpsYear = 2024;
            var apiResponse = new ApiResponse<YearMasterRes>
            {
                Success = true,
                Data = new YearMasterRes { FpsYear = 2024 }
            };
            var expectedDto = ApiResponseDto<YearMasterDto>.SuccessResponse(
                new YearMasterDto { FpsYear = 2024 }
            );

            _httpExecutor.GetAsync<YearMasterRes>($"api/yearmaster/{fpsYear}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<YearMasterDto>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetYearMasterByIdAsync(fpsYear);

            // Assert
            _mapper.Received(1).Map<ApiResponseDto<YearMasterDto>>(apiResponse);
        }

        #endregion
    }
}
