/*
 * TRANSFORMENGINE MIGRATION — TestRequirementRCCostControllerTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New xUnit test class for TestRequirementRCCostController (backend API controller)
 *   - Covers GetByTestCodeAsync, GetByKeyAsync, CreateAsync, UpdateAsync, DeleteAsync
 *   - NSubstitute used for ITestRequirementRCCostService and IMapper mocks
 *   - Tests cover success paths, not-found / KeyNotFoundException paths, and write-operation flows
 *
 * PRESERVED:
 *   - Composite PK semantics: TestCode + Buyer + ProfitCentre + FpsYear
 *   - Naming convention: [MethodName]_[StateUnderTest]_[ExpectedResult]
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
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controllers.TestRequirementRCCostControllerTest
{
    public class TestRequirementRCCostControllerTests
    {
        private const string DefaultTestCode = "TEST001";
        private const string DefaultBuyer = "BUYER01";
        private const string DefaultProfitCentre = "PC001";
        private const int DefaultFpsYear = 2025;

        private readonly ITestRequirementRCCostService _service;
        private readonly IMapper _mapper;
        private readonly TestRequirementRCCostController _controller;

        public TestRequirementRCCostControllerTests()
        {
            _service = Substitute.For<ITestRequirementRCCostService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new TestRequirementRCCostController(_service, _mapper);
        }

        #region GetByTestCodeAsync

        [Fact]
        public async Task GetByTestCodeAsync_ServiceReturnsList_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtoList = new List<TestRequirementRCCostDto> { CreateTestDto(), CreateTestDto() };
            var resList = new List<TestRequirementRCCostRes> { CreateTestRes(), CreateTestRes() };

            _service.GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear).Returns(dtoList);
            _mapper.Map<List<TestRequirementRCCostRes>>(dtoList).Returns(resList);

            // Act
            var result = await _controller.GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<TestRequirementRCCostRes>>(okResult.Value);
            Assert.Equal(2, data.Count);
        }

        [Fact]
        public async Task GetByTestCodeAsync_ServiceReturnsEmpty_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtoList = new List<TestRequirementRCCostDto>();
            var resList = new List<TestRequirementRCCostRes>();

            _service.GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear).Returns(dtoList);
            _mapper.Map<List<TestRequirementRCCostRes>>(dtoList).Returns(resList);

            // Act
            var result = await _controller.GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<TestRequirementRCCostRes>>(okResult.Value);
            Assert.Empty(data);
        }

        [Fact]
        public async Task GetByTestCodeAsync_CallsServiceWithCorrectParameters()
        {
            // Arrange
            var dtoList = new List<TestRequirementRCCostDto>();
            _service.GetByTestCodeAsync("ALPHA", 2024).Returns(dtoList);
            _mapper.Map<List<TestRequirementRCCostRes>>(dtoList).Returns(new List<TestRequirementRCCostRes>());

            // Act
            await _controller.GetByTestCodeAsync("ALPHA", 2024);

            // Assert
            await _service.Received(1).GetByTestCodeAsync("ALPHA", 2024);
        }

        #endregion

        #region GetByKeyAsync

        [Fact]
        public async Task GetByKeyAsync_ExistingRecord_ReturnsOkWithMappedRecord()
        {
            // Arrange
            var dto = CreateTestDto();
            var res = CreateTestRes();

            _service.GetByKeyAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear)
                .Returns(dto);
            _mapper.Map<TestRequirementRCCostRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetByKeyAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<TestRequirementRCCostRes>(okResult.Value);
        }

        [Fact]
        public async Task GetByKeyAsync_RecordNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _service.GetByKeyAsync("NOTEXIST", "B999", "PC999", DefaultFpsYear)
                .Returns((TestRequirementRCCostDto?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.GetByKeyAsync("NOTEXIST", "B999", "PC999", DefaultFpsYear));
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ValidRequest_ReturnsOkWithCreatedRecord()
        {
            // Arrange
            var req = CreateTestReq();
            var dto = CreateTestDto();
            var res = CreateTestRes();

            _mapper.Map<TestRequirementRCCostDto>(req).Returns(dto);
            _service.CreateAsync(dto).Returns(dto);
            _mapper.Map<TestRequirementRCCostRes>(dto).Returns(res);

            // Act
            var result = await _controller.CreateAsync(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<TestRequirementRCCostRes>(okResult.Value);
            await _service.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateAsync_ServiceThrowsInvalidOperation_PropagatesException()
        {
            // Arrange
            var req = CreateTestReq();
            var dto = CreateTestDto();

            _mapper.Map<TestRequirementRCCostDto>(req).Returns(dto);
            _service.CreateAsync(dto).Returns<TestRequirementRCCostDto>(x =>
                throw new InvalidOperationException("Duplicate key"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.CreateAsync(req));
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ValidRequest_ReturnsOkWithUpdatedRecord()
        {
            // Arrange
            var req = CreateTestReq();
            var dto = CreateTestDto();
            var res = CreateTestRes();

            _mapper.Map<TestRequirementRCCostDto>(req).Returns(dto);
            _service.UpdateAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear, dto)
                .Returns(dto);
            _mapper.Map<TestRequirementRCCostRes>(dto).Returns(res);

            // Act
            var result = await _controller.UpdateAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear, req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<TestRequirementRCCostRes>(okResult.Value);
            await _service.Received(1).UpdateAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear, dto);
        }

        [Fact]
        public async Task UpdateAsync_RecordNotFound_PropagatesKeyNotFoundException()
        {
            // Arrange
            var req = CreateTestReq();
            var dto = CreateTestDto();

            _mapper.Map<TestRequirementRCCostDto>(req).Returns(dto);
            _service.UpdateAsync("NOTEXIST", "B999", "PC999", DefaultFpsYear, dto)
                .Returns<TestRequirementRCCostDto>(x => throw new KeyNotFoundException("Not found"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.UpdateAsync("NOTEXIST", "B999", "PC999", DefaultFpsYear, req));
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ExistingRecord_ReturnsOkWithTrue()
        {
            // Arrange
            _service.DeleteAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear).Returns(true);

            // Act
            var result = await _controller.DeleteAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
            await _service.Received(1).DeleteAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear);
        }

        [Fact]
        public async Task DeleteAsync_RecordNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _service.DeleteAsync("NOTEXIST", "B999", "PC999", DefaultFpsYear).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.DeleteAsync("NOTEXIST", "B999", "PC999", DefaultFpsYear));
        }

        #endregion

        #region Helper Methods

        private static TestRequirementRCCostDto CreateTestDto() =>
            new()
            {
                TestCode = DefaultTestCode,
                Buyer = DefaultBuyer,
                ProfitCentre = DefaultProfitCentre,
                FpsYear = DefaultFpsYear,
                Price = 200m
            };

        private static TestRequirementRCCostReq CreateTestReq() =>
            new()
            {
                TestCode = DefaultTestCode,
                Buyer = DefaultBuyer,
                ProfitCentre = DefaultProfitCentre,
                FpsYear = DefaultFpsYear,
                Price = 200m
            };

        private static TestRequirementRCCostRes CreateTestRes() =>
            new()
            {
                TestCode = DefaultTestCode,
                Buyer = DefaultBuyer,
                ProfitCentre = DefaultProfitCentre,
                FpsYear = DefaultFpsYear,
                Price = 200m
            };

        #endregion
    }
}
