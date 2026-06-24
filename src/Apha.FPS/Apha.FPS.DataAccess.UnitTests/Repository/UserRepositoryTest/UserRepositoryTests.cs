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
    }
}
