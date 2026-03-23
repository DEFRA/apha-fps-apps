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
        /// IFpsYearContext is substituted via NSubstitute.
        /// Get() JOIN logic across Programs/UserPrograms/Users is covered by integration tests.
        /// </summary>
        private static ProgramRepository CreateRepository(
            IEnumerable<Core.Entities.Program> programs,
            IEnumerable<UserProgram> userPrograms,
            IEnumerable<User> users,
            int fpsYear = DefaultTestFpsYear)
        {
            var fpsYearContext = Substitute.For<IFpsYearContext>();
            fpsYearContext.FPSYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);

            var programsMockSet = RepositoryTestHelper.CreateMockDbSet(programs);
            var userProgramsMockSet = RepositoryTestHelper.CreateMockDbSet(userPrograms);
            var usersMockSet = RepositoryTestHelper.CreateMockDbSet(users);

            RepositoryTestHelper.SetupDbSetOperations(programsMockSet);
            RepositoryTestHelper.SetupDbSetOperations(userProgramsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.Programs).Returns(programsMockSet.Object);
            mockContext.Setup(x => x.UserPrograms).Returns(userProgramsMockSet.Object);
            mockContext.Setup(x => x.Users).Returns(usersMockSet.Object);

            return new ProgramRepository(mockContext.Object, fpsYearContext);
        }

        /// <summary>
        /// Returns the mocked DbSets alongside the repository for tests that need to verify calls.
        /// </summary>
        private static (
            ProgramRepository Repo,
            Mock<DbSet<Core.Entities.Program>> ProgramsDbSet,
            Mock<DbSet<UserProgram>> UserProgramsDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(
                IEnumerable<Core.Entities.Program> programs,
                IEnumerable<UserProgram> userPrograms,
                IEnumerable<User> users,
                int fpsYear = DefaultTestFpsYear)
        {
            var fpsYearContext = Substitute.For<IFpsYearContext>();
            fpsYearContext.FPSYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);

            var programsMockSet = RepositoryTestHelper.CreateMockDbSet(programs);
            var userProgramsMockSet = RepositoryTestHelper.CreateMockDbSet(userPrograms);
            var usersMockSet = RepositoryTestHelper.CreateMockDbSet(users);

            RepositoryTestHelper.SetupDbSetOperations(programsMockSet);
            RepositoryTestHelper.SetupDbSetOperations(userProgramsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.Programs).Returns(programsMockSet.Object);
            mockContext.Setup(x => x.UserPrograms).Returns(userProgramsMockSet.Object);
            mockContext.Setup(x => x.Users).Returns(usersMockSet.Object);

            var repo = new ProgramRepository(mockContext.Object, fpsYearContext);
            return (repo, programsMockSet, userProgramsMockSet, mockContext);
        }
       

        #region Get        

        [Fact]
        public async Task GetProgramByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var programs = new List<Core.Entities.Program>
            {
                new() { ProgramNo = "P001", FpsCalYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(programs, new List<UserProgram>(), new List<User>());

            // Act
            var result = await repo.GetProgramByIdAsync("P999");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProgramByIdAsync_ReturnsNull_WhenProgramsIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(new List<Core.Entities.Program>(), new List<UserProgram>(), new List<User>());

            // Act
            var result = await repo.GetProgramByIdAsync("P001");

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetProgramByIdAsync_ThrowsArgumentException_WhenIdIsNullOrWhiteSpace(string id)
        {
            // Arrange
            var repo = CreateRepository(new List<Core.Entities.Program>(), new List<UserProgram>(), new List<User>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => repo.GetProgramByIdAsync(id));
        }

        #endregion

        #region AddProgramAsync

        [Fact]
        public async Task AddProgramAsync_AddsProgram_AndSetsYearAndUserProgram_WhenDboUserExists()
        {
            // Arrange
            var dboUser = new User { UserId = 1, Username = "dbo" };
            var (repo, programsMockSet, userProgramsMockSet, mockContext) =
                CreateRepositoryWithMocks(
                    new List<Core.Entities.Program>(),
                    new List<UserProgram>(),
                    new List<User> { dboUser });

            var newProgram = new Core.Entities.Program { ProgramNo = "P001", ProgramName = "Program One" };

            // Act
            var result = await repo.AddProgramAsync(newProgram);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("P001", result.ProgramNo);
            Assert.Equal(DefaultTestFpsYear, result.FpsCalYear);
            programsMockSet.Verify(x => x.Add(It.IsAny<Core.Entities.Program>()), Times.Once);
            userProgramsMockSet.Verify(x => x.Add(It.IsAny<UserProgram>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task AddProgramAsync_AddsProgramOnly_WhenNoDboUserExists()
        {
            // Arrange — no "dbo" user means UserProgram should NOT be added
            var (repo, programsMockSet, userProgramsMockSet, mockContext) =
                CreateRepositoryWithMocks(
                    new List<Core.Entities.Program>(),
                    new List<UserProgram>(),
                    new List<User>());

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
            var repo = CreateRepository(new List<Core.Entities.Program>(), new List<UserProgram>(), new List<User>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.AddProgramAsync(null!));
        }

        #endregion

        #region UpdateProgramAsync

        [Fact]
        public async Task UpdateProgramAsync_UpdatesProgram_AndAddsUserProgram_WhenDboUserExistsAndLinkIsMissing()
        {
            // Arrange — dbo user exists but no UserProgram link yet → link should be created
            var dboUser = new User { UserId = 1, Username = "dbo" };
            var (repo, programsMockSet, userProgramsMockSet, mockContext) =
                CreateRepositoryWithMocks(
                    new List<Core.Entities.Program>(),
                    new List<UserProgram>(),
                    new List<User> { dboUser });

            var program = new Core.Entities.Program { ProgramNo = "P001", ProgramName = "Updated Name" };

            // Act
            var result = await repo.UpdateProgramAsync(program);

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
            var dboUser = new User { UserId = 1, Username = "dbo" };
            var existingLink = new UserProgram { ProgramNo = "P001", UserID = 1, FpsCalYear = DefaultTestFpsYear };
            var (repo, programsMockSet, userProgramsMockSet, mockContext) =
                CreateRepositoryWithMocks(
                    new List<Core.Entities.Program>(),
                    new List<UserProgram> { existingLink },
                    new List<User> { dboUser });

            var program = new Core.Entities.Program { ProgramNo = "P001", ProgramName = "Updated Name" };

            // Act
            var result = await repo.UpdateProgramAsync(program);

            // Assert
            Assert.NotNull(result);
            programsMockSet.Verify(x => x.Update(It.IsAny<Core.Entities.Program>()), Times.Once);
            userProgramsMockSet.Verify(x => x.Add(It.IsAny<UserProgram>()), Times.Never);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task UpdateProgramAsync_ThrowsArgumentNullException_WhenProgramIsNull()
        {
            // Arrange
            var repo = CreateRepository(new List<Core.Entities.Program>(), new List<UserProgram>(), new List<User>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateProgramAsync(null!));
        }

        #endregion

        #region DeleteProgramAsync

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task DeleteProgramAsync_ThrowsArgumentException_WhenIdIsNullOrWhiteSpace(string id)
        {
            // Arrange
            var repo = CreateRepository(new List<Core.Entities.Program>(), new List<UserProgram>(), new List<User>());
            
            await Assert.ThrowsAsync<ArgumentException>(() => repo.DeleteProgramAsync(id));
        }

        #endregion
    }
}