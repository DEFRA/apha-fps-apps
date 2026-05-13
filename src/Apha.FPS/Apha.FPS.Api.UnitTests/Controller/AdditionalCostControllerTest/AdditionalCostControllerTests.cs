using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Api.UnitTests.Controller.AdditionalCostControllerTest
{
    public class AdditionalCostControllerTests
    {
        private readonly IAdditionalCostService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly AdditionalCostController _controller;

        public AdditionalCostControllerTests()
        {
            _serviceMock = Substitute.For<IAdditionalCostService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new AdditionalCostController(_serviceMock, _mapperMock);
        }

        #region GetByJobCodeAsync

        [Fact]
        public async Task GetByJobCodeAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var query = new PaginationReq<string>();
            var mappedQuery = new QueryParameters<string>();
            var serviceResult = new PaginatedResult<AdditionalCostDto>();
            var mappedResult = new PaginationRes<AdditionalCostRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mappedQuery);
            _serviceMock.GetByJobCodeAsync(mappedQuery, "JOB001").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<AdditionalCostRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetByJobCodeAsync(query, "JOB001");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetByJobCodeAsync_EdgeCase_EmptyResult_ReturnsOk()
        {
            // Arrange
            var query = new PaginationReq<string>();
            var serviceResult = new PaginatedResult<AdditionalCostDto>();
            var mappedResult = new PaginationRes<AdditionalCostRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(new QueryParameters<string>());
            _serviceMock.GetByJobCodeAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>()).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<AdditionalCostRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetByJobCodeAsync(query, "");

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetByJobCodeAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new PaginationReq<string>();
            _mapperMock.Map<QueryParameters<string>>(query).Returns(new QueryParameters<string>());
            _serviceMock.GetByJobCodeAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetByJobCodeAsync(query, "JOB001"));
        }

        [Fact]
        public async Task GetByJobCodeAsync_MapperThrows_PropagatesException()
        {
            // Arrange
            var query = new PaginationReq<string>();
            _mapperMock.Map<QueryParameters<string>>(query).Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetByJobCodeAsync(query, "JOB001"));
        }

        #endregion

        #region GetTotalItemCostAsync

        [Fact]
        public async Task GetTotalItemCostAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            _serviceMock.GetTotalItemCostAsync("JOB001").Returns(350m);

            // Act
            var result = await _controller.GetTotalItemCostAsync("JOB001");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(350m, okResult.Value);
        }

        [Fact]
        public async Task GetTotalItemCostAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetTotalItemCostAsync(Arg.Any<string>()).Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetTotalItemCostAsync("JOB001"));
        }

        #endregion

        #region GetAccountCategoriesAsync

        [Fact]
        public async Task GetAccountCategoriesAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var serviceResult = new List<AccountCategoryDto>
            {
                new() { AccShortName = "ACC1", AccountDescription = "Account One" }
            };
            var mappedResult = new List<AccountCategoryRes>
            {
                new() { AccShortName = "ACC1", AccountDescription = "Account One" }
            };

            _serviceMock.GetAccountCategoriesAsync().Returns(serviceResult);
            _mapperMock.Map<List<AccountCategoryRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAccountCategoriesAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetAccountCategoriesAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetAccountCategoriesAsync().Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAccountCategoriesAsync());
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_WhenFound_ReturnsOk()
        {
            // Arrange
            var dto = new AdditionalCostDto { JobCode = "JOB001", Account = "ACC1", Description = "Desc1", ItemCost = 100m };
            var res = new AdditionalCostRes { JobCode = "JOB001", Account = "ACC1", Description = "Desc1", ItemCost = 100m };

            _serviceMock.GetByIdAsync("JOB001", "ACC1", "Desc1").Returns(dto);
            _mapperMock.Map<AdditionalCostRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetByIdAsync("JOB001", "ACC1", "Desc1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, okResult.Value);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _serviceMock.GetByIdAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns((AdditionalCostDto?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.GetByIdAsync("JOB999", "ACC999", "NoExist"));
        }

        [Fact]
        public async Task GetByIdAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetByIdAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetByIdAsync("JOB001", "ACC1", "Desc1"));
        }

        #endregion

        #region AddAsync

        [Fact]
        public async Task AddAsync_HappyPath_ReturnsCreatedAtAction()
        {
            // Arrange
            var req = new AdditionalCostReq { JobCode = "JOB001", Account = "ACC1", Description = "Desc1", ItemCost = 100m };
            var dto = new AdditionalCostDto { JobCode = "JOB001", Account = "ACC1", Description = "Desc1", ItemCost = 100m };
            var res = new AdditionalCostRes { JobCode = "JOB001", Account = "ACC1", Description = "Desc1", ItemCost = 100m };

            _mapperMock.Map<AdditionalCostDto>(req).Returns(dto);
            _serviceMock.AddAsync(dto).Returns(dto);
            _mapperMock.Map<AdditionalCostRes>(dto).Returns(res);

            // Act
            var result = await _controller.AddAsync(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, okResult.Value);
        }

        [Fact]
        public async Task AddAsync_DuplicateKey_ServiceThrowsInvalidOperationException_Propagates()
        {
            // Arrange
            var req = new AdditionalCostReq { JobCode = "JOB001", Account = "ACC1", Description = "Dup", ItemCost = 100m };
            var dto = new AdditionalCostDto { JobCode = "JOB001", Account = "ACC1", Description = "Dup", ItemCost = 100m };

            _mapperMock.Map<AdditionalCostDto>(req).Returns(dto);
            _serviceMock.AddAsync(dto).Throws(new InvalidOperationException("already exists"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.AddAsync(req));
        }

        [Fact]
        public async Task AddAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            var req = new AdditionalCostReq { JobCode = "JOB001", Account = "ACC1", Description = "Desc1", ItemCost = 100m };
            _mapperMock.Map<AdditionalCostDto>(req).Returns(new AdditionalCostDto());
            _serviceMock.AddAsync(Arg.Any<AdditionalCostDto>()).Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.AddAsync(req));
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var req = new AdditionalCostReq { JobCode = "JOB001", Account = "ACC1", Description = "Desc1", ItemCost = 200m };
            var dto = new AdditionalCostDto { JobCode = "JOB001", Account = "ACC1", Description = "Desc1", ItemCost = 200m };
            var res = new AdditionalCostRes { JobCode = "JOB001", Account = "ACC1", Description = "Desc1", ItemCost = 200m };

            _mapperMock.Map<AdditionalCostDto>(req).Returns(dto);
            _serviceMock.UpdateAsync(dto).Returns(dto);
            _mapperMock.Map<AdditionalCostRes>(dto).Returns(res);

            // Act
            var result = await _controller.UpdateAsync(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, okResult.Value);
        }

        [Fact]
        public async Task UpdateAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            var req = new AdditionalCostReq { JobCode = "JOB001", Account = "ACC1", Description = "Desc1", ItemCost = 200m };
            _mapperMock.Map<AdditionalCostDto>(req).Returns(new AdditionalCostDto());
            _serviceMock.UpdateAsync(Arg.Any<AdditionalCostDto>()).Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.UpdateAsync(req));
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_WhenDeleted_ReturnsOk()
        {
            // Arrange
            _serviceMock.DeleteAsync("JOB001", "ACC1", "Desc1").Returns(true);

            // Act
            var result = await _controller.DeleteAsync(new AdditionalCostReq { JobCode = "JOB001", Account = "ACC1", Description = "Desc1" });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(true, okResult.Value);
        }

        [Fact]
        public async Task DeleteAsync_WhenNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _serviceMock.DeleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.DeleteAsync(new AdditionalCostReq { JobCode = "JOB999", Account = "ACC999", Description = "NoExist" }));
        }

        [Fact]
        public async Task DeleteAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.DeleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.DeleteAsync(new AdditionalCostReq { JobCode = "JOB001", Account = "ACC1", Description = "Desc1" }));
        }

        #endregion
    }
}
