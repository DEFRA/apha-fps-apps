using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.MasterLookupControllerTest
{
    public class MasterLookupControllerTests
    {
        private const string TableName = "directorate";

        private readonly IMasterLookupService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly MasterLookupController _controller;

        public MasterLookupControllerTests()
        {
            _serviceMock = Substitute.For<IMasterLookupService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new MasterLookupController(_serviceMock, _mapperMock);
        }

        #region Constructor

        [Fact]
        public void Constructor_WithNullService_ThrowsArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new MasterLookupController(null!, _mapperMock));
            Assert.Equal("masterLookupService", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new MasterLookupController(_serviceMock, null!));
            Assert.Equal("mapper", exception.ParamName);
        }

        #endregion

        #region GetAllMasterLookupsAsync

        [Fact]
        public async Task GetAllMasterLookupsAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            _serviceMock.GetAllMasterLookupsAsync()
                .Returns(new List<string> { "directorate", "disease" });

            // Act
            var result = await _controller.GetAllMasterLookupsAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsType<List<MasterLookupRes>>(okResult.Value);
            Assert.Equal(2, list.Count);
            Assert.Equal("directorate", list[0].MasterTableName);
        }

        [Fact]
        public async Task GetAllMasterLookupsAsync_EmptyList_ReturnsOkEmpty()
        {
            // Arrange
            _serviceMock.GetAllMasterLookupsAsync().Returns(new List<string>());

            // Act
            var result = await _controller.GetAllMasterLookupsAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsType<List<MasterLookupRes>>(okResult.Value);
            Assert.Empty(list);
        }

        [Fact]
        public async Task GetAllMasterLookupsAsync_ServiceThrows_Propagates()
        {
            // Arrange
            _serviceMock.GetAllMasterLookupsAsync().Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllMasterLookupsAsync());
        }

        #endregion

        #region GetLookupItemsAsync

        [Fact]
        public async Task GetLookupItemsAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            _serviceMock.GetLookupItemsAsync(TableName).Returns(new List<string> { "A", "B" });

            // Act
            var result = await _controller.GetLookupItemsAsync(TableName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsType<List<LookupItemRes>>(okResult.Value);
            Assert.Equal(2, list.Count);
            Assert.Equal("A", list[0].Value);
        }

        [Fact]
        public async Task GetLookupItemsAsync_EmptyList_ReturnsOkEmpty()
        {
            // Arrange
            _serviceMock.GetLookupItemsAsync(TableName).Returns(new List<string>());

            // Act
            var result = await _controller.GetLookupItemsAsync(TableName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsType<List<LookupItemRes>>(okResult.Value);
            Assert.Empty(list);
        }

        #endregion

        #region GetLookupItemsPagedAsync

        [Fact]
        public async Task GetLookupItemsPagedAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mappedFilter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<string>(
                new List<string> { "A", "B" },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 });

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mappedFilter);
            _serviceMock.GetLookupItemsPagedAsync(TableName, mappedFilter).Returns(serviceResult);
            _mapperMock.Map<Pagination>(serviceResult.PaginationData).Returns(new Pagination { TotalRecords = 2 });

            // Act
            var result = await _controller.GetLookupItemsPagedAsync(TableName, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginationRes<LookupItemRes>>(okResult.Value);
            Assert.Equal(2, response.Data.Count());
        }

        [Fact]
        public async Task GetLookupItemsPagedAsync_EmptyData_ReturnsOk()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mappedFilter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<string>(
                new List<string>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 });

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mappedFilter);
            _serviceMock.GetLookupItemsPagedAsync(TableName, mappedFilter).Returns(serviceResult);
            _mapperMock.Map<Pagination>(serviceResult.PaginationData).Returns(new Pagination());

            // Act
            var result = await _controller.GetLookupItemsPagedAsync(TableName, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginationRes<LookupItemRes>>(okResult.Value);
            Assert.Empty(response.Data);
        }

        [Fact]
        public async Task GetLookupItemsPagedAsync_ServiceThrows_Propagates()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mappedFilter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _mapperMock.Map<QueryParameters<string>>(query).Returns(mappedFilter);
            _serviceMock.GetLookupItemsPagedAsync(TableName, mappedFilter).Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetLookupItemsPagedAsync(TableName, query));
        }

        #endregion

        #region CreateLookupItemAsync

        [Fact]
        public async Task CreateLookupItemAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var request = new LookupItemReq { Value = "A" };
            _serviceMock.CreateLookupItemAsync(TableName, "A").Returns(true);

            // Act
            var result = await _controller.CreateLookupItemAsync(TableName, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(true, okResult.Value);
            await _serviceMock.Received(1).CreateLookupItemAsync(TableName, "A");
        }

        [Fact]
        public async Task CreateLookupItemAsync_NullRequest_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.CreateLookupItemAsync(TableName, null!));
        }

        [Fact]
        public async Task CreateLookupItemAsync_EmptyValue_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.CreateLookupItemAsync(TableName, new LookupItemReq { Value = " " }));
        }

        #endregion

        #region UpdateLookupItemAsync

        [Fact]
        public async Task UpdateLookupItemAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var request = new LookupItemReq { Value = "New", OriginalValue = "Old" };
            _serviceMock.UpdateLookupItemAsync(TableName, "Old", "New").Returns(true);

            // Act
            var result = await _controller.UpdateLookupItemAsync(TableName, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(true, okResult.Value);
            await _serviceMock.Received(1).UpdateLookupItemAsync(TableName, "Old", "New");
        }

        [Fact]
        public async Task UpdateLookupItemAsync_NullRequest_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.UpdateLookupItemAsync(TableName, null!));
        }

        [Fact]
        public async Task UpdateLookupItemAsync_MissingOriginalValue_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.UpdateLookupItemAsync(TableName, new LookupItemReq { Value = "New" }));
        }

        [Fact]
        public async Task UpdateLookupItemAsync_MissingValue_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.UpdateLookupItemAsync(TableName, new LookupItemReq { Value = " ", OriginalValue = "Old" }));
        }

        #endregion

        #region DeleteLookupItemAsync

        [Fact]
        public async Task DeleteLookupItemAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            _serviceMock.DeleteLookupItemAsync(TableName, "A").Returns(true);

            // Act
            var result = await _controller.DeleteLookupItemAsync(TableName, "A");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(true, okResult.Value);
            await _serviceMock.Received(1).DeleteLookupItemAsync(TableName, "A");
        }

        [Fact]
        public async Task DeleteLookupItemAsync_EmptyValue_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.DeleteLookupItemAsync(TableName, " "));
        }

        #endregion
    }
}
