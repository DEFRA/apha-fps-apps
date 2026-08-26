using Apha.Common.Utilities.ExcelExport;
using Apha.FPS.Application.Common.BulkRates;
using Apha.FPS.Application.Services;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Apha.FPS.Application.UnitTests.Services.BulkRatesServiceTest;

/// <summary>
/// Unit tests for <see cref="BulkStaffRatesService"/> — the Staff process service extracted from
/// StaffAnimalValidationService/BulkRatesValidator/BulkRatesRequestService per Phase 3 of the
/// Bulk Rates low-risk phase-wise execution plan. Exercises the public
/// ProcessUploadAsync/PrepareForReleaseAsync/export/staging surface, same approach as
/// BulkTestRatesServiceTests. StaffAnimalValidationServiceTests continues to cover the
/// still-fully-operational old class (including the Animal half, untouched by this phase).
/// </summary>
public class BulkStaffRatesServiceTests
{
    private const int FpsYear = 2027;
    private static readonly Guid QueueId = Guid.NewGuid();

    private static BulkStaffRatesService CreateService(
        IBulkRatesRepository? repo = null, IExcelExportService? excel = null)
        => new(
            repo ?? Substitute.For<IBulkRatesRepository>(),
            excel ?? Substitute.For<IExcelExportService>(),
            NullLogger<BulkStaffRatesService>.Instance);

    private static ProfitCentreGradeStagingRow Staff(string pcGrade, decimal? payRate, decimal? npr = null, decimal? ohr = null)
        => new() { PcGrade = pcGrade, PayRate = payRate, Npr = npr, Ohr = ohr };

    private static ProfitCentreGradeStagingRow LiveStaff(string pcGrade, decimal? payRate, decimal? npr = null, decimal? ohr = null)
        => new() { PcGrade = pcGrade, PayRate = payRate, Npr = npr, Ohr = ohr };

    private static BulkRatesParseResult ParseResult(
        IReadOnlyList<ProfitCentreGradeStagingRow>? staff = null, IReadOnlyList<string>? parseErrors = null)
        => new() { JobQueueId = QueueId, StaffRows = staff ?? [], ParseErrors = parseErrors ?? [] };

    private static IBulkRatesRepository RepoWith(IReadOnlyList<ProfitCentreGradeStagingRow>? liveStaff = null)
    {
        var repo = Substitute.For<IBulkRatesRepository>();
        repo.GetStaffRowsForExportAsync(FpsYear, Arg.Any<CancellationToken>())
            .Returns(liveStaff ?? Array.Empty<ProfitCentreGradeStagingRow>());
        return repo;
    }

    // ── Missing / duplicate key ──────────────────────────────────────────────────

    [Fact]
    public async Task MissingPcGrade_IsInvalid()
    {
        var sut = CreateService(RepoWith());

        var result = await sut.ProcessUploadAsync(ParseResult(staff: [Staff("", 10)]), FpsYear, 1);

        result.Errors.Should().ContainSingle(e => e.ValidationCode == "MISSING_GRADE");
        result.RowCounts.Invalid.Should().Be(1);
    }

    [Fact]
    public async Task DuplicatePcGrade_RaisesError()
    {
        var sut = CreateService(RepoWith());

        var result = await sut.ProcessUploadAsync(
            ParseResult(staff: [Staff("G1", 10), Staff("g1", 12)]), FpsYear, 1);

        result.Errors.Count(e => e.ValidationCode == "DUPLICATE_GRADE").Should().Be(2);
    }

    // ── Negative rates ───────────────────────────────────────────────────────────

    [Theory]
    [InlineData(-1, null, null, "payrate")]
    [InlineData(null, -1, null, "npr")]
    [InlineData(null, null, -1, "ohr")]
    public async Task NegativeRate_IsBlockingError(int? pay, int? npr, int? ohr, string expectedField)
    {
        var sut = CreateService(RepoWith());

        var result = await sut.ProcessUploadAsync(ParseResult(staff: [Staff("G1", pay, npr, ohr)]), FpsYear, 1);

        result.Errors.Should().ContainSingle(e => e.ValidationCode == "NEGATIVE_RATE" && e.FieldName == expectedField);
    }

    // ── NotFound (update-only — no insert path) ───────────────────────────────────

