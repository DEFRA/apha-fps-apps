using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Services.Costbook;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.Costbook.CostBookSettingsServiceTest
{
    public class CostBookSettingsServiceTests
    {
        private readonly ICostBookApiClient _costBookClient;
        private readonly ICostBookSettingsApiClient _costBookSettingsApiClient;
        private readonly CostBookSettingsService _costBookSettingsService;

        public CostBookSettingsServiceTests()
        {
            _costBookClient = Substitute.For<ICostBookApiClient>();
            _costBookSettingsApiClient = Substitute.For<ICostBookSettingsApiClient>();
            _costBookClient.CostbookSettings.Returns(_costBookSettingsApiClient);
            _costBookSettingsService = new CostBookSettingsService(_costBookClient);
        }

        #region GetSettingValueByIdAsync Tests

        [Fact]
        public async Task GetSettingValueByIdAsync_WithValidId_ReturnsSuccessResponse()
        {
            // Arrange
            var settingId = "HoursInDay";
            var expectedValue = "7.5";
            var expectedResponse = ApiResponseDto<string>.SuccessResponse(expectedValue);

            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId).Returns(expectedResponse);

            // Act
            var result = await _costBookSettingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(expectedValue, result.Data);
            await _costBookSettingsApiClient.Received(1).GetSettingValueByIdAsync(settingId);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithNonExistentId_ReturnsSuccessWithNull()
        {
            // Arrange
            var settingId = "NonExistentSetting";
            var expectedResponse = ApiResponseDto<string>.SuccessResponse(null!);

            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId).Returns(expectedResponse);

            // Act
            var result = await _costBookSettingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Null(result.Data);
            await _costBookSettingsApiClient.Received(1).GetSettingValueByIdAsync(settingId);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var settingId = "HoursInDay";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<string>.FailureResponse(errors, new ApiMetaDto());

            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId).Returns(expectedResponse);

            // Act
            var result = await _costBookSettingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API Error", result.Errors[0].Message);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_CallsApiClientWithCorrectId()
        {
            // Arrange
            var settingId = "TestSetting";
            var expectedResponse = ApiResponseDto<string>.SuccessResponse("TestValue");

            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId).Returns(expectedResponse);

            // Act
            await _costBookSettingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            await _costBookSettingsApiClient.Received(1).GetSettingValueByIdAsync(settingId);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithNullId_CallsApiClient()
        {
            // Arrange
            string? settingId = null;
            var expectedResponse = ApiResponseDto<string>.SuccessResponse(null!);

            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId).Returns(expectedResponse);

            // Act
            var result = await _costBookSettingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.NotNull(result);
            await _costBookSettingsApiClient.Received(1).GetSettingValueByIdAsync(settingId);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithEmptyId_CallsApiClient()
        {
            // Arrange
            var settingId = string.Empty;
            var expectedResponse = ApiResponseDto<string>.SuccessResponse(null!);

            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId).Returns(expectedResponse);

            // Act
            var result = await _costBookSettingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.NotNull(result);
            await _costBookSettingsApiClient.Received(1).GetSettingValueByIdAsync(settingId);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithWhitespaceId_CallsApiClient()
        {
            // Arrange
            var settingId = "   ";
            var expectedResponse = ApiResponseDto<string>.SuccessResponse(null!);

            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId).Returns(expectedResponse);

            // Act
            var result = await _costBookSettingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.NotNull(result);
            await _costBookSettingsApiClient.Received(1).GetSettingValueByIdAsync(settingId);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithNumericValue_ReturnsNumericString()
        {
            // Arrange
            var settingId = "MaxRetries";
            var expectedValue = "5";
            var expectedResponse = ApiResponseDto<string>.SuccessResponse(expectedValue);

            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId).Returns(expectedResponse);

            // Act
            var result = await _costBookSettingsService.GetSettingValueByIdAsync(settingId);

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
            var expectedResponse = ApiResponseDto<string>.SuccessResponse(expectedValue);

            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId).Returns(expectedResponse);

            // Act
            var result = await _costBookSettingsService.GetSettingValueByIdAsync(settingId);

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
            var expectedResponse = ApiResponseDto<string>.SuccessResponse(expectedValue);

            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId).Returns(expectedResponse);

            // Act
            var result = await _costBookSettingsService.GetSettingValueByIdAsync(settingId);

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
            var expectedResponse = ApiResponseDto<string>.SuccessResponse(expectedValue);

            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId).Returns(expectedResponse);

            // Act
            var result = await _costBookSettingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(string.Empty, result.Data);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithZeroValue_ReturnsZeroString()
        {
            // Arrange
            var settingId = "MinValue";
            var expectedValue = "0";
            var expectedResponse = ApiResponseDto<string>.SuccessResponse(expectedValue);

            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId).Returns(expectedResponse);

            // Act
            var result = await _costBookSettingsService.GetSettingValueByIdAsync(settingId);

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
            var expectedResponse = ApiResponseDto<string>.SuccessResponse(expectedValue);

            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId).Returns(expectedResponse);

            // Act
            var result = await _costBookSettingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("-10", result.Data);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_CalledMultipleTimes_CallsApiClientEachTime()
        {
            // Arrange
            var settingId = "HoursInDay";
            var expectedResponse = ApiResponseDto<string>.SuccessResponse("8.0");

            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId).Returns(expectedResponse);

            // Act
            await _costBookSettingsService.GetSettingValueByIdAsync(settingId);
            await _costBookSettingsService.GetSettingValueByIdAsync(settingId);
            await _costBookSettingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            await _costBookSettingsApiClient.Received(3).GetSettingValueByIdAsync(settingId);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithDifferentIds_CallsApiClientWithEachId()
        {
            // Arrange
            var settingId1 = "Setting1";
            var settingId2 = "Setting2";
            var response1 = ApiResponseDto<string>.SuccessResponse("Value1");
            var response2 = ApiResponseDto<string>.SuccessResponse("Value2");

            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId1).Returns(response1);
            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId2).Returns(response2);

            // Act
            var result1 = await _costBookSettingsService.GetSettingValueByIdAsync(settingId1);
            var result2 = await _costBookSettingsService.GetSettingValueByIdAsync(settingId2);

            // Assert
            Assert.Equal("Value1", result1.Data);
            Assert.Equal("Value2", result2.Data);
            await _costBookSettingsApiClient.Received(1).GetSettingValueByIdAsync(settingId1);
            await _costBookSettingsApiClient.Received(1).GetSettingValueByIdAsync(settingId2);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WhenApiThrowsException_PropagatesException()
        {
            // Arrange
            var settingId = "HoursInDay";
            var expectedException = new Exception("API connection failed");
            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId).Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _costBookSettingsService.GetSettingValueByIdAsync(settingId));
            Assert.Equal("API connection failed", exception.Message);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithMultipleErrors_ReturnsAllErrors()
        {
            // Arrange
            var settingId = "HoursInDay";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Error 1", Code = "ERR_001" },
                new ApiErrorDto { Message = "Error 2", Code = "ERR_002" }
            };
            var expectedResponse = ApiResponseDto<string>.FailureResponse(errors, new ApiMetaDto());

            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId).Returns(expectedResponse);

            // Act
            var result = await _costBookSettingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal(2, result.Errors.Count);
            Assert.Equal("Error 1", result.Errors[0].Message);
            Assert.Equal("Error 2", result.Errors[1].Message);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithSpecialCharactersInId_CallsApiClient()
        {
            // Arrange
            var settingId = "setting-with_special.chars@123";
            var expectedResponse = ApiResponseDto<string>.SuccessResponse("special value");

            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId).Returns(expectedResponse);

            // Act
            var result = await _costBookSettingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("special value", result.Data);
            await _costBookSettingsApiClient.Received(1).GetSettingValueByIdAsync(settingId);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithLongId_CallsApiClient()
        {
            // Arrange
            var settingId = new string('A', 500);
            var expectedResponse = ApiResponseDto<string>.SuccessResponse("long id value");

            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId).Returns(expectedResponse);

            // Act
            var result = await _costBookSettingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("long id value", result.Data);
            await _costBookSettingsApiClient.Received(1).GetSettingValueByIdAsync(settingId);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithVeryLongValue_ReturnsLongValue()
        {
            // Arrange
            var settingId = "LongDescription";
            var expectedValue = new string('X', 5000);
            var expectedResponse = ApiResponseDto<string>.SuccessResponse(expectedValue);

            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId).Returns(expectedResponse);

            // Act
            var result = await _costBookSettingsService.GetSettingValueByIdAsync(settingId);

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
            var expectedResponse = ApiResponseDto<string>.SuccessResponse(expectedValue);

            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId).Returns(expectedResponse);

            // Act
            var result = await _costBookSettingsService.GetSettingValueByIdAsync(settingId);

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
            var expectedResponse = ApiResponseDto<string>.SuccessResponse(expectedValue);

            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId).Returns(expectedResponse);

            // Act
            var result = await _costBookSettingsService.GetSettingValueByIdAsync(settingId);

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
            var expectedResponse = ApiResponseDto<string>.SuccessResponse(expectedValue);

            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId).Returns(expectedResponse);

            // Act
            var result = await _costBookSettingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(expectedValue, result.Data);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_ResponseIncludesMetadata()
        {
            // Arrange
            var settingId = "HoursInDay";
            var meta = new ApiMetaDto { /* add metadata properties if needed */ };
            var expectedResponse = new ApiResponseDto<string>
            {
                Success = true,
                Data = "8.0",
                Errors = new List<ApiErrorDto>(),
                Meta = meta
            };

            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId).Returns(expectedResponse);

            // Act
            var result = await _costBookSettingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Meta);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_CaseSensitiveId_CallsApiClientWithExactCase()
        {
            // Arrange
            var settingId1 = "HoursInDay";
            var settingId2 = "hoursInDay";
            var response1 = ApiResponseDto<string>.SuccessResponse("8.0");
            var response2 = ApiResponseDto<string>.SuccessResponse("7.5");

            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId1).Returns(response1);
            _costBookSettingsApiClient.GetSettingValueByIdAsync(settingId2).Returns(response2);

            // Act
            var result1 = await _costBookSettingsService.GetSettingValueByIdAsync(settingId1);
            var result2 = await _costBookSettingsService.GetSettingValueByIdAsync(settingId2);

            // Assert
            Assert.Equal("8.0", result1.Data);
            Assert.Equal("7.5", result2.Data);
            await _costBookSettingsApiClient.Received(1).GetSettingValueByIdAsync(settingId1);
            await _costBookSettingsApiClient.Received(1).GetSettingValueByIdAsync(settingId2);
        }

        #endregion
    }
}
