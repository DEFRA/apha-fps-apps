using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.PIMS.Api.UnitTests.Controllers.ProfitCentreManagerLinkControllerTest
{
    public class ProfitCentreManagerLinkControllerTests
    {
        private readonly IProfitCentreManagerLinkService _service;
        private readonly IMapper _mapper;
        private readonly ProfitCentreManagerLinkController _controller;

        public ProfitCentreManagerLinkControllerTests()
        {
            _service = Substitute.For<IProfitCentreManagerLinkService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new ProfitCentreManagerLinkController(_service, _mapper);
        }

        private static ProfitCentreManagerLinkDto MakeDto(string pc = "PC1", string mgr = "DOMAIN\\user1") =>
            new() { ProfitCentre = pc, Manager = mgr };

        private static ProfitCentreManagerLinkRes MakeRes(string pc = "PC1", string mgr = "DOMAIN\\user1") =>
            new() { ProfitCentre = pc, Manager = mgr };

        [Fact]
        public async Task GetAllProfitCentreManagerLinks_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            var dtos = new List<ProfitCentreManagerLinkDto> { MakeDto("PC1", "a"), MakeDto("PC2", "b") };
            var res = new List<ProfitCentreManagerLinkRes> { MakeRes("PC1", "a"), MakeRes("PC2", "b") };
            _service.GetAllProfitCentreManagerLinksAsync().Returns(dtos);
            _mapper.Map<List<ProfitCentreManagerLinkRes>>(dtos).Returns(res);

            var result = await _controller.GetAllProfitCentreManagerLinks();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
        }

        [Fact]
        public async Task GetProfitCentreManagerLinkById_ServiceReturnsDto_ReturnsOkWithMappedResult()
        {
            const string encodedPc = "PC%201";
            const string encodedManager = "DOMAIN%5Cuser1";
            const string decodedPc = "PC 1";
            const string decodedManager = "DOMAIN\\user1";
            var dto = MakeDto(decodedPc, decodedManager);
            var res = MakeRes(decodedPc, decodedManager);

            _service.GetProfitCentreManagerLinkByIdAsync(decodedPc, decodedManager).Returns(dto);
            _mapper.Map<ProfitCentreManagerLinkRes>(dto).Returns(res);

            var result = await _controller.GetProfitCentreManagerLinkById(encodedPc, encodedManager);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
            await _service.Received(1).GetProfitCentreManagerLinkByIdAsync(decodedPc, decodedManager);
        }

        [Fact]
        public async Task GetProfitCentreManagerLinkById_ServiceReturnsNull_ReturnsJsonSuccessResponseWithNullData()
        {
            _service.GetProfitCentreManagerLinkByIdAsync(Arg.Any<string>(), Arg.Any<string>()).Returns((ProfitCentreManagerLinkDto?)null);

            var result = await _controller.GetProfitCentreManagerLinkById("PCX", "DOMAIN%5Cnone");

            var json = Assert.IsType<JsonResult>(result);
            var apiResponse = Assert.IsType<Apha.Common.Contracts.ApiResponse<ProfitCentreManagerLinkRes>>(json.Value);
            Assert.True(apiResponse.Success);
            Assert.Null(apiResponse.Data);
            Assert.NotNull(apiResponse.Meta);
            _mapper.DidNotReceive().Map<ProfitCentreManagerLinkRes>(Arg.Any<ProfitCentreManagerLinkDto>());
        }

        [Fact]
        public async Task GetPagedByManager_DecodesManagerBeforeServiceCall()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string encodedManager = "DOMAIN%5Cjsmith";
            const string decodedManager = "DOMAIN\\jsmith";
            var pagedDto = new PaginatedResult<ProfitCentreManagerLinkDto>(new List<ProfitCentreManagerLinkDto> { MakeDto("PC1", decodedManager) }, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 });
            var pagedRes = new PaginationRes<ProfitCentreManagerLinkRes>(new List<ProfitCentreManagerLinkRes> { MakeRes("PC1", decodedManager) }, new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 });

            _service.GetPagedByManagerAsync(query, decodedManager).Returns(pagedDto);
            _mapper.Map<PaginationRes<ProfitCentreManagerLinkRes>>(pagedDto).Returns(pagedRes);

            var result = await _controller.GetPagedByManager(query, encodedManager);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(pagedRes, ok.Value);
            await _service.Received(1).GetPagedByManagerAsync(query, decodedManager);
        }
    }
}
