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
    public class ProjectGroupProfitabilityServiceTests
    {
        private readonly IProjectRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProjectService _sut;

        public ProjectGroupProfitabilityServiceTests()
        {
            _mockRepository = Substitute.For<IProjectRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new ProjectService(_mockRepository, _mockMapper);
        }

        // ── GetProjectGroupProfitabilityAsync ─────────────────────────────────

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_WithValidData_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectGroup = "Group1";
            var workTypeFilter = "all";
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var views = new List<ProjectProfitabilityView>
            {
                new() { JobCode = "PP001", JcTotalStaffCosts = 1000m, BudgetCvl = 5000m, JcProfit = 4000m, TargetProfit = 3500m, OffTarget = 500m, ProgramNo = "P001" },
                new() { JobCode = "PP002", JcTotalStaffCosts = 2000m, BudgetCvl = 6000m, JcProfit = 4000m, TargetProfit = 3000m, OffTarget = 1000m, ProgramNo = "P002" }
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
            _mockRepository.GetProjectGroupProfitabilityAsync(paginationParams, projectGroup, workTypeFilter)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectProfitabilityDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetProjectGroupProfitabilityAsync(query, projectGroup, workTypeFilter);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.First().JobCode.Should().Be("PP001");
            await _mockRepository.Received(1).GetProjectGroupProfitabilityAsync(paginationParams, projectGroup, workTypeFilter);
        }

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_WithEmptyResult_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectGroup = "Group1";
            var workTypeFilter = "all";
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var pagedData = new PagedData<ProjectProfitabilityView>(
                new List<ProjectProfitabilityView>(), new PaginationData { TotalRecords = 0 });
            var expectedResult = new PaginatedResult<ProjectProfitabilityDto>(
                new List<ProjectProfitabilityDto>(), new PaginationDto { TotalRecords = 0 });

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectGroupProfitabilityAsync(paginationParams, projectGroup, workTypeFilter)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectProfitabilityDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetProjectGroupProfitabilityAsync(query, projectGroup, workTypeFilter);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
        }

        [Theory]
        [InlineData("approved")]
        [InlineData("not-approved")]
        [InlineData("all")]
        public async Task GetProjectGroupProfitabilityAsync_WithWorkTypeFilters_PassesFilterToRepository(string workTypeFilter)
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectGroup = "Group1";
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PagedData<ProjectProfitabilityView>(
                new List<ProjectProfitabilityView>(), new PaginationData());
            var expectedResult = new PaginatedResult<ProjectProfitabilityDto>(
                new List<ProjectProfitabilityDto>(), new PaginationDto());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectGroupProfitabilityAsync(paginationParams, projectGroup, workTypeFilter)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectProfitabilityDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetProjectGroupProfitabilityAsync(query, projectGroup, workTypeFilter);

            // Assert
            result.Should().NotBeNull();
            await _mockRepository.Received(1).GetProjectGroupProfitabilityAsync(paginationParams, projectGroup, workTypeFilter);
        }

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectGroup = "Group1";
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectGroupProfitabilityAsync(paginationParams, projectGroup, "all")
                .Returns(Task.FromException<PagedData<ProjectProfitabilityView>>(new Exception("DB error")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _sut.GetProjectGroupProfitabilityAsync(query, projectGroup, "all"));
        }

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_MapperIsCalledForQueryAndResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectGroup = "Group1";
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PagedData<ProjectProfitabilityView>(
                new List<ProjectProfitabilityView>(), new PaginationData());
            var expectedResult = new PaginatedResult<ProjectProfitabilityDto>(
                new List<ProjectProfitabilityDto>(), new PaginationDto());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectGroupProfitabilityAsync(paginationParams, projectGroup, "all")
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectProfitabilityDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.GetProjectGroupProfitabilityAsync(query, projectGroup, "all");

            // Assert
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            _mockMapper.Received(1).Map<PaginatedResult<ProjectProfitabilityDto>>(pagedData);
        }

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_DoesNotCallGetProjectProfitabilityAsync()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectGroup = "Group1";
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PagedData<ProjectProfitabilityView>(
                new List<ProjectProfitabilityView>(), new PaginationData());
            var expectedResult = new PaginatedResult<ProjectProfitabilityDto>(
                new List<ProjectProfitabilityDto>(), new PaginationDto());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectGroupProfitabilityAsync(paginationParams, projectGroup, "all")
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectProfitabilityDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.GetProjectGroupProfitabilityAsync(query, projectGroup, "all");

            // Assert — project group path must NOT call programme profitability
            await _mockRepository.DidNotReceive().GetProjectProfitabilityAsync(
                Arg.Any<PaginationParameters<string>>(), Arg.Any<string>(), Arg.Any<string>());
        }
    }
}
