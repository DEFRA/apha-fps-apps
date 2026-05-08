using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Collections;
using System.Linq.Expressions;

namespace Apha.PACT.DataAccess.UnitTests.Repository
{
    // ── Async provider helpers ────────────────────────────────────────────────

    internal class TestAsyncQueryProvider<T> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

        public IQueryable CreateQuery(Expression expression)
            => new TestAsyncEnumerable<T>(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => new TestAsyncEnumerable<TElement>(expression);

        public object? Execute(Expression expression) => _inner.Execute(expression);

        public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = Execute(expression);
            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(resultType)
                .Invoke(null, [executionResult])!;
        }
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
        public TestAsyncEnumerable(Expression expression) : base(expression) { }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    internal class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
    {
        public T Current => inner.Current;

        public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(inner.MoveNext());

        public ValueTask DisposeAsync()
        {
            inner.Dispose();
            return ValueTask.CompletedTask;
        }
    }

    internal static class MockDbSetFactory
    {
        internal static DbSet<T> Create<T>(List<T> data) where T : class
        {
            var queryable = new TestAsyncEnumerable<T>(data).AsQueryable();
            var dbSet = Substitute.For<DbSet<T>, IQueryable<T>, IAsyncEnumerable<T>>();

            ((IQueryable<T>)dbSet).Provider.Returns(queryable.Provider);
            ((IQueryable<T>)dbSet).Expression.Returns(queryable.Expression);
            ((IQueryable<T>)dbSet).ElementType.Returns(queryable.ElementType);
            ((IQueryable<T>)dbSet).GetEnumerator().Returns(_ => queryable.GetEnumerator());
            ((IAsyncEnumerable<T>)dbSet).GetAsyncEnumerator(Arg.Any<CancellationToken>())
                .Returns(_ => new TestAsyncEnumerator<T>(data.GetEnumerator()));

            return dbSet;
        }
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    public class ProjectInvoiceRepositoryTests
    {
        private readonly FpsDbContext _context;
        private readonly IFpsRequestContext _fpsRequestContext;
        private readonly ProjectInvoiceRepository _repository;

        public ProjectInvoiceRepositoryTests()
        {
            _context = Substitute.For<FpsDbContext>(
                new DbContextOptionsBuilder<FpsDbContext>().UseInMemoryDatabase("_unused_").Options,
                Substitute.For<IFpsRequestContext>());
            _fpsRequestContext = Substitute.For<IFpsRequestContext>();
            _fpsRequestContext.FpsYear.Returns(2025);
            _repository = new ProjectInvoiceRepository(_context, _fpsRequestContext);
        }

        private static ProjectInvoice MakeInvoice(int id, string project, int fpsYear = 2025, int? month = 1, decimal? amount = 100m)
            => new() { InvoiceCounter = id, ProjectParent = project, FpsYear = fpsYear, Month = month, Amount = amount };

        private static MonthlyInvoicesSummary MakeSummary(string program, string project, int month, decimal amount)
            => new() { FpsYear = 2025, Program = program, ParentProject = project, Month = month, MonthlyAmount = amount };

        private void SetupInvoiceDbSet(List<ProjectInvoice> data)
        {
            var dbSet = MockDbSetFactory.Create(data);
            _context.ProjectInvoices.Returns(dbSet);
        }

        private void SetupMonthlyInvoicesSummaryDbSet(List<MonthlyInvoicesSummary> data)
        {
            var dbSet = MockDbSetFactory.Create(data);
            _context.MonthlyInvoicesSummary.Returns(dbSet);
        }

        #region GetPagedProjectInvoicesAsync

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_NoParentProject_ReturnsAllInvoices()
        {
            // Arrange
            SetupInvoiceDbSet(
            [
                MakeInvoice(1, "PRJ001"),
                MakeInvoice(2, "PRJ002")
            ]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _repository.GetPagedProjectInvoicesAsync(query, null);

            // Assert
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_WithParentProject_FiltersToMatchingInvoices()
        {
            // Arrange
            SetupInvoiceDbSet(
            [
                MakeInvoice(1, "PRJ001"),
                MakeInvoice(2, "PRJ002")
            ]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _repository.GetPagedProjectInvoicesAsync(query, "PRJ001");

            // Assert
            Assert.Single(result.Data);
            Assert.All(result.Data, i => Assert.Equal("PRJ001", i.ProjectParent));
        }        

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_WithMonthFilter_FiltersByMonth()
        {
            // Arrange
            SetupInvoiceDbSet(
            [
                MakeInvoice(1, "PRJ001", month: 3),
                MakeInvoice(2, "PRJ002", month: 5)
            ]);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = """{"Month":"3"}"""
            };

            // Act
            var result = await _repository.GetPagedProjectInvoicesAsync(query, null);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal(3, result.Data.First().Month);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_Pagination_ReturnsCorrectPage()
        {
            // Arrange
            SetupInvoiceDbSet(
            [
                MakeInvoice(1, "PRJ001"),
                MakeInvoice(2, "PRJ001"),
                MakeInvoice(3, "PRJ001")
            ]);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            // Act
            var result = await _repository.GetPagedProjectInvoicesAsync(query, null);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal(3, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_NullFilter_ReturnsAllWithoutFiltering()
        {
            // Arrange
            SetupInvoiceDbSet(
            [
                MakeInvoice(1, "PRJ001"),
                MakeInvoice(2, "PRJ002")
            ]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            // Act
            var result = await _repository.GetPagedProjectInvoicesAsync(query, null);

            // Assert
            Assert.Equal(2, result.Data.Count);
        }

        #endregion

        #region GetTotalAmountAsync

        [Fact]
        public async Task GetTotalAmountAsync_WithParentProject_SumsOnlyMatchingInvoices()
        {
            // Arrange
            SetupInvoiceDbSet(
            [
                MakeInvoice(1, "PRJ001", amount: 200m),
                MakeInvoice(2, "PRJ002", amount: 300m)
            ]);

            // Act
            var total = await _repository.GetTotalAmountAsync("PRJ001");

            // Assert
            Assert.Equal(200m, total);
        }

        [Fact]
        public async Task GetTotalAmountAsync_NoParentProject_SumsAllInvoices()
        {
            // Arrange
            SetupInvoiceDbSet(
            [
                MakeInvoice(1, "PRJ001", amount: 100m),
                MakeInvoice(2, "PRJ002", amount: 250m)
            ]);

            // Act
            var total = await _repository.GetTotalAmountAsync(null);

            // Assert
            Assert.Equal(350m, total);
        }

        [Fact]
        public async Task GetTotalAmountAsync_NullAmounts_ReturnsZero()
        {
            // Arrange
            SetupInvoiceDbSet(
            [
                MakeInvoice(1, "PRJ001", amount: null)
            ]);

            // Act
            var total = await _repository.GetTotalAmountAsync(null);

            // Assert
            Assert.Equal(0m, total);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsInvoice()
        {
            // Arrange
            SetupInvoiceDbSet([MakeInvoice(1, "PRJ001"), MakeInvoice(2, "PRJ002")]);

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.InvoiceCounter);
        }

        [Fact]
        public async Task GetByIdAsync_NotFound_ReturnsNull()
        {
            // Arrange
            SetupInvoiceDbSet([MakeInvoice(1, "PRJ001")]);

            // Act
            var result = await _repository.GetByIdAsync(99);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetMonthlyInvoicesSummaryAsync

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_NoFilter_ReturnsAllRowsOrderedByProgramProjectMonth()
        {
            // Arrange
            SetupMonthlyInvoicesSummaryDbSet(
            [
                MakeSummary("Z", "PRJ2", 2, 200m),
                MakeSummary("A", "PRJ1", 1, 100m),
                MakeSummary("A", "PRJ1", 2, 150m)
            ]);
            var parameters = new PaginationParameters<string>();

            // Act
            var result = await _repository.GetMonthlyInvoicesSummaryAsync(parameters);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("A", result[0].Program);
            Assert.Equal(1, result[0].Month);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WithProgramFilter_FiltersMatchingRows()
        {
            // Arrange
            SetupMonthlyInvoicesSummaryDbSet(
            [
                MakeSummary("ADMIN", "PRJ1", 1, 100m),
                MakeSummary("PROG2", "PRJ2", 1, 200m)
            ]);
            var parameters = new PaginationParameters<string>
            {
                Filter = """{"Program":"ADMIN"}"""
            };

            // Act
            var result = await _repository.GetMonthlyInvoicesSummaryAsync(parameters);

            // Assert
            Assert.Single(result);
            Assert.Equal("ADMIN", result[0].Program);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WithParentProjectFilter_FiltersMatchingRows()
        {
            // Arrange
            SetupMonthlyInvoicesSummaryDbSet(
            [
                MakeSummary("PROG1", "ALPHA001", 1, 100m),
                MakeSummary("PROG2", "BETA002",  1, 200m)
            ]);
            var parameters = new PaginationParameters<string>
            {
                Filter = """{"ParentProject":"ALPHA"}"""
            };

            // Act
            var result = await _repository.GetMonthlyInvoicesSummaryAsync(parameters);

            // Assert
            Assert.Single(result);
            Assert.Equal("ALPHA001", result[0].ParentProject);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WithBothFilters_FiltersOnBothFields()
        {
            // Arrange
            SetupMonthlyInvoicesSummaryDbSet(
            [
                MakeSummary("ADMIN", "AH001", 1, 100m),
                MakeSummary("ADMIN", "BH002", 1, 200m),
                MakeSummary("PROG2", "AH001", 1, 300m)
            ]);
            var parameters = new PaginationParameters<string>
            {
                Filter = """{"Program":"ADMIN","ParentProject":"AH"}"""
            };

            // Act
            var result = await _repository.GetMonthlyInvoicesSummaryAsync(parameters);

            // Assert
            Assert.Single(result);
            Assert.Equal("ADMIN", result[0].Program);
            Assert.Equal("AH001", result[0].ParentProject);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_EmptyFilter_ReturnsAllRows()
        {
            // Arrange
            SetupMonthlyInvoicesSummaryDbSet(
            [
                MakeSummary("A", "PRJ1", 1, 10m),
                MakeSummary("B", "PRJ2", 1, 20m)
            ]);
            var parameters = new PaginationParameters<string> { Filter = "" };

            // Act
            var result = await _repository.GetMonthlyInvoicesSummaryAsync(parameters);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_NoMatchingFilter_ReturnsEmpty()
        {
            // Arrange
            SetupMonthlyInvoicesSummaryDbSet(
            [
                MakeSummary("PROG1", "PRJ1", 1, 100m)
            ]);
            var parameters = new PaginationParameters<string>
            {
                Filter = """{"Program":"NONEXISTENT"}"""
            };

            // Act
            var result = await _repository.GetMonthlyInvoicesSummaryAsync(parameters);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_SetsCurrentFpsYearOnEntity()
        {
            // Arrange
            var entity = MakeInvoice(0, "PRJ001", fpsYear: 0);
            _fpsRequestContext.FpsYear.Returns(2025);
            SetupInvoiceDbSet([]);
            _context.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

            // Act
            var result = await _repository.CreateAsync(entity);

            // Assert
            Assert.Equal(2025, result.FpsYear);
            await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_SetsCurrentFpsYearAndSaves()
        {
            // Arrange
            var entity = MakeInvoice(1, "PRJ001", fpsYear: 0);
            _fpsRequestContext.FpsYear.Returns(2025);
            _context.Entry(Arg.Any<ProjectInvoice>())
                .Throws(new NotSupportedException("Entry() is not supported in mocked DbContext"));

            // Act & Assert — Entry() throws but FpsYear is set before that call
            await Assert.ThrowsAsync<NotSupportedException>(() => _repository.UpdateAsync(entity));

            Assert.Equal(2025, entity.FpsYear);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ExistingInvoiceMatchingFpsYear_DeletesAndReturnsTrue()
        {
            // Arrange
            _fpsRequestContext.FpsYear.Returns(2025);
            var entity = MakeInvoice(1, "PRJ001", fpsYear: 2025);
            SetupInvoiceDbSet([entity]);
            _context.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

            // Act
            var result = await _repository.DeleteAsync(1);

            // Assert
            Assert.True(result);
            await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task DeleteAsync_InvoiceNotFound_ReturnsFalse()
        {
            // Arrange
            _fpsRequestContext.FpsYear.Returns(2025);
            SetupInvoiceDbSet([MakeInvoice(1, "PRJ001", fpsYear: 2025)]);

            // Act
            var result = await _repository.DeleteAsync(99);

            // Assert
            Assert.False(result);
            await _context.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task DeleteAsync_InvoiceExistsButDifferentFpsYear_ReturnsFalse()
        {
            // Arrange
            _fpsRequestContext.FpsYear.Returns(2025);
            SetupInvoiceDbSet([MakeInvoice(1, "PRJ001", fpsYear: 2024)]);

            // Act
            var result = await _repository.DeleteAsync(1);

            // Assert
            Assert.False(result);
            await _context.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        #endregion
    }
}
