using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.Controllers.MasterLookupControllerTest
{
    public class MasterLookupControllerTests
    {
        private const string TableName = "directorate";
        private readonly IMasterLookupService _service;
        private readonly MasterLookupController _controller;

        public MasterLookupControllerTests()
        {
            _service = Substitute.For<IMasterLookupService>();
            _controller = new MasterLookupController(_service);
        }

        // ── helpers ───────────────────────────────────────────────────────────────

        private static JsonElement GetJsonElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private static ApiResponseDto<T> SuccessResponse<T>(T data) =>
            ApiResponseDto<T>.SuccessResponse(data);

        private static ApiResponseDto<T> FailureResponse<T>(string message = "Error", string code = "ERR") =>
            ApiResponseDto<T>.FailureResponse(
                new List<ApiErrorDto> { new ApiErrorDto { Code = code, Message = message } },
                new ApiMetaDto());

        private static PaginatedResult<LookupItemDto> PagedItems(params string[] values) =>
            new(values.Select(v => new LookupItemDto { Value = v }).ToList(), values.Length, 1, 10);

        private void SetupTableNames(params string[] names)
        {
            _service.GetAllMasterLookupsAsync()
                .Returns(SuccessResponse<IEnumerable<MasterLookupDto>>(
                    names.Select(n => new MasterLookupDto { MasterTableName = n }).ToList()));
        }

        private void SetupPagedItems(string tableName, params string[] values)
        {
            _service.GetLookupItemsPagedAsync(tableName, Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse(PagedItems(values)));
        }

        #region Constructor

        [Fact]
        public void Constructor_WithNullService_ThrowsArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new MasterLookupController(null!));

            Assert.Equal("masterLookupService", exception.ParamName);
        }

        #endregion

        #region Index

        [Fact]
        public async Task Index_WithNoTableSpecified_SelectsFirstTable_ReturnsView()
        {
            // Arrange
            SetupTableNames("directorate", "disease");
            SetupPagedItems("directorate", "A", "B");

            // Act
            var result = await _controller.Index();

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MasterLookupMaintenanceViewModel>(view.Model);
            Assert.Equal("directorate", model.SelectedTable);
            Assert.Equal(2, model.TableNames.Count);
            Assert.NotNull(model.ItemGrid);
        }

        [Fact]
        public async Task Index_WithValidTableName_SelectsThatTable()
        {
            // Arrange
            SetupTableNames("directorate", "disease");
            SetupPagedItems("disease", "X");

            // Act
            var result = await _controller.Index("disease");

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MasterLookupMaintenanceViewModel>(view.Model);
            Assert.Equal("disease", model.SelectedTable);
        }

        [Fact]
        public async Task Index_WithUnknownTableName_FallsBackToFirstTable()
        {
            // Arrange
            SetupTableNames("directorate", "disease");
            SetupPagedItems("directorate", "A");

            // Act
            var result = await _controller.Index("unknown");

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MasterLookupMaintenanceViewModel>(view.Model);
            Assert.Equal("directorate", model.SelectedTable);
        }

        [Fact]
        public async Task Index_WithNoTablesAvailable_SelectedTableNull_GridNull()
        {
            // Arrange
            _service.GetAllMasterLookupsAsync()
                .Returns(SuccessResponse<IEnumerable<MasterLookupDto>>(new List<MasterLookupDto>()));

            // Act
            var result = await _controller.Index();

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MasterLookupMaintenanceViewModel>(view.Model);
            Assert.Null(model.SelectedTable);
            Assert.Null(model.ItemGrid);
        }

        [Fact]
        public async Task Index_WhenGetAllFails_ReturnsEmptyTableNames()
        {
            // Arrange
            _service.GetAllMasterLookupsAsync()
                .Returns(FailureResponse<IEnumerable<MasterLookupDto>>());

            // Act
            var result = await _controller.Index();

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MasterLookupMaintenanceViewModel>(view.Model);
            Assert.Empty(model.TableNames);
            Assert.Null(model.SelectedTable);
        }

        #endregion

        #region LoadItemsGrid

        [Fact]
        public async Task LoadItemsGrid_WithEmptyTableName_ReturnsFailureJson()
        {
            // Act
            var result = await _controller.LoadItemsGrid(new PaginationFilter<string>(), string.Empty);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadItemsGrid_WithInvalidModelState_ReturnsValidationFailureJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("Search", "Invalid");

            // Act
            var result = await _controller.LoadItemsGrid(new PaginationFilter<string>(), TableName);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadItemsGrid_ValidRequest_ReturnsPartialView()
        {
            // Arrange
            SetupPagedItems(TableName, "A", "B");

            // Act
            var result = await _controller.LoadItemsGrid(new PaginationFilter<string>(), TableName);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            var config = Assert.IsType<DataGridConfig<LookupItemViewModel>>(partial.Model);
            Assert.Equal(2, config.Data.Count);
        }

        #endregion

        #region Create (GET)

        [Fact]
        public void Create_Get_WithEmptyTableName_ReturnsFailureJson()
        {
            // Act
            var result = _controller.Create(string.Empty);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public void Create_Get_WithTableName_ReturnsPartialView()
        {
            // Act
            var result = _controller.Create(TableName);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditLookupItem", partial.ViewName);
            Assert.IsType<LookupItemViewModel>(partial.Model);
            Assert.Equal(TableName, _controller.ViewData["TableName"]);
        }

        #endregion

        #region Edit (GET)

        [Fact]
        public void Edit_Get_WithMissingArgs_ReturnsFailureJson()
        {
            // Act
            var result = _controller.Edit(string.Empty, string.Empty);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public void Edit_Get_WithValidArgs_ReturnsPartialViewWithValue()
        {
            // Act
            var result = _controller.Edit(TableName, "Existing");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditLookupItem", partial.ViewName);
            var model = Assert.IsType<LookupItemViewModel>(partial.Model);
            Assert.Equal("Existing", model.Value);
        }

        #endregion

        #region Create (POST)

        [Fact]
        public async Task Create_Post_WithEmptyTableName_ReturnsFailureJson()
        {
            // Act
            var result = await _controller.Create(string.Empty, new LookupItemViewModel { Value = "A" });

            // Assert
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Create_Post_WithInvalidModelState_ReturnsValidationFailureJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("Value", "Required");

            // Act
            var result = await _controller.Create(TableName, new LookupItemViewModel());

            // Assert
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Create_Post_Success_ReturnsSuccessJson()
        {
            // Arrange
            _service.CreateLookupItemAsync(TableName, "A").Returns(SuccessResponse(true));

            // Act
            var result = await _controller.Create(TableName, new LookupItemViewModel { Value = "A" });

            // Assert
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.True(element.GetProperty("success").GetBoolean());
            await _service.Received(1).CreateLookupItemAsync(TableName, "A");
        }

        [Fact]
        public async Task Create_Post_ServiceFailure_ReturnsFailureJson()
        {
            // Arrange
            _service.CreateLookupItemAsync(TableName, "A")
                .Returns(FailureResponse<bool>("Duplicate"));

            // Act
            var result = await _controller.Create(TableName, new LookupItemViewModel { Value = "A" });

            // Assert
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        #region Edit (POST)

        [Fact]
        public async Task Edit_Post_WithMissingArgs_ReturnsFailureJson()
        {
            // Act
            var result = await _controller.Edit(string.Empty, new LookupItemViewModel { Value = "A" }, string.Empty);

            // Assert
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Edit_Post_WithInvalidModelState_ReturnsValidationFailureJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("Value", "Required");

            // Act
            var result = await _controller.Edit(TableName, new LookupItemViewModel(), "Original");

            // Assert
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Edit_Post_Success_ReturnsSuccessJson()
        {
            // Arrange
            _service.UpdateLookupItemAsync(TableName, "Original", "New").Returns(SuccessResponse(true));

            // Act
            var result = await _controller.Edit(TableName, new LookupItemViewModel { Value = "New" }, "Original");

            // Assert
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.True(element.GetProperty("success").GetBoolean());
            await _service.Received(1).UpdateLookupItemAsync(TableName, "Original", "New");
        }

        [Fact]
        public async Task Edit_Post_ServiceFailure_ReturnsFailureJson()
        {
            // Arrange
            _service.UpdateLookupItemAsync(TableName, "Original", "New")
                .Returns(FailureResponse<bool>());

            // Act
            var result = await _controller.Edit(TableName, new LookupItemViewModel { Value = "New" }, "Original");

            // Assert
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        #region Delete

        [Fact]
        public async Task Delete_WithMissingArgs_ReturnsFailureJson()
        {
            // Act
            var result = await _controller.Delete(string.Empty, string.Empty);

            // Assert
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Delete_Success_ReturnsSuccessJson()
        {
            // Arrange
            _service.DeleteLookupItemAsync(TableName, "A").Returns(SuccessResponse(true));

            // Act
            var result = await _controller.Delete(TableName, "A");

            // Assert
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.True(element.GetProperty("success").GetBoolean());
            await _service.Received(1).DeleteLookupItemAsync(TableName, "A");
        }

        [Fact]
        public async Task Delete_ServiceFailure_ReturnsFailureJson()
        {
            // Arrange
            _service.DeleteLookupItemAsync(TableName, "A")
                .Returns(FailureResponse<bool>("In use"));

            // Act
            var result = await _controller.Delete(TableName, "A");

            // Assert
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        #region CheckItemExists

        [Fact]
        public async Task CheckItemExists_WithMissingArgs_ReturnsExistsFalse()
        {
            // Act
            var result = await _controller.CheckItemExists(string.Empty, string.Empty);

            // Assert
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.False(element.GetProperty("exists").GetBoolean());
        }

        [Fact]
        public async Task CheckItemExists_ValueEqualsOriginal_ReturnsExistsFalse()
        {
            // Act
            var result = await _controller.CheckItemExists(TableName, "Same", "same");

            // Assert
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.False(element.GetProperty("exists").GetBoolean());
        }

        [Fact]
        public async Task CheckItemExists_ValueExists_ReturnsExistsTrue()
        {
            // Arrange
            _service.GetLookupItemsAsync(TableName)
                .Returns(SuccessResponse<IEnumerable<LookupItemDto>>(
                    new List<LookupItemDto> { new LookupItemDto { Value = "Existing" } }));

            // Act
            var result = await _controller.CheckItemExists(TableName, "existing");

            // Assert
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.True(element.GetProperty("exists").GetBoolean());
        }

        [Fact]
        public async Task CheckItemExists_ValueDoesNotExist_ReturnsExistsFalse()
        {
            // Arrange
            _service.GetLookupItemsAsync(TableName)
                .Returns(SuccessResponse<IEnumerable<LookupItemDto>>(
                    new List<LookupItemDto> { new LookupItemDto { Value = "Other" } }));

            // Act
            var result = await _controller.CheckItemExists(TableName, "missing");

            // Assert
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.False(element.GetProperty("exists").GetBoolean());
        }

        [Fact]
        public async Task CheckItemExists_ServiceFailure_ReturnsExistsFalse()
        {
            // Arrange
            _service.GetLookupItemsAsync(TableName)
                .Returns(FailureResponse<IEnumerable<LookupItemDto>>());

            // Act
            var result = await _controller.CheckItemExists(TableName, "value");

            // Assert
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.False(element.GetProperty("exists").GetBoolean());
        }

        #endregion
    }
}
