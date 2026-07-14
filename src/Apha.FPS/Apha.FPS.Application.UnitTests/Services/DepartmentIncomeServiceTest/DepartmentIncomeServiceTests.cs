/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeServiceTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New xUnit test class for DepartmentIncomeService (backend Application layer)
 *   - Covers all 6 public methods: GetTimeIncomeAsync, GetTestIncomeAsync, GetAnimalIncomeAsync,
 *     GetAdditionalIncomeAsync, GetTotalsAsync, GetPeriodsAsync
 *   - Tests: happy path, empty result, repository exception propagation per method
 *   - Verifies VBA month-default helpers (ResolveMonthFrom / ResolveMonthTo) via resolved int params to repo
 *   - NSubstitute mocks for IDepartmentIncomeRepository and IMapper
 *
 * PRESERVED:
 *   - Service is read-only (no write paths to test)
 *   - VBA default: monthFrom=null → 1; monthTo=null when monthFrom=1 → 12
 *
 * DEFERRED: none — fully automated.
 */

using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.DepartmentIncomeServiceTest
{
    public class DepartmentIncomeServiceTests
    {
        private const string TestProject = "AH0033";

        private readonly IDepartmentIncomeRepository _repository;
        private readonly IMapper _mapper;
        private readonly DepartmentIncomeService _service;

        public DepartmentIncomeServiceTests()
        {
            _repository = Substitute.For<IDepartmentIncomeRepository>();
            _mapper     = Substitute.For<IMapper>();
            _service    = new DepartmentIncomeService(_repository, _mapper);
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private static List<DepartmentIncomeTime> MakeTimeEntities(int count = 2) =>
            Enumerable.Range(1, count)
                .Select(i => new DepartmentIncomeTime { Project = $"PROJ{i}", Month = i, TotalCost = i * 100m })
                .ToList();

        private static List<DepartmentIncomeTimeDto> MakeTimeDtos(int count = 2) =>
            Enumerable.Range(1, count)
                .Select(i => new DepartmentIncomeTimeDto { Project = $"PROJ{i}", Month = i, TotalCost = i * 100m })
                .ToList();

        private static List<DepartmentIncomeTest> MakeTestEntities(int count = 2) =>
            Enumerable.Range(1, count)
                .Select(i => new DepartmentIncomeTest { Project = $"PROJ{i}", Month = i, TotalCost = i * 50m })
                .ToList();

        private static List<DepartmentIncomeTestDto> MakeTestDtos(int count = 2) =>
            Enumerable.Range(1, count)
                .Select(i => new DepartmentIncomeTestDto { Project = $"PROJ{i}", Month = i, TotalCost = i * 50m })
                .ToList();

        private static List<DepartmentIncomeAnimal> MakeAnimalEntities(int count = 2) =>
            Enumerable.Range(1, count)
                .Select(i => new DepartmentIncomeAnimal { Project = $"PROJ{i}", Month = i, TotalCost = i * 75m })
                .ToList();

        private static List<DepartmentIncomeAnimalDto> MakeAnimalDtos(int count = 2) =>
            Enumerable.Range(1, count)
                .Select(i => new DepartmentIncomeAnimalDto { Project = $"PROJ{i}", Month = i, TotalCost = i * 75m })
                .ToList();

        private static List<DepartmentIncomeAdditional> MakeAdditionalEntities(int count = 2) =>
            Enumerable.Range(1, count)
                .Select(i => new DepartmentIncomeAdditional { Project = $"PROJ{i}", Month = i, TotalCost = i * 25m })
                .ToList();

        private static List<DepartmentIncomeAdditionalDto> MakeAdditionalDtos(int count = 2) =>
            Enumerable.Range(1, count)
                .Select(i => new DepartmentIncomeAdditionalDto { Project = $"PROJ{i}", Month = i, TotalCost = i * 25m })
                .ToList();

        private static List<DepartmentIncomeTotals> MakeTotalsEntities(int count = 2) =>
            Enumerable.Range(1, count)
                .Select(i => new DepartmentIncomeTotals { Project = $"PROJ{i}", TotalCosts = i * 250m })
                .ToList();

        private static List<DepartmentIncomeTotalsDto> MakeTotalsDtos(int count = 2) =>
            Enumerable.Range(1, count)
                .Select(i => new DepartmentIncomeTotalsDto { Project = $"PROJ{i}", TotalCosts = i * 250m })
                .ToList();

        private static List<PeriodLookup> MakePeriodEntities() =>
            new List<PeriodLookup>
            {
                new PeriodLookup { AccntsPeriod = 1, MonthName = "April", MonthNumber = 4 },
                new PeriodLookup { AccntsPeriod = 2, MonthName = "May",   MonthNumber = 5 },
            };

        private static List<PeriodLookupDto> MakePeriodDtos() =>
            new List<PeriodLookupDto>
            {
                new PeriodLookupDto { AccntsPeriod = 1, MonthName = "April", MonthNumber = 4 },
                new PeriodLookupDto { AccntsPeriod = 2, MonthName = "May",   MonthNumber = 5 },
            };

        // ── GetTimeIncomeAsync ──────────────────────────────────────────────────

        #region GetTimeIncomeAsync

        [Fact]
        public async Task GetTimeIncomeAsync_WithProjectAndMonthRange_ReturnsMapperResult()
        {
            // Arrange
            var entities = MakeTimeEntities();
            var dtos     = MakeTimeDtos();

            // TRANSFORMENGINE: VBA defaults: monthFrom=3 → 3, monthTo=6 → 6
            _repository.GetTimeIncomeAsync(TestProject, 3, 6).Returns(entities);
            _mapper.Map<List<DepartmentIncomeTimeDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetTimeIncomeAsync(TestProject, 3, 6);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            await _repository.Received(1).GetTimeIncomeAsync(TestProject, 3, 6);
            _mapper.Received(1).Map<List<DepartmentIncomeTimeDto>>(entities);
        }

        [Fact]
        public async Task GetTimeIncomeAsync_NullMonthFromAndNullMonthTo_AppliesVbaDefaults()
        {
            // Arrange
            // TRANSFORMENGINE: VBA defaults: monthFrom=null→1, monthTo=null+monthFrom=1→12
            var entities = MakeTimeEntities();
            var dtos     = MakeTimeDtos();

            _repository.GetTimeIncomeAsync(TestProject, 1, 12).Returns(entities);
            _mapper.Map<List<DepartmentIncomeTimeDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetTimeIncomeAsync(TestProject, null, null);

            // Assert
            Assert.NotNull(result);
            await _repository.Received(1).GetTimeIncomeAsync(TestProject, 1, 12);
        }

        [Fact]
        public async Task GetTimeIncomeAsync_NullMonthFromNonOneResult_AppliesMonthFromDefault()
        {
            // Arrange
            // TRANSFORMENGINE: VBA defaults: monthFrom=null→1, monthTo=5→5
            var entities = MakeTimeEntities();
            var dtos     = MakeTimeDtos();

            _repository.GetTimeIncomeAsync(TestProject, 1, 5).Returns(entities);
            _mapper.Map<List<DepartmentIncomeTimeDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetTimeIncomeAsync(TestProject, null, 5);

            // Assert
            Assert.NotNull(result);
            await _repository.Received(1).GetTimeIncomeAsync(TestProject, 1, 5);
        }

        [Fact]
        public async Task GetTimeIncomeAsync_NonOneMonthFromNullMonthTo_MonthToDefaultsToMonthFrom()
        {
            // Arrange
            // TRANSFORMENGINE: VBA defaults: monthFrom=4→4, monthTo=null+monthFrom≠1→4
            var entities = MakeTimeEntities();
            var dtos     = MakeTimeDtos();

            _repository.GetTimeIncomeAsync(TestProject, 4, 4).Returns(entities);
            _mapper.Map<List<DepartmentIncomeTimeDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetTimeIncomeAsync(TestProject, 4, null);

            // Assert
            Assert.NotNull(result);
            await _repository.Received(1).GetTimeIncomeAsync(TestProject, 4, 4);
        }

        [Fact]
        public async Task GetTimeIncomeAsync_ServiceReturnsEmpty_ReturnsEmptyList()
        {
            // Arrange
            _repository.GetTimeIncomeAsync(Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<int>())
                .Returns(new List<DepartmentIncomeTime>());
            _mapper.Map<List<DepartmentIncomeTimeDto>>(Arg.Any<List<DepartmentIncomeTime>>())
                .Returns(new List<DepartmentIncomeTimeDto>());

            // Act
            var result = await _service.GetTimeIncomeAsync(null, null, null);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTimeIncomeAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _repository.GetTimeIncomeAsync(Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<int>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.GetTimeIncomeAsync(TestProject, 1, 6));
        }

        #endregion

        // ── GetTestIncomeAsync ──────────────────────────────────────────────────

        #region GetTestIncomeAsync

        [Fact]
        public async Task GetTestIncomeAsync_WithProjectAndMonthRange_ReturnsMapperResult()
        {
            // Arrange
            var entities = MakeTestEntities();
            var dtos     = MakeTestDtos();

            _repository.GetTestIncomeAsync(TestProject, 2, 8).Returns(entities);
            _mapper.Map<List<DepartmentIncomeTestDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetTestIncomeAsync(TestProject, 2, 8);

            // Assert
            Assert.Equal(2, result.Count);
            await _repository.Received(1).GetTestIncomeAsync(TestProject, 2, 8);
        }

        [Fact]
        public async Task GetTestIncomeAsync_NullParams_AppliesVbaDefaults()
        {
            // Arrange
            var entities = MakeTestEntities();
            var dtos     = MakeTestDtos();

            _repository.GetTestIncomeAsync(null, 1, 12).Returns(entities);
            _mapper.Map<List<DepartmentIncomeTestDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetTestIncomeAsync(null, null, null);

            // Assert
            Assert.NotNull(result);
            await _repository.Received(1).GetTestIncomeAsync(null, 1, 12);
        }

        [Fact]
        public async Task GetTestIncomeAsync_ServiceReturnsEmpty_ReturnsEmptyList()
        {
            // Arrange
            _repository.GetTestIncomeAsync(Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<int>())
                .Returns(new List<DepartmentIncomeTest>());
            _mapper.Map<List<DepartmentIncomeTestDto>>(Arg.Any<List<DepartmentIncomeTest>>())
                .Returns(new List<DepartmentIncomeTestDto>());

            // Act
            var result = await _service.GetTestIncomeAsync(null, null, null);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTestIncomeAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _repository.GetTestIncomeAsync(Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<int>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.GetTestIncomeAsync(TestProject, 1, 6));
        }

        #endregion

        // ── GetAnimalIncomeAsync ────────────────────────────────────────────────

        #region GetAnimalIncomeAsync

        [Fact]
        public async Task GetAnimalIncomeAsync_WithProjectAndMonthRange_ReturnsMapperResult()
        {
            // Arrange
            var entities = MakeAnimalEntities();
            var dtos     = MakeAnimalDtos();

            _repository.GetAnimalIncomeAsync(TestProject, 1, 6).Returns(entities);
            _mapper.Map<List<DepartmentIncomeAnimalDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetAnimalIncomeAsync(TestProject, 1, 6);

            // Assert
            Assert.Equal(2, result.Count);
            await _repository.Received(1).GetAnimalIncomeAsync(TestProject, 1, 6);
        }

        [Fact]
        public async Task GetAnimalIncomeAsync_NullParams_AppliesVbaDefaults()
        {
            // Arrange
            _repository.GetAnimalIncomeAsync(null, 1, 12).Returns(MakeAnimalEntities());
            _mapper.Map<List<DepartmentIncomeAnimalDto>>(Arg.Any<List<DepartmentIncomeAnimal>>())
                .Returns(MakeAnimalDtos());

            // Act
            var result = await _service.GetAnimalIncomeAsync(null, null, null);

            // Assert
            Assert.NotNull(result);
            await _repository.Received(1).GetAnimalIncomeAsync(null, 1, 12);
        }

        [Fact]
        public async Task GetAnimalIncomeAsync_ServiceReturnsEmpty_ReturnsEmptyList()
        {
            // Arrange
            _repository.GetAnimalIncomeAsync(Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<int>())
                .Returns(new List<DepartmentIncomeAnimal>());
            _mapper.Map<List<DepartmentIncomeAnimalDto>>(Arg.Any<List<DepartmentIncomeAnimal>>())
                .Returns(new List<DepartmentIncomeAnimalDto>());

            // Act
            var result = await _service.GetAnimalIncomeAsync(null, null, null);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAnimalIncomeAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _repository.GetAnimalIncomeAsync(Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<int>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.GetAnimalIncomeAsync(TestProject, 1, 12));
        }

        #endregion

        // ── GetAdditionalIncomeAsync ────────────────────────────────────────────

        #region GetAdditionalIncomeAsync

        [Fact]
        public async Task GetAdditionalIncomeAsync_WithProjectAndMonthRange_ReturnsMapperResult()
        {
            // Arrange
            var entities = MakeAdditionalEntities();
            var dtos     = MakeAdditionalDtos();

            _repository.GetAdditionalIncomeAsync(TestProject, 1, 12).Returns(entities);
            _mapper.Map<List<DepartmentIncomeAdditionalDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetAdditionalIncomeAsync(TestProject, 1, 12);

            // Assert
            Assert.Equal(2, result.Count);
            await _repository.Received(1).GetAdditionalIncomeAsync(TestProject, 1, 12);
        }

        [Fact]
        public async Task GetAdditionalIncomeAsync_NullParams_AppliesVbaDefaults()
        {
            // Arrange
            _repository.GetAdditionalIncomeAsync(null, 1, 12).Returns(MakeAdditionalEntities());
            _mapper.Map<List<DepartmentIncomeAdditionalDto>>(Arg.Any<List<DepartmentIncomeAdditional>>())
                .Returns(MakeAdditionalDtos());

            // Act
            var result = await _service.GetAdditionalIncomeAsync(null, null, null);

            // Assert
            Assert.NotNull(result);
            await _repository.Received(1).GetAdditionalIncomeAsync(null, 1, 12);
        }

        [Fact]
        public async Task GetAdditionalIncomeAsync_ServiceReturnsEmpty_ReturnsEmptyList()
        {
            // Arrange
            _repository.GetAdditionalIncomeAsync(Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<int>())
                .Returns(new List<DepartmentIncomeAdditional>());
            _mapper.Map<List<DepartmentIncomeAdditionalDto>>(Arg.Any<List<DepartmentIncomeAdditional>>())
                .Returns(new List<DepartmentIncomeAdditionalDto>());

            // Act
            var result = await _service.GetAdditionalIncomeAsync(null, null, null);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAdditionalIncomeAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _repository.GetAdditionalIncomeAsync(Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<int>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.GetAdditionalIncomeAsync(TestProject, 1, 12));
        }

        #endregion

        // ── GetTotalsAsync ──────────────────────────────────────────────────────

        #region GetTotalsAsync

        [Fact]
        public async Task GetTotalsAsync_WithProjectAndMonthRange_ReturnsMapperResult()
        {
            // Arrange
            var entities = MakeTotalsEntities();
            var dtos     = MakeTotalsDtos();

            // TRANSFORMENGINE: Service delegates directly to _repository.GetTotalsAsync (PIVOT logic
            // lives inside the repository implementation, not the service)
            _repository.GetTotalsAsync(TestProject, 1, 12).Returns(entities);
            _mapper.Map<List<DepartmentIncomeTotalsDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetTotalsAsync(TestProject, 1, 12);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            await _repository.Received(1).GetTotalsAsync(TestProject, 1, 12);
            _mapper.Received(1).Map<List<DepartmentIncomeTotalsDto>>(entities);
        }

        [Fact]
        public async Task GetTotalsAsync_NullParams_AppliesVbaDefaults()
        {
            // Arrange
            // TRANSFORMENGINE: VBA defaults: monthFrom=null→1, monthTo=null+monthFrom=1→12
            _repository.GetTotalsAsync(null, 1, 12).Returns(MakeTotalsEntities());
            _mapper.Map<List<DepartmentIncomeTotalsDto>>(Arg.Any<List<DepartmentIncomeTotals>>())
                .Returns(MakeTotalsDtos());

            // Act
            var result = await _service.GetTotalsAsync(null, null, null);

            // Assert
            Assert.NotNull(result);
            await _repository.Received(1).GetTotalsAsync(null, 1, 12);
        }

        [Fact]
        public async Task GetTotalsAsync_ServiceReturnsEmpty_ReturnsEmptyList()
        {
            // Arrange
            _repository.GetTotalsAsync(Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<int>())
                .Returns(new List<DepartmentIncomeTotals>());
            _mapper.Map<List<DepartmentIncomeTotalsDto>>(Arg.Any<List<DepartmentIncomeTotals>>())
                .Returns(new List<DepartmentIncomeTotalsDto>());

            // Act
            var result = await _service.GetTotalsAsync(null, null, null);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTotalsAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _repository.GetTotalsAsync(Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<int>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.GetTotalsAsync(TestProject, 1, 12));
        }

        #endregion

        // ── GetPeriodsAsync ─────────────────────────────────────────────────────

        #region GetPeriodsAsync

        [Fact]
        public async Task GetPeriodsAsync_ServiceReturnsData_ReturnsMapperResult()
        {
            // Arrange
            var entities = MakePeriodEntities();
            var dtos     = MakePeriodDtos();

            _repository.GetPeriodsAsync().Returns(entities);
            _mapper.Map<List<PeriodLookupDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetPeriodsAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("April", result[0].MonthName);
            await _repository.Received(1).GetPeriodsAsync();
            _mapper.Received(1).Map<List<PeriodLookupDto>>(entities);
        }

        [Fact]
        public async Task GetPeriodsAsync_ServiceReturnsEmpty_ReturnsEmptyList()
        {
            // Arrange
            _repository.GetPeriodsAsync().Returns(new List<PeriodLookup>());
            _mapper.Map<List<PeriodLookupDto>>(Arg.Any<List<PeriodLookup>>())
                .Returns(new List<PeriodLookupDto>());

            // Act
            var result = await _service.GetPeriodsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPeriodsAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _repository.GetPeriodsAsync().ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetPeriodsAsync());
        }

        #endregion
    }
}
