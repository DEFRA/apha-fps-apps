using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Api.Controllers;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Api.UnitTests.Controller.TestListControllerTest
{
    /// <summary>
    /// Unit tests for TestListController (API Layer).
    /// Tests API validation, mapping, and exception handling.
    /// </summary>
    public class TestListControllerTests
    {
        private readonly ITestListService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly TestListController _controller;

        public TestListControllerTests()
        {
            _serviceMock = Substitute.For<ITestListService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new TestListController(_serviceMock, _mapperMock);
        }

        #region GetPaged

        [Fact]
        public async Task GetPaged_ValidQuery_ReturnsOkWithWrappedResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<TestOrProductDto> { new() { ItemCode = "TEST001", DefraUnitPrice = 100m } };
            var paginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var serviceResult = new PaginatedResult<TestOrProductDto>(dtos, paginationData);
            var mappedResult = new PaginationRes<TestOrProductRes>
            {
                Data = new List<TestOrProductRes> { new() { ItemCode = "TEST001", DefraUnitPrice = 100m } },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 }
            };

            _serviceMock.GetPagedTestOrProductsAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<TestOrProductRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetPaged(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<PaginationRes<TestOrProductRes>>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal(mappedResult, apiResponse.Data);
        }

        [Fact]
        public async Task GetPaged_EmptyResult_ReturnsOkWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestOrProductDto>(Enumerable.Empty<TestOrProductDto>(), new PaginationDto());
            var mappedResult = new PaginationRes<TestOrProductRes>();

            _serviceMock.GetPagedTestOrProductsAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<TestOrProductRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetPaged(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<PaginationRes<TestOrProductRes>>>(okResult.Value);
            Assert.True(apiResponse.Success);
        }

        [Fact]
        public async Task GetPaged_ServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>();
            _serviceMock.GetPagedTestOrProductsAsync(query).ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPaged(query));
        }

        #endregion

        #region GetById

        [Fact]
        public async Task GetById_ExistingItemCode_ReturnsOkWithMappedData()
        {
            // Arrange
            var dto = new TestOrProductDto { ItemCode = "TEST001", DefraUnitPrice = 100m };
            var mapped = new TestOrProductRes { ItemCode = "TEST001", DefraUnitPrice = 100m };

            _serviceMock.GetTestOrProductByIdAsync("TEST001").Returns(dto);
            _mapperMock.Map<TestOrProductRes>(dto).Returns(mapped);

            // Act
            var result = await _controller.GetById("TEST001");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetById_NonExistentItemCode_ThrowsKeyNotFoundException()
        {
            // Arrange
            _serviceMock.GetTestOrProductByIdAsync("MISSING").Returns((TestOrProductDto?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetById("MISSING"));
            Assert.Contains("MISSING", exception.Message);
            Assert.Contains("not found", exception.Message);
        }

        [Fact]
        public async Task GetById_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetTestOrProductByIdAsync("TEST001").ThrowsAsync(new ArgumentException("Invalid itemCode"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetById("TEST001"));
        }

        #endregion

        #region Create

        [Fact]
        public async Task Create_ValidRequest_ReturnsCreatedAtAction()
        {
            // Arrange
            var request = new TestOrProductReq { ItemCode = "TEST001", DefraUnitPrice = 100m };
            var dto = new TestOrProductDto { ItemCode = "TEST001", DefraUnitPrice = 100m };
            var createdDto = new TestOrProductDto { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = 2024 };
            var mapped = new TestOrProductRes { ItemCode = "TEST001", DefraUnitPrice = 100m };

            _mapperMock.Map<TestOrProductDto>(request).Returns(dto);
            _serviceMock.CreateTestOrProductAsync(dto).Returns(createdDto);
            _mapperMock.Map<TestOrProductRes>(createdDto).Returns(mapped);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(TestListController.GetById), createdResult.ActionName);
            Assert.True(createdResult.RouteValues!.ContainsKey("itemCode"));
            Assert.Equal("TEST001", createdResult.RouteValues["itemCode"]);
            Assert.Equal(mapped, createdResult.Value);
        }

        [Fact]
        public async Task Create_ServiceThrowsArgumentException_PropagatesException()
        {
            // Arrange
            var request = new TestOrProductReq { ItemCode = "TEST001", DefraUnitPrice = -1m };
            var dto = new TestOrProductDto { ItemCode = "TEST001", DefraUnitPrice = -1m };

            _mapperMock.Map<TestOrProductDto>(request).Returns(dto);
            _serviceMock.CreateTestOrProductAsync(dto).ThrowsAsync(new ArgumentException("Validation failed"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.Create(request));
        }

        [Fact]
        public async Task Create_ServiceThrowsInvalidOperationException_PropagatesException()
        {
            // Arrange
            var request = new TestOrProductReq { ItemCode = "TEST001", DefraUnitPrice = 100m };
            var dto = new TestOrProductDto { ItemCode = "TEST001", DefraUnitPrice = 100m };

            _mapperMock.Map<TestOrProductDto>(request).Returns(dto);
            _serviceMock.CreateTestOrProductAsync(dto).ThrowsAsync(new InvalidOperationException("Failed to create"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Create(request));
        }

        #endregion

        #region Update

        [Fact]
        public async Task Update_ValidRequest_ReturnsOkWithMappedData()
        {
            // Arrange
            var itemCode = "TEST001";
            var request = new TestOrProductReq { DefraUnitPrice = 150m };
            var dto = new TestOrProductDto { ItemCode = itemCode, DefraUnitPrice = 150m };
            var updatedDto = new TestOrProductDto { ItemCode = itemCode, DefraUnitPrice = 150m, FpsYear = 2024 };
            var mapped = new TestOrProductRes { ItemCode = itemCode, DefraUnitPrice = 150m };

            _mapperMock.Map<TestOrProductDto>(request).Returns(dto);
            _serviceMock.UpdateTestOrProductAsync(dto).Returns(updatedDto);
            _mapperMock.Map<TestOrProductRes>(updatedDto).Returns(mapped);

            // Act
            var result = await _controller.Update(itemCode, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
            Assert.Equal(itemCode, dto.ItemCode); // Verify itemCode was set
        }

        [Fact]
        public async Task Update_ItemCodeSetInDto_OverridesRequestValue()
        {
            // Arrange
            var itemCode = "TEST001";
            var request = new TestOrProductReq { DefraUnitPrice = 150m };
            var dto = new TestOrProductDto { DefraUnitPrice = 150m };
            var updatedDto = new TestOrProductDto { ItemCode = itemCode, DefraUnitPrice = 150m };
            var mapped = new TestOrProductRes { ItemCode = itemCode, DefraUnitPrice = 150m };

            _mapperMock.Map<TestOrProductDto>(request).Returns(dto);
            _serviceMock.UpdateTestOrProductAsync(Arg.Do<TestOrProductDto>(d => Assert.Equal(itemCode, d.ItemCode))).Returns(updatedDto);
            _mapperMock.Map<TestOrProductRes>(updatedDto).Returns(mapped);

            // Act
            var result = await _controller.Update(itemCode, request);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Update_ServiceThrowsInvalidOperationException_PropagatesException()
        {
            // Arrange
            var itemCode = "MISSING";
            var request = new TestOrProductReq { DefraUnitPrice = 150m };
            var dto = new TestOrProductDto { ItemCode = itemCode, DefraUnitPrice = 150m };

            _mapperMock.Map<TestOrProductDto>(request).Returns(dto);
            _serviceMock.UpdateTestOrProductAsync(dto).ThrowsAsync(new InvalidOperationException($"Test/Product with Item Code '{itemCode}' not found."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Update(itemCode, request));
            Assert.Contains(itemCode, exception.Message);
        }

        [Fact]
        public async Task Update_ServiceThrowsArgumentException_PropagatesException()
        {
            // Arrange
            var itemCode = "TEST001";
            var request = new TestOrProductReq { DefraUnitPrice = -1m };
            var dto = new TestOrProductDto { ItemCode = itemCode, DefraUnitPrice = -1m };

            _mapperMock.Map<TestOrProductDto>(request).Returns(dto);
            _serviceMock.UpdateTestOrProductAsync(dto).ThrowsAsync(new ArgumentException("Validation failed"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.Update(itemCode, request));
        }

        #endregion

        #region Delete

        [Fact]
        public async Task Delete_ExistingItemCode_ReturnsOkTrue()
        {
            // Arrange
            _serviceMock.DeleteTestOrProductAsync("TEST001").Returns(true);

            // Act
            var result = await _controller.Delete("TEST001");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task Delete_NonExistentItemCode_ThrowsKeyNotFoundException()
        {
            // Arrange
            _serviceMock.DeleteTestOrProductAsync("MISSING").Returns(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.Delete("MISSING"));
            Assert.Contains("MISSING", exception.Message);
            Assert.Contains("not found for deletion", exception.Message);
        }

        [Fact]
        public async Task Delete_ServiceThrowsInvalidOperationException_PropagatesException()
        {
            // Arrange
            _serviceMock.DeleteTestOrProductAsync("TEST001").ThrowsAsync(new InvalidOperationException("Not found"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Delete("TEST001"));
        }

        [Fact]
        public async Task Delete_ServiceThrowsArgumentException_PropagatesException()
        {
            // Arrange
            _serviceMock.DeleteTestOrProductAsync("").ThrowsAsync(new ArgumentException("ItemCode cannot be empty"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.Delete(""));
        }

        #endregion

        #region GetOwners

        [Fact]
        public async Task GetOwners_ReturnsOkWithOwnersList()
        {
            // Arrange
            var owners = new List<string> { "OW1", "OW2", "OW3" };
            _serviceMock.GetOwnersAsync().Returns(owners);

            // Act
            var result = await _controller.GetOwners();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(owners, okResult.Value);
        }

        [Fact]
        public async Task GetOwners_EmptyResult_ReturnsOkWithEmptyList()
        {
            // Arrange
            _serviceMock.GetOwnersAsync().Returns(new List<string>());

            // Act
            var result = await _controller.GetOwners();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultList = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value);
            Assert.Empty(resultList);
        }

        [Fact]
        public async Task GetOwners_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetOwnersAsync().ThrowsAsync(new InvalidOperationException("Failed to retrieve owners"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetOwners());
        }

        #endregion
    }
}
