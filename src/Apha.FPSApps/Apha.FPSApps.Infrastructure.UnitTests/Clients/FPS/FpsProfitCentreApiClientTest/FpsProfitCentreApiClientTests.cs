using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
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

        #region GetProfitCentresAsync Tests

        [Fact]
        public async Task GetProfitCentresAsync_WithSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var resList = new List<ProfitCentreRes>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Profit Centre One" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Profit Centre Two" }
            };
            var apiResponse = new ApiResponse<List<ProfitCentreRes>> { Success = true, Data = resList };
            var dtoList     = new List<ProfitCentreDto>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Profit Centre One" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Profit Centre Two" }
            };
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
            var apiResponse = new ApiResponse<List<ProfitCentreRes>>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "API Error", Code = "ERROR" } }
            };
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
            var apiResponse = new ApiResponse<List<ProfitCentreRes>> { Success = true, Data = new List<ProfitCentreRes>() };
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
            var apiResponse = new ApiResponse<List<ProfitCentreRes>> { Success = true, Data = new List<ProfitCentreRes>() };
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

        #region GetAllProfitCentresAsync Tests

        [Fact]
        public async Task GetAllProfitCentresAsync_WithSuccessResponse_ReturnsMappedEnumerable()
        {
            // Arrange
            var resList    = new List<ProfitCentreRes> { new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One" } };
            var apiResponse = new ApiResponse<IEnumerable<ProfitCentreRes>> { Success = true, Data = resList };
            var dtoList    = new List<ProfitCentreDto> { new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One" } };
            var expectedDto = ApiResponseDto<IEnumerable<ProfitCentreDto>>.SuccessResponse(dtoList);

            _http.GetAsync<IEnumerable<ProfitCentreRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<ProfitCentreDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllProfitCentresAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<IEnumerable<ProfitCentreRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetAllProfitCentresAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<IEnumerable<ProfitCentreRes>>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "API Error", Code = "ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<IEnumerable<ProfitCentreDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "API Error", Code = "ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<IEnumerable<ProfitCentreRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<ProfitCentreDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllProfitCentresAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region GetProfitCentreByIdAsync Tests

        [Fact]
        public async Task GetProfitCentreByIdAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var res         = new ProfitCentreRes { ProfitCentreId = "PC01", ProfitCentreName = "Centre One" };
            var apiResponse = new ApiResponse<ProfitCentreRes> { Success = true, Data = res };
            var dto         = new ProfitCentreDto { ProfitCentreId = "PC01", ProfitCentreName = "Centre One" };
            var expectedDto = ApiResponseDto<ProfitCentreDto>.SuccessResponse(dto);

            _http.GetAsync<ProfitCentreRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProfitCentreDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProfitCentreByIdAsync("PC01");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("PC01", result.Data?.ProfitCentreId);
        }

        [Fact]
        public async Task GetProfitCentreByIdAsync_UsesEscapedProfitCentreInUrl()
        {
            // Arrange
            var apiResponse = new ApiResponse<ProfitCentreRes> { Success = true, Data = new ProfitCentreRes() };
            var expectedDto = ApiResponseDto<ProfitCentreDto>.SuccessResponse(new ProfitCentreDto());

            _http.GetAsync<ProfitCentreRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProfitCentreDto>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetProfitCentreByIdAsync("All Admin (support) Departments");

            // Assert
            await _http.Received(1).GetAsync<ProfitCentreRes>(
                Arg.Is<string>(url => url.Contains("All%20Admin%20%28support%29%20Departments")));
        }

        [Fact]
        public async Task GetProfitCentreByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<ProfitCentreRes>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Not found", Code = "404" } }
            };
            var mappedResponse = new ApiResponseDto<ProfitCentreDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "404" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<ProfitCentreRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProfitCentreDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetProfitCentreByIdAsync("PC_MISSING");

            // Assert
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region UpdateProfitCentreSettingsAsync Tests

        [Fact]
        public async Task UpdateProfitCentreSettingsAsync_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.PatchAsync<UpdateProfitCentreSettingsReq, bool?>(Arg.Any<string>(), Arg.Any<UpdateProfitCentreSettingsReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateProfitCentreSettingsAsync("PC01", -1, -1, 1);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).PatchAsync<UpdateProfitCentreSettingsReq, bool?>(
                Arg.Any<string>(), Arg.Any<UpdateProfitCentreSettingsReq>());
        }

        [Fact]
        public async Task UpdateProfitCentreSettingsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Update failed", Code = "ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _http.PatchAsync<UpdateProfitCentreSettingsReq, bool?>(Arg.Any<string>(), Arg.Any<UpdateProfitCentreSettingsReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateProfitCentreSettingsAsync("PC01", -1, -1, 1);

            // Assert
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion
    }
}
