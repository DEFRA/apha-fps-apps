using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.ProjectDepartmentIncomeServiceTest
{
    public class ProjectDepartmentIncomeServiceTests
    {
        private const string TestProject = "AH0033";

        private readonly IProjectDepartmentIncomeRepository _repository;
        private readonly IMapper _mapper;
        private readonly IProjectDepartmentIncomeService _service;

        public ProjectDepartmentIncomeServiceTests()
        {
            _repository = Substitute.For<IProjectDepartmentIncomeRepository>();
            _mapper     = Substitute.For<IMapper>();
            _service    = new ProjectDepartmentIncomeService(_repository, _mapper);
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

        // ── Constructor Tests ────────────────────────────────────────────────────

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ProjectDepartmentIncomeService(null!, _mapper));
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ProjectDepartmentIncomeService(_repository, null!));
        }

        #endregion

        // ── GetSnapshotPeriodsAsync ──────────────────────────────────────────────

        #region GetSnapshotPeriodsAsync

        [Fact]
        public async Task GetSnapshotPeriodsAsync_ServiceReturnsData_ReturnsMapperResult()
        {
            // Arrange
            var entities = new List<Period>
            {
                new() { PeriodName = "April 2025 Only", FpsYear = 2025, EndPeriod = 4, FinalSummariesRun = 1, PeriodLocked = 0 },
                new() { PeriodName = "April - May 2025", FpsYear = 2025, EndPeriod = 5, FinalSummariesRun = 0, PeriodLocked = 0 },
            };
            var dtos = new List<PeriodSnapshotDto>
            {
                new() { PeriodName = "April 2025 Only",  EndPeriod = 4, FinalSummariesRun = false, PeriodLocked = false },
                new() { PeriodName = "April - May 2025", EndPeriod = 5, FinalSummariesRun = false, PeriodLocked = false },
            };

            _repository.GetSnapshotPeriodsAsync().Returns(entities);
            _mapper.Map<List<PeriodSnapshotDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetSnapshotPeriodsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            await _repository.Received(1).GetSnapshotPeriodsAsync();
            _mapper.Received(1).Map<List<PeriodSnapshotDto>>(entities);
        }

        [Fact]
        public async Task GetSnapshotPeriodsAsync_ServiceReturnsEmpty_ReturnsEmptyList()
        {
            // Arrange
            _repository.GetSnapshotPeriodsAsync().Returns(new List<Period>());
            _mapper.Map<List<PeriodSnapshotDto>>(Arg.Any<List<Period>>())
                .Returns(new List<PeriodSnapshotDto>());

            // Act
            var result = await _service.GetSnapshotPeriodsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSnapshotPeriodsAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _repository.GetSnapshotPeriodsAsync().ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetSnapshotPeriodsAsync());
        }

        #endregion

        // ── UpdatePeriodLockedAsync ──────────────────────────────────────────────

        #region UpdatePeriodLockedAsync

        [Fact]
        public async Task UpdatePeriodLockedAsync_PeriodExists_DelegatesToRepository()
        {
            // Arrange
            _repository.UpdatePeriodLockedAsync("April 2025 Only", true).Returns(1);

            // Act
            var result = await _service.UpdatePeriodLockedAsync("April 2025 Only", true);

            // Assert
            Assert.Equal(1, result);
            await _repository.Received(1).UpdatePeriodLockedAsync("April 2025 Only", true);
        }

        [Fact]
        public async Task UpdatePeriodLockedAsync_PeriodNotFound_ReturnsZero()
        {
            // Arrange
            _repository.UpdatePeriodLockedAsync("NonExistent", false).Returns(0);

            // Act
            var result = await _service.UpdatePeriodLockedAsync("NonExistent", false);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task UpdatePeriodLockedAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _repository.UpdatePeriodLockedAsync(Arg.Any<string>(), Arg.Any<bool>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.UpdatePeriodLockedAsync("April 2025 Only", true));
        }

        [Fact]
        public async Task UpdatePeriodLockedAsync_PeriodNameWithSlash_PassedToRepositoryUnchanged()
        {
            // Arrange
            const string slashPeriod = "April - August 2025/25";
            _repository.UpdatePeriodLockedAsync(slashPeriod, true).Returns(1);

            // Act
            var result = await _service.UpdatePeriodLockedAsync(slashPeriod, true);

            // Assert
            Assert.Equal(1, result);
            await _repository.Received(1).UpdatePeriodLockedAsync(slashPeriod, true);
        }

        #endregion

        // ── GetTimeIncomeCurrentAsync ────────────────────────────────────────────

        #region GetTimeIncomeCurrentAsync

        [Fact]
        public async Task GetTimeIncomeCurrentAsync_WithParams_ReturnsMapperResult()
        {
            // Arrange
            var entities = MakeTimeEntities();
            var dtos     = MakeTimeDtos();

            _repository.GetTimeIncomeCurrentAsync(TestProject, 3, 6).Returns(entities);
            _mapper.Map<List<DepartmentIncomeTimeDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetTimeIncomeCurrentAsync(TestProject, 3, 6);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            await _repository.Received(1).GetTimeIncomeCurrentAsync(TestProject, 3, 6);
        }

        [Fact]
        public async Task GetTimeIncomeCurrentAsync_NullParams_AppliesVbaDefaults()
        {
            // Arrange
            _repository.GetTimeIncomeCurrentAsync(null, 1, 12).Returns(MakeTimeEntities());
            _mapper.Map<List<DepartmentIncomeTimeDto>>(Arg.Any<List<DepartmentIncomeTime>>()).Returns(MakeTimeDtos());

            // Act
            await _service.GetTimeIncomeCurrentAsync(null, null, null);

            // Assert
            await _repository.Received(1).GetTimeIncomeCurrentAsync(null, 1, 12);
        }

        [Fact]
        public async Task GetTimeIncomeCurrentAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _repository.GetTimeIncomeCurrentAsync(Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<int>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.GetTimeIncomeCurrentAsync(TestProject, 1, 12));
        }

        #endregion

        // ── GetTestIncomeCurrentAsync ────────────────────────────────────────────

        #region GetTestIncomeCurrentAsync

        [Fact]
        public async Task GetTestIncomeCurrentAsync_WithParams_ReturnsMapperResult()
        {
            // Arrange
            var entities = MakeTestEntities();
            var dtos     = MakeTestDtos();

            _repository.GetTestIncomeCurrentAsync(TestProject, 1, 6).Returns(entities);
            _mapper.Map<List<DepartmentIncomeTestDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetTestIncomeCurrentAsync(TestProject, 1, 6);

            // Assert
            Assert.Equal(2, result.Count);
            await _repository.Received(1).GetTestIncomeCurrentAsync(TestProject, 1, 6);
        }

        [Fact]
        public async Task GetTestIncomeCurrentAsync_NullParams_AppliesVbaDefaults()
        {
            // Arrange
            _repository.GetTestIncomeCurrentAsync(null, 1, 12).Returns(MakeTestEntities());
            _mapper.Map<List<DepartmentIncomeTestDto>>(Arg.Any<List<DepartmentIncomeTest>>()).Returns(MakeTestDtos());

            // Act
            await _service.GetTestIncomeCurrentAsync(null, null, null);

            // Assert
            await _repository.Received(1).GetTestIncomeCurrentAsync(null, 1, 12);
        }

        #endregion

        // ── GetAnimalIncomeCurrentAsync ──────────────────────────────────────────

        #region GetAnimalIncomeCurrentAsync

        [Fact]
        public async Task GetAnimalIncomeCurrentAsync_WithParams_ReturnsMapperResult()
        {
            // Arrange
            var entities = MakeAnimalEntities();
            var dtos     = MakeAnimalDtos();

            _repository.GetAnimalIncomeCurrentAsync(TestProject, 1, 12).Returns(entities);
            _mapper.Map<List<DepartmentIncomeAnimalDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetAnimalIncomeCurrentAsync(TestProject, 1, 12);

            // Assert
            Assert.Equal(2, result.Count);
            await _repository.Received(1).GetAnimalIncomeCurrentAsync(TestProject, 1, 12);
        }

        [Fact]
        public async Task GetAnimalIncomeCurrentAsync_NullParams_AppliesVbaDefaults()
        {
            // Arrange
            _repository.GetAnimalIncomeCurrentAsync(null, 1, 12).Returns(MakeAnimalEntities());
            _mapper.Map<List<DepartmentIncomeAnimalDto>>(Arg.Any<List<DepartmentIncomeAnimal>>()).Returns(MakeAnimalDtos());

            // Act
            await _service.GetAnimalIncomeCurrentAsync(null, null, null);

            // Assert
            await _repository.Received(1).GetAnimalIncomeCurrentAsync(null, 1, 12);
        }

        #endregion

        // ── GetAdditionalIncomeCurrentAsync ─────────────────────────────────────

        #region GetAdditionalIncomeCurrentAsync

        [Fact]
        public async Task GetAdditionalIncomeCurrentAsync_WithParams_ReturnsMapperResult()
        {
            // Arrange
            var entities = MakeAdditionalEntities();
            var dtos     = MakeAdditionalDtos();

            _repository.GetAdditionalIncomeCurrentAsync(TestProject, 1, 12).Returns(entities);
            _mapper.Map<List<DepartmentIncomeAdditionalDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetAdditionalIncomeCurrentAsync(TestProject, 1, 12);

            // Assert
            Assert.Equal(2, result.Count);
            await _repository.Received(1).GetAdditionalIncomeCurrentAsync(TestProject, 1, 12);
        }

        [Fact]
        public async Task GetAdditionalIncomeCurrentAsync_NullParams_AppliesVbaDefaults()
        {
            // Arrange
            _repository.GetAdditionalIncomeCurrentAsync(null, 1, 12).Returns(MakeAdditionalEntities());
            _mapper.Map<List<DepartmentIncomeAdditionalDto>>(Arg.Any<List<DepartmentIncomeAdditional>>()).Returns(MakeAdditionalDtos());

            // Act
            await _service.GetAdditionalIncomeCurrentAsync(null, null, null);

            // Assert
            await _repository.Received(1).GetAdditionalIncomeCurrentAsync(null, 1, 12);
        }

        #endregion

        // ── GetTotalsCurrentAsync ────────────────────────────────────────────────

        #region GetTotalsCurrentAsync

        [Fact]
        public async Task GetTotalsCurrentAsync_WithParams_ReturnsMapperResult()
        {
            // Arrange
            var entities = MakeTotalsEntities();
            var dtos     = MakeTotalsDtos();

            _repository.GetTotalsCurrentAsync(TestProject, 1, 12).Returns(entities);
            _mapper.Map<List<DepartmentIncomeTotalsDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetTotalsCurrentAsync(TestProject, 1, 12);

            // Assert
            Assert.Equal(2, result.Count);
            await _repository.Received(1).GetTotalsCurrentAsync(TestProject, 1, 12);
        }

        [Fact]
        public async Task GetTotalsCurrentAsync_NullParams_AppliesVbaDefaults()
        {
            // Arrange
            _repository.GetTotalsCurrentAsync(null, 1, 12).Returns(MakeTotalsEntities());
            _mapper.Map<List<DepartmentIncomeTotalsDto>>(Arg.Any<List<DepartmentIncomeTotals>>()).Returns(MakeTotalsDtos());

            // Act
            await _service.GetTotalsCurrentAsync(null, null, null);

            // Assert
            await _repository.Received(1).GetTotalsCurrentAsync(null, 1, 12);
        }

        #endregion
    }
}
