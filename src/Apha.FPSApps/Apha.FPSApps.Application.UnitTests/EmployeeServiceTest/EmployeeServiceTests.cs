using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.EmployeeServiceTest
{
    public class EmployeeServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsEmployeeApiClient _fpsEmployeeApiClient;
        private readonly EmployeeService _employeeService;

        public EmployeeServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _fpsEmployeeApiClient = Substitute.For<IFpsEmployeeApiClient>();
            _fpsClient.FpsEmployee.Returns(_fpsEmployeeApiClient);
            _employeeService = new EmployeeService(_fpsClient);
        }

        #region GetFilteredEmployeesAsync Tests

        [Fact]
        public async Task GetFilteredEmployeesAsync_WithValidCriteria_ReturnsSuccessResponse()
        {
            // Arrange
            var queryParameters = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10
            };
            var employees = new List<EmployeeDto>
            {
                new EmployeeDto { SPNumber = "000001", FirstName = "John", LastName = "Doe" },
                new EmployeeDto { SPNumber = "000002", FirstName = "Jane", LastName = "Smith" }
            };
            var expectedResponse = ApiResponseDto<List<EmployeeDto>>.SuccessResponse(
                employees,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );

            _fpsEmployeeApiClient.GetFilteredEmployeesAsync(queryParameters, 1)
                .Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetFilteredEmployeesAsync(queryParameters, 1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsEmployeeApiClient.Received(1).GetFilteredEmployeesAsync(queryParameters, 1);
        }

        [Fact]
        public async Task GetFilteredEmployeesAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var queryParameters = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10
            };
            var expectedResponse = ApiResponseDto<List<EmployeeDto>>.SuccessResponse(
                new List<EmployeeDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            );

            _fpsEmployeeApiClient.GetFilteredEmployeesAsync(queryParameters, 1)
                .Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetFilteredEmployeesAsync(queryParameters, 1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetFilteredEmployeesAsync_WithDifferentFilterOptions_PassesCorrectValue(int filterOption)
        {
            // Arrange
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<EmployeeDto>>.SuccessResponse(
                new List<EmployeeDto>(),
                new PaginationDto()
            );

            _fpsEmployeeApiClient.GetFilteredEmployeesAsync(queryParameters, filterOption)
                .Returns(expectedResponse);

            // Act
            await _employeeService.GetFilteredEmployeesAsync(queryParameters, filterOption);

            // Assert
            await _fpsEmployeeApiClient.Received(1).GetFilteredEmployeesAsync(queryParameters, filterOption);
        }

        [Fact]
        public async Task GetFilteredEmployeesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<EmployeeDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsEmployeeApiClient.GetFilteredEmployeesAsync(queryParameters, 1)
                .Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetFilteredEmployeesAsync(queryParameters, 1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetEmployeeByIdAsync Tests

        [Fact]
        public async Task GetEmployeeByIdAsync_WithValidSPNumber_ReturnsEmployee()
        {
            // Arrange
            var spNumber = "000001";
            var employee = new EmployeeDto
            {
                SPNumber = spNumber,
                FirstName = "John",
                LastName = "Doe",
                Title = "Manager"
            };
            var expectedResponse = ApiResponseDto<EmployeeDto>.SuccessResponse(employee);

            _fpsEmployeeApiClient.GetEmployeeIdAsync(spNumber).Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetEmployeeByIdAsync(spNumber);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(spNumber, result.Data.SPNumber);
            await _fpsEmployeeApiClient.Received(1).GetEmployeeIdAsync(spNumber);
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_WithNonExistentSPNumber_ReturnsFailureResponse()
        {
            // Arrange
            var spNumber = "999999";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Employee not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<EmployeeDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsEmployeeApiClient.GetEmployeeIdAsync(spNumber).Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetEmployeeByIdAsync(spNumber);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Theory]
        [InlineData("000001")]
        [InlineData("123456")]
        [InlineData("SP9999")]
        public async Task GetEmployeeByIdAsync_WithVariousSPNumbers_CallsApiClient(string spNumber)
        {
            // Arrange
            var expectedResponse = ApiResponseDto<EmployeeDto>.SuccessResponse(new EmployeeDto { SPNumber = spNumber });
            _fpsEmployeeApiClient.GetEmployeeIdAsync(spNumber).Returns(expectedResponse);

            // Act
            await _employeeService.GetEmployeeByIdAsync(spNumber);

            // Assert
            await _fpsEmployeeApiClient.Received(1).GetEmployeeIdAsync(spNumber);
        }

        #endregion

        #region CreateEmployeeAsync Tests

        [Fact]
        public async Task CreateEmployeeAsync_WithValidEmployee_ReturnsSuccessResponse()
        {
            // Arrange
            var newEmployee = new EmployeeDto
            {
                SPNumber = "000001",
                FirstName = "John",
                LastName = "Doe",
                Title = "Manager"
            };
            var expectedResponse = ApiResponseDto<EmployeeDto>.SuccessResponse(newEmployee);

            _fpsEmployeeApiClient.CreateEmployeeAsync(newEmployee).Returns(expectedResponse);

            // Act
            var result = await _employeeService.CreateEmployeeAsync(newEmployee);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(newEmployee.SPNumber, result.Data.SPNumber);
            await _fpsEmployeeApiClient.Received(1).CreateEmployeeAsync(newEmployee);
        }

        [Fact]
        public async Task CreateEmployeeAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var newEmployee = new EmployeeDto
            {
                SPNumber = "000001",
                FirstName = "John",
                LastName = "Doe"
            };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Duplicate employee", Code = "DUPLICATE" }
            };
            var expectedResponse = ApiResponseDto<EmployeeDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsEmployeeApiClient.CreateEmployeeAsync(newEmployee).Returns(expectedResponse);

            // Act
            var result = await _employeeService.CreateEmployeeAsync(newEmployee);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task CreateEmployeeAsync_WithMinimalData_CallsApiClient()
        {
            // Arrange
            var newEmployee = new EmployeeDto { SPNumber = "000001" };
            var expectedResponse = ApiResponseDto<EmployeeDto>.SuccessResponse(newEmployee);

            _fpsEmployeeApiClient.CreateEmployeeAsync(newEmployee).Returns(expectedResponse);

            // Act
            await _employeeService.CreateEmployeeAsync(newEmployee);

            // Assert
            await _fpsEmployeeApiClient.Received(1).CreateEmployeeAsync(newEmployee);
        }

        #endregion

        #region UpdateEmployeeAsync Tests

        [Fact]
        public async Task UpdateEmployeeAsync_WithValidEmployee_ReturnsSuccessResponse()
        {
            // Arrange
            var updatedEmployee = new EmployeeDto
            {
                SPNumber = "000001",
                FirstName = "Jane",
                LastName = "Smith",
                Title = "Senior Manager"
            };
            var expectedResponse = ApiResponseDto<EmployeeDto>.SuccessResponse(updatedEmployee);

            _fpsEmployeeApiClient.UpdateEmployeeAsync(updatedEmployee).Returns(expectedResponse);

            // Act
            var result = await _employeeService.UpdateEmployeeAsync(updatedEmployee);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Jane", result.Data.FirstName);
            await _fpsEmployeeApiClient.Received(1).UpdateEmployeeAsync(updatedEmployee);
        }

        [Fact]
        public async Task UpdateEmployeeAsync_WithNonExistentEmployee_ReturnsFailureResponse()
        {
            // Arrange
            var employee = new EmployeeDto { SPNumber = "999999" };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Employee not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<EmployeeDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsEmployeeApiClient.UpdateEmployeeAsync(employee).Returns(expectedResponse);

            // Act
            var result = await _employeeService.UpdateEmployeeAsync(employee);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task UpdateEmployeeAsync_WhenApiReturnsError_ReturnsFailureResponse()
        {
            // Arrange
            var employee = new EmployeeDto { SPNumber = "000001" };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Update failed", Code = "UPDATE_ERROR" }
            };
            var expectedResponse = ApiResponseDto<EmployeeDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsEmployeeApiClient.UpdateEmployeeAsync(employee).Returns(expectedResponse);

            // Act
            var result = await _employeeService.UpdateEmployeeAsync(employee);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteEmployeeAsync Tests

        [Fact]
        public async Task DeleteEmployeeAsync_WithValidSPNumber_ReturnsSuccessResponse()
        {
            // Arrange
            var spNumber = "000001";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _fpsEmployeeApiClient.DeleteEmployeeAsync(spNumber).Returns(expectedResponse);

            // Act
            var result = await _employeeService.DeleteEmployeeAsync(spNumber);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsEmployeeApiClient.Received(1).DeleteEmployeeAsync(spNumber);
        }

        [Fact]
        public async Task DeleteEmployeeAsync_WithNonExistentSPNumber_ReturnsFailureResponse()
        {
            // Arrange
            var spNumber = "999999";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Employee not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _fpsEmployeeApiClient.DeleteEmployeeAsync(spNumber).Returns(expectedResponse);

            // Act
            var result = await _employeeService.DeleteEmployeeAsync(spNumber);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Theory]
        [InlineData("000001")]
        [InlineData("123456")]
        [InlineData("SP9999")]
        public async Task DeleteEmployeeAsync_WithVariousSPNumbers_CallsApiClient(string spNumber)
        {
            // Arrange
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsEmployeeApiClient.DeleteEmployeeAsync(spNumber).Returns(expectedResponse);

            // Act
            await _employeeService.DeleteEmployeeAsync(spNumber);

            // Assert
            await _fpsEmployeeApiClient.Received(1).DeleteEmployeeAsync(spNumber);
        }

        #endregion

        #region GetAllManagersAsync Tests

        [Fact]
        public async Task GetAllManagersAsync_ReturnsListOfManagers()
        {
            // Arrange
            var managers = new List<ManagerDto>
            {
                new ManagerDto { Name = "John Manager", WorkGroup = "Operations", GradeCode = "M1" },
                new ManagerDto { Name = "Jane Director", WorkGroup = "Finance", GradeCode = "D1" }
            };
            var expectedResponse = ApiResponseDto<List<ManagerDto>>.SuccessResponse(managers);

            _fpsEmployeeApiClient.GetAllManagerAsync().Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetAllManagersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _fpsEmployeeApiClient.Received(1).GetAllManagerAsync();
        }

        [Fact]
        public async Task GetAllManagersAsync_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<ManagerDto>>.SuccessResponse(new List<ManagerDto>());

            _fpsEmployeeApiClient.GetAllManagerAsync().Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetAllManagersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllManagersAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<ManagerDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsEmployeeApiClient.GetAllManagerAsync().Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetAllManagersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public async Task GetFilteredEmployeesAsync_CallsApiClientOnce()
        {
            // Arrange
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<EmployeeDto>>.SuccessResponse(
                new List<EmployeeDto>(),
                new PaginationDto()
            );

            _fpsEmployeeApiClient.GetFilteredEmployeesAsync(queryParameters, 1)
                .Returns(expectedResponse);

            // Act
            await _employeeService.GetFilteredEmployeesAsync(queryParameters, 1);

            // Assert
            await _fpsEmployeeApiClient.Received(1).GetFilteredEmployeesAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int>());
        }

        [Fact]
        public async Task CreateEmployeeAsync_PassesExactEmployeeObject()
        {
            // Arrange
            var employee = new EmployeeDto
            {
                SPNumber = "000001",
                FirstName = "Test",
                LastName = "User",
                Title = "Tester"
            };
            var expectedResponse = ApiResponseDto<EmployeeDto>.SuccessResponse(employee);

            _fpsEmployeeApiClient.CreateEmployeeAsync(employee).Returns(expectedResponse);

            // Act
            await _employeeService.CreateEmployeeAsync(employee);

            // Assert
            await _fpsEmployeeApiClient.Received(1).CreateEmployeeAsync(Arg.Is<EmployeeDto>(e =>
                e.SPNumber == employee.SPNumber &&
                e.FirstName == employee.FirstName &&
                e.LastName == employee.LastName &&
                e.Title == employee.Title
            ));
        }

        [Fact]
        public async Task UpdateEmployeeAsync_PassesExactEmployeeObject()
        {
            // Arrange
            var employee = new EmployeeDto
            {
                SPNumber = "000001",
                FirstName = "Updated",
                LastName = "User"
            };
            var expectedResponse = ApiResponseDto<EmployeeDto>.SuccessResponse(employee);

            _fpsEmployeeApiClient.UpdateEmployeeAsync(employee).Returns(expectedResponse);

            // Act
            await _employeeService.UpdateEmployeeAsync(employee);

            // Assert
            await _fpsEmployeeApiClient.Received(1).UpdateEmployeeAsync(Arg.Is<EmployeeDto>(e =>
                e.SPNumber == employee.SPNumber &&
                e.FirstName == employee.FirstName &&
                e.LastName == employee.LastName
            ));
        }

        #endregion
    }
}