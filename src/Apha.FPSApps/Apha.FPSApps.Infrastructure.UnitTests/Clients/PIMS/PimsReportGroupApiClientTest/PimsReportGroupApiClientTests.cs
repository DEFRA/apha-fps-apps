using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients;
using AutoMapper;
using NSubstitute;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsReportGroupApiClientTest
{
    public class PimsReportGroupApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsReportGroupApiClient _client;

        public PimsReportGroupApiClientTests()
        {
            _http = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsReportGroupApiClient(_http, _mapper);
        }

        #region GetAllReportGroupsAsync Tests

        [Fact]
        public async Task GetAllReportGroupsAsync_WhenSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var reportGroupList = new List<ReportGroupRes>
            {
                new ReportGroupRes { GroupId = 1, Description = "Group1" },
                new ReportGroupRes { GroupId = 2, Description = "Group2" }
            };
            var apiResponse = new ApiResponse<List<ReportGroupRes>> { Success = true, Data = reportGroupList };
            var mappedDto = ApiResponseDto<List<ReportGroupDto>>.SuccessResponse(new List<ReportGroupDto>
            {
                new ReportGroupDto { GroupId = 1, Description = "Group1" },
                new ReportGroupDto { GroupId = 2, Description = "Group2" }
            });

            _http.GetAsync<List<ReportGroupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ReportGroupDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllReportGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _http.Received(1).GetAsync<List<ReportGroupRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<ReportGroupDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllReportGroupsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new ApiError { Message = "Error", Code = "ERR" } };
            var apiResponse = new ApiResponse<List<ReportGroupRes>> { Success = false, Data = null, Errors = errors };
            var failDto = new ApiResponseDto<List<ReportGroupDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Error", Code = "ERR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ReportGroupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ReportGroupDto>>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.GetAllReportGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _http.Received(1).GetAsync<List<ReportGroupRes>>(Arg.Any<string>());
        }

        #endregion

        #region GetReportGroupsByReportIdAsync Tests

        [Fact]
        public async Task GetReportGroupsByReportIdAsync_WhenSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var reportId = 1;
            var reportGroupList = new List<ReportGroupRes> { new ReportGroupRes { GroupId = 1, Description = "Group1" } };
            var apiResponse = new ApiResponse<List<ReportGroupRes>> { Success = true, Data = reportGroupList };
            var mappedDto = ApiResponseDto<List<ReportGroupDto>>.SuccessResponse(new List<ReportGroupDto>
            {
                new ReportGroupDto { GroupId = 1, Description = "Group1" }
            });

            _http.GetAsync<List<ReportGroupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ReportGroupDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetReportGroupsByReportIdAsync(reportId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            await _http.Received(1).GetAsync<List<ReportGroupRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetReportGroupsByReportIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var reportId = 999;
            var errors = new List<ApiError> { new ApiError { Message = "Error", Code = "ERR" } };
            var apiResponse = new ApiResponse<List<ReportGroupRes>> { Success = false, Errors = errors };

            _http.GetAsync<List<ReportGroupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ReportGroupDto>>>(apiResponse).Returns(
                new ApiResponseDto<List<ReportGroupDto>>
                {
                    Success = false,
                    Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Error", Code = "ERR" } },
                    Meta = new ApiMetaDto()
                });

            // Act
            var result = await _client.GetReportGroupsByReportIdAsync(reportId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _http.Received(1).GetAsync<List<ReportGroupRes>>(Arg.Any<string>());
        }

        #endregion

        #region GetPagedReportGroupsAsync Tests

        [Fact]
        public async Task GetPagedReportGroupsAsync_WhenSuccessResponse_ReturnsPagedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var reportGroupList = new List<ReportGroupRes> { new ReportGroupRes { GroupId = 1, Description = "Group1" } };
            var apiResponse = new ApiResponse<List<ReportGroupRes>>
            {
                Success = true,
                Data = reportGroupList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var mappedItems = new List<ReportGroupDto> { new ReportGroupDto { GroupId = 1, Description = "Group1" } };

            _http.GetAsync<List<ReportGroupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<List<ReportGroupDto>>(reportGroupList).Returns(mappedItems);

            // Act
            var result = await _client.GetPagedReportGroupsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.data);
            await _http.Received(1).GetAsync<List<ReportGroupRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetPagedReportGroupsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new ApiError { Message = "Error", Code = "ERR" } };
            var apiResponse = new ApiResponse<List<ReportGroupRes>> { Success = false, Errors = errors };

            _http.GetAsync<List<ReportGroupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<List<ApiErrorDto>>(errors).Returns(new List<ApiErrorDto> { new ApiErrorDto { Message = "Error", Code = "ERR" } });
            _mapper.Map<ApiMetaDto>(apiResponse.Meta).Returns(new ApiMetaDto());

            // Act
            var result = await _client.GetPagedReportGroupsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _http.Received(1).GetAsync<List<ReportGroupRes>>(Arg.Any<string>());
        }

        #endregion

        #region GetReportGroupByIdAsync Tests

        [Fact]
        public async Task GetReportGroupByIdAsync_WhenSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var groupId = 1;
            var reportGroupRes = new ReportGroupRes { GroupId = groupId, Description = "Group1" };
            var apiResponse = new ApiResponse<ReportGroupRes> { Success = true, Data = reportGroupRes };
            var mappedDto = ApiResponseDto<ReportGroupDto>.SuccessResponse(new ReportGroupDto { GroupId = groupId, Description = "Group1" });

            _http.GetAsync<ReportGroupRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ReportGroupDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetReportGroupByIdAsync(groupId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(groupId, result.Data.GroupId);
            await _http.Received(1).GetAsync<ReportGroupRes>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetReportGroupByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var groupId = 999;
            var errors = new List<ApiError> { new ApiError { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<ReportGroupRes> { Success = false, Errors = errors };
            var failDto = new ApiResponseDto<ReportGroupDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<ReportGroupRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ReportGroupDto>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.GetReportGroupByIdAsync(groupId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _http.Received(1).GetAsync<ReportGroupRes>(Arg.Any<string>());
        }

        #endregion

        #region CreateReportGroupAsync Tests

        [Fact]
        public async Task CreateReportGroupAsync_WhenSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var dto = new ReportGroupDto { GroupId = 1, Description = "NewGroup" };
            var request = new ReportGroupReq { GroupId = 1, Description = "NewGroup" };
            var response = new ReportGroupRes { GroupId = 1, Description = "NewGroup" };
            var apiResponse = new ApiResponse<ReportGroupRes> { Success = true, Data = response };
            var mappedDto = ApiResponseDto<ReportGroupDto>.SuccessResponse(dto);

            _mapper.Map<ReportGroupReq>(dto).Returns(request);
            _http.PostAsync<ReportGroupReq, ReportGroupRes>(Arg.Any<string>(), request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ReportGroupDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.CreateReportGroupAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            await _http.Received(1).PostAsync<ReportGroupReq, ReportGroupRes>(Arg.Any<string>(), request);
        }

        [Fact]
        public async Task CreateReportGroupAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new ReportGroupDto { GroupId = 1, Description = "NewGroup" };
            var request = new ReportGroupReq { GroupId = 1, Description = "NewGroup" };
            var errors = new List<ApiError> { new ApiError { Message = "Duplicate", Code = "DUPLICATE" } };
            var apiResponse = new ApiResponse<ReportGroupRes> { Success = false, Errors = errors };
            var failDto = new ApiResponseDto<ReportGroupDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Duplicate", Code = "DUPLICATE" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ReportGroupReq>(dto).Returns(request);
            _http.PostAsync<ReportGroupReq, ReportGroupRes>(Arg.Any<string>(), request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ReportGroupDto>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.CreateReportGroupAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _http.Received(1).PostAsync<ReportGroupReq, ReportGroupRes>(Arg.Any<string>(), request);
        }

        #endregion

        #region UpdateReportGroupAsync Tests

        [Fact]
        public async Task UpdateReportGroupAsync_WhenSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var groupId = 1;
            var dto = new ReportGroupDto { GroupId = groupId, Description = "UpdatedGroup" };
            var request = new ReportGroupReq { GroupId = groupId, Description = "UpdatedGroup" };
            var response = new ReportGroupRes { GroupId = groupId, Description = "UpdatedGroup" };
            var apiResponse = new ApiResponse<ReportGroupRes> { Success = true, Data = response };
            var mappedDto = ApiResponseDto<ReportGroupDto>.SuccessResponse(dto);

            _mapper.Map<ReportGroupReq>(dto).Returns(request);
            _http.PutAsync<ReportGroupReq, ReportGroupRes>(Arg.Any<string>(), request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ReportGroupDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateReportGroupAsync(groupId, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            await _http.Received(1).PutAsync<ReportGroupReq, ReportGroupRes>(Arg.Any<string>(), request);
        }

        [Fact]
        public async Task UpdateReportGroupAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var groupId = 999;
            var dto = new ReportGroupDto { GroupId = groupId };
            var request = new ReportGroupReq { GroupId = groupId };
            var errors = new List<ApiError> { new ApiError { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<ReportGroupRes> { Success = false, Errors = errors };
            var failDto = new ApiResponseDto<ReportGroupDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ReportGroupReq>(dto).Returns(request);
            _http.PutAsync<ReportGroupReq, ReportGroupRes>(Arg.Any<string>(), request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ReportGroupDto>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.UpdateReportGroupAsync(groupId, dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _http.Received(1).PutAsync<ReportGroupReq, ReportGroupRes>(Arg.Any<string>(), request);
        }

        #endregion

        #region DeleteReportGroupAsync Tests

        [Fact]
        public async Task DeleteReportGroupAsync_WhenSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var groupId = 1;
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var mappedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.DeleteReportGroupAsync(groupId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool?>(Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteReportGroupAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var groupId = 999;
            var errors = new List<ApiError> { new ApiError { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = errors };
            var failDto = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.DeleteReportGroupAsync(groupId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _http.Received(1).DeleteAsync<bool?>(Arg.Any<string>());
        }

        #endregion
    }
}
