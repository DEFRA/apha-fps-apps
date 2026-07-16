using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Validation;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.WorkgroupControllerTest
{
    public class WorkgroupControllerTests
    {
        private readonly IWorkgroupService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly WorkgroupController _controller;

        public WorkgroupControllerTests()
        {
            _serviceMock = Substitute.For<IWorkgroupService>();
            _mapperMock  = Substitute.For<IMapper>();
            _controller  = new WorkgroupController(_serviceMock, _mapperMock);
        }

        private static WorkgroupDto BuildDto(string name = "WG001") =>
            new() { WorkGroupName = name, ProfitCentre = "PC01", FpsYear = 2025 };

        private static WorkgroupMaintenanceReq BuildReq(string name = "WG001") =>
            new() { WorkGroupName = name, ProfitCentre = "PC01" };

        private static WorkgroupMaintenanceRes BuildRes(string name = "WG001") =>
            new() { WorkGroupName = name, ProfitCentre = "PC01" };

        private static ManagerDto BuildManagerDto(string managerName = "John Smith") =>
            new() { Name = managerName };

        private static ManagerRes BuildManagerRes(string managerName = "John Smith") =>
            new() { Name = managerName };

        // Builds a PostgresException carrying a foreign-key violation (SqlState 23503) for the
        // given constraint name, mimicking how Npgsql surfaces DB FK violations.
        private static PostgresException BuildFkViolation(string constraintName) =>
            new(
                messageText: "foreign key violation",
                severity: "ERROR",
                invariantSeverity: "ERROR",
                sqlState: PostgresErrorCodes.ForeignKeyViolation,
                detail: null,
                hint: null,
                position: 0,
                internalPosition: 0,
                internalQuery: null,
                where: null,
                schemaName: null,
                tableName: null,
                columnName: null,
                dataTypeName: null,
                constraintName: constraintName);

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new WorkgroupController(null!, _mapperMock));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new WorkgroupController(_serviceMock, null!));
        }

        #endregion

        #region GetPagedAsync Tests

        [Fact]
        public async Task GetPagedAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<WorkgroupDto> { BuildDto() };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var serviceResult = new PaginatedResult<WorkgroupDto>(dtos, pagination);
            var expectedResponse = new PaginationRes<WorkgroupMaintenanceRes>
            {
                Data = new List<WorkgroupMaintenanceRes> { BuildRes() },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _serviceMock.GetPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WorkgroupMaintenanceRes>>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetPagedAsync(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expectedResponse, okResult.Value);
            await _serviceMock.Received(1).GetPagedAsync(query);
        }

        [Fact]
        public async Task GetPagedAsync_NullResult_ThrowsArgumentException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _serviceMock.GetPagedAsync(query).Returns((PaginatedResult<WorkgroupDto>)null!);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetPagedAsync(query));
        }

        [Fact]
        public async Task GetPagedAsync_WithFilterAndSorting_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 2, PageSize = 5, SortBy = "WorkGroupName", Descending = true
            };
            var dtos = new List<WorkgroupDto> { BuildDto() };
            var pagination = new PaginationDto { PageNumber = 2, PageSize = 5, TotalRecords = 10 };
            var serviceResult = new PaginatedResult<WorkgroupDto>(dtos, pagination);
            var expectedResponse = new PaginationRes<WorkgroupMaintenanceRes>
            {
                Data = new List<WorkgroupMaintenanceRes> { BuildRes() },
                PaginationData = new Pagination { PageNumber = 2, PageSize = 5, TotalRecords = 10 }
            };

            _serviceMock.GetPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WorkgroupMaintenanceRes>>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetPagedAsync(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<PaginationRes<WorkgroupMaintenanceRes>>(okResult.Value);
            Assert.Equal(2, response.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetPagedAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _serviceMock.GetPagedAsync(query).ThrowsAsync(new InvalidOperationException("service error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetPagedAsync(query));
        }

        #endregion

        #region GetByKeyAsync Tests

        [Fact]
        public async Task GetByKeyAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var dto = BuildDto("WG001");
            var res = BuildRes("WG001");

            _serviceMock.GetByKeyAsync("WG001").Returns(dto);
            _mapperMock.Map<WorkgroupMaintenanceRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetByKeyAsync("WG001");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(res, okResult.Value);
            await _serviceMock.Received(1).GetByKeyAsync("WG001");
        }

        [Fact]
        public async Task GetByKeyAsync_NullOrWhitespace_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetByKeyAsync(""));
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetByKeyAsync("   "));
        }

        [Fact]
        public async Task GetByKeyAsync_NullResult_ThrowsKeyNotFoundException()
        {
            // Arrange
            _serviceMock.GetByKeyAsync("NOTEXIST").Returns((WorkgroupDto?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _controller.GetByKeyAsync("NOTEXIST"));
            Assert.Contains("NOTEXIST", exception.Message);
        }

        [Fact]
        public async Task GetByKeyAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetByKeyAsync("WG001").ThrowsAsync(new InvalidOperationException("service error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetByKeyAsync("WG001"));
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var req     = BuildReq("WG001");
            var dto     = BuildDto("WG001");
            var created = BuildDto("WG001");
            var res     = BuildRes("WG001");

            _mapperMock.Map<WorkgroupDto>(req).Returns(dto);
            _serviceMock.CreateAsync(dto).Returns(created);
            _mapperMock.Map<WorkgroupMaintenanceRes>(created).Returns(res);

            // Act
            var result = await _controller.CreateAsync(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(res, okResult.Value);
            await _serviceMock.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateAsync_WhenServiceThrowsOnDuplicate_PropagatesException()
        {
            // Arrange
            var req = BuildReq("WG001");
            var dto = BuildDto("WG001");

            _mapperMock.Map<WorkgroupDto>(req).Returns(dto);
            _serviceMock.CreateAsync(dto).ThrowsAsync(new InvalidOperationException("Workgroup already exists"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.CreateAsync(req));
        }

        [Fact]
        public async Task CreateAsync_WhenServiceThrowsValidation_PropagatesException()
        {
            // Arrange
            var req = BuildReq();
            var dto = BuildDto();

            _mapperMock.Map<WorkgroupDto>(req).Returns(dto);
            _serviceMock.CreateAsync(dto).ThrowsAsync(new ArgumentException("WorkGroupName is required"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.CreateAsync(req));
        }

        [Fact]
        public async Task CreateAsync_WhenCostCentreFkViolation_ThrowsBusinessValidationError()
        {
            // Arrange
            var req = BuildReq("WG001");
            var dto = BuildDto("WG001");

            _mapperMock.Map<WorkgroupDto>(req).Returns(dto);
            _serviceMock.CreateAsync(dto)
                .ThrowsAsync(new Exception("db error", BuildFkViolation("fk_workgroup_costcentre")));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _controller.CreateAsync(req));
            var error = Assert.Single(ex.Errors);
            Assert.Equal("COSTCENTRE_FK_VIOLATION", error.Code);
            Assert.Contains("Cost Center table", error.Message);
        }

        [Fact]
        public async Task CreateAsync_WhenUnrelatedFkViolation_PropagatesOriginalException()
        {
            // Arrange
            var req = BuildReq("WG001");
            var dto = BuildDto("WG001");
            var original = new Exception("db error", BuildFkViolation("fk_some_other_constraint"));

            _mapperMock.Map<WorkgroupDto>(req).Returns(dto);
            _serviceMock.CreateAsync(dto).ThrowsAsync(original);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.CreateAsync(req));
            Assert.Same(original, ex);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var req     = BuildReq("WG001");
            var dto     = BuildDto("WG001");
            var updated = BuildDto("WG001");
            var res     = BuildRes("WG001");

            _mapperMock.Map<WorkgroupDto>(req).Returns(dto);
            _serviceMock.UpdateAsync("WG001", dto).Returns(updated);
            _mapperMock.Map<WorkgroupMaintenanceRes>(updated).Returns(res);

            // Act
            var result = await _controller.UpdateAsync("WG001", req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(res, okResult.Value);
            await _serviceMock.Received(1).UpdateAsync("WG001", dto);
        }

        [Fact]
        public async Task UpdateAsync_NullOrWhitespaceKey_ThrowsArgumentException()
        {
            // Arrange
            var req = BuildReq();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.UpdateAsync("", req));
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.UpdateAsync("   ", req));
        }

        [Fact]
        public async Task UpdateAsync_WhenServiceThrowsNotFound_PropagatesException()
        {
            // Arrange
            var req = BuildReq();
            var dto = BuildDto();

            _mapperMock.Map<WorkgroupDto>(req).Returns(dto);
            _serviceMock.UpdateAsync("WG001", dto).ThrowsAsync(new KeyNotFoundException("not found"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.UpdateAsync("WG001", req));
        }

        [Fact]
        public async Task UpdateAsync_WithRename_CallsServiceWithOriginalKey()
        {
            // Arrange
            var req     = BuildReq("WG_RENAMED");
            var dto     = BuildDto("WG_RENAMED");
            var updated = BuildDto("WG_RENAMED");
            var res     = BuildRes("WG_RENAMED");

            _mapperMock.Map<WorkgroupDto>(req).Returns(dto);
            _serviceMock.UpdateAsync("WG_ORIGINAL", dto).Returns(updated);
            _mapperMock.Map<WorkgroupMaintenanceRes>(updated).Returns(res);

            // Act
            var result = await _controller.UpdateAsync("WG_ORIGINAL", req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            await _serviceMock.Received(1).UpdateAsync("WG_ORIGINAL", dto);
        }

        [Fact]
        public async Task UpdateAsync_WhenCostCentreFkViolation_ThrowsBusinessValidationError()
        {
            // Arrange
            var req = BuildReq("WG001");
            var dto = BuildDto("WG001");

            _mapperMock.Map<WorkgroupDto>(req).Returns(dto);
            _serviceMock.UpdateAsync("WG001", dto)
                .ThrowsAsync(new Exception("db error", BuildFkViolation("fk_workgroup_costcentre")));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _controller.UpdateAsync("WG001", req));
            var error = Assert.Single(ex.Errors);
            Assert.Equal("COSTCENTRE_FK_VIOLATION", error.Code);
            Assert.Contains("Cost Center table", error.Message);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            _serviceMock.DeleteAsync("WG001").Returns(true);

            // Act
            var result = await _controller.DeleteAsync("WG001");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
            await _serviceMock.Received(1).DeleteAsync("WG001");
        }

        [Fact]
        public async Task DeleteAsync_WithNullOrWhitespace_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteAsync(""));
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteAsync("   "));
        }

        [Fact]
        public async Task DeleteAsync_WhenNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _serviceMock.DeleteAsync("NOTEXIST").Returns(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _controller.DeleteAsync("NOTEXIST"));
            Assert.Contains("NOTEXIST", exception.Message);
        }

        [Fact]
        public async Task DeleteAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.DeleteAsync("WG001").ThrowsAsync(new InvalidOperationException("service error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.DeleteAsync("WG001"));
        }

        [Fact]
        public async Task DeleteAsync_WhenWorkgroupGradeFkViolation_ThrowsBusinessValidationError()
        {
            // Arrange
            _serviceMock.DeleteAsync("WG001")
                .ThrowsAsync(new Exception("db error", BuildFkViolation("fk_workgroupgrade_workgroup_10")));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _controller.DeleteAsync("WG001"));
            var error = Assert.Single(ex.Errors);
            Assert.Equal("WORKGROUPGRADE_FK_VIOLATION", error.Code);
            Assert.Contains("WorkgroupGrade table", error.Message);
        }

        [Fact]
        public async Task DeleteAsync_WhenUnrelatedFkViolation_PropagatesOriginalException()
        {
            // Arrange
            var original = new Exception("db error", BuildFkViolation("fk_some_other_constraint"));
            _serviceMock.DeleteAsync("WG001").ThrowsAsync(original);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.DeleteAsync("WG001"));
            Assert.Same(original, ex);
        }

        #endregion

        #region GetProfitCentresAsync Tests

        [Fact]
        public async Task GetProfitCentresAsync_HappyPath_ReturnsOkWithList()
        {
            // Arrange
            var profitCentres = new List<string> { "PC01", "PC02", "PC03" };
            _serviceMock.GetAllProfitCentresAsync().Returns(profitCentres.AsEnumerable());

            // Act
            var result = await _controller.GetProfitCentresAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value);
            Assert.Equal(3, data.Count());
            await _serviceMock.Received(1).GetAllProfitCentresAsync();
        }

        [Fact]
        public async Task GetProfitCentresAsync_EmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            _serviceMock.GetAllProfitCentresAsync().Returns(Enumerable.Empty<string>());

            // Act
            var result = await _controller.GetProfitCentresAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value);
            Assert.Empty(data);
        }

        [Fact]
        public async Task GetProfitCentresAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetAllProfitCentresAsync().ThrowsAsync(new InvalidOperationException("service error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetProfitCentresAsync());
        }

        #endregion

        #region GetOwnersAsync Tests

        [Fact]
        public async Task GetOwnersAsync_HappyPath_ReturnsOkWithMappedManagers()
        {
            // Arrange
            var managerDtos = new List<ManagerDto> { BuildManagerDto("Alice"), BuildManagerDto("Bob") };
            var managerRes  = new List<ManagerRes>  { BuildManagerRes("Alice"), BuildManagerRes("Bob") };
            _serviceMock.GetOwnersAsync().Returns(managerDtos.AsEnumerable());
            _mapperMock.Map<IEnumerable<ManagerRes>>(Arg.Any<IEnumerable<ManagerDto>>()).Returns(managerRes);

            // Act
            var result = await _controller.GetOwnersAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsAssignableFrom<IEnumerable<ManagerRes>>(okResult.Value);
            Assert.Equal(2, data.Count());
            await _serviceMock.Received(1).GetOwnersAsync();
            _mapperMock.Received(1).Map<IEnumerable<ManagerRes>>(Arg.Any<IEnumerable<ManagerDto>>());
        }

        [Fact]
        public async Task GetOwnersAsync_EmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            _serviceMock.GetOwnersAsync().Returns(Enumerable.Empty<ManagerDto>());
            _mapperMock.Map<IEnumerable<ManagerRes>>(Arg.Any<IEnumerable<ManagerDto>>()).Returns(Enumerable.Empty<ManagerRes>());

            // Act
            var result = await _controller.GetOwnersAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsAssignableFrom<IEnumerable<ManagerRes>>(okResult.Value);
            Assert.Empty(data);
        }

        [Fact]
        public async Task GetOwnersAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetOwnersAsync().ThrowsAsync(new InvalidOperationException("service error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetOwnersAsync());
        }

        #endregion

        #region GetCostCentresAsync Tests

        [Fact]
        public async Task GetCostCentresAsync_HappyPath_ReturnsOkWithCostCentres()
        {
            // Arrange
            var costCentres = new List<double?> { 100.0, 200.0, 300.0 };
            _serviceMock.GetCostCentresByProfitCentreAsync("PC01").Returns(costCentres.AsEnumerable());

            // Act
            var result = await _controller.GetCostCentresAsync("PC01");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsAssignableFrom<IEnumerable<double?>>(okResult.Value);
            Assert.Equal(3, data.Count());
            await _serviceMock.Received(1).GetCostCentresByProfitCentreAsync("PC01");
        }

        [Fact]
        public async Task GetCostCentresAsync_NullOrWhitespaceProfitCentre_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetCostCentresAsync(""));
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetCostCentresAsync("   "));
        }

        [Fact]
        public async Task GetCostCentresAsync_EmptyResult_ReturnsOkWithEmptyList()
        {
            // Arrange
            _serviceMock.GetCostCentresByProfitCentreAsync("PC_EMPTY").Returns(Enumerable.Empty<double?>());

            // Act
            var result = await _controller.GetCostCentresAsync("PC_EMPTY");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsAssignableFrom<IEnumerable<double?>>(okResult.Value);
            Assert.Empty(data);
        }

        [Fact]
        public async Task GetCostCentresAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetCostCentresByProfitCentreAsync("PC01").ThrowsAsync(new InvalidOperationException("service error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetCostCentresAsync("PC01"));
        }

        #endregion
    }
}
