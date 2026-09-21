using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients;
using MapsterMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsPublicationTypeApiClientTest
{
    public class PimsPublicationTypeApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsPublicationTypeApiClient _client;

        public PimsPublicationTypeApiClientTests()
        {
            _http = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsPublicationTypeApiClient(_http, _mapper);
        }

        [Fact]
        public async Task GetAllPublicationTypesAsync_WithSuccessResponse_ReturnsMappedResult()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<PublicationTypeRes>>
            {
                Success = true,
                Data =
                [
                    new PublicationTypeRes { Type = "JOU", Description = "Journal" }
                ]
            };
            var mappedDto = ApiResponseDto<List<PublicationTypeDto>>.SuccessResponse(
            [
                new PublicationTypeDto { Type = "JOU", Description = "Journal" }
            ]);

            _http.GetAsync<List<PublicationTypeRes>>(PimsApiEndpoints.GetAllPublicationTypes).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<PublicationTypeDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllPublicationTypesAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal("JOU", result.Data[0].Type);
            await _http.Received(1).GetAsync<List<PublicationTypeRes>>(PimsApiEndpoints.GetAllPublicationTypes);
            _mapper.Received(1).Map<ApiResponseDto<List<PublicationTypeDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllPublicationTypesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<PublicationTypeRes>>
            {
                Success = false,
                Errors = [new ApiError { Message = "Not found", Code = "NOT_FOUND" }]
            };
            var mappedDto = new ApiResponseDto<List<PublicationTypeDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<PublicationTypeRes>>(PimsApiEndpoints.GetAllPublicationTypes).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<PublicationTypeDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllPublicationTypesAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Not found", result.Errors[0].Message);
        }

        [Fact]
        public async Task GetAllPublicationTypesAsync_WhenHttpThrowsException_PropagatesException()
        {
            // Arrange
            _http.GetAsync<List<PublicationTypeRes>>(PimsApiEndpoints.GetAllPublicationTypes)
                .ThrowsAsync(new Exception("Network error"));

            // Act / Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _client.GetAllPublicationTypesAsync());
            Assert.Equal("Network error", ex.Message);
        }

        [Fact]
        public async Task GetPublicationTypeByCodeAsync_WhenMapperThrowsException_PropagatesException()
        {
            // Arrange
            var type = "JOU";
            var url = string.Format(PimsApiEndpoints.GetPublicationTypeByCode, Uri.EscapeDataString(type));
            var apiResponse = new ApiResponse<PublicationTypeRes>
            {
                Success = true,
                Data = new PublicationTypeRes { Type = type, Description = "Journal" }
            };

            _http.GetAsync<PublicationTypeRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<PublicationTypeDto>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act / Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _client.GetPublicationTypeByCodeAsync(type));
        }

        [Fact]
        public async Task CreatePublicationTypeAsync_WhenMapperThrowsOnRequest_PropagatesException()
        {
            // Arrange
            var dto = new PublicationTypeDto { Type = "JOU", Description = "Journal" };
            _mapper.Map<PublicationTypeReq>(dto).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act / Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _client.CreatePublicationTypeAsync(dto));
        }

        [Fact]
        public async Task DeletePublicationTypeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var type = "JOU";
            var url = string.Format(PimsApiEndpoints.DeletePublicationType, Uri.EscapeDataString(type));
            var apiResponse = new ApiResponse<bool>
            {
                Success = false,
                Errors = [new ApiError { Message = "Delete failed", Code = "DELETE_FAILED" }]
            };
            var mappedDto = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Delete failed", Code = "DELETE_FAILED" }],
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.DeletePublicationTypeAsync(type);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("DELETE_FAILED", result.Errors[0].Code);
        }
    }
}
