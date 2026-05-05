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
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.InvoiceControllerTest
{
    public class InvoiceControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectInvoiceService _invoiceService;
        private readonly IProjectService _projectService;
        private readonly IMonthService _monthService;
        private readonly InvoiceController _controller;

        public InvoiceControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _invoiceService = Substitute.For<IProjectInvoiceService>();
            _projectService = Substitute.For<IProjectService>();
            _monthService = Substitute.For<IMonthService>();
            _controller = new InvoiceController(
                _mapper,
                _invoiceService,
                _projectService,
                _monthService);
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

        private void SetupProjectsList(List<ProjectDto> projects)
        {
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects));
        }

        private void SetupMonthsList(List<MonthDto> months)
        {
            _monthService.GetAllMonthsAsync()
                .Returns(ApiResponseDto<List<MonthDto>>.SuccessResponse(months));
        }

        #region Index

        [Fact]
        public async Task Index_WithParentProjectAndMonth_ReturnsViewWithFilteredViewModel()
        {
            // Arrange
            const string parentProject = "PRJ001";
            const int month = 6;
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PRJ001" },
                new() { ParentProject = "PRJ002" }
            };
            var invoices = new List<ProjectInvoiceDto>
            {
                new() { ProjectParent = parentProject, Month = month, Amount = 1000m }
            };
            var invoiceItems = new List<ProjectInvoiceItem>
            {
                new() { ProjectParent = parentProject, Month = month, Amount = 1000m }
            };

            _invoiceService.GetPagedProjectInvoiceManualAsync(Arg.Any<QueryParameters<string>>(), parentProject)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(invoices, new PaginationDto()));
            SetupProjectsList(projects);
            SetupMonthsList([]);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ProjectInvoiceItem>>(invoices)
                .Returns(invoiceItems);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _controller.Index(parentProject, month);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<InvoiceViewModel>(viewResult.Model);
            Assert.Equal(parentProject, model.ParentProject);
            Assert.Equal(month, model.Month);
            Assert.NotNull(model.FilterProjects);
        }

        [Fact]
        public async Task Index_WithNullParameters_ReturnsViewWithEmptyFilters()
        {
            // Arrange
            _invoiceService.GetPagedProjectInvoiceManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            SetupProjectsList([]);
            SetupMonthsList([]);
            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<InvoiceViewModel>(viewResult.Model);
            Assert.Equal(string.Empty, model.ParentProject);
            Assert.Null(model.Month);
        }

        [Fact]
        public async Task Index_PopulatesProjectsList_InViewBagAndModel()
        {
            // Arrange
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PRJ001" },
                new() { ParentProject = "PRJ002" },
                new() { ParentProject = "PRJ003" }
            };

            _invoiceService.GetPagedProjectInvoiceManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            SetupProjectsList(projects);
            SetupMonthsList([]);
            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<InvoiceViewModel>(viewResult.Model);
            Assert.NotNull(_controller.ViewBag.Projects);
            Assert.NotNull(_controller.ViewBag.FilterProjects);
            Assert.NotEmpty(model.FilterProjects);
        }

        #endregion

        #region GetInvoiceTotalAmount

        #endregion

        #region LoadInvoicesGrid

        [Fact]
        public async Task LoadInvoicesGrid_ValidRequest_ReturnsPartialViewWithGridConfig()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            const string parentProject = "PRJ001";

            _invoiceService.GetPagedProjectInvoiceManualAsync(Arg.Any<QueryParameters<string>>(), parentProject)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.LoadInvoicesGrid(request, parentProject, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<ProjectInvoiceItem>>(partial.Model);
        }

        [Fact]
        public async Task LoadInvoicesGrid_WithMonthFilter_MergesMonthIntoFilter()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            const int month = 7;

            _invoiceService.GetPagedProjectInvoiceManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.LoadInvoicesGrid(request, null, month);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            Assert.Contains("\"Month\":\"7\"", request.Filter);
        }

        [Fact]
        public async Task LoadInvoicesGrid_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "Validation error");

            // Act
            var result = await _controller.LoadInvoicesGrid(new PaginationFilter<string>(), null, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadInvoicesGrid_WithExistingFilter_PreservesExistingFilterData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{\"ProjectParent\":\"PRJ001\"}" };
            const int month = 3;

            _invoiceService.GetPagedProjectInvoiceManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.LoadInvoicesGrid(request, null, month);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            Assert.Contains("ProjectParent", request.Filter);
            Assert.Contains("Month", request.Filter);
        }

        #endregion

        #region GetInvoice

        [Fact]
        public async Task GetInvoice_IdIsZero_ReturnsPartialViewWithNewInvoice()
        {
            // Arrange
            const string parentProject = "PRJ001";
            SetupProjectsList([]);

            // Act
            var result = await _controller.GetInvoice(0, parentProject);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditInvoice", partial.ViewName);
            var model = Assert.IsType<ProjectInvoiceItem>(partial.Model);
            Assert.Equal(parentProject, model.ProjectParent);
            Assert.Equal(0, model.InvoiceCounter);
        }

        [Fact]
        public async Task GetInvoice_IdIsZero_WithNullProject_ReturnsPartialViewWithEmptyProject()
        {
            // Arrange
            SetupProjectsList([]);

            // Act
            var result = await _controller.GetInvoice(0, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<ProjectInvoiceItem>(partial.Model);
            Assert.Equal(string.Empty, model.ProjectParent);
        }

        [Fact]
        public async Task GetInvoice_IdIsNonZero_RecordExists_ReturnsPartialViewWithMappedInvoice()
        {
            // Arrange
            const int invoiceId = 5;
            var dto = new ProjectInvoiceDto
            {
                InvoiceCounter = invoiceId,
                ProjectParent = "PRJ001",
                Month = 4,
                Amount = 2500m
            };
            var viewModel = new ProjectInvoiceItem
            {
                InvoiceCounter = invoiceId,
                ProjectParent = "PRJ001",
                Month = 4,
                Amount = 2500m
            };

            _invoiceService.GetByIdAsync(invoiceId)
                .Returns(ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(dto));
            _mapper.Map<ProjectInvoiceItem>(dto).Returns(viewModel);
            SetupProjectsList([]);

            // Act
            var result = await _controller.GetInvoice(invoiceId, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditInvoice", partial.ViewName);
            var model = Assert.IsType<ProjectInvoiceItem>(partial.Model);
            Assert.Equal(invoiceId, model.InvoiceCounter);
            Assert.Equal("PRJ001", model.ProjectParent);
        }

        [Fact]
        public async Task GetInvoice_IdIsNonZero_ServiceReturnsFailure_ReturnsNotFound()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Invoice not found" } };
            _invoiceService.GetByIdAsync(99)
                .Returns(ApiResponseDto<ProjectInvoiceDto>.FailureResponse(errors, new ApiMetaDto()));
            SetupProjectsList([]);

            // Act
            var result = await _controller.GetInvoice(99, null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetInvoice_IdIsNonZero_DataIsNull_ReturnsNotFound()
        {
            // Arrange
            _invoiceService.GetByIdAsync(10)
                .Returns(new ApiResponseDto<ProjectInvoiceDto> { Success = true, Data = null });
            SetupProjectsList([]);

            // Act
            var result = await _controller.GetInvoice(10, null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetInvoice_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Id", "Invalid ID");

            // Act
            var result = await _controller.GetInvoice(0, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region SaveInvoice

        [Fact]
        public async Task SaveInvoice_CounterIsZero_CallsCreateService_ReturnsSuccessJson()
        {
            // Arrange
            var model = new ProjectInvoiceItem
            {
                InvoiceCounter = 0,
                ProjectParent = "PRJ001",
                Month = 6,
                Amount = 1500m,
                CostOfWork = 1200m,
                Wip = 300m,
                ProfitLoss = 0m
            };
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
            Assert.Equal("Invoice saved successfully.", value.GetProperty("message").GetString());
            await _invoiceService.Received(1).CreateAsync(dto);
            await _invoiceService.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<ProjectInvoiceDto>());
        }

        [Fact]
        public async Task SaveInvoice_CounterIsNonZero_CallsUpdateService_ReturnsSuccessJson()
        {
            // Arrange
            var model = new ProjectInvoiceItem
            {
                InvoiceCounter = 10,
                ProjectParent = "PRJ002",
                Month = 8,
                Amount = 3000m
            };
            var dto = new ProjectInvoiceDto { InvoiceCounter = 10 };

            _mapper.Map<ProjectInvoiceDto>(model).Returns(dto);
            _invoiceService.UpdateAsync(10, dto)
                .Returns(ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.SaveInvoice(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("Invoice updated successfully.", value.GetProperty("message").GetString());
            await _invoiceService.Received(1).UpdateAsync(10, dto);
            await _invoiceService.DidNotReceive().CreateAsync(Arg.Any<ProjectInvoiceDto>());
        }

        [Fact]
        public async Task SaveInvoice_ServiceReturnsFailure_ReturnsFailureJson()
        {
            // Arrange
            var model = new ProjectInvoiceItem { InvoiceCounter = 0, ProjectParent = "PRJ001", Month = 1 };
            var dto = new ProjectInvoiceDto();
            var errors = new List<ApiErrorDto>
            {
                new() { Code = "ERR001", Message = "Failed to save invoice" }
            };

            _mapper.Map<ProjectInvoiceDto>(model).Returns(dto);
            _invoiceService.CreateAsync(dto)
                .Returns(ApiResponseDto<ProjectInvoiceDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.SaveInvoice(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to save invoice.", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task SaveInvoice_InvalidModelState_ReturnsValidationErrorJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("Month", "Month is required");
            _controller.ModelState.AddModelError("Amount", "Amount must be positive");

            // Act
            var result = await _controller.SaveInvoice(new ProjectInvoiceItem());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Please correct the errors below.", value.GetProperty("message").GetString());
            Assert.True(value.GetProperty("errors").GetArrayLength() > 0);
        }

        [Fact]
        public async Task SaveInvoice_ModelStateErrorWithDollarPrefix_FiltersOutDollarKeys()
        {
            // Arrange
            _controller.ModelState.AddModelError("$", "Should be ignored");
            _controller.ModelState.AddModelError("Month", "Month is required");

            // Act
            var result = await _controller.SaveInvoice(new ProjectInvoiceItem());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            var errors = value.GetProperty("errors").EnumerateArray().ToList();
            Assert.Single(errors);
            Assert.Equal("Month", errors[0].GetProperty("field").GetString());
        }

        #endregion

        #region DeleteInvoice

        [Fact]
        public async Task DeleteInvoice_ServiceSucceeds_ReturnsSuccessJson()
        {
            // Arrange
            const int invoiceId = 15;
            _invoiceService.DeleteAsync(invoiceId)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeleteInvoice(invoiceId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            await _invoiceService.Received(1).DeleteAsync(invoiceId);
        }

        [Fact]
        public async Task DeleteInvoice_ServiceFails_ReturnsFailureJson()
        {
            // Arrange
            const int invoiceId = 20;
            var errors = new List<ApiErrorDto> { new() { Message = "Cannot delete invoice" } };
            _invoiceService.DeleteAsync(invoiceId)
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.DeleteInvoice(invoiceId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to delete invoice.", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task DeleteInvoice_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Id", "Invalid ID");

            // Act
            var result = await _controller.DeleteInvoice(0);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region GetProjectsListAsync (Private Method Testing via Index)

        [Fact]
        public async Task Index_ProjectServiceReturnsEmpty_ViewModelHasEmptyProjectsList()
        {
            // Arrange
            _invoiceService.GetPagedProjectInvoiceManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            _invoiceService.GetTotalAmountAsync(null)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));
            SetupProjectsList([]);
            SetupMonthsList([]);
            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<InvoiceViewModel>(viewResult.Model);
            Assert.Empty(model.FilterProjects);
        }

        [Fact]
        public async Task Index_ProjectServiceReturnsProjects_ViewModelHasOrderedProjectsList()
        {
            // Arrange
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PRJ003" },
                new() { ParentProject = "PRJ001" },
                new() { ParentProject = "PRJ002" }
            };

            _invoiceService.GetPagedProjectInvoiceManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            _invoiceService.GetTotalAmountAsync(null)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));
            SetupProjectsList(projects);
            SetupMonthsList([]);
            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<InvoiceViewModel>(viewResult.Model);
            Assert.Equal(3, model.FilterProjects.Count);
            Assert.Equal("PRJ001", model.FilterProjects[0].Value);
            Assert.Equal("PRJ002", model.FilterProjects[1].Value);
            Assert.Equal("PRJ003", model.FilterProjects[2].Value);
        }

        [Fact]
        public async Task Index_ProjectServiceFails_ViewModelHasEmptyProjectsList()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Service error" } };
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto()));
            _invoiceService.GetPagedProjectInvoiceManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            _invoiceService.GetTotalAmountAsync(null)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));
            SetupMonthsList([]);
            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<InvoiceViewModel>(viewResult.Model);
            Assert.Empty(model.FilterProjects);
        }

        #endregion
    }
}
