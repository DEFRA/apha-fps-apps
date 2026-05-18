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

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.AutomaticMonthlyInvoiceControllerTest
{
    public class AutomaticMonthlyInvoiceControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectInvoiceService _invoiceService;
        private readonly IProjectService _projectService;
        private readonly IMonthService _monthService;
        private readonly AutomaticMonthlyInvoiceController _controller;

        public AutomaticMonthlyInvoiceControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _invoiceService = Substitute.For<IProjectInvoiceService>();
            _projectService = Substitute.For<IProjectService>();
            _monthService = Substitute.For<IMonthService>();
            _controller = new AutomaticMonthlyInvoiceController(
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
            _mapper.Map<List<AutomaticInvoiceItem>>(Arg.Any<List<ProjectInvoiceDto>>())
                .Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
        }

        private void SetupProjectsList(List<ProjectDto> projects)
        {
            _projectService.GetAllPactProjectsAsync()
                .Returns(Task.FromResult(ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects)));
        }

        private void SetupMonthsList(List<MonthDto> months)
        {
            _monthService.GetAllMonthsAsync()
                .Returns(Task.FromResult(ApiResponseDto<List<MonthDto>>.SuccessResponse(months)));
        }

        #region Index Tests

        [Fact]
        public async Task Index_WithMonth_ReturnsViewWithFilteredViewModel()
        {
            // Arrange
            const int month = 6;
            var months = new List<MonthDto>
            {
                new() { Monthnumber = 6, Monthname = "June" }
            };
            var invoices = new List<ProjectInvoiceDto>
            {
                new() { ProjectParent = "PRJ001", Month = month, Amount = 1000m }
            };

            _invoiceService.GetPagedProjectInvoiceManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(invoices, new PaginationDto()));
            SetupMonthsList(months);
            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.Index(month);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AutomaticMonthlyInvoiceViewModel>(viewResult.Model);
            Assert.Equal(month, model.SelectedMonth);
            Assert.NotNull(model.InvoicesGrid);
            Assert.NotNull(model.Months);
        }

        [Fact]
        public async Task Index_WithoutMonth_ReturnsViewWithEmptyGrid()
        {
            // Arrange
            var months = new List<MonthDto>
            {
                new() { Monthnumber = 1, Monthname = "January" }
            };

            SetupMonthsList(months);
            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AutomaticMonthlyInvoiceViewModel>(viewResult.Model);
            Assert.Null(model.SelectedMonth);
            Assert.NotNull(model.InvoicesGrid);
            Assert.Empty(model.InvoicesGrid.Data);
        }

        [Fact]
        public async Task Index_PopulatesMonthsList_InDescendingOrder()
        {
            // Arrange
            var months = new List<MonthDto>
            {
                new() { Monthnumber = 12, Monthname = "December" },
                new() { Monthnumber = 1, Monthname = "January" },
                new() { Monthnumber = 6, Monthname = "June" }
            };

            SetupMonthsList(months);
            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AutomaticMonthlyInvoiceViewModel>(viewResult.Model);
            Assert.NotEmpty(model.Months);
            Assert.Equal("1", model.Months[0].Value);
            Assert.Equal("12", model.Months[^1].Value);
        }

        [Fact]
        public async Task Index_MonthServiceReturnsNull_ReturnsViewWithEmptyMonthsList()
        {
            // Arrange
            _monthService.GetAllMonthsAsync()
                .Returns(Task.FromResult<ApiResponseDto<List<MonthDto>>>(null!));
            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AutomaticMonthlyInvoiceViewModel>(viewResult.Model);
            Assert.Empty(model.Months);
        }

        #endregion

        #region LoadInvoicesGrid Tests

        [Fact]
        public async Task LoadInvoicesGrid_ValidRequestWithMonth_ReturnsPartialViewWithGridConfig()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 50, Filter = "{}" };
            const string month = "6";

            _invoiceService.GetPagedProjectInvoiceManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.LoadInvoicesGrid(request, month);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<AutomaticInvoiceItem>>(partial.Model);
        }

        [Fact]
        public async Task LoadInvoicesGrid_WithEmptyMonth_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 50, Filter = "{}" };

            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.LoadInvoicesGrid(request, "");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<AutomaticInvoiceItem>>(partial.Model);
            Assert.Empty(model.Data);
            await _invoiceService.DidNotReceive().GetPagedProjectInvoiceManualAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>());
        }

        [Fact]
        public async Task LoadInvoicesGrid_WithNullMonth_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 50, Filter = "{}" };

            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.LoadInvoicesGrid(request, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<AutomaticInvoiceItem>>(partial.Model);
            Assert.Empty(model.Data);
        }

        [Fact]
        public async Task LoadInvoicesGrid_WithInvalidMonthString_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 50, Filter = "{}" };

            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.LoadInvoicesGrid(request, "invalid");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<AutomaticInvoiceItem>>(partial.Model);
            Assert.Empty(model.Data);
        }

        [Fact]
        public async Task LoadInvoicesGrid_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "Validation error");

            // Act
            var result = await _controller.LoadInvoicesGrid(new PaginationFilter<string>(), null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadInvoicesGrid_MergesMonthIntoFilter_CorrectFormat()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            const string month = "7";

            _invoiceService.GetPagedProjectInvoiceManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.LoadInvoicesGrid(request, month);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            Assert.Contains("\"Month\":\"7\"", request.Filter);
        }

        #endregion

        #region GetInvoice Tests

        [Fact]
        public async Task GetInvoice_IdIsZero_ReturnsPartialViewWithNewInvoice()
        {
            // Arrange
            int? selectedMonth = 6;
            SetupProjectsList([]);
            SetupMonthsList([]);

            // Act
            var result = await _controller.GetInvoice(0, selectedMonth);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditAutomaticInvoice", partial.ViewName);
            var model = Assert.IsType<AutomaticInvoiceItem>(partial.Model);
            Assert.Equal(selectedMonth, model.Month);
            Assert.Equal(0, model.InvoiceCounter);
        }

        [Fact]
        public async Task GetInvoice_IdIsZero_WithNullMonth_ReturnsPartialViewWithNullMonth()
        {
            // Arrange
            SetupProjectsList([]);
            SetupMonthsList([]);

            // Act
            var result = await _controller.GetInvoice(0, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<AutomaticInvoiceItem>(partial.Model);
            Assert.Null(model.Month);
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
            var viewModel = new AutomaticInvoiceItem
            {
                InvoiceCounter = invoiceId,
                ProjectParent = "PRJ001",
                Month = 4,
                Amount = 2500m
            };

            _invoiceService.GetByIdAsync(invoiceId)
                .Returns(ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(dto));
            _mapper.Map<AutomaticInvoiceItem>(dto).Returns(viewModel);
            SetupProjectsList([]);
            SetupMonthsList([]);

            // Act
            var result = await _controller.GetInvoice(invoiceId, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<AutomaticInvoiceItem>(partial.Model);
            Assert.Equal(invoiceId, model.InvoiceCounter);
            Assert.Equal("PRJ001", model.ProjectParent);
        }

        [Fact]
        public async Task GetInvoice_IdIsNonZero_RecordNotFound_ReturnsNotFound()
        {
            // Arrange
            const int invoiceId = 999;

            _invoiceService.GetByIdAsync(invoiceId)
                .Returns(ApiResponseDto<ProjectInvoiceDto>.FailureResponse([], new ApiMetaDto()));
            SetupProjectsList([]);
            SetupMonthsList([]);

            // Act
            var result = await _controller.GetInvoice(invoiceId, null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetInvoice_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "Validation error");

            // Act
            var result = await _controller.GetInvoice(0, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetInvoice_PopulatesViewBag_WithProjectsAndMonths()
        {
            // Arrange
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PRJ001" }
            };
            var months = new List<MonthDto>
            {
                new() { Monthnumber = 1, Monthname = "January" }
            };

            SetupProjectsList(projects);
            SetupMonthsList(months);

            // Act
            var result = await _controller.GetInvoice(0, null);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            Assert.NotNull(_controller.ViewBag.Projects);
            Assert.NotNull(_controller.ViewBag.Months);
        }

        #endregion

        #region SaveInvoice Tests

        [Fact]
        public async Task SaveInvoice_ValidNewInvoice_ReturnsSuccessJson()
        {
            // Arrange
            var model = new AutomaticInvoiceItem
            {
                InvoiceCounter = 0,
                ProjectParent = "PRJ001",
                Month = 6,
                Amount = 1000m
            };
            var dto = new ProjectInvoiceDto
            {
                ProjectParent = "PRJ001",
                Month = 6,
                Amount = 1000m
            };

            _mapper.Map<ProjectInvoiceDto>(model).Returns(dto);
            _invoiceService.CreateAsync(dto)
                .Returns(ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.SaveInvoice(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.True(jsonElement.GetProperty("success").GetBoolean());
            Assert.Contains("saved successfully", jsonElement.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SaveInvoice_ValidExistingInvoice_ReturnsSuccessJson()
        {
            // Arrange
            var model = new AutomaticInvoiceItem
            {
                InvoiceCounter = 5,
                ProjectParent = "PRJ001",
                Month = 6,
                Amount = 1500m
            };
            var dto = new ProjectInvoiceDto
            {
                InvoiceCounter = 5,
                ProjectParent = "PRJ001",
                Month = 6,
                Amount = 1500m
            };

            _mapper.Map<ProjectInvoiceDto>(model).Returns(dto);
            _invoiceService.UpdateAsync(5, dto)
                .Returns(ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.SaveInvoice(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.True(jsonElement.GetProperty("success").GetBoolean());
            Assert.Contains("updated successfully", jsonElement.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SaveInvoice_InvalidModelState_ReturnsValidationErrors()
        {
            // Arrange
            var model = new AutomaticInvoiceItem();
            _controller.ModelState.AddModelError("ProjectParent", "Project is required");
            _controller.ModelState.AddModelError("Month", "Month is required");

            // Act
            var result = await _controller.SaveInvoice(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
            Assert.True(jsonElement.GetProperty("errors").GetArrayLength() > 0);
        }

        [Fact]
        public async Task SaveInvoice_ServiceReturnsFailure_ReturnsErrorJson()
        {
            // Arrange
            var model = new AutomaticInvoiceItem
            {
                InvoiceCounter = 0,
                ProjectParent = "PRJ001",
                Month = 6,
                Amount = 1000m
            };
            var dto = new ProjectInvoiceDto();
            var errors = new List<ApiErrorDto>
            {
                new() { Code = "DUPLICATE", Message = "Invoice already exists" }
            };

            _mapper.Map<ProjectInvoiceDto>(model).Returns(dto);
            _invoiceService.CreateAsync(dto)
                .Returns(ApiResponseDto<ProjectInvoiceDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.SaveInvoice(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
            Assert.Contains("Failed to save", jsonElement.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SaveInvoice_NullAmount_ReturnsValidationError()
        {
            // Arrange
            var model = new AutomaticInvoiceItem
            {
                InvoiceCounter = 0,
                ProjectParent = "PRJ001",
                Month = 6,
                Amount = null
            };
            _controller.ModelState.AddModelError("Amount", "Amount is required");

            // Act
            var result = await _controller.SaveInvoice(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
        }

        #endregion

        #region DeleteInvoice Tests

        [Fact]
        public async Task DeleteInvoice_ValidId_ReturnsSuccessJson()
        {
            // Arrange
            const int invoiceId = 5;
            _invoiceService.DeleteAsync(invoiceId)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeleteInvoice(invoiceId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.True(jsonElement.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteInvoice_ServiceReturnsFailure_ReturnsErrorJson()
        {
            // Arrange
            const int invoiceId = 999;
            _invoiceService.DeleteAsync(invoiceId)
                .Returns(ApiResponseDto<bool>.FailureResponse([], new ApiMetaDto()));

            // Act
            var result = await _controller.DeleteInvoice(invoiceId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
            Assert.Contains("Failed to delete", jsonElement.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task DeleteInvoice_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "Validation error");

            // Act
            var result = await _controller.DeleteInvoice(5);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region CopyInvoices Tests

        [Fact]
        public async Task CopyInvoices_ValidMonths_ReturnsSuccessWithCopiedCount()
        {
            // Arrange
            const int sourceMonth = 5;
            const int targetMonth = 6;
            var sourceInvoices = new List<ProjectInvoiceDto>
            {
                new() { ProjectParent = "PRJ001", Month = sourceMonth, Amount = 1000m },
                new() { ProjectParent = "PRJ002", Month = sourceMonth, Amount = 2000m }
            };

            _invoiceService.GetPagedProjectInvoiceManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(
                    ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(sourceInvoices, new PaginationDto()),
                    ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            _invoiceService.CreateAsync(Arg.Any<ProjectInvoiceDto>())
                .Returns(ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(new ProjectInvoiceDto()));

            // Act
            var result = await _controller.CopyInvoices(sourceMonth, targetMonth);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.True(jsonElement.GetProperty("success").GetBoolean());
            Assert.Contains("2 invoices", jsonElement.GetProperty("message").GetString());
        }

        [Fact]
        public async Task CopyInvoices_SourceMonthZero_ReturnsInvalidMonthError()
        {
            // Arrange
            const int sourceMonth = 0;
            const int targetMonth = 6;

            // Act
            var result = await _controller.CopyInvoices(sourceMonth, targetMonth);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
            Assert.Contains("Invalid month", jsonElement.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CopyInvoices_TargetMonthNegative_ReturnsInvalidMonthError()
        {
            // Arrange
            const int sourceMonth = 5;
            const int targetMonth = -1;

            // Act
            var result = await _controller.CopyInvoices(sourceMonth, targetMonth);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
            Assert.Contains("Invalid month", jsonElement.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CopyInvoices_SameSourceAndTarget_ReturnsDifferentMonthsError()
        {
            // Arrange
            const int sourceMonth = 6;
            const int targetMonth = 6;

            // Act
            var result = await _controller.CopyInvoices(sourceMonth, targetMonth);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
            Assert.Contains("must be different", jsonElement.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CopyInvoices_NoInvoicesInSourceMonth_ReturnsNoInvoicesFoundError()
        {
            // Arrange
            const int sourceMonth = 5;
            const int targetMonth = 6;

            _invoiceService.GetPagedProjectInvoiceManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));

            // Act
            var result = await _controller.CopyInvoices(sourceMonth, targetMonth);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
            Assert.Contains("No invoices found", jsonElement.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CopyInvoices_TargetMonthHasExistingInvoices_ReturnsAlreadyExistsError()
        {
            // Arrange
            const int sourceMonth = 5;
            const int targetMonth = 6;
            var sourceInvoices = new List<ProjectInvoiceDto>
            {
                new() { ProjectParent = "PRJ001", Month = sourceMonth, Amount = 1000m }
            };
            var targetInvoices = new List<ProjectInvoiceDto>
            {
                new() { ProjectParent = "PRJ001", Month = targetMonth, Amount = 1000m }
            };

            _invoiceService.GetPagedProjectInvoiceManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(
                    ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(sourceInvoices, new PaginationDto()),
                    ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(targetInvoices, new PaginationDto()));

            // Act
            var result = await _controller.CopyInvoices(sourceMonth, targetMonth);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
            Assert.Contains("already contains invoices", jsonElement.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CopyInvoices_SomeInvoicesFailToCopy_ReturnsPartialSuccessWithErrors()
        {
            // Arrange
            const int sourceMonth = 5;
            const int targetMonth = 6;
            var sourceInvoices = new List<ProjectInvoiceDto>
            {
                new() { ProjectParent = "PRJ001", Month = sourceMonth, Amount = 1000m },
                new() { ProjectParent = "PRJ002", Month = sourceMonth, Amount = 2000m }
            };

            _invoiceService.GetPagedProjectInvoiceManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(
                    ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(sourceInvoices, new PaginationDto()),
                    ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));

            _invoiceService.CreateAsync(Arg.Is<ProjectInvoiceDto>(x => x.ProjectParent == "PRJ001"))
                .Returns(ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(new ProjectInvoiceDto()));
            _invoiceService.CreateAsync(Arg.Is<ProjectInvoiceDto>(x => x.ProjectParent == "PRJ002"))
                .Returns(ApiResponseDto<ProjectInvoiceDto>.FailureResponse([], new ApiMetaDto()));

            // Act
            var result = await _controller.CopyInvoices(sourceMonth, targetMonth);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
            Assert.Contains("Copied 1 invoices with 1 errors", jsonElement.GetProperty("message").GetString());
        }

        [Fact]
        public async Task CopyInvoices_ExceptionThrown_ReturnsCatchAllError()
        {
            // Arrange
            const int sourceMonth = 5;
            const int targetMonth = 6;

            _invoiceService.GetPagedProjectInvoiceManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns<ApiResponseDto<List<ProjectInvoiceDto>>>(x => throw new Exception("Database connection failed"));

            // Act
            var result = await _controller.CopyInvoices(sourceMonth, targetMonth);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
            Assert.Contains("An error occurred", jsonElement.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CopyInvoices_CopiesCorrectMonthValue_InNewInvoices()
        {
            // Arrange
            const int sourceMonth = 5;
            const int targetMonth = 6;
            var sourceInvoices = new List<ProjectInvoiceDto>
            {
                new() { ProjectParent = "PRJ001", Month = sourceMonth, Amount = 1000m }
            };
            ProjectInvoiceDto? capturedInvoice = null;

            _invoiceService.GetPagedProjectInvoiceManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(
                    ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(sourceInvoices, new PaginationDto()),
                    ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            _invoiceService.CreateAsync(Arg.Do<ProjectInvoiceDto>(x => capturedInvoice = x))
                .Returns(ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(new ProjectInvoiceDto()));

            // Act
            await _controller.CopyInvoices(sourceMonth, targetMonth);

            // Assert
            Assert.NotNull(capturedInvoice);
            Assert.Equal(targetMonth, capturedInvoice.Month);
            Assert.Equal("PRJ001", capturedInvoice.ProjectParent);
            Assert.Equal(1000m, capturedInvoice.Amount);
        }

        #endregion
    }
}
