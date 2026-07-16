using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Api.Controllers;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Api.UnitTests.Controller.TestActualBreakdownControllerTest
{
    public class TestActualBreakdownControllerTests
    {
        private readonly ITestActualBreakdownService _service;
        private readonly IMapper _mapper;
        private readonly TestActualBreakdownController _controller;

        public TestActualBreakdownControllerTests()
        {
            _service    = Substitute.For<ITestActualBreakdownService>();
            _mapper     = Substitute.For<IMapper>();
            _controller = new TestActualBreakdownController(_service, _mapper);
        }

        #region GetPaged

        [Fact]
        public async Task GetPaged_HappyPath_ReturnsOkWithMappedResult()
        {
            var query         = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestActualBreakdownDto>();
            var mapped        = new PaginationRes<TestActualBreakdownRes>();

            _service.GetPagedAsync(query).Returns(serviceResult);
            _mapper.Map<PaginationRes<TestActualBreakdownRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetPaged(query);

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task GetPaged_WithItems_ReturnsMappedItems()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<TestActualBreakdownDto>
            {
                new() { TestCode = "PT0047", Buyer = "SV3300", Program = "Viro",  Portfolio = "QAPTPORT1", Month = 4, PCPrice = 159.00m, PCCost = 319.00m },
                new() { TestCode = "PT0049", Buyer = "SB4600", Program = "Bact",  Portfolio = "QAPTPORT1", Month = 4, PCPrice = 313.00m, PCCost = 313.00m }
            };
            var serviceResult = new PaginatedResult<TestActualBreakdownDto>(dtos, new PaginationDto());
            var mapped        = new PaginationRes<TestActualBreakdownRes>();

            _service.GetPagedAsync(query).Returns(serviceResult);
            _mapper.Map<PaginationRes<TestActualBreakdownRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetPaged(query);

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
            await _service.Received(1).GetPagedAsync(query);
        }

        [Fact]
        public async Task GetPaged_EmptyResult_ReturnsOk()
        {
            var query         = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestActualBreakdownDto>([], new PaginationDto());
            var mapped        = new PaginationRes<TestActualBreakdownRes>();

            _service.GetPagedAsync(query).Returns(serviceResult);
            _mapper.Map<PaginationRes<TestActualBreakdownRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetPaged(query);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetPaged_ServiceThrows_PropagatesException()
        {
            var query = new QueryParameters<string>();
            _service.GetPagedAsync(query).ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetPaged(query));
        }

        [Fact]
        public async Task GetPaged_CallsServiceExactlyOnce_WithExactQuery()
        {
            var query         = new QueryParameters<string> { Page = 2, PageSize = 25, SortBy = "testcode" };
            var serviceResult = new PaginatedResult<TestActualBreakdownDto>();
            var mapped        = new PaginationRes<TestActualBreakdownRes>();

            _service.GetPagedAsync(query).Returns(serviceResult);
            _mapper.Map<PaginationRes<TestActualBreakdownRes>>(serviceResult).Returns(mapped);

            await _controller.GetPaged(query);

            await _service.Received(1).GetPagedAsync(query);
        }

        [Fact]
        public async Task GetPaged_CallsMapperWithServiceResult()
        {
            var query         = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestActualBreakdownDto>();
            var mapped        = new PaginationRes<TestActualBreakdownRes>();

            _service.GetPagedAsync(query).Returns(serviceResult);
            _mapper.Map<PaginationRes<TestActualBreakdownRes>>(serviceResult).Returns(mapped);

            await _controller.GetPaged(query);

            _mapper.Received(1).Map<PaginationRes<TestActualBreakdownRes>>(serviceResult);
        }

        [Fact]
        public async Task GetPaged_WithDescendingSort_PassesQueryToService()
        {
            var query         = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "buyer", Descending = true };
            var serviceResult = new PaginatedResult<TestActualBreakdownDto>();
            var mapped        = new PaginationRes<TestActualBreakdownRes>();

            _service.GetPagedAsync(query).Returns(serviceResult);
            _mapper.Map<PaginationRes<TestActualBreakdownRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetPaged(query);

            Assert.IsType<OkObjectResult>(result);
            await _service.Received(1).GetPagedAsync(query);
        }

        [Fact]
        public async Task GetPaged_WithFilter_PassesQueryToService()
        {
            var query         = new QueryParameters<string> { Page = 1, PageSize = 10, Filter = "{\"TestCode\":\"PT\"}" };
            var serviceResult = new PaginatedResult<TestActualBreakdownDto>();
            var mapped        = new PaginationRes<TestActualBreakdownRes>();

            _service.GetPagedAsync(query).Returns(serviceResult);
            _mapper.Map<PaginationRes<TestActualBreakdownRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetPaged(query);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion
    }
}
