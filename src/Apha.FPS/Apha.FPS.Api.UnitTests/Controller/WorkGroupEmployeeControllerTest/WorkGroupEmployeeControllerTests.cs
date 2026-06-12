// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — WorkGroupEmployeeControllerTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-06-11
 *
 * CHANGED:
 *   - Phase 13 UPDATE: added #region CreateWorkGroupEmployeeAsync Tests covering
 *     the [HttpPost] CreateWorkGroupEmployeeAsync action added in Phase 5/6.
 *   - Two new test methods:
 *     CreateWorkGroupEmployeeAsync_WithValidRequest_ReturnsCreatedAtAction — happy path
 *     CreateWorkGroupEmployeeAsync_WhenServiceThrows_PropagatesException    — exception path
 *
 * PRESERVED:
 *   - All existing test regions unchanged: GetWorkGroupEmployeeAsync, GetWorkGroupEmployeeByIdAsync,
 *     UpdateWorkGroupEmployeeAsync, DeleteWorkGroupEmployeeAsync, Constructor Tests.
 *   - NSubstitute mock setup and FluentAssertions assertion style unchanged.
 *   - Namespace Apha.FPS.Api.UnitTests.Controller.WorkGroupEmployeeControllerTest unchanged.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm IWorkGroupEmployeeService.CreateWorkGroupEmployeeAsync signature
 *     is stable (returns WorkGroupEmployeeDto, not Task<IActionResult>).
 *   - TRANSFORMENGINE TODO: Verify CreatedAtAction route value key "pactId" matches route template
 *     parameter name in GetWorkGroupEmployeeByIdAsync ([HttpGet("{pactId}")]).
 */

