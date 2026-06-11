using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PIMS.Controllers;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PIMS.Controllers.MilestoneControllerTest
{
    public class MilestoneControllerTests
    {
        private readonly IMapper              _mapper;
        private readonly IMilestoneService    _milestoneService;
        private readonly IProjectListService  _projectListService;
        private readonly MilestoneController  _controller;

        public MilestoneControllerTests()
        {
            _mapper             = Substitute.For<IMapper>();
            _milestoneService   = Substitute.For<IMilestoneService>();
            _projectListService = Substitute.For<IProjectListService>();
            _controller         = new MilestoneController(_mapper, _milestoneService, _projectListService);
        }

        // ── shared setup helpers ────────────────────────────────────────────

        /// <summary>Wires every dependency needed for Index / BuildGridsAsync to complete.</summary>
        private void SetupSuccessfulIndexMocks(
            List<ProjectListMilestoneDto>? projects    = null,
            List<MilestoneDto>?            milestones  = null,
            List<MilestoneFormDatesDto>?   formDates   = null,
            List<MilestoneTypeDto>?        types       = null)
        {
            var projectList = projects ?? [new ProjectListMilestoneDto { Parentproject = "PP001", Formrequired = false }];

            _projectListService.GetAllProjectsForMilestoneAsync()
                .Returns(new ApiResponseDto<List<ProjectListMilestoneDto>> { Success = true, Data = projectList });

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());

            _milestoneService.GetAllMilestonesAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(new ApiResponseDto<List<MilestoneDto>>
                {
                    Success = true,
                    Data    = milestones ?? []
                });

            _milestoneService.GetAllMilestoneFormDatesAsync(Arg.Any<string>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<MilestoneFormDatesDto>>
                {
                    Success = true,
                    Data    = formDates ?? []
                });

            _milestoneService.GetMilestoneTypesAsync(Arg.Any<string?>())
                .Returns(new ApiResponseDto<List<MilestoneTypeDto>>
                {
                    Success = true,
                    Data    = types ?? [new MilestoneTypeDto { IdType = 'D', Type = "Deliverable" }]
                });

            _mapper.Map<List<MilestoneItem>>(Arg.Any<List<MilestoneDto>>())
                .Returns([]);
            _mapper.Map<List<MilestoneFormDatesItem>>(Arg.Any<List<MilestoneFormDatesDto>>())
                .Returns([]);

            _milestoneService.GetLogMilestonesAsync(
                    Arg.Any<QueryParameters<string>>(),
                    Arg.Any<string?>(),
                    Arg.Any<string?>(),
                    Arg.Any<string?>())
                .Returns(new ApiResponseDto<List<LogMilestoneDto>> { Success = true, Data = [] });
            _mapper.Map<List<LogMilestoneItem>>(Arg.Any<List<LogMilestoneDto>>()).Returns([]);
        }

        /// <summary>Sets up mocks for the LoadMilestoneGrid / LoadMilestoneFormDatesGrid path.</summary>
        private void SetupGridMocks(
            List<MilestoneDto>?          milestones = null,
            List<MilestoneFormDatesDto>? formDates  = null)
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());

            _milestoneService.GetAllMilestonesAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(new ApiResponseDto<List<MilestoneDto>> { Success = true, Data = milestones ?? [] });

            _milestoneService.GetAllMilestoneFormDatesAsync(Arg.Any<string>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<MilestoneFormDatesDto>> { Success = true, Data = formDates ?? [] });

            _mapper.Map<List<MilestoneItem>>(Arg.Any<List<MilestoneDto>>()).Returns([]);
            _mapper.Map<List<MilestoneFormDatesItem>>(Arg.Any<List<MilestoneFormDatesDto>>()).Returns([]);
        }

        /// <summary>Sets up mocks for the LogIndex path.</summary>
        private void SetupSuccessfulLogIndexMocks(
            List<ProjectListMilestoneDto>? projects      = null,
            List<LogMilestoneDto>?         logMilestones = null)
        {
            var projectList = projects ?? [new ProjectListMilestoneDto { Parentproject = "PP001" }];
            _projectListService.GetAllProjectsForMilestoneAsync()
                .Returns(new ApiResponseDto<List<ProjectListMilestoneDto>> { Success = true, Data = projectList });
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _milestoneService.GetLogMilestonesAsync(
                    Arg.Any<QueryParameters<string>>(),
                    Arg.Any<string?>(),
                    Arg.Any<string?>(),
                    Arg.Any<string?>())
                .Returns(new ApiResponseDto<List<LogMilestoneDto>> { Success = true, Data = logMilestones ?? [] });
            _mapper.Map<List<LogMilestoneItem>>(Arg.Any<List<LogMilestoneDto>>()).Returns([]);
        }

        /// <summary>Sets up mocks for the LoadLogMilestonesGrid path.</summary>
        private void SetupLogGridMocks(List<LogMilestoneDto>? logMilestones = null)
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _milestoneService.GetLogMilestonesAsync(
                    Arg.Any<QueryParameters<string>>(),
                    Arg.Any<string?>(),
                    Arg.Any<string?>(),
                    Arg.Any<string?>())
                .Returns(new ApiResponseDto<List<LogMilestoneDto>> { Success = true, Data = logMilestones ?? [] });
            _mapper.Map<List<LogMilestoneItem>>(Arg.Any<List<LogMilestoneDto>>()).Returns([]);
        }

        private static PaginationFilter<string> DefaultFilter()
            => new() { Page = 1, PageSize = 10, Filter = "{}" };

        // ── Index ───────────────────────────────────────────────────────────

        #region Index

        [Fact]
        public async Task Index_ReturnsViewResult()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_ReturnsViewResultWithMilestoneViewModel()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<MilestoneViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Index_SetsParentprojectFromParameter()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var model = Assert.IsType<MilestoneViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("PP001", model.Parentproject);
        }

        [Fact]
        public async Task Index_WhenNoParentproject_DefaultsToFirstProjectOption()
        {
            // Arrange
            var projects = new List<ProjectListMilestoneDto>
            {
                new() { Parentproject = "PP001", Formrequired = false },
                new() { Parentproject = "PP002", Formrequired = false }
            };
            SetupSuccessfulIndexMocks(projects: projects);

            // Act
            var result = await _controller.Index(null);

            // Assert
            var model = Assert.IsType<MilestoneViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("PP001", model.Parentproject);
        }

        [Fact]
        public async Task Index_PopulatesProjectOptions()
        {
            // Arrange
            var projects = new List<ProjectListMilestoneDto>
            {
                new() { Parentproject = "PP001" },
                new() { Parentproject = "PP002" }
            };
            SetupSuccessfulIndexMocks(projects: projects);

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var model = Assert.IsType<MilestoneViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(2, model.ProjectOptions.Count);
            Assert.Contains(model.ProjectOptions, o => o.Value == "PP001");
            Assert.Contains(model.ProjectOptions, o => o.Value == "PP002");
        }

        [Fact]
        public async Task Index_SetsFormRequired_FromMatchingProject()
        {
            // Arrange
            var projects = new List<ProjectListMilestoneDto>
            {
                new() { Parentproject = "PP001", Formrequired = true },
                new() { Parentproject = "PP002", Formrequired = false }
            };
            SetupSuccessfulIndexMocks(projects: projects);

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var model = Assert.IsType<MilestoneViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.True(model.FormRequired);
        }

        [Fact]
        public async Task Index_CallsGetAllProjectsForMilestoneAsync_Once()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            await _controller.Index("PP001");

            // Assert
            await _projectListService.Received(1).GetAllProjectsForMilestoneAsync();
        }

        [Fact]
        public async Task Index_CallsGetAllMilestonesAsync_Once()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            await _controller.Index("PP001");

            // Assert
            await _milestoneService.Received(1)
                .GetAllMilestonesAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Index_CallsGetAllMilestoneFormDatesAsync_Once()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            await _controller.Index("PP001");

            // Assert
            await _milestoneService.Received(1)
                .GetAllMilestoneFormDatesAsync(Arg.Any<string>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task Index_WhenNullParentprojectAndNoProjects_SetsEmptyParentproject()
        {
            // Arrange
            SetupSuccessfulIndexMocks(projects: []);

            // Act
            var result = await _controller.Index(null);

            // Assert
            var model = Assert.IsType<MilestoneViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(string.Empty, model.Parentproject);
        }

        #endregion

        // ── LoadMilestoneGrid ────────────────────────────────────────────────

        #region LoadMilestoneGrid

        [Fact]
        public async Task LoadMilestoneGrid_WithValidRequest_ReturnsPartialView()
        {
            // Arrange
            SetupGridMocks();

            // Act
            var result = await _controller.LoadMilestoneGrid(DefaultFilter(), "PP001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadMilestoneGrid_WithValidRequest_ReturnsDataGridConfig()
        {
            // Arrange
            SetupGridMocks();

            // Act
            var result = await _controller.LoadMilestoneGrid(DefaultFilter(), "PP001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.IsType<DataGridConfig<MilestoneItem>>(partial.Model);
        }

        [Fact]
        public async Task LoadMilestoneGrid_WhenModelStateInvalid_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Page", "Page must be greater than 0.");

            // Act
            var result = await _controller.LoadMilestoneGrid(new PaginationFilter<string> { Page = -1 }, "PP001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
        }

        [Fact]
        public async Task LoadMilestoneGrid_CallsGetAllMilestonesAsync_Once()
        {
            // Arrange
            SetupGridMocks();

            // Act
            await _controller.LoadMilestoneGrid(DefaultFilter(), "PP001");

            // Assert
            await _milestoneService.Received(1)
                .GetAllMilestonesAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task LoadMilestoneGrid_WhenNullParentproject_UsesEmptyString()
        {
            // Arrange
            SetupGridMocks();

            // Act
            var result = await _controller.LoadMilestoneGrid(DefaultFilter(), null);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            await _milestoneService.Received(1)
                .GetAllMilestonesAsync(Arg.Any<QueryParameters<string>>(), Arg.Is<string>(s => s == string.Empty));
        }

        #endregion

        // ── LoadMilestoneFormDatesGrid ───────────────────────────────────────

        #region LoadMilestoneFormDatesGrid

        [Fact]
        public async Task LoadMilestoneFormDatesGrid_WithValidRequest_ReturnsPartialView()
        {
            // Arrange
            SetupGridMocks();

            // Act
            var result = await _controller.LoadMilestoneFormDatesGrid(DefaultFilter(), "PP001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadMilestoneFormDatesGrid_WithValidRequest_ReturnsDataGridConfig()
        {
            // Arrange
            SetupGridMocks();

            // Act
            var result = await _controller.LoadMilestoneFormDatesGrid(DefaultFilter(), "PP001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.IsType<DataGridConfig<MilestoneFormDatesItem>>(partial.Model);
        }

        [Fact]
        public async Task LoadMilestoneFormDatesGrid_WhenModelStateInvalid_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Page", "Page must be greater than 0.");

            // Act
            var result = await _controller.LoadMilestoneFormDatesGrid(new PaginationFilter<string> { Page = -1 }, "PP001");

            // Assert
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task LoadMilestoneFormDatesGrid_CallsGetAllMilestoneFormDatesAsync_Once()
        {
            // Arrange
            SetupGridMocks();

            // Act
            await _controller.LoadMilestoneFormDatesGrid(DefaultFilter(), "PP001");

            // Assert
            await _milestoneService.Received(1)
                .GetAllMilestoneFormDatesAsync(Arg.Any<string>(), Arg.Any<QueryParameters<string>>());
        }

        #endregion

        // ── GetAddEditMilestonePartial ───────────────────────────────────────

        #region GetAddEditMilestonePartial

        [Fact]
        public async Task GetAddEditMilestonePartial_WhenNoNumber_ReturnsPartialViewWithNewModel()
        {
            // Arrange
            _milestoneService.GetMilestoneTypesAsync(Arg.Any<string?>())
                .Returns(new ApiResponseDto<List<MilestoneTypeDto>> { Success = true, Data = [] });

            // Act
            var result = await _controller.GetAddEditMilestonePartial("PP001", null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditMilestone", partial.ViewName);
            var model = Assert.IsType<MilestoneItem>(partial.Model);
            Assert.Equal("PP001", model.Project);
        }

        [Fact]
        public async Task GetAddEditMilestonePartial_WhenNumberProvided_CallsGetMilestoneAsync()
        {
            // Arrange
            const string number = "M1";
            var dto = new MilestoneDto { Project = "PP001", Number = number };

            _milestoneService.GetMilestoneAsync("PP001", number)
                .Returns(new ApiResponseDto<MilestoneDto> { Success = true, Data = dto });
            _milestoneService.GetMilestoneTypesAsync(Arg.Any<string?>())
                .Returns(new ApiResponseDto<List<MilestoneTypeDto>> { Success = true, Data = [] });
            _mapper.Map<MilestoneItem>(dto).Returns(new MilestoneItem { Project = "PP001", Number = number });

            // Act
            await _controller.GetAddEditMilestonePartial("PP001", number);

            // Assert
            await _milestoneService.Received(1).GetMilestoneAsync("PP001", number);
        }

        [Fact]
        public async Task GetAddEditMilestonePartial_WhenNumberProvided_MapsAndReturnsMilestoneItem()
        {
            // Arrange
            const string number = "M1";
            var dto       = new MilestoneDto { Project = "PP001", Number = number, Description = "Test" };
            var mapped    = new MilestoneItem { Project = "PP001", Number = number, Description = "Test" };

            _milestoneService.GetMilestoneAsync("PP001", number)
                .Returns(new ApiResponseDto<MilestoneDto> { Success = true, Data = dto });
            _milestoneService.GetMilestoneTypesAsync(Arg.Any<string?>())
                .Returns(new ApiResponseDto<List<MilestoneTypeDto>> { Success = true, Data = [] });
            _mapper.Map<MilestoneItem>(dto).Returns(mapped);

            // Act
            var result = await _controller.GetAddEditMilestonePartial("PP001", number);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model   = Assert.IsType<MilestoneItem>(partial.Model);
            Assert.Equal("Test", model.Description);
        }

        [Fact]
        public async Task GetAddEditMilestonePartial_SetsIsAddingNewFalse_WhenNumberProvided()
        {
            // Arrange
            const string number = "M1";
            var dto    = new MilestoneDto { Project = "PP001", Number = number };
            var mapped = new MilestoneItem { Project = "PP001", Number = number };

            _milestoneService.GetMilestoneAsync("PP001", number)
                .Returns(new ApiResponseDto<MilestoneDto> { Success = true, Data = dto });
            _milestoneService.GetMilestoneTypesAsync(Arg.Any<string?>())
                .Returns(new ApiResponseDto<List<MilestoneTypeDto>> { Success = true, Data = [] });
            _mapper.Map<MilestoneItem>(dto).Returns(mapped);

            // Act
            var result = await _controller.GetAddEditMilestonePartial("PP001", number);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.False((bool)partial.ViewData!["IsAddingNew"]!);
        }

        [Fact]
        public async Task GetAddEditMilestonePartial_SetsIsAddingNewTrue_WhenNoNumber()
        {
            // Arrange
            _milestoneService.GetMilestoneTypesAsync(Arg.Any<string?>())
                .Returns(new ApiResponseDto<List<MilestoneTypeDto>> { Success = true, Data = [] });

            // Act
            var result = await _controller.GetAddEditMilestonePartial("PP001", null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.True((bool)partial.ViewData!["IsAddingNew"]!);
        }

        #endregion

        // ── SaveMilestone ────────────────────────────────────────────────────

        #region SaveMilestone

        [Fact]
        public async Task SaveMilestone_WhenModelStateInvalid_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Number", "Number is required");
            var item = new MilestoneItem { Project = "PP001" };

            // Act
            var result = await _controller.SaveMilestone(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
        }

        [Fact]
        public async Task SaveMilestone_WhenIsAddingNew_CallsSaveMilestoneAsync()
        {
            // Arrange
            var item = new MilestoneItem { Project = "PP001", Number = "01/01", IsAddingNew = true };
            var dto  = new MilestoneDto  { Project = "PP001", Number = "01/01" };

            _mapper.Map<MilestoneDto>(item).Returns(dto);
            _milestoneService.SaveMilestoneAsync("PP001", dto)
                .Returns(new ApiResponseDto<MilestoneDto> { Success = true, Data = dto });

            // Act
            await _controller.SaveMilestone(item);

            // Assert
            await _milestoneService.Received(1).SaveMilestoneAsync("PP001", dto);
            await _milestoneService.DidNotReceive().UpdateMilestoneAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<MilestoneDto>());
        }

        [Fact]
        public async Task SaveMilestone_WhenNotAddingNew_CallsUpdateMilestoneAsync()
        {
            // Arrange
            var item = new MilestoneItem { Project = "PP001", Number = "01/01", IsAddingNew = false };
            var dto  = new MilestoneDto  { Project = "PP001", Number = "01/01" };

            _mapper.Map<MilestoneDto>(item).Returns(dto);
            _milestoneService.UpdateMilestoneAsync("PP001", "01/01", dto)
                .Returns(new ApiResponseDto<MilestoneDto> { Success = true, Data = dto });

            // Act
            await _controller.SaveMilestone(item);

            // Assert
            await _milestoneService.Received(1).UpdateMilestoneAsync("PP001", "01/01", dto);
            await _milestoneService.DidNotReceive().SaveMilestoneAsync(Arg.Any<string>(), Arg.Any<MilestoneDto>());
        }

        [Fact]
        public async Task SaveMilestone_WhenSaveSucceeds_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var item = new MilestoneItem { Project = "PP001", Number = "01/01", IsAddingNew = true };
            var dto  = new MilestoneDto  { Project = "PP001", Number = "01/01" };

            _mapper.Map<MilestoneDto>(item).Returns(dto);
            _milestoneService.SaveMilestoneAsync("PP001", dto)
                .Returns(new ApiResponseDto<MilestoneDto> { Success = true, Data = dto });

            // Act
            var result = await _controller.SaveMilestone(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
        }

        [Fact]
        public async Task SaveMilestone_WhenSaveFails_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var item   = new MilestoneItem { Project = "PP001", Number = "01/01", IsAddingNew = true };
            var dto    = new MilestoneDto  { Project = "PP001", Number = "01/01" };
            var errors = new List<ApiErrorDto> { new() { Code = "VALIDATION_ERROR", Message = "Number already exists." } };

            _mapper.Map<MilestoneDto>(item).Returns(dto);
            _milestoneService.SaveMilestoneAsync("PP001", dto)
                .Returns(new ApiResponseDto<MilestoneDto> { Success = false, Errors = errors });

            // Act
            var result = await _controller.SaveMilestone(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
        }

        #endregion

        // ── DeleteMilestone ──────────────────────────────────────────────────

        #region DeleteMilestone

        [Fact]
        public async Task DeleteMilestone_WhenDeleteSucceeds_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            _milestoneService.DeleteMilestoneAsync("PP001", "01/01")
                .Returns(new ApiResponseDto<object> { Success = true });

            // Act
            var result = await _controller.DeleteMilestone("PP001", "01/01");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
            await _milestoneService.Received(1).DeleteMilestoneAsync("PP001", "01/01");
        }

        [Fact]
        public async Task DeleteMilestone_WhenDeleteFails_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Milestone not found." } };
            _milestoneService.DeleteMilestoneAsync("PP001", "01/01")
                .Returns(new ApiResponseDto<object> { Success = false, Errors = errors });

            // Act
            var result = await _controller.DeleteMilestone("PP001", "01/01");

            // Assert
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task DeleteMilestone_UrlDecodesNumber_BeforeCallingService()
        {
            // Arrange
            const string encodedNumber = "01%2F01";
            const string decodedNumber = "01/01";

            _milestoneService.DeleteMilestoneAsync("PP001", decodedNumber)
                .Returns(new ApiResponseDto<object> { Success = true });

            // Act
            await _controller.DeleteMilestone("PP001", encodedNumber);

            // Assert
            await _milestoneService.Received(1).DeleteMilestoneAsync("PP001", decodedNumber);
        }

        #endregion

        // ── GetAddEditMilestoneFormDatesPartial ──────────────────────────────

        #region GetAddEditMilestoneFormDatesPartial

        [Fact]
        public async Task GetAddEditMilestoneFormDatesPartial_WhenNoYear_ReturnsPartialViewWithNewModel()
        {
            // Act
            var result = await _controller.GetAddEditMilestoneFormDatesPartial("PP001", null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditMilestoneFormDates", partial.ViewName);
            var model = Assert.IsType<MilestoneFormDatesItem>(partial.Model);
            Assert.Equal("PP001", model.ParentProject);
        }

        [Fact]
        public async Task GetAddEditMilestoneFormDatesPartial_SetsIsAddingNewTrue_WhenNoYear()
        {
            // Act
            var result = await _controller.GetAddEditMilestoneFormDatesPartial("PP001", null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.True((bool)partial.ViewData!["IsAddingNew"]!);
        }

        [Fact]
        public async Task GetAddEditMilestoneFormDatesPartial_WhenYearProvided_CallsGetMilestoneFormDatesAsync()
        {
            // Arrange
            const short year = 2024;
            var dto    = new MilestoneFormDatesDto { Year = year, ParentProject = "PP001" };
            var mapped = new MilestoneFormDatesItem { Year = year, ParentProject = "PP001" };

            _milestoneService.GetMilestoneFormDatesAsync("PP001", year)
                .Returns(new ApiResponseDto<MilestoneFormDatesDto> { Success = true, Data = dto });
            _mapper.Map<MilestoneFormDatesItem>(dto).Returns(mapped);

            // Act
            await _controller.GetAddEditMilestoneFormDatesPartial("PP001", year);

            // Assert
            await _milestoneService.Received(1).GetMilestoneFormDatesAsync("PP001", year);
        }

        [Fact]
        public async Task GetAddEditMilestoneFormDatesPartial_WhenYearProvided_MapsAndReturnsItem()
        {
            // Arrange
            const short year = 2024;
            var dto    = new MilestoneFormDatesDto { Year = year, ParentProject = "PP001", Jan = new DateTime(2024, 1, 31) };
            var mapped = new MilestoneFormDatesItem { Year = year, ParentProject = "PP001", Jan = new DateTime(2024, 1, 31) };

            _milestoneService.GetMilestoneFormDatesAsync("PP001", year)
                .Returns(new ApiResponseDto<MilestoneFormDatesDto> { Success = true, Data = dto });
            _mapper.Map<MilestoneFormDatesItem>(dto).Returns(mapped);

            // Act
            var result = await _controller.GetAddEditMilestoneFormDatesPartial("PP001", year);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model   = Assert.IsType<MilestoneFormDatesItem>(partial.Model);
            Assert.Equal(year, model.Year);
            Assert.Equal(new DateTime(2024, 1, 31), model.Jan);
        }

        [Fact]
        public async Task GetAddEditMilestoneFormDatesPartial_SetsIsAddingNewFalse_WhenYearProvided()
        {
            // Arrange
            const short year = 2024;
            var dto    = new MilestoneFormDatesDto { Year = year, ParentProject = "PP001" };
            var mapped = new MilestoneFormDatesItem { Year = year, ParentProject = "PP001" };

            _milestoneService.GetMilestoneFormDatesAsync("PP001", year)
                .Returns(new ApiResponseDto<MilestoneFormDatesDto> { Success = true, Data = dto });
            _mapper.Map<MilestoneFormDatesItem>(dto).Returns(mapped);

            // Act
            var result = await _controller.GetAddEditMilestoneFormDatesPartial("PP001", year);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.False((bool)partial.ViewData!["IsAddingNew"]!);
        }

        #endregion

        // ── SaveMilestoneFormDates ───────────────────────────────────────────

        #region SaveMilestoneFormDates

        [Fact]
        public async Task SaveMilestoneFormDates_WhenModelStateInvalid_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Year", "Financial Year is required");
            var item = new MilestoneFormDatesItem { ParentProject = "PP001" };

            // Act
            var result = await _controller.SaveMilestoneFormDates(item);

            // Assert
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task SaveMilestoneFormDates_WhenSaveSucceeds_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var item = new MilestoneFormDatesItem { ParentProject = "PP001", Year = 2024 };
            var dto  = new MilestoneFormDatesDto  { ParentProject = "PP001", Year = 2024 };

            _mapper.Map<MilestoneFormDatesDto>(item).Returns(dto);
            _milestoneService.SaveMilestoneFormDatesAsync("PP001", dto)
                .Returns(new ApiResponseDto<MilestoneFormDatesDto> { Success = true, Data = dto });

            // Act
            var result = await _controller.SaveMilestoneFormDates(item);

            // Assert
            Assert.IsType<JsonResult>(result);
            await _milestoneService.Received(1).SaveMilestoneFormDatesAsync("PP001", dto);
        }

        [Fact]
        public async Task SaveMilestoneFormDates_WhenSaveFails_ReturnsJsonWithErrors()
        {
            // Arrange
            var item   = new MilestoneFormDatesItem { ParentProject = "PP001", Year = 2024 };
            var dto    = new MilestoneFormDatesDto  { ParentProject = "PP001", Year = 2024 };
            var errors = new List<ApiErrorDto> { new() { Code = "VALIDATION_ERROR", Message = "Year already exists." } };

            _mapper.Map<MilestoneFormDatesDto>(item).Returns(dto);
            _milestoneService.SaveMilestoneFormDatesAsync("PP001", dto)
                .Returns(new ApiResponseDto<MilestoneFormDatesDto> { Success = false, Errors = errors });

            // Act
            var result = await _controller.SaveMilestoneFormDates(item);

            // Assert
            Assert.IsType<JsonResult>(result);
        }

        #endregion

        // ── DeleteMilestoneFormDates ─────────────────────────────────────────

        #region DeleteMilestoneFormDates

        [Fact]
        public async Task DeleteMilestoneFormDates_WhenDeleteSucceeds_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            _milestoneService.DeleteMilestoneFormDatesAsync("PP001", 2024)
                .Returns(new ApiResponseDto<object> { Success = true });

            // Act
            var result = await _controller.DeleteMilestoneFormDates("PP001", 2024);

            // Assert
            Assert.IsType<JsonResult>(result);
            await _milestoneService.Received(1).DeleteMilestoneFormDatesAsync("PP001", 2024);
        }

        [Fact]
        public async Task DeleteMilestoneFormDates_WhenDeleteFails_ReturnsJsonWithErrors()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Record not found." } };
            _milestoneService.DeleteMilestoneFormDatesAsync("PP001", 9999)
                .Returns(new ApiResponseDto<object> { Success = false, Errors = errors });

            // Act
            var result = await _controller.DeleteMilestoneFormDates("PP001", 9999);

            // Assert
            Assert.IsType<JsonResult>(result);
        }

        #endregion

        // ── UpdateFormRequired ───────────────────────────────────────────────

        #region UpdateFormRequired

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task UpdateFormRequired_WhenSucceeds_ReturnsJsonWithSuccessTrue(bool formRequired)
        {
            // Arrange
            _milestoneService.UpdateFormRequiredAsync("PP001", formRequired)
                .Returns(new ApiResponseDto<object> { Success = true });

            // Act
            var result = await _controller.UpdateFormRequired("PP001", formRequired);

            // Assert
            Assert.IsType<JsonResult>(result);
            await _milestoneService.Received(1).UpdateFormRequiredAsync("PP001", formRequired);
        }

        [Fact]
        public async Task UpdateFormRequired_WhenFails_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "SERVER_ERROR", Message = "Failed." } };
            _milestoneService.UpdateFormRequiredAsync("PP001", true)
                .Returns(new ApiResponseDto<object> { Success = false, Errors = errors });

            // Act
            var result = await _controller.UpdateFormRequired("PP001", true);

            // Assert
            Assert.IsType<JsonResult>(result);
        }

        #endregion

        // ── GetFormRequired ──────────────────────────────────────────────────

        #region GetFormRequired

        [Fact]
        public async Task GetFormRequired_ReturnsJsonWithFormRequired_WhenProjectFound()
        {
            // Arrange
            var projects = new List<ProjectListMilestoneDto>
            {
                new() { Parentproject = "PP001", Formrequired = true },
                new() { Parentproject = "PP002", Formrequired = false }
            };
            _projectListService.GetAllProjectsForMilestoneAsync()
                .Returns(new ApiResponseDto<List<ProjectListMilestoneDto>> { Success = true, Data = projects });

            // Act
            var result = await _controller.GetFormRequired("PP001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
            await _projectListService.Received(1).GetAllProjectsForMilestoneAsync();
        }

        [Fact]
        public async Task GetFormRequired_ReturnsFalse_WhenProjectNotFound()
        {
            // Arrange
            _projectListService.GetAllProjectsForMilestoneAsync()
                .Returns(new ApiResponseDto<List<ProjectListMilestoneDto>> { Success = true, Data = [] });

            // Act
            var result = await _controller.GetFormRequired("UNKNOWN");

            // Assert
            Assert.IsType<JsonResult>(result);
        }

        #endregion

        // ── LogIndex ─────────────────────────────────────────────────────────

        #region LogIndex

        [Fact]
        public async Task LogIndex_ReturnsViewResult()
        {
            // Arrange
            SetupSuccessfulLogIndexMocks();

            // Act
            var result = await _controller.LogIndex("PP001");

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task LogIndex_ReturnsViewResultWithMilestoneViewModel()
        {
            // Arrange
            SetupSuccessfulLogIndexMocks();

            // Act
            var result = await _controller.LogIndex("PP001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<MilestoneViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task LogIndex_SetsParentprojectFromParameter()
        {
            // Arrange
            SetupSuccessfulLogIndexMocks();

            // Act
            var result = await _controller.LogIndex("PP001");

            // Assert
            var model = Assert.IsType<MilestoneViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("PP001", model.Parentproject);
        }

        [Fact]
        public async Task LogIndex_WhenNoProject_SetsEmptyParentproject()
        {
            // Arrange
            SetupSuccessfulLogIndexMocks();

            // Act
            var result = await _controller.LogIndex(null);

            // Assert
            var model = Assert.IsType<MilestoneViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(string.Empty, model.Parentproject);
        }

        [Fact]
        public async Task LogIndex_PopulatesProjectOptions()
        {
            // Arrange
            var projects = new List<ProjectListMilestoneDto>
            {
                new() { Parentproject = "PP001" },
                new() { Parentproject = "PP002" }
            };
            SetupSuccessfulLogIndexMocks(projects: projects);

            // Act
            var result = await _controller.LogIndex("PP001");

            // Assert
            var model = Assert.IsType<MilestoneViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(2, model.ProjectOptions.Count);
            Assert.Contains(model.ProjectOptions, o => o.Value == "PP001");
            Assert.Contains(model.ProjectOptions, o => o.Value == "PP002");
        }

        [Fact]
        public async Task LogIndex_CallsGetAllProjectsForMilestoneAsync_Once()
        {
            // Arrange
            SetupSuccessfulLogIndexMocks();

            // Act
            await _controller.LogIndex("PP001");

            // Assert
            await _projectListService.Received(1).GetAllProjectsForMilestoneAsync();
        }

        [Fact]
        public async Task LogIndex_CallsGetLogMilestonesAsync_Once()
        {
            // Arrange
            SetupSuccessfulLogIndexMocks();

            // Act
            await _controller.LogIndex("PP001");

            // Assert
            await _milestoneService.Received(1)
                .GetLogMilestonesAsync(
                    Arg.Any<QueryParameters<string>>(),
                    Arg.Any<string?>(),
                    Arg.Any<string?>(),
                    Arg.Any<string?>());
        }

        #endregion

        // ── LoadLogMilestonesGrid ────────────────────────────────────────────

        #region LoadLogMilestonesGrid

        [Fact]
        public async Task LoadLogMilestonesGrid_WithValidRequest_ReturnsPartialView()
        {
            // Arrange
            SetupLogGridMocks();

            // Act
            var result = await _controller.LoadLogMilestonesGrid(DefaultFilter(), "PP001", null, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadLogMilestonesGrid_WithValidRequest_ReturnsDataGridConfig()
        {
            // Arrange
            SetupLogGridMocks();

            // Act
            var result = await _controller.LoadLogMilestonesGrid(DefaultFilter(), "PP001", null, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.IsType<DataGridConfig<LogMilestoneItem>>(partial.Model);
        }

        [Fact]
        public async Task LoadLogMilestonesGrid_WhenModelStateInvalid_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Page", "Page must be greater than 0.");

            // Act
            var result = await _controller.LoadLogMilestonesGrid(
                new PaginationFilter<string> { Page = -1 }, "PP001", null, null);

            // Assert
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task LoadLogMilestonesGrid_CallsGetLogMilestonesAsync_Once()
        {
            // Arrange
            SetupLogGridMocks();

            // Act
            await _controller.LoadLogMilestonesGrid(DefaultFilter(), "PP001", null, null);

            // Assert
            await _milestoneService.Received(1)
                .GetLogMilestonesAsync(
                    Arg.Any<QueryParameters<string>>(),
                    Arg.Any<string?>(),
                    Arg.Any<string?>(),
                    Arg.Any<string?>());
        }

        [Fact]
        public async Task LoadLogMilestonesGrid_WithAllOptionalParams_PassesThemToService()
        {
            // Arrange
            const string project     = "PP001";
            const string numberPart1 = "M";
            const string numberPart2 = "1";
            SetupLogGridMocks();

            // Act
            await _controller.LoadLogMilestonesGrid(DefaultFilter(), project, numberPart1, numberPart2);

            // Assert
            await _milestoneService.Received(1).GetLogMilestonesAsync(
                Arg.Any<QueryParameters<string>>(),
                Arg.Is<string?>(p => p == project),
                Arg.Is<string?>(n => n == numberPart1),
                Arg.Is<string?>(n => n == numberPart2));
        }

        [Fact]
        public async Task LoadLogMilestonesGrid_WhenNullOptionalParams_StillReturnsPartialView()
        {
            // Arrange
            SetupLogGridMocks();

            // Act
            var result = await _controller.LoadLogMilestonesGrid(DefaultFilter(), null, null, null);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            await _milestoneService.Received(1)
                .GetLogMilestonesAsync(
                    Arg.Any<QueryParameters<string>>(),
                    Arg.Is<string?>(p => p == null),
                    Arg.Is<string?>(n => n == null),
                    Arg.Is<string?>(n => n == null));
        }

        #endregion
    }
}
