using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ProfitCentreRepositoryTest
{
    public class ProfitCentreRepositoryTests
    {
        private static ProfitCentreRepository CreateRepository(
            IEnumerable<ProfitCentreView>? profitCentreViews = null,
            IEnumerable<ProfitCentre>? profitCentres = null,
            IEnumerable<UserProfitcentre>? userProfitCentres = null,
            IEnumerable<ProfitCentreGrade>? profitCentreGrades = null,
            IEnumerable<Workgroup>? workgroups = null,
            IEnumerable<User>? users = null)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(2024);
            requestContext.UserEmailId.Returns("test@example.com");

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);

            if (profitCentreViews != null)
            {
                var mockViewSet = RepositoryTestHelper.CreateMockDbSet(profitCentreViews);
                mockContext.Setup(x => x.ProfitCentreViews).Returns(mockViewSet.Object);
            }

            if (profitCentres != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(profitCentres);
                RepositoryTestHelper.SetupDbSetOperations(mockSet);
                mockContext.Setup(x => x.ProfitCentres).Returns(mockSet.Object);
            }

            var upcSet = RepositoryTestHelper.CreateMockDbSet(userProfitCentres ?? []);
            RepositoryTestHelper.SetupDbSetOperations(upcSet);
            mockContext.Setup(x => x.UserProfitcentres).Returns(upcSet.Object);

            var pcgSet = RepositoryTestHelper.CreateMockDbSet(profitCentreGrades ?? []);
            mockContext.Setup(x => x.ProfitCentreGrades).Returns(pcgSet.Object);

            var wgSet = RepositoryTestHelper.CreateMockDbSet(workgroups ?? []);
            mockContext.Setup(x => x.Workgroups).Returns(wgSet.Object);

            var userSet = RepositoryTestHelper.CreateMockDbSet(users ?? []);
            mockContext.Setup(x => x.Users).Returns(userSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new ProfitCentreRepository(mockContext.Object, requestContext);
        }

        private static ProfitCentreView BuildView(
            string id = "PC01",
            string name = "Centre One",
            string division = "DIV1",
            string userEmail = "test@example.com") =>
            new() { ProfitCentreId = id, ProfitCentreName = name, Division = division, UserEmail = userEmail };

        private static ProfitCentre BuildEntity(
            string id = "PC01",
            string name = "Centre One",
            string division = "DIV1") =>
            new() { ProfitCentreId = id, ProfitCentreName = name, Division = division };

        #region GetProfitCentresAsync Tests

        [Fact]
        public async Task GetProfitCentresAsync_ReturnsAllProfitCentres_WhenDataExists()
        {
            var profitCentres = new List<ProfitCentreView>
            {
                BuildView("PC01", "Profit Centre One", "DIV1"),
                BuildView("PC02", "Profit Centre Two", "DIV1"),
                BuildView("PC03", "Profit Centre Three", "DIV2")
            };
            var repo = CreateRepository(profitCentreViews: profitCentres);

            var result = await repo.GetProfitCentresAsync();

            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetProfitCentresAsync_ReturnsEmpty_WhenNoDataExists()
        {
            var repo = CreateRepository(profitCentreViews: []);

            var result = await repo.GetProfitCentresAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetProfitCentresAsync_ReturnsOrderedByProfitCentreId()
        {
            var profitCentres = new List<ProfitCentreView>
            {
                BuildView("PC03", "Centre Three", "DIV1"),
                BuildView("PC01", "Centre One",   "DIV1"),
                BuildView("PC02", "Centre Two",   "DIV1")
            };
            var repo = CreateRepository(profitCentreViews: profitCentres);

            var result = await repo.GetProfitCentresAsync();

            var resultList = result.ToList();
            Assert.Equal("PC01", resultList[0].ProfitCentreId);
            Assert.Equal("PC02", resultList[1].ProfitCentreId);
            Assert.Equal("PC03", resultList[2].ProfitCentreId);
        }

        [Fact]
        public async Task GetProfitCentresAsync_ReturnsSingle_WhenOneItemExists()
        {
            var profitCentres = new List<ProfitCentreView> { BuildView("PC01", "Centre One", "DIV1") };
            var repo = CreateRepository(profitCentreViews: profitCentres);

            var result = await repo.GetProfitCentresAsync();

            var single = Assert.Single(result);
            Assert.Equal("PC01", single.ProfitCentreId);
        }

        #endregion

        #region GetAllProfitCentresPagedAsync Tests

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            var repo = CreateRepository(profitCentres: []);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.GetAllProfitCentresPagedAsync(null!));
        }

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_ReturnsEmptyPagedData_WhenNoRecords()
        {
            var repo = CreateRepository(profitCentres: []);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.GetAllProfitCentresPagedAsync(query);
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_ReturnsAllRecords()
        {
            var entities = new List<ProfitCentre>
            {
                BuildEntity("PC01"), BuildEntity("PC02"), BuildEntity("PC03")
            };
            var repo = CreateRepository(profitCentres: entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.GetAllProfitCentresPagedAsync(query);
            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_ReturnsCorrectPage()
        {
            var entities = new List<ProfitCentre>
            {
                BuildEntity("PC01"), BuildEntity("PC02"), BuildEntity("PC03"),
                BuildEntity("PC04"), BuildEntity("PC05")
            };
            var repo = CreateRepository(profitCentres: entities);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };
            var result = await repo.GetAllProfitCentresPagedAsync(query);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_FiltersByProfitCentreId()
        {
            var entities = new List<ProfitCentre>
            {
                BuildEntity("PC01"), BuildEntity("PC02"), BuildEntity("RC01")
            };
            var repo = CreateRepository(profitCentres: entities);
            var filter = System.Text.Json.JsonSerializer.Serialize(
                new Dictionary<string, string> { { "ProfitCentreId", "PC" } });
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllProfitCentresPagedAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_FiltersByDivision()
        {
            var entities = new List<ProfitCentre>
            {
                BuildEntity("PC01", division: "VSD"),
                BuildEntity("PC02", division: "BSD"),
                BuildEntity("PC03", division: "VSD")
            };
            var repo = CreateRepository(profitCentres: entities);
            var filter = System.Text.Json.JsonSerializer.Serialize(
                new Dictionary<string, string> { { "Division", "VSD" } });
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllProfitCentresPagedAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_OrdersByProfitCentreIdAscByDefault()
        {
            var entities = new List<ProfitCentre>
            {
                BuildEntity("PC03"), BuildEntity("PC01"), BuildEntity("PC02")
            };
            var repo = CreateRepository(profitCentres: entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.GetAllProfitCentresPagedAsync(query);
            var list = result.Data.ToList();
            Assert.Equal("PC01", list[0].ProfitCentreId);
        }

        #endregion

        #region GetProfitCentreByIdAsync Tests

        [Fact]
        public async Task GetProfitCentreByIdAsync_ThrowsArgumentException_WhenIdIsNullOrEmpty()
        {
            var repo = CreateRepository(profitCentres: []);
            await Assert.ThrowsAsync<ArgumentException>(() => repo.GetProfitCentreByIdAsync(""));
        }

        [Fact]
        public async Task GetProfitCentreByIdAsync_ThrowsArgumentException_WhenIdIsWhiteSpace()
        {
            var repo = CreateRepository(profitCentres: []);
            await Assert.ThrowsAsync<ArgumentException>(() => repo.GetProfitCentreByIdAsync("   "));
        }

        [Fact]
        public async Task GetProfitCentreByIdAsync_ReturnsRecord_WhenFound()
        {
            var entities = new List<ProfitCentre> { BuildEntity("PC01", "Centre One", "DIV1") };
            var repo = CreateRepository(profitCentres: entities);
            var result = await repo.GetProfitCentreByIdAsync("PC01");
            Assert.NotNull(result);
            Assert.Equal("PC01", result.ProfitCentreId);
        }

        [Fact]
        public async Task GetProfitCentreByIdAsync_ReturnsNull_WhenNotFound()
        {
            var repo = CreateRepository(profitCentres: []);
            var result = await repo.GetProfitCentreByIdAsync("NOTEXIST");
            Assert.Null(result);
        }

        #endregion

        #region ProfitCentreExistsAsync Tests

        [Fact]
        public async Task ProfitCentreExistsAsync_ReturnsTrue_WhenExists()
        {
            var entities = new List<ProfitCentre> { BuildEntity("PC01") };
            var repo = CreateRepository(profitCentres: entities);
            var result = await repo.ProfitCentreExistsAsync("PC01");
            Assert.True(result);
        }

        [Fact]
        public async Task ProfitCentreExistsAsync_ReturnsFalse_WhenNotExists()
        {
            var repo = CreateRepository(profitCentres: []);
            var result = await repo.ProfitCentreExistsAsync("NOTEXIST");
            Assert.False(result);
        }

        [Fact]
        public async Task ProfitCentreExistsAsync_ThrowsArgumentException_WhenIdIsEmpty()
        {
            var repo = CreateRepository(profitCentres: []);
            await Assert.ThrowsAsync<ArgumentException>(() => repo.ProfitCentreExistsAsync(""));
        }

        #endregion

        #region CreateProfitCentreAsync Tests

        [Fact]
        public async Task CreateProfitCentreAsync_ThrowsArgumentNullException_WhenEntityIsNull()
        {
            var repo = CreateRepository(profitCentres: []);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.CreateProfitCentreAsync(null!));
        }

        [Fact]
        public async Task CreateProfitCentreAsync_ReturnsEntity_WhenSuccessful()
        {
            var entity = BuildEntity("PC01");
            var user = new User { UserId = 10, UserEmail = "test@example.com" };
            var repo = CreateRepository(
                profitCentres: [],
                userProfitCentres: [],
                users: [user]);

            var result = await repo.CreateProfitCentreAsync(entity);

            Assert.NotNull(result);
            Assert.Equal("PC01", result.ProfitCentreId);
        }

        [Fact]
        public async Task CreateProfitCentreAsync_UsesFallbackUserId_WhenUserNotFound()
        {
            var entity = BuildEntity("PC01");
            var repo = CreateRepository(
                profitCentres: [],
                userProfitCentres: [],
                users: []);

            // Should complete without throwing even when user is not found (falls back to userId 42)
            var result = await repo.CreateProfitCentreAsync(entity);

            Assert.NotNull(result);
            Assert.Equal("PC01", result.ProfitCentreId);
        }

        #endregion

        #region UpdateProfitCentreAsync Tests

        [Fact]
        public async Task UpdateProfitCentreAsync_ThrowsArgumentNullException_WhenEntityIsNull()
        {
            var repo = CreateRepository(profitCentres: []);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateProfitCentreAsync("PC01", null!));
        }

        [Fact]
        public async Task UpdateProfitCentreAsync_ThrowsArgumentException_WhenOriginalIdIsEmpty()
        {
            var repo = CreateRepository(profitCentres: []);
            await Assert.ThrowsAsync<ArgumentException>(() => repo.UpdateProfitCentreAsync("", BuildEntity()));
        }

        [Fact]
        public async Task UpdateProfitCentreAsync_ReturnsEntity_WhenNotFound()
        {
            var repo = CreateRepository(profitCentres: []);
            var result = await repo.UpdateProfitCentreAsync("NOTEXIST", BuildEntity("NOTEXIST"));
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateProfitCentreAsync_UpdatesFields_WhenIdUnchanged()
        {
            var existing = BuildEntity("PC01", "Old Name", "OLD");
            var updated  = BuildEntity("PC01", "New Name", "NEW");
            var repo = CreateRepository(
                profitCentres: [existing],
                profitCentreGrades: [],
                workgroups: [],
                userProfitCentres: []);

            var result = await repo.UpdateProfitCentreAsync("PC01", updated);

            Assert.Equal("New Name", result.ProfitCentreName);
            Assert.Equal("NEW", result.Division);
        }

        #endregion

        #region DeleteProfitCentreAsync Tests

        [Fact]
        public async Task DeleteProfitCentreAsync_ThrowsArgumentException_WhenIdIsEmpty()
        {
            var repo = CreateRepository(profitCentres: []);
            await Assert.ThrowsAsync<ArgumentException>(() => repo.DeleteProfitCentreAsync(""));
        }

        [Fact]
        public async Task DeleteProfitCentreAsync_ReturnsFalse_WhenNotFound()
        {
            var repo = CreateRepository(
                profitCentres: [],
                profitCentreGrades: [],
                workgroups: []);
            var result = await repo.DeleteProfitCentreAsync("NOTEXIST");
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteProfitCentreAsync_ReturnsTrue_WhenDeletedSuccessfully()
        {
            var repo = CreateRepository(
                profitCentres: [BuildEntity("PC01")],
                profitCentreGrades: [],
                workgroups: [],
                userProfitCentres: []);

            var result = await repo.DeleteProfitCentreAsync("PC01");

            Assert.True(result);
        }

        #endregion

        #region HasLinkedGradesAsync Tests

        [Fact]
        public async Task HasLinkedGradesAsync_ThrowsArgumentException_WhenIdIsEmpty()
        {
            var repo = CreateRepository(profitCentres: []);
            await Assert.ThrowsAsync<ArgumentException>(() => repo.HasLinkedGradesAsync(""));
        }

        [Fact]
        public async Task HasLinkedGradesAsync_ReturnsFalse_WhenNoGradesExist()
        {
            var repo = CreateRepository(profitCentres: [BuildEntity("PC01")], profitCentreGrades: []);
            var result = await repo.HasLinkedGradesAsync("PC01");
            Assert.False(result);
        }

        [Fact]
        public async Task HasLinkedGradesAsync_ReturnsTrue_WhenGradesExist()
        {
            var grades = new List<ProfitCentreGrade>
            {
                new() { PcGrade = "G1", DivisionGrade = "DG1", GradeCode = "GC1", ProfitCentre = "PC01" }
            };
            var repo = CreateRepository(profitCentres: [BuildEntity("PC01")], profitCentreGrades: grades);
            var result = await repo.HasLinkedGradesAsync("PC01");
            Assert.True(result);
        }

        #endregion

        #region HasLinkedWorkgroupsAsync Tests

        [Fact]
        public async Task HasLinkedWorkgroupsAsync_ThrowsArgumentException_WhenIdIsEmpty()
        {
            var repo = CreateRepository(profitCentres: []);
            await Assert.ThrowsAsync<ArgumentException>(() => repo.HasLinkedWorkgroupsAsync(""));
        }

        [Fact]
        public async Task HasLinkedWorkgroupsAsync_ReturnsFalse_WhenNoWorkgroupsExist()
        {
            var repo = CreateRepository(profitCentres: [BuildEntity("PC01")], workgroups: []);
            var result = await repo.HasLinkedWorkgroupsAsync("PC01");
            Assert.False(result);
        }

        [Fact]
        public async Task HasLinkedWorkgroupsAsync_ReturnsTrue_WhenWorkgroupsExist()
        {
            var workgroups = new List<Workgroup>
            {
                new() { WorkgroupName = "WG1", ProfitCentre = "PC01" }
            };
            var repo = CreateRepository(profitCentres: [BuildEntity("PC01")], workgroups: workgroups);
            var result = await repo.HasLinkedWorkgroupsAsync("PC01");
            Assert.True(result);
        }

        #endregion
    }
}
