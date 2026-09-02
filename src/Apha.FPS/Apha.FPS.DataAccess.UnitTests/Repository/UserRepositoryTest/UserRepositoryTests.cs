using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Apha.FPS.DataAccess.UnitTests.Repository.UserRepositoryTest
{
    public class UserRepositoryTests
    {
        private const int DefaultFpsYear = 2025;
        private const string DefaultUserEmail = "test@example.com";

        #region Helpers

        private static User BuildUser(
            int userId = 1,
            string? username = "testuser",
            string? comments = "Test User",
            string? userEmail = "test@example.com",
            string? dt2Username = "dt2user") =>
            new()
            {
                UserId = userId,
                Username = username,
                Comments = comments,
                UserEmail = userEmail,
                Dt2Username = dt2Username
            };

        private static Mock<IFpsRequestContext> CreateRequestContextMock()
        {
            var mock = new Mock<IFpsRequestContext>();
            mock.Setup(x => x.FpsYear).Returns(DefaultFpsYear);
            mock.Setup(x => x.UserEmailId).Returns(DefaultUserEmail);
            return mock;
        }

        private static UserRepository CreateRepository(IEnumerable<User>? users = null)
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            if (users != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(users);
                RepositoryTestHelper.SetupDbSetOperations(mockSet);
                dbContext.Setup(x => x.Users).Returns(mockSet.Object);
            }

            RepositoryTestHelper.SetupSaveChanges(dbContext);
            return new UserRepository(dbContext.Object, requestCtx.Object);
        }

        private static (UserRepository Repo, Mock<FpsDbContext> Context, Mock<DbSet<User>> DbSet)
            CreateRepositoryWithMocks(IEnumerable<User>? users = null)
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var dbSet = RepositoryTestHelper.CreateMockDbSet(users ?? []);
            RepositoryTestHelper.SetupDbSetOperations(dbSet);
            dbContext.Setup(x => x.Users).Returns(dbSet.Object);

            RepositoryTestHelper.SetupSaveChanges(dbContext);
            return (new UserRepository(dbContext.Object, requestCtx.Object), dbContext, dbSet);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenDbContextIsNull()
        {
            var ctx = CreateRequestContextMock();
            Assert.Throws<ArgumentNullException>(() => new UserRepository(null!, ctx.Object));
        }

        [Fact]
        public void Constructor_DoesNotThrow_WhenRequestContextIsProvided()
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);
            dbContext.Setup(x => x.Users).Returns(RepositoryTestHelper.CreateMockDbSet(new List<User>()).Object);
            RepositoryTestHelper.SetupSaveChanges(dbContext);

            var repo = new UserRepository(dbContext.Object, requestCtx.Object);

            Assert.NotNull(repo);
        }

        #endregion

        #region GetAllUsersAsync Tests

        [Fact]
        public async Task GetAllUsersAsync_ReturnsAllUsers()
        {
            var users = new List<User>
            {
                BuildUser(1, "user1", "User One"),
                BuildUser(2, "user2", "User Two")
            };
            var repo = CreateRepository(users);

            var result = await repo.GetAllUsersAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsEmpty_WhenNoUsers()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetAllUsersAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllUsersAsync_OrdersByComments()
        {
            var users = new List<User>
            {
                BuildUser(1, "user1", "Zebra"),
                BuildUser(2, "user2", "Apple")
            };
            var repo = CreateRepository(users);

            var result = (await repo.GetAllUsersAsync()).ToList();

            Assert.Equal("Apple", result[0].Comments);
            Assert.Equal("Zebra", result[1].Comments);
        }

        #endregion

        #region GetAllUsersPagedAsync Tests

        [Fact]
        public async Task GetAllUsersPagedAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            var repo = CreateRepository([]);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repo.GetAllUsersPagedAsync(null!));
        }

        [Fact]
        public async Task GetAllUsersPagedAsync_ReturnsPagedData()
        {
            var users = Enumerable.Range(1, 20).Select(i =>
                BuildUser(i, $"user{i}", $"User {i}")).ToList();
            var repo = CreateRepository(users);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.GetAllUsersPagedAsync(query);

            Assert.NotNull(result);
            Assert.Equal(10, result.Data.Count());
        }

        [Fact]
        public async Task GetAllUsersPagedAsync_AppliesSorting_Descending()
        {
            var users = new List<User>
            {
                BuildUser(1, "alpha", "Alpha"),
                BuildUser(2, "beta", "Beta")
            };
            var repo = CreateRepository(users);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "username", Descending = true };
            var result = await repo.GetAllUsersPagedAsync(query);

            var list = result.Data.ToList();
            Assert.Equal("beta", list[0].Username);
            Assert.Equal("alpha", list[1].Username);
        }

        #endregion

        #region GetNonSuperUsersPagedAsync Tests

        [Fact]
        public async Task GetNonSuperUsersPagedAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            var repo = CreateRepository([]);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repo.GetNonSuperUsersPagedAsync(null!));
        }

        [Fact]
        public async Task GetNonSuperUsersPagedAsync_ReturnsPagedData()
        {
            var users = Enumerable.Range(1, 20).Select(i =>
                BuildUser(i, $"user{i}", $"User {i}")).ToList();
            var repo = CreateRepository(users);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.GetNonSuperUsersPagedAsync(query);

            Assert.NotNull(result);
            Assert.Equal(10, result.Data.Count());
        }

        [Fact]
        public async Task GetNonSuperUsersPagedAsync_AppliesSorting_Descending()
        {
            var users = new List<User>
            {
                BuildUser(1, "alpha", "Alpha"),
                BuildUser(2, "beta", "Beta")
            };
            var repo = CreateRepository(users);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "username", Descending = true };
            var result = await repo.GetNonSuperUsersPagedAsync(query);

            var list = result.Data.ToList();
            Assert.Equal("beta", list[0].Username);
            Assert.Equal("alpha", list[1].Username);
        }

        #endregion

        #region GetUserByIdAsync Tests

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser_WhenExists()
        {
            var users = new List<User> { BuildUser(1) };
            var repo = CreateRepository(users);

            var result = await repo.GetUserByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result!.UserId);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsNull_WhenNotExists()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetUserByIdAsync(999);

            Assert.Null(result);
        }

        #endregion

        #region GetUserByUsernameAsync Tests

        [Fact]
        public async Task GetUserByUsernameAsync_ReturnsUser_WhenExists()
        {
            var users = new List<User> { BuildUser(1, "testuser") };
            var repo = CreateRepository(users);

            var result = await repo.GetUserByUsernameAsync("testuser");

            Assert.NotNull(result);
            Assert.Equal("testuser", result!.Username);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_ReturnsNull_WhenNotExists()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetUserByUsernameAsync("nonexistent");

            Assert.Null(result);
        }

        #endregion

        #region GetUserByEmailAsync Tests

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsUser_WhenExists()
        {
            var users = new List<User> { BuildUser(1, userEmail: "test@example.com") };
            var repo = CreateRepository(users);

            var result = await repo.GetUserByEmailAsync("test@example.com");

            Assert.NotNull(result);
            Assert.Equal("test@example.com", result!.UserEmail);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsNull_WhenNotExists()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetUserByEmailAsync("nonexistent@example.com");

            Assert.Null(result);
        }

        #endregion

        #region AddUserAsync Tests

        [Fact]
        public async Task AddUserAsync_ThrowsArgumentNullException_WhenEntityIsNull()
        {
            var repo = CreateRepository();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repo.AddUserAsync(null!));
        }

        [Fact]
        public async Task AddUserAsync_AddsEntityToDbSet()
        {
            var (repo, _, dbSet) = CreateRepositoryWithMocks([]);
            var user = BuildUser(0);

            var result = await repo.AddUserAsync(user);

            Assert.NotNull(result);
            dbSet.Verify(d => d.Add(It.IsAny<User>()), Times.Once);
        }

        #endregion

        #region UpdateUserAsync Tests

        [Fact]
        public async Task UpdateUserAsync_ThrowsArgumentNullException_WhenEntityIsNull()
        {
            var repo = CreateRepository();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repo.UpdateUserAsync(null!));
        }

        #endregion

        #region GetUserPermission Lookups Tests

        [Fact]
        public async Task GetUserProfitCentresAsync_ReturnsCorrectData()
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var data = new List<UserProfitcentre>
            {
                new() { UserId = 1, ProfitCentre = "PC1", FpsYear = DefaultFpsYear },
                new() { UserId = 1, ProfitCentre = "PC2", FpsYear = DefaultFpsYear },
                new() { UserId = 2, ProfitCentre = "PC3", FpsYear = DefaultFpsYear }
            };
            var mockSet = RepositoryTestHelper.CreateMockDbSet(data);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            dbContext.Setup(x => x.UserProfitcentres).Returns(mockSet.Object);
            dbContext.Setup(x => x.Users).Returns(RepositoryTestHelper.CreateMockDbSet(new List<User>()).Object);
            RepositoryTestHelper.SetupSaveChanges(dbContext);

            var repo = new UserRepository(dbContext.Object, requestCtx.Object);

            var result = await repo.GetUserProfitCentresAsync(1);

            Assert.Equal(2, result.Count);
            Assert.Contains("PC1", result);
            Assert.Contains("PC2", result);
        }

        [Fact]
        public async Task GetUserProgramsAsync_ReturnsCorrectData()
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var data = new List<UserProgram>
            {
                new() { UserID = 1, ProgramNo = "P1", FpsYear = DefaultFpsYear },
                new() { UserID = 1, ProgramNo = "P2", FpsYear = DefaultFpsYear }
            };
            var mockSet = RepositoryTestHelper.CreateMockDbSet(data);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            dbContext.Setup(x => x.UserPrograms).Returns(mockSet.Object);
            dbContext.Setup(x => x.Users).Returns(RepositoryTestHelper.CreateMockDbSet(new List<User>()).Object);
            RepositoryTestHelper.SetupSaveChanges(dbContext);

            var repo = new UserRepository(dbContext.Object, requestCtx.Object);

            var result = await repo.GetUserProgramsAsync(1);

            Assert.Equal(2, result.Count);
            Assert.Contains("P1", result);
        }

        [Fact]
        public async Task GetUserCategoriesAsync_ReturnsCorrectData()
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var data = new List<UserCategory>
            {
                new() { UserId = 1, Category = "C1", FpsYear = DefaultFpsYear }
            };
            var mockSet = RepositoryTestHelper.CreateMockDbSet(data);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            dbContext.Setup(x => x.UserCategories).Returns(mockSet.Object);
            dbContext.Setup(x => x.Users).Returns(RepositoryTestHelper.CreateMockDbSet(new List<User>()).Object);
            RepositoryTestHelper.SetupSaveChanges(dbContext);

            var repo = new UserRepository(dbContext.Object, requestCtx.Object);

            var result = await repo.GetUserCategoriesAsync(1);

            Assert.Single(result);
            Assert.Contains("C1", result);
        }

        [Fact]
        public async Task GetUserTestOwnersAsync_ReturnsCorrectData()
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var data = new List<UserTestOwner>
            {
                new() { UserId = 1, TestOwner = "T1", FpsYear = DefaultFpsYear }
            };
            var mockSet = RepositoryTestHelper.CreateMockDbSet(data);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            dbContext.Setup(x => x.UserTestOwners).Returns(mockSet.Object);
            dbContext.Setup(x => x.Users).Returns(RepositoryTestHelper.CreateMockDbSet(new List<User>()).Object);
            RepositoryTestHelper.SetupSaveChanges(dbContext);

            var repo = new UserRepository(dbContext.Object, requestCtx.Object);

            var result = await repo.GetUserTestOwnersAsync(1);

            Assert.Single(result);
            Assert.Contains("T1", result);
        }

        [Fact]
        public async Task GetUserProjectGroupsAsync_ReturnsCorrectData()
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var data = new List<UserProjectGroup>
            {
                new() { UserId = 1, ProjectGroup = "PG1", FpsYear = DefaultFpsYear }
            };
            var mockSet = RepositoryTestHelper.CreateMockDbSet(data);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            dbContext.Setup(x => x.UserProjectGroups).Returns(mockSet.Object);
            dbContext.Setup(x => x.Users).Returns(RepositoryTestHelper.CreateMockDbSet(new List<User>()).Object);
            RepositoryTestHelper.SetupSaveChanges(dbContext);

            var repo = new UserRepository(dbContext.Object, requestCtx.Object);

            var result = await repo.GetUserProjectGroupsAsync(1);

            Assert.Single(result);
            Assert.Contains("PG1", result);
        }

        [Fact]
        public async Task GetAllCategoryOptionsAsync_ReturnsDistinctOrderedCategories()
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var data = new List<Category>
            {
                new() { CategoryName = "Zebra" },
                new() { CategoryName = "Alpha" },
                new() { CategoryName = "Alpha" },
                new() { CategoryName = "Beta" }
            };
            var mockSet = RepositoryTestHelper.CreateMockDbSet(data);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            dbContext.Setup(x => x.Categories).Returns(mockSet.Object);
            dbContext.Setup(x => x.Users).Returns(RepositoryTestHelper.CreateMockDbSet(new List<User>()).Object);
            RepositoryTestHelper.SetupSaveChanges(dbContext);

            var repo = new UserRepository(dbContext.Object, requestCtx.Object);

            var result = await repo.GetAllCategoryOptionsAsync();

            Assert.Equal(3, result.Count);
            Assert.Equal("Alpha", result[0]);
            Assert.Equal("Beta", result[1]);
            Assert.Equal("Zebra", result[2]);
        }

        [Fact]
        public async Task GetAllCategoryOptionsAsync_ReturnsEmpty_WhenNoCategories()
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var mockSet = RepositoryTestHelper.CreateMockDbSet(new List<Category>());
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            dbContext.Setup(x => x.Categories).Returns(mockSet.Object);
            dbContext.Setup(x => x.Users).Returns(RepositoryTestHelper.CreateMockDbSet(new List<User>()).Object);
            RepositoryTestHelper.SetupSaveChanges(dbContext);

            var repo = new UserRepository(dbContext.Object, requestCtx.Object);

            var result = await repo.GetAllCategoryOptionsAsync();

            Assert.Empty(result);
        }

        #endregion

        #region UpdateUserAsync Tests (additional coverage)

        [Fact]
        public async Task UpdateUserAsync_UpdatesAllFields_WhenUserExists()
        {
            var existingUser = BuildUser(1, "olduser", "Old Comment", "old@test.com", "olddt2");
            var users = new List<User> { existingUser };

            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var mockSet = RepositoryTestHelper.CreateMockDbSet(users);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            dbContext.Setup(x => x.Users).Returns(mockSet.Object);
            RepositoryTestHelper.SetupSaveChanges(dbContext);

            var repo = new UserRepository(dbContext.Object, requestCtx.Object);

            var updatedEntity = BuildUser(1, "newuser", "New Comment", "new@test.com", "newdt2");

            var result = await repo.UpdateUserAsync(updatedEntity);

            Assert.Equal("newuser", result.Username);
            Assert.Equal("New Comment", result.Comments);
            Assert.Equal("new@test.com", result.UserEmail);
            Assert.Equal("newdt2", result.Dt2Username);
        }

        [Fact]
        public async Task UpdateUserAsync_ThrowsArgumentException_WhenUserNotFound()
        {
            var repo = CreateRepository([]);

            var entity = BuildUser(999, "ghost");

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => repo.UpdateUserAsync(entity));
            Assert.Contains("User with ID 999 not found", ex.Message);
        }

        [Fact]
        public async Task UpdateUserAsync_CallsSaveChangesAsync()
        {
            var existingUser = BuildUser(1, "user1", "Comment", "email@test.com", "dt2");
            var users = new List<User> { existingUser };

            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var mockSet = RepositoryTestHelper.CreateMockDbSet(users);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            dbContext.Setup(x => x.Users).Returns(mockSet.Object);
            RepositoryTestHelper.SetupSaveChanges(dbContext);

            var repo = new UserRepository(dbContext.Object, requestCtx.Object);

            var updatedEntity = BuildUser(1, "updated", "Updated", "updated@test.com", "updateddt2");
            await repo.UpdateUserAsync(updatedEntity);

            RepositoryTestHelper.VerifySaveChanges(dbContext);
        }

        #endregion

        #region AddUserAsync Tests (additional coverage)

        [Fact]
        public async Task AddUserAsync_CallsSaveChangesAsync()
        {
            var (repo, dbContext, _) = CreateRepositoryWithMocks([]);
            var user = BuildUser(0, "newuser");

            await repo.AddUserAsync(user);

            RepositoryTestHelper.VerifySaveChanges(dbContext);
        }

        [Fact]
        public async Task AddUserAsync_ReturnsAddedEntity()
        {
            var (repo, _, _) = CreateRepositoryWithMocks([]);
            var user = BuildUser(0, "newuser", "New User", "new@test.com", "dt2new");

            var result = await repo.AddUserAsync(user);

            Assert.NotNull(result);
            Assert.Equal("newuser", result.Username);
            Assert.Equal("New User", result.Comments);
            Assert.Equal("new@test.com", result.UserEmail);
            Assert.Equal("dt2new", result.Dt2Username);
        }

        #endregion

        #region GetUserByUsernameAsync Tests (additional coverage)

        [Fact]
        public async Task GetUserByUsernameAsync_ReturnsUser_CaseInsensitive()
        {
            var users = new List<User> { BuildUser(1, "TestUser") };
            var repo = CreateRepository(users);

            var result = await repo.GetUserByUsernameAsync("testuser");

            Assert.NotNull(result);
            Assert.Equal("TestUser", result!.Username);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_ReturnsNull_WhenUsernameIsNull()
        {
            var users = new List<User> { BuildUser(1, username: null) };
            var repo = CreateRepository(users);

            var result = await repo.GetUserByUsernameAsync("testuser");

            Assert.Null(result);
        }

        #endregion

        #region GetUserByEmailAsync Tests (additional coverage)

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsUser_CaseInsensitive()
        {
            var users = new List<User> { BuildUser(1, userEmail: "Test@Example.COM") };
            var repo = CreateRepository(users);

            var result = await repo.GetUserByEmailAsync("test@example.com");

            Assert.NotNull(result);
            Assert.Equal("Test@Example.COM", result!.UserEmail);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsNull_WhenEmailIsNull()
        {
            var users = new List<User> { BuildUser(1, userEmail: null) };
            var repo = CreateRepository(users);

            var result = await repo.GetUserByEmailAsync("test@example.com");

            Assert.Null(result);
        }

        #endregion

        #region GetNonSuperUsersPagedAsync Tests (additional coverage)

        [Fact]
        public async Task GetNonSuperUsersPagedAsync_ExcludesSuperUser()
        {
            var superUserId = (int)Core.Enums.SuperUser.SuperUserId;
            var users = new List<User>
            {
                BuildUser(superUserId, "superuser", "Super User"),
                BuildUser(superUserId + 1, "regular1", "Regular One"),
                BuildUser(superUserId + 2, "regular2", "Regular Two")
            };
            var repo = CreateRepository(users);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.GetNonSuperUsersPagedAsync(query);

            Assert.Equal(2, result.Data.Count());
            Assert.DoesNotContain(result.Data, u => u.UserId == superUserId);
        }

        [Fact]
        public async Task GetNonSuperUsersPagedAsync_ReturnsEmpty_WhenOnlySuperUserExists()
        {
            var superUserId = (int)Core.Enums.SuperUser.SuperUserId;
            var users = new List<User> { BuildUser(superUserId, "superuser") };
            var repo = CreateRepository(users);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.GetNonSuperUsersPagedAsync(query);

            Assert.Empty(result.Data);
        }

        #endregion

        #region GetAllUsersPagedAsync Sorting Tests (additional coverage)

        [Fact]
        public async Task GetAllUsersPagedAsync_AppliesSorting_ByDt2Username()
        {
            var users = new List<User>
            {
                BuildUser(1, dt2Username: "Charlie"),
                BuildUser(2, dt2Username: "Alice"),
                BuildUser(3, dt2Username: "Bob")
            };
            var repo = CreateRepository(users);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "dt2username" };
            var result = await repo.GetAllUsersPagedAsync(query);

            var list = result.Data.ToList();
            Assert.Equal("Alice", list[0].Dt2Username);
            Assert.Equal("Bob", list[1].Dt2Username);
            Assert.Equal("Charlie", list[2].Dt2Username);
        }

        [Fact]
        public async Task GetAllUsersPagedAsync_AppliesSorting_ByUserEmail()
        {
            var users = new List<User>
            {
                BuildUser(1, userEmail: "charlie@test.com"),
                BuildUser(2, userEmail: "alice@test.com"),
                BuildUser(3, userEmail: "bob@test.com")
            };
            var repo = CreateRepository(users);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "useremail" };
            var result = await repo.GetAllUsersPagedAsync(query);

            var list = result.Data.ToList();
            Assert.Equal("alice@test.com", list[0].UserEmail);
            Assert.Equal("bob@test.com", list[1].UserEmail);
            Assert.Equal("charlie@test.com", list[2].UserEmail);
        }

        [Fact]
        public async Task GetAllUsersPagedAsync_AppliesSorting_ByUserId()
        {
            var users = new List<User>
            {
                BuildUser(3, "user3"),
                BuildUser(1, "user1"),
                BuildUser(2, "user2")
            };
            var repo = CreateRepository(users);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "userid" };
            var result = await repo.GetAllUsersPagedAsync(query);

            var list = result.Data.ToList();
            Assert.Equal(1, list[0].UserId);
            Assert.Equal(2, list[1].UserId);
            Assert.Equal(3, list[2].UserId);
        }

        [Fact]
        public async Task GetAllUsersPagedAsync_AppliesSorting_ByComments()
        {
            var users = new List<User>
            {
                BuildUser(1, comments: "Zeta"),
                BuildUser(2, comments: "Alpha"),
                BuildUser(3, comments: "Mango")
            };
            var repo = CreateRepository(users);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "comments" };
            var result = await repo.GetAllUsersPagedAsync(query);

            var list = result.Data.ToList();
            Assert.Equal("Alpha", list[0].Comments);
            Assert.Equal("Mango", list[1].Comments);
            Assert.Equal("Zeta", list[2].Comments);
        }

        [Fact]
        public async Task GetAllUsersPagedAsync_DefaultsSorting_WhenSortByIsUnknown()
        {
            var users = new List<User>
            {
                BuildUser(1, comments: "Zeta"),
                BuildUser(2, comments: "Alpha")
            };
            var repo = CreateRepository(users);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "unknownfield" };
            var result = await repo.GetAllUsersPagedAsync(query);

            var list = result.Data.ToList();
            Assert.Equal("Alpha", list[0].Comments);
            Assert.Equal("Zeta", list[1].Comments);
        }

        [Fact]
        public async Task GetAllUsersPagedAsync_DefaultsSorting_WhenSortByIsNull()
        {
            var users = new List<User>
            {
                BuildUser(1, comments: "Zeta"),
                BuildUser(2, comments: "Alpha")
            };
            var repo = CreateRepository(users);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = null };
            var result = await repo.GetAllUsersPagedAsync(query);

            var list = result.Data.ToList();
            Assert.Equal("Alpha", list[0].Comments);
            Assert.Equal("Zeta", list[1].Comments);
        }

        [Fact]
        public async Task GetAllUsersPagedAsync_ReturnsSecondPage()
        {
            var users = Enumerable.Range(1, 25).Select(i =>
                BuildUser(i, $"user{i:D2}", $"Comment{i:D2}")).ToList();
            var repo = CreateRepository(users);

            var query = new PaginationParameters<string> { Page = 2, PageSize = 10 };
            var result = await repo.GetAllUsersPagedAsync(query);

            Assert.Equal(10, result.Data.Count());
        }

        #endregion

        #region GetAllProfitCentreOptionsAsync Tests

        [Fact]
        public async Task GetAllProfitCentreOptionsAsync_ReturnsDistinctOrderedProfitCentres_ForCurrentUserAndYear()
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var users = new List<User> { BuildUser(userId: 1, userEmail: DefaultUserEmail) };
            var data = new List<UserProfitcentre>
            {
                new() { ProfitCentre = "PC3", UserId = 1, FpsYear = DefaultFpsYear },
                new() { ProfitCentre = "PC1", UserId = 1, FpsYear = DefaultFpsYear },
                new() { ProfitCentre = "PC2", UserId = 1, FpsYear = DefaultFpsYear },
                new() { ProfitCentre = "PC1", UserId = 1, FpsYear = DefaultFpsYear },
                // Different user - must be excluded
                new() { ProfitCentre = "PC9", UserId = 2, FpsYear = DefaultFpsYear },
                // Different year - must be excluded
                new() { ProfitCentre = "PC8", UserId = 1, FpsYear = DefaultFpsYear - 1 }
            };
            var mockSet = RepositoryTestHelper.CreateMockDbSet(data);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            dbContext.Setup(x => x.UserProfitcentres).Returns(mockSet.Object);
            dbContext.Setup(x => x.Users).Returns(RepositoryTestHelper.CreateMockDbSet(users).Object);
            RepositoryTestHelper.SetupSaveChanges(dbContext);

            var repo = new UserRepository(dbContext.Object, requestCtx.Object);

            var result = await repo.GetAllProfitCentreOptionsAsync();

            Assert.Equal(3, result.Count);
            Assert.Equal("PC1", result[0]);
            Assert.Equal("PC2", result[1]);
            Assert.Equal("PC3", result[2]);
        }

        [Fact]
        public async Task GetAllProfitCentreOptionsAsync_ReturnsEmpty_WhenCurrentUserHasNoProfitCentres()
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var users = new List<User> { BuildUser(userId: 1, userEmail: DefaultUserEmail) };
            var mockSet = RepositoryTestHelper.CreateMockDbSet(new List<UserProfitcentre>());
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            dbContext.Setup(x => x.UserProfitcentres).Returns(mockSet.Object);
            dbContext.Setup(x => x.Users).Returns(RepositoryTestHelper.CreateMockDbSet(users).Object);
            RepositoryTestHelper.SetupSaveChanges(dbContext);

            var repo = new UserRepository(dbContext.Object, requestCtx.Object);

            var result = await repo.GetAllProfitCentreOptionsAsync();

            Assert.Empty(result);
        }

        #endregion

        #region GetAllProgramOptionsAsync Tests

        [Fact]
        public async Task GetAllProgramOptionsAsync_ReturnsDistinctOrderedPrograms_ForCurrentUserAndYear()
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var users = new List<User> { BuildUser(userId: 1, userEmail: DefaultUserEmail) };
            var data = new List<UserProgram>
            {
                new() { ProgramNo = "B01", UserID = 1, FpsYear = DefaultFpsYear },
                new() { ProgramNo = "A01", UserID = 1, FpsYear = DefaultFpsYear },
                new() { ProgramNo = "C01", UserID = 1, FpsYear = DefaultFpsYear },
                new() { ProgramNo = "A01", UserID = 1, FpsYear = DefaultFpsYear },
                // Different user - must be excluded
                new() { ProgramNo = "Z01", UserID = 2, FpsYear = DefaultFpsYear },
                // Different year - must be excluded
                new() { ProgramNo = "Y01", UserID = 1, FpsYear = DefaultFpsYear - 1 }
            };
            var mockSet = RepositoryTestHelper.CreateMockDbSet(data);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            dbContext.Setup(x => x.UserPrograms).Returns(mockSet.Object);
            dbContext.Setup(x => x.Users).Returns(RepositoryTestHelper.CreateMockDbSet(users).Object);
            RepositoryTestHelper.SetupSaveChanges(dbContext);

            var repo = new UserRepository(dbContext.Object, requestCtx.Object);

            var result = await repo.GetAllProgramOptionsAsync();

            Assert.Equal(3, result.Count);
            Assert.Equal("A01", result[0]);
            Assert.Equal("B01", result[1]);
            Assert.Equal("C01", result[2]);
        }

        [Fact]
        public async Task GetAllProgramOptionsAsync_ReturnsEmpty_WhenCurrentUserHasNoPrograms()
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var users = new List<User> { BuildUser(userId: 1, userEmail: DefaultUserEmail) };
            var mockSet = RepositoryTestHelper.CreateMockDbSet(new List<UserProgram>());
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            dbContext.Setup(x => x.UserPrograms).Returns(mockSet.Object);
            dbContext.Setup(x => x.Users).Returns(RepositoryTestHelper.CreateMockDbSet(users).Object);
            RepositoryTestHelper.SetupSaveChanges(dbContext);

            var repo = new UserRepository(dbContext.Object, requestCtx.Object);

            var result = await repo.GetAllProgramOptionsAsync();

            Assert.Empty(result);
        }

        #endregion

        #region GetAllTestOwnerOptionsAsync Tests

        [Fact]
        public async Task GetAllTestOwnerOptionsAsync_ReturnsDistinctOrderedTestOwners()
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var data = new List<UserTestOwner>
            {
                new() { UserId = 1, TestOwner = "OwnerC", FpsYear = DefaultFpsYear },
                new() { UserId = 2, TestOwner = "OwnerA", FpsYear = DefaultFpsYear },
                new() { UserId = 3, TestOwner = "OwnerB", FpsYear = DefaultFpsYear },
                new() { UserId = 4, TestOwner = "OwnerA", FpsYear = DefaultFpsYear }
            };
            var mockSet = RepositoryTestHelper.CreateMockDbSet(data);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            dbContext.Setup(x => x.UserTestOwners).Returns(mockSet.Object);
            dbContext.Setup(x => x.Users).Returns(RepositoryTestHelper.CreateMockDbSet(new List<User>()).Object);
            RepositoryTestHelper.SetupSaveChanges(dbContext);

            var repo = new UserRepository(dbContext.Object, requestCtx.Object);

            var result = await repo.GetAllTestOwnerOptionsAsync();

            Assert.Equal(3, result.Count);
            Assert.Equal("OwnerA", result[0]);
            Assert.Equal("OwnerB", result[1]);
            Assert.Equal("OwnerC", result[2]);
        }

        [Fact]
        public async Task GetAllTestOwnerOptionsAsync_ReturnsEmpty_WhenNoTestOwners()
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var mockSet = RepositoryTestHelper.CreateMockDbSet(new List<UserTestOwner>());
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            dbContext.Setup(x => x.UserTestOwners).Returns(mockSet.Object);
            dbContext.Setup(x => x.Users).Returns(RepositoryTestHelper.CreateMockDbSet(new List<User>()).Object);
            RepositoryTestHelper.SetupSaveChanges(dbContext);

            var repo = new UserRepository(dbContext.Object, requestCtx.Object);

            var result = await repo.GetAllTestOwnerOptionsAsync();

            Assert.Empty(result);
        }

        #endregion

        #region GetAllProjectGroupOptionsAsync Tests

        [Fact]
        public async Task GetAllProjectGroupOptionsAsync_ReturnsDistinctOrderedProjectGroups()
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var data = new List<ProjectGroup>
            {
                new() { ProjectGroupName = "GroupC", FpsYear = DefaultFpsYear },
                new() { ProjectGroupName = "GroupA", FpsYear = DefaultFpsYear },
                new() { ProjectGroupName = "GroupB", FpsYear = DefaultFpsYear },
                new() { ProjectGroupName = "GroupA", FpsYear = DefaultFpsYear }
            };
            var mockSet = RepositoryTestHelper.CreateMockDbSet(data);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            dbContext.Setup(x => x.ProjectGroups).Returns(mockSet.Object);
            dbContext.Setup(x => x.Users).Returns(RepositoryTestHelper.CreateMockDbSet(new List<User>()).Object);
            RepositoryTestHelper.SetupSaveChanges(dbContext);

            var repo = new UserRepository(dbContext.Object, requestCtx.Object);

            var result = await repo.GetAllProjectGroupOptionsAsync();

            Assert.Equal(3, result.Count);
            Assert.Equal("GroupA", result[0]);
            Assert.Equal("GroupB", result[1]);
            Assert.Equal("GroupC", result[2]);
        }

        [Fact]
        public async Task GetAllProjectGroupOptionsAsync_ReturnsEmpty_WhenNoProjectGroups()
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var mockSet = RepositoryTestHelper.CreateMockDbSet(new List<ProjectGroup>());
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            dbContext.Setup(x => x.ProjectGroups).Returns(mockSet.Object);
            dbContext.Setup(x => x.Users).Returns(RepositoryTestHelper.CreateMockDbSet(new List<User>()).Object);
            RepositoryTestHelper.SetupSaveChanges(dbContext);

            var repo = new UserRepository(dbContext.Object, requestCtx.Object);

            var result = await repo.GetAllProjectGroupOptionsAsync();

            Assert.Empty(result);
        }

        #endregion

        #region GetUserProfitCentresAsync Tests (additional coverage)

        [Fact]
        public async Task GetUserProfitCentresAsync_ReturnsEmpty_WhenUserHasNoProfitCentres()
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var data = new List<UserProfitcentre>
            {
                new() { UserId = 2, ProfitCentre = "PC1", FpsYear = DefaultFpsYear }
            };
            var mockSet = RepositoryTestHelper.CreateMockDbSet(data);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            dbContext.Setup(x => x.UserProfitcentres).Returns(mockSet.Object);
            dbContext.Setup(x => x.Users).Returns(RepositoryTestHelper.CreateMockDbSet(new List<User>()).Object);
            RepositoryTestHelper.SetupSaveChanges(dbContext);

            var repo = new UserRepository(dbContext.Object, requestCtx.Object);

            var result = await repo.GetUserProfitCentresAsync(999);

            Assert.Empty(result);
        }

        #endregion

        #region GetUserProgramsAsync Tests (additional coverage)

        [Fact]
        public async Task GetUserProgramsAsync_ReturnsEmpty_WhenUserHasNoPrograms()
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var data = new List<UserProgram>
            {
                new() { UserID = 2, ProgramNo = "P1", FpsYear = DefaultFpsYear }
            };
            var mockSet = RepositoryTestHelper.CreateMockDbSet(data);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            dbContext.Setup(x => x.UserPrograms).Returns(mockSet.Object);
            dbContext.Setup(x => x.Users).Returns(RepositoryTestHelper.CreateMockDbSet(new List<User>()).Object);
            RepositoryTestHelper.SetupSaveChanges(dbContext);

            var repo = new UserRepository(dbContext.Object, requestCtx.Object);

            var result = await repo.GetUserProgramsAsync(999);

            Assert.Empty(result);
        }

        #endregion

        #region GetUserCategoriesAsync Tests (additional coverage)

        [Fact]
        public async Task GetUserCategoriesAsync_ReturnsEmpty_WhenUserHasNoCategories()
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var data = new List<UserCategory>
            {
                new() { UserId = 2, Category = "C1", FpsYear = DefaultFpsYear }
            };
            var mockSet = RepositoryTestHelper.CreateMockDbSet(data);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            dbContext.Setup(x => x.UserCategories).Returns(mockSet.Object);
            dbContext.Setup(x => x.Users).Returns(RepositoryTestHelper.CreateMockDbSet(new List<User>()).Object);
            RepositoryTestHelper.SetupSaveChanges(dbContext);

            var repo = new UserRepository(dbContext.Object, requestCtx.Object);

            var result = await repo.GetUserCategoriesAsync(999);

            Assert.Empty(result);
        }

        #endregion

        #region GetUserTestOwnersAsync Tests (additional coverage)

        [Fact]
        public async Task GetUserTestOwnersAsync_ReturnsEmpty_WhenUserHasNoTestOwners()
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var data = new List<UserTestOwner>
            {
                new() { UserId = 2, TestOwner = "T1", FpsYear = DefaultFpsYear }
            };
            var mockSet = RepositoryTestHelper.CreateMockDbSet(data);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            dbContext.Setup(x => x.UserTestOwners).Returns(mockSet.Object);
            dbContext.Setup(x => x.Users).Returns(RepositoryTestHelper.CreateMockDbSet(new List<User>()).Object);
            RepositoryTestHelper.SetupSaveChanges(dbContext);

            var repo = new UserRepository(dbContext.Object, requestCtx.Object);

            var result = await repo.GetUserTestOwnersAsync(999);

            Assert.Empty(result);
        }

        #endregion

        #region GetUserProjectGroupsAsync Tests (additional coverage)

        [Fact]
        public async Task GetUserProjectGroupsAsync_ReturnsEmpty_WhenUserHasNoProjectGroups()
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var data = new List<UserProjectGroup>
            {
                new() { UserId = 2, ProjectGroup = "PG1", FpsYear = DefaultFpsYear }
            };
            var mockSet = RepositoryTestHelper.CreateMockDbSet(data);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            dbContext.Setup(x => x.UserProjectGroups).Returns(mockSet.Object);
            dbContext.Setup(x => x.Users).Returns(RepositoryTestHelper.CreateMockDbSet(new List<User>()).Object);
            RepositoryTestHelper.SetupSaveChanges(dbContext);

            var repo = new UserRepository(dbContext.Object, requestCtx.Object);

            var result = await repo.GetUserProjectGroupsAsync(999);

            Assert.Empty(result);
        }

        #endregion

        #region GetAllUsersPagedAsync Filter Tests

        [Fact]
        public async Task GetAllUsersPagedAsync_AppliesFilter_ByUsername()
        {
            var users = new List<User>
            {
                BuildUser(1, "JohnDoe", "John"),
                BuildUser(2, "JaneSmith", "Jane"),
                BuildUser(3, "JohnSmith", "JohnS")
            };
            var repo = CreateRepository(users);

            var filter = "{\"Username\":\"John\"}";
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllUsersPagedAsync(query);

            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, u => Assert.Contains("John", u.Username!, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetAllUsersPagedAsync_AppliesFilter_ByUserEmail()
        {
            var users = new List<User>
            {
                BuildUser(1, "user1", userEmail: "john@test.com"),
                BuildUser(2, "user2", userEmail: "jane@test.com"),
                BuildUser(3, "user3", userEmail: "john.doe@test.com")
            };
            var repo = CreateRepository(users);

            var filter = "{\"UserEmail\":\"john\"}";
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllUsersPagedAsync(query);

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllUsersPagedAsync_AppliesFilter_ByComments()
        {
            var users = new List<User>
            {
                BuildUser(1, comments: "Admin User"),
                BuildUser(2, comments: "Regular User"),
                BuildUser(3, comments: "Admin Manager")
            };
            var repo = CreateRepository(users);

            var filter = "{\"Comments\":\"Admin\"}";
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllUsersPagedAsync(query);

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllUsersPagedAsync_AppliesFilter_ByDt2Username()
        {
            var users = new List<User>
            {
                BuildUser(1, dt2Username: "dt2john"),
                BuildUser(2, dt2Username: "dt2jane"),
                BuildUser(3, dt2Username: "dt2johnson")
            };
            var repo = CreateRepository(users);

            var filter = "{\"Dt2Username\":\"john\"}";
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllUsersPagedAsync(query);

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllUsersPagedAsync_ReturnsAll_WhenFilterIsNull()
        {
            var users = new List<User>
            {
                BuildUser(1, "user1"),
                BuildUser(2, "user2")
            };
            var repo = CreateRepository(users);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };
            var result = await repo.GetAllUsersPagedAsync(query);

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllUsersPagedAsync_ReturnsAll_WhenFilterIsEmptyJson()
        {
            var users = new List<User>
            {
                BuildUser(1, "user1"),
                BuildUser(2, "user2")
            };
            var repo = CreateRepository(users);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var result = await repo.GetAllUsersPagedAsync(query);

            Assert.Equal(2, result.Data.Count());
        }

        #endregion
    }
}
