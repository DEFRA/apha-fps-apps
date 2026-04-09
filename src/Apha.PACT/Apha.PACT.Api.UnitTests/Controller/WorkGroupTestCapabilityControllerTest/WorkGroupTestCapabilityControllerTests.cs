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

        #region GetPagedTestReqmt

        [Fact]
        public async Task GetPagedTestReqmt_HappyPath_ReturnsOkWithPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestRequirementtDto>();
            var mapped = new PaginationRes<TestRequirementtRes>();

            _service.GetPagedTestReqmtAsync(query, "BLOOD").Returns(serviceResult);
            _mapper.Map<PaginationRes<TestRequirementtRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetPagedTestReqmt(query, "BLOOD");

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        #endregion

        #region GetAllTestReqmtForExport

        [Fact]
        public async Task GetAllTestReqmtForExport_HappyPath_ReturnsOkWithAllItems()
        {
            var dtos = new List<TestRequirementtDto>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1" },
                new() { TestCode = "BLOOD", Buyer = "PRJ2" }
            };
            var mapped = new List<TestRequirementtRes>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1" },
                new() { TestCode = "BLOOD", Buyer = "PRJ2" }
            };

            _service.GetAllTestReqmtForExportAsync("BLOOD", null).Returns(dtos);
            _mapper.Map<IEnumerable<TestRequirementtRes>>(dtos).Returns(mapped);

            var result = await _controller.GetAllTestReqmtForExport("BLOOD", null);

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task GetAllTestReqmtForExport_WithFilter_PassesFilterToService()
        {
            const string filter = "{\"Buyer\":\"PRJ\"}";
            var dtos = new List<TestRequirementtDto>();
            var mapped = new List<TestRequirementtRes>();

            _service.GetAllTestReqmtForExportAsync("BLOOD", filter).Returns(dtos);
            _mapper.Map<IEnumerable<TestRequirementtRes>>(dtos).Returns(mapped);

            await _controller.GetAllTestReqmtForExport("BLOOD", filter);

            await _service.Received(1).GetAllTestReqmtForExportAsync("BLOOD", filter);
        }

        [Fact]
        public async Task GetAllTestReqmtForExport_EmptyResult_ReturnsOkWithEmptyList()
        {
            var dtos = new List<TestRequirementtDto>();
            var mapped = new List<TestRequirementtRes>();

            _service.GetAllTestReqmtForExportAsync("BLOOD", null).Returns(dtos);
            _mapper.Map<IEnumerable<TestRequirementtRes>>(dtos).Returns(mapped);

            var result = await _controller.GetAllTestReqmtForExport("BLOOD", null);

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        #endregion

        #region GetTestReqmtById

        [Fact]
        public async Task GetTestReqmtById_RecordFound_ReturnsOk()
        {
            var dto = new TestRequirementtDto { TestCode = "BLOOD", Buyer = "PRJ1" };
            var mapped = new TestRequirementtRes { TestCode = "BLOOD", Buyer = "PRJ1" };

            _service.GetTestReqmtByIdAsync("BLOOD", "PRJ1").Returns(dto);
            _mapper.Map<TestRequirementtRes>(dto).Returns(mapped);

            var result = await _controller.GetTestReqmtById("BLOOD", "PRJ1");

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task GetTestReqmtById_RecordNotFound_ThrowsKeyNotFoundException()
        {
            _service.GetTestReqmtByIdAsync("MISSING", "PRJ1").Returns((TestRequirementtDto?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.GetTestReqmtById("MISSING", "PRJ1"));
        }

        #endregion

        #region CreateTestReqmt

        [Fact]
        public async Task CreateTestReqmt_ValidRequest_ReturnsOk()
        {
            var request = new TestRequirementReq { TestCode = "BLOOD", Buyer = "PRJ1" };
            var dto = new TestRequirementtDto { TestCode = "BLOOD", Buyer = "PRJ1" };
            var created = new TestRequirementtDto { TestCode = "BLOOD", Buyer = "PRJ1" };
            var mapped = new TestRequirementtRes { TestCode = "BLOOD" };

            _mapper.Map<TestRequirementtDto>(request).Returns(dto);
            _service.AddTestReqmtAsync(dto).Returns(created);
            _mapper.Map<TestRequirementtRes>(created).Returns(mapped);

            var result = await _controller.CreateTestReqmt(request);

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task CreateTestReqmt_BothBuyerFieldsNull_ThrowsInvalidOperationException()
        {
            var request = new TestRequirementReq { TestCode = "BLOOD", Buyer = "PRJ1" };
            var dto = new TestRequirementtDto();

            _mapper.Map<TestRequirementtDto>(request).Returns(dto);
            _service.AddTestReqmtAsync(dto)
                .ThrowsAsync(new InvalidOperationException("Must fill in Project Buyer or Test Buyer"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.CreateTestReqmt(request));
        }

        #endregion

        #region UpdateTestReqmt

        [Fact]
        public async Task UpdateTestReqmt_ValidRequest_ReturnsOk()
        {
            var request = new TestRequirementReq { TestCode = "BLOOD", Buyer = "PRJ1" };
            var dto = new TestRequirementtDto { TestCode = "BLOOD", Buyer = "PRJ1" };
            var updated = new TestRequirementtDto { TestCode = "BLOOD", Buyer = "PRJ1" };
            var mapped = new TestRequirementtRes { TestCode = "BLOOD" };

            _mapper.Map<TestRequirementtDto>(request).Returns(dto);
            _service.UpdateTestReqmtAsync(dto).Returns(updated);
            _mapper.Map<TestRequirementtRes>(updated).Returns(mapped);

            var result = await _controller.UpdateTestReqmt(request);

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task UpdateTestReqmt_MonthlyOutputExists_ThrowsInvalidOperationException()
        {
            var request = new TestRequirementReq { TestCode = "BLOOD", Buyer = "PRJ1" };
            var dto = new TestRequirementtDto();

            _mapper.Map<TestRequirementtDto>(request).Returns(dto);
            _service.UpdateTestReqmtAsync(dto)
                .ThrowsAsync(new InvalidOperationException("Cannot update, existing data in Monthly Output."));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.UpdateTestReqmt(request));
        }

        #endregion

        #region DeleteTestReqmt

        [Fact]
        public async Task DeleteTestReqmt_RecordDeleted_ReturnsOkWithTrue()
        {
            _service.DeleteTestReqmtAsync("BLOOD", "PRJ1").Returns(true);

            var result = await _controller.DeleteTestReqmt("BLOOD", "PRJ1");

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(true);
        }

        [Fact]
        public async Task DeleteTestReqmt_MonthlyOutputExists_ThrowsInvalidOperationException()
        {
            _service.DeleteTestReqmtAsync("BLOOD", "PRJ1")
                .ThrowsAsync(new InvalidOperationException("Cannot delete, existing data in MonthlyOutput."));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.DeleteTestReqmt("BLOOD", "PRJ1"));
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

        #region GetTestReqmtPricing

        [Fact]
        public async Task GetTestReqmtPricing_RecordFound_ReturnsOk()
        {
            var dto = new TestRequirementtDto { TestCode = "BLOOD", RecUnitPrice = 10.5m };
            var mapped = new TestRequirementtRes { TestCode = "BLOOD" };

            _service.GetTestReqmtPricingAsync("BLOOD", null).Returns(dto);
            _mapper.Map<TestRequirementtRes>(dto).Returns(mapped);

            var result = await _controller.GetTestReqmtPricing("BLOOD", null);

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task GetTestReqmtPricing_RecordNotFound_ReturnsNotFound()
        {
            _service.GetTestReqmtPricingAsync("MISSING", null).Returns((TestRequirementtDto?)null);

            var result = await _controller.GetTestReqmtPricing("MISSING", null);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetTestReqmtPricing_WithProjectCode_PassesProjectCodeToService()
        {
            var dto = new TestRequirementtDto { TestCode = "BLOOD", RecUnitPrice = 5.0m };
            var mapped = new TestRequirementtRes { TestCode = "BLOOD" };

            _service.GetTestReqmtPricingAsync("BLOOD", "PRJ1").Returns(dto);
            _mapper.Map<TestRequirementtRes>(dto).Returns(mapped);

            await _controller.GetTestReqmtPricing("BLOOD", "PRJ1");

            await _service.Received(1).GetTestReqmtPricingAsync("BLOOD", "PRJ1");
        }

        #endregion
    }
}
