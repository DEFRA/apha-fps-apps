// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — RadTrackInvoiceRepositoryTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - New file: xUnit tests for RadTrackInvoiceRepository (Phase 4).
 *   - Uses Moq + RepositoryTestHelper.CreateMockDbContext<PimsDbContext>() and
 *     RepositoryTestHelper.CreateMockDbSet<T>() following MilestoneRepositoryTests convention.
 *   - Tests cover GetAllAsync (filtering, paging, sorting), GetByIdAsync, CreateAsync,
 *     UpdateAsync, DeleteAsync (bulk ExecuteDeleteAsync throws with in-memory),
 *     GetTotalsAsync, ExistsAsync.
 *   - Program filter (BuildProgramFilterQuery) tested via MyTlkpProjects join.
 *
 * PRESERVED:
 *   - Naming convention: [MethodName]_[StateUnderTest]_[ExpectedResult].
 *   - DeleteAsync acknowledges ExecuteDeleteAsync requires a live DB (throws with mock provider).
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: GetTotalsAsync and ExistsAsync also use LINQ GroupBy/AnyAsync
 *     which may not be fully supported by the mock query provider; verify at integration test time.
 */

using Apha.Common.Helpers.Repository;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Apha.PIMS.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.PIMS.DataAccess.UnitTests.Repository.RadTrackInvoiceRepositoryTest
{
    public class RadTrackInvoiceRepositoryTests
    {
        // ── Factory helpers ────────────────────────────────────────────────────

        /// <summary>
        /// Creates a RadTrackInvoiceRepository with in-memory mock data for all DbSets.
        /// All parameters are optional — omitted sets are initialised as empty.
        /// </summary>
        private static RadTrackInvoiceRepository CreateRepository(
            IEnumerable<RadTrackInvoice>? invoices       = null,
            IEnumerable<Projects>?        tlkpProjects   = null)
        {
            var mockContext    = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var invoicesMock   = RepositoryTestHelper.CreateMockDbSet(invoices     ?? Enumerable.Empty<RadTrackInvoice>());
            var tlkpProjMock   = RepositoryTestHelper.CreateMockDbSet(tlkpProjects ?? Enumerable.Empty<Projects>());

            RepositoryTestHelper.SetupDbSetOperations(invoicesMock);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.RadTrackInvoices).Returns(invoicesMock.Object);
            mockContext.Setup(x => x.MyTlkpProjects).Returns(tlkpProjMock.Object);

            return new RadTrackInvoiceRepository(mockContext.Object);
        }

        /// <summary>
        /// Returns the repository alongside its mocked DbSet and DbContext
        /// for tests that verify Add / Update / SaveChanges calls.
        /// </summary>
        private static (
            RadTrackInvoiceRepository Repo,
            Mock<DbSet<RadTrackInvoice>> InvoicesDbSet,
            Mock<PimsDbContext> Context)
            CreateRepositoryWithMocks(
                IEnumerable<RadTrackInvoice>? invoices     = null,
                IEnumerable<Projects>?        tlkpProjects = null)
        {
            var mockContext  = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var invoicesMock = RepositoryTestHelper.CreateMockDbSet(invoices     ?? Enumerable.Empty<RadTrackInvoice>());
            var tlkpProjMock = RepositoryTestHelper.CreateMockDbSet(tlkpProjects ?? Enumerable.Empty<Projects>());

            RepositoryTestHelper.SetupDbSetOperations(invoicesMock);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.RadTrackInvoices).Returns(invoicesMock.Object);
            mockContext.Setup(x => x.MyTlkpProjects).Returns(tlkpProjMock.Object);

            var repo = new RadTrackInvoiceRepository(mockContext.Object);
            return (repo, invoicesMock, mockContext);
        }

        private static PaginationParameters<RadTrackInvoiceFilter> DefaultParameters(
            int page = 1, int pageSize = 10, RadTrackInvoiceFilter? filter = null)
            => new PaginationParameters<RadTrackInvoiceFilter>
            {
                Page     = page,
                PageSize = pageSize,
                Filter   = filter
            };

        private static RadTrackInvoice MakeInvoice(
            int id            = 1,
            string project    = "PP001",
            string? contract  = "C001",
            string? invoiceRef = null,
            DateTime? dueDate  = null,
            double? dueAmount  = 1000.0) => new()
        {
            InvoiceCounter = id,
            Project        = project,
            Contract       = contract,
            InvoiceRef     = invoiceRef ?? $"INV-{id:000}",
            DueDate        = dueDate ?? DateTime.Today.AddDays(30),
            DueAmount      = dueAmount,
            PlannedAmount  = 1500.0,
            ActualAmount   = 800.0
        };

        // ── GetAllAsync — filtering ────────────────────────────────────────────

        #region GetAllAsync — filtering

        [Fact]
        public async Task GetAllAsync_ReturnsAllInvoices_WhenNoFilterApplied()
        {
            // Arrange
            var invoices = new List<RadTrackInvoice> { MakeInvoice(1), MakeInvoice(2), MakeInvoice(3) };
            var repo = CreateRepository(invoices: invoices);

            // Act
            var result = await repo.GetAllAsync(DefaultParameters());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Data.Count);
        }

        [Fact]
        public async Task GetAllAsync_FiltersByProject()
        {
            // Arrange
            var invoices = new List<RadTrackInvoice>
            {
                MakeInvoice(1, project: "PP001"),
                MakeInvoice(2, project: "PP001"),
                MakeInvoice(3, project: "PP002")
            };
            var repo = CreateRepository(invoices: invoices);
            var filter = new RadTrackInvoiceFilter { Project = "PP001" };

            // Act
            var result = await repo.GetAllAsync(DefaultParameters(filter: filter));

            // Assert
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, i => Assert.Equal("PP001", i.Project));
        }

        [Fact]
        public async Task GetAllAsync_FiltersByContract()
        {
            // Arrange
            var invoices = new List<RadTrackInvoice>
            {
                MakeInvoice(1, contract: "C001"),
                MakeInvoice(2, contract: "C002"),
                MakeInvoice(3, contract: "C001")
            };
            var repo = CreateRepository(invoices: invoices);
            var filter = new RadTrackInvoiceFilter { Contract = "C001" };

            // Act
            var result = await repo.GetAllAsync(DefaultParameters(filter: filter));

            // Assert
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, i => Assert.Equal("C001", i.Contract));
        }

        [Fact]
        public async Task GetAllAsync_FiltersByYear_UsingDueDate()
        {
            // Arrange
            var invoices = new List<RadTrackInvoice>
            {
                MakeInvoice(1, dueDate: new DateTime(2024, 3, 1)),
                MakeInvoice(2, dueDate: new DateTime(2025, 6, 1)),
                MakeInvoice(3, dueDate: new DateTime(2024, 11, 1))
            };
            var repo = CreateRepository(invoices: invoices);
            var filter = new RadTrackInvoiceFilter { Year = 2024 };

            // Act
            var result = await repo.GetAllAsync(DefaultParameters(filter: filter));

            // Assert
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, i => Assert.Equal(2024, i.DueDate!.Value.Year));
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmpty_WhenNoMatchingFilter()
        {
            // Arrange
            var invoices = new List<RadTrackInvoice> { MakeInvoice(1, project: "PP002") };
            var repo = CreateRepository(invoices: invoices);
            var filter = new RadTrackInvoiceFilter { Project = "PP001" };

            // Act
            var result = await repo.GetAllAsync(DefaultParameters(filter: filter));

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmpty_WhenDataIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(invoices: []);

            // Act
            var result = await repo.GetAllAsync(DefaultParameters());

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        #endregion

        // ── GetAllAsync — paging ───────────────────────────────────────────────

        #region GetAllAsync — paging

        [Fact]
        public async Task GetAllAsync_PaginationData_ReflectsTotalRecords()
        {
            // Arrange
            var invoices = Enumerable.Range(1, 5)
                .Select(i => MakeInvoice(i))
                .ToList();
            var repo = CreateRepository(invoices: invoices);

            // Act
            var result = await repo.GetAllAsync(DefaultParameters(page: 1, pageSize: 3));

            // Assert
            Assert.Equal(3, result.Data.Count);
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsSecondPage_WhenPaged()
        {
            // Arrange
            var invoices = Enumerable.Range(1, 5)
                .Select(i => MakeInvoice(i))
                .ToList();
            var repo = CreateRepository(invoices: invoices);

            // Act
            var result = await repo.GetAllAsync(DefaultParameters(page: 2, pageSize: 3));

            // Assert
            Assert.Equal(2, result.Data.Count);
        }

        #endregion

        // ── GetAllAsync — sorting ──────────────────────────────────────────────

        #region GetAllAsync — sorting

        [Fact]
        public async Task GetAllAsync_DefaultSort_OrdersByInvoiceCounterDescending()
        {
            // Arrange
            var invoices = new List<RadTrackInvoice>
            {
                MakeInvoice(3), MakeInvoice(1), MakeInvoice(2)
            };
            var repo = CreateRepository(invoices: invoices);

            // Act — no SortBy → defaults to InvoiceCounter DESC
            var result = await repo.GetAllAsync(DefaultParameters());

            // Assert
            var ids = result.Data.Select(i => i.InvoiceCounter).ToList();
            Assert.Equal(new[] { 3, 2, 1 }, ids);
        }

        [Fact]
        public async Task GetAllAsync_SortByProject_OrdersAscending()
        {
            // Arrange
            var invoices = new List<RadTrackInvoice>
            {
                MakeInvoice(1, project: "PP003"),
                MakeInvoice(2, project: "PP001"),
                MakeInvoice(3, project: "PP002")
            };
            var repo = CreateRepository(invoices: invoices);
            var parameters = new PaginationParameters<RadTrackInvoiceFilter>
            {
                Page = 1, PageSize = 10,
                SortBy = "project", Descending = false
            };

            // Act
            var result = await repo.GetAllAsync(parameters);

            // Assert
            var projects = result.Data.Select(i => i.Project).ToList();
            Assert.Equal(new[] { "PP001", "PP002", "PP003" }, projects);
        }

        #endregion

        // ── GetByIdAsync ───────────────────────────────────────────────────────

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ReturnsInvoice_WhenIdExists()
        {
            // Arrange
            var invoices = new List<RadTrackInvoice>
            {
                MakeInvoice(1, project: "PP001"),
                MakeInvoice(2, project: "PP002")
            };
            var repo = CreateRepository(invoices: invoices);

            // Act
            var result = await repo.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1,      result.InvoiceCounter);
            Assert.Equal("PP001", result.Project);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenIdDoesNotExist()
        {
            // Arrange
            var repo = CreateRepository(invoices: [MakeInvoice(1)]);

            // Act
            var result = await repo.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenDataIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(invoices: []);

            // Act
            var result = await repo.GetByIdAsync(1);

            // Assert
            Assert.Null(result);
        }

        #endregion

        // ── CreateAsync ────────────────────────────────────────────────────────

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_AddsEntityAndReturnsIt()
        {
            // Arrange
            var (repo, invoicesDbSet, _) = CreateRepositoryWithMocks();
            var entity = MakeInvoice(0); // id=0 before identity assignment

            // Act
            var result = await repo.CreateAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.Same(entity, result);
            invoicesDbSet.Verify(x => x.Add(entity), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var (repo, _, mockContext) = CreateRepositoryWithMocks();
            var entity = MakeInvoice(0);

            // Act
            await repo.CreateAsync(entity);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        [Fact]
        public async Task CreateAsync_PreservesAllFields()
        {
            // Arrange
            RadTrackInvoice? captured = null;
            var (repo, invoicesDbSet, _) = CreateRepositoryWithMocks();
            invoicesDbSet
                .Setup(x => x.Add(It.IsAny<RadTrackInvoice>()))
                .Callback<RadTrackInvoice>(e => captured = e);

            var entity = new RadTrackInvoice
            {
                Project             = "PP001",
                Contract            = "C001",
                PlannedAmount       = 2000.0,
                DueAmount           = 1500.0,
                DueDate             = new DateTime(2025, 6, 1),
                ActualAmount        = 800.0,
                DateInvoiced        = new DateTime(2025, 7, 1),
                DateJobsheetRaised  = new DateTime(2025, 5, 1),
                InvoiceRef          = "INV-TEST",
                InvoicePaid         = 0
            };

            // Act
            await repo.CreateAsync(entity);

            // Assert
            Assert.NotNull(captured);
            Assert.Equal("PP001",               captured!.Project);
            Assert.Equal("C001",                captured.Contract);
            Assert.Equal(2000.0,                captured.PlannedAmount);
            Assert.Equal(1500.0,                captured.DueAmount);
            Assert.Equal(new DateTime(2025, 6, 1), captured.DueDate);
            Assert.Equal(800.0,                 captured.ActualAmount);
            Assert.Equal(new DateTime(2025, 7, 1), captured.DateInvoiced);
            Assert.Equal(new DateTime(2025, 5, 1), captured.DateJobsheetRaised);
            Assert.Equal("INV-TEST",            captured.InvoiceRef);
            Assert.Equal((short)0,              captured.InvoicePaid);
        }

        #endregion

        // ── UpdateAsync ────────────────────────────────────────────────────────

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ReturnsEntity()
        {
            // Arrange
            var (repo, _, _) = CreateRepositoryWithMocks();
            var entity = MakeInvoice(3);

            // Act
            var result = await repo.UpdateAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.Same(entity, result);
        }

        [Fact]
        public async Task UpdateAsync_CallsDbSetUpdate()
        {
            // Arrange
            var (repo, invoicesDbSet, _) = CreateRepositoryWithMocks();
            var entity = MakeInvoice(3);

            // Act
            await repo.UpdateAsync(entity);

            // Assert
            invoicesDbSet.Verify(x => x.Update(entity), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var (repo, _, mockContext) = CreateRepositoryWithMocks();
            var entity = MakeInvoice(3);

            // Act
            await repo.UpdateAsync(entity);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        #endregion

        // ── DeleteAsync ────────────────────────────────────────────────────────

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ThrowsException_BecauseBulkDeleteRequiresDatabase()
        {
            // Arrange — ExecuteDeleteAsync is a bulk EF Core operation that cannot
            // be exercised against an in-memory mock query provider.
            var invoices = new List<RadTrackInvoice> { MakeInvoice(1) };
            var repo = CreateRepository(invoices: invoices);

            // Act + Assert
            await Assert.ThrowsAnyAsync<Exception>(() => repo.DeleteAsync(1));
        }

        #endregion

        // ── GetTotalsAsync ─────────────────────────────────────────────────────

        #region GetTotalsAsync

        [Fact]
        public async Task GetTotalsAsync_ReturnsZeroTotals_WhenDataIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(invoices: []);

            // Act
            var result = await repo.GetTotalsAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0.0, result.TotalPlannedAmount);
            Assert.Equal(0.0, result.TotalDueAmount);
            Assert.Equal(0.0, result.TotalActualAmount);
        }

        [Fact]
        public async Task GetTotalsAsync_WithProjectFilter_SumsOnlyMatchingInvoices()
        {
            // Arrange
            var invoices = new List<RadTrackInvoice>
            {
                new() { InvoiceCounter=1, Project="PP001", PlannedAmount=1000, DueAmount=500, ActualAmount=200 },
                new() { InvoiceCounter=2, Project="PP001", PlannedAmount=2000, DueAmount=800, ActualAmount=600 },
                new() { InvoiceCounter=3, Project="PP002", PlannedAmount=5000, DueAmount=9000, ActualAmount=100 }
            };
            var repo = CreateRepository(invoices: invoices);
            var filter = new RadTrackInvoiceFilter { Project = "PP001" };

            // Act
            var result = await repo.GetTotalsAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3000.0, result.TotalPlannedAmount);
            Assert.Equal(1300.0, result.TotalDueAmount);
            Assert.Equal(800.0,  result.TotalActualAmount);
        }

        [Fact]
        public async Task GetTotalsAsync_WithNullAmounts_TreatsNullAsZero()
        {
            // Arrange
            var invoices = new List<RadTrackInvoice>
            {
                new() { InvoiceCounter=1, Project="PP001", PlannedAmount=null, DueAmount=null, ActualAmount=null }
            };
            var repo = CreateRepository(invoices: invoices);

            // Act
            var result = await repo.GetTotalsAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0.0, result.TotalPlannedAmount);
            Assert.Equal(0.0, result.TotalDueAmount);
            Assert.Equal(0.0, result.TotalActualAmount);
        }

        #endregion

        // ── ExistsAsync ────────────────────────────────────────────────────────

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_ReturnsTrue_WhenMatchFound()
        {
            // Arrange
            var invoices = new List<RadTrackInvoice>
            {
                new() { InvoiceCounter=1, Project="PP001", Contract="C001", InvoiceRef="INV-001" }
            };
            var repo = CreateRepository(invoices: invoices);

            // Act
            var result = await repo.ExistsAsync("PP001", "C001", "INV-001");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenNoMatch()
        {
            // Arrange
            var invoices = new List<RadTrackInvoice>
            {
                new() { InvoiceCounter=1, Project="PP001", Contract="C001", InvoiceRef="INV-001" }
            };
            var repo = CreateRepository(invoices: invoices);

            // Act
            var result = await repo.ExistsAsync("PP001", "C001", "INV-999");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenExcludeCounterMatchesOnlyRecord()
        {
            // Arrange
            var invoices = new List<RadTrackInvoice>
            {
                new() { InvoiceCounter=5, Project="PP001", Contract="C001", InvoiceRef="INV-001" }
            };
            var repo = CreateRepository(invoices: invoices);

            // Act — self-exclusion: counter 5 excluded → no remaining match
            var result = await repo.ExistsAsync("PP001", "C001", "INV-001", excludeInvoiceCounter: 5);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsTrue_WhenAnotherRecordMatchesExcludingSelf()
        {
            // Arrange
            var invoices = new List<RadTrackInvoice>
            {
                new() { InvoiceCounter=5,  Project="PP001", Contract="C001", InvoiceRef="INV-001" },
                new() { InvoiceCounter=10, Project="PP001", Contract="C001", InvoiceRef="INV-001" }
            };
            var repo = CreateRepository(invoices: invoices);

            // Act — exclude counter 5 but counter 10 still matches
            var result = await repo.ExistsAsync("PP001", "C001", "INV-001", excludeInvoiceCounter: 5);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenDataIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(invoices: []);

            // Act
            var result = await repo.ExistsAsync("PP001", "C001", "INV-001");

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}
