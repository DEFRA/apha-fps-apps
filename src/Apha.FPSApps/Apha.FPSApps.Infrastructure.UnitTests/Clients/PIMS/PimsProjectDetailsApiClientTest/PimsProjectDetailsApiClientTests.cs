using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsProjectDetailsApiClientTest
{
    public class PimsProjectDetailsApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsProjectDetailsApiClient _client;

        public PimsProjectDetailsApiClientTests()
        {
            _http = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsProjectDetailsApiClient(_http, _mapper);
        }

        #region GetPimsDetailAsync Tests

        [Fact]
        public async Task GetPimsDetailAsync_WithSuccessResponse_ReturnsMappedPimsDetail()
        {
            // Arrange
            var parentproject = "PP001";
            var url = string.Format(PimsApiEndpoints.GetPimsDetail, parentproject);
            var projectDetailRes = new ProjectDetailRes { Parentproject = parentproject, Version = "1.0", Riskid = 1 };
            var apiResponse = new ApiResponse<ProjectDetailRes> { Success = true, Data = projectDetailRes };
            var mappedDto = ApiResponseDto<ProjectDetailDto>.SuccessResponse(
                new ProjectDetailDto { Parentproject = parentproject, Version = "1.0", Riskid = 1 }
            );

            _http.GetAsync<ProjectDetailRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDetailDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetPimsDetailAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(parentproject, result.Data.Parentproject);
            await _http.Received(1).GetAsync<ProjectDetailRes>(url);
            _mapper.Received(1).Map<ApiResponseDto<ProjectDetailDto>>(apiResponse);
        }

        [Fact]
        public async Task GetPimsDetailAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var parentproject = "INVALID";
            var url = string.Format(PimsApiEndpoints.GetPimsDetail, parentproject);
            var errors = new List<ApiError>
            {
                new ApiError { Message = "PIMS detail not found", Code = "NOT_FOUND" }
            };
            var apiResponse = new ApiResponse<ProjectDetailRes> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<ProjectDetailDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "PIMS detail not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<ProjectDetailRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDetailDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetPimsDetailAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("PIMS detail not found", result.Errors[0].Message);
            await _http.Received(1).GetAsync<ProjectDetailRes>(url);
            _mapper.Received(1).Map<ApiResponseDto<ProjectDetailDto>>(apiResponse);
        }

        [Fact]
        public async Task GetPimsDetailAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var parentproject = "PP001";
            var url = string.Format(PimsApiEndpoints.GetPimsDetail, parentproject);
            _http.GetAsync<ProjectDetailRes>(url).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetPimsDetailAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve PIMS detail", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetPimsDetailAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var parentproject = "PP001";
            var url = string.Format(PimsApiEndpoints.GetPimsDetail, parentproject);
            var apiResponse = new ApiResponse<ProjectDetailRes> { Success = true, Data = new ProjectDetailRes { Parentproject = parentproject } };

            _http.GetAsync<ProjectDetailRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDetailDto>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetPimsDetailAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve PIMS detail", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetPimsDetailAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var parentproject = "PP123";
            var expectedUrl = string.Format(PimsApiEndpoints.GetPimsDetail, parentproject);
            var apiResponse = new ApiResponse<ProjectDetailRes> { Success = true, Data = new ProjectDetailRes { Parentproject = parentproject } };
            var mappedDto = ApiResponseDto<ProjectDetailDto>.SuccessResponse(new ProjectDetailDto { Parentproject = parentproject });

            _http.GetAsync<ProjectDetailRes>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDetailDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetPimsDetailAsync(parentproject);

            // Assert
            await _http.Received(1).GetAsync<ProjectDetailRes>(Arg.Is<string>(s => s == expectedUrl));
        }

        #endregion

        #region SavePimsDetailAsync Tests

        [Fact]
        public async Task SavePimsDetailAsync_WithSuccessResponse_ReturnsMappedPimsDetail()
        {
            // Arrange
            var parentproject = "PP001";
            var dto = new ProjectDetailDto { Parentproject = parentproject, Version = "1.0", Riskid = 2 };
            var request = new ProjectDetailReq { Parentproject = parentproject, Version = "1.0", Riskid = 2 };
            var projectDetailRes = new ProjectDetailRes { Parentproject = parentproject, Version = "1.0", Riskid = 2 };
            var apiResponse = new ApiResponse<ProjectDetailRes> { Success = true, Data = projectDetailRes };
            var mappedDto = ApiResponseDto<ProjectDetailDto>.SuccessResponse(
                new ProjectDetailDto { Parentproject = parentproject, Version = "1.0", Riskid = 2 }
            );

            _mapper.Map<ProjectDetailReq>(dto).Returns(request);
            _http.PostAsync<ProjectDetailReq, ProjectDetailRes>(string.Format(PimsApiEndpoints.SavePimsDetail, parentproject), request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDetailDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.SavePimsDetailAsync(parentproject, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(parentproject, result.Data.Parentproject);
            _mapper.Received(1).Map<ProjectDetailReq>(dto);
            await _http.Received(1).PostAsync<ProjectDetailReq, ProjectDetailRes>(string.Format(PimsApiEndpoints.SavePimsDetail, parentproject), request);
            _mapper.Received(1).Map<ApiResponseDto<ProjectDetailDto>>(apiResponse);
        }

        [Fact]
        public async Task SavePimsDetailAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var parentproject = "PP001";
            var dto = new ProjectDetailDto { Parentproject = parentproject };
            var request = new ProjectDetailReq { Parentproject = parentproject };
            var errors = new List<ApiError>
            {
                new ApiError { Message = "Validation error", Code = "VALIDATION_ERROR" }
            };
            var apiResponse = new ApiResponse<ProjectDetailRes> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<ProjectDetailDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Validation error", Code = "VALIDATION_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ProjectDetailReq>(dto).Returns(request);
            _http.PostAsync<ProjectDetailReq, ProjectDetailRes>(string.Format(PimsApiEndpoints.SavePimsDetail, parentproject), request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDetailDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.SavePimsDetailAsync(parentproject, dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Validation error", result.Errors[0].Message);
            await _http.Received(1).PostAsync<ProjectDetailReq, ProjectDetailRes>(string.Format(PimsApiEndpoints.SavePimsDetail, parentproject), request);
        }

        [Fact]
        public async Task SavePimsDetailAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var parentproject = "PP001";
            var dto = new ProjectDetailDto { Parentproject = parentproject };
            var request = new ProjectDetailReq { Parentproject = parentproject };

            _mapper.Map<ProjectDetailReq>(dto).Returns(request);
            _http.PostAsync<ProjectDetailReq, ProjectDetailRes>(string.Format(PimsApiEndpoints.SavePimsDetail, parentproject), request)
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.SavePimsDetailAsync(parentproject, dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to save PIMS detail", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task SavePimsDetailAsync_WhenMapperThrowsExceptionOnRequestMapping_ReturnsInternalError()
        {
            // Arrange
            var parentproject = "PP001";
            var dto = new ProjectDetailDto { Parentproject = parentproject };
            _mapper.Map<ProjectDetailReq>(dto).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.SavePimsDetailAsync(parentproject, dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to save PIMS detail", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task SavePimsDetailAsync_EnsuresCorrectApiEndpoint_CallsPostWithCorrectUrl()
        {
            // Arrange
            var parentproject = "PP001";
            var expectedUrl = string.Format(PimsApiEndpoints.SavePimsDetail, parentproject);
            var dto = new ProjectDetailDto { Parentproject = parentproject };
            var request = new ProjectDetailReq { Parentproject = parentproject };
            var apiResponse = new ApiResponse<ProjectDetailRes> { Success = true, Data = new ProjectDetailRes { Parentproject = parentproject } };
            var mappedDto = ApiResponseDto<ProjectDetailDto>.SuccessResponse(new ProjectDetailDto { Parentproject = parentproject });

            _mapper.Map<ProjectDetailReq>(dto).Returns(request);
            _http.PostAsync<ProjectDetailReq, ProjectDetailRes>(expectedUrl, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDetailDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.SavePimsDetailAsync(parentproject, dto);

            // Assert
            await _http.Received(1).PostAsync<ProjectDetailReq, ProjectDetailRes>(
                Arg.Is<string>(s => s == expectedUrl),
                Arg.Any<ProjectDetailReq>()
            );
        }

        #endregion

        #region GetProposedProjectAsync Tests

        [Fact]
        public async Task GetProposedProjectAsync_WithSuccessResponse_ReturnsMappedProposedProject()
        {
            // Arrange
            var parentproject = "PP001";
            var url = string.Format(PimsApiEndpoints.GetProposedProject, parentproject);
            var proposedProjectRes = new ProposedProjectRes { Id = 1, Parentproject = parentproject, Projecttitle = "Test Proposed Project" };
            var apiResponse = new ApiResponse<ProposedProjectRes> { Success = true, Data = proposedProjectRes };
            var mappedDto = ApiResponseDto<ProposedProjectDto>.SuccessResponse(
                new ProposedProjectDto { Id = 1, Parentproject = parentproject, Projecttitle = "Test Proposed Project" }
            );

            _http.GetAsync<ProposedProjectRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProposedProjectDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetProposedProjectAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(parentproject, result.Data.Parentproject);
            await _http.Received(1).GetAsync<ProposedProjectRes>(url);
            _mapper.Received(1).Map<ApiResponseDto<ProposedProjectDto>>(apiResponse);
        }

        [Fact]
        public async Task GetProposedProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var parentproject = "INVALID";
            var url = string.Format(PimsApiEndpoints.GetProposedProject, parentproject);
            var errors = new List<ApiError>
            {
                new ApiError { Message = "Proposed project not found", Code = "NOT_FOUND" }
            };
            var apiResponse = new ApiResponse<ProposedProjectRes> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<ProposedProjectDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Proposed project not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<ProposedProjectRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProposedProjectDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetProposedProjectAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Proposed project not found", result.Errors[0].Message);
            await _http.Received(1).GetAsync<ProposedProjectRes>(url);
            _mapper.Received(1).Map<ApiResponseDto<ProposedProjectDto>>(apiResponse);
        }

        [Fact]
        public async Task GetProposedProjectAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var parentproject = "PP001";
            var url = string.Format(PimsApiEndpoints.GetProposedProject, parentproject);
            _http.GetAsync<ProposedProjectRes>(url).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetProposedProjectAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve proposed project", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetProposedProjectAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var parentproject = "PP001";
            var url = string.Format(PimsApiEndpoints.GetProposedProject, parentproject);
            var apiResponse = new ApiResponse<ProposedProjectRes> { Success = true, Data = new ProposedProjectRes { Parentproject = parentproject } };

            _http.GetAsync<ProposedProjectRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProposedProjectDto>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetProposedProjectAsync(parentproject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve proposed project", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetProposedProjectAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var parentproject = "PP123";
            var expectedUrl = string.Format(PimsApiEndpoints.GetProposedProject, parentproject);
            var apiResponse = new ApiResponse<ProposedProjectRes> { Success = true, Data = new ProposedProjectRes { Parentproject = parentproject } };
            var mappedDto = ApiResponseDto<ProposedProjectDto>.SuccessResponse(new ProposedProjectDto { Parentproject = parentproject });

            _http.GetAsync<ProposedProjectRes>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProposedProjectDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetProposedProjectAsync(parentproject);

            // Assert
            await _http.Received(1).GetAsync<ProposedProjectRes>(Arg.Is<string>(s => s == expectedUrl));
        }

        #endregion

        #region UpdateProposedProjectAsync Tests

        [Fact]
        public async Task UpdateProposedProjectAsync_WithSuccessResponse_ReturnsMappedProposedProject()
        {
            // Arrange
            var parentproject = "PP001";
            var dto = new ProposedProjectDto { Id = 1, Parentproject = parentproject, Projecttitle = "Updated Project" };
            var request = new ProposedProjectReq { Parentproject = parentproject, Projecttitle = "Updated Project" };
            var proposedProjectRes = new ProposedProjectRes { Id = 1, Parentproject = parentproject, Projecttitle = "Updated Project" };
            var apiResponse = new ApiResponse<ProposedProjectRes> { Success = true, Data = proposedProjectRes };
            var mappedDto = ApiResponseDto<ProposedProjectDto>.SuccessResponse(
                new ProposedProjectDto { Id = 1, Parentproject = parentproject, Projecttitle = "Updated Project" }
            );

            _mapper.Map<ProposedProjectReq>(dto).Returns(request);
            _http.PutAsync<ProposedProjectReq, ProposedProjectRes>(string.Format(PimsApiEndpoints.UpdateProposedProject, parentproject), request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProposedProjectDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateProposedProjectAsync(parentproject, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(parentproject, result.Data.Parentproject);
            _mapper.Received(1).Map<ProposedProjectReq>(dto);
            await _http.Received(1).PutAsync<ProposedProjectReq, ProposedProjectRes>(string.Format(PimsApiEndpoints.UpdateProposedProject, parentproject), request);
            _mapper.Received(1).Map<ApiResponseDto<ProposedProjectDto>>(apiResponse);
        }

        [Fact]
        public async Task UpdateProposedProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var parentproject = "PP001";
            var dto = new ProposedProjectDto { Parentproject = parentproject, Projecttitle = "Updated Project" };
            var request = new ProposedProjectReq { Parentproject = parentproject, Projecttitle = "Updated Project" };
            var errors = new List<ApiError>
            {
                new ApiError { Message = "Validation error", Code = "VALIDATION_ERROR" }
            };
            var apiResponse = new ApiResponse<ProposedProjectRes> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<ProposedProjectDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Validation error", Code = "VALIDATION_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ProposedProjectReq>(dto).Returns(request);
            _http.PutAsync<ProposedProjectReq, ProposedProjectRes>(string.Format(PimsApiEndpoints.UpdateProposedProject, parentproject), request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProposedProjectDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateProposedProjectAsync(parentproject, dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Validation error", result.Errors[0].Message);
            await _http.Received(1).PutAsync<ProposedProjectReq, ProposedProjectRes>(string.Format(PimsApiEndpoints.UpdateProposedProject, parentproject), request);
        }

        [Fact]
        public async Task UpdateProposedProjectAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var parentproject = "PP001";
            var dto = new ProposedProjectDto { Parentproject = parentproject };
            var request = new ProposedProjectReq { Parentproject = parentproject };

            _mapper.Map<ProposedProjectReq>(dto).Returns(request);
            _http.PutAsync<ProposedProjectReq, ProposedProjectRes>(string.Format(PimsApiEndpoints.UpdateProposedProject, parentproject), request)
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.UpdateProposedProjectAsync(parentproject, dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to update proposed project", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task UpdateProposedProjectAsync_WhenMapperThrowsExceptionOnRequestMapping_ReturnsInternalError()
        {
            // Arrange
            var parentproject = "PP001";
            var dto = new ProposedProjectDto { Parentproject = parentproject };
            _mapper.Map<ProposedProjectReq>(dto).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.UpdateProposedProjectAsync(parentproject, dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to update proposed project", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task UpdateProposedProjectAsync_EnsuresCorrectApiEndpoint_CallsPutWithCorrectUrl()
        {
            // Arrange
            var parentproject = "PP123";
            var expectedUrl = string.Format(PimsApiEndpoints.UpdateProposedProject, parentproject);
            var dto = new ProposedProjectDto { Parentproject = parentproject };
            var request = new ProposedProjectReq { Parentproject = parentproject };
            var apiResponse = new ApiResponse<ProposedProjectRes> { Success = true, Data = new ProposedProjectRes { Parentproject = parentproject } };
            var mappedDto = ApiResponseDto<ProposedProjectDto>.SuccessResponse(new ProposedProjectDto { Parentproject = parentproject });

            _mapper.Map<ProposedProjectReq>(dto).Returns(request);
            _http.PutAsync<ProposedProjectReq, ProposedProjectRes>(expectedUrl, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProposedProjectDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.UpdateProposedProjectAsync(parentproject, dto);

            // Assert
            await _http.Received(1).PutAsync<ProposedProjectReq, ProposedProjectRes>(
                Arg.Is<string>(s => s == expectedUrl),
                Arg.Any<ProposedProjectReq>()
            );
        }

        #endregion

        #region GetAllRiskAsync Tests

        [Fact]
        public async Task GetAllRiskAsync_WithSuccessResponseAndData_ReturnsMappedRiskList()
        {
            // Arrange
            var riskResList = new List<RiskRes>
            {
                new RiskRes { Riskid = 1, Riskrating = "Low" },
                new RiskRes { Riskid = 2, Riskrating = "High" }
            };
            var apiResponse = new ApiResponse<List<RiskRes>> { Success = true, Data = riskResList };
            var mappedDto = ApiResponseDto<List<RiskDto>>.SuccessResponse(new List<RiskDto>
            {
                new RiskDto { Riskid = 1, Riskrating = "Low" },
                new RiskDto { Riskid = 2, Riskrating = "High" }
            });

            _http.GetAsync<List<RiskRes>>(PimsApiEndpoints.GetAllRisks).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<RiskDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllRiskAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _http.Received(1).GetAsync<List<RiskRes>>(PimsApiEndpoints.GetAllRisks);
            _mapper.Received(1).Map<ApiResponseDto<List<RiskDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllRiskAsync_WithSuccessResponseButNullData_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<RiskRes>> { Success = true, Data = null };
            var mappedDto = new ApiResponseDto<List<RiskDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "No data", Code = "NO_DATA" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<RiskRes>>(PimsApiEndpoints.GetAllRisks).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<RiskDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllRiskAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.NotNull(result.Meta);
            await _http.Received(1).GetAsync<List<RiskRes>>(PimsApiEndpoints.GetAllRisks);
            _mapper.Received(1).Map<ApiResponseDto<List<RiskDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllRiskAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError>
            {
                new ApiError { Message = "Risk ratings not found", Code = "NOT_FOUND" }
            };
            var apiResponse = new ApiResponse<List<RiskRes>> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<List<RiskDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Risk ratings not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<RiskRes>>(PimsApiEndpoints.GetAllRisks).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<RiskDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllRiskAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Risk ratings not found", result.Errors[0].Message);
            await _http.Received(1).GetAsync<List<RiskRes>>(PimsApiEndpoints.GetAllRisks);
            _mapper.Received(1).Map<ApiResponseDto<List<RiskDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllRiskAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<List<RiskRes>>(PimsApiEndpoints.GetAllRisks).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAllRiskAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve risk ratings", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetAllRiskAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<RiskRes>>
            {
                Success = true,
                Data = new List<RiskRes> { new RiskRes { Riskid = 1, Riskrating = "Low" } }
            };

            _http.GetAsync<List<RiskRes>>(PimsApiEndpoints.GetAllRisks).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<RiskDto>>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetAllRiskAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve risk ratings", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetAllRiskAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<RiskRes>> { Success = true, Data = new List<RiskRes>() };
            var mappedDto = ApiResponseDto<List<RiskDto>>.SuccessResponse(new List<RiskDto>());

            _http.GetAsync<List<RiskRes>>(PimsApiEndpoints.GetAllRisks).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<RiskDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetAllRiskAsync();

            // Assert
            await _http.Received(1).GetAsync<List<RiskRes>>(
                Arg.Is<string>(s => s == PimsApiEndpoints.GetAllRisks)
            );
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_InitializesClient()
        {
            // Arrange & Act
            var client = new PimsProjectDetailsApiClient(_http, _mapper);

            // Assert
            Assert.NotNull(client);
        }

        #endregion
    }
}
