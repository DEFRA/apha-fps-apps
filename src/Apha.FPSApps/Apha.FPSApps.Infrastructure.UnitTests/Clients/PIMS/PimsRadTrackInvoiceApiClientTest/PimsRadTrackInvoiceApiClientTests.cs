using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsRadTrackInvoiceApiClientTest
{
    public class PimsRadTrackInvoiceApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsRadTrackInvoiceApiClient _client;

        public PimsRadTrackInvoiceApiClientTests()
        {
            _http   = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsRadTrackInvoiceApiClient(_http, _mapper);
        }

        private static List<ApiError> OneApiError(string message = "API error", string code = "ERR")
            => [new ApiError { Message = message, Code = code }];

        private static List<ApiErrorDto> OneApiErrorDto(string message = "API error", string code = "ERR")
            => [new ApiErrorDto { Message = message, Code = code }];

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_InitializesClient()
        {
            var client = new PimsRadTrackInvoiceApiClient(_http, _mapper);
            Assert.NotNull(client);
        }

        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WithSuccessResponse_ReturnsMappedInvoiceList()
        {
            // Arrange
            var query      = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList    = new List<RadTrackInvoiceRes> { new() { InvoiceCounter = 1, Project = "PP001" } };
            var apiResponse = new ApiResponse<List<RadTrackInvoiceRes>> { Success = true, Data = resList };
            var mappedDto  = ApiResponseDto<List<RadTrackInvoiceDto>>.SuccessResponse(
                [new RadTrackInvoiceDto { InvoiceCounter = 1, Project = "PP001" }]);

            _http.GetAsync<List<RadTrackInvoiceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<RadTrackInvoiceDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal("PP001", result.Data[0].Project);
            await _http.Received(1).GetAsync<List<RadTrackInvoiceRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<RadTrackInvoiceDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllAsync_WithFilters_AppendsFiltersToUrl()
        {
            // Arrange
            var query       = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string project  = "PP001";
            const string contract = "C001";
            const int    year     = 2024;
            const string program  = "PROG1";
            var apiResponse = new ApiResponse<List<RadTrackInvoiceRes>> { Success = true, Data = [] };
            var mappedDto   = ApiResponseDto<List<RadTrackInvoiceDto>>.SuccessResponse([]);

            _http.GetAsync<List<RadTrackInvoiceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<RadTrackInvoiceDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetAllAsync(query, project, contract, year, program);

            // Assert
            await _http.Received(1).GetAsync<List<RadTrackInvoiceRes>>(
                Arg.Is<string>(u =>
                    u.Contains("filter.project=PP001") &&
                    u.Contains("filter.contract=C001") &&
                    u.Contains("filter.year=2024") &&
                    u.Contains("filter.program=PROG1")));
        }

        [Fact]
        public async Task GetAllAsync_WithNoFilters_UsesBaseEndpoint()
        {
            // Arrange
            var query       = new QueryParameters<string> { Page = 1, PageSize = 5 };
            var apiResponse = new ApiResponse<List<RadTrackInvoiceRes>> { Success = true, Data = [] };
            var mappedDto   = ApiResponseDto<List<RadTrackInvoiceDto>>.SuccessResponse([]);

            _http.GetAsync<List<RadTrackInvoiceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<RadTrackInvoiceDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetAllAsync(query);

            // Assert
            await _http.Received(1).GetAsync<List<RadTrackInvoiceRes>>(
                Arg.Is<string>(u => u.Contains(PimsApiEndpoints.GetAllRadTrackInvoices)));
        }

        [Fact]
        public async Task GetAllAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query       = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<RadTrackInvoiceRes>>
            {
                Success = false,
                Errors  = OneApiError("Not found", "NOT_FOUND")
            };
            var mappedDto = new ApiResponseDto<List<RadTrackInvoiceDto>>
            {
                Success = false,
                Errors  = OneApiErrorDto("Not found", "NOT_FOUND"),
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<RadTrackInvoiceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<RadTrackInvoiceDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("NOT_FOUND", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetAllAsync_WhenHttpExecutorThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<RadTrackInvoiceRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.GetAllAsync(query));
        }

        [Fact]
        public async Task GetAllAsync_WhenMapperThrowsException_PropagatesException()
        {
            // Arrange
            var query       = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<RadTrackInvoiceRes>> { Success = true, Data = [] };

            _http.GetAsync<List<RadTrackInvoiceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<RadTrackInvoiceDto>>>(apiResponse)
                .Throws(new AutoMapperMappingException("Mapping failed"));

            // Act & Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _client.GetAllAsync(query));
        }

        #endregion

        #region GetTotalsAsync Tests

        [Fact]
        public async Task GetTotalsAsync_WithSuccessResponse_ReturnsMappedTotals()
        {
            // Arrange
            var totalsData  = new RadTrackInvoiceTotalsDto { TotalPlannedAmount = 10000, TotalDueAmount = 8000, TotalActualAmount = 7500 };
            var apiResponse = new ApiResponse<RadTrackInvoiceTotalsDto> { Success = true, Data = totalsData };
            var mappedDto   = ApiResponseDto<RadTrackInvoiceTotalsDto>.SuccessResponse(totalsData);

            _http.GetAsync<RadTrackInvoiceTotalsDto>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackInvoiceTotalsDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetTotalsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(10000, result.Data.TotalPlannedAmount);
            Assert.Equal(8000,  result.Data.TotalDueAmount);
            Assert.Equal(7500,  result.Data.TotalActualAmount);
            await _http.Received(1).GetAsync<RadTrackInvoiceTotalsDto>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<RadTrackInvoiceTotalsDto>>(apiResponse);
        }

        [Fact]
        public async Task GetTotalsAsync_WithFilters_AppendsFiltersToUrl()
        {
            // Arrange
            const string project  = "PP001";
            const string contract = "C001";
            const int    year     = 2024;
            const string program  = "PROG1";
            var apiResponse = new ApiResponse<RadTrackInvoiceTotalsDto> { Success = true, Data = new RadTrackInvoiceTotalsDto() };
            var mappedDto   = ApiResponseDto<RadTrackInvoiceTotalsDto>.SuccessResponse(new RadTrackInvoiceTotalsDto());

            _http.GetAsync<RadTrackInvoiceTotalsDto>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackInvoiceTotalsDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetTotalsAsync(project, contract, year, program);

            // Assert
            await _http.Received(1).GetAsync<RadTrackInvoiceTotalsDto>(
                Arg.Is<string>(u =>
                    u.Contains("project=PP001") &&
                    u.Contains("contract=C001") &&
                    u.Contains("year=2024") &&
                    u.Contains("program=PROG1")));
        }

        [Fact]
        public async Task GetTotalsAsync_WithNoFilters_UsesBaseEndpoint()
        {
            // Arrange
            var apiResponse = new ApiResponse<RadTrackInvoiceTotalsDto> { Success = true, Data = new RadTrackInvoiceTotalsDto() };
            var mappedDto   = ApiResponseDto<RadTrackInvoiceTotalsDto>.SuccessResponse(new RadTrackInvoiceTotalsDto());

            _http.GetAsync<RadTrackInvoiceTotalsDto>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackInvoiceTotalsDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetTotalsAsync();

            // Assert
            await _http.Received(1).GetAsync<RadTrackInvoiceTotalsDto>(
                Arg.Is<string>(u => u == PimsApiEndpoints.GetRadTrackInvoiceTotals));
        }

        [Fact]
        public async Task GetTotalsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<RadTrackInvoiceTotalsDto>
            {
                Success = false,
                Errors  = OneApiError("Totals not found", "NOT_FOUND")
            };
            var mappedDto = new ApiResponseDto<RadTrackInvoiceTotalsDto>
            {
                Success = false,
                Errors  = OneApiErrorDto("Totals not found", "NOT_FOUND"),
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<RadTrackInvoiceTotalsDto>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackInvoiceTotalsDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetTotalsAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            Assert.Equal("NOT_FOUND", result.Errors![0].Code);
        }

        [Fact]
        public async Task GetTotalsAsync_WhenHttpExecutorThrowsException_PropagatesException()
        {
            // Arrange
            _http.GetAsync<RadTrackInvoiceTotalsDto>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.GetTotalsAsync());
        }

        [Fact]
        public async Task GetTotalsAsync_WhenMapperThrowsException_PropagatesException()
        {
            // Arrange
            var apiResponse = new ApiResponse<RadTrackInvoiceTotalsDto> { Success = true, Data = new RadTrackInvoiceTotalsDto() };

            _http.GetAsync<RadTrackInvoiceTotalsDto>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackInvoiceTotalsDto>>(apiResponse)
                .Throws(new AutoMapperMappingException("Mapping failed"));

            // Act & Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _client.GetTotalsAsync());
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithSuccessResponse_ReturnsMappedInvoice()
        {
            // Arrange
            const int id    = 1;
            var resData     = new RadTrackInvoiceRes { InvoiceCounter = id, Project = "PP001", InvoiceRef = "INV-001" };
            var apiResponse = new ApiResponse<RadTrackInvoiceRes> { Success = true, Data = resData };
            var mappedDto   = ApiResponseDto<RadTrackInvoiceDto>.SuccessResponse(
                new RadTrackInvoiceDto { InvoiceCounter = id, Project = "PP001", InvoiceRef = "INV-001" });

            _http.GetAsync<RadTrackInvoiceRes>(string.Format(PimsApiEndpoints.GetRadTrackInvoiceById, id)).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(id,        result.Data.InvoiceCounter);
            Assert.Equal("PP001",   result.Data.Project);
            Assert.Equal("INV-001", result.Data.InvoiceRef);
            await _http.Received(1).GetAsync<RadTrackInvoiceRes>(string.Format(PimsApiEndpoints.GetRadTrackInvoiceById, id));
            _mapper.Received(1).Map<ApiResponseDto<RadTrackInvoiceDto>>(apiResponse);
        }

        [Fact]
        public async Task GetByIdAsync_EnsuresCorrectApiEndpoint_UsesFormattedUrl()
        {
            // Arrange
            const int id    = 42;
            var apiResponse = new ApiResponse<RadTrackInvoiceRes> { Success = true, Data = new RadTrackInvoiceRes { InvoiceCounter = id } };
            var mappedDto   = ApiResponseDto<RadTrackInvoiceDto>.SuccessResponse(new RadTrackInvoiceDto { InvoiceCounter = id });

            _http.GetAsync<RadTrackInvoiceRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetByIdAsync(id);

            // Assert
            await _http.Received(1).GetAsync<RadTrackInvoiceRes>(
                Arg.Is<string>(u => u == string.Format(PimsApiEndpoints.GetRadTrackInvoiceById, id)));
        }

        [Fact]
        public async Task GetByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const int id    = 99;
            var apiResponse = new ApiResponse<RadTrackInvoiceRes>
            {
                Success = false,
                Errors  = OneApiError("Invoice not found", "NOT_FOUND")
            };
            var mappedDto = new ApiResponseDto<RadTrackInvoiceDto>
            {
                Success = false,
                Errors  = OneApiErrorDto("Invoice not found", "NOT_FOUND"),
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<RadTrackInvoiceRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("NOT_FOUND", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetByIdAsync_WhenHttpExecutorThrowsException_PropagatesException()
        {
            // Arrange
            const int id = 1;
            _http.GetAsync<RadTrackInvoiceRes>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.GetByIdAsync(id));
        }

        [Fact]
        public async Task GetByIdAsync_WhenMapperThrowsException_PropagatesException()
        {
            // Arrange
            const int id    = 1;
            var apiResponse = new ApiResponse<RadTrackInvoiceRes> { Success = true, Data = new RadTrackInvoiceRes { InvoiceCounter = id } };

            _http.GetAsync<RadTrackInvoiceRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(apiResponse)
                .Throws(new AutoMapperMappingException("Mapping failed"));

            // Act & Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _client.GetByIdAsync(id));
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithSuccessResponse_ReturnsMappedInvoice()
        {
            // Arrange
            var dto     = new RadTrackInvoiceDto { Project = "PP001", InvoiceRef = "INV-001", PlannedAmount = 5000 };
            var request = new RadTrackInvoiceReq { Project = "PP001", InvoiceRef = "INV-001", PlannedAmount = 5000 };
            var resData  = new RadTrackInvoiceRes { InvoiceCounter = 1, Project = "PP001", InvoiceRef = "INV-001" };
            var apiResponse = new ApiResponse<RadTrackInvoiceRes> { Success = true, Data = resData };
            var mappedDto   = ApiResponseDto<RadTrackInvoiceDto>.SuccessResponse(
                new RadTrackInvoiceDto { InvoiceCounter = 1, Project = "PP001", InvoiceRef = "INV-001" });

            _mapper.Map<RadTrackInvoiceReq>(dto).Returns(request);
            _http.PostAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>(PimsApiEndpoints.CreateRadTrackInvoice, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1,         result.Data.InvoiceCounter);
            Assert.Equal("PP001",   result.Data.Project);
            Assert.Equal("INV-001", result.Data.InvoiceRef);
            _mapper.Received(1).Map<RadTrackInvoiceReq>(dto);
            await _http.Received(1).PostAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>(PimsApiEndpoints.CreateRadTrackInvoice, request);
            _mapper.Received(1).Map<ApiResponseDto<RadTrackInvoiceDto>>(apiResponse);
        }

        [Fact]
        public async Task CreateAsync_EnsuresCorrectApiEndpoint_CallsPostWithCorrectUrl()
        {
            // Arrange
            var dto     = new RadTrackInvoiceDto { Project = "PP001" };
            var request = new RadTrackInvoiceReq { Project = "PP001" };
            var apiResponse = new ApiResponse<RadTrackInvoiceRes> { Success = true, Data = new RadTrackInvoiceRes() };
            var mappedDto   = ApiResponseDto<RadTrackInvoiceDto>.SuccessResponse(new RadTrackInvoiceDto());

            _mapper.Map<RadTrackInvoiceReq>(dto).Returns(request);
            _http.PostAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>(PimsApiEndpoints.CreateRadTrackInvoice, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.CreateAsync(dto);

            // Assert
            await _http.Received(1).PostAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>(
                Arg.Is<string>(s => s == PimsApiEndpoints.CreateRadTrackInvoice),
                Arg.Any<RadTrackInvoiceReq>());
        }

        [Fact]
        public async Task CreateAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto     = new RadTrackInvoiceDto { Project = "PP001" };
            var request = new RadTrackInvoiceReq { Project = "PP001" };
            var apiResponse = new ApiResponse<RadTrackInvoiceRes>
            {
                Success = false,
                Errors  = OneApiError("Validation error", "VALIDATION_ERROR")
            };
            var mappedDto = new ApiResponseDto<RadTrackInvoiceDto>
            {
                Success = false,
                Errors  = OneApiErrorDto("Validation error", "VALIDATION_ERROR"),
                Meta    = new ApiMetaDto()
            };

            _mapper.Map<RadTrackInvoiceReq>(dto).Returns(request);
            _http.PostAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>(PimsApiEndpoints.CreateRadTrackInvoice, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("VALIDATION_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task CreateAsync_WhenHttpExecutorThrowsException_PropagatesException()
        {
            // Arrange
            var dto     = new RadTrackInvoiceDto { Project = "PP001" };
            var request = new RadTrackInvoiceReq { Project = "PP001" };

            _mapper.Map<RadTrackInvoiceReq>(dto).Returns(request);
            _http.PostAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>(PimsApiEndpoints.CreateRadTrackInvoice, request)
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_WhenMapperThrowsExceptionOnRequestMapping_PropagatesException()
        {
            // Arrange
            var dto = new RadTrackInvoiceDto { Project = "PP001" };
            _mapper.Map<RadTrackInvoiceReq>(dto).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act & Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _client.CreateAsync(dto));
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithSuccessResponse_ReturnsMappedInvoice()
        {
            // Arrange
            const int id = 1;
            var dto      = new RadTrackInvoiceDto { InvoiceCounter = id, Project = "PP001", InvoiceRef = "INV-UPDATED" };
            var request  = new RadTrackInvoiceReq { Project = "PP001", InvoiceRef = "INV-UPDATED" };
            var resData   = new RadTrackInvoiceRes { InvoiceCounter = id, Project = "PP001", InvoiceRef = "INV-UPDATED" };
            var apiResponse = new ApiResponse<RadTrackInvoiceRes> { Success = true, Data = resData };
            var mappedDto   = ApiResponseDto<RadTrackInvoiceDto>.SuccessResponse(
                new RadTrackInvoiceDto { InvoiceCounter = id, Project = "PP001", InvoiceRef = "INV-UPDATED" });

            _mapper.Map<RadTrackInvoiceReq>(dto).Returns(request);
            _http.PutAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>(
                string.Format(PimsApiEndpoints.UpdateRadTrackInvoice, id), request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateAsync(id, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(id,            result.Data.InvoiceCounter);
            Assert.Equal("INV-UPDATED", result.Data.InvoiceRef);
            _mapper.Received(1).Map<RadTrackInvoiceReq>(dto);
            await _http.Received(1).PutAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>(
                string.Format(PimsApiEndpoints.UpdateRadTrackInvoice, id), request);
            _mapper.Received(1).Map<ApiResponseDto<RadTrackInvoiceDto>>(apiResponse);
        }

        [Fact]
        public async Task UpdateAsync_EnsuresCorrectApiEndpoint_CallsPutWithFormattedUrl()
        {
            // Arrange
            const int id = 5;
            var dto     = new RadTrackInvoiceDto { InvoiceCounter = id };
            var request = new RadTrackInvoiceReq();
            var apiResponse = new ApiResponse<RadTrackInvoiceRes> { Success = true, Data = new RadTrackInvoiceRes() };
            var mappedDto   = ApiResponseDto<RadTrackInvoiceDto>.SuccessResponse(new RadTrackInvoiceDto());

            _mapper.Map<RadTrackInvoiceReq>(dto).Returns(request);
            _http.PutAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>(Arg.Any<string>(), request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.UpdateAsync(id, dto);

            // Assert
            await _http.Received(1).PutAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>(
                Arg.Is<string>(s => s == string.Format(PimsApiEndpoints.UpdateRadTrackInvoice, id)),
                Arg.Any<RadTrackInvoiceReq>());
        }

        [Fact]
        public async Task UpdateAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const int id = 99;
            var dto      = new RadTrackInvoiceDto { InvoiceCounter = id };
            var request  = new RadTrackInvoiceReq();
            var apiResponse = new ApiResponse<RadTrackInvoiceRes>
            {
                Success = false,
                Errors  = OneApiError("Invoice not found", "NOT_FOUND")
            };
            var mappedDto = new ApiResponseDto<RadTrackInvoiceDto>
            {
                Success = false,
                Errors  = OneApiErrorDto("Invoice not found", "NOT_FOUND"),
                Meta    = new ApiMetaDto()
            };

            _mapper.Map<RadTrackInvoiceReq>(dto).Returns(request);
            _http.PutAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>(Arg.Any<string>(), request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateAsync(id, dto);

            // Assert
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            Assert.Equal("NOT_FOUND", result.Errors![0].Code);
        }

        [Fact]
        public async Task UpdateAsync_WhenHttpExecutorThrowsException_PropagatesException()
        {
            // Arrange
            const int id = 1;
            var dto     = new RadTrackInvoiceDto { InvoiceCounter = id };
            var request = new RadTrackInvoiceReq();

            _mapper.Map<RadTrackInvoiceReq>(dto).Returns(request);
            _http.PutAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>(Arg.Any<string>(), request)
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.UpdateAsync(id, dto));
        }

        [Fact]
        public async Task UpdateAsync_WhenMapperThrowsExceptionOnRequestMapping_PropagatesException()
        {
            // Arrange
            const int id = 1;
            var dto = new RadTrackInvoiceDto { InvoiceCounter = id };
            _mapper.Map<RadTrackInvoiceReq>(dto).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act & Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _client.UpdateAsync(id, dto));
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithSuccessResponse_ReturnsSuccess()
        {
            // Arrange
            const int id    = 1;
            var apiResponse = new ApiResponse<object> { Success = true, Data = null };
            var mappedDto   = ApiResponseDto<object>.SuccessResponse(new object());

            _http.DeleteAsync<object>(string.Format(PimsApiEndpoints.DeleteRadTrackInvoice, id)).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.DeleteAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).DeleteAsync<object>(string.Format(PimsApiEndpoints.DeleteRadTrackInvoice, id));
            _mapper.Received(1).Map<ApiResponseDto<object>>(apiResponse);
        }

        [Fact]
        public async Task DeleteAsync_EnsuresCorrectApiEndpoint_UsesFormattedUrl()
        {
            // Arrange
            const int id    = 7;
            var apiResponse = new ApiResponse<object> { Success = true };
            var mappedDto   = ApiResponseDto<object>.SuccessResponse(new object());

            _http.DeleteAsync<object>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.DeleteAsync(id);

            // Assert
            await _http.Received(1).DeleteAsync<object>(
                Arg.Is<string>(s => s == string.Format(PimsApiEndpoints.DeleteRadTrackInvoice, id)));
        }

        [Fact]
        public async Task DeleteAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const int id    = 99;
            var apiResponse = new ApiResponse<object>
            {
                Success = false,
                Errors  = OneApiError("Invoice not found", "NOT_FOUND")
            };
            var mappedDto = new ApiResponseDto<object>
            {
                Success = false,
                Errors  = OneApiErrorDto("Invoice not found", "NOT_FOUND"),
                Meta    = new ApiMetaDto()
            };

            _http.DeleteAsync<object>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.DeleteAsync(id);

            // Assert
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            Assert.Equal("NOT_FOUND", result.Errors![0].Code);
        }

        [Fact]
        public async Task DeleteAsync_WhenHttpExecutorThrowsException_PropagatesException()
        {
            // Arrange
            const int id = 1;
            _http.DeleteAsync<object>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.DeleteAsync(id));
        }

        [Fact]
        public async Task DeleteAsync_WhenMapperThrowsException_PropagatesException()
        {
            // Arrange
            const int id    = 1;
            var apiResponse = new ApiResponse<object> { Success = true };

            _http.DeleteAsync<object>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse)
                .Throws(new AutoMapperMappingException("Mapping failed"));

            // Act & Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _client.DeleteAsync(id));
        }

        #endregion

        #region GetProjectsAsync Tests

        [Fact]
        public async Task GetProjectsAsync_WithSuccessResponseAndData_ReturnsMappedProjectList()
        {
            // Arrange
            var projectList = new List<string> { "PP001", "PP002" };
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = projectList };
            var mappedDto   = ApiResponseDto<List<string>>.SuccessResponse(new List<string> { "PP001", "PP002" });

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetRadTrackInvoiceProjects).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetProjectsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            Assert.Contains("PP001", result.Data);
            await _http.Received(1).GetAsync<List<string>>(PimsApiEndpoints.GetRadTrackInvoiceProjects);
            _mapper.Received(1).Map<ApiResponseDto<List<string>>>(apiResponse);
        }

        [Fact]
        public async Task GetProjectsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<string>>
            {
                Success = false,
                Errors  = OneApiError("Projects not found", "NOT_FOUND")
            };
            var mappedDto = new ApiResponseDto<List<string>>
            {
                Success = false,
                Errors  = OneApiErrorDto("Projects not found", "NOT_FOUND"),
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetRadTrackInvoiceProjects).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetProjectsAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Projects not found", result.Errors![0].Message);
        }

        [Fact]
        public async Task GetProjectsAsync_WhenHttpExecutorThrowsException_PropagatesException()
        {
            // Arrange
            _http.GetAsync<List<string>>(PimsApiEndpoints.GetRadTrackInvoiceProjects)
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.GetProjectsAsync());
        }

        [Fact]
        public async Task GetProjectsAsync_WhenMapperThrowsException_PropagatesException()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = ["PP001"] };

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetRadTrackInvoiceProjects).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse)
                .Throws(new AutoMapperMappingException("Mapping failed"));

            // Act & Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _client.GetProjectsAsync());
        }

        [Fact]
        public async Task GetProjectsAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = [] };
            var mappedDto   = ApiResponseDto<List<string>>.SuccessResponse([]);

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetRadTrackInvoiceProjects).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetProjectsAsync();

            // Assert
            await _http.Received(1).GetAsync<List<string>>(
                Arg.Is<string>(s => s == PimsApiEndpoints.GetRadTrackInvoiceProjects));
        }

        #endregion

        #region GetYearsAsync Tests

        [Fact]
        public async Task GetYearsAsync_WithSuccessResponseAndData_ReturnsMappedYearList()
        {
            // Arrange
            var yearList    = new List<int> { 2022, 2023, 2024 };
            var apiResponse = new ApiResponse<List<int>> { Success = true, Data = yearList };
            var mappedDto   = ApiResponseDto<List<int>>.SuccessResponse(new List<int> { 2022, 2023, 2024 });

            _http.GetAsync<List<int>>(PimsApiEndpoints.GetRadTrackInvoiceYears).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<int>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetYearsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data!.Count);
            Assert.Contains(2024, result.Data);
            await _http.Received(1).GetAsync<List<int>>(PimsApiEndpoints.GetRadTrackInvoiceYears);
            _mapper.Received(1).Map<ApiResponseDto<List<int>>>(apiResponse);
        }

        [Fact]
        public async Task GetYearsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<int>>
            {
                Success = false,
                Errors  = OneApiError("Years not found", "NOT_FOUND")
            };
            var mappedDto = new ApiResponseDto<List<int>>
            {
                Success = false,
                Errors  = OneApiErrorDto("Years not found", "NOT_FOUND"),
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<int>>(PimsApiEndpoints.GetRadTrackInvoiceYears).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<int>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetYearsAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Years not found", result.Errors![0].Message);
        }

        [Fact]
        public async Task GetYearsAsync_WhenHttpExecutorThrowsException_PropagatesException()
        {
            // Arrange
            _http.GetAsync<List<int>>(PimsApiEndpoints.GetRadTrackInvoiceYears)
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.GetYearsAsync());
        }

        [Fact]
        public async Task GetYearsAsync_WhenMapperThrowsException_PropagatesException()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<int>> { Success = true, Data = [2024] };

            _http.GetAsync<List<int>>(PimsApiEndpoints.GetRadTrackInvoiceYears).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<int>>>(apiResponse)
                .Throws(new AutoMapperMappingException("Mapping failed"));

            // Act & Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _client.GetYearsAsync());
        }

        [Fact]
        public async Task GetYearsAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<int>> { Success = true, Data = [] };
            var mappedDto   = ApiResponseDto<List<int>>.SuccessResponse([]);

            _http.GetAsync<List<int>>(PimsApiEndpoints.GetRadTrackInvoiceYears).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<int>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetYearsAsync();

            // Assert
            await _http.Received(1).GetAsync<List<int>>(
                Arg.Is<string>(s => s == PimsApiEndpoints.GetRadTrackInvoiceYears));
        }

        #endregion

        #region GetContractsAsync Tests

        [Fact]
        public async Task GetContractsAsync_WithSuccessResponseAndData_ReturnsMappedContractList()
        {
            // Arrange
            var contractList = new List<string> { "C001", "C002" };
            var apiResponse  = new ApiResponse<List<string>> { Success = true, Data = contractList };
            var mappedDto    = ApiResponseDto<List<string>>.SuccessResponse(new List<string> { "C001", "C002" });

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetRadTrackInvoiceContracts).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetContractsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            Assert.Contains("C001", result.Data);
            await _http.Received(1).GetAsync<List<string>>(PimsApiEndpoints.GetRadTrackInvoiceContracts);
            _mapper.Received(1).Map<ApiResponseDto<List<string>>>(apiResponse);
        }

        [Fact]
        public async Task GetContractsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<string>>
            {
                Success = false,
                Errors  = OneApiError("Contracts not found", "NOT_FOUND")
            };
            var mappedDto = new ApiResponseDto<List<string>>
            {
                Success = false,
                Errors  = OneApiErrorDto("Contracts not found", "NOT_FOUND"),
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetRadTrackInvoiceContracts).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetContractsAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Contracts not found", result.Errors![0].Message);
        }

        [Fact]
        public async Task GetContractsAsync_WhenHttpExecutorThrowsException_PropagatesException()
        {
            // Arrange
            _http.GetAsync<List<string>>(PimsApiEndpoints.GetRadTrackInvoiceContracts)
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.GetContractsAsync());
        }

        [Fact]
        public async Task GetContractsAsync_WhenMapperThrowsException_PropagatesException()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = ["C001"] };

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetRadTrackInvoiceContracts).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse)
                .Throws(new AutoMapperMappingException("Mapping failed"));

            // Act & Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _client.GetContractsAsync());
        }

        [Fact]
        public async Task GetContractsAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = [] };
            var mappedDto   = ApiResponseDto<List<string>>.SuccessResponse([]);

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetRadTrackInvoiceContracts).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetContractsAsync();

            // Assert
            await _http.Received(1).GetAsync<List<string>>(
                Arg.Is<string>(s => s == PimsApiEndpoints.GetRadTrackInvoiceContracts));
        }

        #endregion

        #region GetProgramsAsync Tests

        [Fact]
        public async Task GetProgramsAsync_WithSuccessResponseAndData_ReturnsMappedProgramList()
        {
            // Arrange
            var programList = new List<string> { "PROG1", "PROG2" };
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = programList };
            var mappedDto   = ApiResponseDto<List<string>>.SuccessResponse(new List<string> { "PROG1", "PROG2" });

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetRadTrackInvoicePrograms).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            Assert.Contains("PROG1", result.Data);
            await _http.Received(1).GetAsync<List<string>>(PimsApiEndpoints.GetRadTrackInvoicePrograms);
            _mapper.Received(1).Map<ApiResponseDto<List<string>>>(apiResponse);
        }

        [Fact]
        public async Task GetProgramsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<string>>
            {
                Success = false,
                Errors  = OneApiError("Programs not found", "NOT_FOUND")
            };
            var mappedDto = new ApiResponseDto<List<string>>
            {
                Success = false,
                Errors  = OneApiErrorDto("Programs not found", "NOT_FOUND"),
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetRadTrackInvoicePrograms).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetProgramsAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Programs not found", result.Errors![0].Message);
        }

        [Fact]
        public async Task GetProgramsAsync_WhenHttpExecutorThrowsException_PropagatesException()
        {
            // Arrange
            _http.GetAsync<List<string>>(PimsApiEndpoints.GetRadTrackInvoicePrograms)
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.GetProgramsAsync());
        }

        [Fact]
        public async Task GetProgramsAsync_WhenMapperThrowsException_PropagatesException()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = ["PROG1"] };

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetRadTrackInvoicePrograms).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse)
                .Throws(new AutoMapperMappingException("Mapping failed"));

            // Act & Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _client.GetProgramsAsync());
        }

        [Fact]
        public async Task GetProgramsAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = [] };
            var mappedDto   = ApiResponseDto<List<string>>.SuccessResponse([]);

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetRadTrackInvoicePrograms).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetProgramsAsync();

            // Assert
            await _http.Received(1).GetAsync<List<string>>(
                Arg.Is<string>(s => s == PimsApiEndpoints.GetRadTrackInvoicePrograms));
        }

        #endregion
    }
}
