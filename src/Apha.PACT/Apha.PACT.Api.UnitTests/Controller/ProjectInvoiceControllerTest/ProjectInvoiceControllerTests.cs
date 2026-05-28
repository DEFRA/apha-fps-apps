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
using Xunit;

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

        #region GetPagedProjectInvoicesByMonth

        [Fact]
        public async Task GetPagedProjectInvoicesByMonth_ValidMonthAndQuery_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var month = 3;
            var dtos = new List<ProjectInvoiceDto>
            {
                new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3 },
                new ProjectInvoiceDto { InvoiceCounter = 2, ProjectParent = "PRJ2", Month = 3 }
            };
            var paginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2, TotalPages = 1 };
            var serviceResult = new PaginatedResult<ProjectInvoiceDto>(dtos, paginationData);
            var expectedResponse = new PaginationRes<ProjectInvoiceRes>
            {
                Data = new List<ProjectInvoiceRes>
                {
                    new ProjectInvoiceRes { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3 },
                    new ProjectInvoiceRes { InvoiceCounter = 2, ProjectParent = "PRJ2", Month = 3 }
                },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 2, TotalPages = 1 }
            };

            _serviceMock.GetPagedProjectInvoicesByMonthAsync(query, month).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectInvoiceRes>>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetPagedProjectInvoicesByMonth(query, month);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginationRes<ProjectInvoiceRes>>(okResult.Value);
            Assert.Equal(2, response.Data.Count());
            Assert.All(response.Data, item => Assert.Equal(3, item.Month));
            await _serviceMock.Received(1).GetPagedProjectInvoicesByMonthAsync(query, month);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonth_NullMonth_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            int? month = null;
            var serviceResult = new PaginatedResult<ProjectInvoiceDto>(Enumerable.Empty<ProjectInvoiceDto>(), new PaginationDto());
            var expectedResponse = new PaginationRes<ProjectInvoiceRes>();

            _serviceMock.GetPagedProjectInvoicesByMonthAsync(query, null).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectInvoiceRes>>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetPagedProjectInvoicesByMonth(query, month);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            await _serviceMock.Received(1).GetPagedProjectInvoicesByMonthAsync(query, null);
        }


        [Theory]
        [InlineData(1)]
        [InlineData(6)]
        [InlineData(12)]
        public async Task GetPagedProjectInvoicesByMonth_ValidMonthBoundaries_ReturnsOk(int month)
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<ProjectInvoiceDto>(Enumerable.Empty<ProjectInvoiceDto>(), new PaginationDto());
            var expectedResponse = new PaginationRes<ProjectInvoiceRes>();

            _serviceMock.GetPagedProjectInvoicesByMonthAsync(query, month).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectInvoiceRes>>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetPagedProjectInvoicesByMonth(query, month);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _serviceMock.Received(1).GetPagedProjectInvoicesByMonthAsync(query, month);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonth_EmptyResult_ReturnsOkWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var month = 12;
            var emptyDtos = new List<ProjectInvoiceDto>();
            var paginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0, TotalPages = 0 };
            var serviceResult = new PaginatedResult<ProjectInvoiceDto>(emptyDtos, paginationData);
            var expectedResponse = new PaginationRes<ProjectInvoiceRes>
            {
                Data = new List<ProjectInvoiceRes>(),
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 0, TotalPages = 0 }
            };

            _serviceMock.GetPagedProjectInvoicesByMonthAsync(query, month).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectInvoiceRes>>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetPagedProjectInvoicesByMonth(query, month);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginationRes<ProjectInvoiceRes>>(okResult.Value);
            Assert.Empty(response.Data);
            Assert.Equal(0, response.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonth_ServiceThrowsArgumentException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var month = 3;

            _serviceMock.GetPagedProjectInvoicesByMonthAsync(query, month)
                .ThrowsAsync(new ArgumentException("Invalid month parameter"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetPagedProjectInvoicesByMonth(query, month));
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonth_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 5 };
            var month = 6;
            var dtos = Enumerable.Range(6, 5).Select(i => new ProjectInvoiceDto
            {
                InvoiceCounter = i,
                ProjectParent = $"PRJ{i}",
                Month = 6
            }).ToList();
            var paginationData = new PaginationDto { PageNumber = 2, PageSize = 5, TotalRecords = 15, TotalPages = 3 };
            var serviceResult = new PaginatedResult<ProjectInvoiceDto>(dtos, paginationData);
            var expectedResponse = new PaginationRes<ProjectInvoiceRes>();

            _serviceMock.GetPagedProjectInvoicesByMonthAsync(query, month).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectInvoiceRes>>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetPagedProjectInvoicesByMonth(query, month);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _serviceMock.Received(1).GetPagedProjectInvoicesByMonthAsync(
                Arg.Is<QueryParameters<string>>(q => q.Page == 2 && q.PageSize == 5),
                month);
        }

        #endregion

        #region GetMonthlyInvoicesSummary

        [Fact]
        public async Task GetMonthlyInvoicesSummary_ValidQuery_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new MonthlyInvoicesPivotDto
            {
                Months = new List<int> { 1, 2, 3 },
                Rows = new List<MonthlyInvoicesSummaryDto>
                {
                    new MonthlyInvoicesSummaryDto
                    {
                        Program = "ADMIN",
                        ParentProject = "PRJ1",
                        MonthlyAmounts = new Dictionary<int, decimal>
                        {
                            { 1, 1000m },
                            { 2, 1500m },
                            { 3, 2000m }
                        }
                    }
                },
                Pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 }
            };
            var expectedResponse = new MonthlyInvoicesPivotRes
            {
                Months = new List<int> { 1, 2, 3 },
                Rows = new List<MonthlyInvoicesSummaryItemRes>
                {
                    new MonthlyInvoicesSummaryItemRes
                    {
                        Program = "ADMIN",
                        ParentProject = "PRJ1",
                        MonthlyAmounts = new Dictionary<int, decimal>
                        {
                            { 1, 1000m },
                            { 2, 1500m },
                            { 3, 2000m }
                        }
                    }
                },
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 }
            };

            _serviceMock.GetMonthlyInvoicesSummaryAsync(query).Returns(serviceResult);
            _mapperMock.Map<MonthlyInvoicesPivotRes>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetMonthlyInvoicesSummary(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MonthlyInvoicesPivotRes>(okResult.Value);
            Assert.Equal(3, response.Months.Count());
            Assert.Single(response.Rows);
            Assert.Equal("ADMIN", response.Rows.First().Program);
            await _serviceMock.Received(1).GetMonthlyInvoicesSummaryAsync(query);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummary_EmptyResult_ReturnsOkWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new MonthlyInvoicesPivotDto
            {
                Months = new List<int>(),
                Rows = new List<MonthlyInvoicesSummaryDto>(),
                Pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0, TotalPages = 0 }
            };
            var expectedResponse = new MonthlyInvoicesPivotRes
            {
                Months = new List<int>(),
                Rows = new List<MonthlyInvoicesSummaryItemRes>(),
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 0, TotalPages = 0 }
            };

            _serviceMock.GetMonthlyInvoicesSummaryAsync(query).Returns(serviceResult);
            _mapperMock.Map<MonthlyInvoicesPivotRes>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetMonthlyInvoicesSummary(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MonthlyInvoicesPivotRes>(okResult.Value);
            Assert.Empty(response.Months);
            Assert.Empty(response.Rows);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummary_MultipleProjects_ReturnsAllRows()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new MonthlyInvoicesPivotDto
            {
                Months = new List<int> { 1, 2 },
                Rows = new List<MonthlyInvoicesSummaryDto>
                {
                    new MonthlyInvoicesSummaryDto { Program = "ADMIN", ParentProject = "PRJ1" },
                    new MonthlyInvoicesSummaryDto { Program = "CORE", ParentProject = "PRJ2" },
                    new MonthlyInvoicesSummaryDto { Program = "TEST", ParentProject = "PRJ3" }
                },
                Pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 3, TotalPages = 1 }
            };
            var expectedResponse = new MonthlyInvoicesPivotRes
            {
                Months = new List<int> { 1, 2 },
                Rows = new List<MonthlyInvoicesSummaryItemRes>
                {
                    new MonthlyInvoicesSummaryItemRes { Program = "ADMIN", ParentProject = "PRJ1" },
                    new MonthlyInvoicesSummaryItemRes { Program = "CORE", ParentProject = "PRJ2" },
                    new MonthlyInvoicesSummaryItemRes { Program = "TEST", ParentProject = "PRJ3" }
                }
            };

            _serviceMock.GetMonthlyInvoicesSummaryAsync(query).Returns(serviceResult);
            _mapperMock.Map<MonthlyInvoicesPivotRes>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetMonthlyInvoicesSummary(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MonthlyInvoicesPivotRes>(okResult.Value);
            Assert.Equal(3, response.Rows.Count());
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummary_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 5 };
            var serviceResult = new MonthlyInvoicesPivotDto
            {
                Months = new List<int> { 1 },
                Rows = Enumerable.Range(6, 5).Select(i => new MonthlyInvoicesSummaryDto
                {
                    Program = "ADMIN",
                    ParentProject = $"PRJ{i}"
                }).ToList(),
                Pagination = new PaginationDto { PageNumber = 2, PageSize = 5, TotalRecords = 15, TotalPages = 3 }
            };
            var expectedResponse = new MonthlyInvoicesPivotRes();

            _serviceMock.GetMonthlyInvoicesSummaryAsync(query).Returns(serviceResult);
            _mapperMock.Map<MonthlyInvoicesPivotRes>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetMonthlyInvoicesSummary(query);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _serviceMock.Received(1).GetMonthlyInvoicesSummaryAsync(
                Arg.Is<QueryParameters<string>>(q => q.Page == 2 && q.PageSize == 5));
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummary_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _serviceMock.GetMonthlyInvoicesSummaryAsync(query)
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetMonthlyInvoicesSummary(query));
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummary_WithSortingAndFiltering_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "Program",
                Descending = true,
                Filter = "{\"Program\":\"ADMIN\"}"
            };
            var serviceResult = new MonthlyInvoicesPivotDto
            {
                Months = new List<int> { 1, 2, 3 },
                Rows = new List<MonthlyInvoicesSummaryDto>
                {
                    new MonthlyInvoicesSummaryDto { Program = "ADMIN", ParentProject = "PRJ1" }
                },
                Pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 }
            };
            var expectedResponse = new MonthlyInvoicesPivotRes();

            _serviceMock.GetMonthlyInvoicesSummaryAsync(query).Returns(serviceResult);
            _mapperMock.Map<MonthlyInvoicesPivotRes>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetMonthlyInvoicesSummary(query);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _serviceMock.Received(1).GetMonthlyInvoicesSummaryAsync(Arg.Is<QueryParameters<string>>(q =>
                q.SortBy == "Program" &&
                q.Descending == true &&
                q.Filter == "{\"Program\":\"ADMIN\"}"));
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummary_WithAll12Months_ReturnsCompleteData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new MonthlyInvoicesPivotDto
            {
                Months = Enumerable.Range(1, 12).ToList(),
                Rows = new List<MonthlyInvoicesSummaryDto>
                {
                    new MonthlyInvoicesSummaryDto
                    {
                        Program = "ADMIN",
                        ParentProject = "PRJ1",
                        MonthlyAmounts = Enumerable.Range(1, 12).ToDictionary(m => m, m => m * 1000m)
                    }
                },
                Pagination = new PaginationDto()
            };
            var expectedResponse = new MonthlyInvoicesPivotRes { Months = Enumerable.Range(1, 12).ToList() };

            _serviceMock.GetMonthlyInvoicesSummaryAsync(query).Returns(serviceResult);
            _mapperMock.Map<MonthlyInvoicesPivotRes>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetMonthlyInvoicesSummary(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MonthlyInvoicesPivotRes>(okResult.Value);
            Assert.Equal(12, response.Months.Count());
        }

        #endregion

        #region CopyInvoices Tests
        [Fact]
        public async Task CopyInvoices_ValidRequest_ReturnsOkWithSuccess()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 5, TargetMonth = 6, InvoiceIds = null };
            var dto = new CopyInvoicesDto { SourceMonth = 5, TargetMonth = 6, InvoiceIds = null };
            _mapperMock.Map<CopyInvoicesDto>(request).Returns(dto);
            _serviceMock.CopyInvoicesAsync(dto).Returns(true);

            // Act
            var result = await _controller.CopyInvoices(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<CopyInvoicesRes>(okResult.Value);
            Assert.True(response.Success);
            await _serviceMock.Received(1).CopyInvoicesAsync(dto);
        }

        [Fact]
        public async Task CopyInvoices_WithInvoiceIds_ReturnsOkWithSuccess()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 3, TargetMonth = 4, InvoiceIds = new List<int> { 1, 2 } };
            var dto = new CopyInvoicesDto { SourceMonth = 3, TargetMonth = 4, InvoiceIds = new List<int> { 1, 2 } };
            _mapperMock.Map<CopyInvoicesDto>(request).Returns(dto);
            _serviceMock.CopyInvoicesAsync(dto).Returns(true);

            // Act
            var result = await _controller.CopyInvoices(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<CopyInvoicesRes>(okResult.Value);
            Assert.True(response.Success);
        }

        #endregion

        #region CopyInvoices_ValidationTests

        [Fact]
        public async Task CopyInvoices_CancellationRequested_PropagatesCancellation()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 5, TargetMonth = 6, InvoiceRecords = null };
            var dto = new CopyInvoicesDto { SourceMonth = 5, TargetMonth = 6 };

            _mapperMock.Map<CopyInvoicesDto>(request).Returns(dto);
            _serviceMock.CopyInvoicesAsync(Arg.Any<CopyInvoicesDto>()).ThrowsAsync(new OperationCanceledException("Operation was cancelled"));

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => _controller.CopyInvoices(request));
        }

        [Fact]
        public async Task CopyInvoices_ValidMonthsInRequest_UsesRequestValues()
        {
            // Arrange
            var request = new CopyInvoicesReq { SourceMonth = 11, TargetMonth = 12, InvoiceIds = null };
            var dto = new CopyInvoicesDto { SourceMonth = 11, TargetMonth = 12 };

            _mapperMock.Map<CopyInvoicesDto>(request).Returns(dto);
            _serviceMock.CopyInvoicesAsync(Arg.Is<CopyInvoicesDto>(d => 
                d.SourceMonth == 11 && d.TargetMonth == 12))
                .Returns(true);

            // Act
            var result = await _controller.CopyInvoices(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<CopyInvoicesRes>(okResult.Value);
            Assert.True(response.Success);
            await _serviceMock.Received(1).CopyInvoicesAsync(Arg.Is<CopyInvoicesDto>(d => 
                d.SourceMonth == 11 && d.TargetMonth == 12));
        }

        #endregion
    }
}
