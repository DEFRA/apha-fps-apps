using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Services;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Models;
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

            var entities = new List<RecreateSummaryLogWithUser>
            {
                new() { Id = 1, UserId = TestUserId, UserName = TestUserName, Period = TestPeriod, DateDone = DateTime.UtcNow, FpsYear = 2024 },
                new() { Id = 2, UserId = TestUserId, UserName = TestUserName, Period = 2, DateDone = DateTime.UtcNow.AddDays(-1), FpsYear = 2024 }
            };

            var paginationData = new PaginationData
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 2
            };

            var pagedData = new PagedData<RecreateSummaryLogWithUser>(entities.AsReadOnly(), paginationData);

            var dtos = new List<RecreateSummaryLogDto>
            {
                new() { Id = 1, UserId = TestUserId, UserName = TestUserName, Period = TestPeriod, DateDone = DateTime.UtcNow },
                new() { Id = 2, UserId = TestUserId, UserName = TestUserName, Period = 2, DateDone = DateTime.UtcNow.AddDays(-1) }
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

            var emptyEntities = new List<RecreateSummaryLogWithUser>();
            var paginationData = new PaginationData
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 0,
                TotalRecords = 0
            };

            var pagedData = new PagedData<RecreateSummaryLogWithUser>(emptyEntities.AsReadOnly(), paginationData);

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
                .Returns(Task.FromException<PagedData<RecreateSummaryLogWithUser>>(new InvalidOperationException("Database error")));

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
                .Select(i => new RecreateSummaryLogWithUser
                {
                    Id = i,
                    UserId = TestUserId,
                    UserName = TestUserName,
                    Period = (short)i,
                    DateDone = DateTime.UtcNow.AddDays(-i),
                    FpsYear = 2024
                })
                .ToList();

            var paginationData = new PaginationData
            {
                PageNumber = 2,
                PageSize = 5,
                TotalPages = 4,
                TotalRecords = 20
            };

            var pagedData = new PagedData<RecreateSummaryLogWithUser>(entities.AsReadOnly(), paginationData);

            var dtos = entities.Select(e => new RecreateSummaryLogDto
            {
                Id = e.Id,
                UserId = e.UserId,
                UserName = TestUserName,
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

            var entities = new List<RecreateSummaryLogWithUser>
            {
                new() { Id = 1, UserId = "UserA", UserName = TestUserName, Period = 1, DateDone = DateTime.UtcNow, FpsYear = 2024 },
                new() { Id = 2, UserId = "UserB", UserName = TestUserName, Period = 2, DateDone = DateTime.UtcNow, FpsYear = 2024 }
            };

            var paginationData = new PaginationData
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 2
            };

            var pagedData = new PagedData<RecreateSummaryLogWithUser>(entities.AsReadOnly(), paginationData);

            var dtos = new List<RecreateSummaryLogDto>
            {
                new() { Id = 1, UserId = "UserA", UserName = "User A", Period = 1, DateDone = DateTime.UtcNow },
                new() { Id = 2, UserId = "UserB", UserName = "User B", Period = 2, DateDone = DateTime.UtcNow }
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
    }
}
