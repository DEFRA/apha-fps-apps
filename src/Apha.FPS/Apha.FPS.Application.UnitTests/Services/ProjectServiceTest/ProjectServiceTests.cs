using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.ProjectServiceTest
{
    public class ProjectServiceTests
    {
        private readonly IProjectRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProjectService _sut;

        public ProjectServiceTests()
        {
            _mockRepository = Substitute.For<IProjectRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new ProjectService(_mockRepository, _mockMapper);
        }

        #region GetAllProjectsAsync

        [Fact]
        public async Task GetAllProjectsAsync_WithValidData_ReturnsMappedDtoList()
        {
            // Arrange
            var projectEntities = new List<ProjectView>
            {
                new ProjectView { ParentProject = "PROJ001", ProjectTitle = "FMD Survey",    ProjectStatus = "Active",   Disease = "FMD",  Contract = "CON001" },
                new ProjectView { ParentProject = "PROJ002", ProjectTitle = "TB Eradication", ProjectStatus = "Active",  Disease = "TB",   Contract = "CON002" }
            };

            var expectedDtos = new List<ProjectDto>
            {
                new ProjectDto { ParentProject = "PROJ001", ProjectTitle = "FMD Survey",     ProjectStatus = "Active",  Disease = "FMD", Contract = "CON001" },
                new ProjectDto { ParentProject = "PROJ002", ProjectTitle = "TB Eradication",  ProjectStatus = "Active",  Disease = "TB",  Contract = "CON002" }
            };

            _mockRepository.GetAllProjectsAsync()
                .Returns(Task.FromResult<IEnumerable<ProjectView>>(projectEntities));

            _mockMapper.Map<IEnumerable<ProjectDto>>(projectEntities)
                .Returns(expectedDtos);

            // Act
            var result = await _sut.GetAllProjectsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().ParentProject.Should().Be("PROJ001");
            result.First().ProjectTitle.Should().Be("FMD Survey");

            await _mockRepository.Received(1).GetAllProjectsAsync();
            _mockMapper.Received(1).Map<IEnumerable<ProjectDto>>(projectEntities);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WithEmptyList_ReturnsEmptyDtoList()
        {
            // Arrange
            var emptyEntities = new List<ProjectView>();
            var emptyDtos = new List<ProjectDto>();

            _mockRepository.GetAllProjectsAsync()
                .Returns(Task.FromResult<IEnumerable<ProjectView>>(emptyEntities));

            _mockMapper.Map<IEnumerable<ProjectDto>>(emptyEntities)
                .Returns(emptyDtos);

            // Act
            var result = await _sut.GetAllProjectsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetAllProjectsAsync();
            _mockMapper.Received(1).Map<IEnumerable<ProjectDto>>(emptyEntities);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WhenRepositoryReturnsNull_ReturnsNull()
        {
            // Arrange
            _mockRepository.GetAllProjectsAsync()
                .Returns(Task.FromResult<IEnumerable<ProjectView>>(null!));

            _mockMapper.Map<IEnumerable<ProjectDto>>(null)
                .Returns((IEnumerable<ProjectDto>?)null);

            // Act
            var result = await _sut.GetAllProjectsAsync();

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetAllProjectsAsync();
            _mockMapper.Received(1).Map<IEnumerable<ProjectDto>>(null);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetAllProjectsAsync()
                .Returns(Task.FromException<IEnumerable<ProjectView>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAllProjectsAsync()
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetAllProjectsAsync();
            _mockMapper.DidNotReceive().Map<IEnumerable<ProjectDto>>(Arg.Any<IEnumerable<Project>>());
        }

        #endregion

        #region GetProjectByIdAsync

        [Fact]
        public async Task GetProjectByIdAsync_WithValidParentProject_ReturnsMappedDto()
        {
            // Arrange
            var parentProject = "PROJ001";

            var projectEntity = new Project
            {
                ParentProject = parentProject,
                ProjectTitle = "FMD Survey",
                ProjectStatus = "Active",
                Disease = "FMD",
                Contract = "CON001"
            };

            var expectedDto = new ProjectDto
            {
                ParentProject = parentProject,
                ProjectTitle = "FMD Survey",
                ProjectStatus = "Active",
                Disease = "FMD",
                Contract = "CON001"
            };

            _mockRepository.GetProjectByIdAsync(parentProject)
                .Returns(Task.FromResult<Project?>(projectEntity));

            _mockMapper.Map<ProjectDto>(projectEntity)
                .Returns(expectedDto);

            // Act
            var result = await _sut.GetProjectByIdAsync(parentProject);

            // Assert
            result.Should().NotBeNull();
            result.ParentProject.Should().Be("PROJ001");
            result.ProjectTitle.Should().Be("FMD Survey");
            result.ProjectStatus.Should().Be("Active");

            await _mockRepository.Received(1).GetProjectByIdAsync(parentProject);
            _mockMapper.Received(1).Map<ProjectDto>(projectEntity);
        }

        [Fact]
        public async Task GetProjectByIdAsync_WhenProjectNotFound_ReturnsNull()
        {
            // Arrange
            var parentProject = "PROJ999";

            _mockRepository.GetProjectByIdAsync(parentProject)
                .Returns(Task.FromResult<Project?>(null));

            // Act
            var result = await _sut.GetProjectByIdAsync(parentProject);

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetProjectByIdAsync(parentProject);
            _mockMapper.DidNotReceive().Map<ProjectDto>(Arg.Any<Project>());
        }

        [Theory]        
        [InlineData("")]
        public async Task GetProjectByIdAsync_WithNullOrEmptyId_ReturnsNull(string parentProject)
        {
            // Arrange
            _mockRepository.GetProjectByIdAsync(parentProject)
                .Returns(Task.FromResult<Project?>(null));

            _mockMapper.Map<ProjectDto>(Arg.Any<Project?>())
                .Returns((ProjectDto?)null);

            // Act
            var result = await _sut.GetProjectByIdAsync(parentProject);

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetProjectByIdAsync(parentProject);
        }

        [Fact]
        public async Task GetProjectByIdAsync_WhenMapperReturnsNull_ReturnsNull()
        {
            // Arrange
            var parentProject = "PROJ001";

            var projectEntity = new Project
            {
                ParentProject = parentProject,
                ProjectTitle  = "FMD Survey",
                ProjectStatus = "Active",
                Disease       = "FMD",
                Contract      = "CON001"
            };

            _mockRepository.GetProjectByIdAsync(parentProject)
                .Returns(Task.FromResult<Project?>(projectEntity));

            _mockMapper.Map<ProjectDto>(projectEntity)
                .Returns((ProjectDto?)null);

            // Act
            var result = await _sut.GetProjectByIdAsync(parentProject);

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetProjectByIdAsync(parentProject);
            _mockMapper.Received(1).Map<ProjectDto>(projectEntity);
        }

        [Fact]
        public async Task GetProjectByIdAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var parentProject = "PROJ001";
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetProjectByIdAsync(parentProject)
                .Returns(Task.FromException<Project?>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetProjectByIdAsync(parentProject)
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetProjectByIdAsync(parentProject);
            _mockMapper.DidNotReceive().Map<ProjectDto>(Arg.Any<Project>());
        }

        #endregion

        #region GetProjectsByProgramAsync

        [Fact]
        public async Task GetProjectsByProgramAsync_CallsRepositoryWithMappedParameters_AndReturnsMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "parentproject" };
            var programNo = "P001";
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var projectEntities = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project", Program = "P001", BudgetCvl = 1000m, IsDefraProject = 1 },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Project",  Program = "P001", BudgetCvl = 2000m, IsDefraProject = 0 }
            };
            var paginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var pagedData = new PagedData<Project>(projectEntities, paginationData);
            var expectedDtos = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Project" }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var expectedResult = new PaginatedResult<ProjectDto>(expectedDtos, paginationDto);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectsByProgramAsync(paginationParams, programNo).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetProjectsByProgramAsync(query, programNo);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.First().ParentProject.Should().Be("PP001");
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetProjectsByProgramAsync(paginationParams, programNo);
            _mockMapper.Received(1).Map<PaginatedResult<ProjectDto>>(pagedData);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_WithEmptyResult_ReturnsMappedEmptyResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var emptyPagedData = new PagedData<Project>(
                Enumerable.Empty<Project>(),
                new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 }
            );
            var emptyResult = new PaginatedResult<ProjectDto>(
                Enumerable.Empty<ProjectDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 }
            );

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectsByProgramAsync(paginationParams, programNo).Returns(emptyPagedData);
            _mockMapper.Map<PaginatedResult<ProjectDto>>(emptyPagedData).Returns(emptyResult);

            // Act
            var result = await _sut.GetProjectsByProgramAsync(query, programNo);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
            await _mockRepository.Received(1).GetProjectsByProgramAsync(paginationParams, programNo);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var expectedException = new Exception("Database connection failed");

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectsByProgramAsync(paginationParams, programNo)
                .Returns(Task.FromException<PagedData<Project>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetProjectsByProgramAsync(query, programNo)
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetProjectsByProgramAsync(paginationParams, programNo);
            _mockMapper.DidNotReceive().Map<PaginatedResult<ProjectDto>>(Arg.Any<PagedData<Project>>());
        }

        #endregion

        #region GetAllPactProjectsAsync

        [Fact]
        public async Task GetAllPactProjectsAsync_WithValidData_ReturnsMappedDtoList()
        {
            // Arrange
            var pactEntities = new List<PactProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "PACT Survey" },
                new() { ParentProject = "PP002", ProjectTitle = "PACT Eradication" }
            };
            var expectedDtos = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "PACT Survey" },
                new() { ParentProject = "PP002", ProjectTitle = "PACT Eradication" }
            };

            _mockRepository.GetAllPactProjectsAsync()
                .Returns(Task.FromResult<IEnumerable<PactProjectView>>(pactEntities));
            _mockMapper.Map<IEnumerable<ProjectDto>>(pactEntities)
                .Returns(expectedDtos);

            // Act
            var result = await _sut.GetAllPactProjectsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().ParentProject.Should().Be("PP001");
            await _mockRepository.Received(1).GetAllPactProjectsAsync();
            _mockMapper.Received(1).Map<IEnumerable<ProjectDto>>(pactEntities);
        }

        [Fact]
        public async Task GetAllPactProjectsAsync_WithEmptyList_ReturnsEmptyDtoList()
        {
            // Arrange
            var emptyEntities = new List<PactProjectView>();
            var emptyDtos = new List<ProjectDto>();

            _mockRepository.GetAllPactProjectsAsync()
                .Returns(Task.FromResult<IEnumerable<PactProjectView>>(emptyEntities));
            _mockMapper.Map<IEnumerable<ProjectDto>>(emptyEntities)
                .Returns(emptyDtos);

            // Act
            var result = await _sut.GetAllPactProjectsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            await _mockRepository.Received(1).GetAllPactProjectsAsync();
        }

        [Fact]
        public async Task GetAllPactProjectsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.GetAllPactProjectsAsync()
                .Returns(Task.FromException<IEnumerable<PactProjectView>>(new Exception("Database connection failed")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAllPactProjectsAsync()
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetAllPactProjectsAsync();
            _mockMapper.DidNotReceive().Map<IEnumerable<ProjectDto>>(Arg.Any<IEnumerable<PactProjectView>>());
        }

        #endregion

        #region GetPagedProjectsAsync

        [Fact]
        public async Task GetPagedProjectsAsync_WithValidQuery_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var projectEntities = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" }
            };
            var paginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 };
            var pagedData = new PagedData<Project>(projectEntities, paginationData);
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 };
            var expectedResult = new PaginatedResult<ProjectDto>(
                new List<ProjectDto> { new() { ParentProject = "PP001" } }, paginationDto);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetPagedProjectsAsync(paginationParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetPagedProjectsAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
            result.Data.First().ParentProject.Should().Be("PP001");
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetPagedProjectsAsync(paginationParams);
            _mockMapper.Received(1).Map<PaginatedResult<ProjectDto>>(pagedData);
        }

        [Fact]
        public async Task GetPagedProjectsAsync_WithEmptyResult_ReturnsMappedEmptyResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var emptyPagedData = new PagedData<Project>(
                Enumerable.Empty<Project>(),
                new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });
            var emptyResult = new PaginatedResult<ProjectDto>(
                Enumerable.Empty<ProjectDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetPagedProjectsAsync(paginationParams).Returns(emptyPagedData);
            _mockMapper.Map<PaginatedResult<ProjectDto>>(emptyPagedData).Returns(emptyResult);

            // Act
            var result = await _sut.GetPagedProjectsAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
            await _mockRepository.Received(1).GetPagedProjectsAsync(paginationParams);
        }

        [Fact]
        public async Task GetPagedProjectsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetPagedProjectsAsync(paginationParams)
                .Returns(Task.FromException<PagedData<Project>>(new Exception("Database connection failed")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetPagedProjectsAsync(query)
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetPagedProjectsAsync(paginationParams);
            _mockMapper.DidNotReceive().Map<PaginatedResult<ProjectDto>>(Arg.Any<PagedData<Project>>());
        }

        #endregion

        #region GetPagedPactProjectsAsync

        [Fact]
        public async Task GetPagedPactProjectsAsync_WithValidQuery_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var pactEntities = new List<PactProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "PACT Alpha" }
            };
            var paginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 };
            var pagedData = new PagedData<PactProjectView>(pactEntities, paginationData);
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 };
            var expectedResult = new PaginatedResult<ProjectDto>(
                new List<ProjectDto> { new() { ParentProject = "PP001" } }, paginationDto);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetPagedPactProjectsAsync(paginationParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetPagedPactProjectsAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
            result.Data.First().ParentProject.Should().Be("PP001");
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetPagedPactProjectsAsync(paginationParams);
            _mockMapper.Received(1).Map<PaginatedResult<ProjectDto>>(pagedData);
        }

        [Fact]
        public async Task GetPagedPactProjectsAsync_WithEmptyResult_ReturnsMappedEmptyResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var emptyPagedData = new PagedData<PactProjectView>(
                Enumerable.Empty<PactProjectView>(),
                new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });
            var emptyResult = new PaginatedResult<ProjectDto>(
                Enumerable.Empty<ProjectDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetPagedPactProjectsAsync(paginationParams).Returns(emptyPagedData);
            _mockMapper.Map<PaginatedResult<ProjectDto>>(emptyPagedData).Returns(emptyResult);

            // Act
            var result = await _sut.GetPagedPactProjectsAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
            await _mockRepository.Received(1).GetPagedPactProjectsAsync(paginationParams);
        }

        [Fact]
        public async Task GetPagedPactProjectsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetPagedPactProjectsAsync(paginationParams)
                .Returns(Task.FromException<PagedData<PactProjectView>>(new Exception("Database connection failed")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetPagedPactProjectsAsync(query)
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetPagedPactProjectsAsync(paginationParams);
            _mockMapper.DidNotReceive().Map<PaginatedResult<ProjectDto>>(Arg.Any<PagedData<PactProjectView>>());
        }

        #endregion

        #region CreateProjectAsync

        [Fact]
        public async Task CreateProjectAsync_WithValidDto_ReturnsMappedCreatedDto()
        {
            // Arrange
            var inputDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "New Project", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };
            var projectEntity = new Project { ParentProject = "PP001", ProjectTitle = "New Project", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };
            var createdEntity = new Project { ParentProject = "PP001", ProjectTitle = "New Project", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };
            var expectedDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "New Project" };

            _mockMapper.Map<Project>(inputDto).Returns(projectEntity);
            _mockRepository.CreateProjectAsync(projectEntity).Returns(createdEntity);
            _mockMapper.Map<ProjectDto>(createdEntity).Returns(expectedDto);

            // Act
            var result = await _sut.CreateProjectAsync(inputDto);

            // Assert
            result.Should().NotBeNull();
            result.ParentProject.Should().Be("PP001");
            _mockMapper.Received(1).Map<Project>(inputDto);
            await _mockRepository.Received(1).CreateProjectAsync(projectEntity);
            _mockMapper.Received(1).Map<ProjectDto>(createdEntity);
        }

        [Fact]
        public async Task CreateProjectAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var inputDto = new ProjectDto { ParentProject = "PP001", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };
            var projectEntity = new Project { ParentProject = "PP001", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };

            _mockMapper.Map<Project>(inputDto).Returns(projectEntity);
            _mockRepository.CreateProjectAsync(projectEntity)
                .Returns(Task.FromException<Project>(new Exception("Database connection failed")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.CreateProjectAsync(inputDto)
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).CreateProjectAsync(projectEntity);
            _mockMapper.DidNotReceive().Map<ProjectDto>(Arg.Any<Project>());
        }

        #endregion

        #region UpdateProjectAsync

        [Fact]
        public async Task UpdateProjectAsync_WithValidDto_ReturnsMappedUpdatedDto()
        {
            // Arrange
            var inputDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "Updated Project", Program = "P001", Customer = "DEFRA", ProjectStatus = "Closed" };
            var projectEntity = new Project { ParentProject = "PP001", ProjectTitle = "Updated Project", Program = "P001", Customer = "DEFRA", ProjectStatus = "Closed" };
            var updatedEntity = new Project { ParentProject = "PP001", ProjectTitle = "Updated Project", Program = "P001", Customer = "DEFRA", ProjectStatus = "Closed" };
            var expectedDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "Updated Project", ProjectStatus = "Closed" };

            _mockMapper.Map<Project>(inputDto).Returns(projectEntity);
            _mockRepository.UpdateProjectAsync(projectEntity).Returns(updatedEntity);
            _mockMapper.Map<ProjectDto>(updatedEntity).Returns(expectedDto);

            // Act
            var result = await _sut.UpdateProjectAsync(inputDto);

            // Assert
            result.Should().NotBeNull();
            result.ParentProject.Should().Be("PP001");
            result.ProjectStatus.Should().Be("Closed");
            _mockMapper.Received(1).Map<Project>(inputDto);
            await _mockRepository.Received(1).UpdateProjectAsync(projectEntity);
            _mockMapper.Received(1).Map<ProjectDto>(updatedEntity);
        }

        [Fact]
        public async Task UpdateProjectAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var inputDto = new ProjectDto { ParentProject = "PP001", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };
            var projectEntity = new Project { ParentProject = "PP001", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };

            _mockMapper.Map<Project>(inputDto).Returns(projectEntity);
            _mockRepository.UpdateProjectAsync(projectEntity)
                .Returns(Task.FromException<Project>(new Exception("Database connection failed")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.UpdateProjectAsync(inputDto)
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).UpdateProjectAsync(projectEntity);
            _mockMapper.DidNotReceive().Map<ProjectDto>(Arg.Any<Project>());
        }

        #endregion

        #region UpdatePactProjectDetailsAsync

        [Fact]
        public async Task UpdatePactProjectDetailsAsync_WithValidDto_ReturnsMappedUpdatedDto()
        {
            // Arrange
            var inputDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "PACT Update", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };
            var projectEntity = new Project { ParentProject = "PP001", ProjectTitle = "PACT Update", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };
            var updatedEntity = new Project { ParentProject = "PP001", ProjectTitle = "PACT Update", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };
            var expectedDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "PACT Update" };

            _mockMapper.Map<Project>(inputDto).Returns(projectEntity);
            _mockRepository.UpdatePactProjectDetailsAsync(projectEntity).Returns(updatedEntity);
            _mockMapper.Map<ProjectDto>(updatedEntity).Returns(expectedDto);

            // Act
            var result = await _sut.UpdatePactProjectDetailsAsync(inputDto);

            // Assert
            result.Should().NotBeNull();
            result!.ParentProject.Should().Be("PP001");
            _mockMapper.Received(1).Map<Project>(inputDto);
            await _mockRepository.Received(1).UpdatePactProjectDetailsAsync(projectEntity);
            _mockMapper.Received(1).Map<ProjectDto>(updatedEntity);
        }

        [Fact]
        public async Task UpdatePactProjectDetailsAsync_WhenProjectNotFound_ReturnsNull()
        {
            // Arrange
            var inputDto = new ProjectDto { ParentProject = "PP999", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };
            var projectEntity = new Project { ParentProject = "PP999", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };

            _mockMapper.Map<Project>(inputDto).Returns(projectEntity);
            _mockRepository.UpdatePactProjectDetailsAsync(projectEntity).Returns((Project?)null);

            // Act
            var result = await _sut.UpdatePactProjectDetailsAsync(inputDto);

            // Assert
            result.Should().BeNull();
            await _mockRepository.Received(1).UpdatePactProjectDetailsAsync(projectEntity);
            _mockMapper.DidNotReceive().Map<ProjectDto>(Arg.Any<Project>());
        }

        [Fact]
        public async Task UpdatePactProjectDetailsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var inputDto = new ProjectDto { ParentProject = "PP001", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };
            var projectEntity = new Project { ParentProject = "PP001", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };

            _mockMapper.Map<Project>(inputDto).Returns(projectEntity);
            _mockRepository.UpdatePactProjectDetailsAsync(projectEntity)
                .Returns(Task.FromException<Project?>(new Exception("Database connection failed")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.UpdatePactProjectDetailsAsync(inputDto)
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).UpdatePactProjectDetailsAsync(projectEntity);
            _mockMapper.DidNotReceive().Map<ProjectDto>(Arg.Any<Project>());
        }

        #endregion

        #region DeleteProjectAsync

        [Fact]
        public async Task DeleteProjectAsync_WithExistingProject_ReturnsTrue()
        {
            // Arrange
            var parentProject = "PP001";
            _mockRepository.HasAssociatedJobCodesAsync(parentProject).Returns(false);
            _mockRepository.DeleteProjectAsync(parentProject).Returns(true);

            // Act
            var result = await _sut.DeleteProjectAsync(parentProject);

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).HasAssociatedJobCodesAsync(parentProject);
            await _mockRepository.Received(1).DeleteProjectAsync(parentProject);
        }

        [Fact]
        public async Task DeleteProjectAsync_WithNonExistingProject_ReturnsFalse()
        {
            // Arrange
            var parentProject = "PP999";
            _mockRepository.HasAssociatedJobCodesAsync(parentProject).Returns(false);
            _mockRepository.DeleteProjectAsync(parentProject).Returns(false);

            // Act
            var result = await _sut.DeleteProjectAsync(parentProject);

            // Assert
            result.Should().BeFalse();
            await _mockRepository.Received(1).HasAssociatedJobCodesAsync(parentProject);
            await _mockRepository.Received(1).DeleteProjectAsync(parentProject);
        }

        [Fact]
        public async Task DeleteProjectAsync_WhenProjectHasAssociatedJobCodes_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var parentProject = "PP001";
            _mockRepository.HasAssociatedJobCodesAsync(parentProject).Returns(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.DeleteProjectAsync(parentProject)
            );

            exception.Errors.Should().HaveCount(1);
            exception.Errors[0].Code.Should().Be("PROJECT_HAS_ASSOCIATIONS");
            exception.Errors[0].Message.Should().Contain(parentProject);
            await _mockRepository.Received(1).HasAssociatedJobCodesAsync(parentProject);
            await _mockRepository.DidNotReceive().DeleteProjectAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteProjectAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var parentProject = "PP001";
            _mockRepository.HasAssociatedJobCodesAsync(parentProject).Returns(false);
            _mockRepository.DeleteProjectAsync(parentProject)
                .Returns(Task.FromException<bool>(new Exception("Database connection failed")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.DeleteProjectAsync(parentProject)
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).HasAssociatedJobCodesAsync(parentProject);
            await _mockRepository.Received(1).DeleteProjectAsync(parentProject);
        }

        #endregion
    }
}