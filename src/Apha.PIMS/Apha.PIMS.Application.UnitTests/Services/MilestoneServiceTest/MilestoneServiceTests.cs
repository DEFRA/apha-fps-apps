using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Application.Services;
using Apha.PIMS.Application.Validation;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;

namespace Apha.PIMS.Application.UnitTests.Services.MilestoneServiceTest
{
    public class MilestoneServiceTests
    {
        private readonly IMilestoneRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly MilestoneService _sut;

        public MilestoneServiceTests()
        {
            _mockRepository = Substitute.For<IMilestoneRepository>();
            _mockMapper     = Substitute.For<IMapper>();
            _sut            = new MilestoneService(_mockRepository, _mockMapper);
        }

        /// <summary>Returns a <see cref="MilestoneDto"/> that passes all business-rule validation.</summary>
        private static MilestoneDto ValidMilestoneDto() => new()
        {
            Project = "PP001",
            Number  = "M1",
            IdType  = "D",
            DateDue = DateTime.Today.AddDays(30)
        };

        /// <summary>Returns a <see cref="MilestoneFormDatesDto"/> that passes all business-rule validation.</summary>
        private static MilestoneFormDatesDto ValidFormDatesDto() => new()
        {
            ParentProject = "PP001",
            Year          = 2024
        };

        #region GetAllMilestonesAsync

        [Fact]
        public async Task GetAllMilestonesAsync_WithValidData_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            const string project = "PP001";

            var milestones     = new List<Milestone> { new() { Project = project, Number = "M1" }, new() { Project = project, Number = "M2" } };
            var paginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var pagedData      = new PagedData<Milestone>(milestones, paginationData);

            var dtos         = new List<MilestoneDto> { new() { Project = project, Number = "M1", DateDue = DateTime.Today.AddDays(10) }, new() { Project = project, Number = "M2", DateDue = DateTime.Today.AddDays(20) } };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllMilestonesAsync(paginationParams, project).Returns(pagedData);
            _mockMapper.Map<List<MilestoneDto>>(pagedData.Data).Returns(dtos);
            _mockMapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            var result = await _sut.GetAllMilestonesAsync(query, project);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.First().Number.Should().Be("M1");
            result.PaginationData.TotalRecords.Should().Be(2);

            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetAllMilestonesAsync(paginationParams, project);
            _mockMapper.Received(1).Map<List<MilestoneDto>>(pagedData.Data);
            _mockMapper.Received(1).Map<PaginationDto>(pagedData.PaginationData);
        }

        [Fact]
        public async Task GetAllMilestonesAsync_WithEmptyData_ReturnsMappedEmptyResult()
        {
            // Arrange
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            const string project = "PP001";

            var pagedData     = new PagedData<Milestone>(new List<Milestone>(), new PaginationData { TotalRecords = 0 });
            var emptyDtos     = new List<MilestoneDto>();
            var paginationDto = new PaginationDto();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllMilestonesAsync(paginationParams, project).Returns(pagedData);
            _mockMapper.Map<List<MilestoneDto>>(pagedData.Data).Returns(emptyDtos);
            _mockMapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            var result = await _sut.GetAllMilestonesAsync(query, project);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
        }

