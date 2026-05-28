using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Services.PIMS;
using NSubstitute;

namespace Apha.FPSApps.Application.UnitTests.Services.PIMS.ProposedProjectServiceTest
{
    public class ProposedProjectServiceTests
    {
        private readonly IPimsApiClient _pimsApiClient;
        private readonly IPimsProposedProjectApiClient _pimsProposedProjectApiClient;
        private readonly ProposedProjectService _sut;

        public ProposedProjectServiceTests()
        {
            _pimsApiClient = Substitute.For<IPimsApiClient>();
            _pimsProposedProjectApiClient = Substitute.For<IPimsProposedProjectApiClient>();
            _pimsApiClient.PimsProposedProject.Returns(_pimsProposedProjectApiClient);
            _sut = new ProposedProjectService(_pimsApiClient);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidClient_InitializesService()
        {
            var service = new ProposedProjectService(_pimsApiClient);
            Assert.NotNull(service);
        }

        #endregion

        #region CreateProposedProjectAsync Tests

        [Fact]
        public async Task CreateProjectAsync_WithValidDto_ReturnsSuccessResponse()
        {
            // Arrange
            var dto = new ProposedProjectDto
            {
                Parentproject = "PP001",
                Projecttitle = "New Project",
                Program = "PROG1",
                Customer = "CUST1",
                Projectstatus = "Proposed"
            };
            var expected = new ApiResponseDto<ProposedProjectDto> { Success = true, Data = dto };
            _pimsProposedProjectApiClient.CreateProposedProjectAsync(dto).Returns(expected);

            // Act
            var result = await _sut.CreateProposedProjectAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("PP001", result.Data?.Parentproject);
            await _pimsProposedProjectApiClient.Received(1).CreateProposedProjectAsync(dto);
        }

        [Fact]
        public async Task CreateProjectAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new ProposedProjectDto { Parentproject = "PP001" };
            var expected = new ApiResponseDto<ProposedProjectDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Project already exists", Code = "DUPLICATE" }]
            };
            _pimsProposedProjectApiClient.CreateProposedProjectAsync(dto).Returns(expected);

            // Act
            var result = await _sut.CreateProposedProjectAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            await _pimsProposedProjectApiClient.Received(1).CreateProposedProjectAsync(dto);
        }

        [Fact]
        public async Task CreateProjectAsync_PassesExactDtoToApiClient()
        {
            // Arrange
            var dto = new ProposedProjectDto { Parentproject = "PP001", Projecttitle = "Test" };
            _pimsProposedProjectApiClient.CreateProposedProjectAsync(dto)
                .Returns(new ApiResponseDto<ProposedProjectDto> { Success = true, Data = dto });

            // Act
            await _sut.CreateProposedProjectAsync(dto);

            // Assert
            await _pimsProposedProjectApiClient.Received(1).CreateProposedProjectAsync(dto);
        }

        [Fact]
        public async Task CreateProjectAsync_WhenApiClientThrows_PropagatesException()
        {
            // Arrange
            var dto = new ProposedProjectDto { Parentproject = "PP001" };
            _pimsProposedProjectApiClient.CreateProposedProjectAsync(dto)
                .Returns(Task.FromException<ApiResponseDto<ProposedProjectDto>>(new Exception("API error")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.CreateProposedProjectAsync(dto));
        }

        #endregion

        #region GetProjectProgramsAsync Tests

        [Fact]
        public async Task GetProjectProgramsAsync_WithData_ReturnsListOfPrograms()
        {
            // Arrange
            var programs = new List<string> { "PROG1", "PROG2", "PROG3" };
            _pimsProposedProjectApiClient.GetProjectProgramsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = programs });

