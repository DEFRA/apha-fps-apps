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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.ProjectInvoiceSubContractControllerTest
{
    public class ProjectInvoiceSubContractControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectInvoiceService _invoiceService;
        private readonly IProjectSubContractService _subContractService;
        private readonly IProjectService _projectService;
        private readonly ProjectInvoiceSubContractController _controller;

        public ProjectInvoiceSubContractControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _invoiceService = Substitute.For<IProjectInvoiceService>();
            _subContractService = Substitute.For<IProjectSubContractService>();
            _projectService = Substitute.For<IProjectService>();
            _controller = new ProjectInvoiceSubContractController(
                _mapper,
                _invoiceService,
                _subContractService,
                _projectService);

            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Substitute.For<ITempDataProvider>());
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private void SetupInvoicesGridMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ProjectInvoiceItem>>(Arg.Any<List<ProjectInvoiceDto>>())
                .Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
        }

        private void SetupSubContractsGridMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ProjectSubContractItem>>(Arg.Any<List<ProjectSubContractDto>>())
                .Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
        }

        private void SetupProjectsViewBag()
        {
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse([]));
        }

        #region Index

        [Fact]
        public async Task Index_WithParentProject_ReturnsViewWithViewModel()
        {
            // Arrange
            const string parentProject = "PRJ001";
            _invoiceService.GetPagedProjectInvoicesAsync(Arg.Any<QueryParameters<string>>(), parentProject)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            _subContractService.GetPagedProjectSubContractsAsync(Arg.Any<QueryParameters<string>>(), parentProject)
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse([], new PaginationDto()));
            _invoiceService.GetTotalAmountAsync(parentProject)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(100m));
            _subContractService.GetTotalAmountAsync(parentProject)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(200m));
            SetupInvoicesGridMapper();
            SetupSubContractsGridMapper();

            // Act
            var result = await _controller.Index(parentProject);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectInvoiceSubContractViewModel>(viewResult.Model);
            Assert.Equal(parentProject, model.ParentProject);
        }

        [Fact]
        public async Task Index_WithNullParentProject_ReturnsViewWithEmptyParentProject()
        {
            // Arrange
            _invoiceService.GetPagedProjectInvoicesAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            _subContractService.GetPagedProjectSubContractsAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse([], new PaginationDto()));
            _invoiceService.GetTotalAmountAsync(null)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));
            _subContractService.GetTotalAmountAsync(null)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));
            SetupInvoicesGridMapper();
            SetupSubContractsGridMapper();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectInvoiceSubContractViewModel>(viewResult.Model);
            Assert.Equal(string.Empty, model.ParentProject);
        }

        [Fact]
        public async Task Index_WhenOriginalParentProjectAlreadyInTempData_UsesStoredValueAndFromPortfolioTrue()
        {
            // Arrange
            _invoiceService.GetPagedProjectInvoicesAsync(Arg.Any<QueryParameters<string>>(), "PRJ001")
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            _subContractService.GetPagedProjectSubContractsAsync(Arg.Any<QueryParameters<string>>(), "PRJ001")
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse([], new PaginationDto()));
            _invoiceService.GetTotalAmountAsync("PRJ001")
                .Returns(ApiResponseDto<decimal>.SuccessResponse(10m));
            _subContractService.GetTotalAmountAsync("PRJ001")
                .Returns(ApiResponseDto<decimal>.SuccessResponse(20m));
            SetupInvoicesGridMapper();
            SetupSubContractsGridMapper();

            _controller.TempData["OriginalParentProject"] = "ORIG-PRJ";
            _controller.TempData["NavigationSource"] = "SomePage";
            _controller.TempData["PactOrigin"] = "PortfolioMaintenance";

            // Act
            var result = await _controller.Index("PRJ001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectInvoiceSubContractViewModel>(viewResult.Model);
            Assert.Equal("ORIG-PRJ", model.OriginalParentProject);
            Assert.True(model.FromPortfolio);
        }

        [Fact]
        public async Task Index_WhenFirstVisit_StoresOriginalParentProjectFromNullAsEmpty()
        {
            // Arrange
            _invoiceService.GetPagedProjectInvoicesAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            _subContractService.GetPagedProjectSubContractsAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse([], new PaginationDto()));
            _invoiceService.GetTotalAmountAsync(null)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));
            _subContractService.GetTotalAmountAsync(null)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));
            SetupInvoicesGridMapper();
            SetupSubContractsGridMapper();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectInvoiceSubContractViewModel>(viewResult.Model);
            Assert.Equal(string.Empty, model.OriginalParentProject);
            Assert.False(model.FromPortfolio);
        }

        #endregion

        #region GetInvoiceTotalAmount

        [Fact]
        public async Task GetInvoiceTotalAmount_ValidParentProject_ReturnsJsonWithTotal()
        {
            // Arrange
            _invoiceService.GetTotalAmountAsync("PRJ001")
                .Returns(ApiResponseDto<decimal>.SuccessResponse(500m));

            // Act
            var result = await _controller.GetInvoiceTotalAmount("PRJ001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.Equal(500m, value.GetProperty("total").GetDecimal());
        }

        [Fact]
        public async Task GetInvoiceTotalAmount_NullParentProject_ReturnsJsonWithZeroTotal()
        {
            // Arrange
            _invoiceService.GetTotalAmountAsync(null)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));

            // Act
            var result = await _controller.GetInvoiceTotalAmount(null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.Equal(0m, value.GetProperty("total").GetDecimal());
        }

        #endregion

        #region GetSubContractTotalAmount

        [Fact]
        public async Task GetSubContractTotalAmount_ValidParentProject_ReturnsJsonWithTotal()
        {
            // Arrange
            _subContractService.GetTotalAmountAsync("PRJ001")
                .Returns(ApiResponseDto<decimal>.SuccessResponse(750m));

            // Act
            var result = await _controller.GetSubContractTotalAmount("PRJ001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.Equal(750m, value.GetProperty("total").GetDecimal());
        }

        #endregion

        #region LoadInvoicesGrid

        [Fact]
        public async Task LoadInvoicesGrid_ValidRequest_ReturnsPartialViewWithInvoicesGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _invoiceService.GetPagedProjectInvoicesAsync(Arg.Any<QueryParameters<string>>(), "PRJ001")
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.LoadInvoicesGrid(request, "PRJ001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<ProjectInvoiceItem>>(partial.Model);
        }

        [Fact]
        public async Task LoadInvoicesGrid_WithNullDataAndPaginationAndParentProject_UsesMappedDefaultsAndEscapedBindUrl()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Filter = null,
                SortBy = "Month",
                Descending = true
            };
            var parentProject = "PRJ 001/A";

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _invoiceService.GetPagedProjectInvoicesAsync(Arg.Any<QueryParameters<string>>(), parentProject)
                .Returns(new ApiResponseDto<List<ProjectInvoiceDto>> { Success = true, Data = null, Pagination = null });

            // Act
            var result = await _controller.LoadInvoicesGrid(request, parentProject);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectInvoiceItem>>(partial.Model);
            Assert.Empty(model.Data);
            Assert.Equal("Month", model.Pagination.SortColumn);
            Assert.True(model.Pagination.SortDirection);
            Assert.Contains("parentProject=PRJ%20001%2FA", model.BindGridUrl);
            Assert.NotNull(model.CurrentFilters);
            Assert.Empty(model.CurrentFilters);
        }

        [Fact]
        public async Task LoadInvoicesGrid_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "error");

            // Act
            var result = await _controller.LoadInvoicesGrid(new PaginationFilter<string> { Filter = "{}" }, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region LoadSubContractsGrid

        [Fact]
        public async Task LoadSubContractsGrid_ValidRequest_ReturnsPartialViewWithSubContractsGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _subContractService.GetPagedProjectSubContractsAsync(Arg.Any<QueryParameters<string>>(), "PRJ001")
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse([], new PaginationDto()));
            SetupSubContractsGridMapper();

            // Act
            var result = await _controller.LoadSubContractsGrid(request, "PRJ001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<ProjectSubContractItem>>(partial.Model);
        }

        [Fact]
        public async Task LoadSubContractsGrid_WithNullDataAndPaginationAndNoParentProject_UsesDefaults()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Filter = null,
                SortBy = "Project",
                Descending = false
            };

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _subContractService.GetPagedProjectSubContractsAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(new ApiResponseDto<List<ProjectSubContractDto>> { Success = true, Data = null, Pagination = null });

            // Act
            var result = await _controller.LoadSubContractsGrid(request, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectSubContractItem>>(partial.Model);
            Assert.Empty(model.Data);
            Assert.Equal("Project", model.Pagination.SortColumn);
            Assert.False(model.Pagination.SortDirection);
            Assert.Equal("/PACT/ProjectInvoiceSubContract/LoadSubContractsGrid", model.BindGridUrl);
            Assert.NotNull(model.CurrentFilters);
            Assert.Empty(model.CurrentFilters);
        }

        [Fact]
        public async Task LoadSubContractsGrid_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "error");

            // Act
            var result = await _controller.LoadSubContractsGrid(new PaginationFilter<string> { Filter = "{}" }, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region GetInvoice

        [Fact]
        public async Task GetInvoice_IdIsZero_ReturnsPartialViewWithNewItem()
        {
            // Arrange
            SetupProjectsViewBag();

            // Act
            var result = await _controller.GetInvoice(0, "PRJ001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditInvoice", partial.ViewName);
            var model = Assert.IsType<ProjectInvoiceItem>(partial.Model);
            Assert.Equal("PRJ001", model.ProjectParent);
        }

        [Fact]
        public async Task GetInvoice_WhenProjectsApiFails_SetsEmptyProjectsViewBag()
        {
            // Arrange
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.FailureResponse([new ApiErrorDto { Message = "fail" }], new ApiMetaDto()));

            // Act
            var result = await _controller.GetInvoice(0, "PRJ001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditInvoice", partial.ViewName);
            Assert.NotNull(_controller.ViewBag.Projects);
            var projects = Assert.IsAssignableFrom<List<SelectListItem>>(_controller.ViewBag.Projects);
            Assert.Empty(projects);
        }

        [Fact]
        public async Task GetInvoice_WhenProjectsApiSuccess_ProjectsAreSortedInViewBag()
        {
            // Arrange
            _projectService.GetAllPactProjectsAsync().Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(
            [
                new ProjectDto { ParentProject = "Z2" },
                new ProjectDto { ParentProject = "A1" }
            ]));

            // Act
            var result = await _controller.GetInvoice(0, "PRJ001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditInvoice", partial.ViewName);
            var projects = Assert.IsAssignableFrom<List<SelectListItem>>(_controller.ViewBag.Projects);
            Assert.Equal(2, projects.Count);
            Assert.Equal("A1", projects[0].Value);
            Assert.Equal("Z2", projects[1].Value);
        }

        [Fact]
        public async Task GetInvoice_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "invalid");

            // Act
            var result = await _controller.GetInvoice(0, "PRJ001");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetInvoice_IdIsNonZero_RecordExists_ReturnsPartialViewWithMappedItem()
        {
            // Arrange
            var dto = new ProjectInvoiceDto { InvoiceCounter = 1 };
            var viewModel = new ProjectInvoiceItem { InvoiceCounter = 1, ProjectParent = "PRJ001" };
            _invoiceService.GetByIdAsync(1)
                .Returns(ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(dto));
            _mapper.Map<ProjectInvoiceItem>(dto).Returns(viewModel);
            SetupProjectsViewBag();

            // Act
            var result = await _controller.GetInvoice(1, "PRJ001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditInvoice", partial.ViewName);
            var model = Assert.IsType<ProjectInvoiceItem>(partial.Model);
            Assert.Equal(1, model.InvoiceCounter);
        }

        [Fact]
        public async Task GetInvoice_IdIsNonZero_RecordNotFound_ReturnsNotFound()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found" } };
            _invoiceService.GetByIdAsync(99)
                .Returns(ApiResponseDto<ProjectInvoiceDto>.FailureResponse(errors, new ApiMetaDto()));
            SetupProjectsViewBag();

            // Act
            var result = await _controller.GetInvoice(99, "PRJ001");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetInvoice_IdIsNonZero_DataIsNull_ReturnsNotFound()
        {
            // Arrange
            _invoiceService.GetByIdAsync(1)
                .Returns(new ApiResponseDto<ProjectInvoiceDto> { Success = true, Data = null });
            SetupProjectsViewBag();

            // Act
            var result = await _controller.GetInvoice(1, "PRJ001");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region SaveInvoice

        [Fact]
        public async Task SaveInvoice_CounterIsZero_CallsCreate_ReturnsJsonSuccess()
        {
            // Arrange
            var model = new ProjectInvoiceItem { InvoiceCounter = 0, ProjectParent = "PRJ001", Month = 1, Amount = 100m };
            var dto = new ProjectInvoiceDto { InvoiceCounter = 0 };
            _mapper.Map<ProjectInvoiceDto>(model).Returns(dto);
            _invoiceService.CreateAsync(dto)
                .Returns(ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.SaveInvoice(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            await _invoiceService.Received(1).CreateAsync(dto);
            await _invoiceService.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<ProjectInvoiceDto>());
        }

        [Fact]
        public async Task SaveInvoice_CounterIsNonZero_CallsUpdate_ReturnsJsonSuccess()
        {
            // Arrange
            var model = new ProjectInvoiceItem { InvoiceCounter = 5, ProjectParent = "PRJ001", Month = 2, Amount = 200m };
            var dto = new ProjectInvoiceDto { InvoiceCounter = 5 };
            _mapper.Map<ProjectInvoiceDto>(model).Returns(dto);
            _invoiceService.UpdateAsync(5, dto)
                .Returns(ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.SaveInvoice(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            await _invoiceService.Received(1).UpdateAsync(5, dto);
            await _invoiceService.DidNotReceive().CreateAsync(Arg.Any<ProjectInvoiceDto>());
        }

        [Fact]
        public async Task SaveInvoice_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var model = new ProjectInvoiceItem { InvoiceCounter = 0, ProjectParent = "PRJ001", Month = 1, Amount = 100m };
            var dto = new ProjectInvoiceDto();
            var errors = new List<ApiErrorDto> { new() { Message = "Save failed" } };
            _mapper.Map<ProjectInvoiceDto>(model).Returns(dto);
            _invoiceService.CreateAsync(dto)
                .Returns(ApiResponseDto<ProjectInvoiceDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.SaveInvoice(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveInvoice_InvalidModelState_ReturnsJsonError()
        {
            // Arrange
            _controller.ModelState.AddModelError("Month", "Required");

            // Act
            var result = await _controller.SaveInvoice(new ProjectInvoiceItem());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveInvoice_InvalidModelState_StripsJsonPrefixAndSkipsDollarKeyErrors()
        {
            // Arrange
            _controller.ModelState.AddModelError("$.Month", "Invalid month");
            _controller.ModelState.AddModelError("$", "Top level error should be skipped");

            // Act
            var result = await _controller.SaveInvoice(new ProjectInvoiceItem());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            var errors = value.GetProperty("errors").EnumerateArray().ToList();
            Assert.Single(errors);
            Assert.Equal("Month", errors[0].GetProperty("field").GetString());
            Assert.Equal("Invalid month", errors[0].GetProperty("message").GetString());
        }

        [Fact]
        public async Task SaveInvoice_ServiceFailsWithNullErrorFields_UsesDefaultsInJsonErrors()
        {
            // Arrange
            var model = new ProjectInvoiceItem { InvoiceCounter = 0, ProjectParent = "PRJ001", Month = 1, Amount = 10m };
            var dto = new ProjectInvoiceDto();
            _mapper.Map<ProjectInvoiceDto>(model).Returns(dto);
            _invoiceService.CreateAsync(dto)
                .Returns(ApiResponseDto<ProjectInvoiceDto>.FailureResponse(
                    [new ApiErrorDto { Code = null, Message = null }],
                    new ApiMetaDto()));

            // Act
            var result = await _controller.SaveInvoice(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to create project job code.", value.GetProperty("message").GetString());
            var errors = value.GetProperty("errors").EnumerateArray().ToList();
            Assert.Single(errors);
            Assert.Equal(string.Empty, errors[0].GetProperty("field").GetString());
            Assert.Equal("An unexpected error occurred.", errors[0].GetProperty("message").GetString());
        }

        #endregion

        #region DeleteInvoice

        [Fact]
        public async Task DeleteInvoice_ServiceSucceeds_ReturnsJsonSuccess()
        {
            // Arrange
            _invoiceService.DeleteAsync(1)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeleteInvoice(1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteInvoice_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Delete failed" } };
            _invoiceService.DeleteAsync(1)
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.DeleteInvoice(1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteInvoice_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Id", "invalid");

            // Act
            var result = await _controller.DeleteInvoice(1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region GetSubContract

        [Fact]
        public async Task GetSubContract_IdIsZero_ReturnsPartialViewWithNewItem()
        {
            // Arrange
            SetupProjectsViewBag();

            // Act
            var result = await _controller.GetSubContract(0, "PRJ001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditSubContract", partial.ViewName);
            var model = Assert.IsType<ProjectSubContractItem>(partial.Model);
            Assert.Equal("PRJ001", model.Project);
        }

        [Fact]
        public async Task GetSubContract_WhenProjectsApiSuccessWithNullData_SetsEmptyProjectsViewBag()
        {
            // Arrange
            _projectService.GetAllPactProjectsAsync()
                .Returns(new ApiResponseDto<List<ProjectDto>> { Success = true, Data = null });

            // Act
            var result = await _controller.GetSubContract(0, "PRJ001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditSubContract", partial.ViewName);
            var projects = Assert.IsAssignableFrom<List<SelectListItem>>(_controller.ViewBag.Projects);
            Assert.Empty(projects);
        }

        [Fact]
        public async Task GetSubContract_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "invalid");

            // Act
            var result = await _controller.GetSubContract(0, "PRJ001");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetSubContract_IdIsNonZero_RecordExists_ReturnsPartialViewWithMappedItem()
        {
            // Arrange
            var dto = new ProjectSubContractDto { SubContCounter = 3 };
            var viewModel = new ProjectSubContractItem { SubContCounter = 3, Project = "PRJ001" };
            _subContractService.GetByIdAsync(3)
                .Returns(ApiResponseDto<ProjectSubContractDto>.SuccessResponse(dto));
            _mapper.Map<ProjectSubContractItem>(dto).Returns(viewModel);
            SetupProjectsViewBag();

            // Act
            var result = await _controller.GetSubContract(3, "PRJ001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditSubContract", partial.ViewName);
            var model = Assert.IsType<ProjectSubContractItem>(partial.Model);
            Assert.Equal(3, model.SubContCounter);
        }

        [Fact]
        public async Task GetSubContract_IdIsNonZero_RecordNotFound_ReturnsNotFound()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found" } };
            _subContractService.GetByIdAsync(99)
                .Returns(ApiResponseDto<ProjectSubContractDto>.FailureResponse(errors, new ApiMetaDto()));
            SetupProjectsViewBag();

            // Act
            var result = await _controller.GetSubContract(99, "PRJ001");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetSubContract_IdIsNonZero_DataIsNull_ReturnsNotFound()
        {
            // Arrange
            _subContractService.GetByIdAsync(1)
                .Returns(new ApiResponseDto<ProjectSubContractDto> { Success = true, Data = null });
            SetupProjectsViewBag();

            // Act
            var result = await _controller.GetSubContract(1, "PRJ001");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region SaveSubContract

        [Fact]
        public async Task SaveSubContract_CounterIsZero_CallsCreate_ReturnsJsonSuccess()
        {
            // Arrange
            var model = new ProjectSubContractItem { SubContCounter = 0, Project = "PRJ001", Month = 1, Amount = 300m };
            var dto = new ProjectSubContractDto { SubContCounter = 0 };
            _mapper.Map<ProjectSubContractDto>(model).Returns(dto);
            _subContractService.CreateAsync(dto)
                .Returns(ApiResponseDto<ProjectSubContractDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.SaveSubContract(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            await _subContractService.Received(1).CreateAsync(dto);
            await _subContractService.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<ProjectSubContractDto>());
        }

        [Fact]
        public async Task SaveSubContract_CounterIsNonZero_CallsUpdate_ReturnsJsonSuccess()
        {
            // Arrange
            var model = new ProjectSubContractItem { SubContCounter = 7, Project = "PRJ001", Month = 3, Amount = 400m };
            var dto = new ProjectSubContractDto { SubContCounter = 7 };
            _mapper.Map<ProjectSubContractDto>(model).Returns(dto);
            _subContractService.UpdateAsync(7, dto)
                .Returns(ApiResponseDto<ProjectSubContractDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.SaveSubContract(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            await _subContractService.Received(1).UpdateAsync(7, dto);
            await _subContractService.DidNotReceive().CreateAsync(Arg.Any<ProjectSubContractDto>());
        }

        [Fact]
        public async Task SaveSubContract_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var model = new ProjectSubContractItem { SubContCounter = 0, Project = "PRJ001", Month = 1, Amount = 100m };
            var dto = new ProjectSubContractDto();
            var errors = new List<ApiErrorDto> { new() { Message = "Save failed" } };
            _mapper.Map<ProjectSubContractDto>(model).Returns(dto);
            _subContractService.CreateAsync(dto)
                .Returns(ApiResponseDto<ProjectSubContractDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.SaveSubContract(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveSubContract_InvalidModelState_ReturnsJsonError()
        {
            // Arrange
            _controller.ModelState.AddModelError("Month", "Required");

            // Act
            var result = await _controller.SaveSubContract(new ProjectSubContractItem());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveSubContract_InvalidModelState_StripsJsonPrefixAndSkipsDollarKeyErrors()
        {
            // Arrange
            _controller.ModelState.AddModelError("$.Month", "Invalid month");
            _controller.ModelState.AddModelError("$", "Top level error should be skipped");

            // Act
            var result = await _controller.SaveSubContract(new ProjectSubContractItem());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            var errors = value.GetProperty("errors").EnumerateArray().ToList();
            Assert.Single(errors);
            Assert.Equal("Month", errors[0].GetProperty("field").GetString());
            Assert.Equal("Invalid month", errors[0].GetProperty("message").GetString());
        }

        [Fact]
        public async Task SaveSubContract_ServiceFailsWithNullErrorFields_UsesDefaultsInJsonErrors()
        {
            // Arrange
            var model = new ProjectSubContractItem { SubContCounter = 0, Project = "PRJ001", Month = 1, Amount = 10m };
            var dto = new ProjectSubContractDto();
            _mapper.Map<ProjectSubContractDto>(model).Returns(dto);
            _subContractService.CreateAsync(dto)
                .Returns(ApiResponseDto<ProjectSubContractDto>.FailureResponse(
                    [new ApiErrorDto { Code = null, Message = null }],
                    new ApiMetaDto()));

            // Act
            var result = await _controller.SaveSubContract(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to create project job code.", value.GetProperty("message").GetString());
            var errors = value.GetProperty("errors").EnumerateArray().ToList();
            Assert.Single(errors);
            Assert.Equal(string.Empty, errors[0].GetProperty("field").GetString());
            Assert.Equal("An unexpected error occurred.", errors[0].GetProperty("message").GetString());
        }

        #endregion

        #region DeleteSubContract

        [Fact]
        public async Task DeleteSubContract_ServiceSucceeds_ReturnsJsonSuccess()
        {
            // Arrange
            _subContractService.DeleteAsync(2)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeleteSubContract(2);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteSubContract_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Delete failed" } };
            _subContractService.DeleteAsync(2)
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.DeleteSubContract(2);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteSubContract_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Id", "invalid");

            // Act
            var result = await _controller.DeleteSubContract(2);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion
    }
}
