using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Api.UnitTests.Controller.ProjectDepartmentIncomeControllerTest
{
    public class ProjectDepartmentIncomeControllerTests
    {
        private const string TestProject = "AH0033";
        private const int TestMonthFrom = 1;
        private const int TestMonthTo = 6;

        private readonly IProjectDepartmentIncomeService _service;
        private readonly IMapper _mapper;
        private readonly ProjectDepartmentIncomeController _controller;

        public ProjectDepartmentIncomeControllerTests()
        {
            _service    = Substitute.For<IProjectDepartmentIncomeService>();
            _mapper     = Substitute.For<IMapper>();
            _controller = new ProjectDepartmentIncomeController(_service, _mapper);
        }

        // ── Helper factories ────────────────────────────────────────────────────

        private static List<DepartmentIncomeTimeDto> MakeTimeDtos(int count = 2) =>
            Enumerable.Range(1, count)
                .Select(i => new DepartmentIncomeTimeDto { Project = $"PROJ{i}", Month = i, TotalCost = i * 100m })
                .ToList();

        private static List<DepartmentIncomeTimeRes> MakeTimeRes(int count = 2) =>
            Enumerable.Range(1, count)
                .Select(i => new DepartmentIncomeTimeRes { Project = $"PROJ{i}", Month = i, TotalCost = i * 100m })
                .ToList();

        private static List<DepartmentIncomeTestDto> MakeTestDtos(int count = 2) =>
            Enumerable.Range(1, count)
                .Select(i => new DepartmentIncomeTestDto { Project = $"PROJ{i}", Month = i, TotalCost = i * 50m })
                .ToList();

        private static List<DepartmentIncomeTestRes> MakeTestRes(int count = 2) =>
            Enumerable.Range(1, count)
                .Select(i => new DepartmentIncomeTestRes { Project = $"PROJ{i}", Month = i, TotalCost = i * 50m })
                .ToList();

        private static List<DepartmentIncomeAnimalDto> MakeAnimalDtos(int count = 2) =>
            Enumerable.Range(1, count)
                .Select(i => new DepartmentIncomeAnimalDto { Project = $"PROJ{i}", Month = i, TotalCost = i * 75m })
                .ToList();

        private static List<DepartmentIncomeAnimalRes> MakeAnimalRes(int count = 2) =>
            Enumerable.Range(1, count)
                .Select(i => new DepartmentIncomeAnimalRes { Project = $"PROJ{i}", Month = i, TotalCost = i * 75m })
                .ToList();

        private static List<DepartmentIncomeAdditionalDto> MakeAdditionalDtos(int count = 2) =>
            Enumerable.Range(1, count)
                .Select(i => new DepartmentIncomeAdditionalDto { Project = $"PROJ{i}", Month = i, TotalCost = i * 25m })
                .ToList();

        private static List<DepartmentIncomeAdditionalRes> MakeAdditionalRes(int count = 2) =>
            Enumerable.Range(1, count)
                .Select(i => new DepartmentIncomeAdditionalRes { Project = $"PROJ{i}", Month = i, TotalCost = i * 25m })
                .ToList();

        private static List<DepartmentIncomeTotalsDto> MakeTotalsDtos(int count = 2) =>
            Enumerable.Range(1, count)
                .Select(i => new DepartmentIncomeTotalsDto { Project = $"PROJ{i}", TotalCosts = i * 250m })
                .ToList();

        private static List<DepartmentIncomeTotalsRes> MakeTotalsRes(int count = 2) =>
            Enumerable.Range(1, count)
                .Select(i => new DepartmentIncomeTotalsRes { Project = $"PROJ{i}", TotalCosts = i * 250m })
                .ToList();

        private static List<PeriodLookupDto> MakePeriodDtos() =>
            new List<PeriodLookupDto>
            {
                new PeriodLookupDto { AccntsPeriod = 1, MonthName = "April",   MonthNumber = 4 },
                new PeriodLookupDto { AccntsPeriod = 2, MonthName = "May",     MonthNumber = 5 },
            };

        private static List<PeriodLookupRes> MakePeriodRes() =>
            new List<PeriodLookupRes>
            {
                new PeriodLookupRes { AccntsPeriod = 1, MonthName = "April",   MonthNumber = 4 },
                new PeriodLookupRes { AccntsPeriod = 2, MonthName = "May",     MonthNumber = 5 },
            };

        // ── GetTimeAsync ────────────────────────────────────────────────────────

        #region GetTimeAsync

        [Fact]
        public async Task GetTimeAsync_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = MakeTimeDtos();
            var res  = MakeTimeRes();

            _service.GetTimeIncomeAsync(TestProject, TestMonthFrom, TestMonthTo).Returns(dtos);
            _mapper.Map<List<DepartmentIncomeTimeRes>>(dtos).Returns(res);

            // Act
            var result = await _controller.GetTimeAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<DepartmentIncomeTimeRes>>(ok.Value);
            Assert.Equal(2, data.Count);
            await _service.Received(1).GetTimeIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);
            _mapper.Received(1).Map<List<DepartmentIncomeTimeRes>>(dtos);
        }

        [Fact]
        public async Task GetTimeAsync_ServiceReturnsEmpty_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos = new List<DepartmentIncomeTimeDto>();
            var res  = new List<DepartmentIncomeTimeRes>();

            _service.GetTimeIncomeAsync(null, null, null).Returns(dtos);
            _mapper.Map<List<DepartmentIncomeTimeRes>>(dtos).Returns(res);

            // Act
            var result = await _controller.GetTimeAsync(null, null, null);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<DepartmentIncomeTimeRes>>(ok.Value);
            Assert.Empty(data);
        }

        [Fact]
        public async Task GetTimeAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _service.GetTimeIncomeAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _controller.GetTimeAsync(TestProject, TestMonthFrom, TestMonthTo));
        }

        #endregion

        // ── GetTestsAsync ───────────────────────────────────────────────────────

        #region GetTestsAsync

        [Fact]
        public async Task GetTestsAsync_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = MakeTestDtos();
            var res  = MakeTestRes();

            _service.GetTestIncomeAsync(TestProject, TestMonthFrom, TestMonthTo).Returns(dtos);
            _mapper.Map<List<DepartmentIncomeTestRes>>(dtos).Returns(res);

            // Act
            var result = await _controller.GetTestsAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<DepartmentIncomeTestRes>>(ok.Value);
            Assert.Equal(2, data.Count);
            await _service.Received(1).GetTestIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);
        }

        [Fact]
        public async Task GetTestsAsync_ServiceReturnsEmpty_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos = new List<DepartmentIncomeTestDto>();
            var res  = new List<DepartmentIncomeTestRes>();

            _service.GetTestIncomeAsync(null, null, null).Returns(dtos);
            _mapper.Map<List<DepartmentIncomeTestRes>>(dtos).Returns(res);

            // Act
            var result = await _controller.GetTestsAsync(null, null, null);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Empty(Assert.IsType<List<DepartmentIncomeTestRes>>(ok.Value));
        }

        [Fact]
        public async Task GetTestsAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _service.GetTestIncomeAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _controller.GetTestsAsync(TestProject, TestMonthFrom, TestMonthTo));
        }

        #endregion

        // ── GetAnimalsAsync ─────────────────────────────────────────────────────

        #region GetAnimalsAsync

        [Fact]
        public async Task GetAnimalsAsync_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = MakeAnimalDtos();
            var res  = MakeAnimalRes();

            _service.GetAnimalIncomeAsync(TestProject, TestMonthFrom, TestMonthTo).Returns(dtos);
            _mapper.Map<List<DepartmentIncomeAnimalRes>>(dtos).Returns(res);

            // Act
            var result = await _controller.GetAnimalsAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<DepartmentIncomeAnimalRes>>(ok.Value);
            Assert.Equal(2, data.Count);
            await _service.Received(1).GetAnimalIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);
        }

        [Fact]
        public async Task GetAnimalsAsync_ServiceReturnsEmpty_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos = new List<DepartmentIncomeAnimalDto>();
            var res  = new List<DepartmentIncomeAnimalRes>();

            _service.GetAnimalIncomeAsync(null, null, null).Returns(dtos);
            _mapper.Map<List<DepartmentIncomeAnimalRes>>(dtos).Returns(res);

            // Act
            var result = await _controller.GetAnimalsAsync(null, null, null);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Empty(Assert.IsType<List<DepartmentIncomeAnimalRes>>(ok.Value));
        }

        [Fact]
        public async Task GetAnimalsAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _service.GetAnimalIncomeAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _controller.GetAnimalsAsync(TestProject, TestMonthFrom, TestMonthTo));
        }

        #endregion

        // ── GetAdditionalAsync ──────────────────────────────────────────────────

        #region GetAdditionalAsync

        [Fact]
        public async Task GetAdditionalAsync_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = MakeAdditionalDtos();
            var res  = MakeAdditionalRes();

            _service.GetAdditionalIncomeAsync(TestProject, TestMonthFrom, TestMonthTo).Returns(dtos);
            _mapper.Map<List<DepartmentIncomeAdditionalRes>>(dtos).Returns(res);

            // Act
            var result = await _controller.GetAdditionalAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<DepartmentIncomeAdditionalRes>>(ok.Value);
            Assert.Equal(2, data.Count);
            await _service.Received(1).GetAdditionalIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);
        }

        [Fact]
        public async Task GetAdditionalAsync_ServiceReturnsEmpty_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos = new List<DepartmentIncomeAdditionalDto>();
            var res  = new List<DepartmentIncomeAdditionalRes>();

            _service.GetAdditionalIncomeAsync(null, null, null).Returns(dtos);
            _mapper.Map<List<DepartmentIncomeAdditionalRes>>(dtos).Returns(res);

            // Act
            var result = await _controller.GetAdditionalAsync(null, null, null);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Empty(Assert.IsType<List<DepartmentIncomeAdditionalRes>>(ok.Value));
        }

        [Fact]
        public async Task GetAdditionalAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _service.GetAdditionalIncomeAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _controller.GetAdditionalAsync(TestProject, TestMonthFrom, TestMonthTo));
        }

        #endregion

        // ── GetTotalsAsync ──────────────────────────────────────────────────────

        #region GetTotalsAsync

        [Fact]
        public async Task GetTotalsAsync_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = MakeTotalsDtos();
            var res  = MakeTotalsRes();

            _service.GetTotalsAsync(TestProject, TestMonthFrom, TestMonthTo).Returns(dtos);
            _mapper.Map<List<DepartmentIncomeTotalsRes>>(dtos).Returns(res);

            // Act
            var result = await _controller.GetTotalsAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<DepartmentIncomeTotalsRes>>(ok.Value);
            Assert.Equal(2, data.Count);
            await _service.Received(1).GetTotalsAsync(TestProject, TestMonthFrom, TestMonthTo);
        }

        [Fact]
        public async Task GetTotalsAsync_ServiceReturnsEmpty_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos = new List<DepartmentIncomeTotalsDto>();
            var res  = new List<DepartmentIncomeTotalsRes>();

            _service.GetTotalsAsync(null, null, null).Returns(dtos);
            _mapper.Map<List<DepartmentIncomeTotalsRes>>(dtos).Returns(res);

            // Act
            var result = await _controller.GetTotalsAsync(null, null, null);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Empty(Assert.IsType<List<DepartmentIncomeTotalsRes>>(ok.Value));
        }

        [Fact]
        public async Task GetTotalsAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _service.GetTotalsAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _controller.GetTotalsAsync(TestProject, TestMonthFrom, TestMonthTo));
        }

        #endregion

        // ── GetPeriodsAsync ─────────────────────────────────────────────────────

        #region GetPeriodsAsync

        [Fact]
        public async Task GetPeriodsAsync_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = MakePeriodDtos();
            var res  = MakePeriodRes();

            _service.GetPeriodsAsync().Returns(dtos);
            _mapper.Map<List<PeriodLookupRes>>(dtos).Returns(res);

            // Act
            var result = await _controller.GetPeriodsAsync();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<PeriodLookupRes>>(ok.Value);
            Assert.Equal(2, data.Count);
            await _service.Received(1).GetPeriodsAsync();
            _mapper.Received(1).Map<List<PeriodLookupRes>>(dtos);
        }

        [Fact]
        public async Task GetPeriodsAsync_ServiceReturnsEmpty_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos = new List<PeriodLookupDto>();
            var res  = new List<PeriodLookupRes>();

            _service.GetPeriodsAsync().Returns(dtos);
            _mapper.Map<List<PeriodLookupRes>>(dtos).Returns(res);

            // Act
            var result = await _controller.GetPeriodsAsync();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Empty(Assert.IsType<List<PeriodLookupRes>>(ok.Value));
        }

        [Fact]
        public async Task GetPeriodsAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _service.GetPeriodsAsync().ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPeriodsAsync());
        }

        #endregion

        // ── Constructor Tests ────────────────────────────────────────────────────

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ProjectDepartmentIncomeController(null!, _mapper));
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ProjectDepartmentIncomeController(_service, null!));
        }

        #endregion

        // ── GetSnapshotPeriodsAsync ──────────────────────────────────────────────

        #region GetSnapshotPeriodsAsync

        [Fact]
        public async Task GetSnapshotPeriodsAsync_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = new List<PeriodSnapshotDto>
            {
                new() { PeriodName = "April 2025 Only",  EndPeriod = 4, PeriodLocked = false },
                new() { PeriodName = "April - May 2025", EndPeriod = 5, PeriodLocked = false },
            };
            var res = new List<PeriodSnapshotRes>
            {
                new() { PeriodName = "April 2025 Only",  EndPeriod = 4, PeriodLocked = false },
                new() { PeriodName = "April - May 2025", EndPeriod = 5, PeriodLocked = false },
            };

            _service.GetSnapshotPeriodsAsync().Returns(dtos);
            _mapper.Map<List<PeriodSnapshotRes>>(dtos).Returns(res);

            // Act
            var result = await _controller.GetSnapshotPeriodsAsync();

            // Assert
            var ok   = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<PeriodSnapshotRes>>(ok.Value);
            Assert.Equal(2, data.Count);
            await _service.Received(1).GetSnapshotPeriodsAsync();
            _mapper.Received(1).Map<List<PeriodSnapshotRes>>(dtos);
        }

        [Fact]
        public async Task GetSnapshotPeriodsAsync_ServiceReturnsEmpty_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos = new List<PeriodSnapshotDto>();
            var res  = new List<PeriodSnapshotRes>();

            _service.GetSnapshotPeriodsAsync().Returns(dtos);
            _mapper.Map<List<PeriodSnapshotRes>>(dtos).Returns(res);

            // Act
            var result = await _controller.GetSnapshotPeriodsAsync();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Empty(Assert.IsType<List<PeriodSnapshotRes>>(ok.Value));
        }

        [Fact]
        public async Task GetSnapshotPeriodsAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _service.GetSnapshotPeriodsAsync().ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetSnapshotPeriodsAsync());
        }

        #endregion

        // ── UpdatePeriodLockedAsync ──────────────────────────────────────────────

        #region UpdatePeriodLockedAsync

        [Fact]
        public async Task UpdatePeriodLockedAsync_PeriodFound_ReturnsOkWithTrue()
        {
            // Arrange
            _service.UpdatePeriodLockedAsync("April 2025 Only", true).Returns(1);

            // Act
            var result = await _controller.UpdatePeriodLockedAsync("April 2025 Only", true);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)ok.Value!);
            await _service.Received(1).UpdatePeriodLockedAsync("April 2025 Only", true);
        }

        [Fact]
        public async Task UpdatePeriodLockedAsync_PeriodNotFound_ReturnsNotFound()
        {
            // Arrange
            _service.UpdatePeriodLockedAsync("NonExistent", true).Returns(0);

            // Act
            var result = await _controller.UpdatePeriodLockedAsync("NonExistent", true);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdatePeriodLockedAsync_UnlockPeriod_ReturnsOkWithTrue()
        {
            // Arrange
            _service.UpdatePeriodLockedAsync("April 2025 Only", false).Returns(1);

            // Act
            var result = await _controller.UpdatePeriodLockedAsync("April 2025 Only", false);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)ok.Value!);
        }

        [Fact]
        public async Task UpdatePeriodLockedAsync_PeriodNameWithSlash_PeriodFound_ReturnsOk()
        {
            // Arrange
            const string slashPeriod = "April - August 2025/25";
            _service.UpdatePeriodLockedAsync(slashPeriod, true).Returns(1);

            // Act
            var result = await _controller.UpdatePeriodLockedAsync(slashPeriod, true);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdatePeriodLockedAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _service.UpdatePeriodLockedAsync(Arg.Any<string>(), Arg.Any<bool>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _controller.UpdatePeriodLockedAsync("April 2025 Only", true));
        }

        #endregion

        // ── GetCurrentTimeAsync ──────────────────────────────────────────────────

        #region GetCurrentTimeAsync

        [Fact]
        public async Task GetCurrentTimeAsync_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = MakeTimeDtos();
            var res  = MakeTimeRes();

            _service.GetTimeIncomeCurrentAsync(TestProject, TestMonthFrom, TestMonthTo).Returns(dtos);
            _mapper.Map<List<DepartmentIncomeTimeRes>>(dtos).Returns(res);

            // Act
            var result = await _controller.GetCurrentTimeAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(2, Assert.IsType<List<DepartmentIncomeTimeRes>>(ok.Value).Count);
            await _service.Received(1).GetTimeIncomeCurrentAsync(TestProject, TestMonthFrom, TestMonthTo);
        }

        [Fact]
        public async Task GetCurrentTimeAsync_ServiceReturnsEmpty_ReturnsOkWithEmptyList()
        {
            // Arrange
            _service.GetTimeIncomeCurrentAsync(null, null, null).Returns(new List<DepartmentIncomeTimeDto>());
            _mapper.Map<List<DepartmentIncomeTimeRes>>(Arg.Any<List<DepartmentIncomeTimeDto>>())
                .Returns(new List<DepartmentIncomeTimeRes>());

            // Act
            var result = await _controller.GetCurrentTimeAsync(null, null, null);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Empty(Assert.IsType<List<DepartmentIncomeTimeRes>>(ok.Value));
        }

        #endregion

        // ── GetCurrentTestsAsync ─────────────────────────────────────────────────

        #region GetCurrentTestsAsync

        [Fact]
        public async Task GetCurrentTestsAsync_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = MakeTestDtos();
            var res  = MakeTestRes();

            _service.GetTestIncomeCurrentAsync(TestProject, TestMonthFrom, TestMonthTo).Returns(dtos);
            _mapper.Map<List<DepartmentIncomeTestRes>>(dtos).Returns(res);

            // Act
            var result = await _controller.GetCurrentTestsAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(2, Assert.IsType<List<DepartmentIncomeTestRes>>(ok.Value).Count);
        }

        [Fact]
        public async Task GetCurrentTestsAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _service.GetTestIncomeCurrentAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _controller.GetCurrentTestsAsync(TestProject, TestMonthFrom, TestMonthTo));
        }

        #endregion

        // ── GetCurrentAnimalsAsync ───────────────────────────────────────────────

        #region GetCurrentAnimalsAsync

        [Fact]
        public async Task GetCurrentAnimalsAsync_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = MakeAnimalDtos();
            var res  = MakeAnimalRes();

            _service.GetAnimalIncomeCurrentAsync(TestProject, TestMonthFrom, TestMonthTo).Returns(dtos);
            _mapper.Map<List<DepartmentIncomeAnimalRes>>(dtos).Returns(res);

            // Act
            var result = await _controller.GetCurrentAnimalsAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(2, Assert.IsType<List<DepartmentIncomeAnimalRes>>(ok.Value).Count);
        }

        [Fact]
        public async Task GetCurrentAnimalsAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _service.GetAnimalIncomeCurrentAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _controller.GetCurrentAnimalsAsync(TestProject, TestMonthFrom, TestMonthTo));
        }

        #endregion

        // ── GetCurrentAdditionalAsync ────────────────────────────────────────────

        #region GetCurrentAdditionalAsync

        [Fact]
        public async Task GetCurrentAdditionalAsync_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = MakeAdditionalDtos();
            var res  = MakeAdditionalRes();

            _service.GetAdditionalIncomeCurrentAsync(TestProject, TestMonthFrom, TestMonthTo).Returns(dtos);
            _mapper.Map<List<DepartmentIncomeAdditionalRes>>(dtos).Returns(res);

            // Act
            var result = await _controller.GetCurrentAdditionalAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(2, Assert.IsType<List<DepartmentIncomeAdditionalRes>>(ok.Value).Count);
        }

        [Fact]
        public async Task GetCurrentAdditionalAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _service.GetAdditionalIncomeCurrentAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _controller.GetCurrentAdditionalAsync(TestProject, TestMonthFrom, TestMonthTo));
        }

        #endregion

        // ── GetCurrentTotalsAsync ────────────────────────────────────────────────

        #region GetCurrentTotalsAsync

        [Fact]
        public async Task GetCurrentTotalsAsync_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = MakeTotalsDtos();
            var res  = MakeTotalsRes();

            _service.GetTotalsCurrentAsync(TestProject, TestMonthFrom, TestMonthTo).Returns(dtos);
            _mapper.Map<List<DepartmentIncomeTotalsRes>>(dtos).Returns(res);

            // Act
            var result = await _controller.GetCurrentTotalsAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(2, Assert.IsType<List<DepartmentIncomeTotalsRes>>(ok.Value).Count);
        }

        [Fact]
        public async Task GetCurrentTotalsAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _service.GetTotalsCurrentAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _controller.GetCurrentTotalsAsync(TestProject, TestMonthFrom, TestMonthTo));
        }

        #endregion
    }
}
