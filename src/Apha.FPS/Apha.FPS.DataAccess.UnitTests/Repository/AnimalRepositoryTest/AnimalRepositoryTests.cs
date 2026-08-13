using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Linq.Expressions;

namespace Apha.FPS.DataAccess.UnitTests.Repository.AnimalRepositoryTest
{
    public class AnimalRepositoryTests
    {
        private const int DefaultFpsYear = 2025;
        private const string DefaultUserEmail = "test@example.com";

        #region Helpers

        private static Animal BuildAnimal(
            string animalType = "CATTLE",
            string? species = "Bovine",
            string? securityLevel = "L1",
            decimal? dailyRate = 50m,
            decimal? defraDailyRate = 60m,
            bool planByWeek = false,
            int fpsYear = DefaultFpsYear) =>
            new()
            {
                AnimalType = animalType,
                Species = species,
                SecurityLevel = securityLevel,
                DailyRate = dailyRate,
                DefraDailyRate = defraDailyRate,
                PlanByWeek = planByWeek,
                FpsYear = fpsYear
            };

        private static Mock<IFpsRequestContext> CreateRequestContextMock()
        {
            var mock = new Mock<IFpsRequestContext>();
            mock.Setup(x => x.FpsYear).Returns(DefaultFpsYear);
            mock.Setup(x => x.UserEmailId).Returns(DefaultUserEmail);
            return mock;
        }

        private static AnimalRepository CreateRepository(IEnumerable<Animal>? animals = null)
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            if (animals != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(animals);
                RepositoryTestHelper.SetupDbSetOperations(mockSet);
                dbContext.Setup(x => x.Animals).Returns(mockSet.Object);
            }

            RepositoryTestHelper.SetupSaveChanges(dbContext);
            return new AnimalRepository(dbContext.Object, requestCtx.Object);
        }

        private static AnimalRepository CreateRepositoryForGlobalCost(
            IEnumerable<AnimalRequest> animalRequests,
            IEnumerable<Animal> animals)
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext  = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var animalRequestSet = RepositoryTestHelper.CreateMockDbSet(animalRequests);
            RepositoryTestHelper.SetupDbSetOperations(animalRequestSet);
            dbContext.Setup(x => x.AnimalRequests).Returns(animalRequestSet.Object);

            var animalSet = RepositoryTestHelper.CreateMockDbSet(animals);
            RepositoryTestHelper.SetupDbSetOperations(animalSet);
            dbContext.Setup(x => x.Animals).Returns(animalSet.Object);

