using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsTotalBusinessOverheadsApiClientTest
{
    public class FpsTotalBusinessOverheadsApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsTotalBusinessOverheadsApiClient _client;

        public FpsTotalBusinessOverheadsApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsTotalBusinessOverheadsApiClient(_http, _mapper);
        }

        private static TotalBusinessOverheadsRes BuildRes(decimal? overheads = 1000000m, int fpsYear = 2025) =>
            new() { TotalBusinessOverheads = overheads, FpsYear = fpsYear };

        private static TotalBusinessOverheadsDto BuildDto(decimal? overheads = 1000000m, int fpsYear = 2025) =>
            new() { TotalBusinessOverheads = overheads, FpsYear = fpsYear };

        private static TotalBusinessOverheadsReq BuildReq(decimal? overheads = 1000000m) =>
            new() { TotalBusinessOverheads = overheads };

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenHttpIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new FpsTotalBusinessOverheadsApiClient(null!, _mapper));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new FpsTotalBusinessOverheadsApiClient(_http, null!));
        }

        #endregion

        #region GetAsync Tests

        [Fact]
        public async Task GetAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var res = BuildRes();
            var apiResponse = new Common.Contracts.ApiResponse<TotalBusinessOverheadsRes>
            {
                Success = true,
                Data = res
            };
            var expected = ApiResponseDto<TotalBusinessOverheadsDto>.SuccessResponse(BuildDto());

            _http.GetAsync<TotalBusinessOverheadsRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TotalBusinessOverheadsDto>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.GetAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1000000m, result.Data.TotalBusinessOverheads);
            Assert.Equal(2025, result.Data.FpsYear);
        }

        [Fact]
        public async Task GetAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new Common.Contracts.ApiResponse<TotalBusinessOverheadsRes>
            {
                Success = false,
                Errors = new List<Common.Contracts.ApiError> { new() { Message = "Error", Code = "ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<TotalBusinessOverheadsDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<TotalBusinessOverheadsRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TotalBusinessOverheadsDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetAsync_WhenHttpThrows_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            _http.GetAsync<TotalBusinessOverheadsRes>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "INTERNAL_ERROR");
            Assert.Contains(result.Errors, e => e.Message == "Failed to retrieve Total Business Overheads");
        }

        [Fact]
        public async Task GetAsync_CallsHttpExecutorWithCorrectEndpoint()
        {
            // Arrange
            var apiResponse = new Common.Contracts.ApiResponse<TotalBusinessOverheadsRes>
            {
                Success = true,
                Data = BuildRes()
            };

            _http.GetAsync<TotalBusinessOverheadsRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TotalBusinessOverheadsDto>>(Arg.Any<Common.Contracts.ApiResponse<TotalBusinessOverheadsRes>>())
                .Returns(ApiResponseDto<TotalBusinessOverheadsDto>.SuccessResponse(BuildDto()));

            // Act
            await _client.GetAsync();

            // Assert
            await _http.Received(1).GetAsync<TotalBusinessOverheadsRes>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetAsync_WithNullData_ReturnsSuccessWithNullData()
        {
            // Arrange
            var apiResponse = new Common.Contracts.ApiResponse<TotalBusinessOverheadsRes>
            {
                Success = true,
                Data = null
            };
            var mappedResponse = new ApiResponseDto<TotalBusinessOverheadsDto>
            {
                Success = true,
                Data = null
            };

            _http.GetAsync<TotalBusinessOverheadsRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TotalBusinessOverheadsDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.Data);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var dto = BuildDto(1500000m);
            var req = BuildReq(1500000m);
            var res = BuildRes(1500000m);
            var apiResponse = new Common.Contracts.ApiResponse<TotalBusinessOverheadsRes>
            {
                Success = true,
                Data = res
            };
            var expected = ApiResponseDto<TotalBusinessOverheadsDto>.SuccessResponse(dto);

            _mapper.Map<TotalBusinessOverheadsReq>(dto).Returns(req);
            _http.PutAsync<TotalBusinessOverheadsReq, TotalBusinessOverheadsRes>(Arg.Any<string>(), req)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TotalBusinessOverheadsDto>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1500000m, result.Data.TotalBusinessOverheads);
        }

        [Fact]
        public async Task UpdateAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = BuildDto();
            var req = BuildReq();
            var apiResponse = new Common.Contracts.ApiResponse<TotalBusinessOverheadsRes>
            {
                Success = false,
                Errors = new List<Common.Contracts.ApiError> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<TotalBusinessOverheadsDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<TotalBusinessOverheadsReq>(dto).Returns(req);
            _http.PutAsync<TotalBusinessOverheadsReq, TotalBusinessOverheadsRes>(Arg.Any<string>(), req)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TotalBusinessOverheadsDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task UpdateAsync_WhenHttpThrows_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            var dto = BuildDto();
            var req = BuildReq();

            _mapper.Map<TotalBusinessOverheadsReq>(dto).Returns(req);
            _http.PutAsync<TotalBusinessOverheadsReq, TotalBusinessOverheadsRes>(Arg.Any<string>(), req)
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.UpdateAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "INTERNAL_ERROR");
            Assert.Contains(result.Errors, e => e.Message == "Failed to update Total Business Overheads");
        }

        [Fact]
        public async Task UpdateAsync_CallsMapperToConvertDtoToReq()
        {
            // Arrange
            var dto = BuildDto();
            var req = BuildReq();
            var apiResponse = new Common.Contracts.ApiResponse<TotalBusinessOverheadsRes>
            {
                Success = true,
                Data = BuildRes()
            };

            _mapper.Map<TotalBusinessOverheadsReq>(dto).Returns(req);
            _http.PutAsync<TotalBusinessOverheadsReq, TotalBusinessOverheadsRes>(Arg.Any<string>(), req)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TotalBusinessOverheadsDto>>(Arg.Any<Common.Contracts.ApiResponse<TotalBusinessOverheadsRes>>())
                .Returns(ApiResponseDto<TotalBusinessOverheadsDto>.SuccessResponse(BuildDto()));

            // Act
            await _client.UpdateAsync(dto);

            // Assert
            _mapper.Received(1).Map<TotalBusinessOverheadsReq>(dto);
        }

        [Fact]
        public async Task UpdateAsync_CallsHttpPutWithCorrectEndpointAndRequest()
        {
            // Arrange
            var dto = BuildDto();
            var req = BuildReq();
            var apiResponse = new Common.Contracts.ApiResponse<TotalBusinessOverheadsRes>
            {
                Success = true,
                Data = BuildRes()
            };

            _mapper.Map<TotalBusinessOverheadsReq>(dto).Returns(req);
            _http.PutAsync<TotalBusinessOverheadsReq, TotalBusinessOverheadsRes>(Arg.Any<string>(), req)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TotalBusinessOverheadsDto>>(Arg.Any<Common.Contracts.ApiResponse<TotalBusinessOverheadsRes>>())
                .Returns(ApiResponseDto<TotalBusinessOverheadsDto>.SuccessResponse(BuildDto()));

            // Act
            await _client.UpdateAsync(dto);

            // Assert
            await _http.Received(1).PutAsync<TotalBusinessOverheadsReq, TotalBusinessOverheadsRes>(
                Arg.Any<string>(),
                Arg.Is<TotalBusinessOverheadsReq>(r => r.TotalBusinessOverheads == req.TotalBusinessOverheads));
        }

        [Fact]
        public async Task UpdateAsync_WithNullOverheads_ProcessesSuccessfully()
        {
            // Arrange
            var dto = BuildDto(null);
            var req = BuildReq(null);
            var res = BuildRes(null);
            var apiResponse = new Common.Contracts.ApiResponse<TotalBusinessOverheadsRes>
            {
                Success = true,
                Data = res
            };
            var expected = ApiResponseDto<TotalBusinessOverheadsDto>.SuccessResponse(BuildDto(null));

            _mapper.Map<TotalBusinessOverheadsReq>(dto).Returns(req);
            _http.PutAsync<TotalBusinessOverheadsReq, TotalBusinessOverheadsRes>(Arg.Any<string>(), req)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TotalBusinessOverheadsDto>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.UpdateAsync(dto);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task UpdateAsync_WithZeroOverheads_ProcessesSuccessfully()
        {
            // Arrange
            var dto = BuildDto(0m);
            var req = BuildReq(0m);
            var res = BuildRes(0m);
            var apiResponse = new Common.Contracts.ApiResponse<TotalBusinessOverheadsRes>
            {
                Success = true,
                Data = res
            };
            var expected = ApiResponseDto<TotalBusinessOverheadsDto>.SuccessResponse(BuildDto(0m));

            _mapper.Map<TotalBusinessOverheadsReq>(dto).Returns(req);
            _http.PutAsync<TotalBusinessOverheadsReq, TotalBusinessOverheadsRes>(Arg.Any<string>(), req)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TotalBusinessOverheadsDto>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.UpdateAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(0m, result.Data!.TotalBusinessOverheads);
        }

        #endregion
    }
}
