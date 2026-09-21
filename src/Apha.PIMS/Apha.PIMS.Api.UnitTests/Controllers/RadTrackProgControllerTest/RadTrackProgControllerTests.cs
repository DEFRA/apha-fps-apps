using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.Common.Utilities.Query;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Api.UnitTests.Controllers.RadTrackProgControllerTest
{
    public class RadTrackProgControllerTests
    {
        private readonly IRadTrackProgService _service;
        private readonly IMapper _mapper;
        private readonly RadTrackProgController _controller;

        public RadTrackProgControllerTests()
        {
            _service = Substitute.For<IRadTrackProgService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new RadTrackProgController(_service, _mapper);
        }

        private static RadTrackProgDto MakeDto(string program = "PROG001", bool radTrackProg = true, string? publicationPrefix = "PRE") =>
            new() { Program = program, RadTrackProg = radTrackProg, PublicationPrefix = publicationPrefix };

        private static RadTrackProgRes MakeRes(string program = "PROG001", bool radTrackProg = true, string? publicationPrefix = "PRE") =>
            new() { Program = program, RadTrackProg = radTrackProg, PublicationPrefix = publicationPrefix };

        #region GetAllRadTrackProgs

        [Fact]
        public async Task GetAllRadTrackProgs_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = new List<RadTrackProgDto> { MakeDto("PROG001"), MakeDto("PROG002") };
            var resList = new List<RadTrackProgRes> { MakeRes("PROG001"), MakeRes("PROG002") };
            _service.GetAllRadTrackProgsAsync().Returns(dtos);
            _mapper.Map<List<RadTrackProgRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAllRadTrackProgs();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<List<RadTrackProgRes>>(ok.Value);
            Assert.Equal(2, returned.Count);
            await _service.Received(1).GetAllRadTrackProgsAsync();
        }

        [Fact]
        public async Task GetAllRadTrackProgs_ServiceReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos = new List<RadTrackProgDto>();
            var resList = new List<RadTrackProgRes>();
            _service.GetAllRadTrackProgsAsync().Returns(dtos);
            _mapper.Map<List<RadTrackProgRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAllRadTrackProgs();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Empty(Assert.IsType<List<RadTrackProgRes>>(ok.Value));
        }

        [Fact]
        public async Task GetAllRadTrackProgs_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetAllRadTrackProgsAsync().ThrowsAsync(new Exception("db error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllRadTrackProgs());
        }

        #endregion

        #region GetAllProgramNames

        [Fact]
        public async Task GetAllProgramNames_ServiceReturnsData_ReturnsOkWithList()
        {
            // Arrange
            var programs = new List<string> { "PROG001", "PROG002", "PROG003" };
            _service.GetAllProgramNamesAsync().Returns(programs);

            // Act
            var result = await _controller.GetAllProgramNames();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<List<string>>(ok.Value);
            Assert.Equal(3, returned.Count);
            Assert.Contains("PROG001", returned);
            await _service.Received(1).GetAllProgramNamesAsync();
        }

        [Fact]
        public async Task GetAllProgramNames_ServiceReturnsNull_ReturnsJsonResultWithSuccessTrueAndNullData()
        {
            // Arrange
            _service.GetAllProgramNamesAsync().Returns((List<string>?)null);

            // Act
            var result = await _controller.GetAllProgramNames();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<ApiResponse<List<string>>>(jsonResult.Value);
            Assert.True(response.Success);
            Assert.Null(response.Data);
            Assert.NotEmpty(response.Meta.CorrelationId);
            await _service.Received(1).GetAllProgramNamesAsync();
        }

        [Fact]
        public async Task GetAllProgramNames_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetAllProgramNamesAsync().ThrowsAsync(new Exception("db error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllProgramNames());
        }

        #endregion

        #region GetPagedRadTrackProgs

        [Fact]
        public async Task GetPagedRadTrackProgs_ServiceReturnsPagedData_ReturnsOkWithMappedPaginationResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<RadTrackProgDto> { MakeDto("PROG001") };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var paginatedResult = new PaginatedResult<RadTrackProgDto>(dtos, paginationDto);
            var pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var mappedRes = new PaginationRes<RadTrackProgRes>(
                new List<RadTrackProgRes> { MakeRes("PROG001") },
                pagination
            );
            _service.GetPagedRadTrackProgsAsync(query).Returns(paginatedResult);
            _mapper.Map<PaginationRes<RadTrackProgRes>>(paginatedResult).Returns(mappedRes);

            // Act
            var result = await _controller.GetPagedRadTrackProgs(query);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<PaginationRes<RadTrackProgRes>>(ok.Value);
            Assert.Single(returned.Data);
            await _service.Received(1).GetPagedRadTrackProgsAsync(query);
        }

        [Fact]
        public async Task GetPagedRadTrackProgs_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _service.GetPagedRadTrackProgsAsync(query).ThrowsAsync(new Exception("db error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPagedRadTrackProgs(query));
        }

        #endregion

        #region GetRadTrackProgByProgram

        [Fact]
        public async Task GetRadTrackProgByProgram_ServiceReturnsData_ReturnsOkWithMappedObject()
        {
            // Arrange
            var program = "PROG001";
            var dto = MakeDto(program);
            var res = MakeRes(program);
            _service.GetRadTrackProgByProgramAsync(program).Returns(dto);
            _mapper.Map<RadTrackProgRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetRadTrackProgByProgram(program);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<RadTrackProgRes>(ok.Value);
            Assert.Equal(program, returned.Program);
            await _service.Received(1).GetRadTrackProgByProgramAsync(program);
        }

        [Fact]
        public async Task GetRadTrackProgByProgram_ServiceReturnsNull_ReturnsJsonResultWithSuccessTrueAndNullData()
        {
            // Arrange
            var program = "INVALID";
            _service.GetRadTrackProgByProgramAsync(program).Returns((RadTrackProgDto?)null);

            // Act
            var result = await _controller.GetRadTrackProgByProgram(program);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<ApiResponse<RadTrackProgRes>>(jsonResult.Value);
            Assert.True(response.Success);
            Assert.Null(response.Data);
            Assert.NotEmpty(response.Meta.CorrelationId);
            await _service.Received(1).GetRadTrackProgByProgramAsync(program);
        }

        [Fact]
        public async Task GetRadTrackProgByProgram_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var program = "PROG001";
            _service.GetRadTrackProgByProgramAsync(program).ThrowsAsync(new Exception("db error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetRadTrackProgByProgram(program));
        }

        #endregion

        #region CreateRadTrackProg

        [Fact]
        public async Task CreateRadTrackProg_WithValidRequest_ReturnsCreatedAtActionWithMappedObject()
        {
            // Arrange
            var request = new RadTrackProgReq { Program = "PROG001", RadTrackProg = true, PublicationPrefix = "PRE" };
            var dto = MakeDto("PROG001");
            var res = MakeRes("PROG001");
            _mapper.Map<RadTrackProgDto>(request).Returns(dto);
            _service.CreateRadTrackProgAsync(dto).Returns(dto);
            _mapper.Map<RadTrackProgRes>(dto).Returns(res);

            // Act
            var result = await _controller.CreateRadTrackProg(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(RadTrackProgController.GetRadTrackProgByProgram), createdResult.ActionName);
            Assert.Equal("PROG001", ((RadTrackProgRes)createdResult.Value!).Program);
            await _service.Received(1).CreateRadTrackProgAsync(Arg.Any<RadTrackProgDto>());
        }

        [Fact]
        public async Task CreateRadTrackProg_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new RadTrackProgReq { Program = "PROG001" };
            var dto = MakeDto("PROG001");
            _mapper.Map<RadTrackProgDto>(request).Returns(dto);
            _service.CreateRadTrackProgAsync(dto).ThrowsAsync(new Exception("create error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.CreateRadTrackProg(request));
        }

        #endregion

        #region UpdateRadTrackProg

        [Fact]
        public async Task UpdateRadTrackProg_WithValidRequest_ReturnsOkWithMappedObject()
        {
            // Arrange
            var program = "PROG001";
            var request = new RadTrackProgReq { Program = program, RadTrackProg = true, PublicationPrefix = "UPDATED" };
            var dto = MakeDto(program, true, "UPDATED");
            var res = MakeRes(program, true, "UPDATED");
            _mapper.Map<RadTrackProgDto>(request).Returns(dto);
            _service.UpdateRadTrackProgAsync(Arg.Any<RadTrackProgDto>()).Returns(dto);
            _mapper.Map<RadTrackProgRes>(dto).Returns(res);

            // Act
            var result = await _controller.UpdateRadTrackProg(program, request);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<RadTrackProgRes>(ok.Value);
            Assert.Equal(program, returned.Program);
            await _service.Received(1).UpdateRadTrackProgAsync(Arg.Any<RadTrackProgDto>());
        }

        [Fact]
        public async Task UpdateRadTrackProg_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var program = "PROG001";
            var request = new RadTrackProgReq { Program = program };
            var dto = MakeDto(program);
            _mapper.Map<RadTrackProgDto>(request).Returns(dto);
            _service.UpdateRadTrackProgAsync(Arg.Any<RadTrackProgDto>()).ThrowsAsync(new Exception("update error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.UpdateRadTrackProg(program, request));
        }

        #endregion

        #region DeleteRadTrackProg

        [Fact]
        public async Task DeleteRadTrackProg_WithExistingProgram_ReturnsOkWithTrue()
        {
            // Arrange
            var program = "PROG001";
            _service.DeleteRadTrackProgAsync(program).Returns(true);

            // Act
            var result = await _controller.DeleteRadTrackProg(program);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)ok.Value!);
            await _service.Received(1).DeleteRadTrackProgAsync(program);
        }

        [Fact]
        public async Task DeleteRadTrackProg_WithNonExistingProgram_ReturnsOkWithFalse()
        {
            // Arrange
            var program = "INVALID";
            _service.DeleteRadTrackProgAsync(program).Returns(false);

            // Act
            var result = await _controller.DeleteRadTrackProg(program);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.False((bool)ok.Value!);
            await _service.Received(1).DeleteRadTrackProgAsync(program);
        }

        [Fact]
        public async Task DeleteRadTrackProg_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var program = "PROG001";
            _service.DeleteRadTrackProgAsync(program).ThrowsAsync(new Exception("delete error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.DeleteRadTrackProg(program));
        }

        #endregion
    }
}
