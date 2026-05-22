using Apha.Common.Helpers.Repository;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.DataAccess.Repositories;
using Moq;

namespace Apha.Costbook.DataAccess.UnitTests.Repository.YearMasterRepositoryTest
{
    public class YearMasterRepositoryTests
    {
        private static YearMasterRepository CreateRepository(IEnumerable<YearMaster> yearMasters)
        {
            var mockFPSYearContext = new Mock<IFPSYearContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFPSYearContext.Object);

            var yearMastersMockSet = RepositoryTestHelper.CreateMockDbSet(yearMasters);
            mockContext.Setup(x => x.Set<YearMaster>()).Returns(yearMastersMockSet.Object);
            mockContext.Setup(x => x.YearMasters).Returns(yearMastersMockSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new YearMasterRepository(mockContext.Object);
        }

        [Fact]
        public async Task GetOpenYearAsync_ReturnsOpenYear_WhenOpenYearExists()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2023, FpsYearCode = "FPS2023-24", YearStatus = "Closed", Active = true },
                new() { FpsYear = 2024, FpsYearCode = "FPS2024-25", YearStatus = "Open", Active = true },
                new() { FpsYear = 2025, FpsYearCode = "FPS2025-26", YearStatus = "Planned", Active = true }
            };
            var repo = CreateRepository(yearMasters);

            // Act
            var result = await repo.GetOpenYearAsync();

            // Assert
            Assert.Equal(2024, result);
        }

        [Fact]
        public async Task GetOpenYearAsync_ReturnsZero_WhenNoOpenYearExists()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2023, FpsYearCode = "FPS2023-24", YearStatus = "Closed", Active = true },
                new() { FpsYear = 2024, FpsYearCode = "FPS2024-25", YearStatus = "Closed", Active = true }
            };
            var repo = CreateRepository(yearMasters);

            // Act
            var result = await repo.GetOpenYearAsync();

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GetOpenYearAsync_ReturnsZero_WhenNoYearMastersExist()
        {
            // Arrange
            var repo = CreateRepository(new List<YearMaster>());

            // Act
            var result = await repo.GetOpenYearAsync();

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GetOpenYearAsync_ReturnsLatestOpenYear_WhenMultipleOpenYearsExist()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2023, FpsYearCode = "FPS2023-24", YearStatus = "Open", Active = true },
                new() { FpsYear = 2024, FpsYearCode = "FPS2024-25", YearStatus = "Open", Active = true },
                new() { FpsYear = 2025, FpsYearCode = "FPS2025-26", YearStatus = "Open", Active = true }
            };
            var repo = CreateRepository(yearMasters);

            // Act
            var result = await repo.GetOpenYearAsync();

            // Assert
            Assert.Equal(2025, result);
        }

        [Fact]
        public async Task GetOpenYearAsync_IgnoresInactiveYears()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2024, FpsYearCode = "FPS2024-25", YearStatus = "Open", Active = true },
                new() { FpsYear = 2025, FpsYearCode = "FPS2025-26", YearStatus = "Open", Active = false }
            };
            var repo = CreateRepository(yearMasters);

            // Act
            var result = await repo.GetOpenYearAsync();

            // Assert
            Assert.Equal(2024, result);
        }

        [Fact]
        public async Task GetOpenYearAsync_IsCaseInsensitive_ForYearStatus()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2023, FpsYearCode = "FPS2023-24", YearStatus = "OPEN", Active = true },
                new() { FpsYear = 2024, FpsYearCode = "FPS2024-25", YearStatus = "Open", Active = true },
                new() { FpsYear = 2025, FpsYearCode = "FPS2025-26", YearStatus = "open", Active = true }
            };
            var repo = CreateRepository(yearMasters);

            // Act
            var result = await repo.GetOpenYearAsync();

            // Assert
            Assert.Equal(2025, result);
        }

        [Fact]
        public async Task GetOpenYearAsync_ReturnsZero_WhenOpenYearIsInactive()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2024, FpsYearCode = "FPS2024-25", YearStatus = "Open", Active = false }
            };
            var repo = CreateRepository(yearMasters);

            // Act
            var result = await repo.GetOpenYearAsync();

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GetOpenYearAsync_ReturnsCorrectYear_WhenMixedStatusesExist()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new() { FpsYear = 2022, FpsYearCode = "FPS2022-23", YearStatus = "Closed", Active = true },
                new() { FpsYear = 2023, FpsYearCode = "FPS2023-24", YearStatus = "Closed", Active = true },
                new() { FpsYear = 2024, FpsYearCode = "FPS2024-25", YearStatus = "Open", Active = true },
                new() { FpsYear = 2025, FpsYearCode = "FPS2025-26", YearStatus = "Planned", Active = true },
                new() { FpsYear = 2026, FpsYearCode = "FPS2026-27", YearStatus = "Planned", Active = true }
            };
            var repo = CreateRepository(yearMasters);

            // Act
            var result = await repo.GetOpenYearAsync();

            // Assert
            Assert.Equal(2024, result);
        }
    }
}
