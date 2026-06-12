/*
 * TRANSFORMENGINE MIGRATION — ResourceSetUpControllerTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 15 — Build, Fix, and Final Validation
 * Migrated : 2026-06-11
 *
 * CHANGED:
 *   - Line 383: model.Name → model.StaffName — WorkGroupEmployeeItem.Name was renamed to StaffName
 *     in Phase 11 to align with JS DataGrid field key 'staffName'; this test was not updated at
 *     that time. Fixed during Phase 15 build repair.
 * PRESERVED:
 *   - All other test logic, assertions, mock setup, and test method signatures unchanged.
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ResourceSetUpControllerTest
{
    public class ResourceSetUpControllerTests
    {
        private const string DefaultProfitCentre = "PC01";
        private const string DefaultPcGrade      = "G001";
        private const string DefaultWgGrade      = "WG01";
        private const string DefaultPactId       = "PACT001";

        private readonly IMapper _mapper;
        private readonly IProfitCentreService _profitCentreService;
        private readonly IProfitCentreGradeService _rcGradeService;
        private readonly IWorkGroupGradeService _wgGradeService;
        private readonly IWorkGroupEmployeeService _wgEmployeeService;
        private readonly ResourceSetUpController _controller;

        public ResourceSetUpControllerTests()
        {
            _mapper              = Substitute.For<IMapper>();
            _profitCentreService = Substitute.For<IProfitCentreService>();
            _rcGradeService      = Substitute.For<IProfitCentreGradeService>();
            _wgGradeService      = Substitute.For<IWorkGroupGradeService>();
            _wgEmployeeService   = Substitute.For<IWorkGroupEmployeeService>();

            _controller = new ResourceSetUpController(
                _mapper,
                _profitCentreService,
                _rcGradeService,
                _wgGradeService,
                _wgEmployeeService);
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private static List<ProfitCentreDto> BuildProfitCentreList() =>
        [
            new() { ProfitCentreId = "PC01", ProfitCentreName = "Profit Centre One" },
            new() { ProfitCentreId = "PC02", ProfitCentreName = "Profit Centre Two" }
        ];

        private static List<ProfitCentreGradeDto> BuildRcGradeList() =>
        [
            new() { PcGrade = "G001", ProfitCentre = DefaultProfitCentre, ChargeRate = 100m },
            new() { PcGrade = "G002", ProfitCentre = DefaultProfitCentre, ChargeRate = 200m }
        ];

        #region Index Tests

        [Fact]
        public async Task Index_WithNoProfitCentre_ReturnsViewWithEmptyProfitCentreAndEmptyGrid()
        {
            // Arrange
            var profitCentres = BuildProfitCentreList();
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(profitCentres));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<ResourceSetUpViewModel>(viewResult.Model);

            Assert.Equal(string.Empty, model.ProfitCentre);
            Assert.Equal(2,            model.ProfitCentreList.Count);
            Assert.NotNull(model.RcGradeGrid);
            Assert.NotNull(model.WgGradeGrid);
            Assert.NotNull(model.WgStaffGrid);
            await _rcGradeService.DidNotReceive().GetProfitCentreGradesAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task Index_WithValidProfitCentre_LoadsRcGradesAndReturnsView()
        {
            // Arrange
            var profitCentres = BuildProfitCentreList();
            var rcGrades      = BuildRcGradeList();

            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(profitCentres));
            _rcGradeService.GetProfitCentreGradesAsync(DefaultProfitCentre)
                .Returns(ApiResponseDto<List<ProfitCentreGradeDto>>.SuccessResponse(rcGrades));

            // Act
            var result = await _controller.Index(DefaultProfitCentre);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<ResourceSetUpViewModel>(viewResult.Model);

            Assert.Equal(DefaultProfitCentre, model.ProfitCentre);
            Assert.Equal(2,                   model.ProfitCentreList.Count);
            Assert.NotNull(model.RcGradeGrid);
            Assert.Equal("rcGradeGrid", model.RcGradeGrid.GridId);
            await _rcGradeService.Received(1).GetProfitCentreGradesAsync(DefaultProfitCentre);
        }

        [Fact]
        public async Task Index_WhenGetProfitCentresFails_UsesEmptyProfitCentreList()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } };
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<ResourceSetUpViewModel>(viewResult.Model);

            Assert.Empty(model.ProfitCentreList);
        }

        [Fact]
        public async Task Index_WhenRcGradeServiceFails_GridDataIsEmpty()
        {
            // Arrange
            var profitCentres = BuildProfitCentreList();
            var errors        = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } };

            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(profitCentres));
            _rcGradeService.GetProfitCentreGradesAsync(DefaultProfitCentre)
                .Returns(ApiResponseDto<List<ProfitCentreGradeDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Index(DefaultProfitCentre);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<ResourceSetUpViewModel>(viewResult.Model);

            Assert.Empty(model.RcGradeGrid.Data);
        }

        [Fact]
        public async Task Index_ProfitCentreListItems_HaveCorrectValueAndTextFormat()
        {
            // Arrange
            var profitCentres = BuildProfitCentreList();
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(profitCentres));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<ResourceSetUpViewModel>(viewResult.Model);
            var firstItem  = model.ProfitCentreList[0];

            Assert.Equal("PC01",                       firstItem.Value);
            Assert.Equal("PC01 - Profit Centre One",   firstItem.Text);
        }

        [Fact]
        public async Task Index_GridConfigs_HaveCorrectBindUrls()
        {
            // Arrange
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(BuildProfitCentreList()));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<ResourceSetUpViewModel>(viewResult.Model);

            Assert.Equal("/FPS/ResourceSetUp/LoadRcGradeGrid",  model.RcGradeGrid.BindGridUrl);
            Assert.Equal("/FPS/ResourceSetUp/LoadWgGradeGrid",  model.WgGradeGrid.BindGridUrl);
            Assert.Equal("/FPS/ResourceSetUp/LoadWgStaffGrid",  model.WgStaffGrid.BindGridUrl);
        }

        [Fact]
        public async Task Index_WgStaffGrid_AllowsEdit()
        {
            // Arrange
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(BuildProfitCentreList()));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<ResourceSetUpViewModel>(viewResult.Model);

            Assert.True(model.WgStaffGrid.AllowEdit);
            Assert.False(model.WgStaffGrid.AllowAdd);
            Assert.False(model.WgStaffGrid.AllowDelete);
        }

        #endregion

        #region LoadRcGradeGrid Tests

        [Fact]
        public async Task LoadRcGradeGrid_WithValidProfitCentre_ReturnsPartialView()
        {
            // Arrange
            var rcGrades = BuildRcGradeList();
            _rcGradeService.GetProfitCentreGradesAsync(DefaultProfitCentre)
                .Returns(ApiResponseDto<List<ProfitCentreGradeDto>>.SuccessResponse(rcGrades));

            // Act
            var result = await _controller.LoadRcGradeGrid(DefaultProfitCentre);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            await _rcGradeService.Received(1).GetProfitCentreGradesAsync(DefaultProfitCentre);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task LoadRcGradeGrid_WithEmptyOrWhitespaceProfitCentre_ReturnsFailureJson(string profitCentre)
        {
            // Act
            var result = await _controller.LoadRcGradeGrid(profitCentre);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultElement(jsonResult);

            Assert.False(value.GetProperty("success").GetBoolean());
            await _rcGradeService.DidNotReceive().GetProfitCentreGradesAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task LoadRcGradeGrid_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Load failed", Code = "ERR" } };
            _rcGradeService.GetProfitCentreGradesAsync(DefaultProfitCentre)
                .Returns(ApiResponseDto<List<ProfitCentreGradeDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.LoadRcGradeGrid(DefaultProfitCentre);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultElement(jsonResult);

            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadRcGradeGrid_WithFilterOnRcGradeDisplay_FiltersResults()
        {
            // Arrange
            var rcGrades = BuildRcGradeList();
            _rcGradeService.GetProfitCentreGradesAsync(DefaultProfitCentre)
                .Returns(ApiResponseDto<List<ProfitCentreGradeDto>>.SuccessResponse(rcGrades));

            // Act
            var result = await _controller.LoadRcGradeGrid(DefaultProfitCentre, filter: "{\"RcGradeDisplay\":\"G001\"}");

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig    = Assert.IsType<Apha.FPSApps.Web.Models.Components.DataGrid.DataGridConfig<ProfitCentreGradeItem>>(partialResult.Model);
            Assert.Single(gridConfig.Data);
        }

        [Fact]
        public async Task LoadRcGradeGrid_WithSortByChargeRateDescending_ReturnsSortedResults()
        {
            // Arrange
            var rcGrades = BuildRcGradeList();
            _rcGradeService.GetProfitCentreGradesAsync(DefaultProfitCentre)
                .Returns(ApiResponseDto<List<ProfitCentreGradeDto>>.SuccessResponse(rcGrades));

            // Act
            var result = await _controller.LoadRcGradeGrid(DefaultProfitCentre, sortBy: "chargerate", descending: true);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig    = Assert.IsType<Apha.FPSApps.Web.Models.Components.DataGrid.DataGridConfig<ProfitCentreGradeItem>>(partialResult.Model);
            Assert.Equal(200m, gridConfig.Data.First().ChargeRate);
        }

        #endregion

        #region LoadWgGradeGrid Tests

        [Fact]
        public async Task LoadWgGradeGrid_WithValidPcGrade_ReturnsPartialView()
        {
            // Arrange
            var wgGrades = new List<WorkgroupGradeDto>
            {
                new() { ProfitCentreGrade = DefaultPcGrade, WgGrade = DefaultWgGrade }
            };
            _wgGradeService.GetWorkGroupGradeAsync(DefaultPcGrade)
                .Returns(ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(wgGrades));

            // Act
            var result = await _controller.LoadWgGradeGrid(DefaultPcGrade);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            await _wgGradeService.Received(1).GetWorkGroupGradeAsync(DefaultPcGrade);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task LoadWgGradeGrid_WithEmptyOrWhitespacePcGrade_ReturnsFailureJson(string pcGrade)
        {
            // Act
            var result = await _controller.LoadWgGradeGrid(pcGrade);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultElement(jsonResult);

            Assert.False(value.GetProperty("success").GetBoolean());
            await _wgGradeService.DidNotReceive().GetWorkGroupGradeAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task LoadWgGradeGrid_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Load failed", Code = "ERR" } };
            _wgGradeService.GetWorkGroupGradeAsync(DefaultPcGrade)
                .Returns(ApiResponseDto<List<WorkgroupGradeDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.LoadWgGradeGrid(DefaultPcGrade);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultElement(jsonResult);

            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region EditWgStaff Tests

        [Fact]
        public async Task EditWgStaff_WithValidPactId_ReturnsPartialViewWithItem()
        {
            // Arrange
            var employeeDto = new WorkGroupEmployeeDto
            {
                PactId   = DefaultPactId,
                SpNumber = "SP001",
                Name     = "John Doe",
                HrsPaid  = 40.0
            };
            _wgEmployeeService.GetWorkGroupEmployeeByIdAsync(DefaultPactId)
                .Returns(ApiResponseDto<WorkGroupEmployeeDto>.SuccessResponse(employeeDto));

            // Act
            var result = await _controller.EditWgStaff(DefaultPactId);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var model         = Assert.IsType<WorkGroupEmployeeItem>(partialResult.Model);

            Assert.Equal(DefaultPactId, model.PactId);
            Assert.Equal("SP001",       model.SpNumber);
            Assert.Equal("John Doe",    model.StaffName);
            Assert.Equal(40.0,          model.HrsPaid);
            await _wgEmployeeService.Received(1).GetWorkGroupEmployeeByIdAsync(DefaultPactId);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task EditWgStaff_WithEmptyOrWhitespacePactId_ReturnsFailureJson(string pactId)
        {
            // Act
            var result = await _controller.EditWgStaff(pactId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultElement(jsonResult);

            Assert.False(value.GetProperty("success").GetBoolean());
            await _wgEmployeeService.DidNotReceive().GetWorkGroupEmployeeByIdAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task EditWgStaff_WhenEmployeeNotFound_ReturnsFailureJson()
        {
            // Arrange
            _wgEmployeeService.GetWorkGroupEmployeeByIdAsync(DefaultPactId)
                .Returns(ApiResponseDto<WorkGroupEmployeeDto>.SuccessResponse(null!));

            // Act
            var result = await _controller.EditWgStaff(DefaultPactId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultElement(jsonResult);

            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task EditWgStaff_MapsNegativeOneToTrue_ForMakeAvailable()
        {
            // Arrange
            var employeeDto = new WorkGroupEmployeeDto
            {
                PactId        = DefaultPactId,
                MakeAvailable = -1
            };
            _wgEmployeeService.GetWorkGroupEmployeeByIdAsync(DefaultPactId)
                .Returns(ApiResponseDto<WorkGroupEmployeeDto>.SuccessResponse(employeeDto));

            // Act
            var result = await _controller.EditWgStaff(DefaultPactId);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var model         = Assert.IsType<WorkGroupEmployeeItem>(partialResult.Model);

            Assert.True(model.MakeAvailable);
        }

        [Fact]
        public async Task EditWgStaff_MapsZeroToFalse_ForMakeAvailable()
        {
            // Arrange
            var employeeDto = new WorkGroupEmployeeDto
            {
                PactId        = DefaultPactId,
                MakeAvailable = 0
            };
            _wgEmployeeService.GetWorkGroupEmployeeByIdAsync(DefaultPactId)
                .Returns(ApiResponseDto<WorkGroupEmployeeDto>.SuccessResponse(employeeDto));

            // Act
            var result = await _controller.EditWgStaff(DefaultPactId);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var model         = Assert.IsType<WorkGroupEmployeeItem>(partialResult.Model);

            Assert.False(model.MakeAvailable);
        }

        #endregion

        #region UpdateWgStaff Tests

        [Fact]
        public async Task UpdateWgStaff_WithValidItem_ReturnsSuccessJson()
        {
            // Arrange
            var item = new WorkGroupEmployeeItem
            {
                PactId        = DefaultPactId,
                SpNumber      = "SP001",
                HrsPaid       = 40.0,
                MakeAvailable = true
            };
            var updatedDto = new WorkGroupEmployeeDto { PactId = DefaultPactId };
            _wgEmployeeService.UpdateWorkGroupEmployeeAsync(Arg.Any<WorkGroupEmployeeDto>())
                .Returns(ApiResponseDto<WorkGroupEmployeeDto>.SuccessResponse(updatedDto));

            // Act
            var result = await _controller.UpdateWgStaff(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultElement(jsonResult);

            Assert.True(value.GetProperty("success").GetBoolean());
            await _wgEmployeeService.Received(1).UpdateWorkGroupEmployeeAsync(Arg.Any<WorkGroupEmployeeDto>());
        }

        [Fact]
        public async Task UpdateWgStaff_MapsTrue_ToNegativeOne_ForMakeAvailable()
        {
            // Arrange
            var item = new WorkGroupEmployeeItem { PactId = DefaultPactId, MakeAvailable = true };
            var updatedDto = new WorkGroupEmployeeDto { PactId = DefaultPactId };

            _wgEmployeeService.UpdateWorkGroupEmployeeAsync(Arg.Is<WorkGroupEmployeeDto>(d => d.MakeAvailable == -1))
                .Returns(ApiResponseDto<WorkGroupEmployeeDto>.SuccessResponse(updatedDto));

            // Act
            await _controller.UpdateWgStaff(item);

            // Assert
            await _wgEmployeeService.Received(1)
                .UpdateWorkGroupEmployeeAsync(Arg.Is<WorkGroupEmployeeDto>(d => d.MakeAvailable == -1));
        }

        [Fact]
        public async Task UpdateWgStaff_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var item   = new WorkGroupEmployeeItem { PactId = DefaultPactId };
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "ERR" } };

            _wgEmployeeService.UpdateWorkGroupEmployeeAsync(Arg.Any<WorkGroupEmployeeDto>())
                .Returns(ApiResponseDto<WorkGroupEmployeeDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.UpdateWgStaff(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultElement(jsonResult);

            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion
    }
}
