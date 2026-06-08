using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Api.Controllers;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.PACT.Api.UnitTests.Controller.RecreateAndReleaseSummaryControllerTest
{
    public class RecreateAndReleaseSummaryControllerTests
    {
        private readonly IRecreateAndReleaseSummaryService _mockService;
        private readonly IMapper _mockMapper;
        private readonly RecreateAndReleaseSummaryController _controller;

        private const string TestUserId = "TestUser1";
        private const short TestPeriod = 1;

        public RecreateAndReleaseSummaryControllerTests()
        {
            _mockService = Substitute.For<IRecreateAndReleaseSummaryService>();
            _mockMapper = Substitute.For<IMapper>();
            _controller = new RecreateAndReleaseSummaryController(_mockService, _mockMapper);
        }

        #region GetRecreateSummariesLogs

        [Fact]
        public async Task GetRecreateSummariesLogs_WithExistingLogs_ReturnsOkWithPaginatedResponse()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "datedone",
                Descending = true
            };

            var dtos = new List<RecreateSummaryLogDto>
            {
                new() { Id = 1, UserId = TestUserId, Comments = "Test User", Period = TestPeriod, DateDone = DateTime.UtcNow },
                new() { Id = 2, UserId = TestUserId, Comments = "Test User", Period = 2, DateDone = DateTime.UtcNow.AddDays(-1) }
            };

            var paginationDto = new PaginationDto
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 2
            };

            var paginatedResult = new PaginatedResult<RecreateSummaryLogDto>(dtos, paginationDto);

            var responses = new List<RecreateSummaryLogRes>
            {
                new() { Id = 1, UserId = TestUserId, Comments = "Test User", Period = TestPeriod, DateDone = DateTime.UtcNow },
                new() { Id = 2, UserId = TestUserId, Comments = "Test User", Period = 2, DateDone = DateTime.UtcNow.AddDays(-1) }
            };

            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 2
            };

            var paginationRes = new PaginationRes<RecreateSummaryLogRes>(responses, pagination);

            _mockService.GetRecreateSummaryLogAsync(query).Returns(paginatedResult);
            _mockMapper.Map<PaginationRes<RecreateSummaryLogRes>>(paginatedResult).Returns(paginationRes);

            // Act
            var result = await _controller.GetRecreateSummaryLog(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaginationRes<RecreateSummaryLogRes>>(okResult.Value);
            Assert.Equal(2, returnValue.Data.Count());
            Assert.Equal(1, returnValue.PaginationData.PageNumber);
            Assert.Equal(10, returnValue.PaginationData.PageSize);
            Assert.Equal(1, returnValue.PaginationData.TotalPages);
            Assert.Equal(2, returnValue.PaginationData.TotalRecords);

            await _mockService.Received(1).GetRecreateSummaryLogAsync(query);
            _mockMapper.Received(1).Map<PaginationRes<RecreateSummaryLogRes>>(paginatedResult);
        }

        [Fact]
        public async Task GetRecreateSummariesLogs_WithNoLogs_ReturnsOkWithEmptyPaginatedResponse()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            var emptyDtos = new List<RecreateSummaryLogDto>();
            var paginationDto = new PaginationDto
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 0,
                TotalRecords = 0
            };

            var paginatedResult = new PaginatedResult<RecreateSummaryLogDto>(emptyDtos, paginationDto);

            var emptyResponses = new List<RecreateSummaryLogRes>();
            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 0,
                TotalRecords = 0
            };

            var paginationRes = new PaginationRes<RecreateSummaryLogRes>(emptyResponses, pagination);

            _mockService.GetRecreateSummaryLogAsync(query).Returns(paginatedResult);
            _mockMapper.Map<PaginationRes<RecreateSummaryLogRes>>(paginatedResult).Returns(paginationRes);

            // Act
            var result = await _controller.GetRecreateSummaryLog(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaginationRes<RecreateSummaryLogRes>>(okResult.Value);
            Assert.Empty(returnValue.Data);
            Assert.Equal(0, returnValue.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetRecreateSummariesLogs_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            _mockService.GetRecreateSummaryLogAsync(query)
                .Returns(Task.FromException<PaginatedResult<RecreateSummaryLogDto>>(new InvalidOperationException("Service error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetRecreateSummaryLog(query));
        }

        [Fact]
        public async Task GetRecreateSummariesLogs_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 2,
                PageSize = 5,
                SortBy = "period",
                Descending = false
            };

            var dtos = Enumerable.Range(6, 5)
                .Select(i => new RecreateSummaryLogDto
                {
                    Id = i,
                    UserId = TestUserId,
                    Comments = "Test User",
                    Period = (short)i,
                    DateDone = DateTime.UtcNow.AddDays(-i)
                })
                .ToList();

            var paginationDto = new PaginationDto
            {
                PageNumber = 2,
                PageSize = 5,
                TotalPages = 4,
                TotalRecords = 20
            };

            var paginatedResult = new PaginatedResult<RecreateSummaryLogDto>(dtos, paginationDto);

            var responses = dtos.Select(dto => new RecreateSummaryLogRes
            {
                Id = dto.Id,
                UserId = dto.UserId,
                Comments = dto.Comments,
                Period = dto.Period,
                DateDone = dto.DateDone
            }).ToList();

            var pagination = new Pagination
            {
                PageNumber = 2,
                PageSize = 5,
                TotalPages = 4,
                TotalRecords = 20
            };

            var paginationRes = new PaginationRes<RecreateSummaryLogRes>(responses, pagination);

            _mockService.GetRecreateSummaryLogAsync(query).Returns(paginatedResult);
            _mockMapper.Map<PaginationRes<RecreateSummaryLogRes>>(paginatedResult).Returns(paginationRes);

            // Act
            var result = await _controller.GetRecreateSummaryLog(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaginationRes<RecreateSummaryLogRes>>(okResult.Value);
            Assert.Equal(5, returnValue.Data.Count());
            Assert.Equal(2, returnValue.PaginationData.PageNumber);
            Assert.Equal(5, returnValue.PaginationData.PageSize);
            Assert.Equal(4, returnValue.PaginationData.TotalPages);
            Assert.Equal(20, returnValue.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetRecreateSummariesLogs_WithSortParameters_PassesCorrectQueryToService()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "userid",
                Descending = false
            };

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

            var responses = new List<RecreateSummaryLogRes>
            {
                new() { Id = 1, UserId = "UserA", Comments = "User A", Period = 1, DateDone = DateTime.UtcNow },
                new() { Id = 2, UserId = "UserB", Comments = "User B", Period = 2, DateDone = DateTime.UtcNow }
            };

            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 2
            };

            var paginationRes = new PaginationRes<RecreateSummaryLogRes>(responses, pagination);

            _mockService.GetRecreateSummaryLogAsync(query).Returns(paginatedResult);
            _mockMapper.Map<PaginationRes<RecreateSummaryLogRes>>(paginatedResult).Returns(paginationRes);

            // Act
            var result = await _controller.GetRecreateSummaryLog(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            await _mockService.Received(1).GetRecreateSummaryLogAsync(
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 1 &&
                    q.PageSize == 10 &&
                    q.SortBy == "userid" &&
                    q.Descending == false
                )
            );
        }

        [Fact]
        public async Task GetRecreateSummariesLogs_MapperThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            var dtos = new List<RecreateSummaryLogDto>
            {
                new() { Id = 1, UserId = TestUserId, Comments = "Test User", Period = TestPeriod, DateDone = DateTime.UtcNow }
            };

            var paginationDto = new PaginationDto
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 1
            };

            var paginatedResult = new PaginatedResult<RecreateSummaryLogDto>(dtos, paginationDto);

            _mockService.GetRecreateSummaryLogAsync(query).Returns(paginatedResult);
            _mockMapper.When(m => m.Map<PaginationRes<RecreateSummaryLogRes>>(paginatedResult))
                .Do(_ => throw new InvalidOperationException("Mapper error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetRecreateSummaryLog(query));
        }

        #endregion
    }
}
