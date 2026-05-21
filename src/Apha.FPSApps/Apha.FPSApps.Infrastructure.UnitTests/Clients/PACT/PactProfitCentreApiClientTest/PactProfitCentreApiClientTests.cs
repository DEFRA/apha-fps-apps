using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.Common.Constants;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactProfitCentreApiClientTest
{
    public class PactProfitCentreApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactProfitCentreApiClient _client;

        public PactProfitCentreApiClientTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PactProfitCentreApiClient(_http, _mapper);
        }

        #region GetAllProfitCentresAsync Tests

        [Fact]
        public async Task GetAllProfitCentresAsync_WithSuccessResponse_ReturnsMappedProfitCentreList()
        {
            // Arrange
            var resList = new List<ProfitCentreSettingsRes>
            {
                new() { ProfitCentre = "PC001", ProfitCentreName = "Centre One" },
                new() { ProfitCentre = "PC002", ProfitCentreName = "Centre Two" }
            };
            var apiResponse = new ApiResponse<IEnumerable<ProfitCentreSettingsRes>> { Success = true, Data = resList };
            var expectedDto = ApiResponseDto<IEnumerable<ProfitCentreSettingsDto>>.SuccessResponse(
                new List<ProfitCentreSettingsDto>
                {
                    new() { ProfitCentre = "PC001", ProfitCentreName = "Centre One" },
                    new() { ProfitCentre = "PC002", ProfitCentreName = "Centre Two" }
                });

            _http.GetAsync<IEnumerable<ProfitCentreSettingsRes>>(PactApiEndpoints.GetAllProfitCentres).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<ProfitCentreSettingsDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllProfitCentresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count());
            await _http.Received(1).GetAsync<IEnumerable<ProfitCentreSettingsRes>>(PactApiEndpoints.GetAllProfitCentres);
        }

        [Fact]
        public async Task GetAllProfitCentresAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var apiResponse = new ApiResponse<IEnumerable<ProfitCentreSettingsRes>>
            {
                Success = true,
                Data = Enumerable.Empty<ProfitCentreSettingsRes>()
            };
            var expectedDto = ApiResponseDto<IEnumerable<ProfitCentreSettingsDto>>.SuccessResponse(
                Enumerable.Empty<ProfitCentreSettingsDto>());

            _http.GetAsync<IEnumerable<ProfitCentreSettingsRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<ProfitCentreSettingsDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllProfitCentresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllProfitCentresAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<IEnumerable<ProfitCentreSettingsRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<IEnumerable<ProfitCentreSettingsDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<IEnumerable<ProfitCentreSettingsRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<ProfitCentreSettingsDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllProfitCentresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetProfitCentreSettingsAsync Tests

        [Fact]
        public async Task GetProfitCentreSettingsAsync_WithSuccessResponse_ReturnsMappedSettings()
        {
            // Arrange
            const string profitCentre = "PC001";
            var settingsRes = new ProfitCentreSettingsRes
            {
                ProfitCentre = profitCentre,
                Timesheet = -1,
                Outputsheet = 0,
                TimesheetLayout = 1
            };
            var apiResponse = new ApiResponse<ProfitCentreSettingsRes> { Success = true, Data = settingsRes };
            var expectedDto = ApiResponseDto<ProfitCentreSettingsDto>.SuccessResponse(
                new ProfitCentreSettingsDto
                {
                    ProfitCentre = profitCentre,
                    Timesheet = -1,
                    Outputsheet = 0,
                    TimesheetLayout = 1
                });

            _http.GetAsync<ProfitCentreSettingsRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProfitCentreSettingsDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProfitCentreSettingsAsync(profitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(profitCentre, result.Data?.ProfitCentre);
            await _http.Received(1).GetAsync<ProfitCentreSettingsRes>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetProfitCentreSettingsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<ProfitCentreSettingsRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<ProfitCentreSettingsDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<ProfitCentreSettingsRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProfitCentreSettingsDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetProfitCentreSettingsAsync("PC_MISSING");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetProfitCentreSettingsAsync_EncodesSpecialCharactersInUrl()
        {
            // Arrange
            const string profitCentre = "PC 01/A";
            var apiResponse = new ApiResponse<ProfitCentreSettingsRes> { Success = true, Data = new ProfitCentreSettingsRes() };
            var expectedDto = ApiResponseDto<ProfitCentreSettingsDto>.SuccessResponse(new ProfitCentreSettingsDto());

            _http.GetAsync<ProfitCentreSettingsRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProfitCentreSettingsDto>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetProfitCentreSettingsAsync(profitCentre);

            // Assert: URL must not contain raw spaces or slashes from the profit centre value
            await _http.Received(1).GetAsync<ProfitCentreSettingsRes>(
                Arg.Is<string>(url => !url.Contains(" ") && url.Contains("PC")));
        }

        #endregion

        #region UpdateProfitCentreSettingsAsync Tests

        [Fact]
        public async Task UpdateProfitCentreSettingsAsync_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.PatchAsync<UpdateProfitCentreSettingsReq, bool?>(
                PactApiEndpoints.PatchProfitCentreSettings, Arg.Any<UpdateProfitCentreSettingsReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateProfitCentreSettingsAsync("PC001", -1, 0, 1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).PatchAsync<UpdateProfitCentreSettingsReq, bool?>(
                PactApiEndpoints.PatchProfitCentreSettings, Arg.Any<UpdateProfitCentreSettingsReq>());
        }

        [Fact]
        public async Task UpdateProfitCentreSettingsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Update Failed", Code = "UPDATE_ERROR" } };
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Update Failed", Code = "UPDATE_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.PatchAsync<UpdateProfitCentreSettingsReq, bool?>(Arg.Any<string>(), Arg.Any<UpdateProfitCentreSettingsReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateProfitCentreSettingsAsync("PC001", 0, 0, 2);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task UpdateProfitCentreSettingsAsync_SendsCorrectRequestPayload()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            UpdateProfitCentreSettingsReq? capturedRequest = null;
            _http.PatchAsync<UpdateProfitCentreSettingsReq, bool?>(
                Arg.Any<string>(),
                Arg.Do<UpdateProfitCentreSettingsReq>(r => capturedRequest = r))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.UpdateProfitCentreSettingsAsync("PC001", -1, -1, 2);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal("PC001", capturedRequest!.ProfitCentre);
            Assert.Equal(-1, capturedRequest.Timesheet);
            Assert.Equal(-1, capturedRequest.Outputsheet);
            Assert.Equal((short)2, capturedRequest.TimesheetLayout);
        }

        #endregion
    }
}
