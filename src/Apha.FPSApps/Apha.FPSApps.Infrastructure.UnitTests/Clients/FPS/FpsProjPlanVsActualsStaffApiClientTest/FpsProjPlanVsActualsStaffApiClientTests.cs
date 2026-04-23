using Apha.Common.Constants;
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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsProjPlanVsActualsStaffApiClientTest
{
    public class FpsProjPlanVsActualsStaffApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsProjPlanVsActualsStaffApiClient _client;

        public FpsProjPlanVsActualsStaffApiClientTests()
        {
            _http   = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsProjPlanVsActualsStaffApiClient(_http, _mapper);
        }

        private static QueryParameters<string> DefaultQuery(int page = 1, int pageSize = 10)
            => new QueryParameters<string> { Page = page, PageSize = pageSize };

        #region GetTimeCostCalcsByProjectAsync — Happy path

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_WithSuccessResponse_ReturnsMappedDtoList()
        {
            // Arrange
            var query       = DefaultQuery();
            var projectCode = "AH0033";
            var resList = new List<TimeCostCalcsViewRes>
            {
                new() { Project = projectCode, StaffId = "S01", Name = "Alice", WorkGroup = "WG1", Month = 1, Time = 8, Cost = 100 },
                new() { Project = projectCode, StaffId = "S02", Name = "Bob",   WorkGroup = "WG2", Month = 2, Time = 6, Cost = 80  }
            };
            var apiResponse = new ApiResponse<List<TimeCostCalcsViewRes>>
            {
                Success    = true,
                Data       = resList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };
            var expectedDto = ApiResponseDto<List<TimeCostCalcsViewDto>>.SuccessResponse(
                new List<TimeCostCalcsViewDto>
                {
                    new() { Project = projectCode, StaffId = "S01", Name = "Alice" },
                    new() { Project = projectCode, StaffId = "S02", Name = "Bob"   }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 });

            _http.GetAsync<List<TimeCostCalcsViewRes>>(
                    Arg.Is<string>(url => url.Contains($"projectCode={projectCode}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeCostCalcsViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetTimeCostCalcsByProjectAsync(query, projectCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<TimeCostCalcsViewRes>>(
                Arg.Is<string>(url => url.Contains($"projectCode={projectCode}")));
            _mapper.Received(1).Map<ApiResponseDto<List<TimeCostCalcsViewDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_WithSuccessResponse_UrlContainsProjectCode()
        {
            // Arrange
            var query       = DefaultQuery();
            var projectCode = "PROJ001";
            var apiResponse = new ApiResponse<List<TimeCostCalcsViewRes>> { Success = true, Data = new List<TimeCostCalcsViewRes>() };
            var mappedDto   = ApiResponseDto<List<TimeCostCalcsViewDto>>.SuccessResponse(new List<TimeCostCalcsViewDto>());

            _http.GetAsync<List<TimeCostCalcsViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeCostCalcsViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetTimeCostCalcsByProjectAsync(query, projectCode);

            // Assert
            await _http.Received(1).GetAsync<List<TimeCostCalcsViewRes>>(
                Arg.Is<string>(url => url.Contains("projectCode=PROJ001")));
        }

        #endregion

        #region GetTimeCostCalcsByProjectAsync — Failure path

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query       = DefaultQuery();
            var projectCode = "AH0033";
            var apiResponse = new ApiResponse<List<TimeCostCalcsViewRes>>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "API Error", Code = "ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<List<TimeCostCalcsViewDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "API Error", Code = "ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<TimeCostCalcsViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeCostCalcsViewDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetTimeCostCalcsByProjectAsync(query, projectCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_WhenApiReturnsEmpty_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query       = DefaultQuery();
            var projectCode = "AH0033";
            var apiResponse = new ApiResponse<List<TimeCostCalcsViewRes>>
            {
                Success    = true,
                Data       = new List<TimeCostCalcsViewRes>(),
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            };
            var mappedDto = ApiResponseDto<List<TimeCostCalcsViewDto>>.SuccessResponse(
                new List<TimeCostCalcsViewDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 });

            _http.GetAsync<List<TimeCostCalcsViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeCostCalcsViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetTimeCostCalcsByProjectAsync(query, projectCode);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        #endregion

        #region GetTimeCostCalcsByProjectAsync — URL construction

        [Theory]
        [InlineData("AH0033")]
        [InlineData("PROJ001")]
        [InlineData("BCP-OPS")]
        public async Task GetTimeCostCalcsByProjectAsync_AlwaysIncludesProjectCodeInUrl(string projectCode)
        {
            // Arrange
            var apiResponse = new ApiResponse<List<TimeCostCalcsViewRes>> { Success = true, Data = new List<TimeCostCalcsViewRes>() };
            var mappedDto   = ApiResponseDto<List<TimeCostCalcsViewDto>>.SuccessResponse(new List<TimeCostCalcsViewDto>());

            _http.GetAsync<List<TimeCostCalcsViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeCostCalcsViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetTimeCostCalcsByProjectAsync(DefaultQuery(), projectCode);

            // Assert
            await _http.Received(1).GetAsync<List<TimeCostCalcsViewRes>>(
                Arg.Is<string>(url => url.Contains($"projectCode={projectCode}")));
        }

        #endregion

        #region GetTotalActualByProjectAsync

        [Fact]
        public async Task GetTotalActualByProjectAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var projectCode = "AH0033";
            var apiResponse = new ApiResponse<TimeCostCalcsTotalsRes>
            {
                Success = true,
                Data    = new TimeCostCalcsTotalsRes { TotalHours = 40.5, TotalCost = 5000.0 }
            };
            var mappedDto = ApiResponseDto<TimeCostCalcsTotalsDto>.SuccessResponse(
                new TimeCostCalcsTotalsDto { TotalHours = 40.5, TotalCost = 5000.0 });

            _http.GetAsync<TimeCostCalcsTotalsRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TimeCostCalcsTotalsDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetTotalActualByProjectAsync(projectCode);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(40.5,   result.Data?.TotalHours);
            Assert.Equal(5000.0, result.Data?.TotalCost);
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_UrlContainsProjectCode()
        {
            // Arrange
            var projectCode = "AH0033";
            var apiResponse = new ApiResponse<TimeCostCalcsTotalsRes> { Success = true, Data = new TimeCostCalcsTotalsRes() };
            var mappedDto   = ApiResponseDto<TimeCostCalcsTotalsDto>.SuccessResponse(new TimeCostCalcsTotalsDto());

            _http.GetAsync<TimeCostCalcsTotalsRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TimeCostCalcsTotalsDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetTotalActualByProjectAsync(projectCode);

            // Assert
            await _http.Received(1).GetAsync<TimeCostCalcsTotalsRes>(
                Arg.Is<string>(url => url.Contains($"projectCode={projectCode}")));
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var projectCode = "AH0033";
            var apiResponse = new ApiResponse<TimeCostCalcsTotalsRes>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Error", Code = "ERR" } }
            };
            var mappedDto = new ApiResponseDto<TimeCostCalcsTotalsDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<TimeCostCalcsTotalsRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TimeCostCalcsTotalsDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetTotalActualByProjectAsync(projectCode);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteTimeCostCalcsAsync

        [Fact]
        public async Task DeleteTimeCostCalcsAsync_WithValidParams_ReturnsSuccess()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            _http.DeleteAsync<TimeCostCalcsReq, bool>(Arg.Any<string>(), Arg.Any<TimeCostCalcsReq>()).Returns(apiResponse);

            // Act
            var result = await _client.DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 1, "S01");

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task DeleteTimeCostCalcsAsync_UrlContainsAllParams()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            _http.DeleteAsync<TimeCostCalcsReq, bool>(Arg.Any<string>(), Arg.Any<TimeCostCalcsReq>()).Returns(apiResponse);

            // Act
            await _client.DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 1, "S01");

            // Assert
            await _http.Received(1).DeleteAsync<TimeCostCalcsReq, bool>(
                FpsApiEndpoints.DeleteTimeCostCalcs,
                Arg.Is<TimeCostCalcsReq>(r =>
                    r.WorkGroup == "WG1" &&
                    r.JobCode   == "JOB1" &&
                    r.Project   == "AH0033" &&
                    r.Month     == 1 &&
                    r.StaffId   == "S01"));
        }

        [Fact]
        public async Task DeleteTimeCostCalcsAsync_WhenApiReturnsFailure_ReturnsFailure()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } }
            };
            var mappedDto = new ApiResponseDto<bool>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.DeleteAsync<TimeCostCalcsReq, bool>(Arg.Any<string>(), Arg.Any<TimeCostCalcsReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 1, "S01");

            // Assert
            Assert.False(result.Success);
        }

        #endregion
    }
}
