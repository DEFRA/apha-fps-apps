using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Handler;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.BulkRatesControllerTest
{
    /// <summary>
    /// First test class for <see cref="BulkRatesController"/> (this Web
    /// controller previously had zero coverage). Scoped to the request-scoped download
    /// actions for Staff/Animal — the controller's much larger pre-existing action surface
    /// (queue/create/upload/release/approve/etc.) predates this plan and is a separate concern.
    /// </summary>
    public class BulkRatesControllerTests
    {
        private readonly IBulkRatesService _bulkRatesService;
        private readonly ILogger<BulkRatesController> _logger;
        private readonly IFpsYearContext _fpsYearContext;
        private readonly IMapper _mapper;
        private readonly ITempDataDictionary _tempData;
        private readonly BulkRatesController _sut;

        public BulkRatesControllerTests()
        {
            _bulkRatesService = Substitute.For<IBulkRatesService>();
            _logger = Substitute.For<ILogger<BulkRatesController>>();
            _fpsYearContext = Substitute.For<IFpsYearContext>();
            _mapper = Substitute.For<IMapper>();

            _sut = new BulkRatesController(_bulkRatesService, _logger, _fpsYearContext, _mapper);
            _tempData = Substitute.For<ITempDataDictionary>();
            _sut.TempData = _tempData;
            // Index/Create read User (claims) via GetCurrentUserEmail() — needs a real HttpContext,
            // not just a mocked service layer, or ControllerBase.User NREs on an unset HttpContext.
            _sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        }

        [Fact]
        public async Task DownloadStaffTestDataForRequest_WhenServiceSucceeds_ReturnsFileWithRequestScopedName()
        {
            var id = Guid.NewGuid();
            var bytes = new byte[] { 1, 2, 3 };
            _bulkRatesService.DownloadStaffTestDataForRequestAsync(id).Returns(bytes);

            var result = await _sut.DownloadStaffTestDataForRequest(id);

            var file = Assert.IsType<FileContentResult>(result);
            Assert.Same(bytes, file.FileContents);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.ContentType);
            Assert.Equal($"Staff_Rates_{id}.xlsx", file.FileDownloadName);
        }

        [Fact]
        public async Task DownloadStaffTestDataForRequest_WhenServiceThrows_RedirectsToDetailWithErrorMessage()
        {
            var id = Guid.NewGuid();
            _bulkRatesService.DownloadStaffTestDataForRequestAsync(id)
                .Returns(Task.FromException<byte[]>(new InvalidOperationException("boom")));

            var result = await _sut.DownloadStaffTestDataForRequest(id);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(BulkRatesController.Detail), redirect.ActionName);
            Assert.Equal(id, redirect.RouteValues!["id"]);
            _tempData.Received()["ErrorMessage"] = "The Staff test data could not be downloaded. Please try again.";
        }

        [Fact]
        public async Task DownloadAnimalTestDataForRequest_WhenServiceSucceeds_ReturnsFileWithRequestScopedName()
        {
            var id = Guid.NewGuid();
            var bytes = new byte[] { 4, 5, 6 };
            _bulkRatesService.DownloadAnimalTestDataForRequestAsync(id).Returns(bytes);

            var result = await _sut.DownloadAnimalTestDataForRequest(id);

            var file = Assert.IsType<FileContentResult>(result);
            Assert.Same(bytes, file.FileContents);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.ContentType);
            Assert.Equal($"Animal_Rates_{id}.xlsx", file.FileDownloadName);
        }

        [Fact]
        public async Task DownloadAnimalTestDataForRequest_WhenServiceThrows_RedirectsToDetailWithErrorMessage()
        {
            var id = Guid.NewGuid();
            _bulkRatesService.DownloadAnimalTestDataForRequestAsync(id)
                .Returns(Task.FromException<byte[]>(new InvalidOperationException("boom")));

            var result = await _sut.DownloadAnimalTestDataForRequest(id);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(BulkRatesController.Detail), redirect.ActionName);
            Assert.Equal(id, redirect.RouteValues!["id"]);
            _tempData.Received()["ErrorMessage"] = "The Animal test data could not be downloaded. Please try again.";
        }

        // ── CanInitiateRequestAsync-driven behaviour (Index/Create/poll) ─────────
        // Every case below covers all three ApiResponseDto<bool> outcomes, not just true/false —
        // Success == false must never be read as "a request exists".

        private void StubGridLoad()
        {
            // GetBulkRatesGridConfigAsync is called unconditionally by BuildIndexViewAsync;
            // an empty-but-successful response keeps it out of the way of these tests.
            _bulkRatesService.GetRequestsAsync(
                    Arg.Any<Apha.FPSApps.Application.Pagination.QueryParameters<string>>(),
                    Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<string?>())
                .Returns(ApiResponseDto<List<BulkRatesQueueEntryDto>>.SuccessResponse(new List<BulkRatesQueueEntryDto>()));
        }

        [Fact]
        public async Task Index_WhenCanInitiateRequestTrue_SetsViewModelAccordingly()
        {
            StubGridLoad();
            _bulkRatesService.CanInitiateRequestAsync(BulkRatesController.JobNameFec)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            var result = await _sut.Index(BulkRatesController.JobNameFec);

            var vm = Assert.IsType<BulkRatesQueueViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.True(vm.CanInitiateRequest);
            Assert.Null(vm.ActiveRequestCheckError);
        }

        [Fact]
        public async Task Index_WhenCanInitiateRequestFalse_SetsBlockedStateWithNoCheckError()
        {
            StubGridLoad();
            _bulkRatesService.CanInitiateRequestAsync(BulkRatesController.JobNameFec)
                .Returns(ApiResponseDto<bool>.SuccessResponse(false));

            var result = await _sut.Index(BulkRatesController.JobNameFec);

            var vm = Assert.IsType<BulkRatesQueueViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.False(vm.CanInitiateRequest);
            Assert.Null(vm.ActiveRequestCheckError);
        }

        [Fact]
        public async Task Index_WhenCanInitiateRequestCheckFails_FailsClosedWithDistinctErrorMessage()
        {
            StubGridLoad();
            _bulkRatesService.CanInitiateRequestAsync(BulkRatesController.JobNameFec)
                .Returns(ApiResponseDto<bool>.FailureResponse(null!, new ApiMetaDto()));

            var result = await _sut.Index(BulkRatesController.JobNameFec);

            var vm = Assert.IsType<BulkRatesQueueViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.False(vm.CanInitiateRequest); // fail closed
            Assert.NotNull(vm.ActiveRequestCheckError);
            Assert.DoesNotContain("already exists", vm.ActiveRequestCheckError); // never the false message
        }

        [Fact]
        public async Task Create_WhenCanInitiateRequestTrue_ReturnsCreateView()
        {
            _bulkRatesService.CanInitiateRequestAsync(BulkRatesController.JobNameFec)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));
            _fpsYearContext.YearStatus.Returns("Open");

            var result = await _sut.Create(BulkRatesController.JobNameFec);

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_WhenCanInitiateRequestFalse_RedirectsToIndexWithActiveRequestMessage()
        {
            _bulkRatesService.CanInitiateRequestAsync(BulkRatesController.JobNameFec)
                .Returns(ApiResponseDto<bool>.SuccessResponse(false));

            var result = await _sut.Create(BulkRatesController.JobNameFec);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(BulkRatesController.Fec), redirect.ActionName);
            _tempData.Received()["ErrorMessage"] =
                "An active BulkTestRatesUpdate request already exists. Complete, reject, or cancel it before creating a new one.";
        }

        [Fact]
        public async Task Create_WhenCanInitiateRequestCheckFails_RedirectsWithDistinctErrorMessage_NotActiveRequestMessage()
        {
            _bulkRatesService.CanInitiateRequestAsync(BulkRatesController.JobNameFec)
                .Returns(ApiResponseDto<bool>.FailureResponse(null!, new ApiMetaDto()));

            var result = await _sut.Create(BulkRatesController.JobNameFec);

            Assert.IsType<RedirectToActionResult>(result);
            _tempData.Received()["ErrorMessage"] = "Unable to determine request status. Please try again.";
        }

        [Fact]
        public async Task CanInitiateRequest_Ajax_WhenServiceSucceeds_ReturnsSuccessAndCanInitiateInPayload()
        {
            _bulkRatesService.CanInitiateRequestAsync(BulkRatesController.JobNameFec)
                .Returns(ApiResponseDto<bool>.SuccessResponse(false));

            var result = await _sut.CanInitiateRequest(BulkRatesController.JobNameFec);

            var json = Assert.IsType<JsonResult>(result);
            Assert.Equal(true, json.Value!.GetType().GetProperty("success")!.GetValue(json.Value));
            Assert.Equal(false, json.Value!.GetType().GetProperty("canInitiate")!.GetValue(json.Value));
        }

        [Fact]
        public async Task CanInitiateRequest_Ajax_WhenServiceFails_ReturnsSuccessFalse_NotCanInitiateFalse()
        {
            _bulkRatesService.CanInitiateRequestAsync(BulkRatesController.JobNameFec)
                .Returns(ApiResponseDto<bool>.FailureResponse(null!, new ApiMetaDto()));

            var result = await _sut.CanInitiateRequest(BulkRatesController.JobNameFec);

            var json = Assert.IsType<JsonResult>(result);
            Assert.Equal(false, json.Value!.GetType().GetProperty("success")!.GetValue(json.Value));
        }
    }
}
