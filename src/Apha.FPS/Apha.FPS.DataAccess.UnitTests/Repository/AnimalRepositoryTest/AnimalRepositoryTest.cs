using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
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
            IEnumerable<AnimalRequest> animalRequests,
            IEnumerable<Project>? projects = null)
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
            projectRepo.Get().Returns((projects ?? Enumerable.Empty<Project>()).AsQueryable());

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

        /// <summary>
        /// Creates an AnimalRepository wired with joined Animals, AnimalRequests and Projects
        /// so that GetAnimalCostAsync and GetAnimalRateByIdAsync can be exercised end-to-end
        /// in-memory.
        /// </summary>
        private static AnimalRepository CreateRepositoryWithJoinData(
            IEnumerable<Animal> animals,
            IEnumerable<AnimalRequest> animalRequests,
            IEnumerable<Project> projects)
        {
            return CreateRepository(animals, animalRequests, projects);
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

        #region Get

        [Fact]
        public void Get_ReturnsQueryable_WhenProjectsJoinIsEmpty()
        {
            // Arrange — no matching projects so join yields no rows
            var animalRequests = new List<AnimalRequest>
            {
                new() { IndCounter = 1, JobCode = "JOB001", AnimalType = "Cat" }
            };
            var repo = CreateRepository(new List<Animal>(), animalRequests);

            // Act
            var result = repo.Get();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IQueryable<AnimalRequest>>(result);
        }

        [Fact]
        public void Get_ReturnsMatchingRequests_WhenProjectsMatch()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ParentProject = "JOB001", ProjectTitle = "T", Program = "P",
                        Customer = "C", Disease = "D", Contract = "CT",
                        ProjectStatus = "A", IncomeAccountCode = "I", IsDefraProject = 0 }
            };
            var animalRequests = new List<AnimalRequest>
            {
                new() { IndCounter = 1, JobCode = "JOB001", AnimalType = "Cat" },
                new() { IndCounter = 2, JobCode = "JOB002", AnimalType = "Dog" }
            };
            var repo = CreateRepository(new List<Animal>(), animalRequests, projects);

            // Act
            var result = repo.Get().ToList();

            // Assert — only the request whose JobCode matches a ParentProject is returned
            Assert.Single(result);
            Assert.Equal("JOB001", result[0].JobCode);
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

        #region GetAnimalCostAsync

        [Fact]
        public async Task GetAnimalCostAsync_ReturnsPagedResults_WhenDataExists()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ParentProject = "JOB001", Program = "PRG01", ProjectTitle = "T",
                        Customer = "C", Disease = "D", Contract = "CT",
                        ProjectStatus = "A", IncomeAccountCode = "I", IsDefraProject = 0 }
            };
            var animals = new List<Animal>
            {
                new() { AnimalType = "Cat", DailyRate = 10m, DefraDailyRate = 20m }
            };
            var animalRequests = new List<AnimalRequest>
            {
                new() { IndCounter = 1, JobCode = "JOB001", AnimalType = "Cat",
                        NumberOfDays = 3, NumberOfAnimals = 2 }
            };

            var repo = CreateRepositoryWithJoinData(animals, animalRequests, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAnimalCostAsync(query, "JOB001");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.PaginationData.TotalRecords);

            var row = result.Data.First();
            Assert.Equal("Cat", row.AnimalType);
            Assert.Equal(3, row.NumberOfDays);
            Assert.Equal(2, row.NumberOfAnimals);
            Assert.Equal(6, row.TotalDays);                 // NumberOfAnimals * NumberOfDays
            Assert.Equal(10m, row.DailyRate);               // non-Defra rate
            Assert.Equal(60m, row.AnimalCost);              // 3 * 2 * 10
        }

        [Fact]
        public async Task GetAnimalCostAsync_UsesDefraDailyRate_WhenProjectIsDefra()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ParentProject = "JOB001", Program = "PRG01", ProjectTitle = "T",
                        Customer = "C", Disease = "D", Contract = "CT",
                        ProjectStatus = "A", IncomeAccountCode = "I", IsDefraProject = -1 }
            };
            var animals = new List<Animal>
            {
                new() { AnimalType = "Cat", DailyRate = 10m, DefraDailyRate = 25m }
            };
            var animalRequests = new List<AnimalRequest>
            {
                new() { IndCounter = 1, JobCode = "JOB001", AnimalType = "Cat",
                        NumberOfDays = 2, NumberOfAnimals = 3 }
            };

            var repo = CreateRepositoryWithJoinData(animals, animalRequests, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAnimalCostAsync(query, "JOB001");

            // Assert
            var row = result.Data.First();
            Assert.Equal(25m, row.DailyRate);               // Defra rate applied
            Assert.Equal(150m, row.AnimalCost);             // 2 * 3 * 25
        }

        [Fact]
        public async Task GetAnimalCostAsync_ReturnsEmptyPagedResult_WhenNoMatchingJobCode()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ParentProject = "JOB001", Program = "PRG01", ProjectTitle = "T",
                        Customer = "C", Disease = "D", Contract = "CT",
                        ProjectStatus = "A", IncomeAccountCode = "I", IsDefraProject = 0 }
            };
            var animals = new List<Animal>
            {
                new() { AnimalType = "Cat", DailyRate = 10m }
            };
            var animalRequests = new List<AnimalRequest>
            {
                new() { IndCounter = 1, JobCode = "JOB001", AnimalType = "Cat" }
            };

            var repo = CreateRepositoryWithJoinData(animals, animalRequests, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAnimalCostAsync(query, "JOB999");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAnimalCostAsync_AppliesPaging_WhenMultipleRecordsExist()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ParentProject = "JOB001", Program = "PRG01", ProjectTitle = "T",
                        Customer = "C", Disease = "D", Contract = "CT",
                        ProjectStatus = "A", IncomeAccountCode = "I", IsDefraProject = 0 }
            };
            var animals = new List<Animal>
            {
                new() { AnimalType = "Cat", DailyRate = 10m },
                new() { AnimalType = "Dog", DailyRate = 15m },
                new() { AnimalType = "Pig", DailyRate = 20m }
            };
            var animalRequests = new List<AnimalRequest>
            {
                new() { IndCounter = 1, JobCode = "JOB001", AnimalType = "Cat",  NumberOfDays = 1, NumberOfAnimals = 1 },
                new() { IndCounter = 2, JobCode = "JOB001", AnimalType = "Dog",  NumberOfDays = 1, NumberOfAnimals = 1 },
                new() { IndCounter = 3, JobCode = "JOB001", AnimalType = "Pig",  NumberOfDays = 1, NumberOfAnimals = 1 }
            };

            var repo = CreateRepositoryWithJoinData(animals, animalRequests, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 2 };

            // Act
            var result = await repo.GetAnimalCostAsync(query, "JOB001");

            // Assert
            Assert.Equal(2, result.Data.Count());            // page size respected
            Assert.Equal(3, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetAnimalCostAsync_AppliesSortByAnimalType_Ascending()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ParentProject = "JOB001", Program = "PRG01", ProjectTitle = "T",
                        Customer = "C", Disease = "D", Contract = "CT",
                        ProjectStatus = "A", IncomeAccountCode = "I", IsDefraProject = 0 }
            };
            var animals = new List<Animal>
            {
                new() { AnimalType = "Dog", DailyRate = 15m },
                new() { AnimalType = "Cat", DailyRate = 10m }
            };
            var animalRequests = new List<AnimalRequest>
            {
                new() { IndCounter = 1, JobCode = "JOB001", AnimalType = "Dog", NumberOfDays = 1, NumberOfAnimals = 1 },
                new() { IndCounter = 2, JobCode = "JOB001", AnimalType = "Cat", NumberOfDays = 1, NumberOfAnimals = 1 }
            };

            var repo = CreateRepositoryWithJoinData(animals, animalRequests, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "animaltype", Descending = false };

            // Act
            var result = await repo.GetAnimalCostAsync(query, "JOB001");

            // Assert
            Assert.Equal("Cat", result.Data.First().AnimalType);
            Assert.Equal("Dog", result.Data.Skip(1).First().AnimalType);
        }

        [Fact]
        public async Task GetAnimalCostAsync_AppliesSortByAnimalType_Descending()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ParentProject = "JOB001", Program = "PRG01", ProjectTitle = "T",
                        Customer = "C", Disease = "D", Contract = "CT",
                        ProjectStatus = "A", IncomeAccountCode = "I", IsDefraProject = 0 }
            };
            var animals = new List<Animal>
            {
                new() { AnimalType = "Cat", DailyRate = 10m },
                new() { AnimalType = "Dog", DailyRate = 15m }
            };
            var animalRequests = new List<AnimalRequest>
            {
                new() { IndCounter = 1, JobCode = "JOB001", AnimalType = "Cat", NumberOfDays = 1, NumberOfAnimals = 1 },
                new() { IndCounter = 2, JobCode = "JOB001", AnimalType = "Dog", NumberOfDays = 1, NumberOfAnimals = 1 }
            };

            var repo = CreateRepositoryWithJoinData(animals, animalRequests, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "animaltype", Descending = true };

            // Act
            var result = await repo.GetAnimalCostAsync(query, "JOB001");

            // Assert
            Assert.Equal("Dog", result.Data.ElementAt(0).AnimalType);
            Assert.Equal("Cat", result.Data.ElementAt(1).AnimalType);
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