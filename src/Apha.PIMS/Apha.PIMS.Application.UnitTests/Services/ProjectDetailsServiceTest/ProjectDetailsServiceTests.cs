using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Services;
using Apha.PIMS.Application.Validation;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using AutoMapper;
using FluentAssertions;
using NSubstitute;

namespace Apha.PIMS.Application.UnitTests.Services.ProjectDetailsServiceTest
{
    public class ProjectDetailsServiceTests
    {
        private readonly IProjectDetailsRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProjectDetailsService _sut;

        public ProjectDetailsServiceTests()
        {
            _mockRepository = Substitute.For<IProjectDetailsRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new ProjectDetailsService(_mockRepository, _mockMapper);
        }

        #region GetPimsDetailAsync

        [Fact]
        public async Task GetPimsDetailAsync_WithValidParentProject_ReturnsMappedDto()
        {
            // Arrange
            var parentProject = "PP001";

            var entity = new ProjectDetail
            {
                Parentproject = parentProject,
                Version = "1.0",
                FileRef = "FR001",
                CustomerRef = "CR001",
                StartDate = new DateOnly(2023, 1, 1),
                EndDate = new DateOnly(2024, 12, 31),
                CostbookNumber = "CB001",
                Riskid = 1,
                UseProjectYears = true
            };

            var expectedDto = new ProjectDetailDto
            {
                Parentproject = parentProject,
                Version = "1.0",
                FileRef = "FR001",
                CustomerRef = "CR001",
                StartDate = new DateOnly(2023, 1, 1),
                EndDate = new DateOnly(2024, 12, 31),
                CostbookNumber = "CB001",
                Riskid = 1,
                UseProjectYears = true
            };

            _mockRepository.GetPimsDetailAsync(parentProject)
                .Returns(Task.FromResult<ProjectDetail?>(entity));

            _mockMapper.Map<ProjectDetailDto>(entity).Returns(expectedDto);

            // Act
            var result = await _sut.GetPimsDetailAsync(parentProject);

            // Assert
            result.Should().NotBeNull();
            result!.Parentproject.Should().Be("PP001");
            result.Version.Should().Be("1.0");
            result.Riskid.Should().Be(1);

            await _mockRepository.Received(1).GetPimsDetailAsync(parentProject);
            _mockMapper.Received(1).Map<ProjectDetailDto>(entity);
        }

        [Fact]
        public async Task GetPimsDetailAsync_WhenProjectNotFound_ReturnsNull()
        {
            // Arrange
            var parentProject = "UNKNOWN";

            _mockRepository.GetPimsDetailAsync(parentProject)
                .Returns(Task.FromResult<ProjectDetail?>(null));

            // Act
            var result = await _sut.GetPimsDetailAsync(parentProject);

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetPimsDetailAsync(parentProject);
            _mockMapper.DidNotReceive().Map<ProjectDetailDto>(Arg.Any<ProjectDetail>());
        }

        [Fact]
        public async Task GetPimsDetailAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var parentProject = "PP001";
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetPimsDetailAsync(parentProject)
                .Returns(Task.FromException<ProjectDetail?>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetPimsDetailAsync(parentProject)
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetPimsDetailAsync(parentProject);
            _mockMapper.DidNotReceive().Map<ProjectDetailDto>(Arg.Any<ProjectDetail>());
        }

        #endregion

        #region SavePimsDetailAsync

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SavePimsDetailAsync_WithInvalidParentProject_ThrowsBusinessValidationErrorException(string? parentProject)
        {
            // Arrange
            var dto = new ProjectDetailDto { Parentproject = parentProject };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.SavePimsDetailAsync(dto)
            );

            exception.Errors.Should().ContainSingle();
            exception.Errors.First().Code.Should().Be("PROJECT_REQUIRED");
            exception.Errors.First().Message.Should().Be("Project is required.");

