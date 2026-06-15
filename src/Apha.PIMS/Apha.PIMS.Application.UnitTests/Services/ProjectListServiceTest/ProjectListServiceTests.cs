using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Application.Services;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;

namespace Apha.PIMS.Application.UnitTests.Services.ProjectListServiceTest
{
    public class ProjectListServiceTests
    {
        private readonly IProjectListRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProjectListService _sut;

        public ProjectListServiceTests()
        {
            _mockRepository = Substitute.For<IProjectListRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new ProjectListService(_mockRepository, _mockMapper);
        }

        #region GetAllProjectsAsync

        [Fact]
        public async Task GetAllProjectsAsync_WithValidData_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "parentproject" };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            const int showWhichProjects = 2;

            var projectEntities = new List<ProjectListView>
            {
                new ProjectListView { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1", OnFps = "Yes" },
                new ProjectListView { Parentproject = "PP002", Program = "PROG2", Customer = "CUST2", OnFps = "No" }
            };

            var paginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var pagedData = new PagedData<ProjectListView>(projectEntities, paginationData);

            var expectedDtos = new List<ProjectListViewDto>
            {
                new ProjectListViewDto { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1", OnFps = "Yes" },
                new ProjectListViewDto { Parentproject = "PP002", Program = "PROG2", Customer = "CUST2", OnFps = "No" }
            };

            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var expectedResult = new PaginatedResult<ProjectListViewDto>(expectedDtos, paginationDto);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllProjectsAsync(paginationParams, showWhichProjects).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectListViewDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetAllProjectsAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.First().Parentproject.Should().Be("PP001");
            result.PaginationData.TotalRecords.Should().Be(2);

            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetAllProjectsAsync(paginationParams, showWhichProjects);
            _mockMapper.Received(1).Map<PaginatedResult<ProjectListViewDto>>(pagedData);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WithEmptyList_ReturnsMappedEmptyResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            const int showWhichProjects = 2;

            var emptyPagedData = new PagedData<ProjectListView>(
                new List<ProjectListView>(),
                new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 }
            );

            var emptyResult = new PaginatedResult<ProjectListViewDto>(
                Enumerable.Empty<ProjectListViewDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 }
            );

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllProjectsAsync(paginationParams, showWhichProjects).Returns(emptyPagedData);
            _mockMapper.Map<PaginatedResult<ProjectListViewDto>>(emptyPagedData).Returns(emptyResult);

