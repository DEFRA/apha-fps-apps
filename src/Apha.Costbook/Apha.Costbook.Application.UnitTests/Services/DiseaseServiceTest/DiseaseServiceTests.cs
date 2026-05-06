using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Services;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess;
using AutoMapper;
using NSubstitute;

namespace Apha.Costbook.Application.UnitTests.Services.DiseaseServiceTest
{
    public class DiseaseServiceTests
    {
        private readonly IDiseaseRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly DiseaseService _diseaseService;

        public DiseaseServiceTests()
        {
            _mockRepository = Substitute.For<IDiseaseRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _diseaseService = new DiseaseService(_mockRepository, _mockMapper);
        }

        [Fact]
        public async Task GetAllDiseasesAsync_ReturnsDiseasesDtos()
        {
            // Arrange
            var diseases = new List<Disease>
            {
                new Disease { DiseaseName = "Disease A" },
                new Disease { DiseaseName = "Disease B" }
            };
            var diseaseDtos = new List<DiseaseDto>
            {
                new DiseaseDto { DiseaseName = "Disease A" },
                new DiseaseDto { DiseaseName = "Disease B" }
            };

            _mockRepository.GetAllDiseasesAsync().Returns(diseases);
            _mockMapper.Map<List<DiseaseDto>>(diseases).Returns(diseaseDtos);

            // Act
            var result = await _diseaseService.GetAllDiseasesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Disease A", result[0].DiseaseName);
            Assert.Equal("Disease B", result[1].DiseaseName);
            await _mockRepository.Received(1).GetAllDiseasesAsync();
            _mockMapper.Received(1).Map<List<DiseaseDto>>(diseases);
        }

        [Fact]
        public async Task GetAllDiseasesAsync_EmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var diseases = new List<Disease>();
            var diseaseDtos = new List<DiseaseDto>();

            _mockRepository.GetAllDiseasesAsync().Returns(diseases);
            _mockMapper.Map<List<DiseaseDto>>(diseases).Returns(diseaseDtos);

            // Act
            var result = await _diseaseService.GetAllDiseasesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            await _mockRepository.Received(1).GetAllDiseasesAsync();
            _mockMapper.Received(1).Map<List<DiseaseDto>>(diseases);
        }

        [Fact]
        public async Task GetAllDiseasesAsync_SingleResult_ReturnsSingleItem()
        {
            // Arrange
            var diseases = new List<Disease>
            {
                new Disease { DiseaseName = "Disease A" }
            };
            var diseaseDtos = new List<DiseaseDto>
            {
                new DiseaseDto { DiseaseName = "Disease A" }
            };

            _mockRepository.GetAllDiseasesAsync().Returns(diseases);
            _mockMapper.Map<List<DiseaseDto>>(diseases).Returns(diseaseDtos);

            // Act
            var result = await _diseaseService.GetAllDiseasesAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Disease A", result[0].DiseaseName);
            await _mockRepository.Received(1).GetAllDiseasesAsync();
        }
    }
}
