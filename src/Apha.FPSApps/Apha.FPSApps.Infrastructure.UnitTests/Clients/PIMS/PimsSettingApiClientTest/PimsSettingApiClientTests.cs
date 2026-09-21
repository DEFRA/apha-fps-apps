using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsSettingApiClientTest
{
    public class PimsSettingApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsSettingApiClient _client;

        public PimsSettingApiClientTests()
        {
            _http = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsSettingApiClient(_http, _mapper);
        }

        private static ApiResponse<T> SuccessApiResponse<T>(T data) =>
            new ApiResponse<T> { Success = true, Data = data };

        private static ApiResponse<T> FailureApiResponse<T>() =>
            new ApiResponse<T>
            {
                Success = false,
                Errors = new List<ApiError> { new ApiError { Code = "ERR", Message = "API error" } }
            };

        private static ApiResponseDto<T> SuccessDto<T>(T data) => ApiResponseDto<T>.SuccessResponse(data);

        private static ApiResponseDto<T> FailureDto<T>() =>
            ApiResponseDto<T>.FailureResponse(
                new List<ApiErrorDto> { new ApiErrorDto { Code = "ERR", Message = "Error" } },
                new ApiMetaDto());

        private static SettingRes MakeRes(string id = "setting1", string? setting = "TestSetting") =>
            new SettingRes { Id = id, Setting = setting, UserUpdateable = true };

        private static SettingDto MakeDto(string id = "setting1", string? settingValue = "TestSetting") =>
            new SettingDto { Id = id, SettingValue = settingValue, Userupdateable = true };

        #region GetAllSettingsAsync

        [Fact]
        public async Task GetAllSettingsAsync_WhenSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var settingList = new List<SettingRes> { MakeRes("setting1"), MakeRes("setting2") };
            var apiResponse = SuccessApiResponse(settingList);
            var mappedDto = SuccessDto(new List<SettingDto> { MakeDto("setting1"), MakeDto("setting2") });

            _http.GetAsync<List<SettingRes>>(PimsApiEndpoints.GetAllSettings).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<SettingDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllSettingsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _http.Received(1).GetAsync<List<SettingRes>>(PimsApiEndpoints.GetAllSettings);
            _mapper.Received(1).Map<ApiResponseDto<List<SettingDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllSettingsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = FailureApiResponse<List<SettingRes>>();
            var failDto = FailureDto<List<SettingDto>>();

            _http.GetAsync<List<SettingRes>>(PimsApiEndpoints.GetAllSettings).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<SettingDto>>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.GetAllSettingsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _http.Received(1).GetAsync<List<SettingRes>>(PimsApiEndpoints.GetAllSettings);
        }

        #endregion

        #region GetAllUserUpdateableSettingsAsync

        [Fact]
        public async Task GetAllUserUpdateableSettingsAsync_WhenSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var settingList = new List<SettingRes> { MakeRes("updateable1") };
            var apiResponse = SuccessApiResponse(settingList);
            var mappedDto = SuccessDto(new List<SettingDto> { MakeDto("updateable1") });

            _http.GetAsync<List<SettingRes>>(PimsApiEndpoints.GetAllUserUpdateableSettings).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<SettingDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllUserUpdateableSettingsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            await _http.Received(1).GetAsync<List<SettingRes>>(PimsApiEndpoints.GetAllUserUpdateableSettings);
        }

        [Fact]
        public async Task GetAllUserUpdateableSettingsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = FailureApiResponse<List<SettingRes>>();
            var failDto = FailureDto<List<SettingDto>>();

            _http.GetAsync<List<SettingRes>>(PimsApiEndpoints.GetAllUserUpdateableSettings).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<SettingDto>>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.GetAllUserUpdateableSettingsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetSettingByIdAsync

        [Fact]
        public async Task GetSettingByIdAsync_WhenSuccessResponse_ReturnsMappedObject()
        {
            // Arrange
            var settingRes = MakeRes("setting1");
            var apiResponse = SuccessApiResponse(settingRes);
            var mappedDto = SuccessDto(MakeDto("setting1"));

            _http.GetAsync<SettingRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SettingDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetSettingByIdAsync("setting1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("setting1", result.Data.Id);
            await _http.Received(1).GetAsync<SettingRes>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<SettingDto>>(apiResponse);
        }

        [Fact]
        public async Task GetSettingByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = FailureApiResponse<SettingRes>();
            var failDto = FailureDto<SettingDto>();

            _http.GetAsync<SettingRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SettingDto>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.GetSettingByIdAsync("nonexistent");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _http.Received(1).GetAsync<SettingRes>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetSettingByIdAsync_WithUrlEncodableId_EscapesDataProperly()
        {
            // Arrange
            var settingId = "setting/with spaces";
            var escapedId = Uri.EscapeDataString(settingId);
            var settingRes = MakeRes(settingId);
            var apiResponse = SuccessApiResponse(settingRes);
            var mappedDto = SuccessDto(MakeDto(settingId));

            _http.GetAsync<SettingRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SettingDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetSettingByIdAsync(settingId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<SettingRes>(Arg.Is<string>(s => s.Contains(escapedId)));
        }

        #endregion

        #region UpdateSettingAsync

        [Fact]
        public async Task UpdateSettingAsync_WhenSuccessResponse_ReturnsMappedObject()
        {
            // Arrange
            var dto = MakeDto("setting1");
            var req = new SettingReq { Setting = "TestSetting", UserUpdateable = true };
            var settingRes = MakeRes("setting1");
            var apiResponse = SuccessApiResponse(settingRes);
            var mappedDto = SuccessDto(MakeDto("setting1"));

            _mapper.Map<SettingReq>(dto).Returns(req);
            _http.PutAsync<SettingReq, SettingRes>(Arg.Any<string>(), Arg.Any<SettingReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SettingDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateSettingAsync("setting1", dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("setting1", result.Data.Id);
            await _http.Received(1).PutAsync<SettingReq, SettingRes>(Arg.Any<string>(), Arg.Any<SettingReq>());
        }

        [Fact]
        public async Task UpdateSettingAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = MakeDto("setting1");
            var req = new SettingReq { Setting = "TestSetting", UserUpdateable = true };
            var apiResponse = FailureApiResponse<SettingRes>();
            var failDto = FailureDto<SettingDto>();

            _mapper.Map<SettingReq>(dto).Returns(req);
            _http.PutAsync<SettingReq, SettingRes>(Arg.Any<string>(), Arg.Any<SettingReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SettingDto>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.UpdateSettingAsync("setting1", dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _http.Received(1).PutAsync<SettingReq, SettingRes>(Arg.Any<string>(), Arg.Any<SettingReq>());
        }

        #endregion
    }
}
