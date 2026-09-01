using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Api.Controllers;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Core.Interfaces;
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
        private readonly ICurrentUserContext _currentUserContextMock;
        private readonly ProjectInvoiceController _controller;

        public ProjectInvoiceControllerTests()
        {
            _serviceMock = Substitute.For<IProjectInvoiceService>();
            _mapperMock = Substitute.For<IMapper>();
            _currentUserContextMock = Substitute.For<ICurrentUserContext>();
            _currentUserContextMock.UserId.Returns("test-user");
            _controller = new ProjectInvoiceController(_serviceMock, _mapperMock, _currentUserContextMock);
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

        #region GetMonthlyInvoicesSummary

        [Fact]
        public async Task GetMonthlyInvoicesSummary_ValidQuery_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dto = new MonthlyInvoicesPivotDto();
            var mapped = new MonthlyInvoicesPivotRes();

            _serviceMock.GetMonthlyInvoicesSummaryAsync(query).Returns(dto);
            _mapperMock.Map<MonthlyInvoicesPivotRes>(dto).Returns(mapped);

            // Act
            var result = await _controller.GetMonthlyInvoicesSummary(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummary_ServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>();
            _serviceMock.GetMonthlyInvoicesSummaryAsync(query).ThrowsAsync(new InvalidOperationException("Error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetMonthlyInvoicesSummary(query));
        }

        #endregion

        #region GetFailedInvoiceImport

        [Fact]
        public async Task GetFailedInvoiceImport_ValidQuery_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<InvoiceImportRowDto>(
                new List<InvoiceImportRowDto> { new() { Id = 1 } }, new PaginationDto());
            var mapped = new PaginationRes<InvoiceImportRowRes>
            {
                Data = new List<InvoiceImportRowRes> { new() { Id = 1 } }
            };

            _serviceMock.GetFailedInvoiceImportAsync(query, "test-user").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<InvoiceImportRowRes>>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetFailedInvoiceImport(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetFailedInvoiceImport_UsesCurrentUserContext()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var serviceResult = new PaginatedResult<InvoiceImportRowDto>(
                Enumerable.Empty<InvoiceImportRowDto>(), new PaginationDto());
            _serviceMock.GetFailedInvoiceImportAsync(query, "test-user").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<InvoiceImportRowRes>>(serviceResult).Returns(new PaginationRes<InvoiceImportRowRes>());

            // Act
            await _controller.GetFailedInvoiceImport(query);

            // Assert
            await _serviceMock.Received(1).GetFailedInvoiceImportAsync(query, "test-user");
        }

        [Fact]
        public async Task GetFailedInvoiceImport_ServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>();
            _serviceMock.GetFailedInvoiceImportAsync(query, "test-user")
                .ThrowsAsync(new InvalidOperationException("Error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetFailedInvoiceImport(query));
        }

        #endregion

        #region GetFailedInvoiceImportById

        [Fact]
        public async Task GetFailedInvoiceImportById_ExistingId_ReturnsOk()
        {
            // Arrange
            var dto = new InvoiceImportRowDto { Id = 5, ProjectParent = "PRJ1" };
            var mapped = new InvoiceImportRowRes { Id = 5, ProjectParent = "PRJ1" };

            _serviceMock.GetFailedInvoiceImportByIdAsync(5, "test-user").Returns(dto);
            _mapperMock.Map<InvoiceImportRowRes>(dto).Returns(mapped);

            // Act
            var result = await _controller.GetFailedInvoiceImportById(5);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetFailedInvoiceImportById_NullResult_ThrowsKeyNotFoundException()
        {
            // Arrange
            _serviceMock.GetFailedInvoiceImportByIdAsync(99, "test-user").Returns((InvoiceImportRowDto?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetFailedInvoiceImportById(99));
        }

        [Fact]
        public async Task GetFailedInvoiceImportById_UsesCurrentUserContext()
        {
            // Arrange
            var dto = new InvoiceImportRowDto { Id = 1 };
            _serviceMock.GetFailedInvoiceImportByIdAsync(1, "test-user").Returns(dto);
            _mapperMock.Map<InvoiceImportRowRes>(dto).Returns(new InvoiceImportRowRes());

            // Act
            await _controller.GetFailedInvoiceImportById(1);

            // Assert
            await _serviceMock.Received(1).GetFailedInvoiceImportByIdAsync(1, "test-user");
        }

        [Fact]
        public async Task GetFailedInvoiceImportById_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetFailedInvoiceImportByIdAsync(1, "test-user")
                .ThrowsAsync(new InvalidOperationException("Error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetFailedInvoiceImportById(1));
        }

        #endregion

        #region SaveFailedInvoiceImport

        [Fact]
        public async Task SaveFailedInvoiceImport_ValidRequest_ReturnsOkWithResult()
        {
            // Arrange
            var req = new InvoiceImportRowReq { ProjectParent = "PRJ1" };
            var dto = new InvoiceImportRowDto { ProjectParent = "PRJ1" };

            _mapperMock.Map<InvoiceImportRowDto>(req).Returns(dto);
            _serviceMock.SaveFailedInvoiceImportAsync(5, dto, "test-user").Returns(true);

            // Act
            var result = await _controller.SaveFailedInvoiceImport(5, req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task SaveFailedInvoiceImport_NotMoved_ReturnsOkWithFalse()
        {
            // Arrange
            var req = new InvoiceImportRowReq { ProjectParent = "PRJ1" };
            var dto = new InvoiceImportRowDto { ProjectParent = "PRJ1" };

            _mapperMock.Map<InvoiceImportRowDto>(req).Returns(dto);
            _serviceMock.SaveFailedInvoiceImportAsync(5, dto, "test-user").Returns(false);

            // Act
            var result = await _controller.SaveFailedInvoiceImport(5, req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.False((bool)okResult.Value!);
        }

        [Fact]
        public async Task SaveFailedInvoiceImport_UsesCurrentUserContext()
        {
            // Arrange
            var req = new InvoiceImportRowReq();
            var dto = new InvoiceImportRowDto();
            _mapperMock.Map<InvoiceImportRowDto>(req).Returns(dto);
            _serviceMock.SaveFailedInvoiceImportAsync(1, dto, "test-user").Returns(true);

            // Act
            await _controller.SaveFailedInvoiceImport(1, req);

            // Assert
            await _serviceMock.Received(1).SaveFailedInvoiceImportAsync(1, dto, "test-user");
        }

        [Fact]
        public async Task SaveFailedInvoiceImport_ServiceThrows_PropagatesException()
        {
            // Arrange
            var req = new InvoiceImportRowReq();
            _mapperMock.Map<InvoiceImportRowDto>(req).Returns(new InvoiceImportRowDto());
            _serviceMock.SaveFailedInvoiceImportAsync(Arg.Any<int>(), Arg.Any<InvoiceImportRowDto>(), "test-user")
                .ThrowsAsync(new InvalidOperationException("Error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.SaveFailedInvoiceImport(1, req));
        }

        #endregion

        #region DeleteFailedInvoiceImportById

        [Fact]
        public async Task DeleteFailedInvoiceImportById_ExistingId_ReturnsOkWithTrue()
        {
            // Arrange
            _serviceMock.DeleteFailedInvoiceImportByIdAsync(5, "test-user").Returns(true);

            // Act
            var result = await _controller.DeleteFailedInvoiceImportById(5);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task DeleteFailedInvoiceImportById_NonExistingId_ReturnsOkWithFalse()
        {
            // Arrange
            _serviceMock.DeleteFailedInvoiceImportByIdAsync(99, "test-user").Returns(false);

            // Act
            var result = await _controller.DeleteFailedInvoiceImportById(99);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.False((bool)okResult.Value!);
        }

        [Fact]
        public async Task DeleteFailedInvoiceImportById_UsesCurrentUserContext()
        {
            // Arrange
            _serviceMock.DeleteFailedInvoiceImportByIdAsync(1, "test-user").Returns(true);

            // Act
            await _controller.DeleteFailedInvoiceImportById(1);

            // Assert
            await _serviceMock.Received(1).DeleteFailedInvoiceImportByIdAsync(1, "test-user");
        }

        [Fact]
        public async Task DeleteFailedInvoiceImportById_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.DeleteFailedInvoiceImportByIdAsync(1, "test-user")
                .ThrowsAsync(new InvalidOperationException("Error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.DeleteFailedInvoiceImportById(1));
        }

        #endregion

        #region DeleteFailedInvoiceImportByUser

        [Fact]
        public async Task DeleteFailedInvoiceImportByUser_RecordsDeleted_ReturnsOkWithTrue()
        {
            // Arrange
            _serviceMock.DeleteFailedInvoiceImportByUserAsync("test-user").Returns(5);

            // Act
            var result = await _controller.DeleteFailedInvoiceImportByUser();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task DeleteFailedInvoiceImportByUser_NoRecordsDeleted_ReturnsOkWithFalse()
        {
            // Arrange
            _serviceMock.DeleteFailedInvoiceImportByUserAsync("test-user").Returns(0);

            // Act
            var result = await _controller.DeleteFailedInvoiceImportByUser();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.False((bool)okResult.Value!);
        }

        [Fact]
        public async Task DeleteFailedInvoiceImportByUser_UsesCurrentUserContext()
        {
            // Arrange
            _serviceMock.DeleteFailedInvoiceImportByUserAsync("test-user").Returns(1);

            // Act
            await _controller.DeleteFailedInvoiceImportByUser();

            // Assert
            await _serviceMock.Received(1).DeleteFailedInvoiceImportByUserAsync("test-user");
        }

        [Fact]
        public async Task DeleteFailedInvoiceImportByUser_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.DeleteFailedInvoiceImportByUserAsync("test-user")
                .ThrowsAsync(new InvalidOperationException("Error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.DeleteFailedInvoiceImportByUser());
        }

        #endregion

        #region ImportInvoice

        [Fact]
        public async Task ImportInvoice_ValidRequest_ReturnsOkWithMappedResult()
        {
            // Arrange
            var req = new InvoiceImportReq { FileName = "test.xlsx" };
            var dto = new InvoiceImportDto { FileName = "test.xlsx" };
            var resultDto = new InvoiceImportResultDto { PassedCount = 10, FailedCount = 2, Message = "Imported" };
            var mapped = new InvoiceImportRes { PassedCount = 10, FailedCount = 2, Message = "Imported" };

            _mapperMock.Map<InvoiceImportDto>(req).Returns(dto);
            _serviceMock.ImportInvoiceAsync(dto, "test-user").Returns(resultDto);
            _mapperMock.Map<InvoiceImportRes>(resultDto).Returns(mapped);

            // Act
            var result = await _controller.ImportInvoice(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task ImportInvoice_UsesCurrentUserContext()
        {
            // Arrange
            var req = new InvoiceImportReq();
            var dto = new InvoiceImportDto();
            var resultDto = new InvoiceImportResultDto();
            _mapperMock.Map<InvoiceImportDto>(req).Returns(dto);
            _serviceMock.ImportInvoiceAsync(dto, "test-user").Returns(resultDto);
            _mapperMock.Map<InvoiceImportRes>(resultDto).Returns(new InvoiceImportRes());

            // Act
            await _controller.ImportInvoice(req);

            // Assert
            await _serviceMock.Received(1).ImportInvoiceAsync(dto, "test-user");
        }

        [Fact]
        public async Task ImportInvoice_ServiceThrows_PropagatesException()
        {
            // Arrange
            var req = new InvoiceImportReq();
            _mapperMock.Map<InvoiceImportDto>(req).Returns(new InvoiceImportDto());
            _serviceMock.ImportInvoiceAsync(Arg.Any<InvoiceImportDto>(), "test-user")
                .ThrowsAsync(new InvalidOperationException("Import error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.ImportInvoice(req));
        }

        [Fact]
        public async Task ImportInvoice_MapsRequestCorrectly()
        {
            // Arrange
            var req = new InvoiceImportReq { FileName = "data.xlsx" };
            var dto = new InvoiceImportDto { FileName = "data.xlsx" };
            var resultDto = new InvoiceImportResultDto { PassedCount = 0, FailedCount = 5 };
            var mapped = new InvoiceImportRes { PassedCount = 0, FailedCount = 5 };

            _mapperMock.Map<InvoiceImportDto>(req).Returns(dto);
            _serviceMock.ImportInvoiceAsync(dto, "test-user").Returns(resultDto);
            _mapperMock.Map<InvoiceImportRes>(resultDto).Returns(mapped);

            // Act
            var result = await _controller.ImportInvoice(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<InvoiceImportRes>(okResult.Value);
            Assert.Equal(0, response.PassedCount);
            Assert.Equal(5, response.FailedCount);
        }

        #endregion
    }
}
