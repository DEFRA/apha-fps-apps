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

namespace Apha.PACT.Api.UnitTests.Controller.TestRequirementControllerTest
{
    public class TestRequirementControllerTests
    {
        private readonly ITestRequirementService _service;
        private readonly IMapper _mapper;
        private readonly TestRequirementController _controller;

        public TestRequirementControllerTests()
        {
            _service = Substitute.For<ITestRequirementService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new TestRequirementController(_service, _mapper);
        }

        #region GetPagedBySupplierTestCode

        [Fact]
        public async Task GetPagedBySupplierTestCode_HappyPath_ReturnsOkWithPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestSupplierViewDto>();
            var mapped = new PaginationRes<TestSupplierViewRes>();

            _service.GetPagedBySupplierTestCodeAsync(query, "BLOOD", false).Returns(serviceResult);
            _mapper.Map<PaginationRes<TestSupplierViewRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetPagedBySupplierTestCode(query, "BLOOD");

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCode_ShowRejectedTrue_PassesFlagToService()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestSupplierViewDto>();
            var mapped = new PaginationRes<TestSupplierViewRes>();

            _service.GetPagedBySupplierTestCodeAsync(query, "BLOOD", true).Returns(serviceResult);
            _mapper.Map<PaginationRes<TestSupplierViewRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetPagedBySupplierTestCode(query, "BLOOD", showRejected: true);

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
            await _service.Received(1).GetPagedBySupplierTestCodeAsync(query, "BLOOD", true);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCode_WithItems_ReturnsMappedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<TestSupplierViewDto>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", TestCost = 30m },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", TestCost = 20m }
            };
            var serviceResult = new PaginatedResult<TestSupplierViewDto>(dtos, new PaginationDto());
            var mapped = new PaginationRes<TestSupplierViewRes>();

            _service.GetPagedBySupplierTestCodeAsync(query, "BLOOD", false).Returns(serviceResult);
            _mapper.Map<PaginationRes<TestSupplierViewRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetPagedBySupplierTestCode(query, "BLOOD");

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
            await _service.Received(1).GetPagedBySupplierTestCodeAsync(query, "BLOOD", false);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCode_EmptyResult_ReturnsOkWithEmptyResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestSupplierViewDto>([], new PaginationDto());
            var mapped = new PaginationRes<TestSupplierViewRes>();

            _service.GetPagedBySupplierTestCodeAsync(query, "BLOOD", false).Returns(serviceResult);
            _mapper.Map<PaginationRes<TestSupplierViewRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetPagedBySupplierTestCode(query, "BLOOD");

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCode_ServiceThrows_PropagatesException()
        {
            var query = new QueryParameters<string>();
            _service.GetPagedBySupplierTestCodeAsync(query, "BLOOD", false)
                .ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() =>
                _controller.GetPagedBySupplierTestCode(query, "BLOOD"));
        }

        [Fact]
        public async Task GetPagedBySupplierTestCode_ShowRejectedDefaultsFalse_CallsServiceWithFalse()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 5 };
            var serviceResult = new PaginatedResult<TestSupplierViewDto>();
            var mapped = new PaginationRes<TestSupplierViewRes>();

            _service.GetPagedBySupplierTestCodeAsync(query, "URINE", false).Returns(serviceResult);
            _mapper.Map<PaginationRes<TestSupplierViewRes>>(serviceResult).Returns(mapped);

            await _controller.GetPagedBySupplierTestCode(query, "URINE");

            await _service.Received(1).GetPagedBySupplierTestCodeAsync(query, "URINE", false);
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

        [Fact]
        public async Task GetPagedTestReqmt_ServiceThrows_PropagatesException()
        {
            var query = new QueryParameters<string>();
            _service.GetPagedTestReqmtAsync(query, "BLOOD").ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetPagedTestReqmt(query, "BLOOD"));
        }

        #endregion

        #region GetPagedByProject

        [Fact]
        public async Task GetPagedByProject_HappyPath_ReturnsOkWithPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestRequirementtDto>();
            var mapped = new PaginationRes<TestRequirementtRes>();

            _service.GetPagedTestReqmtByProjectAsync(query, "PRJ1").Returns(serviceResult);
            _mapper.Map<PaginationRes<TestRequirementtRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetPagedByProject(query, "PRJ1");

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task GetPagedByProject_WithItems_ReturnsMappedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<TestRequirementtDto>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1" },
                new() { TestCode = "URINE", Buyer = "PRJ1" }
            };
            var serviceResult = new PaginatedResult<TestRequirementtDto>(dtos, new PaginationDto());
            var mapped = new PaginationRes<TestRequirementtRes>();

            _service.GetPagedTestReqmtByProjectAsync(query, "PRJ1").Returns(serviceResult);
            _mapper.Map<PaginationRes<TestRequirementtRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetPagedByProject(query, "PRJ1");

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
            await _service.Received(1).GetPagedTestReqmtByProjectAsync(query, "PRJ1");
        }

        [Fact]
        public async Task GetPagedByProject_ServiceThrows_PropagatesException()
        {
            var query = new QueryParameters<string>();
            _service.GetPagedTestReqmtByProjectAsync(query, "PRJ1")
                .ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetPagedByProject(query, "PRJ1"));
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
