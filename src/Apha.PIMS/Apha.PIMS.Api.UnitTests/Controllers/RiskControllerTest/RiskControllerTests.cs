using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.PIMS.Api.UnitTests.Controllers.RiskControllerTest
{
    public class RiskControllerTests
    {
        private readonly IRiskService _service;
        private readonly IMapper _mapper;
        private readonly RiskController _controller;

        public RiskControllerTests()
        {
            _service = Substitute.For<IRiskService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new RiskController(_service, _mapper);
        }

        private static RiskDto MakeDto(int id = 1, string rating = "Low") =>
            new() { RiskId = id, RiskRating = rating };

        private static RiskRes MakeRes(int id = 1, string rating = "Low") =>
            new() { Riskid = id, Riskrating = rating };

        [Fact]
        public async Task GetAllRiskRatings_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            var dtos = new List<RiskDto> { MakeDto(1, "Low"), MakeDto(2, "High") };
            var res = new List<RiskRes> { MakeRes(1, "Low"), MakeRes(2, "High") };
            _service.GetAllRiskRatingsAsync().Returns(dtos);
            _mapper.Map<List<RiskRes>>(dtos).Returns(res);

            var result = await _controller.GetAllRiskRatings();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
        }

        [Fact]
        public async Task GetRiskRatingById_ServiceReturnsDto_ReturnsOkWithMappedResult()
        {
            var dto = MakeDto(5, "Medium");
            var res = MakeRes(5, "Medium");
            _service.GetRiskRatingByIdAsync(5).Returns(dto);
            _mapper.Map<RiskRes>(dto).Returns(res);

            var result = await _controller.GetRiskRatingById(5);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
        }

        [Fact]
        public async Task GetRiskRatingById_ServiceReturnsNull_ReturnsJsonSuccessResponseWithNullData()
        {
            _service.GetRiskRatingByIdAsync(Arg.Any<int>()).Returns((RiskDto?)null);

            var result = await _controller.GetRiskRatingById(999);

            var json = Assert.IsType<JsonResult>(result);
            var apiResponse = Assert.IsType<Apha.Common.Contracts.ApiResponse<RiskRes>>(json.Value);
            Assert.True(apiResponse.Success);
            Assert.Null(apiResponse.Data);
            Assert.NotNull(apiResponse.Meta);
            _mapper.DidNotReceive().Map<RiskRes>(Arg.Any<RiskDto>());
        }

        [Fact]
        public async Task GetPagedRiskRatings_ServiceReturnsData_ReturnsOkWithMappedPagination()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pagedDto = new PaginatedResult<RiskDto>(new List<RiskDto> { MakeDto(1, "Low") }, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 });
            var pagedRes = new PaginationRes<RiskRes>(new List<RiskRes> { MakeRes(1, "Low") }, new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 });

            _service.GetPagedRiskRatingsAsync(query).Returns(pagedDto);
            _mapper.Map<PaginationRes<RiskRes>>(pagedDto).Returns(pagedRes);

            var result = await _controller.GetPagedRiskRatings(query);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(pagedRes, ok.Value);
        }
    }
}
