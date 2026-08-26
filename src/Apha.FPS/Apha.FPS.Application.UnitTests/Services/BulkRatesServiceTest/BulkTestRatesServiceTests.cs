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
/// Unit tests for <see cref="BulkTestRatesService"/> — the FEC/AGRUP process service extracted
/// from BulkRatesValidator/BulkRatesValidationService per the Bulk Rates service/validation
/// refactor (Phase 2 of the low-risk phase-wise execution plan). Exercises the public
/// ProcessUploadAsync/PrepareForReleaseAsync/export/staging surface rather than the old rule
/// engine's ValidationContext directly, since that context is now a private implementation
/// detail of this service. BulkRatesValidationServiceTests/BulkRatesValidatorTests continue to
/// cover the still-fully-operational old classes until Phase 8 deletes them — this file does
/// not need to be a line-for-line port of those to avoid a coverage regression during the move.
///
/// Not covered here: ValidationContext.IncludeWorkerOnlyChecks=true (the BC-05 live/snapshot
/// interim rule's worker-only variant). Every current FPS caller — old and new — leaves this
/// false; there is no reachable seam on BulkTestRatesService's public API to exercise the
/// true branch, same as before this move (only a hand-built ValidationContext in
/// BulkRatesValidationServiceTests could reach it). That test class still covers it directly.
/// </summary>
public class BulkTestRatesServiceTests
{
    private const int FpsYear = 2027;
    private static readonly Guid QueueId = Guid.NewGuid();

    // ── Fixtures ─────────────────────────────────────────────────────────────

    private static BulkTestRatesService CreateService(
        IBulkRatesRepository? repo = null, IExcelExportService? excel = null)
        => new(
            repo ?? Substitute.For<IBulkRatesRepository>(),
            excel ?? Substitute.For<IExcelExportService>(),
            NullLogger<BulkTestRatesService>.Instance);

    private static TestOrProductStagingRow Fec(
        string testCode, decimal? rate, string? item = "desc", string? shortDesc = "short", string? owner = "PT")
        => new() { TestCode = testCode, FecNewRate = rate, ItemDescription = item, ShortDescription = shortDesc, Owner = owner };

    private static TestRequirementStagingRow Agrup(
        string testCode, string buyer, decimal? rate,
        string? projectBuyerCode = null, string? testBuyerCode = null, string? testBuyerWorkGroup = null, string? comments = null)
        => new()
        {
            TestCode = testCode, Buyer = buyer, AgrupNew = rate,
            ProjectBuyerCode = projectBuyerCode, TestBuyerCode = testBuyerCode, TestBuyerWorkGroup = testBuyerWorkGroup,
            Comments = comments
        };

    private static TestOrProductStagingRow LiveFec(string testCode, decimal? unitPriceVla, decimal? defraUnitPrice)
        => new() { TestCode = testCode, UnitPriceVla = unitPriceVla, DefraUnitPrice = defraUnitPrice };

    private static TestRequirementStagingRow LiveAgrup(
        string testCode, string buyer, decimal? unitPrice, string? projectBuyerCode = null, string? testBuyerCode = null)
        => new() { TestCode = testCode, Buyer = buyer, Agrup = unitPrice, ProjectBuyerCode = projectBuyerCode, TestBuyerCode = testBuyerCode };

    private static BulkRatesParseResult ParseResult(
        IReadOnlyList<TestOrProductStagingRow>? fec = null,
        IReadOnlyList<TestRequirementStagingRow>? agrup = null,
        IReadOnlyList<string>? parseErrors = null)
        => new()
        {
            JobQueueId = QueueId,
            FecRows = fec ?? [],
            AgrupRows = agrup ?? [],
            ParseErrors = parseErrors ?? []
        };

