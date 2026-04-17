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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactProjectInvoiceApiClientTest
{
    public class PactProjectInvoiceApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactProjectInvoiceApiClient _client;

        public PactProjectInvoiceApiClientTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PactProjectInvoiceApiClient(_http, _mapper);
        }

        #region GetPagedProjectInvoicesAsync Tests

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_WithParentProject_IncludesProjectInUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var parentProject = "PP001";
            var invoiceList = new List<ProjectInvoiceRes>
            {
                new() { InvoiceCounter = 1, ProjectParent = parentProject, Amount = 100.00m },
                new() { InvoiceCounter = 2, ProjectParent = parentProject, Amount = 200.00m }
            };
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>>
            {
                Success = true,
                Data = invoiceList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };
            var expectedDto = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(
                new List<ProjectInvoiceDto>
                {
                    new() { InvoiceCounter = 1, ProjectParent = parentProject },
                    new() { InvoiceCounter = 2, ProjectParent = parentProject }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );

            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/projectinvoice") && url.Contains($"parentProject={Uri.EscapeDataString(parentProject)}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectInvoicesAsync(query, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<ProjectInvoiceRes>>(
                Arg.Is<string>(url => url.Contains("api/v1/projectinvoice") && url.Contains($"parentProject={Uri.EscapeDataString(parentProject)}")));
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_WithNullParentProject_UsesBaseUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>> { Success = true, Data = new List<ProjectInvoiceRes>() };
            var expectedDto = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(new List<ProjectInvoiceDto>(), new PaginationDto());

            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Is<string>(url => url.Contains("api/v1/projectinvoice"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectInvoicesAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<ProjectInvoiceRes>>(Arg.Is<string>(url => url.Contains("api/v1/projectinvoice")));
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<ProjectInvoiceDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetPagedProjectInvoicesAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetTotalAmountAsync Tests

        [Fact]
        public async Task GetTotalAmountAsync_WithParentProject_IncludesProjectInUrl()
        {
            // Arrange
            var parentProject = "PP001";
            var apiResponse = new ApiResponse<decimal?> { Success = true, Data = 1500.00m };
            var expectedDto = ApiResponseDto<decimal>.SuccessResponse(1500.00m);

            _http.GetAsync<decimal?>(Arg.Is<string>(url =>
                url.Contains("api/v1/projectinvoice/total") && url.Contains("parentProject=PP001")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetTotalAmountAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(1500.00m, result.Data);
            await _http.Received(1).GetAsync<decimal?>(
                Arg.Is<string>(url => url.Contains("api/v1/projectinvoice/total") && url.Contains("parentProject=PP001")));
        }

        [Fact]
        public async Task GetTotalAmountAsync_WithNullParentProject_UsesBaseUrl()
        {
            // Arrange
            var apiResponse = new ApiResponse<decimal?> { Success = true, Data = 0m };
            var expectedDto = ApiResponseDto<decimal>.SuccessResponse(0m);

            _http.GetAsync<decimal?>("api/v1/projectinvoice/total").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetTotalAmountAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<decimal?>("api/v1/projectinvoice/total");
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsMappedInvoice()
        {
            // Arrange
            var invoiceCounter = 1;
            var invoiceRes = new ProjectInvoiceRes { InvoiceCounter = invoiceCounter, ProjectParent = "PP001", Amount = 500.00m };
            var apiResponse = new ApiResponse<ProjectInvoiceRes> { Success = true, Data = invoiceRes };
            var expectedDto = ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(
                new ProjectInvoiceDto { InvoiceCounter = invoiceCounter, ProjectParent = "PP001" }
            );

            _http.GetAsync<ProjectInvoiceRes>($"api/v1/projectinvoice/{invoiceCounter}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectInvoiceDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetByIdAsync(invoiceCounter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(invoiceCounter, result.Data?.InvoiceCounter);
            await _http.Received(1).GetAsync<ProjectInvoiceRes>($"api/v1/projectinvoice/{invoiceCounter}");
        }

        [Fact]
        public async Task GetByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<ProjectInvoiceRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<ProjectInvoiceDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<ProjectInvoiceRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectInvoiceDto>>(apiResponse).Returns(mappedResponse);

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
        public async Task CreateAsync_WithValidInvoice_ReturnsMappedCreatedInvoice()
        {
            // Arrange
            var invoiceDto = new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "PP001", Amount = 750.00m };
            var invoiceReq = new ProjectInvoiceReq { ProjectParent = "PP001", Amount = 750.00m };
            var invoiceRes = new ProjectInvoiceRes { InvoiceCounter = 1, ProjectParent = "PP001", Amount = 750.00m };
            var apiResponse = new ApiResponse<ProjectInvoiceRes> { Success = true, Data = invoiceRes };
            var expectedDto = ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(invoiceDto);

            _mapper.Map<ProjectInvoiceReq>(invoiceDto).Returns(invoiceReq);
            _http.PostAsync<ProjectInvoiceReq, ProjectInvoiceRes>("api/v1/projectinvoice", invoiceReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectInvoiceDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreateAsync(invoiceDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(1, result.Data?.InvoiceCounter);
            await _http.Received(1).PostAsync<ProjectInvoiceReq, ProjectInvoiceRes>("api/v1/projectinvoice", invoiceReq);
        }

        [Fact]
        public async Task CreateAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var invoiceDto = new ProjectInvoiceDto { ProjectParent = "PP001" };
            var invoiceReq = new ProjectInvoiceReq { ProjectParent = "PP001" };
            var errors = new List<ApiError> { new() { Message = "Duplicate", Code = "DUPLICATE" } };
            var apiResponse = new ApiResponse<ProjectInvoiceRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<ProjectInvoiceDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Duplicate", Code = "DUPLICATE" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ProjectInvoiceReq>(invoiceDto).Returns(invoiceReq);
            _http.PostAsync<ProjectInvoiceReq, ProjectInvoiceRes>(Arg.Any<string>(), Arg.Any<ProjectInvoiceReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectInvoiceDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CreateAsync(invoiceDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidInvoice_ReturnsMappedUpdatedInvoice()
        {
            // Arrange
            var invoiceCounter = 1;
            var invoiceDto = new ProjectInvoiceDto { InvoiceCounter = invoiceCounter, ProjectParent = "PP001", Amount = 900.00m };
            var invoiceReq = new ProjectInvoiceReq { ProjectParent = "PP001", Amount = 900.00m };
            var invoiceRes = new ProjectInvoiceRes { InvoiceCounter = invoiceCounter, ProjectParent = "PP001", Amount = 900.00m };
            var apiResponse = new ApiResponse<ProjectInvoiceRes> { Success = true, Data = invoiceRes };
            var expectedDto = ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(invoiceDto);

            _mapper.Map<ProjectInvoiceReq>(invoiceDto).Returns(invoiceReq);
            _http.PutAsync<ProjectInvoiceReq, ProjectInvoiceRes>($"api/v1/projectinvoice/{invoiceCounter}", invoiceReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectInvoiceDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateAsync(invoiceCounter, invoiceDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(900.00m, result.Data?.Amount);
            await _http.Received(1).PutAsync<ProjectInvoiceReq, ProjectInvoiceRes>($"api/v1/projectinvoice/{invoiceCounter}", invoiceReq);
        }

        [Fact]
        public async Task UpdateAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var invoiceCounter = 9999;
            var invoiceDto = new ProjectInvoiceDto { InvoiceCounter = invoiceCounter, ProjectParent = "PP001" };
            var invoiceReq = new ProjectInvoiceReq { ProjectParent = "PP001" };
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<ProjectInvoiceRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<ProjectInvoiceDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ProjectInvoiceReq>(invoiceDto).Returns(invoiceReq);
            _http.PutAsync<ProjectInvoiceReq, ProjectInvoiceRes>(Arg.Any<string>(), Arg.Any<ProjectInvoiceReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectInvoiceDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateAsync(invoiceCounter, invoiceDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithValidId_ReturnsSuccess()
        {
            // Arrange
            var invoiceCounter = 1;
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>($"api/v1/projectinvoice/{invoiceCounter}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteAsync(invoiceCounter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool?>($"api/v1/projectinvoice/{invoiceCounter}");
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
