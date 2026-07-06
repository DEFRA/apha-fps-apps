/*
 * TRANSFORMENGINE MIGRATION — ProjectManagerServiceTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New xUnit test class for Apha.PIMS.Application.Services.ProjectManagerService
 *   - Natural varchar PK (projectmanager name string)
 *   - Covers: GetAllAsync, GetByIdAsync, CreateAsync (dup-guard), UpdateAsync, DeleteAsync, ExistsAsync
 *   - Uses NSubstitute for IProjectManagerRepository and IMapper
 *
 * PRESERVED:
 *   - Duplicate-name guard: InvalidOperationException when manager name already exists
 *   - Not-found guard: KeyNotFoundException on update/delete
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Services;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Application.UnitTests.Services.ProjectManagerServiceTest
{
    public class ProjectManagerServiceTests
    {
        private readonly IProjectManagerRepository _repository;
        private readonly IMapper _mapper;
        private readonly ProjectManagerService _service;

        public ProjectManagerServiceTests()
        {
            _repository = Substitute.For<IProjectManagerRepository>();
            _mapper     = Substitute.For<IMapper>();
            _service    = new ProjectManagerService(_repository, _mapper);
        }

        // ── helpers ───────────────────────────────────────────────────────────────

        private static ProjectManager MakeEntity(string name = "J. Smith") =>
            new ProjectManager { Projectmanager = name, Email = "j.smith@apha.gov.uk", Disable = false };

        private static ProjectManagerDto MakeDto(string name = "J. Smith") =>
            new ProjectManagerDto { Projectmanager = name, Email = "j.smith@apha.gov.uk", Disable = false };

        // ── Constructor ───────────────────────────────────────────────────────────

        #region Constructor

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProjectManagerService(null!, _mapper));
        }

        [Fact]
        public void Constructor_NullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProjectManagerService(_repository, null!));
        }

        #endregion

        // ── GetAllAsync ───────────────────────────────────────────────────────────

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_RepositoryReturnsEntities_ReturnsMappedDtoList()
        {
            // Arrange
            var entities = new List<ProjectManager> { MakeEntity("Smith"), MakeEntity("Jones") };
            var dtos     = new List<ProjectManagerDto> { MakeDto("Smith"), MakeDto("Jones") };
            _repository.GetAllAsync().Returns(entities);
            _mapper.Map<List<ProjectManagerDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count);
            await _repository.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAllAsync_RepositoryReturnsEmpty_ReturnsEmptyList()
        {
            // Arrange
            _repository.GetAllAsync().Returns(new List<ProjectManager>());
            _mapper.Map<List<ProjectManagerDto>>(Arg.Any<List<ProjectManager>>()).Returns(new List<ProjectManagerDto>());

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _repository.GetAllAsync().ThrowsAsync(new InvalidOperationException("db error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetAllAsync());
        }

        #endregion

        // ── GetByIdAsync ──────────────────────────────────────────────────────────

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_EntityExists_ReturnsMappedDto()
        {
            // Arrange
            const string name = "J. Smith";
            var entity = MakeEntity(name);
            var dto    = MakeDto(name);
            _repository.GetByIdAsync(name).Returns(entity);
            _mapper.Map<ProjectManagerDto>(entity).Returns(dto);

            // Act
            var result = await _service.GetByIdAsync(name);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(name, result!.Projectmanager);
        }

        [Fact]
        public async Task GetByIdAsync_EntityNotFound_ReturnsNull()
        {
            // Arrange
            _repository.GetByIdAsync(Arg.Any<string>()).Returns((ProjectManager?)null);

            // Act
            var result = await _service.GetByIdAsync("Unknown");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_EmptyName_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByIdAsync(""));
        }

        [Fact]
        public async Task GetByIdAsync_WhitespaceName_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByIdAsync("   "));
        }

        #endregion

        // ── CreateAsync ───────────────────────────────────────────────────────────

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ValidDto_ReturnsMappedCreatedDto()
        {
            // Arrange
            const string name = "New Manager";
            var dto     = MakeDto(name);
            var entity  = MakeEntity(name);
            var created = MakeEntity(name);
            var result_dto = MakeDto(name);
            _repository.ExistsAsync(name).Returns(false);
            _mapper.Map<ProjectManager>(dto).Returns(entity);
            _repository.AddAsync(entity).Returns(created);
            _mapper.Map<ProjectManagerDto>(created).Returns(result_dto);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(name, result.Projectmanager);
            await _repository.Received(1).AddAsync(entity);
        }

        [Fact]
        public async Task CreateAsync_DuplicateName_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = MakeDto("Existing Manager");
            _repository.ExistsAsync("Existing Manager").Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_NullDto_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateAsync(null!));
        }

        [Fact]
        public async Task CreateAsync_EmptyName_ThrowsArgumentException()
        {
            var dto = new ProjectManagerDto { Projectmanager = "" };
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_DoesNotCallAdd_WhenNameAlreadyExists()
        {
            // Arrange
            var dto = MakeDto("Existing");
            _repository.ExistsAsync("Existing").Returns(true);

            // Act + ignore exception
            try { await _service.CreateAsync(dto); } catch { }

            // Assert
            await _repository.DidNotReceive().AddAsync(Arg.Any<ProjectManager>());
        }

        #endregion

        // ── UpdateAsync ───────────────────────────────────────────────────────────

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_EntityExists_ReturnsMappedUpdatedDto()
        {
            // Arrange
            const string name = "J. Smith";
            var dto     = MakeDto(name);
            var entity  = MakeEntity(name);
            var updated = MakeEntity(name);
            var result_dto = MakeDto(name);
            _repository.ExistsAsync(name).Returns(true);
            _mapper.Map<ProjectManager>(dto).Returns(entity);
            _repository.UpdateAsync(entity).Returns(updated);
            _mapper.Map<ProjectManagerDto>(updated).Returns(result_dto);

            // Act
            var result = await _service.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            await _repository.Received(1).UpdateAsync(entity);
        }

        [Fact]
        public async Task UpdateAsync_EntityNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _repository.ExistsAsync(Arg.Any<string>()).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(MakeDto("Unknown")));
        }

        [Fact]
        public async Task UpdateAsync_NullDto_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateAsync(null!));
        }

        [Fact]
        public async Task UpdateAsync_EmptyName_ThrowsArgumentException()
        {
            var dto = new ProjectManagerDto { Projectmanager = "" };
            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(dto));
        }

        #endregion

        // ── DeleteAsync ───────────────────────────────────────────────────────────

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_EntityExists_CallsRepositoryDelete()
        {
            // Arrange
            const string name = "J. Smith";
            _repository.ExistsAsync(name).Returns(true);
            _repository.DeleteAsync(name).Returns(Task.CompletedTask);

            // Act
            await _service.DeleteAsync(name);

            // Assert
            await _repository.Received(1).DeleteAsync(name);
        }

        [Fact]
        public async Task DeleteAsync_EntityNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _repository.ExistsAsync(Arg.Any<string>()).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync("Unknown"));
        }

        [Fact]
        public async Task DeleteAsync_EmptyName_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteAsync(""));
        }

        #endregion

        // ── ExistsAsync ───────────────────────────────────────────────────────────

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_EntityExists_ReturnsTrue()
        {
            // Arrange
            _repository.ExistsAsync("J. Smith").Returns(true);

            // Act
            var result = await _service.ExistsAsync("J. Smith");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_EntityNotFound_ReturnsFalse()
        {
            // Arrange
            _repository.ExistsAsync(Arg.Any<string>()).Returns(false);

            // Act
            var result = await _service.ExistsAsync("Unknown");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_EmptyName_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ExistsAsync(""));
        }

        #endregion
    }
}
