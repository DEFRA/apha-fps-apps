using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ProjectControllerTest
{
    public class ProjectControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly IProgramService _programService;
        private readonly ProjectController _controller;

        public ProjectControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _projectService = Substitute.For<IProjectService>();
            _programService = Substitute.For<IProgramService>();
            _controller = new ProjectController(_mapper, _projectService, _programService);

            // Setup Url helper for actions that use Url.Action
            var urlHelper = Substitute.For<IUrlHelper>();
            urlHelper.Action(Arg.Any<UrlActionContext>()).Returns("http://test/url");
            _controller.Url = urlHelper;
        }

        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json);
        }

        #region Index Tests

        [Fact]
        public void Index_ReturnsViewResult()
        {
            var result = _controller.Index();

            Assert.IsType<ViewResult>(result);
        }

        #endregion

        #region Add GET Tests

        [Fact]
        public async Task Add_Get_ReturnsViewResult_WithModel()
        {
            SetupDropdownMocks();

            var result = await _controller.Add();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("ProjectAddEdit", viewResult.ViewName);
            var model = Assert.IsType<ProgrammeNewProjectViewModel>(viewResult.Model);
            Assert.Equal("Not Specified", model.Disease);
        }

        [Fact]
        public async Task Add_Get_SetsIsEditModeFalse()
        {
            SetupDropdownMocks();

            var result = await _controller.Add();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False((bool)viewResult.ViewData["IsEditMode"]!);
        }

        [Fact]
        public async Task Add_Get_SetsIsDefraProjectToUnselectedSentinel()
        {
            SetupDropdownMocks();

            var result = await _controller.Add();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgrammeNewProjectViewModel>(viewResult.Model);
            // Sentinel value (matches no dropdown option) keeps the "Select" placeholder selected in Add mode
            Assert.Equal((short)-99, model.IsDefraProject);
            Assert.DoesNotContain(model.IsDefraProjectList, i => i.Selected);
        }

        #endregion

        #region Add POST Tests

        [Fact]
        public async Task Add_Post_WithValidModel_ReturnsSuccessJson()
        {
            var model = CreateValidProjectViewModel();
            var dto = new ProjectDto { ParentProject = "PP001" };
            var apiResponse = ApiResponseDto<ProjectDto>.SuccessResponse(dto);

            _mapper.Map<ProjectDto>(model).Returns(dto);
            _projectService.CreateProjectAsync(dto).Returns(apiResponse);

            var result = await _controller.Add(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.True(value!.success);
            Assert.Equal("Project created successfully.", value.message);
        }

        [Fact]
        public async Task Add_Post_WithInvalidModelState_ReturnsJsonError()
        {
            var model = CreateValidProjectViewModel();
            _controller.ModelState.AddModelError("ParentProject", "Required");

            var result = await _controller.Add(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
            Assert.Equal("Please correct the errors below.", value.message);
        }

        [Fact]
        public async Task Add_Post_WhenServiceFails_ReturnsJsonError()
        {
            var model = CreateValidProjectViewModel();
            var dto = new ProjectDto { ParentProject = "PP001" };
            var errors = new List<ApiErrorDto> { new() { Message = "Create failed", Code = "CREATE_ERROR" } };
            var apiResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<ProjectDto>(model).Returns(dto);
            _projectService.CreateProjectAsync(dto).Returns(apiResponse);

            var result = await _controller.Add(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
            Assert.Equal("Create failed", value.message);
        }

        [Fact]
        public async Task Add_Post_CallsMapperAndService()
        {
            var model = CreateValidProjectViewModel();
            var dto = new ProjectDto { ParentProject = "PP001" };
            var apiResponse = ApiResponseDto<ProjectDto>.SuccessResponse(dto);

            _mapper.Map<ProjectDto>(model).Returns(dto);
            _projectService.CreateProjectAsync(dto).Returns(apiResponse);

            await _controller.Add(model);

            _mapper.Received(1).Map<ProjectDto>(model);
            await _projectService.Received(1).CreateProjectAsync(dto);
        }

        #endregion

        #region Edit GET Tests

        [Fact]
        public async Task Edit_Get_WithValidProject_ReturnsViewResult()
        {
            var dto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "Test" };
            var model = new ProgrammeNewProjectViewModel { ParentProject = "PP001" };
            var apiResponse = ApiResponseDto<ProjectDto>.SuccessResponse(dto);

            _projectService.GetProgrammeNewProjectByIdAsync("PP001").Returns(apiResponse);
            _mapper.Map<ProgrammeNewProjectViewModel>(dto).Returns(model);
            SetupDropdownMocks();

            var result = await _controller.Edit("PP001");

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("ProjectAddEdit", viewResult.ViewName);
            Assert.True((bool)viewResult.ViewData["IsEditMode"]!);
        }

        [Fact]
        public async Task Edit_Get_WithNonExistentProject_ReturnsNotFound()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Not found" } };
            var apiResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());

            _projectService.GetProgrammeNewProjectByIdAsync("NOPE").Returns(apiResponse);

            var result = await _controller.Edit("NOPE");

            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region Edit POST Tests

        [Fact]
        public async Task Edit_Post_WithValidModel_ReturnsSuccessJson()
        {
            var model = CreateValidProjectViewModel();
            var dto = new ProjectDto { ParentProject = "PP001" };
            var apiResponse = ApiResponseDto<ProjectDto>.SuccessResponse(dto);

            _mapper.Map<ProjectDto>(model).Returns(dto);
            _projectService.UpdateProjectAsync("PP001", dto).Returns(apiResponse);

            var result = await _controller.Edit("PP001", model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.True(value!.success);
            Assert.Equal("Project updated successfully.", value.message);
        }

        [Fact]
        public async Task Edit_Post_WithInvalidModelState_ReturnsJsonError()
        {
            var model = CreateValidProjectViewModel();
            _controller.ModelState.AddModelError("ProjectTitle", "Required");

            var result = await _controller.Edit("PP001", model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
        }

        [Fact]
        public async Task Edit_Post_WhenServiceFails_ReturnsJsonError()
        {
            var model = CreateValidProjectViewModel();
            var dto = new ProjectDto { ParentProject = "PP001" };
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } };
            var apiResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<ProjectDto>(model).Returns(dto);
            _projectService.UpdateProjectAsync("PP001", dto).Returns(apiResponse);

            var result = await _controller.Edit("PP001", model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
            Assert.Equal("Failed to update project.", value.message);
        }

        [Fact]
        public async Task Edit_Post_CallsMapperAndService()
        {
            var model = CreateValidProjectViewModel();
            var dto = new ProjectDto { ParentProject = "PP001" };
            var apiResponse = ApiResponseDto<ProjectDto>.SuccessResponse(dto);

            _mapper.Map<ProjectDto>(model).Returns(dto);
            _projectService.UpdateProjectAsync("PP001", dto).Returns(apiResponse);

            await _controller.Edit("PP001", model);

            _mapper.Received(1).Map<ProjectDto>(model);
            await _projectService.Received(1).UpdateProjectAsync("PP001", dto);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithValidProject_ReturnsSuccessJson()
        {
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _projectService.DeleteProjectAndChildrenAsync("PP001").Returns(apiResponse);

            var result = await _controller.Delete("PP001");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.True(value!.success);
            Assert.Equal("Project deleted successfully.", value.message);
        }

        [Fact]
        public async Task Delete_WhenServiceFails_ReturnsJsonError()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Delete failed", Code = "DELETE_ERROR" } };
            var apiResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _projectService.DeleteProjectAndChildrenAsync("PP001").Returns(apiResponse);

            var result = await _controller.Delete("PP001");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
            Assert.Equal("Delete failed", value.message);
        }

        [Theory]
        [InlineData("PP001")]
        [InlineData("PROJ123")]
        [InlineData("TEST")]
        public async Task Delete_WithVariousProjectCodes_CallsService(string parentProject)
        {
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _projectService.DeleteProjectAndChildrenAsync(parentProject).Returns(apiResponse);

            await _controller.Delete(parentProject);

            await _projectService.Received(1).DeleteProjectAndChildrenAsync(parentProject);
        }

        #endregion

        #region ChangeCode Tests

        [Fact]
        public async Task ChangeCode_WithValidCodes_ReturnsSuccessJson()
        {
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _projectService.ChangeProjectCodeAsync("OLD1", "NEW1").Returns(apiResponse);

            var result = await _controller.ChangeCode("OLD1", "NEW1");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.True(value!.success);
            Assert.Equal("Project code changed successfully.", value.message);
        }

        [Fact]
        public async Task ChangeCode_WhenServiceFails_ReturnsJsonError()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Change failed", Code = "CHANGE_ERROR" } };
            var apiResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _projectService.ChangeProjectCodeAsync("OLD1", "NEW1").Returns(apiResponse);

            var result = await _controller.ChangeCode("OLD1", "NEW1");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
            Assert.Equal("Change failed", value.message);
        }

        [Theory]
        [InlineData("", "NEW1")]
        [InlineData("OLD1", "")]
        [InlineData("", "")]
        [InlineData(null, "NEW1")]
        [InlineData("OLD1", null)]
        public async Task ChangeCode_WithEmptyCodes_ReturnsJsonError(string? oldCode, string? newCode)
        {
            var result = await _controller.ChangeCode(oldCode!, newCode!);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
            Assert.Equal("Both old and new project codes are required.", value.message);
        }

        [Fact]
        public async Task ChangeCode_WithEmptyCodes_DoesNotCallService()
        {
            await _controller.ChangeCode("", "");

            await _projectService.DidNotReceive().ChangeProjectCodeAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Theory]
        [InlineData("NEW-1")]
        [InlineData("NEW_1")]
        [InlineData("NEW 1")]
        [InlineData("NEW@1")]
        [InlineData("2024/001")]
        public async Task ChangeCode_WithNonAlphanumericNewCode_ReturnsJsonError(string newCode)
        {
            var result = await _controller.ChangeCode("OLD1", newCode);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
            Assert.Equal("Project Code must contain only letters (A-Z, a-z) and numbers (0-9)", value.message);
            Assert.NotNull(value.errors);
        }

        [Theory]
        [InlineData("NEW-1")]
        [InlineData("NEW 1")]
        public async Task ChangeCode_WithNonAlphanumericNewCode_DoesNotCallService(string newCode)
        {
            await _controller.ChangeCode("OLD1", newCode);

            await _projectService.DidNotReceive().ChangeProjectCodeAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Theory]
        [InlineData("NEW1")]
        [InlineData("new1")]
        [InlineData("NEW001")]
        [InlineData("abc123XYZ")]
        public async Task ChangeCode_WithAlphanumericNewCode_CallsService(string newCode)
        {
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _projectService.ChangeProjectCodeAsync("OLD1", newCode).Returns(apiResponse);

            var result = await _controller.ChangeCode("OLD1", newCode);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.True(value!.success);
            await _projectService.Received(1).ChangeProjectCodeAsync("OLD1", newCode);
        }

        #endregion

        #region Helpers

        private ProgrammeNewProjectViewModel CreateValidProjectViewModel()
        {
            return new ProgrammeNewProjectViewModel
            {
                ParentProject = "PP001",
                ProjectTitle = "Test Project",
                ShortTitle = "Test",
                Customer = "DEFRA",
                Program = "P001",
                Manager = "Alice",
                Disease = "FMD",
                ProjectStatus = "Active",
                Contract = "C001",
                IsDefraProject = -1,
                IncomeAccountCode = "INC001",
                SubAccountCode = "SUB001"
            };
        }

        private void SetupDropdownMocks()
        {
            _projectService.GetManagersAsync().Returns(ApiResponseDto<List<ManagerDto>>.SuccessResponse(new List<ManagerDto>()));
            _projectService.GetCostCentresAsync().Returns(ApiResponseDto<List<CostCentreWorkgroupDto>>.SuccessResponse(new List<CostCentreWorkgroupDto>()));
            _projectService.GetProjectGroupsByUserAsync().Returns(ApiResponseDto<List<ProjectGroupDto>>.SuccessResponse(new List<ProjectGroupDto>()));
            _projectService.GetAccountCodesAsync().Returns(ApiResponseDto<List<AccountCodeDto>>.SuccessResponse(new List<AccountCodeDto>()));
            _projectService.GetSubAccountsAsync().Returns(ApiResponseDto<List<SubAccountDto>>.SuccessResponse(new List<SubAccountDto>()));
            _projectService.GetAllStatusesAsync().Returns(ApiResponseDto<List<StatusDto>>.SuccessResponse(new List<StatusDto>()));
            _projectService.GetAllDiseasesAsync().Returns(ApiResponseDto<List<DiseaseDto>>.SuccessResponse(new List<DiseaseDto>()));
            _projectService.GetAllCustomersAsync().Returns(ApiResponseDto<List<CustomerDto>>.SuccessResponse(new List<CustomerDto>()));
            _projectService.GetContractsByUserAsync().Returns(ApiResponseDto<List<ContractDto>>.SuccessResponse(new List<ContractDto>()));
            _programService.GetAllProgramsAsync().Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(Enumerable.Empty<ProgramDto>()));
        }

        private class JsonResponse
        {
            public bool success { get; set; }
            public string? message { get; set; }
            public object? data { get; set; }
            public object? errors { get; set; }
            public string? redirectUrl { get; set; }
        }

        #endregion
    }
}