        [Fact]
        public async Task GetAllMilestonesAsync_SetsIsLateTrue_WhenDateDueIsInPastAndDateCompletedIsNull()
        {
            // Arrange
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            const string project = "PP001";

            var pagedData     = new PagedData<Milestone>(new List<Milestone>(), new PaginationData());
            var paginationDto = new PaginationDto();
            var dtos          = new List<MilestoneDto>
            {
                new() { Project = project, Number = "M1", DateDue = DateTime.Today.AddDays(-1), DateCompleted = null, IsLate = false }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllMilestonesAsync(paginationParams, project).Returns(pagedData);
            _mockMapper.Map<List<MilestoneDto>>(pagedData.Data).Returns(dtos);
            _mockMapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            var result = await _sut.GetAllMilestonesAsync(query, project);

            // Assert
            result.Data.First().IsLate.Should().BeTrue();
        }

        [Fact]
        public async Task GetAllMilestonesAsync_IsLateIsFalse_WhenDateDueIsInFuture()
        {
            // Arrange
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            const string project = "PP001";

            var pagedData     = new PagedData<Milestone>(new List<Milestone>(), new PaginationData());
            var paginationDto = new PaginationDto();
            var dtos          = new List<MilestoneDto>
            {
                new() { Project = project, Number = "M1", DateDue = DateTime.Today.AddDays(10), DateCompleted = null, IsLate = false }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllMilestonesAsync(paginationParams, project).Returns(pagedData);
            _mockMapper.Map<List<MilestoneDto>>(pagedData.Data).Returns(dtos);
            _mockMapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            var result = await _sut.GetAllMilestonesAsync(query, project);

            // Assert
            result.Data.First().IsLate.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllMilestonesAsync_IsLateIsFalse_WhenDateCompletedIsSet()
        {
            // Arrange
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            const string project = "PP001";

            var pagedData     = new PagedData<Milestone>(new List<Milestone>(), new PaginationData());
            var paginationDto = new PaginationDto();
            var dtos          = new List<MilestoneDto>
            {
                new() { Project = project, Number = "M1", DateDue = DateTime.Today.AddDays(-1), DateCompleted = DateTime.Today.AddDays(-2), IsLate = false }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllMilestonesAsync(paginationParams, project).Returns(pagedData);
            _mockMapper.Map<List<MilestoneDto>>(pagedData.Data).Returns(dtos);
            _mockMapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            var result = await _sut.GetAllMilestonesAsync(query, project);

            // Assert
            result.Data.First().IsLate.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllMilestonesAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            const string project = "PP001";

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllMilestonesAsync(paginationParams, project)
                .Returns(Task.FromException<PagedData<Milestone>>(new Exception("DB error")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAllMilestonesAsync(query, project));

            exception.Message.Should().Be("DB error");
        }

        #endregion

        #region GetMilestoneAsync

        [Fact]
        public async Task GetMilestoneAsync_ReturnsMappedDto_WhenMilestoneExists()
        {
            // Arrange
            const string project = "PP001";
            const string number  = "M1";

            var entity = new Milestone { Project = project, Number = number, DateDue = DateTime.Today.AddDays(10) };
            var dto    = new MilestoneDto { Project = project, Number = number, DateDue = DateTime.Today.AddDays(10) };

            _mockRepository.GetMilestoneAsync(project, number).Returns(entity);
            _mockMapper.Map<MilestoneDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetMilestoneAsync(project, number);

            // Assert
            result.Should().NotBeNull();
            result!.Project.Should().Be(project);
            result.Number.Should().Be(number);

            await _mockRepository.Received(1).GetMilestoneAsync(project, number);
            _mockMapper.Received(1).Map<MilestoneDto>(entity);
        }

        [Fact]
        public async Task GetMilestoneAsync_ReturnsNull_WhenMilestoneNotFound()
        {
            // Arrange
            _mockRepository.GetMilestoneAsync("PP001", "UNKNOWN").Returns((Milestone?)null);

            // Act
            var result = await _sut.GetMilestoneAsync("PP001", "UNKNOWN");

            // Assert
            result.Should().BeNull();
            _mockMapper.DidNotReceive().Map<MilestoneDto>(Arg.Any<Milestone>());
        }

        [Fact]
        public async Task GetMilestoneAsync_SetsIsLateTrue_WhenDateDueIsInPastAndDateCompletedIsNull()
        {
            // Arrange
            var entity = new Milestone { Project = "PP001", Number = "M1", DateDue = DateTime.Today.AddDays(-1) };
            var dto    = new MilestoneDto { Project = "PP001", Number = "M1", DateDue = DateTime.Today.AddDays(-1), DateCompleted = null };

            _mockRepository.GetMilestoneAsync("PP001", "M1").Returns(entity);
            _mockMapper.Map<MilestoneDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetMilestoneAsync("PP001", "M1");

            // Assert
            result!.IsLate.Should().BeTrue();
        }

        [Fact]
        public async Task GetMilestoneAsync_SetsIsLateFalse_WhenDateCompletedIsSet()
        {
            // Arrange
            var entity = new Milestone { Project = "PP001", Number = "M1", DateDue = DateTime.Today.AddDays(-1) };
            var dto    = new MilestoneDto { Project = "PP001", Number = "M1", DateDue = DateTime.Today.AddDays(-1), DateCompleted = DateTime.Today.AddDays(-2) };

            _mockRepository.GetMilestoneAsync("PP001", "M1").Returns(entity);
            _mockMapper.Map<MilestoneDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetMilestoneAsync("PP001", "M1");

            // Assert
            result!.IsLate.Should().BeFalse();
        }

        [Fact]
        public async Task GetMilestoneAsync_SetsIsLateFalse_WhenDateDueIsInFuture()
        {
            // Arrange
            var entity = new Milestone { Project = "PP001", Number = "M1", DateDue = DateTime.Today.AddDays(10) };
            var dto    = new MilestoneDto { Project = "PP001", Number = "M1", DateDue = DateTime.Today.AddDays(10) };

            _mockRepository.GetMilestoneAsync("PP001", "M1").Returns(entity);
            _mockMapper.Map<MilestoneDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetMilestoneAsync("PP001", "M1");

            // Assert
            result!.IsLate.Should().BeFalse();
        }

        #endregion

        #region SaveMilestoneAsync — validation

        [Fact]
        public async Task SaveMilestoneAsync_ThrowsBusinessValidationError_WhenProjectIsEmpty()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            dto.Project = string.Empty;

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.SaveMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "PROJECT_REQUIRED");
        }

        [Fact]
        public async Task SaveMilestoneAsync_ThrowsBusinessValidationError_WhenNumberIsEmpty()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            dto.Number = string.Empty;

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.SaveMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "NUMBER_REQUIRED");
        }

        [Fact]
        public async Task SaveMilestoneAsync_ThrowsBusinessValidationError_WhenIdTypeIsEmpty()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            dto.IdType = string.Empty;

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.SaveMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "TYPE_REQUIRED");
        }

        [Fact]
        public async Task SaveMilestoneAsync_ThrowsBusinessValidationError_WhenDateDueIsDefault()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            dto.DateDue = default;

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.SaveMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "DATE_DUE_REQUIRED");
        }

        [Fact]
        public async Task SaveMilestoneAsync_ThrowsBusinessValidationError_WhenDateCompletedIsInFuture()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            dto.DateCompleted = DateTime.Today.AddDays(1);

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.SaveMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "DATE_COMPLETED_FUTURE");
        }

        [Fact]
        public async Task SaveMilestoneAsync_ThrowsBusinessValidationError_WhenOnTargetSetAndDateDueHasPassed()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            dto.DateDue  = DateTime.Today.AddDays(-1);
            dto.OnTarget = 1;

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.SaveMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "ON_TARGET_PAST_DUE");
        }

        [Fact]
        public async Task SaveMilestoneAsync_CollectsAllValidationErrors_WhenAllFieldsInvalid()
        {
            // Arrange — every validated field is invalid at once
            var dto = new MilestoneDto
            {
                Project       = string.Empty,
                Number        = string.Empty,
                IdType        = null,
                DateDue       = default,
                DateCompleted = DateTime.Today.AddDays(1)
            };

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.SaveMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().Contain(e => e.Code == "PROJECT_REQUIRED");
            ex.Errors.Should().Contain(e => e.Code == "NUMBER_REQUIRED");
            ex.Errors.Should().Contain(e => e.Code == "TYPE_REQUIRED");
            ex.Errors.Should().Contain(e => e.Code == "DATE_DUE_REQUIRED");
            ex.Errors.Should().Contain(e => e.Code == "DATE_COMPLETED_FUTURE");
        }

        [Fact]
        public async Task SaveMilestoneAsync_DoesNotCallRepository_WhenValidationFails()
        {
            // Arrange
            var dto = new MilestoneDto { Project = string.Empty, Number = "M1", IdType = "D", DateDue = DateTime.Today.AddDays(10) };

            // Act
            await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.SaveMilestoneAsync(dto));

            // Assert
            await _mockRepository.DidNotReceive().GetMilestoneAsync(Arg.Any<string>(), Arg.Any<string>());
            await _mockRepository.DidNotReceive().AddMilestoneAsync(Arg.Any<Milestone>());
        }

        [Fact]
        public async Task SaveMilestoneAsync_ThrowsNumberExists_WhenMilestoneAlreadyExists()
        {
            // Arrange
            var dto      = ValidMilestoneDto();
            var existing = new Milestone { Project = dto.Project, Number = dto.Number };

            _mockRepository.GetMilestoneAsync(dto.Project, dto.Number).Returns(existing);

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.SaveMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "NUMBER_EXISTS");
            await _mockRepository.DidNotReceive().AddMilestoneAsync(Arg.Any<Milestone>());
        }

        #endregion

        #region SaveMilestoneAsync — mutual exclusions (ApplyMutualExclusions)

        [Fact]
        public async Task SaveMilestoneAsync_WhenDateCompletedIsSet_ClearsOnTargetAndUnderSdReview()
        {
            // Arrange — ApplyMutualExclusions mutates dto in place, so we inspect dto after the call.
            var dto = ValidMilestoneDto();
            dto.DateCompleted = DateTime.Today.AddDays(-1);
            dto.OnTarget      = 1;
            dto.UnderSdReview = 1;

            _mockRepository.GetMilestoneAsync(dto.Project, dto.Number).Returns((Milestone?)null);
            _mockMapper.Map<Milestone>(Arg.Any<object>()).Returns(new Milestone());
            _mockRepository.AddMilestoneAsync(Arg.Any<Milestone>()).Returns(new Milestone());
            _mockMapper.Map<MilestoneDto>(Arg.Any<Milestone>()).Returns(new MilestoneDto { Project = "PP001", Number = "M1", DateDue = DateTime.Today.AddDays(30) });

            // Act
            await _sut.SaveMilestoneAsync(dto);

            // Assert
            dto.OnTarget.Should().Be(0);
            dto.UnderSdReview.Should().Be(0);
            dto.DateCompleted.Should().NotBeNull();
        }

        [Fact]
        public async Task SaveMilestoneAsync_WhenOnTargetIsSet_ClearsUnderSdReviewAndDateCompleted()
        {
            // Arrange — no DateCompleted so the first exclusion block does not fire first.
            var dto = ValidMilestoneDto();
            dto.OnTarget      = 1;
            dto.UnderSdReview = 1;
            dto.DateCompleted = null;

            _mockRepository.GetMilestoneAsync(dto.Project, dto.Number).Returns((Milestone?)null);
            _mockMapper.Map<Milestone>(Arg.Any<object>()).Returns(new Milestone());
            _mockRepository.AddMilestoneAsync(Arg.Any<Milestone>()).Returns(new Milestone());
            _mockMapper.Map<MilestoneDto>(Arg.Any<Milestone>()).Returns(new MilestoneDto { Project = "PP001", Number = "M1", DateDue = DateTime.Today.AddDays(30) });

            // Act
            await _sut.SaveMilestoneAsync(dto);

            // Assert
            dto.UnderSdReview.Should().Be(0);
            dto.DateCompleted.Should().BeNull();
        }

        [Fact]
        public async Task SaveMilestoneAsync_WhenUnderSdReviewIsSet_ClearsOnTargetAndDateCompleted()
        {
            // Arrange — OnTarget is 0 so the second exclusion block does not fire.
            var dto = ValidMilestoneDto();
            dto.UnderSdReview = 1;
            dto.OnTarget      = 0;
            dto.DateCompleted = null;

            _mockRepository.GetMilestoneAsync(dto.Project, dto.Number).Returns((Milestone?)null);
            _mockMapper.Map<Milestone>(Arg.Any<object>()).Returns(new Milestone());
            _mockRepository.AddMilestoneAsync(Arg.Any<Milestone>()).Returns(new Milestone());
            _mockMapper.Map<MilestoneDto>(Arg.Any<Milestone>()).Returns(new MilestoneDto { Project = "PP001", Number = "M1", DateDue = DateTime.Today.AddDays(30) });

            // Act
            await _sut.SaveMilestoneAsync(dto);

            // Assert
            dto.OnTarget.Should().Be(0);
            dto.DateCompleted.Should().BeNull();
        }

        #endregion

        #region SaveMilestoneAsync — happy path

        [Fact]
        public async Task SaveMilestoneAsync_CallsAddAndReturnsMappedDto_WhenValid()
        {
            // Arrange
            var dto       = ValidMilestoneDto();
            var entity    = new Milestone { Project = dto.Project, Number = dto.Number };
            var created   = new Milestone { Project = dto.Project, Number = dto.Number };
            var resultDto = new MilestoneDto { Project = dto.Project, Number = dto.Number };

            _mockRepository.GetMilestoneAsync(dto.Project, dto.Number).Returns((Milestone?)null);
            _mockMapper.Map<Milestone>(Arg.Any<object>()).Returns(entity);
            _mockRepository.AddMilestoneAsync(entity).Returns(created);
            _mockMapper.Map<MilestoneDto>(created).Returns(resultDto);

            // Act
            var result = await _sut.SaveMilestoneAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Project.Should().Be("PP001");
            result.Number.Should().Be("M1");
            await _mockRepository.Received(1).AddMilestoneAsync(entity);
        }

        #endregion

        #region UpdateMilestoneAsync — validation

        [Fact]
        public async Task UpdateMilestoneAsync_ThrowsBusinessValidationError_WhenProjectIsEmpty()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            dto.Project = string.Empty;

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.UpdateMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "PROJECT_REQUIRED");
        }

        [Fact]
        public async Task UpdateMilestoneAsync_ThrowsBusinessValidationError_WhenNumberIsEmpty()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            dto.Number = string.Empty;

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.UpdateMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "NUMBER_REQUIRED");
        }

        [Fact]
        public async Task UpdateMilestoneAsync_ThrowsBusinessValidationError_WhenIdTypeIsEmpty()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            dto.IdType = string.Empty;

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.UpdateMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "TYPE_REQUIRED");
        }

        [Fact]
        public async Task UpdateMilestoneAsync_ThrowsBusinessValidationError_WhenDateDueIsDefault()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            dto.DateDue = default;

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.UpdateMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "DATE_DUE_REQUIRED");
        }

        [Fact]
        public async Task UpdateMilestoneAsync_ThrowsBusinessValidationError_WhenDateCompletedIsInFuture()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            dto.DateCompleted = DateTime.Today.AddDays(1);

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.UpdateMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "DATE_COMPLETED_FUTURE");
        }

        [Fact]
        public async Task UpdateMilestoneAsync_ThrowsBusinessValidationError_WhenOnTargetSetAndDateDueHasPassed()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            dto.DateDue  = DateTime.Today.AddDays(-1);
            dto.OnTarget = 1;

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.UpdateMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "ON_TARGET_PAST_DUE");
        }

        [Fact]
        public async Task UpdateMilestoneAsync_DoesNotCallRepository_WhenValidationFails()
        {
            // Arrange
            var dto = new MilestoneDto { Project = string.Empty, Number = "M1", IdType = "D", DateDue = DateTime.Today.AddDays(10) };

            // Act
            await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.UpdateMilestoneAsync(dto));

            // Assert
            await _mockRepository.DidNotReceive().GetMilestoneAsync(Arg.Any<string>(), Arg.Any<string>());
            await _mockRepository.DidNotReceive().UpdateMilestoneAsync(Arg.Any<Milestone>());
        }

        [Fact]
        public async Task UpdateMilestoneAsync_ThrowsNotFound_WhenMilestoneDoesNotExist()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            _mockRepository.GetMilestoneAsync(dto.Project, dto.Number).Returns((Milestone?)null);

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.UpdateMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "NOT_FOUND");
            await _mockRepository.DidNotReceive().UpdateMilestoneAsync(Arg.Any<Milestone>());
        }

        #endregion

        #region UpdateMilestoneAsync — happy path

        [Fact]
        public async Task UpdateMilestoneAsync_CallsUpdateAndReturnsMappedDto_WhenValid()
        {
            // Arrange
            var dto       = ValidMilestoneDto();
            var existing  = new Milestone { Project = dto.Project, Number = dto.Number };
            var updated   = new Milestone { Project = dto.Project, Number = dto.Number };
            var resultDto = new MilestoneDto { Project = dto.Project, Number = dto.Number };

            _mockRepository.GetMilestoneAsync(dto.Project, dto.Number).Returns(existing);
            _mockRepository.UpdateMilestoneAsync(existing).Returns(updated);
            _mockMapper.Map<MilestoneDto>(updated).Returns(resultDto);

            // Act
            var result = await _sut.UpdateMilestoneAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Project.Should().Be("PP001");
            result.Number.Should().Be("M1");
            await _mockRepository.Received(1).UpdateMilestoneAsync(existing);
        }

        #endregion

        #region DeleteMilestoneAsync

        [Fact]
        public async Task DeleteMilestoneAsync_ReturnsTrue_WhenDeletedSuccessfully()
        {
            // Arrange
            _mockRepository.DeleteMilestoneAsync("PP001", "M1").Returns(true);

            // Act
            var result = await _sut.DeleteMilestoneAsync("PP001", "M1");

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteMilestoneAsync("PP001", "M1");
        }

        [Fact]
        public async Task DeleteMilestoneAsync_ReturnsFalse_WhenNotFound()
        {
            // Arrange
            _mockRepository.DeleteMilestoneAsync("PP001", "UNKNOWN").Returns(false);

            // Act
            var result = await _sut.DeleteMilestoneAsync("PP001", "UNKNOWN");

            // Assert
            result.Should().BeFalse();
            await _mockRepository.Received(1).DeleteMilestoneAsync("PP001", "UNKNOWN");
        }

        #endregion

        #region UpdateFormRequiredAsync

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task UpdateFormRequiredAsync_DelegatesToRepository_AndReturnsResult(bool formRequired)
        {
            // Arrange
            _mockRepository.UpdateFormRequiredAsync("PP001", formRequired).Returns(true);

            // Act
            var result = await _sut.UpdateFormRequiredAsync("PP001", formRequired);

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).UpdateFormRequiredAsync("PP001", formRequired);
        }

        #endregion

        #region GetMilestoneTypesAsync

        [Fact]
        public async Task GetMilestoneTypesAsync_ReturnsMappedDtoList_WhenNoFilterProvided()
        {
            // Arrange
            var types = new List<MilestoneType>
            {
                new() { IdType = 'A', Type = "Alpha", MilestoneDeliverable = 'D' },
                new() { IdType = 'B', Type = "Beta",  MilestoneDeliverable = 'M' }
            };
            var expectedDtos = new List<MilestoneTypeDto>
            {
                new() { IdType = 'A', Type = "Alpha", MilestoneDeliverable = 'D' },
                new() { IdType = 'B', Type = "Beta",  MilestoneDeliverable = 'M' }
            };

            _mockRepository.GetMilestoneTypesAsync(null).Returns(types);
            _mockMapper.Map<List<MilestoneTypeDto>>(types).Returns(expectedDtos);

            // Act
            var result = await _sut.GetMilestoneTypesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.First().IdType.Should().Be('A');

            await _mockRepository.Received(1).GetMilestoneTypesAsync(null);
            _mockMapper.Received(1).Map<List<MilestoneTypeDto>>(types);
        }

        [Fact]
        public async Task GetMilestoneTypesAsync_PassesFilterToRepository()
        {
            // Arrange
            const string filter = "M";
            var types = new List<MilestoneType>
            {
                new() { IdType = 'B', Type = "Beta", MilestoneDeliverable = 'M' }
            };
            var expectedDtos = new List<MilestoneTypeDto>
            {
                new() { IdType = 'B', Type = "Beta", MilestoneDeliverable = 'M' }
            };

            _mockRepository.GetMilestoneTypesAsync(filter).Returns(types);
            _mockMapper.Map<List<MilestoneTypeDto>>(types).Returns(expectedDtos);

            // Act
            var result = await _sut.GetMilestoneTypesAsync(filter);

            // Assert
            result.Should().ContainSingle(t => t.MilestoneDeliverable == 'M');
            await _mockRepository.Received(1).GetMilestoneTypesAsync(filter);
        }

        [Fact]
        public async Task GetMilestoneTypesAsync_ReturnsEmpty_WhenNoTypes()
        {
            // Arrange
            var emptyTypes = new List<MilestoneType>();
            var emptyDtos  = new List<MilestoneTypeDto>();

            _mockRepository.GetMilestoneTypesAsync(null).Returns(emptyTypes);
            _mockMapper.Map<List<MilestoneTypeDto>>(emptyTypes).Returns(emptyDtos);

            // Act
            var result = await _sut.GetMilestoneTypesAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetAllMilestoneFormDatesAsync

        [Fact]
        public async Task GetAllMilestoneFormDatesAsync_WithValidData_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            const string parent  = "PP001";

            var formDatesList  = new List<MilestoneFormDates> { new() { Year = 2024, ParentProject = parent }, new() { Year = 2023, ParentProject = parent } };
            var paginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var pagedData      = new PagedData<MilestoneFormDates>(formDatesList, paginationData);

            var dtos          = new List<MilestoneFormDatesDto> { new() { Year = 2024, ParentProject = parent }, new() { Year = 2023, ParentProject = parent } };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllMilestoneFormDatesAsync(paginationParams, parent).Returns(pagedData);
            _mockMapper.Map<List<MilestoneFormDatesDto>>(pagedData.Data).Returns(dtos);
            _mockMapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            var result = await _sut.GetAllMilestoneFormDatesAsync(query, parent);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.First().Year.Should().Be(2024);
            result.PaginationData.TotalRecords.Should().Be(2);

            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetAllMilestoneFormDatesAsync(paginationParams, parent);
            _mockMapper.Received(1).Map<List<MilestoneFormDatesDto>>(pagedData.Data);
            _mockMapper.Received(1).Map<PaginationDto>(pagedData.PaginationData);
        }

        [Fact]
        public async Task GetAllMilestoneFormDatesAsync_WithEmptyData_ReturnsEmptyResult()
        {
            // Arrange
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            const string parent  = "PP001";

            var pagedData     = new PagedData<MilestoneFormDates>(new List<MilestoneFormDates>(), new PaginationData { TotalRecords = 0 });
            var emptyDtos     = new List<MilestoneFormDatesDto>();
            var paginationDto = new PaginationDto();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllMilestoneFormDatesAsync(paginationParams, parent).Returns(pagedData);
            _mockMapper.Map<List<MilestoneFormDatesDto>>(pagedData.Data).Returns(emptyDtos);
            _mockMapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            var result = await _sut.GetAllMilestoneFormDatesAsync(query, parent);

            // Assert
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
        }

        [Fact]
        public async Task GetAllMilestoneFormDatesAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllMilestoneFormDatesAsync(paginationParams, "PP001")
                .Returns(Task.FromException<PagedData<MilestoneFormDates>>(new Exception("DB error")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAllMilestoneFormDatesAsync(query, "PP001"));

            exception.Message.Should().Be("DB error");
        }

        #endregion

        #region GetMilestoneFormDatesAsync

        [Fact]
        public async Task GetMilestoneFormDatesAsync_ReturnsMappedDto_WhenExists()
        {
            // Arrange
            const short year    = 2024;
            const string parent = "PP001";

            var entity = new MilestoneFormDates { Year = year, ParentProject = parent, Jan = new DateTime(2024, 1, 31) };
            var dto    = new MilestoneFormDatesDto { Year = year, ParentProject = parent, Jan = new DateTime(2024, 1, 31) };

            _mockRepository.GetMilestoneFormDatesAsync(year, parent).Returns(entity);
            _mockMapper.Map<MilestoneFormDatesDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetMilestoneFormDatesAsync(year, parent);

            // Assert
            result.Should().NotBeNull();
            result!.Year.Should().Be(year);
            result.ParentProject.Should().Be(parent);
            result.Jan.Should().Be(new DateTime(2024, 1, 31));

            await _mockRepository.Received(1).GetMilestoneFormDatesAsync(year, parent);
            _mockMapper.Received(1).Map<MilestoneFormDatesDto>(entity);
        }

        [Fact]
        public async Task GetMilestoneFormDatesAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            _mockRepository.GetMilestoneFormDatesAsync(2024, "PP001").Returns((MilestoneFormDates?)null);

            // Act
            var result = await _sut.GetMilestoneFormDatesAsync(2024, "PP001");

            // Assert
            result.Should().BeNull();
            _mockMapper.DidNotReceive().Map<MilestoneFormDatesDto>(Arg.Any<MilestoneFormDates>());
        }

        #endregion

        #region SaveMilestoneFormDatesAsync — validation

        [Fact]
        public async Task SaveMilestoneFormDatesAsync_ThrowsBusinessValidationError_WhenParentProjectIsEmpty()
        {
            // Arrange
            var dto = ValidFormDatesDto();
            dto.ParentProject = string.Empty;

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.SaveMilestoneFormDatesAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "PROJECT_REQUIRED");
        }

        [Fact]
        public async Task SaveMilestoneFormDatesAsync_ThrowsBusinessValidationError_WhenYearIsZero()
        {
            // Arrange
            var dto = ValidFormDatesDto();
            dto.Year = 0;

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.SaveMilestoneFormDatesAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "YEAR_REQUIRED");
        }

        [Fact]
        public async Task SaveMilestoneFormDatesAsync_CollectsAllValidationErrors()
        {
            // Arrange
            var dto = new MilestoneFormDatesDto { ParentProject = string.Empty, Year = 0 };

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.SaveMilestoneFormDatesAsync(dto));

            // Assert
            ex.Errors.Should().Contain(e => e.Code == "PROJECT_REQUIRED");
            ex.Errors.Should().Contain(e => e.Code == "YEAR_REQUIRED");
        }

        #endregion

        #region SaveMilestoneFormDatesAsync — add vs update

        [Fact]
        public async Task SaveMilestoneFormDatesAsync_CallsAdd_WhenNoExistingRecord()
        {
            // Arrange
            var dto       = ValidFormDatesDto();
            var entity    = new MilestoneFormDates { Year = dto.Year, ParentProject = dto.ParentProject };
            var created   = new MilestoneFormDates { Year = dto.Year, ParentProject = dto.ParentProject };
            var resultDto = new MilestoneFormDatesDto { Year = dto.Year, ParentProject = dto.ParentProject };

            _mockRepository.GetMilestoneFormDatesAsync(dto.Year, dto.ParentProject).Returns((MilestoneFormDates?)null);
            _mockMapper.Map<MilestoneFormDates>(dto).Returns(entity);
            _mockRepository.AddMilestoneFormDatesAsync(entity).Returns(created);
            _mockMapper.Map<MilestoneFormDatesDto>(created).Returns(resultDto);

            // Act
            var result = await _sut.SaveMilestoneFormDatesAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Year.Should().Be(dto.Year);
            result.ParentProject.Should().Be(dto.ParentProject);

            await _mockRepository.Received(1).AddMilestoneFormDatesAsync(entity);
            await _mockRepository.DidNotReceive().UpdateMilestoneFormDatesAsync(Arg.Any<MilestoneFormDates>());
        }

        [Fact]
        public async Task SaveMilestoneFormDatesAsync_CallsUpdate_WhenExistingRecordFound()
        {
            // Arrange
            var dto       = ValidFormDatesDto();
            var existing  = new MilestoneFormDates { Year = dto.Year, ParentProject = dto.ParentProject };
            var updated   = new MilestoneFormDates { Year = dto.Year, ParentProject = dto.ParentProject };
            var resultDto = new MilestoneFormDatesDto { Year = dto.Year, ParentProject = dto.ParentProject };

            _mockRepository.GetMilestoneFormDatesAsync(dto.Year, dto.ParentProject).Returns(existing);
            _mockRepository.UpdateMilestoneFormDatesAsync(existing).Returns(updated);
            _mockMapper.Map<MilestoneFormDatesDto>(updated).Returns(resultDto);

            // Act
            var result = await _sut.SaveMilestoneFormDatesAsync(dto);

            // Assert
            result.Should().NotBeNull();
            await _mockRepository.Received(1).UpdateMilestoneFormDatesAsync(existing);
            await _mockRepository.DidNotReceive().AddMilestoneFormDatesAsync(Arg.Any<MilestoneFormDates>());
        }

        #endregion

        #region DeleteMilestoneFormDatesAsync

        [Fact]
        public async Task DeleteMilestoneFormDatesAsync_ReturnsTrue_WhenDeletedSuccessfully()
        {
            // Arrange
            _mockRepository.DeleteMilestoneFormDatesAsync(2024, "PP001").Returns(true);

            // Act
            var result = await _sut.DeleteMilestoneFormDatesAsync(2024, "PP001");

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteMilestoneFormDatesAsync(2024, "PP001");
        }

        [Fact]
        public async Task DeleteMilestoneFormDatesAsync_ReturnsFalse_WhenNotFound()
        {
            // Arrange
            _mockRepository.DeleteMilestoneFormDatesAsync(9999, "PP001").Returns(false);

            // Act
            var result = await _sut.DeleteMilestoneFormDatesAsync(9999, "PP001");

            // Assert
            result.Should().BeFalse();
            await _mockRepository.Received(1).DeleteMilestoneFormDatesAsync(9999, "PP001");
        }

        #endregion
    }
}
