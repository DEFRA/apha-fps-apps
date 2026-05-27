using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Api.Controllers;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Api.UnitTests.Controller.SummarisedWgTimeControllerTest
{
    public class SummarisedWgTimeControllerTests
    {
        private readonly ISummarisedWgTimeService _service;
        private readonly IMapper _mapper;
        private readonly SummarisedWgTimeController _controller;

        public SummarisedWgTimeControllerTests()
        {
            _service = Substitute.For<ISummarisedWgTimeService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new SummarisedWgTimeController(_service, _mapper);
        }

        #region GetPaged

        [Fact]
        public async Task GetPaged_WithValidQuery_ReturnsOkWithMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var workGroup = "WG1";
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [1, 2, 3],
                Rows = new List<SummarisedWgTimeDto>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ1",
                        ProjectTitle = "Project 1",
                        SumOfTime = 100,
                        SumOfCost = 1000
                    }
                },
                Pagination = new PaginationDto
                {
                    PageNumber = 1,
                    PageSize = 10,
                    TotalPages = 1,
                    TotalRecords = 1
                }
            };
            var pivotRes = new SummarisedWgTimePivotRes
            {
                Months = [1, 2, 3],
                Rows = new List<SummarisedWgTimeRes>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ1",
                        ProjectTitle = "Project 1",
                        SumOfTime = 100,
                        SumOfCost = 1000
                    }
                },
                Pagination = new Pagination
                {
                    PageNumber = 1,
                    PageSize = 10,
                    TotalPages = 1,
                    TotalRecords = 1
                }
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup).Returns(pivotDto);
            _mapper.Map<SummarisedWgTimePivotRes>(pivotDto).Returns(pivotRes);

            // Act
            var result = await _controller.GetPaged(query, workGroup);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(pivotRes, ok.Value);
            await _service.Received(1).GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);
            _mapper.Received(1).Map<SummarisedWgTimePivotRes>(pivotDto);
        }

        [Fact]
        public async Task GetPaged_WithNullWorkGroup_ReturnsOkWithAllWorkGroups()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [1, 2, 3],
                Rows = new List<SummarisedWgTimeDto>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ1",
                        ProjectTitle = "Project 1",
                        SumOfTime = 100,
                        SumOfCost = 1000
                    },
                    new()
                    {
                        WorkGroup = "WG2",
                        ParentProject = "PRJ2",
                        ProjectTitle = "Project 2",
                        SumOfTime = 200,
                        SumOfCost = 2000
                    }
                },
                Pagination = new PaginationDto()
            };
            var pivotRes = new SummarisedWgTimePivotRes
            {
                Months = [1, 2, 3],
                Rows = new List<SummarisedWgTimeRes>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ1",
                        ProjectTitle = "Project 1",
                        SumOfTime = 100,
                        SumOfCost = 1000
                    },
                    new()
                    {
                        WorkGroup = "WG2",
                        ParentProject = "PRJ2",
                        ProjectTitle = "Project 2",
                        SumOfTime = 200,
                        SumOfCost = 2000
                    }
                },
                Pagination = new Pagination()
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(query, null).Returns(pivotDto);
            _mapper.Map<SummarisedWgTimePivotRes>(pivotDto).Returns(pivotRes);

            // Act
            var result = await _controller.GetPaged(query, null);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<SummarisedWgTimePivotRes>(ok.Value);
            Assert.Equal(2, value.Rows.Count);
            await _service.Received(1).GetSummarisedWorkgroupTimeSummaryAsync(query, null);
        }

        [Fact]
        public async Task GetPaged_WithEmptyResult_ReturnsOkWithEmptyRows()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var workGroup = "WG_NONEXISTENT";
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [],
                Rows = [],
                Pagination = new PaginationDto
                {
                    PageNumber = 1,
                    PageSize = 10,
                    TotalPages = 0,
                    TotalRecords = 0
                }
            };
            var pivotRes = new SummarisedWgTimePivotRes
            {
                Months = [],
                Rows = [],
                Pagination = new Pagination
                {
                    PageNumber = 1,
                    PageSize = 10,
                    TotalPages = 0,
                    TotalRecords = 0
                }
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup).Returns(pivotDto);
            _mapper.Map<SummarisedWgTimePivotRes>(pivotDto).Returns(pivotRes);

            // Act
            var result = await _controller.GetPaged(query, workGroup);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<SummarisedWgTimePivotRes>(ok.Value);
            Assert.Empty(value.Rows);
            Assert.Empty(value.Months);
        }

        [Fact]
        public async Task GetPaged_WithPaginationParameters_ReturnsCorrectPage()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 5 };
            var workGroup = "WG1";
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [1, 2, 3],
                Rows = new List<SummarisedWgTimeDto>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ6",
                        ProjectTitle = "Project 6",
                        SumOfTime = 600,
                        SumOfCost = 6000
                    }
                },
                Pagination = new PaginationDto
                {
                    PageNumber = 2,
                    PageSize = 5,
                    TotalPages = 3,
                    TotalRecords = 15
                }
            };
            var pivotRes = new SummarisedWgTimePivotRes
            {
                Months = [1, 2, 3],
                Rows = new List<SummarisedWgTimeRes>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ6",
                        ProjectTitle = "Project 6",
                        SumOfTime = 600,
                        SumOfCost = 6000
                    }
                },
                Pagination = new Pagination
                {
                    PageNumber = 2,
                    PageSize = 5,
                    TotalPages = 3,
                    TotalRecords = 15
                }
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup).Returns(pivotDto);
            _mapper.Map<SummarisedWgTimePivotRes>(pivotDto).Returns(pivotRes);

            // Act
            var result = await _controller.GetPaged(query, workGroup);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<SummarisedWgTimePivotRes>(ok.Value);
            Assert.Equal(2, value.Pagination.PageNumber);
            Assert.Equal(5, value.Pagination.PageSize);
            Assert.Equal(15, value.Pagination.TotalRecords);
        }

        [Fact]
        public async Task GetPaged_WithSearchQuery_ReturnsFilteredResults()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Search = "PRJ1"
            };
            var workGroup = "WG1";
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [1, 2],
                Rows = new List<SummarisedWgTimeDto>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ1",
                        ProjectTitle = "Project 1",
                        SumOfTime = 100,
                        SumOfCost = 1000
                    }
                },
                Pagination = new PaginationDto()
            };
            var pivotRes = new SummarisedWgTimePivotRes
            {
                Months = [1, 2],
                Rows = new List<SummarisedWgTimeRes>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ1",
                        ProjectTitle = "Project 1",
                        SumOfTime = 100,
                        SumOfCost = 1000
                    }
                },
                Pagination = new Pagination()
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup).Returns(pivotDto);
            _mapper.Map<SummarisedWgTimePivotRes>(pivotDto).Returns(pivotRes);

            // Act
            var result = await _controller.GetPaged(query, workGroup);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<SummarisedWgTimePivotRes>(ok.Value);
            Assert.Single(value.Rows);
            Assert.Equal("PRJ1", value.Rows[0].ParentProject);
        }

        [Fact]
        public async Task GetPaged_WithSortingQuery_ReturnsSortedResults()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "sumoftime",
                Descending = true
            };
            var workGroup = "WG1";
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [1, 2],
                Rows = new List<SummarisedWgTimeDto>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ2",
                        ProjectTitle = "Project 2",
                        SumOfTime = 200,
                        SumOfCost = 2000
                    },
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ1",
                        ProjectTitle = "Project 1",
                        SumOfTime = 100,
                        SumOfCost = 1000
                    }
                },
                Pagination = new PaginationDto()
            };
            var pivotRes = new SummarisedWgTimePivotRes
            {
                Months = [1, 2],
                Rows = new List<SummarisedWgTimeRes>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ2",
                        ProjectTitle = "Project 2",
                        SumOfTime = 200,
                        SumOfCost = 2000
                    },
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ1",
                        ProjectTitle = "Project 1",
                        SumOfTime = 100,
                        SumOfCost = 1000
                    }
                },
                Pagination = new Pagination()
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup).Returns(pivotDto);
            _mapper.Map<SummarisedWgTimePivotRes>(pivotDto).Returns(pivotRes);

            // Act
            var result = await _controller.GetPaged(query, workGroup);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<SummarisedWgTimePivotRes>(ok.Value);
            Assert.Equal(2, value.Rows.Count);
            Assert.True(value.Rows[0].SumOfTime >= value.Rows[1].SumOfTime);
        }

        [Fact]
        public async Task GetPaged_WithAllMonths_ReturnsAllMonthsInResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var workGroup = "WG1";
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
                Rows = new List<SummarisedWgTimeDto>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ1",
                        ProjectTitle = "Project 1",
                        April = 10,
                        May = 20,
                        June = 30,
                        July = 40,
                        August = 50,
                        September = 60,
                        October = 70,
                        November = 80,
                        December = 90,
                        January = 100,
                        February = 110,
                        March = 120,
                        SumOfTime = 780,
                        SumOfCost = 7800
                    }
                },
                Pagination = new PaginationDto()
            };
            var pivotRes = new SummarisedWgTimePivotRes
            {
                Months = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
                Rows = new List<SummarisedWgTimeRes>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ1",
                        ProjectTitle = "Project 1",
                        April = 10,
                        May = 20,
                        June = 30,
                        July = 40,
                        August = 50,
                        September = 60,
                        October = 70,
                        November = 80,
                        December = 90,
                        January = 100,
                        February = 110,
                        March = 120,
                        SumOfTime = 780,
                        SumOfCost = 7800
                    }
                },
                Pagination = new Pagination()
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup).Returns(pivotDto);
            _mapper.Map<SummarisedWgTimePivotRes>(pivotDto).Returns(pivotRes);

            // Act
            var result = await _controller.GetPaged(query, workGroup);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<SummarisedWgTimePivotRes>(ok.Value);
            Assert.Equal(12, value.Months.Count);
            Assert.Equal(780, value.Rows[0].SumOfTime);
        }

        [Fact]
        public async Task GetPaged_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var workGroup = "WG1";

            _service.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup)
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.GetPaged(query, workGroup));
        }

        [Fact]
        public async Task GetPaged_WhenMapperThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var workGroup = "WG1";
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [1, 2, 3],
                Rows = [],
                Pagination = new PaginationDto()
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup).Returns(pivotDto);
            _mapper.Map<SummarisedWgTimePivotRes>(pivotDto)
                .ThrowsForAnyArgs(new AutoMapperMappingException("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(
                () => _controller.GetPaged(query, workGroup));
        }

        [Fact]
        public async Task GetPaged_WithDefaultQueryParameters_ReturnsFirstPage()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var workGroup = "WG1";
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [1],
                Rows = new List<SummarisedWgTimeDto>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ1",
                        ProjectTitle = "Project 1",
                        SumOfTime = 100,
                        SumOfCost = 1000
                    }
                },
                Pagination = new PaginationDto()
            };
            var pivotRes = new SummarisedWgTimePivotRes
            {
                Months = [1],
                Rows = new List<SummarisedWgTimeRes>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ1",
                        ProjectTitle = "Project 1",
                        SumOfTime = 100,
                        SumOfCost = 1000
                    }
                },
                Pagination = new Pagination()
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup).Returns(pivotDto);
            _mapper.Map<SummarisedWgTimePivotRes>(pivotDto).Returns(pivotRes);

            // Act
            var result = await _controller.GetPaged(query, workGroup);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
            await _service.Received(1).GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);
        }

        [Fact]
        public async Task GetPaged_WithMultipleWorkGroups_ReturnsAllGroupedData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [1, 2, 3],
                Rows = new List<SummarisedWgTimeDto>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ1",
                        ProjectTitle = "Project 1",
                        SumOfTime = 100,
                        SumOfCost = 1000
                    },
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ2",
                        ProjectTitle = "Project 2",
                        SumOfTime = 150,
                        SumOfCost = 1500
                    },
                    new()
                    {
                        WorkGroup = "WG2",
                        ParentProject = "PRJ3",
                        ProjectTitle = "Project 3",
                        SumOfTime = 200,
                        SumOfCost = 2000
                    }
                },
                Pagination = new PaginationDto()
            };
            var pivotRes = new SummarisedWgTimePivotRes
            {
                Months = [1, 2, 3],
                Rows = new List<SummarisedWgTimeRes>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ1",
                        ProjectTitle = "Project 1",
                        SumOfTime = 100,
                        SumOfCost = 1000
                    },
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ2",
                        ProjectTitle = "Project 2",
                        SumOfTime = 150,
                        SumOfCost = 1500
                    },
                    new()
                    {
                        WorkGroup = "WG2",
                        ParentProject = "PRJ3",
                        ProjectTitle = "Project 3",
                        SumOfTime = 200,
                        SumOfCost = 2000
                    }
                },
                Pagination = new Pagination()
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(query, null).Returns(pivotDto);
            _mapper.Map<SummarisedWgTimePivotRes>(pivotDto).Returns(pivotRes);

            // Act
            var result = await _controller.GetPaged(query, null);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<SummarisedWgTimePivotRes>(ok.Value);
            Assert.Equal(3, value.Rows.Count);
            Assert.Contains(value.Rows, r => r.WorkGroup == "WG1" && r.ParentProject == "PRJ1");
            Assert.Contains(value.Rows, r => r.WorkGroup == "WG1" && r.ParentProject == "PRJ2");
            Assert.Contains(value.Rows, r => r.WorkGroup == "WG2" && r.ParentProject == "PRJ3");
        }

        #endregion
    }
}
