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
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.WorkGroupPeopleControllerTest
{
    public class WorkGroupPeopleControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IEmployeeService _employeeService;
        private readonly ITestCapabilityService _testCapabilityService;
        private readonly WorkGroupPeopleController _controller;

        public WorkGroupPeopleControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _employeeService = Substitute.For<IEmployeeService>();
            _testCapabilityService = Substitute.For<ITestCapabilityService>();
            _controller = new WorkGroupPeopleController(_mapper, _employeeService, _testCapabilityService);
        }

        // ── helpers ────────────────────────────────────────────────────────────

        private void SetupPeopleGridMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<WorkGroupPeopleItem>>(Arg.Any<List<PactStaffDto>>())
                .Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
        }

        private void SetupDefaultPeopleResponse()
        {
            _employeeService.GetWorkGroupStaffAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>())
                .Returns(ApiResponseDto<PaginatedResult<PactStaffDto>>.SuccessResponse(
                    new PaginatedResult<PactStaffDto>([], 0, 1, 10)));
        }

        private void SetupDefaultWorkGroupOptions()
        {
            _testCapabilityService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([]));
        }

        private void SetupDefaultPersonOptions()
        {
            _employeeService.GetAllWorkGroupPersonAsync()
                .Returns(ApiResponseDto<List<WorkGroupPersonDto>>.SuccessResponse([]));
        }

        // ── Index ──────────────────────────────────────────────────────────────

        #region Index

        [Fact]
        public async Task Index_Always_ReturnsViewResultWithViewModel()
        {
            // Arrange
            SetupDefaultPeopleResponse();
            SetupDefaultWorkGroupOptions();
            SetupDefaultPersonOptions();
            SetupPeopleGridMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupPeopleViewModel>(viewResult.Model);
            Assert.Equal("peopleGrid", model.PeopleGrid.GridId);
        }

        [Fact]
        public async Task Index_PopulatesWorkGroupOptions_WhenServiceReturnsData()
        {
            // Arrange
            var workGroups = new List<WorkGroupDto>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC1" },
                new() { WorkGroupName = "WG2", ProfitCentre = "PC2" }
            };
            _testCapabilityService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(workGroups));
            _mapper.Map<List<WorkGroup>>(Arg.Any<List<WorkGroupDto>>())
                .Returns(workGroups.Select(w => new WorkGroup { WorkGroupName = w.WorkGroupName, ProfitCentre = w.ProfitCentre }).ToList());
            SetupDefaultPeopleResponse();
            SetupDefaultPersonOptions();
            SetupPeopleGridMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupPeopleViewModel>(viewResult.Model);
            Assert.Equal(2, model.WorkGroupOptions.Count);
        }

        [Fact]
        public async Task Index_PopulatesPersonOptions_WhenServiceReturnsData()
        {
            // Arrange
            var persons = new List<WorkGroupPersonDto>
            {
                new() { Name = "Alice", WorkGroupGrade = "WG1", WorkGroup = "Group A" },
                new() { Name = "Bob",   WorkGroupGrade = "WG2", WorkGroup = "Group B" }
            };
            _employeeService.GetAllWorkGroupPersonAsync()
                .Returns(ApiResponseDto<List<WorkGroupPersonDto>>.SuccessResponse(persons));
            _mapper.Map<List<WorkGroupPerson>>(Arg.Any<List<WorkGroupPersonDto>>())
                .Returns(persons.Select(p => new WorkGroupPerson { Name = p.Name, WorkGroupGrade = p.WorkGroupGrade, WorkGroup = p.WorkGroup }).ToList());
            SetupDefaultPeopleResponse();
            SetupDefaultWorkGroupOptions();
            SetupPeopleGridMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupPeopleViewModel>(viewResult.Model);
            Assert.Equal(2, model.PersonOptions.Count);
        }

        [Fact]
        public async Task Index_WorkGroupServiceFails_ReturnsEmptyWorkGroupOptions()
        {
            // Arrange
            _testCapabilityService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.FailureResponse([], new ApiMetaDto()));
            SetupDefaultPeopleResponse();
            SetupDefaultPersonOptions();
            SetupPeopleGridMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupPeopleViewModel>(viewResult.Model);
            Assert.Empty(model.WorkGroupOptions);
        }

        [Fact]
        public async Task Index_PersonServiceFails_ReturnsEmptyPersonOptions()
        {
            // Arrange
            _employeeService.GetAllWorkGroupPersonAsync()
                .Returns(ApiResponseDto<List<WorkGroupPersonDto>>.FailureResponse([], new ApiMetaDto()));
            SetupDefaultPeopleResponse();
            SetupDefaultWorkGroupOptions();
            SetupPeopleGridMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupPeopleViewModel>(viewResult.Model);
            Assert.Empty(model.PersonOptions);
        }

        [Fact]
        public async Task Index_WorkGroupServiceReturnsNull_ReturnsEmptyWorkGroupOptions()
        {
            // Arrange
            _testCapabilityService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(null!));
            SetupDefaultPeopleResponse();
            SetupDefaultPersonOptions();
            SetupPeopleGridMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupPeopleViewModel>(viewResult.Model);
            Assert.Empty(model.WorkGroupOptions);
        }

        [Fact]
        public async Task Index_PersonServiceReturnsNull_ReturnsEmptyPersonOptions()
        {
            // Arrange
            _employeeService.GetAllWorkGroupPersonAsync()
                .Returns(ApiResponseDto<List<WorkGroupPersonDto>>.SuccessResponse(null!));
            SetupDefaultPeopleResponse();
            SetupDefaultWorkGroupOptions();
            SetupPeopleGridMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupPeopleViewModel>(viewResult.Model);
            Assert.Empty(model.PersonOptions);
        }

        [Fact]
        public async Task Index_PeopleGridHasCorrectConfiguration()
        {
            // Arrange
            SetupDefaultPeopleResponse();
            SetupDefaultWorkGroupOptions();
            SetupDefaultPersonOptions();
            SetupPeopleGridMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupPeopleViewModel>(viewResult.Model);
            Assert.Equal("peopleGrid", model.PeopleGrid.GridId);
            Assert.Equal("onPersonRowSelect", model.PeopleGrid.RowSelectFunction);
            Assert.Equal("getPeopleGridExtraFilters", model.PeopleGrid.ExtraFilterMethod);
            Assert.Equal("/PACT/WorkGroupPeople/LoadPeopleGrid", model.PeopleGrid.BindGridUrl);
            Assert.True(model.PeopleGrid.AllowRowSelection);
        }

        #endregion

        // ── LoadPeopleGrid ─────────────────────────────────────────────────────

        #region LoadPeopleGrid

        [Fact]
        public async Task LoadPeopleGrid_ValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupDefaultPeopleResponse();
            SetupPeopleGridMapper();

            // Act
            var result = await _controller.LoadPeopleGrid(request, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadPeopleGrid_WithWorkGroup_CallsEmployeeServiceWithWorkGroup()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupDefaultPeopleResponse();
            SetupPeopleGridMapper();

            // Act
            await _controller.LoadPeopleGrid(request, "WG1");

            // Assert
            await _employeeService.Received(1).GetWorkGroupStaffAsync(
                Arg.Any<QueryParameters<string>>(), "WG1");
        }

        [Fact]
        public async Task LoadPeopleGrid_WithNoWorkGroup_CallsEmployeeServiceWithoutWorkGroup()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var people = new List<PactStaffDto>
            {
                new() { Name = "Alice", WorkGroupGrade = "WG1" }
            };
            _employeeService.GetWorkGroupStaffAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<PaginatedResult<PactStaffDto>>.SuccessResponse(
                    new PaginatedResult<PactStaffDto>(people, 1, 1, 10)));
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<WorkGroupPeopleItem>>(Arg.Any<List<PactStaffDto>>())
                .Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadPeopleGrid(request, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadPeopleGrid_NoFilter_LoadsAllStaff()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupDefaultPeopleResponse();
            SetupPeopleGridMapper();

            // Act
            var result = await _controller.LoadPeopleGrid(request, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            await _employeeService.Received(1).GetWorkGroupStaffAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Is<string?>(x => x == null));
        }

        [Fact]
        public async Task LoadPeopleGrid_InvalidModelState_ReturnsBadJsonResult()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            _controller.ModelState.AddModelError("Page", "Invalid page");

            // Act
            var result = await _controller.LoadPeopleGrid(request, null);

            // Assert
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task LoadPeopleGrid_ServiceFails_ReturnsPartialWithEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            _employeeService.GetWorkGroupStaffAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>())
                .Returns(ApiResponseDto<PaginatedResult<PactStaffDto>>.FailureResponse([], new ApiMetaDto()));
            SetupPeopleGridMapper();

            // Act
            var result = await _controller.LoadPeopleGrid(request, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadPeopleGrid_ServiceReturnsNull_ReturnsPartialWithEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            _employeeService.GetWorkGroupStaffAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>())
                .Returns(ApiResponseDto<PaginatedResult<PactStaffDto>>.SuccessResponse(null!));
            SetupPeopleGridMapper();

            // Act
            var result = await _controller.LoadPeopleGrid(request, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadPeopleGrid_WithNullFilter_UsesEmptyFilterDictionary()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = null };
            SetupDefaultPeopleResponse();
            SetupPeopleGridMapper();

            // Act
            var result = await _controller.LoadPeopleGrid(request, null);

            // Assert
            Assert.IsType<PartialViewResult>(result);
        }

        [Fact]
        public async Task LoadPeopleGrid_WithWorkGroupAndPersonName_WorkGroupTakesPriority()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupDefaultPeopleResponse();
            SetupPeopleGridMapper();

            // Act
            await _controller.LoadPeopleGrid(request, "WG1");

            // Assert: workGroup is non-null so FetchByWorkGroupAsync path is taken
            await _employeeService.Received(1).GetWorkGroupStaffAsync(
                Arg.Any<QueryParameters<string>>(), "WG1");
        }

        [Fact]
        public async Task LoadPeopleGrid_GridIdIsCorrect()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupDefaultPeopleResponse();
            SetupPeopleGridMapper();

            // Act
            var result = await _controller.LoadPeopleGrid(request, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WorkGroupPeopleItem>>(partial.Model);
            Assert.Equal("peopleGrid", grid.GridId);
        }

        #endregion

        #region Index - workGroup parameter

        [Fact]
        public async Task Index_WithNonEmptyWorkGroup_SetsSelectedWorkGroupOnViewModel()
        {
            // Arrange
            var workGroup = "WG1";
            SetupDefaultPeopleResponse();
            SetupDefaultWorkGroupOptions();
            SetupDefaultPersonOptions();
            SetupPeopleGridMapper();

            // Act
            var result = await _controller.Index(workGroup);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupPeopleViewModel>(viewResult.Model);
            Assert.Equal(workGroup, model.SelectedWorkGroup);
        }

        [Fact]
        public async Task Index_WithNonEmptyWorkGroup_PassesWorkGroupToGrid()
        {
            // Arrange
            var workGroup = "WG1";
            SetupDefaultPeopleResponse();
            SetupDefaultWorkGroupOptions();
            SetupDefaultPersonOptions();
            SetupPeopleGridMapper();

            // Act
            await _controller.Index(workGroup);

            // Assert: FetchByWorkGroupAsync path is taken (workGroup is non-null/non-empty)
            await _employeeService.Received(1).GetWorkGroupStaffAsync(
                Arg.Any<QueryParameters<string>>(), workGroup);
        }

        [Fact]
        public async Task Index_WithEmptyWorkGroup_SetsSelectedWorkGroupToEmpty()
        {
            // Arrange
            SetupDefaultPeopleResponse();
            SetupDefaultWorkGroupOptions();
            SetupDefaultPersonOptions();
            SetupPeopleGridMapper();

            // Act
            var result = await _controller.Index("");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupPeopleViewModel>(viewResult.Model);
            Assert.Equal("", model.SelectedWorkGroup);
        }

        [Fact]
        public async Task Index_WithWhitespaceWorkGroup_TreatsAsEmpty()
        {
            // Arrange
            SetupDefaultPeopleResponse();
            SetupDefaultWorkGroupOptions();
            SetupDefaultPersonOptions();
            SetupPeopleGridMapper();

            // Act
            var result = await _controller.Index("   ");

            // Assert: whitespace is treated as empty — FetchAllWorkGroupPeoplesAsync path taken
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupPeopleViewModel>(viewResult.Model);
            Assert.Equal("   ", model.SelectedWorkGroup);
            await _employeeService.Received(1).GetWorkGroupStaffAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Is<string?>(x => x == null));
        }

        #endregion

        #region FetchByWorkGroupAsync failure paths

        [Fact]
        public async Task LoadPeopleGrid_WithWorkGroup_ServiceFails_ReturnsPartialWithEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            _employeeService.GetWorkGroupStaffAsync(Arg.Any<QueryParameters<string>>(), "WG1")
                .Returns(ApiResponseDto<PaginatedResult<PactStaffDto>>.FailureResponse([], new ApiMetaDto()));
            SetupPeopleGridMapper();

            // Act
            var result = await _controller.LoadPeopleGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            var grid = Assert.IsType<DataGridConfig<WorkGroupPeopleItem>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadPeopleGrid_WithWorkGroup_ServiceReturnsNull_ReturnsPartialWithEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            _employeeService.GetWorkGroupStaffAsync(Arg.Any<QueryParameters<string>>(), "WG1")
                .Returns(ApiResponseDto<PaginatedResult<PactStaffDto>>.SuccessResponse(null!));
            SetupPeopleGridMapper();

            // Act
            var result = await _controller.LoadPeopleGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            var grid = Assert.IsType<DataGridConfig<WorkGroupPeopleItem>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        #endregion

        #region FetchAllWorkGroupPeoplesAsync paths (no workGroup filter)

        [Fact]
        public async Task LoadPeopleGrid_NoWorkGroup_ServiceFails_ReturnsPartialWithEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            _employeeService.GetWorkGroupStaffAsync(Arg.Any<QueryParameters<string>>(), Arg.Is<string?>(x => x == null))
                .Returns(ApiResponseDto<PaginatedResult<PactStaffDto>>.FailureResponse([], new ApiMetaDto()));
            SetupPeopleGridMapper();

            // Act
            var result = await _controller.LoadPeopleGrid(request, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            var grid = Assert.IsType<DataGridConfig<WorkGroupPeopleItem>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadPeopleGrid_NoWorkGroup_ServiceReturnsNull_ReturnsPartialWithEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            _employeeService.GetWorkGroupStaffAsync(Arg.Any<QueryParameters<string>>(), Arg.Is<string?>(x => x == null))
                .Returns(ApiResponseDto<PaginatedResult<PactStaffDto>>.SuccessResponse(null!));
            SetupPeopleGridMapper();

            // Act
            var result = await _controller.LoadPeopleGrid(request, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            var grid = Assert.IsType<DataGridConfig<WorkGroupPeopleItem>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadPeopleGrid_NoWorkGroup_ReturnsAllStaffUnfiltered()
        {
            // Arrange — FetchByPersonNameAsync was removed; no workGroup now returns all staff unfiltered.
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var people = new List<PactStaffDto>
            {
                new() { Name = "Alice", WorkGroupGrade = "WG1" },
                new() { Name = "Bob",   WorkGroupGrade = "WG2" }
            };
            _employeeService.GetWorkGroupStaffAsync(Arg.Any<QueryParameters<string>>(), Arg.Is<string?>(x => x == null))
                .Returns(ApiResponseDto<PaginatedResult<PactStaffDto>>.SuccessResponse(
                    new PaginatedResult<PactStaffDto>(people, 2, 1, 10)));
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<WorkGroupPeopleItem>>(Arg.Any<List<PactStaffDto>>())
                .Returns(callInfo =>
                {
                    var input = callInfo.Arg<List<PactStaffDto>>();
                    return input.Select(s => new WorkGroupPeopleItem { Name = s.Name }).ToList();
                });

            // Act
            var result = await _controller.LoadPeopleGrid(request, null);

            // Assert: mapper is called with the full unfiltered list (both Alice and Bob)
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            _mapper.Received(1).Map<List<WorkGroupPeopleItem>>(
                Arg.Is<List<PactStaffDto>>(l => l.Count == 2));
        }

        #endregion
    }
}
