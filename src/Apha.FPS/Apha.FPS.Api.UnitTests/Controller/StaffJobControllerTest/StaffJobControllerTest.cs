using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Api.UnitTests.Controller.StaffJobControllerTest
{
    public class StaffJobControllerTest
    {
        private readonly IStaffJobService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly StaffJobController _controller;

        public StaffJobControllerTest()
        {
            _serviceMock = Substitute.For<IStaffJobService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new StaffJobController(_serviceMock, _mapperMock);
        }

        #region GetJobStaffCostAsync

        [Fact]
        public async Task GetJobStaffCostAsync_HappyPath_ReturnsOk()
        {
            var query = new PaginationReq<string>();
            var serviceResult = new PaginatedResult<StaffJobViewDto>();
            var mappedResult = new PaginationRes<StaffJobViewRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(new QueryParameters<string>());
            _serviceMock.GetJobStaffCostAsync(Arg.Any<QueryParameters<string>>()).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<StaffJobViewRes>>(serviceResult).Returns(mappedResult);

            var result = await _controller.GetJobStaffCostAsync(query);

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_EdgeCase_EmptyResult()
        {
            var query = new PaginationReq<string>();
            var serviceResult = new PaginatedResult<StaffJobViewDto>();
            var mappedResult = new PaginationRes<StaffJobViewRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(new QueryParameters<string>());
            _serviceMock.GetJobStaffCostAsync(Arg.Any<QueryParameters<string>>()).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<StaffJobViewRes>>(serviceResult).Returns(mappedResult);

            var result = await _controller.GetJobStaffCostAsync(query);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_Error_ServiceThrows()
        {
            var query = new PaginationReq<string>();
            _mapperMock.Map<QueryParameters<string>>(query).Returns(new QueryParameters<string>());
            _serviceMock.GetJobStaffCostAsync(Arg.Any<QueryParameters<string>>()).Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetJobStaffCostAsync(query));
        }

        [Fact]
        public async Task GetJobStaffCostAsync_Error_MapperThrows()
        {
            var query = new PaginationReq<string>();
            _mapperMock.Map<QueryParameters<string>>(query).Throws(new Exception("Mapping error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetJobStaffCostAsync(query));
        }

        #endregion

        #region GetStaffWorkgroupLookup

        [Fact]
        public async Task GetStaffWorkgroupLookup_HappyPath_ReturnsOk()
        {
            var serviceResult = new List<StaffWorkgroupLookupDto>();
            var mappedResult = new List<StaffWorkgroupLookupRes>();

            _serviceMock.GetStaffWorkgroupLookup().Returns(serviceResult);
            _mapperMock.Map<List<StaffWorkgroupLookupRes>>(serviceResult).Returns(mappedResult);

            var result = await _controller.GetStaffWorkgroupLookup();

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task GetStaffWorkgroupLookup_EdgeCase_EmptyList()
        {
            var serviceResult = new List<StaffWorkgroupLookupDto>();
            var mappedResult = new List<StaffWorkgroupLookupRes>();

            _serviceMock.GetStaffWorkgroupLookup().Returns(serviceResult);
            _mapperMock.Map<List<StaffWorkgroupLookupRes>>(serviceResult).Returns(mappedResult);

            var result = await _controller.GetStaffWorkgroupLookup();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetStaffWorkgroupLookup_Error_ServiceThrows()
        {
            _serviceMock.GetStaffWorkgroupLookup().Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetStaffWorkgroupLookup());
        }

        [Fact]
        public async Task GetStaffWorkgroupLookup_Error_MapperThrows()
        {
            var serviceResult = new List<StaffWorkgroupLookupDto>();
            _serviceMock.GetStaffWorkgroupLookup().Returns(serviceResult);
            _mapperMock.Map<List<StaffWorkgroupLookupRes>>(serviceResult).Throws(new Exception("Mapping error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetStaffWorkgroupLookup());
        }

        #endregion

        #region GetStaffChargeRate

        [Fact]
        public async Task GetStaffChargeRate_HappyPath_ReturnsOk()
        {
            _serviceMock.GetStaffChargeRate("S1", "J1").Returns(100m);

            var result = await _controller.GetStaffChargeRate("S1", "J1");

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(100m, ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task GetStaffChargeRate_EdgeCase_NullResult()
        {
            _serviceMock.GetStaffChargeRate("S1", "J1").Returns((decimal?)null);

            var result = await _controller.GetStaffChargeRate("S1", "J1");

            Assert.IsType<OkObjectResult>(result);
            Assert.Null(((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task GetStaffChargeRate_Error_ServiceThrows()
        {
            _serviceMock.GetStaffChargeRate("S1", "J1").Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetStaffChargeRate("S1", "J1"));
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_HappyPath_ReturnsOk()
        {
            var dto = new StaffJobDto { StaffId = "S1", JobCode = "J1" };
            var mapped = new StaffJobRes();

            _serviceMock.GetByIdAsync("S1", "J1").Returns(dto);
            _mapperMock.Map<StaffJobRes>(dto).Returns(mapped);

            var result = await _controller.GetByIdAsync("S1", "J1");

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task GetByIdAsync_EdgeCase_NullResult_ThrowsKeyNotFound()
        {
            _serviceMock.GetByIdAsync("S1", "J1").Returns((StaffJobDto)null!);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetByIdAsync("S1", "J1"));
        }

        [Fact]
        public async Task GetByIdAsync_Error_ServiceThrows()
        {
            _serviceMock.GetByIdAsync("S1", "J1").Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetByIdAsync("S1", "J1"));
        }

        [Fact]
        public async Task GetByIdAsync_Error_MapperThrows()
        {
            var dto = new StaffJobDto { StaffId = "S1", JobCode = "J1" };
            _serviceMock.GetByIdAsync("S1", "J1").Returns(dto);
            _mapperMock.Map<StaffJobRes>(dto).Throws(new Exception("Mapping error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetByIdAsync("S1", "J1"));
        }

        #endregion

        #region AddAsync

        [Fact]
        public async Task AddAsync_HappyPath_ReturnsCreated()
        {
            var req = new StaffJobReq { StaffId = "S1", JobCode = "J1" };
            var dto = new StaffJobDto { StaffId = "S1", JobCode = "J1" };
            var resultDto = new StaffJobDto { StaffId = "S1", JobCode = "J1" };
            var mapped = new StaffJobRes();

            _mapperMock.Map<StaffJobDto>(req).Returns(dto);
            _serviceMock.AddAsync(dto).Returns(resultDto);
            _mapperMock.Map<StaffJobRes>(resultDto).Returns(mapped);

            var result = await _controller.AddAsync(req);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(mapped, createdResult.Value);
        }

        [Fact]
        public async Task AddAsync_EdgeCase_MinimalInput()
        {
            var req = new StaffJobReq { StaffId = "", JobCode = "" };
            var dto = new StaffJobDto { StaffId = "", JobCode = "" };
            var resultDto = new StaffJobDto { StaffId = "", JobCode = "" };
            var mapped = new StaffJobRes();

            _mapperMock.Map<StaffJobDto>(req).Returns(dto);
            _serviceMock.AddAsync(dto).Returns(resultDto);
            _mapperMock.Map<StaffJobRes>(resultDto).Returns(mapped);

            var result = await _controller.AddAsync(req);

            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task AddAsync_Error_ServiceThrows()
        {
            var req = new StaffJobReq { StaffId = "S1", JobCode = "J1" };
            var dto = new StaffJobDto { StaffId = "S1", JobCode = "J1" };

            _mapperMock.Map<StaffJobDto>(req).Returns(dto);
            _serviceMock.AddAsync(dto).Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.AddAsync(req));
        }

        [Fact]
        public async Task AddAsync_Error_MapperThrows()
        {
            var req = new StaffJobReq { StaffId = "S1", JobCode = "J1" };
            _mapperMock.Map<StaffJobDto>(req).Throws(new Exception("Mapping error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.AddAsync(req));
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_HappyPath_ReturnsOk()
        {
            var req = new StaffJobReq { StaffId = "S1", JobCode = "J1" };
            var dto = new StaffJobDto { StaffId = "S1", JobCode = "J1" };
            var resultDto = new StaffJobDto { StaffId = "S1", JobCode = "J1" };
            var mapped = new StaffJobRes();

            _mapperMock.Map<StaffJobDto>(req).Returns(dto);
            _serviceMock.UpdateAsync(dto).Returns(resultDto);
            _mapperMock.Map<StaffJobRes>(resultDto).Returns(mapped);

            var result = await _controller.UpdateAsync(req);

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task UpdateAsync_EdgeCase_MinimalInput()
        {
            var req = new StaffJobReq { StaffId = "", JobCode = "" };
            var dto = new StaffJobDto { StaffId = "", JobCode = "" };
            var resultDto = new StaffJobDto { StaffId = "", JobCode = "" };
            var mapped = new StaffJobRes();

            _mapperMock.Map<StaffJobDto>(req).Returns(dto);
            _serviceMock.UpdateAsync(dto).Returns(resultDto);
            _mapperMock.Map<StaffJobRes>(resultDto).Returns(mapped);

            var result = await _controller.UpdateAsync(req);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateAsync_Error_ServiceThrows()
        {
            var req = new StaffJobReq { StaffId = "S1", JobCode = "J1" };
            var dto = new StaffJobDto { StaffId = "S1", JobCode = "J1" };

            _mapperMock.Map<StaffJobDto>(req).Returns(dto);
            _serviceMock.UpdateAsync(dto).Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.UpdateAsync(req));
        }

        [Fact]
        public async Task UpdateAsync_Error_MapperThrows()
        {
            var req = new StaffJobReq { StaffId = "S1", JobCode = "J1" };
            _mapperMock.Map<StaffJobDto>(req).Throws(new Exception("Mapping error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.UpdateAsync(req));
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_HappyPath_ReturnsNoContent()
        {
            _serviceMock.DeleteAsync("S1", "J1").Returns(true);

            var result = await _controller.DeleteAsync("S1", "J1");

            Assert.IsType<NoContentResult>(result);
        }
        

        [Fact]
        public async Task DeleteAsync_Error_ServiceThrows()
        {
            _serviceMock.DeleteAsync("S1", "J1").Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.DeleteAsync("S1", "J1"));
        }     

        #endregion
    }
}
