// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — InvoiceControllerTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - New file: xUnit tests for InvoiceController (frontend MVC, Phase 11).
 *   - Tests cover Index, LoadInvoiceGrid, GetInvoiceTotals, GetAddEditInvoicePartial,
 *     SaveInvoice (Create + Update paths), DeleteInvoice.
 *   - NSubstitute used for IMapper, IRadTrackInvoiceService, IProjectListService mocks.
 *   - Follows MilestoneControllerTests conventions: shared setup helpers, #region grouping.
 *
 * PRESERVED:
 *   - Naming convention: [MethodName]_[StateUnderTest]_[ExpectedResult].
 *   - InvoiceController injects IMapper, IRadTrackInvoiceService, IProjectListService.
 *   - SaveInvoice Create vs Update discriminated by InvoiceCounter == 0.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Razor partial view names verified at manual build time.
 *   - TRANSFORMENGINE TODO: Anti-forgery token ([ValidateAntiForgeryToken]) is bypassed
 *     in unit tests — verify end-to-end via integration tests.
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PIMS.Controllers;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PIMS.InvoiceControllerTest
{
    public class InvoiceControllerTests
    {
        private readonly IMapper                  _mapper;
        private readonly IRadTrackInvoiceService  _invoiceService;
        private readonly IProjectListService      _projectListService;
        private readonly InvoiceController        _controller;

        public InvoiceControllerTests()
        {
            _mapper             = Substitute.For<IMapper>();
            _invoiceService     = Substitute.For<IRadTrackInvoiceService>();
            _projectListService = Substitute.For<IProjectListService>();
            _controller         = new InvoiceController(_mapper, _invoiceService, _projectListService);
        }

        // ── Shared setup helpers ──────────────────────────────────────────────

        private static PaginationFilter<string> DefaultFilter()
            => new() { Page = 1, PageSize = 10, Filter = "{}" };

        private static ApiResponseDto<List<ProjectListViewDto>> EmptyProjectListResponse()
            => new() { Success = true, Data = [] };

        private static ApiResponseDto<List<RadTrackInvoiceDto>> SuccessListResponse(
            List<RadTrackInvoiceDto>? items = null)
            // TRANSFORMENGINE: Fix CS0029 — Pagination property is PaginationDto (not ApiMetaDto; Meta is a separate property)
            => new() { Success = true, Data = items ?? [], Pagination = new PaginationDto() };

        private static ApiResponseDto<RadTrackInvoiceTotalsDto> SuccessTotalsResponse()
            => new() { Success = true, Data = new RadTrackInvoiceTotalsDto { TotalPlannedAmount = 5000, TotalDueAmount = 3000, TotalActualAmount = 2000 } };

        private static ApiResponseDto<RadTrackInvoiceDto> SuccessSingleResponse(int id = 1)
            => new() { Success = true, Data = new RadTrackInvoiceDto { InvoiceCounter = id, Project = "PP001" } };

        private static ApiResponseDto<object> SuccessDeleteResponse()
            => new() { Success = true, Data = new { success = true } };

        private static ApiErrorDto OneError(string msg = "Error", string code = "ERR")
            => new() { Message = msg, Code = code };

        /// <summary>Wires all dependencies needed for Index() to complete.</summary>
        private void SetupSuccessfulIndexMocks(List<RadTrackInvoiceDto>? items = null)
        {
            _projectListService.GetAllProjectsListAsync()
                .Returns(EmptyProjectListResponse());

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());

            _invoiceService.GetAllAsync(
                    Arg.Any<QueryParameters<string>>(),
                    Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<string?>())
                .Returns(SuccessListResponse(items));

            _invoiceService.GetTotalsAsync(
                    Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<string?>())
                .Returns(SuccessTotalsResponse());

            _mapper.Map<List<InvoiceItem>>(Arg.Any<List<RadTrackInvoiceDto>>())
                .Returns([]);
            // TRANSFORMENGINE: Fix mapper mock — controller maps PaginationDto (not ApiMetaDto) to PaginationModel
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
        }

        // ── JsonResult helper ─────────────────────────────────────────────────

        private static JsonElement GetJsonElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        // ── Index ─────────────────────────────────────────────────────────────

        #region Index

        [Fact]
        public async Task Index_ServiceReturnsData_ReturnsViewResult()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_ServiceReturnsData_ViewModelHasTotals()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<InvoiceViewModel>(view.Model);
            Assert.NotNull(model.InvoiceTotals);
            Assert.Equal(5000.0, model.InvoiceTotals!.TotalPlannedAmount);
        }

        [Fact]
        public async Task Index_ServiceReturnsData_ViewModelGridIsPopulated()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<InvoiceViewModel>(view.Model);
            Assert.NotNull(model.InvoicesGrid);
            Assert.Equal("invoicesGrid", model.InvoicesGrid.GridId);
        }

        [Fact]
        public async Task Index_WithFilterParameters_BoundToViewModel()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            var result = await _controller.Index(project: "PP001", contract: "C001", year: 2025, program: "PROG1");

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<InvoiceViewModel>(view.Model);
            Assert.Equal("PP001",  model.FilterProject);
            Assert.Equal("C001",   model.FilterContract);
            Assert.Equal(2025,     model.FilterYear);
            Assert.Equal("PROG1",  model.FilterProgram);
        }

        #endregion

        // ── LoadInvoiceGrid ────────────────────────────────────────────────────

        #region LoadInvoiceGrid

        [Fact]
        public async Task LoadInvoiceGrid_ServiceReturnsData_ReturnsPartialViewResult()
        {
            // Arrange
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _invoiceService.GetAllAsync(
                    Arg.Any<QueryParameters<string>>(),
                    Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<string?>())
                .Returns(SuccessListResponse());
            _mapper.Map<List<InvoiceItem>>(Arg.Any<List<RadTrackInvoiceDto>>()).Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<ApiMetaDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadInvoiceGrid(DefaultFilter());

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<InvoiceItem>>(partial.Model);
        }

        [Fact]
        public async Task LoadInvoiceGrid_ServiceReturnsEmptyPage_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _invoiceService.GetAllAsync(
                    Arg.Any<QueryParameters<string>>(),
                    Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<string?>())
                .Returns(SuccessListResponse([]));
            _mapper.Map<List<InvoiceItem>>(Arg.Any<List<RadTrackInvoiceDto>>()).Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<ApiMetaDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadInvoiceGrid(DefaultFilter());

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<InvoiceItem>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadInvoiceGrid_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Filter", "Required");

            // Act
            var result = await _controller.LoadInvoiceGrid(DefaultFilter());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var elem = GetJsonElement(jsonResult);
            Assert.False(elem.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── GetInvoiceTotals ───────────────────────────────────────────────────

        #region GetInvoiceTotals

        [Fact]
        public async Task GetInvoiceTotals_ServiceReturnsSuccess_ReturnsPartialView()
        {
            // Arrange
            _invoiceService.GetTotalsAsync(
                    Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<string?>())
                .Returns(SuccessTotalsResponse());
            _mapper.Map<InvoiceTotalsItem>(Arg.Any<RadTrackInvoiceTotalsDto>())
                .Returns(new InvoiceTotalsItem());

            // Act
            var result = await _controller.GetInvoiceTotals();

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_InvoiceTotals", partial.ViewName);
        }

        [Fact]
        public async Task GetInvoiceTotals_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _invoiceService.GetTotalsAsync(
                    Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<string?>())
                .Returns(new ApiResponseDto<RadTrackInvoiceTotalsDto>
                {
                    Success = false,
                    Errors  = [OneError("Totals failed", "TOTALS_ERR")]
                });

            // Act
            var result = await _controller.GetInvoiceTotals();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var elem = GetJsonElement(jsonResult);
            Assert.False(elem.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── GetAddEditInvoicePartial ───────────────────────────────────────────

        #region GetAddEditInvoicePartial

        [Fact]
        public async Task GetAddEditInvoicePartial_NullId_ReturnsPartialViewWithEmptyModel()
        {
            // Arrange
            _projectListService.GetAllProjectsListAsync()
                .Returns(EmptyProjectListResponse());

            // Act
            var result = await _controller.GetAddEditInvoicePartial(null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditInvoice", partial.ViewName);
            var model = Assert.IsType<InvoiceItem>(partial.Model);
            Assert.Equal(0, model.InvoiceCounter); // empty model for Add
        }

        [Fact]
        public async Task GetAddEditInvoicePartial_ValidId_LoadsExistingRecord()
        {
            // Arrange
            const int id = 5;
            var dto    = new RadTrackInvoiceDto { InvoiceCounter = id, Project = "PP001", DueAmount = 1500.0 };
            var item   = new InvoiceItem { InvoiceCounter = id, Project = "PP001", DueAmount = 1500.0 };

            _invoiceService.GetByIdAsync(id)
                .Returns(new ApiResponseDto<RadTrackInvoiceDto> { Success = true, Data = dto });
            _mapper.Map<InvoiceItem>(dto).Returns(item);
            _projectListService.GetAllProjectsListAsync()
                .Returns(EmptyProjectListResponse());

            // Act
            var result = await _controller.GetAddEditInvoicePartial(id);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model   = Assert.IsType<InvoiceItem>(partial.Model);
            Assert.Equal(id,     model.InvoiceCounter);
            Assert.Equal("PP001", model.Project);
        }

        [Fact]
        public async Task GetAddEditInvoicePartial_ServiceReturnsFailure_ReturnsPartialWithEmptyModel()
        {
            // Arrange
            const int id = 99;
            _invoiceService.GetByIdAsync(id)
                .Returns(new ApiResponseDto<RadTrackInvoiceDto> { Success = false });
            _projectListService.GetAllProjectsListAsync()
                .Returns(EmptyProjectListResponse());

            // Act
            var result = await _controller.GetAddEditInvoicePartial(id);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<InvoiceItem>(partial.Model);
            // model remains empty — no mapping called when service fails
            Assert.Equal(0, model.InvoiceCounter);
        }

        #endregion

        // ── SaveInvoice (Create) ───────────────────────────────────────────────

        #region SaveInvoice — Create

        [Fact]
        public async Task SaveInvoice_NewRecord_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var item   = new InvoiceItem { InvoiceCounter = 0, Project = "PP001", DueAmount = 1000.0 };
            var dto    = new RadTrackInvoiceDto { InvoiceCounter = 0, Project = "PP001", DueAmount = 1000.0 };
            var result_dto = new RadTrackInvoiceDto { InvoiceCounter = 10, Project = "PP001" };

            _mapper.Map<RadTrackInvoiceDto>(item).Returns(dto);
            _invoiceService.CreateAsync(dto)
                .Returns(new ApiResponseDto<RadTrackInvoiceDto> { Success = true, Data = result_dto });

            // Act
            var result = await _controller.SaveInvoice(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var elem = GetJsonElement(jsonResult);
            Assert.True(elem.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveInvoice_NewRecord_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var item = new InvoiceItem { InvoiceCounter = 0, Project = "PP001" };
            var dto  = new RadTrackInvoiceDto { InvoiceCounter = 0, Project = "PP001" };

            _mapper.Map<RadTrackInvoiceDto>(item).Returns(dto);
            _invoiceService.CreateAsync(dto)
                .Returns(new ApiResponseDto<RadTrackInvoiceDto>
                {
                    Success = false,
                    Errors  = [OneError("Validation failed", "VALIDATION_ERR")]
                });

            // Act
            var result = await _controller.SaveInvoice(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var elem = GetJsonElement(jsonResult);
            Assert.False(elem.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveInvoice_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Project", "Project is required");
            var item = new InvoiceItem { InvoiceCounter = 0 };

            // Act
            var result = await _controller.SaveInvoice(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var elem = GetJsonElement(jsonResult);
            Assert.False(elem.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── SaveInvoice (Update) ───────────────────────────────────────────────

        #region SaveInvoice — Update

        [Fact]
        public async Task SaveInvoice_ExistingRecord_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var item   = new InvoiceItem { InvoiceCounter = 7, Project = "PP001", DueAmount = 2000.0 };
            var dto    = new RadTrackInvoiceDto { InvoiceCounter = 7, Project = "PP001", DueAmount = 2000.0 };
            var result_dto = new RadTrackInvoiceDto { InvoiceCounter = 7, Project = "PP001" };

            _mapper.Map<RadTrackInvoiceDto>(item).Returns(dto);
            _invoiceService.UpdateAsync(7, dto)
                .Returns(new ApiResponseDto<RadTrackInvoiceDto> { Success = true, Data = result_dto });

            // Act
            var result = await _controller.SaveInvoice(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var elem = GetJsonElement(jsonResult);
            Assert.True(elem.GetProperty("success").GetBoolean());

            await _invoiceService.Received(1).UpdateAsync(7, dto);
        }

        [Fact]
        public async Task SaveInvoice_ExistingRecord_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var item = new InvoiceItem { InvoiceCounter = 7, Project = "PP001" };
            var dto  = new RadTrackInvoiceDto { InvoiceCounter = 7, Project = "PP001" };

            _mapper.Map<RadTrackInvoiceDto>(item).Returns(dto);
            _invoiceService.UpdateAsync(7, dto)
                .Returns(new ApiResponseDto<RadTrackInvoiceDto>
                {
                    Success = false,
                    Errors  = [OneError("Not found", "NOT_FOUND")]
                });

            // Act
            var result = await _controller.SaveInvoice(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var elem = GetJsonElement(jsonResult);
            Assert.False(elem.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── DeleteInvoice ──────────────────────────────────────────────────────

        #region DeleteInvoice

        [Fact]
        public async Task DeleteInvoice_ValidId_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            const int id = 3;
            _invoiceService.DeleteAsync(id).Returns(SuccessDeleteResponse());

            // Act
            var result = await _controller.DeleteInvoice(id);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var elem = GetJsonElement(jsonResult);
            Assert.True(elem.GetProperty("success").GetBoolean());

            await _invoiceService.Received(1).DeleteAsync(id);
        }

        [Fact]
        public async Task DeleteInvoice_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            const int id = 999;
            _invoiceService.DeleteAsync(id)
                .Returns(new ApiResponseDto<object>
                {
                    Success = false,
                    Errors  = [OneError("Not found", "NOT_FOUND")]
                });

            // Act
            var result = await _controller.DeleteInvoice(id);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var elem = GetJsonElement(jsonResult);
            Assert.False(elem.GetProperty("success").GetBoolean());
        }

        #endregion
    }
}
