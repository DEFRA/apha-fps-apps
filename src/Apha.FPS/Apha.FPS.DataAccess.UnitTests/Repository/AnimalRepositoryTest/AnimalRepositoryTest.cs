using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.AnimalRepositoryTest
{
    public class AnimalRepositoryTest
    {
        private const int DefaultTestFpsYear = 2024;

        /// <summary>
        /// Creates an AnimalRepository with in-memory Animals and AnimalRequests data.
        /// IProjectRepository is substituted via NSubstitute with an empty queryable
        /// — JOIN logic is covered by integration tests.
        /// </summary>
        private static AnimalRepository CreateRepository(
            IEnumerable<Animal> animals,
            IEnumerable<AnimalRequest> animalRequests)
        {
            var fpsYearContext = Substitute.For<IFpsYearContext>();
            fpsYearContext.FPSYear.Returns(DefaultTestFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);

            var animalsMockSet = RepositoryTestHelper.CreateMockDbSet(animals);
            var animalRequestsMockSet = RepositoryTestHelper.CreateMockDbSet(animalRequests);

            RepositoryTestHelper.SetupDbSetOperations(animalRequestsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.Animals).Returns(animalsMockSet.Object);
            mockContext.Setup(x => x.AnimalRequests).Returns(animalRequestsMockSet.Object);

            var projectRepo = Substitute.For<IProjectRepository>();
            projectRepo.Get().Returns(Enumerable.Empty<Project>().AsQueryable());

            return new AnimalRepository(mockContext.Object, projectRepo);
        }

        /// <summary>
        /// Returns the mocked DbSets alongside the repository for tests that need to verify calls.
        /// </summary>
        private static (AnimalRepository Repo, Mock<DbSet<AnimalRequest>> AnimalRequestsDbSet, Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(
                IEnumerable<Animal> animals,
                IEnumerable<AnimalRequest> animalRequests)
        {
            var fpsYearContext = Substitute.For<IFpsYearContext>();
            fpsYearContext.FPSYear.Returns(DefaultTestFpsYear);

            var animalList = animalRequests.ToList();

            var (mockContext, animalRequestsMockSet) =
                RepositoryTestHelper.CreateRepositoryContext<FpsDbContext, AnimalRequest>(
                    animalList, fpsYearContext);

            animalRequestsMockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids =>
            {
                var id = (int)ids[0];
                return animalList.FirstOrDefault(e => e.IndCounter == id);
            });
            animalRequestsMockSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).Returns<object[]>(ids =>
            {
                var id = (int)ids[0];
                var entity = animalList.FirstOrDefault(e => e.IndCounter == id);
                return new ValueTask<AnimalRequest?>(entity);
            });

            var animalsMockSet = RepositoryTestHelper.CreateMockDbSet(animals);
            mockContext.Setup(x => x.Animals).Returns(animalsMockSet.Object);
            mockContext.Setup(x => x.AnimalRequests).Returns(animalRequestsMockSet.Object);

            var projectRepo = Substitute.For<IProjectRepository>();
            projectRepo.Get().Returns(Enumerable.Empty<Project>().AsQueryable());

            var repo = new AnimalRepository(mockContext.Object, projectRepo);
            return (repo, animalRequestsMockSet, mockContext);
        }

        #region Constructor

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenDbContextIsNull()
        {
            // Arrange
            var projectRepo = Substitute.For<IProjectRepository>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new AnimalRepository(null!, projectRepo));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenProjectRepositoryIsNull()
        {
            // Arrange
            var fpsYearContext = Substitute.For<IFpsYearContext>();
            fpsYearContext.FPSYear.Returns(DefaultTestFpsYear);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new AnimalRepository(mockContext.Object, null!));
        }

        #endregion

        #region GetAnimalLookup

        [Fact]
        public async Task GetAnimalLookup_ReturnsAllAnimals_WhenDataExists()
        {
            // Arrange
            var animals = new List<Animal>
            {
                new() { AnimalType = "Cat", DailyRate = 10m },
                new() { AnimalType = "Dog", DailyRate = 15m }
            };
            var repo = CreateRepository(animals, new List<AnimalRequest>());

            // Act
            var result = await repo.GetAnimalLookup();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAnimalLookup_ReturnsEmptyList_WhenNoAnimalsExist()
        {
            // Arrange
            var repo = CreateRepository(new List<Animal>(), new List<AnimalRequest>());

            // Act
            var result = await repo.GetAnimalLookup();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region AddAnimalCostAsync

        [Fact]
        public async Task AddAnimalCostAsync_AddsAndReturnsEntity_WhenValid()
        {
            // Arrange
            var (repo, animalRequestsMockSet, mockContext) =
                CreateRepositoryWithMocks(new List<Animal>(), new List<AnimalRequest>());

            var newRequest = new AnimalRequest
            {
                IndCounter = 1,
                JobCode = "JOB001",
                AnimalType = "Cat",
                NumberOfDays = 5,
                NumberOfAnimals = 2
            };

            // Act
            var result = await repo.AddAnimalCostAsync(newRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("JOB001", result.JobCode);
            Assert.Equal("Cat", result.AnimalType);
            animalRequestsMockSet.Verify(x => x.Add(It.IsAny<AnimalRequest>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task AddAnimalCostAsync_ThrowsArgumentNullException_WhenAnimalRequestIsNull()
        {
            // Arrange
            var repo = CreateRepository(new List<Animal>(), new List<AnimalRequest>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repo.AddAnimalCostAsync(null!));
        }

        #endregion

        #region UpdateAnimalCostAsync

        [Fact]
        public async Task UpdateAnimalCostAsync_UpdatesAndReturnsEntity_WhenEntityExists()
        {
            // Arrange
            var existing = new AnimalRequest
            {
                IndCounter = 1,
                JobCode = "JOB001",
                AnimalType = "Cat",
                NumberOfDays = 3,
                NumberOfAnimals = 1
            };
            var (repo, _, mockContext) =
                CreateRepositoryWithMocks(new List<Animal>(), new List<AnimalRequest> { existing });

            var update = new AnimalRequest
            {
                IndCounter = 1,
                JobCode = "JOB002",
                AnimalType = "Dog",
                NumberOfDays = 7,
                NumberOfAnimals = 2
            };

            // Act
            var result = await repo.UpdateAnimalCostAsync(update);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("JOB002", result.JobCode);
            Assert.Equal("Dog", result.AnimalType);
            Assert.Equal(7, result.NumberOfDays);
            Assert.Equal(2, result.NumberOfAnimals);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task UpdateAnimalCostAsync_ThrowsArgumentNullException_WhenAnimalRequestIsNull()
        {
            // Arrange
            var repo = CreateRepository(new List<Animal>(), new List<AnimalRequest>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repo.UpdateAnimalCostAsync(null!));
        }

        [Fact]
        public async Task UpdateAnimalCostAsync_ThrowsInvalidOperationException_WhenEntityNotFound()
        {
            // Arrange
            var (repo, _, _) =
                CreateRepositoryWithMocks(new List<Animal>(), new List<AnimalRequest>());

            var update = new AnimalRequest
            {
                IndCounter = 99,
                AnimalType = "Cat"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                repo.UpdateAnimalCostAsync(update));

            Assert.Contains("Cat", ex.Message);
        }

        #endregion

        #region DeleteJobAnimalCostAsync

        [Fact]
        public async Task DeleteJobAnimalCostAsync_ReturnsTrue_WhenEntityExists()
        {
            // Arrange
            var existing = new AnimalRequest { IndCounter = 1, JobCode = "JOB001", AnimalType = "Cat" };
            var (repo, animalRequestsMockSet, mockContext) =
                CreateRepositoryWithMocks(new List<Animal>(), new List<AnimalRequest> { existing });

            // Act
            var result = await repo.DeleteJobAnimalCostAsync(1);

            // Assert
            Assert.True(result);
            RepositoryTestHelper.VerifyRemove(animalRequestsMockSet);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task DeleteJobAnimalCostAsync_ReturnsFalse_WhenEntityNotFound()
        {
            // Arrange
            var (repo, _, _) =
                CreateRepositoryWithMocks(new List<Animal>(), new List<AnimalRequest>());

            // Act
            var result = await repo.DeleteJobAnimalCostAsync(99);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeleteJobAnimalCostAsync_ThrowsArgumentOutOfRangeException_WhenIndCounterIsNotPositive(int indCounter)
        {
            // Arrange
            var repo = CreateRepository(new List<Animal>(), new List<AnimalRequest>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                repo.DeleteJobAnimalCostAsync(indCounter));
        }

        #endregion
    }
}