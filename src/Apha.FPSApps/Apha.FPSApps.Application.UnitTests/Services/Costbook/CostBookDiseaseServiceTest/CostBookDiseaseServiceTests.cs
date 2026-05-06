using System;
using System.Collections.Generic;
using System.Text;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Services.Costbook;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.Costbook.CostBookDiseaseServiceTest
{
    public class CostBookDiseaseServiceTests
    {
        private readonly ICostBookApiClient _costBookClient;
        private readonly ICostBookDiseaseApiClient _costBookDiseaseApiClient;
        private readonly CostBookDiseaseService _diseaseService;

        public CostBookDiseaseServiceTests()
        {
            _costBookClient = Substitute.For<ICostBookApiClient>();
            _costBookDiseaseApiClient = Substitute.For<ICostBookDiseaseApiClient>();
            _costBookClient.Diseases.Returns(_costBookDiseaseApiClient);
            _diseaseService = new CostBookDiseaseService(_costBookClient);
        }

        #region GetAllDiseasesAsync Tests

        [Fact]
        public async Task GetAllDiseasesAsync_WithSuccessResponse_ReturnsDiseaseList()
        {
            // Arrange
            var diseases = new List<DiseaseDto>
            {
                new DiseaseDto { DiseaseName = "Disease 1" },
                new DiseaseDto { DiseaseName = "Disease 2" }
            };
            var expectedResponse = ApiResponseDto<List<DiseaseDto>>.SuccessResponse(diseases);

            _costBookDiseaseApiClient.GetAllDiseasesAsync().Returns(expectedResponse);

            // Act
            var result = await _diseaseService.GetAllDiseasesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _costBookDiseaseApiClient.Received(1).GetAllDiseasesAsync();
        }

        [Fact]
        public async Task GetAllDiseasesAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<DiseaseDto>>.SuccessResponse(new List<DiseaseDto>());

            _costBookDiseaseApiClient.GetAllDiseasesAsync().Returns(expectedResponse);

            // Act
            var result = await _diseaseService.GetAllDiseasesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllDiseasesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<DiseaseDto>>.FailureResponse(errors, new ApiMetaDto());

            _costBookDiseaseApiClient.GetAllDiseasesAsync().Returns(expectedResponse);

            // Act
            var result = await _diseaseService.GetAllDiseasesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidClient_InitializesService()
        {
            // Arrange & Act
            var service = new CostBookDiseaseService(_costBookClient);

            // Assert
            Assert.NotNull(service);
        }

        #endregion
    }
}
