using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Services;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;


namespace Apha.FPS.Application.UnitTests.Services.FpsSettingServiceTest
{
    public class FpsSettingServiceTests
    {
        private readonly IFpsSettingRepository _mockRepository;
        private readonly IYearEndStagingRepository _mockYearEndStagingRepository;
        private readonly IMapper _mockMapper;
        private readonly FpsSettingService _sut;

        public FpsSettingServiceTests()
        {
            _mockRepository = Substitute.For<IFpsSettingRepository>();
            _mockYearEndStagingRepository = Substitute.For<IYearEndStagingRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new FpsSettingService(_mockRepository, _mockYearEndStagingRepository, _mockMapper);

        }

        #region GetAllSettingsAsync

        [Fact]
        public async Task GetAllSettingsAsync_WhenMultipleSettingsExist_ReturnsAllSettingsMappedToDtos()
        {
            // Arrange
            var updatedAt = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);
            var settings = new List<FpsSetting>
            {
                new FpsSetting { Id = "1", Setting = "MaxFPS", Notes = "Maximum FPS limit", UpdatedBy = "user1", UpdatedAt = updatedAt, FpsYear = 2024 },
                new FpsSetting { Id = "2", Setting = "MinFPS", Notes = "Minimum FPS limit", UpdatedBy = "user2", UpdatedAt = updatedAt, FpsYear = 2023 },
                new FpsSetting { Id = "3", Setting = "AvgFPS", Notes = "Average FPS target", UpdatedBy = "user3", UpdatedAt = updatedAt, FpsYear = 2024 }
            };
            _mockRepository.GetAllAsync().Returns(settings);

            // Act
            var result = await _sut.GetAllSettingsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);

            result[0].Id.Should().Be("1");
            result[0].Setting.Should().Be("MaxFPS");
            result[0].Notes.Should().Be("Maximum FPS limit");
            result[0].UpdatedBy.Should().Be("user1");
            result[0].UpdatedAt.Should().Be(updatedAt);
            result[0].FpsYear.Should().Be(2024);

            result[1].Id.Should().Be("2");
            result[1].Setting.Should().Be("MinFPS");

            result[2].Id.Should().Be("3");
            result[2].Setting.Should().Be("AvgFPS");

            await _mockRepository.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAllSettingsAsync_WhenNoSettingsExist_ReturnsEmptyList()
        {
            // Arrange
            var emptySettings = new List<FpsSetting>();
            _mockRepository.GetAllAsync().Returns(emptySettings);

            // Act
            var result = await _sut.GetAllSettingsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            result.Should().HaveCount(0);

            await _mockRepository.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAllSettingsAsync_WhenSingleSettingExists_ReturnsSingleDtoInList()
        {
            // Arrange
            var updatedAt = new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc);
            var settings = new List<FpsSetting>
            {
                new FpsSetting { Id = "1", Setting = "DefaultFPS", Notes = "Default setting", UpdatedBy = "admin", UpdatedAt = updatedAt, FpsYear = 2024 }
            };
            _mockRepository.GetAllAsync().Returns(settings);

            // Act
            var result = await _sut.GetAllSettingsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Id.Should().Be("1");
            result[0].Setting.Should().Be("DefaultFPS");
            result[0].Notes.Should().Be("Default setting");
            result[0].UpdatedBy.Should().Be("admin");
            result[0].UpdatedAt.Should().Be(updatedAt);
            result[0].FpsYear.Should().Be(2024);

            await _mockRepository.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAllSettingsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.GetAllAsync().Throws(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetAllSettingsAsync());
            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetAllAsync();
        }

        #endregion

        #region GetHoursPerDayAsync

        [Fact]
        public async Task GetHoursPerDayAsync_WhenSettingExistsWithValidValue_ReturnsParsedDecimal()
        {
            // Arrange
            var setting = new FpsSetting { Id = "HoursInDay", Setting = "7.5", FpsYear = 2024 };
            _mockRepository.GetByKeyAsync("HoursInDay").Returns(setting);

            // Act
            var result = await _sut.GetHoursPerDayAsync();

            // Assert
            result.Should().Be(7.5m);
            await _mockRepository.Received(1).GetByKeyAsync("HoursInDay");
        }

