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

namespace Apha.PACT.Api.UnitTests.Controller.ProjectSubContractControllerTest
{
    public class ProjectSubContractControllerTests
    {
        private readonly IProjectSubContractService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ProjectSubContractController _controller;

        public ProjectSubContractControllerTests()
        {
            _serviceMock = Substitute.For<IProjectSubContractService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new ProjectSubContractController(_serviceMock, _mapperMock);
        }

        #region GetPaged

        [Fact]
        public async Task GetPaged_ValidQueryWithProject_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<ProjectSubContractDto> { new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1" } };
            var paginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var serviceResult = new PaginatedResult<ProjectSubContractDto>(dtos, paginationData);
            var expectedResponse = new PaginationRes<ProjectSubContractRes>
            {
                Data = new List<ProjectSubContractRes> { new ProjectSubContractRes { SubContCounter = 1, Project = "PRJ1" } },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 }
            };

            _serviceMock.GetPagedProjectSubContractsAsync(query, "PRJ1").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectSubContractRes>>(serviceResult).Returns(expectedResponse);

            var result = await _controller.GetPaged(query, "PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        [Fact]
        public async Task GetPaged_NullProject_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<ProjectSubContractDto>(Enumerable.Empty<ProjectSubContractDto>(), new PaginationDto());
            var expectedResponse = new PaginationRes<ProjectSubContractRes>();

            _serviceMock.GetPagedProjectSubContractsAsync(query, null).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectSubContractRes>>(serviceResult).Returns(expectedResponse);

            var result = await _controller.GetPaged(query, null);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        #endregion

        #region GetTotal

        [Fact]
        public async Task GetTotal_ValidProject_ReturnsOk()
        {
            _serviceMock.GetTotalAmountAsync("PRJ1").Returns(2500.00m);

            var result = await _controller.GetTotal("PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(2500.00m, okResult.Value);
        }

        [Fact]
        public async Task GetTotal_NullProject_ReturnsOk()
        {
            _serviceMock.GetTotalAmountAsync(null).Returns(0m);

            var result = await _controller.GetTotal(null);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(0m, okResult.Value);
        }

        #endregion

        #region GetById

        [Fact]
        public async Task GetById_ExistingId_ReturnsOk()
        {
            var dto = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1" };
            var mapped = new ProjectSubContractRes { SubContCounter = 1, Project = "PRJ1" };

            _serviceMock.GetByIdAsync(1).Returns(dto);
            _mapperMock.Map<ProjectSubContractRes>(dto).Returns(mapped);

            var result = await _controller.GetById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetById_NullResult_ThrowsKeyNotFoundException()
        {
            _serviceMock.GetByIdAsync(99).Returns((ProjectSubContractDto?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetById(99));
        }

        #endregion

        #region Create

        [Fact]
        public async Task Create_ValidRequest_ReturnsCreatedAtAction()
        {
            var req = new ProjectSubContractReq { Project = "PRJ1", Amount = 500m };
            var dto = new ProjectSubContractDto { Project = "PRJ1", Amount = 500m };
            var createdDto = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", Amount = 500m };
            var mapped = new ProjectSubContractRes { SubContCounter = 1, Project = "PRJ1", Amount = 500m };

            _mapperMock.Map<ProjectSubContractDto>(req).Returns(dto);
            _serviceMock.CreateAsync(dto).Returns(createdDto);
            _mapperMock.Map<ProjectSubContractRes>(createdDto).Returns(mapped);

            var result = await _controller.Create(req);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(mapped, createdResult.Value);
            Assert.Equal(1, createdResult.RouteValues!["id"]);
        }

        [Fact]
        public async Task Create_ServiceThrows_PropagatesException()
        {
            var req = new ProjectSubContractReq { Project = "PRJ1" };
            var dto = new ProjectSubContractDto { Project = "PRJ1" };

            _mapperMock.Map<ProjectSubContractDto>(req).Returns(dto);
            _serviceMock.CreateAsync(dto).ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.Create(req));
        }

        #endregion

        #region Update

        [Fact]
        public async Task Update_ValidRequest_ReturnsOk()
        {
            var req = new ProjectSubContractReq { Project = "PRJ1", Amount = 750m };
            var dto = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", Amount = 750m };
            var updatedDto = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", Amount = 750m };
            var mapped = new ProjectSubContractRes { SubContCounter = 1, Project = "PRJ1", Amount = 750m };

            _mapperMock.Map<ProjectSubContractDto>(req).Returns(dto);
            _serviceMock.UpdateAsync(dto).Returns(updatedDto);
            _mapperMock.Map<ProjectSubContractRes>(updatedDto).Returns(mapped);

            var result = await _controller.Update(1, req);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task Update_ServiceThrows_PropagatesException()
        {
            var req = new ProjectSubContractReq { Project = "PRJ1" };
            var dto = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1" };

            _mapperMock.Map<ProjectSubContractDto>(req).Returns(dto);
            _serviceMock.UpdateAsync(dto).ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.Update(1, req));
        }

        #endregion

        #region Delete

        [Fact]
        public async Task Delete_ExistingId_ReturnsOk()
        {
            _serviceMock.DeleteAsync(1).Returns(true);

            var result = await _controller.Delete(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task Delete_RecordNotFound_ReturnsOkWithFalse()
        {
            _serviceMock.DeleteAsync(99).Returns(false);

            var result = await _controller.Delete(99);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.False((bool)okResult.Value!);
        }

        #endregion

        #region GetFpsProjectSubContracts

        [Fact]
        public async Task GetFpsProjectSubContracts_ValidQueryWithProject_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<ProjectSubContractDto> { new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", AcctCode = "LargeAnimals" } };
            var paginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var serviceResult = new PaginatedResult<ProjectSubContractDto>(dtos, paginationData);
            var expectedResponse = new PaginationRes<ProjectSubContractRes>
            {
                Data = new List<ProjectSubContractRes> { new ProjectSubContractRes { SubContCounter = 1, Project = "PRJ1" } },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 }
            };

            _serviceMock.GetFpsProjectSubContractsAsync(query, "PRJ1").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectSubContractRes>>(serviceResult).Returns(expectedResponse);

            var result = await _controller.GetFpsProjectSubContracts(query, "PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        [Fact]
        public async Task GetFpsProjectSubContracts_NullProject_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<ProjectSubContractDto>(Enumerable.Empty<ProjectSubContractDto>(), new PaginationDto());
            var expectedResponse = new PaginationRes<ProjectSubContractRes>();

            _serviceMock.GetFpsProjectSubContractsAsync(query, null).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectSubContractRes>>(serviceResult).Returns(expectedResponse);

            var result = await _controller.GetFpsProjectSubContracts(query, null);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        #endregion

        #region GetFpsProjectTotal

        [Fact]
        public async Task GetFpsProjectTotal_ValidProject_ReturnsOk()
        {
            _serviceMock.GetFpsProjectSubContractTotalAmountAsync("PRJ1").Returns(1500.00m);

            var result = await _controller.GetFpsProjectTotal("PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1500.00m, okResult.Value);
        }

        [Fact]
        public async Task GetFpsProjectTotal_NullProject_ReturnsOk()
        {
            _serviceMock.GetFpsProjectSubContractTotalAmountAsync(null).Returns(0m);

            var result = await _controller.GetFpsProjectTotal(null);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(0m, okResult.Value);
        }

        #endregion
    }
}
