/*
 * TRANSFORMENGINE MIGRATION — AccountGroupServiceTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New xUnit test class for Apha.Costbook.Application.Services.AccountGroupService
 *   - Tests GetAllAsync, GetByCsg7GroupAsync, AddAsync, UpdateAsync, DeleteAsync
 *   - Uses NSubstitute for IAccountGroupRepository and IMapper
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
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.Costbook.Application.UnitTests.Services.AccountGroupServiceTest
{
    public class AccountGroupServiceTests
    {
        private readonly IAccountGroupRepository _repository;
        private readonly IMapper _mapper;
        private readonly AccountGroupService _service;

        public AccountGroupServiceTests()
        {
            _repository = Substitute.For<IAccountGroupRepository>();
            _mapper = Substitute.For<IMapper>();
            _service = new AccountGroupService(_repository, _mapper);
        }

        // ── GetAllAsync ───────────────────────────────────────────────────────

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_RepositoryReturnsEntities_ReturnsMappedDtos()
        {
            // Arrange
            var entities = new List<AccountGroup>
            {
                new AccountGroup { Csg7group = "CSG001", Useinflation = true },
                new AccountGroup { Csg7group = "CSG002", Useinflation = false }
            };
            var dtos = new List<AccountGroupDto>
            {
                new AccountGroupDto { Csg7Group = "CSG001", UseInflation = true },
                new AccountGroupDto { Csg7Group = "CSG002", UseInflation = false }
            };
            _repository.GetAllAsync().Returns(entities);
            _mapper.Map<List<AccountGroupDto>>(entities).Returns(dtos);

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
            var entities = new List<AccountGroup>();
            var dtos = new List<AccountGroupDto>();
            _repository.GetAllAsync().Returns(entities);
            _mapper.Map<List<AccountGroupDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        // ── GetByCsg7GroupAsync ───────────────────────────────────────────────

        #region GetByCsg7GroupAsync Tests

        [Fact]
        public async Task GetByCsg7GroupAsync_ExistingKey_ReturnsMappedDto()
        {
            // Arrange
            var key = "CSG001";
            var entity = new AccountGroup { Csg7group = key, Useinflation = true };
            var dto = new AccountGroupDto { Csg7Group = key, UseInflation = true };
            _repository.GetByCsg7GroupAsync(key).Returns(entity);
            _mapper.Map<AccountGroupDto>(entity).Returns(dto);

            // Act
            var result = await _service.GetByCsg7GroupAsync(key);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(key, result!.Csg7Group);
        }

        [Fact]
        public async Task GetByCsg7GroupAsync_NonExistentKey_ReturnsNull()
        {
            // Arrange
            var key = "NOTEXIST";
            _repository.GetByCsg7GroupAsync(key).Returns((AccountGroup?)null);

            // Act
            var result = await _service.GetByCsg7GroupAsync(key);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByCsg7GroupAsync_NullKey_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByCsg7GroupAsync(null!));
        }

        [Fact]
        public async Task GetByCsg7GroupAsync_WhitespaceKey_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByCsg7GroupAsync("   "));
        }

        #endregion

        // ── AddAsync ──────────────────────────────────────────────────────────

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_ValidDto_AddsAndReturnsMappedDto()
        {
            // Arrange
            var dto = new AccountGroupDto { Csg7Group = "CSG003", UseInflation = true };
            var entity = new AccountGroup { Csg7group = "CSG003", Useinflation = true };
            var created = new AccountGroup { Csg7group = "CSG003", Useinflation = true };
            var createdDto = new AccountGroupDto { Csg7Group = "CSG003", UseInflation = true };
            _repository.ExistsAsync("CSG003").Returns(false);
            _mapper.Map<AccountGroup>(dto).Returns(entity);
            _repository.AddAsync(entity).Returns(created);
            _mapper.Map<AccountGroupDto>(created).Returns(createdDto);

            // Act
            var result = await _service.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CSG003", result.Csg7Group);
            await _repository.Received(1).AddAsync(entity);
        }

        [Fact]
        public async Task AddAsync_DuplicateKey_ThrowsArgumentException()
        {
            // Arrange
            var dto = new AccountGroupDto { Csg7Group = "CSG001", UseInflation = true };
            _repository.ExistsAsync("CSG001").Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(dto));
            await _repository.DidNotReceive().AddAsync(Arg.Any<AccountGroup>());
        }

        [Fact]
        public async Task AddAsync_NullDto_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(null!));
        }

        [Fact]
        public async Task AddAsync_EmptyKey_ThrowsArgumentException()
        {
            // Arrange
            var dto = new AccountGroupDto { Csg7Group = "", UseInflation = true };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(dto));
        }

        #endregion

        // ── UpdateAsync ───────────────────────────────────────────────────────

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ExistingKey_UpdatesAndReturnsMappedDto()
        {
            // Arrange
            var key = "CSG001";
            var dto = new AccountGroupDto { Csg7Group = key, UseInflation = false };
            var entity = new AccountGroup { Csg7group = key, Useinflation = false };
            var updated = new AccountGroup { Csg7group = key, Useinflation = false };
            var updatedDto = new AccountGroupDto { Csg7Group = key, UseInflation = false };
            _repository.ExistsAsync(key).Returns(true);
            _mapper.Map<AccountGroup>(dto).Returns(entity);
            _repository.UpdateAsync(entity).Returns(updated);
            _mapper.Map<AccountGroupDto>(updated).Returns(updatedDto);

            // Act
            var result = await _service.UpdateAsync(key, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(key, result.Csg7Group);
            await _repository.Received(1).UpdateAsync(entity);
        }

        [Fact]
        public async Task UpdateAsync_NonExistentKey_ThrowsKeyNotFoundException()
        {
            // Arrange
            var key = "NOTEXIST";
            var dto = new AccountGroupDto { Csg7Group = key, UseInflation = true };
            _repository.ExistsAsync(key).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(key, dto));
        }

        [Fact]
        public async Task UpdateAsync_NullKey_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateAsync(null!, new AccountGroupDto { Csg7Group = "CSG001" }));
        }

        [Fact]
        public async Task UpdateAsync_NullDto_ThrowsArgumentException()
        {
            // Arrange
            var key = "CSG001";
            _repository.ExistsAsync(key).Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(key, null!));
        }

        #endregion

        // ── DeleteAsync ───────────────────────────────────────────────────────

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ExistingKey_CallsRepositoryDelete()
        {
            // Arrange
            var key = "CSG001";
            _repository.ExistsAsync(key).Returns(true);
            _repository.DeleteAsync(key).Returns(true);

            // Act
            await _service.DeleteAsync(key);

            // Assert
            await _repository.Received(1).DeleteAsync(key);
        }

        [Fact]
        public async Task DeleteAsync_NonExistentKey_ThrowsKeyNotFoundException()
        {
            // Arrange
            var key = "NOTEXIST";
            _repository.ExistsAsync(key).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(key));
            await _repository.DidNotReceive().DeleteAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteAsync_NullKey_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteAsync(null!));
        }

        #endregion
    }
}
