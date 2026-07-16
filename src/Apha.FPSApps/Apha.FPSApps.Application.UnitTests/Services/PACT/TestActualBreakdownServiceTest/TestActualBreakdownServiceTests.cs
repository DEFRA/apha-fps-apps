using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.TestActualBreakdownServiceTest
{
    public class TestActualBreakdownServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactTestActualBreakdownApiClient _apiClient;
        private readonly TestActualBreakdownService _service;

        public TestActualBreakdownServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _apiClient  = Substitute.For<IPactTestActualBreakdownApiClient>();
            _pactClient.PactTestActualBreakdown.Returns(_apiClient);
            _service = new TestActualBreakdownService(_pactClient);
        }

        #region GetPagedAsync

        [Fact]
        public async Task GetPagedAsync_DelegatesToApiClient_ReturnsResult()
        {
            var query    = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse(
            [
                new TestActualBreakdownDto { TestCode = "PT0047", Buyer = "SV3300", Program = "Viro", Portfolio = "QAPTPORT1", Month = 4, PCPrice = 159.00m, PCCost = 319.00m }
            ]);

            _apiClient.GetPagedAsync(query).Returns(expected);

            var result = await _service.GetPagedAsync(query);

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetPagedAsync(query);
        }

        [Fact]
        public async Task GetPagedAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            var query    = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse([]);

            _apiClient.GetPagedAsync(query).Returns(expected);

            var result = await _service.GetPagedAsync(query);

            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPagedAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var query    = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors   = new List<ApiErrorDto> { new() { Code = "API_ERROR" } };
            var expected = ApiResponseDto<List<TestActualBreakdownDto>>.FailureResponse(errors, new ApiMetaDto());

            _apiClient.GetPagedAsync(query).Returns(expected);

            var result = await _service.GetPagedAsync(query);

            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetPagedAsync_AllDtoPropertiesPopulated_ReturnsAllValues()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dto   = new TestActualBreakdownDto
            {
                TestCode         = "PT0047",
                ShortDescription = "EVA serology",
                Program          = "Viro",
                Buyer            = "SV3300",
                Portfolio        = "QAPTPORT1",
                WorkGroup        = "QASB",
                ProfitCentre     = "Comm",
                Month            = 4,
                PCPrice          = 159.00m,
                PCCost           = 319.00m
            };
            var expected = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse([dto]);

            _apiClient.GetPagedAsync(query).Returns(expected);

            var result = await _service.GetPagedAsync(query);

            var item = result.Data!.Single();
            Assert.Equal("PT0047",       item.TestCode);
            Assert.Equal("EVA serology", item.ShortDescription);
            Assert.Equal("Viro",         item.Program);
            Assert.Equal("SV3300",       item.Buyer);
            Assert.Equal("QAPTPORT1",    item.Portfolio);
            Assert.Equal("QASB",         item.WorkGroup);
            Assert.Equal("Comm",         item.ProfitCentre);
            Assert.Equal(4,              item.Month);
            Assert.Equal(159.00m,        item.PCPrice);
            Assert.Equal(319.00m,        item.PCCost);
        }

        [Fact]
        public async Task GetPagedAsync_MultipleItems_ReturnAllItems()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos  = new List<TestActualBreakdownDto>
            {
                new() { TestCode = "PT0047", Buyer = "SV3300" },
                new() { TestCode = "PT0049", Buyer = "SB4600" },
                new() { TestCode = "TC0001A", Buyer = "EDI300" }
            };
            var expected = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse(dtos, new PaginationDto());

            _apiClient.GetPagedAsync(query).Returns(expected);

            var result = await _service.GetPagedAsync(query);

            Assert.True(result.Success);
            Assert.Equal(3, result.Data!.Count);
        }

        [Fact]
        public async Task GetPagedAsync_WithPaginationParams_PassesQueryToClient()
        {
            var query    = new QueryParameters<string> { Page = 2, PageSize = 25, SortBy = "buyer", Descending = true };
            var expected = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse([]);

            _apiClient.GetPagedAsync(query).Returns(expected);

            await _service.GetPagedAsync(query);

            await _apiClient.Received(1).GetPagedAsync(query);
        }

        #endregion
    }
}
