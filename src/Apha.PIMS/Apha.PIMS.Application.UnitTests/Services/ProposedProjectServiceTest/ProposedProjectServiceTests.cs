using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Services;
using Apha.PIMS.Application.Validation;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using AutoMapper;
using FluentAssertions;
using NSubstitute;

namespace Apha.PIMS.Application.UnitTests.Services.ProposedProjectServiceTest
{
    public class ProposedProjectServiceTests
    {
        private readonly IProposedProjectRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProposedProjectService _sut;

        public ProposedProjectServiceTests()
        {
            _mockRepository = Substitute.For<IProposedProjectRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new ProposedProjectService(_mockRepository, _mockMapper);
        }

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

            _mockRepository.GetFpsProjectByIdAsync(parentProject).Returns(Task.FromResult<Project?>(entity));
            _mockMapper.Map<ProjectDto>(entity).Returns(expectedDto);

            // Act
            var result = await _sut.GetFpsProjectByIdAsync(parentProject);

            // Assert
            result.Should().NotBeNull();
            result!.Parentproject.Should().Be("PP001");
            result.Projecttitle.Should().Be("FMD Survey");

            await _mockRepository.Received(1).GetFpsProjectByIdAsync(parentProject);
            _mockMapper.Received(1).Map<ProjectDto>(entity);
        }

        [Fact]
        public async Task GetFpsProjectByIdAsync_WhenProjectNotFound_ReturnsNull()
        {
            // Arrange
            var parentProject = "UNKNOWN";
            _mockRepository.GetFpsProjectByIdAsync(parentProject).Returns(Task.FromResult<Project?>(null));

            // Act
            var result = await _sut.GetFpsProjectByIdAsync(parentProject);

            // Assert
            result.Should().BeNull();
            await _mockRepository.Received(1).GetFpsProjectByIdAsync(parentProject);
            _mockMapper.DidNotReceive().Map<ProjectDto>(Arg.Any<Project>());
        }

        [Fact]
        public async Task GetFpsProjectByIdAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var parentProject = "PP001";
            var expectedException = new Exception("Database connection failed");
            _mockRepository.GetFpsProjectByIdAsync(parentProject).Returns(Task.FromException<Project?>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetFpsProjectByIdAsync(parentProject));
            exception.Message.Should().Be("Database connection failed");
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

            _mockRepository.GetProposedProjectByIdAsync(parentProject).Returns(Task.FromResult<ProposedProject?>(entity));
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
            _mockRepository.GetProposedProjectByIdAsync(parentProject).Returns(Task.FromResult<ProposedProject?>(null));

            // Act
            var result = await _sut.GetProposedProjectByIdAsync(parentProject);

