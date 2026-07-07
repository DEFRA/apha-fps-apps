/*
 * TRANSFORMENGINE MIGRATION — SetUpStaffResourcesControllerTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-frontend  Phase 7 — Frontend XUnit Tests
 * Migrated : 2026-07-07
 *
 * CHANGED:
 *   - xUnit test class created for SetUpStaffResourcesController (Phase 5 output)
 *   - Mocks: IMapper, IWorkGroupEmployeeService, IProfitCentreService, IWorkGroupGradeService (NSubstitute)
 *   - Covers: Index, LoadStaffGrid, GetGradesByResourceCentre, Edit (GET + POST) actions
 *   - Naming convention: [MethodName]_[StateUnderTest]_[ExpectedResult]
 *   - Grouped by #region per action method
 *
 * PRESERVED:
 *   - All assertion patterns match existing ResourceSetUpControllerTests conventions
 *   - GetJsonResultElement helper preserved verbatim from project pattern
 *   - NSubstitute-style Received(1) delegation checks on each write path
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Expand coverage once _EditStaffModal partial is created in Phase 6
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NSubstitute;
using System.Text.Json;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.SetUpStaffResourcesControllerTest
{
    public class SetUpStaffResourcesControllerTests
    {
        private const string DefaultResourceCentre = "RC01";
        private const string DefaultWgGrade        = "WG01";
        private const string DefaultPactId         = "PACT001";

        private readonly IMapper _mapper;
        private readonly IWorkGroupEmployeeService _workGroupEmployeeService;
        private readonly IProfitCentreService _profitCentreService;
        private readonly IWorkGroupGradeService _workGroupGradeService;
        private readonly SetUpStaffResourcesController _controller;

        public SetUpStaffResourcesControllerTests()
        {
            _mapper                   = Substitute.For<IMapper>();
            _workGroupEmployeeService = Substitute.For<IWorkGroupEmployeeService>();
            _profitCentreService      = Substitute.For<IProfitCentreService>();
            _workGroupGradeService    = Substitute.For<IWorkGroupGradeService>();

            _controller = new SetUpStaffResourcesController(
                _mapper,
                _workGroupEmployeeService,
                _profitCentreService,
                _workGroupGradeService);
        }

        // TRANSFORMENGINE: Helper — serialise JsonResult.Value → JsonElement for property assertions
        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private static List<ProfitCentreDto> BuildProfitCentreList() =>
        [
            new() { ProfitCentreId = "RC01", ProfitCentreName = "Resource Centre One" },
            new() { ProfitCentreId = "RC02", ProfitCentreName = "Resource Centre Two" }
        ];

        private static List<WorkgroupGradeDto> BuildGradeList() =>
        [
            new() { WgGrade = "WG01", Workgroup = "WorkGroup A", ProfitCentreGrade = "G001", GradeCode = "GC01" },
            new() { WgGrade = "WG02", Workgroup = "WorkGroup B", ProfitCentreGrade = "G002", GradeCode = "GC02" }
        ];

        private static List<WorkGroupEmployeeStaffDto> BuildStaffList() =>
        [
            new()
            {
                PactId         = DefaultPactId,
                SpNumber       = "SP001",
                WorkGroupGrade = DefaultWgGrade,
                Name           = "John Smith",
                PersonStatus   = "A",
                HrsPaid        = 37.0,
                Leave          = 0.0,
                SickSpecial    = 0.0,
                HrsAvail       = 37.0,
                MakeAvailable  = 1
            }
        ];

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SetUpStaffResourcesController(
                null!, _workGroupEmployeeService, _profitCentreService, _workGroupGradeService));
        }

        [Fact]
        public void Constructor_WithNullWorkGroupEmployeeService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SetUpStaffResourcesController(
                _mapper, null!, _profitCentreService, _workGroupGradeService));
        }

        [Fact]
        public void Constructor_WithNullProfitCentreService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SetUpStaffResourcesController(
                _mapper, _workGroupEmployeeService, null!, _workGroupGradeService));
        }

        [Fact]
        public void Constructor_WithNullWorkGroupGradeService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SetUpStaffResourcesController(
                _mapper, _workGroupEmployeeService, _profitCentreService, null!));
        }

        #endregion

        #region Index Tests

        [Fact]
        public async Task Index_WithNoResourceCentre_ReturnsViewWithEmptyGradeListAndEmptyGrid()
        {
            // Arrange
            var profitCentres = BuildProfitCentreList();
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(profitCentres));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<SetUpStaffResourcesViewModel>(viewResult.Model);

            Assert.Equal(string.Empty,       model.SelectedResourceCentre);
            Assert.Equal(2,                  model.ResourceCentres.Count);
            Assert.Empty(model.GradeList);
            Assert.NotNull(model.StaffGrid);
            Assert.Empty(model.StaffGrid.Data);
            await _workGroupGradeService.DidNotReceive().GetWorkGroupGradeAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task Index_WithValidResourceCentre_LoadsGradeListAndReturnsView()
        {
            // Arrange
            var profitCentres = BuildProfitCentreList();
            var grades        = BuildGradeList();

            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(profitCentres));
            _workGroupGradeService.GetWorkGroupGradeAsync(DefaultResourceCentre)
                .Returns(ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(grades));

            // Act
            var result = await _controller.Index(DefaultResourceCentre);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<SetUpStaffResourcesViewModel>(viewResult.Model);

            Assert.Equal(DefaultResourceCentre, model.SelectedResourceCentre);
            Assert.Equal(2,                     model.GradeList.Count);
            Assert.Contains("WG01",             model.GradeList);
            await _workGroupGradeService.Received(1).GetWorkGroupGradeAsync(DefaultResourceCentre);
        }

        [Fact]
        public async Task Index_WhenGetProfitCentresFails_UsesEmptyResourceCentreList()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } };
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<SetUpStaffResourcesViewModel>(viewResult.Model);
            Assert.Empty(model.ResourceCentres);
        }

        [Fact]
        public async Task Index_WhenGradeServiceFails_GradeListIsEmpty()
        {
            // Arrange
            var profitCentres = BuildProfitCentreList();
            var errors        = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } };

            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(profitCentres));
            _workGroupGradeService.GetWorkGroupGradeAsync(DefaultResourceCentre)
                .Returns(ApiResponseDto<List<WorkgroupGradeDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Index(DefaultResourceCentre);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<SetUpStaffResourcesViewModel>(viewResult.Model);
            Assert.Empty(model.GradeList);
        }

        [Fact]
        public async Task Index_ResourceCentreListItems_HaveCorrectValueAndTextFormat()
        {
            // Arrange
            var profitCentres = BuildProfitCentreList();
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(profitCentres));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult  = Assert.IsType<ViewResult>(result);
            var model       = Assert.IsType<SetUpStaffResourcesViewModel>(viewResult.Model);
            var firstItem   = model.ResourceCentres[0];

            Assert.Equal("RC01",                              firstItem.Value);
            Assert.Equal("RC01 - Resource Centre One",        firstItem.Text);
        }

        [Fact]
        public async Task Index_StaffGrid_HasCorrectBindUrlAndEditOnlyConfiguration()
        {
            // Arrange
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(BuildProfitCentreList()));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<SetUpStaffResourcesViewModel>(viewResult.Model);

            Assert.Equal("/FPS/SetUpStaffResources/LoadStaffGrid", model.StaffGrid.BindGridUrl);
            Assert.True(model.StaffGrid.AllowEdit);
            Assert.False(model.StaffGrid.AllowAdd);
            Assert.False(model.StaffGrid.AllowDelete);
        }

        #endregion

        #region LoadStaffGrid Tests

        [Fact]
        public async Task LoadStaffGrid_WithInvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("wgGrade", "Required");

            // Act
            var result = await _controller.LoadStaffGrid(new PaginationFilter<string>(), DefaultWgGrade);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadStaffGrid_WithEmptyWgGrade_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.LoadStaffGrid(new PaginationFilter<string>(), "");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadStaffGrid_WithValidRequest_ReturnsPartialViewWithDataGridConfig()
        {
            // Arrange
            var request    = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var staffDtos  = BuildStaffList();
            var staffItems = new List<SetUpStaffResourcesItem>
            {
                new() { PactId = DefaultPactId, SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade }
            };
            var apiResponse     = ApiResponseDto<List<WorkGroupEmployeeStaffDto>>.SuccessResponse(staffDtos);
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _workGroupEmployeeService.GetWorkGroupEmployeeForStaffAsync(queryParameters, DefaultWgGrade)
                .Returns(apiResponse);
            _mapper.Map<SetUpStaffResourcesItem>(Arg.Any<WorkGroupEmployeeStaffDto>())
                .Returns(staffItems[0]);
            _mapper.Map<PaginationModel>(Arg.Any<object>())
                .Returns(new PaginationModel { TotalRecords = 1, PageNumber = 1, PageSize = 10 });

            // Act
            var result = await _controller.LoadStaffGrid(request, DefaultWgGrade);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig    = Assert.IsType<DataGridConfig<SetUpStaffResourcesItem>>(partialResult.Model);
            Assert.Single(gridConfig.Data);
            await _workGroupEmployeeService.Received(1)
                .GetWorkGroupEmployeeForStaffAsync(queryParameters, DefaultWgGrade);
        }

        [Fact]
        public async Task LoadStaffGrid_ServiceReturnsEmptyList_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request         = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse     = ApiResponseDto<List<WorkGroupEmployeeStaffDto>>.SuccessResponse(new List<WorkGroupEmployeeStaffDto>());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _workGroupEmployeeService.GetWorkGroupEmployeeForStaffAsync(queryParameters, DefaultWgGrade)
                .Returns(apiResponse);
            _mapper.Map<PaginationModel>(Arg.Any<object>())
                .Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadStaffGrid(request, DefaultWgGrade);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig    = Assert.IsType<DataGridConfig<SetUpStaffResourcesItem>>(partialResult.Model);
            Assert.Empty(gridConfig.Data);
        }

        [Fact]
        public async Task LoadStaffGrid_WhenServiceFails_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var request         = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors          = new List<ApiErrorDto> { new() { Message = "Load failed", Code = "ERR" } };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _workGroupEmployeeService.GetWorkGroupEmployeeForStaffAsync(queryParameters, DefaultWgGrade)
                .Returns(ApiResponseDto<List<WorkGroupEmployeeStaffDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.LoadStaffGrid(request, DefaultWgGrade);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region GetGradesByResourceCentre Tests

        [Fact]
        public async Task GetGradesByResourceCentre_WithValidResourceCentre_ReturnsJsonWithGradeList()
        {
            // Arrange
            var grades = BuildGradeList();
            _workGroupGradeService.GetWorkGroupGradeAsync(DefaultResourceCentre)
                .Returns(ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(grades));

            // Act
            var result = await _controller.GetGradesByResourceCentre(DefaultResourceCentre);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            await _workGroupGradeService.Received(1).GetWorkGroupGradeAsync(DefaultResourceCentre);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetGradesByResourceCentre_WithEmptyOrWhitespaceResourceCentre_ReturnsJsonWithSuccessFalse(string rc)
        {
            // Act
            var result = await _controller.GetGradesByResourceCentre(rc);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            await _workGroupGradeService.DidNotReceive().GetWorkGroupGradeAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetGradesByResourceCentre_WhenServiceFails_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Grade load failed", Code = "ERR" } };
            _workGroupGradeService.GetWorkGroupGradeAsync(DefaultResourceCentre)
                .Returns(ApiResponseDto<List<WorkgroupGradeDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.GetGradesByResourceCentre(DefaultResourceCentre);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetGradesByResourceCentre_ServiceReturnsEmptyList_ReturnsJsonWithSuccessTrueAndEmptyData()
        {
            // Arrange
            _workGroupGradeService.GetWorkGroupGradeAsync(DefaultResourceCentre)
                .Returns(ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(new List<WorkgroupGradeDto>()));

            // Act
            var result = await _controller.GetGradesByResourceCentre(DefaultResourceCentre);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region Edit GET Tests

        [Fact]
        public async Task Edit_Get_WithValidPactId_ReturnsPartialViewWithStaffItem()
        {
            // Arrange
            var staffDto = new WorkGroupEmployeeStaffDto
            {
                PactId         = DefaultPactId,
                SpNumber       = "SP001",
                WorkGroupGrade = DefaultWgGrade,
                Name           = "John Smith"
            };
            var staffItem = new SetUpStaffResourcesItem
            {
                PactId         = DefaultPactId,
                SpNumber       = "SP001",
                WorkGroupGrade = DefaultWgGrade,
                Name           = "John Smith"
            };

            _workGroupEmployeeService.GetWorkGroupEmployeeByIdForStaffAsync(DefaultPactId)
                .Returns(ApiResponseDto<WorkGroupEmployeeStaffDto>.SuccessResponse(staffDto));
            _mapper.Map<SetUpStaffResourcesItem>(staffDto).Returns(staffItem);

            // Act
            var result = await _controller.Edit(DefaultPactId);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var model         = Assert.IsType<SetUpStaffResourcesItem>(partialResult.Model);
            Assert.Equal(DefaultPactId, model.PactId);
            Assert.Equal("John Smith",  model.Name);
            await _workGroupEmployeeService.Received(1).GetWorkGroupEmployeeByIdForStaffAsync(DefaultPactId);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Edit_Get_WithEmptyOrWhitespacePactId_ReturnsJsonWithSuccessFalse(string pactId)
        {
            // Act
            var result = await _controller.Edit(pactId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            await _workGroupEmployeeService.DidNotReceive().GetWorkGroupEmployeeByIdForStaffAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task Edit_Get_WhenServiceReturnsFailure_ReturnsNotFound()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "ERR" } };
            _workGroupEmployeeService.GetWorkGroupEmployeeByIdForStaffAsync(DefaultPactId)
                .Returns(ApiResponseDto<WorkGroupEmployeeStaffDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Edit(DefaultPactId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_WhenServiceReturnsNullData_ReturnsNotFound()
        {
            // Arrange
            _workGroupEmployeeService.GetWorkGroupEmployeeByIdForStaffAsync(DefaultPactId)
                .Returns(ApiResponseDto<WorkGroupEmployeeStaffDto>.SuccessResponse(null!));

            // Act
            var result = await _controller.Edit(DefaultPactId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region Edit POST Tests

        [Fact]
        public async Task Edit_Post_WithValidDto_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var model      = new WorkGroupEmployeeStaffDto { PactId = DefaultPactId, WorkGroupGrade = DefaultWgGrade };
            var updatedDto = new WorkGroupEmployeeStaffDto { PactId = DefaultPactId };

            _workGroupEmployeeService.UpdateWorkGroupEmployeeForStaffAsync(model)
                .Returns(ApiResponseDto<WorkGroupEmployeeStaffDto>.SuccessResponse(updatedDto));

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            await _workGroupEmployeeService.Received(1).UpdateWorkGroupEmployeeForStaffAsync(model);
        }

        [Fact]
        public async Task Edit_Post_WithNullDto_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.Edit((WorkGroupEmployeeStaffDto)null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Edit_Post_WithInvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var model = new WorkGroupEmployeeStaffDto { PactId = DefaultPactId };
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Edit_Post_WhenServiceFails_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var model  = new WorkGroupEmployeeStaffDto { PactId = DefaultPactId };
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "ERR" } };

            _workGroupEmployeeService.UpdateWorkGroupEmployeeForStaffAsync(model)
                .Returns(ApiResponseDto<WorkGroupEmployeeStaffDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion
    }
}
