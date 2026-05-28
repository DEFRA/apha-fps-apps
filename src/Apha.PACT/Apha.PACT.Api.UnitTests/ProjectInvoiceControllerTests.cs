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

namespace Apha.PACT.Api.UnitTests
{
    public class ProjectInvoiceControllerTests
    {
        private readonly IProjectInvoiceService _service;
        private readonly IMapper _mapper;
        private readonly ProjectInvoiceController _controller;

        public ProjectInvoiceControllerTests()
        {
            _service = Substitute.For<IProjectInvoiceService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new ProjectInvoiceController(_service, _mapper);
        }

        #region GetMonthlyInvoicesSummary

        [Fact]
        public async Task GetMonthlyInvoicesSummary_ValidQuery_ReturnsOkWithMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pivotDto = new MonthlyInvoicesPivotDto
            {
                Months = [1, 2, 3],
                Rows = [],
                Pagination = new PaginationDto()
            };
            var pivotRes = new MonthlyInvoicesPivotRes { Months = [1, 2, 3] };
            _service.GetMonthlyInvoicesSummaryAsync(query).Returns(pivotDto);
            _mapper.Map<MonthlyInvoicesPivotRes>(pivotDto).Returns(pivotRes);

            // Act
            var result = await _controller.GetMonthlyInvoicesSummary(query);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(pivotRes, ok.Value);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummary_EmptyResult_ReturnsOkWithEmptyRows()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var pivotDto = new MonthlyInvoicesPivotDto();
            var pivotRes = new MonthlyInvoicesPivotRes();
            _service.GetMonthlyInvoicesSummaryAsync(query).Returns(pivotDto);
            _mapper.Map<MonthlyInvoicesPivotRes>(pivotDto).Returns(pivotRes);

            // Act
            var result = await _controller.GetMonthlyInvoicesSummary(query);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<MonthlyInvoicesPivotRes>(ok.Value);
        }

        #endregion

        #region GetPaged

        [Fact]
        public async Task GetPaged_ValidQuery_ReturnsOkWithPagedResult()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var paged = new PaginatedResult<ProjectInvoiceDto>(
                [new ProjectInvoiceDto { InvoiceCounter = 1 }],
                new PaginationDto { TotalRecords = 1 });
            var res = new PaginationRes<ProjectInvoiceRes>();
            _service.GetPagedProjectInvoicesAsync(query, "PRJ001").Returns(paged);
            _mapper.Map<PaginationRes<ProjectInvoiceRes>>(paged).Returns(res);

