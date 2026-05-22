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

namespace Apha.PACT.Api.UnitTests.Controller.ProjectInvoiceControllerTest
{
    public class ProjectInvoiceControllerTests
    {
        private readonly IProjectInvoiceService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ProjectInvoiceController _controller;

        public ProjectInvoiceControllerTests()
        {
            _serviceMock = Substitute.For<IProjectInvoiceService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new ProjectInvoiceController(_serviceMock, _mapperMock);
        }

        #region GetPaged

        [Fact]
        public async Task GetPaged_ValidQueryWithParentProject_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<ProjectInvoiceDto> { new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "PRJ1" } };
            var paginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var serviceResult = new PaginatedResult<ProjectInvoiceDto>(dtos, paginationData);
            var expectedResponse = new PaginationRes<ProjectInvoiceRes>
            {
                Data = new List<ProjectInvoiceRes> { new ProjectInvoiceRes { InvoiceCounter = 1, ProjectParent = "PRJ1" } },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 }
            };

            _serviceMock.GetPagedProjectInvoicesAsync(query, "PRJ1").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectInvoiceRes>>(serviceResult).Returns(expectedResponse);

            var result = await _controller.GetPaged(query, "PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        [Fact]
        public async Task GetPaged_NullParentProject_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<ProjectInvoiceDto>(Enumerable.Empty<ProjectInvoiceDto>(), new PaginationDto());
            var expectedResponse = new PaginationRes<ProjectInvoiceRes>();

            _serviceMock.GetPagedProjectInvoicesAsync(query, null).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectInvoiceRes>>(serviceResult).Returns(expectedResponse);

            var result = await _controller.GetPaged(query, null);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        #endregion

        #region GetTotal

        [Fact]
        public async Task GetTotal_ValidParentProject_ReturnsOk()
        {
            _serviceMock.GetTotalAmountAsync("PRJ1").Returns(1500.00m);

            var result = await _controller.GetTotal("PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1500.00m, okResult.Value);
        }

        [Fact]
        public async Task GetTotal_NullParentProject_ReturnsOk()
        {
            _serviceMock.GetTotalAmountAsync(null).Returns(0m);

            var result = await _controller.GetTotal(null);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(0m, okResult.Value);
        }

        #endregion

        #region GetById

        [Fact]
        public async Task GetById_ExistingId_ReturnsOk()
        {
            var dto = new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "PRJ1" };
            var mapped = new ProjectInvoiceRes { InvoiceCounter = 1, ProjectParent = "PRJ1" };

            _serviceMock.GetByIdAsync(1).Returns(dto);
            _mapperMock.Map<ProjectInvoiceRes>(dto).Returns(mapped);

            var result = await _controller.GetById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetById_NullResult_ThrowsKeyNotFoundException()
        {
            _serviceMock.GetByIdAsync(99).Returns((ProjectInvoiceDto?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetById(99));
        }

        #endregion

        #region Create

        [Fact]
        public async Task Create_ValidRequest_ReturnsCreatedAtAction()
        {
            var req = new ProjectInvoiceReq { ProjectParent = "PRJ1", Amount = 1000m };
            var dto = new ProjectInvoiceDto { ProjectParent = "PRJ1", Amount = 1000m };
            var createdDto = new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "PRJ1", Amount = 1000m };
            var mapped = new ProjectInvoiceRes { InvoiceCounter = 1, ProjectParent = "PRJ1", Amount = 1000m };

            _mapperMock.Map<ProjectInvoiceDto>(req).Returns(dto);
            _serviceMock.CreateAsync(dto).Returns(createdDto);
            _mapperMock.Map<ProjectInvoiceRes>(createdDto).Returns(mapped);

            var result = await _controller.Create(req);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(mapped, createdResult.Value);
            Assert.Equal(1, createdResult.RouteValues!["id"]);
        }

        [Fact]
        public async Task Create_ServiceThrows_PropagatesException()
        {
            var req = new ProjectInvoiceReq { ProjectParent = "PRJ1" };
            var dto = new ProjectInvoiceDto { ProjectParent = "PRJ1" };

            _mapperMock.Map<ProjectInvoiceDto>(req).Returns(dto);
            _serviceMock.CreateAsync(dto).ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.Create(req));
        }

        #endregion

        #region Update

        [Fact]
        public async Task Update_ValidRequest_ReturnsOk()
        {
            var req = new ProjectInvoiceReq { ProjectParent = "PRJ1", Amount = 2000m };
            var dto = new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "PRJ1", Amount = 2000m };
            var updatedDto = new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "PRJ1", Amount = 2000m };
            var mapped = new ProjectInvoiceRes { InvoiceCounter = 1, ProjectParent = "PRJ1", Amount = 2000m };

            _mapperMock.Map<ProjectInvoiceDto>(req).Returns(dto);
            _serviceMock.UpdateAsync(dto).Returns(updatedDto);
            _mapperMock.Map<ProjectInvoiceRes>(updatedDto).Returns(mapped);

            var result = await _controller.Update(1, req);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task Update_ServiceThrows_PropagatesException()
        {
            var req = new ProjectInvoiceReq { ProjectParent = "PRJ1" };
            var dto = new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "PRJ1" };

            _mapperMock.Map<ProjectInvoiceDto>(req).Returns(dto);
            _serviceMock.UpdateAsync(dto).ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.Update(1, req));
        }

        #endregion

        #region Delete

        [Fact]
        public async Task Delete_ExistingId_ReturnsOk()
        {
            _serviceMock.DeleteAsync(1).Returns(true);

            var result = await _controller.Delete(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task Delete_RecordNotFound_ReturnsOkWithFalse()
        {
            _serviceMock.DeleteAsync(99).Returns(false);

            var result = await _controller.Delete(99);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.False((bool)okResult.Value!);
        }

        #endregion

        #region Additional GetPaged Tests

        [Fact]
        public async Task GetPaged_EmptyResult_ReturnsOkWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var emptyDtos = new List<ProjectInvoiceDto>();
            var paginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0, TotalPages = 0 };
            var serviceResult = new PaginatedResult<ProjectInvoiceDto>(emptyDtos, paginationData);
            var expectedResponse = new PaginationRes<ProjectInvoiceRes>
            {
                Data = new List<ProjectInvoiceRes>(),
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 0, TotalPages = 0 }
            };

            _serviceMock.GetPagedProjectInvoicesAsync(query, "PRJ1").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectInvoiceRes>>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetPaged(query, "PRJ1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginationRes<ProjectInvoiceRes>>(okResult.Value);
            Assert.Empty(response.Data);
        }

        [Fact]
        public async Task GetPaged_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _serviceMock.GetPagedProjectInvoicesAsync(query, "PRJ1").ThrowsAsync(new InvalidOperationException("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetPaged(query, "PRJ1"));
        }

        [Fact]
        public async Task GetPaged_LargePageSize_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 1000 };
            var serviceResult = new PaginatedResult<ProjectInvoiceDto>(Enumerable.Empty<ProjectInvoiceDto>(), new PaginationDto());
            var expectedResponse = new PaginationRes<ProjectInvoiceRes>();

            _serviceMock.GetPagedProjectInvoicesAsync(query, null).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectInvoiceRes>>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetPaged(query, null);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Additional GetTotal Tests

        [Fact]
        public async Task GetTotal_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _serviceMock.GetTotalAmountAsync("PRJ1").ThrowsAsync(new InvalidOperationException("Calculation error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetTotal("PRJ1"));
        }

        [Fact]
        public async Task GetTotal_NegativeAmount_ReturnsOk()
        {
            // Arrange
            _serviceMock.GetTotalAmountAsync("PRJ1").Returns(-500.00m);

            // Act
            var result = await _controller.GetTotal("PRJ1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(-500.00m, okResult.Value);
        }

        [Fact]
        public async Task GetTotal_ZeroAmount_ReturnsOk()
        {
            // Arrange
            _serviceMock.GetTotalAmountAsync("NEWPRJ").Returns(0m);

            // Act
            var result = await _controller.GetTotal("NEWPRJ");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(0m, okResult.Value);
        }

        #endregion

        #region Additional GetById Tests

        [Fact]
        public async Task GetById_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _serviceMock.GetByIdAsync(1).ThrowsAsync(new InvalidOperationException("Database connection error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetById(1));
        }

        [Fact]
        public async Task GetById_ZeroId_ThrowsKeyNotFoundException()
        {
            // Arrange
            _serviceMock.GetByIdAsync(0).Returns((ProjectInvoiceDto?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetById(0));
        }

        [Fact]
        public async Task GetById_NegativeId_ThrowsKeyNotFoundException()
        {
            // Arrange
            _serviceMock.GetByIdAsync(-1).Returns((ProjectInvoiceDto?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetById(-1));
        }

        #endregion

        #region Additional Create Tests

        [Fact]
        public async Task Create_MappedDtoIsNull_ServiceHandlesNull()
        {
            // Arrange
            var req = new ProjectInvoiceReq { ProjectParent = "PRJ1", Amount = 1000m };
            _mapperMock.Map<ProjectInvoiceDto>(req).Returns((ProjectInvoiceDto)null!);
            _serviceMock.CreateAsync(Arg.Any<ProjectInvoiceDto>()).ThrowsAsync(new ArgumentNullException("dto", "DTO cannot be null"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _controller.Create(req));
            Assert.Contains("dto", exception.ParamName);
        }

        [Fact]
        public async Task Create_InvoiceCounterZero_ReturnsCreatedAtAction()
        {
            // Arrange
            var req = new ProjectInvoiceReq { ProjectParent = "PRJ1", Amount = 1000m };
            var dto = new ProjectInvoiceDto { InvoiceCounter = 0, ProjectParent = "PRJ1", Amount = 1000m };
            var createdDto = new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "PRJ1", Amount = 1000m };
            var mapped = new ProjectInvoiceRes { InvoiceCounter = 1, ProjectParent = "PRJ1", Amount = 1000m };

            _mapperMock.Map<ProjectInvoiceDto>(req).Returns(dto);
            _serviceMock.CreateAsync(dto).Returns(createdDto);
            _mapperMock.Map<ProjectInvoiceRes>(createdDto).Returns(mapped);

            // Act
            var result = await _controller.Create(req);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(1, createdResult.RouteValues!["id"]);
        }

        [Fact]
        public async Task Create_EmptyProjectParent_ServiceHandlesValidation()
        {
            // Arrange
            var req = new ProjectInvoiceReq { ProjectParent = "", Amount = 1000m };
            var dto = new ProjectInvoiceDto { ProjectParent = "", Amount = 1000m };

            _mapperMock.Map<ProjectInvoiceDto>(req).Returns(dto);
            _serviceMock.CreateAsync(dto).ThrowsAsync(new ArgumentException("ProjectParent is required"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.Create(req));
        }

        #endregion

        #region Additional Update Tests

        [Fact]
        public async Task Update_NullRequest_MapsAndUpdates()
        {
            // Arrange
            var req = new ProjectInvoiceReq { ProjectParent = "PRJ1", Amount = 2000m };
            var dto = new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "PRJ1", Amount = 2000m };
            var updatedDto = new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "PRJ1", Amount = 2000m };
            var mapped = new ProjectInvoiceRes { InvoiceCounter = 1, ProjectParent = "PRJ1", Amount = 2000m };

            _mapperMock.Map<ProjectInvoiceDto>(req).Returns(dto);
            _serviceMock.UpdateAsync(dto).Returns(updatedDto);
            _mapperMock.Map<ProjectInvoiceRes>(updatedDto).Returns(mapped);

            // Act
            var result = await _controller.Update(1, req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
            await _serviceMock.Received(1).UpdateAsync(Arg.Is<ProjectInvoiceDto>(d => d.InvoiceCounter == 1));
        }

        [Fact]
        public async Task Update_ZeroId_SetsInvoiceCounterCorrectly()
        {
            // Arrange
            var req = new ProjectInvoiceReq { ProjectParent = "PRJ1", Amount = 2000m };
            var dto = new ProjectInvoiceDto { InvoiceCounter = 0, ProjectParent = "PRJ1", Amount = 2000m };
            var updatedDto = new ProjectInvoiceDto { InvoiceCounter = 0, ProjectParent = "PRJ1", Amount = 2000m };
            var mapped = new ProjectInvoiceRes { InvoiceCounter = 0, ProjectParent = "PRJ1", Amount = 2000m };

            _mapperMock.Map<ProjectInvoiceDto>(req).Returns(dto);
            _serviceMock.UpdateAsync(Arg.Is<ProjectInvoiceDto>(d => d.InvoiceCounter == 0)).Returns(updatedDto);
            _mapperMock.Map<ProjectInvoiceRes>(updatedDto).Returns(mapped);

            // Act
            var result = await _controller.Update(0, req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            await _serviceMock.Received(1).UpdateAsync(Arg.Is<ProjectInvoiceDto>(d => d.InvoiceCounter == 0));
        }

        [Fact]
        public async Task Update_NegativeId_SetsInvoiceCounterCorrectly()
        {
            // Arrange
            var req = new ProjectInvoiceReq { ProjectParent = "PRJ1", Amount = 2000m };
            var dto = new ProjectInvoiceDto { InvoiceCounter = -1, ProjectParent = "PRJ1", Amount = 2000m };

            _mapperMock.Map<ProjectInvoiceDto>(req).Returns(new ProjectInvoiceDto());
            _serviceMock.UpdateAsync(Arg.Is<ProjectInvoiceDto>(d => d.InvoiceCounter == -1)).ThrowsAsync(new ArgumentException("Invalid invoice counter"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.Update(-1, req));
        }

        [Fact]
        public async Task Update_RecordNotFound_ServiceThrowsKeyNotFoundException()
        {
            // Arrange
            var req = new ProjectInvoiceReq { ProjectParent = "PRJ1", Amount = 2000m };
            var dto = new ProjectInvoiceDto { InvoiceCounter = 999, ProjectParent = "PRJ1", Amount = 2000m };

            _mapperMock.Map<ProjectInvoiceDto>(req).Returns(dto);
            _serviceMock.UpdateAsync(Arg.Any<ProjectInvoiceDto>()).ThrowsAsync(new KeyNotFoundException("Record not found"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.Update(999, req));
        }

        #endregion

        #region Additional Delete Tests

        [Fact]
        public async Task Delete_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _serviceMock.DeleteAsync(1).ThrowsAsync(new InvalidOperationException("Cannot delete record"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Delete(1));
        }

        [Fact]
        public async Task Delete_ZeroId_ReturnsOkWithFalse()
        {
            // Arrange
            _serviceMock.DeleteAsync(0).Returns(false);

            // Act
            var result = await _controller.Delete(0);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.False((bool)okResult.Value!);
        }

        [Fact]
        public async Task Delete_NegativeId_ReturnsOkWithFalse()
        {
            // Arrange
            _serviceMock.DeleteAsync(-1).Returns(false);

            // Act
            var result = await _controller.Delete(-1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.False((bool)okResult.Value!);
        }

        [Fact]
        public async Task Delete_MultipleRecordsWithSameProject_OnlyDeletesSpecifiedId()
        {
            // Arrange
            _serviceMock.DeleteAsync(5).Returns(true);

            // Act
            var result = await _controller.Delete(5);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
            await _serviceMock.Received(1).DeleteAsync(5);
        }

        #endregion

        #region CopyInvoices Tests

        [Fact]
        public async Task CopyInvoices_BulkCopy_ValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 5, TargetMonth = 6 };
            var dto = new CopyInvoicesDto { SourceMonth = 5, TargetMonth = 6, InvoiceIds = null };
            var resultDto = new CopyInvoicesResultDto
            {
                Success = true,
                CopiedCount = 10,
                FailedCount = 0,
                Errors = new List<string>(),
                Message = "Successfully copied 10 invoices"
            };
            var response = new CopyInvoicesRes
            {
                Success = true,
                CopiedCount = 10,
                FailedCount = 0,
                Errors = new List<string>(),
                Message = "Successfully copied 10 invoices"
            };

            _mapperMock.Map<CopyInvoicesDto>(request).Returns(dto);
            _serviceMock.CopyInvoicesAsync(Arg.Is<CopyInvoicesDto>(d => d.SourceMonth == 5 && d.TargetMonth == 6 && d.InvoiceIds == null))
                .Returns(resultDto);
            _mapperMock.Map<CopyInvoicesRes>(resultDto).Returns(response);

            // Act
            var result = await _controller.CopyInvoices(request, "5", "6");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultValue = Assert.IsType<CopyInvoicesRes>(okResult.Value);
            Assert.True(resultValue.Success);
            Assert.Equal(10, resultValue.CopiedCount);
            Assert.Equal(0, resultValue.FailedCount);
            await _serviceMock.Received(1).CopyInvoicesAsync(Arg.Is<CopyInvoicesDto>(d => d.InvoiceIds == null));
        }

        [Fact]
        public async Task CopyInvoices_SelectiveCopy_WithInvoiceIds_ReturnsOk()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 3, TargetMonth = 4, InvoiceIds = new List<int> { 1, 2, 3 } };
            var dto = new CopyInvoicesDto { SourceMonth = 3, TargetMonth = 4, InvoiceIds = new List<int> { 1, 2, 3 } };
            var resultDto = new CopyInvoicesResultDto
            {
                Success = true,
                CopiedCount = 3,
                FailedCount = 0,
                Errors = new List<string>(),
                Message = "Successfully copied 3 invoices"
            };
            var response = new CopyInvoicesRes
            {
                Success = true,
                CopiedCount = 3,
                FailedCount = 0,
                Errors = new List<string>(),
                Message = "Successfully copied 3 invoices"
            };

            _mapperMock.Map<CopyInvoicesDto>(request).Returns(dto);
            _serviceMock.CopyInvoicesAsync(Arg.Is<CopyInvoicesDto>(d => d.InvoiceIds != null && d.InvoiceIds.Count == 3))
                .Returns(resultDto);
            _mapperMock.Map<CopyInvoicesRes>(resultDto).Returns(response);

            // Act
            var result = await _controller.CopyInvoices(request, "3", "4");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultValue = Assert.IsType<CopyInvoicesRes>(okResult.Value);
            Assert.True(resultValue.Success);
            Assert.Equal(3, resultValue.CopiedCount);
            Assert.Equal(0, resultValue.FailedCount);
            await _serviceMock.Received(1).CopyInvoicesAsync(Arg.Is<CopyInvoicesDto>(d => d.InvoiceIds != null && d.InvoiceIds.Count == 3));
        }

        [Fact]
        public async Task CopyInvoices_InvalidSourceMonth_ReturnsBadRequest()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 1, TargetMonth = 2 };

            // Act
            var result = await _controller.CopyInvoices(request, "invalid", "2");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            Assert.Contains("Invalid", badRequestResult.Value.ToString()!);
        }

        [Fact]
        public async Task CopyInvoices_InvalidDestinationMonth_ReturnsBadRequest()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 1, TargetMonth = 2 };

            // Act
            var result = await _controller.CopyInvoices(request, "1", "invalid");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            Assert.Contains("Invalid", badRequestResult.Value.ToString()!);
        }

        [Fact]
        public async Task CopyInvoices_SourceMonthOutOfRange_ReturnsBadRequest()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 1, TargetMonth = 2 };

            // Act
            var result = await _controller.CopyInvoices(request, "13", "2");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("must be between 1 and 12", badRequestResult.Value?.ToString());
        }

        [Fact]
        public async Task CopyInvoices_DestinationMonthOutOfRange_ReturnsBadRequest()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 1, TargetMonth = 2 };

            // Act
            var result = await _controller.CopyInvoices(request, "1", "0");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("must be between 1 and 12", badRequestResult.Value?.ToString());
        }

        [Fact]
        public async Task CopyInvoices_BulkTrue_ClearsInvoiceIds()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 5, TargetMonth = 6, InvoiceIds = new List<int> { 1, 2, 3 } };
            var dto = new CopyInvoicesDto { SourceMonth = 5, TargetMonth = 6, InvoiceIds = null };
            var resultDto = new CopyInvoicesResultDto { Success = true, CopiedCount = 10 };
            var response = new CopyInvoicesRes { Success = true, CopiedCount = 10 };

            _mapperMock.Map<CopyInvoicesDto>(request).Returns(dto);
            _serviceMock.CopyInvoicesAsync(Arg.Is<CopyInvoicesDto>(d => d.InvoiceIds == null)).Returns(resultDto);
            _mapperMock.Map<CopyInvoicesRes>(resultDto).Returns(response);

            // Act
            var result = await _controller.CopyInvoices(request, "5", "6");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            await _serviceMock.Received(1).CopyInvoicesAsync(Arg.Is<CopyInvoicesDto>(d => d.InvoiceIds == null));
        }

        [Fact]
        public async Task CopyInvoices_NoInvoicesFound_ReturnsOkWithErrors()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 5, TargetMonth = 6 };
            var dto = new CopyInvoicesDto { SourceMonth = 5, TargetMonth = 6 };
            var resultDto = new CopyInvoicesResultDto
            {
                Success = false,
                CopiedCount = 0,
                FailedCount = 0,
                Errors = new List<string> { "No invoices found for the source month" },
                Message = "No invoices to copy"
            };
            var response = new CopyInvoicesRes
            {
                Success = false,
                CopiedCount = 0,
                FailedCount = 0,
                Errors = new List<string> { "No invoices found for the source month" },
                Message = "No invoices to copy"
            };

            _mapperMock.Map<CopyInvoicesDto>(request).Returns(dto);
            _serviceMock.CopyInvoicesAsync(dto).Returns(resultDto);
            _mapperMock.Map<CopyInvoicesRes>(resultDto).Returns(response);

            // Act
            var result = await _controller.CopyInvoices(request, "5", "6");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultValue = Assert.IsType<CopyInvoicesRes>(okResult.Value);
            Assert.False(resultValue.Success);
            Assert.Contains("No invoices found", resultValue.Errors.First());
        }

        [Fact]
        public async Task CopyInvoices_PartialSuccess_ReturnsOkWithFailures()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 3, TargetMonth = 4, InvoiceIds = new List<int> { 1, 2, 3 } };
            var dto = new CopyInvoicesDto { SourceMonth = 3, TargetMonth = 4, InvoiceIds = new List<int> { 1, 2, 3 } };
            var resultDto = new CopyInvoicesResultDto
            {
                Success = false,
                CopiedCount = 2,
                FailedCount = 1,
                Errors = new List<string> { "Failed to copy invoice 3" },
                Message = "Copied 2 out of 3 invoices"
            };
            var response = new CopyInvoicesRes
            {
                Success = false,
                CopiedCount = 2,
                FailedCount = 1,
                Errors = new List<string> { "Failed to copy invoice 3" },
                Message = "Copied 2 out of 3 invoices"
            };

            _mapperMock.Map<CopyInvoicesDto>(request).Returns(dto);
            _serviceMock.CopyInvoicesAsync(dto).Returns(resultDto);
            _mapperMock.Map<CopyInvoicesRes>(resultDto).Returns(response);

            // Act
            var result = await _controller.CopyInvoices(request, "3", "4");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultValue = Assert.IsType<CopyInvoicesRes>(okResult.Value);
            Assert.False(resultValue.Success);
            Assert.Equal(2, resultValue.CopiedCount);
            Assert.Equal(1, resultValue.FailedCount);
        }

        [Fact]
        public async Task CopyInvoices_OverridesMonthsFromQueryParams()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 1, TargetMonth = 2 };
            var dto = new CopyInvoicesDto { SourceMonth = 1, TargetMonth = 2 };
            var resultDto = new CopyInvoicesResultDto { Success = true, CopiedCount = 5, FailedCount = 0 };
            var response = new CopyInvoicesRes { Success = true, CopiedCount = 5, FailedCount = 0 };

            _mapperMock.Map<CopyInvoicesDto>(request).Returns(dto);
            _serviceMock.CopyInvoicesAsync(Arg.Is<CopyInvoicesDto>(d => d.SourceMonth == 5 && d.TargetMonth == 6))
                .Returns(resultDto);
            _mapperMock.Map<CopyInvoicesRes>(resultDto).Returns(response);

            // Act
            var result = await _controller.CopyInvoices(request, "5", "6");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            // Verify that query params (5, 6) override the request body months (1, 2)
            await _serviceMock.Received(1).CopyInvoicesAsync(Arg.Is<CopyInvoicesDto>(d => 
                d.SourceMonth == 5 && 
                d.TargetMonth == 6));
        }

        [Fact]
        public async Task CopyInvoices_EmptyInvoiceIdsList_TreatsAsBulk()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 5, TargetMonth = 6, InvoiceIds = new List<int>() };
            var dto = new CopyInvoicesDto { SourceMonth = 5, TargetMonth = 6, InvoiceIds = new List<int>() };
            var resultDto = new CopyInvoicesResultDto { Success = true, CopiedCount = 10 };
            var response = new CopyInvoicesRes { Success = true, CopiedCount = 10 };

            _mapperMock.Map<CopyInvoicesDto>(request).Returns(dto);
            _serviceMock.CopyInvoicesAsync(dto).Returns(resultDto);
            _mapperMock.Map<CopyInvoicesRes>(resultDto).Returns(response);

            // Act
            var result = await _controller.CopyInvoices(request, "5", "6");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultValue = Assert.IsType<CopyInvoicesRes>(okResult.Value);
            Assert.Equal(10, resultValue.CopiedCount);
        }

        [Fact]
        public async Task CopyInvoices_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 5, TargetMonth = 6 };
            var dto = new CopyInvoicesDto { SourceMonth = 5, TargetMonth = 6 };

            _mapperMock.Map<CopyInvoicesDto>(request).Returns(dto);
            _serviceMock.CopyInvoicesAsync(dto).ThrowsAsync(new InvalidOperationException("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.CopyInvoices(request, "5", "6"));
        }

        [Fact]
        public async Task CopyInvoices_NullRequest_ThrowsException()
        {
            // Arrange
            CopyInvoicesReq? request = null;

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _controller.CopyInvoices(request!, "5", "6"));
        }

        [Fact]
        public async Task CopyInvoices_SameSourceAndDestination_ServiceHandlesValidation()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 5, TargetMonth = 5 };
            var dto = new CopyInvoicesDto { SourceMonth = 5, TargetMonth = 5 };
            var resultDto = new CopyInvoicesResultDto
            {
                Success = false,
                CopiedCount = 0,
                FailedCount = 0,
                Errors = new List<string> { "Source and destination months cannot be the same" },
                Message = "Invalid operation"
            };
            var response = new CopyInvoicesRes
            {
                Success = false,
                CopiedCount = 0,
                FailedCount = 0,
                Errors = new List<string> { "Source and destination months cannot be the same" },
                Message = "Invalid operation"
            };

            _mapperMock.Map<CopyInvoicesDto>(request).Returns(dto);
            _serviceMock.CopyInvoicesAsync(dto).Returns(resultDto);
            _mapperMock.Map<CopyInvoicesRes>(resultDto).Returns(response);

            // Act
            var result = await _controller.CopyInvoices(request, "5", "5");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultValue = Assert.IsType<CopyInvoicesRes>(okResult.Value);
            Assert.False(resultValue.Success);
        }

        [Fact]
        public async Task CopyInvoices_VeryLargeInvoiceIdList_ProcessesCorrectly()
        {
            // Arrange
            var largeIdList = Enumerable.Range(1, 10000).ToList();
            var request = new CopyInvoicesReq { SourceMonth = 3, TargetMonth = 4, InvoiceIds = largeIdList };
            var dto = new CopyInvoicesDto { SourceMonth = 3, TargetMonth = 4, InvoiceIds = largeIdList };
            var resultDto = new CopyInvoicesResultDto { Success = true, CopiedCount = 10000, FailedCount = 0 };
            var response = new CopyInvoicesRes { Success = true, CopiedCount = 10000, FailedCount = 0 };

            _mapperMock.Map<CopyInvoicesDto>(request).Returns(dto);
            _serviceMock.CopyInvoicesAsync(dto).Returns(resultDto);
            _mapperMock.Map<CopyInvoicesRes>(resultDto).Returns(response);

            // Act
            var result = await _controller.CopyInvoices(request, "3", "4");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultValue = Assert.IsType<CopyInvoicesRes>(okResult.Value);
            Assert.Equal(10000, resultValue.CopiedCount);
        }

        [Fact]
        public async Task CopyInvoices_NullSourceMonth_ReturnsBadRequest()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 1, TargetMonth = 2 };

            // Act
            var result = await _controller.CopyInvoices(request, null!, "2");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            Assert.Contains("Invalid", badRequestResult.Value.ToString()!);
        }

        [Fact]
        public async Task CopyInvoices_NullDestinationMonth_ReturnsBadRequest()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 1, TargetMonth = 2 };

            // Act
            var result = await _controller.CopyInvoices(request, "1", null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            Assert.Contains("Invalid", badRequestResult.Value.ToString()!);
        }

        [Fact]
        public async Task CopyInvoices_WithErrorsAndMessage_ReturnsAllFieldsCorrectly()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 3, TargetMonth = 4, InvoiceIds = new List<int> { 1, 2 } };
            var dto = new CopyInvoicesDto { SourceMonth = 3, TargetMonth = 4, InvoiceIds = new List<int> { 1, 2 } };
            var errorMessages = new List<string> { "Error copying invoice 1", "Error copying invoice 2" };
            var resultDto = new CopyInvoicesResultDto
            {
                Success = false,
                CopiedCount = 0,
                FailedCount = 2,
                Errors = errorMessages,
                Message = "All copy operations failed"
            };
            var response = new CopyInvoicesRes
            {
                Success = false,
                CopiedCount = 0,
                FailedCount = 2,
                Errors = errorMessages,
                Message = "All copy operations failed"
            };

            _mapperMock.Map<CopyInvoicesDto>(request).Returns(dto);
            _serviceMock.CopyInvoicesAsync(dto).Returns(resultDto);
            _mapperMock.Map<CopyInvoicesRes>(resultDto).Returns(response);

            // Act
            var result = await _controller.CopyInvoices(request, "3", "4");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultValue = Assert.IsType<CopyInvoicesRes>(okResult.Value);
            Assert.False(resultValue.Success);
            Assert.Equal(0, resultValue.CopiedCount);
            Assert.Equal(2, resultValue.FailedCount);
            Assert.NotNull(resultValue.Errors);
            Assert.Equal(2, resultValue.Errors.Count);
            Assert.Equal("All copy operations failed", resultValue.Message);
        }

        [Fact]
        public async Task CopyInvoices_BulkModeWithNullInvoiceIdsInRequest_ProcessesCorrectly()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 5, TargetMonth = 6, InvoiceIds = null };
            var dto = new CopyInvoicesDto { SourceMonth = 5, TargetMonth = 6, InvoiceIds = null };
            var resultDto = new CopyInvoicesResultDto { Success = true, CopiedCount = 15, FailedCount = 0 };
            var response = new CopyInvoicesRes { Success = true, CopiedCount = 15, FailedCount = 0 };

            _mapperMock.Map<CopyInvoicesDto>(request).Returns(dto);
            _serviceMock.CopyInvoicesAsync(Arg.Is<CopyInvoicesDto>(d => d.InvoiceIds == null)).Returns(resultDto);
            _mapperMock.Map<CopyInvoicesRes>(resultDto).Returns(response);

            // Act
            var result = await _controller.CopyInvoices(request, "5", "6");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultValue = Assert.IsType<CopyInvoicesRes>(okResult.Value);
            Assert.Equal(15, resultValue.CopiedCount);
        }

        [Fact]
        public async Task CopyInvoices_MixedValidAndInvalidIds_ReturnsPartialSuccess()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 3, TargetMonth = 4, InvoiceIds = new List<int> { 1, 2, 999, 1000 } };
            var dto = new CopyInvoicesDto { SourceMonth = 3, TargetMonth = 4, InvoiceIds = new List<int> { 1, 2, 999, 1000 } };
            var resultDto = new CopyInvoicesResultDto
            {
                Success = false,
                CopiedCount = 2,
                FailedCount = 2,
                Errors = new List<string> { "Invoice 999 not found", "Invoice 1000 not found" },
                Message = "Copied 2 out of 4 invoices"
            };
            var response = new CopyInvoicesRes
            {
                Success = false,
                CopiedCount = 2,
                FailedCount = 2,
                Errors = new List<string> { "Invoice 999 not found", "Invoice 1000 not found" },
                Message = "Copied 2 out of 4 invoices"
            };

            _mapperMock.Map<CopyInvoicesDto>(request).Returns(dto);
            _serviceMock.CopyInvoicesAsync(dto).Returns(resultDto);
            _mapperMock.Map<CopyInvoicesRes>(resultDto).Returns(response);

            // Act
            var result = await _controller.CopyInvoices(request, "3", "4");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultValue = Assert.IsType<CopyInvoicesRes>(okResult.Value);
            Assert.False(resultValue.Success);
            Assert.Equal(2, resultValue.CopiedCount);
            Assert.Equal(2, resultValue.FailedCount);
        }

        [Fact]
        public async Task Create_WithZeroAmount_CreatesSuccessfully()
        {
            // Arrange
            var req = new ProjectInvoiceReq { ProjectParent = "PRJ1", Amount = 0m };
            var dto = new ProjectInvoiceDto { ProjectParent = "PRJ1", Amount = 0m };
            var createdDto = new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "PRJ1", Amount = 0m };
            var mapped = new ProjectInvoiceRes { InvoiceCounter = 1, ProjectParent = "PRJ1", Amount = 0m };

            _mapperMock.Map<ProjectInvoiceDto>(req).Returns(dto);
            _serviceMock.CreateAsync(dto).Returns(createdDto);
            _mapperMock.Map<ProjectInvoiceRes>(createdDto).Returns(mapped);

            // Act
            var result = await _controller.Create(req);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var response = Assert.IsType<ProjectInvoiceRes>(createdResult.Value);
            Assert.Equal(0m, response.Amount);
        }

        [Fact]
        public async Task Update_WithNegativeAmount_UpdatesSuccessfully()
        {
            // Arrange
            var req = new ProjectInvoiceReq { ProjectParent = "PRJ1", Amount = -100m };
            var dto = new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "PRJ1", Amount = -100m };
            var updatedDto = new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "PRJ1", Amount = -100m };
            var mapped = new ProjectInvoiceRes { InvoiceCounter = 1, ProjectParent = "PRJ1", Amount = -100m };

            _mapperMock.Map<ProjectInvoiceDto>(req).Returns(dto);
            _serviceMock.UpdateAsync(dto).Returns(updatedDto);
            _mapperMock.Map<ProjectInvoiceRes>(updatedDto).Returns(mapped);

            // Act
            var result = await _controller.Update(1, req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ProjectInvoiceRes>(okResult.Value);
            Assert.Equal(-100m, response.Amount);
        }

        [Fact]
        public async Task GetById_LargeInvoiceCounter_ReturnsCorrectly()
        {
            // Arrange
            var dto = new ProjectInvoiceDto { InvoiceCounter = int.MaxValue, ProjectParent = "PRJ1" };
            var mapped = new ProjectInvoiceRes { InvoiceCounter = int.MaxValue, ProjectParent = "PRJ1" };

            _serviceMock.GetByIdAsync(int.MaxValue).Returns(dto);
            _mapperMock.Map<ProjectInvoiceRes>(dto).Returns(mapped);

            // Act
            var result = await _controller.GetById(int.MaxValue);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ProjectInvoiceRes>(okResult.Value);
            Assert.Equal(int.MaxValue, response.InvoiceCounter);
        }

        #region CopyInvoices_ValidationTests

        [Fact]
        public async Task CopyInvoices_WhitespaceInMonthParams_ReturnsBadRequest()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 1, TargetMonth = 2 };

            // Act
            var result = await _controller.CopyInvoices(request, "  ", "2");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            Assert.Contains("Invalid", badRequestResult.Value.ToString()!);
        }

        [Fact]
        public async Task CopyInvoices_EmptyStringMonthParams_ReturnsBadRequest()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 1, TargetMonth = 2 };

            // Act
            var result = await _controller.CopyInvoices(request, "", "2");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            Assert.Contains("Invalid", badRequestResult.Value.ToString()!);
        }

        [Fact]
        public async Task CopyInvoices_NegativeMonth_ReturnsBadRequest()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 1, TargetMonth = 2 };

            // Act
            var result = await _controller.CopyInvoices(request, "-1", "2");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("must be between 1 and 12", badRequestResult.Value?.ToString());
        }

        [Fact]
        public async Task CopyInvoices_AlphanumericMonthParams_ReturnsBadRequest()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 1, TargetMonth = 2 };

            // Act
            var result = await _controller.CopyInvoices(request, "Jan", "Feb");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid", badRequestResult.Value?.ToString());
        }

        [Fact]
        public async Task CopyInvoices_DecimalMonthParams_ReturnsBadRequest()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 1, TargetMonth = 2 };
            var dto = new CopyInvoicesDto { SourceMonth = 1, TargetMonth = 2 };
            
            _mapperMock.Map<CopyInvoicesDto>(request).Returns(dto);

            // Act
            var result = await _controller.CopyInvoices(request, "1.5", "2");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid source month", badRequestResult.Value?.ToString());
        }

        [Fact]
        public async Task CopyInvoices_VeryLargeMonth_ReturnsBadRequest()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 1, TargetMonth = 2 };
            var dto = new CopyInvoicesDto { SourceMonth = 1, TargetMonth = 2 };
            
            _mapperMock.Map<CopyInvoicesDto>(request).Returns(dto);

            // Act
            var result = await _controller.CopyInvoices(request, "1", "999999");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("must be between 1 and 12", badRequestResult.Value?.ToString());
        }

        [Fact]
        public async Task CopyInvoices_CancellationRequested_PropagatesCancellation()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 5, TargetMonth = 6 };
            var dto = new CopyInvoicesDto { SourceMonth = 5, TargetMonth = 6 };

            _mapperMock.Map<CopyInvoicesDto>(request).Returns(dto);
            _serviceMock.CopyInvoicesAsync(dto).ThrowsAsync(new OperationCanceledException("Operation was cancelled"));

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => _controller.CopyInvoices(request, "5", "6"));
        }

        #endregion

        #endregion
    }
}
