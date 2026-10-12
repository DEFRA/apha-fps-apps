using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.MabArchive.Loaders;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Tests for TlkpYearLoader reading latestmonthreleased from the fps.tbldb_variables 'Month' row.
/// Behavioral tests require a live PostgreSQL connection and run inside a rolled-back transaction,
/// so changes to the shared 'Month' row never persist.
/// </summary>
[Trait("Category", "Integration")]
public sealed class TlkpYearLoaderTests : IAsyncLifetime
{
    private const int TargetYear = 1899;

    private readonly string _connectionString;
    private string? _skipReason;

    public TlkpYearLoaderTests()
    {
        _connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__FPSConnectionString")
            ?? string.Empty;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await using var ctx = CreateDbContext();
            if (!await ctx.Database.CanConnectAsync())
            {
                _skipReason = "Integration DB unavailable.";
                return;
            }

            var monthRows = await ctx.Database
                .SqlQuery<int>($@"
                    SELECT COUNT(*)::int AS ""Value""
                    FROM fps.tbldb_variables
                    WHERE LOWER(db_var_name) = 'month'")
                .SingleAsync();

            if (monthRows != 1)
            {
                _skipReason = $"Expected exactly one 'Month' row in fps.tbldb_variables, found {monthRows}.";
            }
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public void Loader_HasSequence16_AndName_tlkpyear()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseInMemoryDatabase("tlkpyear-metadata")
            .Options;
        var loader = new TlkpYearLoader(new BatchJobsDbContext(options));

        Assert.Equal(16, loader.Sequence);
        Assert.Equal("tlkpyear", loader.Name);
    }

    [SkippableTheory]
    [InlineData("0", 0)]
    [InlineData("5", 5)]
    [InlineData("12", 12)]
    [InlineData(" 4 ", 4)]
    public async Task LoadAsync_WhenMonthIsValid_InsertsYearWithThatMonth(string storedValue, int expectedMonth)
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await using var ctx = CreateDbContext();
        await using var tx = await ctx.Database.BeginTransactionAsync();

        await SetMonthValueAsync(ctx, storedValue);

        var rowsAffected = await new TlkpYearLoader(ctx).LoadAsync(TargetYear, CancellationToken.None);
        var inserted = await ctx.MaDstTlkpYear.AsNoTracking().SingleOrDefaultAsync(x => x.Year == TargetYear);

        await tx.RollbackAsync();

        Assert.Equal(1, rowsAffected);
        Assert.NotNull(inserted);
        Assert.Equal(expectedMonth, inserted.LatestMonthReleased);
    }

    [SkippableFact]
    public async Task LoadAsync_WhenMonthNameIsLowercase_StillReadsIt()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await using var ctx = CreateDbContext();
        await using var tx = await ctx.Database.BeginTransactionAsync();

        await ctx.Database.ExecuteSqlRawAsync(
            "UPDATE fps.tbldb_variables SET db_var_name = 'month', db_var_value = '3' WHERE LOWER(db_var_name) = 'month'");

        await new TlkpYearLoader(ctx).LoadAsync(TargetYear, CancellationToken.None);
        var inserted = await ctx.MaDstTlkpYear.AsNoTracking().SingleAsync(x => x.Year == TargetYear);

        await tx.RollbackAsync();

        Assert.Equal(3, inserted.LatestMonthReleased);
    }

    [SkippableFact]
    public async Task LoadAsync_WhenMonthRowIsMissing_Throws()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await using var ctx = CreateDbContext();
        await using var tx = await ctx.Database.BeginTransactionAsync();

        await ctx.Database.ExecuteSqlRawAsync("DELETE FROM fps.tbldb_variables WHERE LOWER(db_var_name) = 'month'");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => new TlkpYearLoader(ctx).LoadAsync(TargetYear, CancellationToken.None));

        await tx.RollbackAsync();

        Assert.Contains("fps.tbldb_variables", ex.Message, StringComparison.Ordinal);
        Assert.Contains("found 0", ex.Message, StringComparison.Ordinal);
    }

    [SkippableFact]
    public async Task LoadAsync_WhenMonthRowIsDuplicatedAcrossCasing_Throws()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await using var ctx = CreateDbContext();
        await using var tx = await ctx.Database.BeginTransactionAsync();

        // Differs from both 'Month' and 'month', so it never collides with the real row's PK.
        await ctx.Database.ExecuteSqlRawAsync(
            "INSERT INTO fps.tbldb_variables (db_var_name, db_var_value) VALUES ('MONTH', '5')");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => new TlkpYearLoader(ctx).LoadAsync(TargetYear, CancellationToken.None));

        await tx.RollbackAsync();

        Assert.Contains("fps.tbldb_variables", ex.Message, StringComparison.Ordinal);
        Assert.Contains("found 2", ex.Message, StringComparison.Ordinal);
    }

    [SkippableTheory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("-1")]
    [InlineData("13")]
    [InlineData("4.5")]
    public async Task LoadAsync_WhenMonthValueIsInvalid_Throws(string? storedValue)
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await using var ctx = CreateDbContext();
        await using var tx = await ctx.Database.BeginTransactionAsync();

        await SetMonthValueAsync(ctx, storedValue);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => new TlkpYearLoader(ctx).LoadAsync(TargetYear, CancellationToken.None));
        var inserted = await ctx.MaDstTlkpYear.AsNoTracking().AnyAsync(x => x.Year == TargetYear);

        await tx.RollbackAsync();

        Assert.Contains("Invalid 'Month' value", ex.Message, StringComparison.Ordinal);
        Assert.False(inserted);
    }

    [SkippableFact]
    public async Task LoadAsync_IsIdempotent_WhenCalledTwiceForSameYear()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await using var ctx = CreateDbContext();
        await using var tx = await ctx.Database.BeginTransactionAsync();

        await SetMonthValueAsync(ctx, "6");
        var loader = new TlkpYearLoader(ctx);

        await loader.LoadAsync(TargetYear, CancellationToken.None);
        // Mirrors MabArchiveYearRepository, which clears the tracker before each loader.
        ctx.ChangeTracker.Clear();
        var secondCallRows = await loader.LoadAsync(TargetYear, CancellationToken.None);

        var count = await ctx.MaDstTlkpYear.CountAsync(x => x.Year == TargetYear);

        await tx.RollbackAsync();

        Assert.Equal(1, secondCallRows);
        Assert.Equal(1, count);
    }

    private static Task SetMonthValueAsync(BatchJobsDbContext ctx, string? value) =>
        ctx.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE fps.tbldb_variables SET db_var_value = {value} WHERE LOWER(db_var_name) = 'month';");

    private BatchJobsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new BatchJobsDbContext(options);
    }

    private bool CanRunIntegrationTests() => string.IsNullOrWhiteSpace(_skipReason);
}
