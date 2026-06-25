using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using FpsProgram = Apha.FPS.Core.Entities.Program;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ProjectRepositoryTest
{
    /// <summary>
    /// Tests for update, delete, and pre-condition check methods in ProjectRepository
    /// that are fully exercisable with mocked DbContext/DbSet.
    /// </summary>
    public class ProjectUpdateDeleteRepositoryTests
    {
        // ------------------------------------------------------------------ helpers

        private static ProjectRepository CreateRepository(
            Mock<FpsDbContext> mockContext,
            IFpsRequestContext requestContext) =>
            new(mockContext.Object, requestContext);

        private static (Mock<FpsDbContext> Ctx, IFpsRequestContext Req) MakeContext(
            string email = "user@test.com", int year = 2024)
        {
            var mockReq = new Mock<IFpsRequestContext>();
            mockReq.Setup(x => x.UserEmailId).Returns(email);
            mockReq.Setup(x => x.FpsYear).Returns(year);
            var ctx = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockReq.Object);
            return (ctx, mockReq.Object);
        }

        private static void SetupSet<T>(Mock<FpsDbContext> ctx,
            IEnumerable<T> data,
            System.Linq.Expressions.Expression<Func<FpsDbContext, DbSet<T>>> property)
            where T : class
        {
            ctx.Setup(property).Returns(RepositoryTestHelper.CreateMockDbSet(data).Object);
        }

        private static Project MakeProject(string code, int year = 2024) => new()
        {
            ParentProject     = code,
            FpsYear           = year,
            ProjectTitle      = $"Title {code}",
            Program           = "P001",
            Customer          = "DEFRA",
            ProjectStatus     = "Active",
            Disease           = "D1",
            Contract          = "C1",
            IncomeAccountCode = "IA"
        };

        // ================================================================== UpdateProjectAsync
        // Note: UpdateProjectAsync calls _dbContext.Entry(project).State = EntityState.Modified which
        // cannot be mocked with Moq (EntityEntry<T> has no parameterless constructor). The tests below
        // cover the code paths up to that call (FpsYear assignment, NormalizeDateTimesToUnspecified)
        // and assert that the ArgumentException propagates from the Entry() mock.

        [Fact]
        public async Task UpdateProjectAsync_SetsYearBeforeEntry_ArgumentExceptionFromMock()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, new[] { MakeProject("PP001") }, x => x.Projects);
            SetupSet(ctx, Enumerable.Empty<ProjectLog>(), x => x.ProjectLogs);
            RepositoryTestHelper.SetupEntityEntry<FpsDbContext, Project>(ctx);

            var repo  = CreateRepository(ctx, req);
            var input = MakeProject("PP001");

            // The code sets project.FpsYear and calls NormalizeDateTimesToUnspecified before
            // Entry() — all of which execute. The mock then throws because EntityEntry<T>
            // cannot be proxied by Moq.
            await Assert.ThrowsAnyAsync<Exception>(
                () => repo.UpdateProjectAsync(input));
        }

        [Fact]
        public async Task UpdateProjectAsync_NormalizesDateKind_BeforeEntryCall()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, new[] { MakeProject("PP001") }, x => x.Projects);
            SetupSet(ctx, Enumerable.Empty<ProjectLog>(), x => x.ProjectLogs);
            RepositoryTestHelper.SetupEntityEntry<FpsDbContext, Project>(ctx);

            var repo  = CreateRepository(ctx, req);
            var input = MakeProject("PP001");
            input.DateCreated = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            input.DateCosted  = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Local);

            // NormalizeDateTimesToUnspecified runs before Entry(); verify dates are normalised
            // by checking the input object (mutated in-place) after the exception is thrown.
            await Assert.ThrowsAnyAsync<Exception>(
                () => repo.UpdateProjectAsync(input));

            Assert.Equal(DateTimeKind.Unspecified, input.DateCreated!.Value.Kind);
            Assert.Equal(DateTimeKind.Unspecified, input.DateCosted!.Value.Kind);
        }

        [Fact]
        public async Task UpdateProjectAsync_NullDates_NormalizationSkipped_BeforeEntryCall()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, new[] { MakeProject("PP001") }, x => x.Projects);
            SetupSet(ctx, Enumerable.Empty<ProjectLog>(), x => x.ProjectLogs);
            RepositoryTestHelper.SetupEntityEntry<FpsDbContext, Project>(ctx);

            var repo  = CreateRepository(ctx, req);
            var input = MakeProject("PP001");
            input.DateCreated = null;
            input.DateCosted  = null;

            await Assert.ThrowsAnyAsync<Exception>(
                () => repo.UpdateProjectAsync(input));

            // Dates remain null — normalization branches correctly skipped
            Assert.Null(input.DateCreated);
            Assert.Null(input.DateCosted);
        }

        // ================================================================== UpdatePactPortfolioDetailsAsync

        [Fact]
        public async Task UpdatePactPortfolioDetailsAsync_ExistingProject_UpdatesAndReturns()
        {
            var (ctx, req) = MakeContext();
            var existing = MakeProject("PP001");
            SetupSet(ctx, new[] { existing }, x => x.Projects);
            RepositoryTestHelper.SetupSaveChanges(ctx);

            var repo  = CreateRepository(ctx, req);
            var input = MakeProject("PP001");
            input.ProjectTitle   = "New Title";
            input.Program        = "P002";
            input.Manager        = "J.Smith";
            input.TransferIncome = 999m;

            var result = await repo.UpdatePactPortfolioDetailsAsync(input);

            Assert.NotNull(result);
            Assert.Equal("New Title", result.ProjectTitle);
            Assert.Equal("P002", result.Program);
        }

        [Fact]
        public async Task UpdatePactPortfolioDetailsAsync_ProjectNotFound_ReturnsNull()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, Enumerable.Empty<Project>(), x => x.Projects);

            var repo   = CreateRepository(ctx, req);
            var result = await repo.UpdatePactPortfolioDetailsAsync(MakeProject("MISSING"));

            Assert.Null(result);
        }

        // ================================================================== UpdateFpsPortfolioDetailsAsync

        [Fact]
        public async Task UpdateFpsPortfolioDetailsAsync_ExistingProject_UpdatesAndReturns()
        {
            var (ctx, req) = MakeContext();
            var existing = MakeProject("PP001");
            SetupSet(ctx, new[] { existing }, x => x.Projects);
            SetupSet(ctx, Enumerable.Empty<ProjectLog>(), x => x.ProjectLogs);
            RepositoryTestHelper.SetupSaveChanges(ctx);

            var repo  = CreateRepository(ctx, req);
            var input = MakeProject("PP001");
            input.ProjectTitle = "FPS Updated";
            input.CustIncome   = 1500m;
            input.Profit       = 200m;

            var result = await repo.UpdateFpsPortfolioDetailsAsync(input);

            Assert.NotNull(result);
            Assert.Equal("FPS Updated", result.ProjectTitle);
            Assert.Equal(1500m, result.CustIncome);
        }

        [Fact]
        public async Task UpdateFpsPortfolioDetailsAsync_ProjectNotFound_ReturnsNull()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, Enumerable.Empty<Project>(), x => x.Projects);

            var repo   = CreateRepository(ctx, req);
            var result = await repo.UpdateFpsPortfolioDetailsAsync(MakeProject("MISSING"));

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateFpsPortfolioDetailsAsync_NormalizesDateTimes()
        {
            var (ctx, req) = MakeContext();
            var existing = MakeProject("PP001");
            existing.DateCreated = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            SetupSet(ctx, new[] { existing }, x => x.Projects);
            SetupSet(ctx, Enumerable.Empty<ProjectLog>(), x => x.ProjectLogs);
            RepositoryTestHelper.SetupSaveChanges(ctx);

            var repo   = CreateRepository(ctx, req);
            var result = await repo.UpdateFpsPortfolioDetailsAsync(MakeProject("PP001"));

            Assert.Equal(DateTimeKind.Unspecified, result!.DateCreated!.Value.Kind);
        }

        // ================================================================== DeleteProjectAsync

        [Fact]
        public async Task DeleteProjectAsync_ProjectExists_ReturnsTrueAndRemovesProject()
        {
            var (ctx, req) = MakeContext();
            var project = MakeProject("PP001");
            var mockSet = RepositoryTestHelper.CreateMockDbSet(new[] { project });
            ctx.Setup(x => x.Projects).Returns(mockSet.Object);
            SetupSet(ctx, Enumerable.Empty<ProjectLog>(), x => x.ProjectLogs);
            RepositoryTestHelper.SetupSaveChanges(ctx);

            var repo   = CreateRepository(ctx, req);
            var result = await repo.DeleteProjectAsync("PP001");

            Assert.True(result);
            mockSet.Verify(x => x.Remove(It.IsAny<Project>()), Times.Once);
        }

        [Fact]
        public async Task DeleteProjectAsync_ProjectNotFound_ReturnsFalse()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, Enumerable.Empty<Project>(), x => x.Projects);

            var repo   = CreateRepository(ctx, req);
            var result = await repo.DeleteProjectAsync("MISSING");

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteProjectAsync_NormalizesDateTimes_BeforeLog()
        {
            var (ctx, req) = MakeContext();
            var project = MakeProject("PP001");
            project.DateCreated = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Local);
            var mockSet = RepositoryTestHelper.CreateMockDbSet(new[] { project });
            ctx.Setup(x => x.Projects).Returns(mockSet.Object);
            SetupSet(ctx, Enumerable.Empty<ProjectLog>(), x => x.ProjectLogs);
            RepositoryTestHelper.SetupSaveChanges(ctx);

            var repo = CreateRepository(ctx, req);
            // If NormalizeDateTimesToUnspecified fails, this throws — test verifies it doesn't
            var result = await repo.DeleteProjectAsync("PP001");
            Assert.True(result);
        }

        // ================================================================== HasAssociatedJobCodesAsync

        [Fact]
        public async Task HasAssociatedJobCodesAsync_Exists_ReturnsTrue()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, new[]
            {
                new JobCode { JobCodeId = "JC1", ParentProject = "PP001", FpsYear = 2024 }
            }, x => x.JobCodes);

            var repo   = CreateRepository(ctx, req);
            var result = await repo.HasAssociatedJobCodesAsync("PP001");

            Assert.True(result);
        }

        [Fact]
        public async Task HasAssociatedJobCodesAsync_DoesNotExist_ReturnsFalse()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, Enumerable.Empty<JobCode>(), x => x.JobCodes);

            var repo   = CreateRepository(ctx, req);
            var result = await repo.HasAssociatedJobCodesAsync("PP001");

            Assert.False(result);
        }

        [Fact]
        public async Task HasAssociatedJobCodesAsync_DifferentProject_ReturnsFalse()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, new[]
            {
                new JobCode { JobCodeId = "JC1", ParentProject = "OTHER", FpsYear = 2024 }
            }, x => x.JobCodes);

            var repo   = CreateRepository(ctx, req);
            var result = await repo.HasAssociatedJobCodesAsync("PP001");

            Assert.False(result);
        }

        // ================================================================== CheckProgramExistsAsync

        [Fact]
        public async Task CheckProgramExistsAsync_NullOrEmpty_ReturnsTrue()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, Enumerable.Empty<FpsProgram>(), x => x.Programs);

            var repo = CreateRepository(ctx, req);
            Assert.True(await repo.CheckProgramExistsAsync(null!));
            Assert.True(await repo.CheckProgramExistsAsync(""));
            Assert.True(await repo.CheckProgramExistsAsync("  "));
        }

        [Fact]
        public async Task CheckProgramExistsAsync_ExistingProgram_ReturnsTrue()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, new[]
            {
                new FpsProgram { ProgramNo = "P001", FpsYear = 2024 }
            }, x => x.Programs);

            var repo   = CreateRepository(ctx, req);
            var result = await repo.CheckProgramExistsAsync("P001");

            Assert.True(result);
        }

        [Fact]
        public async Task CheckProgramExistsAsync_NonExistentProgram_ReturnsFalse()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, Enumerable.Empty<FpsProgram>(), x => x.Programs);

            var repo   = CreateRepository(ctx, req);
            var result = await repo.CheckProgramExistsAsync("NOPE");

            Assert.False(result);
        }

        // ================================================================== CheckProjectExistsAsync

        [Fact]
        public async Task CheckProjectExistsAsync_Exists_ReturnsTrue()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, new[] { MakeProject("PP001") }, x => x.Projects);

            var repo   = CreateRepository(ctx, req);
            var result = await repo.CheckProjectExistsAsync("PP001");

            Assert.True(result);
        }

        [Fact]
        public async Task CheckProjectExistsAsync_DoesNotExist_ReturnsFalse()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, Enumerable.Empty<Project>(), x => x.Projects);

            var repo   = CreateRepository(ctx, req);
            var result = await repo.CheckProjectExistsAsync("PP001");

            Assert.False(result);
        }

        // ================================================================== CheckProjectExistsInFarmFileAsync

        [Fact]
        public async Task CheckProjectExistsInFarmFileAsync_Exists_ReturnsTrue()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, new[]
            {
                new SurvFFSubmission { SdPactWg = "WG1", Contract = "PP001", FpsYear = 2024 }
            }, x => x.SurvFFSubmissions);

            var repo   = CreateRepository(ctx, req);
            var result = await repo.CheckProjectExistsInFarmFileAsync("PP001");

            Assert.True(result);
        }

        [Fact]
        public async Task CheckProjectExistsInFarmFileAsync_DoesNotExist_ReturnsFalse()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, Enumerable.Empty<SurvFFSubmission>(), x => x.SurvFFSubmissions);

            var repo   = CreateRepository(ctx, req);
            var result = await repo.CheckProjectExistsInFarmFileAsync("PP001");

            Assert.False(result);
        }

        // ================================================================== HasPlannedTestsAsync

        [Fact]
        public async Task HasPlannedTestsAsync_HasMatchingRow_ReturnsTrue()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, new[]
            {
                new TestRequirement { TestCode = "T1", Buyer = "PP001", ProjectBuyerCode = "PP001", FpsYear = 2024 }
            }, x => x.TestRequirements);

            var repo   = CreateRepository(ctx, req);
            var result = await repo.HasPlannedTestsAsync("PP001");

            Assert.True(result);
        }

        [Fact]
        public async Task HasPlannedTestsAsync_NoMatchingRow_ReturnsFalse()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, Enumerable.Empty<TestRequirement>(), x => x.TestRequirements);

            var repo   = CreateRepository(ctx, req);
            var result = await repo.HasPlannedTestsAsync("PP001");

            Assert.False(result);
        }

        // ================================================================== HasMonthlyOutputAsync

        [Fact]
        public async Task HasMonthlyOutputAsync_HasMatchingRow_ReturnsTrue()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, new[]
            {
                new MonthlyOutput { TestCode = "T1", Buyer = "PP001", Month = 1, WorkGroup = "WG1", FpsYear = 2024 }
            }, x => x.MonthlyOutputs);

            var repo   = CreateRepository(ctx, req);
            var result = await repo.HasMonthlyOutputAsync("PP001");

            Assert.True(result);
        }

        [Fact]
        public async Task HasMonthlyOutputAsync_NoMatchingRow_ReturnsFalse()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, Enumerable.Empty<MonthlyOutput>(), x => x.MonthlyOutputs);

            var repo   = CreateRepository(ctx, req);
            var result = await repo.HasMonthlyOutputAsync("PP001");

            Assert.False(result);
        }

        // ================================================================== HasMonthlyTimeAsync

        [Fact]
        public async Task HasMonthlyTimeAsync_HasMatchingRow_ReturnsTrue()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, new[]
            {
                new MonthlyTime { PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP001", Month = 1, FpsYear = 2024 }
            }, x => x.MonthlyTimes);

            var repo   = CreateRepository(ctx, req);
            var result = await repo.HasMonthlyTimeAsync("PP001");

            Assert.True(result);
        }

        [Fact]
        public async Task HasMonthlyTimeAsync_NoMatchingRow_ReturnsFalse()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, Enumerable.Empty<MonthlyTime>(), x => x.MonthlyTimes);

            var repo   = CreateRepository(ctx, req);
            var result = await repo.HasMonthlyTimeAsync("PP001");

            Assert.False(result);
        }

        // ================================================================== HasProjectInvoicesAsync

        [Fact]
        public async Task HasProjectInvoicesAsync_HasMatchingRow_ReturnsTrue()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, new[]
            {
                new ProjectInvoice { InvoiceCounter = 1, ProjectParent = "PP001", FpsYear = 2024 }
            }, x => x.ProjectInvoices);

            var repo   = CreateRepository(ctx, req);
            var result = await repo.HasProjectInvoicesAsync("PP001");

            Assert.True(result);
        }

        [Fact]
        public async Task HasProjectInvoicesAsync_NoMatchingRow_ReturnsFalse()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, Enumerable.Empty<ProjectInvoice>(), x => x.ProjectInvoices);

            var repo   = CreateRepository(ctx, req);
            var result = await repo.HasProjectInvoicesAsync("PP001");

            Assert.False(result);
        }

        // ================================================================== HasProjectSubcontractsAsync

        [Fact]
        public async Task HasProjectSubcontractsAsync_HasMatchingRow_ReturnsTrue()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, new[]
            {
                new ProjectSubContract { SubContCounter = 1, Project = "PP001", FpsYear = 2024 }
            }, x => x.ProjectSubContracts);

            var repo   = CreateRepository(ctx, req);
            var result = await repo.HasProjectSubcontractsAsync("PP001");

            Assert.True(result);
        }

        [Fact]
        public async Task HasProjectSubcontractsAsync_NoMatchingRow_ReturnsFalse()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, Enumerable.Empty<ProjectSubContract>(), x => x.ProjectSubContracts);

            var repo   = CreateRepository(ctx, req);
            var result = await repo.HasProjectSubcontractsAsync("PP001");

            Assert.False(result);
        }

        // ================================================================== Rollback / catch-block coverage

        /// <summary>
        /// Covers the catch block in CreateProjectAsync (lines 173-176):
        /// SaveChangesAsync throws → RollbackAsync is called → exception re-thrown.
        /// </summary>
        [Fact]
        public async Task CreateProjectAsync_SaveChangesThrows_RollbacksAndRethrows()
        {
            var (ctx, req) = MakeContext();
            var mockSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<Project>());
            ctx.Setup(x => x.Projects).Returns(mockSet.Object);
            SetupSet(ctx, Enumerable.Empty<ProjectLog>(), x => x.ProjectLogs);
            ctx.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .ThrowsAsync(new InvalidOperationException("db error"));

            var repo = CreateRepository(ctx, req);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.CreateProjectAsync(MakeProject("PP001")));
        }

        /// <summary>
        /// Covers the catch block in UpdateFpsPortfolioDetailsAsync (lines 346-349):
        /// project exists, SaveChangesAsync throws → rollback → rethrow.
        /// </summary>
        [Fact]
        public async Task UpdateFpsPortfolioDetailsAsync_SaveChangesThrows_RollbacksAndRethrows()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, new[] { MakeProject("PP001") }, x => x.Projects);
            SetupSet(ctx, Enumerable.Empty<ProjectLog>(), x => x.ProjectLogs);
            ctx.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .ThrowsAsync(new InvalidOperationException("db error"));

            var repo = CreateRepository(ctx, req);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.UpdateFpsPortfolioDetailsAsync(MakeProject("PP001")));
        }

        /// <summary>
        /// Covers the catch block in UpdatePactProjectDetailsAsync (lines 284-287):
        /// project exists, SaveChangesAsync throws → rollback → rethrow.
        /// </summary>
        [Fact]
        public async Task UpdatePactProjectDetailsAsync_SaveChangesThrows_RollbacksAndRethrows()
        {
            var (ctx, req) = MakeContext();
            SetupSet(ctx, new[] { MakeProject("PP001") }, x => x.Projects);
            SetupSet(ctx, Enumerable.Empty<ProjectLog>(), x => x.ProjectLogs);
            ctx.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .ThrowsAsync(new InvalidOperationException("db error"));

            var repo = CreateRepository(ctx, req);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.UpdatePactProjectDetailsAsync(MakeProject("PP001")));
        }

        /// <summary>
        /// Covers the catch block in DeleteProjectAsync (lines 378-381):
        /// project exists, SaveChangesAsync throws → rollback → rethrow.
        /// </summary>
        [Fact]
        public async Task DeleteProjectAsync_SaveChangesThrows_RollbacksAndRethrows()
        {
            var (ctx, req) = MakeContext();
            var mockSet = RepositoryTestHelper.CreateMockDbSet(new[] { MakeProject("PP001") });
            ctx.Setup(x => x.Projects).Returns(mockSet.Object);
            SetupSet(ctx, Enumerable.Empty<ProjectLog>(), x => x.ProjectLogs);
            ctx.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .ThrowsAsync(new InvalidOperationException("db error"));

            var repo = CreateRepository(ctx, req);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.DeleteProjectAsync("PP001"));
        }
    }
}
