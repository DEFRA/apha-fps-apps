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

namespace Apha.FPS.Application.UnitTests.Services.ProjectServiceTest
{
    public class ProjectProfitabilityServiceTests
    {
        private readonly IProjectRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProjectService _sut;

        public ProjectProfitabilityServiceTests()
        {
            _mockRepository = Substitute.For<IProjectRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new ProjectService(_mockRepository, _mockMapper);
        }

        #region GetProjectProfitabilityAsync

        [Fact]
        public async Task GetProjectProfitabilityAsync_WithValidData_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var workTypeFilter = "all";
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var views = new List<ProjectProfitabilityView>
            {
                new() { JobCode = "PP001", JcTotalStaffCosts = 1000m, BudgetCvl = 5000m, JcProfit = 4000m, TargetProfit = 3500m, OffTarget = 500m, ProgramNo = "P001" },
                new() { JobCode = "PP002", JcTotalStaffCosts = 2000m, BudgetCvl = 6000m, JcProfit = 4000m, TargetProfit = 3000m, OffTarget = 1000m, ProgramNo = "P001" }
            };
            var pagedData = new PagedData<ProjectProfitabilityView>(
                views, new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = 2 });

            var expectedDtos = new List<ProjectProfitabilityDto>
            {
                new() { JobCode = "PP001", JcProfit = 4000m },
                new() { JobCode = "PP002", JcProfit = 4000m }
            };
            var expectedResult = new PaginatedResult<ProjectProfitabilityDto>(
                expectedDtos, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 });

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectProfitabilityAsync(paginationParams, programNo, workTypeFilter)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectProfitabilityDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetProjectProfitabilityAsync(query, programNo, workTypeFilter);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.First().JobCode.Should().Be("PP001");
            await _mockRepository.Received(1).GetProjectProfitabilityAsync(paginationParams, programNo, workTypeFilter);
        }

        [Fact]
        public async Task GetProjectProfitabilityAsync_WithEmptyResult_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var workTypeFilter = "all";
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var pagedData = new PagedData<ProjectProfitabilityView>(
                new List<ProjectProfitabilityView>(), new PaginationData { TotalRecords = 0 });
            var expectedResult = new PaginatedResult<ProjectProfitabilityDto>(
                new List<ProjectProfitabilityDto>(), new PaginationDto { TotalRecords = 0 });

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectProfitabilityAsync(paginationParams, programNo, workTypeFilter)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectProfitabilityDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetProjectProfitabilityAsync(query, programNo, workTypeFilter);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
        }

        [Theory]
        [InlineData("approved")]
        [InlineData("not-approved")]
        [InlineData("all")]
        public async Task GetProjectProfitabilityAsync_WithWorkTypeFilters_PassesFilterToRepository(string workTypeFilter)
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PagedData<ProjectProfitabilityView>(
                new List<ProjectProfitabilityView>(), new PaginationData());
            var expectedResult = new PaginatedResult<ProjectProfitabilityDto>(
                new List<ProjectProfitabilityDto>(), new PaginationDto());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectProfitabilityAsync(paginationParams, programNo, workTypeFilter)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectProfitabilityDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetProjectProfitabilityAsync(query, programNo, workTypeFilter);

            // Assert
            result.Should().NotBeNull();
            await _mockRepository.Received(1).GetProjectProfitabilityAsync(paginationParams, programNo, workTypeFilter);
        }

        [Fact]
        public async Task GetProjectProfitabilityAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectProfitabilityAsync(paginationParams, programNo, "all")
                .Returns(Task.FromException<PagedData<ProjectProfitabilityView>>(new Exception("DB error")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _sut.GetProjectProfitabilityAsync(query, programNo, "all"));
        }

        [Fact]
        public async Task GetProjectProfitabilityAsync_MapperIsCalledTwice_ForQueryAndResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PagedData<ProjectProfitabilityView>(
                new List<ProjectProfitabilityView>(), new PaginationData());
            var expectedResult = new PaginatedResult<ProjectProfitabilityDto>(
                new List<ProjectProfitabilityDto>(), new PaginationDto());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectProfitabilityAsync(paginationParams, programNo, "all")
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectProfitabilityDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.GetProjectProfitabilityAsync(query, programNo, "all");

            // Assert
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            _mockMapper.Received(1).Map<PaginatedResult<ProjectProfitabilityDto>>(pagedData);
        }

        #endregion
    }
}
