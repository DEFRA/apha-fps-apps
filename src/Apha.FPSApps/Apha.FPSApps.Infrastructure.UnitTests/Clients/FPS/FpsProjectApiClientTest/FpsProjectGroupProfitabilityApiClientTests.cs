using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsProjectApiClientTest
{
    public class FpsProjectGroupProfitabilityApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsProjectApiClient _client;

        public FpsProjectGroupProfitabilityApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsProjectApiClient(_http, _mapper);
        }

        // ── GetProjectGroupProfitabilityAsync ─────────────────────────────────

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_WithSuccessResponse_ReturnsMappedDtos()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectGroup = "Group1";
            var workTypeFilter = "all";

            var profitabilityList = new List<ProjectProfitabilityRes>
            {
                new() { JobCode = "PP001", BudgetCvl = 5000m, JcProfit = 4000m },
                new() { JobCode = "PP002", BudgetCvl = 6000m, JcProfit = 3500m }
            };
            var apiResponse = new ApiResponse<List<ProjectProfitabilityRes>>
            {
                Success = true,
                Data = profitabilityList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };
            var expectedDto = ApiResponseDto<List<ProjectProfitabilityDto>>.SuccessResponse(
                new List<ProjectProfitabilityDto>
                {
                    new() { JobCode = "PP001", BudgetCvl = 5000m, JcProfit = 4000m },
                    new() { JobCode = "PP002", BudgetCvl = 6000m, JcProfit = 3500m }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 });

            _http.GetAsync<List<ProjectProfitabilityRes>>(Arg.Is<string>(url =>
                    url.Contains("profitability/by-project-group") && url.Contains("Group1")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfitabilityDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProjectGroupProfitabilityAsync(query, projectGroup, workTypeFilter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            Assert.Equal("PP001", result.Data![0].JobCode);
        }

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_UrlContainsEscapedProjectGroup()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectGroup = "Group 1";  // space to test URI encoding
            var apiResponse = new ApiResponse<List<ProjectProfitabilityRes>> { Success = true, Data = new List<ProjectProfitabilityRes>() };
            var expectedDto = ApiResponseDto<List<ProjectProfitabilityDto>>.SuccessResponse(new List<ProjectProfitabilityDto>());

            _http.GetAsync<List<ProjectProfitabilityRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfitabilityDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetProjectGroupProfitabilityAsync(query, projectGroup, "all");

            // Assert
            await _http.Received(1).GetAsync<List<ProjectProfitabilityRes>>(
                Arg.Is<string>(url => url.Contains(Uri.EscapeDataString(projectGroup))));
        }

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_UrlContainsWorkTypeFilter()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var workTypeFilter = "approved";
            var apiResponse = new ApiResponse<List<ProjectProfitabilityRes>> { Success = true, Data = new List<ProjectProfitabilityRes>() };
            var expectedDto = ApiResponseDto<List<ProjectProfitabilityDto>>.SuccessResponse(new List<ProjectProfitabilityDto>());

            _http.GetAsync<List<ProjectProfitabilityRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfitabilityDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetProjectGroupProfitabilityAsync(query, "Group1", workTypeFilter);

            // Assert
            await _http.Received(1).GetAsync<List<ProjectProfitabilityRes>>(
                Arg.Is<string>(url => url.Contains($"workTypeFilter={Uri.EscapeDataString(workTypeFilter)}")));
        }

        [Fact]
        public async Task GetProjectGroupProfitabilityAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<List<ProjectProfitabilityRes>>
            {
                Success = false,
                Errors = errors
            };
            var mappedDto = new ApiResponseDto<List<ProjectProfitabilityDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectProfitabilityRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfitabilityDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetProjectGroupProfitabilityAsync(query, "Group1", "all");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Theory]
        [InlineData("approved")]
        [InlineData("not-approved")]
        [InlineData("all")]
        public async Task GetProjectGroupProfitabilityAsync_WithAllWorkTypeFilters_SendsCorrectUrl(string workTypeFilter)
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProjectProfitabilityRes>> { Success = true, Data = new List<ProjectProfitabilityRes>() };
            var expectedDto = ApiResponseDto<List<ProjectProfitabilityDto>>.SuccessResponse(new List<ProjectProfitabilityDto>());

            _http.GetAsync<List<ProjectProfitabilityRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfitabilityDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProjectGroupProfitabilityAsync(query, "Group1", workTypeFilter);

            // Assert
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<ProjectProfitabilityRes>>(
                Arg.Is<string>(url => url.Contains(Uri.EscapeDataString(workTypeFilter))));
        }
    }
}
