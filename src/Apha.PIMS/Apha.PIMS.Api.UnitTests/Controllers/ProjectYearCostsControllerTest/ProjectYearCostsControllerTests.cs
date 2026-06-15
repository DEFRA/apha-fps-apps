using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Api.UnitTests.Controllers.ProjectYearCostsControllerTest
{
    public class ProjectYearCostsControllerTests
    {
        private readonly IProjectYearCostsService _service;
        private readonly IMapper _mapper;
        private readonly ProjectYearCostsController _controller;

        private const string Project = "PP001";
        private const short Year = 2024;

        public ProjectYearCostsControllerTests()
        {
            _service = Substitute.For<IProjectYearCostsService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new ProjectYearCostsController(_service, _mapper);
        }

        #region GetAdditionalActuals

        [Fact]
        public async Task GetAdditionalActuals_ReturnsOkResult_WithMappedPaginatedResult()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();
            var serviceResult = new PaginatedResult<AdditionalCostDto>();
            var mappedResult = new PaginationRes<AdditionalCostRes>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetAdditionalActualsAsync(Project, Year, paging).Returns(serviceResult);
            _mapper.Map<PaginationRes<AdditionalCostRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAdditionalActuals(Project, Year, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);

            _mapper.Received(1).Map<PaginationParameters<string>>(query);
            await _service.Received(1).GetAdditionalActualsAsync(Project, Year, paging);
            _mapper.Received(1).Map<PaginationRes<AdditionalCostRes>>(serviceResult);
        }

        [Fact]
        public async Task GetAdditionalActuals_WithEmptyResult_ReturnsOkWithEmptyPaginatedResult()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();
            var emptyResult = new PaginatedResult<AdditionalCostDto>();
            var emptyMapped = new PaginationRes<AdditionalCostRes>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetAdditionalActualsAsync(Project, Year, paging).Returns(emptyResult);
            _mapper.Map<PaginationRes<AdditionalCostRes>>(emptyResult).Returns(emptyMapped);

            // Act
            var result = await _controller.GetAdditionalActuals(Project, Year, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(emptyMapped, okResult.Value);

            await _service.Received(1).GetAdditionalActualsAsync(Project, Year, paging);
        }

        [Fact]
        public async Task GetAdditionalActuals_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetAdditionalActualsAsync(Project, Year, paging).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAdditionalActuals(Project, Year, query));

            await _service.Received(1).GetAdditionalActualsAsync(Project, Year, paging);
            _mapper.DidNotReceive().Map<PaginationRes<AdditionalCostRes>>(Arg.Any<PaginatedResult<AdditionalCostDto>>());
        }

        #endregion

        #region GetAdditionalPlans

        [Fact]
        public async Task GetAdditionalPlans_ReturnsOkResult_WithMappedPaginatedResult()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();
            var serviceResult = new PaginatedResult<AdditionalCostDto>();
            var mappedResult = new PaginationRes<AdditionalCostRes>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetAdditionalPlansAsync(Project, Year, paging).Returns(serviceResult);
            _mapper.Map<PaginationRes<AdditionalCostRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAdditionalPlans(Project, Year, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);

            _mapper.Received(1).Map<PaginationParameters<string>>(query);
            await _service.Received(1).GetAdditionalPlansAsync(Project, Year, paging);
            _mapper.Received(1).Map<PaginationRes<AdditionalCostRes>>(serviceResult);
        }

        [Fact]
        public async Task GetAdditionalPlans_WithEmptyResult_ReturnsOkWithEmptyPaginatedResult()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();
            var emptyResult = new PaginatedResult<AdditionalCostDto>();
            var emptyMapped = new PaginationRes<AdditionalCostRes>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetAdditionalPlansAsync(Project, Year, paging).Returns(emptyResult);
            _mapper.Map<PaginationRes<AdditionalCostRes>>(emptyResult).Returns(emptyMapped);

            // Act
            var result = await _controller.GetAdditionalPlans(Project, Year, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(emptyMapped, okResult.Value);

            await _service.Received(1).GetAdditionalPlansAsync(Project, Year, paging);
        }

        [Fact]
        public async Task GetAdditionalPlans_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetAdditionalPlansAsync(Project, Year, paging).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAdditionalPlans(Project, Year, query));

            await _service.Received(1).GetAdditionalPlansAsync(Project, Year, paging);
            _mapper.DidNotReceive().Map<PaginationRes<AdditionalCostRes>>(Arg.Any<PaginatedResult<AdditionalCostDto>>());
        }

        #endregion

        #region GetAnimalActuals

        [Fact]
        public async Task GetAnimalActuals_ReturnsOkResult_WithMappedPaginatedResult()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();
            var serviceResult = new PaginatedResult<AnimalCostDto>();
            var mappedResult = new PaginationRes<AnimalCostRes>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetAnimalActualsAsync(Project, Year, paging).Returns(serviceResult);
            _mapper.Map<PaginationRes<AnimalCostRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAnimalActuals(Project, Year, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);

            _mapper.Received(1).Map<PaginationParameters<string>>(query);
            await _service.Received(1).GetAnimalActualsAsync(Project, Year, paging);
            _mapper.Received(1).Map<PaginationRes<AnimalCostRes>>(serviceResult);
        }

        [Fact]
        public async Task GetAnimalActuals_WithEmptyResult_ReturnsOkWithEmptyPaginatedResult()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();
            var emptyResult = new PaginatedResult<AnimalCostDto>();
            var emptyMapped = new PaginationRes<AnimalCostRes>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetAnimalActualsAsync(Project, Year, paging).Returns(emptyResult);
            _mapper.Map<PaginationRes<AnimalCostRes>>(emptyResult).Returns(emptyMapped);

            // Act
            var result = await _controller.GetAnimalActuals(Project, Year, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(emptyMapped, okResult.Value);

            await _service.Received(1).GetAnimalActualsAsync(Project, Year, paging);
        }

        [Fact]
        public async Task GetAnimalActuals_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetAnimalActualsAsync(Project, Year, paging).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAnimalActuals(Project, Year, query));

            await _service.Received(1).GetAnimalActualsAsync(Project, Year, paging);
            _mapper.DidNotReceive().Map<PaginationRes<AnimalCostRes>>(Arg.Any<PaginatedResult<AnimalCostDto>>());
        }

        #endregion

        #region GetAnimalPlans

        [Fact]
        public async Task GetAnimalPlans_ReturnsOkResult_WithMappedPaginatedResult()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();
            var serviceResult = new PaginatedResult<AnimalCostDto>();
            var mappedResult = new PaginationRes<AnimalCostRes>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetAnimalPlansAsync(Project, Year, paging).Returns(serviceResult);
            _mapper.Map<PaginationRes<AnimalCostRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAnimalPlans(Project, Year, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);

            _mapper.Received(1).Map<PaginationParameters<string>>(query);
            await _service.Received(1).GetAnimalPlansAsync(Project, Year, paging);
            _mapper.Received(1).Map<PaginationRes<AnimalCostRes>>(serviceResult);
        }

        [Fact]
        public async Task GetAnimalPlans_WithEmptyResult_ReturnsOkWithEmptyPaginatedResult()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();
            var emptyResult = new PaginatedResult<AnimalCostDto>();
            var emptyMapped = new PaginationRes<AnimalCostRes>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetAnimalPlansAsync(Project, Year, paging).Returns(emptyResult);
            _mapper.Map<PaginationRes<AnimalCostRes>>(emptyResult).Returns(emptyMapped);

            // Act
            var result = await _controller.GetAnimalPlans(Project, Year, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(emptyMapped, okResult.Value);

            await _service.Received(1).GetAnimalPlansAsync(Project, Year, paging);
        }

        [Fact]
        public async Task GetAnimalPlans_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetAnimalPlansAsync(Project, Year, paging).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAnimalPlans(Project, Year, query));

            await _service.Received(1).GetAnimalPlansAsync(Project, Year, paging);
            _mapper.DidNotReceive().Map<PaginationRes<AnimalCostRes>>(Arg.Any<PaginatedResult<AnimalCostDto>>());
        }

        #endregion

        #region GetTestPlans

        [Fact]
        public async Task GetTestPlans_ReturnsOkResult_WithMappedPaginatedResult()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();
            var serviceResult = new PaginatedResult<TestCostDto>();
            var mappedResult = new PaginationRes<TestCostRes>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetTestPlansAsync(Project, Year, paging).Returns(serviceResult);
            _mapper.Map<PaginationRes<TestCostRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetTestPlans(Project, Year, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);

            _mapper.Received(1).Map<PaginationParameters<string>>(query);
            await _service.Received(1).GetTestPlansAsync(Project, Year, paging);
            _mapper.Received(1).Map<PaginationRes<TestCostRes>>(serviceResult);
        }

        [Fact]
        public async Task GetTestPlans_WithEmptyResult_ReturnsOkWithEmptyPaginatedResult()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();
            var emptyResult = new PaginatedResult<TestCostDto>();
            var emptyMapped = new PaginationRes<TestCostRes>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetTestPlansAsync(Project, Year, paging).Returns(emptyResult);
            _mapper.Map<PaginationRes<TestCostRes>>(emptyResult).Returns(emptyMapped);

            // Act
            var result = await _controller.GetTestPlans(Project, Year, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(emptyMapped, okResult.Value);

            await _service.Received(1).GetTestPlansAsync(Project, Year, paging);
        }

        [Fact]
        public async Task GetTestPlans_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetTestPlansAsync(Project, Year, paging).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetTestPlans(Project, Year, query));

            await _service.Received(1).GetTestPlansAsync(Project, Year, paging);
            _mapper.DidNotReceive().Map<PaginationRes<TestCostRes>>(Arg.Any<PaginatedResult<TestCostDto>>());
        }

        #endregion

        #region GetTestActuals

        [Fact]
        public async Task GetTestActuals_ReturnsOkResult_WithMappedPaginatedResult()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();
            var serviceResult = new PaginatedResult<TestCostDto>();
            var mappedResult = new PaginationRes<TestCostRes>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetTestActualsAsync(Project, Year, paging).Returns(serviceResult);
            _mapper.Map<PaginationRes<TestCostRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetTestActuals(Project, Year, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);

            _mapper.Received(1).Map<PaginationParameters<string>>(query);
            await _service.Received(1).GetTestActualsAsync(Project, Year, paging);
            _mapper.Received(1).Map<PaginationRes<TestCostRes>>(serviceResult);
        }

        [Fact]
        public async Task GetTestActuals_WithEmptyResult_ReturnsOkWithEmptyPaginatedResult()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();
            var emptyResult = new PaginatedResult<TestCostDto>();
            var emptyMapped = new PaginationRes<TestCostRes>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetTestActualsAsync(Project, Year, paging).Returns(emptyResult);
            _mapper.Map<PaginationRes<TestCostRes>>(emptyResult).Returns(emptyMapped);

            // Act
            var result = await _controller.GetTestActuals(Project, Year, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(emptyMapped, okResult.Value);

            await _service.Received(1).GetTestActualsAsync(Project, Year, paging);
        }

        [Fact]
        public async Task GetTestActuals_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetTestActualsAsync(Project, Year, paging).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetTestActuals(Project, Year, query));

            await _service.Received(1).GetTestActualsAsync(Project, Year, paging);
            _mapper.DidNotReceive().Map<PaginationRes<TestCostRes>>(Arg.Any<PaginatedResult<TestCostDto>>());
        }

        #endregion

        #region GetStaffPlans

        [Fact]
        public async Task GetStaffPlans_ReturnsOkResult_WithMappedPaginatedResult()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();
            var serviceResult = new PaginatedResult<StaffCostDto>();
            var mappedResult = new PaginationRes<StaffCostRes>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetStaffPlansAsync(Project, Year, paging).Returns(serviceResult);
            _mapper.Map<PaginationRes<StaffCostRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetStaffPlans(Project, Year, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);

            _mapper.Received(1).Map<PaginationParameters<string>>(query);
            await _service.Received(1).GetStaffPlansAsync(Project, Year, paging);
            _mapper.Received(1).Map<PaginationRes<StaffCostRes>>(serviceResult);
        }

        [Fact]
        public async Task GetStaffPlans_WithEmptyResult_ReturnsOkWithEmptyPaginatedResult()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();
            var emptyResult = new PaginatedResult<StaffCostDto>();
            var emptyMapped = new PaginationRes<StaffCostRes>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetStaffPlansAsync(Project, Year, paging).Returns(emptyResult);
            _mapper.Map<PaginationRes<StaffCostRes>>(emptyResult).Returns(emptyMapped);

            // Act
            var result = await _controller.GetStaffPlans(Project, Year, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(emptyMapped, okResult.Value);

            await _service.Received(1).GetStaffPlansAsync(Project, Year, paging);
        }

        [Fact]
        public async Task GetStaffPlans_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetStaffPlansAsync(Project, Year, paging).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetStaffPlans(Project, Year, query));

            await _service.Received(1).GetStaffPlansAsync(Project, Year, paging);
            _mapper.DidNotReceive().Map<PaginationRes<StaffCostRes>>(Arg.Any<PaginatedResult<StaffCostDto>>());
        }

        #endregion

        #region GetStaffActuals

        [Fact]
        public async Task GetStaffActuals_ReturnsOkResult_WithMappedPaginatedResult()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();
            var serviceResult = new PaginatedResult<StaffCostDto>();
            var mappedResult = new PaginationRes<StaffCostRes>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetStaffActualsAsync(Project, Year, paging).Returns(serviceResult);
            _mapper.Map<PaginationRes<StaffCostRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetStaffActuals(Project, Year, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);

            _mapper.Received(1).Map<PaginationParameters<string>>(query);
            await _service.Received(1).GetStaffActualsAsync(Project, Year, paging);
            _mapper.Received(1).Map<PaginationRes<StaffCostRes>>(serviceResult);
        }

        [Fact]
        public async Task GetStaffActuals_WithEmptyResult_ReturnsOkWithEmptyPaginatedResult()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();
            var emptyResult = new PaginatedResult<StaffCostDto>();
            var emptyMapped = new PaginationRes<StaffCostRes>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetStaffActualsAsync(Project, Year, paging).Returns(emptyResult);
            _mapper.Map<PaginationRes<StaffCostRes>>(emptyResult).Returns(emptyMapped);

            // Act
            var result = await _controller.GetStaffActuals(Project, Year, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(emptyMapped, okResult.Value);

            await _service.Received(1).GetStaffActualsAsync(Project, Year, paging);
        }

        [Fact]
        public async Task GetStaffActuals_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetStaffActualsAsync(Project, Year, paging).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetStaffActuals(Project, Year, query));

            await _service.Received(1).GetStaffActualsAsync(Project, Year, paging);
            _mapper.DidNotReceive().Map<PaginationRes<StaffCostRes>>(Arg.Any<PaginatedResult<StaffCostDto>>());
        }

        #endregion

        #region GetProjectYearDetails

        [Fact]
        public async Task GetProjectYearDetails_ReturnsOkResult_WithMappedDetails()
        {
            // Arrange
            var detailsDto = new ProjectYearDetailsDto
            {
                Year = Year,
                Parentproject = Project,
                Manager = "MGR1",
                Disease = "FMD",
                Contract = "CON001"
            };
            var detailsRes = new ProjectYearDetailsRes
            {
                Year = Year,
                Parentproject = Project,
                Manager = "MGR1",
                Disease = "FMD",
                Contract = "CON001"
            };

            _service.GetProjectYearDetailsAsync(Project, Year).Returns(detailsDto);
            _mapper.Map<ProjectYearDetailsRes>(detailsDto).Returns(detailsRes);

            // Act
            var result = await _controller.GetProjectYearDetails(Project, Year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(detailsRes, okResult.Value);

            await _service.Received(1).GetProjectYearDetailsAsync(Project, Year);
            _mapper.Received(1).Map<ProjectYearDetailsRes>(detailsDto);
        }

        [Fact]
        public async Task GetProjectYearDetails_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetProjectYearDetailsAsync(Project, Year).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetProjectYearDetails(Project, Year));

            await _service.Received(1).GetProjectYearDetailsAsync(Project, Year);
            _mapper.DidNotReceive().Map<ProjectYearDetailsRes>(Arg.Any<ProjectYearDetailsDto>());
        }

        #endregion

        #region GetPactPay

        [Fact]
        public async Task GetPactPay_ReturnsOkResult_WithMappedPaginatedResult()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();
            var serviceResult = new PaginatedResult<PactPayDto>();
            var mappedResult = new PaginationRes<PactPayRes>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetPactPayAsync(Project, Year, paging).Returns(serviceResult);
            _mapper.Map<PaginationRes<PactPayRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetPactPay(Project, Year, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);

            _mapper.Received(1).Map<PaginationParameters<string>>(query);
            await _service.Received(1).GetPactPayAsync(Project, Year, paging);
            _mapper.Received(1).Map<PaginationRes<PactPayRes>>(serviceResult);
        }

        [Fact]
        public async Task GetPactPay_WithEmptyResult_ReturnsOkWithEmptyPaginatedResult()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();
            var emptyResult = new PaginatedResult<PactPayDto>();
            var emptyMapped = new PaginationRes<PactPayRes>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetPactPayAsync(Project, Year, paging).Returns(emptyResult);
            _mapper.Map<PaginationRes<PactPayRes>>(emptyResult).Returns(emptyMapped);

            // Act
            var result = await _controller.GetPactPay(Project, Year, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(emptyMapped, okResult.Value);

            await _service.Received(1).GetPactPayAsync(Project, Year, paging);
        }

        [Fact]
        public async Task GetPactPay_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetPactPayAsync(Project, Year, paging).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPactPay(Project, Year, query));

            await _service.Received(1).GetPactPayAsync(Project, Year, paging);
            _mapper.DidNotReceive().Map<PaginationRes<PactPayRes>>(Arg.Any<PaginatedResult<PactPayDto>>());
        }

        #endregion

        #region GetMonthlyPactData

        [Fact]
        public async Task GetMonthlyPactData_ReturnsOkResult_WithMappedPaginatedResult()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();
            var serviceResult = new PaginatedResult<MonthlyPactDto>();
            var mappedResult = new PaginationRes<MonthlyPactRes>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetMonthlyPactDataAsync(Project, Year, paging).Returns(serviceResult);
            _mapper.Map<PaginationRes<MonthlyPactRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetMonthlyPactData(Project, Year, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);

            _mapper.Received(1).Map<PaginationParameters<string>>(query);
            await _service.Received(1).GetMonthlyPactDataAsync(Project, Year, paging);
            _mapper.Received(1).Map<PaginationRes<MonthlyPactRes>>(serviceResult);
        }

        [Fact]
        public async Task GetMonthlyPactData_WithEmptyResult_ReturnsOkWithEmptyPaginatedResult()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();
            var emptyResult = new PaginatedResult<MonthlyPactDto>();
            var emptyMapped = new PaginationRes<MonthlyPactRes>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetMonthlyPactDataAsync(Project, Year, paging).Returns(emptyResult);
            _mapper.Map<PaginationRes<MonthlyPactRes>>(emptyResult).Returns(emptyMapped);

            // Act
            var result = await _controller.GetMonthlyPactData(Project, Year, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(emptyMapped, okResult.Value);

            await _service.Received(1).GetMonthlyPactDataAsync(Project, Year, paging);
        }

        [Fact]
        public async Task GetMonthlyPactData_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var paging = new PaginationParameters<string>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(paging);
            _service.GetMonthlyPactDataAsync(Project, Year, paging).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetMonthlyPactData(Project, Year, query));

            await _service.Received(1).GetMonthlyPactDataAsync(Project, Year, paging);
            _mapper.DidNotReceive().Map<PaginationRes<MonthlyPactRes>>(Arg.Any<PaginatedResult<MonthlyPactDto>>());
        }

        #endregion

        #region GetFpsYearTotals

        [Fact]
        public async Task GetFpsYearTotals_WhenResultExists_ReturnsOkResult_WithMappedTotals()
        {
            // Arrange
            var totalsDto = new FpsYearTotalsDto
            {
                Year = Year,
                Parentproject = Project,
                Totaladditionalcosts = 1000m,
                Totalanimalcosts = 2000d,
                Totalstaffcosts = 3000d,
                Totaltestcosts = 4000d,
                Totalcosts = 10000d,
                Custincome = 5000m,
                Transferincome = 2000m,
                Totalincome = 7000m
            };
            var totalsRes = new FpsYearTotalsRes
            {
                Year = Year,
                Parentproject = Project,
                Totaladditionalcosts = 1000m,
                Totalcosts = 10000d,
                Totalincome = 7000m
            };

            _service.GetFpsYearTotalsAsync(Project, Year).Returns(totalsDto);
            _mapper.Map<FpsYearTotalsRes>(totalsDto).Returns(totalsRes);

            // Act
            var result = await _controller.GetFpsYearTotals(Project, Year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(totalsRes, okResult.Value);

            await _service.Received(1).GetFpsYearTotalsAsync(Project, Year);
            _mapper.Received(1).Map<FpsYearTotalsRes>(totalsDto);
        }

        [Fact]
        public async Task GetFpsYearTotals_WhenServiceReturnsNull_ReturnsNotFound()
        {
            // Arrange
            _service.GetFpsYearTotalsAsync(Project, Year).Returns((FpsYearTotalsDto?)null);

            // Act
            var result = await _controller.GetFpsYearTotals(Project, Year);

            // Assert
            Assert.IsType<NotFoundResult>(result);

            await _service.Received(1).GetFpsYearTotalsAsync(Project, Year);
            _mapper.DidNotReceive().Map<FpsYearTotalsRes>(Arg.Any<FpsYearTotalsDto>());
        }

        [Fact]
        public async Task GetFpsYearTotals_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetFpsYearTotalsAsync(Project, Year).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetFpsYearTotals(Project, Year));

            await _service.Received(1).GetFpsYearTotalsAsync(Project, Year);
            _mapper.DidNotReceive().Map<FpsYearTotalsRes>(Arg.Any<FpsYearTotalsDto>());
        }

        #endregion

        #region ExportToExcel

        [Fact]
        public async Task ExportToExcel_ReturnsFileContentResult_WithCorrectContentTypeAndFileName()
        {
            // Arrange
            var excelBytes = new byte[] { 0x50, 0x4B, 0x03, 0x04 };
            _service.ExportProjectYearCostsToExcelAsync(Project, Year).Returns(excelBytes);

            // Act
            var result = await _controller.ExportToExcel(Project, Year);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal(excelBytes, fileResult.FileContents);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
            Assert.Equal($"ProjectYearCosts_{Project}_{Year}.xlsx", fileResult.FileDownloadName);

            await _service.Received(1).ExportProjectYearCostsToExcelAsync(Project, Year);
        }

        [Fact]
        public async Task ExportToExcel_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.ExportProjectYearCostsToExcelAsync(Project, Year).Throws(new Exception("Export failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.ExportToExcel(Project, Year));

            await _service.Received(1).ExportProjectYearCostsToExcelAsync(Project, Year);
        }

        #endregion
    }
}