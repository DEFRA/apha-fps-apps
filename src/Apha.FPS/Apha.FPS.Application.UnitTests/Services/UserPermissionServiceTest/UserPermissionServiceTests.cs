using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.UserPermissionServiceTest
{
    public class UserPermissionServiceTests
    {
        private readonly IUserRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly UserPermissionService _sut;

        public UserPermissionServiceTests()
        {
            _mockRepository = Substitute.For<IUserRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new UserPermissionService(_mockRepository, _mockMapper);
        }

        private static UserDto BuildDto(int userId = 1) =>
            new() { UserId = userId, Username = "testuser", Comments = "Test User", UserEmail = "test@example.com", Dt2Username = "dt2user" };

        private static User BuildEntity(int userId = 1) =>
            new() { UserId = userId, Username = "testuser", Comments = "Test User", UserEmail = "test@example.com", Dt2Username = "dt2user" };

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenRepositoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UserPermissionService(null!, _mockMapper));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UserPermissionService(_mockRepository, null!));
        }

        #endregion

        #region GetAllUsersAsync Tests

        [Fact]
        public async Task GetAllUsersAsync_ReturnsMappedDtos()
        {
            var entities = new List<User> { BuildEntity() };
            var dtos = new List<UserDto> { BuildDto() };

            _mockRepository.GetAllUsersAsync().Returns(entities);
            _mockMapper.Map<IEnumerable<UserDto>>(entities).Returns(dtos);

            var result = await _sut.GetAllUsersAsync();

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsEmpty_WhenRepositoryReturnsEmpty()
        {
            _mockRepository.GetAllUsersAsync().Returns(new List<User>());
            _mockMapper.Map<IEnumerable<UserDto>>(Arg.Any<IEnumerable<User>>())
                .Returns(new List<UserDto>());

            var result = await _sut.GetAllUsersAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllUsersAsync_ThrowsException_WhenRepositoryThrows()
        {
            _mockRepository.GetAllUsersAsync().ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetAllUsersAsync());
        }

        #endregion

        #region GetAllUsersPagedAsync Tests

        [Fact]
        public async Task GetAllUsersPagedAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetAllUsersPagedAsync(null!));
        }

        [Fact]
        public async Task GetAllUsersPagedAsync_ReturnsPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PagedData<User>
            {
                Data = [BuildEntity()],
                PaginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var expected = new PaginatedResult<UserDto>
            {
                Data = [BuildDto()],
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllUsersPagedAsync(paginationParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<UserDto>>(pagedData).Returns(expected);

            var result = await _sut.GetAllUsersPagedAsync(query);

            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetAllUsersPagedAsync_ThrowsException_WhenRepositoryThrows()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _mockRepository.GetAllUsersPagedAsync(Arg.Any<PaginationParameters<string>>())
                .ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetAllUsersPagedAsync(query));
        }

        #endregion

        #region GetNonSuperUsersPagedAsync Tests

        [Fact]
        public async Task GetNonSuperUsersPagedAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetNonSuperUsersPagedAsync(null!));
        }

        [Fact]
        public async Task GetNonSuperUsersPagedAsync_ReturnsPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PagedData<User>
            {
                Data = [BuildEntity()],
                PaginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var expected = new PaginatedResult<UserDto>
            {
                Data = [BuildDto()],
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetNonSuperUsersPagedAsync(paginationParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<UserDto>>(pagedData).Returns(expected);

            var result = await _sut.GetNonSuperUsersPagedAsync(query);

            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetNonSuperUsersPagedAsync_ThrowsException_WhenRepositoryThrows()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _mockRepository.GetNonSuperUsersPagedAsync(Arg.Any<PaginationParameters<string>>())
                .ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetNonSuperUsersPagedAsync(query));
        }

        #endregion

        #region GetUserByIdAsync Tests

        [Fact]
        public async Task GetUserByIdAsync_ReturnsMappedDto_WhenUserExists()
        {
            var entity = BuildEntity();
            var dto = BuildDto();

            _mockRepository.GetUserByIdAsync(1).Returns(entity);
            _mockMapper.Map<UserDto>(entity).Returns(dto);

            var result = await _sut.GetUserByIdAsync(1);

            result.Should().NotBeNull();
            result!.UserId.Should().Be(1);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsNull_WhenUserNotFound()
        {
            _mockRepository.GetUserByIdAsync(999).Returns((User?)null);

            var result = await _sut.GetUserByIdAsync(999);

            result.Should().BeNull();
        }

        #endregion

        #region AddUserAsync Tests

        [Fact]
        public async Task AddUserAsync_ThrowsArgumentNullException_WhenDtoIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.AddUserAsync(null!));
        }

        [Fact]
        public async Task AddUserAsync_ThrowsArgumentException_WhenUsernameIsEmpty()
        {
            var dto = BuildDto();
            dto.Username = "   ";

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddUserAsync(dto));
        }

        [Fact]
        public async Task AddUserAsync_ThrowsInvalidOperationException_WhenUsernameAlreadyExists()
        {
            var dto = BuildDto();
            _mockRepository.GetUserByUsernameAsync("testuser").Returns(BuildEntity());

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AddUserAsync(dto));
        }

        [Fact]
        public async Task AddUserAsync_ThrowsInvalidOperationException_WhenEmailAlreadyExists()
        {
            var dto = BuildDto();
            _mockRepository.GetUserByUsernameAsync("testuser").Returns((User?)null);
            _mockRepository.GetUserByEmailAsync("test@example.com").Returns(BuildEntity(2));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AddUserAsync(dto));
        }

        [Fact]
        public async Task AddUserAsync_ReturnsCreatedUser_WhenValid()
        {
            var dto = BuildDto();
            var entity = BuildEntity();
            var addedEntity = BuildEntity();
            var resultDto = BuildDto();

            _mockRepository.GetUserByUsernameAsync("testuser").Returns((User?)null);
            _mockRepository.GetUserByEmailAsync("test@example.com").Returns((User?)null);
            _mockMapper.Map<User>(dto).Returns(entity);
            _mockRepository.AddUserAsync(entity).Returns(addedEntity);
            _mockMapper.Map<UserDto>(addedEntity).Returns(resultDto);

            var result = await _sut.AddUserAsync(dto);

            result.Should().NotBeNull();
            result.UserId.Should().Be(1);
            await _mockRepository.Received(1).AddUserAsync(entity);
        }

        [Fact]
        public async Task AddUserAsync_TrimsWhitespace_FromAllFields()
        {
            var dto = new UserDto
            {
                UserId = 0,
                Username = "  testuser  ",
                Comments = "  Test User  ",
                UserEmail = "  test@example.com  ",
                Dt2Username = "  dt2user  "
            };
            var entity = BuildEntity();
            var addedEntity = BuildEntity();
            var resultDto = BuildDto();

            _mockRepository.GetUserByUsernameAsync("testuser").Returns((User?)null);
            _mockRepository.GetUserByEmailAsync("test@example.com").Returns((User?)null);
            _mockMapper.Map<User>(Arg.Is<UserDto>(d =>
                d.Username == "testuser" &&
                d.Comments == "Test User" &&
                d.UserEmail == "test@example.com" &&
                d.Dt2Username == "dt2user")).Returns(entity);
            _mockRepository.AddUserAsync(entity).Returns(addedEntity);
            _mockMapper.Map<UserDto>(addedEntity).Returns(resultDto);

            await _sut.AddUserAsync(dto);

            dto.Username.Should().Be("testuser");
            dto.Comments.Should().Be("Test User");
            dto.UserEmail.Should().Be("test@example.com");
            dto.Dt2Username.Should().Be("dt2user");
        }

        [Fact]
        public async Task AddUserAsync_SkipsEmailCheck_WhenEmailIsNullOrEmpty()
        {
            var dto = BuildDto();
            dto.UserEmail = null;
            var entity = BuildEntity();
            var addedEntity = BuildEntity();
            var resultDto = BuildDto();

            _mockRepository.GetUserByUsernameAsync("testuser").Returns((User?)null);
            _mockMapper.Map<User>(dto).Returns(entity);
            _mockRepository.AddUserAsync(entity).Returns(addedEntity);
            _mockMapper.Map<UserDto>(addedEntity).Returns(resultDto);

            await _sut.AddUserAsync(dto);

            await _mockRepository.DidNotReceive().GetUserByEmailAsync(Arg.Any<string>());
        }

        #endregion

        #region UpdateUserAsync Tests

        [Fact]
        public async Task UpdateUserAsync_ThrowsArgumentNullException_WhenDtoIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.UpdateUserAsync(null!));
        }

        [Fact]
        public async Task UpdateUserAsync_ThrowsArgumentException_WhenUsernameIsEmpty()
        {
            var dto = BuildDto();
            dto.Username = "";

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateUserAsync(dto));
        }

        [Fact]
        public async Task UpdateUserAsync_ThrowsKeyNotFoundException_WhenUserNotFound()
        {
            var dto = BuildDto();
            _mockRepository.GetUserByIdAsync(1).Returns((User?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateUserAsync(dto));
        }

        [Fact]
        public async Task UpdateUserAsync_ThrowsInvalidOperationException_WhenUsernameAlreadyTakenByAnother()
        {
            var dto = BuildDto();
            _mockRepository.GetUserByIdAsync(1).Returns(BuildEntity());
            _mockRepository.GetUserByUsernameAsync("testuser").Returns(BuildEntity(2));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateUserAsync(dto));
        }

        [Fact]
        public async Task UpdateUserAsync_ThrowsInvalidOperationException_WhenEmailAlreadyTakenByAnother()
        {
            var dto = BuildDto();
            _mockRepository.GetUserByIdAsync(1).Returns(BuildEntity());
            _mockRepository.GetUserByUsernameAsync("testuser").Returns(BuildEntity(1));
            _mockRepository.GetUserByEmailAsync("test@example.com").Returns(BuildEntity(2));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateUserAsync(dto));
        }

        [Fact]
        public async Task UpdateUserAsync_ReturnsUpdatedUser_WhenValid()
        {
            var dto = BuildDto();
            var entity = BuildEntity();
            var updatedEntity = BuildEntity();
            var resultDto = BuildDto();

            _mockRepository.GetUserByIdAsync(1).Returns(BuildEntity());
            _mockRepository.GetUserByUsernameAsync("testuser").Returns(BuildEntity(1));
            _mockRepository.GetUserByEmailAsync("test@example.com").Returns(BuildEntity(1));
            _mockMapper.Map<User>(dto).Returns(entity);
            _mockRepository.UpdateUserAsync(entity).Returns(updatedEntity);
            _mockMapper.Map<UserDto>(updatedEntity).Returns(resultDto);

            var result = await _sut.UpdateUserAsync(dto);

            result.Should().NotBeNull();
            result.UserId.Should().Be(1);
            await _mockRepository.Received(1).UpdateUserAsync(entity);
        }

        [Fact]
        public async Task UpdateUserAsync_TrimsWhitespace_FromAllFields()
        {
            var dto = new UserDto
            {
                UserId = 1,
                Username = "  testuser  ",
                Comments = "  Test User  ",
                UserEmail = "  test@example.com  ",
                Dt2Username = "  dt2user  "
            };
            var entity = BuildEntity();
            var updatedEntity = BuildEntity();
            var resultDto = BuildDto();

            _mockRepository.GetUserByIdAsync(1).Returns(BuildEntity());
            _mockRepository.GetUserByUsernameAsync("testuser").Returns(BuildEntity(1));
            _mockRepository.GetUserByEmailAsync("test@example.com").Returns(BuildEntity(1));
            _mockMapper.Map<User>(Arg.Any<UserDto>()).Returns(entity);
            _mockRepository.UpdateUserAsync(entity).Returns(updatedEntity);
            _mockMapper.Map<UserDto>(updatedEntity).Returns(resultDto);

            await _sut.UpdateUserAsync(dto);

            dto.Username.Should().Be("testuser");
            dto.Comments.Should().Be("Test User");
            dto.UserEmail.Should().Be("test@example.com");
            dto.Dt2Username.Should().Be("dt2user");
        }

        #endregion

        #region DeleteUserAsync Tests

        [Fact]
        public async Task DeleteUserAsync_ReturnsTrue_WhenUserDeleted()
        {
            _mockRepository.DeleteUserAsync(1).Returns(true);

            var result = await _sut.DeleteUserAsync(1);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteUserAsync_ReturnsFalse_WhenUserNotFound()
        {
            _mockRepository.DeleteUserAsync(999).Returns(false);

            var result = await _sut.DeleteUserAsync(999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteUserAsync_ThrowsException_WhenRepositoryThrows()
        {
            _mockRepository.DeleteUserAsync(1).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.DeleteUserAsync(1));
        }

        #endregion

        #region GetUserPermissionsAsync Tests

        [Fact]
        public async Task GetUserPermissionsAsync_ReturnsAllPermissions()
        {
            _mockRepository.GetUserProfitCentresAsync(1).Returns(["PC1", "PC2"]);
            _mockRepository.GetUserProgramsAsync(1).Returns(["P1"]);
            _mockRepository.GetUserCategoriesAsync(1).Returns(["C1", "C2"]);
            _mockRepository.GetUserTestOwnersAsync(1).Returns(["T1"]);
            _mockRepository.GetUserProjectGroupsAsync(1).Returns(["PG1"]);

            var result = await _sut.GetUserPermissionsAsync(1);

            result.Should().NotBeNull();
            result.UserId.Should().Be(1);
            result.ProfitCentres.Should().HaveCount(2);
            result.Programs.Should().HaveCount(1);
            result.Categories.Should().HaveCount(2);
            result.TestOwners.Should().HaveCount(1);
            result.ProjectGroups.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetUserPermissionsAsync_ReturnsEmptyLists_WhenNoPermissions()
        {
            _mockRepository.GetUserProfitCentresAsync(1).Returns([]);
            _mockRepository.GetUserProgramsAsync(1).Returns([]);
            _mockRepository.GetUserCategoriesAsync(1).Returns([]);
            _mockRepository.GetUserTestOwnersAsync(1).Returns([]);
            _mockRepository.GetUserProjectGroupsAsync(1).Returns([]);

            var result = await _sut.GetUserPermissionsAsync(1);

            result.ProfitCentres.Should().BeEmpty();
            result.Programs.Should().BeEmpty();
            result.Categories.Should().BeEmpty();
            result.TestOwners.Should().BeEmpty();
            result.ProjectGroups.Should().BeEmpty();
        }

        #endregion

        #region SaveUserPermissionsAsync Tests

        [Fact]
        public async Task SaveUserPermissionsAsync_ThrowsArgumentNullException_WhenDtoIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.SaveUserPermissionsAsync(null!));
        }

        [Fact]
        public async Task SaveUserPermissionsAsync_CallsRepositoryWithCorrectData()
        {
            var dto = new UserPermissionDto
            {
                UserId = 1,
                ProfitCentres = ["PC1"],
                Programs = ["P1"],
                Categories = ["C1"],
                TestOwners = ["T1"],
                ProjectGroups = ["PG1"]
            };

            await _sut.SaveUserPermissionsAsync(dto);

            await _mockRepository.Received(1).SaveUserPermissionsAsync(
                1,
                Arg.Is<List<string>>(l => l.Contains("PC1")),
                Arg.Is<List<string>>(l => l.Contains("P1")),
                Arg.Is<List<string>>(l => l.Contains("C1")),
                Arg.Is<List<string>>(l => l.Contains("T1")),
                Arg.Is<List<string>>(l => l.Contains("PG1")));
        }

        [Fact]
        public async Task SaveUserPermissionsAsync_ThrowsException_WhenRepositoryThrows()
        {
            var dto = new UserPermissionDto { UserId = 1 };
            _mockRepository.SaveUserPermissionsAsync(
                Arg.Any<int>(), Arg.Any<List<string>>(), Arg.Any<List<string>>(),
                Arg.Any<List<string>>(), Arg.Any<List<string>>(), Arg.Any<List<string>>())
                .ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.SaveUserPermissionsAsync(dto));
        }

        #endregion

        #region GetPermissionOptionsAsync Tests

        [Fact]
        public async Task GetPermissionOptionsAsync_ReturnsAllOptions()
        {
            _mockRepository.GetAllProfitCentreOptionsAsync().Returns(["PC1", "PC2"]);
            _mockRepository.GetAllProgramOptionsAsync().Returns(["P1", "P2"]);
            _mockRepository.GetAllCategoryOptionsAsync().Returns(["C1"]);
            _mockRepository.GetAllTestOwnerOptionsAsync().Returns(["T1"]);
            _mockRepository.GetAllProjectGroupOptionsAsync().Returns(["PG1", "PG2"]);

            var result = await _sut.GetPermissionOptionsAsync();

            result.Should().NotBeNull();
            result.ProfitCentres.Should().HaveCount(2);
            result.Programs.Should().HaveCount(2);
            result.Categories.Should().HaveCount(1);
            result.TestOwners.Should().HaveCount(1);
            result.ProjectGroups.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetPermissionOptionsAsync_ReturnsEmptyLists_WhenNoOptions()
        {
            _mockRepository.GetAllProfitCentreOptionsAsync().Returns([]);
            _mockRepository.GetAllProgramOptionsAsync().Returns([]);
            _mockRepository.GetAllCategoryOptionsAsync().Returns([]);
            _mockRepository.GetAllTestOwnerOptionsAsync().Returns([]);
            _mockRepository.GetAllProjectGroupOptionsAsync().Returns([]);

            var result = await _sut.GetPermissionOptionsAsync();

            result.ProfitCentres.Should().BeEmpty();
            result.Programs.Should().BeEmpty();
            result.Categories.Should().BeEmpty();
            result.TestOwners.Should().BeEmpty();
            result.ProjectGroups.Should().BeEmpty();
        }

        #endregion
    }
}
