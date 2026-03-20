using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;


namespace Apha.FPS.Application.UnitTests.Services.DiseaseServiceTest
{
    public class DiseaseServiceTests
    {
        private readonly IDiseaseRepository _mockRepository;
        private readonly DiseaseService _sut;

        public DiseaseServiceTests()
        {
            _mockRepository = Substitute.For<IDiseaseRepository>();
            _sut = new DiseaseService(_mockRepository);
        }

        [Fact]
        public async Task GetAllDiseasesAsync_WithValidData_ReturnsDiseaseNameList()
        {
            // Arrange
            var diseaseEntities = new List<Disease>
            {
                new Disease { DiseaseName = "Foot and Mouth Disease" },
                new Disease { DiseaseName = "Bovine Tuberculosis" }
            };

            _mockRepository.GetAllDiseasesAsync()
                .Returns(Task.FromResult<IEnumerable<Disease>>(diseaseEntities));

            // Act
            var result = await _sut.GetAllDiseasesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().ContainInOrder("Foot and Mouth Disease", "Bovine Tuberculosis");

            await _mockRepository.Received(1).GetAllDiseasesAsync();
        }

        [Fact]
        public async Task GetAllDiseasesAsync_WithEmptyList_ReturnsEmptyStringList()
        {
            // Arrange
            _mockRepository.GetAllDiseasesAsync()
                .Returns(Task.FromResult<IEnumerable<Disease>>(new List<Disease>()));

            // Act
            var result = await _sut.GetAllDiseasesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetAllDiseasesAsync();
        }

        [Fact]
        public async Task GetAllDiseasesAsync_ProjectsOnlyDiseaseName_ExcludesOtherFields()
        {
            // Arrange
            var diseaseEntities = new List<Disease>
            {
                new Disease { DiseaseName = "Avian Influenza" },
                new Disease { DiseaseName = "African Swine Fever" }
            };

            _mockRepository.GetAllDiseasesAsync()
                .Returns(Task.FromResult<IEnumerable<Disease>>(diseaseEntities));

            // Act
            var result = await _sut.GetAllDiseasesAsync();

            // Assert
            result.Should().BeEquivalentTo("Avian Influenza", "African Swine Fever");

            await _mockRepository.Received(1).GetAllDiseasesAsync();
        }

        [Fact]
        public async Task GetAllDiseasesAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetAllDiseasesAsync()
                .Returns(Task.FromException<IEnumerable<Disease>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAllDiseasesAsync()
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetAllDiseasesAsync();
        }
    }
}