/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeControllerTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New xUnit test class for DepartmentIncomeController (backend API)
 *   - Covers all 6 public actions: GetTimeAsync, GetTestsAsync, GetAnimalsAsync,
 *     GetAdditionalAsync, GetTotalsAsync, GetPeriodsAsync
 *   - NSubstitute mocks for IDepartmentIncomeService and IMapper
 *   - Tests: happy path, empty result, and exception propagation per method
 *
 * PRESERVED:
 *   - Controller is read-only (no CRUD test scenarios)
 *   - Uses OkObjectResult assertions matching controller Ok(...) return pattern
 *
 * DEFERRED: none — fully automated.
 */

using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Api.UnitTests.Controllers.DepartmentIncomeControllerTest
{
    public class DepartmentIncomeControllerTests
    {
        private const string TestProject = "AH0033";
        private const int TestMonthFrom = 1;
        private const int TestMonthTo = 6;

        private readonly IDepartmentIncomeService _service;
        private readonly IMapper _mapper;
        private readonly DepartmentIncomeController _controller;

        public DepartmentIncomeControllerTests()
        {
            _service    = Substitute.For<IDepartmentIncomeService>();
            _mapper     = Substitute.For<IMapper>();
            _controller = new DepartmentIncomeController(_service, _mapper);
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
    }
}
