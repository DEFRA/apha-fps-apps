using Apha.Costbook.Application.Services;
using Apha.Costbook.Core.Interfaces;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.Costbook.Application.UnitTests.Services.SettingsServiceTest
{
    public class SettingsServiceTests
    {
        private readonly ISettingsRepository _mockRepository;
        private readonly SettingsService _settingsService;

        public SettingsServiceTests()
        {
            _mockRepository = Substitute.For<ISettingsRepository>();
            _settingsService = new SettingsService(_mockRepository);
        }

        #region GetSettingValueByIdAsync Tests

        [Fact]
        public async Task GetSettingValueByIdAsync_WithValidId_ReturnsSettingValue()
        {
            // Arrange
            var settingId = "HoursInDay";
            var expectedValue = "7.5";
            _mockRepository.GetSettingValueByIdAsync(settingId).Returns(expectedValue);

            // Act
            var result = await _settingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedValue, result);
            await _mockRepository.Received(1).GetSettingValueByIdAsync(settingId);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithNonExistentId_ReturnsNull()
        {
            // Arrange
            var settingId = "NonExistentSetting";
            _mockRepository.GetSettingValueByIdAsync(settingId).Returns((string?)null);

            // Act
            var result = await _settingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.Null(result);
            await _mockRepository.Received(1).GetSettingValueByIdAsync(settingId);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_CallsRepositoryWithCorrectId()
        {
            // Arrange
            var settingId = "TestSetting";
            var settingValue = "TestValue";
            _mockRepository.GetSettingValueByIdAsync(settingId).Returns(settingValue);

            // Act
            await _settingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            await _mockRepository.Received(1).GetSettingValueByIdAsync(settingId);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithEmptyId_CallsRepository()
        {
            // Arrange
            var settingId = string.Empty;
            _mockRepository.GetSettingValueByIdAsync(settingId).Returns((string?)null);

            // Act
            var result = await _settingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.Null(result);
            await _mockRepository.Received(1).GetSettingValueByIdAsync(settingId);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithWhitespaceId_CallsRepository()
        {
            // Arrange
            var settingId = "   ";
            _mockRepository.GetSettingValueByIdAsync(settingId).Returns((string?)null);

            // Act
            var result = await _settingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.Null(result);
            await _mockRepository.Received(1).GetSettingValueByIdAsync(settingId);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithNumericValue_ReturnsNumericString()
        {
            // Arrange
            var settingId = "MaxRetries";
            var expectedValue = "5";
            _mockRepository.GetSettingValueByIdAsync(settingId).Returns(expectedValue);

            // Act
            var result = await _settingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.Equal(expectedValue, result);
            await _mockRepository.Received(1).GetSettingValueByIdAsync(settingId);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithDecimalValue_ReturnsDecimalString()
        {
            // Arrange
            var settingId = "TaxRate";
            var expectedValue = "0.15";
            _mockRepository.GetSettingValueByIdAsync(settingId).Returns(expectedValue);

            // Act
            var result = await _settingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithTextValue_ReturnsTextString()
        {
            // Arrange
            var settingId = "ApplicationName";
            var expectedValue = "CostBook Application";
            _mockRepository.GetSettingValueByIdAsync(settingId).Returns(expectedValue);

            // Act
            var result = await _settingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithEmptyStringValue_ReturnsEmptyString()
        {
            // Arrange
            var settingId = "EmptySetting";
            var expectedValue = string.Empty;
            _mockRepository.GetSettingValueByIdAsync(settingId).Returns(expectedValue);

            // Act
            var result = await _settingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithZeroValue_ReturnsZeroString()
        {
            // Arrange
            var settingId = "MinValue";
            var expectedValue = "0";
            _mockRepository.GetSettingValueByIdAsync(settingId).Returns(expectedValue);

            // Act
            var result = await _settingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.Equal("0", result);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithNegativeValue_ReturnsNegativeString()
        {
            // Arrange
            var settingId = "Adjustment";
            var expectedValue = "-10";
            _mockRepository.GetSettingValueByIdAsync(settingId).Returns(expectedValue);

            // Act
            var result = await _settingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.Equal("-10", result);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_CalledMultipleTimes_CallsRepositoryEachTime()
        {
            // Arrange
            var settingId = "HoursInDay";
            var settingValue = "8.0";
            _mockRepository.GetSettingValueByIdAsync(settingId).Returns(settingValue);

            // Act
            await _settingsService.GetSettingValueByIdAsync(settingId);
            await _settingsService.GetSettingValueByIdAsync(settingId);
            await _settingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            await _mockRepository.Received(3).GetSettingValueByIdAsync(settingId);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithDifferentIds_CallsRepositoryWithEachId()
        {
            // Arrange
            var settingId1 = "Setting1";
            var settingId2 = "Setting2";
            _mockRepository.GetSettingValueByIdAsync(settingId1).Returns("Value1");
            _mockRepository.GetSettingValueByIdAsync(settingId2).Returns("Value2");

            // Act
            var result1 = await _settingsService.GetSettingValueByIdAsync(settingId1);
            var result2 = await _settingsService.GetSettingValueByIdAsync(settingId2);

            // Assert
            Assert.Equal("Value1", result1);
            Assert.Equal("Value2", result2);
            await _mockRepository.Received(1).GetSettingValueByIdAsync(settingId1);
            await _mockRepository.Received(1).GetSettingValueByIdAsync(settingId2);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var settingId = "HoursInDay";
            var expectedException = new Exception("Database connection failed");
            _mockRepository.GetSettingValueByIdAsync(settingId).Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _settingsService.GetSettingValueByIdAsync(settingId));
            Assert.Equal("Database connection failed", exception.Message);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithSpecialCharactersInId_CallsRepository()
        {
            // Arrange
            var settingId = "setting-with_special.chars@123";
            var expectedValue = "special value";
            _mockRepository.GetSettingValueByIdAsync(settingId).Returns(expectedValue);

            // Act
            var result = await _settingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.Equal(expectedValue, result);
            await _mockRepository.Received(1).GetSettingValueByIdAsync(settingId);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithLongId_CallsRepository()
        {
            // Arrange
            var settingId = new string('A', 500);
            var expectedValue = "long id value";
            _mockRepository.GetSettingValueByIdAsync(settingId).Returns(expectedValue);

            // Act
            var result = await _settingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.Equal(expectedValue, result);
            await _mockRepository.Received(1).GetSettingValueByIdAsync(settingId);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithVeryLongValue_ReturnsLongValue()
        {
            // Arrange
            var settingId = "LongDescription";
            var expectedValue = new string('X', 5000);
            _mockRepository.GetSettingValueByIdAsync(settingId).Returns(expectedValue);

            // Act
            var result = await _settingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5000, result.Length);
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithBooleanTrueValue_ReturnsString()
        {
            // Arrange
            var settingId = "IsEnabled";
            var expectedValue = "true";
            _mockRepository.GetSettingValueByIdAsync(settingId).Returns(expectedValue);

            // Act
            var result = await _settingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.Equal("true", result);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithBooleanFalseValue_ReturnsString()
        {
            // Arrange
            var settingId = "IsEnabled";
            var expectedValue = "false";
            _mockRepository.GetSettingValueByIdAsync(settingId).Returns(expectedValue);

            // Act
            var result = await _settingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.Equal("false", result);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithJsonValue_ReturnsJsonString()
        {
            // Arrange
            var settingId = "Configuration";
            var expectedValue = "{\"key\":\"value\"}";
            _mockRepository.GetSettingValueByIdAsync(settingId).Returns(expectedValue);

            // Act
            var result = await _settingsService.GetSettingValueByIdAsync(settingId);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_CaseSensitiveId_CallsRepositoryWithExactCase()
        {
            // Arrange
            var settingId1 = "HoursInDay";
            var settingId2 = "hoursInDay";
            _mockRepository.GetSettingValueByIdAsync(settingId1).Returns("8.0");
            _mockRepository.GetSettingValueByIdAsync(settingId2).Returns("7.5");

            // Act
            var result1 = await _settingsService.GetSettingValueByIdAsync(settingId1);
            var result2 = await _settingsService.GetSettingValueByIdAsync(settingId2);

            // Assert
            Assert.Equal("8.0", result1);
            Assert.Equal("7.5", result2);
            await _mockRepository.Received(1).GetSettingValueByIdAsync(settingId1);
            await _mockRepository.Received(1).GetSettingValueByIdAsync(settingId2);
        }

        #endregion
    }
}
