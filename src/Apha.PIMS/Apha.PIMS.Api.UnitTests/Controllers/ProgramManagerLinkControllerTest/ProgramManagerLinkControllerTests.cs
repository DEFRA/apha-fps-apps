using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.PIMS.Api.UnitTests.Controllers.ProgramManagerLinkControllerTest
{
    public class ProgramManagerLinkControllerTests
    {
        private readonly IProgramManagerLinkService _service;
        private readonly IMapper _mapper;
        private readonly ProgramManagerLinkController _controller;

        public ProgramManagerLinkControllerTests()
        {
            _service = Substitute.For<IProgramManagerLinkService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new ProgramManagerLinkController(_service, _mapper);
        }

        private static ProgramManagerLinkDto MakeDto(string program = "PR1", string mgr = "DOMAIN\\user1") =>
            new() { Program = program, Manager = mgr };

        private static ProgramManagerLinkRes MakeRes(string program = "PR1", string mgr = "DOMAIN\\user1") =>
            new() { Program = program, Manager = mgr };

        [Fact]
        public async Task GetAllProgramManagerLinks_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            var dtos = new List<ProgramManagerLinkDto> { MakeDto("PR1", "a"), MakeDto("PR2", "b") };
            var res = new List<ProgramManagerLinkRes> { MakeRes("PR1", "a"), MakeRes("PR2", "b") };
            _service.GetAllProgramManagerLinksAsync().Returns(dtos);
            _mapper.Map<List<ProgramManagerLinkRes>>(dtos).Returns(res);

            var result = await _controller.GetAllProgramManagerLinks();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
        }

        [Fact]
        public async Task GetProgramManagerLinkById_ServiceReturnsDto_ReturnsOkWithMappedResult()
        {
            const string encodedProgram = "PR%201";
            const string encodedManager = "DOMAIN%5Cuser1";
            const string decodedProgram = "PR 1";
            const string decodedManager = "DOMAIN\\user1";
            var dto = MakeDto(decodedProgram, decodedManager);
            var res = MakeRes(decodedProgram, decodedManager);

            _service.GetProgramManagerLinkByIdAsync(decodedProgram, decodedManager).Returns(dto);
            _mapper.Map<ProgramManagerLinkRes>(dto).Returns(res);

            var result = await _controller.GetProgramManagerLinkById(encodedProgram, encodedManager);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
            await _service.Received(1).GetProgramManagerLinkByIdAsync(decodedProgram, decodedManager);
        }

        [Fact]
        public async Task GetProgramManagerLinkById_ServiceReturnsNull_ReturnsJsonSuccessResponseWithNullData()
        {
            _service.GetProgramManagerLinkByIdAsync(Arg.Any<string>(), Arg.Any<string>()).Returns((ProgramManagerLinkDto?)null);

            var result = await _controller.GetProgramManagerLinkById("PRX", "DOMAIN%5Cnone");

            var json = Assert.IsType<JsonResult>(result);
            var apiResponse = Assert.IsType<Apha.Common.Contracts.ApiResponse<ProgramManagerLinkRes>>(json.Value);
            Assert.True(apiResponse.Success);
            Assert.Null(apiResponse.Data);
            Assert.NotNull(apiResponse.Meta);
            _mapper.DidNotReceive().Map<ProgramManagerLinkRes>(Arg.Any<ProgramManagerLinkDto>());
        }

        [Fact]
        public async Task GetPagedByManager_DecodesManagerBeforeServiceCall()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string encodedManager = "DOMAIN%5Cjsmith";
            const string decodedManager = "DOMAIN\\jsmith";
            var pagedDto = new PaginatedResult<ProgramManagerLinkDto>(new List<ProgramManagerLinkDto> { MakeDto("PR1", decodedManager) }, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 });
            var pagedRes = new PaginationRes<ProgramManagerLinkRes>(new List<ProgramManagerLinkRes> { MakeRes("PR1", decodedManager) }, new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 });

            _service.GetPagedByManagerAsync(query, decodedManager).Returns(pagedDto);
            _mapper.Map<PaginationRes<ProgramManagerLinkRes>>(pagedDto).Returns(pagedRes);

            var result = await _controller.GetPagedByManager(query, encodedManager);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(pagedRes, ok.Value);
            await _service.Received(1).GetPagedByManagerAsync(query, decodedManager);
        }
    }
}
