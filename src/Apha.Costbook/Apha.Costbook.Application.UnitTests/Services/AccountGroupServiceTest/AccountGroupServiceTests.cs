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
                new AccountGroupDto { Csg7group = "CSG001", Useinflation = true },
                new AccountGroupDto { Csg7group = "CSG002", Useinflation = false }
            };
            _repository.GetAllAccountGroupAsync().Returns(entities);
            _mapper.Map<List<AccountGroupDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetAllAccountGroupAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            await _repository.Received(1).GetAllAccountGroupAsync();
        }

        [Fact]
        public async Task GetAllAsync_RepositoryReturnsEmpty_ReturnsEmptyList()
        {
            // Arrange
            var entities = new List<AccountGroup>();
            var dtos = new List<AccountGroupDto>();
            _repository.GetAllAccountGroupAsync().Returns(entities);
            _mapper.Map<List<AccountGroupDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetAllAccountGroupAsync();

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
            var dto = new AccountGroupDto { Csg7group = key, Useinflation = true };
            _repository.GetByCsg7GroupAsync(key).Returns(entity);
            _mapper.Map<AccountGroupDto>(entity).Returns(dto);

            // Act
            var result = await _service.GetByCsg7GroupAsync(key);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(key, result!.Csg7group);
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
            var dto = new AccountGroupDto { Csg7group = "CSG003", Useinflation = true };
            var entity = new AccountGroup { Csg7group = "CSG003", Useinflation = true };
            var created = new AccountGroup { Csg7group = "CSG003", Useinflation = true };
            var createdDto = new AccountGroupDto { Csg7group = "CSG003", Useinflation = true };
            _repository.ExistsAsync("CSG003").Returns(false);
            _mapper.Map<AccountGroup>(dto).Returns(entity);
            _repository.AddAccountGroupAsync(entity).Returns(created);
            _mapper.Map<AccountGroupDto>(created).Returns(createdDto);

            // Act
            var result = await _service.AddAccountGroupAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CSG003", result.Csg7group);
            await _repository.Received(1).AddAccountGroupAsync(entity);
        }

        [Fact]
        public async Task AddAsync_DuplicateKey_ThrowsArgumentException()
        {
            // Arrange
            var dto = new AccountGroupDto { Csg7group = "CSG001", Useinflation = true };
            _repository.ExistsAsync("CSG001").Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAccountGroupAsync(dto));
            await _repository.DidNotReceive().AddAccountGroupAsync(Arg.Any<AccountGroup>());
        }

        [Fact]
        public async Task AddAsync_NullDto_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAccountGroupAsync(null!));
        }

        [Fact]
        public async Task AddAsync_EmptyKey_ThrowsArgumentException()
        {
            // Arrange
            var dto = new AccountGroupDto { Csg7group = "", Useinflation = true };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAccountGroupAsync(dto));
        }

        #endregion

        // ── UpdateAsync ───────────────────────────────────────────────────────

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ExistingKey_UpdatesAndReturnsMappedDto()
        {
            // Arrange
            var key = "CSG001";
            var dto = new AccountGroupDto { Csg7group = key, Useinflation = false };
            var entity = new AccountGroup { Csg7group = key, Useinflation = false };
            var updated = new AccountGroup { Csg7group = key, Useinflation = false };
            var updatedDto = new AccountGroupDto { Csg7group = key, Useinflation = false };
            _repository.ExistsAsync(key).Returns(true);
            _mapper.Map<AccountGroup>(dto).Returns(entity);
            _repository.UpdateAccountGroupAsync(entity).Returns(updated);
            _mapper.Map<AccountGroupDto>(updated).Returns(updatedDto);

            // Act
            var result = await _service.UpdateAccountGroupAsync(key, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(key, result.Csg7group);
            await _repository.Received(1).UpdateAccountGroupAsync(entity);
        }

        [Fact]
        public async Task UpdateAsync_NonExistentKey_ThrowsKeyNotFoundException()
        {
            // Arrange
            var key = "NOTEXIST";
            var dto = new AccountGroupDto { Csg7group = key, Useinflation = true };
            _repository.ExistsAsync(key).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAccountGroupAsync(key, dto));
        }

        [Fact]
        public async Task UpdateAsync_NullKey_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateAccountGroupAsync(null!, new AccountGroupDto { Csg7group = "CSG001", Useinflation = true }));
        }

        [Fact]
        public async Task UpdateAsync_NullDto_ThrowsArgumentException()
        {
            // Arrange
            var key = "CSG001";
            _repository.ExistsAsync(key).Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAccountGroupAsync(key, null!));
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
            _repository.DeleteAccountGroupAsync(key).Returns(true);

            // Act
            await _service.DeleteAccountGroupAsync(key);

            // Assert
            await _repository.Received(1).DeleteAccountGroupAsync(key);
        }

        [Fact]
        public async Task DeleteAsync_NonExistentKey_ThrowsKeyNotFoundException()
        {
            // Arrange
            var key = "NOTEXIST";
            _repository.ExistsAsync(key).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAccountGroupAsync(key));
            await _repository.DidNotReceive().DeleteAccountGroupAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteAsync_NullKey_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteAccountGroupAsync(null!));
        }

        #endregion
    }
}
