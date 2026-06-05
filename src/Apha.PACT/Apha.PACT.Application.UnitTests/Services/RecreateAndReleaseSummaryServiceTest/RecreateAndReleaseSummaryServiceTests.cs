using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Services;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;
using NSubstitute;

namespace Apha.PACT.Application.UnitTests.Services.RecreateAndReleaseSummaryServiceTest
{
    public class RecreateAndReleaseSummaryServiceTests
    {
        private readonly IRecreateAndReleaseSummaryRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly RecreateAndReleaseSummaryService _service;

        private const string TestUserId = "TestUser1";
        private const string TestUserName = "Test User";
        private const short TestPeriod = 1;

        public RecreateAndReleaseSummaryServiceTests()
        {
            _mockRepository = Substitute.For<IRecreateAndReleaseSummaryRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _service = new RecreateAndReleaseSummaryService(_mockRepository, _mockMapper);
        }

        #region GetRecreateSummariesLogsAsync

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_WithExistingLogs_ReturnsPaginatedDtos()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "datedone",
                Descending = true
            };

            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: "datedone", descending: true);

            var user = new User 
            { 
                UserName = TestUserName, 
                Comments = "Test Comment"
            };

            var entities = new List<RecreateSummaryLogWithComment>
            {
                new() { Id = 1, UserId = TestUserId, Comments = TestUserName, Period = TestPeriod, DateDone = DateTime.UtcNow },
                new() { Id = 2, UserId = TestUserId, Comments = TestUserName, Period = 2, DateDone = DateTime.UtcNow.AddDays(-1) }
            };

            var paginationData = new PaginationData
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 2
            };

            var pagedData = new PagedData<RecreateSummaryLogWithComment>(entities.AsReadOnly(), paginationData);

            var dtos = new List<RecreateSummaryLogDto>
            {
                new() { Id = 1, UserId = TestUserId, Comments = TestUserName, Period = TestPeriod, DateDone = DateTime.UtcNow },
                new() { Id = 2, UserId = TestUserId, Comments = TestUserName, Period = 2, DateDone = DateTime.UtcNow.AddDays(-1) }
            };

            var paginationDto = new PaginationDto
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 2
            };

            var paginatedResult = new PaginatedResult<RecreateSummaryLogDto>(dtos, paginationDto);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetRecreateSummaryLogAsync(parameters).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<RecreateSummaryLogDto>>(pagedData).Returns(paginatedResult);

            // Act
            var result = await _service.GetRecreateSummaryLogAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(1, result.Data.First().Id);
            Assert.Equal(2, result.Data.Last().Id);
            Assert.Equal(1, result.PaginationData.PageNumber);
            Assert.Equal(10, result.PaginationData.PageSize);
            Assert.Equal(1, result.PaginationData.TotalPages);
            Assert.Equal(2, result.PaginationData.TotalRecords);

            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetRecreateSummaryLogAsync(parameters);
            _mockMapper.Received(1).Map<PaginatedResult<RecreateSummaryLogDto>>(pagedData);
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_WithNoLogs_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10);

            var emptyEntities = new List<RecreateSummaryLogWithComment>();
            var paginationData = new PaginationData
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 0,
                TotalRecords = 0
            };

            var pagedData = new PagedData<RecreateSummaryLogWithComment>(emptyEntities.AsReadOnly(), paginationData);

            var emptyDtos = new List<RecreateSummaryLogDto>();
            var paginationDto = new PaginationDto
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 0,
                TotalRecords = 0
            };

            var paginatedResult = new PaginatedResult<RecreateSummaryLogDto>(emptyDtos, paginationDto);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetRecreateSummaryLogAsync(parameters).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<RecreateSummaryLogDto>>(pagedData).Returns(paginatedResult);

            // Act
            var result = await _service.GetRecreateSummaryLogAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
            await _mockRepository.Received(1).GetRecreateSummaryLogAsync(parameters);
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetRecreateSummaryLogAsync(parameters)
                .Returns(Task.FromException<PagedData<RecreateSummaryLogWithComment>>(new InvalidOperationException("Database error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetRecreateSummaryLogAsync(query));
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_MapperThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            _mockMapper.When(m => m.Map<PaginationParameters<string>>(query))
                .Do(_ => throw new InvalidOperationException("Mapper error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetRecreateSummaryLogAsync(query));
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 2,
                PageSize = 5,
                SortBy = "period",
                Descending = false
            };

            var parameters = new PaginationParameters<string>(page: 2, pageSize: 5, sortBy: "period", descending: false);

            var user = new User 
            { 
                UserName = TestUserName, 
                Comments = "Test Comment"
            };

            var entities = Enumerable.Range(6, 5)
                .Select(i => new RecreateSummaryLogWithComment
                {
                    Id = i,
                    UserId = TestUserId,
                    Comments = TestUserName,
                    Period = (short)i,
                    DateDone = DateTime.UtcNow.AddDays(-i)
                })
                .ToList();

            var paginationData = new PaginationData
            {
                PageNumber = 2,
                PageSize = 5,
                TotalPages = 4,
                TotalRecords = 20
            };

            var pagedData = new PagedData<RecreateSummaryLogWithComment>(entities.AsReadOnly(), paginationData);

            var dtos = entities.Select(e => new RecreateSummaryLogDto
            {
                Id = e.Id,
                UserId = e.UserId,
                Comments = TestUserName,
                Period = e.Period,
                DateDone = e.DateDone
            }).ToList();

            var paginationDto = new PaginationDto
            {
                PageNumber = 2,
                PageSize = 5,
                TotalPages = 4,
                TotalRecords = 20
            };

            var paginatedResult = new PaginatedResult<RecreateSummaryLogDto>(dtos, paginationDto);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetRecreateSummaryLogAsync(parameters).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<RecreateSummaryLogDto>>(pagedData).Returns(paginatedResult);

            // Act
            var result = await _service.GetRecreateSummaryLogAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Data.Count());
            Assert.Equal(2, result.PaginationData.PageNumber);
            Assert.Equal(5, result.PaginationData.PageSize);
            Assert.Equal(4, result.PaginationData.TotalPages);
            Assert.Equal(20, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetRecreateSummariesLogsAsync_WithSortParameters_PassesParametersToRepository()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "userid",
                Descending = false
            };

            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10, sortBy: "userid", descending: false);

            var userA = new User 
            { 
                UserName = "User A", 
                Comments = "Comment A"
            };
            var userB = new User 
            { 
                UserName = "User B", 
                Comments = "Comment B"
            };

            var entities = new List<RecreateSummaryLogWithComment>
            {
                new() { Id = 1, UserId = "UserA", Comments = TestUserName, Period = 1, DateDone = DateTime.UtcNow },
                new() { Id = 2, UserId = "UserB", Comments = TestUserName, Period = 2, DateDone = DateTime.UtcNow }
            };

            var paginationData = new PaginationData
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 2
            };

            var pagedData = new PagedData<RecreateSummaryLogWithComment>(entities.AsReadOnly(), paginationData);

            var dtos = new List<RecreateSummaryLogDto>
            {
                new() { Id = 1, UserId = "UserA", Comments = "User A", Period = 1, DateDone = DateTime.UtcNow },
                new() { Id = 2, UserId = "UserB", Comments = "User B", Period = 2, DateDone = DateTime.UtcNow }
            };

            var paginationDto = new PaginationDto
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 2
            };

            var paginatedResult = new PaginatedResult<RecreateSummaryLogDto>(dtos, paginationDto);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetRecreateSummaryLogAsync(parameters).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<RecreateSummaryLogDto>>(pagedData).Returns(paginatedResult);

            // Act
            var result = await _service.GetRecreateSummaryLogAsync(query);

            // Assert
            Assert.NotNull(result);
            await _mockRepository.Received(1).GetRecreateSummaryLogAsync(
                Arg.Is<PaginationParameters<string>>(p =>
                    p.Page == 1 &&
                    p.PageSize == 10 &&
                    p.SortBy == "userid" &&
                    p.Descending == false
                )
            );
        }

        #endregion

        #region GetReleaseSummariesAsync

        [Fact]
        public async Task GetReleaseSummariesAsync_WithExistingPeriods_ReturnsMappedDtos()
        {
            // Arrange
            var entities = new List<ReleasePeriod>
            {
                new() { PeriodName = "Period1", PeriodType = "Month", StartPeriod = 0.5, EndPeriod = 1.0, FinalSummariesRun = 0, PeriodLocked = 0 },
                new() { PeriodName = "Period2", PeriodType = "Month", StartPeriod = 1.5, EndPeriod = 2.0, FinalSummariesRun = 1, PeriodLocked = 0 }
            };

            var dtos = new List<ReleasePeriodDto>
            {
                new() { PeriodName = "Period1", PeriodType = "Month", StartPeriod = 0.5, EndPeriod = 1.0, FinalSummariesRun = 0, PeriodLocked = 0 },
                new() { PeriodName = "Period2", PeriodType = "Month", StartPeriod = 1.5, EndPeriod = 2.0, FinalSummariesRun = 1, PeriodLocked = 0 }
            };

            var releaseSummary = new ReleaseSummary { ReleasePeriods = entities };
            var expectedDto = new ReleaseSummaryDto { ReleasePeriods = dtos.AsReadOnly() };

            _mockRepository.GetReleaseSummariesAsync().Returns(releaseSummary);
            _mockMapper.Map<ReleaseSummaryDto>(Arg.Any<ReleaseSummary>()).Returns(expectedDto);

            // Act
            var result = await _service.GetReleaseSummariesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.ReleasePeriods.Count);
            Assert.Equal("Period1", result.ReleasePeriods[0].PeriodName);
            Assert.Equal("Period2", result.ReleasePeriods[1].PeriodName);

            await _mockRepository.Received(1).GetReleaseSummariesAsync();
            _mockMapper.Received(1).Map<ReleaseSummaryDto>(Arg.Any<ReleaseSummary>());
        }

        [Fact]
        public async Task GetReleaseSummariesAsync_WithNoPeriods_ReturnsEmptyList()
        {
            // Arrange
            var emptyEntities = new List<ReleasePeriod>();
            var releaseSummary = new ReleaseSummary { ReleasePeriods = emptyEntities };
            var expectedDto = new ReleaseSummaryDto { ReleasePeriods = new List<ReleasePeriodDto>().AsReadOnly() };

            _mockRepository.GetReleaseSummariesAsync().Returns(releaseSummary);
            _mockMapper.Map<ReleaseSummaryDto>(Arg.Any<ReleaseSummary>()).Returns(expectedDto);

            // Act
            var result = await _service.GetReleaseSummariesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.ReleasePeriods);

            await _mockRepository.Received(1).GetReleaseSummariesAsync();
            _mockMapper.Received(1).Map<ReleaseSummaryDto>(Arg.Any<ReleaseSummary>());
        }

        [Fact]
        public async Task GetReleaseSummariesAsync_MapsAllFieldsCorrectly()
        {
            // Arrange
            var entities = new List<ReleasePeriod>
            {
                new()
                {
                    PeriodName = "P1",
                    PeriodType = "Quarter",
                    StartPeriod = 1.0,
                    EndPeriod = 3.0,
                    FinalSummariesRun = 2,
                    PeriodLocked = 1
                }
            };

            var expectedDto = new ReleasePeriodDto
            {
                PeriodName = "P1",
                PeriodType = "Quarter",
                StartPeriod = 1.0,
                EndPeriod = 3.0,
                FinalSummariesRun = 2,
                PeriodLocked = 1
            };

            var dtos = new List<ReleasePeriodDto> { expectedDto }.AsReadOnly();

            var releaseSummary = new ReleaseSummary { ReleasePeriods = entities };
            var summaryDto = new ReleaseSummaryDto { ReleasePeriods = dtos };

            _mockRepository.GetReleaseSummariesAsync().Returns(releaseSummary);
            _mockMapper.Map<ReleaseSummaryDto>(Arg.Any<ReleaseSummary>()).Returns(summaryDto);

            // Act
            var result = await _service.GetReleaseSummariesAsync();

            // Assert
            Assert.Single(result.ReleasePeriods);
            var dto = result.ReleasePeriods[0];
            Assert.Equal("P1", dto.PeriodName);
            Assert.Equal("Quarter", dto.PeriodType);
            Assert.Equal(1.0, dto.StartPeriod);
            Assert.Equal(3.0, dto.EndPeriod);
            Assert.Equal((short)2, dto.FinalSummariesRun);
            Assert.Equal((short)1, dto.PeriodLocked);
        }

        [Fact]
        public async Task GetReleaseSummariesAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.GetReleaseSummariesAsync()
                .Returns(Task.FromException<ReleaseSummary>(new InvalidOperationException("Database error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetReleaseSummariesAsync());

            await _mockRepository.Received(1).GetReleaseSummariesAsync();
        }

        [Fact]
        public async Task GetReleaseSummariesAsync_MapperThrowsException_PropagatesException()
        {
            // Arrange
            var entities = new List<ReleasePeriod>
            {
                new() { PeriodName = "Period1", FpsYear = 2024 }
            };
            var releaseSummary = new ReleaseSummary { ReleasePeriods = entities };

            _mockRepository.GetReleaseSummariesAsync().Returns(releaseSummary);
            _mockMapper.When(m => m.Map<ReleaseSummaryDto>(Arg.Any<ReleaseSummary>()))
                .Do(_ => throw new AutoMapperMappingException("Mapper error"));

            // Act & Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _service.GetReleaseSummariesAsync());
        }

        #endregion

        #region SetFinalSummaryRunAsync

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithExistingPeriod_ReturnsMappedDto()
        {
            // Arrange
            const string periodName = "TestPeriod";
            const short finalSummariesRun = 1;

            var entity = new ReleasePeriod
            {
                PeriodName = periodName,
                FinalSummariesRun = finalSummariesRun,
                EndPeriod = 1.0
            };

            var expectedDto = new ReleasePeriodDto
            {
                PeriodName = periodName,
                FinalSummariesRun = finalSummariesRun,
                EndPeriod = 1.0
            };

            _mockRepository.SetFinalSummaryRunAsync(periodName, finalSummariesRun, Arg.Any<string>()).Returns(entity);
            _mockMapper.Map<ReleasePeriodDto?>(entity).Returns(expectedDto);

            // Act
            var result = await _service.SetFinalSummaryRunAsync(periodName, finalSummariesRun, "1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(periodName, result.PeriodName);
            Assert.Equal(finalSummariesRun, result.FinalSummariesRun);

            await _mockRepository.Received(1).SetFinalSummaryRunAsync(periodName, finalSummariesRun, Arg.Any<string>());
            _mockMapper.Received(1).Map<ReleasePeriodDto?>(entity);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_WithNonExistingPeriod_ReturnsNull()
        {
            // Arrange
            const string periodName = "NonExistentPeriod";
            const short finalSummariesRun = 1;

            _mockRepository.SetFinalSummaryRunAsync(periodName, finalSummariesRun, Arg.Any<string>()).Returns((ReleasePeriod?)null);
            _mockMapper.Map<ReleasePeriodDto?>(null).Returns((ReleasePeriodDto?)null);

            // Act
            var result = await _service.SetFinalSummaryRunAsync(periodName, finalSummariesRun, "0");

            // Assert
            Assert.Null(result);

            await _mockRepository.Received(1).SetFinalSummaryRunAsync(periodName, finalSummariesRun, Arg.Any<string>());
            _mockMapper.Received(1).Map<ReleasePeriodDto?>((ReleasePeriod?)null);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_PassesCorrectArgumentsToRepository()
        {
            // Arrange
            const string periodName = "ArgCheckPeriod";
            const short finalSummariesRun = 3;

            var entity = new ReleasePeriod { PeriodName = periodName, FinalSummariesRun = finalSummariesRun };
            var dto = new ReleasePeriodDto { PeriodName = periodName, FinalSummariesRun = finalSummariesRun };

            _mockRepository.SetFinalSummaryRunAsync(periodName, finalSummariesRun, Arg.Any<string>()).Returns(entity);
            _mockMapper.Map<ReleasePeriodDto?>(entity).Returns(dto);

            // Act
            await _service.SetFinalSummaryRunAsync(periodName, finalSummariesRun, "1");

            // Assert
            await _mockRepository.Received(1).SetFinalSummaryRunAsync(
                Arg.Is<string>(p => p == periodName),
                Arg.Is<short>(f => f == finalSummariesRun),
                Arg.Any<string>()
            );
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_MapsAllFieldsFromEntityToDto()
        {
            // Arrange
            const string periodName = "FieldMapPeriod";
            const short finalSummariesRun = 2;

            var entity = new ReleasePeriod
            {
                PeriodName = periodName,
                PeriodType = "Month",
                StartPeriod = 1.5,
                EndPeriod = 2.5,
                FinalSummariesRun = finalSummariesRun,
                PeriodLocked = 0
            };

            var expectedDto = new ReleasePeriodDto
            {
                PeriodName = periodName,
                PeriodType = "Month",
                StartPeriod = 1.5,
                EndPeriod = 2.5,
                FinalSummariesRun = finalSummariesRun,
                PeriodLocked = 0
            };

            _mockRepository.SetFinalSummaryRunAsync(periodName, finalSummariesRun, Arg.Any<string>()).Returns(entity);
            _mockMapper.Map<ReleasePeriodDto?>(entity).Returns(expectedDto);

            // Act
            var result = await _service.SetFinalSummaryRunAsync(periodName, finalSummariesRun, "0");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(periodName, result.PeriodName);
            Assert.Equal("Month", result.PeriodType);
            Assert.Equal(1.5, result.StartPeriod);
            Assert.Equal(2.5, result.EndPeriod);
            Assert.Equal(finalSummariesRun, result.FinalSummariesRun);
            Assert.Equal((short)0, result.PeriodLocked);
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            const string periodName = "ErrorPeriod";
            const short finalSummariesRun = 1;

            _mockRepository.SetFinalSummaryRunAsync(periodName, finalSummariesRun, Arg.Any<string>())
                .Returns(Task.FromException<ReleasePeriod?>(new InvalidOperationException("Database error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.SetFinalSummaryRunAsync(periodName, finalSummariesRun, "1"));

            await _mockRepository.Received(1).SetFinalSummaryRunAsync(periodName, finalSummariesRun, Arg.Any<string>());
        }

        [Fact]
        public async Task SetFinalSummaryRunAsync_MapperThrowsException_PropagatesException()
        {
            // Arrange
            const string periodName = "MapperErrorPeriod";
            const short finalSummariesRun = 1;

            var entity = new ReleasePeriod { PeriodName = periodName, FinalSummariesRun = finalSummariesRun };

            _mockRepository.SetFinalSummaryRunAsync(periodName, finalSummariesRun, Arg.Any<string>()).Returns(entity);
            _mockMapper.When(m => m.Map<ReleasePeriodDto?>(entity))
                .Do(_ => throw new AutoMapperMappingException("Mapper error"));

            // Act & Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(
                () => _service.SetFinalSummaryRunAsync(periodName, finalSummariesRun, "1"));
        }

        #endregion
    }
}
