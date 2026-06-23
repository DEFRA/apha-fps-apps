using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Application.UnitTests.Services.ProjectServiceTest
{
    public class ProjectProfitabilityVlaServiceTests
    {
        private readonly IProjectRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProjectService _sut;

        public ProjectProfitabilityVlaServiceTests()
        {
            _mockRepository = Substitute.For<IProjectRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new ProjectService(_mockRepository, _mockMapper);
        }

        #region GetProjectProfitabilityVlaAsync

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithValidQuery_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 15 };
            var projectStatus = "Approved";
            var programNo = "P001";
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 15 };

            var views = new List<ProjectProfitabilityVlaView>
            {
                new() { JobCode = "PP001", StaffCosts = 1000m, Budget = 5000m, Profit = 4000m, TargetProfit = 3500m, OffTarget = 500m },
                new() { JobCode = "PP002", StaffCosts = 2000m, Budget = 6000m, Profit = 4000m, TargetProfit = 3000m, OffTarget = 1000m }
            };
            var pagedData = new PagedData<ProjectProfitabilityVlaView>(
                views, new PaginationData { PageNumber = 1, PageSize = 15, TotalRecords = 2 });

            var expectedResult = new PaginatedResult<ProjectProfitabilityVlaDto>(
                new List<ProjectProfitabilityVlaDto>
                {
                    new() { JobCode = "PP001", Profit = 4000m },
                    new() { JobCode = "PP002", Profit = 4000m }
                },
                new PaginationDto { PageNumber = 1, PageSize = 15, TotalRecords = 2 });

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectProfitabilityVlaAsync(paginationParams, projectStatus, programNo, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectProfitabilityVlaDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetProjectProfitabilityVlaAsync(query, projectStatus, programNo);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedResult);
            await _mockRepository.Received(1).GetProjectProfitabilityVlaAsync(paginationParams, projectStatus, programNo, null, null);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            _mockMapper.Received(1).Map<PaginatedResult<ProjectProfitabilityVlaDto>>(pagedData);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithEmptyRepositoryResult_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 15 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 15 };
            var emptyPagedData = new PagedData<ProjectProfitabilityVlaView>(
                new List<ProjectProfitabilityVlaView>(),
                new PaginationData { PageNumber = 1, PageSize = 15, TotalRecords = 0 });
            var emptyResult = new PaginatedResult<ProjectProfitabilityVlaDto>(
                new List<ProjectProfitabilityVlaDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 15, TotalRecords = 0 });

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectProfitabilityVlaAsync(paginationParams, null, null, null, null).Returns(emptyPagedData);
            _mockMapper.Map<PaginatedResult<ProjectProfitabilityVlaDto>>(emptyPagedData).Returns(emptyResult);

            // Act
            var result = await _sut.GetProjectProfitabilityVlaAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithNullQuery_ThrowsArgumentNullException()
        {
            // Act & Assert — ArgumentNullException.ThrowIfNull(query) guard in service
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.GetProjectProfitabilityVlaAsync(null!));
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithNoFilters_DelegatesToRepositoryWithMappedParams()
        {
            // Arrange — no filter fields set; verifies delegation path with null filters
            var query = new QueryParameters<string> { Page = 2, PageSize = 25 };
            var paginationParams = new PaginationParameters<string> { Page = 2, PageSize = 25 };
            var pagedData = new PagedData<ProjectProfitabilityVlaView>(
                new List<ProjectProfitabilityVlaView>(),
                new PaginationData { PageNumber = 2, PageSize = 25, TotalRecords = 0 });
            var emptyResult = new PaginatedResult<ProjectProfitabilityVlaDto>(
                new List<ProjectProfitabilityVlaDto>(),
                new PaginationDto { PageNumber = 2, PageSize = 25, TotalRecords = 0 });

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectProfitabilityVlaAsync(paginationParams, null, null, null, null).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectProfitabilityVlaDto>>(pagedData).Returns(emptyResult);

            // Act
            await _sut.GetProjectProfitabilityVlaAsync(query);

            // Assert
            await _mockRepository.Received(1).GetProjectProfitabilityVlaAsync(paginationParams, null, null, null, null);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 15 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 15 };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectProfitabilityVlaAsync(paginationParams, null, null, null, null)
                .ThrowsAsync(new InvalidOperationException("DB connection failed"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.GetProjectProfitabilityVlaAsync(query));
        }

        #endregion
    }
}