            await _mockRepository.DidNotReceive().GetPimsDetailAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task SavePimsDetailAsync_WhenProjectDoesNotExist_CreatesAndReturnsMappedDto()
        {
            // Arrange
            var dto = new ProjectDetailDto
            {
                Parentproject = "PP001",
                Version = "1.0",
                FileRef = "FR001",
                CustomerRef = "CR001",
                StartDate = new DateOnly(2023, 1, 1),
                EndDate = new DateOnly(2024, 12, 31),
                CostbookNumber = "CB001",
                Riskid = 1,
                UseProjectYears = true
            };

            var newEntity = new ProjectDetail
            {
                Parentproject = "PP001",
                Version = "1.0",
                FileRef = "FR001",
                CustomerRef = "CR001",
                StartDate = new DateOnly(2023, 1, 1),
                EndDate = new DateOnly(2024, 12, 31),
                CostbookNumber = "CB001",
                Riskid = 1,
                UseProjectYears = true
            };

            var createdEntity = new ProjectDetail
            {
                Parentproject = "PP001",
                Version = "1.0",
                FileRef = "FR001",
                CustomerRef = "CR001",
                StartDate = new DateOnly(2023, 1, 1),
                EndDate = new DateOnly(2024, 12, 31),
                CostbookNumber = "CB001",
                Riskid = 1,
                UseProjectYears = true
            };

            var expectedDto = new ProjectDetailDto
            {
                Parentproject = "PP001",
                Version = "1.0",
                FileRef = "FR001",
                CustomerRef = "CR001",
                StartDate = new DateOnly(2023, 1, 1),
                EndDate = new DateOnly(2024, 12, 31),
                CostbookNumber = "CB001",
                Riskid = 1,
                UseProjectYears = true
            };

            _mockRepository.GetPimsDetailAsync(dto.Parentproject!)
                .Returns(Task.FromResult<ProjectDetail?>(null));
            _mockMapper.Map<ProjectDetail>(dto).Returns(newEntity);
            _mockRepository.AddPimsDetailAsync(newEntity).Returns(Task.FromResult(createdEntity));
            _mockMapper.Map<ProjectDetailDto>(createdEntity).Returns(expectedDto);

            // Act
            var result = await _sut.SavePimsDetailAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Parentproject.Should().Be("PP001");
            result.Version.Should().Be("1.0");
            result.Riskid.Should().Be(1);

            await _mockRepository.Received(1).GetPimsDetailAsync(dto.Parentproject!);
            _mockMapper.Received(1).Map<ProjectDetail>(dto);
            await _mockRepository.Received(1).AddPimsDetailAsync(newEntity);
            _mockMapper.Received(1).Map<ProjectDetailDto>(createdEntity);
            await _mockRepository.DidNotReceive().UpdatePimsDetailAsync(Arg.Any<ProjectDetail>());
        }

        [Fact]
        public async Task SavePimsDetailAsync_WhenProjectExists_UpdatesAndReturnsMappedDto()
        {
            // Arrange
            var dto = new ProjectDetailDto
            {
                Parentproject = "PP001",
                Version = "2.0",
                FileRef = "FR002",
                CustomerRef = "CR002",
                StartDate = new DateOnly(2023, 1, 1),
                EndDate = new DateOnly(2025, 6, 30),
                CostbookNumber = "CB002",
                Riskid = 3,
                UseProjectYears = false
            };

            var existingEntity = new ProjectDetail
            {
                Parentproject = "PP001",
                Version = "1.0",
                FileRef = "FR001",
                CustomerRef = "CR001",
                Riskid = 1,
                UseProjectYears = true
            };

            var updatedEntity = new ProjectDetail
            {
                Parentproject = "PP001",
                Version = "2.0",
                FileRef = "FR002",
                CustomerRef = "CR002",
                StartDate = new DateOnly(2023, 1, 1),
                EndDate = new DateOnly(2025, 6, 30),
                CostbookNumber = "CB002",
                Riskid = 3,
                UseProjectYears = false
            };

            var expectedDto = new ProjectDetailDto
            {
                Parentproject = "PP001",
                Version = "2.0",
                FileRef = "FR002",
                CustomerRef = "CR002",
                StartDate = new DateOnly(2023, 1, 1),
                EndDate = new DateOnly(2025, 6, 30),
                CostbookNumber = "CB002",
                Riskid = 3,
                UseProjectYears = false
            };

            _mockRepository.GetPimsDetailAsync(dto.Parentproject!)
                .Returns(Task.FromResult<ProjectDetail?>(existingEntity));
            _mockRepository.UpdatePimsDetailAsync(existingEntity).Returns(Task.FromResult(updatedEntity));
            _mockMapper.Map<ProjectDetailDto>(updatedEntity).Returns(expectedDto);

            // Act
            var result = await _sut.SavePimsDetailAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Parentproject.Should().Be("PP001");
            result.Version.Should().Be("2.0");
            result.Riskid.Should().Be(3);

            await _mockRepository.Received(1).GetPimsDetailAsync(dto.Parentproject!);
            _mockMapper.Received(1).Map(dto, existingEntity);
            await _mockRepository.Received(1).UpdatePimsDetailAsync(existingEntity);
            _mockMapper.Received(1).Map<ProjectDetailDto>(updatedEntity);
            await _mockRepository.DidNotReceive().AddPimsDetailAsync(Arg.Any<ProjectDetail>());
        }

