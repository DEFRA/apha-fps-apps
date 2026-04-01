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

namespace Apha.PACT.DataAccess.UnitTests.Repository.TimeCodeValidRepositoryTest
{
    public class TimeCodeValidRepositoryTests
    {
        private const int DefaultTestFpsYear = 2024;

        /// <summary>
        /// Creates a TimeCodeValidRepository alongside mocked DbSet and context for call verification.
        /// AddAsync, AddRangeAsync, and RemoveRange are set up explicitly.
        /// DeleteTimeCodeValidAsync and DeleteAllByJobCodeAsync use Remove/RemoveRange (fully testable).
        /// CopyWorkGroupAsync uses AddRangeAsync (fully testable).
        /// UpdateTimeCodeValidAsync uses Entry().State — tested inline with Callback+Throws (EntityEntry cannot be proxied by Moq).
        /// </summary>
        private static (
            TimeCodeValidRepository Repo,
            Mock<DbSet<TimeCodeValid>> TimeCodeValidsDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(
                IEnumerable<TimeCodeValid> timeCodes,
                int fpsYear = DefaultTestFpsYear)
        {
            var fpsYearContext = Substitute.For<IFpsYearContext>();
            fpsYearContext.FPSYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);
            var timeCodesMockSet = RepositoryTestHelper.CreateMockDbSet(timeCodes);

            RepositoryTestHelper.SetupDbSetOperations(timeCodesMockSet);
            timeCodesMockSet
                .Setup(x => x.AddAsync(It.IsAny<TimeCodeValid>(), It.IsAny<CancellationToken>()))
                .Returns((TimeCodeValid _, CancellationToken __) => new ValueTask<EntityEntry<TimeCodeValid>>());
            timeCodesMockSet
                .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<TimeCodeValid>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            timeCodesMockSet
                .Setup(x => x.RemoveRange(It.IsAny<IEnumerable<TimeCodeValid>>()))
                .Verifiable();
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.TimeCodeValids).Returns(timeCodesMockSet.Object);

            var repo = new TimeCodeValidRepository(mockContext.Object, fpsYearContext);
            return (repo, timeCodesMockSet, mockContext);
        }

        private static TimeCodeValidRepository CreateRepository(
            IEnumerable<TimeCodeValid> timeCodes,
            int fpsYear = DefaultTestFpsYear)
            => CreateRepositoryWithMocks(timeCodes, fpsYear).Repo;

        #region GetByJobCodeAsync

        [Fact]
        public async Task GetByJobCodeAsync_MatchingFilters_ReturnsFilteredList()
        {
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1", FpsYear = DefaultTestFpsYear },
                new() { TimeCode = "TC2", WorkGroup = "WG2", ParentProject = "PRJ2", JobCode = "JC2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(timeCodes);

            var result = (await repo.GetByJobCodeAsync("JC1", "PRJ1")).ToList();

            Assert.Single(result);
            Assert.Equal("TC1", result[0].TimeCode);
        }

        [Fact]
        public async Task GetByJobCodeAsync_NoMatch_ReturnsEmptyList()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetByJobCodeAsync("JC_NONE", "PRJ_NONE");

            Assert.Empty(result);
        }

        #endregion

        #region GetPagedTimeCodesAsync

