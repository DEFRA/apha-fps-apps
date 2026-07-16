using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Controllers;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.ProjectMaintenanceControllerTest
{
    public class ProjectMaintenanceControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly IProjectJobCodeService _jobCodeService;
        private readonly IPactTimeCodeValidService _timeCodeService;
        private readonly IProgramService _programService;
        private readonly IEmployeeService _employeeService;
        private readonly ProjectMaintenanceController _controller;

        public ProjectMaintenanceControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _projectService = Substitute.For<IProjectService>();
            _jobCodeService = Substitute.For<IProjectJobCodeService>();
            _timeCodeService = Substitute.For<IPactTimeCodeValidService>();
            _programService = Substitute.For<IProgramService>();
            _employeeService = Substitute.For<IEmployeeService>();
            _controller = new ProjectMaintenanceController(
                _mapper,
                _projectService,
                _jobCodeService,
                _timeCodeService,
                _programService,
                _employeeService);

            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Substitute.For<ITempDataProvider>());
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private void SetupProjectPagedGridMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<PactProjectViewModel>>(Arg.Any<List<ProjectDto>>())
                .Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
        }

        private void SetupJobCodePagedGridMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ProjectJobCodeViewModel>>(Arg.Any<List<JobCodeDto>>())
                .Returns([]);
            _mapper.Map<List<JobCodeViewModel>>(Arg.Any<List<JobCodeDto>>())
                .Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
        }

        private void SetupDetailsDropdowns(string parentProject)
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _jobCodeService.GetPagedJobCodesAsync(Arg.Any<QueryParameters<string>>(), parentProject)
                .Returns(ApiResponseDto<List<JobCodeDto>>.SuccessResponse([], new PaginationDto()));
            _mapper.Map<List<JobCodeViewModel>>(Arg.Any<List<JobCodeDto>>()).Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            _jobCodeService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([]));
            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse([]));
            _projectService.GetAllStatusesAsync()
                .Returns(ApiResponseDto<List<StatusDto>>.SuccessResponse([]));
            _projectService.GetAllDiseasesAsync()
                .Returns(ApiResponseDto<List<DiseaseDto>>.SuccessResponse([]));
            _projectService.GetAllCustomersAsync()
                .Returns(ApiResponseDto<List<CustomerDto>>.SuccessResponse([]));
            _projectService.GetAllPactContractsAsync()
                .Returns(ApiResponseDto<List<ContractDto>>.SuccessResponse([]));
            _employeeService.GetAllPactManagersAsync()
                .Returns(ApiResponseDto<List<ManagerDto>>.SuccessResponse([]));
        }

        #region Index

        [Fact]
        public async Task Index_Always_ReturnsViewResultWithProjectGrid()
        {
            // Arrange
            _projectService.GetPagedPactProjectsAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                    [new ProjectDto { ParentProject = "PRJ001", ProjectTitle = "Test" }],
                    new PaginationDto()));
            SetupProjectPagedGridMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectListViewModel>(viewResult.Model);
            Assert.Equal("projectGrid", model.ProjectGrid.GridId);
        }

        [Fact]
        public async Task Index_ServiceReturnsEmpty_ReturnsViewResultWithEmptyGrid()
        {
            // Arrange
            _projectService.GetPagedPactProjectsAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse([], new PaginationDto()));
            SetupProjectPagedGridMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ProjectListViewModel>(viewResult.Model);
        }

        #endregion

        #region LoadProjectGrid

        [Fact]
        public async Task LoadProjectGrid_ViewBy1_ReturnsPartialViewWithProjectGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _projectService.GetPagedPactProjectsAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse([], new PaginationDto()));
            SetupProjectPagedGridMapper();

            // Act
            var result = await _controller.LoadProjectGrid(request, viewBy: 1);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<PactProjectViewModel>>(partial.Model);
        }

        [Fact]
        public async Task LoadProjectGrid_ViewBy2_ReturnsPartialViewWithJobCodeGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _jobCodeService.GetPagedJobCodesAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<JobCodeDto>>.SuccessResponse([], new PaginationDto()));
            SetupJobCodePagedGridMapper();

            // Act
            var result = await _controller.LoadProjectGrid(request, viewBy: 2);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<ProjectJobCodeViewModel>>(partial.Model);
        }

        [Fact]
        public async Task LoadProjectGrid_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "Test error");

            // Act
            var result = await _controller.LoadProjectGrid(new PaginationFilter<string> { Filter = "{}" }, viewBy: 1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region Details

        [Fact]
        public async Task Details_ProjectExists_ReturnsViewResultWithViewModel()
        {
            // Arrange
            const string parentProject = "PRJ001";
            var project = new ProjectDto { ParentProject = parentProject, ProjectTitle = "Test Project" };
            _projectService.GetProjectByIdAsync(parentProject)
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(project));
            _mapper.Map<PactProjectViewModel>(project)
                .Returns(new PactProjectViewModel { ParentProject = parentProject, ProjectTitle = "Test Project" });
            SetupDetailsDropdowns(parentProject);

            // Act
            var result = await _controller.Details(parentProject);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectMaintenanceViewModel>(viewResult.Model);
            Assert.Equal(parentProject, model.Project.ParentProject);
        }

        [Fact]
        public async Task Details_ProjectNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            _projectService.GetProjectByIdAsync("MISSING")
                .Returns(ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Details("MISSING");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ProjectDataIsNull_ReturnsNotFoundResult()
        {
            // Arrange
            _projectService.GetProjectByIdAsync("PRJ001")
                .Returns(new ApiResponseDto<ProjectDto> { Success = true, Data = null });

            // Act
            var result = await _controller.Details("PRJ001");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region LoadJobCodeGrid

        [Fact]
        public async Task LoadJobCodeGrid_ValidRequest_ReturnsPartialViewWithJobCodeGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _jobCodeService.GetPagedJobCodesAsync(Arg.Any<QueryParameters<string>>(), "PRJ001")
                .Returns(ApiResponseDto<List<JobCodeDto>>.SuccessResponse([], new PaginationDto()));
            SetupJobCodePagedGridMapper();

            // Act
            var result = await _controller.LoadJobCodeGrid(request, "PRJ001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<JobCodeViewModel>>(partial.Model);
        }

        [Fact]
        public async Task LoadJobCodeGrid_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "error");

            // Act
            var result = await _controller.LoadJobCodeGrid(new PaginationFilter<string> { Filter = "{}" }, "PRJ001");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadJobCodeGrid_EmptyParentProject_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.LoadJobCodeGrid(new PaginationFilter<string> { Filter = "{}" }, string.Empty);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region LoadTimeCodeGrid

        [Fact]
        public async Task LoadTimeCodeGrid_ValidRequest_ReturnsPartialViewWithTimeCodeGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()).Returns(new QueryParameters<string>());
            _timeCodeService.GetPagedTimeCodesAsync(Arg.Any<QueryParameters<string>>(), "JC1", "PRJ001")
                .Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse([], new PaginationDto()));
            _mapper.Map<List<TimeCodeViewModel>>(Arg.Any<List<TimeCodeValidDto>>()).Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadTimeCodeGrid(request, "PRJ001", "JC1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<TimeCodeViewModel>>(partial.Model);
        }

        [Fact]
        public async Task LoadTimeCodeGrid_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "error");

            // Act
            var result = await _controller.LoadTimeCodeGrid(new PaginationFilter<string> { Filter = "{}" }, "PRJ001", "JC1");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadTimeCodeGrid_EmptyParentProject_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.LoadTimeCodeGrid(new PaginationFilter<string> { Filter = "{}" }, string.Empty, "JC1");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadTimeCodeGrid_EmptyJobCodeId_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.LoadTimeCodeGrid(new PaginationFilter<string> { Filter = "{}" }, "PRJ001", string.Empty);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region Edit (Project)

        [Fact]
        public async Task Edit_Get_ProjectExists_ReturnsPartialViewWithModel()
        {
            // Arrange
            var project = new ProjectDto { ParentProject = "PRJ001", ProjectTitle = "Test" };
            var viewModel = new PactProjectViewModel { ParentProject = "PRJ001" };
            _projectService.GetProjectByIdAsync("PRJ001")
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(project));
            _mapper.Map<PactProjectViewModel>(project).Returns(viewModel);

            // Act
            var result = await _controller.Edit("PRJ001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditProject", partial.ViewName);
            Assert.IsType<PactProjectViewModel>(partial.Model);
        }

        [Fact]
        public async Task Edit_Get_ProjectNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found" } };
            _projectService.GetProjectByIdAsync("MISSING")
                .Returns(ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Edit("MISSING");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_ReturnsJsonSuccess()
        {
            // Arrange
            var model = new PactProjectViewModel { ParentProject = "PRJ001", ProjectTitle = "Updated" };
            var dto = new ProjectDto { ParentProject = "PRJ001" };
            _mapper.Map<ProjectDto>(model).Returns(dto);
            _projectService.UpdatePactProjectAsync(dto)
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Edit_Post_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var model = new PactProjectViewModel { ParentProject = "PRJ001" };
            var dto = new ProjectDto { ParentProject = "PRJ001" };
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed" } };
            _mapper.Map<ProjectDto>(model).Returns(dto);
            _projectService.UpdatePactProjectAsync(dto)
                .Returns(ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Edit_Post_InvalidModelState_ReturnsJsonError()
        {
            // Arrange
            _controller.ModelState.AddModelError("ProjectTitle", "Required");

            // Act
            var result = await _controller.Edit(new PactProjectViewModel());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region Delete (Project)

        [Fact]
        public async Task Delete_ProjectExists_ReturnsJsonSuccess()
        {
            // Arrange
            _projectService.DeleteProjectAsync("PRJ001")
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.Delete("PRJ001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Delete_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Delete failed" } };
            _projectService.DeleteProjectAsync("PRJ001")
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Delete("PRJ001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region CreateJobCode

        [Fact]
        public async Task CreateJobCode_Get_ReturnsPartialViewWithModel()
        {
            // Arrange
            _jobCodeService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([]));
            _jobCodeService.GetTypesAsync()
                .Returns(ApiResponseDto<List<string>>.SuccessResponse([]));

            // Act
            var result = await _controller.CreateJobCode("PRJ001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditJobCode", partial.ViewName);
            var model = Assert.IsType<JobCodeViewModel>(partial.Model);
            Assert.Equal("PRJ001", model.ParentProject);
        }

        [Fact]
        public async Task CreateJobCode_Post_ValidModel_ReturnsJsonSuccess()
        {
            // Arrange
            var model = new JobCodeViewModel { JobCodeId = "JC1", ParentProject = "PRJ001" };
            var dto = new JobCodeDto { JobCodeId = "JC1" };
            _mapper.Map<JobCodeDto>(model).Returns(dto);
            _jobCodeService.CreateJobCodeAsync(dto)
                .Returns(ApiResponseDto<JobCodeDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.CreateJobCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CreateJobCode_Post_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var model = new JobCodeViewModel { JobCodeId = "JC1" };
            var dto = new JobCodeDto { JobCodeId = "JC1" };
            var errors = new List<ApiErrorDto> { new() { Message = "Create failed" } };
            _mapper.Map<JobCodeDto>(model).Returns(dto);
            _jobCodeService.CreateJobCodeAsync(dto)
                .Returns(ApiResponseDto<JobCodeDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.CreateJobCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CreateJobCode_Post_InvalidModelState_ReturnsJsonError()
        {
            // Arrange
            _controller.ModelState.AddModelError("JobCodeId", "Required");

            // Act
            var result = await _controller.CreateJobCode(new JobCodeViewModel());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region EditJobCode

        [Fact]
        public async Task EditJobCode_Get_JobCodeExists_ReturnsPartialViewWithModel()
        {
            // Arrange
            var dto = new JobCodeDto { JobCodeId = "JC1", ParentProject = "PRJ001" };
            var viewModel = new JobCodeViewModel { JobCodeId = "JC1" };
            _jobCodeService.GetJobCodeByIdAsync("JC1")
                .Returns(ApiResponseDto<JobCodeDto>.SuccessResponse(dto));
            _jobCodeService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([]));
            _jobCodeService.GetTypesAsync()
                .Returns(ApiResponseDto<List<string>>.SuccessResponse([]));
            _mapper.Map<JobCodeViewModel>(dto).Returns(viewModel);

            // Act
            var result = await _controller.EditJobCode("JC1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditJobCode", partial.ViewName);
            Assert.IsType<JobCodeViewModel>(partial.Model);
        }

        [Fact]
        public async Task EditJobCode_Get_JobCodeNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found" } };
            _jobCodeService.GetJobCodeByIdAsync("MISSING")
                .Returns(ApiResponseDto<JobCodeDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.EditJobCode("MISSING");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditJobCode_Post_ValidModel_ReturnsJsonSuccess()
        {
            // Arrange
            var model = new JobCodeViewModel { JobCodeId = "JC1" };
            var dto = new JobCodeDto { JobCodeId = "JC1" };
            _mapper.Map<JobCodeDto>(model).Returns(dto);
            _jobCodeService.UpdateJobCodeAsync(dto)
                .Returns(ApiResponseDto<JobCodeDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.EditJobCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task EditJobCode_Post_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var model = new JobCodeViewModel { JobCodeId = "JC1" };
            var dto = new JobCodeDto { JobCodeId = "JC1" };
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed" } };
            _mapper.Map<JobCodeDto>(model).Returns(dto);
            _jobCodeService.UpdateJobCodeAsync(dto)
                .Returns(ApiResponseDto<JobCodeDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.EditJobCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task EditJobCode_Post_InvalidModelState_ReturnsJsonError()
        {
            // Arrange
            _controller.ModelState.AddModelError("JobCodeId", "Required");

            // Act
            var result = await _controller.EditJobCode(new JobCodeViewModel());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region DeleteJobCode

        [Fact]
        public async Task DeleteJobCode_JobCodeExists_ReturnsJsonSuccess()
        {
            // Arrange
            _jobCodeService.DeleteJobCodeAsync("JC1")
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeleteJobCode("JC1", "PRJ001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            await _jobCodeService.Received(1).DeleteJobCodeAsync("JC1");
        }

        [Fact]
        public async Task DeleteJobCode_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Delete failed" } };
            _jobCodeService.DeleteJobCodeAsync("JC1")
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.DeleteJobCode("JC1", "PRJ001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteJobCode_HasRelatedTimeCodeValidRecords_ReturnsJsonFailure()
        {
            // Arrange — API returns 409 BUSINESS_RULE_VIOLATION when related TimeCodeValid records exist
            var errors = new List<ApiErrorDto>
            {
                new() { Code = "BUSINESS_RULE_VIOLATION", Message = "This JobCode has related records in TimeCodeValid and cannot be deleted." }
            };
            _jobCodeService.DeleteJobCodeAsync("JC1")
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.DeleteJobCode("JC1", "PRJ001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            await _timeCodeService.DidNotReceive().DeleteAllByJobCodeAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        #endregion

        #region CreateTimeCode

        [Fact]
        public async Task CreateTimeCode_Get_ReturnsPartialViewWithModel()
        {
            // Arrange
            _jobCodeService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([]));

            // Act
            var result = await _controller.CreateTimeCode("PRJ001", "JC1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditTimeCode", partial.ViewName);
            var model = Assert.IsType<TimeCodeViewModel>(partial.Model);
            Assert.Equal("PRJ001", model.ParentProject);
            Assert.Equal("JC1", model.JobCode);
        }

        [Fact]
        public async Task CreateTimeCode_Get_SetsTimeCodeEqualToJobCode()
        {
            // Arrange
            _jobCodeService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([]));

            // Act
            var result = await _controller.CreateTimeCode("PRJ001", "JC001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<TimeCodeViewModel>(partial.Model);
            Assert.Equal("JC001", model.TimeCode);
            Assert.Equal("JC001", model.JobCode);
        }

        [Fact]
        public async Task CreateTimeCode_Post_ValidModel_ReturnsJsonSuccess()
        {
            // Arrange
            var model = new TimeCodeViewModel { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ001" };
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ001" };
            _mapper.Map<TimeCodeValidDto>(model).Returns(dto);
            _timeCodeService.CreateTimeCodeValidAsync(dto)
                .Returns(ApiResponseDto<TimeCodeValidDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.CreateTimeCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CreateTimeCode_Post_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var model = new TimeCodeViewModel { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ001" };
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ001" };
            var errors = new List<ApiErrorDto> { new() { Message = "Create failed" } };
            _mapper.Map<TimeCodeValidDto>(model).Returns(dto);
            _timeCodeService.CreateTimeCodeValidAsync(dto)
                .Returns(ApiResponseDto<TimeCodeValidDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.CreateTimeCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CreateTimeCode_Post_InvalidModelState_ReturnsJsonError()
        {
            // Arrange
            _controller.ModelState.AddModelError("TimeCode", "Required");

            // Act
            var result = await _controller.CreateTimeCode(new TimeCodeViewModel());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region EditTimeCode

        [Fact]
        public async Task EditTimeCode_Get_TimeCodeFound_ReturnsPartialViewWithModel()
        {
            // Arrange
            var timeCodes = new List<TimeCodeValidDto>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ001", JobCode = "JC1" }
            };
            _timeCodeService.GetByJobCodeAsync("JC1", "PRJ001")
                .Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(timeCodes));
            _jobCodeService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([]));
            _mapper.Map<TimeCodeViewModel>(timeCodes[0])
                .Returns(new TimeCodeViewModel { TimeCode = "TC1", WorkGroup = "WG1" });

            // Act
            var result = await _controller.EditTimeCode("WG1", "TC1", "JC1", "PRJ001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditTimeCode", partial.ViewName);
            Assert.IsType<TimeCodeViewModel>(partial.Model);
        }

        [Fact]
        public async Task EditTimeCode_Get_TimeCodeFound_SetsOriginalWorkGroup()
        {
            // Arrange
            var timeCodes = new List<TimeCodeValidDto>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ001", JobCode = "JC1" }
            };
            _timeCodeService.GetByJobCodeAsync("JC1", "PRJ001")
                .Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(timeCodes));
            _jobCodeService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([]));
            _mapper.Map<TimeCodeViewModel>(timeCodes[0])
                .Returns(new TimeCodeViewModel { TimeCode = "TC1", WorkGroup = "WG1" });

            // Act
            var result = await _controller.EditTimeCode("WG1", "TC1", "JC1", "PRJ001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<TimeCodeViewModel>(partial.Model);
            Assert.Equal("WG1", model.OriginalWorkGroup);
        }

        [Fact]
        public async Task EditTimeCode_Get_NullWorkGroup_FindsByTimeCodeOnly()
        {
            // Arrange
            var timeCodes = new List<TimeCodeValidDto>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ001", JobCode = "JC1" }
            };
            _timeCodeService.GetByJobCodeAsync("JC1", "PRJ001")
                .Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(timeCodes));
            _jobCodeService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([]));
            _mapper.Map<TimeCodeViewModel>(timeCodes[0])
                .Returns(new TimeCodeViewModel { TimeCode = "TC1" });

            // Act
            var result = await _controller.EditTimeCode(null, "TC1", "JC1", "PRJ001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditTimeCode", partial.ViewName);
        }

        [Fact]
        public async Task EditTimeCode_Get_TimeCodeNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            _timeCodeService.GetByJobCodeAsync("JC1", "PRJ001")
                .Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse([]));

            // Act
            var result = await _controller.EditTimeCode("WG1", "TC1", "JC1", "PRJ001");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditTimeCode_Post_ValidModel_ReturnsJsonSuccess()
        {
            // Arrange
            var model = new TimeCodeViewModel { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ001" };
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ001" };
            _mapper.Map<TimeCodeValidDto>(model).Returns(dto);
            _timeCodeService.UpdateTimeCodeValidAsync(dto)
                .Returns(ApiResponseDto<TimeCodeValidDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.EditTimeCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task EditTimeCode_Post_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var model = new TimeCodeViewModel { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ001" };
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ001" };
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed" } };
            _mapper.Map<TimeCodeValidDto>(model).Returns(dto);
            _timeCodeService.UpdateTimeCodeValidAsync(dto)
                .Returns(ApiResponseDto<TimeCodeValidDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.EditTimeCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task EditTimeCode_Post_InvalidModelState_ReturnsJsonError()
        {
            // Arrange
            _controller.ModelState.AddModelError("TimeCode", "Required");

            // Act
            var result = await _controller.EditTimeCode(new TimeCodeViewModel());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task EditTimeCode_Post_WorkGroupChanged_DeleteAndCreateSucceed_ReturnsJsonSuccess()
        {
            // Arrange — OriginalWorkGroup differs from WorkGroup → delete+create path
            var model = new TimeCodeViewModel
            {
                TimeCode = "TC1",
                WorkGroup = "WG_NEW",
                OriginalWorkGroup = "WG_OLD",
                ParentProject = "PRJ001"
            };
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG_NEW", ParentProject = "PRJ001" };
            _timeCodeService.DeleteTimeCodeValidAsync("WG_OLD", "TC1", "PRJ001")
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));
            _mapper.Map<TimeCodeValidDto>(model).Returns(dto);
            _timeCodeService.CreateTimeCodeValidAsync(dto)
                .Returns(ApiResponseDto<TimeCodeValidDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.EditTimeCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            await _timeCodeService.Received(1).DeleteTimeCodeValidAsync("WG_OLD", "TC1", "PRJ001");
            await _timeCodeService.Received(1).CreateTimeCodeValidAsync(dto);
            await _timeCodeService.DidNotReceive().UpdateTimeCodeValidAsync(Arg.Any<TimeCodeValidDto>());
        }

        [Fact]
        public async Task EditTimeCode_Post_WorkGroupChanged_DeleteFails_ReturnsJsonFailure()
        {
            // Arrange
            var model = new TimeCodeViewModel
            {
                TimeCode = "TC1",
                WorkGroup = "WG_NEW",
                OriginalWorkGroup = "WG_OLD",
                ParentProject = "PRJ001"
            };
            var errors = new List<ApiErrorDto> { new() { Message = "Delete failed", Code = "DELETE_ERROR" } };
            _timeCodeService.DeleteTimeCodeValidAsync("WG_OLD", "TC1", "PRJ001")
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.EditTimeCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            await _timeCodeService.DidNotReceive().CreateTimeCodeValidAsync(Arg.Any<TimeCodeValidDto>());
        }

        [Fact]
        public async Task EditTimeCode_Post_WorkGroupChanged_CreateFails_ReturnsJsonFailure()
        {
            // Arrange
            var model = new TimeCodeViewModel
            {
                TimeCode = "TC1",
                WorkGroup = "WG_NEW",
                OriginalWorkGroup = "WG_OLD",
                ParentProject = "PRJ001"
            };
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG_NEW", ParentProject = "PRJ001" };
            _timeCodeService.DeleteTimeCodeValidAsync("WG_OLD", "TC1", "PRJ001")
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));
            _mapper.Map<TimeCodeValidDto>(model).Returns(dto);
            var errors = new List<ApiErrorDto> { new() { Message = "Create failed", Code = "CREATE_ERROR" } };
            _timeCodeService.CreateTimeCodeValidAsync(dto)
                .Returns(ApiResponseDto<TimeCodeValidDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.EditTimeCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task EditTimeCode_Post_WorkGroupUnchanged_CallsUpdateNotDeleteCreate()
        {
            // Arrange — same WorkGroup → standard update path
            var model = new TimeCodeViewModel
            {
                TimeCode = "TC1",
                WorkGroup = "WG1",
                OriginalWorkGroup = "WG1",
                ParentProject = "PRJ001"
            };
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ001" };
            _mapper.Map<TimeCodeValidDto>(model).Returns(dto);
            _timeCodeService.UpdateTimeCodeValidAsync(dto)
                .Returns(ApiResponseDto<TimeCodeValidDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.EditTimeCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            await _timeCodeService.Received(1).UpdateTimeCodeValidAsync(dto);
            await _timeCodeService.DidNotReceive().DeleteTimeCodeValidAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            await _timeCodeService.DidNotReceive().CreateTimeCodeValidAsync(Arg.Any<TimeCodeValidDto>());
        }

        #endregion

        #region DeleteTimeCode

        [Fact]
        public async Task DeleteTimeCode_TimeCodeExists_ReturnsJsonSuccess()
        {
            // Arrange
            _timeCodeService.DeleteTimeCodeValidAsync("WG1", "TC1", "PRJ001")
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeleteTimeCode("WG1", "TC1", "PRJ001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteTimeCode_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Delete failed" } };
            _timeCodeService.DeleteTimeCodeValidAsync("WG1", "TC1", "PRJ001")
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.DeleteTimeCode("WG1", "TC1", "PRJ001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region CopyProjectJobCode

        [Fact]
        public async Task CopyProjectJobCode_Get_JobCodeExists_ReturnsPartialViewWithModel()
        {
            // Arrange
            var dto = new JobCodeDto { JobCodeId = "JC1", ParentProject = "PRJ001" };
            var viewModel = new JobCodeViewModel { JobCodeId = "JC1" };
            _jobCodeService.GetJobCodeByIdAsync("JC1")
                .Returns(ApiResponseDto<JobCodeDto>.SuccessResponse(dto));
            _jobCodeService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([]));
            _jobCodeService.GetTypesAsync()
                .Returns(ApiResponseDto<List<string>>.SuccessResponse([]));
            _mapper.Map<JobCodeViewModel>(dto).Returns(viewModel);

            // Act
            var result = await _controller.CopyProjectJobCode("PRJ001", "JC1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CopyJobCode", partial.ViewName);
            Assert.IsType<JobCodeViewModel>(partial.Model);
        }

        [Fact]
        public async Task CopyProjectJobCode_Get_JobCodeNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found" } };
            _jobCodeService.GetJobCodeByIdAsync("MISSING")
                .Returns(ApiResponseDto<JobCodeDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.CopyProjectJobCode("PRJ001", "MISSING");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CopyProjectJobCode_Post_ValidModelWithoutCopyWorkGroup_ReturnsJsonSuccess()
        {
            // Arrange
            var model = new CopyJobCodeRequest
            {
                SourceJobCode = "JC_SRC",
                JobCodeId = "JC_TGT",
                JobCodeWorkGroup = "WG1",
                ParentProject = "PRJ001",
                CopyWorkGroup = false
            };
            _jobCodeService.CreateJobCodeAsync(Arg.Any<JobCodeDto>())
                .Returns(ApiResponseDto<JobCodeDto>.SuccessResponse(new JobCodeDto { JobCodeId = "JC_TGT" }));

            // Act
            var result = await _controller.CopyProjectJobCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            await _timeCodeService.DidNotReceive().CopyWorkGroupAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task CopyProjectJobCode_Post_ValidModelWithCopyWorkGroup_ReturnsJsonSuccess()
        {
            // Arrange
            var model = new CopyJobCodeRequest
            {
                SourceJobCode = "JC_SRC",
                JobCodeId = "JC_TGT",
                JobCodeWorkGroup = "WG1",
                ParentProject = "PRJ001",
                CopyWorkGroup = true
            };
            _jobCodeService.CreateJobCodeAsync(Arg.Any<JobCodeDto>())
                .Returns(ApiResponseDto<JobCodeDto>.SuccessResponse(new JobCodeDto { JobCodeId = "JC_TGT" }));
            _timeCodeService.CopyWorkGroupAsync("JC_SRC", "JC_TGT", "PRJ001")
                .Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse([]));

            // Act
            var result = await _controller.CopyProjectJobCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            await _timeCodeService.Received(1).CopyWorkGroupAsync("JC_SRC", "JC_TGT", "PRJ001");
        }

        [Fact]
        public async Task CopyProjectJobCode_Post_CreateJobCodeFails_ReturnsJsonFailure()
        {
            // Arrange
            var model = new CopyJobCodeRequest
            {
                SourceJobCode = "JC_SRC",
                JobCodeId = "JC_TGT",
                JobCodeWorkGroup = "WG1",
                ParentProject = "PRJ001"
            };
            var errors = new List<ApiErrorDto> { new() { Message = "Create failed" } };
            _jobCodeService.CreateJobCodeAsync(Arg.Any<JobCodeDto>())
                .Returns(ApiResponseDto<JobCodeDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.CopyProjectJobCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CopyProjectJobCode_Post_CopyWorkGroupFails_ReturnsJsonFailure()
        {
            // Arrange
            var model = new CopyJobCodeRequest
            {
                SourceJobCode = "JC_SRC",
                JobCodeId = "JC_TGT",
                JobCodeWorkGroup = "WG1",
                ParentProject = "PRJ001",
                CopyWorkGroup = true
            };
            _jobCodeService.CreateJobCodeAsync(Arg.Any<JobCodeDto>())
                .Returns(ApiResponseDto<JobCodeDto>.SuccessResponse(new JobCodeDto { JobCodeId = "JC_TGT" }));
            var errors = new List<ApiErrorDto> { new() { Message = "Copy time codes failed" } };
            _timeCodeService.CopyWorkGroupAsync("JC_SRC", "JC_TGT", "PRJ001")
                .Returns(ApiResponseDto<List<TimeCodeValidDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.CopyProjectJobCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CopyProjectJobCode_Post_InvalidModelState_ReturnsJsonError()
        {
            // Arrange
            _controller.ModelState.AddModelError("JobCodeId", "Required");

            // Act
            var result = await _controller.CopyProjectJobCode(new CopyJobCodeRequest());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region CopyWorkGroupPartial

        [Fact]
        public async Task CopyWorkGroupPartial_Get_ValidParams_ReturnsPartialViewWithTargetJobCodes()
        {
            // Arrange
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _jobCodeService.GetPagedJobCodesAsync(Arg.Any<QueryParameters<string>>(), "PRJ001")
                .Returns(ApiResponseDto<List<JobCodeDto>>.SuccessResponse(
                    [new JobCodeDto { JobCodeId = "JC1" }, new JobCodeDto { JobCodeId = "JC2" }],
                    new PaginationDto()));

            // Act
            var result = await _controller.CopyWorkGroupPartial("PRJ001", "JC1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CopyWorkGroup", partial.ViewName);
        }

        [Fact]
        public async Task CopyWorkGroupPartial_Get_NoJobCodes_ReturnsPartialViewWithEmptyTargetJobCodes()
        {
            // Arrange
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _jobCodeService.GetPagedJobCodesAsync(Arg.Any<QueryParameters<string>>(), "PRJ001")
                .Returns(ApiResponseDto<List<JobCodeDto>>.SuccessResponse([], new PaginationDto()));

            // Act
            var result = await _controller.CopyWorkGroupPartial("PRJ001", "JC1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CopyWorkGroup", partial.ViewName);
        }

        #endregion

        #region CopyBulkWorkGroup

        [Fact]
        public async Task CopyBulkWorkGroup_ValidModel_ReturnsJsonSuccess()
        {
            // Arrange
            var model = new CopyBulkWorkGroupRequest
            {
                ParentProject = "PRJ001",
                SourceJobCodeId = "JC_SRC",
                TargetJobCodeId = "JC_TGT",
                WorkGroups = ["WG1", "WG2"]
            };
            _timeCodeService.CopySelectedWorkGroupsAsync(Arg.Any<BulkCopyWorkGroupRequestDto>())
                .Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse([]));

            // Act
            var result = await _controller.CopyBulkWorkGroup(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            await _timeCodeService.Received(1).CopySelectedWorkGroupsAsync(Arg.Any<BulkCopyWorkGroupRequestDto>());
        }

        [Fact]
        public async Task CopyBulkWorkGroup_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var model = new CopyBulkWorkGroupRequest
            {
                ParentProject = "PRJ001",
                SourceJobCodeId = "JC_SRC",
                TargetJobCodeId = "JC_TGT",
                WorkGroups = ["WG1"]
            };
            var errors = new List<ApiErrorDto> { new() { Message = "Copy failed", Code = "COPY_ERROR" } };
            _timeCodeService.CopySelectedWorkGroupsAsync(Arg.Any<BulkCopyWorkGroupRequestDto>())
                .Returns(ApiResponseDto<List<TimeCodeValidDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.CopyBulkWorkGroup(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CopyBulkWorkGroup_InvalidModelState_ReturnsJsonError()
        {
            // Arrange
            _controller.ModelState.AddModelError("TargetJobCodeId", "Required");

            // Act
            var result = await _controller.CopyBulkWorkGroup(new CopyBulkWorkGroupRequest());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region DeleteBulkTimeCode

        [Fact]
        public async Task DeleteBulkTimeCode_ValidModel_ReturnsJsonSuccess()
        {
            // Arrange
            var model = new BulkDeleteTimeCodeRequest
            {
                ParentProject = "PRJ001",
                Items = [new TimeCodeKeyItemRequest { WorkGroup = "WG1", TimeCode = "TC1" }]
            };
            _timeCodeService.DeleteBulkAsync(Arg.Any<BulkDeleteTimeCodeRequestDto>())
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeleteBulkTimeCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            await _timeCodeService.Received(1).DeleteBulkAsync(Arg.Any<BulkDeleteTimeCodeRequestDto>());
        }

        [Fact]
        public async Task DeleteBulkTimeCode_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var model = new BulkDeleteTimeCodeRequest
            {
                ParentProject = "PRJ001",
                Items = [new TimeCodeKeyItemRequest { WorkGroup = "WG1", TimeCode = "TC1" }]
            };
            var errors = new List<ApiErrorDto> { new() { Message = "Bulk delete failed", Code = "API_ERROR" } };
            _timeCodeService.DeleteBulkAsync(Arg.Any<BulkDeleteTimeCodeRequestDto>())
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.DeleteBulkTimeCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteBulkTimeCode_InvalidModelState_ReturnsJsonError()
        {
            // Arrange
            _controller.ModelState.AddModelError("ParentProject", "Required");

            // Act
            var result = await _controller.DeleteBulkTimeCode(new BulkDeleteTimeCodeRequest());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region DeleteAllJobCodeTimeCodes

        [Fact]
        public async Task DeleteAllJobCodeTimeCodes_ServiceSucceeds_ReturnsJsonSuccess()
        {
            // Arrange
            _timeCodeService.DeleteAllByJobCodeAsync("JC1", "PRJ001")
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeleteAllJobCodeTimeCodes("PRJ001", "JC1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteAllJobCodeTimeCodes_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Delete failed", Code = "API_ERROR" } };
            _timeCodeService.DeleteAllByJobCodeAsync("JC1", "PRJ001")
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.DeleteAllJobCodeTimeCodes("PRJ001", "JC1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region CopyAllJobCodeWorkGroups

        [Fact]
        public async Task CopyAllJobCodeWorkGroups_ServiceSucceeds_ReturnsJsonSuccess()
        {
            // Arrange
            _timeCodeService.CopyWorkGroupAsync("JC_SRC", "JC_TGT", "PRJ001")
                .Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse([]));

            // Act
            var result = await _controller.CopyAllJobCodeWorkGroups("PRJ001", "JC_SRC", "JC_TGT");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CopyAllJobCodeWorkGroups_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Copy failed", Code = "COPY_ERROR" } };
            _timeCodeService.CopyWorkGroupAsync("JC_SRC", "JC_TGT", "PRJ001")
                .Returns(ApiResponseDto<List<TimeCodeValidDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.CopyAllJobCodeWorkGroups("PRJ001", "JC_SRC", "JC_TGT");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion
    }
}