        [Fact]
        public async Task GetHoursPerDayAsync_WhenSettingExistsWithIntegerValue_ReturnsParsedDecimal()
        {
            // Arrange
            var setting = new FpsSetting { Id = "HoursInDay", Setting = "8", FpsYear = 2024 };
            _mockRepository.GetByKeyAsync("HoursInDay").Returns(setting);

            // Act
            var result = await _sut.GetHoursPerDayAsync();

            // Assert
            result.Should().Be(8m);
            await _mockRepository.Received(1).GetByKeyAsync("HoursInDay");
        }

        [Fact]
        public async Task GetHoursPerDayAsync_WhenSettingDoesNotExist_ReturnsDefaultEight()
        {
            // Arrange
            _mockRepository.GetByKeyAsync("HoursInDay").Returns((FpsSetting?)null);

            // Act
            var result = await _sut.GetHoursPerDayAsync();

            // Assert
            result.Should().Be(8m);
            await _mockRepository.Received(1).GetByKeyAsync("HoursInDay");
        }

        [Fact]
        public async Task GetHoursPerDayAsync_WhenSettingValueIsNotNumeric_ReturnsDefaultEight()
        {
            // Arrange
            var setting = new FpsSetting { Id = "HoursInDay", Setting = "not-a-number", FpsYear = 2024 };
            _mockRepository.GetByKeyAsync("HoursInDay").Returns(setting);

            // Act
            var result = await _sut.GetHoursPerDayAsync();

            // Assert
            result.Should().Be(8m);
            await _mockRepository.Received(1).GetByKeyAsync("HoursInDay");
        }

        [Fact]
        public async Task GetHoursPerDayAsync_WhenSettingValueIsNull_ReturnsDefaultEight()
        {
            // Arrange
            var setting = new FpsSetting { Id = "HoursInDay", Setting = null, FpsYear = 2024 };
            _mockRepository.GetByKeyAsync("HoursInDay").Returns(setting);

            // Act
            var result = await _sut.GetHoursPerDayAsync();

            // Assert
            result.Should().Be(8m);
            await _mockRepository.Received(1).GetByKeyAsync("HoursInDay");
        }

