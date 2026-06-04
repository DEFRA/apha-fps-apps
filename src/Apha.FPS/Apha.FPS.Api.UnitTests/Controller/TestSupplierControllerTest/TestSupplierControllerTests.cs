using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Api.UnitTests.Controller.TestSupplierControllerTest
{
    public class TestSupplierControllerTests
    {
        private const string DefaultTestCode = "TST001";
        private const int DefaultPageNumber = 1;
        private const int DefaultPageSize = 10;

        private readonly ITestSupplierService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly TestSupplierController _controller;

        public TestSupplierControllerTests()
        {
            _serviceMock = Substitute.For<ITestSupplierService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new TestSupplierController(_serviceMock, _mapperMock);
        }

        private static PaginationReq<string> DefaultReq() =>
            new() { Page = DefaultPageNumber, PageSize = DefaultPageSize };

        private static QueryParameters<string> DefaultQuery() =>
            new() { Page = DefaultPageNumber, PageSize = DefaultPageSize };

        private static PaginatedResult<TestSupplierViewDto> BuildServiceResult(int count = 2) =>
            new(
                Enumerable.Range(1, count).Select(i => new TestSupplierViewDto
                {
                    TestCode = DefaultTestCode,
                    Buyer = $"B{i:D3}"
                }),
                new PaginationDto { PageNumber = 1, PageSize = DefaultPageSize, TotalRecords = count });

        private static PaginationRes<TestSupplierViewRes> BuildMappedRes(int count = 2) =>
            new(
                Enumerable.Range(1, count).Select(i => new TestSupplierViewRes
                {
                    TestCode = DefaultTestCode,
                    Buyer = $"B{i:D3}"
                }),
                new Pagination { PageNumber = 1, PageSize = DefaultPageSize, TotalRecords = count });

        #region GetPagedAsync Tests

        [Fact]
        public async Task GetPagedAsync_WithValidRequest_ReturnsOkWithPagedData()
        {
            // Arrange
            var req = DefaultReq();
            var query = DefaultQuery();
            var serviceResult = BuildServiceResult();
            var mapped = BuildMappedRes();

            _mapperMock.Map<QueryParameters<string>>(req).Returns(query);
            _serviceMock.GetPagedAsync(query, DefaultTestCode, false).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<TestSupplierViewRes>>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetPagedAsync(req, DefaultTestCode);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<PaginationRes<TestSupplierViewRes>>(ok.Value);
            Assert.Equal(2, data.Data.Count());
        }

        [Fact]
        public async Task GetPagedAsync_ShowRejectedTrue_PassesFlagToService()
        {
            // Arrange
            var req = DefaultReq();
            var query = DefaultQuery();
            var serviceResult = BuildServiceResult(1);
            var mapped = BuildMappedRes(1);

            _mapperMock.Map<QueryParameters<string>>(req).Returns(query);
            _serviceMock.GetPagedAsync(query, DefaultTestCode, true).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<TestSupplierViewRes>>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetPagedAsync(req, DefaultTestCode, showRejected: true);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _serviceMock.Received(1).GetPagedAsync(query, DefaultTestCode, true);
        }

        [Fact]
        public async Task GetPagedAsync_EmptyServiceResult_ReturnsOkWithEmptyData()
        {
            // Arrange
            var req = DefaultReq();
            var query = DefaultQuery();
            var serviceResult = BuildServiceResult(0);
            var mapped = BuildMappedRes(0);

            _mapperMock.Map<QueryParameters<string>>(req).Returns(query);
            _serviceMock.GetPagedAsync(query, DefaultTestCode, false).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<TestSupplierViewRes>>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetPagedAsync(req, DefaultTestCode);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<PaginationRes<TestSupplierViewRes>>(ok.Value);
            Assert.Empty(data.Data);
        }

        [Fact]
        public async Task GetPagedAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            var req = DefaultReq();
            var query = DefaultQuery();

            _mapperMock.Map<QueryParameters<string>>(req).Returns(query);
            _serviceMock.GetPagedAsync(query, DefaultTestCode, false)
                .Throws(new Exception("Service failure"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _controller.GetPagedAsync(req, DefaultTestCode));
        }

        [Fact]
        public async Task GetPagedAsync_CallsMapperForRequest_ThenService_ThenMapperForResult()
        {
            // Arrange
            var req = DefaultReq();
            var query = DefaultQuery();
            var serviceResult = BuildServiceResult();
            var mapped = BuildMappedRes();

            _mapperMock.Map<QueryParameters<string>>(req).Returns(query);
            _serviceMock.GetPagedAsync(query, DefaultTestCode, false).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<TestSupplierViewRes>>(serviceResult).Returns(mapped);

            // Act
            await _controller.GetPagedAsync(req, DefaultTestCode);

            // Assert
            _mapperMock.Received(1).Map<QueryParameters<string>>(req);
            await _serviceMock.Received(1).GetPagedAsync(query, DefaultTestCode, false);
            _mapperMock.Received(1).Map<PaginationRes<TestSupplierViewRes>>(serviceResult);
        }

        #endregion
    }
}
