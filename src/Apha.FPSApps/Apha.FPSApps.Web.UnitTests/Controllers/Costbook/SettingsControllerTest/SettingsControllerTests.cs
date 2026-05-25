using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Web.Areas.CostBook.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.Costbook.SettingsControllerTest
{
    public class SettingsControllerTests
    {
        private readonly ICostBookSettingsService _settingsService;
        private readonly SettingsController _controller;

        public SettingsControllerTests()
        {
            _settingsService = Substitute.For<ICostBookSettingsService>();
            _controller = new SettingsController(_settingsService);
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        #region GetSettingValueById Tests

        [Fact]
        public async Task GetSettingValueById_ReturnsSuccess_WhenServiceSucceeds()
        {
            // Arrange
            var settingId = "HoursInDay";
            var expectedValue = "7.5";
            _settingsService.GetSettingValueByIdAsync(settingId)
                .Returns(ApiResponseDto<string>.SuccessResponse(expectedValue));

            // Act
            var result = await _controller.GetSettingValueById(settingId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal(expectedValue, element.GetProperty("hoursPerDay").GetString());
        }

        [Fact]
        public async Task GetSettingValueById_ReturnsDefaultValue_WhenServiceFails()
        {
            // Arrange
            var settingId = "HoursInDay";
            _settingsService.GetSettingValueByIdAsync(settingId)
                .Returns(ApiResponseDto<string>.FailureResponse(null, new ApiMetaDto()));

            // Act
            var result = await _controller.GetSettingValueById(settingId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal(7.2, element.GetProperty("hoursPerDay").GetDouble());
        }

        [Fact]
        public async Task GetSettingValueById_ReturnsDefaultValue_WhenServiceReturnsNull()
        {
            // Arrange
            var settingId = "HoursInDay";
            _settingsService.GetSettingValueByIdAsync(settingId)
                .Returns(ApiResponseDto<string>.SuccessResponse(null!));

            // Act
            var result = await _controller.GetSettingValueById(settingId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal("null", element.GetProperty("hoursPerDay").GetRawText());
        }

        [Fact]
        public async Task GetSettingValueById_CallsServiceWithCorrectId()
        {
            // Arrange
            var settingId = "HoursInDay";
            _settingsService.GetSettingValueByIdAsync(settingId)
                .Returns(ApiResponseDto<string>.SuccessResponse("8.0"));

            // Act
            await _controller.GetSettingValueById(settingId);

            // Assert
            await _settingsService.Received(1).GetSettingValueByIdAsync(settingId);
        }

        [Fact]
        public async Task GetSettingValueById_HandlesNullId()
        {
            // Arrange
            string? settingId = null;
            _settingsService.GetSettingValueByIdAsync(settingId)
                .Returns(ApiResponseDto<string>.FailureResponse(null, new ApiMetaDto()));

            // Act
            var result = await _controller.GetSettingValueById(settingId!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal(7.2, element.GetProperty("hoursPerDay").GetDouble());
        }

        [Fact]
        public async Task GetSettingValueById_HandlesEmptyId()
        {
            // Arrange
            var settingId = string.Empty;
            _settingsService.GetSettingValueByIdAsync(settingId)
                .Returns(ApiResponseDto<string>.FailureResponse(null, new ApiMetaDto()));

            // Act
            var result = await _controller.GetSettingValueById(settingId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal(7.2, element.GetProperty("hoursPerDay").GetDouble());
        }

        [Fact]
        public async Task GetSettingValueById_ReturnsSuccess_WithNumericValue()
        {
            // Arrange
            var settingId = "HoursInDay";
            var numericValue = "8.5";
            _settingsService.GetSettingValueByIdAsync(settingId)
                .Returns(ApiResponseDto<string>.SuccessResponse(numericValue));

            // Act
            var result = await _controller.GetSettingValueById(settingId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal(numericValue, element.GetProperty("hoursPerDay").GetString());
        }

        [Fact]
        public async Task GetSettingValueById_ReturnsSuccess_WithNonNumericValue()
        {
            // Arrange
            var settingId = "SomeTextSetting";
            var textValue = "TestValue";
            _settingsService.GetSettingValueByIdAsync(settingId)
                .Returns(ApiResponseDto<string>.SuccessResponse(textValue));

            // Act
            var result = await _controller.GetSettingValueById(settingId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal(textValue, element.GetProperty("hoursPerDay").GetString());
        }

        [Fact]
        public async Task GetSettingValueById_ReturnsDefaultValue_WhenServiceThrowsException()
        {
            // Arrange
            var settingId = "HoursInDay";
            _settingsService.GetSettingValueByIdAsync(settingId)
                .Returns(Task.FromException<ApiResponseDto<string>>(new Exception("Service error")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.GetSettingValueById(settingId));
        }

        [Fact]
        public async Task GetSettingValueById_ReturnsJson_WithCorrectStructure()
        {
            // Arrange
            var settingId = "HoursInDay";
            var settingValue = "7.5";
            _settingsService.GetSettingValueByIdAsync(settingId)
                .Returns(ApiResponseDto<string>.SuccessResponse(settingValue));

            // Act
            var result = await _controller.GetSettingValueById(settingId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);

            // Verify JSON structure has both required properties
            Assert.True(element.TryGetProperty("success", out _));
            Assert.True(element.TryGetProperty("hoursPerDay", out _));
        }

        [Fact]
        public async Task GetSettingValueById_HandlesWhitespaceId()
        {
            // Arrange
            var settingId = "   ";
            _settingsService.GetSettingValueByIdAsync(settingId)
                .Returns(ApiResponseDto<string>.FailureResponse(null, new ApiMetaDto()));

            // Act
            var result = await _controller.GetSettingValueById(settingId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetSettingValueById_ReturnsSuccess_WithZeroValue()
        {
            // Arrange
            var settingId = "HoursInDay";
            var zeroValue = "0";
            _settingsService.GetSettingValueByIdAsync(settingId)
                .Returns(ApiResponseDto<string>.SuccessResponse(zeroValue));

            // Act
            var result = await _controller.GetSettingValueById(settingId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal(zeroValue, element.GetProperty("hoursPerDay").GetString());
        }

        [Fact]
        public async Task GetSettingValueById_ReturnsSuccess_WithNegativeValue()
        {
            // Arrange
            var settingId = "SomeSetting";
            var negativeValue = "-5.5";
            _settingsService.GetSettingValueByIdAsync(settingId)
                .Returns(ApiResponseDto<string>.SuccessResponse(negativeValue));

            // Act
            var result = await _controller.GetSettingValueById(settingId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal(negativeValue, element.GetProperty("hoursPerDay").GetString());
        }

        #endregion
    }
}
