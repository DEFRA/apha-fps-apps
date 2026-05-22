using Apha.Common.Contracts;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.Costbook.CostBookSettingsApiClientTest
{
    public class CostBookSettingsApiClientTests
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly CostBookSettingsApiClient _client;

        public CostBookSettingsApiClientTests()
        {
            _http = Substitute.For<ICostBookHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new CostBookSettingsApiClient(_http, _mapper);
        }

        #region GetSettingValueByIdAsync Tests

        [Fact]
        public async Task GetSettingValueByIdAsync_WithValidId_ReturnsSuccessWithData()
        {
            // Arrange
            var settingId = "HoursInDay";
            var expectedValue = "7.5";
            var apiResponse = new ApiResponse<string> { Success = true, Data = expectedValue };

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(expectedValue, result.Data);
            await _http.Received(1).GetAsync<string>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithValidId_EncodesUrlCorrectly()
        {
            // Arrange
            var settingId = "HoursInDay";
            var apiResponse = new ApiResponse<string> { Success = true, Data = "8.0" };

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            await _client.GetSettingValueByIdAsync(settingId);

            // Assert
            await _http.Received(1).GetAsync<string>(Arg.Is<string>(s => s.Contains("?id=")));
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithSpecialCharactersInId_EncodesUrlCorrectly()
        {
            // Arrange
            var settingId = "setting-with/special&chars";
            var apiResponse = new ApiResponse<string> { Success = true, Data = "value" };

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            await _client.GetSettingValueByIdAsync(settingId);

            // Assert - URL should be encoded, not contain raw special characters
            await _http.Received(1).GetAsync<string>(Arg.Is<string>(s => !s.Contains("setting-with/special&chars")));
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithNullId_DoesNotAppendQueryString()
        {
            // Arrange
            string? settingId = null;
            var apiResponse = new ApiResponse<string> { Success = true, Data = null };
            var mappedResponse = new ApiResponseDto<string>
            {
                Success = false,
                Errors = [],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<string>>(apiResponse).Returns(mappedResponse);

            // Act
            await _client.GetSettingValueByIdAsync(settingId);

            // Assert - Should not contain query string when id is null
            await _http.Received(1).GetAsync<string>(Arg.Is<string>(s => !s.Contains("?id=")));
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithEmptyId_DoesNotAppendQueryString()
        {
            // Arrange
            var settingId = string.Empty;
            var apiResponse = new ApiResponse<string> { Success = true, Data = null };
            var mappedResponse = new ApiResponseDto<string>
            {
                Success = false,
                Errors = [],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<string>>(apiResponse).Returns(mappedResponse);

            // Act
            await _client.GetSettingValueByIdAsync(settingId);

            // Assert
            await _http.Received(1).GetAsync<string>(Arg.Is<string>(s => !s.Contains("?id=")));
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithWhitespaceId_AppendsQueryString()
        {
            // Arrange - string.IsNullOrEmpty returns false for whitespace, so query string is appended
            var settingId = "   ";
            var apiResponse = new ApiResponse<string> { Success = true, Data = null };
            var mappedResponse = new ApiResponseDto<string>
            {
                Success = false,
                Errors = [],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<string>>(apiResponse).Returns(mappedResponse);

            // Act
            await _client.GetSettingValueByIdAsync(settingId);

            // Assert - Whitespace is not considered null/empty, so query string is appended
            await _http.Received(1).GetAsync<string>(Arg.Is<string>(s => s.Contains("?id=")));
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WhenApiSuccessButNullData_ReturnsFailureResponse()
        {
            // Arrange
            var settingId = "NonExistentSetting";
            var apiResponse = new ApiResponse<string> { Success = true, Data = null };
            var errors = new List<ApiErrorDto>();
            var mappedResponse = new ApiResponseDto<string>
            {
                Success = false,
                Errors = errors,
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<string>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            _mapper.Received(1).Map<ApiResponseDto<string>>(apiResponse);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var settingId = "HoursInDay";
            var apiResponse = new ApiResponse<string>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiError> { new ApiError { Message = "Not Found", Code = "NOT_FOUND" } }
            };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" } };
            var mappedResponse = new ApiResponseDto<string>
            {
                Success = false,
                Errors = errors,
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<string>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Not Found", result.Errors[0].Message);
            Assert.Equal("NOT_FOUND", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithNumericValue_ReturnsNumericString()
        {
            // Arrange
            var settingId = "MaxRetries";
            var expectedValue = "5";
            var apiResponse = new ApiResponse<string> { Success = true, Data = expectedValue };

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(expectedValue, result.Data);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithDecimalValue_ReturnsDecimalString()
        {
            // Arrange
            var settingId = "TaxRate";
            var expectedValue = "0.15";
            var apiResponse = new ApiResponse<string> { Success = true, Data = expectedValue };

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(expectedValue, result.Data);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithTextValue_ReturnsTextString()
        {
            // Arrange
            var settingId = "ApplicationName";
            var expectedValue = "CostBook Application";
            var apiResponse = new ApiResponse<string> { Success = true, Data = expectedValue };

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(expectedValue, result.Data);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithEmptyStringValue_ReturnsEmptyString()
        {
            // Arrange
            var settingId = "EmptySetting";
            var expectedValue = string.Empty;
            var apiResponse = new ApiResponse<string> { Success = true, Data = expectedValue };

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(string.Empty, result.Data);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithMultipleErrors_ReturnsAllErrors()
        {
            // Arrange
            var settingId = "HoursInDay";
            var apiResponse = new ApiResponse<string>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiError>
                {
                    new ApiError { Message = "Error 1", Code = "ERR_001" },
                    new ApiError { Message = "Error 2", Code = "ERR_002" }
                }
            };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Error 1", Code = "ERR_001" },
                new ApiErrorDto { Message = "Error 2", Code = "ERR_002" }
            };
            var mappedResponse = new ApiResponseDto<string>
            {
                Success = false,
                Errors = errors,
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<string>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal(2, result.Errors.Count);
            Assert.Equal("Error 1", result.Errors[0].Message);
            Assert.Equal("Error 2", result.Errors[1].Message);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_CallsHttpExecutorOnce()
        {
            // Arrange
            var settingId = "TestSetting";
            var apiResponse = new ApiResponse<string> { Success = true, Data = "TestValue" };

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            await _client.GetSettingValueByIdAsync(settingId);

            // Assert
            await _http.Received(1).GetAsync<string>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithLongValue_ReturnsLongValue()
        {
            // Arrange
            var settingId = "LongDescription";
            var expectedValue = new string('X', 5000);
            var apiResponse = new ApiResponse<string> { Success = true, Data = expectedValue };

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(5000, result.Data.Length);
            Assert.Equal(expectedValue, result.Data);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithBooleanTrueValue_ReturnsString()
        {
            // Arrange
            var settingId = "IsEnabled";
            var expectedValue = "true";
            var apiResponse = new ApiResponse<string> { Success = true, Data = expectedValue };

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("true", result.Data);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithBooleanFalseValue_ReturnsString()
        {
            // Arrange
            var settingId = "IsEnabled";
            var expectedValue = "false";
            var apiResponse = new ApiResponse<string> { Success = true, Data = expectedValue };

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("false", result.Data);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithJsonValue_ReturnsJsonString()
        {
            // Arrange
            var settingId = "Configuration";
            var expectedValue = "{\"key\":\"value\"}";
            var apiResponse = new ApiResponse<string> { Success = true, Data = expectedValue };

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(expectedValue, result.Data);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithZeroValue_ReturnsZeroString()
        {
            // Arrange
            var settingId = "MinValue";
            var expectedValue = "0";
            var apiResponse = new ApiResponse<string> { Success = true, Data = expectedValue };

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("0", result.Data);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithNegativeValue_ReturnsNegativeString()
        {
            // Arrange
            var settingId = "Adjustment";
            var expectedValue = "-10";
            var apiResponse = new ApiResponse<string> { Success = true, Data = expectedValue };

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("-10", result.Data);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_UsesMapperWhenResponseFails()
        {
            // Arrange
            var settingId = "HoursInDay";
            var apiResponse = new ApiResponse<string> { Success = false, Data = null };
            var mappedResponse = new ApiResponseDto<string>
            {
                Success = false,
                Errors = [],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<string>>(apiResponse).Returns(mappedResponse);

            // Act
            await _client.GetSettingValueByIdAsync(settingId);

            // Assert
            _mapper.Received(1).Map<ApiResponseDto<string>>(apiResponse);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_UsesMapperWhenDataIsNull()
        {
            // Arrange
            var settingId = "HoursInDay";
            var apiResponse = new ApiResponse<string> { Success = true, Data = null };
            var mappedResponse = new ApiResponseDto<string>
            {
                Success = false,
                Errors = [],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<string>>(apiResponse).Returns(mappedResponse);

            // Act
            await _client.GetSettingValueByIdAsync(settingId);

            // Assert
            _mapper.Received(1).Map<ApiResponseDto<string>>(apiResponse);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_DoesNotUseMapper_WhenSuccessWithData()
        {
            // Arrange
            var settingId = "HoursInDay";
            var apiResponse = new ApiResponse<string> { Success = true, Data = "8.0" };

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            await _client.GetSettingValueByIdAsync(settingId);

            // Assert
            _mapper.DidNotReceive().Map<ApiResponseDto<string>>(Arg.Any<ApiResponse<string>>());
        }

        #endregion
    }
}
