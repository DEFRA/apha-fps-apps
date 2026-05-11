using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactProjectSubContractApiClientTest
{
    public class PactProjectSubContractApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactProjectSubContractApiClient _client;

        public PactProjectSubContractApiClientTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PactProjectSubContractApiClient(_http, _mapper);
        }

        #region GetPagedProjectSubContractsAsync Tests

        [Fact]
        public async Task GetPagedProjectSubContractsAsync_WithProject_IncludesProjectInUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var project = "PP001";
            var subContractList = new List<ProjectSubContractRes>
            {
                new() { SubContCounter = 1, Project = project, Amount = 300.00m },
                new() { SubContCounter = 2, Project = project, Amount = 600.00m }
            };
            var apiResponse = new ApiResponse<List<ProjectSubContractRes>>
            {
                Success = true,
                Data = subContractList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };
            var expectedDto = ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse(
                new List<ProjectSubContractDto>
                {
                    new() { SubContCounter = 1, Project = project },
                    new() { SubContCounter = 2, Project = project }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );

            _http.GetAsync<List<ProjectSubContractRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/projectsubcontract") && url.Contains("project=PP001")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectSubContractDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectSubContractsAsync(query, project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<ProjectSubContractRes>>(
                Arg.Is<string>(url => url.Contains("api/v1/projectsubcontract") && url.Contains("project=PP001")));
        }

        [Fact]
        public async Task GetPagedProjectSubContractsAsync_WithNullProject_UsesBaseUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProjectSubContractRes>> { Success = true, Data = new List<ProjectSubContractRes>() };
            var expectedDto = ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse(new List<ProjectSubContractDto>(), new PaginationDto());

            _http.GetAsync<List<ProjectSubContractRes>>(Arg.Is<string>(url => url.Contains("api/v1/projectsubcontract"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectSubContractDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectSubContractsAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<ProjectSubContractRes>>(Arg.Is<string>(url => url.Contains("api/v1/projectsubcontract")));
        }

        [Fact]
        public async Task GetPagedProjectSubContractsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<ProjectSubContractRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<ProjectSubContractDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectSubContractRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectSubContractDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetPagedProjectSubContractsAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetTotalAmountAsync Tests

        [Fact]
        public async Task GetTotalAmountAsync_WithProject_IncludesProjectInUrl()
        {
            // Arrange
            var project = "PP001";
            var apiResponse = new ApiResponse<decimal?> { Success = true, Data = 2000.00m };
            var expectedDto = ApiResponseDto<decimal>.SuccessResponse(2000.00m);

            _http.GetAsync<decimal?>(Arg.Is<string>(url =>
                url.Contains("api/v1/projectsubcontract/total") && url.Contains("project=PP001")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetTotalAmountAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2000.00m, result.Data);
            await _http.Received(1).GetAsync<decimal?>(
                Arg.Is<string>(url => url.Contains("api/v1/projectsubcontract/total") && url.Contains("project=PP001")));
        }

        [Fact]
        public async Task GetTotalAmountAsync_WithNullProject_UsesBaseUrl()
        {
            // Arrange
            var apiResponse = new ApiResponse<decimal?> { Success = true, Data = 0m };
            var expectedDto = ApiResponseDto<decimal>.SuccessResponse(0m);

            _http.GetAsync<decimal?>("api/v1/projectsubcontract/total").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetTotalAmountAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<decimal?>("api/v1/projectsubcontract/total");
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsMappedSubContract()
        {
            // Arrange
            var subContCounter = 1;
            var subContractRes = new ProjectSubContractRes { SubContCounter = subContCounter, Project = "PP001", Amount = 400.00m };
            var apiResponse = new ApiResponse<ProjectSubContractRes> { Success = true, Data = subContractRes };
            var expectedDto = ApiResponseDto<ProjectSubContractDto>.SuccessResponse(
                new ProjectSubContractDto { SubContCounter = subContCounter, Project = "PP001" }
            );

            _http.GetAsync<ProjectSubContractRes>($"api/v1/projectsubcontract/{subContCounter}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectSubContractDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetByIdAsync(subContCounter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(subContCounter, result.Data?.SubContCounter);
            await _http.Received(1).GetAsync<ProjectSubContractRes>($"api/v1/projectsubcontract/{subContCounter}");
        }

        [Fact]
        public async Task GetByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<ProjectSubContractRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<ProjectSubContractDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<ProjectSubContractRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectSubContractDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetByIdAsync(9999);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidSubContract_ReturnsMappedCreatedSubContract()
        {
            // Arrange
            var subContractDto = new ProjectSubContractDto { SubContCounter = 1, Project = "PP001", Supplier = "Supplier A", Amount = 500.00m };
            var subContractReq = new ProjectSubContractReq { Project = "PP001", Supplier = "Supplier A", Amount = 500.00m };
            var subContractRes = new ProjectSubContractRes { SubContCounter = 1, Project = "PP001", Amount = 500.00m };
            var apiResponse = new ApiResponse<ProjectSubContractRes> { Success = true, Data = subContractRes };
            var expectedDto = ApiResponseDto<ProjectSubContractDto>.SuccessResponse(subContractDto);

            _mapper.Map<ProjectSubContractReq>(subContractDto).Returns(subContractReq);
            _http.PostAsync<ProjectSubContractReq, ProjectSubContractRes>("api/v1/projectsubcontract", subContractReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectSubContractDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreateAsync(subContractDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(1, result.Data?.SubContCounter);
            await _http.Received(1).PostAsync<ProjectSubContractReq, ProjectSubContractRes>("api/v1/projectsubcontract", subContractReq);
        }

        [Fact]
        public async Task CreateAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var subContractDto = new ProjectSubContractDto { Project = "PP001" };
            var subContractReq = new ProjectSubContractReq { Project = "PP001" };
            var errors = new List<ApiError> { new() { Message = "Duplicate", Code = "DUPLICATE" } };
            var apiResponse = new ApiResponse<ProjectSubContractRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<ProjectSubContractDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Duplicate", Code = "DUPLICATE" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ProjectSubContractReq>(subContractDto).Returns(subContractReq);
            _http.PostAsync<ProjectSubContractReq, ProjectSubContractRes>(Arg.Any<string>(), Arg.Any<ProjectSubContractReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectSubContractDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CreateAsync(subContractDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidSubContract_ReturnsMappedUpdatedSubContract()
        {
            // Arrange
            var subContCounter = 1;
            var subContractDto = new ProjectSubContractDto { SubContCounter = subContCounter, Project = "PP001", Amount = 800.00m };
            var subContractReq = new ProjectSubContractReq { Project = "PP001", Amount = 800.00m };
            var subContractRes = new ProjectSubContractRes { SubContCounter = subContCounter, Project = "PP001", Amount = 800.00m };
            var apiResponse = new ApiResponse<ProjectSubContractRes> { Success = true, Data = subContractRes };
            var expectedDto = ApiResponseDto<ProjectSubContractDto>.SuccessResponse(subContractDto);

            _mapper.Map<ProjectSubContractReq>(subContractDto).Returns(subContractReq);
            _http.PutAsync<ProjectSubContractReq, ProjectSubContractRes>($"api/v1/projectsubcontract/{subContCounter}", subContractReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectSubContractDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateAsync(subContCounter, subContractDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(800.00m, result.Data?.Amount);
            await _http.Received(1).PutAsync<ProjectSubContractReq, ProjectSubContractRes>($"api/v1/projectsubcontract/{subContCounter}", subContractReq);
        }

        [Fact]
        public async Task UpdateAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var subContCounter = 9999;
            var subContractDto = new ProjectSubContractDto { SubContCounter = subContCounter, Project = "PP001" };
            var subContractReq = new ProjectSubContractReq { Project = "PP001" };
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<ProjectSubContractRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<ProjectSubContractDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ProjectSubContractReq>(subContractDto).Returns(subContractReq);
            _http.PutAsync<ProjectSubContractReq, ProjectSubContractRes>(Arg.Any<string>(), Arg.Any<ProjectSubContractReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectSubContractDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateAsync(subContCounter, subContractDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetMonthlySubContractsSummaryAsync Tests

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_WhenApiReturnsSuccess_ReturnsMappedDto()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pivotRes = new MonthlySubContractsPivotRes
            {
                Months = [1, 2, 3],
                Rows = [new MonthlySubContractsSummaryItemRes { Program = "ADMIN", ParentProject = "AH" }],
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var apiResponse = new ApiResponse<MonthlySubContractsPivotRes> { Success = true, Data = pivotRes };
            var expectedDto = new MonthlySubContractsPivotDto
            {
                Months = [1, 2, 3],
                Rows = [new MonthlySubContractsSummaryItemDto { Program = "ADMIN", ParentProject = "AH" }],
                Pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _http.GetAsync<MonthlySubContractsPivotRes>(
                    Arg.Is<string>(url => url.Contains("api/v1/projectsubcontract/monthly-summary")))
                .Returns(apiResponse);
            _mapper.Map<MonthlySubContractsPivotDto>(pivotRes).Returns(expectedDto);

            // Act
            var result = await _client.GetMonthlySubContractsSummaryAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Months.Count);
            Assert.Single(result.Data.Rows);
            await _http.Received(1).GetAsync<MonthlySubContractsPivotRes>(
                Arg.Is<string>(url => url.Contains("api/v1/projectsubcontract/monthly-summary")));
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_WithQueryParameters_IncludesQueryStringInUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 5, SortBy = "program", Descending = true };
            var apiResponse = new ApiResponse<MonthlySubContractsPivotRes> { Success = true, Data = new MonthlySubContractsPivotRes() };
            var expectedDto = new MonthlySubContractsPivotDto();

            _http.GetAsync<MonthlySubContractsPivotRes>(
                    Arg.Is<string>(url => url.Contains("api/v1/projectsubcontract/monthly-summary")))
                .Returns(apiResponse);
            _mapper.Map<MonthlySubContractsPivotDto>(Arg.Any<MonthlySubContractsPivotRes>()).Returns(expectedDto);

            // Act
            await _client.GetMonthlySubContractsSummaryAsync(query);

            // Assert
            await _http.Received(1).GetAsync<MonthlySubContractsPivotRes>(
                Arg.Is<string>(url => url.Contains("api/v1/projectsubcontract/monthly-summary")));
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_WhenApiReturnsSuccess_MapsResponseData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pivotRes = new MonthlySubContractsPivotRes();
            var apiResponse = new ApiResponse<MonthlySubContractsPivotRes> { Success = true, Data = pivotRes };
            var expectedDto = new MonthlySubContractsPivotDto();

            _http.GetAsync<MonthlySubContractsPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<MonthlySubContractsPivotDto>(pivotRes).Returns(expectedDto);

            // Act
            var result = await _client.GetMonthlySubContractsSummaryAsync(query);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<MonthlySubContractsPivotDto>(pivotRes);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<MonthlySubContractsPivotRes> { Success = false, Errors = errors };
            var mappedFailure = new ApiResponseDto<MonthlySubContractsPivotDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<MonthlySubContractsPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlySubContractsPivotDto>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetMonthlySubContractsSummaryAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_WhenApiReturnsFailure_DoesNotMapResponseData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<MonthlySubContractsPivotRes> { Success = false, Errors = [] };
            var mappedFailure = new ApiResponseDto<MonthlySubContractsPivotDto>
            {
                Success = false,
                Errors = [],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<MonthlySubContractsPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlySubContractsPivotDto>>(apiResponse).Returns(mappedFailure);

            // Act
            await _client.GetMonthlySubContractsSummaryAsync(query);

            // Assert
            _mapper.DidNotReceive().Map<MonthlySubContractsPivotDto>(Arg.Any<MonthlySubContractsPivotRes>());
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithValidId_ReturnsSuccess()
        {
            // Arrange
            var subContCounter = 1;
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>($"api/v1/projectsubcontract/{subContCounter}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteAsync(subContCounter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool?>($"api/v1/projectsubcontract/{subContCounter}");
        }

        [Fact]
        public async Task DeleteAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteAsync(9999);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion
    }
}
