using Apha.Common.Utilities.ExcelExport;
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
using NSubstitute;
using System.Text;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.InvoiceImportControllerTest
{
    public class InvoiceImportControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectInvoiceService _invoiceService;
        private readonly IProjectService _projectService;
        private readonly IMonthService _monthService;
        private readonly IExcelExportService _excelExportService;
        private readonly InvoiceImportController _controller;

        public InvoiceImportControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _invoiceService = Substitute.For<IProjectInvoiceService>();
            _projectService = Substitute.For<IProjectService>();
            _monthService = Substitute.For<IMonthService>();
            _excelExportService = Substitute.For<IExcelExportService>();
            _controller = new InvoiceImportController(
                _mapper,
                _invoiceService,
                _projectService,
                _monthService,
                _excelExportService);
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private void SetupGridMappers()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<InvoiceItem>>(Arg.Any<List<ProjectInvoiceDto>>())
                .Returns([]);
            _mapper.Map<List<InvoiceImportFailedItem>>(Arg.Any<List<InvoiceImportRowDto>>())
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

        private void SetupDefaultServices()
        {
            SetupProjectsList([]);
            SetupMonthsList([]);
            SetupGridMappers();

            _invoiceService.GetPagedProjectInvoiceManualAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>())
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            _invoiceService.GetFailedInvoiceImportAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<List<InvoiceImportRowDto>>.SuccessResponse([], new PaginationDto()));
        }

        #region Index

        [Fact]
        public async Task Index_WithParentProjectAndMonth_ReturnsViewWithFilteredViewModel()
        {
            // Arrange
            SetupDefaultServices();

            // Act
            var result = await _controller.Index("PRJ001", 6);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<InvoiceViewModel>(viewResult.Model);
            Assert.Equal("PRJ001", model.ParentProject);
            Assert.Equal(6, model.Month);
        }

        [Fact]
        public async Task Index_WithNullParameters_ReturnsViewWithEmptyFilters()
        {
            // Arrange
            SetupDefaultServices();

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<InvoiceViewModel>(viewResult.Model);
            Assert.Equal(string.Empty, model.ParentProject);
            Assert.Null(model.Month);
        }

        [Fact]
        public async Task Index_PopulatesViewBagProjects()
        {
            // Arrange
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PRJ001" },
                new() { ParentProject = "PRJ002" }
            };
            SetupProjectsList(projects);
            SetupMonthsList([]);
            SetupGridMappers();
            _invoiceService.GetPagedProjectInvoiceManualAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>())
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            _invoiceService.GetFailedInvoiceImportAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<List<InvoiceImportRowDto>>.SuccessResponse([], new PaginationDto()));

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(_controller.ViewBag.Projects);
            Assert.NotNull(_controller.ViewBag.FilterProjects);
            var model = Assert.IsType<InvoiceViewModel>(viewResult.Model);
            Assert.NotEmpty(model.FilterProjects);
        }

        [Fact]
        public async Task Index_WithMonth_SetsFilterOnRequest()
        {
            // Arrange
            SetupDefaultServices();

            // Act
            var result = await _controller.Index(null, 3);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<InvoiceViewModel>(viewResult.Model);
            Assert.Equal(3, model.Month);
        }

        [Fact]
        public async Task Index_PopulatesMonthsList()
        {
            // Arrange
            var months = new List<MonthDto>
            {
                new() { Monthnumber = 1, Monthname = "January" },
                new() { Monthnumber = 2, Monthname = "February" }
            };
            SetupProjectsList([]);
            SetupMonthsList(months);
            SetupGridMappers();
            _invoiceService.GetPagedProjectInvoiceManualAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>())
                .Returns(ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse([], new PaginationDto()));
            _invoiceService.GetFailedInvoiceImportAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<List<InvoiceImportRowDto>>.SuccessResponse([], new PaginationDto()));

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<InvoiceViewModel>(viewResult.Model);
            Assert.NotEmpty(model.FilterMonths);
        }

        [Fact]
        public async Task Index_FailedInvoicesGrid_IsPopulated()
        {
            // Arrange
            SetupDefaultServices();

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<InvoiceViewModel>(viewResult.Model);
            Assert.NotNull(model.FailedInvoicesGrid);
        }

        #endregion

        #region LoadInvoicesGrid

        [Fact]
        public async Task LoadInvoicesGrid_ValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupDefaultServices();

            // Act
            var result = await _controller.LoadInvoicesGrid(request, "PRJ001", null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadInvoicesGrid_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "Error");

            // Act
            var result = await _controller.LoadInvoicesGrid(new PaginationFilter<string>(), null, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadInvoicesGrid_WithMonth_MergesMonthIntoFilter()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            SetupDefaultServices();

            // Act
            var result = await _controller.LoadInvoicesGrid(request, null, 7);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            Assert.Contains("\"Month\":\"7\"", request.Filter);
        }

        [Fact]
        public async Task LoadInvoicesGrid_WithExistingFilter_PreservesAndMerges()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{\"ProjectParent\":\"PRJ001\"}" };
            SetupDefaultServices();

            // Act
            var result = await _controller.LoadInvoicesGrid(request, null, 3);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            Assert.Contains("ProjectParent", request.Filter);
            Assert.Contains("Month", request.Filter);
        }

        [Fact]
        public async Task LoadInvoicesGrid_WithNullFilter_AndMonth_CreatesNewFilter()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = null };
            SetupDefaultServices();

            // Act
            var result = await _controller.LoadInvoicesGrid(request, null, 5);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            Assert.Contains("Month", request.Filter!);
        }

        [Fact]
        public async Task LoadInvoicesGrid_WithEmptyFilter_AndMonth_CreatesNewFilter()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "" };
            SetupDefaultServices();

            // Act
            var result = await _controller.LoadInvoicesGrid(request, null, 5);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            Assert.Contains("Month", request.Filter!);
        }

        #endregion

        #region GetInvoice

        [Fact]
        public async Task GetInvoice_IdIsZero_ReturnsPartialViewWithNewInvoice()
        {
            // Arrange
            SetupProjectsList([]);
            SetupMonthsList([]);

            // Act
            var result = await _controller.GetInvoice(0, "PRJ001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditInvoice", partial.ViewName);
            var model = Assert.IsType<InvoiceItem>(partial.Model);
            Assert.Equal("PRJ001", model.ProjectParent);
        }

        [Fact]
        public async Task GetInvoice_IdIsZero_NullParentProject_SetsEmptyString()
        {
            // Arrange
            SetupProjectsList([]);
            SetupMonthsList([]);

            // Act
            var result = await _controller.GetInvoice(0, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<InvoiceItem>(partial.Model);
            Assert.Equal(string.Empty, model.ProjectParent);
        }

        [Fact]
        public async Task GetInvoice_ExistingInvoice_ReturnsPartialViewWithMappedItem()
        {
            // Arrange
            var invoiceDto = new ProjectInvoiceDto { InvoiceCounter = 5, ProjectParent = "PRJ001" };
            _invoiceService.GetByIdAsync(5).Returns(ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(invoiceDto));
            _mapper.Map<InvoiceItem>(invoiceDto).Returns(new InvoiceItem { InvoiceCounter = 5, ProjectParent = "PRJ001" });
            SetupProjectsList([]);
            SetupMonthsList([]);

            // Act
            var result = await _controller.GetInvoice(5, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditInvoice", partial.ViewName);
            var model = Assert.IsType<InvoiceItem>(partial.Model);
            Assert.Equal(5, model.InvoiceCounter);
        }

        [Fact]
        public async Task GetInvoice_NotFound_ReturnsNotFound()
        {
            // Arrange
            _invoiceService.GetByIdAsync(99).Returns(new ApiResponseDto<ProjectInvoiceDto> { Success = false });
            SetupProjectsList([]);
            SetupMonthsList([]);

            // Act
            var result = await _controller.GetInvoice(99, null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetInvoice_SuccessButNullData_ReturnsNotFound()
        {
            // Arrange
            _invoiceService.GetByIdAsync(99).Returns(new ApiResponseDto<ProjectInvoiceDto> { Success = true, Data = null });
            SetupProjectsList([]);
            SetupMonthsList([]);

            // Act
            var result = await _controller.GetInvoice(99, null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetInvoice_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "Error");

            // Act
            var result = await _controller.GetInvoice(1, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region SaveInvoice

        [Fact]
        public async Task SaveInvoice_InvalidModelState_ReturnsJsonWithErrors()
        {
            // Arrange
            _controller.ModelState.AddModelError("Amount", "Amount is required");

            // Act
            var result = await _controller.SaveInvoice(new InvoiceItem());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal("Please correct the errors below.", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task SaveInvoice_InvalidModelState_WithDollarDotPrefix_TrimsPrefix()
        {
            // Arrange
            _controller.ModelState.AddModelError("$.Amount", "Amount is required");

            // Act
            var result = await _controller.SaveInvoice(new InvoiceItem());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            var errors = element.GetProperty("errors");
            var firstError = errors.EnumerateArray().First();
            Assert.Equal("Amount", firstError.GetProperty("field").GetString());
        }

        [Fact]
        public async Task SaveInvoice_InvalidModelState_WithDollarKey_ExcludedFromErrors()
        {
            // Arrange
            _controller.ModelState.AddModelError("$", "Root error");
            _controller.ModelState.AddModelError("Amount", "Amount is required");

            // Act
            var result = await _controller.SaveInvoice(new InvoiceItem());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            var errors = element.GetProperty("errors").EnumerateArray().ToList();
            Assert.Single(errors);
            Assert.Equal("Amount", errors[0].GetProperty("field").GetString());
        }

        [Fact]
        public async Task SaveInvoice_NewInvoice_CallsCreateAndReturnsSuccess()
        {
            // Arrange
            var model = new InvoiceItem { InvoiceCounter = 0 };
            var dto = new ProjectInvoiceDto();
            _mapper.Map<ProjectInvoiceDto>(model).Returns(dto);
            _invoiceService.CreateAsync(dto).Returns(ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.SaveInvoice(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal("Invoice saved successfully.", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task SaveInvoice_ExistingInvoice_CallsUpdateAndReturnsSuccess()
        {
            // Arrange
            var model = new InvoiceItem { InvoiceCounter = 5 };
            var dto = new ProjectInvoiceDto();
            _mapper.Map<ProjectInvoiceDto>(model).Returns(dto);
            _invoiceService.UpdateAsync(5, dto).Returns(ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.SaveInvoice(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal("Invoice updated successfully.", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task SaveInvoice_CreateFails_ReturnsFailureJsonWithErrors()
        {
            // Arrange
            var model = new InvoiceItem { InvoiceCounter = 0 };
            var dto = new ProjectInvoiceDto();
            _mapper.Map<ProjectInvoiceDto>(model).Returns(dto);
            var failResponse = new ApiResponseDto<ProjectInvoiceDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Code = "Amount", Message = "Invalid amount" }]
            };
            _invoiceService.CreateAsync(dto).Returns(failResponse);

            // Act
            var result = await _controller.SaveInvoice(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to save invoice.", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task SaveInvoice_CreateFails_NullErrors_UsesEmptyList()
        {
            // Arrange
            var model = new InvoiceItem { InvoiceCounter = 0 };
            var dto = new ProjectInvoiceDto();
            _mapper.Map<ProjectInvoiceDto>(model).Returns(dto);
            var failResponse = new ApiResponseDto<ProjectInvoiceDto>
            {
                Success = false,
                Errors = null
            };
            _invoiceService.CreateAsync(dto).Returns(failResponse);

            // Act
            var result = await _controller.SaveInvoice(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveInvoice_CreateFails_NullCodeAndMessage_UsesDefaults()
        {
            // Arrange
            var model = new InvoiceItem { InvoiceCounter = 0 };
            var dto = new ProjectInvoiceDto();
            _mapper.Map<ProjectInvoiceDto>(model).Returns(dto);
            var failResponse = new ApiResponseDto<ProjectInvoiceDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Code = null!, Message = null! }]
            };
            _invoiceService.CreateAsync(dto).Returns(failResponse);

            // Act
            var result = await _controller.SaveInvoice(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            var errors = element.GetProperty("errors").EnumerateArray().ToList();
            Assert.Single(errors);
            Assert.Equal(string.Empty, errors[0].GetProperty("field").GetString());
            Assert.Equal("An unexpected error occurred.", errors[0].GetProperty("message").GetString());
        }

        #endregion

        #region DeleteInvoice

        [Fact]
        public async Task DeleteInvoice_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "Error");

            // Act
            var result = await _controller.DeleteInvoice(1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteInvoice_Success_ReturnsJsonSuccess()
        {
            // Arrange
            _invoiceService.DeleteAsync(5).Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeleteInvoice(5);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteInvoice_Failure_ReturnsJsonFailure()
        {
            // Arrange
            _invoiceService.DeleteAsync(5).Returns(new ApiResponseDto<bool> { Success = false });

            // Act
            var result = await _controller.DeleteInvoice(5);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to delete invoice.", element.GetProperty("message").GetString());
        }

        #endregion

        #region DownloadTemplate

        [Fact]
        public void DownloadTemplate_FileExists_ReturnsFileResult()
        {
            // Arrange
            var templateDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "PACT");
            Directory.CreateDirectory(templateDir);
            var templatePath = Path.Combine(templateDir, "InvoiceImport-Template.xlsx");
            File.WriteAllBytes(templatePath, [0x50, 0x4B, 0x03, 0x04]); // minimal bytes

            try
            {
                // Act
                var result = _controller.DownloadTemplate();

                // Assert
                var fileResult = Assert.IsType<FileContentResult>(result);
                Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
                Assert.StartsWith("InvoiceImport_", fileResult.FileDownloadName);
                Assert.EndsWith(".xlsx", fileResult.FileDownloadName);
            }
            finally
            {
                File.Delete(templatePath);
            }
        }

        [Fact]
        public void DownloadTemplate_FileNotExists_ReturnsNotFound()
        {
            // Arrange — ensure template file does not exist
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "PACT", "InvoiceImport-Template.xlsx");
            if (File.Exists(templatePath))
                File.Delete(templatePath);

            // Act
            var result = _controller.DownloadTemplate();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region Import

        [Fact]
        public async Task Import_NullFile_ReturnsJsonFailure()
        {
            // Act
            var result = await _controller.Import(null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal("Please select an Excel file to import.", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Import_EmptyFile_ReturnsJsonFailure()
        {
            // Arrange
            var file = Substitute.For<IFormFile>();
            file.Length.Returns(0);

            // Act
            var result = await _controller.Import(file);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal("Please select an Excel file to import.", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Import_ServiceReturnsFailure_ReturnsJsonFailureWithErrorMessage()
        {
            // Arrange
            var file = Substitute.For<IFormFile>();
            file.Length.Returns(100);
            var failResponse = new ApiResponseDto<InvoiceImportResultDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Invalid format" }]
            };
            _invoiceService.ImportInvoiceAsync(file).Returns(failResponse);

            // Act
            var result = await _controller.Import(file);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal("Invalid format", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Import_ServiceReturnsFailure_NullErrors_ReturnsDefaultMessage()
        {
            // Arrange
            var file = Substitute.For<IFormFile>();
            file.Length.Returns(100);
            var failResponse = new ApiResponseDto<InvoiceImportResultDto>
            {
                Success = false,
                Errors = null
            };
            _invoiceService.ImportInvoiceAsync(file).Returns(failResponse);

            // Act
            var result = await _controller.Import(file);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.Equal("Import failed.", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Import_ServiceReturnsSuccessWithNullData_ReturnsJsonFailure()
        {
            // Arrange
            var file = Substitute.For<IFormFile>();
            file.Length.Returns(100);
            var response = new ApiResponseDto<InvoiceImportResultDto>
            {
                Success = true,
                Data = null
            };
            _invoiceService.ImportInvoiceAsync(file).Returns(response);

            // Act
            var result = await _controller.Import(file);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Import_ServiceReturnsSuccess_ReturnsJsonSuccessWithCounts()
        {
            // Arrange
            var file = Substitute.For<IFormFile>();
            file.Length.Returns(100);
            var importResult = new InvoiceImportResultDto { PassedCount = 10, FailedCount = 2, Message = "Imported" };
            _invoiceService.ImportInvoiceAsync(file)
                .Returns(ApiResponseDto<InvoiceImportResultDto>.SuccessResponse(importResult));

            // Act
            var result = await _controller.Import(file);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal(10, element.GetProperty("passedCount").GetInt32());
            Assert.Equal(2, element.GetProperty("failedCount").GetInt32());
            Assert.Equal("Imported", element.GetProperty("message").GetString());
        }

        #endregion

        #region LoadFailedInvoiceImportGrid

        [Fact]
        public async Task LoadFailedInvoiceImportGrid_ValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            SetupDefaultServices();

            // Act
            var result = await _controller.LoadFailedInvoiceImportGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadFailedInvoiceImportGrid_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "Error");

            // Act
            var result = await _controller.LoadFailedInvoiceImportGrid(new PaginationFilter<string>());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region ExportFailedInvoiceImport

        [Fact]
        public async Task ExportFailedInvoiceImport_SuccessWithData_ReturnsExcelFile()
        {
            // Arrange
            var failedRows = new List<InvoiceImportRowDto> { new() { Id = 1 } };
            var failedItems = new List<InvoiceImportFailedItem> { new() { Id = 1 } };
            _invoiceService.GetFailedInvoiceImportAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<List<InvoiceImportRowDto>>.SuccessResponse(failedRows));
            _mapper.Map<List<InvoiceImportFailedItem>>(failedRows).Returns(failedItems);
            _excelExportService.ExportToExcel(Arg.Any<List<InvoiceImportFailedItem>>(), Arg.Any<string>(), Arg.Any<Dictionary<string, string>>())
                .Returns([0x50, 0x4B]);

            // Act
            var result = await _controller.ExportFailedInvoiceImport();

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
            Assert.StartsWith("ExportedInvoiceImport_", fileResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportFailedInvoiceImport_FailureResponse_ReturnsEmptyExcel()
        {
            // Arrange
            _invoiceService.GetFailedInvoiceImportAsync(Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<InvoiceImportRowDto>> { Success = false, Data = null });
            _excelExportService.ExportToExcel(Arg.Any<List<InvoiceImportFailedItem>>(), Arg.Any<string>(), Arg.Any<Dictionary<string, string>>())
                .Returns([0x50, 0x4B]);

            // Act
            var result = await _controller.ExportFailedInvoiceImport();

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.NotNull(fileResult.FileContents);
        }

        [Fact]
        public async Task ExportFailedInvoiceImport_SuccessWithNullData_ReturnsEmptyExcel()
        {
            // Arrange
            _invoiceService.GetFailedInvoiceImportAsync(Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<InvoiceImportRowDto>> { Success = true, Data = null });
            _excelExportService.ExportToExcel(Arg.Any<List<InvoiceImportFailedItem>>(), Arg.Any<string>(), Arg.Any<Dictionary<string, string>>())
                .Returns([0x50, 0x4B]);

            // Act
            var result = await _controller.ExportFailedInvoiceImport();

            // Assert
            Assert.IsType<FileContentResult>(result);
        }

        #endregion

        #region DeleteAllFailedInvoiceImport

        [Fact]
        public async Task DeleteAllFailedInvoiceImport_SuccessAndDataTrue_ReturnsSuccessJson()
        {
            // Arrange
            _invoiceService.DeleteFailedInvoiceImportByUserAsync()
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeleteAllFailedInvoiceImport();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteAllFailedInvoiceImport_SuccessAndDataFalse_ReturnsFailureJson()
        {
            // Arrange
            _invoiceService.DeleteFailedInvoiceImportByUserAsync()
                .Returns(ApiResponseDto<bool>.SuccessResponse(false));

            // Act
            var result = await _controller.DeleteAllFailedInvoiceImport();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteAllFailedInvoiceImport_Failure_ReturnsJsonWithErrorMessage()
        {
            // Arrange
            var response = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "No records found" }]
            };
            _invoiceService.DeleteFailedInvoiceImportByUserAsync().Returns(response);

            // Act
            var result = await _controller.DeleteAllFailedInvoiceImport();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal("No records found", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task DeleteAllFailedInvoiceImport_NullErrors_ReturnsDefaultMessage()
        {
            // Arrange
            var response = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = null
            };
            _invoiceService.DeleteFailedInvoiceImportByUserAsync().Returns(response);

            // Act
            var result = await _controller.DeleteAllFailedInvoiceImport();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.Equal("Failed to delete failed imported records.", element.GetProperty("message").GetString());
        }

        #endregion

        #region GetFailedInvoiceImport

        [Fact]
        public async Task GetFailedInvoiceImport_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "Error");

            // Act
            var result = await _controller.GetFailedInvoiceImport(1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetFailedInvoiceImport_NotFound_ReturnsNotFound()
        {
            // Arrange
            _invoiceService.GetFailedInvoiceImportByIdAsync(99)
                .Returns(new ApiResponseDto<InvoiceImportRowDto> { Success = false });

            // Act
            var result = await _controller.GetFailedInvoiceImport(99);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetFailedInvoiceImport_SuccessButNullData_ReturnsNotFound()
        {
            // Arrange
            _invoiceService.GetFailedInvoiceImportByIdAsync(99)
                .Returns(new ApiResponseDto<InvoiceImportRowDto> { Success = true, Data = null });

            // Act
            var result = await _controller.GetFailedInvoiceImport(99);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetFailedInvoiceImport_Success_ReturnsPartialViewWithModel()
        {
            // Arrange
            var dto = new InvoiceImportRowDto { Id = 5, ProjectParent = "PRJ001" };
            _invoiceService.GetFailedInvoiceImportByIdAsync(5)
                .Returns(ApiResponseDto<InvoiceImportRowDto>.SuccessResponse(dto));
            _mapper.Map<InvoiceImportFailedItem>(dto)
                .Returns(new InvoiceImportFailedItem { Id = 5, ProjectParent = "PRJ001" });

            // Act
            var result = await _controller.GetFailedInvoiceImport(5);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_EditFailedInvoiceImport", partial.ViewName);
            var model = Assert.IsType<InvoiceImportFailedItem>(partial.Model);
            Assert.Equal(5, model.Id);
        }

        #endregion

        #region SaveFailedInvoiceImport

        [Fact]
        public async Task SaveFailedInvoiceImport_InvalidModelState_ReturnsJsonWithErrors()
        {
            // Arrange
            _controller.ModelState.AddModelError("Amount", "Amount is required");

            // Act
            var result = await _controller.SaveFailedInvoiceImport(new InvoiceImportFailedItem());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal("Please correct the errors below.", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task SaveFailedInvoiceImport_InvalidModelState_DollarDotPrefix_Trimmed()
        {
            // Arrange
            _controller.ModelState.AddModelError("$.Amount", "Amount is required");

            // Act
            var result = await _controller.SaveFailedInvoiceImport(new InvoiceImportFailedItem());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            var errors = element.GetProperty("errors").EnumerateArray().ToList();
            Assert.Equal("Amount", errors[0].GetProperty("field").GetString());
        }

        [Fact]
        public async Task SaveFailedInvoiceImport_InvalidModelState_DollarKey_Excluded()
        {
            // Arrange
            _controller.ModelState.AddModelError("$", "Root error");
            _controller.ModelState.AddModelError("Amount", "Required");

            // Act
            var result = await _controller.SaveFailedInvoiceImport(new InvoiceImportFailedItem());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            var errors = element.GetProperty("errors").EnumerateArray().ToList();
            Assert.Single(errors);
        }

        [Fact]
        public async Task SaveFailedInvoiceImport_SuccessWithDataTrue_ReturnsMovedToInvoiceMessage()
        {
            // Arrange
            var model = new InvoiceImportFailedItem { Id = 1 };
            var dto = new InvoiceImportRowDto { Id = 1 };
            _mapper.Map<InvoiceImportRowDto>(model).Returns(dto);
            _invoiceService.SaveFailedInvoiceImportAsync(1, dto)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.SaveFailedInvoiceImport(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal("Record successfully validated and is now live.", element.GetProperty("message").GetString());
            Assert.True(element.GetProperty("movedToInvoice").GetBoolean());
        }

        [Fact]
        public async Task SaveFailedInvoiceImport_SuccessWithDataFalse_ReturnsUpdatedMessage()
        {
            // Arrange
            var model = new InvoiceImportFailedItem { Id = 2 };
            var dto = new InvoiceImportRowDto { Id = 2 };
            _mapper.Map<InvoiceImportRowDto>(model).Returns(dto);
            _invoiceService.SaveFailedInvoiceImportAsync(2, dto)
                .Returns(ApiResponseDto<bool>.SuccessResponse(false));

            // Act
            var result = await _controller.SaveFailedInvoiceImport(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal("Failed record updated successfully.", element.GetProperty("message").GetString());
            Assert.False(element.GetProperty("movedToInvoice").GetBoolean());
        }

        [Fact]
        public async Task SaveFailedInvoiceImport_Failure_ReturnsJsonWithValidationErrors()
        {
            // Arrange
            var model = new InvoiceImportFailedItem { Id = 1 };
            var dto = new InvoiceImportRowDto { Id = 1 };
            _mapper.Map<InvoiceImportRowDto>(model).Returns(dto);
            var failResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Code = "Amount", Message = "Invalid" }]
            };
            _invoiceService.SaveFailedInvoiceImportAsync(1, dto).Returns(failResponse);

            // Act
            var result = await _controller.SaveFailedInvoiceImport(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal("Validation failed. Please correct the errors below.", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task SaveFailedInvoiceImport_Failure_NullErrors_UsesEmptyList()
        {
            // Arrange
            var model = new InvoiceImportFailedItem { Id = 1 };
            var dto = new InvoiceImportRowDto { Id = 1 };
            _mapper.Map<InvoiceImportRowDto>(model).Returns(dto);
            var failResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = null
            };
            _invoiceService.SaveFailedInvoiceImportAsync(1, dto).Returns(failResponse);

            // Act
            var result = await _controller.SaveFailedInvoiceImport(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveFailedInvoiceImport_Failure_NullCodeAndMessage_UsesDefaults()
        {
            // Arrange
            var model = new InvoiceImportFailedItem { Id = 1 };
            var dto = new InvoiceImportRowDto { Id = 1 };
            _mapper.Map<InvoiceImportRowDto>(model).Returns(dto);
            var failResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Code = null!, Message = null! }]
            };
            _invoiceService.SaveFailedInvoiceImportAsync(1, dto).Returns(failResponse);

            // Act
            var result = await _controller.SaveFailedInvoiceImport(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            var errors = element.GetProperty("errors").EnumerateArray().ToList();
            Assert.Equal(string.Empty, errors[0].GetProperty("field").GetString());
            Assert.Equal("An unexpected error occurred.", errors[0].GetProperty("message").GetString());
        }

        #endregion

        #region DeleteFailedInvoiceImport

        [Fact]
        public async Task DeleteFailedInvoiceImport_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "Error");

            // Act
            var result = await _controller.DeleteFailedInvoiceImport(1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteFailedInvoiceImport_Success_ReturnsJsonSuccess()
        {
            // Arrange
            _invoiceService.DeleteFailedInvoiceImportByIdAsync(5)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeleteFailedInvoiceImport(5);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteFailedInvoiceImport_Failure_ReturnsJsonFailure()
        {
            // Arrange
            _invoiceService.DeleteFailedInvoiceImportByIdAsync(5)
                .Returns(new ApiResponseDto<bool> { Success = false });

            // Act
            var result = await _controller.DeleteFailedInvoiceImport(5);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to delete failed record.", element.GetProperty("message").GetString());
        }

        #endregion
    }
}
