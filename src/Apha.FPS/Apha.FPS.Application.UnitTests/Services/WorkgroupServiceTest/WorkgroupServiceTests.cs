/*
 * TRANSFORMENGINE MIGRATION — WorkgroupServiceTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - NEW FILE: xUnit tests for WorkgroupService (frmMaintWorkGroup2 application layer)
 *   - Tests cover all 8 public methods: GetPagedAsync, GetByKeyAsync, CreateAsync, UpdateAsync,
 *     DeleteAsync, GetAllProfitCentresAsync, GetOwnersAsync, GetCostCentresByProfitCentreAsync
 *   - Uses NSubstitute for IWorkgroupRepository and IMapper mocks
 *   - Business-logic guards tested: null query, null dto, empty WorkGroupName, empty ProfitCentre,
 *     duplicate-name check (ExistsAsync), not-found check on Update
 *
 * PRESERVED:
 *   - Naming convention [MethodName]_[StateUnderTest]_[ExpectedResult]
 *   - #region grouping per method, consistent with GradeServiceTests pattern
 *   - BusinessValidationErrorException guard paths tested via Assert.ThrowsAsync
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify BusinessValidationErrorException is the correct exception type
 *     (not ArgumentException) — matches WorkgroupService guard implementation
 */

using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Npgsql;
using Xunit;

namespace Apha.FPS.Application.UnitTests.Services.WorkgroupServiceTest
{
    public class WorkgroupServiceTests
    {
        private readonly IWorkgroupRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly WorkgroupService _sut;

        public WorkgroupServiceTests()
        {
            _mockRepository = Substitute.For<IWorkgroupRepository>();
            _mockMapper     = Substitute.For<IMapper>();
            _sut            = new WorkgroupService(_mockRepository, _mockMapper);
        }

        // TRANSFORMENGINE: static helpers — minimal valid objects for test setup
        private static WorkgroupDto BuildDto(string name = "WG001") =>
            new() { WorkGroupName = name, ProfitCentre = "PC01", FpsYear = 2025 };

        private static Workgroup BuildEntity(string name = "WG001") =>
            new() { WorkGroupName = name, ProfitCentre = "PC01", FpsYear = 2025 };

        private static ManagerDto BuildManagerDto(string name = "Alice Smith") =>
            new() { Name = name };

        private static Manager BuildManagerEntity(string name = "Alice Smith") =>
            new() { Name = name, GradeCode = "A1" };

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

        // Builds a PostgresException that is NOT a foreign-key violation (e.g. a unique/other
        // violation), used to assert that unrelated DB errors are propagated unchanged.
        private static PostgresException BuildNonForeignKeyError() =>
            new(
                messageText: "unique violation",
                severity: "ERROR",
                invariantSeverity: "ERROR",
                sqlState: PostgresErrorCodes.UniqueViolation,
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
                constraintName: "uq_some_other_constraint");

        #region Constructor Tests

        [Fact]
        public void Constructor_CreatesInstance_WhenAllDependenciesProvided()
        {
            // TRANSFORMENGINE: WorkgroupService constructor has no null guards — instance creation succeeds
            var service = new WorkgroupService(_mockRepository, _mockMapper);
            Assert.NotNull(service);
        }

        #endregion

        #region GetPagedAsync Tests

        [Fact]
        public async Task GetPagedAsync_NullQuery_ThrowsBusinessValidationErrorException()
        {
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.GetPagedAsync(null!));
        }

