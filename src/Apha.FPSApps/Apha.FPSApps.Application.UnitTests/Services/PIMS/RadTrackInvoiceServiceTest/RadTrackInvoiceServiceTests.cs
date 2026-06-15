// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — RadTrackInvoiceServiceTests.cs (FPSApps Application)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - New file: xUnit tests for frontend RadTrackInvoiceService (Phase 8).
 *   - Service is a thin delegate — every method forwards to _client.PimsRadTrackInvoice.
 *   - Tests verify correct delegation: success responses, failure responses, parameter passing.
 *   - NSubstitute used for IPimsApiClient and IPimsRadTrackInvoiceApiClient mocks.
 *   - Follows MilestoneServiceTests (FPSApps) conventions: OneError helper, #region grouping.
 *
 * PRESERVED:
 *   - Naming convention: [MethodName]_[StateUnderTest]_[ExpectedResult].
 *   - _pimsApiClient.PimsRadTrackInvoice returns _pimsRadTrackInvoiceApiClient (NSubstitute .Returns).
 *   - All 6 method delegations tested.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify _client.PimsRadTrackInvoice resolves correctly after
 *     PimsApiClient.cs (Phase 9) is registered in DI.
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PIMS;
using NSubstitute;

namespace Apha.FPSApps.Application.UnitTests.Services.PIMS.RadTrackInvoiceServiceTest
{
    public class RadTrackInvoiceServiceTests
    {
        private readonly IPimsApiClient                    _pimsApiClient;
        private readonly IPimsRadTrackInvoiceApiClient     _pimsRadTrackInvoiceApiClient;
        private readonly RadTrackInvoiceService            _sut;

        public RadTrackInvoiceServiceTests()
        {
            _pimsApiClient               = Substitute.For<IPimsApiClient>();
            _pimsRadTrackInvoiceApiClient = Substitute.For<IPimsRadTrackInvoiceApiClient>();
            _pimsApiClient.PimsRadTrackInvoice.Returns(_pimsRadTrackInvoiceApiClient);
            _sut = new RadTrackInvoiceService(_pimsApiClient);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static List<ApiErrorDto> OneError(string message = "Error", string code = "ERR")
            => [new ApiErrorDto { Message = message, Code = code }];

        private static RadTrackInvoiceDto SampleDto(int id = 1)
            => new() { InvoiceCounter = id, Project = "PP001", DueAmount = 1000.0, DueDate = DateTime.Today };

        // ── GetAllAsync ────────────────────────────────────────────────────────

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_WithSuccessResponse_ReturnsInvoiceList()
        {
            // Arrange
            var query  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var data   = new List<RadTrackInvoiceDto> { SampleDto(1), SampleDto(2) };
            var expected = ApiResponseDto<List<RadTrackInvoiceDto>>.SuccessResponse(data);

            _pimsRadTrackInvoiceApiClient
                .GetAllAsync(query, null, null, null, null)
                .Returns(expected);

            // Act
            var result = await _sut.GetAllAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _pimsRadTrackInvoiceApiClient.Received(1).GetAllAsync(query, null, null, null, null);
        }

        [Fact]
        public async Task GetAllAsync_WithFilterParameters_PassesCorrectParametersToClient()
        {
            // Arrange
            var query    = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string project  = "PP001";
            const string contract = "C001";
            const int    year     = 2025;
            const string program  = "PROG1";

            var expected = ApiResponseDto<List<RadTrackInvoiceDto>>.SuccessResponse([]);
            _pimsRadTrackInvoiceApiClient
                .GetAllAsync(query, project, contract, year, program)
                .Returns(expected);

            // Act
            await _sut.GetAllAsync(query, project, contract, year, program);

            // Assert
            await _pimsRadTrackInvoiceApiClient.Received(1)
                .GetAllAsync(
                    Arg.Is<QueryParameters<string>>(q => q.Page == 1 && q.PageSize == 10),
                    project, contract, year, program);
        }

        [Fact]
        public async Task GetAllAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query    = new QueryParameters<string>();
            var expected = ApiResponseDto<List<RadTrackInvoiceDto>>.FailureResponse(
                OneError("Not found", "NOT_FOUND"), new ApiMetaDto());

            _pimsRadTrackInvoiceApiClient
                .GetAllAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<string?>())
                .Returns(expected);

            // Act
            var result = await _sut.GetAllAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("NOT_FOUND", result.Errors[0].Code);
        }

        #endregion

        // ── GetTotalsAsync ─────────────────────────────────────────────────────

        #region GetTotalsAsync

        [Fact]
        public async Task GetTotalsAsync_WithSuccessResponse_ReturnsTotals()
        {
            // Arrange
            var totals = new RadTrackInvoiceTotalsDto { TotalPlannedAmount = 5000, TotalDueAmount = 3000, TotalActualAmount = 2000 };
            var expected = ApiResponseDto<RadTrackInvoiceTotalsDto>.SuccessResponse(totals);

            _pimsRadTrackInvoiceApiClient
                .GetTotalsAsync(null, null, null, null)
                .Returns(expected);

            // Act
            var result = await _sut.GetTotalsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(5000.0, result.Data!.TotalPlannedAmount);
            await _pimsRadTrackInvoiceApiClient.Received(1).GetTotalsAsync(null, null, null, null);
        }

