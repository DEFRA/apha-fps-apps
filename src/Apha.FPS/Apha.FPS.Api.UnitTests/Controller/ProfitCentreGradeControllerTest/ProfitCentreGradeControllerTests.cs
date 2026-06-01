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

namespace Apha.FPS.Api.UnitTests.Controller.ProfitCentreGradeControllerTest
{
    public class ProfitCentreGradeControllerTests
    {
        private const string DefaultProfitCentre = "PC01";

        private readonly IProfitCentreGradeService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ProfitCentreGradeController _controller;

        public ProfitCentreGradeControllerTests()
        {
            _serviceMock = Substitute.For<IProfitCentreGradeService>();
            _mapperMock  = Substitute.For<IMapper>();
            _controller  = new ProfitCentreGradeController(_serviceMock, _mapperMock);
        }

        #region GetProfitCentreGradesAsync Tests

        [Fact]
        public async Task GetProfitCentreGradesAsync_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var query  = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mapped = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var grades = new List<ProfitCentreGradeDto>
            {
                new() { PcGrade = "G001", ProfitCentre = DefaultProfitCentre, ChargeRate = 100m }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var serviceResult = new PaginatedResult<ProfitCentreGradeDto>(grades, paginationDto);
            var expectedRes   = new PaginationRes<ProfitCentreGradeRes>
            {
                Data           = new List<ProfitCentreGradeRes> { new() { PcGrade = "G001" } },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mapped);
            _serviceMock.GetProfitCentreGradesAsync(mapped, DefaultProfitCentre).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProfitCentreGradeRes>>(serviceResult).Returns(expectedRes);

            // Act
            var result = await _controller.GetProfitCentreGradesAsync(query, DefaultProfitCentre);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(expectedRes);
            await _serviceMock.Received(1).GetProfitCentreGradesAsync(mapped, DefaultProfitCentre);
        }

        [Fact]
        public async Task GetProfitCentreGradesAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var query  = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mapped = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mapped);
            _serviceMock.GetProfitCentreGradesAsync(mapped, DefaultProfitCentre)
                .ThrowsAsync(new ArgumentException("Invalid profit centre"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _controller.GetProfitCentreGradesAsync(query, DefaultProfitCentre));
        }

