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

namespace Apha.PACT.Api.UnitTests.Controller.TestorProductControllerTest
{
    public class TestPriceCheckControllerTests
    {
        private readonly ITestorProductService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly TestorProductController _controller;

        public TestPriceCheckControllerTests()
        {
            _serviceMock = Substitute.For<ITestorProductService>();
            _mapperMock  = Substitute.For<IMapper>();
            _controller  = new TestorProductController(_serviceMock, _mapperMock);
        }

        #region GetTestPriceCheckPaged

        [Fact]
        public async Task GetTestPriceCheckPaged_ValidQuery_ReturnsOkWithMappedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestPriceCheckDto>(
                [new TestPriceCheckDto { TestCode = "T001", JobCode = "JOB001" }],
                new PaginationDto { TotalRecords = 1 });
            var mapped = new PaginationRes<TestPriceCheckRes>
            {
                Data = [new TestPriceCheckRes { TestCode = "T001", JobCode = "JOB001" }]
            };

            _serviceMock.GetTestPriceCheckPagedAsync(query, "all", null).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<TestPriceCheckRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetTestPriceCheckPaged(query, "all", null);

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginationRes<TestPriceCheckRes>>(ok.Value);
            Assert.Single(response.Data!);
            Assert.Equal("T001", response.Data!.First().TestCode);
        }

        [Fact]
        public async Task GetTestPriceCheckPaged_WithPriceFilterAndOwner_PassesParametersToService()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestPriceCheckDto>([], new PaginationDto());
            _serviceMock.GetTestPriceCheckPagedAsync(query, "zero", "AB").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<TestPriceCheckRes>>(serviceResult)
                .Returns(new PaginationRes<TestPriceCheckRes>());

            await _controller.GetTestPriceCheckPaged(query, "zero", "AB");

            await _serviceMock.Received(1).GetTestPriceCheckPagedAsync(query, "zero", "AB");
        }

        [Fact]
        public async Task GetTestPriceCheckPaged_EmptyResult_ReturnsOkWithEmptyData()
        {
            var query = new QueryParameters<string>();
            var serviceResult = new PaginatedResult<TestPriceCheckDto>([], new PaginationDto());
            _serviceMock.GetTestPriceCheckPagedAsync(query, "all", null).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<TestPriceCheckRes>>(serviceResult)
                .Returns(new PaginationRes<TestPriceCheckRes>());

            var result = await _controller.GetTestPriceCheckPaged(query);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetTestPriceCheckPaged_ServiceThrows_PropagatesException()
        {
            var query = new QueryParameters<string>();
            _serviceMock.GetTestPriceCheckPagedAsync(query, "all", null)
                .ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetTestPriceCheckPaged(query));
        }

        #endregion

        #region GetTestPriceCheckByKey

        [Fact]
        public async Task GetTestPriceCheckByKey_ExistingKey_ReturnsOkWithMappedData()
        {
            var dto    = new TestPriceCheckDto { TestCode = "T001", JobCode = "JOB001", TestPrice = 50m };
            var mapped = new TestPriceCheckRes { TestCode = "T001", JobCode = "JOB001", TestPrice = 50m };

            _serviceMock.GetTestPriceCheckByKeyAsync("T001", "JOB001").Returns(dto);
            _mapperMock.Map<TestPriceCheckRes>(dto).Returns(mapped);

            var result = await _controller.GetTestPriceCheckByKey("T001", "JOB001");

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<TestPriceCheckRes>(ok.Value);
            Assert.Equal("T001",   response.TestCode);
            Assert.Equal("JOB001", response.JobCode);
        }

        [Fact]
        public async Task GetTestPriceCheckByKey_NonExistentKey_ThrowsKeyNotFoundException()
        {
            _serviceMock.GetTestPriceCheckByKeyAsync("MISSING", "MISSING")
                .Returns((TestPriceCheckDto?)null);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _controller.GetTestPriceCheckByKey("MISSING", "MISSING"));

            Assert.Contains("MISSING", ex.Message);
        }

        [Fact]
        public async Task GetTestPriceCheckByKey_ServiceThrows_PropagatesException()
        {
            _serviceMock.GetTestPriceCheckByKeyAsync("T001", "JOB001")
                .ThrowsAsync(new InvalidOperationException("DB error"));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.GetTestPriceCheckByKey("T001", "JOB001"));
        }

        #endregion

        #region UpdateTestPriceCheck

        [Fact]
        public async Task UpdateTestPriceCheck_ValidRequest_ReturnsOkTrue()
        {
            var request = new TestPriceCheckReq { IsDefraProject = -1, TestPrice = 75m, DefraUnitPrice = 120m };
            var dto     = new TestPriceCheckDto  { IsDefraProject = -1, TestPrice = 75m, DefraUnitPrice = 120m };

            _mapperMock.Map<TestPriceCheckDto>(request).Returns(dto);
            _serviceMock.UpdateTestPriceCheckAsync("T001", "JOB001", dto).Returns(true);

            var result = await _controller.UpdateTestPriceCheck("T001", "JOB001", request);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(true, ok.Value);
        }

        [Fact]
        public async Task UpdateTestPriceCheck_MapsRequestToDto()
        {
            var request = new TestPriceCheckReq { IsDefraProject = 0, TestPrice = 50m, DefraUnitPrice = 80m };
            var dto     = new TestPriceCheckDto  { IsDefraProject = 0, TestPrice = 50m, DefraUnitPrice = 80m };

            _mapperMock.Map<TestPriceCheckDto>(request).Returns(dto);
            _serviceMock.UpdateTestPriceCheckAsync("T001", "JOB001", dto).Returns(true);

            await _controller.UpdateTestPriceCheck("T001", "JOB001", request);

            _mapperMock.Received(1).Map<TestPriceCheckDto>(request);
            await _serviceMock.Received(1).UpdateTestPriceCheckAsync("T001", "JOB001", dto);
        }

        [Fact]
        public async Task UpdateTestPriceCheck_ServiceReturnsFalse_ReturnsOkFalse()
        {
            var request = new TestPriceCheckReq { IsDefraProject = 0, TestPrice = 50m, DefraUnitPrice = 80m };
            var dto     = new TestPriceCheckDto  { IsDefraProject = 0, TestPrice = 50m, DefraUnitPrice = 80m };

            _mapperMock.Map<TestPriceCheckDto>(request).Returns(dto);
            _serviceMock.UpdateTestPriceCheckAsync("T001", "JOB001", dto).Returns(false);

            var result = await _controller.UpdateTestPriceCheck("T001", "JOB001", request);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(false, ok.Value);
        }

        [Fact]
        public async Task UpdateTestPriceCheck_ServiceThrows_PropagatesException()
        {
            var request = new TestPriceCheckReq { IsDefraProject = 0, TestPrice = 50m, DefraUnitPrice = 80m };
            var dto     = new TestPriceCheckDto  { IsDefraProject = 0, TestPrice = 50m, DefraUnitPrice = 80m };

            _mapperMock.Map<TestPriceCheckDto>(request).Returns(dto);
            _serviceMock.UpdateTestPriceCheckAsync("T001", "JOB001", dto)
                .ThrowsAsync(new InvalidOperationException("Update failed"));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.UpdateTestPriceCheck("T001", "JOB001", request));
        }

        #endregion
    }
}
