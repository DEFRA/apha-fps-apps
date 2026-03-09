using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;


namespace Apha.FPS.Application.UnitTests.Services.FpsSettingServiceTest
{
    public class FpsSettingServiceTest
    {
        private readonly IFpsSettingRepository _mockRepository;
        private readonly FpsSettingService _sut;

        public FpsSettingServiceTest()
        {
            _mockRepository = Substitute.For<IFpsSettingRepository>();
            _sut = new FpsSettingService(_mockRepository);
        }

        [Fact]
        public async Task GetAllSettingsAsync_WhenMultipleSettingsExist_ReturnsAllSettingsMappedToDtos()
        {
            // Arrange
            var settings = new List<FpsSetting>{
                new FpsSetting { Id = "1", Setting = "MaxFPS", Notes = "Maximum FPS limit", TestSetting = "Test1", FpsCalYear = 2024 },
                new FpsSetting { Id = "2", Setting = "MinFPS", Notes = "Minimum FPS limit", TestSetting = "Test2", FpsCalYear = 2023 },
                new FpsSetting { Id = "3", Setting = "AvgFPS", Notes = "Average FPS target", TestSetting = "Test3", FpsCalYear = 2024 }
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
            result[0].TestSetting.Should().Be("Test1");
            result[0].FpsCalYear.Should().Be(2024);

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
            var settings = new List<FpsSetting>
            {
            new FpsSetting { Id = "1", Setting = "DefaultFPS", Notes = "Default setting", TestSetting = "TestDefault", FpsCalYear = 2024 }
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
            result[0].TestSetting.Should().Be("TestDefault");
            result[0].FpsCalYear.Should().Be(2024);

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
    }
}

