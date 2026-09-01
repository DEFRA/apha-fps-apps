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
/// Unit tests for <see cref="BulkAnimalRatesService"/> — the Animal process service extracted from
/// StaffAnimalValidationService/BulkRatesValidator/BulkRatesRequestService per Phase 4 of the
/// Bulk Rates low-risk phase-wise execution plan. Exercises the public
/// ProcessUploadAsync/PrepareForReleaseAsync/export/staging surface, same approach as
/// BulkTestRatesServiceTests/BulkStaffRatesServiceTests. StaffAnimalValidationServiceTests
/// continues to cover the still-fully-operational old class.
/// </summary>
public class BulkAnimalRatesServiceTests
{
    private const int FpsYear = 2027;
    private static readonly Guid QueueId = Guid.NewGuid();

    private static BulkAnimalRatesService CreateService(
        IBulkRatesRepository? repo = null, IExcelExportService? excel = null)
        => new(
            repo ?? Substitute.For<IBulkRatesRepository>(),
            excel ?? Substitute.For<IExcelExportService>(),
            NullLogger<BulkAnimalRatesService>.Instance);

    private static AnimalStagingRow Animal(
        string animalType, decimal? dailyRate, decimal? defraDailyRate = null,
        bool? planByWeek = null, string? species = null, string? securityLevel = null)
        => new()
        {
            AnimalType = animalType, DailyRate = dailyRate, DefraDailyRate = defraDailyRate,
            PlanByWeek = planByWeek, Species = species, SecurityLevel = securityLevel
        };

    private static AnimalStagingRow LiveAnimal(
        string animalType, decimal? dailyRate, decimal? defraDailyRate = null,
        bool planByWeek = false, string? species = null, string? securityLevel = null)
        => new()
        {
            AnimalType = animalType, DailyRate = dailyRate, DefraDailyRate = defraDailyRate,
            PlanByWeek = planByWeek, Species = species, SecurityLevel = securityLevel
        };

    private static BulkRatesParseResult ParseResult(
        IReadOnlyList<AnimalStagingRow>? animal = null, IReadOnlyList<string>? parseErrors = null)
        => new() { JobQueueId = QueueId, AnimalRows = animal ?? [], ParseErrors = parseErrors ?? [] };

    private static IBulkRatesRepository RepoWith(IReadOnlyList<AnimalStagingRow>? liveAnimal = null)
    {
        var repo = Substitute.For<IBulkRatesRepository>();
        repo.GetAnimalRowsForExportAsync(FpsYear, Arg.Any<CancellationToken>())
            .Returns(liveAnimal ?? Array.Empty<AnimalStagingRow>());
        return repo;
    }

    // ── Missing / duplicate key ──────────────────────────────────────────────────

    [Fact]
    public async Task MissingAnimalType_IsInvalid()
    {
        var sut = CreateService(RepoWith());

        var result = await sut.ProcessUploadAsync(ParseResult(animal: [Animal("", 10)]), FpsYear, 1);

        result.Errors.Should().ContainSingle(e => e.ValidationCode == "MISSING_ANIMAL_TYPE");
        result.RowCounts.Invalid.Should().Be(1);
    }

    [Fact]
    public async Task DuplicateAnimalType_RaisesError()
    {
        var sut = CreateService(RepoWith());

        var result = await sut.ProcessUploadAsync(
            ParseResult(animal: [Animal("Cattle", 10), Animal("cattle", 12)]), FpsYear, 1);

        result.Errors.Count(e => e.ValidationCode == "DUPLICATE_ANIMAL_TYPE").Should().Be(2);
    }

    // ── Negative rates ───────────────────────────────────────────────────────────

    [Theory]
    [InlineData(-1, null, "dailyrate")]
    [InlineData(null, -1, "defradailyrate")]
    public async Task NegativeRate_IsBlockingError(int? dailyRate, int? defraDailyRate, string expectedField)
    {
        var sut = CreateService(RepoWith());

        var result = await sut.ProcessUploadAsync(ParseResult(animal: [Animal("Cattle", dailyRate, defraDailyRate)]), FpsYear, 1);

        result.Errors.Should().ContainSingle(e => e.ValidationCode == "NEGATIVE_RATE" && e.FieldName == expectedField);
    }

    // ── NotFound (update-only — no insert path) ───────────────────────────────────

