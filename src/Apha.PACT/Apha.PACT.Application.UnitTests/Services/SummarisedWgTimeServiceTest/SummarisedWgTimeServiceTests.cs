using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Services;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace Apha.PACT.Application.UnitTests.Services.SummarisedWgTimeServiceTest
{
    public class SummarisedWgTimeServiceTests
    {
        private readonly ISummarisedWgTimeRepository _mockRepository;
        private readonly SummarisedWgTimeService _sut;

        public SummarisedWgTimeServiceTests()
        {
            _mockRepository = Substitute.For<ISummarisedWgTimeRepository>();
            _sut = new SummarisedWgTimeService(_mockRepository);
        }

        #region GetSummarisedWorkgroupTimeSummaryAsync

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithValidData_ReturnsPivotedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var workGroup = "WG1";
            var data = new List<SummarisedWgTimeView>
            {
                new()
                {
                    WorkGroup = "WG1",
                    ProfitCentre = "PC1",
                    ParentProject = "PRJ1",
                    Name = "Project 1",
                    MonthName = "April",
                    TotalTime = 100,
                    TotalCost = 1000
                },
                new()
                {
                    WorkGroup = "WG1",
                    ProfitCentre = "PC1",
                    ParentProject = "PRJ1",
                    Name = "Project 1",
                    MonthName = "May",
                    TotalTime = 150,
                    TotalCost = 1500
                }
            };

            _mockRepository.GetSummarisedWorkgroupTimeAsync(workGroup, Arg.Any<CancellationToken>())
                .Returns(data);

            // Act
            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);

            // Assert
            result.Should().NotBeNull();
            result.Rows.Should().HaveCount(1);
            result.Rows[0].ParentProject.Should().Be("PRJ1");
            result.Rows[0].April.Should().Be(100);
            result.Rows[0].May.Should().Be(150);
            result.Rows[0].SumOfTime.Should().Be(250);
            result.Rows[0].SumOfCost.Should().Be(2500);
            result.Pagination.PageNumber.Should().Be(1);
            result.Pagination.TotalRecords.Should().Be(1);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithNullWorkGroup_ReturnsAllWorkGroups()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var data = new List<SummarisedWgTimeView>
            {
                new()
                {
                    WorkGroup = "WG1",
                    ProfitCentre = "PC1",
                    ParentProject = "PRJ1",
                    Name = "Project 1",
                    MonthName = "April",
                    TotalTime = 100,
                    TotalCost = 1000
                },
                new()
                {
                    WorkGroup = "WG2",
                    ProfitCentre = "PC2",
                    ParentProject = "PRJ2",
                    Name = "Project 2",
                    MonthName = "April",
                    TotalTime = 200,
                    TotalCost = 2000
                }
            };

            _mockRepository.GetSummarisedWorkgroupTimeAsync(null, Arg.Any<CancellationToken>())
                .Returns(data);

            // Act
            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(query, null);

            // Assert
            result.Should().NotBeNull();
            result.Rows.Should().HaveCount(2);
            result.Rows[0].WorkGroup.Should().Be("WG1");
            result.Rows[1].WorkGroup.Should().Be("WG2");
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithEmptyData_ReturnsEmptyResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1", Arg.Any<CancellationToken>())
                .Returns(new List<SummarisedWgTimeView>());

            // Act
            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(query, "WG1");

            // Assert
            result.Should().NotBeNull();
            result.Rows.Should().BeEmpty();
            result.Pagination.TotalRecords.Should().Be(0);
            result.Pagination.TotalPages.Should().Be(0);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithMultipleMonths_PivotsCorrectly()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var data = new List<SummarisedWgTimeView>
            {
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "April", TotalTime = 10, TotalCost = 100 },
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "May", TotalTime = 20, TotalCost = 200 },
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "June", TotalTime = 30, TotalCost = 300 },
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "July", TotalTime = 40, TotalCost = 400 },
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "August", TotalTime = 50, TotalCost = 500 },
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "September", TotalTime = 60, TotalCost = 600 },
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "October", TotalTime = 70, TotalCost = 700 },
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "November", TotalTime = 80, TotalCost = 800 },
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "December", TotalTime = 90, TotalCost = 900 },
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "January", TotalTime = 100, TotalCost = 1000 },
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "February", TotalTime = 110, TotalCost = 1100 },
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "March", TotalTime = 120, TotalCost = 1200 }
            };

            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1", Arg.Any<CancellationToken>())
                .Returns(data);

            // Act
            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(query, "WG1");

            // Assert
            result.Rows.Should().HaveCount(1);
            var row = result.Rows[0];
            row.April.Should().Be(10);
            row.May.Should().Be(20);
            row.June.Should().Be(30);
            row.July.Should().Be(40);
            row.August.Should().Be(50);
            row.September.Should().Be(60);
            row.October.Should().Be(70);
            row.November.Should().Be(80);
            row.December.Should().Be(90);
            row.January.Should().Be(100);
            row.February.Should().Be(110);
            row.March.Should().Be(120);
            row.SumOfTime.Should().Be(780);
            row.SumOfCost.Should().Be(7800);
            result.Months.Should().Contain([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithDuplicateMonthData_AggregatesCorrectly()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var data = new List<SummarisedWgTimeView>
            {
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "April", TotalTime = 100, TotalCost = 1000 },
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "April", TotalTime = 50, TotalCost = 500 }
            };

            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1", Arg.Any<CancellationToken>())
                .Returns(data);

            // Act
            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(query, "WG1");

            // Assert
            result.Rows[0].April.Should().Be(150); // 100 + 50
            result.Rows[0].SumOfTime.Should().Be(150);
            result.Rows[0].SumOfCost.Should().Be(1500);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithSearchFilter_FiltersCorrectly()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, Search = "PRJ1" };
            var data = new List<SummarisedWgTimeView>
            {
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "April", TotalTime = 100, TotalCost = 1000 },
                new() { WorkGroup = "WG2", ParentProject = "PRJ2", Name = "Project 2", MonthName = "April", TotalTime = 200, TotalCost = 2000 }
            };

            _mockRepository.GetSummarisedWorkgroupTimeAsync(null, Arg.Any<CancellationToken>())
                .Returns(data);

            // Act
            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(query, null);

            // Assert
            result.Rows.Should().HaveCount(1);
            result.Rows[0].ParentProject.Should().Be("PRJ1");
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithSearchFilter_SearchesMultipleFields()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, Search = "WG2" };
            var data = new List<SummarisedWgTimeView>
            {
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "April", TotalTime = 100, TotalCost = 1000 },
                new() { WorkGroup = "WG2", ParentProject = "PRJ2", Name = "Project 2", MonthName = "April", TotalTime = 200, TotalCost = 2000 }
            };

            _mockRepository.GetSummarisedWorkgroupTimeAsync(null, Arg.Any<CancellationToken>())
                .Returns(data);

            // Act
            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(query, null);

            // Assert
            result.Rows.Should().HaveCount(1);
            result.Rows[0].WorkGroup.Should().Be("WG2");
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 2 };
            var data = new List<SummarisedWgTimeView>
            {
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "April", TotalTime = 100, TotalCost = 1000 },
                new() { WorkGroup = "WG2", ParentProject = "PRJ2", Name = "Project 2", MonthName = "April", TotalTime = 200, TotalCost = 2000 },
                new() { WorkGroup = "WG3", ParentProject = "PRJ3", Name = "Project 3", MonthName = "April", TotalTime = 300, TotalCost = 3000 },
                new() { WorkGroup = "WG4", ParentProject = "PRJ4", Name = "Project 4", MonthName = "April", TotalTime = 400, TotalCost = 4000 }
            };

            _mockRepository.GetSummarisedWorkgroupTimeAsync(null, Arg.Any<CancellationToken>())
                .Returns(data);

            // Act
            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(query, null);

            // Assert
            result.Rows.Should().HaveCount(2);
            result.Rows[0].ParentProject.Should().Be("PRJ3");
            result.Rows[1].ParentProject.Should().Be("PRJ4");
            result.Pagination.PageNumber.Should().Be(2);
            result.Pagination.PageSize.Should().Be(2);
            result.Pagination.TotalRecords.Should().Be(4);
            result.Pagination.TotalPages.Should().Be(2);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithSortByWorkGroup_SortsCorrectly()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "workgroup", Descending = false };
            var data = new List<SummarisedWgTimeView>
            {
                new() { WorkGroup = "WG3", ParentProject = "PRJ3", Name = "Project 3", MonthName = "April", TotalTime = 300, TotalCost = 3000 },
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "April", TotalTime = 100, TotalCost = 1000 },
                new() { WorkGroup = "WG2", ParentProject = "PRJ2", Name = "Project 2", MonthName = "April", TotalTime = 200, TotalCost = 2000 }
            };

            _mockRepository.GetSummarisedWorkgroupTimeAsync(null, Arg.Any<CancellationToken>())
                .Returns(data);

            // Act
            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(query, null);

            // Assert
            result.Rows[0].WorkGroup.Should().Be("WG1");
            result.Rows[1].WorkGroup.Should().Be("WG2");
            result.Rows[2].WorkGroup.Should().Be("WG3");
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithSortByWorkGroupDescending_SortsCorrectly()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "workgroup", Descending = true };
            var data = new List<SummarisedWgTimeView>
            {
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "April", TotalTime = 100, TotalCost = 1000 },
                new() { WorkGroup = "WG2", ParentProject = "PRJ2", Name = "Project 2", MonthName = "April", TotalTime = 200, TotalCost = 2000 },
                new() { WorkGroup = "WG3", ParentProject = "PRJ3", Name = "Project 3", MonthName = "April", TotalTime = 300, TotalCost = 3000 }
            };

            _mockRepository.GetSummarisedWorkgroupTimeAsync(null, Arg.Any<CancellationToken>())
                .Returns(data);

            // Act
            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(query, null);

            // Assert
            result.Rows[0].WorkGroup.Should().Be("WG3");
            result.Rows[1].WorkGroup.Should().Be("WG2");
            result.Rows[2].WorkGroup.Should().Be("WG1");
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithSortByParentProject_SortsCorrectly()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "parentproject", Descending = false };
            var data = new List<SummarisedWgTimeView>
            {
                new() { WorkGroup = "WG1", ParentProject = "PRJ3", Name = "Project 3", MonthName = "April", TotalTime = 300, TotalCost = 3000 },
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "April", TotalTime = 100, TotalCost = 1000 },
                new() { WorkGroup = "WG1", ParentProject = "PRJ2", Name = "Project 2", MonthName = "April", TotalTime = 200, TotalCost = 2000 }
            };

            _mockRepository.GetSummarisedWorkgroupTimeAsync(null, Arg.Any<CancellationToken>())
                .Returns(data);

            // Act
            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(query, null);

            // Assert
            result.Rows[0].ParentProject.Should().Be("PRJ1");
            result.Rows[1].ParentProject.Should().Be("PRJ2");
            result.Rows[2].ParentProject.Should().Be("PRJ3");
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithSortBySumOfTime_SortsCorrectly()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "sumoftime", Descending = false };
            var data = new List<SummarisedWgTimeView>
            {
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "April", TotalTime = 300, TotalCost = 3000 },
                new() { WorkGroup = "WG2", ParentProject = "PRJ2", Name = "Project 2", MonthName = "April", TotalTime = 100, TotalCost = 1000 },
                new() { WorkGroup = "WG3", ParentProject = "PRJ3", Name = "Project 3", MonthName = "April", TotalTime = 200, TotalCost = 2000 }
            };

            _mockRepository.GetSummarisedWorkgroupTimeAsync(null, Arg.Any<CancellationToken>())
                .Returns(data);

            // Act
            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(query, null);

            // Assert
            result.Rows[0].SumOfTime.Should().Be(100);
            result.Rows[1].SumOfTime.Should().Be(200);
            result.Rows[2].SumOfTime.Should().Be(300);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithSortBySumOfCost_SortsCorrectly()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "sumofcost", Descending = true };
            var data = new List<SummarisedWgTimeView>
            {
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "April", TotalTime = 100, TotalCost = 1000 },
                new() { WorkGroup = "WG2", ParentProject = "PRJ2", Name = "Project 2", MonthName = "April", TotalTime = 200, TotalCost = 2000 },
                new() { WorkGroup = "WG3", ParentProject = "PRJ3", Name = "Project 3", MonthName = "April", TotalTime = 300, TotalCost = 3000 }
            };

            _mockRepository.GetSummarisedWorkgroupTimeAsync(null, Arg.Any<CancellationToken>())
                .Returns(data);

            // Act
            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(query, null);

            // Assert
            result.Rows[0].SumOfCost.Should().Be(3000);
            result.Rows[1].SumOfCost.Should().Be(2000);
            result.Rows[2].SumOfCost.Should().Be(1000);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithInvalidMonthName_IgnoresRecord()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var data = new List<SummarisedWgTimeView>
            {
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "April", TotalTime = 100, TotalCost = 1000 },
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "InvalidMonth", TotalTime = 50, TotalCost = 500 }
            };

            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1", Arg.Any<CancellationToken>())
                .Returns(data);

            // Act
            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(query, "WG1");

            // Assert
            result.Rows[0].April.Should().Be(100);
            result.Rows[0].SumOfTime.Should().Be(150); // Still includes the invalid month in total
            result.Rows[0].SumOfCost.Should().Be(1500);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithNullMonthName_IgnoresRecord()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var data = new List<SummarisedWgTimeView>
            {
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "April", TotalTime = 100, TotalCost = 1000 },
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = null, TotalTime = 50, TotalCost = 500 }
            };

            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1", Arg.Any<CancellationToken>())
                .Returns(data);

            // Act
            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(query, "WG1");

            // Assert
            result.Rows[0].April.Should().Be(100);
            result.Rows[0].SumOfTime.Should().Be(150);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithMultipleProjects_GroupsCorrectly()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var data = new List<SummarisedWgTimeView>
            {
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "April", TotalTime = 100, TotalCost = 1000 },
                new() { WorkGroup = "WG1", ParentProject = "PRJ2", Name = "Project 2", MonthName = "April", TotalTime = 200, TotalCost = 2000 }
            };

            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1", Arg.Any<CancellationToken>())
                .Returns(data);

            // Act
            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(query, "WG1");

            // Assert
            result.Rows.Should().HaveCount(2);
            result.Rows[0].ParentProject.Should().Be("PRJ1");
            result.Rows[1].ParentProject.Should().Be("PRJ2");
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithNoSortBy_ReturnsDataInGroupingOrder()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = null };
            var data = new List<SummarisedWgTimeView>
            {
                new() { WorkGroup = "WG3", ParentProject = "PRJ3", Name = "Project 3", MonthName = "April", TotalTime = 300, TotalCost = 3000 },
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "April", TotalTime = 100, TotalCost = 1000 },
                new() { WorkGroup = "WG2", ParentProject = "PRJ2", Name = "Project 2", MonthName = "April", TotalTime = 200, TotalCost = 2000 }
            };

            _mockRepository.GetSummarisedWorkgroupTimeAsync(null, Arg.Any<CancellationToken>())
                .Returns(data);

            // Act
            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(query, null);

            // Assert
            // When SortBy is null, data is returned in grouping order (as-is from GroupBy)
            result.Rows[0].WorkGroup.Should().Be("WG3");
            result.Rows[1].WorkGroup.Should().Be("WG1");
            result.Rows[2].WorkGroup.Should().Be("WG2");
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WithEmptySearch_ReturnsAllRecords()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, Search = "" };
            var data = new List<SummarisedWgTimeView>
            {
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "April", TotalTime = 100, TotalCost = 1000 },
                new() { WorkGroup = "WG2", ParentProject = "PRJ2", Name = "Project 2", MonthName = "April", TotalTime = 200, TotalCost = 2000 }
            };

            _mockRepository.GetSummarisedWorkgroupTimeAsync(null, Arg.Any<CancellationToken>())
                .Returns(data);

            // Act
            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(query, null);

            // Assert
            result.Rows.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_CalculatesMonthsList_BasedOnDataPresent()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var data = new List<SummarisedWgTimeView>
            {
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "April", TotalTime = 100, TotalCost = 1000 },
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "June", TotalTime = 200, TotalCost = 2000 },
                new() { WorkGroup = "WG1", ParentProject = "PRJ1", Name = "Project 1", MonthName = "December", TotalTime = 300, TotalCost = 3000 }
            };

            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1", Arg.Any<CancellationToken>())
                .Returns(data);

            // Act
            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(query, "WG1");

            // Assert
            result.Months.Should().Contain([1, 3, 9]); // April=1, June=3, December=9
            result.Months.Should().NotContain([2, 4, 5, 6, 7, 8, 10, 11, 12]); // May, July, August, etc.
        }

        #endregion
    }
}
