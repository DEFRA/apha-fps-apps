using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Api.UnitTests.Controllers.FrequencyControllerTest
{
    public class FrequencyControllerTests
    {
        private readonly IFrequencyService _service;
        private readonly IMapper _mapper;
        private readonly FrequencyController _controller;

        public FrequencyControllerTests()
        {
            _service = Substitute.For<IFrequencyService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new FrequencyController(_service, _mapper);
        }

        private static FrequencyDto MakeDto(int id = 1, string value = "Monthly") =>
            new() { FrequencyId = id, FrequencyValue = value };

        private static FrequencyRes MakeRes(int id = 1, string value = "Monthly") =>
            new() { Frequencyid = id, FrequencyValue = value };

        [Fact]
        public async Task GetAllFrequencies_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            var dtos = new List<FrequencyDto> { MakeDto(1), MakeDto(2) };
            var res = new List<FrequencyRes> { MakeRes(1), MakeRes(2) };
            _service.GetAllFrequenciesAsync().Returns(dtos);
            _mapper.Map<List<FrequencyRes>>(dtos).Returns(res);

            var result = await _controller.GetAllFrequencies();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
        }

        [Fact]
        public async Task GetPagedFrequencies_ServiceReturnsData_ReturnsOkWithMappedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pagedDto = new PaginatedResult<FrequencyDto>(new List<FrequencyDto> { MakeDto() }, new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 });
            var pageRes = new PaginationRes<FrequencyRes>(new List<FrequencyRes> { MakeRes() }, new Pagination { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 });
            _service.GetPagedFrequenciesAsync(query).Returns(pagedDto);
            _mapper.Map<PaginationRes<FrequencyRes>>(pagedDto).Returns(pageRes);

            var result = await _controller.GetPagedFrequencies(query);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(pageRes, ok.Value);
        }

        [Fact]
        public async Task GetFrequencyById_ServiceReturnsDto_ReturnsOkWithMappedResult()
        {
            var dto = MakeDto(1);
            var res = MakeRes(1);
            _service.GetFrequencyByIdAsync(1).Returns(dto);
            _mapper.Map<FrequencyRes>(dto).Returns(res);

            var result = await _controller.GetFrequencyById(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
        }

        [Fact]
        public async Task GetFrequencyById_ServiceReturnsNull_ReturnsJsonSuccessResponseWithNullData()
        {
            _service.GetFrequencyByIdAsync(Arg.Any<int>()).Returns((FrequencyDto?)null);

            var result = await _controller.GetFrequencyById(99);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var apiResponse = Assert.IsType<Apha.Common.Contracts.ApiResponse<FrequencyRes>>(jsonResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Null(apiResponse.Data);
            Assert.NotNull(apiResponse.Meta);
            _mapper.DidNotReceive().Map<FrequencyRes>(Arg.Any<FrequencyDto>());
        }

        [Fact]
        public async Task CreateFrequency_ValidRequest_ReturnsCreatedAtAction()
        {
            var req = new FrequencyReq { FrequencyValue = "Monthly" };
            var dto = MakeDto(0);
            var created = MakeDto(5);
            var res = MakeRes(5);
            _mapper.Map<FrequencyDto>(req).Returns(dto);
            _service.CreateFrequencyAsync(dto).Returns(created);
            _mapper.Map<FrequencyRes>(created).Returns(res);

            var result = await _controller.CreateFrequency(req);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(FrequencyController.GetFrequencyById), createdResult.ActionName);
            Assert.Equal(res, createdResult.Value);
        }

        [Fact]
        public async Task UpdateFrequency_ReturnsOkWithMappedResult()
        {
            var req = new FrequencyReq { FrequencyValue = "Updated" };
            var dto = MakeDto(0, "Updated");
            var updated = MakeDto(3, "Updated");
            var res = MakeRes(3, "Updated");
            _mapper.Map<FrequencyDto>(req).Returns(dto);
            _service.UpdateFrequencyAsync(Arg.Any<FrequencyDto>()).Returns(updated);
            _mapper.Map<FrequencyRes>(updated).Returns(res);

            var result = await _controller.UpdateFrequency(3, req);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
        }

        [Fact]
        public async Task DeleteFrequency_ReturnsOkWithDeletedFlag()
        {
            _service.DeleteFrequencyAsync(2).Returns(true);

            var result = await _controller.DeleteFrequency(2);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.True(Assert.IsType<bool>(ok.Value));
        }

        [Fact]
        public async Task GetAllFrequencies_ServiceThrowsException_PropagatesException()
        {
            _service.GetAllFrequenciesAsync().ThrowsAsync(new Exception("db error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllFrequencies());
        }
    }
}
