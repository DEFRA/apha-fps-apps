using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Integrations.FPSApis.Clients.FpsMasterLookupApiClientTest
{
    public class FpsMasterLookupApiClientTests
    {
        private const string TableName = "directorate";

        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsMasterLookupApiClient _client;

        public FpsMasterLookupApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsMasterLookupApiClient(_http, _mapper);
        }

        #region Constructor

        [Fact]
        public void Constructor_WithNullHttp_ThrowsArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new FpsMasterLookupApiClient(null!, _mapper));
            Assert.Equal("http", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new FpsMasterLookupApiClient(_http, null!));
            Assert.Equal("mapper", exception.ParamName);
        }

        #endregion

        #region GetAllMasterLookupsAsync

        [Fact]
        public async Task GetAllMasterLookupsAsync_Success_ReturnsMappedResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<MasterLookupRes>>
            {
                Success = true,
                Data = new List<MasterLookupRes> { new MasterLookupRes { MasterTableName = TableName } }
            };
            var mapped = new ApiResponseDto<IEnumerable<MasterLookupDto>>
            {
                Success = true,
                Data = new List<MasterLookupDto> { new MasterLookupDto { MasterTableName = TableName } }
            };
            _http.GetAsync<List<MasterLookupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<MasterLookupDto>>>(apiResponse).Returns(mapped);

            // Act
            var result = await _client.GetAllMasterLookupsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<MasterLookupRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetAllMasterLookupsAsync_ApiFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<MasterLookupRes>> { Success = false };
            var mapped = new ApiResponseDto<IEnumerable<MasterLookupDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "err" } }
            };
            _http.GetAsync<List<MasterLookupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<MasterLookupDto>>>(apiResponse).Returns(mapped);

            // Act
            var result = await _client.GetAllMasterLookupsAsync();

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAllMasterLookupsAsync_Exception_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<List<MasterLookupRes>>(Arg.Any<string>()).Throws(new Exception("boom"));

            // Act
            var result = await _client.GetAllMasterLookupsAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region GetLookupItemsAsync

        [Fact]
        public async Task GetLookupItemsAsync_Success_ReturnsMappedResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<LookupItemRes>>
            {
                Success = true,
                Data = new List<LookupItemRes> { new LookupItemRes { Value = "A" } }
            };
            var mapped = new ApiResponseDto<IEnumerable<LookupItemDto>>
            {
                Success = true,
                Data = new List<LookupItemDto> { new LookupItemDto { Value = "A" } }
            };
            _http.GetAsync<List<LookupItemRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<LookupItemDto>>>(apiResponse).Returns(mapped);

            // Act
            var result = await _client.GetLookupItemsAsync(TableName);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetLookupItemsAsync_ApiFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<LookupItemRes>> { Success = false };
            var mapped = new ApiResponseDto<IEnumerable<LookupItemDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "err" } }
            };
            _http.GetAsync<List<LookupItemRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<LookupItemDto>>>(apiResponse).Returns(mapped);

            // Act
            var result = await _client.GetLookupItemsAsync(TableName);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetLookupItemsAsync_Exception_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<List<LookupItemRes>>(Arg.Any<string>()).Throws(new Exception("boom"));

            // Act
            var result = await _client.GetLookupItemsAsync(TableName);

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region GetLookupItemsPagedAsync

        [Fact]
        public async Task GetLookupItemsPagedAsync_Success_ReturnsPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<LookupItemRes>>
            {
                Success = true,
                Data = new List<LookupItemRes> { new LookupItemRes { Value = "A" } },
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            _http.GetAsync<List<LookupItemRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<List<LookupItemDto>>(apiResponse.Data)
                .Returns(new List<LookupItemDto> { new LookupItemDto { Value = "A" } });

            // Act
            var result = await _client.GetLookupItemsPagedAsync(TableName, query);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!.data);
            Assert.Equal(1, result.Data.TotalCount);
        }

        [Fact]
        public async Task GetLookupItemsPagedAsync_ApiFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<LookupItemRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new ApiError { Message = "err" } }
            };
            _http.GetAsync<List<LookupItemRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<List<ApiErrorDto>>(Arg.Any<List<ApiError>>())
                .Returns(new List<ApiErrorDto> { new ApiErrorDto { Message = "err" } });
            _mapper.Map<ApiMetaDto>(Arg.Any<ApiMeta>()).Returns(new ApiMetaDto());

            // Act
            var result = await _client.GetLookupItemsPagedAsync(TableName, query);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetLookupItemsPagedAsync_Exception_ReturnsInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<LookupItemRes>>(Arg.Any<string>()).Throws(new Exception("boom"));

            // Act
            var result = await _client.GetLookupItemsPagedAsync(TableName, query);

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region CreateLookupItemAsync

        [Fact]
        public async Task CreateLookupItemAsync_Success_ReturnsMappedResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var mapped = new ApiResponseDto<bool> { Success = true, Data = true };
            _http.PostAsync<LookupItemReq, bool?>(Arg.Any<string>(), Arg.Any<LookupItemReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mapped);

            // Act
            var result = await _client.CreateLookupItemAsync(TableName, "A");

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task CreateLookupItemAsync_ApiFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = false };
            var mapped = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "err" } }
            };
            _http.PostAsync<LookupItemReq, bool?>(Arg.Any<string>(), Arg.Any<LookupItemReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mapped);

            // Act
            var result = await _client.CreateLookupItemAsync(TableName, "A");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateLookupItemAsync_Exception_ReturnsInternalError()
        {
            // Arrange
            _http.PostAsync<LookupItemReq, bool?>(Arg.Any<string>(), Arg.Any<LookupItemReq>())
                .Throws(new Exception("boom"));

            // Act
            var result = await _client.CreateLookupItemAsync(TableName, "A");

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region UpdateLookupItemAsync

        [Fact]
        public async Task UpdateLookupItemAsync_Success_ReturnsMappedResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var mapped = new ApiResponseDto<bool> { Success = true, Data = true };
            _http.PutAsync<LookupItemReq, bool?>(Arg.Any<string>(), Arg.Any<LookupItemReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mapped);

            // Act
            var result = await _client.UpdateLookupItemAsync(TableName, "Old", "New");

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task UpdateLookupItemAsync_ApiFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = false };
            var mapped = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "err" } }
            };
            _http.PutAsync<LookupItemReq, bool?>(Arg.Any<string>(), Arg.Any<LookupItemReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mapped);

            // Act
            var result = await _client.UpdateLookupItemAsync(TableName, "Old", "New");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateLookupItemAsync_Exception_ReturnsInternalError()
        {
            // Arrange
            _http.PutAsync<LookupItemReq, bool?>(Arg.Any<string>(), Arg.Any<LookupItemReq>())
                .Throws(new Exception("boom"));

            // Act
            var result = await _client.UpdateLookupItemAsync(TableName, "Old", "New");

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region DeleteLookupItemAsync

        [Fact]
        public async Task DeleteLookupItemAsync_Success_ReturnsMappedResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var mapped = new ApiResponseDto<bool> { Success = true, Data = true };
            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mapped);

            // Act
            var result = await _client.DeleteLookupItemAsync(TableName, "A");

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task DeleteLookupItemAsync_ApiFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = false };
            var mapped = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "err" } }
            };
            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mapped);

            // Act
            var result = await _client.DeleteLookupItemAsync(TableName, "A");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task DeleteLookupItemAsync_Exception_ReturnsInternalError()
        {
            // Arrange
            _http.DeleteAsync<bool?>(Arg.Any<string>()).Throws(new Exception("boom"));

            // Act
            var result = await _client.DeleteLookupItemAsync(TableName, "A");

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion
    }
}
