using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.TestListServiceTest
{
    public class TestPriceCheckServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactTestorProductApiClient _apiClient;
        private readonly TestorProductService _service;

        public TestPriceCheckServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _apiClient  = Substitute.For<IPactTestorProductApiClient>();
            _pactClient.PactTestList.Returns(_apiClient);
            _service = new TestorProductService(_pactClient);
        }

        #region GetTestPriceCheckPagedAsync

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_Success_ReturnsPagedDtos()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var items = new List<TestPriceCheckDto>
            {
                new() { TestCode = "T001", JobCode = "JOB001", TestPrice = 50m  },
                new() { TestCode = "T002", JobCode = "JOB002", TestPrice = 0m   }
            };
            var response = ApiResponseDto<List<TestPriceCheckDto>>.SuccessResponse(items);
            _apiClient.GetTestPriceCheckPagedAsync(query, "all", null).Returns(response);

            var result = await _service.GetTestPriceCheckPagedAsync(query, "all", null);

            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            await _apiClient.Received(1).GetTestPriceCheckPagedAsync(query, "all", null);
        }

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_WithPriceFilterAndOwner_ForwardsParametersToApiClient()
        {
            var query    = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var response = ApiResponseDto<List<TestPriceCheckDto>>.SuccessResponse(new List<TestPriceCheckDto>());
            _apiClient.GetTestPriceCheckPagedAsync(query, "zero", "AB").Returns(response);

            await _service.GetTestPriceCheckPagedAsync(query, "zero", "AB");

            await _apiClient.Received(1).GetTestPriceCheckPagedAsync(query, "zero", "AB");
        }

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_ApiFails_ReturnsFailureResponse()
        {
            var query  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "ERR" } };
            var response = ApiResponseDto<List<TestPriceCheckDto>>.FailureResponse(errors, new ApiMetaDto());
            _apiClient.GetTestPriceCheckPagedAsync(query, "all", null).Returns(response);

            var result = await _service.GetTestPriceCheckPagedAsync(query, "all", null);

            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region GetTestPriceCheckByKeyAsync

        [Fact]
        public async Task GetTestPriceCheckByKeyAsync_ExistingKey_ReturnsDto()
        {
            var dto = new TestPriceCheckDto { TestCode = "T001", JobCode = "JOB001", NormalPrice = 50m };
            var response = ApiResponseDto<TestPriceCheckDto>.SuccessResponse(dto);
            _apiClient.GetTestPriceCheckByKeyAsync("T001", "JOB001").Returns(response);

            var result = await _service.GetTestPriceCheckByKeyAsync("T001", "JOB001");

            Assert.True(result.Success);
            Assert.Equal("T001",   result.Data!.TestCode);
            Assert.Equal("JOB001", result.Data.JobCode);
        }

        [Fact]
        public async Task GetTestPriceCheckByKeyAsync_ApiFails_ReturnsFailureResponse()
        {
            var errors   = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var response = ApiResponseDto<TestPriceCheckDto>.FailureResponse(errors, new ApiMetaDto());
            _apiClient.GetTestPriceCheckByKeyAsync("MISSING", "MISSING").Returns(response);

            var result = await _service.GetTestPriceCheckByKeyAsync("MISSING", "MISSING");

            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetTestPriceCheckByKeyAsync_CallsApiClientOnce()
        {
            var response = ApiResponseDto<TestPriceCheckDto>.SuccessResponse(
                new TestPriceCheckDto { TestCode = "T001", JobCode = "JOB001" });
            _apiClient.GetTestPriceCheckByKeyAsync("T001", "JOB001").Returns(response);

            await _service.GetTestPriceCheckByKeyAsync("T001", "JOB001");

            await _apiClient.Received(1).GetTestPriceCheckByKeyAsync("T001", "JOB001");
        }

        #endregion

        #region UpdateTestPriceCheckByKeyAsync

        [Fact]
        public async Task UpdateTestPriceCheckByKeyAsync_Success_ReturnsTrue()
        {
            var dto = new TestPriceCheckDto { IsDefraProject = -1, TestPrice = 75m, DefraUnitPrice = 120m };
            var response = ApiResponseDto<bool>.SuccessResponse(true);
            _apiClient.UpdateTestPriceCheckByKeyAsync("T001", "JOB001", dto).Returns(response);

            var result = await _service.UpdateTestPriceCheckByKeyAsync("T001", "JOB001", dto);

            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task UpdateTestPriceCheckByKeyAsync_ApiFails_ReturnsFailureResponse()
        {
            var dto      = new TestPriceCheckDto { IsDefraProject = 0, TestPrice = 50m, DefraUnitPrice = 80m };
            var errors   = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "ERR" } };
            var response = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _apiClient.UpdateTestPriceCheckByKeyAsync("T001", "JOB001", dto).Returns(response);

            var result = await _service.UpdateTestPriceCheckByKeyAsync("T001", "JOB001", dto);

            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        [Fact]
        public async Task UpdateTestPriceCheckByKeyAsync_CallsApiClientOnce()
        {
            var dto      = new TestPriceCheckDto { IsDefraProject = 0, TestPrice = 50m, DefraUnitPrice = 80m };
            var response = ApiResponseDto<bool>.SuccessResponse(true);
            _apiClient.UpdateTestPriceCheckByKeyAsync("T001", "JOB001", dto).Returns(response);

            await _service.UpdateTestPriceCheckByKeyAsync("T001", "JOB001", dto);

            await _apiClient.Received(1).UpdateTestPriceCheckByKeyAsync("T001", "JOB001", dto);
        }

        #endregion
    }
}
