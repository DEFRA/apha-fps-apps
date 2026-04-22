using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.ProjectSubContractRepositoryTest
{
    public class ProjectSubContractRepositoryTests
    {
        private const int DefaultTestFpsYear = 2024;

        /// <summary>
        /// Creates a ProjectSubContractRepository alongside mocked DbSet and context for call verification.
        /// AddAsync is set up explicitly since it differs from the base SetupDbSetOperations.
        /// UpdateAsync uses Entry().State — tested via Callback+Throws pattern (mirrors JobCodeRepositoryTests).
        /// </summary>
        private static (
            ProjectSubContractRepository Repo,
            Mock<DbSet<ProjectSubContract>> SubContractsDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(
                IEnumerable<ProjectSubContract> subContracts,
                int fpsYear = DefaultTestFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var subContractsMockSet = RepositoryTestHelper.CreateMockDbSet(subContracts);

            RepositoryTestHelper.SetupDbSetOperations(subContractsMockSet);
            subContractsMockSet
                .Setup(x => x.AddAsync(It.IsAny<ProjectSubContract>(), It.IsAny<CancellationToken>()))
                .Returns((ProjectSubContract _, CancellationToken __) => new ValueTask<EntityEntry<ProjectSubContract>>());
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.ProjectSubContracts).Returns(subContractsMockSet.Object);

            var repo = new ProjectSubContractRepository(mockContext.Object, fpsRequestContext);
            return (repo, subContractsMockSet, mockContext);
        }

        private static ProjectSubContractRepository CreateRepository(
            IEnumerable<ProjectSubContract> subContracts,
            int fpsYear = DefaultTestFpsYear)
            => CreateRepositoryWithMocks(subContracts, fpsYear).Repo;

        #region GetPagedProjectSubContractsAsync

        [Fact]
        public async Task GetPagedProjectSubContractsAsync_WithProject_ReturnsFilteredPagedResult()
        {
            var subContracts = new List<ProjectSubContract>
            {
                new() { SubContCounter = 1, Project = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 2, Project = "PRJ2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(subContracts);
            var query = new PaginationParameters<string>();

            var result = await repo.GetPagedProjectSubContractsAsync(query, "PRJ1");

            Assert.Single(result.Data);
            Assert.Equal(1, result.Data.First().SubContCounter);
            Assert.Equal(1, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedProjectSubContractsAsync_NullProject_ReturnsAllRecordsPaged()
        {
            var subContracts = new List<ProjectSubContract>
            {
                new() { SubContCounter = 1, Project = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 2, Project = "PRJ2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(subContracts);
            var query = new PaginationParameters<string>();

            var result = await repo.GetPagedProjectSubContractsAsync(query, null);

            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        #endregion

        #region GetTotalAmountAsync

        [Fact]
        public async Task GetTotalAmountAsync_WithMatchingProject_ReturnsSumOfAmounts()
        {
            var subContracts = new List<ProjectSubContract>
            {
                new() { SubContCounter = 1, Project = "PRJ1", Amount = 800m,  FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 2, Project = "PRJ1", Amount = 200m,  FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 3, Project = "PRJ2", Amount = 1000m, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(subContracts);

            var result = await repo.GetTotalAmountAsync("PRJ1");

            Assert.Equal(1000m, result);
        }

        [Fact]
        public async Task GetTotalAmountAsync_NullProject_ReturnsTotalOfAllAmounts()
        {
            var subContracts = new List<ProjectSubContract>
            {
                new() { SubContCounter = 1, Project = "PRJ1", Amount = 500m, FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 2, Project = "PRJ2", Amount = 300m, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(subContracts);

            var result = await repo.GetTotalAmountAsync(null);

            Assert.Equal(800m, result);
        }

        [Fact]
        public async Task GetTotalAmountAsync_NoMatchingRecords_ReturnsZero()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetTotalAmountAsync("PRJ_NONE");

            Assert.Equal(0m, result);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsSubContract()
        {
            var subContracts = new List<ProjectSubContract>
            {
                new() { SubContCounter = 1, Project = "PRJ1", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(subContracts);

            var result = await repo.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.SubContCounter);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentId_ReturnsNull()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetByIdAsync(99);

            Assert.Null(result);
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ValidEntity_SetsFpsYearAndSaves()
        {
            var (repo, subContractsMockSet, mockContext) = CreateRepositoryWithMocks([]);
            var entity = new ProjectSubContract { Project = "PRJ1", Amount = 500m };

            var result = await repo.CreateAsync(entity);

            Assert.NotNull(result);
            Assert.Equal(DefaultTestFpsYear, result.FpsYear);
            subContractsMockSet.Verify(x => x.AddAsync(It.IsAny<ProjectSubContract>(), It.IsAny<CancellationToken>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task CreateAsync_SetsFpsYear_FromYearContext()
        {
            const int customYear = 2025;
            var (repo, _, _) = CreateRepositoryWithMocks([], fpsYear: customYear);
            var entity = new ProjectSubContract { Project = "PRJ1" };

            var result = await repo.CreateAsync(entity);

            Assert.Equal(customYear, result.FpsYear);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ValidEntity_SetsFpsYearBeforeEntryIsCalled()
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultTestFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var subContractsMockSet = RepositoryTestHelper.CreateMockDbSet<ProjectSubContract>([]);
            mockContext.Setup(x => x.ProjectSubContracts).Returns(subContractsMockSet.Object);

            var entryWasCalled = false;
            mockContext.Setup(x => x.Entry(It.IsAny<ProjectSubContract>()))
                .Callback(() => entryWasCalled = true)
                .Throws(new NotSupportedException("Entry() is not supported in mocked DbContext"));

            var repo = new ProjectSubContractRepository(mockContext.Object, fpsRequestContext);
            var entity = new ProjectSubContract { SubContCounter = 1, Project = "PRJ1" };

            await Assert.ThrowsAsync<NotSupportedException>(() => repo.UpdateAsync(entity));

            Assert.Equal(DefaultTestFpsYear, entity.FpsYear);
            Assert.True(entryWasCalled);
        }

        [Fact]
        public async Task UpdateAsync_SetsFpsYear_FromYearContext()
        {
            const int customYear = 2025;
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(customYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var subContractsMockSet = RepositoryTestHelper.CreateMockDbSet<ProjectSubContract>([]);
            mockContext.Setup(x => x.ProjectSubContracts).Returns(subContractsMockSet.Object);

            mockContext.Setup(x => x.Entry(It.IsAny<ProjectSubContract>()))
                .Throws(new NotSupportedException("Entry() is not supported in mocked DbContext"));

            var repo = new ProjectSubContractRepository(mockContext.Object, fpsRequestContext);
            var entity = new ProjectSubContract { SubContCounter = 1 };

            await Assert.ThrowsAsync<NotSupportedException>(() => repo.UpdateAsync(entity));

            Assert.Equal(customYear, entity.FpsYear);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ExistingId_RemovesAndReturnsTrue()
        {
            var subContracts = new List<ProjectSubContract>
            {
                new() { SubContCounter = 1, FpsYear = DefaultTestFpsYear }
            };
            var (repo, subContractsMockSet, mockContext) = CreateRepositoryWithMocks(subContracts);

            var result = await repo.DeleteAsync(1);

            Assert.True(result);
            RepositoryTestHelper.VerifyRemove(subContractsMockSet);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task DeleteAsync_NonExistentId_ReturnsFalse()
        {
            var repo = CreateRepository([]);

            var result = await repo.DeleteAsync(99);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_WrongFpsYear_ReturnsFalse()
        {
            var subContracts = new List<ProjectSubContract>
            {
                new() { SubContCounter = 1, FpsYear = 2020 }
            };
            var repo = CreateRepository(subContracts, fpsYear: DefaultTestFpsYear);

            var result = await repo.DeleteAsync(1);

            Assert.False(result);
        }

        #endregion
    }
}
