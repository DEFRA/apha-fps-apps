using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.DepartmentIncomeServiceTest
{
    public class DepartmentIncomeServiceTests
    {
        private const string TestProject = "AH0033";

        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsDepartmentIncomeApiClient _fpsDepartmentIncomeApiClient;
        private readonly DepartmentIncomeService _service;

        public DepartmentIncomeServiceTests()
        {
            _fpsClient                     = Substitute.For<IFpsApiClient>();
            _fpsDepartmentIncomeApiClient  = Substitute.For<IFpsDepartmentIncomeApiClient>();
            _fpsClient.FpsDepartmentIncome.Returns(_fpsDepartmentIncomeApiClient);
            _service = new DepartmentIncomeService(_fpsClient);
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private static ApiResponseDto<List<DepartmentIncomeTimeDto>> TimeSuccess() =>
            ApiResponseDto<List<DepartmentIncomeTimeDto>>.SuccessResponse(
                new List<DepartmentIncomeTimeDto>
                {
                    new() { Project = "PROJ1", Month = 1, TotalCost = 100m },
                    new() { Project = "PROJ2", Month = 2, TotalCost = 200m },
                });

        private static ApiResponseDto<List<DepartmentIncomeTestDto>> TestSuccess() =>
            ApiResponseDto<List<DepartmentIncomeTestDto>>.SuccessResponse(
                new List<DepartmentIncomeTestDto>
                {
                    new() { Project = "PROJ1", Month = 1, TotalCost = 50m },
                });

        private static ApiResponseDto<List<DepartmentIncomeAnimalDto>> AnimalSuccess() =>
            ApiResponseDto<List<DepartmentIncomeAnimalDto>>.SuccessResponse(
                new List<DepartmentIncomeAnimalDto>
                {
                    new() { Project = "PROJ1", Month = 1, TotalCost = 75m },
                });

        private static ApiResponseDto<List<DepartmentIncomeAdditionalDto>> AdditionalSuccess() =>
            ApiResponseDto<List<DepartmentIncomeAdditionalDto>>.SuccessResponse(
                new List<DepartmentIncomeAdditionalDto>
                {
                    new() { Project = "PROJ1", Month = 1, TotalCost = 25m },
                });

        private static ApiResponseDto<List<DepartmentIncomeTotalsDto>> TotalsSuccess() =>
            ApiResponseDto<List<DepartmentIncomeTotalsDto>>.SuccessResponse(
                new List<DepartmentIncomeTotalsDto>
                {
                    new() { Project = "PROJ1", TotalCosts = 250m },
                });

        private static ApiResponseDto<List<PeriodLookupDto>> PeriodsSuccess() =>
            ApiResponseDto<List<PeriodLookupDto>>.SuccessResponse(
                new List<PeriodLookupDto>
                {
                    new() { AccntsPeriod = 1, MonthName = "April", MonthNumber = 4 },
                    new() { AccntsPeriod = 2, MonthName = "May",   MonthNumber = 5 },
                });

        private static List<ApiErrorDto> Errors() =>
            new List<ApiErrorDto> { new() { Message = "API failure", Code = "ERROR" } };

        // ── GetTimeIncomeAsync ──────────────────────────────────────────────────

        #region GetTimeIncomeAsync

        [Fact]
        public async Task GetTimeIncomeAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = TimeSuccess();
            _fpsDepartmentIncomeApiClient.GetTimeIncomeAsync(TestProject, 1, 6).Returns(expected);

            // Act
            var result = await _service.GetTimeIncomeAsync(TestProject, 1, 6);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            await _fpsDepartmentIncomeApiClient.Received(1).GetTimeIncomeAsync(TestProject, 1, 6);
        }

        [Fact]
        public async Task GetTimeIncomeAsync_NullParams_DelegatesToApiClientWithNulls()
        {
            // Arrange
            var expected = TimeSuccess();
            _fpsDepartmentIncomeApiClient.GetTimeIncomeAsync(null, null, null).Returns(expected);

            // Act
            var result = await _service.GetTimeIncomeAsync(null, null, null);

            // Assert
            Assert.True(result.Success);
            await _fpsDepartmentIncomeApiClient.Received(1).GetTimeIncomeAsync(null, null, null);
        }

        [Fact]
        public async Task GetTimeIncomeAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var failure = ApiResponseDto<List<DepartmentIncomeTimeDto>>.FailureResponse(Errors(), new ApiMetaDto());
            _fpsDepartmentIncomeApiClient.GetTimeIncomeAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .Returns(failure);

            // Act
            var result = await _service.GetTimeIncomeAsync(TestProject, 1, 6);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors!);
        }

        [Fact]
        public async Task GetTimeIncomeAsync_ApiClientReturnsEmptyList_ReturnsSuccessWithEmpty()
        {
            // Arrange
            var empty = ApiResponseDto<List<DepartmentIncomeTimeDto>>.SuccessResponse(new List<DepartmentIncomeTimeDto>());
            _fpsDepartmentIncomeApiClient.GetTimeIncomeAsync(null, null, null).Returns(empty);

            // Act
            var result = await _service.GetTimeIncomeAsync(null, null, null);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        #endregion

        // ── GetTestIncomeAsync ──────────────────────────────────────────────────

        #region GetTestIncomeAsync

        [Fact]
        public async Task GetTestIncomeAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = TestSuccess();
            _fpsDepartmentIncomeApiClient.GetTestIncomeAsync(TestProject, 1, 12).Returns(expected);

            // Act
            var result = await _service.GetTestIncomeAsync(TestProject, 1, 12);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsDepartmentIncomeApiClient.Received(1).GetTestIncomeAsync(TestProject, 1, 12);
        }

        [Fact]
        public async Task GetTestIncomeAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var failure = ApiResponseDto<List<DepartmentIncomeTestDto>>.FailureResponse(Errors(), new ApiMetaDto());
            _fpsDepartmentIncomeApiClient.GetTestIncomeAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .Returns(failure);

            // Act
            var result = await _service.GetTestIncomeAsync(TestProject, 1, 12);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetTestIncomeAsync_NullParams_DelegatesToApiClientWithNulls()
        {
            // Arrange
            _fpsDepartmentIncomeApiClient.GetTestIncomeAsync(null, null, null)
                .Returns(ApiResponseDto<List<DepartmentIncomeTestDto>>.SuccessResponse(new List<DepartmentIncomeTestDto>()));

            // Act
            await _service.GetTestIncomeAsync(null, null, null);

            // Assert
            await _fpsDepartmentIncomeApiClient.Received(1).GetTestIncomeAsync(null, null, null);
        }

        #endregion

        // ── GetTestSnapshotIncomeAsync ──────────────────────────────────────────

        #region GetTestSnapshotIncomeAsync

        [Fact]
        public async Task GetTestSnapshotIncomeAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = TestSuccess();
            _fpsDepartmentIncomeApiClient.GetTestSnapshotIncomeAsync(TestProject, 1, 6).Returns(expected);

            // Act
            var result = await _service.GetTestSnapshotIncomeAsync(TestProject, 1, 6);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsDepartmentIncomeApiClient.Received(1).GetTestSnapshotIncomeAsync(TestProject, 1, 6);
        }

        [Fact]
        public async Task GetTestSnapshotIncomeAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var failure = ApiResponseDto<List<DepartmentIncomeTestDto>>.FailureResponse(Errors(), new ApiMetaDto());
            _fpsDepartmentIncomeApiClient
                .GetTestSnapshotIncomeAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .Returns(failure);

            // Act
            var result = await _service.GetTestSnapshotIncomeAsync(TestProject, 1, 6);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors!);
        }

        [Fact]
        public async Task GetTestSnapshotIncomeAsync_NullParams_DelegatesToApiClientWithNulls()
        {
            // Arrange
            _fpsDepartmentIncomeApiClient.GetTestSnapshotIncomeAsync(null, null, null)
                .Returns(ApiResponseDto<List<DepartmentIncomeTestDto>>.SuccessResponse(new List<DepartmentIncomeTestDto>()));

            // Act
            await _service.GetTestSnapshotIncomeAsync(null, null, null);

            // Assert
            await _fpsDepartmentIncomeApiClient.Received(1).GetTestSnapshotIncomeAsync(null, null, null);
        }

        [Fact]
        public async Task GetTestSnapshotIncomeAsync_ApiClientReturnsEmptyList_ReturnsSuccessWithEmpty()
        {
            // Arrange
            var empty = ApiResponseDto<List<DepartmentIncomeTestDto>>.SuccessResponse(new List<DepartmentIncomeTestDto>());
            _fpsDepartmentIncomeApiClient.GetTestSnapshotIncomeAsync(null, null, null).Returns(empty);

            // Act
            var result = await _service.GetTestSnapshotIncomeAsync(null, null, null);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        #endregion

        // ── GetAnimalIncomeAsync ────────────────────────────────────────────────

        #region GetAnimalIncomeAsync

        [Fact]
        public async Task GetAnimalIncomeAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = AnimalSuccess();
            _fpsDepartmentIncomeApiClient.GetAnimalIncomeAsync(TestProject, 1, 12).Returns(expected);

            // Act
            var result = await _service.GetAnimalIncomeAsync(TestProject, 1, 12);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsDepartmentIncomeApiClient.Received(1).GetAnimalIncomeAsync(TestProject, 1, 12);
        }

        [Fact]
        public async Task GetAnimalIncomeAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var failure = ApiResponseDto<List<DepartmentIncomeAnimalDto>>.FailureResponse(Errors(), new ApiMetaDto());
            _fpsDepartmentIncomeApiClient.GetAnimalIncomeAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .Returns(failure);

            // Act
            var result = await _service.GetAnimalIncomeAsync(TestProject, 1, 12);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAnimalIncomeAsync_NullParams_DelegatesToApiClientWithNulls()
        {
            // Arrange
            _fpsDepartmentIncomeApiClient.GetAnimalIncomeAsync(null, null, null)
                .Returns(ApiResponseDto<List<DepartmentIncomeAnimalDto>>.SuccessResponse(new List<DepartmentIncomeAnimalDto>()));

            // Act
            await _service.GetAnimalIncomeAsync(null, null, null);

            // Assert
            await _fpsDepartmentIncomeApiClient.Received(1).GetAnimalIncomeAsync(null, null, null);
        }

        #endregion

        // ── GetAdditionalIncomeAsync ────────────────────────────────────────────

        #region GetAdditionalIncomeAsync

        [Fact]
        public async Task GetAdditionalIncomeAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = AdditionalSuccess();
            _fpsDepartmentIncomeApiClient.GetAdditionalIncomeAsync(TestProject, 1, 12).Returns(expected);

            // Act
            var result = await _service.GetAdditionalIncomeAsync(TestProject, 1, 12);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsDepartmentIncomeApiClient.Received(1).GetAdditionalIncomeAsync(TestProject, 1, 12);
        }

        [Fact]
        public async Task GetAdditionalIncomeAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var failure = ApiResponseDto<List<DepartmentIncomeAdditionalDto>>.FailureResponse(Errors(), new ApiMetaDto());
            _fpsDepartmentIncomeApiClient.GetAdditionalIncomeAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .Returns(failure);

            // Act
            var result = await _service.GetAdditionalIncomeAsync(TestProject, 1, 12);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAdditionalIncomeAsync_NullParams_DelegatesToApiClientWithNulls()
        {
            // Arrange
            _fpsDepartmentIncomeApiClient.GetAdditionalIncomeAsync(null, null, null)
                .Returns(ApiResponseDto<List<DepartmentIncomeAdditionalDto>>.SuccessResponse(new List<DepartmentIncomeAdditionalDto>()));

            // Act
            await _service.GetAdditionalIncomeAsync(null, null, null);

            // Assert
            await _fpsDepartmentIncomeApiClient.Received(1).GetAdditionalIncomeAsync(null, null, null);
        }

        #endregion

        // ── GetTotalsAsync ──────────────────────────────────────────────────────

        #region GetTotalsAsync

        [Fact]
        public async Task GetTotalsAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = TotalsSuccess();
            _fpsDepartmentIncomeApiClient.GetTotalsAsync(TestProject, 1, 12).Returns(expected);

            // Act
            var result = await _service.GetTotalsAsync(TestProject, 1, 12);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsDepartmentIncomeApiClient.Received(1).GetTotalsAsync(TestProject, 1, 12);
        }

        [Fact]
        public async Task GetTotalsAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var failure = ApiResponseDto<List<DepartmentIncomeTotalsDto>>.FailureResponse(Errors(), new ApiMetaDto());
            _fpsDepartmentIncomeApiClient.GetTotalsAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .Returns(failure);

            // Act
            var result = await _service.GetTotalsAsync(TestProject, 1, 12);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetTotalsAsync_NullParams_DelegatesToApiClientWithNulls()
        {
            // Arrange
            _fpsDepartmentIncomeApiClient.GetTotalsAsync(null, null, null)
                .Returns(ApiResponseDto<List<DepartmentIncomeTotalsDto>>.SuccessResponse(new List<DepartmentIncomeTotalsDto>()));

            // Act
            await _service.GetTotalsAsync(null, null, null);

            // Assert
            await _fpsDepartmentIncomeApiClient.Received(1).GetTotalsAsync(null, null, null);
        }

        #endregion

        // ── GetPeriodsAsync ─────────────────────────────────────────────────────

        #region GetPeriodsAsync

        [Fact]
        public async Task GetPeriodsAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = PeriodsSuccess();
            _fpsDepartmentIncomeApiClient.GetPeriodsAsync().Returns(expected);

            // Act
            var result = await _service.GetPeriodsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            await _fpsDepartmentIncomeApiClient.Received(1).GetPeriodsAsync();
        }

        [Fact]
        public async Task GetPeriodsAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var failure = ApiResponseDto<List<PeriodLookupDto>>.FailureResponse(Errors(), new ApiMetaDto());
            _fpsDepartmentIncomeApiClient.GetPeriodsAsync().Returns(failure);

            // Act
            var result = await _service.GetPeriodsAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors!);
            await _fpsDepartmentIncomeApiClient.Received(1).GetPeriodsAsync();
        }

        [Fact]
        public async Task GetPeriodsAsync_ApiClientReturnsEmptyList_ReturnsSuccessWithEmpty()
        {
            // Arrange
            var empty = ApiResponseDto<List<PeriodLookupDto>>.SuccessResponse(new List<PeriodLookupDto>());
            _fpsDepartmentIncomeApiClient.GetPeriodsAsync().Returns(empty);

            // Act
            var result = await _service.GetPeriodsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        #endregion

        // ── Constructor Tests ────────────────────────────────────────────────────

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullFpsClient_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DepartmentIncomeService(null!));
        }

        #endregion

        // ── GetSnapshotPeriodsAsync ──────────────────────────────────────────────

        #region GetSnapshotPeriodsAsync

        [Fact]
        public async Task GetSnapshotPeriodsAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = ApiResponseDto<List<PeriodSnapshotDto>>.SuccessResponse(
                new List<PeriodSnapshotDto>
                {
                    new() { PeriodName = "April 2025 Only",  EndPeriod = 4, PeriodLocked = false },
                    new() { PeriodName = "April - May 2025", EndPeriod = 5, PeriodLocked = false },
                });
            _fpsDepartmentIncomeApiClient.GetSnapshotPeriodsAsync().Returns(expected);

            // Act
            var result = await _service.GetSnapshotPeriodsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            await _fpsDepartmentIncomeApiClient.Received(1).GetSnapshotPeriodsAsync();
        }

        [Fact]
        public async Task GetSnapshotPeriodsAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var failure = ApiResponseDto<List<PeriodSnapshotDto>>.FailureResponse(Errors(), new ApiMetaDto());
            _fpsDepartmentIncomeApiClient.GetSnapshotPeriodsAsync().Returns(failure);

            // Act
            var result = await _service.GetSnapshotPeriodsAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors!);
        }

        [Fact]
        public async Task GetSnapshotPeriodsAsync_ApiClientReturnsEmptyList_ReturnsSuccessWithEmpty()
        {
            // Arrange
            var empty = ApiResponseDto<List<PeriodSnapshotDto>>.SuccessResponse(new List<PeriodSnapshotDto>());
            _fpsDepartmentIncomeApiClient.GetSnapshotPeriodsAsync().Returns(empty);

            // Act
            var result = await _service.GetSnapshotPeriodsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        #endregion

        // ── UpdatePeriodLockedAsync ──────────────────────────────────────────────

        #region UpdatePeriodLockedAsync

        [Fact]
        public async Task UpdatePeriodLockedAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsDepartmentIncomeApiClient.UpdatePeriodLockedAsync("April 2025 Only", true).Returns(expected);

            // Act
            var result = await _service.UpdatePeriodLockedAsync("April 2025 Only", true);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsDepartmentIncomeApiClient.Received(1).UpdatePeriodLockedAsync("April 2025 Only", true);
        }

        [Fact]
        public async Task UpdatePeriodLockedAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var failure = ApiResponseDto<bool>.FailureResponse(Errors(), new ApiMetaDto());
            _fpsDepartmentIncomeApiClient.UpdatePeriodLockedAsync(Arg.Any<string>(), Arg.Any<bool>())
                .Returns(failure);

            // Act
            var result = await _service.UpdatePeriodLockedAsync("NonExistent", true);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors!);
        }

        [Fact]
        public async Task UpdatePeriodLockedAsync_PeriodNameWithSlash_DelegatesWithUnchangedName()
        {
            // Arrange
            const string slashPeriod = "April - August 2025/25";
            var expected = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsDepartmentIncomeApiClient.UpdatePeriodLockedAsync(slashPeriod, true).Returns(expected);

            // Act
            var result = await _service.UpdatePeriodLockedAsync(slashPeriod, true);

            // Assert
            Assert.True(result.Success);
            await _fpsDepartmentIncomeApiClient.Received(1).UpdatePeriodLockedAsync(slashPeriod, true);
        }

        #endregion

        // ── GetTimeIncomeCurrentAsync ────────────────────────────────────────────

        #region GetTimeIncomeCurrentAsync

        [Fact]
        public async Task GetTimeIncomeCurrentAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = TimeSuccess();
            _fpsDepartmentIncomeApiClient.GetTimeIncomeCurrentAsync(TestProject, 1, 6).Returns(expected);

            // Act
            var result = await _service.GetTimeIncomeCurrentAsync(TestProject, 1, 6);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            await _fpsDepartmentIncomeApiClient.Received(1).GetTimeIncomeCurrentAsync(TestProject, 1, 6);
        }

        [Fact]
        public async Task GetTimeIncomeCurrentAsync_NullParams_DelegatesToApiClientWithNulls()
        {
            // Arrange
            _fpsDepartmentIncomeApiClient.GetTimeIncomeCurrentAsync(null, null, null).Returns(TimeSuccess());

            // Act
            await _service.GetTimeIncomeCurrentAsync(null, null, null);

            // Assert
            await _fpsDepartmentIncomeApiClient.Received(1).GetTimeIncomeCurrentAsync(null, null, null);
        }

        [Fact]
        public async Task GetTimeIncomeCurrentAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var failure = ApiResponseDto<List<DepartmentIncomeTimeDto>>.FailureResponse(Errors(), new ApiMetaDto());
            _fpsDepartmentIncomeApiClient.GetTimeIncomeCurrentAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .Returns(failure);

            // Act
            var result = await _service.GetTimeIncomeCurrentAsync(TestProject, 1, 6);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── GetTestIncomeCurrentAsync ────────────────────────────────────────────

        #region GetTestIncomeCurrentAsync

        [Fact]
        public async Task GetTestIncomeCurrentAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = TestSuccess();
            _fpsDepartmentIncomeApiClient.GetTestIncomeCurrentAsync(TestProject, 1, 12).Returns(expected);

            // Act
            var result = await _service.GetTestIncomeCurrentAsync(TestProject, 1, 12);

            // Assert
            Assert.True(result.Success);
            await _fpsDepartmentIncomeApiClient.Received(1).GetTestIncomeCurrentAsync(TestProject, 1, 12);
        }

        [Fact]
        public async Task GetTestIncomeCurrentAsync_NullParams_DelegatesToApiClientWithNulls()
        {
            // Arrange
            _fpsDepartmentIncomeApiClient.GetTestIncomeCurrentAsync(null, null, null).Returns(TestSuccess());

            // Act
            await _service.GetTestIncomeCurrentAsync(null, null, null);

            // Assert
            await _fpsDepartmentIncomeApiClient.Received(1).GetTestIncomeCurrentAsync(null, null, null);
        }

        #endregion

        // ── GetAnimalIncomeCurrentAsync ──────────────────────────────────────────

        #region GetAnimalIncomeCurrentAsync

        [Fact]
        public async Task GetAnimalIncomeCurrentAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = AnimalSuccess();
            _fpsDepartmentIncomeApiClient.GetAnimalIncomeCurrentAsync(TestProject, 1, 12).Returns(expected);

            // Act
            var result = await _service.GetAnimalIncomeCurrentAsync(TestProject, 1, 12);

            // Assert
            Assert.True(result.Success);
            await _fpsDepartmentIncomeApiClient.Received(1).GetAnimalIncomeCurrentAsync(TestProject, 1, 12);
        }

        [Fact]
        public async Task GetAnimalIncomeCurrentAsync_NullParams_DelegatesToApiClientWithNulls()
        {
            // Arrange
            _fpsDepartmentIncomeApiClient.GetAnimalIncomeCurrentAsync(null, null, null).Returns(AnimalSuccess());

            // Act
            await _service.GetAnimalIncomeCurrentAsync(null, null, null);

            // Assert
            await _fpsDepartmentIncomeApiClient.Received(1).GetAnimalIncomeCurrentAsync(null, null, null);
        }

        #endregion

        // ── GetAdditionalIncomeCurrentAsync ─────────────────────────────────────

        #region GetAdditionalIncomeCurrentAsync

        [Fact]
        public async Task GetAdditionalIncomeCurrentAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = AdditionalSuccess();
            _fpsDepartmentIncomeApiClient.GetAdditionalIncomeCurrentAsync(TestProject, 1, 12).Returns(expected);

            // Act
            var result = await _service.GetAdditionalIncomeCurrentAsync(TestProject, 1, 12);

            // Assert
            Assert.True(result.Success);
            await _fpsDepartmentIncomeApiClient.Received(1).GetAdditionalIncomeCurrentAsync(TestProject, 1, 12);
        }

        [Fact]
        public async Task GetAdditionalIncomeCurrentAsync_NullParams_DelegatesToApiClientWithNulls()
        {
            // Arrange
            _fpsDepartmentIncomeApiClient.GetAdditionalIncomeCurrentAsync(null, null, null).Returns(AdditionalSuccess());

            // Act
            await _service.GetAdditionalIncomeCurrentAsync(null, null, null);

            // Assert
            await _fpsDepartmentIncomeApiClient.Received(1).GetAdditionalIncomeCurrentAsync(null, null, null);
        }

        #endregion

        // ── GetTotalsCurrentAsync ────────────────────────────────────────────────

        #region GetTotalsCurrentAsync

        [Fact]
        public async Task GetTotalsCurrentAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = TotalsSuccess();
            _fpsDepartmentIncomeApiClient.GetTotalsCurrentAsync(TestProject, 1, 12).Returns(expected);

            // Act
            var result = await _service.GetTotalsCurrentAsync(TestProject, 1, 12);

            // Assert
            Assert.True(result.Success);
            await _fpsDepartmentIncomeApiClient.Received(1).GetTotalsCurrentAsync(TestProject, 1, 12);
        }

        [Fact]
        public async Task GetTotalsCurrentAsync_NullParams_DelegatesToApiClientWithNulls()
        {
            // Arrange
            _fpsDepartmentIncomeApiClient.GetTotalsCurrentAsync(null, null, null).Returns(TotalsSuccess());

            // Act
            await _service.GetTotalsCurrentAsync(null, null, null);

            // Assert
            await _fpsDepartmentIncomeApiClient.Received(1).GetTotalsCurrentAsync(null, null, null);
        }

        [Fact]
        public async Task GetTotalsCurrentAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var failure = ApiResponseDto<List<DepartmentIncomeTotalsDto>>.FailureResponse(Errors(), new ApiMetaDto());
            _fpsDepartmentIncomeApiClient.GetTotalsCurrentAsync(Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                .Returns(failure);

            // Act
            var result = await _service.GetTotalsCurrentAsync(TestProject, 1, 12);

            // Assert
            Assert.False(result.Success);
        }

        #endregion
    }
}
