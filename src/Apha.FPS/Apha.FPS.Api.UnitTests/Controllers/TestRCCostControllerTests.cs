using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controllers.TestRCCostControllerTest
{
    public class TestRCCostControllerTests
    {
        private const string DefaultTestCode = "TEST001";
        private const string DefaultProfitCentre = "PC001";
        private const int DefaultFpsYear = 2025;

        private readonly ITestRCCostService _service;
        private readonly IMapper _mapper;
        private readonly TestRCCostController _controller;

        public TestRCCostControllerTests()
        {
            _service = Substitute.For<ITestRCCostService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new TestRCCostController(_service, _mapper);
        }

        #region GetByTestCodeAsync

        [Fact]
        public async Task GetByTestCodeAsync_ServiceReturnsList_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtoList = new List<TestRCCostDto> { CreateTestDto(), CreateTestDto() };
            var resList = new List<TestRCCostRes> { CreateTestRes(), CreateTestRes() };

            _service.GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear).Returns(dtoList);
            _mapper.Map<List<TestRCCostRes>>(dtoList).Returns(resList);

            // Act
            var result = await _controller.GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<TestRCCostRes>>(okResult.Value);
            Assert.Equal(2, data.Count);
        }

        [Fact]
        public async Task GetByTestCodeAsync_ServiceReturnsEmpty_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtoList = new List<TestRCCostDto>();
            var resList = new List<TestRCCostRes>();

            _service.GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear).Returns(dtoList);
            _mapper.Map<List<TestRCCostRes>>(dtoList).Returns(resList);

            // Act
            var result = await _controller.GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<TestRCCostRes>>(okResult.Value);
            Assert.Empty(data);
        }

        [Fact]
        public async Task GetByTestCodeAsync_CallsServiceWithCorrectParameters()
        {
            // Arrange
            var dtoList = new List<TestRCCostDto>();
            _service.GetByTestCodeAsync("ALPHA", 2024).Returns(dtoList);
            _mapper.Map<List<TestRCCostRes>>(dtoList).Returns(new List<TestRCCostRes>());

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

            _service.GetByKeyAsync(DefaultTestCode, DefaultProfitCentre, DefaultFpsYear).Returns(dto);
            _mapper.Map<TestRCCostRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetByKeyAsync(DefaultTestCode, DefaultProfitCentre, DefaultFpsYear);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<TestRCCostRes>(okResult.Value);
        }

        [Fact]
        public async Task GetByKeyAsync_RecordNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _service.GetByKeyAsync("NOTEXIST", "PC999", DefaultFpsYear).Returns((TestRCCostDto?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.GetByKeyAsync("NOTEXIST", "PC999", DefaultFpsYear));
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

            _mapper.Map<TestRCCostDto>(req).Returns(dto);
            _service.CreateAsync(dto).Returns(dto);
            _mapper.Map<TestRCCostRes>(dto).Returns(res);

            // Act
            var result = await _controller.CreateAsync(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<TestRCCostRes>(okResult.Value);
            await _service.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateAsync_ServiceThrowsInvalidOperation_PropagatesException()
        {
            // Arrange
            var req = CreateTestReq();
            var dto = CreateTestDto();

            _mapper.Map<TestRCCostDto>(req).Returns(dto);
            _service.CreateAsync(dto).Returns<TestRCCostDto>(x =>
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

            _mapper.Map<TestRCCostDto>(req).Returns(dto);
            _service.UpdateAsync(DefaultTestCode, DefaultProfitCentre, DefaultFpsYear, dto).Returns(dto);
            _mapper.Map<TestRCCostRes>(dto).Returns(res);

            // Act
            var result = await _controller.UpdateAsync(DefaultTestCode, DefaultProfitCentre, DefaultFpsYear, req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<TestRCCostRes>(okResult.Value);
            await _service.Received(1).UpdateAsync(DefaultTestCode, DefaultProfitCentre, DefaultFpsYear, dto);
        }

        [Fact]
        public async Task UpdateAsync_RecordNotFound_PropagatesKeyNotFoundException()
        {
            // Arrange
            var req = CreateTestReq();
            var dto = CreateTestDto();

            _mapper.Map<TestRCCostDto>(req).Returns(dto);
            _service.UpdateAsync("NOTEXIST", "PC999", DefaultFpsYear, dto).Returns<TestRCCostDto>(x =>
                throw new KeyNotFoundException("Not found"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.UpdateAsync("NOTEXIST", "PC999", DefaultFpsYear, req));
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ExistingRecord_ReturnsOkWithTrue()
        {
            // Arrange
            _service.DeleteAsync(DefaultTestCode, DefaultProfitCentre, DefaultFpsYear).Returns(true);

            // Act
            var result = await _controller.DeleteAsync(DefaultTestCode, DefaultProfitCentre, DefaultFpsYear);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
            await _service.Received(1).DeleteAsync(DefaultTestCode, DefaultProfitCentre, DefaultFpsYear);
        }

        [Fact]
        public async Task DeleteAsync_RecordNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _service.DeleteAsync("NOTEXIST", "PC999", DefaultFpsYear).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.DeleteAsync("NOTEXIST", "PC999", DefaultFpsYear));
        }

        #endregion

        #region Helper Methods

        private static TestRCCostDto CreateTestDto() =>
            new()
            {
                TestCode = DefaultTestCode,
                ProfitCentre = DefaultProfitCentre,
                FpsYear = DefaultFpsYear,
                Price = 150m
            };

        private static TestRCCostReq CreateTestReq() =>
            new()
            {
                TestCode = DefaultTestCode,
                ProfitCentre = DefaultProfitCentre,
                FpsYear = DefaultFpsYear,
                Price = 150m
            };

        private static TestRCCostRes CreateTestRes() =>
            new()
            {
                TestCode = DefaultTestCode,
                ProfitCentre = DefaultProfitCentre,
                FpsYear = DefaultFpsYear,
                Price = 150m
            };

        #endregion
    }
}
