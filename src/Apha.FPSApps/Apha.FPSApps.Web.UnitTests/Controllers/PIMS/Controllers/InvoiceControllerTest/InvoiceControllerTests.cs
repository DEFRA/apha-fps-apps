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
using NSubstitute.ExceptionExtensions;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PIMS.Controllers.InvoiceControllerTest
{
    public class InvoiceControllerTests
    {
        private readonly IMapper                  _mapper;
        private readonly IRadTrackInvoiceService  _invoiceService;
        private readonly InvoiceController        _controller;

        public InvoiceControllerTests()
        {
            _mapper         = Substitute.For<IMapper>();
            _invoiceService = Substitute.For<IRadTrackInvoiceService>();
            _controller     = new InvoiceController(_mapper, _invoiceService);
        }

        // ── helpers ──────────────────────────────────────────────────────────────

        private static JsonElement GetJsonElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        /// <summary>Sets up all four dropdown lookup service calls.</summary>
        private void SetupDropdownMocks(
            List<string>? projects  = null,
            List<int>?    years     = null,
            List<string>? contracts = null,
            List<string>? programs  = null)
        {
            _invoiceService.GetProjectsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = projects  ?? ["PP001", "PP002"] });
            _invoiceService.GetYearsAsync()
                .Returns(new ApiResponseDto<List<int>>    { Success = true, Data = years     ?? [2020, 2021, 2022] });
            _invoiceService.GetContractsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = contracts ?? ["C001", "C002"] });
            _invoiceService.GetProgramsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = programs  ?? ["PROG1", "PROG2"] });
        }

        /// <summary>Sets up mapper + GetAllAsync used inside BuildInvoiceGridAsync.</summary>
        private void SetupGridMocks(List<RadTrackInvoiceDto>? data = null)
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _invoiceService.GetAllAsync(
                    Arg.Any<QueryParameters<string>>(),
                    Arg.Any<string?>(),
                    Arg.Any<string?>(),
                    Arg.Any<int?>(),
                    Arg.Any<string?>())
                .Returns(new ApiResponseDto<List<RadTrackInvoiceDto>> { Success = true, Data = data ?? [] });
            _mapper.Map<List<InvoiceItem>>(Arg.Any<List<RadTrackInvoiceDto>>()).Returns([]);
        }

        /// <summary>Full setup required for the Index action.</summary>
        private void SetupIndexMocks(
            List<string>?          projects  = null,
            List<int>?             years     = null,
            List<string>?          contracts = null,
            List<string>?          programs  = null,
            List<RadTrackInvoiceDto>? gridData = null)
        {
            SetupDropdownMocks(projects, years, contracts, programs);
            SetupGridMocks(gridData);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_InitializesController()
        {
            var controller = new InvoiceController(_mapper, _invoiceService);
            Assert.NotNull(controller);
        }

        #endregion

        #region Index (GET) Tests

        [Fact]
        public async Task Index_ReturnsViewResult()
        {
            SetupIndexMocks();
            var result = await _controller.Index();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_ReturnsInvoiceViewModel()
        {
            SetupIndexMocks();
            var result = await _controller.Index();
            Assert.IsType<InvoiceViewModel>(Assert.IsType<ViewResult>(result).Model);
        }

        [Fact]
        public async Task Index_CallsGetProjectsAsync_Once()
        {
            SetupIndexMocks();
            await _controller.Index();
            await _invoiceService.Received(1).GetProjectsAsync();
        }

        [Fact]
        public async Task Index_CallsGetYearsAsync_Once()
        {
            SetupIndexMocks();
            await _controller.Index();
            await _invoiceService.Received(1).GetYearsAsync();
        }

        [Fact]
        public async Task Index_CallsGetContractsAsync_Once()
        {
            SetupIndexMocks();
            await _controller.Index();
            await _invoiceService.Received(1).GetContractsAsync();
        }

        [Fact]
        public async Task Index_CallsGetProgramsAsync_Once()
        {
            SetupIndexMocks();
            await _controller.Index();
            await _invoiceService.Received(1).GetProgramsAsync();
        }

        [Fact]
        public async Task Index_WithNoProjectParam_SetsFilterProjectToFirstInList()
        {
            SetupIndexMocks(projects: ["PP001", "PP002"]);
            var result = await _controller.Index();
            var model = Assert.IsType<InvoiceViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("PP001", model.FilterProject);
        }

        [Fact]
        public async Task Index_WithValidProjectParam_UsesProvidedProject()
        {
            SetupIndexMocks(projects: ["PP001", "PP002"]);
            var result = await _controller.Index(project: "PP002");
            var model = Assert.IsType<InvoiceViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("PP002", model.FilterProject);
        }

        [Fact]
        public async Task Index_WithProjectNotInList_FallsBackToFirstProject()
        {
            SetupIndexMocks(projects: ["PP001", "PP002"]);
            var result = await _controller.Index(project: "PP999");
            var model = Assert.IsType<InvoiceViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("PP001", model.FilterProject);
        }

        [Fact]
        public async Task Index_WithEmptyProjectList_SetsFilterProjectToNull()
        {
            SetupIndexMocks(projects: []);
            var result = await _controller.Index();
            var model = Assert.IsType<InvoiceViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Null(model.FilterProject);
        }

        [Fact]
        public async Task Index_WithNoYearParam_ResolvesToMaxPastYear()
        {
            SetupIndexMocks(years: [2020, 2021, 2022]);
            var result = await _controller.Index();
            var model = Assert.IsType<InvoiceViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(2022, model.FilterYear);
        }

        [Fact]
        public async Task Index_WithValidYearParam_UsesProvidedYear()
        {
            SetupIndexMocks(years: [2020, 2021, 2022]);
            var result = await _controller.Index(year: 2021);
            var model = Assert.IsType<InvoiceViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(2021, model.FilterYear);
        }

        [Fact]
        public async Task Index_WithZeroYear_NormalizesToNullAndResolvesFromList()
        {
            SetupIndexMocks(years: [2020, 2021, 2022]);
            var result = await _controller.Index(year: 0);
            var model = Assert.IsType<InvoiceViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(2022, model.FilterYear);
        }

        [Fact]
        public async Task Index_WithNegativeYear_NormalizesToNullAndResolvesFromList()
        {
            SetupIndexMocks(years: [2020, 2021, 2022]);
            var result = await _controller.Index(year: -1);
            var model = Assert.IsType<InvoiceViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(2022, model.FilterYear);
        }

        [Fact]
        public async Task Index_WithEmptyYearList_SetsFilterYearToNull()
        {
            SetupIndexMocks(years: []);
            var result = await _controller.Index();
            var model = Assert.IsType<InvoiceViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Null(model.FilterYear);
        }

        [Fact]
        public async Task Index_WithContractParam_SetsFilterContractOnViewModel()
        {
            SetupIndexMocks();
            var result = await _controller.Index(contract: "C001");
            var model = Assert.IsType<InvoiceViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("C001", model.FilterContract);
        }

        [Fact]
        public async Task Index_WithProgramParam_SetsFilterProgramOnViewModel()
        {
            SetupIndexMocks();
            var result = await _controller.Index(program: "PROG1");
            var model = Assert.IsType<InvoiceViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("PROG1", model.FilterProgram);
        }

        [Fact]
        public async Task Index_ResolvedProject_IsMarkedSelectedInProjectList()
        {
            SetupIndexMocks(projects: ["PP001", "PP002"]);
            var result = await _controller.Index(project: "PP002");
            var model = Assert.IsType<InvoiceViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.True(model.ProjectList.Single(p => p.Value == "PP002").Selected);
            Assert.False(model.ProjectList.Single(p => p.Value == "PP001").Selected);
        }

        [Fact]
        public async Task Index_ResolvedYear_IsMarkedSelectedInYearList()
        {
            SetupIndexMocks(years: [2020, 2021, 2022]);
            var result = await _controller.Index(year: 2021);
            var model = Assert.IsType<InvoiceViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.True(model.YearList.Single(y => y.Value == "2021").Selected);
            Assert.False(model.YearList.Single(y => y.Value == "2022").Selected);
        }

        [Fact]
        public async Task Index_InvoicesGrid_IsNotNull()
        {
            SetupIndexMocks();
            var result = await _controller.Index();
            var model = Assert.IsType<InvoiceViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.NotNull(model.InvoicesGrid);
        }

        [Fact]
        public async Task Index_CallsGetAllAsync_Once()
        {
            SetupIndexMocks();
            await _controller.Index();
            await _invoiceService.Received(1).GetAllAsync(
                Arg.Any<QueryParameters<string>>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<int?>(),
                Arg.Any<string?>());
        }

        [Fact]
        public async Task Index_WhenServiceThrowsException_PropagatesException()
        {
            _invoiceService.GetProjectsAsync().ThrowsAsync(new Exception("Service unavailable"));
            _invoiceService.GetYearsAsync()
                .Returns(new ApiResponseDto<List<int>> { Success = true, Data = [] });
            _invoiceService.GetContractsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });
            _invoiceService.GetProgramsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });

            await Assert.ThrowsAsync<Exception>(() => _controller.Index());
        }

        #endregion

        #region LoadInvoiceGrid (POST) Tests

        [Fact]
        public async Task LoadInvoiceGrid_WithValidRequest_ReturnsPartialViewResult()
        {
            SetupGridMocks();
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var result = await _controller.LoadInvoiceGrid(request);
            Assert.IsType<PartialViewResult>(result);
        }

        [Fact]
        public async Task LoadInvoiceGrid_WithValidRequest_ReturnsDataGridPartialView()
        {
            SetupGridMocks();
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var result = await _controller.LoadInvoiceGrid(request);
            Assert.Equal("_DataGrid", Assert.IsType<PartialViewResult>(result).ViewName);
        }

        [Fact]
        public async Task LoadInvoiceGrid_WithValidRequest_ReturnsDataGridConfigModel()
        {
            SetupGridMocks();
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var result = await _controller.LoadInvoiceGrid(request);
            Assert.IsType<DataGridConfig<InvoiceItem>>(Assert.IsType<PartialViewResult>(result).Model);
        }

        [Fact]
        public async Task LoadInvoiceGrid_WithInvalidModelState_ReturnsJsonResult()
        {
            _controller.ModelState.AddModelError("Page", "Page must be greater than 0.");
            var request = new PaginationFilter<string> { Page = 0, PageSize = 10, Filter = "{}" };
            var result = await _controller.LoadInvoiceGrid(request);
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task LoadInvoiceGrid_WithInvalidModelState_ReturnsFailureJson()
        {
            _controller.ModelState.AddModelError("Page", "Page must be greater than 0.");
            var request = new PaginationFilter<string> { Page = 0, PageSize = 10, Filter = "{}" };
            var result = await _controller.LoadInvoiceGrid(request);
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadInvoiceGrid_WithInvalidModelState_DoesNotCallService()
        {
            _controller.ModelState.AddModelError("Page", "Page must be greater than 0.");
            var request = new PaginationFilter<string> { Page = 0, PageSize = 10, Filter = "{}" };
            await _controller.LoadInvoiceGrid(request);
            await _invoiceService.DidNotReceive().GetAllAsync(
                Arg.Any<QueryParameters<string>>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<int?>(),
                Arg.Any<string?>());
        }

        [Fact]
        public async Task LoadInvoiceGrid_WithValidRequest_CallsGetAllAsync()
        {
            SetupGridMocks();
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            await _controller.LoadInvoiceGrid(request);
            await _invoiceService.Received(1).GetAllAsync(
                Arg.Any<QueryParameters<string>>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<int?>(),
                Arg.Any<string?>());
        }

        [Fact]
        public async Task LoadInvoiceGrid_WithFilters_PassesFiltersToService()
        {
            SetupGridMocks();
            const string project  = "PP001";
            const string contract = "C001";
            const int    year     = 2022;
            const string program  = "PROG1";
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };

            await _controller.LoadInvoiceGrid(request, project, contract, year, program);

            await _invoiceService.Received(1).GetAllAsync(
                Arg.Any<QueryParameters<string>>(),
                project,
                contract,
                year,
                program);
        }

        [Fact]
        public async Task LoadInvoiceGrid_WithZeroYear_NormalizesToNull()
        {
            SetupGridMocks();
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };

            await _controller.LoadInvoiceGrid(request, year: 0);

            await _invoiceService.Received(1).GetAllAsync(
                Arg.Any<QueryParameters<string>>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                null,
                Arg.Any<string?>());
        }

        #endregion

        #region GetInvoiceTotals (GET) Tests

        [Fact]
        public async Task GetInvoiceTotals_WhenServiceSucceeds_ReturnsJsonResult()
        {
            var data      = new RadTrackInvoiceTotalsDto { TotalPlannedAmount = 1000, TotalDueAmount = 800, TotalActualAmount = 750 };
            var totalsItem = new InvoiceTotalsItem         { TotalPlannedAmount = 1000, TotalDueAmount = 800, TotalActualAmount = 750 };
            _invoiceService.GetTotalsAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<string?>())
                .Returns(new ApiResponseDto<RadTrackInvoiceTotalsDto> { Success = true, Data = data });
            _mapper.Map<InvoiceTotalsItem>(data).Returns(totalsItem);

            var result = await _controller.GetInvoiceTotals();
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task GetInvoiceTotals_WithPositiveAmounts_ReturnsFormattedCurrencyStrings()
        {
            var data      = new RadTrackInvoiceTotalsDto { TotalPlannedAmount = 1000, TotalDueAmount = 800, TotalActualAmount = 750 };
            var totalsItem = new InvoiceTotalsItem         { TotalPlannedAmount = 1000, TotalDueAmount = 800, TotalActualAmount = 750 };
            _invoiceService.GetTotalsAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<string?>())
                .Returns(new ApiResponseDto<RadTrackInvoiceTotalsDto> { Success = true, Data = data });
            _mapper.Map<InvoiceTotalsItem>(data).Returns(totalsItem);

            var result  = await _controller.GetInvoiceTotals();
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.StartsWith("£", element.GetProperty("totalPlanned").GetString());
            Assert.StartsWith("£", element.GetProperty("totalDue").GetString());
            Assert.StartsWith("£", element.GetProperty("totalInvoiced").GetString());
        }

        [Fact]
        public async Task GetInvoiceTotals_WithZeroAmounts_ReturnsEmptyStrings()
        {
            var data      = new RadTrackInvoiceTotalsDto { TotalPlannedAmount = 0, TotalDueAmount = 0, TotalActualAmount = 0 };
            var totalsItem = new InvoiceTotalsItem         { TotalPlannedAmount = 0, TotalDueAmount = 0, TotalActualAmount = 0 };
            _invoiceService.GetTotalsAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<string?>())
                .Returns(new ApiResponseDto<RadTrackInvoiceTotalsDto> { Success = true, Data = data });
            _mapper.Map<InvoiceTotalsItem>(data).Returns(totalsItem);

            var result  = await _controller.GetInvoiceTotals();
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.Equal("", element.GetProperty("totalPlanned").GetString());
            Assert.Equal("", element.GetProperty("totalDue").GetString());
            Assert.Equal("", element.GetProperty("totalInvoiced").GetString());
        }

        [Fact]
        public async Task GetInvoiceTotals_WithNegativeAmounts_ReturnsNegativeCurrencyStrings()
        {
            var data      = new RadTrackInvoiceTotalsDto { TotalPlannedAmount = -500, TotalDueAmount = -400, TotalActualAmount = -300 };
            var totalsItem = new InvoiceTotalsItem         { TotalPlannedAmount = -500, TotalDueAmount = -400, TotalActualAmount = -300 };
            _invoiceService.GetTotalsAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<string?>())
                .Returns(new ApiResponseDto<RadTrackInvoiceTotalsDto> { Success = true, Data = data });
            _mapper.Map<InvoiceTotalsItem>(data).Returns(totalsItem);

            var result  = await _controller.GetInvoiceTotals();
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.StartsWith("-£", element.GetProperty("totalPlanned").GetString());
            Assert.StartsWith("-£", element.GetProperty("totalDue").GetString());
            Assert.StartsWith("-£", element.GetProperty("totalInvoiced").GetString());
        }

        [Fact]
        public async Task GetInvoiceTotals_WhenServiceFails_ReturnsFailureJson()
        {
            _invoiceService.GetTotalsAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<string?>())
                .Returns(new ApiResponseDto<RadTrackInvoiceTotalsDto>
                {
                    Success = false,
                    Errors  = [new ApiErrorDto { Message = "Failed", Code = "ERR" }]
                });

            var result  = await _controller.GetInvoiceTotals();
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetInvoiceTotals_WhenDataIsNull_ReturnsFailureJson()
        {
            _invoiceService.GetTotalsAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<string?>())
                .Returns(new ApiResponseDto<RadTrackInvoiceTotalsDto> { Success = true, Data = null });

            var result  = await _controller.GetInvoiceTotals();
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetInvoiceTotals_WithFilters_PassesFiltersToService()
        {
            const string project  = "PP001";
            const string contract = "C001";
            const int    year     = 2022;
            const string program  = "PROG1";
            var data       = new RadTrackInvoiceTotalsDto { TotalPlannedAmount = 1000 };
            var totalsItem = new InvoiceTotalsItem         { TotalPlannedAmount = 1000 };
            _invoiceService.GetTotalsAsync(project, contract, year, program)
                .Returns(new ApiResponseDto<RadTrackInvoiceTotalsDto> { Success = true, Data = data });
            _mapper.Map<InvoiceTotalsItem>(data).Returns(totalsItem);

            await _controller.GetInvoiceTotals(project, contract, year, program);

            await _invoiceService.Received(1).GetTotalsAsync(project, contract, year, program);
        }

        [Fact]
        public async Task GetInvoiceTotals_WithZeroYear_NormalizesToNull()
        {
            _invoiceService.GetTotalsAsync(Arg.Any<string?>(), Arg.Any<string?>(), null, Arg.Any<string?>())
                .Returns(new ApiResponseDto<RadTrackInvoiceTotalsDto> { Success = false });

            await _controller.GetInvoiceTotals(year: 0);

            await _invoiceService.Received(1).GetTotalsAsync(
                Arg.Any<string?>(), Arg.Any<string?>(), null, Arg.Any<string?>());
        }

        #endregion

        #region GetAddEditInvoicePartial (GET) Tests

        [Fact]
        public async Task GetAddEditInvoicePartial_WithNullId_ReturnsPartialViewResult()
        {
            _invoiceService.GetProjectsAsync().Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });
            _invoiceService.GetContractsAsync().Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });

            var result = await _controller.GetAddEditInvoicePartial(null);
            Assert.IsType<PartialViewResult>(result);
        }

        [Fact]
        public async Task GetAddEditInvoicePartial_WithNullId_ReturnsAddEditPartialView()
        {
            _invoiceService.GetProjectsAsync().Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });
            _invoiceService.GetContractsAsync().Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });

            var result = await _controller.GetAddEditInvoicePartial(null);
            Assert.Equal("_AddEditInvoice", Assert.IsType<PartialViewResult>(result).ViewName);
        }

        [Fact]
        public async Task GetAddEditInvoicePartial_WithNullId_ReturnsNewEmptyInvoiceItem()
        {
            _invoiceService.GetProjectsAsync().Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });
            _invoiceService.GetContractsAsync().Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });

            var result = await _controller.GetAddEditInvoicePartial(null);
            var model = Assert.IsType<InvoiceItem>(Assert.IsType<PartialViewResult>(result).Model);
            Assert.Equal(0, model.InvoiceCounter);
        }

        [Fact]
        public async Task GetAddEditInvoicePartial_WithZeroId_DoesNotCallGetByIdAsync()
        {
            _invoiceService.GetProjectsAsync().Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });
            _invoiceService.GetContractsAsync().Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });

            await _controller.GetAddEditInvoicePartial(0);
            await _invoiceService.DidNotReceive().GetByIdAsync(Arg.Any<int>());
        }

        [Fact]
        public async Task GetAddEditInvoicePartial_WithNullId_SetsIsAddingNewToTrue()
        {
            _invoiceService.GetProjectsAsync().Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });
            _invoiceService.GetContractsAsync().Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });

            await _controller.GetAddEditInvoicePartial(null);
            Assert.True((bool)_controller.ViewBag.IsAddingNew);
        }

        [Fact]
        public async Task GetAddEditInvoicePartial_WithValidId_CallsGetByIdAsync()
        {
            const int id  = 5;
            var dto  = new RadTrackInvoiceDto { InvoiceCounter = id, Project = "PP001" };
            _invoiceService.GetByIdAsync(id).Returns(new ApiResponseDto<RadTrackInvoiceDto> { Success = true, Data = dto });
            _mapper.Map<InvoiceItem>(dto).Returns(new InvoiceItem { InvoiceCounter = id, Project = "PP001" });
            _invoiceService.GetProjectsAsync().Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });
            _invoiceService.GetContractsAsync().Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });

            await _controller.GetAddEditInvoicePartial(id);
            await _invoiceService.Received(1).GetByIdAsync(id);
        }

        [Fact]
        public async Task GetAddEditInvoicePartial_WithValidId_ReturnsMappedModel()
        {
            const int id   = 5;
            var dto  = new RadTrackInvoiceDto { InvoiceCounter = id, Project = "PP001" };
            var item = new InvoiceItem         { InvoiceCounter = id, Project = "PP001" };
            _invoiceService.GetByIdAsync(id).Returns(new ApiResponseDto<RadTrackInvoiceDto> { Success = true, Data = dto });
            _mapper.Map<InvoiceItem>(dto).Returns(item);
            _invoiceService.GetProjectsAsync().Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });
            _invoiceService.GetContractsAsync().Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });

            var result = await _controller.GetAddEditInvoicePartial(id);
            var model = Assert.IsType<InvoiceItem>(Assert.IsType<PartialViewResult>(result).Model);
            Assert.Equal(id,      model.InvoiceCounter);
            Assert.Equal("PP001", model.Project);
        }

        [Fact]
        public async Task GetAddEditInvoicePartial_WithValidId_SetsIsAddingNewToFalse()
        {
            const int id = 5;
            var dto = new RadTrackInvoiceDto { InvoiceCounter = id };
            _invoiceService.GetByIdAsync(id).Returns(new ApiResponseDto<RadTrackInvoiceDto> { Success = true, Data = dto });
            _mapper.Map<InvoiceItem>(dto).Returns(new InvoiceItem { InvoiceCounter = id });
            _invoiceService.GetProjectsAsync().Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });
            _invoiceService.GetContractsAsync().Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });

            await _controller.GetAddEditInvoicePartial(id);
            Assert.False((bool)_controller.ViewBag.IsAddingNew);
        }

        [Fact]
        public async Task GetAddEditInvoicePartial_SetsViewBagProjectList()
        {
            _invoiceService.GetProjectsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = ["PP001", "PP002"] });
            _invoiceService.GetContractsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });

            await _controller.GetAddEditInvoicePartial(null);
            Assert.NotNull(_controller.ViewBag.ProjectList);
        }

        [Fact]
        public async Task GetAddEditInvoicePartial_SetsViewBagContractList()
        {
            _invoiceService.GetProjectsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = [] });
            _invoiceService.GetContractsAsync()
                .Returns(new ApiResponseDto<List<string>> { Success = true, Data = ["C001", "C002"] });

            await _controller.GetAddEditInvoicePartial(null);
            Assert.NotNull(_controller.ViewBag.ContractList);
        }

        #endregion

        #region SaveInvoice (POST) Tests

        [Fact]
        public async Task SaveInvoice_WithInvalidModelState_ReturnsJsonResult()
        {
            _controller.ModelState.AddModelError("Project", "Project is required.");
            var result = await _controller.SaveInvoice(new InvoiceItem());
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task SaveInvoice_WithInvalidModelState_ReturnsFailureJson()
        {
            _controller.ModelState.AddModelError("Project", "Project is required.");
            var result  = await _controller.SaveInvoice(new InvoiceItem());
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveInvoice_WithInvalidModelState_DoesNotCallService()
        {
            _controller.ModelState.AddModelError("Project", "Project is required.");
            await _controller.SaveInvoice(new InvoiceItem());
            await _invoiceService.DidNotReceive().CreateAsync(Arg.Any<RadTrackInvoiceDto>());
            await _invoiceService.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<RadTrackInvoiceDto>());
        }

        [Fact]
        public async Task SaveInvoice_WithNewInvoice_CallsCreateAsync()
        {
            var item = new InvoiceItem { InvoiceCounter = 0, Project = "PP001" };
            var dto  = new RadTrackInvoiceDto { Project = "PP001" };
            _mapper.Map<RadTrackInvoiceDto>(item).Returns(dto);
            _invoiceService.CreateAsync(dto)
                .Returns(new ApiResponseDto<RadTrackInvoiceDto> { Success = true, Data = dto });

            await _controller.SaveInvoice(item);
            await _invoiceService.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task SaveInvoice_WithNewInvoice_DoesNotCallUpdateAsync()
        {
            var item = new InvoiceItem { InvoiceCounter = 0, Project = "PP001" };
            var dto  = new RadTrackInvoiceDto { Project = "PP001" };
            _mapper.Map<RadTrackInvoiceDto>(item).Returns(dto);
            _invoiceService.CreateAsync(dto)
                .Returns(new ApiResponseDto<RadTrackInvoiceDto> { Success = true, Data = dto });

            await _controller.SaveInvoice(item);
            await _invoiceService.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<RadTrackInvoiceDto>());
        }

        [Fact]
        public async Task SaveInvoice_WithNewInvoice_WhenServiceSucceeds_ReturnsSuccessJson()
        {
            var item = new InvoiceItem { InvoiceCounter = 0, Project = "PP001" };
            var dto  = new RadTrackInvoiceDto { Project = "PP001" };
            _mapper.Map<RadTrackInvoiceDto>(item).Returns(dto);
            _invoiceService.CreateAsync(dto)
                .Returns(new ApiResponseDto<RadTrackInvoiceDto> { Success = true, Data = dto });

            var result  = await _controller.SaveInvoice(item);
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveInvoice_WithNewInvoice_WhenServiceSucceeds_ReturnsCreatedMessage()
        {
            var item = new InvoiceItem { InvoiceCounter = 0, Project = "PP001" };
            var dto  = new RadTrackInvoiceDto { Project = "PP001" };
            _mapper.Map<RadTrackInvoiceDto>(item).Returns(dto);
            _invoiceService.CreateAsync(dto)
                .Returns(new ApiResponseDto<RadTrackInvoiceDto> { Success = true, Data = dto });

            var result  = await _controller.SaveInvoice(item);
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.Equal("Invoice created successfully.", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task SaveInvoice_WithNewInvoice_WhenServiceFails_ReturnsFailureJson()
        {
            var item = new InvoiceItem { InvoiceCounter = 0, Project = "PP001" };
            var dto  = new RadTrackInvoiceDto { Project = "PP001" };
            _mapper.Map<RadTrackInvoiceDto>(item).Returns(dto);
            _invoiceService.CreateAsync(dto)
                .Returns(new ApiResponseDto<RadTrackInvoiceDto>
                {
                    Success = false,
                    Errors  = [new ApiErrorDto { Message = "Duplicate", Code = "DUPLICATE" }]
                });

            var result  = await _controller.SaveInvoice(item);
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveInvoice_WithExistingInvoice_CallsUpdateAsync()
        {
            const int id = 5;
            var item = new InvoiceItem         { InvoiceCounter = id, Project = "PP001" };
            var dto  = new RadTrackInvoiceDto  { InvoiceCounter = id, Project = "PP001" };
            _mapper.Map<RadTrackInvoiceDto>(item).Returns(dto);
            _invoiceService.UpdateAsync(id, dto)
                .Returns(new ApiResponseDto<RadTrackInvoiceDto> { Success = true, Data = dto });

            await _controller.SaveInvoice(item);
            await _invoiceService.Received(1).UpdateAsync(id, dto);
        }

        [Fact]
        public async Task SaveInvoice_WithExistingInvoice_DoesNotCallCreateAsync()
        {
            const int id = 5;
            var item = new InvoiceItem        { InvoiceCounter = id, Project = "PP001" };
            var dto  = new RadTrackInvoiceDto { InvoiceCounter = id, Project = "PP001" };
            _mapper.Map<RadTrackInvoiceDto>(item).Returns(dto);
            _invoiceService.UpdateAsync(id, dto)
                .Returns(new ApiResponseDto<RadTrackInvoiceDto> { Success = true, Data = dto });

            await _controller.SaveInvoice(item);
            await _invoiceService.DidNotReceive().CreateAsync(Arg.Any<RadTrackInvoiceDto>());
        }

        [Fact]
        public async Task SaveInvoice_WithExistingInvoice_WhenServiceSucceeds_ReturnsSuccessJson()
        {
            const int id = 5;
            var item = new InvoiceItem        { InvoiceCounter = id, Project = "PP001" };
            var dto  = new RadTrackInvoiceDto { InvoiceCounter = id, Project = "PP001" };
            _mapper.Map<RadTrackInvoiceDto>(item).Returns(dto);
            _invoiceService.UpdateAsync(id, dto)
                .Returns(new ApiResponseDto<RadTrackInvoiceDto> { Success = true, Data = dto });

            var result  = await _controller.SaveInvoice(item);
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveInvoice_WithExistingInvoice_WhenServiceSucceeds_ReturnsUpdatedMessage()
        {
            const int id = 5;
            var item = new InvoiceItem        { InvoiceCounter = id, Project = "PP001" };
            var dto  = new RadTrackInvoiceDto { InvoiceCounter = id, Project = "PP001" };
            _mapper.Map<RadTrackInvoiceDto>(item).Returns(dto);
            _invoiceService.UpdateAsync(id, dto)
                .Returns(new ApiResponseDto<RadTrackInvoiceDto> { Success = true, Data = dto });

            var result  = await _controller.SaveInvoice(item);
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.Equal("Invoice updated successfully.", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task SaveInvoice_WithExistingInvoice_WhenServiceFails_ReturnsFailureJson()
        {
            const int id = 5;
            var item = new InvoiceItem        { InvoiceCounter = id, Project = "PP001" };
            var dto  = new RadTrackInvoiceDto { InvoiceCounter = id, Project = "PP001" };
            _mapper.Map<RadTrackInvoiceDto>(item).Returns(dto);
            _invoiceService.UpdateAsync(id, dto)
                .Returns(new ApiResponseDto<RadTrackInvoiceDto>
                {
                    Success = false,
                    Errors  = [new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" }]
                });

            var result  = await _controller.SaveInvoice(item);
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveInvoice_MapsInvoiceItemToDtoBeforeCallingService()
        {
            var item = new InvoiceItem { InvoiceCounter = 0, Project = "PP001" };
            var dto  = new RadTrackInvoiceDto { Project = "PP001" };
            _mapper.Map<RadTrackInvoiceDto>(item).Returns(dto);
            _invoiceService.CreateAsync(dto)
                .Returns(new ApiResponseDto<RadTrackInvoiceDto> { Success = true, Data = dto });

            await _controller.SaveInvoice(item);
            _mapper.Received(1).Map<RadTrackInvoiceDto>(item);
        }

        #endregion

        #region DeleteInvoice (DELETE) Tests

        [Fact]
        public async Task DeleteInvoice_WhenServiceSucceeds_ReturnsJsonResult()
        {
            const int id = 1;
            _invoiceService.DeleteAsync(id).Returns(new ApiResponseDto<object> { Success = true });

            var result = await _controller.DeleteInvoice(id);
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task DeleteInvoice_WhenServiceSucceeds_ReturnsSuccessJson()
        {
            const int id = 1;
            _invoiceService.DeleteAsync(id).Returns(new ApiResponseDto<object> { Success = true });

            var result  = await _controller.DeleteInvoice(id);
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteInvoice_WhenServiceSucceeds_ReturnsDeletedMessage()
        {
            const int id = 1;
            _invoiceService.DeleteAsync(id).Returns(new ApiResponseDto<object> { Success = true });

            var result  = await _controller.DeleteInvoice(id);
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.Equal("Invoice deleted successfully.", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task DeleteInvoice_WhenServiceFails_ReturnsFailureJson()
        {
            const int id = 99;
            _invoiceService.DeleteAsync(id)
                .Returns(new ApiResponseDto<object>
                {
                    Success = false,
                    Errors  = [new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" }]
                });

            var result  = await _controller.DeleteInvoice(id);
            var element = GetJsonElement(Assert.IsType<JsonResult>(result));
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteInvoice_CallsDeleteAsyncWithCorrectId()
        {
            const int id = 7;
            _invoiceService.DeleteAsync(id).Returns(new ApiResponseDto<object> { Success = true });

            await _controller.DeleteInvoice(id);
            await _invoiceService.Received(1).DeleteAsync(id);
        }

        #endregion
    }
}
