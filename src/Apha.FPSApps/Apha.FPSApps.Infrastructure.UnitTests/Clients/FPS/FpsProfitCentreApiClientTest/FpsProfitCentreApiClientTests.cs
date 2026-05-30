using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsProfitCentreApiClientTest
{
    public class FpsProfitCentreApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsProfitCentreApiClient _client;

        public FpsProfitCentreApiClientTests()
        {
            _http   = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsProfitCentreApiClient(_http, _mapper);
        }

        private static ProfitCentreRes BuildRes(string id = "PC01") =>
            new() { ProfitCentreId = id, ProfitCentreName = "Centre One", Division = "DIV1" };

        private static ProfitCentreDto BuildDto(string id = "PC01") =>
            new() { ProfitCentreId = id, ProfitCentreName = "Centre One", Division = "DIV1" };

        private static ApiResponse<T> SuccessApiResponse<T>(T data) =>
            new() { Success = true, Data = data };

        private static ApiResponse<T> FailureApiResponse<T>() =>
            new() { Success = false, Errors = new List<ApiError> { new() { Message = "Error", Code = "ERROR" } } };

        #region GetProfitCentresAsync Tests

        [Fact]
        public async Task GetProfitCentresAsync_WithSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var resList     = new List<ProfitCentreRes> { BuildRes("PC01"), BuildRes("PC02") };
            var apiResponse = SuccessApiResponse(resList);
            var dtoList     = new List<ProfitCentreDto> { BuildDto("PC01"), BuildDto("PC02") };
            var expectedDto = ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(dtoList);

            _http.GetAsync<List<ProfitCentreRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProfitCentresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<ProfitCentreRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetProfitCentresAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = FailureApiResponse<List<ProfitCentreRes>>();
            var mappedResponse = new ApiResponseDto<List<ProfitCentreDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "API Error", Code = "ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<ProfitCentreRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetProfitCentresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        [Fact]
        public async Task GetProfitCentresAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var apiResponse = SuccessApiResponse(new List<ProfitCentreRes>());
            var expectedDto = ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(new List<ProfitCentreDto>());

            _http.GetAsync<List<ProfitCentreRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProfitCentresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetProfitCentresAsync_UsesCorrectEndpoint()
        {
            // Arrange
            var apiResponse = SuccessApiResponse(new List<ProfitCentreRes>());
            var expectedDto = ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(new List<ProfitCentreDto>());

            _http.GetAsync<List<ProfitCentreRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetProfitCentresAsync();

            // Assert
            await _http.Received(1).GetAsync<List<ProfitCentreRes>>(
                Arg.Is<string>(url => url.Contains("profitcentres")));
        }

        #endregion

        #region GetAllProfitCentresPagedAsync Tests

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_WithSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var query       = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList     = new List<ProfitCentreRes> { BuildRes() };
            var apiResponse = SuccessApiResponse(resList);
            var expected    = ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(
                new List<ProfitCentreDto> { BuildDto() },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });

            _http.GetAsync<List<ProfitCentreRes>>(Arg.Is<string>(u => u.Contains("paged")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.GetAllProfitCentresPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query       = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = FailureApiResponse<List<ProfitCentreRes>>();
            var mappedResponse = new ApiResponseDto<List<ProfitCentreDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<ProfitCentreRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllProfitCentresPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetProfitCentreByIdAsync Tests

        [Fact]
        public async Task GetProfitCentreByIdAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var res         = BuildRes("PC01");
            var apiResponse = SuccessApiResponse(res);
            var expected    = ApiResponseDto<ProfitCentreDto>.SuccessResponse(BuildDto("PC01"));

            _http.GetAsync<ProfitCentreRes>(Arg.Is<string>(u => u.Contains("PC01"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProfitCentreDto>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.GetProfitCentreByIdAsync("PC01");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("PC01", result.Data!.ProfitCentreId);
        }

        [Fact]
        public async Task GetProfitCentreByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = FailureApiResponse<ProfitCentreRes>();
            var mappedResponse = new ApiResponseDto<ProfitCentreDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<ProfitCentreRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProfitCentreDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetProfitCentreByIdAsync("NOTEXIST");

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region CreateProfitCentreAsync Tests

        [Fact]
        public async Task CreateProfitCentreAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var dto         = BuildDto("PC01");
            var req         = new ProfitCentreReq { ProfitCentreId = "PC01", ProfitCentreName = "Centre One", Division = "DIV1" };
            var res         = BuildRes("PC01");
            var apiResponse = SuccessApiResponse(res);
            var expected    = ApiResponseDto<ProfitCentreDto>.SuccessResponse(dto);

            _mapper.Map<ProfitCentreReq>(dto).Returns(req);
            _http.PostAsync<ProfitCentreReq, ProfitCentreRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProfitCentreDto>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.CreateProfitCentreAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task CreateProfitCentreAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto         = BuildDto();
            var req         = new ProfitCentreReq { ProfitCentreId = "PC01" };
            var apiResponse = FailureApiResponse<ProfitCentreRes>();
            var mappedResponse = new ApiResponseDto<ProfitCentreDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Failed", Code = "ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _mapper.Map<ProfitCentreReq>(dto).Returns(req);
            _http.PostAsync<ProfitCentreReq, ProfitCentreRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProfitCentreDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CreateProfitCentreAsync(dto);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region UpdateProfitCentreAsync Tests

        [Fact]
        public async Task UpdateProfitCentreAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var dto         = BuildDto("PC01");
            var req         = new ProfitCentreReq { ProfitCentreId = "PC01", ProfitCentreName = "Centre One", Division = "DIV1" };
            var res         = BuildRes("PC01");
            var apiResponse = SuccessApiResponse(res);
            var expected    = ApiResponseDto<ProfitCentreDto>.SuccessResponse(dto);

            _mapper.Map<ProfitCentreReq>(dto).Returns(req);
            _http.PutAsync<ProfitCentreReq, ProfitCentreRes>(Arg.Is<string>(u => u.Contains("PC01")), req)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProfitCentreDto>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.UpdateProfitCentreAsync("PC01", dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task UpdateProfitCentreAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto         = BuildDto();
            var req         = new ProfitCentreReq { ProfitCentreId = "PC01" };
            var apiResponse = FailureApiResponse<ProfitCentreRes>();
            var mappedResponse = new ApiResponseDto<ProfitCentreDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Failed", Code = "ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _mapper.Map<ProfitCentreReq>(dto).Returns(req);
            _http.PutAsync<ProfitCentreReq, ProfitCentreRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProfitCentreDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateProfitCentreAsync("PC01", dto);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteProfitCentreAsync Tests

        [Fact]
        public async Task DeleteProfitCentreAsync_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var apiResponse = SuccessApiResponse<bool?>(true);
            var expected    = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(Arg.Is<string>(u => u.Contains("PC01"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.DeleteProfitCentreAsync("PC01");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task DeleteProfitCentreAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = FailureApiResponse<bool?>();
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteProfitCentreAsync("NOTEXIST");

            // Assert
            Assert.False(result.Success);
        }

        #endregion

            }
        }
