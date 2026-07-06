/*
 * TRANSFORMENGINE MIGRATION — ProjectManagerControllerTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New xUnit test class for Apha.PIMS.Api.Controllers.ProjectManagerController
 *   - Natural varchar PK (projectmanager name string) with URL-decode semantics
 *   - Covers: GetAll, GetById (found/null), Create, Update (PK injection), Delete
 *   - Uses NSubstitute for IProjectManagerService and IMapper mocks
 *
 * PRESERVED:
 *   - Natural varchar PK semantics; name is URL-decoded before service calls
 *   - Duplicate-name guard via InvalidOperationException propagation
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
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
    public class ProjectManagerControllerTests
    {
        private readonly IProjectManagerService _service;
        private readonly IMapper _mapper;
        private readonly ProjectManagerController _controller;

        public ProjectManagerControllerTests()
        {
            _service    = Substitute.For<IProjectManagerService>();
            _mapper     = Substitute.For<IMapper>();
            _controller = new ProjectManagerController(_service, _mapper);
        }

        // ── helpers ───────────────────────────────────────────────────────────────

        private static ProjectManagerDto MakeDto(string name = "J. Smith") =>
            new ProjectManagerDto { Projectmanager = name, Email = "j.smith@apha.gov.uk", Disable = false };

        private static ProjectManagerRes MakeRes(string name = "J. Smith") =>
            new ProjectManagerRes { ProjectManager = name, Email = "j.smith@apha.gov.uk", Disable = false };

        private static ProjectManagerReq MakeReq(string name = "J. Smith") =>
            new ProjectManagerReq { ProjectManager = name, Email = "j.smith@apha.gov.uk", Disable = false };

        // ── GetAll ────────────────────────────────────────────────────────────────

        #region GetAll

        [Fact]
        public async Task GetAll_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos    = new List<ProjectManagerDto> { MakeDto("Smith"), MakeDto("Jones") };
            var resList = new List<ProjectManagerRes> { MakeRes("Smith"), MakeRes("Jones") };
            _service.GetAllAsync().Returns(dtos);
            _mapper.Map<List<ProjectManagerRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<List<ProjectManagerRes>>(ok.Value);
            Assert.Equal(2, returned.Count);
            await _service.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAll_ServiceReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos    = new List<ProjectManagerDto>();
            var resList = new List<ProjectManagerRes>();
            _service.GetAllAsync().Returns(dtos);
            _mapper.Map<List<ProjectManagerRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Empty(Assert.IsType<List<ProjectManagerRes>>(ok.Value));
        }

        [Fact]
        public async Task GetAll_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetAllAsync().ThrowsAsync(new Exception("db error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAll());
        }

        #endregion

        // ── GetById ───────────────────────────────────────────────────────────────

        #region GetById

        [Fact]
        public async Task GetById_ServiceReturnsDto_ReturnsOkWithMappedResult()
        {
            // Arrange
            const string name = "J. Smith";
            var dto = MakeDto(name);
            var res = MakeRes(name);
            _service.GetByIdAsync(name).Returns(dto);
            _mapper.Map<ProjectManagerRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetById(name);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
            await _service.Received(1).GetByIdAsync(name);
        }

        [Fact]
        public async Task GetById_ServiceReturnsNull_ReturnsNotFound()
        {
            // Arrange
            _service.GetByIdAsync(Arg.Any<string>()).Returns((ProjectManagerDto?)null);

            // Act
            var result = await _controller.GetById("Unknown");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetById_DecodesNameBeforeServiceCall()
        {
            // Arrange — URL-encoded space %20 should be decoded
            const string encoded = "J.%20Smith";
            const string decoded = "J. Smith";
            _service.GetByIdAsync(decoded).Returns(MakeDto(decoded));
            _mapper.Map<ProjectManagerRes>(Arg.Any<ProjectManagerDto>()).Returns(MakeRes(decoded));

            // Act
            await _controller.GetById(encoded);

            // Assert
            await _service.Received(1).GetByIdAsync(decoded);
        }

        #endregion

        // ── Create ────────────────────────────────────────────────────────────────

        #region Create

        [Fact]
        public async Task Create_ValidRequest_ReturnsCreatedAtActionWithMappedResult()
        {
            // Arrange
            var req     = MakeReq("New Manager");
            var dto     = MakeDto("New Manager");
            var created = MakeDto("New Manager");
            var res     = MakeRes("New Manager");
            _mapper.Map<ProjectManagerDto>(req).Returns(dto);
            _service.CreateAsync(dto).Returns(created);
            _mapper.Map<ProjectManagerRes>(created).Returns(res);

            // Act
            var result = await _controller.Create(req);

            // Assert
            var created201 = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(ProjectManagerController.GetById), created201.ActionName);
            Assert.Equal(res, created201.Value);
        }

        [Fact]
        public async Task Create_DuplicateName_PropagatesInvalidOperationException()
        {
            // Arrange
            _mapper.Map<ProjectManagerDto>(Arg.Any<ProjectManagerReq>()).Returns(MakeDto());
            _service.CreateAsync(Arg.Any<ProjectManagerDto>()).ThrowsAsync(new InvalidOperationException("already exists"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Create(MakeReq()));
        }

        #endregion

        // ── Update ────────────────────────────────────────────────────────────────

        #region Update

        [Fact]
        public async Task Update_ServiceReturnsDto_ReturnsOkWithMappedResult()
        {
            // Arrange
            const string name = "J. Smith";
            var dto     = MakeDto("");
            var updated = MakeDto(name);
            var res     = MakeRes(name);
            _mapper.Map<ProjectManagerDto>(Arg.Any<ProjectManagerReq>()).Returns(dto);
            _service.UpdateAsync(Arg.Any<ProjectManagerDto>()).Returns(updated);
            _mapper.Map<ProjectManagerRes>(updated).Returns(res);

            // Act
            var result = await _controller.Update(name, MakeReq(name));

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
        }

        [Fact]
        public async Task Update_SetsRouteNameOnDtoBeforeCallingService()
        {
            // Arrange — mapper returns empty name; controller sets dto.Projectmanager = route name
            const string routeName = "J. Smith";
            var dto = new ProjectManagerDto { Projectmanager = "" };
            _mapper.Map<ProjectManagerDto>(Arg.Any<ProjectManagerReq>()).Returns(dto);
            _service.UpdateAsync(Arg.Any<ProjectManagerDto>()).Returns(MakeDto(routeName));
            _mapper.Map<ProjectManagerRes>(Arg.Any<ProjectManagerDto>()).Returns(MakeRes(routeName));

            // Act
            await _controller.Update(routeName, MakeReq(""));

            // Assert
            await _service.Received(1).UpdateAsync(
                Arg.Is<ProjectManagerDto>(d => d.Projectmanager == routeName));
        }

        [Fact]
        public async Task Update_ServiceThrowsKeyNotFoundException_PropagatesException()
        {
            // Arrange
            _mapper.Map<ProjectManagerDto>(Arg.Any<ProjectManagerReq>()).Returns(MakeDto());
            _service.UpdateAsync(Arg.Any<ProjectManagerDto>()).ThrowsAsync(new KeyNotFoundException("not found"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.Update("Unknown", MakeReq("Unknown")));
        }

        #endregion

        // ── Delete ────────────────────────────────────────────────────────────────

        #region Delete

        [Fact]
        public async Task Delete_ServiceCompletes_ReturnsOkWithSuccessTrue()
        {
            // Arrange
            _service.DeleteAsync("J. Smith").Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete("J.%20Smith");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var json    = System.Text.Json.JsonSerializer.Serialize(ok.Value);
            var element = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);
            Assert.True(element.GetProperty("success").GetBoolean());
            await _service.Received(1).DeleteAsync("J. Smith");
        }

        [Fact]
        public async Task Delete_DecodesNameBeforeServiceCall()
        {
            // Arrange
            const string encoded = "J.%20Smith";
            const string decoded = "J. Smith";
            _service.DeleteAsync(Arg.Any<string>()).Returns(Task.CompletedTask);

            // Act
            await _controller.Delete(encoded);

            // Assert
            await _service.Received(1).DeleteAsync(decoded);
        }

        [Fact]
        public async Task Delete_ServiceThrowsKeyNotFoundException_PropagatesException()
        {
            // Arrange
            _service.DeleteAsync(Arg.Any<string>()).ThrowsAsync(new KeyNotFoundException("not found"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.Delete("Unknown"));
        }

        #endregion
    }
}
