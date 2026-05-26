using Apha.Common.Constants;
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

        #region GetPagedProjectInvoiceManualAsync Tests

        [Fact]
        public async Task GetPagedProjectInvoiceManualAsync_WithParentProject_IncludesProjectInUrl()
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
            var result = await _client.GetPagedProjectInvoiceManualAsync(query, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<ProjectInvoiceRes>>(
                Arg.Is<string>(url => url.Contains("api/v1/projectinvoice") && url.Contains($"parentProject={Uri.EscapeDataString(parentProject)}")));
        }

        [Fact]
        public async Task GetPagedProjectInvoiceManualAsync_WithNullParentProject_UsesBaseUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>> { Success = true, Data = new List<ProjectInvoiceRes>() };
            var expectedDto = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(new List<ProjectInvoiceDto>(), new PaginationDto());

            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Is<string>(url => url.Contains("api/v1/projectinvoice"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectInvoiceManualAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<ProjectInvoiceRes>>(Arg.Is<string>(url => url.Contains("api/v1/projectinvoice") && !url.Contains("parentProject=")));
        }

        [Fact]
        public async Task GetPagedProjectInvoiceManualAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
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
            var result = await _client.GetPagedProjectInvoiceManualAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetPagedProjectInvoiceManualAsync_WithEmptyParentProject_DoesNotIncludeInUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>> { Success = true, Data = new List<ProjectInvoiceRes>() };
            var expectedDto = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(new List<ProjectInvoiceDto>(), new PaginationDto());

            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Is<string>(url => url.Contains("api/v1/projectinvoice") && !url.Contains("parentProject="))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectInvoiceManualAsync(query, "");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<ProjectInvoiceRes>>(Arg.Is<string>(url => !url.Contains("parentProject=")));
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
        public async Task GetTotalAmountAsync_WithNullParentProject_ReturnsZeroWithoutHttpCall()
        {
            // Act
            var result = await _client.GetTotalAmountAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(0m, result.Data);
            await _http.DidNotReceive().GetAsync<decimal?>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetTotalAmountAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<decimal?>
            {
                Success = false,
                Errors = [new ApiError { Message = "Server error" }]
            };
            var mappedResponse = new ApiResponseDto<decimal>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Server error" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<decimal?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetTotalAmountAsync("PROJ-001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetTotalAmountAsync_LargeAmounts_HandlesCorrectly()
        {
            // Arrange
            var parentProject = "PP001";
            var largeAmount = 999999999.99m;
            var apiResponse = new ApiResponse<decimal?> { Success = true, Data = largeAmount };
            var expectedDto = ApiResponseDto<decimal>.SuccessResponse(largeAmount);

            _http.GetAsync<decimal?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetTotalAmountAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(largeAmount, result.Data);
        }

        [Fact]
        public async Task GetTotalAmountAsync_EmptyParentProject_ReturnsZeroWithoutHttpCall()
        {
            // Act
            var result = await _client.GetTotalAmountAsync("");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(0m, result.Data);
            await _http.DidNotReceive().GetAsync<decimal?>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetTotalAmountAsync_WhitespaceParentProject_ReturnsZeroWithoutHttpCall()
        {
            // Act
            var result = await _client.GetTotalAmountAsync("   ");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(0m, result.Data);
            await _http.DidNotReceive().GetAsync<decimal?>(Arg.Any<string>());
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

        #region GetPagedProjectInvoicesByMonthAsync Tests

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithValidMonth_IncludesMonthInUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const int month = 6;
            var invoiceList = new List<ProjectInvoiceRes>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PP001", Month = month, Amount = 100.00m },
                new() { InvoiceCounter = 2, ProjectParent = "PP002", Month = month, Amount = 200.00m }
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
                    new() { InvoiceCounter = 1, ProjectParent = "PP001", Month = month },
                    new() { InvoiceCounter = 2, ProjectParent = "PP002", Month = month }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );

            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Is<string>(url =>
                url.Contains("by-month") && url.Contains($"month={month}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectInvoicesByMonthAsync(query, month);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            Assert.All(result.Data!, invoice => Assert.Equal(month, invoice.Month));
            await _http.Received(1).GetAsync<List<ProjectInvoiceRes>>(
                Arg.Is<string>(url => url.Contains("by-month") && url.Contains($"month={month}")));
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithNullMonth_UsesBaseUrlWithoutMonthParameter()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>> 
            { 
                Success = true, 
                Data = new List<ProjectInvoiceRes>(),
                Pagination = new Pagination()
            };
            var expectedDto = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(
                new List<ProjectInvoiceDto>(), 
                new PaginationDto());

            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Is<string>(url => 
                url.Contains("by-month") && !url.Contains("month=")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectInvoicesByMonthAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<ProjectInvoiceRes>>(
                Arg.Is<string>(url => url.Contains("by-month") && !url.Contains("month=")));
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_UsesCorrectEndpoint()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const int month = 3;
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>> 
            { 
                Success = true, 
                Data = new List<ProjectInvoiceRes>() 
            };
            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(new List<ProjectInvoiceDto>(), new PaginationDto()));

            // Act
            await _client.GetPagedProjectInvoicesByMonthAsync(query, month);

            // Assert
            await _http.Received(1).GetAsync<List<ProjectInvoiceRes>>(
                Arg.Is<string>(url => url.StartsWith(PactApiEndpoints.GetPagedProjectInvoicesByMonth)));
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithQueryParameters_AppendsCorrectly()
        {
            // Arrange
            var query = new QueryParameters<string> 
            { 
                Page = 2, 
                PageSize = 25,
                SortBy = "ProjectParent",
                Descending = true
            };
            const int month = 7;
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>> 
            { 
                Success = true, 
                Data = new List<ProjectInvoiceRes>() 
            };
            var expectedDto = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(
                new List<ProjectInvoiceDto>(), 
                new PaginationDto());

            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectInvoicesByMonthAsync(query, month);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<ProjectInvoiceRes>>(
                Arg.Is<string>(url => 
                    url.Contains("month=7") && 
                    url.Contains("Page=2") && 
                    url.Contains("PageSize=25")));
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
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
            var result = await _client.GetPagedProjectInvoicesByMonthAsync(query, 6);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(6)]
        [InlineData(12)]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithBoundaryMonths_Works(int month)
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>> 
            { 
                Success = true, 
                Data = new List<ProjectInvoiceRes>() 
            };
            var expectedDto = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(
                new List<ProjectInvoiceDto>(), 
                new PaginationDto());

            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectInvoicesByMonthAsync(query, month);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<ProjectInvoiceRes>>(
                Arg.Is<string>(url => url.Contains($"month={month}")));
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>> 
            { 
                Success = true, 
                Data = new List<ProjectInvoiceRes>(),
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            };
            var expectedDto = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(
                new List<ProjectInvoiceDto>(), 
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 });

            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectInvoicesByMonthAsync(query, 5);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            Assert.Equal(0, result.Pagination?.TotalRecords);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithLargePageSize_ProcessesCorrectly()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 1000 };
            var invoiceList = Enumerable.Range(1, 100)
                .Select(i => new ProjectInvoiceRes 
                { 
                    InvoiceCounter = i, 
                    ProjectParent = $"PP{i:000}", 
                    Month = 8,
                    Amount = i * 100m 
                })
                .ToList();
            var apiResponse = new ApiResponse<List<ProjectInvoiceRes>> 
            { 
                Success = true, 
                Data = invoiceList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 1000, TotalRecords = 100 }
            };
            var mappedDtos = invoiceList.Select(r => new ProjectInvoiceDto 
            { 
                InvoiceCounter = r.InvoiceCounter, 
                ProjectParent = r.ProjectParent,
                Month = r.Month 
            }).ToList();
            var expectedDto = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(
                mappedDtos,
                new PaginationDto { PageNumber = 1, PageSize = 1000, TotalRecords = 100 });

            _http.GetAsync<List<ProjectInvoiceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectInvoicesByMonthAsync(query, 8);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(100, result.Data?.Count);
        }

        #endregion

        #region CopyInvoicesAsync Tests

        [Fact]
        public async Task CopyInvoicesAsync_BulkCopy_WithNullInvoiceRecords_SuccessResponse()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 5,
                TargetMonth = 6,
                InvoiceRecords = null
            };
            var copyRes = new CopyInvoicesRes
            {
                Success = true,
                Message = "Successfully copied 10 invoices",
                CopiedCount = 10,
                FailedCount = 0,
                Errors = new List<string>()
            };
            var apiResponse = new ApiResponse<CopyInvoicesRes> { Success = true, Data = copyRes };
            var expectedDto = new ApiResponseDto<CopyInvoicesResultDto>
            {
                Success = true,
                Data = new CopyInvoicesResultDto
                {
                    Success = true,
                    Message = "Successfully copied 10 invoices",
                    CopiedCount = 10,
                    FailedCount = 0,
                    Errors = new List<string>()
                }
            };

            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(
                Arg.Any<string>(),
                Arg.Is<CopyInvoicesReq>(req => req.InvoiceRecords == null))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CopyInvoicesResultDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(10, result.Data?.CopiedCount);
            Assert.Equal(0, result.Data?.FailedCount);
            await _http.Received(1).PostAsync<CopyInvoicesReq, CopyInvoicesRes>(
                Arg.Any<string>(),
                Arg.Is<CopyInvoicesReq>(req => req.InvoiceRecords == null));
        }

        [Fact]
        public async Task CopyInvoicesAsync_BulkCopy_WithEmptyInvoiceRecords_SendsNullToApi()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 3,
                TargetMonth = 4,
                InvoiceRecords = new List<ProjectInvoiceDto>()
            };
            var copyRes = new CopyInvoicesRes { Success = true, CopiedCount = 5 };
            var apiResponse = new ApiResponse<CopyInvoicesRes> { Success = true, Data = copyRes };
            var expectedDto = new ApiResponseDto<CopyInvoicesResultDto>
            {
                Success = true,
                Data = new CopyInvoicesResultDto { Success = true, CopiedCount = 5 }
            };

            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CopyInvoicesResultDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            // Empty list should result in null InvoiceRecords in request
            await _http.Received(1).PostAsync<CopyInvoicesReq, CopyInvoicesRes>(
                Arg.Any<string>(),
                Arg.Is<CopyInvoicesReq>(req => req.InvoiceRecords == null));
        }

        [Fact]
        public async Task CopyInvoicesAsync_SelectiveCopy_WithInvoiceRecords_MapsAndSendsCorrectly()
        {
            // Arrange
            var invoiceDtos = new List<ProjectInvoiceDto>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PP001", Month = 5, Amount = 1000m },
                new() { InvoiceCounter = 2, ProjectParent = "PP002", Month = 5, Amount = 2000m }
            };
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 5,
                TargetMonth = 6,
                InvoiceRecords = invoiceDtos
            };
            var invoiceReqs = new List<ProjectInvoiceReq>
            {
                new() { ProjectParent = "PP001", Amount = 1000m },
                new() { ProjectParent = "PP002", Amount = 2000m }
            };
            var copyRes = new CopyInvoicesRes 
            { 
                Success = true, 
                CopiedCount = 2,
                FailedCount = 0,
                Errors = new List<string>()
            };
            var apiResponse = new ApiResponse<CopyInvoicesRes> { Success = true, Data = copyRes };
            var expectedDto = new ApiResponseDto<CopyInvoicesResultDto>
            {
                Success = true,
                Data = new CopyInvoicesResultDto { Success = true, CopiedCount = 2, FailedCount = 0 }
            };

            _mapper.Map<ProjectInvoiceReq>(invoiceDtos[0]).Returns(invoiceReqs[0]);
            _mapper.Map<ProjectInvoiceReq>(invoiceDtos[1]).Returns(invoiceReqs[1]);
            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CopyInvoicesResultDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.CopiedCount);
            await _http.Received(1).PostAsync<CopyInvoicesReq, CopyInvoicesRes>(
                Arg.Any<string>(),
                Arg.Is<CopyInvoicesReq>(req => 
                    req.InvoiceRecords != null && 
                    req.InvoiceRecords.Count == 2));
        }

        [Fact]
        public async Task CopyInvoicesAsync_UsesCorrectEndpoint()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 7,
                TargetMonth = 8,
                InvoiceRecords = null
            };
            var apiResponse = new ApiResponse<CopyInvoicesRes>
            {
                Success = true,
                Data = new CopyInvoicesRes { Success = true }
            };
            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CopyInvoicesResultDto>>(apiResponse)
                .Returns(new ApiResponseDto<CopyInvoicesResultDto> { Success = true });

            // Act
            await _client.CopyInvoicesAsync(copyDto);

            // Assert - Verify correct endpoint is called
            await _http.Received(1).PostAsync<CopyInvoicesReq, CopyInvoicesRes>(
                Arg.Is<string>(url => url == PactApiEndpoints.CopyProjectInvoices),
                Arg.Any<CopyInvoicesReq>());
        }

        [Fact]
        public async Task CopyInvoicesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto { SourceMonth = 5, TargetMonth = 6 };
            var errors = new List<ApiError> { new() { Message = "Copy failed", Code = "COPY_ERROR" } };
            var apiResponse = new ApiResponse<CopyInvoicesRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<CopyInvoicesResultDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Copy failed", Code = "COPY_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CopyInvoicesResultDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task CopyInvoicesAsync_PartialSuccess_ReturnsCorrectCounts()
        {
            // Arrange
            var invoiceDtos = new List<ProjectInvoiceDto>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PP001" },
                new() { InvoiceCounter = 2, ProjectParent = "PP002" },
                new() { InvoiceCounter = 3, ProjectParent = "PP003" }
            };
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 5,
                TargetMonth = 6,
                InvoiceRecords = invoiceDtos
            };
            var copyRes = new CopyInvoicesRes 
            { 
                Success = false,
                Message = "Copied 2 out of 3 invoices",
                CopiedCount = 2,
                FailedCount = 1,
                Errors = new List<string> { "Failed to copy invoice 3" }
            };
            var apiResponse = new ApiResponse<CopyInvoicesRes> { Success = true, Data = copyRes };
            var expectedDto = new ApiResponseDto<CopyInvoicesResultDto>
            {
                Success = true,
                Data = new CopyInvoicesResultDto 
                { 
                    Success = false,
                    CopiedCount = 2,
                    FailedCount = 1,
                    Errors = new List<string> { "Failed to copy invoice 3" }
                }
            };

            _mapper.Map<ProjectInvoiceReq>(Arg.Any<ProjectInvoiceDto>())
                .Returns(new ProjectInvoiceReq());
            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CopyInvoicesResultDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success); // HTTP call succeeded
            Assert.False(result.Data?.Success); // But operation had failures
            Assert.Equal(2, result.Data?.CopiedCount);
            Assert.Equal(1, result.Data?.FailedCount);
            Assert.Single(result.Data!.Errors);
        }

        [Fact]
        public async Task CopyInvoicesAsync_WithManyInvoices_MapsAllCorrectly()
        {
            // Arrange
            var invoiceDtos = Enumerable.Range(1, 50)
                .Select(i => new ProjectInvoiceDto 
                { 
                    InvoiceCounter = i, 
                    ProjectParent = $"PP{i:000}",
                    Month = 3,
                    Amount = i * 100m 
                })
                .ToList();
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 3,
                TargetMonth = 4,
                InvoiceRecords = invoiceDtos
            };
            var copyRes = new CopyInvoicesRes { Success = true, CopiedCount = 50 };
            var apiResponse = new ApiResponse<CopyInvoicesRes> { Success = true, Data = copyRes };
            var expectedDto = new ApiResponseDto<CopyInvoicesResultDto>
            {
                Success = true,
                Data = new CopyInvoicesResultDto { Success = true, CopiedCount = 50 }
            };

            _mapper.Map<ProjectInvoiceReq>(Arg.Any<ProjectInvoiceDto>())
                .Returns(new ProjectInvoiceReq());
            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CopyInvoicesResultDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(50, result.Data?.CopiedCount);
            await _http.Received(1).PostAsync<CopyInvoicesReq, CopyInvoicesRes>(
                Arg.Any<string>(),
                Arg.Is<CopyInvoicesReq>(req => 
                    req.InvoiceRecords != null && 
                    req.InvoiceRecords.Count == 50));
        }

        [Theory]
        [InlineData(1, 2)]
        [InlineData(5, 6)]
        [InlineData(11, 12)]
        public async Task CopyInvoicesAsync_WithDifferentMonths_SendsCorrectRequestBody(int source, int destination)
        {
            // Arrange
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = source,
                TargetMonth = destination,
                InvoiceRecords = null
            };
            var apiResponse = new ApiResponse<CopyInvoicesRes>
            {
                Success = true,
                Data = new CopyInvoicesRes { Success = true }
            };
            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CopyInvoicesResultDto>>(apiResponse)
                .Returns(new ApiResponseDto<CopyInvoicesResultDto> { Success = true });

            // Act
            await _client.CopyInvoicesAsync(copyDto);

            // Assert - Verify the request body contains correct months
            await _http.Received(1).PostAsync<CopyInvoicesReq, CopyInvoicesRes>(
                Arg.Any<string>(),
                Arg.Is<CopyInvoicesReq>(req =>
                    req.SourceMonth == source &&
                    req.TargetMonth == destination));
        }

        [Fact]
        public async Task CopyInvoicesAsync_SelectiveCopy_CallsMapperForEachInvoice()
        {
            // Arrange
            var invoiceDtos = new List<ProjectInvoiceDto>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PP001" },
                new() { InvoiceCounter = 2, ProjectParent = "PP002" },
                new() { InvoiceCounter = 3, ProjectParent = "PP003" }
            };
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 5,
                TargetMonth = 6,
                InvoiceRecords = invoiceDtos
            };
            var copyRes = new CopyInvoicesRes { Success = true, CopiedCount = 3 };
            var apiResponse = new ApiResponse<CopyInvoicesRes> { Success = true, Data = copyRes };
            var expectedDto = new ApiResponseDto<CopyInvoicesResultDto>
            {
                Success = true,
                Data = new CopyInvoicesResultDto { Success = true, CopiedCount = 3 }
            };

            _mapper.Map<ProjectInvoiceReq>(Arg.Any<ProjectInvoiceDto>())
                .Returns(new ProjectInvoiceReq());
            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CopyInvoicesResultDto>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.CopyInvoicesAsync(copyDto);

            // Assert
            _mapper.Received(3).Map<ProjectInvoiceReq>(Arg.Any<ProjectInvoiceDto>());
        }

        [Fact]
        public async Task CopyInvoicesAsync_SetsSourceAndTargetMonthInRequest()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 7,
                TargetMonth = 8,
                InvoiceRecords = null
            };
            var apiResponse = new ApiResponse<CopyInvoicesRes> 
            { 
                Success = true, 
                Data = new CopyInvoicesRes { Success = true } 
            };
            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CopyInvoicesResultDto>>(apiResponse)
                .Returns(new ApiResponseDto<CopyInvoicesResultDto> { Success = true });

            // Act
            await _client.CopyInvoicesAsync(copyDto);

            // Assert
            await _http.Received(1).PostAsync<CopyInvoicesReq, CopyInvoicesRes>(
                Arg.Any<string>(),
                Arg.Is<CopyInvoicesReq>(req => 
                    req.SourceMonth == 7 && 
                    req.TargetMonth == 8));
        }

        [Fact]
        public async Task CopyInvoicesAsync_WithNullResponseData_HandlesGracefully()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto { SourceMonth = 5, TargetMonth = 6 };
            var apiResponse = new ApiResponse<CopyInvoicesRes> { Success = true, Data = null };
            var expectedDto = new ApiResponseDto<CopyInvoicesResultDto>
            {
                Success = true,
                Data = null
            };

            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CopyInvoicesResultDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task CopyInvoicesAsync_WithSingleInvoice_ProcessesCorrectly()
        {
            // Arrange
            var singleInvoice = new List<ProjectInvoiceDto>
            {
                new() { InvoiceCounter = 42, ProjectParent = "PP-TEST", Amount = 500m }
            };
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 10,
                TargetMonth = 11,
                InvoiceRecords = singleInvoice
            };
            var copyRes = new CopyInvoicesRes { Success = true, CopiedCount = 1 };
            var apiResponse = new ApiResponse<CopyInvoicesRes> { Success = true, Data = copyRes };
            var expectedDto = new ApiResponseDto<CopyInvoicesResultDto>
            {
                Success = true,
                Data = new CopyInvoicesResultDto { Success = true, CopiedCount = 1 }
            };

            _mapper.Map<ProjectInvoiceReq>(Arg.Any<ProjectInvoiceDto>())
                .Returns(new ProjectInvoiceReq());
            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CopyInvoicesResultDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(1, result.Data?.CopiedCount);
            await _http.Received(1).PostAsync<CopyInvoicesReq, CopyInvoicesRes>(
                Arg.Any<string>(),
                Arg.Is<CopyInvoicesReq>(req => 
                    req.InvoiceRecords != null && 
                    req.InvoiceRecords.Count == 1));
        }

        #endregion

        #region CopyInvoicesAsync - Additional Edge Case Tests

        [Fact]
        public async Task CopyInvoicesAsync_WithNullDto_ThrowsNullReferenceException()
        {
            // Act & Assert - Implementation doesn't have null check, so it throws NullReferenceException
            await Assert.ThrowsAsync<NullReferenceException>(() => _client.CopyInvoicesAsync(null!));
        }

        [Fact]
        public async Task CopyInvoicesAsync_WithZeroMonth_SendsToApi()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 0,
                TargetMonth = 1,
                InvoiceRecords = null
            };
            var apiResponse = new ApiResponse<CopyInvoicesRes>
            {
                Success = true,
                Data = new CopyInvoicesRes { Success = true, CopiedCount = 0 }
            };
            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CopyInvoicesResultDto>>(apiResponse)
                .Returns(new ApiResponseDto<CopyInvoicesResultDto> { Success = true });

            // Act
            await _client.CopyInvoicesAsync(copyDto);

            // Assert - Verify the request object has the correct months
            await _http.Received(1).PostAsync<CopyInvoicesReq, CopyInvoicesRes>(
                Arg.Any<string>(),
                Arg.Is<CopyInvoicesReq>(req => req.SourceMonth == 0 && req.TargetMonth == 1));
        }

        [Fact]
        public async Task CopyInvoicesAsync_WithNegativeMonth_SendsToApi()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = -1,
                TargetMonth = 1,
                InvoiceRecords = null
            };
            var apiResponse = new ApiResponse<CopyInvoicesRes>
            {
                Success = false,
                Errors = [new ApiError { Message = "Invalid month" }]
            };
            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CopyInvoicesResultDto>>(apiResponse)
                .Returns(new ApiResponseDto<CopyInvoicesResultDto>
                {
                    Success = false,
                    Errors = [new ApiErrorDto { Message = "Invalid month" }]
                });

            // Act
            var result = await _client.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CopyInvoicesAsync_WithSameSourceAndTargetMonth_ProcessesCorrectly()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 6,
                TargetMonth = 6,
                InvoiceRecords = null
            };
            var apiResponse = new ApiResponse<CopyInvoicesRes>
            {
                Success = true,
                Data = new CopyInvoicesRes { Success = true, CopiedCount = 0 }
            };
            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CopyInvoicesResultDto>>(apiResponse)
                .Returns(new ApiResponseDto<CopyInvoicesResultDto>
                {
                    Success = true,
                    Data = new CopyInvoicesResultDto { Success = true, CopiedCount = 0 }
                });

            // Act
            var result = await _client.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task CopyInvoicesAsync_WithAllFailures_ReturnsCorrectCounts()
        {
            // Arrange
            var invoiceDtos = new List<ProjectInvoiceDto>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PP001" },
                new() { InvoiceCounter = 2, ProjectParent = "PP002" }
            };
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 5,
                TargetMonth = 6,
                InvoiceRecords = invoiceDtos
            };
            var copyRes = new CopyInvoicesRes
            {
                Success = false,
                Message = "All copies failed",
                CopiedCount = 0,
                FailedCount = 2,
                Errors = ["Failed to copy invoice 1", "Failed to copy invoice 2"]
            };
            var apiResponse = new ApiResponse<CopyInvoicesRes> { Success = true, Data = copyRes };
            var expectedDto = new ApiResponseDto<CopyInvoicesResultDto>
            {
                Success = true,
                Data = new CopyInvoicesResultDto
                {
                    Success = false,
                    CopiedCount = 0,
                    FailedCount = 2,
                    Errors = ["Failed to copy invoice 1", "Failed to copy invoice 2"]
                }
            };

            _mapper.Map<ProjectInvoiceReq>(Arg.Any<ProjectInvoiceDto>())
                .Returns(new ProjectInvoiceReq());
            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CopyInvoicesResultDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data?.Success);
            Assert.Equal(0, result.Data?.CopiedCount);
            Assert.Equal(2, result.Data?.FailedCount);
            Assert.Equal(2, result.Data?.Errors.Count);
        }

        [Fact]
        public async Task CopyInvoicesAsync_WithHttpException_PropagatesException()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto { SourceMonth = 5, TargetMonth = 6 };
            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns<ApiResponse<CopyInvoicesRes>>(_ => throw new HttpRequestException("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _client.CopyInvoicesAsync(copyDto));
        }

        [Fact]
        public async Task CopyInvoicesAsync_WithInvoiceRecordsContainingNullValues_HandlesGracefully()
        {
            // Arrange
            var invoiceDtos = new List<ProjectInvoiceDto>
            {
                new() { InvoiceCounter = 1, ProjectParent = null, Amount = 1000m },
                new() { InvoiceCounter = 2, ProjectParent = "", Amount = 2000m }
            };
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 5,
                TargetMonth = 6,
                InvoiceRecords = invoiceDtos
            };
            var copyRes = new CopyInvoicesRes { Success = true, CopiedCount = 2 };
            var apiResponse = new ApiResponse<CopyInvoicesRes> { Success = true, Data = copyRes };
            var expectedDto = new ApiResponseDto<CopyInvoicesResultDto>
            {
                Success = true,
                Data = new CopyInvoicesResultDto { Success = true, CopiedCount = 2 }
            };

            _mapper.Map<ProjectInvoiceReq>(Arg.Any<ProjectInvoiceDto>())
                .Returns(new ProjectInvoiceReq());
            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CopyInvoicesResultDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.CopiedCount);
        }

        [Fact]
        public async Task CopyInvoicesAsync_WithInvalidMonthsGreaterThan12_SendsToApi()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 13,
                TargetMonth = 14,
                InvoiceRecords = null
            };
            var apiResponse = new ApiResponse<CopyInvoicesRes>
            {
                Success = false,
                Errors = [new ApiError { Message = "Month must be between 1 and 12" }]
            };
            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CopyInvoicesResultDto>>(apiResponse)
                .Returns(new ApiResponseDto<CopyInvoicesResultDto>
                {
                    Success = false,
                    Errors = [new ApiErrorDto { Message = "Month must be between 1 and 12" }]
                });

            // Act
            var result = await _client.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.False(result.Success);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(25)]
        [InlineData(100)]
        public async Task CopyInvoicesAsync_WithVariousInvoiceCounts_ProcessesCorrectly(int count)
        {
            // Arrange
            var invoiceDtos = Enumerable.Range(1, count)
                .Select(i => new ProjectInvoiceDto
                {
                    InvoiceCounter = i,
                    ProjectParent = $"PP{i:000}",
                    Amount = i * 100m
                })
                .ToList();
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 3,
                TargetMonth = 4,
                InvoiceRecords = invoiceDtos
            };
            var copyRes = new CopyInvoicesRes { Success = true, CopiedCount = count };
            var apiResponse = new ApiResponse<CopyInvoicesRes> { Success = true, Data = copyRes };
            var expectedDto = new ApiResponseDto<CopyInvoicesResultDto>
            {
                Success = true,
                Data = new CopyInvoicesResultDto { Success = true, CopiedCount = count }
            };

            _mapper.Map<ProjectInvoiceReq>(Arg.Any<ProjectInvoiceDto>())
                .Returns(new ProjectInvoiceReq());
            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CopyInvoicesResultDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(count, result.Data?.CopiedCount);
        }

        [Fact]
        public async Task CopyInvoicesAsync_WithApiReturningMultipleErrors_ReturnsAllErrors()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto { SourceMonth = 5, TargetMonth = 6 };
            var errors = new List<ApiError>
            {
                new() { Message = "Error 1", Code = "ERR_001" },
                new() { Message = "Error 2", Code = "ERR_002" },
                new() { Message = "Error 3", Code = "ERR_003" }
            };
            var apiResponse = new ApiResponse<CopyInvoicesRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<CopyInvoicesResultDto>
            {
                Success = false,
                Errors = errors.Select(e => new ApiErrorDto { Message = e.Message, Code = e.Code }).ToList(),
                Meta = new ApiMetaDto()
            };

            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CopyInvoicesResultDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(3, result.Errors?.Count);
        }

        [Fact]
        public async Task CopyInvoicesAsync_WithEmptyErrorsInSuccessResponse_HandlesCorrectly()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 5,
                TargetMonth = 6,
                InvoiceRecords = null
            };
            var copyRes = new CopyInvoicesRes
            {
                Success = true,
                Message = "Successfully copied",
                CopiedCount = 5,
                FailedCount = 0,
                Errors = new List<string>()
            };
            var apiResponse = new ApiResponse<CopyInvoicesRes> { Success = true, Data = copyRes };
            var expectedDto = new ApiResponseDto<CopyInvoicesResultDto>
            {
                Success = true,
                Data = new CopyInvoicesResultDto
                {
                    Success = true,
                    CopiedCount = 5,
                    Errors = new List<string>()
                }
            };

            _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(Arg.Any<string>(), Arg.Any<CopyInvoicesReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CopyInvoicesResultDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CopyInvoicesAsync(copyDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!.Errors);
        }

        #endregion

        #region GetMonthlyInvoicesSummaryAsync Tests

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_SuccessResponse_ReturnsMappedPivotDto()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pivotRes = new MonthlyInvoicesPivotRes
            {
                Months = [1, 2],
                Rows = [],
                Pagination = new Pagination()
            };
            var apiResponse = new ApiResponse<MonthlyInvoicesPivotRes> { Success = true, Data = pivotRes };
            var expectedDto = new ApiResponseDto<MonthlyInvoicesPivotDto>
            {
                Success = true,
                Data = new MonthlyInvoicesPivotDto { Months = [1, 2] }
            };

            _http.GetAsync<MonthlyInvoicesPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyInvoicesPivotDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(expectedDto.Data, result.Data);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_FailureResponse_ReturnsFailure()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var apiResponse = new ApiResponse<MonthlyInvoicesPivotRes>
            {
                Success = false,
                Errors = [new ApiError { Message = "Server error" }]
            };
            var mappedResponse = new ApiResponseDto<MonthlyInvoicesPivotDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Server error" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<MonthlyInvoicesPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyInvoicesPivotDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_UsesCorrectEndpoint()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var apiResponse = new ApiResponse<MonthlyInvoicesPivotRes> { Success = true, Data = new MonthlyInvoicesPivotRes() };
            _http.GetAsync<MonthlyInvoicesPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyInvoicesPivotDto>>(apiResponse)
                .Returns(new ApiResponseDto<MonthlyInvoicesPivotDto> { Success = true });

            // Act
            await _client.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            await _http.Received(1).GetAsync<MonthlyInvoicesPivotRes>(
                Arg.Is<string>(url => url.StartsWith(PactApiEndpoints.GetMonthlyInvoicesSummary)));
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WithComplexData_ReturnsMappedData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 50 };
            var pivotRes = new MonthlyInvoicesPivotRes
            {
                Months = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
                Rows = [],
                Pagination = new Pagination { PageNumber = 1, PageSize = 50, TotalRecords = 100 }
            };
            var apiResponse = new ApiResponse<MonthlyInvoicesPivotRes> { Success = true, Data = pivotRes };
            var expectedDto = new ApiResponseDto<MonthlyInvoicesPivotDto>
            {
                Success = true,
                Data = new MonthlyInvoicesPivotDto
                {
                    Months = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]
                },
                Pagination = new PaginationDto { PageNumber = 1, PageSize = 50, TotalRecords = 100 }
            };

            _http.GetAsync<MonthlyInvoicesPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyInvoicesPivotDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(12, result.Data?.Months.Count);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WithNullData_HandlesGracefully()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var apiResponse = new ApiResponse<MonthlyInvoicesPivotRes> { Success = true, Data = null };
            var expectedDto = new ApiResponseDto<MonthlyInvoicesPivotDto>
            {
                Success = true,
                Data = null
            };

            _http.GetAsync<MonthlyInvoicesPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyInvoicesPivotDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WithEmptyMonthsList_ReturnsEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var pivotRes = new MonthlyInvoicesPivotRes
            {
                Months = [],
                Rows = [],
                Pagination = new Pagination()
            };
            var apiResponse = new ApiResponse<MonthlyInvoicesPivotRes> { Success = true, Data = pivotRes };
            var expectedDto = new ApiResponseDto<MonthlyInvoicesPivotDto>
            {
                Success = true,
                Data = new MonthlyInvoicesPivotDto { Months = [] }
            };

            _http.GetAsync<MonthlyInvoicesPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyInvoicesPivotDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!.Months);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WithQueryParameters_AppendsCorrectly()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 2,
                PageSize = 25,
                SortBy = "ProjectParent",
                Descending = true
            };
            var apiResponse = new ApiResponse<MonthlyInvoicesPivotRes>
            {
                Success = true,
                Data = new MonthlyInvoicesPivotRes()
            };
            _http.GetAsync<MonthlyInvoicesPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyInvoicesPivotDto>>(apiResponse)
                .Returns(new ApiResponseDto<MonthlyInvoicesPivotDto> { Success = true });

            // Act
            await _client.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            await _http.Received(1).GetAsync<MonthlyInvoicesPivotRes>(
                Arg.Is<string>(url =>
                    url.Contains("Page=2") &&
                    url.Contains("PageSize=25")));
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WithMultipleErrors_ReturnsAllErrors()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var errors = new List<ApiError>
            {
                new() { Message = "Error 1", Code = "ERR_001" },
                new() { Message = "Error 2", Code = "ERR_002" }
            };
            var apiResponse = new ApiResponse<MonthlyInvoicesPivotRes>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<MonthlyInvoicesPivotDto>
            {
                Success = false,
                Errors = errors.Select(e => new ApiErrorDto { Message = e.Message, Code = e.Code }).ToList(),
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<MonthlyInvoicesPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyInvoicesPivotDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(2, result.Errors?.Count);
        }

        #endregion
    }
}