            // Act
            var result = await _sut.GetProjectProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data?.Count);
            await _pimsProposedProjectApiClient.Received(1).GetProjectProgramsAsync();
        }

        [Fact]
        public async Task GetProjectProgramsAsync_WithEmptyList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            _pimsProposedProjectApiClient.GetProjectProgramsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = new List<string>() });

            // Act
            var result = await _sut.GetProjectProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetProjectProgramsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            _pimsProposedProjectApiClient.GetProjectProgramsAsync()
                .Returns(new ApiResponseDto<List<string>>
                {
                    Success = false,
                    Errors = [new ApiErrorDto { Message = "API error", Code = "ERR" }]
                });

            // Act
            var result = await _sut.GetProjectProgramsAsync();

            // Assert
            Assert.False(result.Success);
            await _pimsProposedProjectApiClient.Received(1).GetProjectProgramsAsync();
        }

        [Fact]
        public async Task GetProjectProgramsAsync_DelegatesToApiClient()
        {
            // Arrange
            _pimsProposedProjectApiClient.GetProjectProgramsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });

            // Act
            await _sut.GetProjectProgramsAsync();

            // Assert
            await _pimsProposedProjectApiClient.Received(1).GetProjectProgramsAsync();
        }

        #endregion

        #region GetProjectCustomersAsync Tests

        [Fact]
        public async Task GetProjectCustomersAsync_WithData_ReturnsListOfCustomers()
        {
            // Arrange
            var customers = new List<string> { "CUST1", "CUST2", "CUST3" };
            _pimsProposedProjectApiClient.GetProjectCustomersAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = customers });

            // Act
            var result = await _sut.GetProjectCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data?.Count);
            await _pimsProposedProjectApiClient.Received(1).GetProjectCustomersAsync();
        }

        [Fact]
        public async Task GetProjectCustomersAsync_WithEmptyList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            _pimsProposedProjectApiClient.GetProjectCustomersAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = new List<string>() });

            // Act
            var result = await _sut.GetProjectCustomersAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetProjectCustomersAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            _pimsProposedProjectApiClient.GetProjectCustomersAsync()
                .Returns(new ApiResponseDto<List<string>>
                {
                    Success = false,
                    Errors = [new ApiErrorDto { Message = "API error", Code = "ERR" }]
                });

            // Act
            var result = await _sut.GetProjectCustomersAsync();

            // Assert
            Assert.False(result.Success);
            await _pimsProposedProjectApiClient.Received(1).GetProjectCustomersAsync();
        }

        [Fact]
        public async Task GetProjectCustomersAsync_DelegatesToApiClient()
        {
            // Arrange
            _pimsProposedProjectApiClient.GetProjectCustomersAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });

            // Act
            await _sut.GetProjectCustomersAsync();

            // Assert
            await _pimsProposedProjectApiClient.Received(1).GetProjectCustomersAsync();
        }

        #endregion

        #region GetProjectStatusesAsync Tests

        [Fact]
        public async Task GetProjectStatusesAsync_WithData_ReturnsListOfStatuses()
        {
            // Arrange
            var statuses = new List<string> { "Active", "Proposed", "Closed" };
            _pimsProposedProjectApiClient.GetProjectStatusesAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = statuses });

            // Act
            var result = await _sut.GetProjectStatusesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data?.Count);
            await _pimsProposedProjectApiClient.Received(1).GetProjectStatusesAsync();
        }

        [Fact]
        public async Task GetProjectStatusesAsync_WithEmptyList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            _pimsProposedProjectApiClient.GetProjectStatusesAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = new List<string>() });

            // Act
            var result = await _sut.GetProjectStatusesAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetProjectStatusesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            _pimsProposedProjectApiClient.GetProjectStatusesAsync()
                .Returns(new ApiResponseDto<List<string>>
                {
                    Success = false,
                    Errors = [new ApiErrorDto { Message = "API error", Code = "ERR" }]
                });

            // Act
            var result = await _sut.GetProjectStatusesAsync();

            // Assert
            Assert.False(result.Success);
            await _pimsProposedProjectApiClient.Received(1).GetProjectStatusesAsync();
        }

        [Fact]
        public async Task GetProjectStatusesAsync_DelegatesToApiClient()
        {
            // Arrange
            _pimsProposedProjectApiClient.GetProjectStatusesAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });

            // Act
            await _sut.GetProjectStatusesAsync();

            // Assert
            await _pimsProposedProjectApiClient.Received(1).GetProjectStatusesAsync();
        }

        #endregion
    }
}
