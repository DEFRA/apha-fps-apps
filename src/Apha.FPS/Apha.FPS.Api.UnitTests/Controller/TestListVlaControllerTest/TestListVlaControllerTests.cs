using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.TestListVlaControllerTest
{
    public class TestListVlaControllerTests
    {
        private const string DefaultItemCode = "TEST001";
        private const int DefaultFpsYear = 2025;

        private readonly ITestListVlaService _service;
        private readonly IMapper _mapper;
        private readonly TestListVlaController _controller;

        public TestListVlaControllerTests()
        {
            _service = Substitute.For<ITestListVlaService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new TestListVlaController(_service, _mapper);
        }

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_ServiceReturnsPagedData_ReturnsOkWithMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestListVlaDto>
            {
                Data = new List<TestListVlaDto> { CreateTestDto() },
                PaginationData = new PaginationDto { TotalRecords = 1 }
            };
            var mappedRes = new PaginationRes<TestListVlaRes>
            {
                Data = new List<TestListVlaRes> { CreateTestRes() },
                PaginationData = new Pagination { TotalRecords = 1 }
            };

            _service.GetAllAsync(query).Returns(serviceResult);
            _mapper.Map<PaginationRes<TestListVlaRes>>(serviceResult).Returns(mappedRes);

            // Act
            var result = await _controller.GetAllAsync(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<PaginationRes<TestListVlaRes>>(okResult.Value);
            Assert.Single(data.Data);
        }

        [Fact]
        public async Task GetAllAsync_ServiceReturnsEmpty_ReturnsOkWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestListVlaDto>
            {
                Data = new List<TestListVlaDto>(),
                PaginationData = new PaginationDto { TotalRecords = 0 }
            };
            var mappedRes = new PaginationRes<TestListVlaRes>
            {
                Data = new List<TestListVlaRes>(),
                PaginationData = new Pagination { TotalRecords = 0 }
            };

            _service.GetAllAsync(query).Returns(serviceResult);
            _mapper.Map<PaginationRes<TestListVlaRes>>(serviceResult).Returns(mappedRes);

            // Act
            var result = await _controller.GetAllAsync(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<PaginationRes<TestListVlaRes>>(okResult.Value);
            Assert.Empty(data.Data);
        }

        [Fact]
        public async Task GetAllAsync_CallsServiceWithCorrectParameters()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 5 };
            var serviceResult = new PaginatedResult<TestListVlaDto>
            {
                Data = new List<TestListVlaDto>(),
                PaginationData = new PaginationDto()
            };
            _service.GetAllAsync(query).Returns(serviceResult);
            _mapper.Map<PaginationRes<TestListVlaRes>>(serviceResult)
                .Returns(new PaginationRes<TestListVlaRes> { Data = new List<TestListVlaRes>(), PaginationData = new Pagination() });

            // Act
            await _controller.GetAllAsync(query);

            // Assert
            await _service.Received(1).GetAllAsync(query);
        }

        #endregion

        #region GetAllByYearAsync

        [Fact]
        public async Task GetAllByYearAsync_ServiceReturnsList_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtoList = new List<TestListVlaDto> { CreateTestDto(), CreateTestDto() };
            var resList = new List<TestListVlaRes> { CreateTestRes(), CreateTestRes() };

            _service.GetAllByYearAsync().Returns(dtoList);
            _mapper.Map<List<TestListVlaRes>>(dtoList).Returns(resList);

            // Act
            var result = await _controller.GetAllByYearAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<TestListVlaRes>>(okResult.Value);
            Assert.Equal(2, data.Count);
        }

        [Fact]
        public async Task GetAllByYearAsync_ServiceReturnsEmpty_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtoList = new List<TestListVlaDto>();
            var resList = new List<TestListVlaRes>();

            _service.GetAllByYearAsync().Returns(dtoList);
            _mapper.Map<List<TestListVlaRes>>(dtoList).Returns(resList);

            // Act
            var result = await _controller.GetAllByYearAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<TestListVlaRes>>(okResult.Value);
            Assert.Empty(data);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ExistingRecord_ReturnsOkWithMappedRecord()
        {
            // Arrange
            var dto = CreateTestDto();
            var res = CreateTestRes();

            _service.GetByKeyAsync(DefaultItemCode).Returns(dto);
            _mapper.Map<TestListVlaRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetByIdAsync(DefaultItemCode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<TestListVlaRes>(okResult.Value);
        }

        [Fact]
        public async Task GetByIdAsync_RecordNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _service.GetByKeyAsync("NOTEXIST").Returns((TestListVlaDto?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.GetByIdAsync("NOTEXIST"));
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

            _mapper.Map<TestListVlaDto>(req).Returns(dto);
            _service.CreateAsync(dto).Returns(dto);
            _mapper.Map<TestListVlaRes>(dto).Returns(res);

            // Act
            var result = await _controller.CreateAsync(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<TestListVlaRes>(okResult.Value);
            await _service.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateAsync_ServiceThrowsInvalidOperation_PropagatesException()
        {
            // Arrange
            var req = CreateTestReq();
            var dto = CreateTestDto();

            _mapper.Map<TestListVlaDto>(req).Returns(dto);
            _service.CreateAsync(dto).Returns<TestListVlaDto>(x =>
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

            _mapper.Map<TestListVlaDto>(req).Returns(dto);
            _service.UpdateAsync(DefaultItemCode, dto).Returns(dto);
            _mapper.Map<TestListVlaRes>(dto).Returns(res);

            // Act
            var result = await _controller.UpdateAsync(DefaultItemCode, req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<TestListVlaRes>(okResult.Value);
            await _service.Received(1).UpdateAsync(DefaultItemCode, dto);
        }

        [Fact]
        public async Task UpdateAsync_RecordNotFound_PropagatesKeyNotFoundException()
        {
            // Arrange
            var req = CreateTestReq();
            var dto = CreateTestDto();

            _mapper.Map<TestListVlaDto>(req).Returns(dto);
            _service.UpdateAsync("NOTEXIST", dto).Returns<TestListVlaDto>(x =>
                throw new KeyNotFoundException("Not found"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.UpdateAsync("NOTEXIST", req));
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ExistingRecord_ReturnsOkWithTrue()
        {
            // Arrange
            _service.DeleteAsync(DefaultItemCode).Returns(true);

            // Act
            var result = await _controller.DeleteAsync(DefaultItemCode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
            await _service.Received(1).DeleteAsync(DefaultItemCode);
        }

        [Fact]
        public async Task DeleteAsync_RecordNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _service.DeleteAsync("NOTEXIST").Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.DeleteAsync("NOTEXIST"));
        }

        #endregion

        #region Helper Methods

        private static TestListVlaDto CreateTestDto() =>
            new()
            {
                ItemCode = DefaultItemCode,
                FpsYear = DefaultFpsYear,
                ItemDescription = "Test Description",
                Owner = "PT",
                DefraUnitPrice = 100m
            };

        private static TestListVlaReq CreateTestReq() =>
            new()
            {
                ItemCode = DefaultItemCode,
                FpsYear = DefaultFpsYear,
                ItemDescription = "Test Description",
                Owner = "PT",
                DefraUnitPrice = 100m
            };

        private static TestListVlaRes CreateTestRes() =>
            new()
            {
                ItemCode = DefaultItemCode,
                FpsYear = DefaultFpsYear,
                ItemDescription = "Test Description",
                Owner = "PT",
                DefraUnitPrice = 100m
            };

        #endregion
    }
}