        [Fact]
        public void Constructor_WithNullService_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ProfitCentreGradeController(null!, _mapperMock));
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ProfitCentreGradeController(_serviceMock, null!));
        }

        #endregion

        #region GetAllPagedAsync Tests

        [Fact]
        public async Task GetAllPagedAsync_WithValidQuery_ReturnsOk()
        {
            // Arrange
            var query  = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mapped = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<ProfitCentreGradeDto>();
            var expectedRes   = new PaginationRes<ProfitCentreGradeRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mapped);
            _serviceMock.GetAllPagedAsync(mapped).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProfitCentreGradeRes>>(serviceResult).Returns(expectedRes);

            // Act
            var result = await _controller.GetAllPagedAsync(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(expectedRes);
            await _serviceMock.Received(1).GetAllPagedAsync(mapped);
        }

        [Fact]
        public async Task GetAllPagedAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var query  = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mapped = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mapped);
            _serviceMock.GetAllPagedAsync(mapped).ThrowsAsync(new InvalidOperationException("service error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetAllPagedAsync(query));
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WhenFound_ReturnsOk()
        {
            // Arrange
            var dto = new ProfitCentreGradeDto { PcGrade = "G001", ProfitCentre = DefaultProfitCentre };
            var res = new ProfitCentreGradeRes { PcGrade = "G001", ProfitCentre = DefaultProfitCentre };

            _serviceMock.GetByIdAsync("G001").Returns(dto);
            _mapperMock.Map<ProfitCentreGradeRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetByIdAsync("G001");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(res);
            await _serviceMock.Received(1).GetByIdAsync("G001");
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ReturnsNotFound()
        {
            _serviceMock.GetByIdAsync("NOTEXIST").Returns((ProfitCentreGradeDto?)null);

            var result = await _controller.GetByIdAsync("NOTEXIST");

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetByIdAsync_WhenServiceThrows_PropagatesException()
        {
            _serviceMock.GetByIdAsync("G001").ThrowsAsync(new InvalidOperationException("service error"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetByIdAsync("G001"));
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var req     = new ProfitCentreGradeReq { PcGrade = "G001", ProfitCentre = DefaultProfitCentre };
            var dto     = new ProfitCentreGradeDto { PcGrade = "G001", ProfitCentre = DefaultProfitCentre };
            var created = new ProfitCentreGradeDto { PcGrade = "G001", ProfitCentre = DefaultProfitCentre };
            var res     = new ProfitCentreGradeRes { PcGrade = "G001", ProfitCentre = DefaultProfitCentre };

            _mapperMock.Map<ProfitCentreGradeDto>(req).Returns(dto);
            _serviceMock.CreateAsync(dto).Returns(created);
            _mapperMock.Map<ProfitCentreGradeRes>(created).Returns(res);

            // Act
            var result = await _controller.CreateAsync(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(res);
            await _serviceMock.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateAsync_WhenServiceThrowsInvalidOperation_ReturnsBadRequest()
        {
            // Arrange
            var req = new ProfitCentreGradeReq { PcGrade = "G001", ProfitCentre = "INVALID" };
            var dto = new ProfitCentreGradeDto { PcGrade = "G001", ProfitCentre = "INVALID" };

            _mapperMock.Map<ProfitCentreGradeDto>(req).Returns(dto);
            _serviceMock.CreateAsync(dto).ThrowsAsync(new InvalidOperationException("ProfitCentre 'INVALID' does not exist."));

            // Act
            var result = await _controller.CreateAsync(req);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.Value.Should().NotBeNull();
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var req     = new ProfitCentreGradeReq { PcGrade = "G001", ProfitCentre = DefaultProfitCentre };
            var dto     = new ProfitCentreGradeDto { PcGrade = "G001", ProfitCentre = DefaultProfitCentre };
            var updated = new ProfitCentreGradeDto { PcGrade = "G001", ProfitCentre = DefaultProfitCentre };
            var res     = new ProfitCentreGradeRes { PcGrade = "G001", ProfitCentre = DefaultProfitCentre };

            _mapperMock.Map<ProfitCentreGradeDto>(req).Returns(dto);
            _serviceMock.UpdateAsync("G001", dto).Returns(updated);
            _mapperMock.Map<ProfitCentreGradeRes>(updated).Returns(res);

            // Act
            var result = await _controller.UpdateAsync("G001", req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(res);
            await _serviceMock.Received(1).UpdateAsync("G001", dto);
        }

        [Fact]
        public async Task UpdateAsync_WhenServiceThrowsInvalidOperation_ReturnsBadRequest()
        {
            // Arrange
            var req = new ProfitCentreGradeReq { PcGrade = "G001", ProfitCentre = "INVALID" };
            var dto = new ProfitCentreGradeDto { PcGrade = "G001", ProfitCentre = "INVALID" };

            _mapperMock.Map<ProfitCentreGradeDto>(req).Returns(dto);
            _serviceMock.UpdateAsync("G001", dto).ThrowsAsync(new InvalidOperationException("ProfitCentre 'INVALID' does not exist."));

            // Act
            var result = await _controller.UpdateAsync("G001", req);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WhenDeleted_ReturnsOk()
        {
            _serviceMock.DeleteAsync("G001").Returns(true);

            var result = await _controller.DeleteAsync("G001");

            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().NotBeNull();
            await _serviceMock.Received(1).DeleteAsync("G001");
        }

        [Fact]
        public async Task DeleteAsync_WhenNotFound_ReturnsNotFound()
        {
            _serviceMock.DeleteAsync("NOTEXIST").Returns(false);

            var result = await _controller.DeleteAsync("NOTEXIST");

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteAsync_WhenServiceThrows_PropagatesException()
        {
            _serviceMock.DeleteAsync("G001").ThrowsAsync(new InvalidOperationException("service error"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.DeleteAsync("G001"));
        }

        #endregion

        #region GetProfitCentreCodesAsync Tests

        [Fact]
        public async Task GetProfitCentreCodesAsync_ReturnsOkWithCodes()
        {
            // Arrange
            var codes = new List<string> { "PC01", "PC02", "PC03" };
            _serviceMock.GetAllProfitCentreCodesAsync().Returns(codes);

            // Act
            var result = await _controller.GetProfitCentreCodesAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<string>>(okResult.Value);
            data.Should().HaveCount(3);
            await _serviceMock.Received(1).GetAllProfitCentreCodesAsync();
        }

        [Fact]
        public async Task GetProfitCentreCodesAsync_ReturnsEmpty_WhenNoCodes()
        {
            _serviceMock.GetAllProfitCentreCodesAsync().Returns([]);

            var result = await _controller.GetProfitCentreCodesAsync();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<string>>(okResult.Value);
            data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetProfitCentreCodesAsync_WhenServiceThrows_PropagatesException()
        {
            _serviceMock.GetAllProfitCentreCodesAsync()
                .ThrowsAsync(new InvalidOperationException("service error"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.GetProfitCentreCodesAsync());
        }

        #endregion
    }
}