            // Act
            var result = await _sut.GetAllProjectsAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);

            await _mockRepository.Received(1).GetAllProjectsAsync(paginationParams, showWhichProjects);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            const int showWhichProjects = 2;
            var expectedException = new Exception("Database connection failed");

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllProjectsAsync(paginationParams, showWhichProjects)
                .Returns(Task.FromException<PagedData<ProjectListView>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAllProjectsAsync(query)
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetAllProjectsAsync(paginationParams, showWhichProjects);
            _mockMapper.DidNotReceive().Map<PaginatedResult<ProjectListViewDto>>(Arg.Any<PagedData<ProjectListView>>());
        }

        #endregion

        #region GetYearlyDetailsByProjectAsync

        [Fact]
        public async Task GetYearlyDetailsByProjectAsync_WithValidParentProject_ReturnsMappedDtoList()
        {
            // Arrange
            var parentProject = "PP001";

            var entities = new List<Projects>
            {
                new Projects { Year = 2023, Parentproject = parentProject, Program = "PROG1", Customer = "CUST1", Manager = "MGR1" },
                new Projects { Year = 2024, Parentproject = parentProject, Program = "PROG1", Customer = "CUST1", Manager = "MGR1" }
            };

            var expectedDtos = new List<ProjectsDto>
            {
                new ProjectsDto { Year = 2023, Parentproject = parentProject, Program = "PROG1", Customer = "CUST1", Manager = "MGR1" },
                new ProjectsDto { Year = 2024, Parentproject = parentProject, Program = "PROG1", Customer = "CUST1", Manager = "MGR1" }
            };

            _mockRepository.GetYearlyDetailsByProjectAsync(parentProject)
                .Returns(Task.FromResult(entities));

            _mockMapper.Map<List<ProjectsDto>>(entities).Returns(expectedDtos);

            // Act
            var result = await _sut.GetYearlyDetailsByProjectAsync(parentProject);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().Year.Should().Be(2023);
            result.First().Parentproject.Should().Be("PP001");

            await _mockRepository.Received(1).GetYearlyDetailsByProjectAsync(parentProject);
            _mockMapper.Received(1).Map<List<ProjectsDto>>(entities);
        }

        [Fact]
        public async Task GetYearlyDetailsByProjectAsync_WithEmptyList_ReturnsEmptyDtoList()
        {
            // Arrange
            var parentProject = "PP001";
            var emptyEntities = new List<Projects>();
            var emptyDtos = new List<ProjectsDto>();

            _mockRepository.GetYearlyDetailsByProjectAsync(parentProject)
                .Returns(Task.FromResult(emptyEntities));

            _mockMapper.Map<List<ProjectsDto>>(emptyEntities).Returns(emptyDtos);

            // Act
            var result = await _sut.GetYearlyDetailsByProjectAsync(parentProject);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetYearlyDetailsByProjectAsync(parentProject);
            _mockMapper.Received(1).Map<List<ProjectsDto>>(emptyEntities);
        }

        [Fact]
        public async Task GetYearlyDetailsByProjectAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var parentProject = "PP001";
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetYearlyDetailsByProjectAsync(parentProject)
                .Returns(Task.FromException<List<Projects>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetYearlyDetailsByProjectAsync(parentProject)
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetYearlyDetailsByProjectAsync(parentProject);
            _mockMapper.DidNotReceive().Map<List<ProjectsDto>>(Arg.Any<List<Projects>>());
        }

        #endregion

        #region GetAllProjectsForDropDownAsync

        [Fact]
        public async Task GetAllProjectsForDropDownAsync_WithValidData_ReturnsMappedDtoList()
        {
            // Arrange
            var entities = new List<ProjectListView>
            {
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1", OnFps = "Yes" },
                new() { Parentproject = "PP002", Program = "PROG2", Customer = "CUST2", OnFps = "Yes" }
            };
            var expectedDtos = new List<ProjectListViewDto>
            {
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1", OnFps = "Yes" },
                new() { Parentproject = "PP002", Program = "PROG2", Customer = "CUST2", OnFps = "Yes" }
            };

            _mockRepository.GetAllProjectsForDropDownAsync().Returns(Task.FromResult(entities));
            _mockMapper.Map<List<ProjectListViewDto>>(entities).Returns(expectedDtos);

            // Act
            var result = await _sut.GetAllProjectsForDropDownAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().Parentproject.Should().Be("PP001");
            result.Should().AllSatisfy(p => p.OnFps.Should().Be("Yes"));

            await _mockRepository.Received(1).GetAllProjectsForDropDownAsync();
            _mockMapper.Received(1).Map<List<ProjectListViewDto>>(entities);
        }

        [Fact]
        public async Task GetAllProjectsForDropDownAsync_WithEmptyList_ReturnsEmptyDtoList()
        {
            // Arrange
            var emptyEntities = new List<ProjectListView>();
            var emptyDtos = new List<ProjectListViewDto>();

            _mockRepository.GetAllProjectsForDropDownAsync().Returns(Task.FromResult(emptyEntities));
            _mockMapper.Map<List<ProjectListViewDto>>(emptyEntities).Returns(emptyDtos);

            // Act
            var result = await _sut.GetAllProjectsForDropDownAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetAllProjectsForDropDownAsync();
            _mockMapper.Received(1).Map<List<ProjectListViewDto>>(emptyEntities);
        }

        [Fact]
        public async Task GetAllProjectsForDropDownAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetAllProjectsForDropDownAsync()
                .Returns(Task.FromException<List<ProjectListView>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAllProjectsForDropDownAsync()
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetAllProjectsForDropDownAsync();
            _mockMapper.DidNotReceive().Map<List<ProjectListViewDto>>(Arg.Any<List<ProjectListView>>());
        }

        #endregion

        #region GetAllProjectsForMilestoneAsync

        [Fact]
        public async Task GetAllProjectsForMilestoneAsync_WithValidData_ReturnsMappedDtoList()
        {
            // Arrange
            var entities = new List<ProjectListMilestone>
            {
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1", ProjectGroup = "GRP1" },
                new() { Parentproject = "PP002", Program = "PROG2", Customer = "CUST2", ProjectGroup = "GRP2" }
            };
            var expectedDtos = new List<ProjectListMilestoneDto>
            {
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1", ProjectGroup = "GRP1" },
                new() { Parentproject = "PP002", Program = "PROG2", Customer = "CUST2", ProjectGroup = "GRP2" }
            };

            _mockRepository.GetAllProjectsForMilestone().Returns(Task.FromResult(entities));
            _mockMapper.Map<List<ProjectListMilestoneDto>>(entities).Returns(expectedDtos);

            // Act
            var result = await _sut.GetAllProjectsForMilestoneAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().Parentproject.Should().Be("PP001");
            result.First().ProjectGroup.Should().Be("GRP1");

            await _mockRepository.Received(1).GetAllProjectsForMilestone();
            _mockMapper.Received(1).Map<List<ProjectListMilestoneDto>>(entities);
        }

        [Fact]
        public async Task GetAllProjectsForMilestoneAsync_WithEmptyList_ReturnsEmptyDtoList()
        {
            // Arrange
            var emptyEntities = new List<ProjectListMilestone>();
            var emptyDtos = new List<ProjectListMilestoneDto>();

            _mockRepository.GetAllProjectsForMilestone().Returns(Task.FromResult(emptyEntities));
            _mockMapper.Map<List<ProjectListMilestoneDto>>(emptyEntities).Returns(emptyDtos);

            // Act
            var result = await _sut.GetAllProjectsForMilestoneAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetAllProjectsForMilestone();
            _mockMapper.Received(1).Map<List<ProjectListMilestoneDto>>(emptyEntities);
        }

        [Fact]
        public async Task GetAllProjectsForMilestoneAsync_MapsProjectGroupCorrectly()
        {
            // Arrange
            var entities = new List<ProjectListMilestone>
            {
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1", ProjectGroup = null }
            };
            var expectedDtos = new List<ProjectListMilestoneDto>
            {
                new() { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1", ProjectGroup = null }
            };

            _mockRepository.GetAllProjectsForMilestone().Returns(Task.FromResult(entities));
            _mockMapper.Map<List<ProjectListMilestoneDto>>(entities).Returns(expectedDtos);

            // Act
            var result = await _sut.GetAllProjectsForMilestoneAsync();

            // Assert
            result.Should().HaveCount(1);
            result.First().ProjectGroup.Should().BeNull();

            await _mockRepository.Received(1).GetAllProjectsForMilestone();
        }

        [Fact]
        public async Task GetAllProjectsForMilestoneAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetAllProjectsForMilestone()
                .Returns(Task.FromException<List<ProjectListMilestone>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAllProjectsForMilestoneAsync()
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetAllProjectsForMilestone();
            _mockMapper.DidNotReceive().Map<List<ProjectListMilestoneDto>>(Arg.Any<List<ProjectListMilestone>>());
        }

        #endregion
    }
}
