using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Application.UnitTests.Services.WorkGroupServiceTest
{
    public class WorkGroupServiceTests
    {
        private readonly IWorkGroupRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly WorkGroupService _sut;

        public WorkGroupServiceTests()
        {
            _mockRepository = Substitute.For<IWorkGroupRepository>();
            _mockMapper     = Substitute.For<IMapper>();
            _sut            = new WorkGroupService(_mockRepository, _mockMapper);
        }

        [Fact]
        public void Constructor_WithNullRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkGroupService(null!, _mockMapper));
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkGroupService(_mockRepository, null!));
        }

        #region GetAllWorkGroupNamesAsync Tests

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WithData_ReturnsWorkgroupNames()
        {
            // Arrange
            var names = new List<string> { "WG01", "WG02", "WG03" };
            _mockRepository.GetAllWorkGroupNamesAsync().Returns(names);

            // Act
            var result = await _sut.GetAllWorkGroupNamesAsync();

            // Assert
            Assert.Equal(names, result);
            await _mockRepository.Received(1).GetAllWorkGroupNamesAsync();
        }

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WithEmptyRepository_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.GetAllWorkGroupNamesAsync().Returns(new List<string>());

            // Act
            var result = await _sut.GetAllWorkGroupNamesAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.GetAllWorkGroupNamesAsync()
                .ThrowsAsync(new InvalidOperationException("DB failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.GetAllWorkGroupNamesAsync());
        }

        #endregion

        #region GetWorkGroupsAsync Tests

        [Fact]
        public async Task GetWorkGroupsAsync_WithValidProfitCentre_ReturnsMappedDtos()
        {
            // Arrange
            var entities = new List<WorkGroupView>
            {
                new() { WorkgroupName = "WG01", ProfitCentre = "PC01" }
            };
            var dtos = new List<WorkGroupViewDto>
            {
                new() { WorkgroupName = "WG01", ProfitCentre = "PC01" }
            };
            _mockRepository.GetWorkGroupsByProfitCentreAsync("PC01").Returns(entities);
            _mockMapper.Map<List<WorkGroupViewDto>>(entities).Returns(dtos);

            // Act
            var result = await _sut.GetWorkGroupsAsync("PC01");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("WG01", result[0].WorkgroupName);
            await _mockRepository.Received(1).GetWorkGroupsByProfitCentreAsync("PC01");
        }

        [Fact]
        public async Task GetWorkGroupsAsync_WithNull_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetWorkGroupsAsync(null!));
        }

        [Fact]
        public async Task GetWorkGroupsAsync_WithEmptyOrWhiteSpace_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetWorkGroupsAsync(""));
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetWorkGroupsAsync("  "));
        }

        [Fact]
        public async Task GetWorkGroupsAsync_WithNoResults_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.GetWorkGroupsByProfitCentreAsync("PC01").Returns(new List<WorkGroupView>());
            _mockMapper.Map<List<WorkGroupViewDto>>(Arg.Any<List<WorkGroupView>>()).Returns(new List<WorkGroupViewDto>());

            // Act
            var result = await _sut.GetWorkGroupsAsync("PC01");

            // Assert
            Assert.Empty(result);
        }

        #endregion
    }
}