    [Fact]
    public async Task UnknownPcGrade_IsNotFound_NotInsert()
    {
        var sut = CreateService(RepoWith());

        var result = await sut.ProcessUploadAsync(ParseResult(staff: [Staff("UNKNOWN", 10)]), FpsYear, 1);

        result.Errors.Should().ContainSingle(e => e.ValidationCode == "GRADE_NOT_FOUND");
        result.RowCounts.Insert.Should().Be(0);
        result.RowCounts.Invalid.Should().Be(1);
    }

    // ── Update-only rate classification ───────────────────────────────────────────

    [Fact]
    public async Task ExistingGrade_SameRates_ClassifiesAsNoChange()
    {
        var repo = RepoWith([LiveStaff("G1", 10, 5, 2)]);
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(ParseResult(staff: [Staff("G1", 10, 5, 2)]), FpsYear, 1);

        result.Errors.Should().BeEmpty();
        result.RowCounts.Unchanged.Should().Be(1);
        result.RowCounts.Update.Should().Be(0);
    }

    [Fact]
    public async Task ExistingGrade_DifferentRate_ClassifiesAsUpdate()
    {
        var repo = RepoWith([LiveStaff("G1", 10, 5, 2)]);
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(ParseResult(staff: [Staff("G1", 15, 5, 2)]), FpsYear, 1);

        result.RowCounts.Update.Should().Be(1);
    }

    [Fact]
    public async Task ExistingGrade_BlankFieldTreatedAsZero_NotAsUnchangedLiveValue()
    {
        // StaffAnimalFieldComparer.AmountEquals: null and 0 are equivalent, so a blank staged
        // NPR against a live NPR of 5 is a real change (blank means "target 0"), not "leave as is".
        var repo = RepoWith([LiveStaff("G1", 10, 5, 2)]);
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(ParseResult(staff: [Staff("G1", 10, null, 2)]), FpsYear, 1);

        result.RowCounts.Update.Should().Be(1);
    }

    // ── Parse-error staging quirk (§1c) ──────────────────────────────────────────

