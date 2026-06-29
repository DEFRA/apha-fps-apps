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

namespace Apha.PACT.Api.UnitTests.Controller.JobCodeControllerTest
{
    public class JobCodeControllerTests
    {
        private readonly IJobCodeService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly JobCodeController _controller;

        public JobCodeControllerTests()
        {
            _serviceMock = Substitute.For<IJobCodeService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new JobCodeController(_serviceMock, _mapperMock);
        }

        #region GetAll

        [Fact]
        public async Task GetAll_HappyPath_ReturnsOk()
        {
            var dtos = new List<JobCodeDto>
            {
                new JobCodeDto { JobCodeId = "JC1", ParentProject = "PRJ1" },
                new JobCodeDto { JobCodeId = "JC2", ParentProject = "PRJ2" }
            };
            var mapped = new List<JobCodeRes>
            {
                new JobCodeRes { JobCodeId = "JC1", ParentProject = "PRJ1" },
                new JobCodeRes { JobCodeId = "JC2", ParentProject = "PRJ2" }
            };

            _serviceMock.GetJobCodesAsync().Returns(dtos);
            _mapperMock.Map<IEnumerable<JobCodeRes>>(dtos).Returns(mapped);

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetAll_EmptyList_ReturnsOkWithEmptyCollection()
        {
            var dtos = new List<JobCodeDto>();
            var mapped = new List<JobCodeRes>();

            _serviceMock.GetJobCodesAsync().Returns(dtos);
            _mapperMock.Map<IEnumerable<JobCodeRes>>(dtos).Returns(mapped);

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetAll_ServiceThrows_PropagatesException()
        {
            _serviceMock.GetJobCodesAsync().ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetAll());
        }

        #endregion

        #region GetByProject

        [Fact]
        public async Task GetByProject_HappyPath_ReturnsOk()
        {
            var dtos = new List<JobCodeDto> { new JobCodeDto { JobCodeId = "JC1", ParentProject = "PRJ1" } };
            var mapped = new List<JobCodeRes> { new JobCodeRes { JobCodeId = "JC1", ParentProject = "PRJ1" } };

            _serviceMock.GetJobCodesByProjectAsync("PRJ1").Returns(dtos);
            _mapperMock.Map<IEnumerable<JobCodeRes>>(dtos).Returns(mapped);

            var result = await _controller.GetByProject("PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetByProject_ServiceThrows_PropagatesException()
        {
            _serviceMock.GetJobCodesByProjectAsync("PRJ1").ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetByProject("PRJ1"));
        }

        #endregion

        #region GetPaged

        [Fact]
        public async Task GetPaged_HappyPath_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<JobCodeDto> { new JobCodeDto { JobCodeId = "JC1" } };
            var paginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var serviceResult = new PaginatedResult<JobCodeDto>(dtos, paginationData);
            var expectedResponse = new PaginationRes<JobCodeRes>
            {
                Data = new List<JobCodeRes> { new JobCodeRes { JobCodeId = "JC1" } },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 }
            };

            _serviceMock.GetPagedJobCodesAsync(query, "PRJ1").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<JobCodeRes>>(serviceResult).Returns(expectedResponse);

            var result = await _controller.GetPaged(query, "PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        [Fact]
        public async Task GetPaged_NullParentProject_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<JobCodeDto>(Enumerable.Empty<JobCodeDto>(), new PaginationDto());
            var expectedResponse = new PaginationRes<JobCodeRes>();

            _serviceMock.GetPagedJobCodesAsync(query, null).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<JobCodeRes>>(serviceResult).Returns(expectedResponse);

            var result = await _controller.GetPaged(query, null);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        #endregion

        #region GetById

        [Fact]
        public async Task GetById_HappyPath_ReturnsOk()
        {
            var dto = new JobCodeDto { JobCodeId = "JC1" };
            var mapped = new JobCodeRes { JobCodeId = "JC1" };

            _serviceMock.GetJobCodeByIdAsync("JC1").Returns(dto);
            _mapperMock.Map<JobCodeRes>(dto).Returns(mapped);

            var result = await _controller.GetById("JC1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetById_NullResult_ReturnsNotFound()
        {
            _serviceMock.GetJobCodeByIdAsync("MISSING").Returns((JobCodeDto?)null);

            var result = await _controller.GetById("MISSING");

            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region GetTypes

        [Fact]
        public async Task GetTypes_HappyPath_ReturnsOk()
        {
            var types = new List<string> { "TypeA", "TypeB" };
            _serviceMock.GetTypesAsync().Returns(types);

            var result = await _controller.GetTypes();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(types, okResult.Value);
        }

        #endregion

        #region Create

        [Fact]
        public async Task Create_HappyPath_ReturnsCreatedAtAction()
        {
            var req = new JobCodeReq { JobCodeId = "JC1", ParentProject = "PRJ1" };
            var dto = new JobCodeDto { JobCodeId = "JC1", ParentProject = "PRJ1" };
            var createdDto = new JobCodeDto { JobCodeId = "JC1", ParentProject = "PRJ1" };
            var mapped = new JobCodeRes { JobCodeId = "JC1", ParentProject = "PRJ1" };

            _mapperMock.Map<JobCodeDto>(req).Returns(dto);
            _serviceMock.CreateJobCodeAsync(dto).Returns(createdDto);
            _mapperMock.Map<JobCodeRes>(createdDto).Returns(mapped);

            var result = await _controller.Create(req);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(mapped, createdResult.Value);
            Assert.Equal("JC1", createdResult.RouteValues!["jobCodeId"]);
        }

        [Fact]
        public async Task Create_ServiceThrows_PropagatesException()
        {
            var req = new JobCodeReq { JobCodeId = "JC1" };
            var dto = new JobCodeDto { JobCodeId = "JC1" };

            _mapperMock.Map<JobCodeDto>(req).Returns(dto);
            _serviceMock.CreateJobCodeAsync(dto).ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.Create(req));
        }

        #endregion

        #region Update

        [Fact]
        public async Task Update_HappyPath_ReturnsOk()
        {
            var req = new JobCodeReq { JobCodeId = "JC1" };
            var dto = new JobCodeDto { JobCodeId = "JC1" };
            var updatedDto = new JobCodeDto { JobCodeId = "JC1" };
            var mapped = new JobCodeRes { JobCodeId = "JC1" };

            _mapperMock.Map<JobCodeDto>(req).Returns(dto);
            _serviceMock.UpdateJobCodeAsync(dto).Returns(updatedDto);
            _mapperMock.Map<JobCodeRes>(updatedDto).Returns(mapped);

            var result = await _controller.Update(req);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task Update_ServiceThrows_PropagatesException()
        {
            var req = new JobCodeReq { JobCodeId = "JC1" };
            var dto = new JobCodeDto { JobCodeId = "JC1" };

            _mapperMock.Map<JobCodeDto>(req).Returns(dto);
            _serviceMock.UpdateJobCodeAsync(dto).ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.Update(req));
        }

        #endregion

        #region Delete

        [Fact]
        public async Task Delete_HappyPath_ReturnsOk()
        {
            _serviceMock.DeleteJobCodeAsync("JC1").Returns(true);

            var result = await _controller.Delete("JC1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task Delete_NotFound_ThrowsArgumentException()
        {
            _serviceMock.DeleteJobCodeAsync("MISSING").Returns(false);

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.Delete("MISSING"));
        }

        #endregion

        #region GetZtCodesAsync

        [Fact]
        public async Task GetZtCodesAsync_HappyPath_ReturnsOk()
        {
            var dtos = new List<JobCodeZtDto>
            {
                new() { JobCode = "ZT001", Description = "ZT Project 1" },
                new() { JobCode = "ZT002", Description = "ZT Project 2" }
            };
            var mapped = new List<JobCodeZtRes>
            {
                new() { JobCode = "ZT001", Description = "ZT Project 1" },
                new() { JobCode = "ZT002", Description = "ZT Project 2" }
            };

            _serviceMock.GetZtCodeLookupAsync().Returns(dtos);
            _mapperMock.Map<IEnumerable<JobCodeZtRes>>(dtos).Returns(mapped);

            var result = await _controller.GetZtCodesAsync();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetZtCodesAsync_EmptyList_ReturnsOkWithEmptyCollection()
        {
            var dtos = new List<JobCodeZtDto>();
            var mapped = new List<JobCodeZtRes>();

            _serviceMock.GetZtCodeLookupAsync().Returns(dtos);
            _mapperMock.Map<IEnumerable<JobCodeZtRes>>(dtos).Returns(mapped);

            var result = await _controller.GetZtCodesAsync();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetZtCodesAsync_ServiceThrows_PropagatesException()
        {
            _serviceMock.GetZtCodeLookupAsync().ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetZtCodesAsync());
        }

        #endregion
    }
}
