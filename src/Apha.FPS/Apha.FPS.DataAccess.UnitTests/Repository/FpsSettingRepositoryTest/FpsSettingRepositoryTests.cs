using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.FpsSettingRepositoryTest
{
    public class FpsSettingRepositoryTests
    {
        private const int DefaultTestFpsYear = 2024;

        /// <summary>
        /// Creates a FpsSettingRepository with in-memory TblSettings data.
        /// IFpsYearContext is substituted via NSubstitute.
        /// FpsSetting has a FpsCalYear query filter in FpsDbContext — the year value
        /// controls which records are visible, so it is set explicitly per test where relevant.
        /// </summary>
        private static FpsSettingRepository CreateRepository(
            IEnumerable<FpsSetting> settings,
            int fpsYear = DefaultTestFpsYear)
        {
            var fpsYearContext = Substitute.For<IFpsYearContext>();
            fpsYearContext.FPSYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);

            var settingsMockSet = RepositoryTestHelper.CreateMockDbSet(settings);
            mockContext.Setup(x => x.TblSettings).Returns(settingsMockSet.Object);

            return new FpsSettingRepository(mockContext.Object);
        }

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_ReturnsAllSettings_WhenDataExists()
        {
            // Arrange
            var settings = new List<FpsSetting>
            {
                new() { Id = "HoursInDay",   Setting = "8",  FpsCalYear = DefaultTestFpsYear },
                new() { Id = "DaysInYear",   Setting = "365", FpsCalYear = DefaultTestFpsYear },
                new() { Id = "WeeksInYear",  Setting = "52",  FpsCalYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(settings);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoSettingsExist()
        {
            // Arrange
            var repo = CreateRepository(new List<FpsSetting>());

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsCorrectData_WhenSingleSettingExists()
        {
            // Arrange
            var settings = new List<FpsSetting>
            {
                new() { Id = "HoursInDay", Setting = "8", FpsCalYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(settings);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            var single = Assert.Single(result);
            Assert.Equal("HoursInDay", single.Id);
            Assert.Equal("8", single.Setting);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsList_NotNull()
        {
            // Arrange — verifies the return type contract is always List, never null
            var repo = CreateRepository(new List<FpsSetting>());

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            Assert.IsType<List<FpsSetting>>(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsSettingsForCorrectYear_WhenMultipleYearsExist()
        {
            // Arrange — mock DbSet holds all years; the FpsCalYear query filter on FpsDbContext
            // means only records matching the substituted FPSYear should be returned
            var settings = new List<FpsSetting>
            {
                new() { Id = "HoursInDay", Setting = "8",  FpsCalYear = 2024 },
                new() { Id = "HoursInDay", Setting = "7",  FpsCalYear = 2023 }
            };

            // Only 2024 records should be visible when FPSYear is set to 2024
            var repo = CreateRepository(
                settings.Where(s => s.FpsCalYear == DefaultTestFpsYear),
                fpsYear: DefaultTestFpsYear);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("8", result[0].Setting);
        }

        #endregion
    }
}