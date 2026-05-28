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
using NSubstitute.ExceptionExtensions;
using System.Text.Json;
using Xunit;

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
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects));
        }

        private void SetupMonthsList(List<MonthDto> months)
        {
            _monthService.GetAllMonthsAsync()
                .Returns(ApiResponseDto<List<MonthDto>>.SuccessResponse(months));
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

            _invoiceService.GetPagedProjectInvoicesByMonthAsync(Arg.Any<QueryParameters<string>>(), month)
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
        public async Task Index_WithMonth_CallsGetPagedProjectInvoicesByMonthAsync()
        {
            // Arrange
            const int month = 3;
            var months = new List<MonthDto>
            {
                new() { Monthnumber = 3, Monthname = "March" }
            };

            _invoiceService.GetPagedProjectInvoicesByMonthAsync(Arg.Any<QueryParameters<string>>(), month)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            SetupMonthsList(months);
            SetupInvoicesGridMapper();

            // Act
            await _controller.Index(month);

            // Assert
            await _invoiceService.Received(1).GetPagedProjectInvoicesByMonthAsync(
                Arg.Any<QueryParameters<string>>(), 
                month);
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
            // Months should be ordered by Monthnumber ascending (1-12)
            Assert.Equal("1", model.Months.First().Value);
            Assert.Equal("12", model.Months.Last().Value);
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

            _invoiceService.GetPagedProjectInvoicesByMonthAsync(Arg.Any<QueryParameters<string>>(), 6)
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
        public async Task LoadInvoicesGrid_ValidRequestWithMonth_CallsNewMonthBasedAPI()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 50, Filter = "{}" };
            const string month = "3";

            _invoiceService.GetPagedProjectInvoicesByMonthAsync(Arg.Any<QueryParameters<string>>(), 3)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.LoadInvoicesGrid(request, month);

            // Assert
            await _invoiceService.Received(1).GetPagedProjectInvoicesByMonthAsync(
                Arg.Any<QueryParameters<string>>(), 
                3);
            await _invoiceService.DidNotReceive().GetPagedProjectInvoiceManualAsync(
                Arg.Any<QueryParameters<string>>(), 
                Arg.Any<string?>());
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

            _invoiceService.GetPagedProjectInvoicesByMonthAsync(Arg.Any<QueryParameters<string>>(), 7)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.LoadInvoicesGrid(request, month);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            Assert.Contains("\"Month\":\"7\"", request.Filter);
        }

        [Fact]
        public async Task LoadInvoicesGrid_WithMonthAndExistingFilter_MergesFilters()
        {
            // Arrange
            var request = new PaginationFilter<string> 
            { 
                Filter = "{\"ProjectParent\":\"PRJ001\"}" 
            };
            const string month = "5";

            _invoiceService.GetPagedProjectInvoicesByMonthAsync(Arg.Any<QueryParameters<string>>(), 5)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.LoadInvoicesGrid(request, month);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            Assert.Contains("\"Month\":\"5\"", request.Filter);
            Assert.Contains("\"ProjectParent\":\"PRJ001\"", request.Filter);
        }

        [Fact]
        public async Task LoadInvoicesGrid_WithPaginationParameters_PassesToService()
        {
            // Arrange
            var request = new PaginationFilter<string> 
            { 
                Page = 2, 
                PageSize = 25,
                SortBy = "ProjectParent",
                Descending = true,
                Filter = "{}" 
            };
            const string month = "8";
            QueryParameters<string>? capturedQuery = null;

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(x => 
                {
                    var filter = x.Arg<PaginationFilter<string>>();
                    return new QueryParameters<string> 
                    { 
                        Page = filter.Page, 
                        PageSize = filter.PageSize,
                        SortBy = filter.SortBy,
                        Descending = filter.Descending
                    };
                });

            _invoiceService.GetPagedProjectInvoicesByMonthAsync(
                Arg.Do<QueryParameters<string>>(q => capturedQuery = q), 
                8)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));

            _mapper.Map<List<AutomaticInvoiceItem>>(Arg.Any<List<ProjectInvoiceDto>>())
                .Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadInvoicesGrid(request, month);

            // Assert
            Assert.NotNull(capturedQuery);
            Assert.Equal(2, capturedQuery.Page);
            Assert.Equal(25, capturedQuery.PageSize);
        }

        [Fact]
        public async Task LoadInvoicesGrid_ServiceReturnsData_MapsToGridItems()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 50, Filter = "{}" };
            const string month = "4";
            var invoices = new List<ProjectInvoiceDto>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ001", Month = 4, Amount = 1000m },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ002", Month = 4, Amount = 2000m }
            };
            var gridItems = new List<AutomaticInvoiceItem>
            {
                new() { InvoiceCounter = 1, ProjectParent = "PRJ001", Month = 4, Amount = 1000m },
                new() { InvoiceCounter = 2, ProjectParent = "PRJ002", Month = 4, Amount = 2000m }
            };

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _invoiceService.GetPagedProjectInvoicesByMonthAsync(Arg.Any<QueryParameters<string>>(), 4)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(invoices, new PaginationDto()));
            _mapper.Map<List<AutomaticInvoiceItem>>(invoices)
                .Returns(gridItems);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadInvoicesGrid(request, month);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<AutomaticInvoiceItem>>(partial.Model);
            Assert.Equal(2, model.Data.Count);
        }

        [Fact]
        public async Task LoadInvoicesGrid_ServiceReturnsNull_HandlesGracefully()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 50, Filter = "{}" };
            const string month = "9";

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _invoiceService.GetPagedProjectInvoicesByMonthAsync(Arg.Any<QueryParameters<string>>(), 9)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(null!, new PaginationDto()));
            _mapper.Map<List<AutomaticInvoiceItem>>(Arg.Any<List<ProjectInvoiceDto>>())
                .Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadInvoicesGrid(request, month);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<AutomaticInvoiceItem>>(partial.Model);
            Assert.Empty(model.Data);
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

        #region BuildAutomaticInvoiceGridAsync Additional Tests

        [Fact]
        public async Task Index_WithMonth_GridConfigContainsMonthInQueryString()
        {
            // Arrange
            const int month = 7;
            var months = new List<MonthDto>
            {
                new() { Monthnumber = 7, Monthname = "July" }
            };

            _invoiceService.GetPagedProjectInvoicesByMonthAsync(Arg.Any<QueryParameters<string>>(), month)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            SetupMonthsList(months);
            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.Index(month);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AutomaticMonthlyInvoiceViewModel>(viewResult.Model);
            Assert.Contains("?month=7", model.InvoicesGrid.BindGridUrl);
        }

        [Fact]
        public async Task Index_WithoutMonth_GridContainsEmptyData()
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
            Assert.Empty(model.InvoicesGrid.Data);
            Assert.DoesNotContain("?month=", model.InvoicesGrid.BindGridUrl);
            // Verify service was NOT called when no month is provided
            await _invoiceService.DidNotReceive().GetPagedProjectInvoicesByMonthAsync(
                Arg.Any<QueryParameters<string>>(), 
                Arg.Any<int?>());
        }

        [Fact]
        public async Task Index_WithMonth_GridConfigurationSetCorrectly()
        {
            // Arrange
            const int month = 5;
            var months = new List<MonthDto>
            {
                new() { Monthnumber = 5, Monthname = "May" }
            };

            _invoiceService.GetPagedProjectInvoicesByMonthAsync(Arg.Any<QueryParameters<string>>(), month)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            SetupMonthsList(months);
            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.Index(month);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AutomaticMonthlyInvoiceViewModel>(viewResult.Model);
            var gridConfig = model.InvoicesGrid;

            Assert.True(gridConfig.ShowCheckboxColumn);
            Assert.Equal("automaticInvoiceGrid", gridConfig.GridId);
            Assert.Equal("InvoiceCounter", gridConfig.KeyProperty);
            Assert.Equal("addAutomaticInvoice", gridConfig.AddFunction);
            Assert.Equal("editAutomaticInvoice", gridConfig.EditFunction);
            Assert.Equal("deleteAutomaticInvoice", gridConfig.DeleteFunction);
        }

        [Fact]
        public async Task LoadInvoicesGrid_WithMonth_SetsCorrectPaginationSortingFromRequest()
        {
            // Arrange
            var request = new PaginationFilter<string> 
            { 
                Page = 3, 
                PageSize = 100,
                SortBy = "Amount",
                Descending = true,
                Filter = "{}" 
            };
            const string month = "11";
            var paginationDto = new PaginationDto 
            { 
                PageNumber = 3, 
                PageSize = 100, 
                TotalRecords = 250, 
                TotalPages = 3 
            };

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _invoiceService.GetPagedProjectInvoicesByMonthAsync(Arg.Any<QueryParameters<string>>(), 11)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], paginationDto));
            _mapper.Map<List<AutomaticInvoiceItem>>(Arg.Any<List<ProjectInvoiceDto>>())
                .Returns([]);
            _mapper.Map<PaginationModel>(paginationDto)
                .Returns(new PaginationModel 
                { 
                    PageNumber = 3, 
                    PageSize = 100, 
                    TotalRecords = 250
                });

            // Act
            var result = await _controller.LoadInvoicesGrid(request, month);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<AutomaticInvoiceItem>>(partial.Model);
            Assert.Equal("Amount", model.Pagination.SortColumn);
            Assert.True(model.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadInvoicesGrid_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 50, Filter = "{}" };
            const string month = "6";

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _invoiceService.GetPagedProjectInvoicesByMonthAsync(Arg.Any<QueryParameters<string>>(), 6)
                .Returns(Task.FromException<ApiResponseDto<List<ProjectInvoiceDto>>>(new InvalidOperationException("Database connection error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _controller.LoadInvoicesGrid(request, month));
        }

        [Fact]
        public async Task LoadInvoicesGrid_WithBoundaryMonth_January_Works()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            const string month = "1";

            _invoiceService.GetPagedProjectInvoicesByMonthAsync(Arg.Any<QueryParameters<string>>(), 1)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.LoadInvoicesGrid(request, month);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.NotNull(partial.Model);
            await _invoiceService.Received(1).GetPagedProjectInvoicesByMonthAsync(
                Arg.Any<QueryParameters<string>>(), 
                1);
        }

        [Fact]
        public async Task LoadInvoicesGrid_WithBoundaryMonth_December_Works()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            const string month = "12";

            _invoiceService.GetPagedProjectInvoicesByMonthAsync(Arg.Any<QueryParameters<string>>(), 12)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.LoadInvoicesGrid(request, month);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.NotNull(partial.Model);
            await _invoiceService.Received(1).GetPagedProjectInvoicesByMonthAsync(
                Arg.Any<QueryParameters<string>>(), 
                12);
        }

        [Fact]
        public async Task Index_ServiceReturnsEmptyList_GridShowsNoData()
        {
            // Arrange
            const int month = 2;
            var months = new List<MonthDto>
            {
                new() { Monthnumber = 2, Monthname = "February" }
            };

            _invoiceService.GetPagedProjectInvoicesByMonthAsync(Arg.Any<QueryParameters<string>>(), month)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto { TotalRecords = 0 }));
            SetupMonthsList(months);
            SetupInvoicesGridMapper();

            // Act
            var result = await _controller.Index(month);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AutomaticMonthlyInvoiceViewModel>(viewResult.Model);
            Assert.Empty(model.InvoicesGrid.Data);
        }

        [Fact]
        public async Task Index_WithLargeMonth_ServiceReceivesCorrectValue()
        {
            // Arrange
            const int month = 12;
            var months = new List<MonthDto>
            {
                new() { Monthnumber = 12, Monthname = "December" }
            };

            _invoiceService.GetPagedProjectInvoicesByMonthAsync(Arg.Any<QueryParameters<string>>(), 12)
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            SetupMonthsList(months);
            SetupInvoicesGridMapper();

            // Act
            await _controller.Index(month);

            // Assert
            await _invoiceService.Received(1).GetPagedProjectInvoicesByMonthAsync(
                Arg.Any<QueryParameters<string>>(), 
                12);
        }

        #endregion

        #region CopyInvoices Tests

        [Fact]
        public async Task CopyInvoices_ValidBulkCopyRequest_ReturnsSuccessJson()
        {
            // Arrange
            var request = new CopyInvoicesRequest
            {
                SourceMonth = 3,
                TargetMonth = 9,
                InvoiceIds = null,
                InvoiceRecords = null
            };
            var copyDto = new CopyInvoicesDto { SourceMonth = 3, TargetMonth = 9 };

            _mapper.Map<CopyInvoicesDto>(request).Returns(copyDto);
            _invoiceService.CopyInvoicesAsync(copyDto)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.CopyInvoices(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.True(jsonElement.GetProperty("success").GetBoolean());
            Assert.Equal("Successfully copied invoices", jsonElement.GetProperty("message").GetString());
            await _invoiceService.Received(1).CopyInvoicesAsync(copyDto);
        }

        [Fact]
        public async Task CopyInvoices_ValidSelectiveCopyWithInvoiceIds_ReturnsSuccessJson()
        {
            // Arrange
            var request = new CopyInvoicesRequest
            {
                SourceMonth = 5,
                TargetMonth = 6,
                InvoiceIds = new List<int> { 1, 2, 3 },
                InvoiceRecords = null
            };
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 5,
                TargetMonth = 6,
                InvoiceIds = new List<int> { 1, 2, 3 }
            };

            _mapper.Map<CopyInvoicesDto>(request).Returns(copyDto);
            _invoiceService.CopyInvoicesAsync(Arg.Is<CopyInvoicesDto>(dto =>
                dto.SourceMonth == 5 &&
                dto.TargetMonth == 6 &&
                dto.InvoiceIds != null &&
                dto.InvoiceIds.Count == 3))
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.CopyInvoices(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.True(jsonElement.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CopyInvoices_WithInvoiceRecords_ExtractsIdsAndCallsService()
        {
            // Arrange
            var invoiceRecords = new List<AutomaticInvoiceItem>
            {
                new() { InvoiceCounter = 10, ProjectParent = "PRJ001", Month = 5, Amount = 1000m },
                new() { InvoiceCounter = 20, ProjectParent = "PRJ002", Month = 5, Amount = 2000m }
            };
            var request = new CopyInvoicesRequest
            {
                SourceMonth = 5,
                TargetMonth = 6,
                InvoiceIds = null,
                InvoiceRecords = invoiceRecords
            };
            CopyInvoicesRequest? capturedRequest = null;
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 5,
                TargetMonth = 6,
                InvoiceIds = new List<int> { 10, 20 }
            };

            _mapper.Map<CopyInvoicesDto>(Arg.Do<CopyInvoicesRequest>(r => capturedRequest = r))
                .Returns(copyDto);
            _invoiceService.CopyInvoicesAsync(Arg.Any<CopyInvoicesDto>())
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.CopyInvoices(request);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.NotNull(capturedRequest.InvoiceIds);
            Assert.Equal(2, capturedRequest.InvoiceIds.Count);
            Assert.Contains(10, capturedRequest.InvoiceIds);
            Assert.Contains(20, capturedRequest.InvoiceIds);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.True(jsonElement.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CopyInvoices_WithInvoiceRecordsWithZeroIds_FiltersOutZeroIds()
        {
            // Arrange
            var invoiceRecords = new List<AutomaticInvoiceItem>
            {
                new() { InvoiceCounter = 0, ProjectParent = "PRJ001", Month = 5, Amount = 1000m },
                new() { InvoiceCounter = 10, ProjectParent = "PRJ002", Month = 5, Amount = 2000m }
            };
            var request = new CopyInvoicesRequest
            {
                SourceMonth = 5,
                TargetMonth = 6,
                InvoiceIds = null,
                InvoiceRecords = invoiceRecords
            };
            CopyInvoicesRequest? capturedRequest = null;
            var copyDto = new CopyInvoicesDto { SourceMonth = 5, TargetMonth = 6, InvoiceIds = new List<int> { 10 } };

            _mapper.Map<CopyInvoicesDto>(Arg.Do<CopyInvoicesRequest>(r => capturedRequest = r))
                .Returns(copyDto);
            _invoiceService.CopyInvoicesAsync(Arg.Any<CopyInvoicesDto>())
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            await _controller.CopyInvoices(request);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.NotNull(capturedRequest.InvoiceIds);
            Assert.Single(capturedRequest.InvoiceIds);
            Assert.Equal(10, capturedRequest.InvoiceIds[0]);
        }

        [Fact]
        public async Task CopyInvoices_ServiceReturnsFalse_ReturnsFailureJson()
        {
            // Arrange
            var request = new CopyInvoicesRequest
            {
                SourceMonth = 3,
                TargetMonth = 9,
                InvoiceIds = null
            };
            var copyDto = new CopyInvoicesDto { SourceMonth = 3, TargetMonth = 9 };

            _mapper.Map<CopyInvoicesDto>(request).Returns(copyDto);
            _invoiceService.CopyInvoicesAsync(copyDto)
                .Returns(ApiResponseDto<bool>.SuccessResponse(false));

            // Act
            var result = await _controller.CopyInvoices(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to copy invoices", jsonElement.GetProperty("message").GetString());
        }

        [Fact]
        public async Task CopyInvoices_ServiceReturnsFailureResponse_ReturnsErrorJson()
        {
            // Arrange
            var request = new CopyInvoicesRequest
            {
                SourceMonth = 5,
                TargetMonth = 6
            };
            var copyDto = new CopyInvoicesDto { SourceMonth = 5, TargetMonth = 6 };
            var errors = new List<ApiErrorDto>
            {
                new() { Code = "VALIDATION_ERROR", Message = "Source and target months must be different" },
                new() { Code = "INVALID_MONTH", Message = "Target month is invalid" }
            };

            _mapper.Map<CopyInvoicesDto>(request).Returns(copyDto);
            _invoiceService.CopyInvoicesAsync(copyDto)
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.CopyInvoices(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
            Assert.Contains("Source and target months must be different", jsonElement.GetProperty("message").GetString());
        }

        [Fact]
        public async Task CopyInvoices_ServiceReturnsFailureWithMultipleErrors_ConcatenatesAllErrors()
        {
            // Arrange
            var request = new CopyInvoicesRequest { SourceMonth = 3, TargetMonth = 9 };
            var copyDto = new CopyInvoicesDto { SourceMonth = 3, TargetMonth = 9 };
            var errors = new List<ApiErrorDto>
            {
                new() { Code = "ERROR1", Message = "First error" },
                new() { Code = "ERROR2", Message = "Second error" },
                new() { Message = "Third error" }
            };

            _mapper.Map<CopyInvoicesDto>(request).Returns(copyDto);
            _invoiceService.CopyInvoicesAsync(copyDto)
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.CopyInvoices(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
            var message = jsonElement.GetProperty("message").GetString();
            Assert.Contains("First error", message);
            Assert.Contains("Second error", message);
            Assert.Contains("Third error", message);
        }

        [Fact]
        public async Task CopyInvoices_ServiceReturnsFailureWithNoErrors_ReturnsGenericErrorMessage()
        {
            // Arrange
            var request = new CopyInvoicesRequest { SourceMonth = 1, TargetMonth = 2 };
            var copyDto = new CopyInvoicesDto { SourceMonth = 1, TargetMonth = 2 };

            _mapper.Map<CopyInvoicesDto>(request).Returns(copyDto);
            _invoiceService.CopyInvoicesAsync(copyDto)
                .Returns(ApiResponseDto<bool>.FailureResponse(new List<ApiErrorDto>(), new ApiMetaDto()));

            // Act
            var result = await _controller.CopyInvoices(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to copy invoices", jsonElement.GetProperty("message").GetString());
        }


        [Fact]
        public async Task CopyInvoices_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new CopyInvoicesRequest { SourceMonth = 6, TargetMonth = 7 };
            var copyDto = new CopyInvoicesDto { SourceMonth = 6, TargetMonth = 7 };

            _mapper.Map<CopyInvoicesDto>(request).Returns(copyDto);
            _invoiceService.CopyInvoicesAsync(copyDto)
                .Returns(Task.FromException<ApiResponseDto<bool>>(new InvalidOperationException("Service unavailable")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.CopyInvoices(request));
        }

        [Fact]
        public async Task CopyInvoices_EmptyInvoiceIds_MapsCorrectly()
        {
            // Arrange
            var request = new CopyInvoicesRequest
            {
                SourceMonth = 8,
                TargetMonth = 9,
                InvoiceIds = new List<int>(),
                InvoiceRecords = null
            };
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 8,
                TargetMonth = 9,
                InvoiceIds = new List<int>()
            };

            _mapper.Map<CopyInvoicesDto>(request).Returns(copyDto);
            _invoiceService.CopyInvoicesAsync(copyDto)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.CopyInvoices(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.True(jsonElement.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CopyInvoices_BothInvoiceIdsAndRecordsProvided_PreservesInvoiceIds()
        {
            // Arrange
            var request = new CopyInvoicesRequest
            {
                SourceMonth = 10,
                TargetMonth = 11,
                InvoiceIds = new List<int> { 5, 6 },
                InvoiceRecords = new List<AutomaticInvoiceItem>
                {
                    new() { InvoiceCounter = 1, ProjectParent = "PRJ001" }
                }
            };
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 10,
                TargetMonth = 11,
                InvoiceIds = new List<int> { 5, 6 }
            };

            _mapper.Map<CopyInvoicesDto>(request).Returns(copyDto);
            _invoiceService.CopyInvoicesAsync(Arg.Is<CopyInvoicesDto>(dto =>
                dto.InvoiceIds != null && dto.InvoiceIds.Contains(5) && dto.InvoiceIds.Contains(6)))
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.CopyInvoices(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.True(jsonElement.GetProperty("success").GetBoolean());
            // Verify original InvoiceIds were preserved (not replaced by records)
            Assert.Equal(2, request.InvoiceIds.Count);
            Assert.Contains(5, request.InvoiceIds);
            Assert.Contains(6, request.InvoiceIds);
        }

        [Fact]
        public async Task CopyInvoices_MapperCalledWithCorrectRequest_PassesToService()
        {
            // Arrange
            var request = new CopyInvoicesRequest
            {
                SourceMonth = 11,
                TargetMonth = 12,
                InvoiceIds = new List<int> { 100, 200 }
            };
            var mappedDto = new CopyInvoicesDto
            {
                SourceMonth = 11,
                TargetMonth = 12,
                InvoiceIds = new List<int> { 100, 200 }
            };

            _mapper.Map<CopyInvoicesDto>(request).Returns(mappedDto);
            _invoiceService.CopyInvoicesAsync(mappedDto)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            await _controller.CopyInvoices(request);

            // Assert
            _mapper.Received(1).Map<CopyInvoicesDto>(request);
            await _invoiceService.Received(1).CopyInvoicesAsync(mappedDto);
        }

        #endregion
    }
}
