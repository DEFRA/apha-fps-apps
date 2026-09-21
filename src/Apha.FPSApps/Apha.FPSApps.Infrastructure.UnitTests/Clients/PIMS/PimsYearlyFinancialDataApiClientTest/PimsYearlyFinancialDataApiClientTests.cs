using Apha.Common.Constants;
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
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsYearlyFinancialDataApiClientTest
{
    public class PimsYearlyFinancialDataApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsYearlyFinancialDataApiClient _client;

        public PimsYearlyFinancialDataApiClientTests()
        {
            _http = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsYearlyFinancialDataApiClient(_http, _mapper);
        }

        private static ApiResponse<T> SuccessApiResponse<T>(T data) =>
            new ApiResponse<T> { Success = true, Data = data };

        private static ApiResponse<T> FailureApiResponse<T>() =>
            new ApiResponse<T>
            {
                Success = false,
                Errors = new List<ApiError> { new ApiError { Code = "ERR", Message = "API error" } }
            };

        private static ApiResponseDto<T> SuccessDto<T>(T data) => ApiResponseDto<T>.SuccessResponse(data);

        private static ApiResponseDto<T> FailureDto<T>() =>
            ApiResponseDto<T>.FailureResponse(
                new List<ApiErrorDto> { new ApiErrorDto { Code = "ERR", Message = "Error" } },
                new ApiMetaDto());

        private static YearlyFinancialDataRes MakeRes(short year = 2024, string? project = "PRJ001") =>
            new YearlyFinancialDataRes { Year = year, Project = project, BfBudget = 1000m };

        private static YearlyFinancialDataDto MakeDto(short year = 2024, string? project = "PRJ001") =>
            new YearlyFinancialDataDto { Year = year, Project = project, BfBudget = 1000m };

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_WhenSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var yearlyDataList = new List<YearlyFinancialDataRes> { MakeRes(2024), MakeRes(2025) };
            var apiResponse = SuccessApiResponse(yearlyDataList);
            var mappedDto = SuccessDto(new List<YearlyFinancialDataDto> { MakeDto(2024), MakeDto(2025) });

            _http.GetAsync<List<YearlyFinancialDataRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<YearlyFinancialDataDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllAsync("PRJ001", query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _http.Received(1).GetAsync<List<YearlyFinancialDataRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetAllAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = FailureApiResponse<List<YearlyFinancialDataRes>>();
            var failDto = FailureDto<List<YearlyFinancialDataDto>>();

            _http.GetAsync<List<YearlyFinancialDataRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<YearlyFinancialDataDto>>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.GetAllAsync("PRJ001", query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetByKeyAsync

        [Fact]
        public async Task GetByKeyAsync_WhenSuccessResponse_ReturnsMappedObject()
        {
            // Arrange
            var yearlyDataRes = MakeRes(2024);
            var apiResponse = SuccessApiResponse(yearlyDataRes);
            var mappedDto = SuccessDto(MakeDto(2024));

            _http.GetAsync<YearlyFinancialDataRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetByKeyAsync(2024, "PRJ001");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal((short)2024, result.Data.Year);
            await _http.Received(1).GetAsync<YearlyFinancialDataRes>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetByKeyAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = FailureApiResponse<YearlyFinancialDataRes>();
            var failDto = FailureDto<YearlyFinancialDataDto>();

            _http.GetAsync<YearlyFinancialDataRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.GetByKeyAsync(2024, "PRJ001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_WhenSuccessResponse_ReturnsMappedObject()
        {
            // Arrange
            var dto = MakeDto(2024);
            var req = new YearlyFinancialDataReq { BfBudget = 1000m };
            var yearlyDataRes = MakeRes(2024);
            var apiResponse = SuccessApiResponse(yearlyDataRes);
            var mappedDto = SuccessDto(MakeDto(2024));

            _mapper.Map<YearlyFinancialDataReq>(dto).Returns(req);
            _http.PostAsync<YearlyFinancialDataReq, YearlyFinancialDataRes>(Arg.Any<string>(), Arg.Any<YearlyFinancialDataReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal((short)2024, result.Data.Year);
            await _http.Received(1).PostAsync<YearlyFinancialDataReq, YearlyFinancialDataRes>(Arg.Any<string>(), Arg.Any<YearlyFinancialDataReq>());
        }

        [Fact]
        public async Task CreateAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = MakeDto(2024);
            var req = new YearlyFinancialDataReq { BfBudget = 1000m };
            var apiResponse = FailureApiResponse<YearlyFinancialDataRes>();
            var failDto = FailureDto<YearlyFinancialDataDto>();

            _mapper.Map<YearlyFinancialDataReq>(dto).Returns(req);
            _http.PostAsync<YearlyFinancialDataReq, YearlyFinancialDataRes>(Arg.Any<string>(), Arg.Any<YearlyFinancialDataReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_WhenSuccessResponse_ReturnsMappedObject()
        {
            // Arrange
            var dto = MakeDto(2024);
            var req = new YearlyFinancialDataReq { BfBudget = 1500m };
            var yearlyDataRes = MakeRes(2024);
            var apiResponse = SuccessApiResponse(yearlyDataRes);
            var mappedDto = SuccessDto(MakeDto(2024));

            _mapper.Map<YearlyFinancialDataReq>(dto).Returns(req);
            _http.PutAsync<YearlyFinancialDataReq, YearlyFinancialDataRes>(Arg.Any<string>(), Arg.Any<YearlyFinancialDataReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateAsync(2024, "PRJ001", dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            await _http.Received(1).PutAsync<YearlyFinancialDataReq, YearlyFinancialDataRes>(Arg.Any<string>(), Arg.Any<YearlyFinancialDataReq>());
        }

        [Fact]
        public async Task UpdateAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = MakeDto(2024);
            var req = new YearlyFinancialDataReq { BfBudget = 1500m };
            var apiResponse = FailureApiResponse<YearlyFinancialDataRes>();
            var failDto = FailureDto<YearlyFinancialDataDto>();

            _mapper.Map<YearlyFinancialDataReq>(dto).Returns(req);
            _http.PutAsync<YearlyFinancialDataReq, YearlyFinancialDataRes>(Arg.Any<string>(), Arg.Any<YearlyFinancialDataReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.UpdateAsync(2024, "PRJ001", dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_WhenSuccessResponse_ReturnsSuccess()
        {
            // Arrange
            var apiResponse = SuccessApiResponse<object>(null!);
            var mappedDto = SuccessDto<object>(null!);

            _http.DeleteAsync<object>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.DeleteAsync(2024, "PRJ001");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).DeleteAsync<object>(Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = FailureApiResponse<object>();
            var failDto = FailureDto<object>();

            _http.DeleteAsync<object>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.DeleteAsync(2024, "PRJ001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetPactCostsAsync

        [Fact]
        public async Task GetPactCostsAsync_WhenSuccessResponse_ReturnsAggregatedCosts()
        {
            // Arrange
            var pactRes = new List<PactProjectYearCostsRes>
            {
                new PactProjectYearCostsRes { Project = "PRJ001", Year = 2024, Pay = 1000m, Tests = 500m, Animals = 300m }
            };
            var apiResponse = SuccessApiResponse(pactRes);
            var mappedDto = SuccessDto(new PactProjectYearCostsDto { Project = "PRJ001", Year = 2024, Pay = 1000m });

            _http.GetAsync<List<PactProjectYearCostsRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<PactProjectYearCostsDto>>(Arg.Any<ApiResponse<List<PactProjectYearCostsRes>>>()).Returns(mappedDto);

            // Act
            var result = await _client.GetPactCostsAsync("PRJ001", 2024);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            await _http.Received(1).GetAsync<List<PactProjectYearCostsRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetPactCostsAsync_WhenEmptyList_ReturnsEmptyAggregate()
        {
            // Arrange
            var pactRes = new List<PactProjectYearCostsRes>();
            var apiResponse = SuccessApiResponse(pactRes);

            _http.GetAsync<List<PactProjectYearCostsRes>>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetPactCostsAsync("PRJ001", 2024);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("PRJ001", result.Data.Project);
            Assert.Equal((short)2024, result.Data.Year);
        }

        #endregion

        #region GetSettingValueByIdAsync

        [Fact]
        public async Task GetSettingValueByIdAsync_WhenSuccessResponse_ReturnsValue()
        {
            // Arrange
            var settingValue = "TestValue";
            var apiResponse = SuccessApiResponse(settingValue);
            var mappedDto = SuccessDto(settingValue);

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<string>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetSettingValueByIdAsync("SettingId");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(settingValue, result.Data);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = FailureApiResponse<string>();
            var failDto = FailureDto<string>();

            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<string>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.GetSettingValueByIdAsync("SettingId");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion
    }
}
