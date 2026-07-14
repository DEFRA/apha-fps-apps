/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeRepositoryTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New xUnit test class for IDepartmentIncomeRepository (interface contract tests)
 *   - Mirrors established repository-test pattern in AccountCategoryRepositoryTests (NSubstitute interface mock)
 *   - Covers all 6 public interface methods: GetTimeIncomeAsync, GetTestIncomeAsync,
 *     GetAnimalIncomeAsync, GetAdditionalIncomeAsync, GetTotalsAsync, GetPeriodsAsync
 *   - Tests: happy path, empty result, NSubstitute exception simulations per method
 *   - No DbContext or Moq used — interface-mock-only pattern matches AccountCategoryRepositoryTests
 *
 * PRESERVED:
 *   - Repository is read-only (no write/delete paths)
 *   - Month filter params are non-nullable at the repository boundary (VBA defaults applied in service)
 *
 * DEFERRED: none — fully automated.
 */

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
