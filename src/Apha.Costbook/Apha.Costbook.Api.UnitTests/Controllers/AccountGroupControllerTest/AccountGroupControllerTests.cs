/*
 * TRANSFORMENGINE MIGRATION — AccountGroupControllerTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New xUnit test class for Apha.Costbook.Api.Controllers.AccountGroupController
 *   - Covers GET all, GET by csg7Group, POST, PUT, DELETE endpoints
 *   - Uses NSubstitute for IAccountGroupService and IMapper
 *
 * PRESERVED:
 *   - Test naming convention: [MethodName]_[StateUnderTest]_[ExpectedResult]
 *   - AAA pattern with explicit Arrange/Act/Assert comments
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: none — fully automated.
 */

using Apha.Common.Contracts.Costbook;
using Apha.Costbook.Api.Controllers;
using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.Costbook.Api.UnitTests.Controllers.AccountGroupControllerTest
{
    public class AccountGroupControllerTests
    {
        private readonly IAccountGroupService _service;
        private readonly IMapper _mapper;
        private readonly AccountGroupController _controller;

        public AccountGroupControllerTests()
        {
            _service = Substitute.For<IAccountGroupService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new AccountGroupController(_service, _mapper);
        }

        // ── GetAllAccountGroups ───────────────────────────────────────────────

        #region GetAllAccountGroups Tests

        [Fact]
        public async Task GetAllAccountGroups_ServiceReturnsList_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = new List<AccountGroupDto>
            {
                new AccountGroupDto { Csg7Group = "CSG001", UseInflation = true },
                new AccountGroupDto { Csg7Group = "CSG002", UseInflation = false }
            };
            var resList = new List<AccountGroupRes>
            {
                new AccountGroupRes { Csg7Group = "CSG001", UseInflation = true },
                new AccountGroupRes { Csg7Group = "CSG002", UseInflation = false }
            };
            _service.GetAllAsync().Returns(dtos);
            _mapper.Map<List<AccountGroupRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAllAccountGroups();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(resList, okResult.Value);
            await _service.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAllAccountGroups_ServiceReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos = new List<AccountGroupDto>();
            var resList = new List<AccountGroupRes>();
            _service.GetAllAsync().Returns(dtos);
            _mapper.Map<List<AccountGroupRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAllAccountGroups();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(resList, okResult.Value);
        }

        #endregion

        // ── GetAccountGroup ───────────────────────────────────────────────────

        #region GetAccountGroup Tests

        [Fact]
        public async Task GetAccountGroup_ExistingKey_ReturnsOkWithMappedRes()
        {
            // Arrange
            var key = "CSG001";
            var dto = new AccountGroupDto { Csg7Group = key, UseInflation = true };
            var res = new AccountGroupRes { Csg7Group = key, UseInflation = true };
            _service.GetByCsg7GroupAsync(key).Returns(dto);
            _mapper.Map<AccountGroupRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetAccountGroup(key);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(res, okResult.Value);
            await _service.Received(1).GetByCsg7GroupAsync(key);
        }

        [Fact]
        public async Task GetAccountGroup_NonExistentKey_ReturnsNotFound()
        {
            // Arrange
            var key = "NOTEXIST";
            _service.GetByCsg7GroupAsync(key).Returns((AccountGroupDto?)null);

            // Act
            var result = await _controller.GetAccountGroup(key);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        // ── AddAccountGroup ───────────────────────────────────────────────────

        #region AddAccountGroup Tests

        [Fact]
        public async Task AddAccountGroup_ValidRequest_ReturnsCreatedAtActionWithMappedRes()
        {
            // Arrange
            var req = new AccountGroupReq { Csg7Group = "CSG003", UseInflation = true };
            var dto = new AccountGroupDto { Csg7Group = "CSG003", UseInflation = true };
            var created = new AccountGroupDto { Csg7Group = "CSG003", UseInflation = true };
            var res = new AccountGroupRes { Csg7Group = "CSG003", UseInflation = true };
            _mapper.Map<AccountGroupDto>(req).Returns(dto);
            _service.AddAsync(dto).Returns(created);
            _mapper.Map<AccountGroupRes>(created).Returns(res);

            // Act
            var result = await _controller.AddAccountGroup(req);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetAccountGroup), createdResult.ActionName);
            Assert.Same(res, createdResult.Value);
            await _service.Received(1).AddAsync(dto);
        }

        [Fact]
        public async Task AddAccountGroup_DuplicateKey_PropagatesArgumentException()
        {
            // Arrange
            var req = new AccountGroupReq { Csg7Group = "CSG001", UseInflation = true };
            var dto = new AccountGroupDto { Csg7Group = "CSG001", UseInflation = true };
            _mapper.Map<AccountGroupDto>(req).Returns(dto);
            _service.AddAsync(dto).Throws(new ArgumentException("AccountGroup 'CSG001' already exists."));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.AddAccountGroup(req));
        }

        #endregion

        // ── UpdateAccountGroup ────────────────────────────────────────────────

        #region UpdateAccountGroup Tests

        [Fact]
        public async Task UpdateAccountGroup_ValidRequest_ReturnsOkWithUpdatedRes()
        {
            // Arrange
            var key = "CSG001";
            var req = new AccountGroupReq { Csg7Group = key, UseInflation = false };
            var dto = new AccountGroupDto { Csg7Group = key, UseInflation = false };
            var updated = new AccountGroupDto { Csg7Group = key, UseInflation = false };
            var res = new AccountGroupRes { Csg7Group = key, UseInflation = false };
            _mapper.Map<AccountGroupDto>(req).Returns(dto);
            _service.UpdateAsync(key, dto).Returns(updated);
            _mapper.Map<AccountGroupRes>(updated).Returns(res);

            // Act
            var result = await _controller.UpdateAccountGroup(key, req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(res, okResult.Value);
            await _service.Received(1).UpdateAsync(key, dto);
        }

        [Fact]
        public async Task UpdateAccountGroup_NonExistentKey_PropagatesKeyNotFoundException()
        {
            // Arrange
            var key = "NOTEXIST";
            var req = new AccountGroupReq { Csg7Group = key, UseInflation = true };
            var dto = new AccountGroupDto { Csg7Group = key, UseInflation = true };
            _mapper.Map<AccountGroupDto>(req).Returns(dto);
            _service.UpdateAsync(key, dto).Throws(new KeyNotFoundException($"AccountGroup '{key}' not found."));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.UpdateAccountGroup(key, req));
        }

        #endregion

        // ── DeleteAccountGroup ────────────────────────────────────────────────

        #region DeleteAccountGroup Tests

        [Fact]
        public async Task DeleteAccountGroup_ExistingKey_ReturnsNoContent()
        {
            // Arrange
            var key = "CSG001";
            _service.DeleteAsync(key).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteAccountGroup(key);

            // Assert
            Assert.IsType<NoContentResult>(result);
            await _service.Received(1).DeleteAsync(key);
        }

        [Fact]
        public async Task DeleteAccountGroup_NonExistentKey_PropagatesKeyNotFoundException()
        {
            // Arrange
            var key = "NOTEXIST";
            _service.DeleteAsync(key).Throws(new KeyNotFoundException($"AccountGroup '{key}' not found."));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.DeleteAccountGroup(key));
        }

        [Fact]
        public async Task DeleteAccountGroup_WhitespaceKey_PropagatesArgumentException()
        {
            // Arrange — controller guard throws before calling service for blank csg7Group
            var key = "   ";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteAccountGroup(key));
            await _service.DidNotReceive().DeleteAsync(Arg.Any<string>());
        }

        #endregion
    }
}
