using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.AnimalRepositoryTest
{
    public class AnimalRepositoryTest
    {
        /// <summary>
        /// Creates an AnimalRepository with the specified in-memory DbSets.
        /// GetAnimalCostAsync() and GetAnimalRateByIdAsync() use multi-table JOINs across
        /// AnimalRequestViews, Animals, and ProjectViews and are covered by integration tests.
        /// </summary>
        private static AnimalRepository CreateRepository(
            IEnumerable<Animal>? animals = null,
            IEnumerable<AnimalRequest>? animalRequests = null)
        {
            var mockFpsYearContext = new Mock<IFpsYearContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            if (animals != null)
            {
                var animalsMockSet = RepositoryTestHelper.CreateMockDbSet(animals);
                RepositoryTestHelper.SetupDbSetOperations(animalsMockSet);
                mockContext.Setup(x => x.Animals).Returns(animalsMockSet.Object);
            }

            if (animalRequests != null)
            {
                var animalRequestsMockSet = RepositoryTestHelper.CreateMockDbSet(animalRequests);
                RepositoryTestHelper.SetupDbSetOperations(animalRequestsMockSet);
                mockContext.Setup(x => x.AnimalRequests).Returns(animalRequestsMockSet.Object);
            }

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new AnimalRepository(mockContext.Object);
        }

        /// <summary>
        /// Returns the mocked DbSets alongside the repository for tests that need to verify calls.
        /// </summary>
        private static (
            AnimalRepository Repo,
            Mock<DbSet<AnimalRequest>> AnimalRequestsDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<AnimalRequest>? animalRequests = null)
        {
            var mockFpsYearContext = new Mock<IFpsYearContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            var animalRequestsMockSet = RepositoryTestHelper.CreateMockDbSet(animalRequests ?? []);
            RepositoryTestHelper.SetupDbSetOperations(animalRequestsMockSet);
            mockContext.Setup(x => x.AnimalRequests).Returns(animalRequestsMockSet.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var repo = new AnimalRepository(mockContext.Object);
            return (repo, animalRequestsMockSet, mockContext);
        }

        [Fact]
        public async Task GetAnimalLookup_ReturnsEmptyList_WhenNoAnimals()
        {
            // Arrange
            var repo = CreateRepository(animals: []);

            // Act
            var result = await repo.GetAnimalLookup();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAnimalLookup_MapsFieldsCorrectly()
        {
            // Arrange
            var animals = new List<Animal>
            {
                new()
                {
                    AnimalType     = "CAT",
                    Species        = "Felis catus",
                    SecurityLevel  = "Low",
                    DailyRate      = 10.50m,
                    DefraDailyRate = 13.00m,
                    PlanByWeek     = true,
                    FpsYear     = 2024
                }
            };
            var repo = CreateRepository(animals: animals);

            // Act
            var result = await repo.GetAnimalLookup();

            // Assert
            var animal = Assert.Single(result);
            Assert.Equal("CAT",         animal.AnimalType);
            Assert.Equal("Felis catus", animal.Species);
            Assert.Equal("Low",         animal.SecurityLevel);
            Assert.Equal(10.50m,        animal.DailyRate);
            Assert.Equal(13.00m,        animal.DefraDailyRate);
            Assert.True(animal.PlanByWeek);
            Assert.Equal(2024,          animal.FpsYear);
        }

        

        #region AddAnimalCostAsync Tests

        [Fact]
        public async Task AddAnimalCostAsync_ThrowsArgumentNullException_WhenAnimalRequestIsNull()
        {
            // Arrange
            var repo = CreateRepository(animalRequests: []);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.AddAnimalCostAsync(null!));
        }

        [Fact]
        public async Task AddAnimalCostAsync_AddsAnimalRequest_WhenValid()
        {
            // Arrange
            var (repo, animalRequestsMockSet, mockContext) = CreateRepositoryWithMocks([]);
            var newRequest = new AnimalRequest
            {
                IndCounter      = 1,
                JobCode         = "JC001",
                AnimalType      = "CAT",
                NumberOfDays    = 5.0,
                NumberOfAnimals = 3.0
            };

            // Act
            var result = await repo.AddAnimalCostAsync(newRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("JC001", result.JobCode);
            Assert.Equal("CAT",   result.AnimalType);
            Assert.Equal(5.0,     result.NumberOfDays);
            Assert.Equal(3.0,     result.NumberOfAnimals);
            animalRequestsMockSet.Verify(x => x.Add(It.IsAny<AnimalRequest>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task AddAnimalCostAsync_ReturnsTheSameEntity_ThatWasAdded()
        {
            // Arrange
            var (repo, _, _) = CreateRepositoryWithMocks([]);
            var newRequest = new AnimalRequest
            {
                IndCounter      = 10,
                JobCode         = "JC002",
                AnimalType      = "DOG",
                NumberOfDays    = 2.0,
                NumberOfAnimals = 1.0,
                FpsYear      = 2024
            };

            // Act
            var result = await repo.AddAnimalCostAsync(newRequest);

            // Assert — repository returns the same entity reference
            Assert.Same(newRequest, result);
        }

        #endregion

        #region UpdateAnimalCostAsync Tests

        [Fact]
        public async Task UpdateAnimalCostAsync_ThrowsArgumentNullException_WhenAnimalRequestIsNull()
        {
            // Arrange
            var repo = CreateRepository(animalRequests: []);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateAnimalCostAsync(null!));
        }

        [Fact]
        public async Task UpdateAnimalCostAsync_ThrowsInvalidOperationException_WhenNotFound()
        {
            // Arrange — no AnimalRequest with matching IndCounter in the store
            var repo = CreateRepository(animalRequests: []);
            var request = new AnimalRequest
            {
                IndCounter   = 999,
                JobCode      = "JC001",
                AnimalType   = "CAT",
                NumberOfDays = 5.0,
                NumberOfAnimals = 2.0
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.UpdateAnimalCostAsync(request));
        }

        #endregion

        #region DeleteJobAnimalCostAsync Tests

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task DeleteJobAnimalCostAsync_ThrowsArgumentOutOfRangeException_WhenIndCounterIsNotPositive(int indCounter)
        {
            // Arrange
            var repo = CreateRepository(animalRequests: []);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repo.DeleteJobAnimalCostAsync(indCounter));
        }

        [Fact]
        public async Task DeleteJobAnimalCostAsync_ReturnsFalse_WhenNotFound()
        {
            // Arrange — FindAsync returns null because no matching entity
            var mockFpsYearContext = new Mock<IFpsYearContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            var animalRequestsMockSet = RepositoryTestHelper.CreateMockDbSet<AnimalRequest>([]);
            RepositoryTestHelper.SetupDbSetOperations(animalRequestsMockSet);
            animalRequestsMockSet
                .Setup(x => x.FindAsync(It.IsAny<object?[]?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AnimalRequest?)null);

            mockContext.Setup(x => x.AnimalRequests).Returns(animalRequestsMockSet.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var repo = new AnimalRepository(mockContext.Object);

            // Act
            var result = await repo.DeleteJobAnimalCostAsync(999);

            // Assert
            Assert.False(result);
        }        

        [Fact]
        public async Task DeleteJobAnimalCostAsync_DoesNotCallRemove_WhenNotFound()
        {
            // Arrange
            var mockFpsYearContext = new Mock<IFpsYearContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            var animalRequestsMockSet = RepositoryTestHelper.CreateMockDbSet<AnimalRequest>([]);
            RepositoryTestHelper.SetupDbSetOperations(animalRequestsMockSet);
            animalRequestsMockSet
                .Setup(x => x.FindAsync(It.IsAny<object?[]?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AnimalRequest?)null);

            mockContext.Setup(x => x.AnimalRequests).Returns(animalRequestsMockSet.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var repo = new AnimalRepository(mockContext.Object);

            // Act
            await repo.DeleteJobAnimalCostAsync(42);

            // Assert
            animalRequestsMockSet.Verify(x => x.Remove(It.IsAny<AnimalRequest>()), Times.Never);
        }

        #endregion
    }
}