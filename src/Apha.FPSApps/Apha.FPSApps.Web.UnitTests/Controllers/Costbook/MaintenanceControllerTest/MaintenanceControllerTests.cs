/*
 * TRANSFORMENGINE MIGRATION — MaintenanceControllerTests.cs (Frontend Web UnitTests)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New xUnit test class for Apha.FPSApps.Web.Areas.CostBook.Controllers.MaintenanceController (frontend MVC)
 *   - Covers Index, SaveInflationSettings, SaveProfitMargins, LoadAccountCategoryGrid,
 *     EditAccountCategory (GET+POST), LoadCsg7GroupGrid, CreateCsg7Group (GET+POST),
 *     EditCsg7Group (GET+POST), DeleteCsg7Group, LoadCapsStaffGrid,
 *     CreateCapsStaff (GET+POST), EditCapsStaff (GET+POST), DeleteCapsStaff
 *   - Uses NSubstitute for IMapper, ICostBookMaintenanceService, ICostBookAccountGroupService,
 *     ICostBookCapsStaffService
 *
 * PRESERVED:
 *   - Test naming convention: [MethodName]_[StateUnderTest]_[ExpectedResult]
 *   - AAA pattern with explicit Arrange/Act/Assert comments
 *   - JSON result helper matching ProjectsControllerTests pattern
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: none — fully automated.
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.CostBook.Controllers;
using Apha.FPSApps.Web.Areas.CostBook.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.CostBook.MaintenanceControllerTest
{
    public class MaintenanceControllerTests
    {
        private readonly IMapper _mapper;
        private readonly ICostBookMaintenanceService _maintenanceService;
        private readonly ICostBookAccountGroupService _accountGroupService;
        private readonly ICostBookCapsStaffService _capsStaffService;
        private readonly MaintenanceController _controller;

        public MaintenanceControllerTests()
        {
            _mapper             = Substitute.For<IMapper>();
            _maintenanceService = Substitute.For<ICostBookMaintenanceService>();
            _accountGroupService = Substitute.For<ICostBookAccountGroupService>();
            _capsStaffService   = Substitute.For<ICostBookCapsStaffService>();
            _controller = new MaintenanceController(
                _mapper,
                _maintenanceService,
                _accountGroupService,
                _capsStaffService);
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        // ── Index ─────────────────────────────────────────────────────────────

        #region Index Tests

        [Fact]
        public async Task Index_ServiceReturnsSettings_ReturnsViewWithPopulatedViewModel()
        {
            // Arrange
            var settingsDto = new MaintenanceSettingsDto
            {
                InflationAnimals = 2.5m, InflationExceptionalCosts = 1.8m, InflationStaff = 3.0m,
                InflationTests = 2.0m, CurrentFinancialYear = 2024, WorkingHoursInDay = 7.4m,
                WorkingDaysInYear = 220m, ProfitAnimals = 15m, ProfitExceptionalCosts = 12m,
                ProfitStaff = 10m, ProfitTests = 8m
            };
            var settingsResult = ApiResponseDto<MaintenanceSettingsDto>.SuccessResponse(settingsDto);
            var groupResult = ApiResponseDto<List<AccountGroupDto>>.SuccessResponse(new List<AccountGroupDto>());

            _maintenanceService.GetSettingsAsync().Returns(settingsResult);
            _accountGroupService.GetAllAccountGroupsAsync().Returns(groupResult);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MaintenanceViewModel>(viewResult.Model);
            Assert.Equal(2.5m, model.InflationAnimals);
            Assert.Equal(2024, model.CurrentFinancialYear);
            Assert.Equal(15m, model.ProfitAnimals);
        }

        [Fact]
        public async Task Index_ServiceReturnsFailure_ReturnsViewWithEmptyViewModel()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Code = "ERR", Message = "Failed" } };
            var settingsResult = ApiResponseDto<MaintenanceSettingsDto>.FailureResponse(errors, new ApiMetaDto());
            var groupResult = ApiResponseDto<List<AccountGroupDto>>.SuccessResponse(new List<AccountGroupDto>());

            _maintenanceService.GetSettingsAsync().Returns(settingsResult);
            _accountGroupService.GetAllAccountGroupsAsync().Returns(groupResult);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MaintenanceViewModel>(viewResult.Model);
            Assert.Equal(0m, model.InflationAnimals);
        }

        #endregion

        // ── SaveInflationSettings ─────────────────────────────────────────────

        #region SaveInflationSettings Tests

        [Fact]
        public async Task SaveInflationSettings_ValidItem_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var item = new InflationSettingsItem
            {
                InflationAnimals = 2.5m, InflationExceptionalCosts = 1.8m, InflationStaff = 3.0m,
                InflationTests = 2.0m, CurrentFinancialYear = 2024, WorkingHoursInDay = 7.4m, WorkingDaysInYear = 220m
            };
            var settingsDto = new MaintenanceSettingsDto { ProfitAnimals = 15m };
            var currentResult = ApiResponseDto<MaintenanceSettingsDto>.SuccessResponse(settingsDto);
            var updateResult = ApiResponseDto<MaintenanceSettingsDto>.SuccessResponse(new MaintenanceSettingsDto());

            _maintenanceService.GetSettingsAsync().Returns(currentResult);
            _maintenanceService.UpdateSettingsAsync(Arg.Any<MaintenanceSettingsDto>()).Returns(updateResult);

            // Act
            var result = await _controller.SaveInflationSettings(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveInflationSettings_NullItem_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.SaveInflationSettings(null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveInflationSettings_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var item = new InflationSettingsItem { InflationAnimals = 2.5m };
            var settingsDto = new MaintenanceSettingsDto();
            var currentResult = ApiResponseDto<MaintenanceSettingsDto>.SuccessResponse(settingsDto);
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Code = "ERR", Message = "Update failed" } };
            var updateResult = ApiResponseDto<MaintenanceSettingsDto>.FailureResponse(errors, new ApiMetaDto());

            _maintenanceService.GetSettingsAsync().Returns(currentResult);
            _maintenanceService.UpdateSettingsAsync(Arg.Any<MaintenanceSettingsDto>()).Returns(updateResult);

            // Act
            var result = await _controller.SaveInflationSettings(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── SaveProfitMargins ─────────────────────────────────────────────────

        #region SaveProfitMargins Tests

        [Fact]
        public async Task SaveProfitMargins_ValidItem_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var item = new ProfitMarginsItem { ProfitAnimals = 15m, ProfitExceptionalCosts = 12m, ProfitStaff = 10m, ProfitTests = 8m };
            var settingsDto = new MaintenanceSettingsDto { InflationAnimals = 2.5m };
            var currentResult = ApiResponseDto<MaintenanceSettingsDto>.SuccessResponse(settingsDto);
            var updateResult = ApiResponseDto<MaintenanceSettingsDto>.SuccessResponse(new MaintenanceSettingsDto());

            _maintenanceService.GetSettingsAsync().Returns(currentResult);
            _maintenanceService.UpdateSettingsAsync(Arg.Any<MaintenanceSettingsDto>()).Returns(updateResult);

            // Act
            var result = await _controller.SaveProfitMargins(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveProfitMargins_NullItem_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.SaveProfitMargins(null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── LoadAccountCategoryGrid ───────────────────────────────────────────

        #region LoadAccountCategoryGrid Tests

        [Fact]
        public async Task LoadAccountCategoryGrid_ServiceReturnsData_ReturnsPartialViewWithGridConfig()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var accCats = new List<AccountCategoryMaintenanceDto>
            {
                new AccountCategoryMaintenanceDto { AccShortName = "ACC01", Csg7Group = "CSG001" }
            };
            var result = ApiResponseDto<List<AccountCategoryMaintenanceDto>>.SuccessResponse(accCats);
            var items = new List<AccountCategoryItem> { new AccountCategoryItem { AccShortName = "ACC01" } };

            _maintenanceService.GetAccountCategoriesAsync().Returns(result);
            _mapper.Map<List<AccountCategoryItem>>(accCats).Returns(items);

            // Act
            var actionResult = await _controller.LoadAccountCategoryGrid(request);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(actionResult);
            Assert.Equal("_DataGrid", partialResult.ViewName);
        }

        [Fact]
        public async Task LoadAccountCategoryGrid_ServiceReturnsEmpty_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var emptyResult = ApiResponseDto<List<AccountCategoryMaintenanceDto>>.SuccessResponse(new List<AccountCategoryMaintenanceDto>());
            var emptyItems = new List<AccountCategoryItem>();

            _maintenanceService.GetAccountCategoriesAsync().Returns(emptyResult);
            _mapper.Map<List<AccountCategoryItem>>(Arg.Any<List<AccountCategoryMaintenanceDto>>()).Returns(emptyItems);

            // Act
            var actionResult = await _controller.LoadAccountCategoryGrid(request);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(actionResult);
            Assert.Equal("_DataGrid", partialResult.ViewName);
        }

        #endregion

        // ── EditAccountCategory ───────────────────────────────────────────────

        #region EditAccountCategory Tests

        [Fact]
        public async Task EditAccountCategory_Get_ExistingKey_ReturnsPartialViewWithItem()
        {
            // Arrange
            var accShortName = "ACC01";
            var accCats = new List<AccountCategoryMaintenanceDto>
            {
                new AccountCategoryMaintenanceDto { AccShortName = accShortName, Csg7Group = "CSG001" }
            };
            var listResult = ApiResponseDto<List<AccountCategoryMaintenanceDto>>.SuccessResponse(accCats);
            var item = new AccountCategoryItem { AccShortName = accShortName, Csg7Group = "CSG001" };

            _maintenanceService.GetAccountCategoriesAsync().Returns(listResult);
            _mapper.Map<AccountCategoryItem>(Arg.Any<AccountCategoryMaintenanceDto>()).Returns(item);

            // Act
            var result = await _controller.EditAccountCategory(accShortName);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditAccountCategory", partialResult.ViewName);
        }

        [Fact]
        public async Task EditAccountCategory_Get_EmptyKey_ReturnsNotFound()
        {
            // Act
            var result = await _controller.EditAccountCategory(string.Empty);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task EditAccountCategory_Post_ValidItem_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var accShortName = "ACC01";
            var item = new AccountCategoryItem { AccShortName = accShortName, Csg7Group = "CSG001" };
            var dto = new AccountCategoryMaintenanceDto { AccShortName = accShortName, Csg7Group = "CSG001" };
            var updateResult = ApiResponseDto<AccountCategoryMaintenanceDto>.SuccessResponse(dto);

            _mapper.Map<AccountCategoryMaintenanceDto>(item).Returns(dto);
            _maintenanceService.UpdateAccountCategoryAsync(accShortName, dto).Returns(updateResult);

            // Act
            var result = await _controller.EditAccountCategory(accShortName, item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task EditAccountCategory_Post_NullItem_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.EditAccountCategory("ACC01", null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── LoadCsg7GroupGrid ─────────────────────────────────────────────────

        #region LoadCsg7GroupGrid Tests

        [Fact]
        public async Task LoadCsg7GroupGrid_ServiceReturnsData_ReturnsPartialViewWithGridConfig()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var groups = new List<AccountGroupDto> { new AccountGroupDto { Csg7Group = "CSG001", UseInflation = true } };
            var groupResult = ApiResponseDto<List<AccountGroupDto>>.SuccessResponse(groups);
            var items = new List<Csg7GroupItem> { new Csg7GroupItem { Csg7Group = "CSG001" } };

            _accountGroupService.GetAllAccountGroupsAsync().Returns(groupResult);
            _mapper.Map<List<Csg7GroupItem>>(groups).Returns(items);

            // Act
            var actionResult = await _controller.LoadCsg7GroupGrid(request);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(actionResult);
            Assert.Equal("_DataGrid", partialResult.ViewName);
        }

        #endregion

        // ── CreateCsg7Group ───────────────────────────────────────────────────

        #region CreateCsg7Group Tests

        [Fact]
        public void CreateCsg7Group_Get_ReturnsPartialViewWithEmptyItem()
        {
            // Act
            var result = _controller.CreateCsg7Group();

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditCsg7Group", partialResult.ViewName);
            Assert.IsType<Csg7GroupItem>(partialResult.Model);
        }

        [Fact]
        public async Task CreateCsg7Group_Post_ValidDto_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var dto = new AccountGroupDto { Csg7Group = "CSG003", UseInflation = true };
            var addResult = ApiResponseDto<AccountGroupDto>.SuccessResponse(dto);
            _accountGroupService.AddAccountGroupAsync(dto).Returns(addResult);

            // Act
            var result = await _controller.CreateCsg7Group(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CreateCsg7Group_Post_NullDto_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.CreateCsg7Group((AccountGroupDto)null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CreateCsg7Group_Post_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var dto = new AccountGroupDto { Csg7Group = "CSG001", UseInflation = true };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Code = "DUP", Message = "Already exists" } };
            var addResult = ApiResponseDto<AccountGroupDto>.FailureResponse(errors, new ApiMetaDto());
            _accountGroupService.AddAccountGroupAsync(dto).Returns(addResult);

            // Act
            var result = await _controller.CreateCsg7Group(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── EditCsg7Group ─────────────────────────────────────────────────────

        #region EditCsg7Group Tests

        [Fact]
        public async Task EditCsg7Group_Get_ExistingKey_ReturnsPartialViewWithMappedItem()
        {
            // Arrange
            var key = "CSG001";
            var dto = new AccountGroupDto { Csg7Group = key, UseInflation = true };
            var groupResult = ApiResponseDto<AccountGroupDto>.SuccessResponse(dto);
            var item = new Csg7GroupItem { Csg7Group = key, UseInflation = true };

            _accountGroupService.GetAccountGroupAsync(key).Returns(groupResult);
            _mapper.Map<Csg7GroupItem>(dto).Returns(item);

            // Act
            var result = await _controller.EditCsg7Group(key);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditCsg7Group", partialResult.ViewName);
        }

        [Fact]
        public async Task EditCsg7Group_Get_EmptyKey_ReturnsNotFound()
        {
            // Act
            var result = await _controller.EditCsg7Group(string.Empty);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task EditCsg7Group_Post_ValidDto_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var key = "CSG001";
            var dto = new AccountGroupDto { Csg7Group = key, UseInflation = false };
            var updateResult = ApiResponseDto<AccountGroupDto>.SuccessResponse(dto);
            _accountGroupService.UpdateAccountGroupAsync(key, dto).Returns(updateResult);

            // Act
            var result = await _controller.EditCsg7Group(key, dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task EditCsg7Group_Post_NullDto_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.EditCsg7Group("CSG001", (AccountGroupDto)null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── DeleteCsg7Group ───────────────────────────────────────────────────

        #region DeleteCsg7Group Tests

        [Fact]
        public async Task DeleteCsg7Group_ExistingKey_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var key = "CSG001";
            var deleteResult = ApiResponseDto<bool>.SuccessResponse(true);
            _accountGroupService.DeleteAccountGroupAsync(key).Returns(deleteResult);

            // Act
            var result = await _controller.DeleteCsg7Group(key);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteCsg7Group_EmptyKey_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.DeleteCsg7Group(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteCsg7Group_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var key = "CSG001";
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Code = "ERR", Message = "Delete failed" } };
            var deleteResult = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _accountGroupService.DeleteAccountGroupAsync(key).Returns(deleteResult);

            // Act
            var result = await _controller.DeleteCsg7Group(key);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── LoadCapsStaffGrid ─────────────────────────────────────────────────

        #region LoadCapsStaffGrid Tests

        [Fact]
        public async Task LoadCapsStaffGrid_ServiceReturnsData_ReturnsPartialViewWithGridConfig()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<CapsStaffDto> { new CapsStaffDto { MNumber = "M001", Name = "Alice" } };
            var pagedResult = ApiResponseDto<List<CapsStaffDto>>.SuccessResponse(dtos);
            var items = new List<CapsStaffItem> { new CapsStaffItem { MNumber = "M001", Name = "Alice" } };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _capsStaffService.GetPaginatedCapsStaffAsync(queryParameters).Returns(pagedResult);
            _mapper.Map<List<CapsStaffItem>>(dtos).Returns(items);

            // Act
            var actionResult = await _controller.LoadCapsStaffGrid(request);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(actionResult);
            Assert.Equal("_DataGrid", partialResult.ViewName);
        }

        #endregion

        // ── CreateCapsStaff ───────────────────────────────────────────────────

        #region CreateCapsStaff Tests

        [Fact]
        public void CreateCapsStaff_Get_ReturnsPartialViewWithEmptyItem()
        {
            // Act
            var result = _controller.CreateCapsStaff();

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditCapsStaff", partialResult.ViewName);
            Assert.IsType<CapsStaffItem>(partialResult.Model);
        }

        [Fact]
        public async Task CreateCapsStaff_Post_ValidDto_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var dto = new CapsStaffDto { MNumber = "M003", Name = "Charlie" };
            var addResult = ApiResponseDto<CapsStaffDto>.SuccessResponse(dto);
            _capsStaffService.AddCapsStaffAsync(dto).Returns(addResult);

            // Act
            var result = await _controller.CreateCapsStaff(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CreateCapsStaff_Post_NullDto_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.CreateCapsStaff((CapsStaffDto)null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CreateCapsStaff_Post_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var dto = new CapsStaffDto { MNumber = "M001", Name = "Duplicate" };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Code = "DUP", Message = "Already exists" } };
            var addResult = ApiResponseDto<CapsStaffDto>.FailureResponse(errors, new ApiMetaDto());
            _capsStaffService.AddCapsStaffAsync(dto).Returns(addResult);

            // Act
            var result = await _controller.CreateCapsStaff(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── EditCapsStaff ─────────────────────────────────────────────────────

        #region EditCapsStaff Tests

        [Fact]
        public async Task EditCapsStaff_Get_ExistingMNumber_ReturnsPartialViewWithMappedItem()
        {
            // Arrange
            var mNumber = "M001";
            var dto = new CapsStaffDto { MNumber = mNumber, Name = "Alice" };
            var staffResult = ApiResponseDto<CapsStaffDto>.SuccessResponse(dto);
            var item = new CapsStaffItem { MNumber = mNumber, Name = "Alice" };

            _capsStaffService.GetCapsStaffByMNumberAsync(mNumber).Returns(staffResult);
            _mapper.Map<CapsStaffItem>(dto).Returns(item);

            // Act
            var result = await _controller.EditCapsStaff(mNumber);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditCapsStaff", partialResult.ViewName);
        }

        [Fact]
        public async Task EditCapsStaff_Get_EmptyMNumber_ReturnsNotFound()
        {
            // Act
            var result = await _controller.EditCapsStaff(string.Empty);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task EditCapsStaff_Get_ServiceReturnsFailure_ReturnsNotFound()
        {
            // Arrange
            var mNumber = "NOTEXIST";
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Code = "404", Message = "Not found" } };
            var staffResult = ApiResponseDto<CapsStaffDto>.FailureResponse(errors, new ApiMetaDto());
            _capsStaffService.GetCapsStaffByMNumberAsync(mNumber).Returns(staffResult);

            // Act
            var result = await _controller.EditCapsStaff(mNumber);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task EditCapsStaff_Post_ValidDto_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var mNumber = "M001";
            var dto = new CapsStaffDto { MNumber = mNumber, Name = "Alice Updated" };
            var updateResult = ApiResponseDto<CapsStaffDto>.SuccessResponse(dto);
            _capsStaffService.UpdateCapsStaffAsync(mNumber, dto).Returns(updateResult);

            // Act
            var result = await _controller.EditCapsStaff(mNumber, dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task EditCapsStaff_Post_NullDto_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.EditCapsStaff("M001", (CapsStaffDto)null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── DeleteCapsStaff ───────────────────────────────────────────────────

        #region DeleteCapsStaff Tests

        [Fact]
        public async Task DeleteCapsStaff_ExistingMNumber_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var mNumber = "M001";
            var deleteResult = ApiResponseDto<bool>.SuccessResponse(true);
            _capsStaffService.DeleteCapsStaffAsync(mNumber).Returns(deleteResult);

            // Act
            var result = await _controller.DeleteCapsStaff(mNumber);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteCapsStaff_EmptyMNumber_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.DeleteCapsStaff(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteCapsStaff_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var mNumber = "M001";
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Code = "ERR", Message = "Delete failed" } };
            var deleteResult = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _capsStaffService.DeleteCapsStaffAsync(mNumber).Returns(deleteResult);

            // Act
            var result = await _controller.DeleteCapsStaff(mNumber);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion
    }
}
