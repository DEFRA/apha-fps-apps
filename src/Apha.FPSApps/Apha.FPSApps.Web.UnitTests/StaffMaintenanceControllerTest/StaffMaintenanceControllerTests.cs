using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.StaffMaintenanceControllerTest
{
    public class StaffMaintenanceControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IEmployeeService _employeeService;
        private readonly StaffMaintenanceController _controller;

        public StaffMaintenanceControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _employeeService = Substitute.For<IEmployeeService>();
            _controller = new StaffMaintenanceController(_mapper, _employeeService);
        }

        // Helper method to extract properties from JsonResult
        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json);
        }

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewResult_WithStaffMaintenanceViewModel()
        {
            // Arrange
            var employees = new List<EmployeeDto>
            {
                new EmployeeDto { SPNumber = "000001", FirstName = "John", LastName = "Doe", Title = "Manager" }
            };
            var employeeViewModels = new List<EmployeeViewModel>
            {
                new EmployeeViewModel { SPNumber = "000001", FirstName = "John", LastName = "Doe", Title = "Manager" }
            };

            var apiResponse = ApiResponseDto<List<EmployeeDto>>.SuccessResponse(employees, new PaginationDto { PageNumber = 1, PageSize = 15, TotalRecords = 1 });

            _employeeService.GetFilteredEmployeesAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int>())
                .Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<EmployeeViewModel>>(Arg.Any<List<EmployeeDto>>())
                .Returns(employeeViewModels);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel { PageNumber = 1, PageSize = 15, TotalRecords = 1 });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<StaffMaintenanceViewModel>(viewResult.Model);
            Assert.NotNull(model.StaffGrid);
            Assert.Equal("staffGrid", model.StaffGrid.GridId);
        }

        [Fact]
        public async Task Index_CallsGetFilteredEmployeesAsync_WithDefaultParameters()
        {
            // Arrange
            var apiResponse = ApiResponseDto<List<EmployeeDto>>.SuccessResponse(new List<EmployeeDto>(), new PaginationDto());
            _employeeService.GetFilteredEmployeesAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int>())
                .Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<EmployeeViewModel>>(Arg.Any<List<EmployeeDto>>())
                .Returns(new List<EmployeeViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            await _controller.Index();

            // Assert
            await _employeeService.Received(1).GetFilteredEmployeesAsync(Arg.Any<QueryParameters<string>>(), 1);
        }

        #endregion

        #region LoadStaffGrid Tests

        [Fact]
        public async Task LoadStaffGrid_WithValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            var employees = new List<EmployeeDto>
            {
                new EmployeeDto { SPNumber = "000001", FirstName = "John", LastName = "Doe" }
            };
            var apiResponse = ApiResponseDto<List<EmployeeDto>>.SuccessResponse(employees, new PaginationDto { PageNumber = 1, PageSize = 10 });

            _employeeService.GetFilteredEmployeesAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int>())
                .Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<EmployeeViewModel>>(Arg.Any<List<EmployeeDto>>())
                .Returns(new List<EmployeeViewModel> { new EmployeeViewModel { SPNumber = "000001" } });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadStaffGrid(request, 1);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
            Assert.IsType<DataGridConfig<EmployeeViewModel>>(partialViewResult.Model);
        }

        [Fact]
        public async Task LoadStaffGrid_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _controller.ModelState.AddModelError("Test", "Test error");

            // Act
            var result = await _controller.LoadStaffGrid(request, 1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Invalid request data", value.message);
        }

        [Fact]
        public async Task LoadStaffGrid_PassesFilterOptionToService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            var apiResponse = ApiResponseDto<List<EmployeeDto>>.SuccessResponse(new List<EmployeeDto>(), new PaginationDto());
            
            _employeeService.GetFilteredEmployeesAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int>())
                .Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<EmployeeViewModel>>(Arg.Any<List<EmployeeDto>>())
                .Returns(new List<EmployeeViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            await _controller.LoadStaffGrid(request, 2);

            // Assert
            await _employeeService.Received(1).GetFilteredEmployeesAsync(Arg.Any<QueryParameters<string>>(), 2);
        }

        #endregion

        #region Create Tests

        [Fact]
        public void Create_Get_ReturnsPartialView()
        {
            // Act
            var result = _controller.Create();

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditStaff", partialViewResult.ViewName);
        }

        [Fact]
        public async Task Create_Post_WithValidModel_ReturnsSuccessJson()
        {
            // Arrange
            var employeeViewModel = new EmployeeViewModel
            {
                SPNumber = "000001",
                FirstName = "John",
                LastName = "Doe",
                Title = "Manager"
            };
            var employeeDto = new EmployeeDto
            {
                SPNumber = "000001",
                FirstName = "John",
                LastName = "Doe",
                Title = "Manager"
            };
            var apiResponse = ApiResponseDto<EmployeeDto>.SuccessResponse(employeeDto);

            _mapper.Map<EmployeeDto>(employeeViewModel).Returns(employeeDto);
            _employeeService.CreateEmployeeAsync(employeeDto).Returns(apiResponse);

            // Act
            var result = await _controller.Create(employeeViewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Employee created successfully", value.message);
        }

        [Fact]
        public async Task Create_Post_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var employeeViewModel = new EmployeeViewModel();
            _controller.ModelState.AddModelError("SPNumber", "SP Number is required");

            // Act
            var result = await _controller.Create(employeeViewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Invalid employee data", value.message);
        }

        [Fact]
        public async Task Create_Post_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var employeeViewModel = new EmployeeViewModel { SPNumber = "000001" };
            var employeeDto = new EmployeeDto { SPNumber = "000001" };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Creation failed", Code = "CREATE_ERROR" } };
            var apiResponse = ApiResponseDto<EmployeeDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<EmployeeDto>(employeeViewModel).Returns(employeeDto);
            _employeeService.CreateEmployeeAsync(employeeDto).Returns(apiResponse);

            // Act
            var result = await _controller.Create(employeeViewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        #endregion

        #region Edit Tests

        [Fact]
        public async Task Edit_Get_WithValidSPNumber_ReturnsPartialViewWithModel()
        {
            // Arrange
            var spNumber = "000001";
            var employeeDto = new EmployeeDto { SPNumber = spNumber, FirstName = "John", LastName = "Doe" };
            var employeeViewModel = new EmployeeViewModel { SPNumber = spNumber, FirstName = "John", LastName = "Doe" };
            var apiResponse = ApiResponseDto<EmployeeDto>.SuccessResponse(employeeDto);

            _employeeService.GetEmployeeByIdAsync(spNumber).Returns(apiResponse);
            _mapper.Map<EmployeeViewModel>(employeeDto).Returns(employeeViewModel);

            // Act
            var result = await _controller.Edit(spNumber);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditStaff", partialViewResult.ViewName);
            var model = Assert.IsType<EmployeeViewModel>(partialViewResult.Model);
            Assert.Equal(spNumber, model.SPNumber);
        }

        [Fact]
        public async Task Edit_Get_WithNullSPNumber_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Edit(" ");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("SP Number is required", badRequestResult.Value);
        }

        [Fact]
        public async Task Edit_Get_WithEmptySPNumber_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Edit("");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("SP Number is required", badRequestResult.Value);
        }

        [Fact]
        public async Task Edit_Get_WhenEmployeeNotFound_ReturnsNotFound()
        {
            // Arrange
            var spNumber = "000001";
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = ApiResponseDto<EmployeeDto>.FailureResponse(errors, new ApiMetaDto());

            _employeeService.GetEmployeeByIdAsync(spNumber).Returns(apiResponse);

            // Act
            var result = await _controller.Edit(spNumber);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"Employee with SP Number {spNumber} not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task Edit_Post_WithValidModel_ReturnsSuccessJson()
        {
            // Arrange
            var employeeViewModel = new EmployeeViewModel
            {
                SPNumber = "000001",
                FirstName = "John",
                LastName = "Doe",
                Title = "Manager"
            };
            var employeeDto = new EmployeeDto
            {
                SPNumber = "000001",
                FirstName = "John",
                LastName = "Doe",
                Title = "Manager"
            };
            var apiResponse = ApiResponseDto<EmployeeDto>.SuccessResponse(employeeDto);

            _mapper.Map<EmployeeDto>(employeeViewModel).Returns(employeeDto);
            _employeeService.UpdateEmployeeAsync(employeeDto).Returns(apiResponse);

            // Act
            var result = await _controller.Edit(employeeViewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Employee updated successfully", value.message);
        }

        [Fact]
        public async Task Edit_Post_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var employeeViewModel = new EmployeeViewModel();
            _controller.ModelState.AddModelError("SPNumber", "SP Number is required");

            // Act
            var result = await _controller.Edit(employeeViewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Invalid employee data", value.message);
        }

        [Fact]
        public async Task Edit_Post_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var employeeViewModel = new EmployeeViewModel { SPNumber = "000001" };
            var employeeDto = new EmployeeDto { SPNumber = "000001" };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Update failed", Code = "UPDATE_ERROR" } };
            var apiResponse = ApiResponseDto<EmployeeDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<EmployeeDto>(employeeViewModel).Returns(employeeDto);
            _employeeService.UpdateEmployeeAsync(employeeDto).Returns(apiResponse);

            // Act
            var result = await _controller.Edit(employeeViewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithValidSPNumber_ReturnsSuccessJson()
        {
            // Arrange
            var spNumber = "000001";
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _employeeService.DeleteEmployeeAsync(spNumber).Returns(apiResponse);

            // Act
            var result = await _controller.Delete(spNumber);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Employee deleted successfully", value.message);
        }

        [Fact]
        public async Task Delete_WithNullSPNumber_ReturnsJsonError()
        {
            // Act
            var result = await _controller.Delete(null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("SP Number is required", value.message);
        }

        [Fact]
        public async Task Delete_WithEmptySPNumber_ReturnsJsonError()
        {
            // Act
            var result = await _controller.Delete("");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("SP Number is required", value.message);
        }

        [Fact]
        public async Task Delete_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var spNumber = "000001";
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Delete failed", Code = "DELETE_ERROR" } };
            var apiResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _employeeService.DeleteEmployeeAsync(spNumber).Returns(apiResponse);

            // Act
            var result = await _controller.Delete(spNumber);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task Delete_CallsDeleteEmployeeAsync_Once()
        {
            // Arrange
            var spNumber = "000001";
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _employeeService.DeleteEmployeeAsync(spNumber).Returns(apiResponse);

            // Act
            await _controller.Delete(spNumber);

            // Assert
            await _employeeService.Received(1).DeleteEmployeeAsync(spNumber);
        }

        #endregion

        #region GetEmployee Tests

        [Fact]
        public async Task GetEmployee_WithValidSPNumber_ReturnsSuccessJson()
        {
            // Arrange
            var spNumber = "000001";
            var employeeDto = new EmployeeDto { SPNumber = spNumber, FirstName = "John", LastName = "Doe" };
            var apiResponse = ApiResponseDto<EmployeeDto>.SuccessResponse(employeeDto);

            _employeeService.GetEmployeeByIdAsync(spNumber).Returns(apiResponse);

            // Act
            var result = await _controller.GetEmployee(spNumber);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.NotNull(value.data);
        }

        [Fact]
        public async Task GetEmployee_WithNullSPNumber_ReturnsJsonError()
        {
            // Act
            var result = await _controller.GetEmployee(null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("SP Number is required", value.message);
        }

        [Fact]
        public async Task GetEmployee_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var spNumber = "000001";
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = ApiResponseDto<EmployeeDto>.FailureResponse(errors, new ApiMetaDto());

            _employeeService.GetEmployeeByIdAsync(spNumber).Returns(apiResponse);

            // Act
            var result = await _controller.GetEmployee(spNumber);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public async Task LoadStaffGrid_WithNullFilter_HandlesGracefully()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = null };
            var apiResponse = ApiResponseDto<List<EmployeeDto>>.SuccessResponse(new List<EmployeeDto>(), new PaginationDto());

            _employeeService.GetFilteredEmployeesAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int>())
                .Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<EmployeeViewModel>>(Arg.Any<List<EmployeeDto>>())
                .Returns(new List<EmployeeViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadStaffGrid(request, 1);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.NotNull(partialViewResult);
        }

        [Fact]
        public async Task LoadStaffGrid_WithEmptyDataResponse_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            var apiResponse = ApiResponseDto<List<EmployeeDto>>.SuccessResponse(new List<EmployeeDto>(), new PaginationDto());

            _employeeService.GetFilteredEmployeesAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int>())
                .Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<EmployeeViewModel>>(Arg.Any<List<EmployeeDto>>())
                .Returns(new List<EmployeeViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadStaffGrid(request, 1);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<EmployeeViewModel>>(partialViewResult.Model);
            Assert.Empty(gridConfig.Data);
        }

        [Fact]
        public async Task Create_Post_CallsMapperAndService_InCorrectOrder()
        {
            // Arrange
            var employeeViewModel = new EmployeeViewModel { SPNumber = "000001" };
            var employeeDto = new EmployeeDto { SPNumber = "000001" };
            var apiResponse = ApiResponseDto<EmployeeDto>.SuccessResponse(employeeDto);

            _mapper.Map<EmployeeDto>(employeeViewModel).Returns(employeeDto);
            _employeeService.CreateEmployeeAsync(employeeDto).Returns(apiResponse);

            // Act
            await _controller.Create(employeeViewModel);

            // Assert
            _mapper.Received(1).Map<EmployeeDto>(employeeViewModel);
            await _employeeService.Received(1).CreateEmployeeAsync(employeeDto);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task LoadStaffGrid_WithDifferentFilterOptions_PassesCorrectValue(int filterOption)
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            var apiResponse = ApiResponseDto<List<EmployeeDto>>.SuccessResponse(new List<EmployeeDto>(), new PaginationDto());

            _employeeService.GetFilteredEmployeesAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int>())
                .Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<EmployeeViewModel>>(Arg.Any<List<EmployeeDto>>())
                .Returns(new List<EmployeeViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            await _controller.LoadStaffGrid(request, filterOption);

            // Assert
            await _employeeService.Received(1).GetFilteredEmployeesAsync(Arg.Any<QueryParameters<string>>(), filterOption);
        }

        [Fact]
        public async Task Edit_Post_UpdatesExistingEmployee_Successfully()
        {
            // Arrange
            var employeeViewModel = new EmployeeViewModel
            {
                SPNumber = "000001",
                FirstName = "Jane",
                LastName = "Smith",
                Title = "Director"
            };
            var employeeDto = new EmployeeDto
            {
                SPNumber = "000001",
                FirstName = "Jane",
                LastName = "Smith",
                Title = "Director"
            };
            var apiResponse = ApiResponseDto<EmployeeDto>.SuccessResponse(employeeDto);

            _mapper.Map<EmployeeDto>(employeeViewModel).Returns(employeeDto);
            _employeeService.UpdateEmployeeAsync(employeeDto).Returns(apiResponse);

            // Act
            var result = await _controller.Edit(employeeViewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            await _employeeService.Received(1).UpdateEmployeeAsync(employeeDto);
        }

        #endregion

        #region GetEmployeeGridConfigAsync Private Method Tests (via public methods)

        [Fact]
        public async Task LoadStaffGrid_ConfiguresGridCorrectly()
        {
            // Arrange
            var request = new PaginationFilter<string> 
            { 
                Filter = "{}", 
                SortBy = "LastName", 
                Descending = true,
                PageSize = 20
            };
            var apiResponse = ApiResponseDto<List<EmployeeDto>>.SuccessResponse(new List<EmployeeDto>(), new PaginationDto());

            _employeeService.GetFilteredEmployeesAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int>())
                .Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<EmployeeViewModel>>(Arg.Any<List<EmployeeDto>>())
                .Returns(new List<EmployeeViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadStaffGrid(request, 1);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<EmployeeViewModel>>(partialViewResult.Model);
            
            Assert.Equal("staffGrid", gridConfig.GridId);
            Assert.Equal("Staff Maintenance", gridConfig.Title);
            Assert.True(gridConfig.ShowCheckboxColumn);
            Assert.True(gridConfig.ShowPagination);
            Assert.Equal("SPNumber", gridConfig.KeyProperty);
            Assert.Equal("addStaff", gridConfig.AddFunction);
            Assert.Equal("editStaff", gridConfig.EditFunction);
            Assert.Equal("deleteStaff", gridConfig.DeleteFunction);
            Assert.Equal("getStaffExtraFilters", gridConfig.ExtraFilterMethod);
            Assert.Equal("/FPS/StaffMaintenance/LoadStaffGrid", gridConfig.BindGridUrl);
        }

        #endregion

        // Helper class to deserialize JSON responses
        private class JsonResponse
        {
            public bool success { get; set; }
            public string? message { get; set; }
            public object? data { get; set; }
            public object? errors { get; set; }
        }
    }
}