            // Assert
            result.Should().BeNull();
            await _mockRepository.Received(1).GetProposedProjectByIdAsync(parentProject);
            _mockMapper.DidNotReceive().Map<ProposedProjectDto>(Arg.Any<ProposedProject>());
        }

        [Fact]
        public async Task GetProposedProjectByIdAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var parentProject = "PP001";
            var expectedException = new Exception("Database connection failed");
            _mockRepository.GetProposedProjectByIdAsync(parentProject).Returns(Task.FromException<ProposedProject?>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetProposedProjectByIdAsync(parentProject));
            exception.Message.Should().Be("Database connection failed");
        }

        #endregion

        #region AddProposedProjectAsync

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

            _mockRepository.GetFpsProjectByIdAsync("PP001").Returns(Task.FromResult<Project?>(null));
            _mockRepository.GetProposedProjectByIdAsync("PP001").Returns(Task.FromResult<ProposedProject?>(null));
            _mockMapper.Map<ProposedProject>(dto).Returns(entity);
            _mockRepository.AddProposedProjectAsync(entity).Returns(Task.FromResult(createdEntity));
            _mockMapper.Map<ProposedProjectDto>(createdEntity).Returns(expectedDto);

            // Act
            var result = await _sut.AddProposedProjectAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(42);
            result.Parentproject.Should().Be("PP001");

            _mockMapper.Received(1).Map<ProposedProject>(dto);
            await _mockRepository.Received(1).AddProposedProjectAsync(entity);
            _mockMapper.Received(1).Map<ProposedProjectDto>(createdEntity);
        }

        [Theory]
        [InlineData(null, "New Project", "PROJECT_REQUIRED")]
        [InlineData("", "New Project", "PROJECT_REQUIRED")]
        [InlineData("   ", "New Project", "PROJECT_REQUIRED")]
        public async Task AddProjectAsync_WithMissingParentproject_ThrowsBusinessValidationErrorException(
            string? parentProject, string projectTitle, string expectedErrorCode)
        {
            // Arrange
            var dto = new ProposedProjectDto { Parentproject = parentProject, Projecttitle = projectTitle };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.AddProposedProjectAsync(dto));

            exception.Errors.Should().ContainSingle(e => e.Code == expectedErrorCode);
            await _mockRepository.DidNotReceive().GetFpsProjectByIdAsync(Arg.Any<string>());
            await _mockRepository.DidNotReceive().AddProposedProjectAsync(Arg.Any<ProposedProject>());
        }

        [Fact]
        public async Task AddProjectAsync_WhenProjectExistsInFps_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = new ProposedProjectDto { Parentproject = "PP001", Projecttitle = "New Project" };
            var fpsProject = new Project { Parentproject = "PP001", Projecttitle = "Existing FPS Project", Projectstatus = "Active" };

            _mockRepository.GetFpsProjectByIdAsync("PP001").Returns(Task.FromResult<Project?>(fpsProject));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.AddProposedProjectAsync(dto));

            exception.Errors.Should().ContainSingle(e => e.Code == "PROJECT_EXISTS_IN_FPS");
            exception.Errors.First().Message.Should().Be("This project already exists in FPS. Only use this form for projects NOT on FPS.");
            await _mockRepository.DidNotReceive().AddProposedProjectAsync(Arg.Any<ProposedProject>());
        }

        [Fact]
        public async Task AddProjectAsync_WhenProjectAlreadyPlanned_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = new ProposedProjectDto { Parentproject = "PP001", Projecttitle = "New Project" };
            var existingProposedProject = new ProposedProject { Id = 10, Parentproject = "PP001", Projecttitle = "Already Planned Project" };

            _mockRepository.GetFpsProjectByIdAsync("PP001").Returns(Task.FromResult<Project?>(null));
            _mockRepository.GetProposedProjectByIdAsync("PP001").Returns(Task.FromResult<ProposedProject?>(existingProposedProject));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.AddProposedProjectAsync(dto));

            exception.Errors.Should().ContainSingle(e => e.Code == "PROJECT_ALREADY_PLANNED");
            exception.Errors.First().Message.Should().Be("This project has already been planned. Please select it from the list.");
            await _mockRepository.DidNotReceive().AddProposedProjectAsync(Arg.Any<ProposedProject>());
        }

        [Fact]
        public async Task AddProjectAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var dto = new ProposedProjectDto { Parentproject = "PP001", Projecttitle = "New Project" };
            var entity = new ProposedProject { Parentproject = "PP001", Projecttitle = "New Project" };
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetFpsProjectByIdAsync("PP001").Returns(Task.FromResult<Project?>(null));
            _mockRepository.GetProposedProjectByIdAsync("PP001").Returns(Task.FromResult<ProposedProject?>(null));
            _mockMapper.Map<ProposedProject>(dto).Returns(entity);
            _mockRepository.AddProposedProjectAsync(entity).Returns(Task.FromException<ProposedProject>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.AddProposedProjectAsync(dto));
            exception.Message.Should().Be("Database connection failed");
        }

        #endregion

        #region GetProjectProgramsAsync

        [Fact]
        public async Task GetProjectProgramsAsync_WithData_ReturnsListOfPrograms()
        {
            // Arrange
            var expectedPrograms = new List<string> { "PROG1", "PROG2", "PROG3" };
            _mockRepository.GetProjectProgramsAsync().Returns(Task.FromResult(expectedPrograms));

            // Act
            var result = await _sut.GetProjectProgramsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(expectedPrograms);
            await _mockRepository.Received(1).GetProjectProgramsAsync();
        }

        [Fact]
        public async Task GetProjectProgramsAsync_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.GetProjectProgramsAsync().Returns(Task.FromResult(new List<string>()));

            // Act
            var result = await _sut.GetProjectProgramsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            await _mockRepository.Received(1).GetProjectProgramsAsync();
        }

        [Fact]
        public async Task GetProjectProgramsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.GetProjectProgramsAsync().Returns(Task.FromException<List<string>>(new Exception("Database connection failed")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetProjectProgramsAsync());
            exception.Message.Should().Be("Database connection failed");
        }

        #endregion

        #region GetProjectCustomersAsync

        [Fact]
        public async Task GetProjectCustomersAsync_WithData_ReturnsListOfCustomers()
        {
            // Arrange
            var expectedCustomers = new List<string> { "CUST1", "CUST2", "CUST3" };
            _mockRepository.GetProjectCustomersAsync().Returns(Task.FromResult(expectedCustomers));

            // Act
            var result = await _sut.GetProjectCustomersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(expectedCustomers);
            await _mockRepository.Received(1).GetProjectCustomersAsync();
        }

        [Fact]
        public async Task GetProjectCustomersAsync_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.GetProjectCustomersAsync().Returns(Task.FromResult(new List<string>()));

            // Act
            var result = await _sut.GetProjectCustomersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            await _mockRepository.Received(1).GetProjectCustomersAsync();
        }

        [Fact]
        public async Task GetProjectCustomersAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.GetProjectCustomersAsync().Returns(Task.FromException<List<string>>(new Exception("Database connection failed")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetProjectCustomersAsync());
            exception.Message.Should().Be("Database connection failed");
        }

        #endregion

        #region GetProjectStatusesAsync

        [Fact]
        public async Task GetProjectStatusesAsync_WithData_ReturnsListOfStatusStrings()
        {
            // Arrange
            var statusEntities = new List<ProjectStatus>
            {
                new ProjectStatus { Projectstatus = "Active", IsFps = true, IsPims = true },
                new ProjectStatus { Projectstatus = "Proposed", IsFps = false, IsPims = true },
                new ProjectStatus { Projectstatus = "Closed", IsFps = true, IsPims = false }
            };
            _mockRepository.GetProjectStatusesAsync().Returns(Task.FromResult(statusEntities));

            // Act
            var result = await _sut.GetProjectStatusesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(new List<string> { "Active", "Proposed", "Closed" });
            await _mockRepository.Received(1).GetProjectStatusesAsync();
        }

        [Fact]
        public async Task GetProjectStatusesAsync_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.GetProjectStatusesAsync().Returns(Task.FromResult(new List<ProjectStatus>()));

            // Act
            var result = await _sut.GetProjectStatusesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            await _mockRepository.Received(1).GetProjectStatusesAsync();
        }

        [Fact]
        public async Task GetProjectStatusesAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.GetProjectStatusesAsync().Returns(Task.FromException<List<ProjectStatus>>(new Exception("Database connection failed")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetProjectStatusesAsync());
            exception.Message.Should().Be("Database connection failed");
        }

        #endregion
    }
}