using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.WorkGroupEmployeeControllerTest
{
    public class WorkGroupEmployeeControllerTests
    {
        private const string DefaultWgGrade = "WG01";
        private const string DefaultPactId  = "PACT001";

        private readonly IWorkGroupEmployeeService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly WorkGroupEmployeeController _controller;

        public WorkGroupEmployeeControllerTests()
        {
            _serviceMock = Substitute.For<IWorkGroupEmployeeService>();
            _mapperMock  = Substitute.For<IMapper>();
            _controller  = new WorkGroupEmployeeController(_serviceMock, _mapperMock);
        }

        #region GetWorkGroupEmployeeAsync Tests

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var query  = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mapped = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var employees = new List<WorkGroupEmployeeDto>
            {
                new() { PactId = DefaultPactId, SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var serviceResult = new PaginatedResult<WorkGroupEmployeeDto>(employees, paginationDto);
            var expectedRes   = new PaginationRes<WorkGroupEmployeeRes>
            {
                Data           = new List<WorkGroupEmployeeRes> { new() { PactId = DefaultPactId } },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mapped);
            _serviceMock.GetWorkGroupEmployeeAsync(mapped, DefaultWgGrade).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WorkGroupEmployeeRes>>(serviceResult).Returns(expectedRes);

            // Act
            var result = await _controller.GetWorkGroupEmployeeAsync(query, DefaultWgGrade);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(expectedRes);
            await _serviceMock.Received(1).GetWorkGroupEmployeeAsync(mapped, DefaultWgGrade);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var query  = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mapped = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mapped);
            _serviceMock.GetWorkGroupEmployeeAsync(mapped, DefaultWgGrade)
                .ThrowsAsync(new ArgumentException("Invalid wg grade"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _controller.GetWorkGroupEmployeeAsync(query, DefaultWgGrade));
        }

        #endregion

        #region GetWorkGroupEmployeeByIdAsync Tests

        [Fact]
        public async Task GetWorkGroupEmployeeByIdAsync_WithValidPactId_ReturnsOk()
        {
            // Arrange
            var dto = new WorkGroupEmployeeDto
            {
                PactId         = DefaultPactId,
                SpNumber       = "SP001",
                WorkGroupGrade = DefaultWgGrade
            };
            var expectedRes = new WorkGroupEmployeeRes { PactId = DefaultPactId };

            _serviceMock.GetWorkGroupEmployeeByIdAsync(DefaultPactId).Returns(dto);
            _mapperMock.Map<WorkGroupEmployeeRes>(dto).Returns(expectedRes);

            // Act
            var result = await _controller.GetWorkGroupEmployeeByIdAsync(DefaultPactId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(expectedRes);
            await _serviceMock.Received(1).GetWorkGroupEmployeeByIdAsync(DefaultPactId);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeByIdAsync_WhenNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _serviceMock.GetWorkGroupEmployeeByIdAsync(DefaultPactId).Returns((WorkGroupEmployeeDto?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.GetWorkGroupEmployeeByIdAsync(DefaultPactId));
        }

        #endregion

        #region CreateWorkGroupEmployeeAsync Tests

        // TRANSFORMENGINE: tests for [HttpPost] CreateWorkGroupEmployeeAsync added — Phase 13.
        // Action returns 201 CreatedAtAction pointing to GetWorkGroupEmployeeByIdAsync.

        [Fact]
        public async Task CreateWorkGroupEmployeeAsync_WithValidRequest_ReturnsCreatedAtAction()
        {
            // Arrange
            var req         = new WorkGroupEmployeeReq { PactId = DefaultPactId, HrsPaid = 40.0 };
            var dto         = new WorkGroupEmployeeDto { PactId = DefaultPactId, WorkGroupGrade = DefaultWgGrade };
            var createdDto  = new WorkGroupEmployeeDto { PactId = DefaultPactId, WorkGroupGrade = DefaultWgGrade };
            var expectedRes = new WorkGroupEmployeeRes { PactId = DefaultPactId };

            _mapperMock.Map<WorkGroupEmployeeDto>(req).Returns(dto);
            _serviceMock.CreateWorkGroupEmployeeAsync(dto).Returns(createdDto);
            _mapperMock.Map<WorkGroupEmployeeRes>(createdDto).Returns(expectedRes);

            // Act
            var result = await _controller.CreateWorkGroupEmployeeAsync(req);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            createdResult.ActionName.Should().Be(nameof(WorkGroupEmployeeController.GetWorkGroupEmployeeByIdAsync));
            createdResult.RouteValues.Should().ContainKey("pactId");
            createdResult.RouteValues!["pactId"].Should().Be(DefaultPactId);
            createdResult.Value.Should().Be(expectedRes);
            await _serviceMock.Received(1).CreateWorkGroupEmployeeAsync(dto);
        }

        [Fact]
        public async Task CreateWorkGroupEmployeeAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var req = new WorkGroupEmployeeReq { PactId = DefaultPactId };
            var dto = new WorkGroupEmployeeDto { PactId = DefaultPactId, WorkGroupGrade = DefaultWgGrade };

            _mapperMock.Map<WorkGroupEmployeeDto>(req).Returns(dto);
            _serviceMock.CreateWorkGroupEmployeeAsync(dto)
                .ThrowsAsync(new InvalidOperationException("Duplicate PactId."));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.CreateWorkGroupEmployeeAsync(req));
        }

        #endregion

        #region UpdateWorkGroupEmployeeAsync Tests

        [Fact]
        public async Task UpdateWorkGroupEmployeeAsync_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var req = new WorkGroupEmployeeReq { PactId = DefaultPactId, HrsPaid = 40.0 };
            var dto = new WorkGroupEmployeeDto { PactId = DefaultPactId };
            var updatedDto  = new WorkGroupEmployeeDto { PactId = DefaultPactId };
            var expectedRes = new WorkGroupEmployeeRes { PactId = DefaultPactId };

            _mapperMock.Map<WorkGroupEmployeeDto>(req).Returns(dto);
            _serviceMock.UpdateWorkGroupEmployeeAsync(dto).Returns(updatedDto);
            _mapperMock.Map<WorkGroupEmployeeRes>(updatedDto).Returns(expectedRes);

            // Act
            var result = await _controller.UpdateWorkGroupEmployeeAsync(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(expectedRes);
            await _serviceMock.Received(1).UpdateWorkGroupEmployeeAsync(dto);
        }

        [Fact]
        public async Task UpdateWorkGroupEmployeeAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var req = new WorkGroupEmployeeReq { PactId = DefaultPactId };
            var dto = new WorkGroupEmployeeDto { PactId = DefaultPactId };

            _mapperMock.Map<WorkGroupEmployeeDto>(req).Returns(dto);
            _serviceMock.UpdateWorkGroupEmployeeAsync(dto)
                .ThrowsAsync(new KeyNotFoundException("Employee not found."));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.UpdateWorkGroupEmployeeAsync(req));
        }

        #endregion

        #region DeleteWorkGroupEmployeeAsync Tests

        [Fact]
        public async Task DeleteWorkGroupEmployeeAsync_WithValidPactId_ReturnsOk()
        {
            // Arrange
            _serviceMock.DeleteWorkGroupEmployeeAsync(DefaultPactId).Returns(true);

            // Act
            var result = await _controller.DeleteWorkGroupEmployeeAsync(DefaultPactId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(true, okResult.Value);
            await _serviceMock.Received(1).DeleteWorkGroupEmployeeAsync(DefaultPactId);
        }

        [Fact]
        public async Task DeleteWorkGroupEmployeeAsync_WhenNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _serviceMock.DeleteWorkGroupEmployeeAsync(DefaultPactId).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.DeleteWorkGroupEmployeeAsync(DefaultPactId));
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullService_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new WorkGroupEmployeeController(null!, _mapperMock));
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new WorkGroupEmployeeController(_serviceMock, null!));
        }

        #endregion
    }
}
