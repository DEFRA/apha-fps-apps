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

namespace Apha.FPS.Api.UnitTests.Controller.WorkgroupGradeControllerTest
{
    public class WorkgroupGradeControllerTest
    {
        private readonly IWorkgroupGradeService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly WorkgroupGradeController _controller;

        public WorkgroupGradeControllerTest()
        {
            _serviceMock = Substitute.For<IWorkgroupGradeService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new WorkgroupGradeController(_serviceMock, _mapperMock);
        }

        #region Constructor

        [Fact]
        public void Constructor_NullService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkgroupGradeController(null!, _mapperMock));
        }

        [Fact]
        public void Constructor_NullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkgroupGradeController(_serviceMock, null!));
        }

        #endregion

        #region GetAllWorkgroupGradesPagedAsync

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_HappyPath_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            var dtos = new List<WorkgroupGradeDto>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" }
            };
            var paginationData = new PaginationDto
            {
                PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1
            };
            var serviceResult = new PaginatedResult<WorkgroupGradeDto>(dtos, paginationData);

            var expectedResponse = new PaginationRes<WorkgroupGradeRes>
            {
                Data = new List<WorkgroupGradeRes>
                {
                    new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" }
                },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 }
            };

            _serviceMock.GetAllWorkgroupGradesPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WorkgroupGradeRes>>(serviceResult).Returns(expectedResponse);

            var result = await _controller.GetAllWorkgroupGradesPagedAsync(query);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_ServiceThrows_PropagatesException()
        {
            var query = new QueryParameters<string>();
            _serviceMock.GetAllWorkgroupGradesPagedAsync(query).Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllWorkgroupGradesPagedAsync(query));
        }

        #endregion

        #region GetByWgGradeAsync

        [Fact]
        public async Task GetByWgGradeAsync_HappyPath_ReturnsOk()
        {
            var dto = new WorkgroupGradeDto { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };
            var mapped = new WorkgroupGradeRes { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };

            _serviceMock.GetByWgGradeAsync("WG01").Returns(dto);
            _mapperMock.Map<WorkgroupGradeRes>(dto).Returns(mapped);

            var result = await _controller.GetByWgGradeAsync("WG01");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetByWgGradeAsync_NotFound_ThrowsKeyNotFoundException()
        {
            _serviceMock.GetByWgGradeAsync("INVALID").Returns((WorkgroupGradeDto?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetByWgGradeAsync("INVALID"));
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_HappyPath_ReturnsOk()
        {
            var req = new WorkgroupGradeReq { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };
            var dto = new WorkgroupGradeDto { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };
            var createdDto = new WorkgroupGradeDto { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };
            var mapped = new WorkgroupGradeRes { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };

            _mapperMock.Map<WorkgroupGradeDto>(req).Returns(dto);
            _serviceMock.CreateAsync(dto).Returns(createdDto);
            _mapperMock.Map<WorkgroupGradeRes>(createdDto).Returns(mapped);

            var result = await _controller.CreateAsync(req);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task CreateAsync_ServiceThrows_PropagatesException()
        {
            var req = new WorkgroupGradeReq { WgGrade = "WG01" };
            var dto = new WorkgroupGradeDto { WgGrade = "WG01" };

            _mapperMock.Map<WorkgroupGradeDto>(req).Returns(dto);
            _serviceMock.CreateAsync(dto).Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.CreateAsync(req));
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_HappyPath_ReturnsOk()
        {
            var req = new WorkgroupGradeReq { WgGrade = "OLD", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };
            var dto = new WorkgroupGradeDto { WgGrade = "OLD", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };
            var updatedDto = new WorkgroupGradeDto { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };
            var mapped = new WorkgroupGradeRes { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };

            _mapperMock.Map<WorkgroupGradeDto>(req).Returns(dto);
            _serviceMock.UpdateAsync(dto).Returns(updatedDto);
            _mapperMock.Map<WorkgroupGradeRes>(updatedDto).Returns(mapped);

            var result = await _controller.UpdateAsync("WG01", req);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
            Assert.Equal("WG01", dto.WgGrade);
        }

        [Fact]
        public async Task UpdateAsync_ServiceThrows_PropagatesException()
        {
            var req = new WorkgroupGradeReq { WgGrade = "WG01" };
            var dto = new WorkgroupGradeDto { WgGrade = "WG01" };

            _mapperMock.Map<WorkgroupGradeDto>(req).Returns(dto);
            _serviceMock.UpdateAsync(dto).Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.UpdateAsync("WG01", req));
        }

        [Fact]
        public async Task UpdateAsync_SetsWgGradeFromRoute()
        {
            var req = new WorkgroupGradeReq { WgGrade = "ORIGINAL" };
            var dto = new WorkgroupGradeDto { WgGrade = "ORIGINAL" };
            var updatedDto = new WorkgroupGradeDto { WgGrade = "ROUTE_VALUE" };
            var mapped = new WorkgroupGradeRes { WgGrade = "ROUTE_VALUE" };

            _mapperMock.Map<WorkgroupGradeDto>(req).Returns(dto);
            _serviceMock.UpdateAsync(dto).Returns(updatedDto);
            _mapperMock.Map<WorkgroupGradeRes>(updatedDto).Returns(mapped);

            await _controller.UpdateAsync("ROUTE_VALUE", req);

            dto.WgGrade.Should().Be("ROUTE_VALUE");
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_HappyPath_ReturnsOkTrue()
        {
            _serviceMock.DeleteAsync("WG01").Returns(true);

            var result = await _controller.DeleteAsync("WG01");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task DeleteAsync_NotFound_ReturnsOkFalse()
        {
            _serviceMock.DeleteAsync("INVALID").Returns(false);

            var result = await _controller.DeleteAsync("INVALID");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.False((bool)okResult.Value!);
        }

        [Fact]
        public async Task DeleteAsync_ServiceThrows_PropagatesException()
        {
            _serviceMock.DeleteAsync("WG01").Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.DeleteAsync("WG01"));
        }

        #endregion

        #region GetAllPcGradesAsync

        [Fact]
        public async Task GetAllPcGradesAsync_HappyPath_ReturnsOk()
        {
            var grades = new List<string> { "PC01", "PC02" };
            _serviceMock.GetAllPcGradesAsync().Returns(grades);

            var result = await _controller.GetAllPcGradesAsync();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(grades, okResult.Value);
        }

        [Fact]
        public async Task GetAllPcGradesAsync_EmptyList_ReturnsOk()
        {
            _serviceMock.GetAllPcGradesAsync().Returns(new List<string>());

            var result = await _controller.GetAllPcGradesAsync();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsType<List<string>>(okResult.Value);
            list.Should().BeEmpty();
        }

        #endregion

        #region GetAllGradeCodesAsync

        [Fact]
        public async Task GetAllGradeCodesAsync_HappyPath_ReturnsOk()
        {
            var codes = new List<string> { "G01", "G02" };
            _serviceMock.GetAllGradeCodesAsync().Returns(codes);

            var result = await _controller.GetAllGradeCodesAsync();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(codes, okResult.Value);
        }

        [Fact]
        public async Task GetAllGradeCodesAsync_EmptyList_ReturnsOk()
        {
            _serviceMock.GetAllGradeCodesAsync().Returns(new List<string>());

            var result = await _controller.GetAllGradeCodesAsync();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsType<List<string>>(okResult.Value);
            list.Should().BeEmpty();
        }

        #endregion

        #region GetAllWorkgroupNamesAsync

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_HappyPath_ReturnsOk()
        {
            var names = new List<string> { "IT", "HR" };
            _serviceMock.GetAllWorkgroupNamesAsync().Returns(names);

            var result = await _controller.GetAllWorkgroupNamesAsync();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(names, okResult.Value);
        }

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_EmptyList_ReturnsOk()
        {
            _serviceMock.GetAllWorkgroupNamesAsync().Returns(new List<string>());

            var result = await _controller.GetAllWorkgroupNamesAsync();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsType<List<string>>(okResult.Value);
            list.Should().BeEmpty();
        }

        #endregion
    }
}
