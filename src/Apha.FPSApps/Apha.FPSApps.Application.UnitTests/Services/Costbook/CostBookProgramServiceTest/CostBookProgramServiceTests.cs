using System;
using System.Collections.Generic;
using System.Text;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Services.Costbook;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.Costbook.CostBookProgramServiceTest
{
    public class CostBookProgramServiceTests
    {
        private readonly ICostBookApiClient _costBookClient;
        private readonly ICostBookProgramApiClient _costBookProgramApiClient;
        private readonly CostBookProgramService _programService;

        public CostBookProgramServiceTests()
        {
            _costBookClient = Substitute.For<ICostBookApiClient>();
            _costBookProgramApiClient = Substitute.For<ICostBookProgramApiClient>();
            _costBookClient.Programs.Returns(_costBookProgramApiClient);
            _programService = new CostBookProgramService(_costBookClient);
        }

        #region GetAllProgramsAsync Tests

        [Fact]
        public async Task GetAllProgramsAsync_WithSuccessResponse_ReturnsProgramList()
        {
            // Arrange
            var programs = new List<ProgramDto>
            {
                new ProgramDto { ProgramNo = "PR001", ProgramName = "Program 1" },
                new ProgramDto { ProgramNo = "PR002", ProgramName = "Program 2" }
            };
            var expectedResponse = ApiResponseDto<List<ProgramDto>>.SuccessResponse(programs);

            _costBookProgramApiClient.GetAllProgramsAsync().Returns(expectedResponse);

            // Act
            var result = await _programService.GetAllProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _costBookProgramApiClient.Received(1).GetAllProgramsAsync();
        }

        [Fact]
        public async Task GetAllProgramsAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<ProgramDto>>.SuccessResponse(new List<ProgramDto>());

            _costBookProgramApiClient.GetAllProgramsAsync().Returns(expectedResponse);

            // Act
            var result = await _programService.GetAllProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllProgramsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<ProgramDto>>.FailureResponse(errors, new ApiMetaDto());

            _costBookProgramApiClient.GetAllProgramsAsync().Returns(expectedResponse);

            // Act
            var result = await _programService.GetAllProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidClient_InitializesService()
        {
            // Arrange & Act
            var service = new CostBookProgramService(_costBookClient);

            // Assert
            Assert.NotNull(service);
        }

        #endregion
    }
}