            RepositoryTestHelper.SetupSaveChanges(dbContext);
            return new AnimalRepository(dbContext.Object, requestCtx.Object);
        }

        private static AnimalRepository CreateRepositoryForProgrammeCost(
            IEnumerable<Project> projects,
            IEnumerable<AnimalRequest> animalRequests,
            IEnumerable<Animal> animals)
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var projectSet = RepositoryTestHelper.CreateMockDbSet(projects);
            RepositoryTestHelper.SetupDbSetOperations(projectSet);
            dbContext.Setup(x => x.Projects).Returns(projectSet.Object);

            var animalRequestSet = RepositoryTestHelper.CreateMockDbSet(animalRequests);
            RepositoryTestHelper.SetupDbSetOperations(animalRequestSet);
            dbContext.Setup(x => x.AnimalRequests).Returns(animalRequestSet.Object);

            var animalSet = RepositoryTestHelper.CreateMockDbSet(animals);
            RepositoryTestHelper.SetupDbSetOperations(animalSet);
            dbContext.Setup(x => x.Animals).Returns(animalSet.Object);

            RepositoryTestHelper.SetupSaveChanges(dbContext);
            return new AnimalRepository(dbContext.Object, requestCtx.Object);
        }

        private static Project BuildProject(
            string parentProject = "J1",
            string program = "PROG1",
            short isDefraProject = 0) =>
            new()
            {
                ParentProject = parentProject,
                ProjectTitle = "Title",
                Program = program,
                Customer = "CUST",
                ProjectStatus = "Open",
                Disease = "TB",
                IsDefraProject = isDefraProject
            };

        private static AnimalRequest BuildAnimalRequest(
            string jobCode = "J1",
            string animalType = "CATTLE",
            double numberOfDays = 2d,
            double numberOfAnimals = 3d,
            int indCounter = 1) =>
            new()
            {
                JobCode = jobCode,
                AnimalType = animalType,
                NumberOfDays = numberOfDays,
                NumberOfAnimals = numberOfAnimals,
                IndCounter = indCounter,
                FpsYear = DefaultFpsYear
            };

        private static (AnimalRepository Repo, Mock<FpsDbContext> Context, Mock<DbSet<Animal>> DbSet)
            CreateRepositoryWithMocks(IEnumerable<Animal>? animals = null)
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var dbSet = RepositoryTestHelper.CreateMockDbSet(animals ?? []);
            RepositoryTestHelper.SetupDbSetOperations(dbSet);
            dbContext.Setup(x => x.Animals).Returns(dbSet.Object);

            RepositoryTestHelper.SetupSaveChanges(dbContext);
            return (new AnimalRepository(dbContext.Object, requestCtx.Object), dbContext, dbSet);
        }

        private static AnimalRequestView BuildAnimalRequestView(
            string jobCode = "J1",
            string animalType = "CATTLE",
            double numberOfDays = 2d,
            double numberOfAnimals = 3d,
            int indCounter = 1,
            int? userId = 1,
            string? userEmail = DefaultUserEmail) =>
            new()
            {
                JobCode = jobCode,
                AnimalType = animalType,
                NumberOfDays = numberOfDays,
                NumberOfAnimals = numberOfAnimals,
                IndCounter = indCounter,
                FpsYear = DefaultFpsYear,
                UserId = userId,
                UserEmail = userEmail
            };

        private static ProjectView BuildProjectView(
            string parentProject = "J1",
            string program = "PROG1",
            short isDefraProject = 0,
            int? userId = 1) =>
            new()
            {
                ParentProject = parentProject,
                ProjectTitle = "Title",
                Program = program,
                ProjectStatus = "Open",
                IsDefraProject = isDefraProject,
                UserId = userId
            };

        private static AnimalRepository CreateRepositoryForAnimalCost(
            IEnumerable<AnimalRequestView> animalRequestViews,
            IEnumerable<Animal> animals,
            IEnumerable<ProjectView> projectViews)
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var animalRequestViewSet = RepositoryTestHelper.CreateMockDbSet(animalRequestViews);
            RepositoryTestHelper.SetupDbSetOperations(animalRequestViewSet);
            dbContext.Setup(x => x.AnimalRequestViews).Returns(animalRequestViewSet.Object);

            var animalSet = RepositoryTestHelper.CreateMockDbSet(animals);
            RepositoryTestHelper.SetupDbSetOperations(animalSet);
            dbContext.Setup(x => x.Animals).Returns(animalSet.Object);

            var projectViewSet = RepositoryTestHelper.CreateMockDbSet(projectViews);
            RepositoryTestHelper.SetupDbSetOperations(projectViewSet);
            dbContext.Setup(x => x.ProjectViews).Returns(projectViewSet.Object);

            RepositoryTestHelper.SetupSaveChanges(dbContext);
            return new AnimalRepository(dbContext.Object, requestCtx.Object);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenDbContextIsNull()
        {
            var ctx = CreateRequestContextMock();
            Assert.Throws<ArgumentNullException>(() => new AnimalRepository(null!, ctx.Object));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenRequestContextIsNull()
        {
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(CreateRequestContextMock().Object);
            Assert.Throws<ArgumentNullException>(() => new AnimalRepository(dbContext.Object, null!));
        }

        #endregion

        #region GetAllAnimalsAsync (non-paged) Tests

        [Fact]
        public async Task GetAllAnimalsAsync_ReturnsOnlyCurrentFpsYear()
        {
            var animals = new List<Animal>
            {
                BuildAnimal("CATTLE", fpsYear: DefaultFpsYear),
                BuildAnimal("SHEEP", fpsYear: 2020)
            };
            var repo = CreateRepository(animals);
            var result = await repo.GetAllAnimalsAsync();
            Assert.Single(result);
            Assert.Equal("CATTLE", result.First().AnimalType);
        }

        [Fact]
        public async Task GetAllAnimalsAsync_ReturnsEmpty_WhenNoAnimals()
        {
            var repo = CreateRepository([]);
            var result = await repo.GetAllAnimalsAsync();
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAnimalsAsync_ReturnsOrderedByAnimalType()
        {
            var animals = new List<Animal>
            {
                BuildAnimal("SHEEP"),
                BuildAnimal("CATTLE"),
                BuildAnimal("PIG")
            };
            var repo = CreateRepository(animals);
            var result = (await repo.GetAllAnimalsAsync()).ToList();
            Assert.Equal("CATTLE", result[0].AnimalType);
            Assert.Equal("PIG", result[1].AnimalType);
            Assert.Equal("SHEEP", result[2].AnimalType);
        }

        [Fact]
        public async Task GetAllAnimalsAsync_ReturnsMultipleRecords_WhenMultipleExist()
        {
            var animals = new List<Animal> { BuildAnimal("CATTLE"), BuildAnimal("SHEEP") };
            var repo = CreateRepository(animals);
            var result = await repo.GetAllAnimalsAsync();
            Assert.Equal(2, result.Count());
        }

        #endregion

        #region GetAllAnimalsAsync (paged) Tests

        [Fact]
        public async Task GetAllAnimalsPagedAsync_ReturnsEmptyPagedData_WhenNoRecords()
        {
            var repo = CreateRepository([]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.GetAllAnimalsAsync(query);
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_ReturnsAllRecordsOnPage1()
        {
            var animals = new List<Animal> { BuildAnimal("CATTLE"), BuildAnimal("SHEEP"), BuildAnimal("PIG") };
            var repo = CreateRepository(animals);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.GetAllAnimalsAsync(query);
            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_ReturnsCorrectPage()
        {
            var animals = new List<Animal>
            {
                BuildAnimal("A"), BuildAnimal("B"), BuildAnimal("C"),
                BuildAnimal("D"), BuildAnimal("E")
            };
            var repo = CreateRepository(animals);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };
            var result = await repo.GetAllAnimalsAsync(query);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_FiltersByAnimalType()
        {
            var animals = new List<Animal>
            {
                BuildAnimal("CATTLE"),
                BuildAnimal("SHEEP"),
                BuildAnimal("CAT")
            };
            var repo = CreateRepository(animals);
            var filter = System.Text.Json.JsonSerializer.Serialize(
                new Dictionary<string, string> { { "AnimalType", "CAT" } });
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllAnimalsAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_FiltersBySpecies()
        {
            var animals = new List<Animal>
            {
                BuildAnimal("CATTLE", species: "Bovine"),
                BuildAnimal("SHEEP", species: "Ovine"),
                BuildAnimal("COW", species: "Bovine")
            };
            var repo = CreateRepository(animals);
            var filter = System.Text.Json.JsonSerializer.Serialize(
                new Dictionary<string, string> { { "Species", "Bovine" } });
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllAnimalsAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_FiltersBySecurityLevel()
        {
            var animals = new List<Animal>
            {
                BuildAnimal("CATTLE", securityLevel: "L1"),
                BuildAnimal("SHEEP", securityLevel: "L2"),
                BuildAnimal("PIG", securityLevel: "L1")
            };
            var repo = CreateRepository(animals);
            var filter = System.Text.Json.JsonSerializer.Serialize(
                new Dictionary<string, string> { { "SecurityLevel", "L1" } });
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllAnimalsAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_ReturnsAll_WhenFilterIsNull()
        {
            var animals = new List<Animal> { BuildAnimal("CATTLE"), BuildAnimal("SHEEP") };
            var repo = CreateRepository(animals);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };
            var result = await repo.GetAllAnimalsAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_ReturnsAll_WhenFilterIsEmpty()
        {
            var animals = new List<Animal> { BuildAnimal("CATTLE"), BuildAnimal("SHEEP") };
            var repo = CreateRepository(animals);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "" };
            var result = await repo.GetAllAnimalsAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        #endregion

        #region Sorting Tests

        [Theory]
        [InlineData("animaltype", false, "CATTLE", "SHEEP")]
        [InlineData("animaltype", true, "SHEEP", "CATTLE")]
        [InlineData("species", false, "Bovine", "Ovine")]
        [InlineData("species", true, "Ovine", "Bovine")]
        [InlineData("dailyrate", false, "40", "60")]
        [InlineData("dailyrate", true, "60", "40")]
        [InlineData("defradailyrate", false, "50", "70")]
        [InlineData("defradailyrate", true, "70", "50")]
        public async Task GetAllAnimalsPagedAsync_AppliesSorting_Correctly(
            string sortBy, bool descending, string expectedFirst, string expectedSecond)
        {
            var animals = new List<Animal>
            {
                BuildAnimal("SHEEP", species: "Ovine",  dailyRate: 60m, defraDailyRate: 70m),
                BuildAnimal("CATTLE", species: "Bovine", dailyRate: 40m, defraDailyRate: 50m)
            };
            var repo = CreateRepository(animals);
            var query = new PaginationParameters<string>
            { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };
            var result = await repo.GetAllAnimalsAsync(query);
            var list = result.Data.ToList();
            var actualFirst = sortBy switch
            {
                "species"        => list[0].Species,
                "dailyrate"      => list[0].DailyRate?.ToString(),
                "defradailyrate" => list[0].DefraDailyRate?.ToString(),
                _                => list[0].AnimalType
            };
            var actualSecond = sortBy switch
            {
                "species"        => list[1].Species,
                "dailyrate"      => list[1].DailyRate?.ToString(),
                "defradailyrate" => list[1].DefraDailyRate?.ToString(),
                _                => list[1].AnimalType
            };
            Assert.Equal(expectedFirst, actualFirst);
            Assert.Equal(expectedSecond, actualSecond);
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_SortsByPlanByWeek_Ascending()
        {
            var animals = new List<Animal>
            {
                BuildAnimal("SHEEP", planByWeek: true),
                BuildAnimal("CATTLE", planByWeek: false)
            };
            var repo = CreateRepository(animals);
            var query = new PaginationParameters<string>
            { Page = 1, PageSize = 10, SortBy = "planbyweek", Descending = false };
            var result = await repo.GetAllAnimalsAsync(query);
            Assert.False(result.Data.First().PlanByWeek);
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_UnknownSortBy_ReturnsAllRecords()
        {
            var animals = new List<Animal> { BuildAnimal("CATTLE"), BuildAnimal("SHEEP") };
            var repo = CreateRepository(animals);
            var query = new PaginationParameters<string>
            { Page = 1, PageSize = 10, SortBy = "unknown" };
            var result = await repo.GetAllAnimalsAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_NullSortBy_ReturnsAllRecords()
        {
            var animals = new List<Animal> { BuildAnimal("CATTLE"), BuildAnimal("SHEEP") };
            var repo = CreateRepository(animals);
            var query = new PaginationParameters<string>
            { Page = 1, PageSize = 10, SortBy = null };
            var result = await repo.GetAllAnimalsAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        #endregion

        #region GetAnimalByIdAsync Tests

        [Fact]
        public async Task GetAnimalByIdAsync_ReturnsAnimal_WhenFound()
        {
            var repo = CreateRepository([BuildAnimal("CATTLE")]);
            var result = await repo.GetAnimalByIdAsync("CATTLE");
            Assert.NotNull(result);
            Assert.Equal("CATTLE", result.AnimalType);
        }

        [Fact]
        public async Task GetAnimalByIdAsync_ReturnsAnimal_WhenCasingDiffers()
        {
            var repo = CreateRepository([BuildAnimal("CATTLE")]);
            var result = await repo.GetAnimalByIdAsync("cattle");
            Assert.NotNull(result);
            Assert.Equal("CATTLE", result.AnimalType);
        }

        [Fact]
        public async Task GetAnimalByIdAsync_ReturnsNull_WhenNotFound()
        {
            var repo = CreateRepository([]);
            var result = await repo.GetAnimalByIdAsync("NOTEXIST");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAnimalByIdAsync_ThrowsArgumentException_WhenEmpty()
        {
            var repo = CreateRepository([]);
            await Assert.ThrowsAsync<ArgumentException>(() => repo.GetAnimalByIdAsync(""));
        }

        [Fact]
        public async Task GetAnimalByIdAsync_ThrowsArgumentException_WhenWhiteSpace()
        {
            var repo = CreateRepository([]);
            await Assert.ThrowsAsync<ArgumentException>(() => repo.GetAnimalByIdAsync("   "));
        }

        [Fact]
        public async Task GetAnimalByIdAsync_ReturnsOnlyCurrentFpsYear()
        {
            var animals = new List<Animal>
            {
                BuildAnimal("CATTLE", fpsYear: DefaultFpsYear),
                BuildAnimal("CATTLE", fpsYear: 2020)
            };
            var repo = CreateRepository(animals);
            var result = await repo.GetAnimalByIdAsync("CATTLE");
            Assert.NotNull(result);
            Assert.Equal(DefaultFpsYear, result.FpsYear);
        }

        #endregion

        #region AddAnimalAsync Tests

        [Fact]
        public async Task AddAnimalAsync_ThrowsArgumentNullException_WhenEntityIsNull()
        {
            var repo = CreateRepository([]);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.AddAnimalAsync(null!));
        }

        [Fact]
        public async Task AddAnimalAsync_SetsFpsYearFromContext()
        {
            var (repo, _, dbSet) = CreateRepositoryWithMocks([]);
            dbSet.Setup(x => x.Add(It.IsAny<Animal>()));
            var entity = BuildAnimal("NEWANIMAL");
            entity.FpsYear = 0;
            var result = await repo.AddAnimalAsync(entity);
            Assert.Equal(DefaultFpsYear, result.FpsYear);
        }

        [Fact]
        public async Task AddAnimalAsync_AddsEntityAndSavesChanges()
        {
            var (repo, context, dbSet) = CreateRepositoryWithMocks([]);
            dbSet.Setup(x => x.Add(It.IsAny<Animal>()));
            var entity = BuildAnimal("NEWANIMAL");
            var result = await repo.AddAnimalAsync(entity);
            Assert.NotNull(result);
            Assert.Equal("NEWANIMAL", result.AnimalType);
            dbSet.Verify(x => x.Add(It.Is<Animal>(a => a.AnimalType == "NEWANIMAL")), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(context);
        }

        #endregion

        #region UpdateAnimalAsync Tests

        [Fact]
        public async Task UpdateAnimalAsync_ThrowsArgumentNullException_WhenEntityIsNull()
        {
            var repo = CreateRepository([]);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateAnimalAsync(null!));
        }

        [Fact]
        public async Task UpdateAnimalAsync_SetsFpsYearFromContext()
        {
            var (repo, _, dbSet) = CreateRepositoryWithMocks([BuildAnimal("CATTLE")]);
            dbSet.Setup(x => x.Update(It.IsAny<Animal>()));
            var entity = BuildAnimal("CATTLE");
            entity.FpsYear = 0;
            var result = await repo.UpdateAnimalAsync(entity);
            Assert.Equal(DefaultFpsYear, result.FpsYear);
        }

        [Fact]
        public async Task UpdateAnimalAsync_UpdatesEntityAndSavesChanges()
        {
            var (repo, context, dbSet) = CreateRepositoryWithMocks([BuildAnimal("CATTLE")]);
            dbSet.Setup(x => x.Update(It.IsAny<Animal>()));
            var entity = BuildAnimal("CATTLE", species: "Updated", dailyRate: 99m);
            var result = await repo.UpdateAnimalAsync(entity);
            Assert.NotNull(result);
            dbSet.Verify(x => x.Update(It.Is<Animal>(a => a.AnimalType == "CATTLE")), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(context);
        }

        #endregion

        #region DeleteAnimalAsync Tests

        [Fact]
        public async Task DeleteAnimalAsync_ThrowsArgumentException_WhenEmpty()
        {
            var repo = CreateRepository([]);
            await Assert.ThrowsAsync<ArgumentException>(() => repo.DeleteAnimalAsync(""));
        }

        [Fact]
        public async Task DeleteAnimalAsync_ThrowsArgumentException_WhenWhiteSpace()
        {
            var repo = CreateRepository([]);
            await Assert.ThrowsAsync<ArgumentException>(() => repo.DeleteAnimalAsync("   "));
        }

        [Fact]
        public async Task DeleteAnimalAsync_ReturnsFalse_WhenNotFound()
        {
            var repo = CreateRepository([]);
            var result = await repo.DeleteAnimalAsync("NOTEXIST");
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAnimalAsync_ReturnsTrue_WhenFound()
        {
            var repo = CreateRepository([BuildAnimal("CATTLE")]);
            var result = await repo.DeleteAnimalAsync("CATTLE");
            Assert.True(result);
        }

        #endregion

        #region GetGlobalAnimalCostAsync Tests

        [Fact]
        public async Task GetGlobalAnimalCostAsync_ReturnsZero_WhenNoData()
        {
            var repo = CreateRepositoryForGlobalCost([], []);
            var result = await repo.GetGlobalAnimalCostAsync();
            Assert.Equal(0m, result);
        }

        [Fact]
        public async Task GetGlobalAnimalCostAsync_ReturnsSingleRowCost()
        {
            // 2 days × 3 animals × £10/day = £60
            var requests = new List<AnimalRequest>
            {
                new() { JobCode = "J1", AnimalType = "CATTLE", NumberOfDays = 2d, NumberOfAnimals = 3d }
            };
            var animals = new List<Animal>
            {
                BuildAnimal("CATTLE", dailyRate: 10m)
            };
            var repo = CreateRepositoryForGlobalCost(requests, animals);
            var result = await repo.GetGlobalAnimalCostAsync();
            Assert.Equal(60m, result);
        }

        [Fact]
        public async Task GetGlobalAnimalCostAsync_SumsMultipleRows()
        {
            // Row 1: 1 × 2 × 5 = 10; Row 2: 4 × 1 × 3 = 12 → total 22
            var requests = new List<AnimalRequest>
            {
                new() { JobCode = "J1", AnimalType = "CATTLE", NumberOfDays = 1d, NumberOfAnimals = 2d },
                new() { JobCode = "J2", AnimalType = "SHEEP",  NumberOfDays = 4d, NumberOfAnimals = 1d }
            };
            var animals = new List<Animal>
            {
                BuildAnimal("CATTLE", dailyRate: 5m),
                BuildAnimal("SHEEP",  dailyRate: 3m)
            };
            var repo = CreateRepositoryForGlobalCost(requests, animals);
            var result = await repo.GetGlobalAnimalCostAsync();
            Assert.Equal(22m, result);
        }

        [Fact]
        public async Task GetGlobalAnimalCostAsync_TreatsNullDailyRateAsZero()
        {
            var requests = new List<AnimalRequest>
            {
                new() { JobCode = "J1", AnimalType = "CATTLE", NumberOfDays = 5d, NumberOfAnimals = 2d }
            };
            var animals = new List<Animal>
            {
                BuildAnimal("CATTLE", dailyRate: null)
            };
            var repo = CreateRepositoryForGlobalCost(requests, animals);
            var result = await repo.GetGlobalAnimalCostAsync();
            Assert.Equal(0m, result);
        }

        #endregion

        #region GetAnimalCostByAnimalTypeAsync Tests

        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_ReturnsEmpty_WhenNoMatchingRecords()
        {
            var repo = CreateRepositoryForProgrammeCost([], [], []);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetAnimalCostByAnimalTypeAsync(query, "CATTLE");

            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_ReturnsRecord_WithComputedAnimalCost_ForNonDefraProject()
        {
            // Non-Defra (IsDefraProject = 0) → uses DailyRate (10). 2 days × 3 animals × £10 = £60
            var projects = new List<Project> { BuildProject("J1", "PROG1", isDefraProject: 0) };
            var requests = new List<AnimalRequest> { BuildAnimalRequest("J1", "CATTLE", 2d, 3d, 1) };
            var animals = new List<Animal> { BuildAnimal("CATTLE", dailyRate: 10m, defraDailyRate: 99m) };
            var repo = CreateRepositoryForProgrammeCost(projects, requests, animals);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetAnimalCostByAnimalTypeAsync(query, "CATTLE");

            var row = Assert.Single(result.Data);
            Assert.Equal("CATTLE", row.AnimalType);
            Assert.Equal("PROG1", row.Programme);
            Assert.Equal(10m, row.DailyRate);
            Assert.Equal(60m, row.AnimalCost);
            Assert.Equal(6d, row.TotalDays);
        }

        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_UsesDefraDailyRate_ForDefraProject()
        {
            // Defra (IsDefraProject = 1) → uses DefraDailyRate (20). 2 × 3 × £20 = £120
            var projects = new List<Project> { BuildProject("J1", "PROG1", isDefraProject: 1) };
            var requests = new List<AnimalRequest> { BuildAnimalRequest("J1", "CATTLE", 2d, 3d, 1) };
            var animals = new List<Animal> { BuildAnimal("CATTLE", dailyRate: 10m, defraDailyRate: 20m) };
            var repo = CreateRepositoryForProgrammeCost(projects, requests, animals);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetAnimalCostByAnimalTypeAsync(query, "CATTLE");

            var row = Assert.Single(result.Data);
            Assert.Equal(20m, row.DailyRate);
            Assert.Equal(120m, row.AnimalCost);
        }

        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_FiltersByAnimalType_CaseInsensitive()
        {
            var projects = new List<Project>
            {
                BuildProject("J1", "PROG1"),
                BuildProject("J2", "PROG2")
            };
            var requests = new List<AnimalRequest>
            {
                BuildAnimalRequest("J1", "CATTLE", 1d, 1d, 1),
                BuildAnimalRequest("J2", "SHEEP",  1d, 1d, 2)
            };
            var animals = new List<Animal>
            {
                BuildAnimal("CATTLE", dailyRate: 10m),
                BuildAnimal("SHEEP",  dailyRate: 5m)
            };
            var repo = CreateRepositoryForProgrammeCost(projects, requests, animals);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetAnimalCostByAnimalTypeAsync(query, "cattle");

            var row = Assert.Single(result.Data);
            Assert.Equal("CATTLE", row.AnimalType);
        }

        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_TreatsNullDailyRateAsZeroCost()
        {
            var projects = new List<Project> { BuildProject("J1", "PROG1", isDefraProject: 0) };
            var requests = new List<AnimalRequest> { BuildAnimalRequest("J1", "CATTLE", 5d, 2d, 1) };
            var animals = new List<Animal> { BuildAnimal("CATTLE", dailyRate: null) };
            var repo = CreateRepositoryForProgrammeCost(projects, requests, animals);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetAnimalCostByAnimalTypeAsync(query, "CATTLE");

            var row = Assert.Single(result.Data);
            Assert.Equal(0m, row.AnimalCost);
        }

        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_SortsByAnimalCostDescending()
        {
            var projects = new List<Project>
            {
                BuildProject("J1", "PROG1"),
                BuildProject("J2", "PROG2")
            };
            var requests = new List<AnimalRequest>
            {
                BuildAnimalRequest("J1", "CATTLE", 1d, 1d, 1), // 1×1×10 = 10
                BuildAnimalRequest("J2", "CATTLE", 5d, 2d, 2)  // 5×2×10 = 100
            };
            var animals = new List<Animal> { BuildAnimal("CATTLE", dailyRate: 10m) };
            var repo = CreateRepositoryForProgrammeCost(projects, requests, animals);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "AnimalCost",
                Descending = true
            };

            var result = await repo.GetAnimalCostByAnimalTypeAsync(query, "CATTLE");

            Assert.Equal(2, result.Data.Count());
            Assert.Equal(100m, result.Data.First().AnimalCost);
            Assert.Equal(10m, result.Data.Last().AnimalCost);
        }

        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_SortsByJobCodeAscending()
        {
            var projects = new List<Project>
            {
                BuildProject("J2", "PROG2"),
                BuildProject("J1", "PROG1")
            };
            var requests = new List<AnimalRequest>
            {
                BuildAnimalRequest("J2", "CATTLE", 1d, 1d, 1),
                BuildAnimalRequest("J1", "CATTLE", 1d, 1d, 2)
            };
            var animals = new List<Animal> { BuildAnimal("CATTLE", dailyRate: 10m) };
            var repo = CreateRepositoryForProgrammeCost(projects, requests, animals);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "JobCode",
                Descending = false
            };

            var result = await repo.GetAnimalCostByAnimalTypeAsync(query, "CATTLE");

            Assert.Equal(2, result.Data.Count());
            Assert.Equal("J1", result.Data.First().JobCode);
            Assert.Equal("J2", result.Data.Last().JobCode);
        }

        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_SortsByDailyRateAscending()
        {
            var projects = new List<Project>
            {
                BuildProject("J1", "PROG1"),
                BuildProject("J2", "PROG2")
            };
            var requests = new List<AnimalRequest>
            {
                BuildAnimalRequest("J1", "CATTLE", 1d, 1d, 1),
                BuildAnimalRequest("J2", "SHEEP",  1d, 1d, 2)
            };
            var animals = new List<Animal>
            {
                BuildAnimal("CATTLE", dailyRate: 20m),
                BuildAnimal("SHEEP",  dailyRate: 5m)
            };
            var repo = CreateRepositoryForProgrammeCost(projects, requests, animals);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "DailyRate",
                Descending = false
            };

            var result = await repo.GetAnimalCostByAnimalTypeAsync(query, "CATTLE");

            var row = Assert.Single(result.Data);
            Assert.Equal(20m, row.DailyRate);
        }

        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_SortsByTotalDaysDescending()
        {
            var projects = new List<Project>
            {
                BuildProject("J1", "PROG1"),
                BuildProject("J2", "PROG2")
            };
            var requests = new List<AnimalRequest>
            {
                BuildAnimalRequest("J1", "CATTLE", 1d, 1d, 1), // TotalDays = 1
                BuildAnimalRequest("J2", "CATTLE", 5d, 2d, 2)  // TotalDays = 10
            };
            var animals = new List<Animal> { BuildAnimal("CATTLE", dailyRate: 10m) };
            var repo = CreateRepositoryForProgrammeCost(projects, requests, animals);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "TotalDays",
                Descending = true
            };

            var result = await repo.GetAnimalCostByAnimalTypeAsync(query, "CATTLE");

            Assert.Equal(2, result.Data.Count());
            Assert.Equal(10d, result.Data.First().TotalDays);
            Assert.Equal(1d, result.Data.Last().TotalDays);
        }

        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_SortsByNumberOfAnimalsAscending()
        {
            var projects = new List<Project>
            {
                BuildProject("J1", "PROG1"),
                BuildProject("J2", "PROG2")
            };
            var requests = new List<AnimalRequest>
            {
                BuildAnimalRequest("J1", "CATTLE", 1d, 4d, 1), // NumberOfAnimals = 4
                BuildAnimalRequest("J2", "CATTLE", 1d, 2d, 2)  // NumberOfAnimals = 2
            };
            var animals = new List<Animal> { BuildAnimal("CATTLE", dailyRate: 10m) };
            var repo = CreateRepositoryForProgrammeCost(projects, requests, animals);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "NumberOfAnimals",
                Descending = false
            };

            var result = await repo.GetAnimalCostByAnimalTypeAsync(query, "CATTLE");

            Assert.Equal(2, result.Data.Count());
            Assert.Equal(2d, result.Data.First().NumberOfAnimals);
            Assert.Equal(4d, result.Data.Last().NumberOfAnimals);
        }

        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_UnknownSortBy_ReturnsUnsorted()
        {
            var projects = new List<Project>
            {
                BuildProject("J2", "PROG2"),
                BuildProject("J1", "PROG1")
            };
            var requests = new List<AnimalRequest>
            {
                BuildAnimalRequest("J2", "CATTLE", 1d, 1d, 1),
                BuildAnimalRequest("J1", "CATTLE", 1d, 1d, 2)
            };
            var animals = new List<Animal> { BuildAnimal("CATTLE", dailyRate: 10m) };
            var repo = CreateRepositoryForProgrammeCost(projects, requests, animals);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "DoesNotExist",
                Descending = false
            };

            var result = await repo.GetAnimalCostByAnimalTypeAsync(query, "CATTLE");

            // Unknown sort key → original ordering preserved
            Assert.Equal(2, result.Data.Count());
            Assert.Equal("J2", result.Data.First().JobCode);
            Assert.Equal("J1", result.Data.Last().JobCode);
        }

        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_EmptySortBy_ReturnsUnsorted()
        {
            var projects = new List<Project>
            {
                BuildProject("J2", "PROG2"),
                BuildProject("J1", "PROG1")
            };
            var requests = new List<AnimalRequest>
            {
                BuildAnimalRequest("J2", "CATTLE", 1d, 1d, 1),
                BuildAnimalRequest("J1", "CATTLE", 1d, 1d, 2)
            };
            var animals = new List<Animal> { BuildAnimal("CATTLE", dailyRate: 10m) };
            var repo = CreateRepositoryForProgrammeCost(projects, requests, animals);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = string.Empty,
                Descending = false
            };

            var result = await repo.GetAnimalCostByAnimalTypeAsync(query, "CATTLE");

            // Empty sort key → original ordering preserved
            Assert.Equal(2, result.Data.Count());
            Assert.Equal("J2", result.Data.First().JobCode);
            Assert.Equal("J1", result.Data.Last().JobCode);
        }

        #endregion

        #region GetAnimalCostAsync Tests

        [Fact]
        public async Task GetAnimalCostAsync_WithMatchingUserAndJobCode_ReturnsPagedData()
        {
            var animalRequestViews = new List<AnimalRequestView>
            {
                BuildAnimalRequestView("J1", "CATTLE", 2d, 3d, indCounter: 1),
                BuildAnimalRequestView("J1", "CATTLE", 4d, 1d, indCounter: 2),
                BuildAnimalRequestView("J2", "CATTLE", 1d, 1d, indCounter: 3)
            };
            var animals = new List<Animal> { BuildAnimal("CATTLE", dailyRate: 10m, defraDailyRate: 20m) };
            var projectViews = new List<ProjectView> { BuildProjectView("J1"), BuildProjectView("J2") };
            var repo = CreateRepositoryForAnimalCost(animalRequestViews, animals, projectViews);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetAnimalCostAsync(query, "J1");

            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, item => Assert.Equal("J1", item.JobCode));
        }

        [Fact]
        public async Task GetAnimalCostAsync_WithNoMatchingJobCode_ReturnsEmptyPage()
        {
            var animalRequestViews = new List<AnimalRequestView>
            {
                BuildAnimalRequestView("J1", "CATTLE")
            };
            var animals = new List<Animal> { BuildAnimal("CATTLE", dailyRate: 10m) };
            var projectViews = new List<ProjectView> { BuildProjectView("J1") };
            var repo = CreateRepositoryForAnimalCost(animalRequestViews, animals, projectViews);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetAnimalCostAsync(query, "J999");

            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAnimalCostAsync_WithDifferentUserEmail_ExcludesRecord()
        {
            var animalRequestViews = new List<AnimalRequestView>
            {
                BuildAnimalRequestView("J1", "CATTLE", userEmail: "other@example.com")
            };
            var animals = new List<Animal> { BuildAnimal("CATTLE", dailyRate: 10m) };
            var projectViews = new List<ProjectView> { BuildProjectView("J1") };
            var repo = CreateRepositoryForAnimalCost(animalRequestViews, animals, projectViews);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetAnimalCostAsync(query, "J1");

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAnimalCostAsync_NonDefraProject_UsesStandardDailyRate()
        {
            var animalRequestViews = new List<AnimalRequestView>
            {
                BuildAnimalRequestView("J1", "CATTLE", numberOfDays: 2d, numberOfAnimals: 1d)
            };
            var animals = new List<Animal> { BuildAnimal("CATTLE", dailyRate: 10m, defraDailyRate: 99m) };
            var projectViews = new List<ProjectView> { BuildProjectView("J1", isDefraProject: 0) };
            var repo = CreateRepositoryForAnimalCost(animalRequestViews, animals, projectViews);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetAnimalCostAsync(query, "J1");

            var item = Assert.Single(result.Data);
            Assert.Equal(10m, item.DailyRate);
            Assert.Equal(20m, item.AnimalCost);
        }

        [Fact(Skip = "Scalar Select projection not supported by mock async provider; covered by integration tests.")]
        public async Task GetAnimalCostAsync_DefraProject_UsesDefraDailyRate()
        {
            var animalRequestViews = new List<AnimalRequestView>
            {
                BuildAnimalRequestView("J1", "CATTLE", numberOfDays: 2d, numberOfAnimals: 1d)
            };
            var animals = new List<Animal> { BuildAnimal("CATTLE", dailyRate: 10m, defraDailyRate: 99m) };
            var projectViews = new List<ProjectView> { BuildProjectView("J1", isDefraProject: -1) };
            var repo = CreateRepositoryForAnimalCost(animalRequestViews, animals, projectViews);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetAnimalCostAsync(query, "J1");

            var item = Assert.Single(result.Data);
            Assert.Equal(99m, item.DailyRate);
            Assert.Equal(198m, item.AnimalCost);
        }

        [Fact]
        public async Task GetAnimalCostAsync_WithSorting_OrdersByNumberOfDaysDescending()
        {
            var animalRequestViews = new List<AnimalRequestView>
            {
                BuildAnimalRequestView("J1", "CATTLE", numberOfDays: 1d, indCounter: 1),
                BuildAnimalRequestView("J1", "CATTLE", numberOfDays: 5d, indCounter: 2)
            };
            var animals = new List<Animal> { BuildAnimal("CATTLE", dailyRate: 10m) };
            var projectViews = new List<ProjectView> { BuildProjectView("J1") };
            var repo = CreateRepositoryForAnimalCost(animalRequestViews, animals, projectViews);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "numberofdays", Descending = true
            };

            var result = await repo.GetAnimalCostAsync(query, "J1");

            Assert.Equal(5d, result.Data.First().NumberOfDays);
            Assert.Equal(1d, result.Data.Last().NumberOfDays);
        }

        [Fact]
        public async Task GetAnimalCostAsync_WithPaging_ReturnsCorrectPage()
        {
            var animalRequestViews = new List<AnimalRequestView>
            {
                BuildAnimalRequestView("J1", "CATTLE", indCounter: 1),
                BuildAnimalRequestView("J1", "CATTLE", indCounter: 2),
                BuildAnimalRequestView("J1", "CATTLE", indCounter: 3)
            };
            var animals = new List<Animal> { BuildAnimal("CATTLE", dailyRate: 10m) };
            var projectViews = new List<ProjectView> { BuildProjectView("J1") };
            var repo = CreateRepositoryForAnimalCost(animalRequestViews, animals, projectViews);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            var result = await repo.GetAnimalCostAsync(query, "J1");

            Assert.Single(result.Data);
            Assert.Equal(3, result.PaginationData.TotalRecords);
        }

        #endregion

        #region GetAnimalLookup Tests

        [Fact]
        public async Task GetAnimalLookup_ReturnsAllAnimals()
        {
            var animals = new List<Animal>
            {
                BuildAnimal("CATTLE"),
                BuildAnimal("SHEEP")
            };
            var repo = CreateRepository(animals);

            var result = await repo.GetAnimalLookup();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAnimalLookup_ReturnsEmpty_WhenNoAnimals()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetAnimalLookup();

            Assert.Empty(result);
        }

        #endregion

        #region GetTotalAnimalCostAsync Tests

        [Fact]
        public async Task GetTotalAnimalCostAsync_ReturnsZero_WhenNoMatchingRecords()
        {
            var repo = CreateRepositoryForAnimalCost([], [], []);

            var result = await repo.GetTotalAnimalCostAsync("J1");

            Assert.Equal(0m, result);
        }

        [Fact]
        public async Task GetTotalAnimalCostAsync_SumsMatchingRecords()
        {
            var animalRequestViews = new List<AnimalRequestView>
            {
                BuildAnimalRequestView("J1", "CATTLE", numberOfDays: 2d, numberOfAnimals: 3d, indCounter: 1),
                BuildAnimalRequestView("J1", "CATTLE", numberOfDays: 1d, numberOfAnimals: 4d, indCounter: 2)
            };
            var animals = new List<Animal> { BuildAnimal("CATTLE", dailyRate: 10m) };
            var projectViews = new List<ProjectView> { BuildProjectView("J1", isDefraProject: 0) };
            var repo = CreateRepositoryForAnimalCost(animalRequestViews, animals, projectViews);

            var result = await repo.GetTotalAnimalCostAsync("J1");

            // (2*3*10) + (1*4*10) = 60 + 40 = 100
            Assert.Equal(100m, result);
        }

        #endregion

        #region GetAnimalCostViewByIdAsync Tests

        [Fact]
        public async Task GetAnimalCostViewByIdAsync_ReturnsNull_WhenNotFound()
        {
            var animalRequestViews = new List<AnimalRequestView>
            {
                BuildAnimalRequestView("J1", "CATTLE", indCounter: 1)
            };
            var animals = new List<Animal> { BuildAnimal("CATTLE", dailyRate: 10m) };
            var projectViews = new List<ProjectView> { BuildProjectView("J1") };
            var repo = CreateRepositoryForAnimalCost(animalRequestViews, animals, projectViews);

            var result = await repo.GetAnimalCostViewByIdAsync(999, "J1");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAnimalCostViewByIdAsync_ReturnsRecord_WithComputedCost()
        {
            var animalRequestViews = new List<AnimalRequestView>
            {
                BuildAnimalRequestView("J1", "CATTLE", numberOfDays: 2d, numberOfAnimals: 3d, indCounter: 5)
            };
            var animals = new List<Animal> { BuildAnimal("CATTLE", dailyRate: 10m) };
            var projectViews = new List<ProjectView> { BuildProjectView("J1", isDefraProject: 0) };
            var repo = CreateRepositoryForAnimalCost(animalRequestViews, animals, projectViews);

            var result = await repo.GetAnimalCostViewByIdAsync(5, "J1");

            Assert.NotNull(result);
            Assert.Equal(5, result!.IndCounter);
            // 2 * 3 * 10 = 60
            Assert.Equal(60m, result.AnimalCost);
        }

        #endregion

        #region GetAnimalRateByIdAsync Tests

        // NOTE: GetAnimalRateByIdAsync projects a value-type scalar (Select(p => p.IsDefraProject)
        // and Select(... ? DefraDailyRate : DailyRate)). The mock async query provider
        // (TestAsyncEnumerable<T> constrained to reference types) cannot materialise scalar
        // projections, so these paths are covered by integration tests against PostgreSQL.
        [Fact(Skip = "Scalar Select projection not supported by mock async provider; covered by integration tests.")]
        public async Task GetAnimalRateByIdAsync_ReturnsStandardRate_ForNonDefraProject()
        {
            var repo = CreateRepositoryForAnimalRate(
                new List<Project> { BuildProject("J1", isDefraProject: 0) },
                new List<Animal> { BuildAnimal("CATTLE", dailyRate: 15m, defraDailyRate: 99m) });

            var result = await repo.GetAnimalRateByIdAsync("CATTLE", "J1");

            Assert.Equal(15m, result);
        }

        [Fact(Skip = "Scalar Select projection not supported by mock async provider; covered by integration tests.")]
        public async Task GetAnimalRateByIdAsync_ReturnsDefraRate_ForDefraProject()
        {
            var repo = CreateRepositoryForAnimalRate(
                new List<Project> { BuildProject("J1", isDefraProject: -1) },
                new List<Animal> { BuildAnimal("CATTLE", dailyRate: 15m, defraDailyRate: 99m) });

            var result = await repo.GetAnimalRateByIdAsync("CATTLE", "J1");

            Assert.Equal(99m, result);
        }

        #endregion

        #region AddAnimalCostAsync Tests

        [Fact]
        public async Task AddAnimalCostAsync_ThrowsArgumentNullException_WhenNull()
        {
            var (repo, _, _, _) = CreateRepositoryForAnimalRequestCrud([]);

            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.AddAnimalCostAsync(null!));
        }

        [Fact]
        public async Task AddAnimalCostAsync_AddsRequestAndLog_AndSetsFpsYear()
        {
            var (repo, requestSet, logSet, context) = CreateRepositoryForAnimalRequestCrud([]);
            var animalReq = BuildAnimalRequest("J1", "CATTLE");
            animalReq.FpsYear = 0;

            var result = await repo.AddAnimalCostAsync(animalReq);

            Assert.Equal(DefaultFpsYear, result.FpsYear);
            requestSet.Verify(x => x.Add(animalReq), Times.Once);
            logSet.Verify(x => x.Add(It.Is<AnimalRequestLog>(l => l.InsertDelete == "I")), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(context, 1);
        }

        #endregion

        #region UpdateAnimalCostAsync Tests

        [Fact]
        public async Task UpdateAnimalCostAsync_ThrowsArgumentNullException_WhenNull()
        {
            var (repo, _, _, _) = CreateRepositoryForAnimalRequestCrud([]);

            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateAnimalCostAsync(null!));
        }

        [Fact]
        public async Task UpdateAnimalCostAsync_ThrowsInvalidOperationException_WhenNotFound()
        {
            var (repo, requestSet, _, _) = CreateRepositoryForAnimalRequestCrud([]);
            requestSet
                .Setup(x => x.FindAsync(It.IsAny<object[]>()))
                .Returns(new ValueTask<AnimalRequest?>((AnimalRequest?)null));
            var animalReq = BuildAnimalRequest("J1", "CATTLE", indCounter: 1);

            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.UpdateAnimalCostAsync(animalReq));
        }

        [Fact]
        public async Task UpdateAnimalCostAsync_UpdatesExistingEntity_AndAddsLog()
        {
            var existing = BuildAnimalRequest("J1", "CATTLE", numberOfDays: 1d, numberOfAnimals: 1d, indCounter: 1);
            var (repo, requestSet, logSet, context) = CreateRepositoryForAnimalRequestCrud([existing]);
            requestSet
                .Setup(x => x.FindAsync(It.IsAny<object[]>()))
                .Returns(new ValueTask<AnimalRequest?>(existing));

            var update = BuildAnimalRequest("J2", "SHEEP", numberOfDays: 7d, numberOfAnimals: 9d, indCounter: 1);

            var result = await repo.UpdateAnimalCostAsync(update);

            Assert.Equal("J2", result.JobCode);
            Assert.Equal("SHEEP", result.AnimalType);
            Assert.Equal(7d, result.NumberOfDays);
            Assert.Equal(9d, result.NumberOfAnimals);
            Assert.Equal(DefaultFpsYear, result.FpsYear);
            logSet.Verify(x => x.Add(It.Is<AnimalRequestLog>(l => l.InsertDelete == "U")), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(context, 1);
        }

        #endregion

        #region DeleteJobAnimalCostAsync Tests

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeleteJobAnimalCostAsync_ThrowsArgumentOutOfRange_WhenIndCounterNotPositive(int indCounter)
        {
            var (repo, _, _, _) = CreateRepositoryForAnimalRequestCrud([]);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => repo.DeleteJobAnimalCostAsync(indCounter));
        }

        [Fact]
        public async Task DeleteJobAnimalCostAsync_ReturnsFalse_WhenNotFound()
        {
            var (repo, requestSet, _, _) = CreateRepositoryForAnimalRequestCrud([]);
            requestSet
                .Setup(x => x.FindAsync(It.IsAny<object[]>()))
                .Returns(new ValueTask<AnimalRequest?>((AnimalRequest?)null));

            var result = await repo.DeleteJobAnimalCostAsync(1);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteJobAnimalCostAsync_RemovesEntityAndAddsLog_WhenFound()
        {
            var existing = BuildAnimalRequest("J1", "CATTLE", indCounter: 1);
            var (repo, requestSet, logSet, context) = CreateRepositoryForAnimalRequestCrud([existing]);
            requestSet
                .Setup(x => x.FindAsync(It.IsAny<object[]>()))
                .Returns(new ValueTask<AnimalRequest?>(existing));

            var result = await repo.DeleteJobAnimalCostAsync(1);

            Assert.True(result);
            requestSet.Verify(x => x.Remove(existing), Times.Once);
            logSet.Verify(x => x.Add(It.Is<AnimalRequestLog>(l => l.InsertDelete == "D")), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(context, 1);
        }

        #endregion

        #region CRUD Helpers

        private static AnimalRepository CreateRepositoryForAnimalRate(
            IEnumerable<Project> projects,
            IEnumerable<Animal> animals)
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var projectSet = RepositoryTestHelper.CreateMockDbSet(projects);
            RepositoryTestHelper.SetupDbSetOperations(projectSet);
            dbContext.Setup(x => x.Projects).Returns(projectSet.Object);

            var animalSet = RepositoryTestHelper.CreateMockDbSet(animals);
            RepositoryTestHelper.SetupDbSetOperations(animalSet);
            dbContext.Setup(x => x.Animals).Returns(animalSet.Object);

            RepositoryTestHelper.SetupSaveChanges(dbContext);
            return new AnimalRepository(dbContext.Object, requestCtx.Object);
        }

        private static (AnimalRepository Repo,
            Mock<DbSet<AnimalRequest>> RequestSet,
            Mock<DbSet<AnimalRequestLog>> LogSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryForAnimalRequestCrud(IEnumerable<AnimalRequest> animalRequests)
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var requestSet = RepositoryTestHelper.CreateMockDbSet(animalRequests);
            RepositoryTestHelper.SetupDbSetOperations(requestSet);
            dbContext.Setup(x => x.AnimalRequests).Returns(requestSet.Object);

            var logSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<AnimalRequestLog>());
            RepositoryTestHelper.SetupDbSetOperations(logSet);
            dbContext.Setup(x => x.AnimalRequestLogs).Returns(logSet.Object);

            RepositoryTestHelper.SetupSaveChanges(dbContext);
            return (new AnimalRepository(dbContext.Object, requestCtx.Object), requestSet, logSet, dbContext);
        }

        #endregion

        #region ApplyAnimalSnapshotFilter Tests

        // ApplyAnimalSnapshotFilter is a private static method whose predicates use
        // EF.Functions.ILike. The shared TestAsyncQueryProvider (via TestAsyncEnumerable)
        // rewrites EF.Functions.ILike(col, "%pattern%") into col.ToLower().Contains(pattern)
        // for client-side evaluation, so these tests invoke the method via reflection over a
        // real in-memory queryable and assert on the ACTUAL filtered results.

        private static IQueryable<AnimalSnapshotView> InvokeApplyAnimalSnapshotFilter(
            IQueryable<AnimalSnapshotView> query, string? filter)
        {
            var method = typeof(AnimalRepository).GetMethod(
                "ApplyAnimalSnapshotFilter",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            Assert.NotNull(method);
            return (IQueryable<AnimalSnapshotView>)method!.Invoke(null, new object?[] { query, filter })!;
        }

        private static IQueryable<AnimalSnapshotView> SnapshotQueryable(IEnumerable<AnimalSnapshotView> rows)
            => new TestAsyncEnumerable<AnimalSnapshotView>(rows);

        private static AnimalSnapshotView SnapshotRow(
            string? directorate = "DirA",
            string? program = "P001",
            string? contract = "C001",
            string? project = "PP001",
            string? projectStatus = "Active",
            string? species = "Bovine",
            string? securityLevel = "Low",
            string? animalType = "CATTLE",
            string? jobCode = "J001",
            decimal? cost = 0m) =>
            new()
            {
                Directorate = directorate,
                Program = program,
                Contract = contract,
                Project = project,
                ProjectStatus = projectStatus,
                Species = species,
                SecurityLevel = securityLevel,
                AnimalType = animalType,
                JobCode = jobCode,
                Cost = cost
            };

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task ApplyAnimalSnapshotFilter_ReturnsAllRows_WhenFilterNullOrEmpty(string? filter)
        {
            var source = SnapshotQueryable([SnapshotRow(directorate: "DirA"), SnapshotRow(directorate: "DirB")]);

            var result = InvokeApplyAnimalSnapshotFilter(source, filter);

            Assert.Equal(2, (await result.ToListAsync()).Count);
        }

        [Fact]
        public async Task ApplyAnimalSnapshotFilter_ReturnsAllRows_WhenFilterModelIsNull()
        {
            // "null" deserialises to a null ExpandoObject, exercising the null-model guard.
            var source = SnapshotQueryable([SnapshotRow(), SnapshotRow()]);

            var result = InvokeApplyAnimalSnapshotFilter(source, "null");

            Assert.Equal(2, (await result.ToListAsync()).Count);
        }

        [Fact]
        public async Task ApplyAnimalSnapshotFilter_ReturnsAllRows_WhenNoKnownKeys()
        {
            var source = SnapshotQueryable([SnapshotRow(), SnapshotRow()]);

            var result = InvokeApplyAnimalSnapshotFilter(source, "{\"Unknown\":\"abc\"}");

            Assert.Equal(2, (await result.ToListAsync()).Count);
        }

        [Fact]
        public async Task ApplyAnimalSnapshotFilter_ReturnsAllRows_WhenValueIsExplicitNull()
        {
            var source = SnapshotQueryable([SnapshotRow(directorate: "DirA"), SnapshotRow(directorate: "DirB")]);

            var result = InvokeApplyAnimalSnapshotFilter(source, "{\"Directorate\":null}");

            Assert.Equal(2, (await result.ToListAsync()).Count);
        }

        [Fact]
        public async Task ApplyAnimalSnapshotFilter_FiltersByDirectorate_CaseInsensitivePartialMatch()
        {
            var source = SnapshotQueryable(
            [
                SnapshotRow(directorate: "Animal Health"),
                SnapshotRow(directorate: "Plant Health"),
                SnapshotRow(directorate: "Marine")
            ]);

            var result = InvokeApplyAnimalSnapshotFilter(source, "{\"Directorate\":\"health\"}");

            var rows = await result.ToListAsync();
            Assert.Equal(2, rows.Count);
            Assert.All(rows, r => Assert.Contains("Health", r.Directorate!));
        }

        [Theory]
        [InlineData("Program", "P002")]
        [InlineData("Contract", "C002")]
        [InlineData("Project", "PP002")]
        [InlineData("ProjectStatus", "Closed")]
        [InlineData("Species", "Porcine")]
        [InlineData("SecurityLevel", "High")]
        [InlineData("AnimalType", "SHEEP")]
        [InlineData("JobCode", "J002")]
        public async Task ApplyAnimalSnapshotFilter_FiltersBySupportedKey_ReturnsMatchingRow(string key, string value)
        {
            var match = SnapshotRow(
                program: "P002", contract: "C002", project: "PP002", projectStatus: "Closed",
                species: "Porcine", securityLevel: "High", animalType: "SHEEP", jobCode: "J002");
            var nonMatch = SnapshotRow(
                program: "P001", contract: "C001", project: "PP001", projectStatus: "Active",
                species: "Bovine", securityLevel: "Low", animalType: "CATTLE", jobCode: "J001");
            var source = SnapshotQueryable([match, nonMatch]);

            var result = InvokeApplyAnimalSnapshotFilter(source, $"{{\"{key}\":\"{value}\"}}");

            Assert.Single(await result.ToListAsync());
        }

        [Fact]
        public async Task ApplyAnimalSnapshotFilter_FiltersByCost_ReturnsMatchingRow()
        {
            var source = SnapshotQueryable(
            [
                SnapshotRow(cost: 125m),
                SnapshotRow(cost: 300m)
            ]);

            var result = InvokeApplyAnimalSnapshotFilter(source, "{\"Cost\":\"125\"}");

            var rows = await result.ToListAsync();
            Assert.Single(rows);
            Assert.Equal(125m, rows[0].Cost);
        }

        [Fact]
        public async Task ApplyAnimalSnapshotFilter_AppliesAllKeys_AsAndCondition()
        {
            var match = SnapshotRow(directorate: "DirA", program: "P001", animalType: "CATTLE");
            var partial = SnapshotRow(directorate: "DirA", program: "P001", animalType: "SHEEP");
            var source = SnapshotQueryable([match, partial]);

            var filter = "{\"Directorate\":\"DirA\",\"Program\":\"P001\",\"AnimalType\":\"CATTLE\"}";
            var result = InvokeApplyAnimalSnapshotFilter(source, filter);

            Assert.Single(await result.ToListAsync());
        }

        [Fact]
        public async Task ApplyAnimalSnapshotFilter_IgnoresUnknownKeys_WhenMixedWithKnownKeys()
        {
            var match = SnapshotRow(directorate: "DirA", jobCode: "J001");
            var nonMatch = SnapshotRow(directorate: "DirB", jobCode: "J002");
            var source = SnapshotQueryable([match, nonMatch]);

            var filter = "{\"Directorate\":\"DirA\",\"Unknown\":\"x\",\"JobCode\":\"J001\"}";
            var result = InvokeApplyAnimalSnapshotFilter(source, filter);

            Assert.Single(await result.ToListAsync());
        }

        [Fact]
        public async Task ApplyAnimalSnapshotFilter_ReturnsEmpty_WhenNoRowMatches()
        {
            var source = SnapshotQueryable([SnapshotRow(directorate: "DirA"), SnapshotRow(directorate: "DirB")]);

            var result = InvokeApplyAnimalSnapshotFilter(source, "{\"Directorate\":\"NOMATCH\"}");

            Assert.Empty(await result.ToListAsync());
        }

        #endregion

        #region SnapshotFilterMap Tests

        // SnapshotFilterMap is a private static readonly field holding the (key, predicate)
        // pairs used by ApplyAnimalSnapshotFilter. These tests read it via reflection and
        // assert on its shape and that each predicate filters the expected column. Predicates
        // use EF.Functions.ILike, so they are executed through TestAsyncEnumerable (which
        // rewrites ILike to a client-side Contains) rather than compiled directly.

        private static IReadOnlyList<(string Key, Func<string, Expression<Func<AnimalSnapshotView, bool>>> Predicate)>
            GetSnapshotFilterMap()
        {
            var field = typeof(AnimalRepository).GetField(
                "SnapshotFilterMap",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            Assert.NotNull(field);
            return (IReadOnlyList<(string, Func<string, Expression<Func<AnimalSnapshotView, bool>>>)>)field!.GetValue(null)!;
        }

        private static async Task<List<AnimalSnapshotView>> ApplyPredicateAsync(
            Func<string, Expression<Func<AnimalSnapshotView, bool>>> predicate,
            string value,
            IEnumerable<AnimalSnapshotView> rows)
        {
            var query = SnapshotQueryable(rows).Where(predicate(value));
            return await query.ToListAsync();
        }

        [Fact]
        public void SnapshotFilterMap_ContainsExpectedKeys_InOrder()
        {
            var map = GetSnapshotFilterMap();

            var keys = map.Select(e => e.Key).ToArray();

            Assert.Equal(
                new[]
                {
                    "Directorate", "Program", "Contract", "Project", "ProjectStatus",
                    "Species", "SecurityLevel", "AnimalType", "JobCode", "Cost"
                },
                keys);
        }

        [Fact]
        public void SnapshotFilterMap_HasUniqueKeys_AndNonNullPredicates()
        {
            var map = GetSnapshotFilterMap();

            Assert.Equal(map.Count, map.Select(e => e.Key).Distinct().Count());
            Assert.All(map, e => Assert.NotNull(e.Predicate));
        }

        [Fact]
        public void SnapshotFilterMap_PredicateFactory_ProducesLambdaExpression()
        {
            var map = GetSnapshotFilterMap();

            foreach (var (key, predicate) in map)
            {
                var expression = predicate("x");
                Assert.NotNull(expression);
                Assert.Single(expression.Parameters);
                Assert.Equal(typeof(bool), expression.ReturnType);
            }
        }

        [Fact]
        public async Task SnapshotFilterMap_DirectoratePredicate_FiltersDirectorateColumn()
        {
            var predicate = GetSnapshotFilterMap().Single(e => e.Key == "Directorate").Predicate;

            var result = await ApplyPredicateAsync(
                predicate,
                "health",
                [
                    SnapshotRow(directorate: "Animal Health"),
                    SnapshotRow(directorate: "Marine")
                ]);

            Assert.Single(result);
            Assert.Equal("Animal Health", result[0].Directorate);
        }

        [Fact]
        public async Task SnapshotFilterMap_AnimalTypePredicate_IsCaseInsensitive()
        {
            var predicate = GetSnapshotFilterMap().Single(e => e.Key == "AnimalType").Predicate;

            var result = await ApplyPredicateAsync(
                predicate,
                "cattle",
                [
                    SnapshotRow(animalType: "CATTLE"),
                    SnapshotRow(animalType: "SHEEP")
                ]);

            Assert.Single(result);
            Assert.Equal("CATTLE", result[0].AnimalType);
        }

        [Fact]
        public async Task SnapshotFilterMap_CostPredicate_FiltersOnStringifiedCost()
        {
            var predicate = GetSnapshotFilterMap().Single(e => e.Key == "Cost").Predicate;

            var result = await ApplyPredicateAsync(
                predicate,
                "125",
                [
                    SnapshotRow(cost: 125m),
                    SnapshotRow(cost: 300m)
                ]);

            Assert.Single(result);
            Assert.Equal(125m, result[0].Cost);
        }

        [Fact]
        public async Task SnapshotFilterMap_EachPredicate_MatchesItsOwnColumnOnly()
        {
            var map = GetSnapshotFilterMap();

            // A row whose every filterable column has a distinct sentinel value.
            var target = new AnimalSnapshotView
            {
                Directorate = "DIR_VAL",
                Program = "PRG_VAL",
                Contract = "CON_VAL",
                Project = "PRJ_VAL",
                ProjectStatus = "STA_VAL",
                Species = "SPE_VAL",
                SecurityLevel = "SEC_VAL",
                AnimalType = "ANI_VAL",
                JobCode = "JOB_VAL",
                Cost = 777m
            };
            var other = SnapshotRow(
                directorate: "zzz", program: "zzz", contract: "zzz", project: "zzz",
                projectStatus: "zzz", species: "zzz", securityLevel: "zzz",
                animalType: "zzz", jobCode: "zzz", cost: 111m);

            var searchValues = new Dictionary<string, string>
            {
                ["Directorate"] = "DIR_VAL",
                ["Program"] = "PRG_VAL",
                ["Contract"] = "CON_VAL",
                ["Project"] = "PRJ_VAL",
                ["ProjectStatus"] = "STA_VAL",
                ["Species"] = "SPE_VAL",
                ["SecurityLevel"] = "SEC_VAL",
                ["AnimalType"] = "ANI_VAL",
                ["JobCode"] = "JOB_VAL",
                ["Cost"] = "777"
            };

            foreach (var (key, predicate) in map)
            {
                var result = await ApplyPredicateAsync(predicate, searchValues[key], [target, other]);
                Assert.Single(result);
                Assert.Same(target, result[0]);
            }
        }

        #endregion

        #region GetAnimalSnapshotAsync Tests

        // NOTE: GetAnimalSnapshotAsync builds its query via BuildAnimalSnapshotQuery which
        // joins Programs, ProjectViews, AnimalRequests and Animals and filters with
        // EF.Functions.ILike(prj.UserEmail, _requestContext.UserEmailId). EF.Functions.ILike
        // cannot be evaluated client-side by the in-memory/mock provider and requires a real
        // PostgreSQL connection, so these paths are covered by integration tests.

        [Fact(Skip = "EF.Functions.ILike in join query requires PostgreSQL provider; covered by integration tests.")]
        public async Task GetAnimalSnapshotAsync_WithMatchingRows_ReturnsPagedData()
        {
            await Task.CompletedTask;
        }

        [Fact(Skip = "EF.Functions.ILike in join query requires PostgreSQL provider; covered by integration tests.")]
        public async Task GetAnimalSnapshotAsync_WithNoMatchingRows_ReturnsEmptyPage()
        {
            await Task.CompletedTask;
        }

        [Fact(Skip = "EF.Functions.ILike in join query requires PostgreSQL provider; covered by integration tests.")]
        public async Task GetAnimalSnapshotAsync_AppliesFilterSortingAndPaging()
        {
            await Task.CompletedTask;
        }

        #endregion
    }
}
