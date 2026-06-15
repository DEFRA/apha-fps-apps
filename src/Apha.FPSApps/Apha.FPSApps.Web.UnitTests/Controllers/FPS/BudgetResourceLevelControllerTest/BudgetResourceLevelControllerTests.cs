using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Apha.Common.Utilities.ExcelExport;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.BudgetResourceLevelControllerTest
{
    public class BudgetResourceLevelControllerTests
    {
        private readonly IMapper _mapper;
        private readonly Apha.FPSApps.Application.Interfaces.PACT.IWorkGroupService _workGroupService;
        private readonly IBudgetBidsService _budgetBidsService;
        private readonly IPurchasesService _purchasesService;
        private readonly IProfitCentreService _profitCentreService;
        private readonly IExcelExportService _excelExportService;
        private readonly BudgetResourceLevelController _controller;

        public BudgetResourceLevelControllerTests()
        {
            _mapper              = Substitute.For<IMapper>();
            _workGroupService    = Substitute.For<Apha.FPSApps.Application.Interfaces.PACT.IWorkGroupService>();
            _budgetBidsService   = Substitute.For<IBudgetBidsService>();
            _purchasesService    = Substitute.For<IPurchasesService>();
            _profitCentreService = Substitute.For<IProfitCentreService>();
            _excelExportService  = Substitute.For<IExcelExportService>();

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            _controller = new BudgetResourceLevelController(
                _mapper,
                _workGroupService,
                _budgetBidsService,
                _purchasesService,
                _profitCentreService,
                _excelExportService);

            // Stub IUrlHelper so Url.Action() calls made by the S1075 fix do not throw in tests
            var urlHelper = Substitute.For<IUrlHelper>();
            urlHelper.Action(Arg.Any<UrlActionContext>()).Returns(callInfo =>
            {
                var ctx = callInfo.Arg<UrlActionContext>();
                return $"/FPS/BudgetResourceLevel/{ctx.Action}";
            });
            _controller.Url = urlHelper;
        }

        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json);
        }

        private void SetupDefaultProfitCentres()
        {
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(new List<ProfitCentreDto>
                {
                    new() { ProfitCentreId = "PC01", ProfitCentreName = "PC 01" }
                }));
        }

        private void SetupDefaultWorkGroups(string profitCentre = "PC01")
        {
            var workGroups = new List<WorkGroupViewDto>
            {
                new() { WorkGroupName = "WG01", ProfitCentre = profitCentre }
            };
            _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync(profitCentre)
                .Returns(ApiResponseDto<List<WorkGroupViewDto>>.SuccessResponse(workGroups));
            _workGroupService.GetWorkGroupsByProfitCentreForBudgetPagedAsync(
                Arg.Any<QueryParameters<string>>(), profitCentre)
                .Returns(ApiResponseDto<List<WorkGroupViewDto>>.SuccessResponse(
                    workGroups, new PaginationDto { PageNumber = 1, PageSize = 5, TotalRecords = 1, TotalPages = 1 }));
        }

        private void SetupDefaultBidView(string workgroup = "WG01")
        {
            var bids = new List<BidViewDto>
            {
                new() { WorkGroupName = workgroup, Account = "ACC1", GenBid = 100m }
            };
            _budgetBidsService.GetBidViewAsync(workgroup)
                .Returns(ApiResponseDto<List<BidViewDto>>.SuccessResponse(bids));
            _budgetBidsService.GetBidViewPagedAsync(Arg.Any<QueryParameters<string>>(), workgroup)
                .Returns(ApiResponseDto<List<BidViewDto>>.SuccessResponse(
                    bids, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 }));
            _budgetBidsService.GetAccountCategoriesAsync()
                .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(new List<AccountCategoryDto>
                {
                    new() { AccShortName = "ACC1", AccountDescription = "Account 1" }
                }));
        }

        private void SetupDefaultPurchases(string workgroup = "WG01", string account = "ACC1")
        {
            var purchases = new List<PurchaseDto>
            {
                new() { WorkGroupName = workgroup, Account = account, ItemDescription = "Item A", Amount = 50m }
            };
            _purchasesService.GetPurchasesAsync(workgroup, account)
                .Returns(ApiResponseDto<List<PurchaseDto>>.SuccessResponse(purchases));
            _purchasesService.GetPurchasesPagedAsync(Arg.Any<QueryParameters<string>>(), workgroup, account)
                .Returns(ApiResponseDto<List<PurchaseDto>>.SuccessResponse(
                    purchases, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 }));
        }

        #region Index Tests

        [Fact]
        public async Task Index_WithNoProfitCentre_ReturnsViewResultWithViewModel()
        {
            // Arrange
            SetupDefaultProfitCentres();
            _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync(Arg.Any<string>())
                .Returns(ApiResponseDto<List<WorkGroupViewDto>>.SuccessResponse(new List<WorkGroupViewDto>()));
            _budgetBidsService.GetBidViewAsync(Arg.Any<string>())
                .Returns(ApiResponseDto<List<BidViewDto>>.SuccessResponse(new List<BidViewDto>()));
            _budgetBidsService.GetAccountCategoriesAsync()
                .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(new List<AccountCategoryDto>()));
            _purchasesService.GetPurchasesAsync(Arg.Any<string>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<PurchaseDto>>.SuccessResponse(new List<PurchaseDto>()));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BudgetResourceLevelViewModel>(viewResult.Model);
            Assert.Equal(string.Empty, model.SelectedProfitCentre);
            Assert.NotNull(model.WorkGroupGrid);
            Assert.NotNull(model.BudgetBidsGrid);
            Assert.NotNull(model.PurchasesGrid);
        }

        [Fact]
        public async Task Index_WithProfitCentre_LoadsWorkGroupGrid()
        {
            // Arrange
            SetupDefaultProfitCentres();
            SetupDefaultWorkGroups("PC01");
            _budgetBidsService.GetBidViewAsync(Arg.Any<string>())
                .Returns(ApiResponseDto<List<BidViewDto>>.SuccessResponse(new List<BidViewDto>()));
            _budgetBidsService.GetAccountCategoriesAsync()
                .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(new List<AccountCategoryDto>()));
            _purchasesService.GetPurchasesAsync(Arg.Any<string>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<PurchaseDto>>.SuccessResponse(new List<PurchaseDto>()));

            // Act
            var result = await _controller.Index("PC01");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BudgetResourceLevelViewModel>(viewResult.Model);
            Assert.Equal("PC01", model.SelectedProfitCentre);
            Assert.Equal("workGroupGrid", model.WorkGroupGrid.GridId);
        }

        [Fact]
        public async Task Index_SetsGridIds_Correctly()
        {
            // Arrange
            SetupDefaultProfitCentres();
            _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync(Arg.Any<string>())
                .Returns(ApiResponseDto<List<WorkGroupViewDto>>.SuccessResponse(new List<WorkGroupViewDto>()));
            _budgetBidsService.GetBidViewAsync(Arg.Any<string>())
                .Returns(ApiResponseDto<List<BidViewDto>>.SuccessResponse(new List<BidViewDto>()));
            _budgetBidsService.GetAccountCategoriesAsync()
                .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(new List<AccountCategoryDto>()));
            _purchasesService.GetPurchasesAsync(Arg.Any<string>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<PurchaseDto>>.SuccessResponse(new List<PurchaseDto>()));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BudgetResourceLevelViewModel>(viewResult.Model);
            Assert.Equal("workGroupGrid",   model.WorkGroupGrid.GridId);
            Assert.Equal("budgetBidsGrid",  model.BudgetBidsGrid.GridId);
            Assert.Equal("purchasesGrid",   model.PurchasesGrid.GridId);
        }

        #endregion

        #region LoadWorkGroupGrid Tests

        [Fact]
        public async Task LoadWorkGroupGrid_WithValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupDefaultWorkGroups("PC01");

            // Act
            var result = await _controller.LoadWorkGroupGrid(request, "PC01");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            Assert.IsType<DataGridConfig<WorkGroupItem>>(partialView.Model);
        }

        [Fact]
        public async Task LoadWorkGroupGrid_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _controller.ModelState.AddModelError("test", "error");

            // Act
            var result = await _controller.LoadWorkGroupGrid(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Invalid request data", value.message);
        }

        [Fact]
        public async Task LoadWorkGroupGrid_WithNullProfitCentre_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadWorkGroupGrid(request, null);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var config = Assert.IsType<DataGridConfig<WorkGroupItem>>(partialView.Model);
            Assert.Empty(config.Data!);
        }

        [Fact]
        public async Task LoadWorkGroupGrid_WhenServiceReturnsFailure_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            _workGroupService.GetWorkGroupsByProfitCentreForBudgetPagedAsync(
                Arg.Any<QueryParameters<string>>(), "PC01")
                .Returns(ApiResponseDto<List<WorkGroupViewDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Error" } }, new ApiMetaDto()));

            // Act
            var result = await _controller.LoadWorkGroupGrid(request, "PC01");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var config = Assert.IsType<DataGridConfig<WorkGroupItem>>(partialView.Model);
            Assert.Empty(config.Data!);
        }

        [Fact]
        public async Task LoadWorkGroupGrid_GridConfig_HasNoActionColumn()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupDefaultWorkGroups("PC01");

            // Act
            var result = await _controller.LoadWorkGroupGrid(request, "PC01");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var config = Assert.IsType<DataGridConfig<WorkGroupItem>>(partialView.Model);
            Assert.Equal("workGroupGrid", config.GridId);
            Assert.False(config.AllowAdd);
            Assert.False(config.AllowEdit);
            Assert.False(config.AllowDelete);
        }

        #endregion

        #region LoadBudgetBidsGrid Tests

        [Fact]
        public async Task LoadBudgetBidsGrid_WithValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupDefaultBidView("WG01");

            // Act
            var result = await _controller.LoadBudgetBidsGrid(request, "WG01");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            Assert.IsType<DataGridConfig<BudgetResourceCentreLevelItem>>(partialView.Model);
        }

        [Fact]
        public async Task LoadBudgetBidsGrid_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _controller.ModelState.AddModelError("test", "error");

            // Act
            var result = await _controller.LoadBudgetBidsGrid(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task LoadBudgetBidsGrid_WithNullWorkgroup_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadBudgetBidsGrid(request, null);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var config = Assert.IsType<DataGridConfig<BudgetResourceCentreLevelItem>>(partialView.Model);
            Assert.Empty(config.Data!);
        }

        [Fact]
        public async Task LoadBudgetBidsGrid_GridConfig_HasCorrectSettings()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupDefaultBidView("WG01");

            // Act
            var result = await _controller.LoadBudgetBidsGrid(request, "WG01");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var config = Assert.IsType<DataGridConfig<BudgetResourceCentreLevelItem>>(partialView.Model);
            Assert.Equal("budgetBidsGrid", config.GridId);
            Assert.False(config.AllowAdd);
            Assert.True(config.AllowEdit);
            Assert.True(config.AllowDelete);
        }

        #endregion

        #region LoadPurchasesGrid Tests

        [Fact]
        public async Task LoadPurchasesGrid_WithValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupDefaultPurchases("WG01", "ACC1");

            // Act
            var result = await _controller.LoadPurchasesGrid(request, "WG01", "ACC1");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            Assert.IsType<DataGridConfig<PurchaseItem>>(partialView.Model);
        }

        [Fact]
        public async Task LoadPurchasesGrid_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _controller.ModelState.AddModelError("test", "error");

            // Act
            var result = await _controller.LoadPurchasesGrid(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task LoadPurchasesGrid_WithNullWorkgroupOrAccount_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };

            // Act - null workgroup
            var result1 = await _controller.LoadPurchasesGrid(request, null, "ACC1");
            var config1 = Assert.IsType<DataGridConfig<PurchaseItem>>(Assert.IsType<PartialViewResult>(result1).Model);
            Assert.Empty(config1.Data!);

            // Act - null account
            var result2 = await _controller.LoadPurchasesGrid(request, "WG01", null);
            var config2 = Assert.IsType<DataGridConfig<PurchaseItem>>(Assert.IsType<PartialViewResult>(result2).Model);
            Assert.Empty(config2.Data!);
        }

        [Fact]
        public async Task LoadPurchasesGrid_GridConfig_HasCorrectSettings()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupDefaultPurchases("WG01", "ACC1");

            // Act
            var result = await _controller.LoadPurchasesGrid(request, "WG01", "ACC1");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var config = Assert.IsType<DataGridConfig<PurchaseItem>>(partialView.Model);
            Assert.Equal("purchasesGrid", config.GridId);
            Assert.False(config.AllowAdd);
            Assert.True(config.AllowEdit);
            Assert.Equal("editPurchase", config.EditFunction);
            Assert.True(config.AllowDelete);
            Assert.Equal("deletePurchase", config.DeleteFunction);
        }

        #endregion

        #region CreateBudgetBid Tests

        [Fact]
        public async Task CreateBudgetBid_Get_ReturnsPartialViewWithModel()
        {
            // Arrange
            _budgetBidsService.GetAccountCategoriesAsync()
                .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(new List<AccountCategoryDto>
                {
                    new() { AccShortName = "ACC1", AccountDescription = "Account 1" }
                }));

            // Act
            var result = await _controller.CreateBudgetBid("WG01");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditBudgetResourceLevel", partialView.ViewName);
            var model = Assert.IsType<BudgetResourceLevelItem>(partialView.Model);
            Assert.Equal(BudgetResourceLevelModalType.BudgetBid, model.ModalType);
        }

        [Fact]
        public async Task CreateBudgetBid_Post_WithValidModel_ReturnsSuccessJson()
        {
            // Arrange
            var model = new BudgetResourceCentreLevelItem { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m };
            var dto   = new BidDto { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m };
            _budgetBidsService.CreateBidAsync(Arg.Any<BidDto>())
                .Returns(ApiResponseDto<BidDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.CreateBudgetBid(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Budget bid created successfully.", value.message);
        }

        [Fact]
        public async Task CreateBudgetBid_Post_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var model = new BudgetResourceCentreLevelItem();
            _controller.ModelState.AddModelError("Account", "Account is required");

            // Act
            var result = await _controller.CreateBudgetBid(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Please correct the validation errors.", value.message);
        }

        [Fact]
        public async Task CreateBudgetBid_Post_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var model = new BudgetResourceCentreLevelItem { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m };
            _budgetBidsService.CreateBidAsync(Arg.Any<BidDto>())
                .Returns(ApiResponseDto<BidDto>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Failed to create bid.", Code = "ERR" } },
                    new ApiMetaDto()));

            // Act
            var result = await _controller.CreateBudgetBid(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Failed to create bid.", value.message);
        }

        #endregion

        #region EditBudgetBid Tests

        [Fact]
        public async Task EditBudgetBid_Get_WithExistingBid_ReturnsPartialView()
        {
            // Arrange
            var dto = new BidDto { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m };
            _budgetBidsService.GetBidByIdAsync("WG01", "ACC1")
                .Returns(ApiResponseDto<BidDto>.SuccessResponse(dto));
            _budgetBidsService.GetAccountCategoriesAsync()
                .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(new List<AccountCategoryDto>()));

            // Act
            var result = await _controller.EditBudgetBid("WG01", "ACC1");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditBudgetResourceLevel", partialView.ViewName);
        }

        [Fact]
        public async Task EditBudgetBid_Get_WhenBidNotFound_ReturnsJsonError()
        {
            // Arrange
            _budgetBidsService.GetBidByIdAsync("WG01", "NOTEXIST")
                .Returns(ApiResponseDto<BidDto>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Not found" } }, new ApiMetaDto()));

            // Act
            var result = await _controller.EditBudgetBid("WG01", "NOTEXIST");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task EditBudgetBid_Post_WithValidModel_ReturnsSuccessJson()
        {
            // Arrange
            var model = new BudgetResourceCentreLevelItem { WorkGroupName = "WG01", Account = "ACC1", GenBid = 200m };
            var dto   = new BidDto { WorkGroupName = "WG01", Account = "ACC1", GenBid = 200m };
            _budgetBidsService.UpdateBidAsync(Arg.Any<BidDto>())
                .Returns(ApiResponseDto<BidDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.EditBudgetBid(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Budget bid updated successfully.", value.message);
        }

        [Fact]
        public async Task EditBudgetBid_Post_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var model = new BudgetResourceCentreLevelItem();
            _controller.ModelState.AddModelError("Account", "Account is required");

            // Act
            var result = await _controller.EditBudgetBid(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task EditBudgetBid_Post_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var model = new BudgetResourceCentreLevelItem { WorkGroupName = "WG01", Account = "ACC1", GenBid = 200m };
            _budgetBidsService.UpdateBidAsync(Arg.Any<BidDto>())
                .Returns(ApiResponseDto<BidDto>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Failed to update bid.", Code = "ERR" } },
                    new ApiMetaDto()));

            // Act
            var result = await _controller.EditBudgetBid(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Failed to update bid.", value.message);
        }

        #endregion

        #region DeleteBudgetBid Tests

        [Fact]
        public async Task DeleteBudgetBid_WithExistingBid_ReturnsSuccessJson()
        {
            // Arrange
            _budgetBidsService.DeleteBidAsync(Arg.Any<BidDto>())
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeleteBudgetBid("WG01", "ACC1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Budget bid deleted successfully.", value.message);
        }

        [Fact]
        public async Task DeleteBudgetBid_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            _budgetBidsService.DeleteBidAsync(Arg.Any<BidDto>())
                .Returns(ApiResponseDto<bool>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Failed to delete budget bid.", Code = "ERR" } },
                    new ApiMetaDto()));

            // Act
            var result = await _controller.DeleteBudgetBid("WG01", "ACC1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task DeleteBudgetBid_WhenRelatedPurchasesExist_ReturnsValidationMessage()
        {
            // Arrange
            const string validationMessage = "This record cannot be deleted as it has a related entry in the Purchase table.";
            _budgetBidsService.DeleteBidAsync(Arg.Any<BidDto>())
                .Returns(ApiResponseDto<bool>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = validationMessage, Code = "BUSINESS_RULE_VIOLATION" } },
                    new ApiMetaDto()));

            // Act
            var result = await _controller.DeleteBudgetBid("WG01", "ACC1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal(validationMessage, value.message);
        }

        #endregion

        #region CreatePurchase Tests

        [Fact]
        public void CreatePurchase_Get_ReturnsPartialViewWithModel()
        {
            // Act
            var result = _controller.CreatePurchase("WG01", "ACC1");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditBudgetResourceLevel", partialView.ViewName);
            var model = Assert.IsType<BudgetResourceLevelItem>(partialView.Model);
            Assert.Equal(BudgetResourceLevelModalType.Purchase, model.ModalType);
            Assert.Equal("WG01", model.Purchase!.WorkGroupName);
            Assert.Equal("ACC1", model.Purchase.Account);
        }

        [Fact]
        public async Task CreatePurchase_Post_WithValidModel_ReturnsSuccessJson()
        {
            // Arrange
            var model = new PurchaseItem { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            var dto   = new PurchaseDto { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            _purchasesService.CreatePurchaseAsync(Arg.Any<PurchaseDto>())
                .Returns(ApiResponseDto<PurchaseDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.CreatePurchase(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Purchase created successfully.", value.message);
        }

        [Fact]
        public async Task CreatePurchase_Post_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var model = new PurchaseItem();
            _controller.ModelState.AddModelError("ItemDescription", "Item Description is required");

            // Act
            var result = await _controller.CreatePurchase(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task CreatePurchase_Post_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var model = new PurchaseItem { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            _purchasesService.CreatePurchaseAsync(Arg.Any<PurchaseDto>())
                .Returns(ApiResponseDto<PurchaseDto>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Failed to create purchase.", Code = "ERR" } },
                    new ApiMetaDto()));

            // Act
            var result = await _controller.CreatePurchase(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Failed to create purchase.", value.message);
        }

        #endregion

        #region EditPurchase Tests

        [Fact]
        public async Task EditPurchase_Get_WithExistingPurchase_ReturnsPartialView()
        {
            // Arrange
            var dto = new PurchaseDto { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            _purchasesService.GetPurchaseByIdAsync("WG01", "ACC1", "Item A")
                .Returns(ApiResponseDto<PurchaseDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.EditPurchase("WG01", "ACC1", "Item A");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditBudgetResourceLevel", partialView.ViewName);
        }

        [Fact]
        public async Task EditPurchase_Get_WhenPurchaseNotFound_ReturnsJsonError()
        {
            // Arrange
            _purchasesService.GetPurchaseByIdAsync("WG01", "ACC1", "NOTEXIST")
                .Returns(ApiResponseDto<PurchaseDto>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Not found" } }, new ApiMetaDto()));

            // Act
            var result = await _controller.EditPurchase("WG01", "ACC1", "NOTEXIST");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task EditPurchase_Post_WithValidModel_ReturnsSuccessJson()
        {
            // Arrange
            var model = new PurchaseItem { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", OldItemDescription = "Item A", Amount = 200m };
            var dto   = new PurchaseDto { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", OldItemDescription = "Item A", Amount = 200m };
            PurchaseDto? capturedDto = null;
            _purchasesService.UpdatePurchaseAsync(Arg.Do<PurchaseDto>(d => capturedDto = d))
                .Returns(ApiResponseDto<PurchaseDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.EditPurchase(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Purchase updated successfully.", value.message);
            Assert.NotNull(capturedDto);
            Assert.Equal("Item A", capturedDto!.OldItemDescription);
            Assert.Equal("Item B", capturedDto.ItemDescription);
        }

        [Fact]
        public async Task EditPurchase_Post_PassesOldItemDescription_ToService()
        {
            // Arrange — simulates a rename: original description is "Item A", new value is "Item A Renamed".
            // The service must receive OldItemDescription = "Item A" as the lookup key, NOT the new value.
            var model = new PurchaseItem { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A Renamed", OldItemDescription = "Item A", Amount = 100m };
            PurchaseDto? capturedDto = null;
            _purchasesService.UpdatePurchaseAsync(Arg.Do<PurchaseDto>(d => capturedDto = d))
                .Returns(ApiResponseDto<PurchaseDto>.SuccessResponse(new PurchaseDto()));

            // Act
            await _controller.EditPurchase(model);

            // Assert
            Assert.NotNull(capturedDto);
            Assert.Equal("Item A",         capturedDto!.OldItemDescription);
            Assert.Equal("Item A Renamed", capturedDto.ItemDescription);
        }

        [Fact]
        public async Task EditPurchase_Post_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var model = new PurchaseItem();
            _controller.ModelState.AddModelError("ItemDescription", "Item Description is required");

            // Act
            var result = await _controller.EditPurchase(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task EditPurchase_Post_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var model = new PurchaseItem { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m };
            _purchasesService.UpdatePurchaseAsync(Arg.Any<PurchaseDto>())
                .Returns(ApiResponseDto<PurchaseDto>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Failed to update purchase.", Code = "ERR" } },
                    new ApiMetaDto()));

            // Act
            var result = await _controller.EditPurchase(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        #endregion

        #region DeletePurchase Tests

        [Fact]
        public async Task DeletePurchase_WithExistingPurchase_ReturnsSuccessJson()
        {
            // Arrange
            _purchasesService.DeletePurchaseAsync(Arg.Any<PurchaseDto>())
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeletePurchase("WG01", "ACC1", "Item A");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Purchase deleted successfully.", value.message);
        }

        [Fact]
        public async Task DeletePurchase_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            _purchasesService.DeletePurchaseAsync(Arg.Any<PurchaseDto>())
                .Returns(ApiResponseDto<bool>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Failed to delete purchase.", Code = "ERR" } },
                    new ApiMetaDto()));

            // Act
            var result = await _controller.DeletePurchase("WG01", "ACC1", "Item A");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        #endregion

        #region ExportToExcel Tests

        [Fact]
        public async Task ExportToExcel_WithWorkgroupsAndBids_ReturnsExcelFile()
        {
            // Arrange
            SetupDefaultWorkGroups("PC01");
            SetupDefaultBidView("WG01");   // also stubs GetAccountCategoriesAsync

            // Act
            var result = await _controller.ExportToExcel("PC01", 2024);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
            Assert.Equal("qryBidxCrosstab_2024.xlsx", fileResult.FileDownloadName);
            Assert.NotEmpty(fileResult.FileContents);
        }

        [Fact]
        public async Task ExportToExcel_WhenWorkgroupServiceFails_ReturnsEmptyExcel()
        {
            // Arrange
            _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync("PC01")
                .Returns(ApiResponseDto<List<WorkGroupViewDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Error" } }, new ApiMetaDto()));
            _budgetBidsService.GetAccountCategoriesAsync()
                .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(new List<AccountCategoryDto>()));

            // Act
            var result = await _controller.ExportToExcel("PC01", 2024);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.NotEmpty(fileResult.FileContents);
        }

        [Fact]
        public async Task ExportToExcel_WhenNoBids_ReturnsExcelWithHeadersOnly()
        {
            // Arrange
            SetupDefaultWorkGroups("PC01");
            _budgetBidsService.GetBidViewAsync("WG01")
                .Returns(ApiResponseDto<List<BidViewDto>>.SuccessResponse(new List<BidViewDto>()));
            _budgetBidsService.GetAccountCategoriesAsync()
                .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(new List<AccountCategoryDto>()));

            // Act
            var result = await _controller.ExportToExcel("PC01", 2024);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.NotEmpty(fileResult.FileContents);
        }

        #endregion
    }

    // Local helper record for deserialising JsonResult values
    internal record JsonResponse(bool success, string message);
}
