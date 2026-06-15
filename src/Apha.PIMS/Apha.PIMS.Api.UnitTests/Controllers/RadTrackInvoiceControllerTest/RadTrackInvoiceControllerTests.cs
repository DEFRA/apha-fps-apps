// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — RadTrackInvoiceControllerTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - New file: xUnit tests for RadTrackInvoiceController (Phase 5).
 *   - Tests cover all 6 API controller actions: GetAll, GetTotals, GetById, Create, Update, Delete.
 *   - NSubstitute used for IRadTrackInvoiceService and IMapper mocks.
 *   - Follows MilestoneControllerTests conventions: constructor mock setup, #region grouping.
 *
 * PRESERVED:
 *   - Naming convention: [MethodName]_[StateUnderTest]_[ExpectedResult].
 *   - Verifies ActionResult surface explicitly (OkObjectResult, CreatedAtActionResult, NotFoundResult).
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify QueryParameters<RadTrackInvoiceFilter> model binding integration
 *     once the backend is built against a live database.
 */

using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Api.UnitTests.Controllers.RadTrackInvoiceControllerTest
{
    public class RadTrackInvoiceControllerTests
    {
        private readonly IRadTrackInvoiceService _service;
        private readonly IMapper _mapper;
        private readonly RadTrackInvoiceController _controller;

        public RadTrackInvoiceControllerTests()
        {
            _service    = Substitute.For<IRadTrackInvoiceService>();
            _mapper     = Substitute.For<IMapper>();
            _controller = new RadTrackInvoiceController(_service, _mapper);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static RadTrackInvoiceDto SampleDto(int id = 1) => new()
        {
            InvoiceCounter = id,
            Project        = "PP001",
            Contract       = "C001",
            DueAmount      = 1000.0,
            DueDate        = DateTime.Today.AddDays(30)
        };

        private static RadTrackInvoiceRes SampleRes(int id = 1) => new()
        {
            InvoiceCounter = id,
            Project        = "PP001",
            Contract       = "C001"
        };

        private static RadTrackInvoiceTotalsDto SampleTotalsDto() => new()
        {
            TotalPlannedAmount = 5000.0,
            TotalDueAmount     = 3000.0,
            TotalActualAmount  = 2000.0
        };

        // ── GetAll ─────────────────────────────────────────────────────────────

        #region GetAll

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithMappedPaginatedResult()
        {
            // Arrange
            var parameters = new QueryParameters<RadTrackInvoiceFilter> { Page = 1, PageSize = 10 };
            var dtos = new List<RadTrackInvoiceDto> { SampleDto(1), SampleDto(2) };
            var paginatedResult = new PaginatedResult<RadTrackInvoiceDto>(dtos, new PaginationDto { TotalRecords = 2 });
            var resList = new List<RadTrackInvoiceRes> { SampleRes(1), SampleRes(2) };
            var paginationRes = new PaginationRes<RadTrackInvoiceRes>(resList, new Pagination { TotalRecords = 2 });

            _service.GetAllAsync(parameters).Returns(paginatedResult);
            _mapper.Map<PaginationRes<RadTrackInvoiceRes>>(paginatedResult).Returns(paginationRes);

            // Act
            var result = await _controller.GetAll(parameters);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paginationRes, okResult.Value);

            await _service.Received(1).GetAllAsync(parameters);
            _mapper.Received(1).Map<PaginationRes<RadTrackInvoiceRes>>(paginatedResult);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithEmptyData()
        {
            // Arrange
            var parameters = new QueryParameters<RadTrackInvoiceFilter> { Page = 1, PageSize = 10 };
            var emptyResult  = new PaginatedResult<RadTrackInvoiceDto>(new List<RadTrackInvoiceDto>(), new PaginationDto());
            var emptyPageRes = new PaginationRes<RadTrackInvoiceRes>();

            _service.GetAllAsync(parameters).Returns(emptyResult);
            _mapper.Map<PaginationRes<RadTrackInvoiceRes>>(emptyResult).Returns(emptyPageRes);

            // Act
            var result = await _controller.GetAll(parameters);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(emptyPageRes, okResult.Value);
        }

        [Fact]
        public async Task GetAll_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var parameters = new QueryParameters<RadTrackInvoiceFilter> { Page = 1, PageSize = 10 };
            _service.GetAllAsync(parameters).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAll(parameters));

            await _service.Received(1).GetAllAsync(parameters);
            _mapper.DidNotReceive().Map<PaginationRes<RadTrackInvoiceRes>>(Arg.Any<PaginatedResult<RadTrackInvoiceDto>>());
        }

        #endregion

        // ── GetTotals ──────────────────────────────────────────────────────────

        #region GetTotals

        [Fact]
        public async Task GetTotals_ReturnsOkResult_WithTotalsDto()
        {
            // Arrange
            var filter = new RadTrackInvoiceFilter { Project = "PP001" };
            var totalsDto = SampleTotalsDto();

            _service.GetTotalsAsync(filter).Returns(totalsDto);

            // Act
            var result = await _controller.GetTotals(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(totalsDto, okResult.Value);

            await _service.Received(1).GetTotalsAsync(filter);
        }

        [Fact]
        public async Task GetTotals_WithNullFilter_ReturnsOkResult()
        {
            // Arrange
            var totalsDto = SampleTotalsDto();
            _service.GetTotalsAsync(null).Returns(totalsDto);

            // Act
            var result = await _controller.GetTotals(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(totalsDto, okResult.Value);
        }

        [Fact]
        public async Task GetTotals_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetTotalsAsync(Arg.Any<RadTrackInvoiceFilter?>()).Throws(new Exception("DB failure"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetTotals(null));
        }

        #endregion

        // ── GetById ────────────────────────────────────────────────────────────

        #region GetById

        [Fact]
        public async Task GetById_ReturnsOkResult_WithMappedDto_WhenRecordExists()
        {
            // Arrange
            const int id = 42;
            var dto = SampleDto(id);
            var res = SampleRes(id);

            _service.GetByIdAsync(id).Returns(dto);
            _mapper.Map<RadTrackInvoiceRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, okResult.Value);

            await _service.Received(1).GetByIdAsync(id);
            _mapper.Received(1).Map<RadTrackInvoiceRes>(dto);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenRecordDoesNotExist()
        {
            // Arrange
            const int id = 999;
            _service.GetByIdAsync(id).Returns((RadTrackInvoiceDto?)null);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);

            await _service.Received(1).GetByIdAsync(id);
            _mapper.DidNotReceive().Map<RadTrackInvoiceRes>(Arg.Any<RadTrackInvoiceDto>());
        }

        [Fact]
        public async Task GetById_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetByIdAsync(Arg.Any<int>()).Throws(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetById(1));
        }

        #endregion

        // ── Create ─────────────────────────────────────────────────────────────

        #region Create

        [Fact]
        public async Task Create_ValidRequest_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var request = new RadTrackInvoiceReq { Project = "PP001", DueAmount = 1000.0, DueDate = DateTime.Today.AddDays(30) };
            var dto     = SampleDto(1);
            var res     = SampleRes(1);

            _mapper.Map<RadTrackInvoiceDto>(request).Returns(dto);
            _service.CreateAsync(dto).Returns(dto);
            _mapper.Map<RadTrackInvoiceRes>(dto).Returns(res);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetById), createdResult.ActionName);
            Assert.Equal(res, createdResult.Value);
            Assert.Equal(dto.InvoiceCounter, (createdResult.RouteValues!["id"]));

            await _service.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task Create_WhenServiceThrowsArgumentException_PropagatesException()
        {
            // Arrange
            var request = new RadTrackInvoiceReq();
            var dto     = new RadTrackInvoiceDto();

            _mapper.Map<RadTrackInvoiceDto>(request).Returns(dto);
            _service.CreateAsync(dto).Throws(new ArgumentException("Project is required.", "dto"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.Create(request));
        }

        [Fact]
        public async Task Create_WhenServiceThrowsInvalidOperationException_PropagatesException()
        {
            // Arrange
            var request = new RadTrackInvoiceReq { Project = "PP001", InvoiceRef = "INV-001" };
            var dto     = new RadTrackInvoiceDto { Project = "PP001", InvoiceRef = "INV-001" };

            _mapper.Map<RadTrackInvoiceDto>(request).Returns(dto);
            _service.CreateAsync(dto).Throws(new InvalidOperationException("Duplicate InvoiceRef"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Create(request));
        }

        #endregion

        // ── Update ─────────────────────────────────────────────────────────────

        #region Update

        [Fact]
        public async Task Update_ValidRequest_ReturnsOkResult_WithMappedResponse()
        {
            // Arrange
            const int id = 5;
            var request  = new RadTrackInvoiceReq { Project = "PP001", DueAmount = 2000.0, DueDate = DateTime.Today.AddDays(15) };
            var dto      = SampleDto(id);
            var updated  = SampleDto(id);
            var res      = SampleRes(id);

            _mapper.Map<RadTrackInvoiceDto>(request).Returns(dto);
            _service.UpdateAsync(Arg.Is<RadTrackInvoiceDto>(d => d.InvoiceCounter == id)).Returns(updated);
            _mapper.Map<RadTrackInvoiceRes>(updated).Returns(res);

            // Act
            var result = await _controller.Update(id, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, okResult.Value);

            // TRANSFORMENGINE: verify route id is applied to dto.InvoiceCounter before service call.
            await _service.Received(1).UpdateAsync(Arg.Is<RadTrackInvoiceDto>(d => d.InvoiceCounter == id));
        }

        [Fact]
        public async Task Update_RouteIdAppliedToDto_BeforeServiceCall()
        {
            // Arrange
            const int routeId = 7;
            var request  = new RadTrackInvoiceReq { Project = "PP001", DueAmount = 500.0, DueDate = DateTime.Today };
            var dto      = new RadTrackInvoiceDto { InvoiceCounter = 0, Project = "PP001" }; // body id not set
            var updated  = SampleDto(routeId);
            var res      = SampleRes(routeId);

            _mapper.Map<RadTrackInvoiceDto>(request).Returns(dto);
            _service.UpdateAsync(Arg.Any<RadTrackInvoiceDto>()).Returns(updated);
            _mapper.Map<RadTrackInvoiceRes>(updated).Returns(res);

            // Act
            await _controller.Update(routeId, request);

            // Assert: route id must override the body dto.InvoiceCounter
            await _service.Received(1).UpdateAsync(Arg.Is<RadTrackInvoiceDto>(d => d.InvoiceCounter == routeId));
        }

        [Fact]
        public async Task Update_WhenServiceThrowsKeyNotFoundException_PropagatesException()
        {
            // Arrange
            const int id = 999;
            var request  = new RadTrackInvoiceReq { Project = "PP001", DueAmount = 100.0, DueDate = DateTime.Today };
            var dto      = SampleDto(id);

            _mapper.Map<RadTrackInvoiceDto>(request).Returns(dto);
            _service.UpdateAsync(Arg.Any<RadTrackInvoiceDto>()).Throws(new KeyNotFoundException("Not found"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.Update(id, request));
        }

        #endregion

        // ── Delete ─────────────────────────────────────────────────────────────

        #region Delete

        [Fact]
        public async Task Delete_ExistingRecord_ReturnsOkWithSuccessTrue()
        {
            // Arrange
            const int id = 3;
            _service.DeleteAsync(id).Returns(true);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value    = okResult.Value!;
            var success  = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            Assert.True(success);

            await _service.Received(1).DeleteAsync(id);
        }

        [Fact]
        public async Task Delete_NonExistentRecord_ReturnsOkWithSuccessFalse()
        {
            // Arrange
            const int id = 999;
            _service.DeleteAsync(id).Returns(false);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value    = okResult.Value!;
            var success  = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            Assert.False(success);
        }

        [Fact]
        public async Task Delete_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.DeleteAsync(Arg.Any<int>()).Throws(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Delete(1));
        }

        #endregion
    }
}
