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
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.DivisionGradeMaintenanceControllerTest
{
    public class DivisionGradeMaintenanceControllerTests
    {
        private readonly IDivisionGradeMaintenanceService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly DivisionGradeMaintenanceController _controller;

        public DivisionGradeMaintenanceControllerTests()
        {
            _serviceMock = Substitute.For<IDivisionGradeMaintenanceService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new DivisionGradeMaintenanceController(_serviceMock, _mapperMock);
        }

        private static DivisionGradeMaintenanceDto BuildDto(string code = "A-VSD") =>
            new() { DivisionGradeCode = code, GradeCode = "A", Division = "VSD", ChargeRate = 100m };

        private static DivisionGradeReq BuildReq(string code = "A-VSD") =>
            new() { DivisionGradeCode = code, GradeCode = "A", Division = "VSD", ChargeRate = 100m };

        private static DivisionGradeRes BuildRes(string code = "A-VSD") =>
            new() { DivisionGradeCode = code, GradeCode = "A", Division = "VSD", ChargeRate = 100m };

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DivisionGradeMaintenanceController(null!, _mapperMock));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DivisionGradeMaintenanceController(_serviceMock, null!));
        }

        #endregion

        #region GetAllPagedAsync Tests

        [Fact]
        public async Task GetAllPagedAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<DivisionGradeMaintenanceDto> { BuildDto() };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var serviceResult = new PaginatedResult<DivisionGradeMaintenanceDto>(dtos, pagination);
            var expectedResponse = new PaginationRes<DivisionGradeRes>
            {
                Data = new List<DivisionGradeRes> { BuildRes() },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _serviceMock.GetAllPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<DivisionGradeRes>>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetAllPagedAsync(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
            await _serviceMock.Received(1).GetAllPagedAsync(query);
        }

        [Fact]
        public async Task GetAllPagedAsync_NullResult_ThrowsArgumentException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _serviceMock.GetAllPagedAsync(query).Returns((PaginatedResult<DivisionGradeMaintenanceDto>)null!);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetAllPagedAsync(query));
        }

        [Fact]
        public async Task GetAllPagedAsync_WithFilterAndSorting_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 2, PageSize = 5, SortBy = "DivisionGradeCode", Descending = true
            };
            var dtos = new List<DivisionGradeMaintenanceDto> { BuildDto() };
            var pagination = new PaginationDto { PageNumber = 2, PageSize = 5, TotalRecords = 10 };
            var serviceResult = new PaginatedResult<DivisionGradeMaintenanceDto>(dtos, pagination);
            var expectedResponse = new PaginationRes<DivisionGradeRes>
            {
                Data = new List<DivisionGradeRes> { BuildRes() },
                PaginationData = new Pagination { PageNumber = 2, PageSize = 5, TotalRecords = 10 }
            };

            _serviceMock.GetAllPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<DivisionGradeRes>>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetAllPagedAsync(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginationRes<DivisionGradeRes>>(okResult.Value);
            Assert.Equal(2, response.PaginationData.PageNumber);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var dto = BuildDto("A-VSD");
            var res = BuildRes("A-VSD");

            _serviceMock.GetByIdAsync("A-VSD").Returns(dto);
            _mapperMock.Map<DivisionGradeRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetByIdAsync("A-VSD");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(res, okResult.Value);
            await _serviceMock.Received(1).GetByIdAsync("A-VSD");
        }

        [Fact]
        public async Task GetByIdAsync_NullResult_ThrowsArgumentException()
        {
            // Arrange
            _serviceMock.GetByIdAsync("NOTEXIST").Returns((DivisionGradeMaintenanceDto?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.GetByIdAsync("NOTEXIST"));
            Assert.Contains("NOTEXIST", exception.Message);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var req = BuildReq("A-VSD");
            var dto = BuildDto("A-VSD");
            var created = BuildDto("A-VSD");
            var res = BuildRes("A-VSD");

            _mapperMock.Map<DivisionGradeMaintenanceDto>(req).Returns(dto);
            _serviceMock.CreateAsync(dto).Returns(created);
            _mapperMock.Map<DivisionGradeRes>(created).Returns(res);

            // Act
            var result = await _controller.CreateAsync(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(res, okResult.Value);
            await _serviceMock.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var req = BuildReq();
            var dto = BuildDto();

            _mapperMock.Map<DivisionGradeMaintenanceDto>(req).Returns(dto);
            _serviceMock.CreateAsync(dto).ThrowsAsync(new InvalidOperationException("already exists"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.CreateAsync(req));
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var req = BuildReq("A-VSD");
            var dto = BuildDto("A-VSD");
            var updated = BuildDto("A-VSD");
            var res = BuildRes("A-VSD");

            _mapperMock.Map<DivisionGradeMaintenanceDto>(req).Returns(dto);
            _serviceMock.UpdateAsync("A-VSD", dto).Returns(updated);
            _mapperMock.Map<DivisionGradeRes>(updated).Returns(res);

            // Act
            var result = await _controller.UpdateAsync("A-VSD", req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(res, okResult.Value);
            await _serviceMock.Received(1).UpdateAsync("A-VSD", dto);
        }

        [Fact]
        public async Task UpdateAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var req = BuildReq();
            var dto = BuildDto();

            _mapperMock.Map<DivisionGradeMaintenanceDto>(req).Returns(dto);
            _serviceMock.UpdateAsync("A-VSD", dto).ThrowsAsync(new InvalidOperationException("not found"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.UpdateAsync("A-VSD", req));
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            _serviceMock.DeleteAsync("A-VSD").Returns(true);

            // Act
            var result = await _controller.DeleteAsync("A-VSD");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(true, okResult.Value);
            await _serviceMock.Received(1).DeleteAsync("A-VSD");
        }

        [Fact]
        public async Task DeleteAsync_WithNullOrWhitespace_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteAsync(""));
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteAsync("   "));
        }

        [Fact]
        public async Task DeleteAsync_WhenNotFound_ThrowsArgumentException()
        {
            // Arrange
            _serviceMock.DeleteAsync("NOTEXIST").Returns(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.DeleteAsync("NOTEXIST"));
            Assert.Contains("NOTEXIST", exception.Message);
        }

        #endregion

        #region GetAllGradeCodesAsync Tests

        [Fact]
        public async Task GetAllGradeCodesAsync_ReturnsOkWithGradeCodes()
        {
            // Arrange
            var gradeCodes = new List<string> { "A", "B", "C" };
            _serviceMock.GetAllGradeCodesAsync().Returns(gradeCodes);

            // Act
            var result = await _controller.GetAllGradeCodesAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<List<string>>(okResult.Value);
            Assert.Equal(3, data.Count);
            await _serviceMock.Received(1).GetAllGradeCodesAsync();
        }

        [Fact]
        public async Task GetAllGradeCodesAsync_ReturnsEmptyList_WhenNoGrades()
        {
            // Arrange
            _serviceMock.GetAllGradeCodesAsync().Returns([]);

            // Act
            var result = await _controller.GetAllGradeCodesAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<List<string>>(okResult.Value);
            Assert.Empty(data);
        }

        #endregion
    }
}
