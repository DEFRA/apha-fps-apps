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
        private readonly IProjectListService _projectListServiceMock;
        private readonly IMapper _mapperMock;
        private readonly ProposedProjectController _controller;

        public ProposedProjectControllerTests()
        {
            _projectListServiceMock = Substitute.For<IProjectListService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new ProposedProjectController(_mapperMock, _projectListServiceMock);

            // TempData must be set up so controller can write to it
            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Substitute.For<ITempDataProvider>());
        }

        /// <summary>
        /// Sets up all three dropdown service calls used by BuildViewModelAsync.
        /// </summary>
        private void SetupBuildViewModelMocks(
            List<string>? programs = null,
            List<string>? customers = null,
            List<string>? statuses = null)
        {
            _projectListServiceMock.GetProjectProgramsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = programs ?? ["Program A", "Program B"] });

            _projectListServiceMock.GetProjectCustomersAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = customers ?? ["Customer A", "Customer B"] });

            _projectListServiceMock.GetProjectStatusesAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = statuses ?? ["Active", "Inactive"] });
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_InitializesController()
        {
            // Arrange & Act
            var controller = new ProposedProjectController(_mapperMock, _projectListServiceMock);

            // Assert
            Assert.NotNull(controller);
        }

        #endregion

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewResult()
        {
            // Arrange
            SetupBuildViewModelMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_ReturnsProposedProjectViewModel()
        {
            // Arrange
            SetupBuildViewModelMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ProposedProjectViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Index_CallsGetProjectProgramsAsync_Once()
        {
            // Arrange
            SetupBuildViewModelMocks();

            // Act
            await _controller.Index();

            // Assert
            await _projectListServiceMock.Received(1).GetProjectProgramsAsync();
        }

        [Fact]
        public async Task Index_CallsGetProjectCustomersAsync_Once()
        {
            // Arrange
            SetupBuildViewModelMocks();

            // Act
            await _controller.Index();

            // Assert
            await _projectListServiceMock.Received(1).GetProjectCustomersAsync();
        }

        [Fact]
        public async Task Index_CallsGetProjectStatusesAsync_Once()
        {
            // Arrange
            SetupBuildViewModelMocks();

            // Act
            await _controller.Index();

            // Assert
            await _projectListServiceMock.Received(1).GetProjectStatusesAsync();
        }

        [Fact]
        public async Task Index_ProgramOptions_ContainsDefaultPlaceholder()
        {
            // Arrange
            SetupBuildViewModelMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProposedProjectViewModel>(viewResult.Model);
            Assert.Contains(model.ProgramOptions, o => o.Text == "-- Select program --" && o.Value == "");
        }

        [Fact]
        public async Task Index_CustomerOptions_ContainsDefaultPlaceholder()
        {
            // Arrange
            SetupBuildViewModelMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProposedProjectViewModel>(viewResult.Model);
            Assert.Contains(model.CustomerOptions, o => o.Text == "-- Select customer --" && o.Value == "");
        }

        [Fact]
        public async Task Index_StatusOptions_ContainsDefaultPlaceholder()
        {
            // Arrange
            SetupBuildViewModelMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProposedProjectViewModel>(viewResult.Model);
            Assert.Contains(model.StatusOptions, o => o.Text == "-- Select status --" && o.Value == "");
        }

        [Fact]
        public async Task Index_ProgramOptions_ContainsServiceReturnedPrograms()
        {
            // Arrange
            SetupBuildViewModelMocks(programs: ["Program A", "Program B"]);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProposedProjectViewModel>(viewResult.Model);
            Assert.Contains(model.ProgramOptions, o => o.Value == "Program A");
            Assert.Contains(model.ProgramOptions, o => o.Value == "Program B");
        }

        [Fact]
        public async Task Index_CustomerOptions_ContainsServiceReturnedCustomers()
        {
            // Arrange
            SetupBuildViewModelMocks(customers: ["Customer A", "Customer B"]);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProposedProjectViewModel>(viewResult.Model);
            Assert.Contains(model.CustomerOptions, o => o.Value == "Customer A");
            Assert.Contains(model.CustomerOptions, o => o.Value == "Customer B");
        }

        [Fact]
        public async Task Index_StatusOptions_ContainsServiceReturnedStatuses()
        {
            // Arrange
            SetupBuildViewModelMocks(statuses: ["Active", "Inactive"]);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProposedProjectViewModel>(viewResult.Model);
            Assert.Contains(model.StatusOptions, o => o.Value == "Active");
            Assert.Contains(model.StatusOptions, o => o.Value == "Inactive");
        }

        [Fact]
        public async Task Index_WhenProgramsDataIsNull_ProgramOptions_ContainsOnlyPlaceholder()
        {
            // Arrange
            _projectListServiceMock.GetProjectProgramsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = null });
            _projectListServiceMock.GetProjectCustomersAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = ["Customer A"] });
            _projectListServiceMock.GetProjectStatusesAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = ["Active"] });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProposedProjectViewModel>(viewResult.Model);
            Assert.Single(model.ProgramOptions);
            Assert.Equal("-- Select program --", model.ProgramOptions[0].Text);
        }

        [Fact]
        public async Task Index_WhenCustomersDataIsNull_CustomerOptions_ContainsOnlyPlaceholder()
        {
            // Arrange
            _projectListServiceMock.GetProjectProgramsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = ["Program A"] });
            _projectListServiceMock.GetProjectCustomersAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = null });
            _projectListServiceMock.GetProjectStatusesAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = ["Active"] });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProposedProjectViewModel>(viewResult.Model);
            Assert.Single(model.CustomerOptions);
            Assert.Equal("-- Select customer --", model.CustomerOptions[0].Text);
        }

        [Fact]
        public async Task Index_WhenStatusesDataIsNull_StatusOptions_ContainsOnlyPlaceholder()
        {
            // Arrange
            _projectListServiceMock.GetProjectProgramsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = ["Program A"] });
            _projectListServiceMock.GetProjectCustomersAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = ["Customer A"] });
            _projectListServiceMock.GetProjectStatusesAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = null });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProposedProjectViewModel>(viewResult.Model);
            Assert.Single(model.StatusOptions);
            Assert.Equal("-- Select status --", model.StatusOptions[0].Text);
        }

        [Fact]
        public async Task Index_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _projectListServiceMock.GetProjectProgramsAsync()
                .ThrowsAsync(new Exception("Service unavailable"));
            _projectListServiceMock.GetProjectCustomersAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });
            _projectListServiceMock.GetProjectStatusesAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Index());
        }

        #endregion

        #region Create (POST) - Invalid ModelState Tests

        [Fact]
        public async Task Create_WithInvalidModelState_ReturnsViewResult()
        {
            // Arrange
            SetupBuildViewModelMocks();
            _controller.ModelState.AddModelError("Parentproject", "Project is required");
            var model = new ProposedProjectViewModel();

            // Act
            var result = await _controller.Create(model);

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_WithInvalidModelState_ReturnsIndexView()
        {
            // Arrange
            SetupBuildViewModelMocks();
            _controller.ModelState.AddModelError("Parentproject", "Project is required");
            var model = new ProposedProjectViewModel();

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", viewResult.ViewName);
        }

        [Fact]
        public async Task Create_WithInvalidModelState_ReturnsProposedProjectViewModel()
        {
            // Arrange
            SetupBuildViewModelMocks();
            _controller.ModelState.AddModelError("Parentproject", "Project is required");
            var model = new ProposedProjectViewModel();

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ProposedProjectViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Create_WithInvalidModelState_DoesNotCallCreateProjectAsync()
        {
            // Arrange
            SetupBuildViewModelMocks();
            _controller.ModelState.AddModelError("Parentproject", "Project is required");
            var model = new ProposedProjectViewModel();

            // Act
            await _controller.Create(model);

            // Assert
            await _projectListServiceMock.DidNotReceive().CreateProjectAsync(Arg.Any<ProposedProjectDto>());
        }

        [Fact]
        public async Task Create_WithInvalidModelState_PreservesParentproject()
        {
            // Arrange
            SetupBuildViewModelMocks();
            _controller.ModelState.AddModelError("Parentproject", "Project is required");
            var model = new ProposedProjectViewModel { Parentproject = "PP001" };

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<ProposedProjectViewModel>(viewResult.Model);
            Assert.Equal("PP001", returnedModel.Parentproject);
        }

        [Fact]
        public async Task Create_WithInvalidModelState_PreservesAllFormFields()
        {
            // Arrange
            SetupBuildViewModelMocks();
            _controller.ModelState.AddModelError("Parentproject", "Project is required");
            var model = new ProposedProjectViewModel
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

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<ProposedProjectViewModel>(viewResult.Model);
            Assert.Equal("PP001", returnedModel.Parentproject);
            Assert.Equal("Title A", returnedModel.Projecttitle);
            Assert.Equal("Active", returnedModel.Projectstatus);
            Assert.Equal("CB001", returnedModel.Costbookno);
            Assert.Equal("Disease A", returnedModel.Disease);
            Assert.Equal("Program A", returnedModel.Program);
            Assert.Equal("Customer A", returnedModel.Customer);
            Assert.Equal("Manager A", returnedModel.Manager);
            Assert.Equal("Reason A", returnedModel.Reason);
        }

        [Fact]
        public async Task Create_WithInvalidModelState_RebuildsProgramOptions()
        {
            // Arrange
            SetupBuildViewModelMocks(programs: ["Program A"]);
            _controller.ModelState.AddModelError("Parentproject", "Required");
            var model = new ProposedProjectViewModel();

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<ProposedProjectViewModel>(viewResult.Model);
            Assert.NotEmpty(returnedModel.ProgramOptions);
        }

        [Fact]
        public async Task Create_WithInvalidModelState_RebuildsCustomerOptions()
        {
            // Arrange
            SetupBuildViewModelMocks(customers: ["Customer A"]);
            _controller.ModelState.AddModelError("Parentproject", "Required");
            var model = new ProposedProjectViewModel();

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<ProposedProjectViewModel>(viewResult.Model);
            Assert.NotEmpty(returnedModel.CustomerOptions);
        }

        [Fact]
        public async Task Create_WithInvalidModelState_RebuildsStatusOptions()
        {
            // Arrange
            SetupBuildViewModelMocks(statuses: ["Active"]);
            _controller.ModelState.AddModelError("Parentproject", "Required");
            var model = new ProposedProjectViewModel();

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<ProposedProjectViewModel>(viewResult.Model);
            Assert.NotEmpty(returnedModel.StatusOptions);
        }

        #endregion

        #region Create (POST) - Success Tests

        [Fact]
        public async Task Create_WithValidModelState_AndServiceSuccess_ReturnsRedirectToActionResult()
        {
            // Arrange
            SetupBuildViewModelMocks();
            var model = new ProposedProjectViewModel { Parentproject = "PP001" };
            var dto = new ProposedProjectDto { Parentproject = "PP001" };

            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>()).Returns(dto);
            _projectListServiceMock.CreateProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto> { Success = true, Data = dto });

            // Act
            var result = await _controller.Create(model);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task Create_WithValidModelState_AndServiceSuccess_RedirectsToProjectListIndex()
        {
            // Arrange
            SetupBuildViewModelMocks();
            var model = new ProposedProjectViewModel { Parentproject = "PP001" };
            var dto = new ProposedProjectDto { Parentproject = "PP001" };

            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>()).Returns(dto);
            _projectListServiceMock.CreateProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto> { Success = true, Data = dto });

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("ProjectList", redirect.ControllerName);
            Assert.Equal("PIMS", redirect.RouteValues?["area"]?.ToString());
        }

        [Fact]
        public async Task Create_WithValidModelState_AndServiceSuccess_SetsTempDataSuccessMessage()
        {
            // Arrange
            SetupBuildViewModelMocks();
            var model = new ProposedProjectViewModel { Parentproject = "PP001" };
            var dto = new ProposedProjectDto { Parentproject = "PP001" };

            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>()).Returns(dto);
            _projectListServiceMock.CreateProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto> { Success = true, Data = dto });

            // Act
            await _controller.Create(model);

            // Assert
            Assert.Equal("Project created successfully.", _controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task Create_WithValidModelState_CallsMapperToMapDto()
        {
            // Arrange
            SetupBuildViewModelMocks();
            var model = new ProposedProjectViewModel { Parentproject = "PP001" };
            var dto = new ProposedProjectDto { Parentproject = "PP001" };

            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>()).Returns(dto);
            _projectListServiceMock.CreateProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto> { Success = true, Data = dto });

            // Act
            await _controller.Create(model);

            // Assert
            _mapperMock.Received(1).Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>());
        }

        [Fact]
        public async Task Create_WithValidModelState_CallsCreateProjectAsync_Once()
        {
            // Arrange
            SetupBuildViewModelMocks();
            var model = new ProposedProjectViewModel { Parentproject = "PP001" };
            var dto = new ProposedProjectDto { Parentproject = "PP001" };

            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>()).Returns(dto);
            _projectListServiceMock.CreateProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto> { Success = true, Data = dto });

            // Act
            await _controller.Create(model);

            // Assert
            await _projectListServiceMock.Received(1).CreateProjectAsync(Arg.Any<ProposedProjectDto>());
        }

        #endregion

        #region Create (POST) - Service Failure Tests

        [Fact]
        public async Task Create_WithValidModelState_AndServiceFailure_ReturnsViewResult()
        {
            // Arrange
            SetupBuildViewModelMocks();
            var model = new ProposedProjectViewModel { Parentproject = "PP001" };
            var dto = new ProposedProjectDto { Parentproject = "PP001" };

            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>()).Returns(dto);
            _projectListServiceMock.CreateProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto>
                {
                    Success = false,
                    Errors = [new ApiErrorDto { Message = "Duplicate project", Code = "DUPLICATE" }]
                });

            // Act
            var result = await _controller.Create(model);

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_WithValidModelState_AndServiceFailure_ReturnsIndexView()
        {
            // Arrange
            SetupBuildViewModelMocks();
            var model = new ProposedProjectViewModel { Parentproject = "PP001" };
            var dto = new ProposedProjectDto { Parentproject = "PP001" };

            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>()).Returns(dto);
            _projectListServiceMock.CreateProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto>
                {
                    Success = false,
                    Errors = [new ApiErrorDto { Message = "Duplicate project", Code = "DUPLICATE" }]
                });

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", viewResult.ViewName);
        }

        [Fact]
        public async Task Create_WithValidModelState_AndServiceFailure_SetsModelStateError()
        {
            // Arrange
            SetupBuildViewModelMocks();
            var model = new ProposedProjectViewModel { Parentproject = "PP001" };
            var dto = new ProposedProjectDto { Parentproject = "PP001" };
            const string errorMessage = "Duplicate project";

            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>()).Returns(dto);
            _projectListServiceMock.CreateProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto>
                {
                    Success = false,
                    Errors = [new ApiErrorDto { Message = errorMessage, Code = "DUPLICATE" }]
                });

            // Act
            await _controller.Create(model);

            // Assert
            Assert.False(_controller.ModelState.IsValid);
            var errors = _controller.ModelState[""]?.Errors;
            Assert.NotNull(errors);
            Assert.Contains(errors, e => e.ErrorMessage == errorMessage);
        }

        [Fact]
        public async Task Create_WithValidModelState_AndServiceFailure_SetsTempDataError()
        {
            // Arrange
            SetupBuildViewModelMocks();
            var model = new ProposedProjectViewModel { Parentproject = "PP001" };
            var dto = new ProposedProjectDto { Parentproject = "PP001" };
            const string errorMessage = "Duplicate project";

            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>()).Returns(dto);
            _projectListServiceMock.CreateProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto>
                {
                    Success = false,
                    Errors = [new ApiErrorDto { Message = errorMessage, Code = "DUPLICATE" }]
                });

            // Act
            await _controller.Create(model);

            // Assert
            Assert.Equal(errorMessage, _controller.TempData["Error"]);
        }

        [Fact]
        public async Task Create_WithValidModelState_AndServiceFailure_PreservesAllFormFields()
        {
            // Arrange
            SetupBuildViewModelMocks();
            var model = new ProposedProjectViewModel
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
            var dto = new ProposedProjectDto();

            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>()).Returns(dto);
            _projectListServiceMock.CreateProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto>
                {
                    Success = false,
                    Errors = [new ApiErrorDto { Message = "Error", Code = "ERR" }]
                });

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<ProposedProjectViewModel>(viewResult.Model);
            Assert.Equal("PP001", returnedModel.Parentproject);
            Assert.Equal("Title A", returnedModel.Projecttitle);
            Assert.Equal("Active", returnedModel.Projectstatus);
            Assert.Equal("CB001", returnedModel.Costbookno);
            Assert.Equal("Disease A", returnedModel.Disease);
            Assert.Equal("Program A", returnedModel.Program);
            Assert.Equal("Customer A", returnedModel.Customer);
            Assert.Equal("Manager A", returnedModel.Manager);
            Assert.Equal("Reason A", returnedModel.Reason);
        }

        [Fact]
        public async Task Create_WithValidModelState_AndServiceFailure_RebuildsProgramOptions()
        {
            // Arrange
            SetupBuildViewModelMocks(programs: ["Program A"]);
            var model = new ProposedProjectViewModel { Parentproject = "PP001" };
            var dto = new ProposedProjectDto();

            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>()).Returns(dto);
            _projectListServiceMock.CreateProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto>
                {
                    Success = false,
                    Errors = [new ApiErrorDto { Message = "Error", Code = "ERR" }]
                });

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<ProposedProjectViewModel>(viewResult.Model);
            Assert.NotEmpty(returnedModel.ProgramOptions);
        }

        [Fact]
        public async Task Create_WithValidModelState_AndMultipleServiceErrors_AddsAllModelStateErrors()
        {
            // Arrange
            SetupBuildViewModelMocks();
            var model = new ProposedProjectViewModel { Parentproject = "PP001" };
            var dto = new ProposedProjectDto();

            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>()).Returns(dto);
            _projectListServiceMock.CreateProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto>
                {
                    Success = false,
                    Errors =
                    [
                        new ApiErrorDto { Message = "Error 1", Code = "ERR1" },
                        new ApiErrorDto { Message = "Error 2", Code = "ERR2" }
                    ]
                });

            // Act
            await _controller.Create(model);

            // Assert
            var errors = _controller.ModelState[""]?.Errors;
            Assert.NotNull(errors);
            // Errors are added twice (once in first loop, once in second loop in controller)
            Assert.True(errors.Count >= 2);
        }

        [Fact]
        public async Task Create_WithValidModelState_AndNullErrorMessage_UsesDefaultErrorMessage()
        {
            // Arrange
            SetupBuildViewModelMocks();
            var model = new ProposedProjectViewModel { Parentproject = "PP001" };
            var dto = new ProposedProjectDto();

            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>()).Returns(dto);
            _projectListServiceMock.CreateProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto>
                {
                    Success = false,
                    Errors = [new ApiErrorDto { Message = null, Code = "ERR" }]
                });

            // Act
            await _controller.Create(model);

            // Assert
            var errors = _controller.ModelState[""]?.Errors;
            Assert.NotNull(errors);
            Assert.Contains(errors, e => e.ErrorMessage == "An error occurred.");
        }

        [Fact]
        public async Task Create_WithValidModelState_AndNullErrors_ReturnsIndexView()
        {
            // Arrange
            SetupBuildViewModelMocks();
            var model = new ProposedProjectViewModel { Parentproject = "PP001" };
            var dto = new ProposedProjectDto();

            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>()).Returns(dto);
            _projectListServiceMock.CreateProjectAsync(Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto> { Success = false, Errors = null });

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", viewResult.ViewName);
        }

        [Fact]
        public async Task Create_WithValidModelState_AndServiceThrowsException_PropagatesException()
        {
            // Arrange
            SetupBuildViewModelMocks();
            var model = new ProposedProjectViewModel { Parentproject = "PP001" };
            var dto = new ProposedProjectDto();

            _mapperMock.Map<ProposedProjectDto>(Arg.Any<ProposedProjectViewModel>()).Returns(dto);
            _projectListServiceMock.CreateProjectAsync(Arg.Any<ProposedProjectDto>())
                .ThrowsAsync(new Exception("Service unavailable"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Create(model));
        }

        #endregion
    }
}