/*
 * TRANSFORMENGINE MIGRATION — CapsStaffServiceTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New xUnit test class for Apha.Costbook.Application.Services.CapsStaffService
 *   - Tests GetAllAsync, GetPaginatedAsync, GetByMNumberAsync, AddAsync, UpdateAsync, DeleteAsync
 *   - Uses NSubstitute for ICapsStaffRepository and IMapper
 *
 * PRESERVED:
 *   - Test naming convention: [MethodName]_[StateUnderTest]_[ExpectedResult]
 *   - Business guard behaviour (ArgumentException, KeyNotFoundException) tested explicitly
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: none — fully automated.
 */

using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Services;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.Costbook.Application.UnitTests.Services.CapsStaffServiceTest
{
    public class CapsStaffServiceTests
    {
        private readonly ICapsStaffRepository _repository;
        private readonly IMapper _mapper;
        private readonly CapsStaffService _service;

        public CapsStaffServiceTests()
        {
            _repository = Substitute.For<ICapsStaffRepository>();
            _mapper = Substitute.For<IMapper>();
            _service = new CapsStaffService(_repository, _mapper);
        }

        // ── GetAllAsync ───────────────────────────────────────────────────────

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_RepositoryReturnsEntities_ReturnsMappedDtos()
        {
            // Arrange
            var entities = new List<CapsStaff>
            {
                new CapsStaff { MNumber = "M001", Name = "Alice" },
                new CapsStaff { MNumber = "M002", Name = "Bob" }
            };
            var dtos = new List<CapsStaffDto>
            {
                new CapsStaffDto { MNumber = "M001", Name = "Alice" },
                new CapsStaffDto { MNumber = "M002", Name = "Bob" }
            };
            _repository.GetAllAsync().Returns(entities);
            _mapper.Map<List<CapsStaffDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            await _repository.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAllAsync_RepositoryReturnsEmpty_ReturnsEmptyList()
        {
            // Arrange
            var entities = new List<CapsStaff>();
            var dtos = new List<CapsStaffDto>();
            _repository.GetAllAsync().Returns(entities);
            _mapper.Map<List<CapsStaffDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        // ── GetPaginatedAsync ─────────────────────────────────────────────────

        #region GetPaginatedAsync Tests

        [Fact]
        public async Task GetPaginatedAsync_ValidParameters_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var coreParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PagedData<CapsStaff>(
                new List<CapsStaff> { new CapsStaff { MNumber = "M001", Name = "Alice" } },
                new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 });
            var paginatedResult = new PaginatedResult<CapsStaffDto>(
                new List<CapsStaffDto> { new CapsStaffDto { MNumber = "M001", Name = "Alice" } },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 });

            _mapper.Map<PaginationParameters<string>>(queryParameters).Returns(coreParams);
            _repository.GetPaginatedAsync(coreParams).Returns(pagedData);
            _mapper.Map<PaginatedResult<CapsStaffDto>>(pagedData).Returns(paginatedResult);

            // Act
            var result = await _service.GetPaginatedAsync(queryParameters);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            await _repository.Received(1).GetPaginatedAsync(coreParams);
        }

        [Fact]
        public async Task GetPaginatedAsync_NullParameters_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetPaginatedAsync(null!));
            await _repository.DidNotReceive().GetPaginatedAsync(Arg.Any<PaginationParameters<string>>());
        }

        #endregion

        // ── GetByMNumberAsync ─────────────────────────────────────────────────

        #region GetByMNumberAsync Tests

        [Fact]
        public async Task GetByMNumberAsync_ExistingMNumber_ReturnsMappedDto()
        {
            // Arrange
            var mNumber = "M001";
            var entity = new CapsStaff { MNumber = mNumber, Name = "Alice" };
            var dto = new CapsStaffDto { MNumber = mNumber, Name = "Alice" };
            _repository.GetByMNumberAsync(mNumber).Returns(entity);
            _mapper.Map<CapsStaffDto>(entity).Returns(dto);

            // Act
            var result = await _service.GetByMNumberAsync(mNumber);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mNumber, result!.MNumber);
        }

        [Fact]
        public async Task GetByMNumberAsync_NonExistentMNumber_ReturnsNull()
        {
            // Arrange
            var mNumber = "NOTEXIST";
            _repository.GetByMNumberAsync(mNumber).Returns((CapsStaff?)null);

            // Act
            var result = await _service.GetByMNumberAsync(mNumber);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByMNumberAsync_NullMNumber_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByMNumberAsync(null!));
            await _repository.DidNotReceive().GetByMNumberAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetByMNumberAsync_WhitespaceMNumber_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByMNumberAsync("   "));
        }

        #endregion

        // ── AddAsync ──────────────────────────────────────────────────────────

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_ValidDto_AddsAndReturnsMappedDto()
        {
            // Arrange
            var dto = new CapsStaffDto { MNumber = "M003", Name = "Charlie" };
            var entity = new CapsStaff { MNumber = "M003", Name = "Charlie" };
            var created = new CapsStaff { MNumber = "M003", Name = "Charlie" };
            var createdDto = new CapsStaffDto { MNumber = "M003", Name = "Charlie" };
            _repository.ExistsAsync("M003").Returns(false);
            _mapper.Map<CapsStaff>(dto).Returns(entity);
            _repository.AddAsync(entity).Returns(created);
            _mapper.Map<CapsStaffDto>(created).Returns(createdDto);

            // Act
            var result = await _service.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("M003", result.MNumber);
            await _repository.Received(1).AddAsync(entity);
        }

        [Fact]
        public async Task AddAsync_DuplicateMNumber_ThrowsArgumentException()
        {
            // Arrange
            var dto = new CapsStaffDto { MNumber = "M001", Name = "Duplicate" };
            _repository.ExistsAsync("M001").Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(dto));
            await _repository.DidNotReceive().AddAsync(Arg.Any<CapsStaff>());
        }

        [Fact]
        public async Task AddAsync_NullDto_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(null!));
        }

        [Fact]
        public async Task AddAsync_EmptyMNumber_ThrowsArgumentException()
        {
            // Arrange
            var dto = new CapsStaffDto { MNumber = "", Name = "Alice" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(dto));
        }

        [Fact]
        public async Task AddAsync_EmptyName_ThrowsArgumentException()
        {
            // Arrange
            var dto = new CapsStaffDto { MNumber = "M003", Name = "" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(dto));
        }

        #endregion

        // ── UpdateAsync ───────────────────────────────────────────────────────

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ExistingMNumber_UpdatesAndReturnsMappedDto()
        {
            // Arrange
            var mNumber = "M001";
            var dto = new CapsStaffDto { MNumber = mNumber, Name = "Alice Updated" };
            var entity = new CapsStaff { MNumber = mNumber, Name = "Alice Updated" };
            var updated = new CapsStaff { MNumber = mNumber, Name = "Alice Updated" };
            var updatedDto = new CapsStaffDto { MNumber = mNumber, Name = "Alice Updated" };
            _repository.ExistsAsync(mNumber).Returns(true);
            _mapper.Map<CapsStaff>(dto).Returns(entity);
            _repository.UpdateAsync(entity).Returns(updated);
            _mapper.Map<CapsStaffDto>(updated).Returns(updatedDto);

            // Act
            var result = await _service.UpdateAsync(mNumber, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mNumber, result.MNumber);
            await _repository.Received(1).UpdateAsync(entity);
        }

        [Fact]
        public async Task UpdateAsync_NonExistentMNumber_ThrowsKeyNotFoundException()
        {
            // Arrange
            var mNumber = "NOTEXIST";
            var dto = new CapsStaffDto { MNumber = mNumber, Name = "Ghost" };
            _repository.ExistsAsync(mNumber).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(mNumber, dto));
        }

        [Fact]
        public async Task UpdateAsync_NullMNumber_ThrowsArgumentException()
        {
            // Arrange
            var dto = new CapsStaffDto { MNumber = "M001", Name = "Alice" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(null!, dto));
        }

        [Fact]
        public async Task UpdateAsync_NullDto_ThrowsArgumentException()
        {
            // Arrange
            var mNumber = "M001";
            _repository.ExistsAsync(mNumber).Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(mNumber, null!));
        }

        #endregion

        // ── DeleteAsync ───────────────────────────────────────────────────────

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ExistingMNumber_CallsRepositoryDelete()
        {
            // Arrange
            var mNumber = "M001";
            _repository.ExistsAsync(mNumber).Returns(true);
            _repository.DeleteAsync(mNumber).Returns(true);

            // Act
            await _service.DeleteAsync(mNumber);

            // Assert
            await _repository.Received(1).DeleteAsync(mNumber);
        }

        [Fact]
        public async Task DeleteAsync_NonExistentMNumber_ThrowsKeyNotFoundException()
        {
            // Arrange
            var mNumber = "NOTEXIST";
            _repository.ExistsAsync(mNumber).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(mNumber));
            await _repository.DidNotReceive().DeleteAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteAsync_NullMNumber_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteAsync(null!));
        }

        #endregion
    }
}
