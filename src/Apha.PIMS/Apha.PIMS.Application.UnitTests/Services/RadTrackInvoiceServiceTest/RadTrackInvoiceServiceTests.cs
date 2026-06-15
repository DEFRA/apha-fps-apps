// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — RadTrackInvoiceServiceTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - New file: xUnit tests for RadTrackInvoiceService (Phase 3).
 *   - Tests cover GetAllAsync, GetByIdAsync, CreateAsync, UpdateAsync, DeleteAsync,
 *     GetTotalsAsync, ExistsAsync — all 7 public service methods.
 *   - NSubstitute used for IRadTrackInvoiceRepository and IMapper mocks.
 *   - FluentAssertions used (project already references FluentAssertions 8.9.0).
 *   - Follows MilestoneServiceTests conventions (ValidDto helper, #region grouping).
 *
 * PRESERVED:
 *   - All business validation guards: PROJECT_REQUIRED, DUE_AMOUNT_REQUIRED, DUE_DATE_REQUIRED,
 *     INVOICE_COUNTER_REQUIRED, INVOICE_REF_DUPLICATE tested explicitly.
 *   - Naming convention: [MethodName]_[StateUnderTest]_[ExpectedResult].
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: BusinessValidationErrorException catch assertions assume the type
 *     is thrown directly; verify exception-handling middleware alignment if tested end-to-end.
 */

using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Application.Services;
using Apha.PIMS.Application.Validation;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Application.UnitTests.Services.RadTrackInvoiceServiceTest
{
    public class RadTrackInvoiceServiceTests
    {
        private readonly IRadTrackInvoiceRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly RadTrackInvoiceService _sut;

        public RadTrackInvoiceServiceTests()
        {
            _mockRepository = Substitute.For<IRadTrackInvoiceRepository>();
            _mockMapper     = Substitute.For<IMapper>();
            _sut            = new RadTrackInvoiceService(_mockRepository, _mockMapper);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static RadTrackInvoiceDto ValidDto(int id = 0) => new()
        {
            InvoiceCounter = id,
            Project        = "PP001",
            Contract       = "C001",
            DueAmount      = 1500.0,
            DueDate        = DateTime.Today.AddDays(30),
            InvoiceRef     = "INV-001"
        };

        private static RadTrackInvoice ValidEntity(int id = 1) => new()
        {
            InvoiceCounter = id,
            Project        = "PP001",
            Contract       = "C001",
            DueAmount      = 1500.0,
            DueDate        = DateTime.Today.AddDays(30),
            InvoiceRef     = "INV-001"
        };

        private static RadTrackInvoiceTotals SampleTotals() => new()
        {
            TotalPlannedAmount = 5000.0,
            TotalDueAmount     = 3000.0,
            TotalActualAmount  = 2500.0
        };

        private static RadTrackInvoiceTotalsDto SampleTotalsDto() => new()
        {
            TotalPlannedAmount = 5000.0,
            TotalDueAmount     = 3000.0,
            TotalActualAmount  = 2500.0
        };

        // ── GetAllAsync ────────────────────────────────────────────────────────

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_WithValidData_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query            = new QueryParameters<RadTrackInvoiceFilter> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<RadTrackInvoiceFilter>(page: 1, pageSize: 10);

            var entities = new List<RadTrackInvoice> { ValidEntity(1), ValidEntity(2) };
            var paginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var pagedData      = new PagedData<RadTrackInvoice>(entities, paginationData);

            var dtos          = new List<RadTrackInvoiceDto> { ValidDto(), ValidDto() };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };

            _mockMapper.Map<PaginationParameters<RadTrackInvoiceFilter>>(query).Returns(paginationParams);
            _mockRepository.GetAllAsync(paginationParams).Returns(pagedData);
            _mockMapper.Map<List<RadTrackInvoiceDto>>(pagedData.Data).Returns(dtos);
            _mockMapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            var result = await _sut.GetAllAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.PaginationData.TotalRecords.Should().Be(2);

            _mockMapper.Received(1).Map<PaginationParameters<RadTrackInvoiceFilter>>(query);
            await _mockRepository.Received(1).GetAllAsync(paginationParams);
            _mockMapper.Received(1).Map<List<RadTrackInvoiceDto>>(pagedData.Data);
            _mockMapper.Received(1).Map<PaginationDto>(pagedData.PaginationData);
        }

        [Fact]
        public async Task GetAllAsync_WithEmptyData_ReturnsMappedEmptyResult()
        {
            // Arrange
            var query            = new QueryParameters<RadTrackInvoiceFilter> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<RadTrackInvoiceFilter>(page: 1, pageSize: 10);

            var pagedData     = new PagedData<RadTrackInvoice>(new List<RadTrackInvoice>(), new PaginationData { TotalRecords = 0 });
            var emptyDtos     = new List<RadTrackInvoiceDto>();
            var paginationDto = new PaginationDto();

            _mockMapper.Map<PaginationParameters<RadTrackInvoiceFilter>>(query).Returns(paginationParams);
            _mockRepository.GetAllAsync(paginationParams).Returns(pagedData);
            _mockMapper.Map<List<RadTrackInvoiceDto>>(pagedData.Data).Returns(emptyDtos);
            _mockMapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            var result = await _sut.GetAllAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_NullParameters_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetAllAsync(null!));
        }

        #endregion

        // ── GetByIdAsync ───────────────────────────────────────────────────────

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_WhenRecordExists_ReturnsMappedDto()
        {
            // Arrange
            const int id    = 42;
            var entity      = ValidEntity(id);
            var expectedDto = ValidDto(id);

            _mockRepository.GetByIdAsync(id).Returns(entity);
            _mockMapper.Map<RadTrackInvoiceDto>(entity).Returns(expectedDto);

            // Act
            var result = await _sut.GetByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result!.InvoiceCounter.Should().Be(id);
            await _mockRepository.Received(1).GetByIdAsync(id);
            _mockMapper.Received(1).Map<RadTrackInvoiceDto>(entity);
        }

        [Fact]
        public async Task GetByIdAsync_WhenRecordDoesNotExist_ReturnsNull()
        {
            // Arrange
            _mockRepository.GetByIdAsync(999).Returns((RadTrackInvoice?)null);

            // Act
            var result = await _sut.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
            _mockMapper.DidNotReceive().Map<RadTrackInvoiceDto>(Arg.Any<RadTrackInvoice>());
        }

        #endregion

        // ── CreateAsync ────────────────────────────────────────────────────────

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_WithValidDto_CreatesAndReturnsMappedDto()
        {
            // Arrange
            var dto     = ValidDto();
            var entity  = ValidEntity();
            var created = ValidEntity(5);
            var result_dto = ValidDto(5);

            _mockRepository.ExistsAsync(dto.Project, dto.Contract, dto.InvoiceRef, null).Returns(false);
            _mockMapper.Map<RadTrackInvoice>(dto).Returns(entity);
            _mockRepository.CreateAsync(entity).Returns(created);
            _mockMapper.Map<RadTrackInvoiceDto>(created).Returns(result_dto);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.InvoiceCounter.Should().Be(5);

            await _mockRepository.Received(1).CreateAsync(entity);
            _mockMapper.Received(1).Map<RadTrackInvoice>(dto);
            _mockMapper.Received(1).Map<RadTrackInvoiceDto>(created);
        }

        [Fact]
        public async Task CreateAsync_NullDto_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(null!));
        }

        [Fact]
        public async Task CreateAsync_MissingProject_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = ValidDto();
            dto.Project = null;

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));
            ex.Errors.Should().Contain(e => e.Code == "PROJECT_REQUIRED");
        }

        [Fact]
        public async Task CreateAsync_MissingDueAmount_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = ValidDto();
            dto.DueAmount = null;

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));
            ex.Errors.Should().Contain(e => e.Code == "DUE_AMOUNT_REQUIRED");
        }

        [Fact]
        public async Task CreateAsync_MissingDueDate_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = ValidDto();
            dto.DueDate = null;

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));
            ex.Errors.Should().Contain(e => e.Code == "DUE_DATE_REQUIRED");
        }

        [Fact]
        public async Task CreateAsync_DuplicateInvoiceRef_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = ValidDto();
            _mockRepository.ExistsAsync(dto.Project, dto.Contract, dto.InvoiceRef, null).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));
            ex.Errors.Should().Contain(e => e.Code == "INVOICE_REF_DUPLICATE");
        }

        [Fact]
        public async Task CreateAsync_NullInvoiceRef_SkipsDuplicateCheck()
        {
            // Arrange
            var dto = ValidDto();
            dto.InvoiceRef = null;
            var entity  = ValidEntity();
            var created = ValidEntity(10);
            var resultDto = ValidDto(10);

            _mockMapper.Map<RadTrackInvoice>(dto).Returns(entity);
            _mockRepository.CreateAsync(entity).Returns(created);
            _mockMapper.Map<RadTrackInvoiceDto>(created).Returns(resultDto);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            // ExistsAsync must NOT be called when InvoiceRef is null
            await _mockRepository.DidNotReceive().ExistsAsync(
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>());
        }

        #endregion

        // ── UpdateAsync ────────────────────────────────────────────────────────

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_WithValidDto_UpdatesAndReturnsMappedDto()
        {
            // Arrange
            var dto      = ValidDto(7);
            var existing = ValidEntity(7);
            var updated  = ValidEntity(7);
            var resultDto = ValidDto(7);

            _mockRepository.GetByIdAsync(7).Returns(existing);
            _mockRepository.ExistsAsync(dto.Project, dto.Contract, dto.InvoiceRef, 7).Returns(false);
            _mockMapper.Map(dto, existing);
            _mockRepository.UpdateAsync(existing).Returns(updated);
            _mockMapper.Map<RadTrackInvoiceDto>(updated).Returns(resultDto);

            // Act
            var result = await _sut.UpdateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.InvoiceCounter.Should().Be(7);

            await _mockRepository.Received(1).GetByIdAsync(7);
            await _mockRepository.Received(1).UpdateAsync(existing);
        }

        [Fact]
        public async Task UpdateAsync_NullDto_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateAsync(null!));
        }

        [Fact]
        public async Task UpdateAsync_ZeroInvoiceCounter_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = ValidDto(0); // InvoiceCounter = 0 is invalid for update

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));
            ex.Errors.Should().Contain(e => e.Code == "INVOICE_COUNTER_REQUIRED");
        }

        [Fact]
        public async Task UpdateAsync_MissingProject_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = ValidDto(3);
            dto.Project = null;

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));
            ex.Errors.Should().Contain(e => e.Code == "PROJECT_REQUIRED");
        }

        [Fact]
        public async Task UpdateAsync_RecordNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = ValidDto(999);
            _mockRepository.GetByIdAsync(999).Returns((RadTrackInvoice?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateAsync(dto));
        }

        [Fact]
        public async Task UpdateAsync_DuplicateInvoiceRef_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto      = ValidDto(5);
            var existing = ValidEntity(5);

            _mockRepository.GetByIdAsync(5).Returns(existing);
            _mockRepository.ExistsAsync(dto.Project, dto.Contract, dto.InvoiceRef, 5).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));
            ex.Errors.Should().Contain(e => e.Code == "INVOICE_REF_DUPLICATE");
        }

        #endregion

        // ── DeleteAsync ────────────────────────────────────────────────────────

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ExistingRecord_ReturnsTrue()
        {
            // Arrange
            _mockRepository.DeleteAsync(1).Returns(true);

            // Act
            var result = await _sut.DeleteAsync(1);

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteAsync(1);
        }

        [Fact]
        public async Task DeleteAsync_NonExistentRecord_ReturnsFalse()
        {
            // Arrange
            _mockRepository.DeleteAsync(999).Returns(false);

            // Act
            var result = await _sut.DeleteAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        // ── GetTotalsAsync ─────────────────────────────────────────────────────

        #region GetTotalsAsync

        [Fact]
        public async Task GetTotalsAsync_WithFilter_ReturnsMappedTotalsDto()
        {
            // Arrange
            var filter = new RadTrackInvoiceFilter { Project = "PP001" };
            var totals = SampleTotals();
            var totalsDto = SampleTotalsDto();

            _mockRepository.GetTotalsAsync(filter).Returns(totals);
            _mockMapper.Map<RadTrackInvoiceTotalsDto>(totals).Returns(totalsDto);

            // Act
            var result = await _sut.GetTotalsAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.TotalPlannedAmount.Should().Be(5000.0);
            result.TotalDueAmount.Should().Be(3000.0);
            result.TotalActualAmount.Should().Be(2500.0);

            await _mockRepository.Received(1).GetTotalsAsync(filter);
            _mockMapper.Received(1).Map<RadTrackInvoiceTotalsDto>(totals);
        }

        [Fact]
        public async Task GetTotalsAsync_WithNullFilter_ReturnsMappedZeroTotals()
        {
            // Arrange
            var zeroTotals = new RadTrackInvoiceTotals();
            var zeroDto    = new RadTrackInvoiceTotalsDto();

            _mockRepository.GetTotalsAsync(null).Returns(zeroTotals);
            _mockMapper.Map<RadTrackInvoiceTotalsDto>(zeroTotals).Returns(zeroDto);

            // Act
            var result = await _sut.GetTotalsAsync(null);

            // Assert
            result.Should().NotBeNull();
            await _mockRepository.Received(1).GetTotalsAsync(null);
        }

        #endregion

        // ── ExistsAsync ────────────────────────────────────────────────────────

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_WhenDuplicateExists_ReturnsTrue()
        {
            // Arrange
            _mockRepository.ExistsAsync("PP001", "C001", "INV-001", null).Returns(true);

            // Act
            var result = await _sut.ExistsAsync("PP001", "C001", "INV-001");

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).ExistsAsync("PP001", "C001", "INV-001", null);
        }

        [Fact]
        public async Task ExistsAsync_WhenNoDuplicate_ReturnsFalse()
        {
            // Arrange
            _mockRepository.ExistsAsync("PP001", "C001", "INV-NEW", null).Returns(false);

            // Act
            var result = await _sut.ExistsAsync("PP001", "C001", "INV-NEW");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsAsync_WithExcludeCounter_PassesExcludeToRepository()
        {
            // Arrange
            _mockRepository.ExistsAsync("PP001", "C001", "INV-001", 5).Returns(false);

            // Act
            var result = await _sut.ExistsAsync("PP001", "C001", "INV-001", excludeInvoiceCounter: 5);

            // Assert
            result.Should().BeFalse();
            await _mockRepository.Received(1).ExistsAsync("PP001", "C001", "INV-001", 5);
        }

        #endregion
    }
}
