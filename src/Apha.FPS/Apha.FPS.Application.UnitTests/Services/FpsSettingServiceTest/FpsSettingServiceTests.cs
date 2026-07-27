using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;


namespace Apha.FPS.Application.UnitTests.Services.FpsSettingServiceTest
{
    public class FpsSettingServiceTests
    {
        private readonly IFpsSettingRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly FpsSettingService _sut;

        public FpsSettingServiceTests()
        {
            _mockRepository = Substitute.For<IFpsSettingRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new FpsSettingService(_mockRepository, _mockMapper);
           
        }

        #region GetAllSettingsAsync

        [Fact]
        public async Task GetAllSettingsAsync_WhenMultipleSettingsExist_ReturnsAllSettingsMappedToDtos()
        {
            // Arrange
            var updatedAt = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);
            var settings = new List<FpsSetting>
            {
                new FpsSetting { Id = "1", Setting = "MaxFPS", Notes = "Maximum FPS limit", UpdatedBy = "user1", UpdatedAt = updatedAt, FpsYear = 2024 },
                new FpsSetting { Id = "2", Setting = "MinFPS", Notes = "Minimum FPS limit", UpdatedBy = "user2", UpdatedAt = updatedAt, FpsYear = 2023 },
                new FpsSetting { Id = "3", Setting = "AvgFPS", Notes = "Average FPS target", UpdatedBy = "user3", UpdatedAt = updatedAt, FpsYear = 2024 }
            };
            _mockRepository.GetAllAsync().Returns(settings);

            // Act
            var result = await _sut.GetAllSettingsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);

            result[0].Id.Should().Be("1");
            result[0].Setting.Should().Be("MaxFPS");
            result[0].Notes.Should().Be("Maximum FPS limit");
            result[0].UpdatedBy.Should().Be("user1");
            result[0].UpdatedAt.Should().Be(updatedAt);
            result[0].FpsYear.Should().Be(2024);

            result[1].Id.Should().Be("2");
            result[1].Setting.Should().Be("MinFPS");

            result[2].Id.Should().Be("3");
            result[2].Setting.Should().Be("AvgFPS");

            await _mockRepository.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAllSettingsAsync_WhenNoSettingsExist_ReturnsEmptyList()
        {
            // Arrange
            var emptySettings = new List<FpsSetting>();
            _mockRepository.GetAllAsync().Returns(emptySettings);

            // Act
            var result = await _sut.GetAllSettingsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            result.Should().HaveCount(0);

            await _mockRepository.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAllSettingsAsync_WhenSingleSettingExists_ReturnsSingleDtoInList()
        {
            // Arrange
            var updatedAt = new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc);
            var settings = new List<FpsSetting>
            {
                new FpsSetting { Id = "1", Setting = "DefaultFPS", Notes = "Default setting", UpdatedBy = "admin", UpdatedAt = updatedAt, FpsYear = 2024 }
            };
            _mockRepository.GetAllAsync().Returns(settings);

            // Act
            var result = await _sut.GetAllSettingsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Id.Should().Be("1");
            result[0].Setting.Should().Be("DefaultFPS");
            result[0].Notes.Should().Be("Default setting");
            result[0].UpdatedBy.Should().Be("admin");
            result[0].UpdatedAt.Should().Be(updatedAt);
            result[0].FpsYear.Should().Be(2024);

            await _mockRepository.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAllSettingsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.GetAllAsync().Throws(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetAllSettingsAsync());
            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetAllAsync();
        }

        #endregion

        #region GetHoursPerDayAsync

        [Fact]
        public async Task GetHoursPerDayAsync_WhenSettingExistsWithValidValue_ReturnsParsedDecimal()
        {
            // Arrange
            var setting = new FpsSetting { Id = "HoursInDay", Setting = "7.5", FpsYear = 2024 };
            _mockRepository.GetByKeyAsync("HoursInDay").Returns(setting);

            // Act
            var result = await _sut.GetHoursPerDayAsync();

            // Assert
            result.Should().Be(7.5m);
            await _mockRepository.Received(1).GetByKeyAsync("HoursInDay");
        }

        [Fact]
        public async Task GetHoursPerDayAsync_WhenSettingExistsWithIntegerValue_ReturnsParsedDecimal()
        {
            // Arrange
            var setting = new FpsSetting { Id = "HoursInDay", Setting = "8", FpsYear = 2024 };
            _mockRepository.GetByKeyAsync("HoursInDay").Returns(setting);

            // Act
            var result = await _sut.GetHoursPerDayAsync();

            // Assert
            result.Should().Be(8m);
            await _mockRepository.Received(1).GetByKeyAsync("HoursInDay");
        }

        [Fact]
        public async Task GetHoursPerDayAsync_WhenSettingDoesNotExist_ReturnsDefaultEight()
        {
            // Arrange
            _mockRepository.GetByKeyAsync("HoursInDay").Returns((FpsSetting?)null);

            // Act
            var result = await _sut.GetHoursPerDayAsync();

            // Assert
            result.Should().Be(8m);
            await _mockRepository.Received(1).GetByKeyAsync("HoursInDay");
        }

        [Fact]
        public async Task GetHoursPerDayAsync_WhenSettingValueIsNotNumeric_ReturnsDefaultEight()
        {
            // Arrange
            var setting = new FpsSetting { Id = "HoursInDay", Setting = "not-a-number", FpsYear = 2024 };
            _mockRepository.GetByKeyAsync("HoursInDay").Returns(setting);

            // Act
            var result = await _sut.GetHoursPerDayAsync();

            // Assert
            result.Should().Be(8m);
            await _mockRepository.Received(1).GetByKeyAsync("HoursInDay");
        }

        [Fact]
        public async Task GetHoursPerDayAsync_WhenSettingValueIsNull_ReturnsDefaultEight()
        {
            // Arrange
            var setting = new FpsSetting { Id = "HoursInDay", Setting = null, FpsYear = 2024 };
            _mockRepository.GetByKeyAsync("HoursInDay").Returns(setting);

            // Act
            var result = await _sut.GetHoursPerDayAsync();

            // Assert
            result.Should().Be(8m);
            await _mockRepository.Received(1).GetByKeyAsync("HoursInDay");
        }

        [Fact]
        public async Task GetHoursPerDayAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.GetByKeyAsync("HoursInDay").Throws(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetHoursPerDayAsync());
            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetByKeyAsync("HoursInDay");
        }

        #endregion
    }
}

