using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.WorkGroupEmployeeServiceTest
{
    public class WorkGroupEmployeeServiceTests
    {
        private const string DefaultWgGrade = "WG01";
        private const string DefaultPactId  = "PACT001";

        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsWorkGroupEmployeeApiClient _fpsWgEmployeeApiClient;
        private readonly WorkGroupEmployeeService _sut;

        public WorkGroupEmployeeServiceTests()
        {
            _fpsClient              = Substitute.For<IFpsApiClient>();
            _fpsWgEmployeeApiClient = Substitute.For<IFpsWorkGroupEmployeeApiClient>();
            _fpsClient.FpsWorkGroupEmployee.Returns(_fpsWgEmployeeApiClient);
            _sut = new WorkGroupEmployeeService(_fpsClient);
        }

        #region Legacy ResourceSetUp Methods

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_WithSuccessResponse_ReturnsEmployeeList()
        {
            var employees = new List<WorkGroupEmployeeDto>
            {
                new() { PactId = DefaultPactId, SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade }
            };
            var expectedResponse = ApiResponseDto<List<WorkGroupEmployeeDto>>.SuccessResponse(employees);
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _fpsWgEmployeeApiClient.GetWorkGroupEmployeeAsync(query, DefaultWgGrade).Returns(expectedResponse);

            var result = await _sut.GetWorkGroupEmployeeAsync(query, DefaultWgGrade);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsWgEmployeeApiClient.Received(1).GetWorkGroupEmployeeAsync(query, DefaultWgGrade);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeByIdAsync_WithSuccessResponse_ReturnsEmployee()
        {
            var dto = new WorkGroupEmployeeDto { PactId = DefaultPactId, SpNumber = "SP001" };
            var expectedResponse = ApiResponseDto<WorkGroupEmployeeDto>.SuccessResponse(dto);

            _fpsWgEmployeeApiClient.GetWorkGroupEmployeeByIdAsync(DefaultPactId).Returns(expectedResponse);

            var result = await _sut.GetWorkGroupEmployeeByIdAsync(DefaultPactId);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(DefaultPactId, result.Data?.PactId);
            await _fpsWgEmployeeApiClient.Received(1).GetWorkGroupEmployeeByIdAsync(DefaultPactId);
        }

        [Fact]
        public async Task UpdateWorkGroupEmployeeAsync_WithValidDto_ReturnsSuccessResponse()
        {
            var dto     = new WorkGroupEmployeeDto { PactId = DefaultPactId, HrsPaid = 40.0 };
            var updated = new WorkGroupEmployeeDto { PactId = DefaultPactId, HrsPaid = 40.0 };
            var expectedResponse = ApiResponseDto<WorkGroupEmployeeDto>.SuccessResponse(updated);

            _fpsWgEmployeeApiClient.UpdateWorkGroupEmployeeAsync(dto).Returns(expectedResponse);

            var result = await _sut.UpdateWorkGroupEmployeeAsync(dto);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(DefaultPactId, result.Data?.PactId);
            await _fpsWgEmployeeApiClient.Received(1).UpdateWorkGroupEmployeeAsync(dto);
        }

        #endregion

        #region Staff Maintenance Methods

        [Fact]
        public async Task GetWorkGroupEmployeeByIdForStaffAsync_WithSuccessResponse_ReturnsEmployee()
        {
            var dto = new WorkGroupEmployeeStaffDto { PactId = DefaultPactId, SpNumber = "SP001" };
            var expectedResponse = ApiResponseDto<WorkGroupEmployeeStaffDto>.SuccessResponse(dto);

            _fpsWgEmployeeApiClient.GetWorkGroupEmployeeByIdForStaffAsync(DefaultPactId).Returns(expectedResponse);

            var result = await _sut.GetWorkGroupEmployeeByIdForStaffAsync(DefaultPactId);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(DefaultPactId, result.Data?.PactId);
            await _fpsWgEmployeeApiClient.Received(1).GetWorkGroupEmployeeByIdForStaffAsync(DefaultPactId);
        }

        [Fact]
        public async Task CreateWorkGroupEmployeeForStaffAsync_WithSuccessResponse_ReturnsEmployee()
        {
            var dto = new WorkGroupEmployeeStaffDto { PactId = DefaultPactId, SpNumber = "SP001" };
            var expectedResponse = ApiResponseDto<WorkGroupEmployeeStaffDto>.SuccessResponse(dto);

            _fpsWgEmployeeApiClient.CreateWorkGroupEmployeeForStaffAsync(dto).Returns(expectedResponse);

            var result = await _sut.CreateWorkGroupEmployeeForStaffAsync(dto);

            Assert.NotNull(result);
            Assert.True(result.Success);
            await _fpsWgEmployeeApiClient.Received(1).CreateWorkGroupEmployeeForStaffAsync(dto);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeForStaffAsync_WithSuccessResponse_ReturnsEmployeeList()
        {
            var employees = new List<WorkGroupEmployeeStaffDto>
            {
                new() { PactId = DefaultPactId, SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade }
            };
            var expectedResponse = ApiResponseDto<List<WorkGroupEmployeeStaffDto>>.SuccessResponse(employees);
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _fpsWgEmployeeApiClient.GetWorkGroupEmployeeForStaffAsync(query, DefaultWgGrade).Returns(expectedResponse);

            var result = await _sut.GetWorkGroupEmployeeForStaffAsync(query, DefaultWgGrade);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsWgEmployeeApiClient.Received(1).GetWorkGroupEmployeeForStaffAsync(query, DefaultWgGrade);
        }

        [Fact]
        public async Task UpdateWorkGroupEmployeeForStaffAsync_WithSuccessResponse_ReturnsEmployee()
        {
            var dto = new WorkGroupEmployeeStaffDto { PactId = DefaultPactId, SpNumber = "SP001" };
            var expectedResponse = ApiResponseDto<WorkGroupEmployeeStaffDto>.SuccessResponse(dto);

            _fpsWgEmployeeApiClient.UpdateWorkGroupEmployeeForStaffAsync(dto).Returns(expectedResponse);

            var result = await _sut.UpdateWorkGroupEmployeeForStaffAsync(dto);

            Assert.NotNull(result);
            Assert.True(result.Success);
            await _fpsWgEmployeeApiClient.Received(1).UpdateWorkGroupEmployeeForStaffAsync(dto);
        }

        #endregion

        #region Shared Methods

        [Fact]
        public async Task DeleteWorkGroupEmployeeAsync_WithSuccessResponse_ReturnsSuccess()
        {
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _fpsWgEmployeeApiClient.DeleteWorkGroupEmployeeAsync(DefaultPactId).Returns(expectedResponse);

            var result = await _sut.DeleteWorkGroupEmployeeAsync(DefaultPactId);

            Assert.NotNull(result);
            Assert.True(result.Success);
            await _fpsWgEmployeeApiClient.Received(1).DeleteWorkGroupEmployeeAsync(DefaultPactId);
        }

        #endregion
    }
}
