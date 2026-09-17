using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients;
using AutoMapper;
using NSubstitute;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsReportGroupLinkApiClientTest
{
    public class PimsReportGroupLinkApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsReportGroupLinkApiClient _client;

        public PimsReportGroupLinkApiClientTests()
        {
            _http = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsReportGroupLinkApiClient(_http, _mapper);
        }

        #region GetAllReportGroupLinksAsync Tests

        [Fact]
        public async Task GetAllReportGroupLinksAsync_WhenSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var reportGroupLinkList = new List<ReportGroupLinkRes>
            {
                new ReportGroupLinkRes { ReportId = 1, GroupId = 1 },
                new ReportGroupLinkRes { ReportId = 1, GroupId = 2 }
            };
            var apiResponse = new ApiResponse<List<ReportGroupLinkRes>> { Success = true, Data = reportGroupLinkList };
            var mappedDto = ApiResponseDto<List<ReportGroupLinkDto>>.SuccessResponse(new List<ReportGroupLinkDto>
            {
                new ReportGroupLinkDto { ReportId = 1, GroupId = 1 },
                new ReportGroupLinkDto { ReportId = 1, GroupId = 2 }
            });

            _http.GetAsync<List<ReportGroupLinkRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ReportGroupLinkDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllReportGroupLinksAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _http.Received(1).GetAsync<List<ReportGroupLinkRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<ReportGroupLinkDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllReportGroupLinksAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new ApiError { Message = "Error", Code = "ERR" } };
            var apiResponse = new ApiResponse<List<ReportGroupLinkRes>> { Success = false, Data = null, Errors = errors };
            var failDto = new ApiResponseDto<List<ReportGroupLinkDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Error", Code = "ERR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ReportGroupLinkRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ReportGroupLinkDto>>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.GetAllReportGroupLinksAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _http.Received(1).GetAsync<List<ReportGroupLinkRes>>(Arg.Any<string>());
        }

        #endregion

        #region GetReportGroupLinksByReportIdAsync Tests

        [Fact]
        public async Task GetReportGroupLinksByReportIdAsync_WhenSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var reportId = 1;
            var reportGroupLinkList = new List<ReportGroupLinkRes> { new ReportGroupLinkRes { ReportId = 1, GroupId = 1 } };
            var apiResponse = new ApiResponse<List<ReportGroupLinkRes>> { Success = true, Data = reportGroupLinkList };
            var mappedDto = ApiResponseDto<List<ReportGroupLinkDto>>.SuccessResponse(new List<ReportGroupLinkDto>
            {
                new ReportGroupLinkDto { ReportId = 1, GroupId = 1 }
            });

            _http.GetAsync<List<ReportGroupLinkRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ReportGroupLinkDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetReportGroupLinksByReportIdAsync(reportId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Count);
            await _http.Received(1).GetAsync<List<ReportGroupLinkRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetReportGroupLinksByReportIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var reportId = 1;
            var errors = new List<ApiError> { new ApiError { Message = "Error", Code = "ERR" } };
            var apiResponse = new ApiResponse<List<ReportGroupLinkRes>> { Success = false, Data = null, Errors = errors };
            var failDto = new ApiResponseDto<List<ReportGroupLinkDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Error", Code = "ERR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ReportGroupLinkRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ReportGroupLinkDto>>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.GetReportGroupLinksByReportIdAsync(reportId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _http.Received(1).GetAsync<List<ReportGroupLinkRes>>(Arg.Any<string>());
        }

        #endregion

        #region GetReportGroupLinkByIdAsync Tests

        [Fact]
        public async Task GetReportGroupLinkByIdAsync_WhenSuccessResponse_ReturnsMappedObject()
        {
            // Arrange
            var reportId = 1;
            var groupId = 1;
            var reportGroupLink = new ReportGroupLinkRes { ReportId = 1, GroupId = 1 };
            var apiResponse = new ApiResponse<ReportGroupLinkRes> { Success = true, Data = reportGroupLink };
            var mappedDto = ApiResponseDto<ReportGroupLinkDto>.SuccessResponse(new ReportGroupLinkDto { ReportId = 1, GroupId = 1 });

            _http.GetAsync<ReportGroupLinkRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ReportGroupLinkDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetReportGroupLinkByIdAsync(reportId, groupId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.ReportId);
            Assert.Equal(1, result.Data.GroupId);
            await _http.Received(1).GetAsync<ReportGroupLinkRes>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetReportGroupLinkByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var reportId = 1;
            var groupId = 1;
            var errors = new List<ApiError> { new ApiError { Message = "Error", Code = "ERR" } };
            var apiResponse = new ApiResponse<ReportGroupLinkRes> { Success = false, Data = null, Errors = errors };
            var failDto = new ApiResponseDto<ReportGroupLinkDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Error", Code = "ERR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<ReportGroupLinkRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ReportGroupLinkDto>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.GetReportGroupLinkByIdAsync(reportId, groupId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _http.Received(1).GetAsync<ReportGroupLinkRes>(Arg.Any<string>());
        }

        #endregion

        #region CreateReportGroupLinkAsync Tests

        [Fact]
        public async Task CreateReportGroupLinkAsync_WhenSuccessResponse_ReturnsMappedObject()
        {
            // Arrange
            var dto = new ReportGroupLinkDto { ReportId = 1, GroupId = 1 };
            var request = new ReportGroupLinkReq { ReportId = 1, GroupId = 1 };
            var reportGroupLink = new ReportGroupLinkRes { ReportId = 1, GroupId = 1 };
            var apiResponse = new ApiResponse<ReportGroupLinkRes> { Success = true, Data = reportGroupLink };
            var mappedDto = ApiResponseDto<ReportGroupLinkDto>.SuccessResponse(new ReportGroupLinkDto { ReportId = 1, GroupId = 1 });

            _mapper.Map<ReportGroupLinkReq>(dto).Returns(request);
            _http.PostAsync<ReportGroupLinkReq, ReportGroupLinkRes>(Arg.Any<string>(), Arg.Any<ReportGroupLinkReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ReportGroupLinkDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.CreateReportGroupLinkAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            await _http.Received(1).PostAsync<ReportGroupLinkReq, ReportGroupLinkRes>(Arg.Any<string>(), Arg.Any<ReportGroupLinkReq>());
        }

        [Fact]
        public async Task CreateReportGroupLinkAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new ReportGroupLinkDto { ReportId = 1, GroupId = 1 };
            var request = new ReportGroupLinkReq { ReportId = 1, GroupId = 1 };
            var errors = new List<ApiError> { new ApiError { Message = "Error", Code = "ERR" } };
            var apiResponse = new ApiResponse<ReportGroupLinkRes> { Success = false, Data = null, Errors = errors };
            var failDto = new ApiResponseDto<ReportGroupLinkDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Error", Code = "ERR" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ReportGroupLinkReq>(dto).Returns(request);
            _http.PostAsync<ReportGroupLinkReq, ReportGroupLinkRes>(Arg.Any<string>(), Arg.Any<ReportGroupLinkReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ReportGroupLinkDto>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.CreateReportGroupLinkAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _http.Received(1).PostAsync<ReportGroupLinkReq, ReportGroupLinkRes>(Arg.Any<string>(), Arg.Any<ReportGroupLinkReq>());
        }

        #endregion

        #region DeleteReportGroupLinkAsync Tests

        [Fact]
        public async Task DeleteReportGroupLinkAsync_WhenSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var reportId = 1;
            var groupId = 1;
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            var mappedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.DeleteReportGroupLinkAsync(reportId, groupId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).DeleteAsync<bool>(Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteReportGroupLinkAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var reportId = 1;
            var groupId = 1;
            var errors = new List<ApiError> { new ApiError { Message = "Error", Code = "ERR" } };
            var apiResponse = new ApiResponse<bool> { Success = false, Data = false, Errors = errors };
            var failDto = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Error", Code = "ERR" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.DeleteReportGroupLinkAsync(reportId, groupId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _http.Received(1).DeleteAsync<bool>(Arg.Any<string>());
        }

        #endregion
    }
}