        [Fact]
        public async Task GetHoursPerDayAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.GetByKeyAsync("HoursInDay").Throws(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetHoursPerDayAsync());
            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetByKeyAsync("HoursInDay");
        }

        #endregion

        // -----------------------------------------------------------------------
        // GetYearEndSettingsAsync
        // -----------------------------------------------------------------------

        #region GetYearEndSettingsAsync

        [Fact]
        public async Task GetYearEndSettingsAsync_WhenSettingsExist_ReturnsMappedDtos()
        {
            // Arrange
            var entities = new List<YearEndFpsSetting>
            {
                new YearEndFpsSetting { Id = "HoursInDay", Setting = "8", ExistsForPlannedYear = "Yes", FpsYear = 2025 },
                new YearEndFpsSetting { Id = "CapApprovalReceivedForReset", Setting = "yes", ExistsForPlannedYear = "Yes", FpsYear = 2025 }
            };
            var expectedDtos = new List<YearEndFpsSettingDto>
            {
                new YearEndFpsSettingDto { Id = "HoursInDay", Setting = "8", ExistsForPlannedYear = "Yes", FpsYear = 2025 },
                new YearEndFpsSettingDto { Id = "CapApprovalReceivedForReset", Setting = "yes", ExistsForPlannedYear = "Yes", FpsYear = 2025 }
            };

            _mockRepository.GetYearEndSettingsAsync().Returns(entities);
            _mockMapper.Map<List<YearEndFpsSettingDto>>(entities).Returns(expectedDtos);

            // Act
            var result = await _sut.GetYearEndSettingsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Id.Should().Be("HoursInDay");
            result[1].Id.Should().Be("CapApprovalReceivedForReset");

            await _mockRepository.Received(1).GetYearEndSettingsAsync();
            _mockMapper.Received(1).Map<List<YearEndFpsSettingDto>>(entities);
        }

        [Fact]
        public async Task GetYearEndSettingsAsync_WhenNoSettingsExist_ReturnsEmptyList()
        {
            // Arrange
            var empty = new List<YearEndFpsSetting>();
            _mockRepository.GetYearEndSettingsAsync().Returns(empty);
            _mockMapper.Map<List<YearEndFpsSettingDto>>(empty).Returns(new List<YearEndFpsSettingDto>());

            // Act
            var result = await _sut.GetYearEndSettingsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetYearEndSettingsAsync();
        }

        [Fact]
        public async Task GetYearEndSettingsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.GetYearEndSettingsAsync().Throws(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetYearEndSettingsAsync());
            exception.Message.Should().Be("Database error");

            await _mockRepository.Received(1).GetYearEndSettingsAsync();
        }

        #endregion

        // -----------------------------------------------------------------------
        // AddSettingAsync
        // -----------------------------------------------------------------------

        #region AddSettingAsync

        [Fact]
        public async Task AddSettingAsync_WhenDtoIsValid_ReturnsAddedDto()
        {
            // Arrange
            var dto = new FpsSettingDto { Id = "NewKey", Setting = "10", Notes = "Note", FpsYear = 2025 };
            var entity = new FpsSetting { Id = "NewKey", Setting = "10", Notes = "Note", FpsYear = 2025 };
            var resultEntity = new FpsSetting { Id = "NewKey", Setting = "10", Notes = "Note", FpsYear = 2025 };
            var expectedDto = new FpsSettingDto { Id = "NewKey", Setting = "10", Notes = "Note", FpsYear = 2025 };

            _mockMapper.Map<FpsSetting>(dto).Returns(entity);
            _mockRepository.AddAsync(entity).Returns(resultEntity);
            _mockMapper.Map<FpsSettingDto>(resultEntity).Returns(expectedDto);

            // Act
            var result = await _sut.AddSettingAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be("NewKey");
            result.Setting.Should().Be("10");

            await _mockRepository.Received(1).AddAsync(entity);
            _mockMapper.Received(1).Map<FpsSetting>(dto);
            _mockMapper.Received(1).Map<FpsSettingDto>(resultEntity);
        }

        [Fact]
        public async Task AddSettingAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var dto = new FpsSettingDto { Id = "NewKey", Setting = "10" };
            var entity = new FpsSetting { Id = "NewKey", Setting = "10" };
            _mockMapper.Map<FpsSetting>(dto).Returns(entity);
            _mockRepository.AddAsync(entity).Throws(new Exception("Insert failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.AddSettingAsync(dto));
            exception.Message.Should().Be("Insert failed");

            await _mockRepository.Received(1).AddAsync(entity);
        }

        #endregion

        // -----------------------------------------------------------------------
        // UpdateSettingAsync
        // -----------------------------------------------------------------------

        #region UpdateSettingAsync

        [Fact]
        public async Task UpdateSettingAsync_WhenDtoIsValid_ReturnsUpdatedDto()
        {
            // Arrange
            var dto = new FpsSettingDto { Id = "HoursInDay", Setting = "9", Notes = "Updated", FpsYear = 2025 };
            var entity = new FpsSetting { Id = "HoursInDay", Setting = "9", Notes = "Updated", FpsYear = 2025 };
            var resultEntity = new FpsSetting { Id = "HoursInDay", Setting = "9", Notes = "Updated", FpsYear = 2025 };
            var expectedDto = new FpsSettingDto { Id = "HoursInDay", Setting = "9", Notes = "Updated", FpsYear = 2025 };

            _mockMapper.Map<FpsSetting>(dto).Returns(entity);
            _mockRepository.UpdateAsync(entity).Returns(resultEntity);
            _mockMapper.Map<FpsSettingDto>(resultEntity).Returns(expectedDto);

            // Act
            var result = await _sut.UpdateSettingAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be("HoursInDay");
            result.Setting.Should().Be("9");

            await _mockRepository.Received(1).UpdateAsync(entity);
            _mockMapper.Received(1).Map<FpsSetting>(dto);
            _mockMapper.Received(1).Map<FpsSettingDto>(resultEntity);
        }

        [Fact]
        public async Task UpdateSettingAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var dto = new FpsSettingDto { Id = "HoursInDay", Setting = "9" };
            var entity = new FpsSetting { Id = "HoursInDay", Setting = "9" };
            _mockMapper.Map<FpsSetting>(dto).Returns(entity);
            _mockRepository.UpdateAsync(entity).Throws(new Exception("Update failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.UpdateSettingAsync(dto));
            exception.Message.Should().Be("Update failed");

            await _mockRepository.Received(1).UpdateAsync(entity);
        }

        #endregion

        // -----------------------------------------------------------------------
        // SaveSettingAsync — validation
        // -----------------------------------------------------------------------

        #region SaveSettingAsync — value validation (unchanged by the staging design — runs before resolve)

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("abc")]
        [InlineData("-5")]
        [InlineData("0")]
        public async Task SaveSettingAsync_WhenHoursInDayIsInvalid_ThrowsBusinessValidationError(string? value)
        {
            // Arrange
            var dto = new FpsSettingDto { Id = "HoursInDay", Setting = value };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.SaveSettingAsync(Guid.NewGuid(), dto));
            ex.Errors.Should().ContainSingle(e => e.Code == "Missing_HoursInDay");

            // Fails before even resolving the request — no staging touched.
            await _mockYearEndStagingRepository.DidNotReceive().ResolveRequestAsync(Arg.Any<Guid>());
        }

        [Theory]
        [InlineData("maybe")]
        [InlineData("true")]
        [InlineData("1")]
        public async Task SaveSettingAsync_WhenCapApprovalValueIsInvalid_ThrowsBusinessValidationError(string value)
        {
            // Arrange
            var dto = new FpsSettingDto { Id = "CapApprovalReceivedForReset", Setting = value };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.SaveSettingAsync(Guid.NewGuid(), dto));
            ex.Errors.Should().ContainSingle(e => e.Code == "Missing_CapApprovalReceivedForReset");

            await _mockYearEndStagingRepository.DidNotReceive().ResolveRequestAsync(Arg.Any<Guid>());
        }

        #endregion

        #region SaveSettingAsync — planned-year staging (Confirm)

        private static YearEndRequestSummary InitiatedRequest(Guid? jobQueueId = null, int? targetFpsYear = null)
            => new(jobQueueId ?? Guid.NewGuid(), 2025, targetFpsYear ?? 2026, "Initiated");

        [Fact]
        public async Task SaveSettingAsync_WhenJobExecutionIdDoesNotResolve_ThrowsKeyNotFoundException()
        {
            // Arrange — value-valid, but the request itself doesn't exist.
            var jobExecutionId = Guid.NewGuid();
            var dto = new FpsSettingDto { Id = "HoursInDay", Setting = "8" };
            _mockYearEndStagingRepository.ResolveRequestAsync(jobExecutionId).Returns((YearEndRequestSummary?)null);

            // Act & Assert — never falls back to "whichever request is currently active".
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.SaveSettingAsync(jobExecutionId, dto));

            await _mockYearEndStagingRepository.DidNotReceive().UpsertStagedSettingAsync(Arg.Any<YearEndSettingStaging>());
        }

        [Theory]
        [InlineData("Approved")]
        [InlineData("Running")]
        [InlineData("Completed")]
        [InlineData("Failed")]
        [InlineData("Rejected")]
        public async Task SaveSettingAsync_WhenRequestIsNotInitiated_ThrowsBusinessValidationError(string status)
        {
            // Arrange — staging is immutable once Approve succeeds and stays so through every
            // terminal/in-flight status.
            var jobExecutionId = Guid.NewGuid();
            var dto = new FpsSettingDto { Id = "HoursInDay", Setting = "8" };
            _mockYearEndStagingRepository.ResolveRequestAsync(jobExecutionId)
                .Returns(new YearEndRequestSummary(Guid.NewGuid(), 2025, 2026, status));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.SaveSettingAsync(jobExecutionId, dto));
            ex.Errors.Should().ContainSingle(e => e.Code == "REQUEST_NOT_EDITABLE");

            await _mockYearEndStagingRepository.DidNotReceive().UpsertStagedSettingAsync(Arg.Any<YearEndSettingStaging>());
        }

        [Fact]
        public async Task SaveSettingAsync_WhenInitiated_UpsertsStagedRow_NeverWritesRealTable()
        {
            // Arrange
            var jobExecutionId = Guid.NewGuid();
            var jobQueueId = Guid.NewGuid();
            var dto = new FpsSettingDto { Id = "HoursInDay", Setting = "7.5", Notes = "confirmed" };
            _mockYearEndStagingRepository.ResolveRequestAsync(jobExecutionId)
                .Returns(InitiatedRequest(jobQueueId, targetFpsYear: 2026));

            // Act
            var result = await _sut.SaveSettingAsync(jobExecutionId, dto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be("HoursInDay");
            result.Setting.Should().Be("7.5");
            result.FpsYear.Should().Be(2026); // displayed year is the request's target, not the open year

            await _mockYearEndStagingRepository.Received(1).UpsertStagedSettingAsync(
                Arg.Is<YearEndSettingStaging>(s =>
                    s.JobQueueId == jobQueueId && s.Id == "HoursInDay" && s.Setting == "7.5" && s.Notes == "confirmed"));

            // The real table is never touched by Confirm under the staging design.
            await _mockRepository.DidNotReceive().SaveAsync(Arg.Any<FpsSetting>());
        }

        #endregion
    }
}

