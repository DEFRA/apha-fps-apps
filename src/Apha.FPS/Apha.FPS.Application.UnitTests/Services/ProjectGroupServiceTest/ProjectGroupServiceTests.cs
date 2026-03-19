using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;


namespace Apha.FPS.Application.UnitTests.Services.ProjectGroupServiceTest
{
    public class ProjectGroupServiceTests
    {
        private readonly IProjectGroupRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProjectGroupService _sut;

        public ProjectGroupServiceTests()
        {
            _mockRepository = Substitute.For<IProjectGroupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new ProjectGroupService(_mockRepository, _mockMapper);
        }

        [Fact]
        public async Task GetAllProjectGroupsAsync_WithValidData_ReturnsMappedDtoList()
        {
            // Arrange
            var projectGroupEntities = new List<ProjectGroup>
            {
                new ProjectGroup { ProjectGroupName = "Alpha Group" },
                new ProjectGroup { ProjectGroupName = "Beta Group" }
            };

            var expectedDtos = new List<ProjectGroupDto>
            {
                new ProjectGroupDto { ProjectGroupName = "Alpha Group" },
                new ProjectGroupDto { ProjectGroupName = "Beta Group" }
            };

            _mockRepository.GetAllProjectGroupsAsync()
                .Returns(Task.FromResult<IEnumerable<ProjectGroup>>(projectGroupEntities));

            _mockMapper.Map<IEnumerable<ProjectGroupDto>>(projectGroupEntities)
                .Returns(expectedDtos);

            // Act
            var result = await _sut.GetAllProjectGroupsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().ProjectGroupName.Should().Be("Alpha Group");

            await _mockRepository.Received(1).GetAllProjectGroupsAsync();
            _mockMapper.Received(1).Map<IEnumerable<ProjectGroupDto>>(projectGroupEntities);
        }

        [Fact]
        public async Task GetAllProjectGroupsAsync_WithEmptyList_ReturnsEmptyDtoList()
        {
            // Arrange
            var emptyEntities = new List<ProjectGroup>();
            var emptyDtos = new List<ProjectGroupDto>();

            _mockRepository.GetAllProjectGroupsAsync()
                .Returns(Task.FromResult<IEnumerable<ProjectGroup>>(emptyEntities));

            _mockMapper.Map<IEnumerable<ProjectGroupDto>>(emptyEntities)
                .Returns(emptyDtos);

            // Act
            var result = await _sut.GetAllProjectGroupsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetAllProjectGroupsAsync();
            _mockMapper.Received(1).Map<IEnumerable<ProjectGroupDto>>(emptyEntities);
        }

        [Fact]
        public async Task GetAllProjectGroupsAsync_WhenRepositoryReturnsNull_ReturnsNull()
        {
            // Arrange
            _mockRepository.GetAllProjectGroupsAsync()
                .Returns(Task.FromResult<IEnumerable<ProjectGroup>>(null!));

            _mockMapper.Map<IEnumerable<ProjectGroupDto>>(null)
                .Returns((IEnumerable<ProjectGroupDto>?)null);

            // Act
            var result = await _sut.GetAllProjectGroupsAsync();

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetAllProjectGroupsAsync();
            _mockMapper.Received(1).Map<IEnumerable<ProjectGroupDto>>(null);
        }

        [Fact]
        public async Task GetAllProjectGroupsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetAllProjectGroupsAsync()
                .Returns(Task.FromException<IEnumerable<ProjectGroup>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAllProjectGroupsAsync()
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetAllProjectGroupsAsync();
            _mockMapper.DidNotReceive().Map<IEnumerable<ProjectGroupDto>>(Arg.Any<IEnumerable<ProjectGroup>>());
        }
    }
}