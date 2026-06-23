using Apha.Common.Contracts;
using Apha.Costbook.Api.Controllers;
using Apha.Costbook.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.Costbook.Api.UnitTests.Controller.SettingsControllerTest
{
    public class SettingsControllerTests
    {
        private readonly ISettingsService _settingsService;
        private readonly SettingsController _controller;

        public SettingsControllerTests()
        {
            _settingsService = Substitute.For<ISettingsService>();
            _controller = new SettingsController(_settingsService);
        }

        #region GetSettingValueByIdAsync Tests

        [Fact]
        public async Task GetSettingValueByIdAsync_ReturnsOkResult_WithSuccessResponse_WhenServiceReturnsValue()
        {
            // Arrange
            var settingId = "HoursInDay";
            var expectedValue = "7.5";
            _settingsService.GetSettingValueByIdAsync(settingId).Returns(expectedValue);

            // Act
            var result = await _controller.GetSettingValueByIdAsync(settingId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<string>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(expectedValue, response.Data);
            Assert.NotNull(response.Errors);
            Assert.Empty(response.Errors);
            Assert.NotNull(response.Meta);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_ReturnsOkResult_WithNullData_WhenServiceReturnsNull()
        {
            // Arrange
            var settingId = "NonExistentSetting";
            _settingsService.GetSettingValueByIdAsync(settingId).Returns((string?)null);

            // Act
            var result = await _controller.GetSettingValueByIdAsync(settingId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<string>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Null(response.Data);
            Assert.NotNull(response.Errors);
            Assert.Empty(response.Errors);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_CallsServiceWithCorrectId()
        {
            // Arrange
            var settingId = "HoursInDay";
            var expectedValue = "8.0";
            _settingsService.GetSettingValueByIdAsync(settingId).Returns(expectedValue);

            // Act
            await _controller.GetSettingValueByIdAsync(settingId);

            // Assert
            await _settingsService.Received(1).GetSettingValueByIdAsync(settingId);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_HandlesNullId()
        {
            // Arrange — controller converts null to string.Empty before calling the service
            string? settingId = null;
            _settingsService.GetSettingValueByIdAsync(string.Empty).Returns((string?)null);

            // Act
            var result = await _controller.GetSettingValueByIdAsync(settingId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<string>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Null(response.Data);
            await _settingsService.Received(1).GetSettingValueByIdAsync(string.Empty);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_HandlesEmptyId()
        {
            // Arrange
            var settingId = string.Empty;
            _settingsService.GetSettingValueByIdAsync(settingId).Returns((string?)null);

            // Act
            var result = await _controller.GetSettingValueByIdAsync(settingId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<string>>(okResult.Value);
            Assert.True(response.Success);
            await _settingsService.Received(1).GetSettingValueByIdAsync(settingId);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_HandlesWhitespaceId()
        {
            // Arrange
            var settingId = "   ";
            _settingsService.GetSettingValueByIdAsync(settingId).Returns((string?)null);

            // Act
            var result = await _controller.GetSettingValueByIdAsync(settingId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<string>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Null(response.Data);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_ReturnsOkResult_WithNumericValue()
        {
            // Arrange
            var settingId = "HoursInDay";
            var numericValue = "8.5";
            _settingsService.GetSettingValueByIdAsync(settingId).Returns(numericValue);

            // Act
            var result = await _controller.GetSettingValueByIdAsync(settingId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<string>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(numericValue, response.Data);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_ReturnsOkResult_WithTextValue()
        {
            // Arrange
            var settingId = "AppName";
            var textValue = "CostBook Application";
            _settingsService.GetSettingValueByIdAsync(settingId).Returns(textValue);

            // Act
            var result = await _controller.GetSettingValueByIdAsync(settingId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<string>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(textValue, response.Data);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_ReturnsOkResult_WithZeroValue()
        {
            // Arrange
            var settingId = "MinValue";
            var zeroValue = "0";
            _settingsService.GetSettingValueByIdAsync(settingId).Returns(zeroValue);

            // Act
            var result = await _controller.GetSettingValueByIdAsync(settingId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<string>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(zeroValue, response.Data);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_ReturnsOkResult_WithNegativeValue()
        {
            // Arrange
            var settingId = "Adjustment";
            var negativeValue = "-5.5";
            _settingsService.GetSettingValueByIdAsync(settingId).Returns(negativeValue);

            // Act
            var result = await _controller.GetSettingValueByIdAsync(settingId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<string>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(negativeValue, response.Data);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_ReturnsOkResult_WithEmptyStringValue()
        {
            // Arrange
            var settingId = "EmptySetting";
            var emptyValue = string.Empty;
            _settingsService.GetSettingValueByIdAsync(settingId).Returns(emptyValue);

            // Act
            var result = await _controller.GetSettingValueByIdAsync(settingId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<string>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(emptyValue, response.Data);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_ThrowsException_WhenServiceThrows()
        {
            // Arrange
            var settingId = "HoursInDay";
            var expectedException = new Exception("Database connection failed");
            _settingsService.GetSettingValueByIdAsync(settingId).Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _controller.GetSettingValueByIdAsync(settingId));
            Assert.Equal("Database connection failed", exception.Message);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_ResponseStructure_IsCorrect()
        {
            // Arrange
            var settingId = "HoursInDay";
            var settingValue = "7.5";
            _settingsService.GetSettingValueByIdAsync(settingId).Returns(settingValue);

            // Act
            var result = await _controller.GetSettingValueByIdAsync(settingId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<string>>(okResult.Value);

            // Verify all required properties are present
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Errors);
            Assert.NotNull(response.Meta);
            Assert.IsType<List<ApiError>>(response.Errors);
            Assert.IsType<ApiMeta>(response.Meta);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_HandlesLongId()
        {
            // Arrange
            var longId = new string('A', 1000);
            _settingsService.GetSettingValueByIdAsync(longId).Returns("value");

            // Act
            var result = await _controller.GetSettingValueByIdAsync(longId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<string>>(okResult.Value);
            Assert.True(response.Success);
            await _settingsService.Received(1).GetSettingValueByIdAsync(longId);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_HandlesSpecialCharactersInId()
        {
            // Arrange
            var specialId = "setting-with_special.chars@123";
            var value = "test value";
            _settingsService.GetSettingValueByIdAsync(specialId).Returns(value);

            // Act
            var result = await _controller.GetSettingValueByIdAsync(specialId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<string>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(value, response.Data);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_MultipleCallsWithSameId_CallsServiceMultipleTimes()
        {
            // Arrange
            var settingId = "HoursInDay";
            var value = "8.0";
            _settingsService.GetSettingValueByIdAsync(settingId).Returns(value);

            // Act
            await _controller.GetSettingValueByIdAsync(settingId);
            await _controller.GetSettingValueByIdAsync(settingId);
            await _controller.GetSettingValueByIdAsync(settingId);

            // Assert
            await _settingsService.Received(3).GetSettingValueByIdAsync(settingId);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_ReturnsOkResult_WithVeryLongValue()
        {
            // Arrange
            var settingId = "LongDescription";
            var longValue = new string('X', 10000);
            _settingsService.GetSettingValueByIdAsync(settingId).Returns(longValue);

            // Act
            var result = await _controller.GetSettingValueByIdAsync(settingId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<string>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(longValue, response.Data);
            Assert.Equal(10000, response.Data!.Length);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_ResponseAlwaysHasSuccessTrue()
        {
            // Arrange - Test that even with null data, Success is always true
            var settingId = "AnySetting";
            _settingsService.GetSettingValueByIdAsync(settingId).Returns((string?)null);

            // Act
            var result = await _controller.GetSettingValueByIdAsync(settingId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<string>>(okResult.Value);
            Assert.True(response.Success); // Controller always returns Success = true
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_ErrorsListIsAlwaysEmpty()
        {
            // Arrange
            var settingId = "HoursInDay";
            _settingsService.GetSettingValueByIdAsync(settingId).Returns("8.0");

            // Act
            var result = await _controller.GetSettingValueByIdAsync(settingId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<string>>(okResult.Value);
            Assert.NotNull(response.Errors);
            Assert.Empty(response.Errors);
        }

        #endregion
    }
}
