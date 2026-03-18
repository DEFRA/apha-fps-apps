using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.ProgramControllerTest
{
    public class ProgramControllerTest
    {
        private readonly IProgramService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ProgramController _controller;

        public ProgramControllerTest()
        {
            _serviceMock = Substitute.For<IProgramService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new ProgramController(_serviceMock, _mapperMock);
        }

        [Fact]
        public async Task GetAllProgramsAsync_HappyPath_ReturnsOk()
        {
            var serviceResult = new List<ProgramDto> { new ProgramDto { ProgramNo = "P1" } };
            var mappedResult = new List<ProgramRes> { new ProgramRes { ProgramNo = "P1" } };

            _serviceMock.GetAllProgramsAsync().Returns(serviceResult);
            _mapperMock.Map<List<ProgramRes>>(serviceResult).Returns(mappedResult);

            var result = await _controller.GetAllProgramsAsync();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetAllProgramsAsync_NullResult_ThrowsArgumentException()
        {
            _serviceMock.GetAllProgramsAsync().Returns((List<ProgramDto>?)null);

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetAllProgramsAsync());
        }

        [Fact]
        public async Task GetAllProgramsPagedAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var query = new Apha.FPS.Application.Pagination.QueryParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            var programDtos = new List<ProgramDto>
            {
                new ProgramDto { ProgramNo = "P1", ProgramName = "Test Program" }
            };
            var paginationData = new Apha.FPS.Application.Pagination.PaginationDto
            {
                PageNumber = 1,
                PageSize = 10,
                TotalRecords = 1,
                TotalPages = 1
            };
            var serviceResult = new Apha.FPS.Application.Pagination.PaginatedResult<ProgramDto>(programDtos, paginationData);

            var expectedApiResponse = new Apha.Common.Contracts.PaginationRes<ProgramRes>
            {
                Data = new List<ProgramRes>
                        {
                            new ProgramRes { ProgramNo = "P1", ProgramName = "Test Program" }
                        },
                PaginationData = new Apha.Common.Contracts.Pagination
                {
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 1,
                    TotalPages = 1
                }
            };

            _serviceMock.GetAllProgramsAsync(query).Returns(serviceResult);
            _mapperMock.Map<Apha.Common.Contracts.PaginationRes<ProgramRes>>(serviceResult).Returns(expectedApiResponse);

            // Act
            var result = await _controller.GetAllProgramsPagedAsync(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedApiResponse, okResult.Value);
        }

        [Fact]
        public async Task GetAllProgramsPagedAsync_NullResult_ThrowsArgumentException()
        {
            var query = new Apha.FPS.Application.Pagination.QueryParameters<string>();
            _serviceMock.GetAllProgramsAsync(query).Returns((Apha.FPS.Application.Pagination.PaginatedResult<ProgramDto>?)null);

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetAllProgramsPagedAsync(query));
        }

        [Fact]
        public async Task GetProgramById_HappyPath_ReturnsOk()
        {
            var dto = new ProgramDto { ProgramNo = "P1" };
            var mapped = new ProgramRes { ProgramNo = "P1" };

            _serviceMock.GetProgramByIdAsync("P1").Returns(dto);
            _mapperMock.Map<ProgramRes>(dto).Returns(mapped);

            var result = await _controller.GetProgramById("P1");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetProgramById_NullResult_ThrowsArgumentException()
        {
            _serviceMock.GetProgramByIdAsync("P2").Returns((ProgramDto?)null);

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetProgramById("P2"));
        }

        [Fact]
        public async Task CreateProgram_HappyPath_ReturnsOk()
        {
            var req = new ProgramReq { ProgramNo = "P1" };
            var dto = new ProgramDto { ProgramNo = "P1" };
            var resultDto = new ProgramDto { ProgramNo = "P1" };
            var mapped = new ProgramRes { ProgramNo = "P1" };

            _mapperMock.Map<ProgramDto>(req).Returns(dto);
            _serviceMock.AddProgramAsync(dto).Returns(resultDto);
            _mapperMock.Map<ProgramRes>(resultDto).Returns(mapped);

            var result = await _controller.CreateProgram(req);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task CreateProgram_Error_ServiceThrows()
        {
            var req = new ProgramReq { ProgramNo = "P1" };
            var dto = new ProgramDto { ProgramNo = "P1" };

            _mapperMock.Map<ProgramDto>(req).Returns(dto);
            _serviceMock.AddProgramAsync(dto).Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.CreateProgram(req));
        }

        [Fact]
        public async Task UpdateProgram_HappyPath_ReturnsOk()
        {
            var req = new ProgramReq { ProgramNo = "P1" };
            var dto = new ProgramDto { ProgramNo = "P1" };
            var resultDto = new ProgramDto { ProgramNo = "P1" };
            var mapped = new ProgramRes { ProgramNo = "P1" };

            _mapperMock.Map<ProgramDto>(req).Returns(dto);
            _serviceMock.UpdateProgramAsync(dto).Returns(resultDto);
            _mapperMock.Map<ProgramRes>(resultDto).Returns(mapped);

            var result = await _controller.UpdateProgram(req);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task UpdateProgram_Error_ServiceThrows()
        {
            var req = new ProgramReq { ProgramNo = "P1" };
            var dto = new ProgramDto { ProgramNo = "P1" };

            _mapperMock.Map<ProgramDto>(req).Returns(dto);
            _serviceMock.UpdateProgramAsync(dto).Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.UpdateProgram(req));
        }

        [Fact]
        public async Task DeleteProgram_HappyPath_ReturnsOk()
        {
            _serviceMock.DeleteProgramAsync("P1").Returns(true);

            var result = await _controller.DeleteProgram("P1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task DeleteProgram_NullOrEmpty_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteProgram(""));
        }

        [Fact]
        public async Task DeleteProgram_NotFound_ThrowsArgumentException()
        {
            _serviceMock.DeleteProgramAsync("P2").Returns(false);

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteProgram("P2"));
        }
    }
}