        [Fact]
        public async Task GetPagedAsync_HappyPath_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData    = new PagedData<Workgroup>
            {
                Data           = new List<Workgroup> { BuildEntity() },
                PaginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var pagedResult = new PaginatedResult<WorkgroupDto>
            {
                Data           = new List<WorkgroupDto> { BuildDto() },
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedAsync(mappedParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<WorkgroupDto>>(pagedData).Returns(pagedResult);

            // Act
            var result = await _sut.GetPagedAsync(query);

            // Assert
            result.Should().Be(pagedResult);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetPagedAsync(mappedParams);
        }

        [Fact]
        public async Task GetPagedAsync_EmptyResult_ReturnsPaginatedResultWithEmptyData()
        {
            // Arrange
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData    = new PagedData<Workgroup>
            {
                Data           = [],
                PaginationData = new PaginationData { TotalRecords = 0 }
            };
            var emptyResult = new PaginatedResult<WorkgroupDto>
            {
                Data           = [],
                PaginationData = new PaginationDto { TotalRecords = 0 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedAsync(mappedParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<WorkgroupDto>>(pagedData).Returns(emptyResult);

            // Act
            var result = await _sut.GetPagedAsync(query);

            // Assert
            result.Data.Should().BeEmpty();
        }

        #endregion

        #region GetByKeyAsync Tests

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetByKeyAsync_InvalidKey_ThrowsBusinessValidationErrorException(string key)
        {
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.GetByKeyAsync(key));
        }

        [Fact]
        public async Task GetByKeyAsync_NullKey_ThrowsBusinessValidationErrorException()
        {
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.GetByKeyAsync(null!));
        }

        [Fact]
        public async Task GetByKeyAsync_NotFound_ReturnsNull()
        {
            // Arrange
            _mockRepository.GetByKeyAsync("NOTEXIST").Returns((Workgroup?)null);

            // Act
            var result = await _sut.GetByKeyAsync("NOTEXIST");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByKeyAsync_HappyPath_ReturnsMappedDto()
        {
            // Arrange
            var entity = BuildEntity("WG001");
            var dto    = BuildDto("WG001");

            _mockRepository.GetByKeyAsync("WG001").Returns(entity);
            _mockMapper.Map<WorkgroupDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetByKeyAsync("WG001");

            // Assert
            result.Should().Be(dto);
            await _mockRepository.Received(1).GetByKeyAsync("WG001");
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_NullDto_ThrowsBusinessValidationErrorException()
        {
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(null!));
        }

        [Fact]
        public async Task CreateAsync_EmptyWorkGroupName_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = new WorkgroupDto { WorkGroupName = "", ProfitCentre = "PC01" };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_EmptyProfitCentre_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = new WorkgroupDto { WorkGroupName = "WG001", ProfitCentre = "" };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_DuplicateWorkGroupName_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = BuildDto("WG001");

            // TRANSFORMENGINE: tI_WorkGroup trigger guard — ExistsAsync returns true → duplicate rejected
            _mockRepository.ExistsAsync("WG001").Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_HappyPath_ReturnsMappedDto()
        {
            // Arrange
            var dto     = BuildDto("WG001");
            var entity  = BuildEntity("WG001");
            var created = BuildEntity("WG001");
            var createdDto = BuildDto("WG001");

            // TRANSFORMENGINE: duplicate guard — ExistsAsync returns false → no duplicate
            _mockRepository.ExistsAsync("WG001").Returns(false);
            _mockMapper.Map<Workgroup>(dto).Returns(entity);
            _mockRepository.CreateAsync(entity).Returns(created);
            _mockMapper.Map<WorkgroupDto>(created).Returns(createdDto);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            result.Should().Be(createdDto);
            await _mockRepository.Received(1).CreateAsync(entity);
        }

        [Fact]
        public async Task CreateAsync_WhenCostCentreFkViolation_ThrowsBusinessValidationError()
        {
            // Arrange
            var dto    = BuildDto("WG001");
            var entity = BuildEntity("WG001");

            _mockRepository.ExistsAsync("WG001").Returns(false);
            _mockMapper.Map<Workgroup>(dto).Returns(entity);
            _mockRepository.CreateAsync(entity)
                .ThrowsAsync(new Exception("db error", BuildFkViolation("fk_workgroup_costcentre")));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));
            var error = Assert.Single(ex.Errors);
            Assert.Equal("COSTCENTRE_FK_VIOLATION", error.Code);
            Assert.Contains("Cost Center table", error.Message);
        }

        [Fact]
        public async Task CreateAsync_WhenUnrelatedDbError_PropagatesOriginalException()
        {
            // Arrange
            // WorkgroupService only special-cases foreign-key violations (SqlState 23503);
            // any other DB error must propagate unchanged.
            var dto      = BuildDto("WG001");
            var entity   = BuildEntity("WG001");
            var original = new Exception("db error", BuildNonForeignKeyError());

            _mockRepository.ExistsAsync("WG001").Returns(false);
            _mockMapper.Map<Workgroup>(dto).Returns(entity);
            _mockRepository.CreateAsync(entity).ThrowsAsync(original);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.CreateAsync(dto));
            Assert.Same(original, ex);
        }

        #endregion

        #region UpdateAsync Tests

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task UpdateAsync_InvalidOriginalKey_ThrowsBusinessValidationErrorException(string key)
        {
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(key, BuildDto()));
        }