            // Act
            var result = await _controller.GetPaged(query, "PRJ001");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
        }

        #endregion

        #region GetTotal

        [Fact]
        public async Task GetTotal_ValidParentProject_ReturnsOkWithTotal()
        {
            // Arrange
            _service.GetTotalAmountAsync("PRJ001").Returns(3000m);

            // Act
            var result = await _controller.GetTotal("PRJ001");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(3000m, ok.Value);
        }

        [Fact]
        public async Task GetTotal_NullParentProject_ReturnsOkWithTotal()
        {
            // Arrange
            _service.GetTotalAmountAsync(null).Returns(0m);

            // Act
            var result = await _controller.GetTotal(null);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(0m, ok.Value);
        }

        #endregion

        #region GetById

        [Fact]
        public async Task GetById_ExistingId_ReturnsOkWithMappedRes()
        {
            // Arrange
            var dto = new ProjectInvoiceDto { InvoiceCounter = 1 };
            var res = new ProjectInvoiceRes { InvoiceCounter = 1 };
            _service.GetByIdAsync(1).Returns(dto);
            _mapper.Map<ProjectInvoiceRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
        }

        [Fact]
        public async Task GetById_NotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _service.GetByIdAsync(99).Returns((ProjectInvoiceDto?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetById(99));
        }

        #endregion

        #region Create

        [Fact]
        public async Task Create_ValidRequest_ReturnsCreatedAtAction()
        {
            // Arrange
            var req = new ProjectInvoiceReq();
            var dto = new ProjectInvoiceDto { InvoiceCounter = 10 };
            var created = new ProjectInvoiceDto { InvoiceCounter = 10 };
            var res = new ProjectInvoiceRes { InvoiceCounter = 10 };
            _mapper.Map<ProjectInvoiceDto>(req).Returns(dto);
            _service.CreateAsync(dto).Returns(created);
            _mapper.Map<ProjectInvoiceRes>(created).Returns(res);

            // Act
            var result = await _controller.Create(req);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetById), createdResult.ActionName);
        }

        #endregion

        #region Update

        [Fact]
        public async Task Update_ValidRequest_ReturnsOkWithUpdatedRes()
        {
            // Arrange
            var req = new ProjectInvoiceReq();
            var dto = new ProjectInvoiceDto { InvoiceCounter = 5 };
            var updated = new ProjectInvoiceDto { InvoiceCounter = 5 };
            var res = new ProjectInvoiceRes { InvoiceCounter = 5 };
            _mapper.Map<ProjectInvoiceDto>(req).Returns(dto);
            _service.UpdateAsync(Arg.Is<ProjectInvoiceDto>(d => d.InvoiceCounter == 5)).Returns(updated);
            _mapper.Map<ProjectInvoiceRes>(updated).Returns(res);

            // Act
            var result = await _controller.Update(5, req);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
        }

        #endregion

        #region CopyInvoices

        [Fact]
        public async Task CopyInvoices_ValidRequest_ReturnsOkWithSuccessResponse()
        {
            // Arrange
            var request = new CopyInvoicesReq
            {
                SourceMonth = 3,
                TargetMonth = 9,
                InvoiceIds = new List<int> { 1, 2, 3 }
            };
            var dto = new CopyInvoicesDto
            {
                SourceMonth = 3,
                TargetMonth = 9,
                InvoiceIds = new List<int> { 1, 2, 3 }
            };
            _mapper.Map<CopyInvoicesDto>(request).Returns(dto);
            _service.CopyInvoicesAsync(dto).Returns(true);

            // Act
            var result = await _controller.CopyInvoices(request);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<CopyInvoicesRes>(ok.Value);
            Assert.True(response.Success);
            Assert.Equal("Invoices copied successfully", response.Message);
            await _service.Received(1).CopyInvoicesAsync(dto);
        }

        [Fact]
        public async Task CopyInvoices_BulkCopyRequest_ReturnsOkWithSuccessResponse()
        {
            // Arrange
            var request = new CopyInvoicesReq
            {
                SourceMonth = 5,
                TargetMonth = 6,
                InvoiceIds = null,
                InvoiceRecords = null
            };
            var dto = new CopyInvoicesDto { SourceMonth = 5, TargetMonth = 6 };
            _mapper.Map<CopyInvoicesDto>(request).Returns(dto);
            _service.CopyInvoicesAsync(dto).Returns(true);

            // Act
            var result = await _controller.CopyInvoices(request);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<CopyInvoicesRes>(ok.Value);
            Assert.True(response.Success);
            Assert.Equal(0, response.CopiedCount);
            Assert.Equal(0, response.FailedCount);
            Assert.Empty(response.Errors);
        }

        [Fact]
        public async Task CopyInvoices_WithInvoiceRecords_ReturnsOkWithCopiedCount()
        {
            // Arrange
            var invoiceRecords = new List<ProjectInvoiceReq>
            {
                new ProjectInvoiceReq { ProjectParent = "PP001", Month = 3 },
                new ProjectInvoiceReq { ProjectParent = "PP002", Month = 3 }
            };
            var request = new CopyInvoicesReq
            {
                SourceMonth = 3,
                TargetMonth = 9,
                InvoiceRecords = invoiceRecords
            };
            var dto = new CopyInvoicesDto { SourceMonth = 3, TargetMonth = 9 };
            _mapper.Map<CopyInvoicesDto>(request).Returns(dto);
            _service.CopyInvoicesAsync(dto).Returns(true);

            // Act
            var result = await _controller.CopyInvoices(request);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<CopyInvoicesRes>(ok.Value);
            Assert.True(response.Success);
            Assert.Equal(2, response.CopiedCount);
        }

        [Fact]
        public async Task CopyInvoices_ServiceReturnsFalse_ReturnsOkWithFailureMessage()
        {
            // Arrange
            var request = new CopyInvoicesReq
            {
                SourceMonth = 3,
                TargetMonth = 9
            };
            var dto = new CopyInvoicesDto { SourceMonth = 3, TargetMonth = 9 };
            _mapper.Map<CopyInvoicesDto>(request).Returns(dto);
            _service.CopyInvoicesAsync(dto).Returns(false);

            // Act
            var result = await _controller.CopyInvoices(request);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<CopyInvoicesRes>(ok.Value);
            Assert.False(response.Success);
            Assert.Equal("Failed to copy invoices", response.Message);
            Assert.Equal(0, response.CopiedCount);
        }

        [Fact]
        public async Task CopyInvoices_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new CopyInvoicesReq
            {
                SourceMonth = 3,
                TargetMonth = 9
            };
            var dto = new CopyInvoicesDto { SourceMonth = 3, TargetMonth = 9 };
            _mapper.Map<CopyInvoicesDto>(request).Returns(dto);
            _service.CopyInvoicesAsync(dto).ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.CopyInvoices(request));
        }

        [Fact]
        public async Task CopyInvoices_EmptyInvoiceIdsList_ReturnsOkWithSuccess()
        {
            // Arrange
            var request = new CopyInvoicesReq
            {
                SourceMonth = 1,
                TargetMonth = 12,
                InvoiceIds = new List<int>()
            };
            var dto = new CopyInvoicesDto
            {
                SourceMonth = 1,
                TargetMonth = 12,
                InvoiceIds = new List<int>()
            };
            _mapper.Map<CopyInvoicesDto>(request).Returns(dto);
            _service.CopyInvoicesAsync(dto).Returns(true);

            // Act
            var result = await _controller.CopyInvoices(request);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<CopyInvoicesRes>(ok.Value);
            Assert.True(response.Success);
        }

        [Fact]
        public async Task CopyInvoices_SelectiveCopyWithSpecificIds_CallsServiceWithMappedDto()
        {
            // Arrange
            var invoiceIds = new List<int> { 10, 20, 30 };
            var request = new CopyInvoicesReq
            {
                SourceMonth = 2,
                TargetMonth = 8,
                InvoiceIds = invoiceIds
            };
            var dto = new CopyInvoicesDto
            {
                SourceMonth = 2,
                TargetMonth = 8,
                InvoiceIds = invoiceIds
            };
            _mapper.Map<CopyInvoicesDto>(request).Returns(dto);
            _service.CopyInvoicesAsync(Arg.Is<CopyInvoicesDto>(d =>
                d.SourceMonth == 2 &&
                d.TargetMonth == 8 &&
                d.InvoiceIds != null &&
                d.InvoiceIds.Count == 3)).Returns(true);

            // Act
            var result = await _controller.CopyInvoices(request);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            await _service.Received(1).CopyInvoicesAsync(Arg.Any<CopyInvoicesDto>());
        }

        [Fact]
        public async Task CopyInvoices_MapperReturnsDto_PassesDtoToService()
        {
            // Arrange
            var request = new CopyInvoicesReq
            {
                SourceMonth = 4,
                TargetMonth = 7
            };
            var mappedDto = new CopyInvoicesDto
            {
                SourceMonth = 4,
                TargetMonth = 7,
                InvoiceIds = null
            };
            _mapper.Map<CopyInvoicesDto>(request).Returns(mappedDto);
            _service.CopyInvoicesAsync(mappedDto).Returns(true);

            // Act
            await _controller.CopyInvoices(request);

            // Assert
            _mapper.Received(1).Map<CopyInvoicesDto>(request);
            await _service.Received(1).CopyInvoicesAsync(mappedDto);
        }

        [Fact]
        public async Task CopyInvoices_SuccessWithNullInvoiceRecords_SetsCopiedCountToZero()
        {
            // Arrange
            var request = new CopyInvoicesReq
            {
                SourceMonth = 1,
                TargetMonth = 2,
                InvoiceRecords = null
            };
            var dto = new CopyInvoicesDto { SourceMonth = 1, TargetMonth = 2 };
            _mapper.Map<CopyInvoicesDto>(request).Returns(dto);
            _service.CopyInvoicesAsync(dto).Returns(true);

            // Act
            var result = await _controller.CopyInvoices(request);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<CopyInvoicesRes>(ok.Value);
            Assert.Equal(0, response.CopiedCount);
        }

        [Fact]
        public async Task CopyInvoices_ResponseStructure_ContainsAllRequiredFields()
        {
            // Arrange
            var request = new CopyInvoicesReq
            {
                SourceMonth = 5,
                TargetMonth = 11
            };
            var dto = new CopyInvoicesDto { SourceMonth = 5, TargetMonth = 11 };
            _mapper.Map<CopyInvoicesDto>(request).Returns(dto);
            _service.CopyInvoicesAsync(dto).Returns(true);

            // Act
            var result = await _controller.CopyInvoices(request);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<CopyInvoicesRes>(ok.Value);
            Assert.NotNull(response.Message);
            Assert.NotNull(response.Errors);
            Assert.True(response.CopiedCount >= 0);
            Assert.True(response.FailedCount >= 0);
        }

        #endregion
    }
}
