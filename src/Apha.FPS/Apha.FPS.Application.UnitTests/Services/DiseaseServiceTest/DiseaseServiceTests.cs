using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.DiseaseServiceTest
{
    public class DiseaseServiceTests
    {
        private readonly IDiseaseRepository _mockRepository;
        private readonly IMapper _mockMapper = Substitute.For<IMapper>();
        private readonly DiseaseService _sut;

        public DiseaseServiceTests()
        {
            _mockRepository = Substitute.For<IDiseaseRepository>();
            _sut = new DiseaseService(_mockRepository, _mockMapper);
        }

        #region GetAllDiseasesAsync

        [Fact]
        public async Task GetAllDiseasesAsync_WithValidData_ReturnsMappedDtoList()
        {
            // Arrange
            var diseaseEntities = new List<Disease>
            {
                new Disease { DiseaseName = "Foot and Mouth Disease" },
                new Disease { DiseaseName = "Bovine Tuberculosis" }
            };

            var expectedDtos = new List<DiseaseDto>
            {
                new DiseaseDto { DiseaseName = "Foot and Mouth Disease" },
                new DiseaseDto { DiseaseName = "Bovine Tuberculosis" }
            };

            _mockRepository.GetAllDiseasesAsync()
                .Returns(Task.FromResult<IEnumerable<Disease>>(diseaseEntities));
            _mockMapper.Map<IEnumerable<DiseaseDto>>(diseaseEntities)
                .Returns(expectedDtos);

            // Act
            var result = await _sut.GetAllDiseasesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().ContainInOrder(expectedDtos);

            await _mockRepository.Received(1).GetAllDiseasesAsync();
            _mockMapper.Received(1).Map<IEnumerable<DiseaseDto>>(diseaseEntities);
        }

        [Fact]
        public async Task GetAllDiseasesAsync_WhenNoDiseases_ReturnsEmptyList()
        {
            // Arrange
            var diseaseEntities = new List<Disease>();
            var expectedDtos = new List<DiseaseDto>();

            _mockRepository.GetAllDiseasesAsync()
                .Returns(Task.FromResult<IEnumerable<Disease>>(diseaseEntities));
            _mockMapper.Map<IEnumerable<DiseaseDto>>(diseaseEntities)
                .Returns(expectedDtos);

            // Act
            var result = await _sut.GetAllDiseasesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetAllDiseasesAsync();
        }

        [Fact]
        public async Task GetAllDiseasesAsync_WhenRepositoryThrows_PropagatesException()
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

        #endregion

        #region GetDiseaseByNameAsync

        [Fact]
        public async Task GetDiseaseByNameAsync_FoundByName_ReturnsMappedDto()
        {
            // Arrange
            const string diseaseName = "Foot and Mouth Disease";
            var entity = new Disease { DiseaseName = diseaseName };
            var expectedDto = new DiseaseDto { DiseaseName = diseaseName };

            _mockRepository.GetByNameAsync(diseaseName).Returns(entity);
            _mockMapper.Map<DiseaseDto>(entity).Returns(expectedDto);

            // Act
            var result = await _sut.GetDiseaseByNameAsync(diseaseName);

            // Assert
            result.Should().NotBeNull();
            result!.DiseaseName.Should().Be(diseaseName);

            await _mockRepository.Received(1).GetByNameAsync(diseaseName);
            _mockMapper.Received(1).Map<DiseaseDto>(entity);
        }

        [Fact]
        public async Task GetDiseaseByNameAsync_NotFound_ReturnsNull()
        {
            // Arrange
            const string diseaseName = "Nonexistent Disease";
            _mockRepository.GetByNameAsync(diseaseName).Returns((Disease?)null);

            // Act
            var result = await _sut.GetDiseaseByNameAsync(diseaseName);

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetByNameAsync(diseaseName);
            _mockMapper.DidNotReceive().Map<DiseaseDto>(Arg.Any<Disease>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetDiseaseByNameAsync_NullOrWhitespaceName_ThrowsArgumentException(string? diseaseName)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.GetDiseaseByNameAsync(diseaseName!));

            await _mockRepository.DidNotReceive().GetByNameAsync(Arg.Any<string>());
        }

        #endregion

        #region CreateDiseaseAsync

        [Fact]
        public async Task CreateDiseaseAsync_ValidDto_MapsAndCallsRepoAddAsync_ReturnsMappedDto()
        {
            // Arrange
            var dto = new DiseaseDto { DiseaseName = "Avian Influenza" };
            var entity = new Disease { DiseaseName = "Avian Influenza" };
            var addedEntity = new Disease { DiseaseName = "Avian Influenza" };
            var expectedDto = new DiseaseDto { DiseaseName = "Avian Influenza" };

            _mockRepository.ExistsAsync(dto.DiseaseName).Returns(false);
            _mockMapper.Map<Disease>(dto).Returns(entity);
            _mockRepository.AddAsync(entity).Returns(addedEntity);
            _mockMapper.Map<DiseaseDto>(addedEntity).Returns(expectedDto);

            // Act
            var result = await _sut.CreateDiseaseAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.DiseaseName.Should().Be("Avian Influenza");

            await _mockRepository.Received(1).ExistsAsync(dto.DiseaseName);
            await _mockRepository.Received(1).AddAsync(entity);
        }

        [Fact]
        public async Task CreateDiseaseAsync_NullDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.CreateDiseaseAsync(null!));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateDiseaseAsync_NullOrEmptyOrWhitespaceDiseaseName_ThrowsArgumentException(string? diseaseName)
        {
            // Arrange
            var dto = new DiseaseDto { DiseaseName = diseaseName! };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.CreateDiseaseAsync(dto));
        }

        [Fact]
        public async Task CreateDiseaseAsync_WhenExistsAsyncReturnsTrue_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new DiseaseDto { DiseaseName = "Bovine Tuberculosis" };

            _mockRepository.ExistsAsync(dto.DiseaseName).Returns(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.CreateDiseaseAsync(dto));

            exception.Message.Should().Contain(dto.DiseaseName);
            exception.Message.Should().Contain("already exists");

            await _mockRepository.Received(1).ExistsAsync(dto.DiseaseName);
            await _mockRepository.DidNotReceive().AddAsync(Arg.Any<Disease>());
        }

        #endregion

        #region DeleteDiseaseAsync

        [Fact]
        public async Task DeleteDiseaseAsync_ValidName_RepoReturnsTrue_ReturnsTrue()
        {
            // Arrange
            const string diseaseName = "Foot and Mouth Disease";
            _mockRepository.DeleteAsync(diseaseName).Returns(true);

            // Act
            var result = await _sut.DeleteDiseaseAsync(diseaseName);

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteAsync(diseaseName);
        }

        [Fact]
        public async Task DeleteDiseaseAsync_ValidName_RepoReturnsFalse_ReturnsFalse()
        {
            // Arrange
            const string diseaseName = "Nonexistent Disease";
            _mockRepository.DeleteAsync(diseaseName).Returns(false);

            // Act
            var result = await _sut.DeleteDiseaseAsync(diseaseName);

            // Assert
            result.Should().BeFalse();
            await _mockRepository.Received(1).DeleteAsync(diseaseName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task DeleteDiseaseAsync_NullOrWhitespaceName_ThrowsArgumentException(string? diseaseName)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.DeleteDiseaseAsync(diseaseName!));

            await _mockRepository.DidNotReceive().DeleteAsync(Arg.Any<string>());
        }

        #endregion
    }
}