        [Fact]
        public async Task UpdateAsync_NullDto_ThrowsBusinessValidationErrorException()
        {
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync("WG001", null!));
        }

        [Fact]
        public async Task UpdateAsync_EmptyWorkGroupNameInDto_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = new WorkgroupDto { WorkGroupName = "", ProfitCentre = "PC01" };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync("WG001", dto));
        }

        [Fact]
        public async Task UpdateAsync_EmptyProfitCentreInDto_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = new WorkgroupDto { WorkGroupName = "WG001", ProfitCentre = "" };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync("WG001", dto));
        }

        [Fact]
        public async Task UpdateAsync_NotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = BuildDto("WG001");

            // TRANSFORMENGINE: pre-check — ExistsAsync returns false → not found
            _mockRepository.ExistsAsync("WG001").Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateAsync("WG001", dto));
        }

        [Fact]
        public async Task UpdateAsync_HappyPath_ReturnsMappedDto()
        {
            // Arrange
            var dto        = BuildDto("WG001");
            var entity     = BuildEntity("WG001");
            var updated    = BuildEntity("WG001");
            var updatedDto = BuildDto("WG001");

            _mockRepository.ExistsAsync("WG001").Returns(true);
            _mockMapper.Map<Workgroup>(dto).Returns(entity);
            _mockRepository.UpdateAsync("WG001", entity).Returns(updated);
            _mockMapper.Map<WorkgroupDto>(updated).Returns(updatedDto);

            // Act
            var result = await _sut.UpdateAsync("WG001", dto);

            // Assert
            result.Should().Be(updatedDto);
            await _mockRepository.Received(1).UpdateAsync("WG001", entity);
        }

        [Fact]
        public async Task UpdateAsync_WithRename_PassesOriginalKeyToRepository()
        {
            // Arrange
            var dto = BuildDto("WG_RENAMED");
            var entity = BuildEntity("WG_RENAMED");
            var updated = BuildEntity("WG_RENAMED");
            var updatedDto = BuildDto("WG_RENAMED");

            _mockRepository.ExistsAsync("WG_ORIGINAL").Returns(true);
            _mockMapper.Map<Workgroup>(dto).Returns(entity);
            _mockRepository.UpdateAsync("WG_ORIGINAL", entity).Returns(updated);
            _mockMapper.Map<WorkgroupDto>(updated).Returns(updatedDto);

            // Act
            await _sut.UpdateAsync("WG_ORIGINAL", dto);

            // Assert
            await _mockRepository.Received(1).UpdateAsync("WG_ORIGINAL", entity);
        }

        [Fact]
        public async Task UpdateAsync_WhenCostCentreFkViolation_ThrowsBusinessValidationError()
        {
            // Arrange
            var dto    = BuildDto("WG001");
            var entity = BuildEntity("WG001");

            _mockRepository.ExistsAsync("WG001").Returns(true);
            _mockMapper.Map<Workgroup>(dto).Returns(entity);
            _mockRepository.UpdateAsync("WG001", entity)
                .ThrowsAsync(new Exception("db error", BuildFkViolation("fk_workgroup_costcentre")));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync("WG001", dto));
            var error = Assert.Single(ex.Errors);
            Assert.Equal("COSTCENTRE_FK_VIOLATION", error.Code);
            Assert.Contains("Cost Center table", error.Message);
        }

        [Fact]
        public async Task UpdateAsync_WhenUnrelatedFkViolation_PropagatesOriginalException()
        {
            // Arrange
            var dto      = BuildDto("WG001");
            var entity   = BuildEntity("WG001");
            var original = new Exception("db error", BuildFkViolation("fk_some_other_constraint"));

            _mockRepository.ExistsAsync("WG001").Returns(true);
            _mockMapper.Map<Workgroup>(dto).Returns(entity);
            _mockRepository.UpdateAsync("WG001", entity).ThrowsAsync(original);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.UpdateAsync("WG001", dto));
            Assert.Same(original, ex);
        }

        #endregion

        #region DeleteAsync Tests

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task DeleteAsync_InvalidKey_ThrowsBusinessValidationErrorException(string key)
        {
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.DeleteAsync(key));
        }

        [Fact]
        public async Task DeleteAsync_NullKey_ThrowsBusinessValidationErrorException()
        {
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.DeleteAsync(null!));
        }

        [Fact]
        public async Task DeleteAsync_HappyPath_ReturnsTrue()
        {
            // Arrange
            _mockRepository.DeleteAsync("WG001").Returns(true);

            // Act
            var result = await _sut.DeleteAsync("WG001");

            // Assert
            Assert.True(result);
            await _mockRepository.Received(1).DeleteAsync("WG001");
        }

        [Fact]
        public async Task DeleteAsync_NotFound_ReturnsFalse()
        {
            // Arrange
            _mockRepository.DeleteAsync("NOTEXIST").Returns(false);

            // Act
            var result = await _sut.DeleteAsync("NOTEXIST");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_WhenWorkgroupGradeFkViolation_ThrowsBusinessValidationError()
        {
            // Arrange
            _mockRepository.DeleteAsync("WG001")
                .ThrowsAsync(new Exception("db error", BuildFkViolation("fk_workgroupgrade_workgroup_10")));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.DeleteAsync("WG001"));
            var error = Assert.Single(ex.Errors);
            Assert.Equal("WORKGROUPGRADE_FK_VIOLATION", error.Code);
            Assert.Contains("associated records in the system", error.Message);
        }

        [Fact]
        public async Task DeleteAsync_WhenUnrelatedDbError_PropagatesOriginalException()
        {
            // Arrange
            // WorkgroupService only special-cases foreign-key violations (SqlState 23503);
            // any other DB error must propagate unchanged.
            var original = new Exception("db error", BuildNonForeignKeyError());
            _mockRepository.DeleteAsync("WG001").ThrowsAsync(original);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.DeleteAsync("WG001"));
            Assert.Same(original, ex);
        }

        #endregion

        #region GetAllProfitCentresAsync Tests

        [Fact]
        public async Task GetAllProfitCentresAsync_HappyPath_ReturnsList()
        {
            // Arrange
            var profitCentres = new List<string> { "PC01", "PC02" };
            _mockRepository.GetAllProfitCentresAsync().Returns(profitCentres.AsEnumerable());

            // Act
            var result = await _sut.GetAllProfitCentresAsync();

            // Assert
            result.Should().BeEquivalentTo(profitCentres);
            await _mockRepository.Received(1).GetAllProfitCentresAsync();
        }

        [Fact]
        public async Task GetAllProfitCentresAsync_EmptyList_ReturnsEmptyEnumerable()
        {
            // Arrange
            _mockRepository.GetAllProfitCentresAsync().Returns(Enumerable.Empty<string>());

            // Act
            var result = await _sut.GetAllProfitCentresAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetOwnersAsync Tests

        [Fact]
        public async Task GetOwnersAsync_HappyPath_ReturnsMappedManagerDtos()
        {
            // Arrange
            var managers    = new List<Manager> { BuildManagerEntity("Alice"), BuildManagerEntity("Bob") };
            var managerDtos = new List<ManagerDto> { BuildManagerDto("Alice"), BuildManagerDto("Bob") };

            _mockRepository.GetOwnersAsync().Returns(managers.AsEnumerable());
            _mockMapper.Map<IEnumerable<ManagerDto>>(Arg.Any<IEnumerable<Manager>>()).Returns(managerDtos);

            // Act
            var result = await _sut.GetOwnersAsync();

            // Assert
            result.Should().HaveCount(2);
            await _mockRepository.Received(1).GetOwnersAsync();
            _mockMapper.Received(1).Map<IEnumerable<ManagerDto>>(Arg.Any<IEnumerable<Manager>>());
        }

        [Fact]
        public async Task GetOwnersAsync_EmptyList_ReturnsEmptyEnumerable()
        {
            // Arrange
            _mockRepository.GetOwnersAsync().Returns(Enumerable.Empty<Manager>());
            _mockMapper.Map<IEnumerable<ManagerDto>>(Arg.Any<IEnumerable<Manager>>()).Returns(Enumerable.Empty<ManagerDto>());

            // Act
            var result = await _sut.GetOwnersAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetCostCentresByProfitCentreAsync Tests

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetCostCentresByProfitCentreAsync_InvalidProfitCentre_ThrowsBusinessValidationErrorException(string profitCentre)
        {
            await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetCostCentresByProfitCentreAsync(profitCentre));
        }

        [Fact]
        public async Task GetCostCentresByProfitCentreAsync_NullProfitCentre_ThrowsBusinessValidationErrorException()
        {
            await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetCostCentresByProfitCentreAsync(null!));
        }

        [Fact]
        public async Task GetCostCentresByProfitCentreAsync_HappyPath_ReturnsCostCentres()
        {
            // Arrange
            var costCentres = new List<double?> { 100.0, 200.0 };
            _mockRepository.GetCostCentresByProfitCentreAsync("PC01").Returns(costCentres.AsEnumerable());

            // Act
            var result = await _sut.GetCostCentresByProfitCentreAsync("PC01");

            // Assert
            result.Should().BeEquivalentTo(costCentres);
            await _mockRepository.Received(1).GetCostCentresByProfitCentreAsync("PC01");
        }

        [Fact]
        public async Task GetCostCentresByProfitCentreAsync_EmptyResult_ReturnsEmptyEnumerable()
        {
            // Arrange
            _mockRepository.GetCostCentresByProfitCentreAsync("PC_EMPTY").Returns(Enumerable.Empty<double?>());

            // Act
            var result = await _sut.GetCostCentresByProfitCentreAsync("PC_EMPTY");

            // Assert
            result.Should().BeEmpty();
        }

        #endregion
    }
}
