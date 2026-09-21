using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients;
using NSubstitute;
using MapsterMapper;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsRadTrackProgApiClientTest
{
    public class PimsRadTrackProgApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsRadTrackProgApiClient _client;

        public PimsRadTrackProgApiClientTests()
        {
            _http = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsRadTrackProgApiClient(_http, _mapper);
        }

        #region GetAllRadTrackProgsAsync Tests

        [Fact]
        public async Task GetAllRadTrackProgsAsync_WhenSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var radTrackProgList = new List<RadTrackProgRes>
            {
                new RadTrackProgRes { Program = "PROG001", RadTrackProg = true, PublicationPrefix = "PREFIX1" },
                new RadTrackProgRes { Program = "PROG002", RadTrackProg = false, PublicationPrefix = "PREFIX2" }
            };
            var apiResponse = new ApiResponse<List<RadTrackProgRes>> { Success = true, Data = radTrackProgList };
            var mappedDto = ApiResponseDto<List<RadTrackProgDto>>.SuccessResponse(new List<RadTrackProgDto>
            {
                new RadTrackProgDto { Program = "PROG001", Radtrackprog = true, Publicationprefix = "PREFIX1" },
                new RadTrackProgDto { Program = "PROG002", Radtrackprog = false, Publicationprefix = "PREFIX2" }
            });

            _http.GetAsync<List<RadTrackProgRes>>(PimsApiEndpoints.GetAllRadTrackProgs).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<RadTrackProgDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllRadTrackProgsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _http.Received(1).GetAsync<List<RadTrackProgRes>>(PimsApiEndpoints.GetAllRadTrackProgs);
            _mapper.Received(1).Map<ApiResponseDto<List<RadTrackProgDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllRadTrackProgsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new ApiError { Message = "Error", Code = "ERR" } };
            var apiResponse = new ApiResponse<List<RadTrackProgRes>> { Success = false, Data = null, Errors = errors };
            var failDto = new ApiResponseDto<List<RadTrackProgDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Error", Code = "ERR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<RadTrackProgRes>>(PimsApiEndpoints.GetAllRadTrackProgs).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<RadTrackProgDto>>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.GetAllRadTrackProgsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _http.Received(1).GetAsync<List<RadTrackProgRes>>(PimsApiEndpoints.GetAllRadTrackProgs);
        }

        #endregion

        #region GetPagedRadTrackProgsAsync Tests

        [Fact]
        public async Task GetPagedRadTrackProgsAsync_WhenSuccessResponse_ReturnsPagedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var radTrackProgList = new List<RadTrackProgRes>
            {
                new RadTrackProgRes { Program = "PROG001", RadTrackProg = true, PublicationPrefix = "PREFIX1" }
            };
            var apiResponse = new ApiResponse<List<RadTrackProgRes>>
            {
                Success = true,
                Data = radTrackProgList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var mappedItems = new List<RadTrackProgDto> 
            { 
                new RadTrackProgDto { Program = "PROG001", Radtrackprog = true, Publicationprefix = "PREFIX1" } 
            };

            _http.GetAsync<List<RadTrackProgRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<List<RadTrackProgDto>>(radTrackProgList).Returns(mappedItems);

            // Act
            var result = await _client.GetPagedRadTrackProgsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.data);
            await _http.Received(1).GetAsync<List<RadTrackProgRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetPagedRadTrackProgsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new ApiError { Message = "Error", Code = "ERR" } };
            var apiResponse = new ApiResponse<List<RadTrackProgRes>> { Success = false, Errors = errors };

            _http.GetAsync<List<RadTrackProgRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<List<ApiErrorDto>>(errors).Returns(new List<ApiErrorDto> { new ApiErrorDto { Message = "Error", Code = "ERR" } });
            _mapper.Map<ApiMetaDto>(apiResponse.Meta).Returns(new ApiMetaDto());

            // Act
            var result = await _client.GetPagedRadTrackProgsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _http.Received(1).GetAsync<List<RadTrackProgRes>>(Arg.Any<string>());
        }

        #endregion

        #region GetRadTrackProgByProgramAsync Tests

        [Fact]
        public async Task GetRadTrackProgByProgramAsync_WhenSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var program = "PROG001";
            var url = string.Format(PimsApiEndpoints.GetRadTrackProgByProgram, Uri.EscapeDataString(program));
            var radTrackProgRes = new RadTrackProgRes { Program = program, RadTrackProg = true, PublicationPrefix = "PREFIX1" };
            var apiResponse = new ApiResponse<RadTrackProgRes> { Success = true, Data = radTrackProgRes };
            var mappedDto = ApiResponseDto<RadTrackProgDto>.SuccessResponse(
                new RadTrackProgDto { Program = program, Radtrackprog = true, Publicationprefix = "PREFIX1" });

            _http.GetAsync<RadTrackProgRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackProgDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetRadTrackProgByProgramAsync(program);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(program, result.Data.Program);
            await _http.Received(1).GetAsync<RadTrackProgRes>(url);
        }

        [Fact]
        public async Task GetRadTrackProgByProgramAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var program = "INVALID";
            var url = string.Format(PimsApiEndpoints.GetRadTrackProgByProgram, Uri.EscapeDataString(program));
            var errors = new List<ApiError> { new ApiError { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<RadTrackProgRes> { Success = false, Errors = errors };
            var failDto = new ApiResponseDto<RadTrackProgDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<RadTrackProgRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackProgDto>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.GetRadTrackProgByProgramAsync(program);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _http.Received(1).GetAsync<RadTrackProgRes>(url);
        }

        #endregion

        #region CreateRadTrackProgAsync Tests

        [Fact]
        public async Task CreateRadTrackProgAsync_WhenSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var dto = new RadTrackProgDto { Program = "PROG001", Radtrackprog = true, Publicationprefix = "PREFIX1" };
            var request = new RadTrackProgReq { Program = "PROG001", RadTrackProg = true, PublicationPrefix = "PREFIX1" };
            var response = new RadTrackProgRes { Program = "PROG001", RadTrackProg = true, PublicationPrefix = "PREFIX1" };
            var apiResponse = new ApiResponse<RadTrackProgRes> { Success = true, Data = response };
            var mappedDto = ApiResponseDto<RadTrackProgDto>.SuccessResponse(dto);

            _mapper.Map<RadTrackProgReq>(dto).Returns(request);
            _http.PostAsync<RadTrackProgReq, RadTrackProgRes>(PimsApiEndpoints.CreateRadTrackProg, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackProgDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.CreateRadTrackProgAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            await _http.Received(1).PostAsync<RadTrackProgReq, RadTrackProgRes>(PimsApiEndpoints.CreateRadTrackProg, request);
        }

        [Fact]
        public async Task CreateRadTrackProgAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new RadTrackProgDto { Program = "PROG001" };
            var request = new RadTrackProgReq { Program = "PROG001" };
            var errors = new List<ApiError> { new ApiError { Message = "Duplicate", Code = "DUPLICATE" } };
            var apiResponse = new ApiResponse<RadTrackProgRes> { Success = false, Errors = errors };
            var failDto = new ApiResponseDto<RadTrackProgDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Duplicate", Code = "DUPLICATE" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<RadTrackProgReq>(dto).Returns(request);
            _http.PostAsync<RadTrackProgReq, RadTrackProgRes>(PimsApiEndpoints.CreateRadTrackProg, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackProgDto>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.CreateRadTrackProgAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _http.Received(1).PostAsync<RadTrackProgReq, RadTrackProgRes>(PimsApiEndpoints.CreateRadTrackProg, request);
        }

        #endregion

        #region UpdateRadTrackProgAsync Tests

        [Fact]
        public async Task UpdateRadTrackProgAsync_WhenSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var program = "PROG001";
            var url = string.Format(PimsApiEndpoints.UpdateRadTrackProg, Uri.EscapeDataString(program));
            var dto = new RadTrackProgDto { Program = program, Radtrackprog = true, Publicationprefix = "UPDATED" };
            var request = new RadTrackProgReq { Program = program, RadTrackProg = true, PublicationPrefix = "UPDATED" };
            var response = new RadTrackProgRes { Program = program, RadTrackProg = true, PublicationPrefix = "UPDATED" };
            var apiResponse = new ApiResponse<RadTrackProgRes> { Success = true, Data = response };
            var mappedDto = ApiResponseDto<RadTrackProgDto>.SuccessResponse(dto);

            _mapper.Map<RadTrackProgReq>(dto).Returns(request);
            _http.PutAsync<RadTrackProgReq, RadTrackProgRes>(url, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackProgDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateRadTrackProgAsync(program, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            await _http.Received(1).PutAsync<RadTrackProgReq, RadTrackProgRes>(url, request);
        }

        [Fact]
        public async Task UpdateRadTrackProgAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var program = "INVALID";
            var url = string.Format(PimsApiEndpoints.UpdateRadTrackProg, Uri.EscapeDataString(program));
            var dto = new RadTrackProgDto { Program = program };
            var request = new RadTrackProgReq { Program = program };
            var errors = new List<ApiError> { new ApiError { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<RadTrackProgRes> { Success = false, Errors = errors };
            var failDto = new ApiResponseDto<RadTrackProgDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<RadTrackProgReq>(dto).Returns(request);
            _http.PutAsync<RadTrackProgReq, RadTrackProgRes>(url, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<RadTrackProgDto>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.UpdateRadTrackProgAsync(program, dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _http.Received(1).PutAsync<RadTrackProgReq, RadTrackProgRes>(url, request);
        }

        #endregion

        #region DeleteRadTrackProgAsync Tests

        [Fact]
        public async Task DeleteRadTrackProgAsync_WhenSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var program = "PROG001";
            var url = string.Format(PimsApiEndpoints.DeleteRadTrackProg, Uri.EscapeDataString(program));
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            var mappedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.DeleteRadTrackProgAsync(program);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool>(url);
        }

        [Fact]
        public async Task DeleteRadTrackProgAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var program = "INVALID";
            var url = string.Format(PimsApiEndpoints.DeleteRadTrackProg, Uri.EscapeDataString(program));
            var errors = new List<ApiError> { new ApiError { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<bool> { Success = false, Errors = errors };
            var failDto = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.DeleteRadTrackProgAsync(program);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _http.Received(1).DeleteAsync<bool>(url);
        }

        #endregion

        #region GetAllProgramNamesAsync Tests

        [Fact]
        public async Task GetAllProgramNamesAsync_WhenSuccessResponse_ReturnsProgramNames()
        {
            // Arrange
            var programNames = new List<string> { "PROG001", "PROG002", "PROG003" };
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = programNames };

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetAllRadTrackProgNames).Returns(apiResponse);
            _mapper.Map<List<ApiErrorDto>>(Arg.Any<List<ApiError>>()).Returns(new List<ApiErrorDto>());
            _mapper.Map<ApiMetaDto>(apiResponse.Meta).Returns(new ApiMetaDto());

            // Act
            var result = await _client.GetAllProgramNamesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count);
            Assert.Contains("PROG001", result.Data);
            await _http.Received(1).GetAsync<List<string>>(PimsApiEndpoints.GetAllRadTrackProgNames);
        }

        [Fact]
        public async Task GetAllProgramNamesAsync_WhenApiReturnsEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = null };

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetAllRadTrackProgNames).Returns(apiResponse);
            _mapper.Map<List<ApiErrorDto>>(Arg.Any<List<ApiError>>()).Returns(new List<ApiErrorDto>());
            _mapper.Map<ApiMetaDto>(apiResponse.Meta).Returns(new ApiMetaDto());

            // Act
            var result = await _client.GetAllProgramNamesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
            await _http.Received(1).GetAsync<List<string>>(PimsApiEndpoints.GetAllRadTrackProgNames);
        }

        [Fact]
        public async Task GetAllProgramNamesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new ApiError { Message = "Error", Code = "ERR" } };
            var apiResponse = new ApiResponse<List<string>> { Success = false, Errors = errors };

            _http.GetAsync<List<string>>(PimsApiEndpoints.GetAllRadTrackProgNames).Returns(apiResponse);
            _mapper.Map<List<ApiErrorDto>>(errors).Returns(new List<ApiErrorDto> { new ApiErrorDto { Message = "Error", Code = "ERR" } });
            _mapper.Map<ApiMetaDto>(apiResponse.Meta).Returns(new ApiMetaDto());

            // Act
            var result = await _client.GetAllProgramNamesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _http.Received(1).GetAsync<List<string>>(PimsApiEndpoints.GetAllRadTrackProgNames);
        }

        #endregion
    }
}
