/*
 * TRANSFORMENGINE MIGRATION — ReportControllerTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New xUnit test class for Apha.PIMS.Api.Controllers.ReportController
 *   - Covers: GetAll, GetById (found/null), Create, Update (route-PK injection), Delete
 *   - Uses NSubstitute for IReportService and IMapper mocks
 *
 * PRESERVED:
 *   - All controller action semantics: OkObjectResult, NotFoundResult, CreatedAtActionResult, Ok({success:true})
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: expand test coverage to include authorization checks once test middleware is set up
 */
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Api.UnitTests.Controllers
{
    public class ReportControllerTests
    {
        private readonly IReportService _service;
        private readonly IMapper _mapper;
        private readonly ReportController _controller;

        public ReportControllerTests()
        {
            _service    = Substitute.For<IReportService>();
            _mapper     = Substitute.For<IMapper>();
            _controller = new ReportController(_service, _mapper);
        }

        // ── helper ────────────────────────────────────────────────────────────────

        private static ReportDto MakeDto(int id = 1) => new ReportDto
        {
            Id          = id,
            Reportname  = $"Report {id}",
            Type        = "R",
            Emailable   = false
        };

        private static ReportRes MakeRes(int id = 1) => new ReportRes
        {
            Id         = id,
            ReportName = $"Report {id}",   // TRANSFORMENGINE: ReportRes uses PascalCase ReportName
            Type       = "R"
        };

        private static ReportReq MakeReq() => new ReportReq
        {
            ReportName = "New Report",     // TRANSFORMENGINE: ReportReq uses PascalCase ReportName
            Type       = "R"
        };

        // ── GetAll ────────────────────────────────────────────────────────────────

        #region GetAll

        [Fact]
        public async Task GetAll_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos  = new List<ReportDto> { MakeDto(1), MakeDto(2) };
            var resList = new List<ReportRes> { MakeRes(1), MakeRes(2) };
            _service.GetAllAsync().Returns(dtos);
            _mapper.Map<List<ReportRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<List<ReportRes>>(ok.Value);
            Assert.Equal(2, returned.Count);
            await _service.Received(1).GetAllAsync();
            _mapper.Received(1).Map<List<ReportRes>>(dtos);
        }

        [Fact]
        public async Task GetAll_ServiceReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos    = new List<ReportDto>();
            var resList = new List<ReportRes>();
            _service.GetAllAsync().Returns(dtos);
            _mapper.Map<List<ReportRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<List<ReportRes>>(ok.Value);
            Assert.Empty(returned);
        }

        [Fact]
        public async Task GetAll_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetAllAsync().ThrowsAsync(new InvalidOperationException("db error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetAll());
        }

        #endregion

        // ── GetById ───────────────────────────────────────────────────────────────

        #region GetById

        [Fact]
        public async Task GetById_ServiceReturnsDto_ReturnsOkWithMappedResult()
        {
            // Arrange
            var dto = MakeDto(3);
            var res = MakeRes(3);
            _service.GetByIdAsync(3).Returns(dto);
            _mapper.Map<ReportRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetById(3);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
            await _service.Received(1).GetByIdAsync(3);
        }

        [Fact]
        public async Task GetById_ServiceReturnsNull_ReturnsNotFound()
        {
            // Arrange
            _service.GetByIdAsync(99).Returns((ReportDto?)null);

            // Act
            var result = await _controller.GetById(99);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetById_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetByIdAsync(Arg.Any<int>()).ThrowsAsync(new Exception("unexpected"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetById(1));
        }

        #endregion

        // ── Create ────────────────────────────────────────────────────────────────

        #region Create

        [Fact]
        public async Task Create_ValidRequest_ReturnsCreatedAtActionWithMappedResult()
        {
            // Arrange
            var req     = MakeReq();
            var dto     = MakeDto(0);
            var created = MakeDto(10);
            var res     = MakeRes(10);
            _mapper.Map<ReportDto>(req).Returns(dto);
            _service.CreateAsync(dto).Returns(created);
            _mapper.Map<ReportRes>(created).Returns(res);

            // Act
            var result = await _controller.Create(req);

            // Assert
            var created201 = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(ReportController.GetById), created201.ActionName);
            Assert.Equal(res, created201.Value);
            _mapper.Received(1).Map<ReportDto>(req);
            await _service.Received(1).CreateAsync(dto);
            _mapper.Received(1).Map<ReportRes>(created);
        }

        [Fact]
        public async Task Create_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _mapper.Map<ReportDto>(Arg.Any<ReportReq>()).Returns(MakeDto(0));
            _service.CreateAsync(Arg.Any<ReportDto>()).ThrowsAsync(new ArgumentException("invalid"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.Create(MakeReq()));
        }

        #endregion

        // ── Update ────────────────────────────────────────────────────────────────

        #region Update

        [Fact]
        public async Task Update_ServiceReturnsDto_ReturnsOkWithMappedResult()
        {
            // Arrange
            var req     = MakeReq();
            var dto     = MakeDto(0);
            var updated = MakeDto(5);
            var res     = MakeRes(5);
            _mapper.Map<ReportDto>(req).Returns(dto);
            _service.UpdateAsync(Arg.Any<ReportDto>()).Returns(updated);
            _mapper.Map<ReportRes>(updated).Returns(res);

            // Act
            var result = await _controller.Update(5, req);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
        }

        [Fact]
        public async Task Update_SetsRoutePkOnDtoBeforeCallingService_DtoIdMatchesRouteId()
        {
            // Arrange — mapper returns a dto with Id=0; controller should set dto.Id = 5 before calling service
            var dto = MakeDto(0);
            _mapper.Map<ReportDto>(Arg.Any<ReportReq>()).Returns(dto);
            _service.UpdateAsync(Arg.Any<ReportDto>()).Returns(MakeDto(5));
            _mapper.Map<ReportRes>(Arg.Any<ReportDto>()).Returns(MakeRes(5));

            // Act
            await _controller.Update(5, MakeReq());

            // Assert — service must receive the dto with Id == 5 (set from route)
            await _service.Received(1).UpdateAsync(Arg.Is<ReportDto>(d => d.Id == 5));
        }

        [Fact]
        public async Task Update_ServiceThrowsKeyNotFoundException_PropagatesException()
        {
            // Arrange
            _mapper.Map<ReportDto>(Arg.Any<ReportReq>()).Returns(MakeDto(0));
            _service.UpdateAsync(Arg.Any<ReportDto>()).ThrowsAsync(new KeyNotFoundException("not found"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.Update(99, MakeReq()));
        }

        #endregion

        // ── Delete ────────────────────────────────────────────────────────────────

        #region Delete

        [Fact]
        public async Task Delete_ServiceCompletes_ReturnsOkWithSuccessTrue()
        {
            // Arrange
            _service.DeleteAsync(7).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(7);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
            // anonymous type { success = true }
            var json    = System.Text.Json.JsonSerializer.Serialize(ok.Value);
            var element = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);
            Assert.True(element.GetProperty("success").GetBoolean());
            await _service.Received(1).DeleteAsync(7);
        }

        [Fact]
        public async Task Delete_ServiceThrowsKeyNotFoundException_PropagatesException()
        {
            // Arrange
            _service.DeleteAsync(Arg.Any<int>()).ThrowsAsync(new KeyNotFoundException("not found"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.Delete(99));
        }

        #endregion
    }
}
