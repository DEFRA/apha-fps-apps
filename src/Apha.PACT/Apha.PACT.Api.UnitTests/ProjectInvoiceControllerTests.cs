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

        #region Delete

        [Fact]
        public async Task Delete_ExistingId_ReturnsOkWithTrue()
        {
            // Arrange
            _service.DeleteAsync(3).Returns(true);

            // Act
            var result = await _controller.Delete(3);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(true, ok.Value);
        }

        [Fact]
        public async Task Delete_NotFound_ReturnsOkWithFalse()
        {
            // Arrange
            _service.DeleteAsync(99).Returns(false);

            // Act
            var result = await _controller.Delete(99);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(false, ok.Value);
        }

        #endregion
    }
}
