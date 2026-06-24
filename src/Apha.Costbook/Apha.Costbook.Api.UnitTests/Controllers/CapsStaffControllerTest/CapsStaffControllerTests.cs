/*
 * TRANSFORMENGINE MIGRATION — CapsStaffControllerTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New xUnit test class for Apha.Costbook.Api.Controllers.CapsStaffController
 *   - Covers GET /api/v1/capsstaff, GET paginated, GET by mNumber, POST, PUT, DELETE
 *   - Uses NSubstitute for ICapsStaffService and IMapper
 *
 * PRESERVED:
 *   - Test naming convention: [MethodName]_[StateUnderTest]_[ExpectedResult]
 *   - AAA pattern with explicit Arrange/Act/Assert comments
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: none — fully automated.
 */

using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.Costbook.Api.Controllers;
using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.Costbook.Api.UnitTests.Controllers.CapsStaffControllerTest
{
    public class CapsStaffControllerTests
    {
        private readonly ICapsStaffService _service;
        private readonly IMapper _mapper;
        private readonly CapsStaffController _controller;

        public CapsStaffControllerTests()
        {
            _service = Substitute.For<ICapsStaffService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new CapsStaffController(_service, _mapper);
        }

        // ── GetAllCapsStaff ───────────────────────────────────────────────────

        #region GetAllCapsStaff Tests

        [Fact]
        public async Task GetAllCapsStaff_ServiceReturnsList_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = new List<CapsStaffDto>
            {
                new CapsStaffDto { MNumber = "M001", Name = "Alice" },
                new CapsStaffDto { MNumber = "M002", Name = "Bob" }
            };
            var resList = new List<CapsStaffRes>
            {
                new CapsStaffRes { MNumber = "M001", Name = "Alice" },
                new CapsStaffRes { MNumber = "M002", Name = "Bob" }
            };
            _service.GetAllAsync().Returns(dtos);
            _mapper.Map<List<CapsStaffRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAllCapsStaff();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(resList, okResult.Value);
            await _service.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAllCapsStaff_ServiceReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos = new List<CapsStaffDto>();
            var resList = new List<CapsStaffRes>();
            _service.GetAllAsync().Returns(dtos);
            _mapper.Map<List<CapsStaffRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAllCapsStaff();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(resList, okResult.Value);
        }

        #endregion

        // ── GetPaginatedCapsStaff ─────────────────────────────────────────────

        #region GetPaginatedCapsStaff Tests

        [Fact]
        public async Task GetPaginatedCapsStaff_ValidQuery_ReturnsOkWithPaginatedResult()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var queryParams = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PaginatedResult<CapsStaffDto>(
                new List<CapsStaffDto> { new CapsStaffDto { MNumber = "M001", Name = "Alice" } },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });
            var pagedRes = new PaginationRes<CapsStaffRes>();

            _mapper.Map<QueryParameters<string>>(query).Returns(queryParams);
            _service.GetPaginatedAsync(queryParams).Returns(pagedData);
            _mapper.Map<PaginationRes<CapsStaffRes>>(pagedData).Returns(pagedRes);

            // Act
            var result = await _controller.GetPaginatedCapsStaff(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(pagedRes, okResult.Value);
            await _service.Received(1).GetPaginatedAsync(queryParams);
        }

        #endregion

        // ── GetCapsStaff ──────────────────────────────────────────────────────

        #region GetCapsStaff Tests

        [Fact]
        public async Task GetCapsStaff_ExistingMNumber_ReturnsOkWithMappedRes()
        {
            // Arrange
            var mNumber = "M001";
            var dto = new CapsStaffDto { MNumber = mNumber, Name = "Alice" };
            var res = new CapsStaffRes { MNumber = mNumber, Name = "Alice" };
            _service.GetByMNumberAsync(mNumber).Returns(dto);
            _mapper.Map<CapsStaffRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetCapsStaff(mNumber);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(res, okResult.Value);
            await _service.Received(1).GetByMNumberAsync(mNumber);
        }

        [Fact]
        public async Task GetCapsStaff_NonExistentMNumber_ReturnsNotFound()
        {
            // Arrange
            var mNumber = "NOTEXIST";
            _service.GetByMNumberAsync(mNumber).Returns((CapsStaffDto?)null);

            // Act
            var result = await _controller.GetCapsStaff(mNumber);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        // ── AddCapsStaff ──────────────────────────────────────────────────────

        #region AddCapsStaff Tests

        [Fact]
        public async Task AddCapsStaff_ValidRequest_ReturnsCreatedAtActionWithMappedRes()
        {
            // Arrange
            var req = new CapsStaffReq { MNumber = "M003", Name = "Charlie" };
            var dto = new CapsStaffDto { MNumber = "M003", Name = "Charlie" };
            var created = new CapsStaffDto { MNumber = "M003", Name = "Charlie" };
            var res = new CapsStaffRes { MNumber = "M003", Name = "Charlie" };
            _mapper.Map<CapsStaffDto>(req).Returns(dto);
            _service.AddAsync(dto).Returns(created);
            _mapper.Map<CapsStaffRes>(created).Returns(res);

            // Act
            var result = await _controller.AddCapsStaff(req);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetCapsStaff), createdResult.ActionName);
            Assert.Same(res, createdResult.Value);
            await _service.Received(1).AddAsync(dto);
        }

        [Fact]
        public async Task AddCapsStaff_DuplicateMNumber_PropagatesArgumentException()
        {
            // Arrange
            var req = new CapsStaffReq { MNumber = "M001", Name = "Duplicate" };
            var dto = new CapsStaffDto { MNumber = "M001", Name = "Duplicate" };
            _mapper.Map<CapsStaffDto>(req).Returns(dto);
            _service.AddAsync(dto).Throws(new ArgumentException("A CAPS staff member with MNumber 'M001' already exists."));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.AddCapsStaff(req));
        }

        #endregion

        // ── UpdateCapsStaff ───────────────────────────────────────────────────

        #region UpdateCapsStaff Tests

        [Fact]
        public async Task UpdateCapsStaff_ValidRequest_ReturnsOkWithUpdatedRes()
        {
            // Arrange
            var mNumber = "M001";
            var req = new CapsStaffReq { MNumber = mNumber, Name = "Alice Updated" };
            var dto = new CapsStaffDto { MNumber = mNumber, Name = "Alice Updated" };
            var updated = new CapsStaffDto { MNumber = mNumber, Name = "Alice Updated" };
            var res = new CapsStaffRes { MNumber = mNumber, Name = "Alice Updated" };
            _mapper.Map<CapsStaffDto>(req).Returns(dto);
            _service.UpdateAsync(mNumber, dto).Returns(updated);
            _mapper.Map<CapsStaffRes>(updated).Returns(res);

            // Act
            var result = await _controller.UpdateCapsStaff(mNumber, req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(res, okResult.Value);
            await _service.Received(1).UpdateAsync(mNumber, dto);
        }

        [Fact]
        public async Task UpdateCapsStaff_NonExistentMNumber_PropagatesKeyNotFoundException()
        {
            // Arrange
            var mNumber = "NOTEXIST";
            var req = new CapsStaffReq { MNumber = mNumber, Name = "Ghost" };
            var dto = new CapsStaffDto { MNumber = mNumber, Name = "Ghost" };
            _mapper.Map<CapsStaffDto>(req).Returns(dto);
            _service.UpdateAsync(mNumber, dto).Throws(new KeyNotFoundException($"CAPS staff member '{mNumber}' not found."));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.UpdateCapsStaff(mNumber, req));
        }

        #endregion

        // ── DeleteCapsStaff ───────────────────────────────────────────────────

        #region DeleteCapsStaff Tests

        [Fact]
        public async Task DeleteCapsStaff_ExistingMNumber_ReturnsNoContent()
        {
            // Arrange
            var mNumber = "M001";
            _service.DeleteAsync(mNumber).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteCapsStaff(mNumber);

            // Assert
            Assert.IsType<NoContentResult>(result);
            await _service.Received(1).DeleteAsync(mNumber);
        }

        [Fact]
        public async Task DeleteCapsStaff_NonExistentMNumber_PropagatesKeyNotFoundException()
        {
            // Arrange
            var mNumber = "NOTEXIST";
            _service.DeleteAsync(mNumber).Throws(new KeyNotFoundException($"CAPS staff member '{mNumber}' not found."));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.DeleteCapsStaff(mNumber));
        }

        [Fact]
        public async Task DeleteCapsStaff_WhitespaceMNumber_PropagatesArgumentException()
        {
            // Arrange — controller throws ArgumentException before calling the service for blank mNumber
            var mNumber = "   ";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteCapsStaff(mNumber));
            await _service.DidNotReceive().DeleteAsync(Arg.Any<string>());
        }

        #endregion
    }
}
