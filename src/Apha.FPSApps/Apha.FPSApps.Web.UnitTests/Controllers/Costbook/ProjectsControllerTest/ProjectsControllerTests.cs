using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.CostBook.Controllers;
using Apha.FPSApps.Web.Areas.CostBook.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.Costbook.ProjectsControllerTest
{
    public class ProjectsControllerTests
    {
        private readonly IMapper _mapper;
        private readonly ICostBookProjectService _projectService;
        private readonly ICostBookCustomerService _customerService;
        private readonly ICostBookDiseaseService _diseaseService;
        private readonly ICostBookProgramService _programService;
        private readonly ICostBookStaffService _staffService;
        private readonly ICostBookContractService _contractService;
        private readonly ProjectsController _controller;

        public ProjectsControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _projectService = Substitute.For<ICostBookProjectService>();
            _customerService = Substitute.For<ICostBookCustomerService>();
            _diseaseService = Substitute.For<ICostBookDiseaseService>();
            _programService = Substitute.For<ICostBookProgramService>();
            _staffService = Substitute.For<ICostBookStaffService>();
            _contractService = Substitute.For<ICostBookContractService>();

            _controller = new ProjectsController(
                _projectService,
                _customerService,
                _diseaseService,
                _programService,
                _staffService,
                _contractService,
                _mapper);

            // Setup TempData
            _controller.TempData = Substitute.For<ITempDataDictionary>();
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }       

        #region Index Tests

        [Fact]
        public async Task Index_WithDefaultParameters_ReturnsViewWithProjectGrid()
        {
            // Arrange
            var projects = new List<ProjectDto>
            {
                new() { ProjectId = "P001", ProjectTitle = "Project 1" },
                new() { ProjectId = "P002", ProjectTitle = "Project 2" }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 5, TotalRecords = 2 };
            var serviceResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects, paginationDto);
            var projectItems = new List<ProjectItemViewModel>
            {
                new() { ProjectId = "P001", ProjectTitle = "Project 1", ContractNumber = "C001" },
                new() { ProjectId = "P002", ProjectTitle = "Project 2", ContractNumber = "C002" }
            };
            var paginationModel = new PaginationModel { PageNumber = 1, PageSize = 5, TotalRecords = 2 };

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _projectService.GetFilteredProjectsAsync(Arg.Any<QueryParameters<string>>())
                .Returns(serviceResponse);
            _mapper.Map<List<ProjectItemViewModel>>(Arg.Any<List<ProjectDto>>()).Returns(projectItems);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(paginationModel);

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectViewModel>(viewResult.Model);
            Assert.NotNull(model.ProjectGrid);
            Assert.Equal("projectGrid", model.ProjectGrid.GridId);
            Assert.Equal(2, model.ProjectGrid.Data.Count);
        }

        [Fact]
        public async Task Index_WithCustomParameters_PassesParametersToViewModel()
        {
            // Arrange
            var searchTerm = "test";
            var selectedYear = 2024;
            var recordsPerPage = 10;
            var currentPage = 2;

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _projectService.GetFilteredProjectsAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>(), new PaginationDto()));
            _mapper.Map<List<ProjectItemViewModel>>(Arg.Any<List<ProjectDto>>()).Returns(new List<ProjectItemViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.Index(searchTerm, selectedYear, recordsPerPage, currentPage);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectViewModel>(viewResult.Model);
            Assert.Equal(searchTerm, model.SearchTerm);
            Assert.Equal(selectedYear, model.SelectedYear);
            Assert.Equal(recordsPerPage, model.RecordsPerPage);
            Assert.Equal(currentPage, model.CurrentPage);
        }

        #endregion

        #region LoadProjectGrid Tests

        [Fact]
        public async Task LoadProjectGrid_WithValidRequest_ReturnsPartialViewWithDataGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var projects = new List<ProjectDto>
            {
                new() { ProjectId = "P001", ProjectTitle = "Project 1" }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var serviceResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects, paginationDto);
            var projectItems = new List<ProjectItemViewModel>
            {
                new() { ProjectId = "P001", ProjectTitle = "Project 1", ContractNumber = "C001" }
            };
            var paginationModel = new PaginationModel { PageNumber = 1, PageSize = 10, TotalRecords = 1 };

            _mapper.Map<QueryParameters<string>>(request).Returns(new QueryParameters<string>());
            _projectService.GetFilteredProjectsAsync(Arg.Any<QueryParameters<string>>()).Returns(serviceResponse);
            _mapper.Map<List<ProjectItemViewModel>>(Arg.Any<List<ProjectDto>>()).Returns(projectItems);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(paginationModel);

            // Act
            var result = await _controller.LoadProjectGrid(request);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectItemViewModel>>(partialView.Model);
            Assert.Equal("projectGrid", gridConfig.GridId);
            Assert.Single(gridConfig.Data);
        }

        [Fact]
        public async Task LoadProjectGrid_WhenModelStateIsInvalid_ReturnsFailureJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("Filter", "Filter is required");
            var request = new PaginationFilter<string>();

            // Act
            var result = await _controller.LoadProjectGrid(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Invalid request data", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task LoadProjectGrid_WhenServiceReturnsNullData_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var serviceResponse = ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(new QueryParameters<string>());
            _projectService.GetFilteredProjectsAsync(Arg.Any<QueryParameters<string>>()).Returns(serviceResponse);

            // Act
            var result = await _controller.LoadProjectGrid(request);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectItemViewModel>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
        }

        #endregion

        

        #region Create (GET) Tests

        [Fact]
        public async Task Create_Get_ReturnsViewWithPopulatedDropdowns()
        {
            // Arrange
            var programs = new List<ProgramDto> { new() { ProgramNo = "PR001", ProgramName = "Program 1" } };
            var customers = new List<CustomerDto> { new() { CustomerName = "Customer 1" } };
            var diseases = new List<DiseaseDto> { new() { DiseaseName = "Disease 1" } };
            var staff = new List<StaffDto> { new() { Name = "John Doe" } };

            _programService.GetAllProgramsAsync().Returns(ApiResponseDto<List<ProgramDto>>.SuccessResponse(programs));
            _customerService.GetAllCustomersAsync().Returns(ApiResponseDto<List<CustomerDto>>.SuccessResponse(customers));
            _diseaseService.GetAllDiseasesAsync().Returns(ApiResponseDto<List<DiseaseDto>>.SuccessResponse(diseases));
            _staffService.GetAllStaffAsync().Returns(ApiResponseDto<List<StaffDto>>.SuccessResponse(staff));

            // Act
            var result = await _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectCreateEditViewModel>(viewResult.Model);
            Assert.NotNull(model.AvailablePrograms);
            Assert.NotNull(model.AvailableCustomers);
            Assert.NotNull(model.AvailableDiseases);
            Assert.NotNull(model.AvailableStaff);
        }

        #endregion

        #region Create (POST) Tests

        [Fact]
        public async Task Create_Post_WithValidModel_RedirectsToEdit()
        {
            // Arrange
            var viewModel = new ProjectCreateEditViewModel
            {
                ProjectId = "P001",
                ProjectTitle = "New Project",
                StartDate = DateOnly.FromDateTime(DateTime.Today)
            };
            var projectDto = new ProjectDto { ProjectId = "P001", ProjectTitle = "New Project" };
            var serviceResponse = ApiResponseDto<ProjectDto>.SuccessResponse(projectDto);

            _mapper.Map<ProjectDto>(viewModel).Returns(projectDto);
            _projectService.AddProjectAsync(projectDto).Returns(serviceResponse);

            // Act
            var result = await _controller.Create(viewModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirectResult.ActionName);
            Assert.NotNull(redirectResult.RouteValues);
            Assert.Equal("P001", redirectResult.RouteValues["id"]);
        }

        [Fact]
        public async Task Create_Post_WithInvalidModel_ReturnsViewWithErrors()
        {
            // Arrange
            var viewModel = new ProjectCreateEditViewModel();
            _controller.ModelState.AddModelError("ProjectTitle", "Required"); // "ProjectId" is removed by the controller, use a different key

            _programService.GetAllProgramsAsync().Returns(ApiResponseDto<List<ProgramDto>>.SuccessResponse(new List<ProgramDto>()));
            _customerService.GetAllCustomersAsync().Returns(ApiResponseDto<List<CustomerDto>>.SuccessResponse(new List<CustomerDto>()));
            _diseaseService.GetAllDiseasesAsync().Returns(ApiResponseDto<List<DiseaseDto>>.SuccessResponse(new List<DiseaseDto>()));
            _staffService.GetAllStaffAsync().Returns(ApiResponseDto<List<StaffDto>>.SuccessResponse(new List<StaffDto>()));

            // Act
            var result = await _controller.Create(viewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ProjectCreateEditViewModel>(viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Create_Post_WhenServiceFails_AddsModelErrorAndReturnsView()
        {
            // Arrange
            var viewModel = new ProjectCreateEditViewModel { ProjectId = "P001", ProjectTitle = "Test" };
            var projectDto = new ProjectDto { ProjectId = "P001" };
            var errors = new List<ApiErrorDto> { new() { Message = "Creation failed", Code = "CREATE_ERROR" } };
            var serviceResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<ProjectDto>(viewModel).Returns(projectDto);
            _projectService.AddProjectAsync(projectDto).Returns(serviceResponse);
            _programService.GetAllProgramsAsync().Returns(ApiResponseDto<List<ProgramDto>>.SuccessResponse(new List<ProgramDto>()));
            _customerService.GetAllCustomersAsync().Returns(ApiResponseDto<List<CustomerDto>>.SuccessResponse(new List<CustomerDto>()));
            _diseaseService.GetAllDiseasesAsync().Returns(ApiResponseDto<List<DiseaseDto>>.SuccessResponse(new List<DiseaseDto>()));
            _staffService.GetAllStaffAsync().Returns(ApiResponseDto<List<StaffDto>>.SuccessResponse(new List<StaffDto>()));

            // Act
            var result = await _controller.Create(viewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        #endregion

        #region Edit (GET) Tests

        [Fact]
        public async Task Edit_Get_WithValidId_ReturnsViewWithProject()
        {
            // Arrange
            var projectId = "P001";
            var projectDto = new ProjectDto { ProjectId = projectId, ProjectTitle = "Test Project" };
            var serviceResponse = ApiResponseDto<ProjectDto>.SuccessResponse(projectDto);
            var viewModel = new ProjectCreateEditViewModel { ProjectId = projectId, ProjectTitle = "Test Project" };

            _projectService.GetProjectByIdAsync(projectId).Returns(serviceResponse);
            _mapper.Map<ProjectCreateEditViewModel>(projectDto).Returns(viewModel);
            _programService.GetAllProgramsAsync().Returns(ApiResponseDto<List<ProgramDto>>.SuccessResponse(new List<ProgramDto>()));
            _customerService.GetAllCustomersAsync().Returns(ApiResponseDto<List<CustomerDto>>.SuccessResponse(new List<CustomerDto>()));
            _diseaseService.GetAllDiseasesAsync().Returns(ApiResponseDto<List<DiseaseDto>>.SuccessResponse(new List<DiseaseDto>()));
            _staffService.GetAllStaffAsync().Returns(ApiResponseDto<List<StaffDto>>.SuccessResponse(new List<StaffDto>()));

            // Act
            var result = await _controller.Edit(projectId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectCreateEditViewModel>(viewResult.Model);
            Assert.Equal(projectId, model.ProjectId);
        }

        [Fact]
        public async Task Edit_Get_WithNullId_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.Edit(null!));
        }

        #endregion

        #region Edit (POST) Tests

        [Fact]
        public async Task Edit_Post_WithValidModel_RedirectsToEdit()
        {
            // Arrange
            var projectId = "P001";
            var viewModel = new ProjectCreateEditViewModel
            {
                ProjectId = projectId,
                ProjectTitle = "Updated Project"
            };
            var projectDto = new ProjectDto { ProjectId = projectId, ProjectTitle = "Updated Project" };
            var serviceResponse = ApiResponseDto<ProjectDto>.SuccessResponse(projectDto);

            _mapper.Map<ProjectDto>(viewModel).Returns(projectDto);
            _projectService.UpdateProjectAsync(projectId, projectDto).Returns(serviceResponse);

            // Act
            var result = await _controller.Edit(projectId, viewModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirectResult.ActionName);
            Assert.NotNull(redirectResult.RouteValues);
            Assert.Equal(projectId, redirectResult.RouteValues["id"]);
        }

        [Fact]
        public async Task Edit_Post_WithMismatchedId_ThrowsArgumentException()
        {
            // Arrange
            var projectId = "P001";
            var viewModel = new ProjectCreateEditViewModel { ProjectId = "P002" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.Edit(projectId, viewModel));
        }

        [Fact]
        public async Task Edit_Post_WhenServiceFails_AddsModelErrorAndReturnsView()
        {
            // Arrange
            var projectId = "P001";
            var viewModel = new ProjectCreateEditViewModel { ProjectId = projectId, ProjectTitle = "Test" };
            var projectDto = new ProjectDto { ProjectId = projectId };
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } };
            var serviceResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<ProjectDto>(viewModel).Returns(projectDto);
            _projectService.UpdateProjectAsync(projectId, projectDto).Returns(serviceResponse);
            _programService.GetAllProgramsAsync().Returns(ApiResponseDto<List<ProgramDto>>.SuccessResponse(new List<ProgramDto>()));
            _customerService.GetAllCustomersAsync().Returns(ApiResponseDto<List<CustomerDto>>.SuccessResponse(new List<CustomerDto>()));
            _diseaseService.GetAllDiseasesAsync().Returns(ApiResponseDto<List<DiseaseDto>>.SuccessResponse(new List<DiseaseDto>()));
            _staffService.GetAllStaffAsync().Returns(ApiResponseDto<List<StaffDto>>.SuccessResponse(new List<StaffDto>()));

            // Act
            var result = await _controller.Edit(projectId, viewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task DeleteConfirmed_WithValidId_ReturnsJsonSuccessTrue()
        {
            // Arrange
            var projectId = "P001";
            var serviceResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _projectService.DeleteProjectAsync(projectId).Returns(serviceResponse);

            // Act
            var result = await _controller.DeleteConfirmed(projectId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.True(root.GetProperty("success").GetBoolean());
            Assert.Equal("Project deleted successfully!", root.GetProperty("message").GetString());
        }

        [Fact]
        public async Task DeleteConfirmed_WithValidId_ReturnsSuccessJson()
        {
            // Arrange
            var projectId = "P001";
            var serviceResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _projectService.DeleteProjectAsync(projectId).Returns(serviceResponse);

            // Act
            var result = await _controller.DeleteConfirmed(projectId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("Project deleted successfully!", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task DeleteConfirmed_WithNullId_ReturnsFailureJson()
        {
            // Act
            var result = await _controller.DeleteConfirmed(null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Project ID is required.", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task DeleteConfirmed_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var projectId = "P001";
            var errors = new List<ApiErrorDto> { new() { Message = "Delete failed", Code = "DELETE_ERROR" } };
            var serviceResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _projectService.DeleteProjectAsync(projectId).Returns(serviceResponse);

            // Act
            var result = await _controller.DeleteConfirmed(projectId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Contains("Delete failed", value.GetProperty("message").GetString());
        }

        #endregion

        #region Copy Tests

        [Fact]
        public async Task Copy_WithValidId_ReturnsSuccessJson()
        {
            // Arrange
            var sourceId = "P001";
            var newId = "P002";
            var nextIdResponse = ApiResponseDto<string>.SuccessResponse(newId);
            var copyResponse = ApiResponseDto<ProjectDto>.SuccessResponse(new ProjectDto { ProjectId = newId });

            _projectService.GetNextProjectNumberAsync(sourceId).Returns(nextIdResponse);
            _projectService.CopyProjectAsync(sourceId, newId).Returns(copyResponse);

            // Act
            var result = await _controller.Copy(sourceId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal(newId, value.GetProperty("generatedId").GetString());
        }

        [Fact]
        public async Task Copy_WhenGenerateIdFails_ReturnsFailureJson()
        {
            // Arrange
            var sourceId = "P001";
            var errors = new List<ApiErrorDto> { new() { Message = "Failed to generate ID", Code = "ID_ERROR" } };
            var nextIdResponse = ApiResponseDto<string>.FailureResponse(errors, new ApiMetaDto());

            _projectService.GetNextProjectNumberAsync(sourceId).Returns(nextIdResponse);

            // Act
            var result = await _controller.Copy(sourceId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Copy_WhenCopyFails_ReturnsFailureJson()
        {
            // Arrange
            var sourceId = "P001";
            var newId = "P002";
            var nextIdResponse = ApiResponseDto<string>.SuccessResponse(newId);
            var errors = new List<ApiErrorDto> { new() { Message = "Copy failed", Code = "COPY_ERROR" } };
            var copyResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());

            _projectService.GetNextProjectNumberAsync(sourceId).Returns(nextIdResponse);
            _projectService.CopyProjectAsync(sourceId, newId).Returns(copyResponse);

            // Act
            var result = await _controller.Copy(sourceId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region Recost Tests

        [Fact]
        public async Task Recost_WithValidId_ReturnsSuccessJson()
        {
            // Arrange
            var projectId = "P001";
            var serviceResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _projectService.RecostProjectAsync(projectId).Returns(serviceResponse);

            // Act
            var result = await _controller.Recost(projectId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Recost_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var projectId = "P001";
            var serviceResponse = ApiResponseDto<bool>.SuccessResponse(false);

            _projectService.RecostProjectAsync(projectId).Returns(serviceResponse);

            // Act
            var result = await _controller.Recost(projectId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region OnCustomerChange Tests

        [Fact]
        public async Task OnCustomerChange_WithValidCustomer_ReturnsProgramsJson()
        {
            // Arrange
            var customerName = "Customer1";
            var programs = new List<ProgramDto>
            {
                new() { ProgramNo = "PR001", ProgramName = "Program 1", Customer = customerName },
                new() { ProgramNo = "PR002", ProgramName = "Program 2", Customer = customerName }
            };
            var serviceResponse = ApiResponseDto<List<ProgramDto>>.SuccessResponse(programs);

            _programService.GetAllProgramsAsync().Returns(serviceResponse);

            // Act
            var result = await _controller.OnCustomerChange(customerName);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            var programsArray = value.GetProperty("programs");
            Assert.Equal(2, programsArray.GetArrayLength());
        }

        [Fact]
        public async Task OnCustomerChange_WhenServiceFails_ReturnsEmptyProgramsList()
        {
            // Arrange
            var customerName = "Customer1";
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var serviceResponse = ApiResponseDto<List<ProgramDto>>.FailureResponse(errors, new ApiMetaDto());

            _programService.GetAllProgramsAsync().Returns(serviceResponse);

            // Act
            var result = await _controller.OnCustomerChange(customerName);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            var programsArray = value.GetProperty("programs");
            Assert.Equal(0, programsArray.GetArrayLength());
        }

        #endregion
    }
}