    [Fact]
    public async Task ParseErrors_StillStagesParsedRows_AndSkipsBusinessValidation()
    {
        var repo = RepoWith();
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(
            ParseResult(staff: [Staff("G1", 10)], parseErrors: ["Sheet 'Staff' is missing a column."]), FpsYear, 1);

        result.Errors.Should().ContainSingle(e => e.ValidationCode == "FILE_ERROR" && e.SourceRowNumber == 0);
        result.Errors.Should().NotContain(e => e.ValidationCode == "GRADE_NOT_FOUND");
        await repo.Received(1).ReplaceStagingStaffAsync(
            QueueId, Arg.Is<IReadOnlyList<ProfitCentreGradeStagingRow>>(l => l.Count == 1), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HappyPath_AlwaysReplacesStaging()
    {
        var repo = RepoWith([LiveStaff("G1", 10)]);
        var sut = CreateService(repo);

        await sut.ProcessUploadAsync(ParseResult(staff: [Staff("G1", 15)]), FpsYear, 1);

        await repo.Received(1).ReplaceStagingStaffAsync(
            QueueId, Arg.Any<IReadOnlyList<ProfitCentreGradeStagingRow>>(), Arg.Any<CancellationToken>());
    }

    // ── Release / freeze ─────────────────────────────────────────────────────────

    [Fact]
    public async Task PrepareForReleaseAsync_BlockingErrors_ThrowsAndDoesNotFreeze()
    {
        var repo = RepoWith();
        repo.GetProfitCentreGradeStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns([Staff("UNKNOWN", 10)]); // NotFound -> blocking error
        var sut = CreateService(repo);

        var act = async () => await sut.PrepareForReleaseAsync(QueueId, FpsYear);

        await act.Should().ThrowAsync<BusinessValidationErrorException>();
        await repo.DidNotReceive().FreezeStaffStagingAsync(
            Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<IReadOnlyList<StaffFreezeEntry>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PrepareForReleaseAsync_NoBlockingErrors_FreezesStaffClassifications()
    {
        var repo = RepoWith([LiveStaff("G1", 10, 5, 2)]);
        repo.GetProfitCentreGradeStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns([Staff("G1", 15, 5, 2)]);
        var sut = CreateService(repo);

        await sut.PrepareForReleaseAsync(QueueId, FpsYear);

        await repo.Received(1).FreezeStaffStagingAsync(
            QueueId, 1,
            Arg.Is<IReadOnlyList<StaffFreezeEntry>>(l => l.Count == 1 && l[0].PcGrade == "G1" && l[0].CalculatedAction == "Update" && l[0].EffectivePayRate == 15),
            Arg.Any<CancellationToken>());
    }

    // ── Export / download / staging ──────────────────────────────────────────────

    [Fact]
    public async Task ExportTestDataAsync_ExportsLiveStaffRows()
    {
        var excel = Substitute.For<IExcelExportService>();
        var repo = RepoWith([LiveStaff("G1", 10)]);
        var sut = CreateService(repo, excel);

        await sut.ExportTestDataAsync(FpsYear);

        excel.Received(1).ExportToExcelMultiSheet(Arg.Is<IEnumerable<ExcelSheetDefinition>>(
            sheets => sheets.Count() == 1 && sheets.Single().SheetName == "Staff"));
    }

    [Fact]
    public async Task DownloadTestDataAsync_CreatesSnapshotAndMarksReady()
    {
        var repo = RepoWith([LiveStaff("G1", 10)]);
        repo.GetNextDownloadVersionAsync(QueueId, Arg.Any<CancellationToken>()).Returns(1);
        var entry = new BulkRatesQueueRow { JobQueueId = QueueId, FpsYear = FpsYear };
        var sut = CreateService(repo);

        await sut.DownloadTestDataAsync(entry);

        await repo.Received(1).CreateStaffDownloadSnapshotAsync(QueueId, 1, Arg.Any<IReadOnlyList<ProfitCentreGradeStagingRow>>(), Arg.Any<CancellationToken>());
        await repo.Received(1).MarkDownloadReadyAsync(QueueId, 1, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DownloadTestDataAsync_WhenExportThrows_MarksFailed_AndRethrows()
    {
        var excel = Substitute.For<IExcelExportService>();
        excel.ExportToExcelMultiSheet(Arg.Any<IEnumerable<ExcelSheetDefinition>>(), Arg.Any<IReadOnlyDictionary<string, string>>())
            .Returns(_ => throw new InvalidOperationException("boom"));
        var repo = RepoWith();
        repo.GetNextDownloadVersionAsync(QueueId, Arg.Any<CancellationToken>()).Returns(1);
        var entry = new BulkRatesQueueRow { JobQueueId = QueueId, FpsYear = FpsYear };
        var sut = CreateService(repo, excel);

        var act = async () => await sut.DownloadTestDataAsync(entry);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await repo.Received(1).MarkDownloadFailedAsync(QueueId, 1, Arg.Any<CancellationToken>());
        await repo.DidNotReceive().MarkDownloadReadyAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetStagingDataAsync_UnknownGrade_ShowsAsNotFound()
    {
        var repo = RepoWith([LiveStaff("G1", 10, 5, 2)]);
        repo.GetProfitCentreGradeStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns([Staff("G1", 10, 5, 2), Staff("UNKNOWN", 20)]);
        var entry = new BulkRatesQueueRow { JobQueueId = QueueId, FpsYear = FpsYear };
        var sut = CreateService(repo);

        var result = await sut.GetStagingDataAsync(entry);

        result.StaffRows.Should().Contain(r => r.PcGrade == "UNKNOWN" && r.Status == "Not Found");
        result.StaffRows.Should().Contain(r => r.PcGrade == "G1" && r.Status == "No Change");
        // Not Found sorts before No Change.
        result.StaffRows.First().PcGrade.Should().Be("UNKNOWN");
    }

    [Fact]
    public async Task GetStagingDataAsync_ChangedRate_ShowsAsUpdated()
    {
        var repo = RepoWith([LiveStaff("G1", 10, 5, 2)]);
        repo.GetProfitCentreGradeStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns([Staff("G1", 99, 5, 2)]);
        var entry = new BulkRatesQueueRow { JobQueueId = QueueId, FpsYear = FpsYear };
        var sut = CreateService(repo);

        var result = await sut.GetStagingDataAsync(entry);

        result.StaffRows.Should().ContainSingle(r => r.PcGrade == "G1" && r.Status == "Updated" && r.PayRateNew == 99);
    }

    [Fact]
    public async Task ExportStagingDataAsync_ExportsStagedRows()
    {
        var excel = Substitute.For<IExcelExportService>();
        var repo = RepoWith();
        repo.GetProfitCentreGradeStagingRowsAsync(QueueId, Arg.Any<CancellationToken>()).Returns([Staff("G1", 10)]);
        var sut = CreateService(repo, excel);

        await sut.ExportStagingDataAsync(QueueId);

        excel.Received(1).ExportToExcelMultiSheet(Arg.Is<IEnumerable<ExcelSheetDefinition>>(
            sheets => sheets.Single().SheetName == "Staff"));
    }
}
