using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Api.Controllers;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Api.UnitTests.Controller.TimeCodeValidControllerTest
{
    public class TimeCodeValidControllerTests
    {
        private readonly ITimeCodeValidService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly TimeCodeValidController _controller;

        public TimeCodeValidControllerTests()
        {
            _serviceMock = Substitute.For<ITimeCodeValidService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new TimeCodeValidController(_serviceMock, _mapperMock);
        }

        #region GetByJobCode

        [Fact]
        public async Task GetByJobCode_HappyPath_ReturnsOk()
        {
            var dtos = new List<TimeCodeValidDto> { new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" } };
            var mapped = new List<TimeCodeValidRes> { new TimeCodeValidRes { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" } };

            _serviceMock.GetByJobCodeAsync("JC1", "PRJ1").Returns(dtos);
            _mapperMock.Map<IEnumerable<TimeCodeValidRes>>(dtos).Returns(mapped);

            var result = await _controller.GetByJobCode("JC1", "PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetByJobCode_ServiceThrows_PropagatesException()
        {
            _serviceMock.GetByJobCodeAsync("JC1", "PRJ1").Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetByJobCode("JC1", "PRJ1"));
        }

        #endregion

        #region GetPaged

        [Fact]
        public async Task GetPaged_HappyPath_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<TimeCodeValidDto> { new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" } };
            var paginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var serviceResult = new PaginatedResult<TimeCodeValidDto>(dtos, paginationData);
            var expectedResponse = new PaginationRes<TimeCodeValidRes>
            {
                Data = new List<TimeCodeValidRes> { new TimeCodeValidRes { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" } },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 }
            };

            _serviceMock.GetPagedTimeCodesAsync(query, "JC1", "PRJ1").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<TimeCodeValidRes>>(serviceResult).Returns(expectedResponse);

            var result = await _controller.GetPaged(query, "JC1", "PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        [Fact]
        public async Task GetPaged_NullFilters_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TimeCodeValidDto>(Enumerable.Empty<TimeCodeValidDto>(), new PaginationDto());
            var expectedResponse = new PaginationRes<TimeCodeValidRes>();

            _serviceMock.GetPagedTimeCodesAsync(query, null, null).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<TimeCodeValidRes>>(serviceResult).Returns(expectedResponse);

            var result = await _controller.GetPaged(query, null, null);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        #endregion

        #region GetById

        [Fact]
        public async Task GetById_HappyPath_ReturnsOk()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };
            var mapped = new TimeCodeValidRes { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };

            _serviceMock.GetTimeCodeValidAsync("WG1", "TC1", "PRJ1").Returns(dto);
            _mapperMock.Map<TimeCodeValidRes>(dto).Returns(mapped);

            var result = await _controller.GetById("WG1", "TC1", "PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetById_NullResult_ReturnsNotFound()
        {
            _serviceMock.GetTimeCodeValidAsync("WG_MISSING", "TC_MISSING", "PRJ1").Returns((TimeCodeValidDto?)null);

            var result = await _controller.GetById("WG_MISSING", "TC_MISSING", "PRJ1");

            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region Create

        [Fact]
        public async Task Create_HappyPath_ReturnsOk()
        {
            var req = new TimeCodeValidReq { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };
            var createdDto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };
            var mapped = new TimeCodeValidRes { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };

            _mapperMock.Map<TimeCodeValidDto>(req).Returns(dto);
            _serviceMock.CreateTimeCodeValidAsync(dto).Returns(createdDto);
            _mapperMock.Map<TimeCodeValidRes>(createdDto).Returns(mapped);

            var result = await _controller.Create(req);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task Create_ServiceThrows_PropagatesException()
        {
            var req = new TimeCodeValidReq { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };

            _mapperMock.Map<TimeCodeValidDto>(req).Returns(dto);
            _serviceMock.CreateTimeCodeValidAsync(dto).Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.Create(req));
        }

        #endregion

        #region Update

        [Fact]
        public async Task Update_HappyPath_ReturnsOk()
        {
            var req = new TimeCodeValidReq { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };
            var updatedDto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };
            var mapped = new TimeCodeValidRes { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };

            _mapperMock.Map<TimeCodeValidDto>(req).Returns(dto);
            _serviceMock.UpdateTimeCodeValidAsync(dto).Returns(updatedDto);
            _mapperMock.Map<TimeCodeValidRes>(updatedDto).Returns(mapped);

            var result = await _controller.Update(req);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task Update_ServiceThrows_PropagatesException()
        {
            var req = new TimeCodeValidReq { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };

            _mapperMock.Map<TimeCodeValidDto>(req).Returns(dto);
            _serviceMock.UpdateTimeCodeValidAsync(dto).Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.Update(req));
        }

        #endregion

        #region Delete

        [Fact]
        public async Task Delete_HappyPath_ReturnsOk()
        {
            _serviceMock.DeleteTimeCodeValidAsync("WG1", "TC1", "PRJ1").Returns(true);

            var result = await _controller.Delete("WG1", "TC1", "PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task Delete_NotFound_ThrowsArgumentException()
        {
            _serviceMock.DeleteTimeCodeValidAsync("WG_MISSING", "TC_MISSING", "PRJ1").Returns(false);

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.Delete("WG_MISSING", "TC_MISSING", "PRJ1"));
        }

        #endregion

        #region DeleteAllByJobCode

        [Fact]
        public async Task DeleteAllByJobCode_HappyPath_ReturnsOk()
        {
            _serviceMock.DeleteAllByJobCodeAsync("JC1", "PRJ1").Returns(true);

            var result = await _controller.DeleteAllByJobCode("JC1", "PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task DeleteAllByJobCode_NotFound_ThrowsArgumentException()
        {
            _serviceMock.DeleteAllByJobCodeAsync("JC_MISSING", "PRJ1").Returns(false);

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteAllByJobCode("JC_MISSING", "PRJ1"));
        }

        #endregion

        #region CopyWorkGroup

        [Fact]
        public async Task CopyWorkGroup_HappyPath_ReturnsOk()
        {
            var dtos = new List<TimeCodeValidDto> { new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG2", ParentProject = "PRJ1" } };
            var mapped = new List<TimeCodeValidRes> { new TimeCodeValidRes { TimeCode = "TC1", WorkGroup = "WG2", ParentProject = "PRJ1" } };

            _serviceMock.CopyWorkGroupAsync("JC_SRC", "JC_TGT", "PRJ1").Returns(dtos);
            _mapperMock.Map<IEnumerable<TimeCodeValidRes>>(dtos).Returns(mapped);

            var result = await _controller.CopyWorkGroup("JC_SRC", "JC_TGT", "PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task CopyWorkGroup_ServiceThrows_PropagatesException()
        {
            _serviceMock.CopyWorkGroupAsync("JC_SRC", "JC_TGT", "PRJ1").Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.CopyWorkGroup("JC_SRC", "JC_TGT", "PRJ1"));
        }

        #endregion
    }
}
