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

        #region GetRecreateSummariesAllLogs

        [Fact]
        public async Task GetRecreateSummariesAllLogs_WithExistingLogs_ReturnsOkWithPaginatedResponse()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "datedone",
                Descending = true
            };

            var dtos = new List<RecreateSummariesLogDto>
            {
                new() { Id = 1, UserId = TestUserId, UserName = "Test User", Period = TestPeriod, DateDone = DateTime.UtcNow },
                new() { Id = 2, UserId = TestUserId, UserName = "Test User", Period = 2, DateDone = DateTime.UtcNow.AddDays(-1) }
            };

            var paginationDto = new PaginationDto
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 2
            };

            var paginatedResult = new PaginatedResult<RecreateSummariesLogDto>(dtos, paginationDto);

            var responses = new List<RecreateSummariesLogRes>
            {
                new() { Id = 1, UserId = TestUserId, UserName = "Test User", Period = TestPeriod, DateDone = DateTime.UtcNow },
                new() { Id = 2, UserId = TestUserId, UserName = "Test User", Period = 2, DateDone = DateTime.UtcNow.AddDays(-1) }
            };

            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 2
            };

            var paginationRes = new PaginationRes<RecreateSummariesLogRes>(responses, pagination);

            _mockService.GetRecreateSummariesAllLogsAsync(query).Returns(paginatedResult);
            _mockMapper.Map<PaginationRes<RecreateSummariesLogRes>>(paginatedResult).Returns(paginationRes);

            // Act
            var result = await _controller.GetRecreateSummariesAllLogs(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaginationRes<RecreateSummariesLogRes>>(okResult.Value);
            Assert.Equal(2, returnValue.Data.Count());
            Assert.Equal(1, returnValue.PaginationData.PageNumber);
            Assert.Equal(10, returnValue.PaginationData.PageSize);
            Assert.Equal(1, returnValue.PaginationData.TotalPages);
            Assert.Equal(2, returnValue.PaginationData.TotalRecords);

            await _mockService.Received(1).GetRecreateSummariesAllLogsAsync(query);
            _mockMapper.Received(1).Map<PaginationRes<RecreateSummariesLogRes>>(paginatedResult);
        }

        [Fact]
        public async Task GetRecreateSummariesAllLogs_WithNoLogs_ReturnsOkWithEmptyPaginatedResponse()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            var emptyDtos = new List<RecreateSummariesLogDto>();
            var paginationDto = new PaginationDto
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 0,
                TotalRecords = 0
            };

            var paginatedResult = new PaginatedResult<RecreateSummariesLogDto>(emptyDtos, paginationDto);

            var emptyResponses = new List<RecreateSummariesLogRes>();
            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 0,
                TotalRecords = 0
            };

            var paginationRes = new PaginationRes<RecreateSummariesLogRes>(emptyResponses, pagination);

            _mockService.GetRecreateSummariesAllLogsAsync(query).Returns(paginatedResult);
            _mockMapper.Map<PaginationRes<RecreateSummariesLogRes>>(paginatedResult).Returns(paginationRes);

            // Act
            var result = await _controller.GetRecreateSummariesAllLogs(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaginationRes<RecreateSummariesLogRes>>(okResult.Value);
            Assert.Empty(returnValue.Data);
            Assert.Equal(0, returnValue.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetRecreateSummariesAllLogs_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            _mockService.GetRecreateSummariesAllLogsAsync(query)
                .Returns(Task.FromException<PaginatedResult<RecreateSummariesLogDto>>(new InvalidOperationException("Service error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetRecreateSummariesAllLogs(query));
        }

        [Fact]
        public async Task GetRecreateSummariesAllLogs_WithPagination_ReturnsCorrectPage()
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
                .Select(i => new RecreateSummariesLogDto
                {
                    Id = i,
                    UserId = TestUserId,
                    UserName = "Test User",
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

            var paginatedResult = new PaginatedResult<RecreateSummariesLogDto>(dtos, paginationDto);

            var responses = dtos.Select(dto => new RecreateSummariesLogRes
            {
                Id = dto.Id,
                UserId = dto.UserId,
                UserName = dto.UserName,
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

            var paginationRes = new PaginationRes<RecreateSummariesLogRes>(responses, pagination);

            _mockService.GetRecreateSummariesAllLogsAsync(query).Returns(paginatedResult);
            _mockMapper.Map<PaginationRes<RecreateSummariesLogRes>>(paginatedResult).Returns(paginationRes);

            // Act
            var result = await _controller.GetRecreateSummariesAllLogs(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaginationRes<RecreateSummariesLogRes>>(okResult.Value);
            Assert.Equal(5, returnValue.Data.Count());
            Assert.Equal(2, returnValue.PaginationData.PageNumber);
            Assert.Equal(5, returnValue.PaginationData.PageSize);
            Assert.Equal(4, returnValue.PaginationData.TotalPages);
            Assert.Equal(20, returnValue.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetRecreateSummariesAllLogs_WithSortParameters_PassesCorrectQueryToService()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "userid",
                Descending = false
            };

            var dtos = new List<RecreateSummariesLogDto>
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

            var paginatedResult = new PaginatedResult<RecreateSummariesLogDto>(dtos, paginationDto);

            var responses = new List<RecreateSummariesLogRes>
            {
                new() { Id = 1, UserId = "UserA", UserName = "User A", Period = 1, DateDone = DateTime.UtcNow },
                new() { Id = 2, UserId = "UserB", UserName = "User B", Period = 2, DateDone = DateTime.UtcNow }
            };

            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 2
            };

            var paginationRes = new PaginationRes<RecreateSummariesLogRes>(responses, pagination);

            _mockService.GetRecreateSummariesAllLogsAsync(query).Returns(paginatedResult);
            _mockMapper.Map<PaginationRes<RecreateSummariesLogRes>>(paginatedResult).Returns(paginationRes);

            // Act
            var result = await _controller.GetRecreateSummariesAllLogs(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            await _mockService.Received(1).GetRecreateSummariesAllLogsAsync(
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 1 &&
                    q.PageSize == 10 &&
                    q.SortBy == "userid" &&
                    q.Descending == false
                )
            );
        }

        [Fact]
        public async Task GetRecreateSummariesAllLogs_MapperThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            var dtos = new List<RecreateSummariesLogDto>
            {
                new() { Id = 1, UserId = TestUserId, UserName = "Test User", Period = TestPeriod, DateDone = DateTime.UtcNow }
            };

            var paginationDto = new PaginationDto
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 1
            };

            var paginatedResult = new PaginatedResult<RecreateSummariesLogDto>(dtos, paginationDto);

            _mockService.GetRecreateSummariesAllLogsAsync(query).Returns(paginatedResult);
            _mockMapper.When(m => m.Map<PaginationRes<RecreateSummariesLogRes>>(paginatedResult))
                .Do(_ => throw new InvalidOperationException("Mapper error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetRecreateSummariesAllLogs(query));
        }

        #endregion

        #region GetReleaseSummaries

        [Fact]
        public async Task GetReleaseSummaries_WithExistingPeriods_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = new List<ReleasePeriodDto>
            {
                new() { PeriodName = "Period1", PeriodType = "Month", StartPeriod = 0.5, EndPeriod = 1.0, FinalSummariesRun = 0, PeriodLocked = 0 },
                new() { PeriodName = "Period2", PeriodType = "Month", StartPeriod = 1.5, EndPeriod = 2.0, FinalSummariesRun = 1, PeriodLocked = 0 }
            };

            var responses = new List<ReleasePeriodRes>
            {
                new() { PeriodName = "Period1", PeriodType = "Month", StartPeriod = 0.5, EndPeriod = 1.0, FinalSummariesRun = 0, PeriodLocked = 0 },
                new() { PeriodName = "Period2", PeriodType = "Month", StartPeriod = 1.5, EndPeriod = 2.0, FinalSummariesRun = 1, PeriodLocked = 0 }
            };

            _mockService.GetReleaseSummariesAsync().Returns(dtos.AsReadOnly());
            _mockMapper.Map<IReadOnlyList<ReleasePeriodRes>>(Arg.Any<IReadOnlyList<ReleasePeriodDto>>()).Returns(responses.AsReadOnly());

            // Act
            var result = await _controller.GetReleaseSummaries();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IReadOnlyList<ReleasePeriodRes>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.Equal("Period1", returnValue[0].PeriodName);
            Assert.Equal("Period2", returnValue[1].PeriodName);

            await _mockService.Received(1).GetReleaseSummariesAsync();
            _mockMapper.Received(1).Map<IReadOnlyList<ReleasePeriodRes>>(Arg.Any<IReadOnlyList<ReleasePeriodDto>>());
        }

        [Fact]
        public async Task GetReleaseSummaries_WithNoPeriods_ReturnsOkWithEmptyList()
        {
            // Arrange
            var emptyDtos = new List<ReleasePeriodDto>().AsReadOnly();
            var emptyResponses = new List<ReleasePeriodRes>().AsReadOnly();

            _mockService.GetReleaseSummariesAsync().Returns(emptyDtos);
            _mockMapper.Map<IReadOnlyList<ReleasePeriodRes>>(Arg.Any<IReadOnlyList<ReleasePeriodDto>>()).Returns(emptyResponses);

            // Act
            var result = await _controller.GetReleaseSummaries();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IReadOnlyList<ReleasePeriodRes>>(okResult.Value);
            Assert.Empty(returnValue);

            await _mockService.Received(1).GetReleaseSummariesAsync();
        }

        [Fact]
        public async Task GetReleaseSummaries_MapsAllFieldsCorrectly()
        {
            // Arrange
            var dtos = new List<ReleasePeriodDto>
            {
                new()
                {
                    PeriodName  = "P1",
                    PeriodType  = "Quarter",
                    StartPeriod = 1.0,
                    EndPeriod   = 3.0,
                    FinalSummariesRun = 2,
                    PeriodLocked = 1
                }
            }.AsReadOnly();

            var responses = new List<ReleasePeriodRes>
            {
                new()
                {
                    PeriodName  = "P1",
                    PeriodType  = "Quarter",
                    StartPeriod = 1.0,
                    EndPeriod   = 3.0,
                    FinalSummariesRun = 2,
                    PeriodLocked = 1
                }
            }.AsReadOnly();

            _mockService.GetReleaseSummariesAsync().Returns(dtos);
            _mockMapper.Map<IReadOnlyList<ReleasePeriodRes>>(Arg.Any<IReadOnlyList<ReleasePeriodDto>>()).Returns(responses);

            // Act
            var result = await _controller.GetReleaseSummaries();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IReadOnlyList<ReleasePeriodRes>>(okResult.Value);
            Assert.Single(returnValue);
            var res = returnValue[0];
            Assert.Equal("P1",      res.PeriodName);
            Assert.Equal("Quarter", res.PeriodType);
            Assert.Equal(1.0,       res.StartPeriod);
            Assert.Equal(3.0,       res.EndPeriod);
            Assert.Equal((short)2,  res.FinalSummariesRun);
            Assert.Equal((short)1,  res.PeriodLocked);
        }

        [Fact]
        public async Task GetReleaseSummaries_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _mockService.GetReleaseSummariesAsync()
                .Returns(Task.FromException<IReadOnlyList<ReleasePeriodDto>>(new InvalidOperationException("Service error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetReleaseSummaries());

            await _mockService.Received(1).GetReleaseSummariesAsync();
        }

        [Fact]
        public async Task GetReleaseSummaries_MapperThrowsException_PropagatesException()
        {
            // Arrange
            var dtos = new List<ReleasePeriodDto>
            {
                new() { PeriodName = "Period1" }
            }.AsReadOnly();

            _mockService.GetReleaseSummariesAsync().Returns(dtos);
            _mockMapper.When(m => m.Map<IReadOnlyList<ReleasePeriodRes>>(Arg.Any<IReadOnlyList<ReleasePeriodDto>>()))
                .Do(_ => throw new InvalidOperationException("Mapper error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetReleaseSummaries());
        }

        #endregion

        #region SetFinalSummaryRun

        [Fact]
        public async Task SetFinalSummaryRun_WithExistingPeriod_ReturnsOkWithMappedResponse()
        {
            // Arrange
            var request = new ReleasePeriodReq { PeriodName = "TestPeriod", FinalSummariesRun = 1 };

            var dto = new ReleasePeriodDto
            {
                PeriodName = "TestPeriod",
                FinalSummariesRun = 1,
                EndPeriod = 1.0
            };

            var response = new ReleasePeriodRes
            {
                PeriodName = "TestPeriod",
                FinalSummariesRun = 1,
                EndPeriod = 1.0
            };

            _mockService.SetFinalSummaryRunAsync(request.PeriodName, request.FinalSummariesRun).Returns(dto);
            _mockMapper.Map<ReleasePeriodRes>(dto).Returns(response);

            // Act
            var result = await _controller.SetFinalSummaryRun(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ReleasePeriodRes>(okResult.Value);
            Assert.Equal("TestPeriod", returnValue.PeriodName);
            Assert.Equal((short)1, returnValue.FinalSummariesRun);

            await _mockService.Received(1).SetFinalSummaryRunAsync(request.PeriodName, request.FinalSummariesRun);
            _mockMapper.Received(1).Map<ReleasePeriodRes>(dto);
        }

        [Fact]
        public async Task SetFinalSummaryRun_WithNonExistingPeriod_ReturnsOkWithNullMappedResponse()
        {
            // Arrange
            var request = new ReleasePeriodReq { PeriodName = "NonExistentPeriod", FinalSummariesRun = 1 };

            _mockService.SetFinalSummaryRunAsync(request.PeriodName, request.FinalSummariesRun)
                .Returns((ReleasePeriodDto?)null);
            _mockMapper.Map<ReleasePeriodRes>((ReleasePeriodDto?)null).Returns((ReleasePeriodRes?)null);

            // Act
            var result = await _controller.SetFinalSummaryRun(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);

            await _mockService.Received(1).SetFinalSummaryRunAsync(request.PeriodName, request.FinalSummariesRun);
            _mockMapper.Received(1).Map<ReleasePeriodRes>((ReleasePeriodDto?)null);
        }

        [Fact]
        public async Task SetFinalSummaryRun_PassesCorrectArgumentsToService()
        {
            // Arrange
            var request = new ReleasePeriodReq { PeriodName = "ArgCheckPeriod", FinalSummariesRun = 3 };

            var dto      = new ReleasePeriodDto { PeriodName = "ArgCheckPeriod", FinalSummariesRun = 3 };
            var response = new ReleasePeriodRes { PeriodName = "ArgCheckPeriod", FinalSummariesRun = 3 };

            _mockService.SetFinalSummaryRunAsync(request.PeriodName, request.FinalSummariesRun).Returns(dto);
            _mockMapper.Map<ReleasePeriodRes>(dto).Returns(response);

            // Act
            await _controller.SetFinalSummaryRun(request);

            // Assert
            await _mockService.Received(1).SetFinalSummaryRunAsync(
                Arg.Is<string>(p  => p  == "ArgCheckPeriod"),
                Arg.Is<short>(f   => f  == (short)3)
            );
        }

        [Fact]
        public async Task SetFinalSummaryRun_MapsAllFieldsFromDtoToResponse()
        {
            // Arrange
            var request = new ReleasePeriodReq { PeriodName = "FieldMapPeriod", FinalSummariesRun = 2 };

            var dto = new ReleasePeriodDto
            {
                PeriodName       = "FieldMapPeriod",
                PeriodType       = "Month",
                StartPeriod      = 1.5,
                EndPeriod        = 2.5,
                FinalSummariesRun = 2,
                PeriodLocked     = 0
            };

            var response = new ReleasePeriodRes
            {
                PeriodName       = "FieldMapPeriod",
                PeriodType       = "Month",
                StartPeriod      = 1.5,
                EndPeriod        = 2.5,
                FinalSummariesRun = 2,
                PeriodLocked     = 0
            };

            _mockService.SetFinalSummaryRunAsync(request.PeriodName, request.FinalSummariesRun).Returns(dto);
            _mockMapper.Map<ReleasePeriodRes>(dto).Returns(response);

            // Act
            var result = await _controller.SetFinalSummaryRun(request);

            // Assert
            var okResult    = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ReleasePeriodRes>(okResult.Value);
            Assert.Equal("FieldMapPeriod", returnValue.PeriodName);
            Assert.Equal("Month",          returnValue.PeriodType);
            Assert.Equal(1.5,              returnValue.StartPeriod);
            Assert.Equal(2.5,              returnValue.EndPeriod);
            Assert.Equal((short)2,         returnValue.FinalSummariesRun);
            Assert.Equal((short)0,         returnValue.PeriodLocked);
        }

        [Fact]
        public async Task SetFinalSummaryRun_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new ReleasePeriodReq { PeriodName = "ErrorPeriod", FinalSummariesRun = 1 };

            _mockService.SetFinalSummaryRunAsync(request.PeriodName, request.FinalSummariesRun)
                .Returns(Task.FromException<ReleasePeriodDto?>(new InvalidOperationException("Service error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.SetFinalSummaryRun(request));

            await _mockService.Received(1).SetFinalSummaryRunAsync(request.PeriodName, request.FinalSummariesRun);
        }

        [Fact]
        public async Task SetFinalSummaryRun_MapperThrowsException_PropagatesException()
        {
            // Arrange
            var request = new ReleasePeriodReq { PeriodName = "MapperErrorPeriod", FinalSummariesRun = 1 };
            var dto = new ReleasePeriodDto { PeriodName = "MapperErrorPeriod", FinalSummariesRun = 1 };

            _mockService.SetFinalSummaryRunAsync(request.PeriodName, request.FinalSummariesRun).Returns(dto);
            _mockMapper.When(m => m.Map<ReleasePeriodRes>(dto))
                .Do(_ => throw new InvalidOperationException("Mapper error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.SetFinalSummaryRun(request));
        }

        #endregion
    }
}
