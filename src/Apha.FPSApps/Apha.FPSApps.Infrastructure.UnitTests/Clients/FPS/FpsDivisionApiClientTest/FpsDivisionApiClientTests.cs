using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsDivisionApiClientTest
{
    public class FpsDivisionApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsDivisionApiClient _client;

        public FpsDivisionApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsDivisionApiClient(_http, _mapper);
        }

        #region GetAllDivisionsAsync Tests

        [Fact]
        public async Task GetAllDivisionsAsync_WithSuccessResponse_ReturnsMappedDivisionList()
        {
            // Arrange
            var resList = new List<DivisionRes>
            {
                new() { DivName = "DIV1", AgencyId = 1, AgencyName = "Agency One" },
                new() { DivName = "DIV2", AgencyId = 2, AgencyName = "Agency Two" }
            };
            var apiResponse = new ApiResponse<List<DivisionRes>> { Success = true, Data = resList };
            var expectedDto = ApiResponseDto<IEnumerable<DivisionDto>>.SuccessResponse(
                new List<DivisionDto>
                {
                    new() { DivName = "DIV1", AgencyId = 1 },
                    new() { DivName = "DIV2", AgencyId = 2 }
                }
            );

            _http.GetAsync<List<DivisionRes>>("api/v1/division").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<DivisionDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllDivisionsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count());
            await _http.Received(1).GetAsync<List<DivisionRes>>("api/v1/division");
            _mapper.Received(1).Map<ApiResponseDto<IEnumerable<DivisionDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllDivisionsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<DivisionRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<IEnumerable<DivisionDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<DivisionRes>>("api/v1/division").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<DivisionDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllDivisionsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAllDivisionsAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<List<DivisionRes>>("api/v1/division")
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAllDivisionsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve division data", error.Message);
        }

        #endregion

        #region GetAllDivisionsPagedAsync Tests

        [Fact]
        public async Task GetAllDivisionsPagedAsync_WithSuccessResponse_ReturnsMappedPagedDivisionList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList = new List<DivisionRes>
            {
                new() { DivName = "DIV1", AgencyId = 1, AgencyName = "Agency One" },
                new() { DivName = "DIV2", AgencyId = 2, AgencyName = "Agency Two" }
            };
            var apiResponse = new ApiResponse<List<DivisionRes>>
            {
                Success = true,
                Data = resList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };
            var expectedDto = ApiResponseDto<List<DivisionDto>>.SuccessResponse(
                new List<DivisionDto>
                {
                    new() { DivName = "DIV1", AgencyId = 1 },
                    new() { DivName = "DIV2", AgencyId = 2 }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );

            _http.GetAsync<List<DivisionRes>>(Arg.Is<string>(url => url.Contains("api/v1/division/paged")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<DivisionDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllDivisionsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<DivisionRes>>(Arg.Is<string>(url => url.Contains("api/v1/division/paged")));
            _mapper.Received(1).Map<ApiResponseDto<List<DivisionDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllDivisionsPagedAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<DivisionRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Error", Code = "ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<List<DivisionDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<DivisionRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<DivisionDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllDivisionsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAllDivisionsPagedAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<DivisionRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAllDivisionsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve paginated division data", error.Message);
        }

        #endregion

        #region GetDivisionByNameAsync Tests

        [Fact]
        public async Task GetDivisionByNameAsync_WithValidDivName_ReturnsMappedDivision()
        {
            // Arrange
            var divName = "DIV1";
            var divisionRes = new DivisionRes { DivName = divName, AgencyId = 1, AgencyName = "Agency One" };
            var apiResponse = new ApiResponse<DivisionRes> { Success = true, Data = divisionRes };
            var expectedDto = ApiResponseDto<DivisionDto>.SuccessResponse(
                new DivisionDto { DivName = divName, AgencyId = 1 }
            );

            _http.GetAsync<DivisionRes>($"api/v1/division/{divName}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<DivisionDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetDivisionByNameAsync(divName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(divName, result.Data?.DivName);
            await _http.Received(1).GetAsync<DivisionRes>($"api/v1/division/{divName}");
        }

        [Fact]
        public async Task GetDivisionByNameAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var divName = "DIV999";
            var apiResponse = new ApiResponse<DivisionRes>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<DivisionDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<DivisionRes>($"api/v1/division/{divName}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<DivisionDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetDivisionByNameAsync(divName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetDivisionByNameAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var divName = "DIV1";
            _http.GetAsync<DivisionRes>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetDivisionByNameAsync(divName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal($"Failed to retrieve division '{divName}'", error.Message);
        }

        #endregion

        #region CreateDivisionAsync Tests

        [Fact]
        public async Task CreateDivisionAsync_WithValidRequest_ReturnsMappedCreatedDivision()
        {
            // Arrange
            var divisionDto = new DivisionDto { DivName = "DIV3", AgencyId = 1, CentOverhead = 500.00m };
            var divisionReq = new DivisionReq { DivName = "DIV3", AgencyId = 1, CentOverhead = 500.00m };
            var divisionRes = new DivisionRes { DivName = "DIV3", AgencyId = 1, CentOverhead = 500.00m, AgencyName = "Agency One" };
            var apiResponse = new ApiResponse<DivisionRes> { Success = true, Data = divisionRes };
            var expectedDto = ApiResponseDto<DivisionDto>.SuccessResponse(divisionDto);

            _mapper.Map<DivisionReq>(divisionDto).Returns(divisionReq);
            _http.PostAsync<DivisionReq, DivisionRes>("api/v1/division", divisionReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<DivisionDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreateDivisionAsync(divisionDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("DIV3", result.Data?.DivName);
            await _http.Received(1).PostAsync<DivisionReq, DivisionRes>("api/v1/division", divisionReq);
        }

        [Fact]
        public async Task CreateDivisionAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var divisionDto = new DivisionDto { DivName = "DIV3", AgencyId = 1 };
            var divisionReq = new DivisionReq { DivName = "DIV3", AgencyId = 1 };
            var apiResponse = new ApiResponse<DivisionRes>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Validation failed", Code = "VALIDATION_ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<DivisionDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Validation failed", Code = "VALIDATION_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<DivisionReq>(divisionDto).Returns(divisionReq);
            _http.PostAsync<DivisionReq, DivisionRes>("api/v1/division", divisionReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<DivisionDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CreateDivisionAsync(divisionDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateDivisionAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var divisionDto = new DivisionDto { DivName = "DIV3", AgencyId = 1 };
            var divisionReq = new DivisionReq { DivName = "DIV3", AgencyId = 1 };

            _mapper.Map<DivisionReq>(divisionDto).Returns(divisionReq);
            _http.PostAsync<DivisionReq, DivisionRes>(Arg.Any<string>(), Arg.Any<DivisionReq>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.CreateDivisionAsync(divisionDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to create division", error.Message);
        }

        #endregion

        #region UpdateDivisionAsync Tests

        [Fact]
        public async Task UpdateDivisionAsync_WithValidRequest_ReturnsMappedUpdatedDivision()
        {
            // Arrange
            var divName = "DIV1";
            var divisionDto = new DivisionDto { DivName = divName, AgencyId = 2, CentOverhead = 750.00m };
            var divisionReq = new DivisionReq { DivName = divName, AgencyId = 2, CentOverhead = 750.00m };
            var divisionRes = new DivisionRes { DivName = divName, AgencyId = 2, CentOverhead = 750.00m, AgencyName = "Agency Two" };
            var apiResponse = new ApiResponse<DivisionRes> { Success = true, Data = divisionRes };
            var expectedDto = ApiResponseDto<DivisionDto>.SuccessResponse(divisionDto);

            _mapper.Map<DivisionReq>(divisionDto).Returns(divisionReq);
            _http.PutAsync<DivisionReq, DivisionRes>($"api/v1/division/{divName}", divisionReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<DivisionDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateDivisionAsync(divName, divisionDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(divName, result.Data?.DivName);
            await _http.Received(1).PutAsync<DivisionReq, DivisionRes>($"api/v1/division/{divName}", divisionReq);
        }

        [Fact]
        public async Task UpdateDivisionAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var divName = "DIV999";
            var divisionDto = new DivisionDto { DivName = divName, AgencyId = 1 };
            var divisionReq = new DivisionReq { DivName = divName, AgencyId = 1 };
            var apiResponse = new ApiResponse<DivisionRes>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<DivisionDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<DivisionReq>(divisionDto).Returns(divisionReq);
            _http.PutAsync<DivisionReq, DivisionRes>($"api/v1/division/{divName}", divisionReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<DivisionDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateDivisionAsync(divName, divisionDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateDivisionAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var divName = "DIV1";
            var divisionDto = new DivisionDto { DivName = divName, AgencyId = 1 };
            var divisionReq = new DivisionReq { DivName = divName, AgencyId = 1 };

            _mapper.Map<DivisionReq>(divisionDto).Returns(divisionReq);
            _http.PutAsync<DivisionReq, DivisionRes>(Arg.Any<string>(), Arg.Any<DivisionReq>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.UpdateDivisionAsync(divName, divisionDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal($"Failed to update division '{divName}'", error.Message);
        }

        #endregion

        #region DeleteDivisionAsync Tests

        [Fact]
        public async Task DeleteDivisionAsync_WithValidDivName_ReturnsSuccess()
        {
            // Arrange
            var divName = "DIV1";
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>($"api/v1/division/{divName}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteDivisionAsync(divName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool?>($"api/v1/division/{divName}");
        }

        [Fact]
        public async Task DeleteDivisionAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var divName = "DIV999";
            var apiResponse = new ApiResponse<bool?>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>($"api/v1/division/{divName}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteDivisionAsync(divName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task DeleteDivisionAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var divName = "DIV1";
            _http.DeleteAsync<bool?>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.DeleteDivisionAsync(divName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal($"Failed to delete division '{divName}'", error.Message);
        }

        #endregion
    }
}
