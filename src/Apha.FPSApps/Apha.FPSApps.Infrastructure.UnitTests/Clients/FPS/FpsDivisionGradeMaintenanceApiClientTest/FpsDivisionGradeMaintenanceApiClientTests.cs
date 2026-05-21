using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsDivisionGradeMaintenanceApiClientTest
{
    public class FpsDivisionGradeMaintenanceApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsDivisionGradeMaintenanceApiClient _client;

        public FpsDivisionGradeMaintenanceApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsDivisionGradeMaintenanceApiClient(_http, _mapper);
        }

        private static DivisionGradeRes BuildRes(string code = "A-VSD") =>
            new() { DivisionGradeCode = code, GradeCode = "A", Division = "VSD", ChargeRate = 100m };

        private static DivisionGradeMaintenanceDto BuildDto(string code = "A-VSD") =>
            new() { DivisionGradeCode = code, GradeCode = "A", Division = "VSD", ChargeRate = 100m };

        private static ApiResponse<T> SuccessApiResponse<T>(T data) =>
            new() { Success = true, Data = data };

        private static ApiResponse<T> FailureApiResponse<T>() =>
            new()
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Error", Code = "ERROR" } }
            };

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenHttpIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new FpsDivisionGradeMaintenanceApiClient(null!, _mapper));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new FpsDivisionGradeMaintenanceApiClient(_http, null!));
        }

        #endregion

        #region GetAllPagedAsync Tests

        [Fact]
        public async Task GetAllPagedAsync_WithSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList = new List<DivisionGradeRes> { BuildRes() };
            var apiResponse = SuccessApiResponse(resList);
            var expected = ApiResponseDto<List<DivisionGradeMaintenanceDto>>.SuccessResponse(
                new List<DivisionGradeMaintenanceDto> { BuildDto() },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });

            _http.GetAsync<List<DivisionGradeRes>>(Arg.Is<string>(u => u.Contains("divisiongrademaintenance/paged")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<DivisionGradeMaintenanceDto>>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.GetAllPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetAllPagedAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = FailureApiResponse<List<DivisionGradeRes>>();
            var mappedResponse = new ApiResponseDto<List<DivisionGradeMaintenanceDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<DivisionGradeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<DivisionGradeMaintenanceDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAllPagedAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<DivisionGradeRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAllPagedAsync(query);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to retrieve paginated division grade data", error.Message);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var res = BuildRes("A-VSD");
            var apiResponse = SuccessApiResponse(res);
            var expected = ApiResponseDto<DivisionGradeMaintenanceDto>.SuccessResponse(BuildDto("A-VSD"));

            _http.GetAsync<DivisionGradeRes>(Arg.Is<string>(u => u.Contains("A-VSD"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<DivisionGradeMaintenanceDto>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.GetByIdAsync("A-VSD");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("A-VSD", result.Data!.DivisionGradeCode);
        }

        [Fact]
        public async Task GetByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = FailureApiResponse<DivisionGradeRes>();
            var mappedResponse = new ApiResponseDto<DivisionGradeMaintenanceDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<DivisionGradeRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<DivisionGradeMaintenanceDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetByIdAsync("NOTEXIST");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetByIdAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<DivisionGradeRes>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetByIdAsync("A-VSD");

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Contains("A-VSD", error.Message);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var dto = BuildDto("A-VSD");
            var req = new DivisionGradeReq { DivisionGradeCode = "A-VSD", GradeCode = "A", Division = "VSD" };
            var res = BuildRes("A-VSD");
            var apiResponse = SuccessApiResponse(res);
            var expected = ApiResponseDto<DivisionGradeMaintenanceDto>.SuccessResponse(dto);

            _mapper.Map<DivisionGradeReq>(dto).Returns(req);
            _http.PostAsync<DivisionGradeReq, DivisionGradeRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<DivisionGradeMaintenanceDto>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task CreateAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = BuildDto();
            var req = new DivisionGradeReq { DivisionGradeCode = "A-VSD" };
            var apiResponse = FailureApiResponse<DivisionGradeRes>();
            var mappedResponse = new ApiResponseDto<DivisionGradeMaintenanceDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Failed", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<DivisionGradeReq>(dto).Returns(req);
            _http.PostAsync<DivisionGradeReq, DivisionGradeRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<DivisionGradeMaintenanceDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var dto = BuildDto();
            var req = new DivisionGradeReq { DivisionGradeCode = "A-VSD" };

            _mapper.Map<DivisionGradeReq>(dto).Returns(req);
            _http.PostAsync<DivisionGradeReq, DivisionGradeRes>(Arg.Any<string>(), req)
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Equal("Failed to create division grade", error.Message);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var dto = BuildDto("A-VSD");
            var req = new DivisionGradeReq { DivisionGradeCode = "A-VSD", GradeCode = "A", Division = "VSD" };
            var res = BuildRes("A-VSD");
            var apiResponse = SuccessApiResponse(res);
            var expected = ApiResponseDto<DivisionGradeMaintenanceDto>.SuccessResponse(dto);

            _mapper.Map<DivisionGradeReq>(dto).Returns(req);
            _http.PutAsync<DivisionGradeReq, DivisionGradeRes>(Arg.Is<string>(u => u.Contains("A-VSD")), req)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<DivisionGradeMaintenanceDto>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.UpdateAsync("A-VSD", dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task UpdateAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            var dto = BuildDto();
            var req = new DivisionGradeReq { DivisionGradeCode = "A-VSD" };

            _mapper.Map<DivisionGradeReq>(dto).Returns(req);
            _http.PutAsync<DivisionGradeReq, DivisionGradeRes>(Arg.Any<string>(), req)
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.UpdateAsync("A-VSD", dto);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Contains("A-VSD", error.Message);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var apiResponse = SuccessApiResponse<bool?>(true);
            var expected = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(Arg.Is<string>(u => u.Contains("A-VSD"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.DeleteAsync("A-VSD");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task DeleteAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = FailureApiResponse<bool?>();
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteAsync("NOTEXIST");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task DeleteAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            _http.DeleteAsync<bool?>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.DeleteAsync("A-VSD");

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
            Assert.Contains("A-VSD", error.Message);
        }

        #endregion

        #region GetAllGradeCodesAsync Tests

        [Fact]
        public async Task GetAllGradeCodesAsync_WithSuccessResponse_ReturnsList()
        {
            // Arrange
            var gradeCodes = new List<string> { "A", "B", "C" };
            var apiResponse = SuccessApiResponse(gradeCodes);
            var expected = ApiResponseDto<List<string>>.SuccessResponse(gradeCodes);

            _http.GetAsync<List<string>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.GetAllGradeCodesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data!.Count);
        }

        [Fact]
        public async Task GetAllGradeCodesAsync_WhenExceptionThrown_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<List<string>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAllGradeCodesAsync();

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors!);
            Assert.Equal("INTERNAL_ERROR", error.Code);
        }

        #endregion
    }
}
