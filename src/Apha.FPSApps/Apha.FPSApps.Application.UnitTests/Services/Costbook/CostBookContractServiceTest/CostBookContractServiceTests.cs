using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Services.Costbook;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.Costbook.CostBookContractServiceTest
{
    public class CostBookContractServiceTests
    {
        private readonly ICostBookApiClient _costBookClient;
        private readonly ICostBookContractApiClient _costBookContractApiClient;
        private readonly CostBookContractService _contractService;

        public CostBookContractServiceTests()
        {
            _costBookClient = Substitute.For<ICostBookApiClient>();
            _costBookContractApiClient = Substitute.For<ICostBookContractApiClient>();
            _costBookClient.Contracts.Returns(_costBookContractApiClient);
            _contractService = new CostBookContractService(_costBookClient);
        }

        #region GetAllContractNumbersAsync Tests

        [Fact]
        public async Task GetAllContractNumbersAsync_WithSuccessResponse_ReturnsContractList()
        {
            // Arrange
            var contracts = new List<ContractDto>
            {
                new ContractDto { ContractNumber = "C001" },
                new ContractDto { ContractNumber = "C002" }
            };
            var expectedResponse = ApiResponseDto<List<ContractDto>>.SuccessResponse(contracts);

            _costBookContractApiClient.GetAllContractNumbersAsync().Returns(expectedResponse);

            // Act
            var result = await _contractService.GetAllContractNumbersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _costBookContractApiClient.Received(1).GetAllContractNumbersAsync();
        }

        [Fact]
        public async Task GetAllContractNumbersAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<ContractDto>>.SuccessResponse(new List<ContractDto>());

            _costBookContractApiClient.GetAllContractNumbersAsync().Returns(expectedResponse);

            // Act
            var result = await _contractService.GetAllContractNumbersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllContractNumbersAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<ContractDto>>.FailureResponse(errors, new ApiMetaDto());

            _costBookContractApiClient.GetAllContractNumbersAsync().Returns(expectedResponse);

            // Act
            var result = await _contractService.GetAllContractNumbersAsync();

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
            var service = new CostBookContractService(_costBookClient);

            // Assert
            Assert.NotNull(service);
        }

        #endregion
    }
}
