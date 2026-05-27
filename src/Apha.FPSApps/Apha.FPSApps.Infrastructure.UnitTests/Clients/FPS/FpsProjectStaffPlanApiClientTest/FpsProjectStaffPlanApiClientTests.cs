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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsProjectStaffPlanApiClientTest
{
    public class FpsProjectStaffPlanApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsProjectStaffPlanApiClient _client;

        public FpsProjectStaffPlanApiClientTests()
        {
            _http   = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsProjectStaffPlanApiClient(_http, _mapper);
        }

        private static QueryParameters<string> DefaultQuery(int page = 1, int pageSize = 10)
            => new QueryParameters<string> { Page = page, PageSize = pageSize };

        private static List<ProjectStaffPlanViewRes> BuildResList() =>
        [
            new() { ParentProject = "AH0032", ProgramNo = "Wildlife", Name = "E_WILDLIFE, General",
                    StaffId = "1625", PlannedHours = 25344, ChargeRate = 53.34m, Cost = 1351848.96m, PayCost = 1001341.44m,
                    WorkGroup = "Wildlife", GradeCode = "E" },
            new() { ParentProject = "ED1044", ProgramNo = "SIU",      Name = "C_SVCA, General",
                    StaffId = "1357", PlannedHours = 12000, ChargeRate = 69.92m, Cost = 839040.00m, PayCost = 624720.00m,
                    WorkGroup = "SVCA",     GradeCode = "C" }
        ];

        private static ApiResponse<List<ProjectStaffPlanViewRes>> BuildSuccessApiResponse(
            List<ProjectStaffPlanViewRes>? data = null,
            int totalRecords = 2) =>
            new()
            {
                Success    = true,
                Data       = data ?? BuildResList(),
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = totalRecords }
            };

        private static ApiResponse<List<ProjectStaffPlanViewRes>> BuildFailureApiResponse() =>
            new()
            {
                Success = false,
                Errors  = [new ApiError { Code = "API_ERROR", Message = "API error" }]
            };

        #region GetPagedAsync — Happy path

        [Fact]
        public async Task GetPagedAsync_WithSuccessResponse_ReturnsMappedDtoList()
        {
            // Arrange
            var query       = DefaultQuery();
            var apiResponse = BuildSuccessApiResponse();
            var mappedDto   = ApiResponseDto<List<ProjectStaffPlanViewDto>>.SuccessResponse(
                [
                    new() { ParentProject = "AH0032", StaffId = "1625" },
                    new() { ParentProject = "ED1044", StaffId = "1357" }
                ],
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 });

            _http.GetAsync<List<ProjectStaffPlanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectStaffPlanViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
        }

        [Fact]
        public async Task GetPagedAsync_WithSuccessResponse_CallsHttpGetWithCorrectEndpoint()
        {
            // Arrange
            var query       = DefaultQuery();
            var apiResponse = BuildSuccessApiResponse();
            var mappedDto   = ApiResponseDto<List<ProjectStaffPlanViewDto>>.SuccessResponse([]);

            _http.GetAsync<List<ProjectStaffPlanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectStaffPlanViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetPagedAsync(query);

            // Assert
            await _http.Received(1).GetAsync<List<ProjectStaffPlanViewRes>>(
                Arg.Is<string>(url => url.Contains(FpsApiEndpoints.GetPagedProjectStaffPlan)));
        }

        [Fact]
        public async Task GetPagedAsync_WithSuccessResponse_CallsMapperWithApiResponse()
        {
            // Arrange
            var query       = DefaultQuery();
            var apiResponse = BuildSuccessApiResponse();
            var mappedDto   = ApiResponseDto<List<ProjectStaffPlanViewDto>>.SuccessResponse([]);

            _http.GetAsync<List<ProjectStaffPlanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectStaffPlanViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetPagedAsync(query);

            // Assert
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectStaffPlanViewDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetPagedAsync_WithSuccessResponse_UrlContainsPaginationParameters()
        {
            // Arrange
            var query       = DefaultQuery(page: 2, pageSize: 25);
            var apiResponse = BuildSuccessApiResponse();
            var mappedDto   = ApiResponseDto<List<ProjectStaffPlanViewDto>>.SuccessResponse([]);

            _http.GetAsync<List<ProjectStaffPlanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectStaffPlanViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetPagedAsync(query);

            // Assert
            await _http.Received(1).GetAsync<List<ProjectStaffPlanViewRes>>(
                Arg.Is<string>(url => url.Contains("Page=2") || url.Contains("page=2")));
        }

        [Fact]
        public async Task GetPagedAsync_WithEmptyList_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            var query       = DefaultQuery();
            var apiResponse = BuildSuccessApiResponse(data: [], totalRecords: 0);
            var mappedDto   = ApiResponseDto<List<ProjectStaffPlanViewDto>>.SuccessResponse([]);

            _http.GetAsync<List<ProjectStaffPlanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectStaffPlanViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetPagedAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        #endregion

        #region GetPagedAsync — Failure path

        [Fact]
        public async Task GetPagedAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query       = DefaultQuery();
            var apiResponse = BuildFailureApiResponse();
            var mappedDto   = ApiResponseDto<List<ProjectStaffPlanViewDto>>.FailureResponse(
                [new ApiErrorDto { Code = "API_ERROR", Message = "API error" }],
                new ApiMetaDto());

            _http.GetAsync<List<ProjectStaffPlanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectStaffPlanViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetPagedAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetPagedAsync_WhenApiReturnsFailure_DataIsNull()
        {
            // Arrange
            var query       = DefaultQuery();
            var apiResponse = BuildFailureApiResponse();
            var mappedDto   = ApiResponseDto<List<ProjectStaffPlanViewDto>>.FailureResponse([], new ApiMetaDto());

            _http.GetAsync<List<ProjectStaffPlanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectStaffPlanViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetPagedAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Data);
        }

        #endregion
    }
}
