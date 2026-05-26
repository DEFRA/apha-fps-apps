using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Web.Areas.PIMS.Controllers;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PIMS.Controllers.ProposedProjectControllerTest
{
    public class ProposedProjectControllerTests
    {
        private readonly IProposedProjectService _proposedProjectServiceMock;
        private readonly IMapper _mapperMock;
        private readonly ProposedProjectController _controller;

        public ProposedProjectControllerTests()
        {
            _proposedProjectServiceMock = Substitute.For<IProposedProjectService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new ProposedProjectController(_mapperMock, _proposedProjectServiceMock);

            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Substitute.For<ITempDataProvider>());
        }

        private void SetupBuildViewModelMocks(
            List<string>? programs = null,
            List<string>? customers = null,
            List<string>? statuses = null)
        {
            _proposedProjectServiceMock.GetProjectProgramsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = programs ?? ["Program A", "Program B"] });

            _proposedProjectServiceMock.GetProjectCustomersAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = customers ?? ["Customer A", "Customer B"] });

            _proposedProjectServiceMock.GetProjectStatusesAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = statuses ?? ["Active", "Inactive"] });
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_InitializesController()
        {
            var controller = new ProposedProjectController(_mapperMock, _proposedProjectServiceMock);
            Assert.NotNull(controller);
        }

        #endregion

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewResult()
        {
            SetupBuildViewModelMocks();
            var result = await _controller.Index();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_ReturnsProposedProjectViewModel()
        {
            SetupBuildViewModelMocks();
            var result = await _controller.Index();
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ProposedProjectViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Index_CallsGetProjectProgramsAsync_Once()
        {
            SetupBuildViewModelMocks();
            await _controller.Index();
            await _proposedProjectServiceMock.Received(1).GetProjectProgramsAsync();
        }

        [Fact]
        public async Task Index_CallsGetProjectCustomersAsync_Once()
        {
            SetupBuildViewModelMocks();
            await _controller.Index();
            await _proposedProjectServiceMock.Received(1).GetProjectCustomersAsync();
        }

        [Fact]
        public async Task Index_CallsGetProjectStatusesAsync_Once()
        {
            SetupBuildViewModelMocks();
            await _controller.Index();
            await _proposedProjectServiceMock.Received(1).GetProjectStatusesAsync();
        }

        [Fact]
        public async Task Index_ProgramOptions_ContainsDefaultPlaceholder()
        {
            SetupBuildViewModelMocks();
            var result = await _controller.Index();
            var model = Assert.IsType<ProposedProjectViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Contains(model.ProgramOptions, o => o.Text == "-- Select program --" && o.Value == "");
        }

        [Fact]
        public async Task Index_CustomerOptions_ContainsDefaultPlaceholder()
        {
            SetupBuildViewModelMocks();
            var result = await _controller.Index();
            var model = Assert.IsType<ProposedProjectViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Contains(model.CustomerOptions, o => o.Text == "-- Select customer --" && o.Value == "");
        }

        [Fact]
        public async Task Index_StatusOptions_ContainsDefaultPlaceholder()
        {
            SetupBuildViewModelMocks();
            var result = await _controller.Index();
            var model = Assert.IsType<ProposedProjectViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Contains(model.StatusOptions, o => o.Text == "-- Select status --" && o.Value == "");
        }

        [Fact]
        public async Task Index_ProgramOptions_ContainsServiceReturnedPrograms()
        {
            SetupBuildViewModelMocks(programs: ["Program A", "Program B"]);
            var result = await _controller.Index();
            var model = Assert.IsType<ProposedProjectViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Contains(model.ProgramOptions, o => o.Value == "Program A");
            Assert.Contains(model.ProgramOptions, o => o.Value == "Program B");
        }

        [Fact]
        public async Task Index_CustomerOptions_ContainsServiceReturnedCustomers()
        {
            SetupBuildViewModelMocks(customers: ["Customer A", "Customer B"]);
            var result = await _controller.Index();
            var model = Assert.IsType<ProposedProjectViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Contains(model.CustomerOptions, o => o.Value == "Customer A");
            Assert.Contains(model.CustomerOptions, o => o.Value == "Customer B");
        }

        [Fact]
        public async Task Index_StatusOptions_ContainsServiceReturnedStatuses()
        {
            SetupBuildViewModelMocks(statuses: ["Active", "Inactive"]);
            var result = await _controller.Index();
            var model = Assert.IsType<ProposedProjectViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Contains(model.StatusOptions, o => o.Value == "Active");
            Assert.Contains(model.StatusOptions, o => o.Value == "Inactive");
        }

        [Fact]
        public async Task Index_WhenProgramsDataIsNull_ProgramOptions_ContainsOnlyPlaceholder()
        {
            _proposedProjectServiceMock.GetProjectProgramsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = null });
            _proposedProjectServiceMock.GetProjectCustomersAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = ["Customer A"] });
            _proposedProjectServiceMock.GetProjectStatusesAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = ["Active"] });

            var result = await _controller.Index();
            var model = Assert.IsType<ProposedProjectViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Single(model.ProgramOptions);
            Assert.Equal("-- Select program --", model.ProgramOptions[0].Text);
        }

        [Fact]
        public async Task Index_WhenCustomersDataIsNull_CustomerOptions_ContainsOnlyPlaceholder()
        {
            _proposedProjectServiceMock.GetProjectProgramsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = ["Program A"] });
            _proposedProjectServiceMock.GetProjectCustomersAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = null });
            _proposedProjectServiceMock.GetProjectStatusesAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = ["Active"] });

            var result = await _controller.Index();
            var model = Assert.IsType<ProposedProjectViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Single(model.CustomerOptions);
            Assert.Equal("-- Select customer --", model.CustomerOptions[0].Text);
        }

        [Fact]
        public async Task Index_WhenStatusesDataIsNull_StatusOptions_ContainsOnlyPlaceholder()
        {
            _proposedProjectServiceMock.GetProjectProgramsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = ["Program A"] });
            _proposedProjectServiceMock.GetProjectCustomersAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = ["Customer A"] });
            _proposedProjectServiceMock.GetProjectStatusesAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = null });

            var result = await _controller.Index();
            var model = Assert.IsType<ProposedProjectViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Single(model.StatusOptions);
            Assert.Equal("-- Select status --", model.StatusOptions[0].Text);
        }

        [Fact]
        public async Task Index_WhenServiceThrowsException_PropagatesException()
        {
            _proposedProjectServiceMock.GetProjectProgramsAsync()
                .ThrowsAsync(new Exception("Service unavailable"));
            _proposedProjectServiceMock.GetProjectCustomersAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });
            _proposedProjectServiceMock.GetProjectStatusesAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });

            await Assert.ThrowsAsync<Exception>(() => _controller.Index());
        }

        #endregion

        #region Create (POST) - Invalid ModelState Tests

        [Fact]
        public async Task Create_WithInvalidModelState_ReturnsViewResult()
        {
            SetupBuildViewModelMocks();
            _controller.ModelState.AddModelError("Parentproject", "Project is required");
            var result = await _controller.Create(new ProposedProjectViewModel());
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_WithInvalidModelState_ReturnsIndexView()
        {
            SetupBuildViewModelMocks();
            _controller.ModelState.AddModelError("Parentproject", "Project is required");
            var result = await _controller.Create(new ProposedProjectViewModel());
            Assert.Equal("Index", Assert.IsType<ViewResult>(result).ViewName);
        }

        [Fact]
        public async Task Create_WithInvalidModelState_ReturnsProposedProjectViewModel()
        {
            SetupBuildViewModelMocks();
            _controller.ModelState.AddModelError("Parentproject", "Project is required");
            var result = await _controller.Create(new ProposedProjectViewModel());
            Assert.IsType<ProposedProjectViewModel>(Assert.IsType<ViewResult>(result).Model);
        }

        [Fact]
        public async Task Create_WithInvalidModelState_DoesNotCallCreateProjectAsync()
        {
            SetupBuildViewModelMocks();
            _controller.ModelState.AddModelError("Parentproject", "Project is required");
            await _controller.Create(new ProposedProjectViewModel());
            await _proposedProjectServiceMock.DidNotReceive().CreateProposedProjectAsync(Arg.Any<ProposedProjectDto>());
        }

        [Fact]
        public async Task Create_WithInvalidModelState_PreservesParentproject()
        {
            SetupBuildViewModelMocks();
            _controller.ModelState.AddModelError("Parentproject", "Project is required");
            var result = await _controller.Create(new ProposedProjectViewModel { Parentproject = "PP001" });
            var model = Assert.IsType<ProposedProjectViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("PP001", model.Parentproject);
        }

        [Fact]
        public async Task Create_WithInvalidModelState_PreservesAllFormFields()
        {
            SetupBuildViewModelMocks();
            _controller.ModelState.AddModelError("Parentproject", "Project is required");
            var input = new ProposedProjectViewModel
            {
                Parentproject = "PP001",
                Projecttitle = "Title A",
                Projectstatus = "Active",
                Costbookno = "CB001",
                Disease = "Disease A",
                Program = "Program A",
                Customer = "Customer A",
                Manager = "Manager A",
                Reason = "Reason A"
            };

            var result = await _controller.Create(input);
            var model = Assert.IsType<ProposedProjectViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("PP001", model.Parentproject);
            Assert.Equal("Title A", model.Projecttitle);
            Assert.Equal("Active", model.Projectstatus);
            Assert.Equal("CB001", model.Costbookno);
            Assert.Equal("Disease A", model.Disease);
            Assert.Equal("Program A", model.Program);
            Assert.Equal("Customer A", model.Customer);
            Assert.Equal("Manager A", model.Manager);
            Assert.Equal("Reason A", model.Reason);
        }

        [Fact]
        public async Task Create_WithInvalidModelState_RebuildsProgramOptions()
        {
            SetupBuildViewModelMocks(programs: ["Program A"]);
            _controller.ModelState.AddModelError("Parentproject", "Required");
            var result = await _controller.Create(new ProposedProjectViewModel());
            var model = Assert.IsType<ProposedProjectViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.NotEmpty(model.ProgramOptions);
        }

        [Fact]
        public async Task Create_WithInvalidModelState_RebuildsCustomerOptions()
        {
            SetupBuildViewModelMocks(customers: ["Customer A"]);
            _controller.ModelState.AddModelError("Parentproject", "Required");
            var result = await _controller.Create(new ProposedProjectViewModel());
            var model = Assert.IsType<ProposedProjectViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.NotEmpty(model.CustomerOptions);
        }

        [Fact]
        public async Task Create_WithInvalidModelState_RebuildsStatusOptions()
        {
            SetupBuildViewModelMocks(statuses: ["Active"]);
            _controller.ModelState.AddModelError("Parentproject", "Required");
            var result = await _controller.Create(new ProposedProjectViewModel());
            var model = Assert.IsType<ProposedProjectViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.NotEmpty(model.StatusOptions);
        }

        #endregion

        #region Create (POST) - Success Tests

        [Fact]
        public async Task Create_WithValidModelState_AndServiceSuccess_ReturnsRedirectToActionResult()
        {
            SetupBuildViewModelMocks();
            var dto = new ProposedProjectDto { Parentproject = "PP001" };
            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>()).Returns(dto);
            _proposedProjectServiceMock.CreateProposedProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto> { Success = true, Data = dto });

            var result = await _controller.Create(new ProposedProjectViewModel { Parentproject = "PP001" });
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task Create_WithValidModelState_AndServiceSuccess_RedirectsToProjectListIndex()
        {
            SetupBuildViewModelMocks();
            var dto = new ProposedProjectDto { Parentproject = "PP001" };
            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>()).Returns(dto);
            _proposedProjectServiceMock.CreateProposedProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto> { Success = true, Data = dto });

            var result = await _controller.Create(new ProposedProjectViewModel { Parentproject = "PP001" });
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("ProjectList", redirect.ControllerName);
            Assert.Equal("PIMS", redirect.RouteValues?["area"]?.ToString());
        }

        [Fact]
        public async Task Create_WithValidModelState_AndServiceSuccess_SetsTempDataSuccessMessage()
        {
            SetupBuildViewModelMocks();
            var dto = new ProposedProjectDto { Parentproject = "PP001" };
            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>()).Returns(dto);
            _proposedProjectServiceMock.CreateProposedProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto> { Success = true, Data = dto });

            await _controller.Create(new ProposedProjectViewModel { Parentproject = "PP001" });
            Assert.Equal("Project created successfully.", _controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task Create_WithValidModelState_CallsMapperToMapDto()
        {
            SetupBuildViewModelMocks();
            var dto = new ProposedProjectDto { Parentproject = "PP001" };
            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>()).Returns(dto);
            _proposedProjectServiceMock.CreateProposedProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto> { Success = true, Data = dto });

            await _controller.Create(new ProposedProjectViewModel { Parentproject = "PP001" });
            _mapperMock.Received(1).Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>());
        }

        [Fact]
        public async Task Create_WithValidModelState_CallsCreateProjectAsync_Once()
        {
            SetupBuildViewModelMocks();
            var dto = new ProposedProjectDto { Parentproject = "PP001" };
            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>()).Returns(dto);
            _proposedProjectServiceMock.CreateProposedProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto> { Success = true, Data = dto });

            await _controller.Create(new ProposedProjectViewModel { Parentproject = "PP001" });
            await _proposedProjectServiceMock.Received(1).CreateProposedProjectAsync(Arg.Any<ProposedProjectDto>());
        }

        #endregion

        #region Create (POST) - Service Failure Tests

        [Fact]
        public async Task Create_WithValidModelState_AndServiceFailure_ReturnsViewResult()
        {
            SetupBuildViewModelMocks();
            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>())
                .Returns(new ProposedProjectDto());
            _proposedProjectServiceMock.CreateProposedProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto>
                {
                    Success = false,
                    Errors = [new ApiErrorDto { Message = "Duplicate project", Code = "DUPLICATE" }]
                });

            var result = await _controller.Create(new ProposedProjectViewModel { Parentproject = "PP001" });
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_WithValidModelState_AndServiceFailure_ReturnsIndexView()
        {
            SetupBuildViewModelMocks();
            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>())
                .Returns(new ProposedProjectDto());
            _proposedProjectServiceMock.CreateProposedProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto>
                {
                    Success = false,
                    Errors = [new ApiErrorDto { Message = "Duplicate project", Code = "DUPLICATE" }]
                });

            var result = await _controller.Create(new ProposedProjectViewModel { Parentproject = "PP001" });
            Assert.Equal("Index", Assert.IsType<ViewResult>(result).ViewName);
        }

        [Fact]
        public async Task Create_WithValidModelState_AndServiceFailure_SetsModelStateError()
        {
            SetupBuildViewModelMocks();
            const string errorMessage = "Duplicate project";
            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>())
                .Returns(new ProposedProjectDto());
            _proposedProjectServiceMock.CreateProposedProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto>
                {
                    Success = false,
                    Errors = [new ApiErrorDto { Message = errorMessage, Code = "DUPLICATE" }]
                });

            await _controller.Create(new ProposedProjectViewModel { Parentproject = "PP001" });

            Assert.False(_controller.ModelState.IsValid);
            var errors = _controller.ModelState[""]?.Errors;
            Assert.NotNull(errors);
            Assert.Contains(errors, e => e.ErrorMessage == errorMessage);
        }

        [Fact]
        public async Task Create_WithValidModelState_AndServiceFailure_SetsTempDataError()
        {
            SetupBuildViewModelMocks();
            const string errorMessage = "Duplicate project";
            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>())
                .Returns(new ProposedProjectDto());
            _proposedProjectServiceMock.CreateProposedProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto>
                {
                    Success = false,
                    Errors = [new ApiErrorDto { Message = errorMessage, Code = "DUPLICATE" }]
                });

            await _controller.Create(new ProposedProjectViewModel { Parentproject = "PP001" });
            Assert.Equal(errorMessage, _controller.TempData["Error"]);
        }

        [Fact]
        public async Task Create_WithValidModelState_AndServiceFailure_PreservesAllFormFields()
        {
            SetupBuildViewModelMocks();
            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>())
                .Returns(new ProposedProjectDto());
            _proposedProjectServiceMock.CreateProposedProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto>
                {
                    Success = false,
                    Errors = [new ApiErrorDto { Message = "Error", Code = "ERR" }]
                });

            var input = new ProposedProjectViewModel
            {
                Parentproject = "PP001",
                Projecttitle = "Title A",
                Projectstatus = "Active",
                Costbookno = "CB001",
                Disease = "Disease A",
                Program = "Program A",
                Customer = "Customer A",
                Manager = "Manager A",
                Reason = "Reason A"
            };

            var result = await _controller.Create(input);
            var model = Assert.IsType<ProposedProjectViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("PP001", model.Parentproject);
            Assert.Equal("Title A", model.Projecttitle);
            Assert.Equal("Active", model.Projectstatus);
            Assert.Equal("CB001", model.Costbookno);
            Assert.Equal("Disease A", model.Disease);
            Assert.Equal("Program A", model.Program);
            Assert.Equal("Customer A", model.Customer);
            Assert.Equal("Manager A", model.Manager);
            Assert.Equal("Reason A", model.Reason);
        }

        [Fact]
        public async Task Create_WithValidModelState_AndServiceFailure_RebuildsProgramOptions()
        {
            SetupBuildViewModelMocks(programs: ["Program A"]);
            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>())
                .Returns(new ProposedProjectDto());
            _proposedProjectServiceMock.CreateProposedProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto>
                {
                    Success = false,
                    Errors = [new ApiErrorDto { Message = "Error", Code = "ERR" }]
                });

            var result = await _controller.Create(new ProposedProjectViewModel { Parentproject = "PP001" });
            var model = Assert.IsType<ProposedProjectViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.NotEmpty(model.ProgramOptions);
        }

        [Fact]
        public async Task Create_WithValidModelState_AndMultipleServiceErrors_AddsAllModelStateErrors()
        {
            SetupBuildViewModelMocks();
            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>())
                .Returns(new ProposedProjectDto());
            _proposedProjectServiceMock.CreateProposedProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto>
                {
                    Success = false,
                    Errors =
                    [
                        new ApiErrorDto { Message = "Error 1", Code = "ERR1" },
                        new ApiErrorDto { Message = "Error 2", Code = "ERR2" }
                    ]
                });

            await _controller.Create(new ProposedProjectViewModel { Parentproject = "PP001" });

            var errors = _controller.ModelState[""]?.Errors;
            Assert.NotNull(errors);
            Assert.True(errors.Count >= 2);
        }

        [Fact]
        public async Task Create_WithValidModelState_AndNullErrorMessage_UsesDefaultErrorMessage()
        {
            SetupBuildViewModelMocks();
            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>())
                .Returns(new ProposedProjectDto());
            _proposedProjectServiceMock.CreateProposedProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto>
                {
                    Success = false,
                    Errors = [new ApiErrorDto { Message = null!, Code = "ERR" }]
                });

            await _controller.Create(new ProposedProjectViewModel { Parentproject = "PP001" });

            var errors = _controller.ModelState[""]?.Errors;
            Assert.NotNull(errors);
            Assert.Contains(errors, e => e.ErrorMessage == "An error occurred.");
        }

        [Fact]
        public async Task Create_WithValidModelState_AndNullErrors_ReturnsIndexView()
        {
            SetupBuildViewModelMocks();
            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>())
                .Returns(new ProposedProjectDto());
            _proposedProjectServiceMock.CreateProposedProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto> { Success = false, Errors = null });

            var result = await _controller.Create(new ProposedProjectViewModel { Parentproject = "PP001" });
            Assert.Equal("Index", Assert.IsType<ViewResult>(result).ViewName);
        }

        [Fact]
        public async Task Create_WithValidModelState_AndServiceThrowsException_PropagatesException()
        {
            SetupBuildViewModelMocks();
            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>())
                .Returns(new ProposedProjectDto());
            _proposedProjectServiceMock.CreateProposedProjectAsync(Arg.Any<ProposedProjectDto>())
                .ThrowsAsync(new Exception("Service unavailable"));

            await Assert.ThrowsAsync<Exception>(() =>
                _controller.Create(new ProposedProjectViewModel { Parentproject = "PP001" }));
        }

        #endregion
    }
}