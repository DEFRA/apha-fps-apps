using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.WorkGroupStaffMaintenanceControllerTest
{
    public class WorkGroupStaffMaintenanceControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IWorkGroupEmployeeService _workGroupEmployeeService;
        private readonly IEmployeeService _employeeService;
        private readonly IWorkGroupGradeService _workGroupGradeService;
        private readonly WorkGroupStaffMaintenanceController _controller;

        public WorkGroupStaffMaintenanceControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _workGroupEmployeeService = Substitute.For<IWorkGroupEmployeeService>();
            _employeeService = Substitute.For<IEmployeeService>();
            _workGroupGradeService = Substitute.For<IWorkGroupGradeService>();

            _controller = new WorkGroupStaffMaintenanceController(
                _mapper,
                _workGroupEmployeeService,
                _employeeService,
                _workGroupGradeService);
        }

        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json);
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkGroupStaffMaintenanceController(
                null!, _workGroupEmployeeService, _employeeService, _workGroupGradeService));
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithWorkGroupStaffMaintenanceViewModel()
        {
            var employeeDtos = new List<WorkGroupEmployeeDto>
            {
                new()
                {
                    PactId = "P001",
                    SpNumber = "SP001",
                    WorkGroupGrade = "WG-A",
                    Name = "Alice",
                    PersonStatus = "A",
                    HrsPaid = 37,
                    Leave = 0,
                    SickSpecial = 0,
                    HrsAvail = 37,
                    MakeAvailable = 1,
                    TimeRecorder = 0
                }
            };

            var apiResponse = ApiResponseDto<List<WorkGroupEmployeeDto>>.SuccessResponse(
                employeeDtos,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });

            _workGroupEmployeeService.GetWorkGroupEmployeeAsync(Arg.Any<QueryParameters<string>>(), string.Empty)
                .Returns(apiResponse);
            _mapper.Map<List<WorkGroupEmployeeItem>>(Arg.Any<List<WorkGroupEmployeeDto>>())
                .Returns(new List<WorkGroupEmployeeItem> { new() { PactId = "P001", WgGrade = "WG-A" } });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel { PageNumber = 1, PageSize = 10, TotalRecords = 1 });

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Areas/FPS/Views/WorkGroupStaffMaintenance/Index.cshtml", viewResult.ViewName);
            var model = Assert.IsType<WorkGroupStaffMaintenanceViewModel>(viewResult.Model);
            Assert.Equal("wgStaffGrid", model.WGStaffGrid.GridId);
        }

        [Fact]
        public async Task LoadWGStaffGrid_WithInvalidModelState_ReturnsJsonError()
        {
            _controller.ModelState.AddModelError("Filter", "Invalid filter");

            var result = await _controller.LoadWGStaffGrid(new PaginationFilter<string> { Filter = "{}" });

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value!.success);
            Assert.Equal("Invalid request data", value.message);
        }

        [Fact]
        public async Task LoadWGStaffGrid_WithStaffNameAndWgGradeFilters_MapsApiFilterKeysForQuery()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(callInfo =>
                {
                    var request = callInfo.Arg<PaginationFilter<string>>();
                    return new QueryParameters<string>
                    {
                        Page = request.Page,
                        PageSize = request.PageSize,
                        SortBy = request.SortBy,
                        Descending = request.Descending,
                        Filter = request.Filter
                    };
                });

            _workGroupEmployeeService.GetWorkGroupEmployeeAsync(Arg.Any<QueryParameters<string>>(), string.Empty)
                .Returns(ApiResponseDto<List<WorkGroupEmployeeDto>>.SuccessResponse(
                    new List<WorkGroupEmployeeDto>(),
                    new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }));

            _mapper.Map<List<WorkGroupEmployeeItem>>(Arg.Any<List<WorkGroupEmployeeDto>>())
                .Returns(new List<WorkGroupEmployeeItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel { PageNumber = 1, PageSize = 10, TotalRecords = 0 });

            var request = new PaginationFilter<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"StaffName\":\"Alice\",\"WgGrade\":\"WG01\"}"
            };

            var result = await _controller.LoadWGStaffGrid(request);

            Assert.IsType<PartialViewResult>(result);
            await _workGroupEmployeeService.Received(1).GetWorkGroupEmployeeAsync(
                Arg.Is<QueryParameters<string>>(q =>
                    q.Filter != null
                    && q.Filter.Contains("\"Name\":\"Alice\"")
                    && q.Filter.Contains("\"WorkGroupGrade\":\"WG01\"")),
                string.Empty);
        }

        [Theory]
        [InlineData("StaffName", "Name")]
        [InlineData("WgGrade", "WorkGroupGrade")]
        public async Task LoadWGStaffGrid_WithUiSortColumn_MapsSortForApiAndPreservesUiSortOnGrid(string uiSortBy, string apiSortBy)
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(callInfo =>
                {
                    var request = callInfo.Arg<PaginationFilter<string>>();
                    return new QueryParameters<string>
                    {
                        Page = request.Page,
                        PageSize = request.PageSize,
                        SortBy = request.SortBy,
                        Descending = request.Descending,
                        Filter = request.Filter
                    };
                });

            _workGroupEmployeeService.GetWorkGroupEmployeeAsync(Arg.Any<QueryParameters<string>>(), string.Empty)
                .Returns(ApiResponseDto<List<WorkGroupEmployeeDto>>.SuccessResponse(
                    new List<WorkGroupEmployeeDto>(),
                    new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }));

            _mapper.Map<List<WorkGroupEmployeeItem>>(Arg.Any<List<WorkGroupEmployeeDto>>())
                .Returns(new List<WorkGroupEmployeeItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel { PageNumber = 1, PageSize = 10, TotalRecords = 0 });

            var request = new PaginationFilter<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = uiSortBy,
                Descending = true,
                Filter = "{}"
            };

            var result = await _controller.LoadWGStaffGrid(request);

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupEmployeeItem>>(partialViewResult.Model);

            await _workGroupEmployeeService.Received(1).GetWorkGroupEmployeeAsync(
                Arg.Is<QueryParameters<string>>(q => q.SortBy == apiSortBy && q.Descending),
                string.Empty);

            Assert.Equal(uiSortBy, model.Pagination.SortColumn);
            Assert.True(model.Pagination.SortDirection);
        }

        [Fact]
        public async Task Create_Get_ReturnsPartialView_WithLookupOptionsPopulated()
        {
            var employeeResponse = ApiResponseDto<List<EmployeeDto>>.SuccessResponse(
                new List<EmployeeDto>
                {
                    new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Brown" }
                });
            var gradeResponse = ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(
                new List<WorkgroupGradeDto>
                {
                    new() { WgGrade = "A_BAC1", ProfitCentreGrade = "A-Bact", GradeCode = "A", Workgroup = "BAC1" }
                });

            _employeeService.GetFilteredEmployeesAsync(Arg.Any<QueryParameters<string>>(), 0).Returns(employeeResponse);
            _workGroupGradeService.GetAllWorkgroupGradesPagedAsync(Arg.Any<QueryParameters<string>>()).Returns(gradeResponse);

            var result = await _controller.Create();

            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("~/Areas/FPS/Views/WorkGroupStaffMaintenance/_AddEditWorkGroupStaff.cshtml", partialView.ViewName);
            var model = Assert.IsType<WorkGroupEmployeeItem>(partialView.Model);
            Assert.Contains("A_BAC1", model.WgGradeOptions);
            Assert.Contains(model.StaffLookupOptions, s => s.SpNumber == "SP001");
            var isEditMode = Assert.IsType<bool>(_controller.ViewData["IsEditMode"]);
            Assert.False(isEditMode);
        }

        [Fact]
        public async Task Create_Post_WithValidModel_ReturnsSuccessJson()
        {
            var dto = new WorkGroupEmployeeDto
            {
                PactId = "P001",
                SpNumber = "SP001",
                WorkGroupGrade = "WG-A",
                Name = "Alice",
                PersonStatus = "A",
                HrsPaid = 37,
                Leave = 0,
                SickSpecial = 0,
                HrsAvail = 37,
                MakeAvailable = 1,
                TimeRecorder = 0
            };

            _workGroupEmployeeService.CreateWorkGroupEmployeeAsync(dto)
                .Returns(ApiResponseDto<WorkGroupEmployeeDto>.SuccessResponse(dto));

            var result = await _controller.Create(dto);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value!.success);
            Assert.Equal("WG Staff record created successfully", value.message);
        }

        [Fact]
        public async Task Edit_Get_WhenServiceReturnsNoData_ReturnsNotFound()
        {
            _workGroupEmployeeService.GetWorkGroupEmployeeByIdAsync("P404")
                .Returns(ApiResponseDto<WorkGroupEmployeeDto>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.Edit("P404");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_WithValidModel_ReturnsSuccessJson()
        {
            var dto = new WorkGroupEmployeeDto
            {
                PactId = "P001",
                SpNumber = "SP001",
                WorkGroupGrade = "WG-A",
                Name = "Alice",
                PersonStatus = "A",
                HrsPaid = 37,
                Leave = 0,
                SickSpecial = 0,
                HrsAvail = 37,
                MakeAvailable = 1,
                TimeRecorder = 1
            };

            _workGroupEmployeeService.UpdateWorkGroupEmployeeAsync(dto)
                .Returns(ApiResponseDto<WorkGroupEmployeeDto>.SuccessResponse(dto));

            var result = await _controller.Edit(dto);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value!.success);
            Assert.Equal("WG Staff record updated successfully.", value.message);
        }

        [Fact]
        public async Task Delete_WhenServiceFails_ReturnsFailureJson()
        {
            _workGroupEmployeeService.DeleteWorkGroupEmployeeAsync("P001")
                .Returns(ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Code = "ERR", Message = "Delete failed" }],
                    new ApiMetaDto()));

            var result = await _controller.Delete("P001");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value!.success);
            Assert.Equal("Delete failed", value.message);
        }

        private class JsonResponse
        {
            public bool success { get; set; }
            public string? message { get; set; }
        }
    }
}
