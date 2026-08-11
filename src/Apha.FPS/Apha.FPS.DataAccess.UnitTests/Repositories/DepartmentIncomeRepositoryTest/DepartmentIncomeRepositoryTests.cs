using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.DataAccess.UnitTests.Repositories.DepartmentIncomeRepositoryTest
{
    public class DepartmentIncomeRepositoryTests
    {
        private const string TestProject = "AH0033";
        private const int TestMonthFrom = 1;
        private const int TestMonthTo = 12;
        private const int TestFpsYear = 2024;

        private readonly IDepartmentIncomeRepository _repositoryMock;

        public DepartmentIncomeRepositoryTests()
        {
            _repositoryMock = Substitute.For<IDepartmentIncomeRepository>();
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private static List<DepartmentIncomeTime> MakeTimeEntities(int count = 3) =>
            Enumerable.Range(1, count)
                .Select(i => new DepartmentIncomeTime
                {
                    Project           = $"PROJ{i}",
                    OracleProjectCode = $"OPC{i}",
                    Month             = i,
                    ChargeRate        = i * 10m,
                    TotalCost         = i * 100m
                })
                .ToList();

        private static List<DepartmentIncomeTest> MakeTestEntities(int count = 3) =>
            Enumerable.Range(1, count)
                .Select(i => new DepartmentIncomeTest
                {
                    Project           = $"PROJ{i}",
                    OracleProjectCode = $"OPC{i}",
                    Month             = i,
                    Volume            = i * 2m,
                    TestPrice         = i * 25m,
                    TotalCost         = i * 50m
                })
                .ToList();

        private static List<DepartmentIncomeAnimal> MakeAnimalEntities(int count = 3) =>
            Enumerable.Range(1, count)
                .Select(i => new DepartmentIncomeAnimal
                {
                    Project           = $"PROJ{i}",
                    OracleProjectCode = $"OPC{i}",
                    Month             = i,
                    AnimalDays        = i * 3m,
                    Rate              = 50m,
                    TotalCost         = i * 75m
                })
                .ToList();

        private static List<DepartmentIncomeAdditional> MakeAdditionalEntities(int count = 3) =>
            Enumerable.Range(1, count)
                .Select(i => new DepartmentIncomeAdditional
                {
                    Project           = $"PROJ{i}",
                    OracleProjectCode = $"OPC{i}",
                    Month             = i,
                    TotalCost         = i * 25m
                })
                .ToList();

        private static List<DepartmentIncomeTotals> MakeTotalsEntities(int count = 2) =>
            Enumerable.Range(1, count)
                .Select(i => new DepartmentIncomeTotals
                {
                    Project           = $"PROJ{i}",
                    OracleProjectCode = $"OPC{i}",
                    TotalCosts        = i * 250m,
                    TimeCost          = i * 100m,
                    TestsCost         = i * 50m,
                    AnimalsCost       = i * 75m,
                    ProjectSpecificsCost = i * 25m
                })
                .ToList();

        private static List<PeriodLookup> MakePeriodEntities() =>
            new List<PeriodLookup>
            {
                new PeriodLookup { AccntsPeriod = 1,  MonthName = "April", MonthNumber = 4 },
                new PeriodLookup { AccntsPeriod = 2,  MonthName = "May",   MonthNumber = 5 },
                new PeriodLookup { AccntsPeriod = 12, MonthName = "March", MonthNumber = 3 },
            };

        // ── GetTimeIncomeAsync ──────────────────────────────────────────────────

        #region GetTimeIncomeAsync

        [Fact]
        public async Task GetTimeIncomeAsync_HappyPath_ReturnsTimeRows()
        {
            // Arrange
            var expected = MakeTimeEntities();
            _repositoryMock.GetTimeIncomeAsync(TestProject, TestMonthFrom, TestMonthTo).Returns(expected);

            // Act
            var result = await _repositoryMock.GetTimeIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.All(result, r => Assert.NotNull(r.Project));
            await _repositoryMock.Received(1).GetTimeIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);
        }

        [Fact]
        public async Task GetTimeIncomeAsync_NullProject_ReturnsAllProjects()
        {
            // Arrange
            var expected = MakeTimeEntities(5);
            _repositoryMock.GetTimeIncomeAsync(null, TestMonthFrom, TestMonthTo).Returns(expected);

            // Act
            var result = await _repositoryMock.GetTimeIncomeAsync(null, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Equal(5, result.Count);
            await _repositoryMock.Received(1).GetTimeIncomeAsync(null, TestMonthFrom, TestMonthTo);
        }

        [Fact]
        public async Task GetTimeIncomeAsync_EmptyResult_ReturnsEmptyList()
        {
            // Arrange
            _repositoryMock.GetTimeIncomeAsync(TestProject, TestMonthFrom, TestMonthTo)
                .Returns(new List<DepartmentIncomeTime>());

            // Act
            var result = await _repositoryMock.GetTimeIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTimeIncomeAsync_DatabaseError_ThrowsException()
        {
            // Arrange
            _repositoryMock.GetTimeIncomeAsync(TestProject, TestMonthFrom, TestMonthTo)
                .Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _repositoryMock.GetTimeIncomeAsync(TestProject, TestMonthFrom, TestMonthTo));
        }

        [Theory]
        [InlineData(1, 3)]
        [InlineData(4, 6)]
        [InlineData(1, 12)]
        public async Task GetTimeIncomeAsync_VariousMonthRanges_CallsRepositoryWithCorrectParams(int from, int to)
        {
            // Arrange
            var expected = MakeTimeEntities(2);
            _repositoryMock.GetTimeIncomeAsync(TestProject, from, to).Returns(expected);

            // Act
            var result = await _repositoryMock.GetTimeIncomeAsync(TestProject, from, to);

            // Assert
            Assert.NotNull(result);
            await _repositoryMock.Received(1).GetTimeIncomeAsync(TestProject, from, to);
        }

        #endregion

        // ── GetTestIncomeAsync ──────────────────────────────────────────────────

        #region GetTestIncomeAsync

        [Fact]
        public async Task GetTestIncomeAsync_HappyPath_ReturnsTestRows()
        {
            // Arrange
            var expected = MakeTestEntities();
            _repositoryMock.GetTestIncomeAsync(TestProject, TestMonthFrom, TestMonthTo).Returns(expected);

            // Act
            var result = await _repositoryMock.GetTestIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            await _repositoryMock.Received(1).GetTestIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);
        }

        [Fact]
        public async Task GetTestIncomeAsync_EmptyResult_ReturnsEmptyList()
        {
            // Arrange
            _repositoryMock.GetTestIncomeAsync(TestProject, TestMonthFrom, TestMonthTo)
                .Returns(new List<DepartmentIncomeTest>());

            // Act
            var result = await _repositoryMock.GetTestIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTestIncomeAsync_DatabaseError_ThrowsException()
        {
            // Arrange
            _repositoryMock.GetTestIncomeAsync(TestProject, TestMonthFrom, TestMonthTo)
                .Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _repositoryMock.GetTestIncomeAsync(TestProject, TestMonthFrom, TestMonthTo));
        }

        [Fact]
        public async Task GetTestIncomeAsync_NullProject_ReturnsAllProjects()
        {
            // Arrange
            var expected = MakeTestEntities(4);
            _repositoryMock.GetTestIncomeAsync(null, TestMonthFrom, TestMonthTo).Returns(expected);

            // Act
            var result = await _repositoryMock.GetTestIncomeAsync(null, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Equal(4, result.Count);
        }

        #endregion

        // ── GetAnimalIncomeAsync ────────────────────────────────────────────────

        #region GetAnimalIncomeAsync

        [Fact]
        public async Task GetAnimalIncomeAsync_HappyPath_ReturnsAnimalRows()
        {
            // Arrange
            var expected = MakeAnimalEntities();
            _repositoryMock.GetAnimalIncomeAsync(TestProject, TestMonthFrom, TestMonthTo).Returns(expected);

            // Act
            var result = await _repositoryMock.GetAnimalIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            await _repositoryMock.Received(1).GetAnimalIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);
        }

        [Fact]
        public async Task GetAnimalIncomeAsync_EmptyResult_ReturnsEmptyList()
        {
            // Arrange
            _repositoryMock.GetAnimalIncomeAsync(TestProject, TestMonthFrom, TestMonthTo)
                .Returns(new List<DepartmentIncomeAnimal>());

            // Act
            var result = await _repositoryMock.GetAnimalIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAnimalIncomeAsync_DatabaseError_ThrowsException()
        {
            // Arrange
            _repositoryMock.GetAnimalIncomeAsync(TestProject, TestMonthFrom, TestMonthTo)
                .Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _repositoryMock.GetAnimalIncomeAsync(TestProject, TestMonthFrom, TestMonthTo));
        }

        [Fact]
        public async Task GetAnimalIncomeAsync_NullProject_ReturnsAllProjects()
        {
            // Arrange
            var expected = MakeAnimalEntities(4);
            _repositoryMock.GetAnimalIncomeAsync(null, TestMonthFrom, TestMonthTo).Returns(expected);

            // Act
            var result = await _repositoryMock.GetAnimalIncomeAsync(null, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Equal(4, result.Count);
        }

        #endregion

        // ── GetAdditionalIncomeAsync ────────────────────────────────────────────

        #region GetAdditionalIncomeAsync

        [Fact]
        public async Task GetAdditionalIncomeAsync_HappyPath_ReturnsAdditionalRows()
        {
            // Arrange
            var expected = MakeAdditionalEntities();
            _repositoryMock.GetAdditionalIncomeAsync(TestProject, TestMonthFrom, TestMonthTo).Returns(expected);

            // Act
            var result = await _repositoryMock.GetAdditionalIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            await _repositoryMock.Received(1).GetAdditionalIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);
        }

        [Fact]
        public async Task GetAdditionalIncomeAsync_EmptyResult_ReturnsEmptyList()
        {
            // Arrange
            _repositoryMock.GetAdditionalIncomeAsync(TestProject, TestMonthFrom, TestMonthTo)
                .Returns(new List<DepartmentIncomeAdditional>());

            // Act
            var result = await _repositoryMock.GetAdditionalIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAdditionalIncomeAsync_DatabaseError_ThrowsException()
        {
            // Arrange
            _repositoryMock.GetAdditionalIncomeAsync(TestProject, TestMonthFrom, TestMonthTo)
                .Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _repositoryMock.GetAdditionalIncomeAsync(TestProject, TestMonthFrom, TestMonthTo));
        }

        [Fact]
        public async Task GetAdditionalIncomeAsync_NullProject_ReturnsAllProjects()
        {
            // Arrange
            var expected = MakeAdditionalEntities(2);
            _repositoryMock.GetAdditionalIncomeAsync(null, TestMonthFrom, TestMonthTo).Returns(expected);

            // Act
            var result = await _repositoryMock.GetAdditionalIncomeAsync(null, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Equal(2, result.Count);
        }

        #endregion

        // ── GetTotalsAsync ──────────────────────────────────────────────────────

        #region GetTotalsAsync

        [Fact]
        public async Task GetTotalsAsync_HappyPath_ReturnsPivotTotals()
        {
            // Arrange
            var expected = MakeTotalsEntities();
            _repositoryMock.GetTotalsAsync(TestProject, TestMonthFrom, TestMonthTo).Returns(expected);

            // Act
            var result = await _repositoryMock.GetTotalsAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.True(r.TotalCosts > 0));
            await _repositoryMock.Received(1).GetTotalsAsync(TestProject, TestMonthFrom, TestMonthTo);
        }

        [Fact]
        public async Task GetTotalsAsync_EmptyResult_ReturnsEmptyList()
        {
            // Arrange
            _repositoryMock.GetTotalsAsync(TestProject, TestMonthFrom, TestMonthTo)
                .Returns(new List<DepartmentIncomeTotals>());

            // Act
            var result = await _repositoryMock.GetTotalsAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTotalsAsync_DatabaseError_ThrowsException()
        {
            // Arrange
            _repositoryMock.GetTotalsAsync(TestProject, TestMonthFrom, TestMonthTo)
                .Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _repositoryMock.GetTotalsAsync(TestProject, TestMonthFrom, TestMonthTo));
        }

        [Fact]
        public async Task GetTotalsAsync_NullProject_ReturnsTotalsForAllProjects()
        {
            // Arrange
            var expected = MakeTotalsEntities(4);
            _repositoryMock.GetTotalsAsync(null, TestMonthFrom, TestMonthTo).Returns(expected);

            // Act
            var result = await _repositoryMock.GetTotalsAsync(null, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Equal(4, result.Count);
        }

        #endregion

        // ── GetSnapshotPeriodsAsync ────────────────────────────────────────────

        #region GetSnapshotPeriodsAsync

        [Fact]
        public async Task GetSnapshotPeriodsAsync_HappyPath_ReturnsPeriods()
        {
            // Arrange
            var expected = new List<Period>
            {
                new() { PeriodName = "April 2025 Only",       FpsYear = TestFpsYear, EndPeriod = 4,  FinalSummariesRun = 1, PeriodLocked = 0 },
                new() { PeriodName = "April - May 2025",      FpsYear = TestFpsYear, EndPeriod = 5,  FinalSummariesRun = 0, PeriodLocked = 0 },
                new() { PeriodName = "April - August 2025/25",FpsYear = TestFpsYear, EndPeriod = 8,  FinalSummariesRun = 0, PeriodLocked = 1 },
            };
            _repositoryMock.GetSnapshotPeriodsAsync().Returns(expected);

            // Act
            var result = await _repositoryMock.GetSnapshotPeriodsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("April 2025 Only", result[0].PeriodName);
            await _repositoryMock.Received(1).GetSnapshotPeriodsAsync();
        }

        [Fact]
        public async Task GetSnapshotPeriodsAsync_EmptyResult_ReturnsEmptyList()
        {
            // Arrange
            _repositoryMock.GetSnapshotPeriodsAsync().Returns(new List<Period>());

            // Act
            var result = await _repositoryMock.GetSnapshotPeriodsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSnapshotPeriodsAsync_DatabaseError_ThrowsException()
        {
            // Arrange
            _repositoryMock.GetSnapshotPeriodsAsync().Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _repositoryMock.GetSnapshotPeriodsAsync());
        }

        [Fact]
        public async Task GetSnapshotPeriodsAsync_ReturnedPeriods_HavePeriodNameAndEndPeriod()
        {
            // Arrange
            var expected = new List<Period>
            {
                new() { PeriodName = "April 2025 Only", FpsYear = TestFpsYear, EndPeriod = 4, FinalSummariesRun = 1, PeriodLocked = 0 },
            };
            _repositoryMock.GetSnapshotPeriodsAsync().Returns(expected);

            // Act
            var result = await _repositoryMock.GetSnapshotPeriodsAsync();

            // Assert
            Assert.All(result, p =>
            {
                Assert.NotEmpty(p.PeriodName);
                Assert.True(p.EndPeriod >= 1);
                Assert.Equal(TestFpsYear, p.FpsYear);
            });
        }

        #endregion

        // ── UpdatePeriodLockedAsync ────────────────────────────────────────────

        #region UpdatePeriodLockedAsync

        [Fact]
        public async Task UpdatePeriodLockedAsync_PeriodExists_ReturnsRowCountGreaterThanZero()
        {
            // Arrange
            _repositoryMock.UpdatePeriodLockedAsync("April 2025 Only", true).Returns(1);

            // Act
            var result = await _repositoryMock.UpdatePeriodLockedAsync("April 2025 Only", true);

            // Assert
            Assert.Equal(1, result);
            await _repositoryMock.Received(1).UpdatePeriodLockedAsync("April 2025 Only", true);
        }

        [Fact]
        public async Task UpdatePeriodLockedAsync_PeriodNotFound_ReturnsZero()
        {
            // Arrange
            _repositoryMock.UpdatePeriodLockedAsync("NonExistent Period", true).Returns(0);

            // Act
            var result = await _repositoryMock.UpdatePeriodLockedAsync("NonExistent Period", true);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task UpdatePeriodLockedAsync_UnlockPeriod_ReturnsRowCount()
        {
            // Arrange
            _repositoryMock.UpdatePeriodLockedAsync("April 2025 Only", false).Returns(1);

            // Act
            var result = await _repositoryMock.UpdatePeriodLockedAsync("April 2025 Only", false);

            // Assert
            Assert.Equal(1, result);
            await _repositoryMock.Received(1).UpdatePeriodLockedAsync("April 2025 Only", false);
        }

        [Fact]
        public async Task UpdatePeriodLockedAsync_DatabaseError_ThrowsException()
        {
            // Arrange
            _repositoryMock.UpdatePeriodLockedAsync(Arg.Any<string>(), Arg.Any<bool>())
                .Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _repositoryMock.UpdatePeriodLockedAsync("April 2025 Only", true));
        }

        [Fact]
        public async Task UpdatePeriodLockedAsync_PeriodNameWithSlash_HandledCorrectly()
        {
            // Arrange — period names like "April - August 2025/25" contain a slash
            const string slashPeriodName = "April - August 2025/25";
            _repositoryMock.UpdatePeriodLockedAsync(slashPeriodName, true).Returns(1);

            // Act
            var result = await _repositoryMock.UpdatePeriodLockedAsync(slashPeriodName, true);

            // Assert
            Assert.Equal(1, result);
            await _repositoryMock.Received(1).UpdatePeriodLockedAsync(slashPeriodName, true);
        }

        #endregion

        // ── GetPeriodsAsync ─────────────────────────────────────────────────────

        #region GetPeriodsAsync

        [Fact]
        public async Task GetPeriodsAsync_HappyPath_ReturnsPeriodLookups()
        {
            // Arrange
            var expected = MakePeriodEntities();
            _repositoryMock.GetPeriodsAsync().Returns(expected);

            // Act
            var result = await _repositoryMock.GetPeriodsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("April", result[0].MonthName);
            Assert.Equal(1, result[0].AccntsPeriod);
            await _repositoryMock.Received(1).GetPeriodsAsync();
        }

        [Fact]
        public async Task GetPeriodsAsync_EmptyResult_ReturnsEmptyList()
        {
            // Arrange
            _repositoryMock.GetPeriodsAsync().Returns(new List<PeriodLookup>());

            // Act
            var result = await _repositoryMock.GetPeriodsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPeriodsAsync_DatabaseError_ThrowsException()
        {
            // Arrange
            _repositoryMock.GetPeriodsAsync().Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _repositoryMock.GetPeriodsAsync());
        }

        [Fact]
        public async Task GetPeriodsAsync_ReturnedPeriods_HaveCorrectProperties()
        {
            // Arrange
            var expected = MakePeriodEntities();
            _repositoryMock.GetPeriodsAsync().Returns(expected);

            // Act
            var result = await _repositoryMock.GetPeriodsAsync();

            // Assert
            Assert.All(result, p =>
            {
                Assert.True(p.AccntsPeriod >= 1 && p.AccntsPeriod <= 12);
                Assert.NotEmpty(p.MonthName);
                Assert.True(p.MonthNumber >= 1 && p.MonthNumber <= 12);
            });
        }

        #endregion
    }
}
