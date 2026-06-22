using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ProgramRepositoryTest
{
    public class ProgramRepositoryTests
    {
        private const int DefaultTestFpsYear = 2024;

        /// <summary>
        /// Creates a ProgramRepository with in-memory Programs, UserPrograms, and Users data.
        /// IFpsRequestContext is substituted via NSubstitute.
        /// Get() JOIN logic across Programs/UserPrograms/Users is covered by integration tests.
        /// ExecuteDeleteAsync() used in DeleteProgramAsync is not mockable and is covered by integration tests.
        /// </summary>
        private static ProgramRepository CreateRepository(
            IEnumerable<Core.Entities.Program> programs,
            IEnumerable<UserProgram> userPrograms,
            IEnumerable<User> users,
            int fpsYear = DefaultTestFpsYear,
            string userEmailId = "test@example.com", // always lowercase — matches middleware ToLowerInvariant()
            IEnumerable<ProgramView>? programViews = null)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(fpsYear);
            requestContext.UserEmailId.Returns(userEmailId);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);

            var programsMockSet = RepositoryTestHelper.CreateMockDbSet(programs);
            var userProgramsMockSet = RepositoryTestHelper.CreateMockDbSet(userPrograms);
            var usersMockSet = RepositoryTestHelper.CreateMockDbSet(users);
            var programViewsMockSet = RepositoryTestHelper.CreateMockDbSet(programViews ?? []);

            RepositoryTestHelper.SetupDbSetOperations(programsMockSet);
            RepositoryTestHelper.SetupDbSetOperations(userProgramsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.Programs).Returns(programsMockSet.Object);
            mockContext.Setup(x => x.UserPrograms).Returns(userProgramsMockSet.Object);
            mockContext.Setup(x => x.Users).Returns(usersMockSet.Object);
            mockContext.Setup(x => x.ProgramViews).Returns(programViewsMockSet.Object);

            return new ProgramRepository(mockContext.Object, requestContext);
        }

        private static (
            ProgramRepository Repo,
            Mock<DbSet<Core.Entities.Program>> ProgramsDbSet,
            Mock<DbSet<UserProgram>> UserProgramsDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(
                IEnumerable<Core.Entities.Program> programs,
                IEnumerable<UserProgram> userPrograms,
                IEnumerable<User> users,
                int fpsYear = DefaultTestFpsYear,
                string userEmailId = "test@example.com")
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(fpsYear);
            requestContext.UserEmailId.Returns(userEmailId);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);

            var programsMockSet = RepositoryTestHelper.CreateMockDbSet(programs);
            var userProgramsMockSet = RepositoryTestHelper.CreateMockDbSet(userPrograms);
            var usersMockSet = RepositoryTestHelper.CreateMockDbSet(users);

            RepositoryTestHelper.SetupDbSetOperations(programsMockSet);
            RepositoryTestHelper.SetupDbSetOperations(userProgramsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.Programs).Returns(programsMockSet.Object);
            mockContext.Setup(x => x.UserPrograms).Returns(userProgramsMockSet.Object);
            mockContext.Setup(x => x.Users).Returns(usersMockSet.Object);

            var repo = new ProgramRepository(mockContext.Object, requestContext);
            return (repo, programsMockSet, userProgramsMockSet, mockContext);
        }

        #region GetAllProgramsAsync

        [Fact]
        public async Task GetAllProgramsAsync_ReturnsPrograms_WhenUserEmailMatchesExactly()
        {
            // Arrange — DB email already lowercase, matches the normalised UserEmailId
            var views = new List<ProgramView>
            {
                new() { ProgramNo = "P001", ProgramName = "Alpha", UserEmail = "test@example.com" },
                new() { ProgramNo = "P002", ProgramName = "Beta",  UserEmail = "test@example.com" }
            };
            var repo = CreateRepository([], [], [], programViews: views);

            // Act
            var result = (await repo.GetAllProgramsAsync()).ToList();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Theory]
        [InlineData("Test@Example.COM")]
        [InlineData("TEST@EXAMPLE.COM")]
        [InlineData("Test@example.com")]
        public async Task GetAllProgramsAsync_ReturnsPrograms_WhenDbEmailIsMixedCase(string dbEmail)
        {
            // Arrange — DB stores mixed-case email; middleware normalises incoming to lowercase.
            // The query must use LOWER(UserEmail) so the comparison still matches.
            var views = new List<ProgramView>
            {
                new() { ProgramNo = "P001", ProgramName = "Alpha", UserEmail = dbEmail }
            };
            var repo = CreateRepository([], [], [],
                userEmailId: "test@example.com", // lowercase — as set by middleware
                programViews: views);

            // Act
            var result = (await repo.GetAllProgramsAsync()).ToList();

            // Assert — must find the record despite casing mismatch in DB
            Assert.Single(result);
            Assert.Equal("P001", result[0].ProgramNo);
        }

        [Fact]
        public async Task GetAllProgramsAsync_ExcludesPrograms_WhenEmailBelongsToDifferentUser()
        {
            // Arrange — two records with different emails; only the matching one should be returned
            var views = new List<ProgramView>
            {
                new() { ProgramNo = "P001", UserEmail = "test@example.com" },
                new() { ProgramNo = "P002", UserEmail = "other@example.com" }
            };
            var repo = CreateRepository([], [], [],
                userEmailId: "test@example.com",
                programViews: views);

            // Act
            var result = (await repo.GetAllProgramsAsync()).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("P001", result[0].ProgramNo);
        }

        [Fact]
        public async Task GetAllProgramsAsync_ExcludesPrograms_WhenDbEmailIsNull()
        {
            // Arrange — null UserEmail in DB must not match any user
            var views = new List<ProgramView>
            {
                new() { ProgramNo = "P001", UserEmail = null }
            };
            var repo = CreateRepository([], [], [], programViews: views);

            // Act
            var result = (await repo.GetAllProgramsAsync()).ToList();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetAllProgramsForAllUsers

        [Fact]
        public async Task GetAllProgramsForAllUsers_ReturnsAllPrograms_WithoutEmailFilter()
        {
            // Arrange — Programs table has records; no user email filtering expected
            var programs = new List<Core.Entities.Program>
            {
                new() { ProgramNo = "P001", ProgramName = "Alpha" },
                new() { ProgramNo = "P002", ProgramName = "Beta" }
            };
            var repo = CreateRepository(programs, [], []);

            // Act
            var result = (await repo.GetAllProgramsForAllUsers()).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("P001", result[0].ProgramNo);
            Assert.Equal("P002", result[1].ProgramNo);
        }

        [Fact]
        public async Task GetAllProgramsForAllUsers_ReturnsEmptyList_WhenNoProgramsExist()
        {
            // Arrange
            var repo = CreateRepository([], [], []);

            // Act
            var result = (await repo.GetAllProgramsForAllUsers()).ToList();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllProgramsForAllUsers_ReturnsAllPrograms_RegardlessOfUserEmail()
        {
            // Arrange — programs exist but current user email does not matter for unfiltered
            var programs = new List<Core.Entities.Program>
            {
                new() { ProgramNo = "P001", ProgramName = "Alpha" },
                new() { ProgramNo = "P002", ProgramName = "Beta" },
                new() { ProgramNo = "P003", ProgramName = "Gamma" }
            };
            var repo = CreateRepository(programs, [], [], userEmailId: "differentuser@example.com");

            // Act
            var result = (await repo.GetAllProgramsForAllUsers()).ToList();

            // Assert — all programs returned regardless of the user context
            Assert.Equal(3, result.Count);
        }

        #endregion

        #region GetProgramByIdAsync

        [Fact]
        public async Task GetProgramByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var programs = new List<Core.Entities.Program>
            {
                new() { ProgramNo = "P001", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(programs, [], []);

            // Act
            var result = await repo.GetProgramByIdAsync("P999");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProgramByIdAsync_ReturnsNull_WhenProgramsIsEmpty()
        {
            // Arrange
            var repo = CreateRepository([], [], []);

            // Act
            var result = await repo.GetProgramByIdAsync("P001");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProgramByIdAsync_ReturnsProgram_WhenFound()
        {
            // Arrange
            var programs = new List<Core.Entities.Program>
            {
                new() { ProgramNo = "P001", ProgramName = "Program One", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(programs, [], []);

            // Act
            var result = await repo.GetProgramByIdAsync("P001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("P001", result.ProgramNo);
            Assert.Equal("Program One", result.ProgramName);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetProgramByIdAsync_ThrowsArgumentException_WhenIdIsNullOrWhiteSpace(string id)
        {
            // Arrange
            var repo = CreateRepository([], [], []);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => repo.GetProgramByIdAsync(id));
        }

        #endregion

        #region AddProgramAsync

        [Fact]
        public async Task AddProgramAsync_ThrowsArgumentNullException_WhenProgramIsNull()
        {
            // Arrange
            var repo = CreateRepository([], [], []);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.AddProgramAsync(null!));
        }

        [Fact]
        public async Task AddProgramAsync_AddsProgram_AndSetsYearAndUserProgram_WhenRequestingUserExists()
        {
            // Arrange
            var requestingUser = new User { UserId = 1, UserEmail = "test@example.com" };
            var (repo, programsMockSet, userProgramsMockSet, mockContext) =
                CreateRepositoryWithMocks([], [], [requestingUser]);

            var newProgram = new Core.Entities.Program { ProgramNo = "P001", ProgramName = "Program One" };

            // Act
            var result = await repo.AddProgramAsync(newProgram);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("P001", result.ProgramNo);
            Assert.Equal(DefaultTestFpsYear, result.FpsYear);
            programsMockSet.Verify(x => x.Add(It.IsAny<Core.Entities.Program>()), Times.Once);
            userProgramsMockSet.Verify(x => x.Add(It.IsAny<UserProgram>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task AddProgramAsync_SetsCorrectUserProgramFields_WhenRequestingUserExists()
        {
            // Arrange
            var requestingUser = new User { UserId = 7, UserEmail = "test@example.com" };
            UserProgram? capturedUserProgram = null;
            var (repo, _, userProgramsMockSet, _) =
                CreateRepositoryWithMocks([], [], [requestingUser]);

            userProgramsMockSet
                .Setup(x => x.Add(It.IsAny<UserProgram>()))
                .Callback<UserProgram>(up => capturedUserProgram = up);

            var newProgram = new Core.Entities.Program { ProgramNo = "P001", ProgramName = "Program One" };

            // Act
            await repo.AddProgramAsync(newProgram);

            // Assert
            Assert.NotNull(capturedUserProgram);
            Assert.Equal("P001", capturedUserProgram!.ProgramNo);
            Assert.Equal(7, capturedUserProgram.UserID);
            Assert.Equal(DefaultTestFpsYear, capturedUserProgram.FpsYear);
        }

        [Fact]
        public async Task AddProgramAsync_AddsProgramOnly_WhenRequestingUserNotFound()
        {
            // Arrange — no user found by email means UserProgram should NOT be added
            var (repo, programsMockSet, userProgramsMockSet, mockContext) =
                CreateRepositoryWithMocks([], [], []);

            var newProgram = new Core.Entities.Program { ProgramNo = "P001", ProgramName = "Program One" };

            // Act
            var result = await repo.AddProgramAsync(newProgram);

            // Assert
            Assert.NotNull(result);
            programsMockSet.Verify(x => x.Add(It.IsAny<Core.Entities.Program>()), Times.Once);
            userProgramsMockSet.Verify(x => x.Add(It.IsAny<UserProgram>()), Times.Never);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task AddProgramAsync_SetsFpsCalYear_FromYearContext()
        {
            // Arrange
            const int customYear = 2025;
            var (repo, _, _, _) = CreateRepositoryWithMocks([], [], [], fpsYear: customYear);
            var newProgram = new Core.Entities.Program { ProgramNo = "P001" };

            // Act
            var result = await repo.AddProgramAsync(newProgram);

            // Assert
            Assert.Equal(customYear, result.FpsYear);
        }

        #endregion

        #region UpdateProgramAsync

        [Fact]
        public async Task UpdateProgramAsync_ThrowsArgumentNullException_WhenProgramIsNull()
        {
            // Arrange
            var repo = CreateRepository([], [], []);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateProgramAsync(null!, string.Empty));
        }

        [Fact]
        public async Task UpdateProgramAsync_UpdatesProgram_AndAddsUserProgram_WhenRequestingUserExistsAndLinkIsMissing()
        {
            // Arrange — requesting user exists but no UserProgram link yet → link should be created
            var requestingUser = new User { UserId = 1, UserEmail = "test@example.com" };
            var (repo, programsMockSet, userProgramsMockSet, mockContext) =
                CreateRepositoryWithMocks([], [], [requestingUser]);

            var program = new Core.Entities.Program { ProgramNo = "P001", ProgramName = "Updated Name" };

            // Act
            var result = await repo.UpdateProgramAsync(program, program.ProgramNo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("P001", result.ProgramNo);
            programsMockSet.Verify(x => x.Update(It.IsAny<Core.Entities.Program>()), Times.Once);
            userProgramsMockSet.Verify(x => x.Add(It.IsAny<UserProgram>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task UpdateProgramAsync_UpdatesProgram_AndSkipsUserProgram_WhenLinkAlreadyExists()
        {
            // Arrange — UserProgram link already exists → should NOT add a duplicate
            var requestingUser = new User { UserId = 1, UserEmail = "test@example.com" };
            var existingLink = new UserProgram { ProgramNo = "P001", UserID = 1, FpsYear = DefaultTestFpsYear };
            var (repo, programsMockSet, userProgramsMockSet, mockContext) =
                CreateRepositoryWithMocks([], [existingLink], [requestingUser]);

            var program = new Core.Entities.Program { ProgramNo = "P001", ProgramName = "Updated Name" };

            // Act
            var result = await repo.UpdateProgramAsync(program, program.ProgramNo);

            // Assert
            Assert.NotNull(result);
            programsMockSet.Verify(x => x.Update(It.IsAny<Core.Entities.Program>()), Times.Once);
            userProgramsMockSet.Verify(x => x.Add(It.IsAny<UserProgram>()), Times.Never);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task UpdateProgramAsync_UpdatesProgramOnly_WhenRequestingUserNotFound()
        {
            // Arrange — no user found by email means UserProgram should NOT be touched
            var (repo, programsMockSet, userProgramsMockSet, mockContext) =
                CreateRepositoryWithMocks([], [], []);

            var program = new Core.Entities.Program { ProgramNo = "P001", ProgramName = "Updated Name" };

            // Act
            var result = await repo.UpdateProgramAsync(program, program.ProgramNo);

            // Assert
            Assert.NotNull(result);
            programsMockSet.Verify(x => x.Update(It.IsAny<Core.Entities.Program>()), Times.Once);
            userProgramsMockSet.Verify(x => x.Add(It.IsAny<UserProgram>()), Times.Never);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        #endregion

        #region DeleteProgramAsync

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task DeleteProgramAsync_ThrowsArgumentException_WhenIdIsNullOrWhiteSpace(string id)
        {
            // Arrange
            var repo = CreateRepository([], [], []);

            // Act & Assert
            // ExecuteDeleteAsync() is not mockable with Moq; full delete logic is covered by integration tests.
            await Assert.ThrowsAsync<ArgumentException>(() => repo.DeleteProgramAsync(id));
        }

        #endregion
    }
}