    private static IBulkRatesRepository RepoWith(
        IReadOnlyList<TestOrProductStagingRow>? liveFec = null,
        IReadOnlyList<TestRequirementStagingRow>? liveAgrup = null,
        IReadOnlySet<string>? projectCodes = null,
        IReadOnlySet<(string TestCode, string WorkGroup)>? capabilityPairs = null,
        IReadOnlyList<TestOrProductStagingRow>? snapshotFec = null,
        IReadOnlyList<TestRequirementStagingRow>? snapshotAgrup = null)
    {
        var repo = Substitute.For<IBulkRatesRepository>();
        repo.GetFecRowsForExportAsync(FpsYear, Arg.Any<CancellationToken>())
            .Returns(liveFec ?? Array.Empty<TestOrProductStagingRow>());
        repo.GetAgrupRowsForExportAsync(FpsYear, Arg.Any<CancellationToken>())
            .Returns(liveAgrup ?? Array.Empty<TestRequirementStagingRow>());
        repo.GetExistingProjectCodesAsync(Arg.Any<IEnumerable<string>>(), FpsYear, Arg.Any<CancellationToken>())
            .Returns(projectCodes ?? new HashSet<string>());
        repo.GetExistingCapabilityPairsAsync(Arg.Any<IEnumerable<(string, string)>>(), FpsYear, Arg.Any<CancellationToken>())
            .Returns(capabilityPairs ?? new HashSet<(string, string)>());
        repo.GetFecSnapshotRowsAsync(QueueId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(snapshotFec ?? Array.Empty<TestOrProductStagingRow>());
        repo.GetAgrupSnapshotRowsAsync(QueueId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(snapshotAgrup ?? Array.Empty<TestRequirementStagingRow>());
        return repo;
    }

    // ── FEC: existing-row blank/zero -> Zero-Rate Withdrawal ────────────────────

    [Fact]
    public async Task ExistingFecRow_BlankRate_ClassifiesAsZeroRateWithdrawal_NotError()
    {
        var repo = RepoWith(liveFec: [LiveFec("TC001", 10, 10)]);
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(ParseResult(fec: [Fec("TC001", null)]), FpsYear, 1, null);

        result.Errors.Should().NotContain(e => e.ValidationCode == "MISSING_FEC_NEW_RATE");
        result.RowCounts.FecUpdate.Should().Be(1);
        result.RowCounts.Invalid.Should().Be(0);
    }

    [Fact]
    public async Task ExistingFecRow_AlreadyZeroLiveRate_BlankUpload_ClassifiesAsNoChange_NotWithdrawal()
    {
        var repo = RepoWith(liveFec: [LiveFec("TC001", 0, 0)]);
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(ParseResult(fec: [Fec("TC001", null)]), FpsYear, 1, null);

        result.RowCounts.FecUnchanged.Should().Be(1);
        result.RowCounts.FecUpdate.Should().Be(0);
    }

    [Fact]
    public async Task NewFecRow_BlankRate_IsBlockingError()
    {
        var sut = CreateService(RepoWith());

        var result = await sut.ProcessUploadAsync(ParseResult(fec: [Fec("TC999", null)]), FpsYear, 1, null);

        result.Errors.Should().ContainSingle(e => e.ValidationCode == "MISSING_FEC_NEW_RATE" && e.Severity == "Error");
    }

    [Fact]
    public async Task ExistingFecRow_SameRateAsLive_ClassifiesAsNoChange()
    {
        var repo = RepoWith(liveFec: [LiveFec("TC001", 10, 10)]);
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(ParseResult(fec: [Fec("TC001", 10)]), FpsYear, 1, null);

        result.RowCounts.FecUnchanged.Should().Be(1);
    }

    [Fact]
    public async Task ExistingFecRow_DifferentRate_ClassifiesAsUpdate()
    {
        var repo = RepoWith(liveFec: [LiveFec("TC001", 10, 10)]);
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(ParseResult(fec: [Fec("TC001", 15)]), FpsYear, 1, null);

        result.RowCounts.FecUpdate.Should().Be(1);
    }

    [Fact]
    public async Task NewFecRow_ValidData_ClassifiesAsInsert()
    {
        var sut = CreateService(RepoWith());

        var result = await sut.ProcessUploadAsync(ParseResult(fec: [Fec("TC999", 5)]), FpsYear, 1, null);

        result.Errors.Should().BeEmpty();
        result.RowCounts.FecInsert.Should().Be(1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task NewFecRow_MissingRequiredFields_RaisesMissingForInsert(string? blank)
    {
        var sut = CreateService(RepoWith());

        var result = await sut.ProcessUploadAsync(
            ParseResult(fec: [Fec("TC999", 5, item: blank, shortDesc: blank, owner: blank)]), FpsYear, 1, null);

        result.Errors.Count(e => e.ValidationCode == "MISSING_FOR_INSERT").Should().Be(3);
    }

    [Fact]
    public async Task FecRow_NegativeRate_IsBlockingError_RegardlessOfNewOrExisting()
    {
        var repo = RepoWith(liveFec: [LiveFec("TC001", 10, 10)]);
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(ParseResult(fec: [Fec("TC001", -5)]), FpsYear, 1, null);

        result.Errors.Should().ContainSingle(e => e.ValidationCode == "NEGATIVE_RATE" && e.SheetName == "FEC");
        result.RowCounts.FecInsert.Should().Be(0);
        result.RowCounts.FecUpdate.Should().Be(0);
    }

    [Fact]
    public async Task DuplicateFecTestCode_RaisesError()
    {
        var sut = CreateService(RepoWith());

        var result = await sut.ProcessUploadAsync(
            ParseResult(fec: [Fec("TC001", 5), Fec("tc001", 6)]), FpsYear, 1, null);

        result.Errors.Count(e => e.ValidationCode == "DUPLICATE_TEST_CODE").Should().Be(2);
    }

    // ── AGRUP: existing-row blank/zero -> Zero-Rate Withdrawal ──────────────────

    [Fact]
    public async Task ExistingAgrupRow_BlankRate_ClassifiesAsZeroRateWithdrawal_NotUnchanged()
    {
        var repo = RepoWith(
            liveFec: [LiveFec("TC001", 10, 10)],
            liveAgrup: [LiveAgrup("TC001", "B001", 10)]);
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(
            ParseResult(fec: [Fec("TC001", 10)], agrup: [Agrup("TC001", "B001", null)]), FpsYear, 1, null);

        result.RowCounts.AgrupUpdate.Should().Be(1);
        result.RowCounts.AgrupUnchanged.Should().Be(0);
    }

    [Fact]
    public async Task NewAgrupRow_ZeroRate_IsBlocked_BC01()
    {
        var repo = RepoWith(liveFec: [LiveFec("TC001", 10, 10)], projectCodes: new HashSet<string> { "PRJ001" });
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(
            ParseResult(fec: [Fec("TC001", 10)], agrup: [Agrup("TC001", "B999", 0, projectBuyerCode: "PRJ001")]), FpsYear, 1, null);

        result.Errors.Should().ContainSingle(e => e.ValidationCode == "NEW_AGRUP_ZERO_RATE_BLOCKED");
    }

    [Fact]
    public async Task AgrupRow_UnknownTestCode_RaisesTestCodeNotFound()
    {
        var repo = RepoWith(projectCodes: new HashSet<string> { "PRJ001" });
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(
            ParseResult(agrup: [Agrup("UNKNOWN", "B001", 5, projectBuyerCode: "PRJ001")]), FpsYear, 1, null);

        result.Errors.Should().ContainSingle(e => e.ValidationCode == "TEST_CODE_NOT_FOUND");
    }

    [Fact]
    public async Task AgrupRow_TestCodeInSameUploadFecSheet_DoesNotRaiseTestCodeNotFound()
    {
        var repo = RepoWith(projectCodes: new HashSet<string> { "PRJ001" });
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(
            ParseResult(fec: [Fec("TC999", 5)], agrup: [Agrup("TC999", "B001", 5, projectBuyerCode: "PRJ001")]), FpsYear, 1, null);

        result.Errors.Should().NotContain(e => e.ValidationCode == "TEST_CODE_NOT_FOUND");
    }

    // ── AGRUP routing fields (BC-02) ─────────────────────────────────────────────

    [Fact]
    public async Task NewAgrupRow_NoRoutingFieldSupplied_RaisesMissingRoutingField()
    {
        var repo = RepoWith(liveFec: [LiveFec("TC001", 10, 10)]);
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(
            ParseResult(fec: [Fec("TC001", 10)], agrup: [Agrup("TC001", "B999", 5)]), FpsYear, 1, null);

        result.Errors.Should().ContainSingle(e => e.ValidationCode == "MISSING_ROUTING_FIELD");
    }

    [Fact]
    public async Task NewAgrupRow_InvalidProjectBuyerCode_RaisesError()
    {
        var repo = RepoWith(liveFec: [LiveFec("TC001", 10, 10)]);
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(
            ParseResult(fec: [Fec("TC001", 10)], agrup: [Agrup("TC001", "B999", 5, projectBuyerCode: "BOGUS")]), FpsYear, 1, null);

        result.Errors.Should().ContainSingle(e => e.ValidationCode == "INVALID_PROJECT_BUYER_CODE");
    }

    [Fact]
    public async Task NewAgrupRow_ValidProjectBuyerCode_NoRoutingError()
    {
        var repo = RepoWith(liveFec: [LiveFec("TC001", 10, 10)], projectCodes: new HashSet<string> { "PRJ001" });
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(
            ParseResult(fec: [Fec("TC001", 10)], agrup: [Agrup("TC001", "B999", 5, projectBuyerCode: "PRJ001")]), FpsYear, 1, null);

        result.Errors.Should().NotContain(e => e.ValidationCode == "MISSING_ROUTING_FIELD" || e.ValidationCode == "INVALID_PROJECT_BUYER_CODE");
    }

    [Fact]
    public async Task NewAgrupRow_InvalidTestBuyerWorkGroup_RaisesError()
    {
        var repo = RepoWith(liveFec: [LiveFec("TC001", 10, 10)]);
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(
            ParseResult(fec: [Fec("TC001", 10)], agrup: [Agrup("TC001", "B999", 5, testBuyerWorkGroup: "WG-BOGUS")]), FpsYear, 1, null);

        result.Errors.Should().ContainSingle(e => e.ValidationCode == "INVALID_TEST_BUYER_WORKGROUP");
    }

    [Fact]
    public async Task NewAgrupRow_ValidTestBuyerWorkGroup_NoRoutingError()
    {
        var repo = RepoWith(
            liveFec: [LiveFec("TC001", 10, 10)],
            capabilityPairs: new HashSet<(string, string)> { ("TC001", "WG1") });
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(
            ParseResult(fec: [Fec("TC001", 10)], agrup: [Agrup("TC001", "B999", 5, testBuyerWorkGroup: "WG1")]), FpsYear, 1, null);

        result.Errors.Should().NotContain(e => e.ValidationCode == "MISSING_ROUTING_FIELD" || e.ValidationCode == "INVALID_TEST_BUYER_WORKGROUP");
    }

    // ── AGRUP existing-row routing immutability ──────────────────────────────────

    [Fact]
    public async Task ExistingAgrupRow_ChangedProjectBuyerCode_RaisesRoutingFieldChanged()
    {
        var repo = RepoWith(
            liveFec: [LiveFec("TC001", 10, 10)],
            liveAgrup: [LiveAgrup("TC001", "B001", 5, projectBuyerCode: "PRJ001")]);
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(
            ParseResult(fec: [Fec("TC001", 10)], agrup: [Agrup("TC001", "B001", 5, projectBuyerCode: "PRJ002")]), FpsYear, 1, null);

        result.Errors.Should().ContainSingle(e => e.ValidationCode == "ROUTING_FIELD_CHANGED" && e.FieldName == "projectbuyercode");
    }

    [Fact]
    public async Task ExistingAgrupRow_SameProjectBuyerCode_DifferentCase_IsNotChanged_CitextSemantics()
    {
        var repo = RepoWith(
            liveFec: [LiveFec("TC001", 10, 10)],
            liveAgrup: [LiveAgrup("TC001", "B001", 5, projectBuyerCode: "PRJ001")]);
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(
            ParseResult(fec: [Fec("TC001", 10)], agrup: [Agrup("TC001", "B001", 5, projectBuyerCode: "prj001")]), FpsYear, 1, null);

        result.Errors.Should().NotContain(e => e.ValidationCode == "ROUTING_FIELD_CHANGED");
    }

    [Fact]
    public async Task ExistingAgrupRow_UnchangedRoutingFields_NoRoutingCapabilityRevalidation()
    {
        // Existing rows aren't re-checked against ProjectLookup/CapabilityLookup at all — only
        // new rows are (immutability, not re-validation): projectCodes deliberately left empty.
        var repo = RepoWith(
            liveFec: [LiveFec("TC001", 10, 10)],
            liveAgrup: [LiveAgrup("TC001", "B001", 5, projectBuyerCode: "PRJ001")]);
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(
            ParseResult(fec: [Fec("TC001", 10)], agrup: [Agrup("TC001", "B001", 5, projectBuyerCode: "PRJ001")]), FpsYear, 1, null);

        result.Errors.Should().NotContain(e => e.ValidationCode == "INVALID_PROJECT_BUYER_CODE" || e.ValidationCode == "MISSING_ROUTING_FIELD");
    }

    // ── FEC-withdrawal / AGRUP conflict (interim BC-05, staged variant) ──────────────

    [Fact]
    public async Task WithdrawnFecTestCode_StagedPositiveAgrupRow_RaisesConflictError()
    {
        var repo = RepoWith(
            liveFec: [LiveFec("TC001", 10, 10)],
            liveAgrup: [LiveAgrup("TC001", "B001", 5)]);
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(
            ParseResult(fec: [Fec("TC001", null)], agrup: [Agrup("TC001", "B001", 5)]), FpsYear, 1, null);

        result.Errors.Should().ContainSingle(e => e.ValidationCode == "AGRUP_POSITIVE_FOR_WITHDRAWN_FEC");
    }

    [Fact]
    public async Task WithdrawnFecTestCode_StagedZeroAgrupRow_NoConflictError()
    {
        var repo = RepoWith(
            liveFec: [LiveFec("TC001", 10, 10)],
            liveAgrup: [LiveAgrup("TC001", "B001", 5)]);
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(
            ParseResult(fec: [Fec("TC001", null)], agrup: [Agrup("TC001", "B001", 0)]), FpsYear, 1, null);

        result.Errors.Should().NotContain(e => e.ValidationCode == "AGRUP_POSITIVE_FOR_WITHDRAWN_FEC");
    }

    // ── Downloaded-snapshot preservation ──────────────────────────────────────────

    [Fact]
    public async Task MissingDownloadedFecKey_RaisesRequestLevelError()
    {
        var repo = RepoWith(snapshotFec: [LiveFec("TC001", null, 10)]);
        var sut = CreateService(repo);

        // TC001 was downloaded (snapshot) but not re-uploaded (fec: []).
        var result = await sut.ProcessUploadAsync(ParseResult(), FpsYear, 1, downloadVersion: 1);

        var finding = result.Errors.Should().ContainSingle(e => e.ValidationCode == "MISSING_DOWNLOADED_KEY" && e.SheetName == "FEC").Subject;
        finding.IsRequestLevel.Should().BeTrue();
        finding.SourceRowNumber.Should().Be(0);
    }

    [Fact]
    public async Task DownloadedKeyStillPresentInUpload_NoMissingKeyError()
    {
        var repo = RepoWith(
            liveFec: [LiveFec("TC001", 10, 10)],
            snapshotFec: [LiveFec("TC001", null, 10)]);
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(ParseResult(fec: [Fec("TC001", 12)]), FpsYear, 1, downloadVersion: 1);

        result.Errors.Should().NotContain(e => e.ValidationCode == "MISSING_DOWNLOADED_KEY");
    }

    [Fact]
    public async Task NoDownloadVersion_SkipsSnapshotPreservationCheck()
    {
        var sut = CreateService(RepoWith());

        var result = await sut.ProcessUploadAsync(ParseResult(), FpsYear, 1, downloadVersion: null);

        result.Errors.Should().NotContain(e => e.ValidationCode == "MISSING_DOWNLOADED_KEY");
    }

    // ── Parse-error staging quirk (§1c) — the critical regression trap ──────────────

    [Fact]
    public async Task ParseErrors_StillStagesParsedRows_AndSkipsBusinessValidation()
    {
        var repo = RepoWith();
        var sut = CreateService(repo);

        var result = await sut.ProcessUploadAsync(
            ParseResult(fec: [Fec("TC999", 5)], parseErrors: ["Sheet 'AGRUP' is missing."]),
            FpsYear, 1, null);

        result.Errors.Should().ContainSingle(e => e.ValidationCode == "FILE_ERROR" && e.SourceRowNumber == 0);
        result.Errors.Should().NotContain(e => e.ValidationCode == "MISSING_FOR_INSERT");
        await repo.Received(1).ReplaceStagingFecAsync(
            QueueId, Arg.Is<IReadOnlyList<TestOrProductStagingRow>>(l => l.Count == 1), Arg.Any<IReadOnlyList<TestRequirementStagingRow>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HappyPath_AlwaysReplacesStaging()
    {
        var repo = RepoWith();
        var sut = CreateService(repo);

        await sut.ProcessUploadAsync(ParseResult(fec: [Fec("TC999", 5)]), FpsYear, 1, null);

        await repo.Received(1).ReplaceStagingFecAsync(
            QueueId, Arg.Any<IReadOnlyList<TestOrProductStagingRow>>(), Arg.Any<IReadOnlyList<TestRequirementStagingRow>>(),
            Arg.Any<CancellationToken>());
    }

    // ── Release / freeze ─────────────────────────────────────────────────────────

    [Fact]
    public async Task PrepareForReleaseAsync_BlockingErrors_ThrowsAndDoesNotFreeze()
    {
        var repo = RepoWith();
        repo.GetTestOrProductStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns([Fec("TC999", null)]); // new row, blank rate -> MISSING_FEC_NEW_RATE
        var sut = CreateService(repo);

        var act = async () => await sut.PrepareForReleaseAsync(QueueId, FpsYear, 1, null);

        await act.Should().ThrowAsync<BusinessValidationErrorException>();
        await repo.DidNotReceive().FreezeStagingCalculatedActionsAsync(
            Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<IReadOnlyList<TestFreezeEntry>>(), Arg.Any<IReadOnlyList<TestFreezeEntry>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PrepareForReleaseAsync_NoBlockingErrors_FreezesFecAndAgrupClassifications()
    {
        var repo = RepoWith(liveFec: [LiveFec("TC001", 10, 10)]);
        repo.GetTestOrProductStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns([Fec("TC001", 15)]);
        repo.GetTestRequirementStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<TestRequirementStagingRow>());
        var sut = CreateService(repo);

        await sut.PrepareForReleaseAsync(QueueId, FpsYear, 1, null);

        await repo.Received(1).FreezeStagingCalculatedActionsAsync(
            QueueId, 1,
            Arg.Is<IReadOnlyList<TestFreezeEntry>>(l => l.Count == 1 && l[0].TestCode == "TC001" && l[0].CalculatedAction == "Update"),
            Arg.Any<IReadOnlyList<TestFreezeEntry>>(),
            Arg.Any<CancellationToken>());
    }

    // ── Export / download / staging ──────────────────────────────────────────────

    [Fact]
    public async Task ExportTestDataAsync_ExportsLiveFecAndAgrupRows()
    {
        var excel = Substitute.For<IExcelExportService>();
        var repo = RepoWith(
            liveFec: [LiveFec("TC001", 10, 10)],
            liveAgrup: [LiveAgrup("TC001", "B001", 5)]);
        var sut = CreateService(repo, excel);

        await sut.ExportTestDataAsync(FpsYear);

        excel.Received(1).ExportToExcelMultiSheet(Arg.Is<IEnumerable<ExcelSheetDefinition>>(
            sheets => sheets.Count() == 2 && sheets.Any(s => s.SheetName == "FEC") && sheets.Any(s => s.SheetName == "AGRUP")));
    }

    [Fact]
    public async Task DownloadTestDataAsync_CreatesSnapshotAndMarksReady()
    {
        var repo = RepoWith(liveFec: [LiveFec("TC001", 10, 10)]);
        repo.GetNextDownloadVersionAsync(QueueId, Arg.Any<CancellationToken>()).Returns(1);
        var entry = new BulkRatesQueueRow { JobQueueId = QueueId, FpsYear = FpsYear };
        var sut = CreateService(repo);

        await sut.DownloadTestDataAsync(entry);

        await repo.Received(1).CreateDownloadSnapshotAsync(QueueId, 1, Arg.Any<IReadOnlyList<TestOrProductStagingRow>>(), Arg.Any<IReadOnlyList<TestRequirementStagingRow>>(), Arg.Any<CancellationToken>());
        await repo.Received(1).MarkDownloadReadyAsync(QueueId, 1, Arg.Any<CancellationToken>());
        await repo.DidNotReceive().MarkDownloadFailedAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
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
    public async Task GetStagingDataAsync_UnstagedLiveTestCode_ShowsAsDeleted()
    {
        var repo = RepoWith(liveFec: [LiveFec("TC001", 10, 10), LiveFec("TC002", 20, 20)]);
        repo.GetTestOrProductStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns([new TestOrProductStagingRow { TestCode = "TC001", CalculatedAction = "NoChange" }]);
        repo.GetTestRequirementStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<TestRequirementStagingRow>());
        var entry = new BulkRatesQueueRow { JobQueueId = QueueId, FpsYear = FpsYear, Status = "Initiated" };
        var sut = CreateService(repo);

        var result = await sut.GetStagingDataAsync(entry);

        result.FecRows.Should().Contain(r => r.TestCode == "TC002" && r.Status == "Deleted");
        result.FecRows.Should().Contain(r => r.TestCode == "TC001" && r.Status == "No Change");
    }

    [Fact]
    public async Task GetStagingDataAsync_CompletedRequest_SkipsLiveFetch_NoDeletedRows()
    {
        var repo = RepoWith(liveFec: [LiveFec("TC002", 20, 20)]);
        repo.GetTestOrProductStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns([new TestOrProductStagingRow { TestCode = "TC001", CalculatedAction = "NoChange" }]);
        repo.GetTestRequirementStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<TestRequirementStagingRow>());
        var entry = new BulkRatesQueueRow { JobQueueId = QueueId, FpsYear = FpsYear, Status = "Completed" };
        var sut = CreateService(repo);

        var result = await sut.GetStagingDataAsync(entry);

        result.FecRows.Should().NotContain(r => r.Status == "Deleted");
        await repo.DidNotReceive().GetFecRowsForExportAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExportStagingDataAsync_ExportsStagedFecAndAgrupRows()
    {
        var excel = Substitute.For<IExcelExportService>();
        var repo = RepoWith();
        repo.GetTestOrProductStagingRowsAsync(QueueId, Arg.Any<CancellationToken>()).Returns([Fec("TC001", 5)]);
        repo.GetTestRequirementStagingRowsAsync(QueueId, Arg.Any<CancellationToken>()).Returns(Array.Empty<TestRequirementStagingRow>());
        var sut = CreateService(repo, excel);

        await sut.ExportStagingDataAsync(QueueId);

        excel.Received(1).ExportToExcelMultiSheet(Arg.Is<IEnumerable<ExcelSheetDefinition>>(
            sheets => sheets.Any(s => s.SheetName == "FEC")));
    }
}