        [Fact]
        public async Task GetTotalsAsync_WithFilterParameters_PassesCorrectParametersToClient()
        {
            // Arrange
            const string project  = "PP001";
            const string contract = "C001";
            const int    year     = 2025;
            const string program  = "PROG1";

            var expected = ApiResponseDto<RadTrackInvoiceTotalsDto>.SuccessResponse(new RadTrackInvoiceTotalsDto());
            _pimsRadTrackInvoiceApiClient
                .GetTotalsAsync(project, contract, year, program)
                .Returns(expected);

            // Act
            await _sut.GetTotalsAsync(project, contract, year, program);

            // Assert
            await _pimsRadTrackInvoiceApiClient.Received(1).GetTotalsAsync(project, contract, year, program);
        }

        [Fact]
        public async Task GetTotalsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var expected = ApiResponseDto<RadTrackInvoiceTotalsDto>.FailureResponse(
                OneError("Totals failed", "TOTALS_ERR"), new ApiMetaDto());

            _pimsRadTrackInvoiceApiClient
                .GetTotalsAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<string?>())
                .Returns(expected);

            // Act
            var result = await _sut.GetTotalsAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("TOTALS_ERR", result.Errors[0].Code);
        }

        #endregion

        // ── GetByIdAsync ───────────────────────────────────────────────────────

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_WithSuccessResponse_ReturnsInvoice()
        {
            // Arrange
            const int id  = 42;
            var data      = SampleDto(id);
            var expected  = ApiResponseDto<RadTrackInvoiceDto>.SuccessResponse(data);

            _pimsRadTrackInvoiceApiClient.GetByIdAsync(id).Returns(expected);

            // Act
            var result = await _sut.GetByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(id, result.Data!.InvoiceCounter);
            await _pimsRadTrackInvoiceApiClient.Received(1).GetByIdAsync(id);
        }

        [Fact]
        public async Task GetByIdAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const int id = 999;
            var expected = ApiResponseDto<RadTrackInvoiceDto>.FailureResponse(
                OneError("Not found", "NOT_FOUND"), new ApiMetaDto());

            _pimsRadTrackInvoiceApiClient.GetByIdAsync(id).Returns(expected);

            // Act
            var result = await _sut.GetByIdAsync(id);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("NOT_FOUND", result.Errors[0].Code);
        }

        #endregion

        // ── CreateAsync ────────────────────────────────────────────────────────

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_WithSuccessResponse_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var dto      = SampleDto(0);
            var created  = SampleDto(10);
            var expected = ApiResponseDto<RadTrackInvoiceDto>.SuccessResponse(created);

            _pimsRadTrackInvoiceApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(10, result.Data!.InvoiceCounter);
            await _pimsRadTrackInvoiceApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var dto      = SampleDto(0);
            var expected = ApiResponseDto<RadTrackInvoiceDto>.FailureResponse(
                OneError("Validation error", "VALIDATION_ERR"), new ApiMetaDto());

            _pimsRadTrackInvoiceApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("VALIDATION_ERR", result.Errors[0].Code);
        }

        [Fact]
        public async Task CreateAsync_NullDto_DelegatesToClientWithNull()
        {
            // Arrange
            var expected = ApiResponseDto<RadTrackInvoiceDto>.FailureResponse(
                OneError(), new ApiMetaDto());
            _pimsRadTrackInvoiceApiClient.CreateAsync(null!).Returns(expected);

            // Act
            var result = await _sut.CreateAsync(null!);

            // Assert
            await _pimsRadTrackInvoiceApiClient.Received(1).CreateAsync(null!);
        }

        #endregion

        // ── UpdateAsync ────────────────────────────────────────────────────────

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_WithSuccessResponse_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            const int id = 7;
            var dto      = SampleDto(id);
            var updated  = SampleDto(id);
            var expected = ApiResponseDto<RadTrackInvoiceDto>.SuccessResponse(updated);

            _pimsRadTrackInvoiceApiClient.UpdateAsync(id, dto).Returns(expected);

            // Act
            var result = await _sut.UpdateAsync(id, dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(id, result.Data!.InvoiceCounter);
            await _pimsRadTrackInvoiceApiClient.Received(1).UpdateAsync(id, dto);
        }

        [Fact]
        public async Task UpdateAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const int id = 7;
            var dto      = SampleDto(id);
            var expected = ApiResponseDto<RadTrackInvoiceDto>.FailureResponse(
                OneError("Not found", "NOT_FOUND"), new ApiMetaDto());

            _pimsRadTrackInvoiceApiClient.UpdateAsync(id, dto).Returns(expected);

            // Act
            var result = await _sut.UpdateAsync(id, dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.Errors![0].Code);
        }

        #endregion

        // ── DeleteAsync ────────────────────────────────────────────────────────

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_WithSuccessResponse_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            const int id = 3;
            var expected = ApiResponseDto<object>.SuccessResponse(new { success = true });

            _pimsRadTrackInvoiceApiClient.DeleteAsync(id).Returns(expected);

            // Act
            var result = await _sut.DeleteAsync(id);

            // Assert
            Assert.True(result.Success);
            await _pimsRadTrackInvoiceApiClient.Received(1).DeleteAsync(id);
        }

        [Fact]
        public async Task DeleteAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const int id = 999;
            var expected = ApiResponseDto<object>.FailureResponse(
                OneError("Not found", "NOT_FOUND"), new ApiMetaDto());

            _pimsRadTrackInvoiceApiClient.DeleteAsync(id).Returns(expected);

            // Act
            var result = await _sut.DeleteAsync(id);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.Errors![0].Code);
        }

        #endregion
    }
}