    [Fact]
    public async Task UnknownAnimalType_IsNotFound_NotInsert()
    {
        var sut = CreateService(RepoWith());

        var result = await sut.ProcessUploadAsync(ParseResult(animal: [Animal("Unknown", 10)]), FpsYear, 1);

        result.Errors.Should().ContainSingle(e => e.ValidationCode == "ANIMAL_TYPE_NOT_FOUND");
        result.RowCounts.Insert.Should().Be(0);
        result.RowCounts.Invalid.Should().Be(1);
    }

    // ── Update-only field-diff classification (5-field drift, not just rate) ─────────

    [Fact]
    public async Task ExistingType_AllFieldsSame_ClassifiesAsNoChange()
    {
        var repo = RepoWith([LiveAnimal("Cattle", 10, 5, true, "Bovine", "High")]);
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(
            ParseResult(animal: [Animal("Cattle", 10, 5, true, "Bovine", "High")]), FpsYear, 1);

        result.Errors.Should().BeEmpty();
        result.RowCounts.Unchanged.Should().Be(1);
        result.RowCounts.Update.Should().Be(0);
    }

    [Fact]
    public async Task ExistingType_DifferentRate_ClassifiesAsUpdate()
    {
        var repo = RepoWith([LiveAnimal("Cattle", 10, 5, true, "Bovine", "High")]);
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(
            ParseResult(animal: [Animal("Cattle", 99, 5, true, "Bovine", "High")]), FpsYear, 1);

        result.RowCounts.Update.Should().Be(1);
    }

    [Fact]
    public async Task ExistingType_OnlySpeciesChanged_ClassifiesAsUpdate()
    {
        // Species/SecurityLevel participate in drift detection alongside the rate fields —
        // unlike Staff, Animal's classification isn't rate-only.
        var repo = RepoWith([LiveAnimal("Cattle", 10, 5, true, "Bovine", "High")]);
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(
            ParseResult(animal: [Animal("Cattle", 10, 5, true, "Ovine", "High")]), FpsYear, 1);

        result.RowCounts.Update.Should().Be(1);
    }

    [Fact]
    public async Task ExistingType_TextComparison_TrimmedAndCaseInsensitive()
    {
        var repo = RepoWith([LiveAnimal("Cattle", 10, 5, true, "Bovine", "High")]);
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(
            ParseResult(animal: [Animal("Cattle", 10, 5, true, "  bovine  ", "high")]), FpsYear, 1);

        result.RowCounts.Unchanged.Should().Be(1);
    }

    // ── Parse-error staging quirk (§1c) ──────────────────────────────────────────

