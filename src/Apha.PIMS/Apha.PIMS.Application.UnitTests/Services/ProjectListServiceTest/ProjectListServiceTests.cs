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

        #region GetFpsProjectByIdAsync

        [Fact]
        public async Task GetFpsProjectByIdAsync_WithValidParentProject_ReturnsMappedDto()
        {
            // Arrange
            var parentProject = "PP001";

            var entity = new Project
            {
                Parentproject = parentProject,
                Projecttitle = "FMD Survey",
                Disease = "FMD",
                Contract = "CON001",
                Projectstatus = "Active",
                Shorttitle = "FMD",
                Costbookno = "CB001"
            };

            var expectedDto = new ProjectDto
            {
                Parentproject = parentProject,
                Projecttitle = "FMD Survey",
                Disease = "FMD",
                Contract = "CON001",
                Projectstatus = "Active"
            };

            _mockRepository.GetFpsProjectByIdAsync(parentProject)
                .Returns(Task.FromResult<Project?>(entity));

            _mockMapper.Map<ProjectDto>(entity).Returns(expectedDto);

            // Act
            var result = await _sut.GetFpsProjectByIdAsync(parentProject);

            // Assert
            result.Should().NotBeNull();
            result!.Parentproject.Should().Be("PP001");
            result.Projecttitle.Should().Be("FMD Survey");
            result.Disease.Should().Be("FMD");

            await _mockRepository.Received(1).GetFpsProjectByIdAsync(parentProject);
            _mockMapper.Received(1).Map<ProjectDto>(entity);
        }

        [Fact]
        public async Task GetFpsProjectByIdAsync_WhenProjectNotFound_ReturnsNull()
        {
            // Arrange
            var parentProject = "UNKNOWN";

            _mockRepository.GetFpsProjectByIdAsync(parentProject)
                .Returns(Task.FromResult<Project?>(null));

            // Act
            var result = await _sut.GetFpsProjectByIdAsync(parentProject);

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetFpsProjectByIdAsync(parentProject);
            _mockMapper.DidNotReceive().Map<ProjectDto>(Arg.Any<Project>());
        }

        [Theory]
        [InlineData("")]
        public async Task GetFpsProjectByIdAsync_WithNullOrEmptyId_ReturnsNull(string parentProject)
        {
            // Arrange
            _mockRepository.GetFpsProjectByIdAsync(parentProject)
                .Returns(Task.FromResult<Project?>(null));

            // Act
            var result = await _sut.GetFpsProjectByIdAsync(parentProject);

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetFpsProjectByIdAsync(parentProject);
        }

        [Fact]
        public async Task GetFpsProjectByIdAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var parentProject = "PP001";
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetFpsProjectByIdAsync(parentProject)
                .Returns(Task.FromException<Project?>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetFpsProjectByIdAsync(parentProject)
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetFpsProjectByIdAsync(parentProject);
            _mockMapper.DidNotReceive().Map<ProjectDto>(Arg.Any<Project>());
        }

        #endregion

        #region GetProposedProjectByIdAsync

        [Fact]
        public async Task GetProposedProjectByIdAsync_WithValidParentProject_ReturnsMappedDto()
        {
            // Arrange
            var parentProject = "PP001";

            var entity = new ProposedProject
            {
                Id = 1,
                Parentproject = parentProject,
                Projecttitle = "TB Eradication",
                Program = "PROG1",
                Customer = "CUST1",
                Manager = "MGR1",
                Projectstatus = "Proposed",
                Disease = "TB"
            };

            var expectedDto = new ProposedProjectDto
            {
                Id = 1,
                Parentproject = parentProject,
                Projecttitle = "TB Eradication",
                Program = "PROG1",
                Customer = "CUST1",
                Manager = "MGR1",
                Projectstatus = "Proposed",
                Disease = "TB"
            };

            _mockRepository.GetProposedProjectByIdAsync(parentProject)
                .Returns(Task.FromResult<ProposedProject?>(entity));

            _mockMapper.Map<ProposedProjectDto>(entity).Returns(expectedDto);

            // Act
            var result = await _sut.GetProposedProjectByIdAsync(parentProject);

            // Assert
            result.Should().NotBeNull();
            result!.Parentproject.Should().Be("PP001");
            result.Projecttitle.Should().Be("TB Eradication");
            result.Projectstatus.Should().Be("Proposed");

            await _mockRepository.Received(1).GetProposedProjectByIdAsync(parentProject);
            _mockMapper.Received(1).Map<ProposedProjectDto>(entity);
        }

        [Fact]
        public async Task GetProposedProjectByIdAsync_WhenProjectNotFound_ReturnsNull()
        {
            // Arrange
            var parentProject = "UNKNOWN";

            _mockRepository.GetProposedProjectByIdAsync(parentProject)
                .Returns(Task.FromResult<ProposedProject?>(null));

            // Act
            var result = await _sut.GetProposedProjectByIdAsync(parentProject);

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetProposedProjectByIdAsync(parentProject);
            _mockMapper.DidNotReceive().Map<ProposedProjectDto>(Arg.Any<ProposedProject>());
        }

        [Theory]
        [InlineData("")]
        public async Task GetProposedProjectByIdAsync_WithNullOrEmptyId_ReturnsNull(string parentProject)
        {
            // Arrange
            _mockRepository.GetProposedProjectByIdAsync(parentProject)
                .Returns(Task.FromResult<ProposedProject?>(null));

            // Act
            var result = await _sut.GetProposedProjectByIdAsync(parentProject);

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetProposedProjectByIdAsync(parentProject);
        }

        [Fact]
        public async Task GetProposedProjectByIdAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var parentProject = "PP001";
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetProposedProjectByIdAsync(parentProject)
                .Returns(Task.FromException<ProposedProject?>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetProposedProjectByIdAsync(parentProject)
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetProposedProjectByIdAsync(parentProject);
            _mockMapper.DidNotReceive().Map<ProposedProjectDto>(Arg.Any<ProposedProject>());
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

        #region AddProjectAsync

        [Fact]
        public async Task AddProjectAsync_WithValidDto_ReturnsMappedCreatedDto()
        {
            // Arrange
            var dto = new ProposedProjectDto
            {
                Parentproject = "PP001",
                Projecttitle = "New Project",
                Program = "PROG1",
                Customer = "CUST1",
                Manager = "MGR1",
                Projectstatus = "Proposed",
                Disease = "FMD"
            };

            var entity = new ProposedProject
            {
                Parentproject = "PP001",
                Projecttitle = "New Project",
                Program = "PROG1",
                Customer = "CUST1",
                Manager = "MGR1",
                Projectstatus = "Proposed",
                Disease = "FMD"
            };

            var createdEntity = new ProposedProject
            {
                Id = 42,
                Parentproject = "PP001",
                Projecttitle = "New Project",
                Program = "PROG1",
                Customer = "CUST1",
                Manager = "MGR1",
                Projectstatus = "Proposed",
                Disease = "FMD"
            };

            var expectedDto = new ProposedProjectDto
            {
                Id = 42,
                Parentproject = "PP001",
                Projecttitle = "New Project",
                Program = "PROG1",
                Customer = "CUST1",
                Manager = "MGR1",
                Projectstatus = "Proposed",
                Disease = "FMD"
            };

            _mockMapper.Map<ProposedProject>(dto).Returns(entity);
            _mockRepository.AddProjectAsync(entity).Returns(Task.FromResult(createdEntity));
            _mockMapper.Map<ProposedProjectDto>(createdEntity).Returns(expectedDto);

            // Act
            var result = await _sut.AddProjectAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(42);
            result.Parentproject.Should().Be("PP001");
            result.Projecttitle.Should().Be("New Project");

            _mockMapper.Received(1).Map<ProposedProject>(dto);
            await _mockRepository.Received(1).AddProjectAsync(entity);
            _mockMapper.Received(1).Map<ProposedProjectDto>(createdEntity);
        }

        [Fact]
        public async Task AddProjectAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var dto = new ProposedProjectDto { Parentproject = "PP001", Projecttitle = "New Project" };
            var entity = new ProposedProject { Parentproject = "PP001", Projecttitle = "New Project" };
            var expectedException = new Exception("Database connection failed");

            _mockMapper.Map<ProposedProject>(dto).Returns(entity);
            _mockRepository.AddProjectAsync(entity)
                .Returns(Task.FromException<ProposedProject>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.AddProjectAsync(dto)
            );

            exception.Message.Should().Be("Database connection failed");

            _mockMapper.Received(1).Map<ProposedProject>(dto);
            await _mockRepository.Received(1).AddProjectAsync(entity);
            _mockMapper.DidNotReceive().Map<ProposedProjectDto>(Arg.Any<ProposedProject>());
        }

        #endregion
        
    }
}
