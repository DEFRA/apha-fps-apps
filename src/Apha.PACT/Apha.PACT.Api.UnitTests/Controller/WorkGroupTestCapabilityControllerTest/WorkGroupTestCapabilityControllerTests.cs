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

namespace Apha.PACT.Api.UnitTests.Controller.WorkGroupTestCapabilityControllerTest
{
    public class WorkGroupTestCapabilityControllerTests
    {
        private readonly IWorkGroupTestCapabilityService _service;
        private readonly IMapper _mapper;
        private readonly WorkGroupTestCapabilityController _controller;

        public WorkGroupTestCapabilityControllerTests()
        {
            _service = Substitute.For<IWorkGroupTestCapabilityService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new WorkGroupTestCapabilityController(_service, _mapper);
        }

        #region GetPagedByWorkGroup

        [Fact]
        public async Task GetPagedByWorkGroup_HappyPath_ReturnsOkWithPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestCapabilityDto>();
            var mapped = new PaginationRes<TestCapabilityRes>();

            _service.GetPagedByWorkGroupAsync(query, "WG1").Returns(serviceResult);
            _mapper.Map<PaginationRes<TestCapabilityRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetPagedByWorkGroup(query, "WG1");

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task GetPagedByWorkGroup_NullWorkGroup_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestCapabilityDto>();
            var mapped = new PaginationRes<TestCapabilityRes>();

            _service.GetPagedByWorkGroupAsync(query, null).Returns(serviceResult);
            _mapper.Map<PaginationRes<TestCapabilityRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetPagedByWorkGroup(query, null);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetPagedByWorkGroup_ServiceThrows_PropagatesException()
        {
            var query = new QueryParameters<string>();
            _service.GetPagedByWorkGroupAsync(query, null).ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetPagedByWorkGroup(query, null));
        }

        #endregion

        #region GetPagedByTestCode

        [Fact]
        public async Task GetPagedByTestCode_HappyPath_ReturnsOkWithPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestCapabilityDto>();
            var mapped = new PaginationRes<TestCapabilityRes>();

            _service.GetPagedByTestCodeAsync(query, "TC1").Returns(serviceResult);
            _mapper.Map<PaginationRes<TestCapabilityRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetPagedByTestCode(query, "TC1");

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task GetPagedByTestCode_NullTestCode_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _service.GetPagedByTestCodeAsync(query, null).Returns(new PaginatedResult<TestCapabilityDto>());
            _mapper.Map<PaginationRes<TestCapabilityRes>>(Arg.Any<PaginatedResult<TestCapabilityDto>>())
                .Returns(new PaginationRes<TestCapabilityRes>());

            var result = await _controller.GetPagedByTestCode(query, null);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetTestCapabilityById

        [Fact]
        public async Task GetTestCapabilityById_RecordFound_ReturnsOk()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };
            var mapped = new TestCapabilityRes { TestCode = "TC1" };

            _service.GetTestCapabilityByIdAsync("TC1", "WG1").Returns(dto);
            _mapper.Map<TestCapabilityRes>(dto).Returns(mapped);

            var result = await _controller.GetTestCapabilityById("TC1", "WG1");

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task GetTestCapabilityById_RecordNotFound_ThrowsKeyNotFoundException()
        {
            _service.GetTestCapabilityByIdAsync("MISSING", "WG1").Returns((TestCapabilityDto?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.GetTestCapabilityById("MISSING", "WG1"));
        }

        #endregion

        #region CreateTestCapability

        [Fact]
        public async Task CreateTestCapability_ValidRequest_ReturnsOk()
        {
            var request = new TestCapabilityReq { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };
            var created = new TestCapabilityDto { TestCode = "TC1" };
            var mapped = new TestCapabilityRes { TestCode = "TC1" };

            _mapper.Map<TestCapabilityDto>(request).Returns(dto);
            _service.AddTestCapabilityAsync(dto).Returns(created);
            _mapper.Map<TestCapabilityRes>(created).Returns(mapped);

            var result = await _controller.CreateTestCapability(request);

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task CreateTestCapability_DuplicateRecord_ThrowsInvalidOperationException()
        {
            var request = new TestCapabilityReq { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var dto = new TestCapabilityDto();

            _mapper.Map<TestCapabilityDto>(request).Returns(dto);
            _service.AddTestCapabilityAsync(dto)
                .ThrowsAsync(new InvalidOperationException("Duplicate record exists."));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.CreateTestCapability(request));
        }

        #endregion

        #region UpdateTestCapability

        [Fact]
        public async Task UpdateTestCapability_ValidRequest_ReturnsOk()
        {
            var request = new TestCapabilityReq { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };
            var updated = new TestCapabilityDto { TestCode = "TC1" };
            var mapped = new TestCapabilityRes { TestCode = "TC1" };

            _mapper.Map<TestCapabilityDto>(request).Returns(dto);
            _service.UpdateTestCapabilityAsync(dto).Returns(updated);
            _mapper.Map<TestCapabilityRes>(updated).Returns(mapped);

            var result = await _controller.UpdateTestCapability(request);

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task UpdateTestCapability_HasDependentReqmts_ThrowsInvalidOperationException()
        {
            var request = new TestCapabilityReq { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var dto = new TestCapabilityDto();

            _mapper.Map<TestCapabilityDto>(request).Returns(dto);
            _service.UpdateTestCapabilityAsync(dto)
                .ThrowsAsync(new InvalidOperationException("Cannot update, test requirements are dependant on this."));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.UpdateTestCapability(request));
        }

        #endregion

        #region DeleteTestCapability

        [Fact]
        public async Task DeleteTestCapability_RecordDeleted_ReturnsOkWithTrue()
        {
            _service.DeleteTestCapabilityAsync("TC1", "WG1").Returns(true);

            var result = await _controller.DeleteTestCapability("TC1", "WG1");

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(true);
        }

        [Fact]
        public async Task DeleteTestCapability_HasReqmtsDependency_ThrowsInvalidOperationException()
        {
            _service.DeleteTestCapabilityAsync("TC1", "WG1")
                .ThrowsAsync(new InvalidOperationException("Cannot delete, test requirements are dependant on this."));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.DeleteTestCapability("TC1", "WG1"));
        }

        #endregion

        #region GetAllTestorProducts

        [Fact]
        public async Task GetAllTestorProducts_HappyPath_ReturnsOkWithMappedList()
        {
            var dtos = new List<TestorProductDto> { new() { ItemCode = "BLOOD" } };
            var mapped = new List<TestorProductRes> { new() { ItemCode = "BLOOD" } };

            _service.GetAllTestorProductsAsync().Returns(dtos);
            _mapper.Map<IEnumerable<TestorProductRes>>(dtos).Returns(mapped);

            var result = await _controller.GetAllTestorProducts();

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task GetAllTestorProducts_EmptyList_ReturnsOkWithEmptyCollection()
        {
            var dtos = new List<TestorProductDto>();
            var mapped = new List<TestorProductRes>();

            _service.GetAllTestorProductsAsync().Returns(dtos);
            _mapper.Map<IEnumerable<TestorProductRes>>(dtos).Returns(mapped);

            var result = await _controller.GetAllTestorProducts();

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        #endregion
    }
}
