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
        private const int DefaultFpsYear = 2024;
        private const int DefaultUserId  = 42;
        private const string DefaultUserEmail = "test@example.com";

        private static Mock<IFpsRequestContext> CreateMockFpsYearContext(int year = DefaultFpsYear)
        {
            var mock = new Mock<IFpsRequestContext>();
            mock.Setup(x => x.FpsYear).Returns(year);
            mock.Setup(x => x.UserEmailId).Returns(DefaultUserEmail);
            return mock;
        }

        /// <summary>
        /// Creates an AnimalRepository with the specified in-memory DbSets.
        /// GetAnimalCostAsync() and GetAnimalRateByIdAsync() use multi-table JOINs across
        /// AnimalRequestViews, Animals, and ProjectViews and are covered by integration tests.
        /// </summary>
        private static AnimalRepository CreateRepository(
            IEnumerable<Animal>? animals = null,
            IEnumerable<AnimalRequest>? animalRequests = null,
            IEnumerable<AnimalRequestView>? animalRequestViews = null,
            IEnumerable<ProjectView>? projectViews = null,
            int fpsYear = DefaultFpsYear)
        {
            var mockFpsYearContext = CreateMockFpsYearContext(fpsYear);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            if (animals != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(animals);
                RepositoryTestHelper.SetupDbSetOperations(mockSet);
                mockContext.Setup(x => x.Animals).Returns(mockSet.Object);
            }

            if (animalRequests != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(animalRequests);
                RepositoryTestHelper.SetupDbSetOperations(mockSet);
                mockContext.Setup(x => x.AnimalRequests).Returns(mockSet.Object);
            }

            if (animalRequestViews != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(animalRequestViews);
                RepositoryTestHelper.SetupDbSetOperations(mockSet);
                mockContext.Setup(x => x.AnimalRequestViews).Returns(mockSet.Object);
            }

            if (projectViews != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(projectViews);
                RepositoryTestHelper.SetupDbSetOperations(mockSet);
                mockContext.Setup(x => x.ProjectViews).Returns(mockSet.Object);
            }

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new AnimalRepository(mockContext.Object, mockFpsYearContext.Object);
        }

        /// <summary>
        /// Returns the mocked DbSets alongside the repository for tests that need to verify calls.
        /// </summary>
        private static (
            AnimalRepository Repo,
            Mock<DbSet<AnimalRequest>> AnimalRequestsDbSet,
            Mock<DbSet<AnimalRequestLog>> AnimalRequestLogsDbSet,
            Mock<FpsDbContext> Context,
            Mock<IFpsRequestContext> YearContext)
            CreateRepositoryWithMocks(IEnumerable<AnimalRequest>? animalRequests = null, int fpsYear = DefaultFpsYear)
        {
            var mockFpsYearContext = CreateMockFpsYearContext(fpsYear);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            var animalRequestsMockSet = RepositoryTestHelper.CreateMockDbSet(animalRequests ?? []);
            RepositoryTestHelper.SetupDbSetOperations(animalRequestsMockSet);
            mockContext.Setup(x => x.AnimalRequests).Returns(animalRequestsMockSet.Object);

            var animalRequestLogsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<AnimalRequestLog>());
            mockContext.Setup(x => x.AnimalRequestLogs).Returns(animalRequestLogsMockSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var repo = new AnimalRepository(mockContext.Object, mockFpsYearContext.Object);
            return (repo, animalRequestsMockSet, animalRequestLogsMockSet, mockContext, mockFpsYearContext);
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
            var (repo, animalRequestsMockSet, animalRequestLogsMockSet, mockContext, _) = CreateRepositoryWithMocks([]);
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
            animalRequestLogsMockSet.Verify(m => m.Add(It.Is<AnimalRequestLog>(log =>
                log.JobCode == "JC001" &&
                log.AnimalType == "CAT" &&
                log.InsertDelete == "I")), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task AddAnimalCostAsync_ReturnsTheSameEntity_ThatWasAdded()
        {
            // Arrange
            var (repo, _, _, _, _) = CreateRepositoryWithMocks([]);
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

        [Fact]
        public async Task UpdateAnimalCostAsync_UpdatesEntity_WhenFound()
        {
            // Arrange
            var existing = new AnimalRequest
            {
                IndCounter      = 5,
                JobCode         = "OLD",
                AnimalType      = "CAT",
                NumberOfDays    = 1.0,
                NumberOfAnimals = 1.0
            };

            var mockFpsYearContext = CreateMockFpsYearContext(DefaultFpsYear);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            var animalRequestsMockSet = RepositoryTestHelper.CreateMockDbSet<AnimalRequest>([existing]);
            RepositoryTestHelper.SetupDbSetOperations(animalRequestsMockSet);
            animalRequestsMockSet
                .Setup(x => x.FindAsync(It.IsAny<object?[]?>()))
                .ReturnsAsync(existing);

            mockContext.Setup(x => x.AnimalRequests).Returns(animalRequestsMockSet.Object);

            var animalRequestLogsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<AnimalRequestLog>());
            mockContext.Setup(x => x.AnimalRequestLogs).Returns(animalRequestLogsMockSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var repo = new AnimalRepository(mockContext.Object, mockFpsYearContext.Object);

            var updated = new AnimalRequest
            {
                IndCounter      = 5,
                JobCode         = "JC001",
                AnimalType      = "DOG",
                NumberOfDays    = 3.0,
                NumberOfAnimals = 4.0
            };

            // Act
            var result = await repo.UpdateAnimalCostAsync(updated);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("JC001",        result.JobCode);
            Assert.Equal("DOG",          result.AnimalType);
            Assert.Equal(3.0,            result.NumberOfDays);
            Assert.Equal(4.0,            result.NumberOfAnimals);
            Assert.Equal(DefaultFpsYear, result.FpsYear);
            animalRequestLogsMockSet.Verify(m => m.Add(It.Is<AnimalRequestLog>(log =>
                log.JobCode == "JC001" &&
                log.AnimalType == "DOG" &&
                log.InsertDelete == "U")), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
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
            var mockFpsYearContext = new Mock<IFpsRequestContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            var animalRequestsMockSet = RepositoryTestHelper.CreateMockDbSet<AnimalRequest>([]);
            RepositoryTestHelper.SetupDbSetOperations(animalRequestsMockSet);
            animalRequestsMockSet
                .Setup(x => x.FindAsync(It.IsAny<object?[]?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AnimalRequest?)null);

            mockContext.Setup(x => x.AnimalRequests).Returns(animalRequestsMockSet.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var repo = new AnimalRepository(mockContext.Object, mockFpsYearContext.Object);

            // Act
            var result = await repo.DeleteJobAnimalCostAsync(999);

            // Assert
            Assert.False(result);
        }        

        [Fact]
        public async Task DeleteJobAnimalCostAsync_DoesNotCallRemove_WhenNotFound()
        {
            // Arrange
            var mockFpsYearContext = new Mock<IFpsRequestContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            var animalRequestsMockSet = RepositoryTestHelper.CreateMockDbSet<AnimalRequest>([]);
            RepositoryTestHelper.SetupDbSetOperations(animalRequestsMockSet);
            animalRequestsMockSet
                .Setup(x => x.FindAsync(It.IsAny<object?[]?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AnimalRequest?)null);

            mockContext.Setup(x => x.AnimalRequests).Returns(animalRequestsMockSet.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var repo = new AnimalRepository(mockContext.Object, mockFpsYearContext.Object);

            // Act
            await repo.DeleteJobAnimalCostAsync(42);

            // Assert
            animalRequestsMockSet.Verify(x => x.Remove(It.IsAny<AnimalRequest>()), Times.Never);
        }

        [Fact]
        public async Task DeleteJobAnimalCostAsync_ReturnsTrue_WhenFound()
        {
            // Arrange
            var entity = new AnimalRequest { IndCounter = 1, JobCode = "JC001", AnimalType = "CAT" };

            var mockFpsYearContext = CreateMockFpsYearContext();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            var animalRequestsMockSet = RepositoryTestHelper.CreateMockDbSet<AnimalRequest>([entity]);
            RepositoryTestHelper.SetupDbSetOperations(animalRequestsMockSet);
            animalRequestsMockSet
                .Setup(x => x.FindAsync(It.IsAny<object?[]?>()))
                .ReturnsAsync(entity);

            mockContext.Setup(x => x.AnimalRequests).Returns(animalRequestsMockSet.Object);

            var animalRequestLogsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<AnimalRequestLog>());
            mockContext.Setup(x => x.AnimalRequestLogs).Returns(animalRequestLogsMockSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var repo = new AnimalRepository(mockContext.Object, mockFpsYearContext.Object);

            // Act
            var result = await repo.DeleteJobAnimalCostAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteJobAnimalCostAsync_CallsRemoveAndSave_WhenFound()
        {
            // Arrange
            var entity = new AnimalRequest { IndCounter = 7, JobCode = "JC007", AnimalType = "DOG" };

            var mockFpsYearContext = CreateMockFpsYearContext();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            var animalRequestsMockSet = RepositoryTestHelper.CreateMockDbSet<AnimalRequest>([entity]);
            RepositoryTestHelper.SetupDbSetOperations(animalRequestsMockSet);
            animalRequestsMockSet
                .Setup(x => x.FindAsync(It.IsAny<object?[]?>()))
                .ReturnsAsync(entity);

            mockContext.Setup(x => x.AnimalRequests).Returns(animalRequestsMockSet.Object);

            var animalRequestLogsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<AnimalRequestLog>());
            mockContext.Setup(x => x.AnimalRequestLogs).Returns(animalRequestLogsMockSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var repo = new AnimalRepository(mockContext.Object, mockFpsYearContext.Object);

            // Act
            await repo.DeleteJobAnimalCostAsync(7);

            // Assert
            animalRequestsMockSet.Verify(x => x.Remove(It.IsAny<AnimalRequest>()), Times.Once);
            animalRequestLogsMockSet.Verify(m => m.Add(It.Is<AnimalRequestLog>(log =>
                log.JobCode == "JC007" &&
                log.AnimalType == "DOG" &&
                log.InsertDelete == "D")), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        #endregion

        #region GetTotalAnimalCostAsync Tests

        [Fact]
        public async Task GetTotalAnimalCostAsync_ReturnsZero_WhenNoMatchingRecords()
        {
            // Arrange — empty collections produce empty JOIN result
            var repo = CreateRepository(
                animalRequestViews: [],
                animals: [],
                projectViews: []);

            // Act
            var result = await repo.GetTotalAnimalCostAsync("JOB001");

            // Assert
            Assert.Equal(0m, result);
        }

        [Fact]
        public async Task GetTotalAnimalCostAsync_ReturnsCorrectTotal_WithMatchingData()
        {
            // Arrange
            // Record 1: 5 days * 2 animals * 10/day = 100
            // Record 2: 3 days * 4 animals * 10/day = 120  →  total = 220
            var animalRequestViews = new List<AnimalRequestView>
            {
                new() { IndCounter = 1, JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = 5.0, NumberOfAnimals = 2.0, UserId = DefaultUserId, UserEmail = DefaultUserEmail },
                new() { IndCounter = 2, JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = 3.0, NumberOfAnimals = 4.0, UserId = DefaultUserId, UserEmail = DefaultUserEmail }
            };
            var animals = new List<Animal>
            {
                new() { AnimalType = "CAT", DailyRate = 10m, DefraDailyRate = 15m }
            };
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "JOB001", UserId = DefaultUserId, IsDefraProject = 0 }
            };

            var repo = CreateRepository(
                animalRequestViews: animalRequestViews,
                animals: animals,
                projectViews: projectViews);

            // Act
            var result = await repo.GetTotalAnimalCostAsync("JOB001");

            // Assert
            Assert.Equal(220m, result);
        }

        [Fact]
        public async Task GetTotalAnimalCostAsync_UsesDefraDailyRate_WhenIsDefraProject()
        {
            // Arrange — IsDefraProject = -1 → uses DefraDailyRate (15m)
            // 4 days * 3 animals * 15/day = 180
            var animalRequestViews = new List<AnimalRequestView>
            {
                new() { IndCounter = 1, JobCode = "JOB001", AnimalType = "DOG", NumberOfDays = 4.0, NumberOfAnimals = 3.0, UserId = DefaultUserId, UserEmail = DefaultUserEmail }
            };
            var animals = new List<Animal>
            {
                new() { AnimalType = "DOG", DailyRate = 10m, DefraDailyRate = 15m }
            };
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "JOB001", UserId = DefaultUserId, IsDefraProject = -1 }
            };

            var repo = CreateRepository(
                animalRequestViews: animalRequestViews,
                animals: animals,
                projectViews: projectViews);

            // Act
            var result = await repo.GetTotalAnimalCostAsync("JOB001");

            // Assert — 4 * 3 * 15 = 180
            Assert.Equal(180m, result);
        }

        [Fact]
        public async Task GetTotalAnimalCostAsync_ExcludesRecords_WhenJobCodeDoesNotMatch()
        {
            // Arrange — records exist but for a different JobCode
            var animalRequestViews = new List<AnimalRequestView>
            {
                new() { IndCounter = 1, JobCode = "OTHER", AnimalType = "CAT", NumberOfDays = 5.0, NumberOfAnimals = 2.0, UserId = DefaultUserId, UserEmail = DefaultUserEmail }
            };
            var animals = new List<Animal>
            {
                new() { AnimalType = "CAT", DailyRate = 10m, DefraDailyRate = 15m }
            };
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "OTHER", UserId = DefaultUserId, IsDefraProject = 0 }
            };

            var repo = CreateRepository(
                animalRequestViews: animalRequestViews,
                animals: animals,
                projectViews: projectViews);

            // Act
            var result = await repo.GetTotalAnimalCostAsync("JOB001");

            // Assert
            Assert.Equal(0m, result);
        }

        #endregion

        #region GetAnimalCostViewByIdAsync Tests

        [Fact]
        public async Task GetAnimalCostViewByIdAsync_ReturnsNull_WhenNoMatchingRecord()
        {
            // Arrange — empty collections produce no JOIN result
            var repo = CreateRepository(
                animalRequestViews: [],
                animals: [],
                projectViews: []);

            // Act
            var result = await repo.GetAnimalCostViewByIdAsync(999, "JOB001");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAnimalCostViewByIdAsync_ReturnsRecord_WithComputedAnimalCost()
        {
            // Arrange — 5 days * 2 animals * 10/day = AnimalCost 100
            var animalRequestViews = new List<AnimalRequestView>
            {
                new() { IndCounter = 1, JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = 5.0, NumberOfAnimals = 2.0, UserId = DefaultUserId, UserEmail = DefaultUserEmail }
            };
            var animals = new List<Animal>
            {
                new() { AnimalType = "CAT", DailyRate = 10m, DefraDailyRate = 15m }
            };
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "JOB001", UserId = DefaultUserId, IsDefraProject = 0 }
            };

            var repo = CreateRepository(
                animalRequestViews: animalRequestViews,
                animals: animals,
                projectViews: projectViews);

            // Act
            var result = await repo.GetAnimalCostViewByIdAsync(1, "JOB001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1,        result.IndCounter);
            Assert.Equal("JOB001", result.JobCode);
            Assert.Equal("CAT",    result.AnimalType);
            Assert.Equal(100m,     result.AnimalCost);   // 5 * 2 * 10
            Assert.Equal(5.0,      result.NumberOfDays);
            Assert.Equal(2.0,      result.NumberOfAnimals);
        }

        [Fact]
        public async Task GetAnimalCostViewByIdAsync_ReturnsNull_WhenIndCounterDoesNotMatch()
        {
            // Arrange — record exists but IndCounter differs
            var animalRequestViews = new List<AnimalRequestView>
            {
                new() { IndCounter = 1, JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = 5.0, NumberOfAnimals = 2.0, UserId = DefaultUserId, UserEmail = DefaultUserEmail }
            };
            var animals = new List<Animal>
            {
                new() { AnimalType = "CAT", DailyRate = 10m, DefraDailyRate = 15m }
            };
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "JOB001", UserId = DefaultUserId, IsDefraProject = 0 }
            };

            var repo = CreateRepository(
                animalRequestViews: animalRequestViews,
                animals: animals,
                projectViews: projectViews);

            // Act
            var result = await repo.GetAnimalCostViewByIdAsync(999, "JOB001");

            // Assert
            Assert.Null(result);
        }

        #endregion
    }
}