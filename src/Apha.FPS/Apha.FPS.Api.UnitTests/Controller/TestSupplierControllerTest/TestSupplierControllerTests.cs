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
        private const string DefaultTestCode = "TEST001";
        private const string DefaultBuyer = "BUYER001";

        private readonly ITestSupplierService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly TestSupplierController _controller;

        public TestSupplierControllerTests()
        {
            _serviceMock = Substitute.For<ITestSupplierService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new TestSupplierController(_serviceMock, _mapperMock);
        }

        #region GetPagedAsync

        [Fact]
        public async Task GetPagedAsync_WithValidRequest_ReturnsOk()
        {
            var query = new PaginationReq<string>();
            var serviceResult = new PaginatedResult<TestSupplierViewDto>();
            var mappedResult = new PaginationRes<TestSupplierViewRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(new QueryParameters<string>());
            _serviceMock.GetPagedByTestCodeAsync(Arg.Any<QueryParameters<string>>(), DefaultTestCode, false).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<TestSupplierViewRes>>(serviceResult).Returns(mappedResult);

            var result = await _controller.GetPagedAsync(query, DefaultTestCode, false);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetPagedAsync_WhenServiceThrows_PropagatesException()
        {
            var query = new PaginationReq<string>();
            _mapperMock.Map<QueryParameters<string>>(query).Returns(new QueryParameters<string>());
            _serviceMock.GetPagedByTestCodeAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>(), Arg.Any<bool>())
                .Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetPagedAsync(query, DefaultTestCode, false));
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_WhenRecordFound_ReturnsOk()
        {
            var dto = new TestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var res = new TestRequirementRes { TestCode = DefaultTestCode, Buyer = DefaultBuyer };

            _serviceMock.GetByIdAsync(DefaultTestCode, DefaultBuyer).Returns(dto);
            _mapperMock.Map<TestRequirementRes>(dto).Returns(res);

            var result = await _controller.GetByIdAsync(DefaultTestCode, DefaultBuyer);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, okResult.Value);
        }

        [Fact]
        public async Task GetByIdAsync_WhenRecordNotFound_ReturnsNotFound()
        {
            _serviceMock.GetByIdAsync(DefaultTestCode, DefaultBuyer).Returns((TestRequirementDto?)null);

            var result = await _controller.GetByIdAsync(DefaultTestCode, DefaultBuyer);

            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region GetTestOrProductsAsync

        [Fact]
        public async Task GetTestOrProductsAsync_WithItems_ReturnsOk()
        {
            var dtos = new List<TestOrProductDto> { new() { ItemCode = "T001" } };
            var resList = new List<TestorProductRes> { new() { ItemCode = "T001" } };

            _serviceMock.GetTestOrProductsAsync().Returns(dtos);
            _mapperMock.Map<List<TestorProductRes>>(dtos).Returns(resList);

            var result = await _controller.GetTestOrProductsAsync();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(resList, okResult.Value);
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_WithValidRequest_ReturnsOk()
        {
            var req = new TestRequirementReq { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var dto = new TestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var resultDto = new TestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var res = new TestRequirementRes { TestCode = DefaultTestCode, Buyer = DefaultBuyer };

            _mapperMock.Map<TestRequirementDto>(req).Returns(dto);
            _serviceMock.AddAsync(dto).Returns(resultDto);
            _mapperMock.Map<TestRequirementRes>(resultDto).Returns(res);

            var result = await _controller.CreateAsync(req);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, okResult.Value);
        }

        [Fact]
        public async Task CreateAsync_WhenServiceThrows_PropagatesException()
        {
            var req = new TestRequirementReq { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var dto = new TestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };

            _mapperMock.Map<TestRequirementDto>(req).Returns(dto);
            _serviceMock.AddAsync(dto).Throws(new InvalidOperationException("Validation failed"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.CreateAsync(req));
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_WithValidRequest_ReturnsOk()
        {
            var req = new TestRequirementReq { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var dto = new TestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var resultDto = new TestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var res = new TestRequirementRes { TestCode = DefaultTestCode, Buyer = DefaultBuyer };

            _mapperMock.Map<TestRequirementDto>(req).Returns(dto);
            _serviceMock.UpdateAsync(dto).Returns(resultDto);
            _mapperMock.Map<TestRequirementRes>(resultDto).Returns(res);

            var result = await _controller.UpdateAsync(req);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, okResult.Value);
        }

        [Fact]
        public async Task UpdateAsync_WhenServiceThrows_PropagatesException()
        {
            var req = new TestRequirementReq { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var dto = new TestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };

            _mapperMock.Map<TestRequirementDto>(req).Returns(dto);
            _serviceMock.UpdateAsync(dto).Throws(new InvalidOperationException("Not found"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.UpdateAsync(req));
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_WhenDeleteSucceeds_ReturnsOk()
        {
            _serviceMock.DeleteAsync(DefaultTestCode, DefaultBuyer).Returns(true);

            var result = await _controller.DeleteAsync(DefaultTestCode, DefaultBuyer);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task DeleteAsync_WhenRecordNotFound_ReturnsNotFound()
        {
            _serviceMock.DeleteAsync(DefaultTestCode, DefaultBuyer).Returns(false);

            var result = await _controller.DeleteAsync(DefaultTestCode, DefaultBuyer);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteAsync_WhenServiceThrowsInvalidOperation_ReturnsConflict()
        {
            _serviceMock.DeleteAsync(DefaultTestCode, DefaultBuyer)
                .Throws(new InvalidOperationException("Monthly output records exist"));

            var result = await _controller.DeleteAsync(DefaultTestCode, DefaultBuyer);

            Assert.IsType<ConflictObjectResult>(result);
        }

        #endregion
    }
}
