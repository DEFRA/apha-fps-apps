// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — MaintWGStaffControllerTests.cs (new file)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-06-11
 *
 * CHANGED:
 *   - New file — no prior test coverage for MaintWGStaffController.
 *   - Covers all public actions of MaintWGStaffController created in Phase 11:
 *       Index (GET)                    — builds MaintWGStaffViewModel with populated WGStaffGrid
 *       LoadWGStaffGrid (POST)         — DataGrid AJAX reload endpoint
 *       Create (GET)                   — returns _AddEditMaintWGStaff partial with empty item
 *       Create (POST, WorkGroupEmployeeDto) — creates record, returns JSON
 *       Edit   (GET, pactId)           — loads record, returns _AddEditMaintWGStaff partial
 *       Edit   (POST, WorkGroupEmployeeDto) — updates record, returns JSON
 *       Delete (DELETE, pactId)        — deletes record, returns JSON
 *   - Uses NSubstitute for IMapper and IWorkGroupEmployeeService mocks.
 *   - Follows [MethodName]_[StateUnderTest]_[ExpectedResult] naming convention.
 *   - Mirrors folder and namespace conventions of existing FPS Web.UnitTests files.
 *
 * PRESERVED:
 *   - No production code altered by this file.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Index and LoadWGStaffGrid tests verify that the grid's GridId is
 *     "wgStaffGrid". Confirm this matches GetWGStaffGridConfigAsync in MaintWGStaffController.cs.
 *   - TRANSFORMENGINE TODO: wgGrade passed as string.Empty to the service in the controller.
 *     Tests verify service.GetWorkGroupEmployeeAsync is called with string.Empty. If backend
 *     changes this contract, update test stubs accordingly.
 *   - TRANSFORMENGINE TODO: Create/Edit POST tests accept WorkGroupEmployeeDto directly from body
 *     (no mapper call between item and dto). Confirm controller signature does not use
 *     WorkGroupEmployeeItem as the binding model.
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
using NSubstitute;
using System.Text.Json;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.MaintWGStaffControllerTest
{
    public class MaintWGStaffControllerTests
    {
        private const string DefaultPactId  = "PACT001";
        private const string DefaultWgGrade = "WG01";

        private readonly IMapper _mapper;
        private readonly IWorkGroupEmployeeService _service;
        private readonly MaintWGStaffController _controller;

        public MaintWGStaffControllerTests()
        {
            _mapper     = Substitute.For<IMapper>();
            _service    = Substitute.For<IWorkGroupEmployeeService>();
            _controller = new MaintWGStaffController(_mapper, _service);
        }

        // Helper: serialize and deserialize JsonResult.Value for property inspection
        private static T? GetJsonValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json);
        }

        // Local record used to inspect anonymous JSON response objects
        private record JsonResponse(bool success, string? message, object? errors);

        // ─────────────────────────────────────────────────────────────────────────
        #region Index Tests
        // ─────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task Index_ServiceReturnsData_ReturnsViewResultWithPopulatedModel()
        {
            // Arrange
            var employees = new List<WorkGroupEmployeeDto>
            {
                new() { PactId = DefaultPactId, WorkGroupGrade = DefaultWgGrade }
            };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 15, TotalRecords = 1 };
            var apiResponse = ApiResponseDto<List<WorkGroupEmployeeDto>>.SuccessResponse(employees, pagination);

            _service.GetWorkGroupEmployeeAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<WorkGroupEmployeeItem>>(Arg.Any<List<WorkGroupEmployeeDto>>())
                .Returns(new List<WorkGroupEmployeeItem> { new WorkGroupEmployeeItem { PactId = DefaultPactId } });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel { PageNumber = 1, PageSize = 15, TotalRecords = 1 });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<MaintWGStaffViewModel>(viewResult.Model);
            Assert.NotNull(model.WGStaffGrid);
            Assert.Equal("wgStaffGrid", model.WGStaffGrid.GridId);
        }

        [Fact]
        public async Task Index_CallsServiceWithEmptyWgGrade_AsAllGradesConvention()
        {
            // Arrange — backend wgGrade="" is the controller's "all grades" convention (see DEFERRED note)
            var apiResponse = ApiResponseDto<List<WorkGroupEmployeeDto>>.SuccessResponse(
                new List<WorkGroupEmployeeDto>(), new PaginationDto());

            _service.GetWorkGroupEmployeeAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<WorkGroupEmployeeItem>>(Arg.Any<List<WorkGroupEmployeeDto>>())
                .Returns(new List<WorkGroupEmployeeItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            await _controller.Index();

            // Assert — wgGrade is string.Empty; service must be called exactly once
            await _service.Received(1).GetWorkGroupEmployeeAsync(
                Arg.Any<QueryParameters<string>>(), string.Empty);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region LoadWGStaffGrid Tests
        // ─────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task LoadWGStaffGrid_ServiceReturnsData_ReturnsPartialViewWithDataGridConfig()
        {
            // Arrange
            var request     = new PaginationFilter<string> { Filter = "{}" };
            var employees   = new List<WorkGroupEmployeeDto>
            {
                new() { PactId = DefaultPactId, WorkGroupGrade = DefaultWgGrade }
            };
            var pagination   = new PaginationDto { PageNumber = 1, PageSize = 15, TotalRecords = 1 };
            var apiResponse  = ApiResponseDto<List<WorkGroupEmployeeDto>>.SuccessResponse(employees, pagination);

            _service.GetWorkGroupEmployeeAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<WorkGroupEmployeeItem>>(Arg.Any<List<WorkGroupEmployeeDto>>())
                .Returns(new List<WorkGroupEmployeeItem> { new WorkGroupEmployeeItem { PactId = DefaultPactId } });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadWGStaffGrid(request);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var grid = Assert.IsType<DataGridConfig<WorkGroupEmployeeItem>>(partialView.Model);
            Assert.NotEmpty(grid.Data);
        }

        [Fact]
        public async Task LoadWGStaffGrid_ServiceReturnsEmptyPage_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request     = new PaginationFilter<string> { Filter = "{}" };
            var apiResponse = ApiResponseDto<List<WorkGroupEmployeeDto>>.SuccessResponse(
                new List<WorkGroupEmployeeDto>(), new PaginationDto());

            _service.GetWorkGroupEmployeeAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<WorkGroupEmployeeItem>>(Arg.Any<List<WorkGroupEmployeeDto>>())
                .Returns(new List<WorkGroupEmployeeItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadWGStaffGrid(request);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var grid        = Assert.IsType<DataGridConfig<WorkGroupEmployeeItem>>(partialView.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadWGStaffGrid_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _controller.ModelState.AddModelError("Filter", "Filter is invalid");

            // Act
            var result = await _controller.LoadWGStaffGrid(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Invalid request data", value.message);
        }

        [Fact]
        public async Task LoadWGStaffGrid_ConfiguresGridWithCorrectProperties()
        {
            // Arrange
            var request     = new PaginationFilter<string>
            {
                Filter    = "{}",
                SortBy    = "PactId",
                Descending = false,
                PageSize  = 20
            };
            var apiResponse = ApiResponseDto<List<WorkGroupEmployeeDto>>.SuccessResponse(
                new List<WorkGroupEmployeeDto>(), new PaginationDto());

            _service.GetWorkGroupEmployeeAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<WorkGroupEmployeeItem>>(Arg.Any<List<WorkGroupEmployeeDto>>())
                .Returns(new List<WorkGroupEmployeeItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadWGStaffGrid(request);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var grid        = Assert.IsType<DataGridConfig<WorkGroupEmployeeItem>>(partialView.Model);

            Assert.Equal("wgStaffGrid",                          grid.GridId);
            Assert.Equal("WG Staff",                             grid.Title);
            Assert.Equal("PactId",                               grid.KeyProperty);
            Assert.Equal("addMaintWGStaff",                      grid.AddFunction);
            Assert.Equal("editMaintWGStaff",                     grid.EditFunction);
            Assert.Equal("deleteMaintWGStaff",                   grid.DeleteFunction);
            Assert.Equal("getMaintWGStaffExtraFilters",          grid.ExtraFilterMethod);
            Assert.Equal("/FPS/MaintWGStaff/LoadWGStaffGrid",   grid.BindGridUrl);
            Assert.True(grid.AllowAdd);
            Assert.True(grid.AllowEdit);
            Assert.True(grid.AllowDelete);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Create Tests
        // ─────────────────────────────────────────────────────────────────────────

        [Fact]
        public void Create_Get_ReturnsPartialViewWithEmptyWorkGroupEmployeeItem()
        {
            // Act
            var result = _controller.Create();

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditMaintWGStaff", partialView.ViewName);
            Assert.IsType<WorkGroupEmployeeItem>(partialView.Model);
        }

        [Fact]
        public async Task Create_Post_ValidDto_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var dto         = new WorkGroupEmployeeDto { PactId = DefaultPactId, WorkGroupGrade = DefaultWgGrade };
            var created     = new WorkGroupEmployeeDto { PactId = DefaultPactId };
            var apiResponse = ApiResponseDto<WorkGroupEmployeeDto>.SuccessResponse(created);

            _service.CreateWorkGroupEmployeeAsync(dto).Returns(apiResponse);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            await _service.Received(1).CreateWorkGroupEmployeeAsync(dto);
        }

        [Fact]
        public async Task Create_Post_NullDto_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.Create(null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            await _service.DidNotReceive().CreateWorkGroupEmployeeAsync(Arg.Any<WorkGroupEmployeeDto>());
        }

        [Fact]
        public async Task Create_Post_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var dto = new WorkGroupEmployeeDto { PactId = DefaultPactId };
            _controller.ModelState.AddModelError("WorkGroupGrade", "WG Grade is required");

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Please correct the errors below.", value.message);
            await _service.DidNotReceive().CreateWorkGroupEmployeeAsync(Arg.Any<WorkGroupEmployeeDto>());
        }

        [Fact]
        public async Task Create_Post_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var dto    = new WorkGroupEmployeeDto { PactId = DefaultPactId, WorkGroupGrade = DefaultWgGrade };
            var errors = new List<ApiErrorDto> { new() { Message = "Duplicate PactId", Code = "DUPLICATE" } };
            var apiResponse = ApiResponseDto<WorkGroupEmployeeDto>.FailureResponse(errors, new ApiMetaDto());

            _service.CreateWorkGroupEmployeeAsync(dto).Returns(apiResponse);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Edit Tests
        // ─────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task Edit_Get_ValidPactId_ServiceReturnsData_ReturnsPartialViewWithItem()
        {
            // Arrange
            var dto         = new WorkGroupEmployeeDto { PactId = DefaultPactId, WorkGroupGrade = DefaultWgGrade };
            var item        = new WorkGroupEmployeeItem { PactId = DefaultPactId };
            var apiResponse = ApiResponseDto<WorkGroupEmployeeDto>.SuccessResponse(dto);

            _service.GetWorkGroupEmployeeByIdAsync(DefaultPactId).Returns(apiResponse);
            _mapper.Map<WorkGroupEmployeeItem>(dto).Returns(item);

            // Act
            var result = await _controller.Edit(DefaultPactId);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditMaintWGStaff", partialView.ViewName);
            var model = Assert.IsType<WorkGroupEmployeeItem>(partialView.Model);
            Assert.Equal(DefaultPactId, model.PactId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Edit_Get_NullOrWhitespacePactId_ReturnsJsonWithSuccessFalse(string? pactId)
        {
            // Act
            var result = await _controller.Edit(pactId!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            await _service.DidNotReceive().GetWorkGroupEmployeeByIdAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task Edit_Get_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = ApiResponseDto<WorkGroupEmployeeDto>.FailureResponse(errors, new ApiMetaDto());

            _service.GetWorkGroupEmployeeByIdAsync(DefaultPactId).Returns(apiResponse);

            // Act
            var result = await _controller.Edit(DefaultPactId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task Edit_Post_ValidDto_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var dto         = new WorkGroupEmployeeDto { PactId = DefaultPactId, WorkGroupGrade = DefaultWgGrade };
            var updated     = new WorkGroupEmployeeDto { PactId = DefaultPactId };
            var apiResponse = ApiResponseDto<WorkGroupEmployeeDto>.SuccessResponse(updated);

            _service.UpdateWorkGroupEmployeeAsync(dto).Returns(apiResponse);

            // Act
            var result = await _controller.Edit(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            await _service.Received(1).UpdateWorkGroupEmployeeAsync(dto);
        }

        [Fact]
        public async Task Edit_Post_NullDto_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.Edit((WorkGroupEmployeeDto)null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            await _service.DidNotReceive().UpdateWorkGroupEmployeeAsync(Arg.Any<WorkGroupEmployeeDto>());
        }

        [Fact]
        public async Task Edit_Post_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var dto = new WorkGroupEmployeeDto { PactId = DefaultPactId };
            _controller.ModelState.AddModelError("WorkGroupGrade", "WG Grade is required");

            // Act
            var result = await _controller.Edit(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Please correct the errors below.", value.message);
            await _service.DidNotReceive().UpdateWorkGroupEmployeeAsync(Arg.Any<WorkGroupEmployeeDto>());
        }

        [Fact]
        public async Task Edit_Post_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var dto    = new WorkGroupEmployeeDto { PactId = DefaultPactId, WorkGroupGrade = DefaultWgGrade };
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } };
            var apiResponse = ApiResponseDto<WorkGroupEmployeeDto>.FailureResponse(errors, new ApiMetaDto());

            _service.UpdateWorkGroupEmployeeAsync(dto).Returns(apiResponse);

            // Act
            var result = await _controller.Edit(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Delete Tests
        // ─────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task Delete_ValidPactId_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _service.DeleteWorkGroupEmployeeAsync(DefaultPactId).Returns(apiResponse);

            // Act
            var result = await _controller.Delete(DefaultPactId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            await _service.Received(1).DeleteWorkGroupEmployeeAsync(DefaultPactId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Delete_NullOrWhitespacePactId_ReturnsJsonWithSuccessFalse(string? pactId)
        {
            // Act
            var result = await _controller.Delete(pactId!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            await _service.DidNotReceive().DeleteWorkGroupEmployeeAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task Delete_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Delete failed", Code = "DELETE_ERROR" } };
            var apiResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _service.DeleteWorkGroupEmployeeAsync(DefaultPactId).Returns(apiResponse);

            // Act
            var result = await _controller.Delete(DefaultPactId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        #endregion
    }
}
