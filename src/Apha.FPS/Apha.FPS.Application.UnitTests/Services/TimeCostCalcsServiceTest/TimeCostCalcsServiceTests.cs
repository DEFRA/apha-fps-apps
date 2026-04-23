using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.TimeCostCalcsServiceTest
{
    public class TimeCostCalcsServiceTests
    {
        private readonly ITimeCostCalcsRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly TimeCostCalcsService _sut;

        public TimeCostCalcsServiceTests()
        {
            _mockRepository = Substitute.For<ITimeCostCalcsRepository>();
            _mockMapper     = Substitute.For<IMapper>();
            _sut            = new TimeCostCalcsService(_mockRepository, _mockMapper);
        }

        private static QueryParameters<string> DefaultQuery(int page = 1, int pageSize = 10)
            => new QueryParameters<string> { Page = page, PageSize = pageSize };

        private static PaginationParameters<string> DefaultFilter(int page = 1, int pageSize = 10)
            => new PaginationParameters<string> { Page = page, PageSize = pageSize };

        private static PaginatedResult<TimeCostCalcsViewDto> MakePaginatedResult(IEnumerable<TimeCostCalcsViewDto> items)
        {
            var list = items.ToList();
            return new PaginatedResult<TimeCostCalcsViewDto>(list,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = list.Count });
        }

        private static PagedData<TimeCostCalcsView> MakePagedData(IEnumerable<TimeCostCalcsView> items)
        {
            var list = items.ToList();
            return new PagedData<TimeCostCalcsView>(list,
                new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = list.Count });
        }

        #region GetTimeCostCalcsByProjectAsync — Happy path

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_WithValidData_ReturnsMappedDtoList()
        {
            // Arrange
            var projectCode = "AH0033";
            var query       = DefaultQuery();
            var filter      = DefaultFilter();
            var entities    = new List<TimeCostCalcsView>
            {
                new() { Project = projectCode, StaffId = "S01", Name = "Alice", WorkGroup = "WG1", Month = 1, Time = 8, Cost = 100 },
                new() { Project = projectCode, StaffId = "S02", Name = "Bob",   WorkGroup = "WG2", Month = 2, Time = 6, Cost = 80  }
            };
            var pagedData = MakePagedData(entities);
            var expectedDtos = new List<TimeCostCalcsViewDto>
            {
                new() { Project = projectCode, StaffId = "S01", Name = "Alice", WorkGroup = "WG1", GradeCode = "G1", JobCode = "JB1", Month = 1, Time = 8, Cost = 100 },
                new() { Project = projectCode, StaffId = "S02", Name = "Bob",   WorkGroup = "WG2", GradeCode = "G2", JobCode = "JB2", Month = 2, Time = 6, Cost = 80  }
            };
            var expectedPaginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 };
            var expectedResult        = new PaginatedResult<TimeCostCalcsViewDto>(expectedDtos, expectedPaginationDto);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _mockRepository.GetTimeCostCalcsByProjectAsync(filter, projectCode).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<TimeCostCalcsViewDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetTimeCostCalcsByProjectAsync(query, projectCode);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.First().StaffId.Should().Be("S01");
            result.Data.First().Name.Should().Be("Alice");

            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetTimeCostCalcsByProjectAsync(filter, projectCode);
            _mockMapper.Received(1).Map<PaginatedResult<TimeCostCalcsViewDto>>(pagedData);
        }

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_WithEmptyData_ReturnsMappedEmptyList()
        {
            // Arrange
            var projectCode = "AH0033";
            var query       = DefaultQuery();
            var filter      = DefaultFilter();
            var pagedData   = MakePagedData(Enumerable.Empty<TimeCostCalcsView>());
            var emptyResult = new PaginatedResult<TimeCostCalcsViewDto>(
                new List<TimeCostCalcsViewDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 });

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _mockRepository.GetTimeCostCalcsByProjectAsync(filter, projectCode).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<TimeCostCalcsViewDto>>(pagedData).Returns(emptyResult);

            // Act
            var result = await _sut.GetTimeCostCalcsByProjectAsync(query, projectCode);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            await _mockRepository.Received(1).GetTimeCostCalcsByProjectAsync(filter, projectCode);
        }

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_MapsQueryToFilterBeforeCallingRepository()
        {
            // Arrange
            var projectCode = "AH0033";
            var query       = new QueryParameters<string> { Page = 2, PageSize = 5, SortBy = "name", Descending = true };
            var filter      = new PaginationParameters<string> { Page = 2, PageSize = 5, SortBy = "name", Descending = true };
            var pagedData   = MakePagedData(new List<TimeCostCalcsView>());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _mockRepository.GetTimeCostCalcsByProjectAsync(filter, projectCode).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<TimeCostCalcsViewDto>>(pagedData)
                .Returns(new PaginatedResult<TimeCostCalcsViewDto>(
                    new List<TimeCostCalcsViewDto>(), new PaginationDto()));

            // Act
            await _sut.GetTimeCostCalcsByProjectAsync(query, projectCode);

            // Assert — mapper called with original query before repo
            Received.InOrder(() =>
            {
                _mockMapper.Map<PaginationParameters<string>>(query);
                _mockRepository.GetTimeCostCalcsByProjectAsync(filter, projectCode);
            });
        }

        #endregion

        #region GetTimeCostCalcsByProjectAsync — Null / guard checks

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_WhenQueryIsNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _sut.GetTimeCostCalcsByProjectAsync(null!, "AH0033"));
        }

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var projectCode = "AH0033";
            var query       = DefaultQuery();
            var filter      = DefaultFilter();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _mockRepository.GetTimeCostCalcsByProjectAsync(filter, projectCode)
                .Throws(new Exception("DB connection failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _sut.GetTimeCostCalcsByProjectAsync(query, projectCode));
        }

        #endregion

        #region GetTimeCostCalcsByProjectAsync — Different project codes

        [Theory]
        [InlineData("AH0033")]
        [InlineData("PROJ001")]
        [InlineData("XYZ-99")]
        public async Task GetTimeCostCalcsByProjectAsync_PassesCorrectProjectCodeToRepository(string projectCode)
        {
            // Arrange
            var query     = DefaultQuery();
            var filter    = DefaultFilter();
            var pagedData = MakePagedData(new List<TimeCostCalcsView>());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _mockRepository.GetTimeCostCalcsByProjectAsync(filter, projectCode).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<TimeCostCalcsViewDto>>(pagedData)
                .Returns(new PaginatedResult<TimeCostCalcsViewDto>(
                    new List<TimeCostCalcsViewDto>(), new PaginationDto()));

            // Act
            await _sut.GetTimeCostCalcsByProjectAsync(query, projectCode);

            // Assert
            await _mockRepository.Received(1).GetTimeCostCalcsByProjectAsync(filter, projectCode);
        }

        #endregion

        #region GetTotalActualByProjectAsync

        [Fact]
        public async Task GetTotalActualByProjectAsync_CallsRepositoryAndReturnsDto()
        {
            // Arrange
            var projectCode = "AH0033";
            _mockRepository.GetTotalActualByProjectAsync(projectCode)
                .Returns((40.5, 5000.0));

            // Act
            var result = await _sut.GetTotalActualByProjectAsync(projectCode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(40.5,   result.TotalHours);
            Assert.Equal(5000.0, result.TotalCost);
            await _mockRepository.Received(1).GetTotalActualByProjectAsync(projectCode);
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_WhenRepositoryReturnsZero_ReturnsDtoWithZeros()
        {
            // Arrange
            var projectCode = "AH0033";
            _mockRepository.GetTotalActualByProjectAsync(projectCode)
                .Returns((0.0, 0.0));

            // Act
            var result = await _sut.GetTotalActualByProjectAsync(projectCode);

            // Assert
            Assert.Equal(0.0, result.TotalHours);
            Assert.Equal(0.0, result.TotalCost);
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var projectCode = "AH0033";
            _mockRepository.GetTotalActualByProjectAsync(projectCode)
                .Throws(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _sut.GetTotalActualByProjectAsync(projectCode));
        }

        #endregion

        #region DeleteTimeCostCalcsAsync

        [Fact]
        public async Task DeleteTimeCostCalcsAsync_WhenRecordExists_ReturnsTrue()
        {
            // Arrange
            _mockRepository.DeleteAsync("WG1", "JOB1", "AH0033", 1, "S01")
                .Returns(true);

            // Act
            var result = await _sut.DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 1, "S01");

            // Assert
            Assert.True(result);
            await _mockRepository.Received(1).DeleteAsync("WG1", "JOB1", "AH0033", 1, "S01");
        }

        [Fact]
        public async Task DeleteTimeCostCalcsAsync_WhenRecordNotFound_ReturnsFalse()
        {
            // Arrange
            _mockRepository.DeleteAsync("WG1", "JOB1", "AH0033", 1, "S01")
                .Returns(false);

            // Act
            var result = await _sut.DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 1, "S01");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteTimeCostCalcsAsync_PassesAllParamsToRepository()
        {
            // Arrange
            _mockRepository.DeleteAsync("WG1", "JOB1", "AH0033", 3.5, "S01")
                .Returns(true);

            // Act
            await _sut.DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 3.5, "S01");

            // Assert
            await _mockRepository.Received(1).DeleteAsync("WG1", "JOB1", "AH0033", 3.5, "S01");
        }

        #endregion
    }
}