        [Fact]
        public async Task SavePimsDetailAsync_WhenRepositoryThrowsOnAdd_PropagatesException()
        {
            // Arrange
            var dto = new ProjectDetailDto { Parentproject = "PP001", Version = "1.0" };
            var newEntity = new ProjectDetail { Parentproject = "PP001", Version = "1.0" };
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetPimsDetailAsync(dto.Parentproject!)
                .Returns(Task.FromResult<ProjectDetail?>(null));
            _mockMapper.Map<ProjectDetail>(dto).Returns(newEntity);
            _mockRepository.AddPimsDetailAsync(newEntity)
                .Returns(Task.FromException<ProjectDetail>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.SavePimsDetailAsync(dto)
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetPimsDetailAsync(dto.Parentproject!);
            await _mockRepository.Received(1).AddPimsDetailAsync(newEntity);
            _mockMapper.DidNotReceive().Map<ProjectDetailDto>(Arg.Any<ProjectDetail>());
        }

        [Fact]
        public async Task SavePimsDetailAsync_WhenRepositoryThrowsOnUpdate_PropagatesException()
        {
            // Arrange
            var dto = new ProjectDetailDto { Parentproject = "PP001", Version = "2.0" };
            var existingEntity = new ProjectDetail { Parentproject = "PP001", Version = "1.0" };
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetPimsDetailAsync(dto.Parentproject!)
                .Returns(Task.FromResult<ProjectDetail?>(existingEntity));
            _mockRepository.UpdatePimsDetailAsync(existingEntity)
                .Returns(Task.FromException<ProjectDetail>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.SavePimsDetailAsync(dto)
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetPimsDetailAsync(dto.Parentproject!);
            await _mockRepository.Received(1).UpdatePimsDetailAsync(existingEntity);
            _mockMapper.DidNotReceive().Map<ProjectDetailDto>(Arg.Any<ProjectDetail>());
        }

        #endregion

        #region GetProposedProjectAsync

        [Fact]
        public async Task GetProposedProjectAsync_WithValidParentProject_ReturnsMappedDto()
        {
            // Arrange
            var parentProject = "PP001";

            var entity = new ProposedProject
            {
                Id = 1,
                Parentproject = parentProject,
                Projecttitle = "FMD Survey",
                Program = "PROG1",
                Customer = "CUST1",
                Manager = "MGR1",
                Projectstatus = "Proposed",
                Disease = "FMD"
            };

            var expectedDto = new ProposedProjectDto
            {
                Id = 1,
                Parentproject = parentProject,
                Projecttitle = "FMD Survey",
                Program = "PROG1",
                Customer = "CUST1",
                Manager = "MGR1",
                Projectstatus = "Proposed",
                Disease = "FMD"
            };

            _mockRepository.GetProposedProjectAsync(parentProject)
                .Returns(Task.FromResult<ProposedProject?>(entity));

            _mockMapper.Map<ProposedProjectDto>(entity).Returns(expectedDto);

            // Act
            var result = await _sut.GetProposedProjectAsync(parentProject);

            // Assert
            result.Should().NotBeNull();
            result!.Parentproject.Should().Be("PP001");
            result.Projecttitle.Should().Be("FMD Survey");
            result.Projectstatus.Should().Be("Proposed");

            await _mockRepository.Received(1).GetProposedProjectAsync(parentProject);
            _mockMapper.Received(1).Map<ProposedProjectDto>(entity);
        }

        [Fact]
        public async Task GetProposedProjectAsync_WhenProjectNotFound_ReturnsNull()
        {
            // Arrange
            var parentProject = "UNKNOWN";

            _mockRepository.GetProposedProjectAsync(parentProject)
                .Returns(Task.FromResult<ProposedProject?>(null));

            // Act
            var result = await _sut.GetProposedProjectAsync(parentProject);

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetProposedProjectAsync(parentProject);
            _mockMapper.DidNotReceive().Map<ProposedProjectDto>(Arg.Any<ProposedProject>());
        }

        [Fact]
        public async Task GetProposedProjectAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var parentProject = "PP001";
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetProposedProjectAsync(parentProject)
                .Returns(Task.FromException<ProposedProject?>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetProposedProjectAsync(parentProject)
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetProposedProjectAsync(parentProject);
            _mockMapper.DidNotReceive().Map<ProposedProjectDto>(Arg.Any<ProposedProject>());
        }

        #endregion

        #region UpdateProposedProjectAsync

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task UpdateProposedProjectAsync_WithInvalidParentProject_ThrowsBusinessValidationErrorException(string? parentProject)
        {
            // Arrange
            var dto = new ProposedProjectDto { Parentproject = parentProject! };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.UpdateProposedProjectAsync(dto)
            );

            exception.Errors.Should().ContainSingle();
            exception.Errors.First().Code.Should().Be("PROJECT_REQUIRED");
            exception.Errors.First().Message.Should().Be("Project is required.");

            await _mockRepository.DidNotReceive().UpdateProposedProjectAsync(Arg.Any<ProposedProject>());
        }

        [Fact]
        public async Task UpdateProposedProjectAsync_WithValidDto_ReturnsMappedUpdatedDto()
        {
            // Arrange
            var dto = new ProposedProjectDto
            {
                Id = 1,
                Parentproject = "PP001",
                Projecttitle = "Updated FMD Survey",
                Program = "PROG1",
                Customer = "CUST1",
                Manager = "MGR1",
                Projectstatus = "Active",
                Disease = "FMD"
            };

            var entity = new ProposedProject
            {
                Id = 1,
                Parentproject = "PP001",
                Projecttitle = "Updated FMD Survey",
                Program = "PROG1",
                Customer = "CUST1",
                Manager = "MGR1",
                Projectstatus = "Active",
                Disease = "FMD"
            };

            var updatedEntity = new ProposedProject
            {
                Id = 1,
                Parentproject = "PP001",
                Projecttitle = "Updated FMD Survey",
                Program = "PROG1",
                Customer = "CUST1",
                Manager = "MGR1",
                Projectstatus = "Active",
                Disease = "FMD"
            };

            var expectedDto = new ProposedProjectDto
            {
                Id = 1,
                Parentproject = "PP001",
                Projecttitle = "Updated FMD Survey",
                Program = "PROG1",
                Customer = "CUST1",
                Manager = "MGR1",
                Projectstatus = "Active",
                Disease = "FMD"
            };

            _mockMapper.Map<ProposedProject>(dto).Returns(entity);
            _mockRepository.UpdateProposedProjectAsync(entity).Returns(Task.FromResult(updatedEntity));
            _mockMapper.Map<ProposedProjectDto>(updatedEntity).Returns(expectedDto);

            // Act
            var result = await _sut.UpdateProposedProjectAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Parentproject.Should().Be("PP001");
            result.Projecttitle.Should().Be("Updated FMD Survey");
            result.Projectstatus.Should().Be("Active");

            _mockMapper.Received(1).Map<ProposedProject>(dto);
            await _mockRepository.Received(1).UpdateProposedProjectAsync(entity);
            _mockMapper.Received(1).Map<ProposedProjectDto>(updatedEntity);
        }

        [Fact]
        public async Task UpdateProposedProjectAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var dto = new ProposedProjectDto
            {
                Id = 1,
                Parentproject = "PP001",
                Projecttitle = "FMD Survey"
            };

            var entity = new ProposedProject
            {
                Id = 1,
                Parentproject = "PP001",
                Projecttitle = "FMD Survey"
            };

            var expectedException = new Exception("Database connection failed");

            _mockMapper.Map<ProposedProject>(dto).Returns(entity);
            _mockRepository.UpdateProposedProjectAsync(entity)
                .Returns(Task.FromException<ProposedProject>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.UpdateProposedProjectAsync(dto)
            );

            exception.Message.Should().Be("Database connection failed");

            _mockMapper.Received(1).Map<ProposedProject>(dto);
            await _mockRepository.Received(1).UpdateProposedProjectAsync(entity);
            _mockMapper.DidNotReceive().Map<ProposedProjectDto>(Arg.Any<ProposedProject>());
        }

        #endregion
    }
}