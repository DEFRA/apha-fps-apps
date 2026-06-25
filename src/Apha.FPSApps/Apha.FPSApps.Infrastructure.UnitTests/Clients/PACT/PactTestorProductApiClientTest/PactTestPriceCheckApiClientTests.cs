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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactTestListApiClientTest
{
    public class PactTestPriceCheckApiClientTests
    {
        private readonly IPactHttpExecutor _httpExecutor;
        private readonly IMapper _mapper;
        private readonly PactTestorProductApiClient _client;

        public PactTestPriceCheckApiClientTests()
        {
            _httpExecutor = Substitute.For<IPactHttpExecutor>();
            _mapper       = Substitute.For<IMapper>();
            SetupMapper();
            _client = new PactTestorProductApiClient(_httpExecutor, _mapper);
        }

        private void SetupMapper()
        {
            _mapper.Map<ApiResponseDto<TestPriceCheckDto>>(Arg.Any<ApiResponse<TestPriceCheckRes>>())
                .Returns(callInfo =>
                {
                    var r = callInfo.ArgAt<ApiResponse<TestPriceCheckRes>>(0);
                    if (r == null || !r.Success || r.Data == null)
                        return ApiResponseDto<TestPriceCheckDto>.FailureResponse(
                            r?.Errors?.Select(e => new ApiErrorDto { Message = e.Message, Code = e.Code }).ToList() ?? [],
                            new ApiMetaDto());
                    return ApiResponseDto<TestPriceCheckDto>.SuccessResponse(new TestPriceCheckDto
                    {
                        TestCode       = r.Data.TestCode,
                        JobCode        = r.Data.JobCode,
                        TestPrice      = r.Data.TestPrice,
                        UnitPriceVla   = r.Data.UnitPriceVla,
                        DefraUnitPrice = r.Data.DefraUnitPrice,
                        IsDefraProject = r.Data.IsDefraProject,
                        NormalPrice    = r.Data.NormalPrice,
                        IsZeroPrice    = r.Data.IsZeroPrice,
                        IsNotStandard  = r.Data.IsNotStandard,
                        Manager        = r.Data.Manager,
                        Program        = r.Data.Program,
                        Owner          = r.Data.Owner
                    });
                });

            _mapper.Map<ApiResponseDto<List<TestPriceCheckDto>>>(Arg.Any<ApiResponse<List<TestPriceCheckRes>>>())
                .Returns(callInfo =>
                {
                    var r = callInfo.ArgAt<ApiResponse<List<TestPriceCheckRes>>>(0);
                    if (r == null || !r.Success || r.Data == null)
                        return ApiResponseDto<List<TestPriceCheckDto>>.FailureResponse(
                            r?.Errors?.Select(e => new ApiErrorDto { Message = e.Message, Code = e.Code }).ToList() ?? [],
                            new ApiMetaDto());
                    var dtos = r.Data.Select(d => new TestPriceCheckDto
                    {
                        TestCode  = d.TestCode,
                        JobCode   = d.JobCode,
                        TestPrice = d.TestPrice,
                        Owner     = d.Owner
                    }).ToList();
                    return ApiResponseDto<List<TestPriceCheckDto>>.SuccessResponse(dtos);
                });

            _mapper.Map<ApiResponseDto<bool>>(Arg.Any<ApiResponse<bool>>())
                .Returns(callInfo =>
                {
                    var r = callInfo.ArgAt<ApiResponse<bool>>(0);
                    if (r == null || !r.Success)
                        return ApiResponseDto<bool>.FailureResponse(
                            r?.Errors?.Select(e => new ApiErrorDto { Message = e.Message, Code = e.Code }).ToList() ?? [],
                            new ApiMetaDto());
                    return ApiResponseDto<bool>.SuccessResponse(r.Data);
                });

            _mapper.Map<TestPriceCheckReq>(Arg.Any<TestPriceCheckDto>())
                .Returns(callInfo =>
                {
                    var dto = callInfo.ArgAt<TestPriceCheckDto>(0);
                    return new TestPriceCheckReq
                    {
                        IsDefraProject = dto.IsDefraProject,
                        TestPrice      = dto.TestPrice,
                        DefraUnitPrice = dto.DefraUnitPrice
                    };
                });
        }

        #region GetTestPriceCheckPagedAsync

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_Success_ReturnsMappedDtos()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList = new List<TestPriceCheckRes>
            {
                new() { TestCode = "T001", JobCode = "JOB001", TestPrice = 50m,  Owner = "AB" },
                new() { TestCode = "T002", JobCode = "JOB002", TestPrice = 0m,   Owner = "CD" }
            };
            var httpResponse = new ApiResponse<List<TestPriceCheckRes>> { Success = true, Data = resList };
            _httpExecutor.GetAsync<List<TestPriceCheckRes>>(Arg.Any<string>()).Returns(httpResponse);

            var result = await _client.GetTestPriceCheckPagedAsync(query, "all", null);

            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            Assert.Equal("T001", result.Data[0].TestCode);
        }

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_WithOwner_AppendsOwnerToUrl()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var httpResponse = new ApiResponse<List<TestPriceCheckRes>> { Success = true, Data = [] };
            _httpExecutor.GetAsync<List<TestPriceCheckRes>>(Arg.Any<string>()).Returns(httpResponse);

            await _client.GetTestPriceCheckPagedAsync(query, "all", "AB");

            await _httpExecutor.Received(1)
                .GetAsync<List<TestPriceCheckRes>>(Arg.Is<string>(url => url.Contains("owner=AB")));
        }

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_HttpFails_ReturnsFailureResponse()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var httpResponse = new ApiResponse<List<TestPriceCheckRes>>
            {
                Success = false,
                Errors  = [new ApiError { Message = "HTTP error", Code = "ERR" }]
            };
            _httpExecutor.GetAsync<List<TestPriceCheckRes>>(Arg.Any<string>()).Returns(httpResponse);

            var result = await _client.GetTestPriceCheckPagedAsync(query, "all", null);

            Assert.False(result.Success);
        }

        #endregion

        #region GetTestPriceCheckByKeyAsync

        [Fact]
        public async Task GetTestPriceCheckByKeyAsync_ExistingKey_ReturnsMappedDto()
        {
            var res = new TestPriceCheckRes
            {
                TestCode = "T001", JobCode = "JOB001", TestPrice = 50m,
                IsDefraProject = 0, UnitPriceVla = 50m, NormalPrice = 50m
            };
            var httpResponse = new ApiResponse<TestPriceCheckRes> { Success = true, Data = res };
            _httpExecutor.GetAsync<TestPriceCheckRes>(Arg.Any<string>()).Returns(httpResponse);

            var result = await _client.GetTestPriceCheckByKeyAsync("T001", "JOB001");

            Assert.True(result.Success);
            Assert.Equal("T001",   result.Data!.TestCode);
            Assert.Equal("JOB001", result.Data.JobCode);
        }

        [Fact]
        public async Task GetTestPriceCheckByKeyAsync_EncodesTestCodeAndJobCodeInUrl()
        {
            var httpResponse = new ApiResponse<TestPriceCheckRes>
            {
                Success = true,
                Data    = new TestPriceCheckRes { TestCode = "T 01", JobCode = "JOB/001" }
            };
            _httpExecutor.GetAsync<TestPriceCheckRes>(Arg.Any<string>()).Returns(httpResponse);

            await _client.GetTestPriceCheckByKeyAsync("T 01", "JOB/001");

            await _httpExecutor.Received(1)
                .GetAsync<TestPriceCheckRes>(Arg.Is<string>(url =>
                    url.Contains("T%2001") && url.Contains("JOB%2F001")));
        }

        [Fact]
        public async Task GetTestPriceCheckByKeyAsync_HttpFails_ReturnsFailureResponse()
        {
            var httpResponse = new ApiResponse<TestPriceCheckRes>
            {
                Success = false,
                Errors  = [new ApiError { Message = "Not found", Code = "NOT_FOUND" }]
            };
            _httpExecutor.GetAsync<TestPriceCheckRes>(Arg.Any<string>()).Returns(httpResponse);

            var result = await _client.GetTestPriceCheckByKeyAsync("MISSING", "MISSING");

            Assert.False(result.Success);
        }

        #endregion

        #region UpdateTestPriceCheckByKeyAsync

        [Fact]
        public async Task UpdateTestPriceCheckByKeyAsync_Success_ReturnsTrueResponse()
        {
            var dto = new TestPriceCheckDto { IsDefraProject = -1, TestPrice = 75m, DefraUnitPrice = 120m };
            var httpResponse = new ApiResponse<bool> { Success = true, Data = true };
            _httpExecutor.PutAsync<TestPriceCheckReq, bool>(Arg.Any<string>(), Arg.Any<TestPriceCheckReq>())
                .Returns(httpResponse);

            var result = await _client.UpdateTestPriceCheckByKeyAsync("T001", "JOB001", dto);

            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task UpdateTestPriceCheckByKeyAsync_MapsToReqAndCallsPut()
        {
            var dto = new TestPriceCheckDto { IsDefraProject = 0, TestPrice = 50m, DefraUnitPrice = 80m };
            var httpResponse = new ApiResponse<bool> { Success = true, Data = true };
            _httpExecutor.PutAsync<TestPriceCheckReq, bool>(Arg.Any<string>(), Arg.Any<TestPriceCheckReq>())
                .Returns(httpResponse);

            await _client.UpdateTestPriceCheckByKeyAsync("T001", "JOB001", dto);

            await _httpExecutor.Received(1)
                .PutAsync<TestPriceCheckReq, bool>(
                    Arg.Is<string>(url => url.Contains("T001") && url.Contains("JOB001")),
                    Arg.Is<TestPriceCheckReq>(r =>
                        r.IsDefraProject == 0 &&
                        r.TestPrice      == 50m &&
                        r.DefraUnitPrice == 80m));
        }

        [Fact]
        public async Task UpdateTestPriceCheckByKeyAsync_HttpFails_ReturnsFailureResponse()
        {
            var dto = new TestPriceCheckDto { IsDefraProject = 0, TestPrice = 50m, DefraUnitPrice = 80m };
            var httpResponse = new ApiResponse<bool>
            {
                Success = false,
                Errors  = [new ApiError { Message = "Update failed", Code = "ERR" }]
            };
            _httpExecutor.PutAsync<TestPriceCheckReq, bool>(Arg.Any<string>(), Arg.Any<TestPriceCheckReq>())
                .Returns(httpResponse);

            var result = await _client.UpdateTestPriceCheckByKeyAsync("T001", "JOB001", dto);

            Assert.False(result.Success);
        }

        #endregion
    }
}