    [Fact]
    public async Task ParseErrors_StillStagesParsedRows_AndSkipsBusinessValidation()
    {
        var repo = RepoWith();
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(
            ParseResult(animal: [Animal("Cattle", 10)], parseErrors: ["Sheet 'Animals' is missing a column."]), FpsYear, 1);

        result.Errors.Should().ContainSingle(e => e.ValidationCode == "FILE_ERROR" && e.SourceRowNumber == 0);
        result.Errors.Should().NotContain(e => e.ValidationCode == "ANIMAL_TYPE_NOT_FOUND");
        await repo.Received(1).ReplaceStagingAnimalAsync(
            QueueId, Arg.Is<IReadOnlyList<AnimalStagingRow>>(l => l.Count == 1), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HappyPath_AlwaysReplacesStaging()
    {
        var repo = RepoWith([LiveAnimal("Cattle", 10)]);
        var sut = CreateService(repo);

        await sut.ProcessUploadAsync(ParseResult(animal: [Animal("Cattle", 15)]), FpsYear, 1);

        await repo.Received(1).ReplaceStagingAnimalAsync(
            QueueId, Arg.Any<IReadOnlyList<AnimalStagingRow>>(), Arg.Any<CancellationToken>());
    }

    // ── Release / freeze ─────────────────────────────────────────────────────────

    [Fact]
    public async Task PrepareForReleaseAsync_BlockingErrors_ThrowsAndDoesNotFreeze()
    {
        var repo = RepoWith();
        repo.GetAnimalStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns([Animal("Unknown", 10)]); // NotFound -> blocking error
        var sut = CreateService(repo);

        var act = async () => await sut.PrepareForReleaseAsync(QueueId, FpsYear);

        await act.Should().ThrowAsync<BusinessValidationErrorException>();
        await repo.DidNotReceive().FreezeAnimalStagingAsync(
            Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<IReadOnlyList<AnimalFreezeEntry>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PrepareForReleaseAsync_NoBlockingErrors_FreezesAnimalClassifications()
    {
        var repo = RepoWith([LiveAnimal("Cattle", 10, 5, true, "Bovine", "High")]);
        repo.GetAnimalStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns([Animal("Cattle", 99, 5, true, "Bovine", "High")]);
        var sut = CreateService(repo);

        await sut.PrepareForReleaseAsync(QueueId, FpsYear);

        await repo.Received(1).FreezeAnimalStagingAsync(
            QueueId, 1,
            Arg.Is<IReadOnlyList<AnimalFreezeEntry>>(l => l.Count == 1 && l[0].AnimalType == "Cattle" && l[0].CalculatedAction == "Update" && l[0].EffectiveDailyRate == 99),
            Arg.Any<CancellationToken>());
    }

    // ── Export / download / staging ──────────────────────────────────────────────

    [Fact]
    public async Task ExportTestDataAsync_ExportsLiveAnimalRows()
    {
        var excel = Substitute.For<IExcelExportService>();
        var repo = RepoWith([LiveAnimal("Cattle", 10)]);
        var sut = CreateService(repo, excel);

        await sut.ExportTestDataAsync(FpsYear);

        excel.Received(1).ExportToExcelMultiSheet(Arg.Is<IEnumerable<ExcelSheetDefinition>>(
            sheets => sheets.Count() == 1 && sheets.Single().SheetName == "Animals"));
    }

    [Fact]
    public async Task DownloadTestDataAsync_CreatesSnapshotAndMarksReady()
    {
        var repo = RepoWith([LiveAnimal("Cattle", 10)]);
        repo.GetNextDownloadVersionAsync(QueueId, Arg.Any<CancellationToken>()).Returns(1);
        var entry = new BulkRatesQueueRow { JobQueueId = QueueId, FpsYear = FpsYear };
        var sut = CreateService(repo);

        await sut.DownloadTestDataAsync(entry);

        await repo.Received(1).CreateAnimalDownloadSnapshotAsync(QueueId, 1, Arg.Any<IReadOnlyList<AnimalStagingRow>>(), Arg.Any<CancellationToken>());
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
    public async Task GetStagingDataAsync_UnknownType_ShowsAsNotFound()
    {
        var repo = RepoWith([LiveAnimal("Cattle", 10)]);
        repo.GetAnimalStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns([Animal("Cattle", 10), Animal("Unknown", 20)]);
        var entry = new BulkRatesQueueRow { JobQueueId = QueueId, FpsYear = FpsYear };
        var sut = CreateService(repo);

        var result = await sut.GetStagingDataAsync(entry);

        result.AnimalRows.Should().Contain(r => r.AnimalType == "Unknown" && r.Status == "Not Found");
        result.AnimalRows.Should().Contain(r => r.AnimalType == "Cattle" && r.Status == "No Change");
        result.AnimalRows.First().AnimalType.Should().Be("Unknown");
    }

    [Fact]
    public async Task GetStagingDataAsync_ChangedRate_ShowsAsUpdated()
    {
        var repo = RepoWith([LiveAnimal("Cattle", 10)]);
        repo.GetAnimalStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns([Animal("Cattle", 99)]);
        var entry = new BulkRatesQueueRow { JobQueueId = QueueId, FpsYear = FpsYear };
        var sut = CreateService(repo);

        var result = await sut.GetStagingDataAsync(entry);

        result.AnimalRows.Should().ContainSingle(r => r.AnimalType == "Cattle" && r.Status == "Updated" && r.DailyRateNew == 99);
    }

    [Fact]
    public async Task ExportStagingDataAsync_ExportsStagedRows()
    {
        var excel = Substitute.For<IExcelExportService>();
        var repo = RepoWith();
        repo.GetAnimalStagingRowsAsync(QueueId, Arg.Any<CancellationToken>()).Returns([Animal("Cattle", 10)]);
        var sut = CreateService(repo, excel);

        await sut.ExportStagingDataAsync(QueueId);

        excel.Received(1).ExportToExcelMultiSheet(Arg.Is<IEnumerable<ExcelSheetDefinition>>(
            sheets => sheets.Single().SheetName == "Animals"));
    }
}
