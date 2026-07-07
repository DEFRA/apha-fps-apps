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
        /// <summary>
        /// Creates a <see cref="RadTrackInvoiceRepository"/> backed entirely by in-memory data.
        /// All parameters are optional — omitted sets are initialised as empty.
        /// </summary>
        private static RadTrackInvoiceRepository CreateRepository(
            IEnumerable<RadTrackInvoice>?    invoices         = null,
            IEnumerable<RadTrackContract>?   contracts        = null,
            IEnumerable<Year>?               years            = null,
            IEnumerable<ProjectRadTrackData>? radTrackData    = null,
            IEnumerable<ProjectLatestDetail>? projectDetails  = null,
            IEnumerable<Projects>?           myTlkpProjects   = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();

            var invoicesMockSet        = RepositoryTestHelper.CreateMockDbSet(invoices         ?? Enumerable.Empty<RadTrackInvoice>());
            var contractsMockSet       = RepositoryTestHelper.CreateMockDbSet(contracts        ?? Enumerable.Empty<RadTrackContract>());
            var yearsMockSet           = RepositoryTestHelper.CreateMockDbSet(years            ?? Enumerable.Empty<Year>());
            var radTrackDataMockSet    = RepositoryTestHelper.CreateMockDbSet(radTrackData     ?? Enumerable.Empty<ProjectRadTrackData>());
            var projectDetailsMockSet  = RepositoryTestHelper.CreateMockDbSet(projectDetails   ?? Enumerable.Empty<ProjectLatestDetail>());
            var myTlkpProjectsMockSet  = RepositoryTestHelper.CreateMockDbSet(myTlkpProjects   ?? Enumerable.Empty<Projects>());

            RepositoryTestHelper.SetupDbSetOperations(invoicesMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.RadTrackInvoices).Returns(invoicesMockSet.Object);
            mockContext.Setup(x => x.RadTrackContracts).Returns(contractsMockSet.Object);
            mockContext.Setup(x => x.Years).Returns(yearsMockSet.Object);
            mockContext.Setup(x => x.ProjectRadTrackData).Returns(radTrackDataMockSet.Object);
            mockContext.Setup(x => x.ProjectLatestDetails).Returns(projectDetailsMockSet.Object);
            mockContext.Setup(x => x.MyTlkpProjects).Returns(myTlkpProjectsMockSet.Object);

            return new RadTrackInvoiceRepository(mockContext.Object);
        }

        /// <summary>
        /// Returns the repository alongside its mocked <see cref="DbSet{RadTrackInvoice}"/>
        /// and <see cref="PimsDbContext"/> for tests that need to verify
        /// Add / Update / SaveChanges calls.
        /// </summary>
        private static (
            RadTrackInvoiceRepository       Repo,
            Mock<DbSet<RadTrackInvoice>>    InvoicesDbSet,
            Mock<PimsDbContext>             Context)
            CreateRepositoryWithMocks(
                IEnumerable<RadTrackInvoice>?    invoices        = null,
                IEnumerable<RadTrackContract>?   contracts       = null,
                IEnumerable<Year>?               years           = null,
                IEnumerable<ProjectRadTrackData>? radTrackData   = null,
                IEnumerable<ProjectLatestDetail>? projectDetails = null,
                IEnumerable<Projects>?           myTlkpProjects  = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();

            var invoicesMockSet        = RepositoryTestHelper.CreateMockDbSet(invoices         ?? Enumerable.Empty<RadTrackInvoice>());
            var contractsMockSet       = RepositoryTestHelper.CreateMockDbSet(contracts        ?? Enumerable.Empty<RadTrackContract>());
            var yearsMockSet           = RepositoryTestHelper.CreateMockDbSet(years            ?? Enumerable.Empty<Year>());
            var radTrackDataMockSet    = RepositoryTestHelper.CreateMockDbSet(radTrackData     ?? Enumerable.Empty<ProjectRadTrackData>());
            var projectDetailsMockSet  = RepositoryTestHelper.CreateMockDbSet(projectDetails   ?? Enumerable.Empty<ProjectLatestDetail>());
            var myTlkpProjectsMockSet  = RepositoryTestHelper.CreateMockDbSet(myTlkpProjects   ?? Enumerable.Empty<Projects>());

            RepositoryTestHelper.SetupDbSetOperations(invoicesMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.RadTrackInvoices).Returns(invoicesMockSet.Object);
            mockContext.Setup(x => x.RadTrackContracts).Returns(contractsMockSet.Object);
            mockContext.Setup(x => x.Years).Returns(yearsMockSet.Object);
            mockContext.Setup(x => x.ProjectRadTrackData).Returns(radTrackDataMockSet.Object);
            mockContext.Setup(x => x.ProjectLatestDetails).Returns(projectDetailsMockSet.Object);
            mockContext.Setup(x => x.MyTlkpProjects).Returns(myTlkpProjectsMockSet.Object);

            var repo = new RadTrackInvoiceRepository(mockContext.Object);
            return (repo, invoicesMockSet, mockContext);
        }

        private static PaginationParameters<RadTrackInvoiceFilter> DefaultParameters(int page = 1, int pageSize = 10)
            => new PaginationParameters<RadTrackInvoiceFilter> { Page = page, PageSize = pageSize };

        // ── sample data factory ───────────────────────────────────────────────────────

        private static List<RadTrackInvoice> ThreeInvoices() =>
        [
            new() { InvoiceCounter = 1, Project = "PP001", Contract = "C001", InvoiceRef = "INV-001", DueDate = new DateTime(2024, 3, 1), DueAmount = 1000, PlannedAmount = 1200, ActualAmount = 900 },
            new() { InvoiceCounter = 2, Project = "PP001", Contract = "C002", InvoiceRef = "INV-002", DueDate = new DateTime(2024, 6, 1), DueAmount = 2000, PlannedAmount = 2200, ActualAmount = 1800 },
            new() { InvoiceCounter = 3, Project = "PP002", Contract = "C001", InvoiceRef = "INV-003", DueDate = new DateTime(2023, 9, 1), DueAmount = 3000, PlannedAmount = 3200, ActualAmount = 2700 }
        ];

        #region GetAllAsync — filtering

        [Fact]
        public async Task GetAllAsync_WithNoFilter_ReturnsAllInvoices()
        {
            // Arrange
            var repo = CreateRepository(invoices: ThreeInvoices());

            // Act
            var result = await repo.GetAllAsync(DefaultParameters());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAllAsync_WithEmptyData_ReturnsEmptyResult()
        {
            // Arrange
            var repo = CreateRepository(invoices: []);

            // Act
            var result = await repo.GetAllAsync(DefaultParameters());

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAllAsync_WithProjectFilter_ReturnsOnlyMatchingInvoices()
        {
            // Arrange
            var parameters = new PaginationParameters<RadTrackInvoiceFilter>
            {
                Page = 1, PageSize = 10,
                Filter = new RadTrackInvoiceFilter { Project = "PP001" }
            };
            var repo = CreateRepository(invoices: ThreeInvoices());

            // Act
            var result = await repo.GetAllAsync(parameters);

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, i => Assert.Equal("PP001", i.Project));
        }

        [Fact]
        public async Task GetAllAsync_WithContractFilter_ReturnsOnlyMatchingInvoices()
        {
            // Arrange
            var parameters = new PaginationParameters<RadTrackInvoiceFilter>
            {
                Page = 1, PageSize = 10,
                Filter = new RadTrackInvoiceFilter { Contract = "C001" }
            };
            var repo = CreateRepository(invoices: ThreeInvoices());

            // Act
            var result = await repo.GetAllAsync(parameters);

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, i => Assert.Equal("C001", i.Contract));
        }

        [Fact]
        public async Task GetAllAsync_WithYearFilter_ReturnsInvoicesMatchingDueDateYear()
        {
            // Arrange
            var parameters = new PaginationParameters<RadTrackInvoiceFilter>
            {
                Page = 1, PageSize = 10,
                Filter = new RadTrackInvoiceFilter { Year = 2024 }
            };
            var repo = CreateRepository(invoices: ThreeInvoices());

            // Act
            var result = await repo.GetAllAsync(parameters);

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, i => Assert.Equal(2024, i.DueDate!.Value.Year));
        }

        [Fact]
        public async Task GetAllAsync_WithZeroYear_DoesNotFilterByYear()
        {
            // Arrange
            var parameters = new PaginationParameters<RadTrackInvoiceFilter>
            {
                Page = 1, PageSize = 10,
                Filter = new RadTrackInvoiceFilter { Year = 0 }
            };
            var repo = CreateRepository(invoices: ThreeInvoices());

            // Act
            var result = await repo.GetAllAsync(parameters);

            // Assert — year = 0 is ignored, all three invoices returned
            Assert.Equal(3, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAllAsync_WithProjectAndContractFilter_ReturnsIntersection()
        {
            // Arrange
            var parameters = new PaginationParameters<RadTrackInvoiceFilter>
            {
                Page = 1, PageSize = 10,
                Filter = new RadTrackInvoiceFilter { Project = "PP001", Contract = "C001" }
            };
            var repo = CreateRepository(invoices: ThreeInvoices());

            // Act
            var result = await repo.GetAllAsync(parameters);

            // Assert
            Assert.Equal(1, result.PaginationData.TotalRecords);
            Assert.Equal("PP001", result.Data.First().Project);
            Assert.Equal("C001",  result.Data.First().Contract);
        }

        [Fact]
        public async Task GetAllAsync_WithFilterThatMatchesNothing_ReturnsEmpty()
        {
            // Arrange
            var parameters = new PaginationParameters<RadTrackInvoiceFilter>
            {
                Page = 1, PageSize = 10,
                Filter = new RadTrackInvoiceFilter { Project = "NONEXISTENT" }
            };
            var repo = CreateRepository(invoices: ThreeInvoices());

            // Act
            var result = await repo.GetAllAsync(parameters);

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        #endregion

        #region GetAllAsync — sorting

        [Fact]
        public async Task GetAllAsync_WithProjectSortAscending_ReturnsSortedByProject()
        {
            // Arrange
            var invoices = new List<RadTrackInvoice>
            {
                new() { InvoiceCounter = 1, Project = "PP003", DueDate = DateTime.Today, DueAmount = 100 },
                new() { InvoiceCounter = 2, Project = "PP001", DueDate = DateTime.Today, DueAmount = 200 },
                new() { InvoiceCounter = 3, Project = "PP002", DueDate = DateTime.Today, DueAmount = 300 }
            };
            var parameters = new PaginationParameters<RadTrackInvoiceFilter>
                { Page = 1, PageSize = 10, SortBy = "project", Descending = false };
            var repo = CreateRepository(invoices: invoices);

            // Act
            var result = await repo.GetAllAsync(parameters);

            // Assert
            var projects = result.Data.Select(i => i.Project).ToList();
            Assert.Equal(["PP001", "PP002", "PP003"], projects);
        }

        [Fact]
        public async Task GetAllAsync_WithProjectSortDescending_ReturnsSortedByProjectDescending()
        {
            // Arrange
            var invoices = new List<RadTrackInvoice>
            {
                new() { InvoiceCounter = 1, Project = "PP001", DueDate = DateTime.Today, DueAmount = 100 },
                new() { InvoiceCounter = 2, Project = "PP003", DueDate = DateTime.Today, DueAmount = 200 },
                new() { InvoiceCounter = 3, Project = "PP002", DueDate = DateTime.Today, DueAmount = 300 }
            };
            var parameters = new PaginationParameters<RadTrackInvoiceFilter>
                { Page = 1, PageSize = 10, SortBy = "project", Descending = true };
            var repo = CreateRepository(invoices: invoices);

            // Act
            var result = await repo.GetAllAsync(parameters);

            // Assert
            var projects = result.Data.Select(i => i.Project).ToList();
            Assert.Equal(["PP003", "PP002", "PP001"], projects);
        }

        [Fact]
        public async Task GetAllAsync_WithDueDateSortAscending_ReturnsSortedByDueDateAscending()
        {
            // Arrange
            var invoices = new List<RadTrackInvoice>
            {
                new() { InvoiceCounter = 1, Project = "PP001", DueDate = new DateTime(2024, 6, 1),  DueAmount = 100 },
                new() { InvoiceCounter = 2, Project = "PP001", DueDate = new DateTime(2024, 1, 1),  DueAmount = 200 },
                new() { InvoiceCounter = 3, Project = "PP001", DueDate = new DateTime(2024, 12, 1), DueAmount = 300 }
            };
            var parameters = new PaginationParameters<RadTrackInvoiceFilter>
                { Page = 1, PageSize = 10, SortBy = "duedate", Descending = false };
            var repo = CreateRepository(invoices: invoices);

            // Act
            var result = await repo.GetAllAsync(parameters);

            // Assert
            var dates = result.Data.Select(i => i.DueDate).ToList();
            Assert.Equal(new DateTime(2024, 1,  1), dates[0]);
            Assert.Equal(new DateTime(2024, 6,  1), dates[1]);
            Assert.Equal(new DateTime(2024, 12, 1), dates[2]);
        }

        [Fact]
        public async Task GetAllAsync_WithUnknownSortBy_DefaultsToInvoiceCounterDescending()
        {
            // Arrange
            var invoices = new List<RadTrackInvoice>
            {
                new() { InvoiceCounter = 1, Project = "PP001", DueDate = DateTime.Today, DueAmount = 100 },
                new() { InvoiceCounter = 3, Project = "PP001", DueDate = DateTime.Today, DueAmount = 200 },
                new() { InvoiceCounter = 2, Project = "PP001", DueDate = DateTime.Today, DueAmount = 300 }
            };
            var parameters = new PaginationParameters<RadTrackInvoiceFilter>
                { Page = 1, PageSize = 10, SortBy = "unknowncolumn", Descending = false };
            var repo = CreateRepository(invoices: invoices);

            // Act
            var result = await repo.GetAllAsync(parameters);

            // Assert — default sort is InvoiceCounter descending
            var counters = result.Data.Select(i => i.InvoiceCounter).ToList();
            Assert.Equal([3, 2, 1], counters);
        }

        #endregion

        #region GetAllAsync — pagination

        [Fact]
        public async Task GetAllAsync_PaginationData_ReflectsTotalRecords()
        {
            // Arrange
            var invoices = Enumerable.Range(1, 5)
                .Select(i => new RadTrackInvoice { InvoiceCounter = i, Project = "PP001", DueDate = DateTime.Today, DueAmount = 100 })
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
                .Select(i => new RadTrackInvoice { InvoiceCounter = i, Project = "PP001", DueDate = DateTime.Today, DueAmount = 100 })
                .ToList();
            var repo = CreateRepository(invoices: invoices);

            // Act
            var result = await repo.GetAllAsync(DefaultParameters(page: 2, pageSize: 3));

            // Assert
            Assert.Equal(2, result.Data.Count);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ReturnsInvoice_WhenExists()
        {
            // Arrange
            var invoices = new List<RadTrackInvoice>
            {
                new() { InvoiceCounter = 1, Project = "PP001", InvoiceRef = "INV-001" },
                new() { InvoiceCounter = 2, Project = "PP002", InvoiceRef = "INV-002" }
            };
            var repo = CreateRepository(invoices: invoices);

            // Act
            var result = await repo.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1,         result.InvoiceCounter);
            Assert.Equal("PP001",   result.Project);
            Assert.Equal("INV-001", result.InvoiceRef);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenInvoiceDoesNotExist()
        {
            // Arrange
            var invoices = new List<RadTrackInvoice>
            {
                new() { InvoiceCounter = 1, Project = "PP001" }
            };
            var repo = CreateRepository(invoices: invoices);

            // Act
            var result = await repo.GetByIdAsync(99);

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

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetByIdAsync_ReturnsCorrectInvoice_ForEachId(int id)
        {
            // Arrange
            var repo = CreateRepository(invoices: ThreeInvoices());

            // Act
            var result = await repo.GetByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.InvoiceCounter);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_AddsEntityAndReturnsIt()
        {
            // Arrange
            var (repo, _, _) = CreateRepositoryWithMocks();
            var entity = new RadTrackInvoice
            {
                Project    = "PP001",
                Contract   = "C001",
                InvoiceRef = "INV-NEW",
                DueAmount  = 5000,
                DueDate    = DateTime.Today.AddDays(30)
            };

            // Act
            var result = await repo.CreateAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.Same(entity, result);
            Assert.Equal("PP001",   result.Project);
            Assert.Equal("INV-NEW", result.InvoiceRef);
        }

        [Fact]
        public async Task CreateAsync_ResetsInvoiceCounterToZero()
        {
            // Arrange
            var (repo, _, _) = CreateRepositoryWithMocks();
            var entity = new RadTrackInvoice { InvoiceCounter = 42, Project = "PP001", DueAmount = 100, DueDate = DateTime.Today };

            // Act
            var result = await repo.CreateAsync(entity);

            // Assert — repository sets InvoiceCounter = 0 before saving
            Assert.Equal(0, result.InvoiceCounter);
        }

        [Fact]
        public async Task CreateAsync_CallsDbSetAdd()
        {
            // Arrange
            var (repo, invoicesDbSet, _) = CreateRepositoryWithMocks();
            var entity = new RadTrackInvoice { Project = "PP001", DueAmount = 100, DueDate = DateTime.Today };

            // Act
            await repo.CreateAsync(entity);

            // Assert
            invoicesDbSet.Verify(x => x.Add(entity), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var (repo, _, mockContext) = CreateRepositoryWithMocks();
            var entity = new RadTrackInvoice { Project = "PP001", DueAmount = 100, DueDate = DateTime.Today };

            // Act
            await repo.CreateAsync(entity);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        [Fact]
        public async Task CreateAsync_DoesNotCallSaveChangesMoreThanOnce()
        {
            // Arrange
            var (repo, _, mockContext) = CreateRepositoryWithMocks();
            var entity = new RadTrackInvoice { Project = "PP001", DueAmount = 100, DueDate = DateTime.Today };

            // Act
            await repo.CreateAsync(entity);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ReturnsUpdatedEntity()
        {
            // Arrange
            var (repo, _, _) = CreateRepositoryWithMocks();
            var entity = new RadTrackInvoice { InvoiceCounter = 1, Project = "PP001", DueAmount = 9999, DueDate = DateTime.Today };

            // Act
            var result = await repo.UpdateAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.Same(entity, result);
            Assert.Equal(1,      result.InvoiceCounter);
            Assert.Equal(9999,   result.DueAmount);
            Assert.Equal("PP001", result.Project);
        }

        [Fact]
        public async Task UpdateAsync_CallsDbSetUpdate()
        {
            // Arrange
            var (repo, invoicesDbSet, _) = CreateRepositoryWithMocks();
            var entity = new RadTrackInvoice { InvoiceCounter = 1, Project = "PP001", DueAmount = 100, DueDate = DateTime.Today };

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
            var entity = new RadTrackInvoice { InvoiceCounter = 1, Project = "PP001", DueAmount = 100, DueDate = DateTime.Today };

            // Act
            await repo.UpdateAsync(entity);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        [Fact]
        public async Task UpdateAsync_DoesNotCallSaveChangesMoreThanOnce()
        {
            // Arrange
            var (repo, _, mockContext) = CreateRepositoryWithMocks();
            var entity = new RadTrackInvoice { InvoiceCounter = 1, Project = "PP001", DueAmount = 100, DueDate = DateTime.Today };

            // Act
            await repo.UpdateAsync(entity);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        #endregion

        #region GetTotalsAsync Tests

        [Fact]
        public async Task GetTotalsAsync_WithNullFilter_ReturnsTotalsOfAllInvoices()
        {
            // Arrange
            var repo = CreateRepository(invoices: ThreeInvoices());

            // Act
            var result = await repo.GetTotalsAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1000 + 2000 + 3000, result.TotalDueAmount);
            Assert.Equal(1200 + 2200 + 3200, result.TotalPlannedAmount);
            Assert.Equal(900  + 1800 + 2700, result.TotalActualAmount);
        }

        [Fact]
        public async Task GetTotalsAsync_WithProjectFilter_ReturnsTotalsForMatchingProject()
        {
            // Arrange
            var filter = new RadTrackInvoiceFilter { Project = "PP001" };
            var repo   = CreateRepository(invoices: ThreeInvoices());

            // Act
            var result = await repo.GetTotalsAsync(filter);

            // Assert
            Assert.Equal(1000 + 2000, result.TotalDueAmount);
            Assert.Equal(1200 + 2200, result.TotalPlannedAmount);
            Assert.Equal(900  + 1800, result.TotalActualAmount);
        }

        [Fact]
        public async Task GetTotalsAsync_WithContractFilter_ReturnsTotalsForMatchingContract()
        {
            // Arrange
            var filter = new RadTrackInvoiceFilter { Contract = "C001" };
            var repo   = CreateRepository(invoices: ThreeInvoices());

            // Act
            var result = await repo.GetTotalsAsync(filter);

            // Assert
            Assert.Equal(1000 + 3000, result.TotalDueAmount);
        }

        [Fact]
        public async Task GetTotalsAsync_WithYearFilter_ReturnsTotalsForMatchingYear()
        {
            // Arrange
            var filter = new RadTrackInvoiceFilter { Year = 2024 };
            var repo   = CreateRepository(invoices: ThreeInvoices());

            // Act
            var result = await repo.GetTotalsAsync(filter);

            // Assert — INV-001 (2024) + INV-002 (2024) = 3000 due
            Assert.Equal(1000 + 2000, result.TotalDueAmount);
        }

        [Fact]
        public async Task GetTotalsAsync_WithEmptyData_ReturnsZeroTotals()
        {
            // Arrange
            var repo = CreateRepository(invoices: []);

            // Act
            var result = await repo.GetTotalsAsync(null);

            // Assert
            Assert.Equal(0, result.TotalPlannedAmount);
            Assert.Equal(0, result.TotalDueAmount);
            Assert.Equal(0, result.TotalActualAmount);
        }

        [Fact]
        public async Task GetTotalsAsync_WithFilterMatchingNothing_ReturnsZeroTotals()
        {
            // Arrange
            var filter = new RadTrackInvoiceFilter { Project = "NONEXISTENT" };
            var repo   = CreateRepository(invoices: ThreeInvoices());

            // Act
            var result = await repo.GetTotalsAsync(filter);

            // Assert
            Assert.Equal(0, result.TotalPlannedAmount);
            Assert.Equal(0, result.TotalDueAmount);
            Assert.Equal(0, result.TotalActualAmount);
        }

        #endregion

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_ReturnsTrue_WhenMatchingInvoiceExists()
        {
            // Arrange
            var repo = CreateRepository(invoices: ThreeInvoices());

            // Act
            var result = await repo.ExistsAsync("PP001", "C001", "INV-001");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenNoMatchingInvoice()
        {
            // Arrange
            var repo = CreateRepository(invoices: ThreeInvoices());

            // Act
            var result = await repo.ExistsAsync("PP001", "C001", "INV-NONE");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WithEmptyData()
        {
            // Arrange
            var repo = CreateRepository(invoices: []);

            // Act
            var result = await repo.ExistsAsync("PP001", "C001", "INV-001");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_WithExcludeCounter_ExcludesMatchingInvoice()
        {
            // Arrange
            var repo = CreateRepository(invoices: ThreeInvoices());

            // Act — invoice 1 has Project=PP001, Contract=C001, InvoiceRef=INV-001
            var result = await repo.ExistsAsync("PP001", "C001", "INV-001", excludeInvoiceCounter: 1);

            // Assert — the only match is excluded
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_WithExcludeCounter_StillFindsOtherMatch()
        {
            // Arrange — add a second invoice with same Project/Contract/Ref
            var invoices = new List<RadTrackInvoice>
            {
                new() { InvoiceCounter = 1, Project = "PP001", Contract = "C001", InvoiceRef = "INV-DUP" },
                new() { InvoiceCounter = 2, Project = "PP001", Contract = "C001", InvoiceRef = "INV-DUP" }
            };
            var repo = CreateRepository(invoices: invoices);

            // Act — exclude counter 1 but counter 2 still matches
            var result = await repo.ExistsAsync("PP001", "C001", "INV-DUP", excludeInvoiceCounter: 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNullProject_DoesNotFilterByProject()
        {
            // Arrange
            var repo = CreateRepository(invoices: ThreeInvoices());

            // Act
            var result = await repo.ExistsAsync(null, "C001", "INV-001");

            // Assert — matches INV-001 regardless of project
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNullContract_DoesNotFilterByContract()
        {
            // Arrange
            var repo = CreateRepository(invoices: ThreeInvoices());

            // Act
            var result = await repo.ExistsAsync("PP001", null, "INV-001");

            // Assert — matches INV-001 regardless of contract
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNullInvoiceRef_DoesNotFilterByInvoiceRef()
        {
            // Arrange
            var repo = CreateRepository(invoices: ThreeInvoices());

            // Act
            var result = await repo.ExistsAsync("PP001", "C001", null);

            // Assert — matches any invoice for PP001 + C001
            Assert.True(result);
        }

        #endregion

        #region GetContractsAsync Tests

        [Fact]
        public async Task GetContractsAsync_ReturnsContractsOrderedAscending()
        {
            // Arrange
            var contracts = new List<RadTrackContract>
            {
                new() { Contract = "C003" },
                new() { Contract = "C001" },
                new() { Contract = "C002" }
            };
            var repo = CreateRepository(contracts: contracts);

            // Act
            var result = await repo.GetContractsAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(["C001", "C002", "C003"], result);
        }

        [Fact]
        public async Task GetContractsAsync_ReturnsEmptyList_WhenNoContractsExist()
        {
            // Arrange
            var repo = CreateRepository(contracts: []);

            // Act
            var result = await repo.GetContractsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetContractsAsync_ReturnsSingleItem_WhenOneContractExists()
        {
            // Arrange
            var contracts = new List<RadTrackContract> { new() { Contract = "C001" } };
            var repo = CreateRepository(contracts: contracts);

            // Act
            var result = await repo.GetContractsAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("C001", result[0]);
        }

        #endregion

        #region GetYearsAsync Tests

        [Fact(Skip = "TestAsyncEnumerable<T> has 'where T : class' constraint; Select(y => y.Value) projects to int (value type) which the mock query provider cannot handle")]
        public async Task GetYearsAsync_ReturnsYearsWithThreeFutureYearsAppended()
        {
            // Arrange
            var years = new List<Year>
            {
                new() { Value = 2022 },
                new() { Value = 2023 },
                new() { Value = 2024 }
            };
            var repo = CreateRepository(years: years);

            // Act
            var result = await repo.GetYearsAsync();

            // Assert — max is 2024, so 2025 2026 2027 are appended
            Assert.Contains(2025, result);
            Assert.Contains(2026, result);
            Assert.Contains(2027, result);
        }

        [Fact(Skip = "TestAsyncEnumerable<T> has 'where T : class' constraint; Select(y => y.Value) projects to int (value type) which the mock query provider cannot handle")]
        public async Task GetYearsAsync_ReturnsAllYearsOrderedAscending()
        {
            // Arrange
            var years = new List<Year>
            {
                new() { Value = 2023 },
                new() { Value = 2021 },
                new() { Value = 2022 }
            };
            var repo = CreateRepository(years: years);

            // Act
            var result = await repo.GetYearsAsync();

            // Assert — result is sorted ascending
            for (int i = 1; i < result.Count; i++)
                Assert.True(result[i] >= result[i - 1]);
        }

        [Fact(Skip = "TestAsyncEnumerable<T> has 'where T : class' constraint; Select(y => y.Value) projects to int (value type) which the mock query provider cannot handle")]
        public async Task GetYearsAsync_ReturnsEmptyList_WhenNoYearsExist()
        {
            // Arrange
            var repo = CreateRepository(years: []);

            // Act
            var result = await repo.GetYearsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact(Skip = "TestAsyncEnumerable<T> has 'where T : class' constraint; Select(y => y.Value) projects to int (value type) which the mock query provider cannot handle")]
        public async Task GetYearsAsync_ReturnsDistinctYears()
        {
            // Arrange
            var years = new List<Year>
            {
                new() { Value = 2022 },
                new() { Value = 2022 },
                new() { Value = 2023 }
            };
            var repo = CreateRepository(years: years);

            // Act
            var result = await repo.GetYearsAsync();

            // Assert — no duplicates
            Assert.Equal(result.Distinct().Count(), result.Count);
        }

        #endregion
    }
}