        [Fact]
        public async Task GetPagedTimeCodesAsync_WithJobCodeAndProject_ReturnsFilteredPagedResult()
        {
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1", FpsYear = DefaultTestFpsYear },
                new() { TimeCode = "TC2", WorkGroup = "WG2", ParentProject = "PRJ2", JobCode = "JC2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(timeCodes);
            var query = new PaginationParameters<string>();

            var result = await repo.GetPagedTimeCodesAsync(query, "JC1", "PRJ1");

            Assert.Single(result.Data);
            Assert.Equal("TC1", result.Data.First().TimeCode);
            Assert.Equal(1, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedTimeCodesAsync_NullFilters_ReturnsAllRecordsPaged()
        {
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1", FpsYear = DefaultTestFpsYear },
                new() { TimeCode = "TC2", WorkGroup = "WG2", ParentProject = "PRJ2", JobCode = "JC2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(timeCodes);
            var query = new PaginationParameters<string>();

            var result = await repo.GetPagedTimeCodesAsync(query, null, null);

            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        #endregion

        #region GetTimeCodeValidAsync

        [Fact]
        public async Task GetTimeCodeValidAsync_ExistingKey_ReturnsEntity()
        {
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(timeCodes);

            var result = await repo.GetTimeCodeValidAsync("WG1", "TC1", "PRJ1");

            Assert.NotNull(result);
            Assert.Equal("TC1", result.TimeCode);
            Assert.Equal("WG1", result.WorkGroup);
        }

        [Fact]
        public async Task GetTimeCodeValidAsync_NonExistentKey_ReturnsNull()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetTimeCodeValidAsync("WG_MISSING", "TC_MISSING", "PRJ1");

            Assert.Null(result);
        }

        #endregion

        #region CreateTimeCodeValidAsync

        [Fact]
        public async Task CreateTimeCodeValidAsync_ValidEntity_SetsFpsYearAndSaves()
        {
            var (repo, timeCodesMockSet, mockContext) = CreateRepositoryWithMocks([]);
            var entity = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };

            var result = await repo.CreateTimeCodeValidAsync(entity);

            Assert.NotNull(result);
            Assert.Equal(DefaultTestFpsYear, result.FpsYear);
            timeCodesMockSet.Verify(x => x.AddAsync(It.IsAny<TimeCodeValid>(), It.IsAny<CancellationToken>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task CreateTimeCodeValidAsync_SetsFpsYear_FromYearContext()
        {
            const int customYear = 2025;
            var (repo, _, _) = CreateRepositoryWithMocks([], fpsYear: customYear);
            var entity = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };

            var result = await repo.CreateTimeCodeValidAsync(entity);

            Assert.Equal(customYear, result.FpsYear);
        }

        #endregion

        #region UpdateTimeCodeValidAsync

        [Fact]
        public async Task UpdateTimeCodeValidAsync_ValidEntity_SetsFpsYearBeforeEntryIsCalled()
        {
            // Arrange — Entry() cannot be proxied by Moq; use Callback+Throws to verify
            // FpsYear is stamped on the entity BEFORE Entry() is invoked (mirrors EmployeeRepositoryTests pattern)
            var fpsYearContext = Substitute.For<IFpsYearContext>();
            fpsYearContext.FPSYear.Returns(DefaultTestFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);
            var timeCodesMockSet = RepositoryTestHelper.CreateMockDbSet<TimeCodeValid>([]);
            mockContext.Setup(x => x.TimeCodeValids).Returns(timeCodesMockSet.Object);

            var entryWasCalled = false;
            mockContext.Setup(x => x.Entry(It.IsAny<TimeCodeValid>()))
                .Callback(() => entryWasCalled = true)
                .Throws(new NotSupportedException("Entry() is not supported in mocked DbContext"));

            var repo = new TimeCodeValidRepository(mockContext.Object, fpsYearContext);
            var entity = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };

            // Act & Assert
            await Assert.ThrowsAsync<NotSupportedException>(() => repo.UpdateTimeCodeValidAsync(entity));

            // FpsYear is assigned before Entry() is called, so it must be set despite the exception
            Assert.Equal(DefaultTestFpsYear, entity.FpsYear);
            Assert.True(entryWasCalled);
        }

        [Fact]
        public async Task UpdateTimeCodeValidAsync_SetsFpsYear_FromYearContext()
        {
            const int customYear = 2025;
            var fpsYearContext = Substitute.For<IFpsYearContext>();
            fpsYearContext.FPSYear.Returns(customYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);
            var timeCodesMockSet = RepositoryTestHelper.CreateMockDbSet<TimeCodeValid>([]);
            mockContext.Setup(x => x.TimeCodeValids).Returns(timeCodesMockSet.Object);

            mockContext.Setup(x => x.Entry(It.IsAny<TimeCodeValid>()))
                .Throws(new NotSupportedException("Entry() is not supported in mocked DbContext"));

            var repo = new TimeCodeValidRepository(mockContext.Object, fpsYearContext);
            var entity = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };

            await Assert.ThrowsAsync<NotSupportedException>(() => repo.UpdateTimeCodeValidAsync(entity));

            Assert.Equal(customYear, entity.FpsYear);
        }

        #endregion

        #region DeleteTimeCodeValidAsync

        [Fact]
        public async Task DeleteTimeCodeValidAsync_ExistingEntity_RemovesAndReturnsTrue()
        {
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", FpsYear = DefaultTestFpsYear }
            };
            var (repo, timeCodesMockSet, mockContext) = CreateRepositoryWithMocks(timeCodes);

            var result = await repo.DeleteTimeCodeValidAsync("WG1", "TC1", "PRJ1");

            Assert.True(result);
            RepositoryTestHelper.VerifyRemove(timeCodesMockSet);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task DeleteTimeCodeValidAsync_NonExistentEntity_ReturnsFalse()
        {
            var repo = CreateRepository([]);

            var result = await repo.DeleteTimeCodeValidAsync("WG_MISSING", "TC_MISSING", "PRJ1");

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteTimeCodeValidAsync_WrongFpsYear_ReturnsFalse()
        {
            // Entity exists but its FpsYear doesn't match the context year
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", FpsYear = 2020 }
            };
            var repo = CreateRepository(timeCodes, fpsYear: DefaultTestFpsYear);

            var result = await repo.DeleteTimeCodeValidAsync("WG1", "TC1", "PRJ1");

            Assert.False(result);
        }

        #endregion

        #region DeleteAllByJobCodeAsync

        [Fact]
        public async Task DeleteAllByJobCodeAsync_WithMatchingEntities_RemovesAllAndReturnsTrue()
        {
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1", FpsYear = DefaultTestFpsYear },
                new() { TimeCode = "TC2", WorkGroup = "WG2", ParentProject = "PRJ1", JobCode = "JC1", FpsYear = DefaultTestFpsYear }
            };
            var (repo, timeCodesMockSet, mockContext) = CreateRepositoryWithMocks(timeCodes);

            var result = await repo.DeleteAllByJobCodeAsync("JC1", "PRJ1");

            Assert.True(result);
            timeCodesMockSet.Verify(x => x.RemoveRange(It.IsAny<IEnumerable<TimeCodeValid>>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task DeleteAllByJobCodeAsync_NoMatchingEntities_DoesNotCallSaveChangesAndReturnsTrue()
        {
            var (repo, timeCodesMockSet, mockContext) = CreateRepositoryWithMocks([]);

            var result = await repo.DeleteAllByJobCodeAsync("JC_NONE", "PRJ_NONE");

            Assert.True(result);
            timeCodesMockSet.Verify(x => x.RemoveRange(It.IsAny<IEnumerable<TimeCodeValid>>()), Times.Never);
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 0);
        }

        #endregion

        #region CopyWorkGroupAsync

        [Fact]
        public async Task CopyWorkGroupAsync_WithSourceEntries_CreatesCopiesAndReturns()
        {
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC_SRC", Active = true, FpsYear = DefaultTestFpsYear }
            };
            var (repo, timeCodesMockSet, mockContext) = CreateRepositoryWithMocks(timeCodes);

            var result = (await repo.CopyWorkGroupAsync("JC_SRC", "JC_TGT", "PRJ1")).ToList();

            Assert.Single(result);
            Assert.Equal("JC_TGT", result[0].JobCode);
            Assert.Equal("JC_TGT", result[0].TimeCode);
            Assert.Equal("WG1", result[0].WorkGroup);
            Assert.Equal(DefaultTestFpsYear, result[0].FpsYear);
            timeCodesMockSet.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<TimeCodeValid>>(), It.IsAny<CancellationToken>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task CopyWorkGroupAsync_NoSourceEntries_ReturnsEmptyCollection()
        {
            var (repo, timeCodesMockSet, _) = CreateRepositoryWithMocks([]);

            var result = await repo.CopyWorkGroupAsync("JC_SRC", "JC_TGT", "PRJ1");

            Assert.Empty(result);
            timeCodesMockSet.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<TimeCodeValid>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion
    }
}
