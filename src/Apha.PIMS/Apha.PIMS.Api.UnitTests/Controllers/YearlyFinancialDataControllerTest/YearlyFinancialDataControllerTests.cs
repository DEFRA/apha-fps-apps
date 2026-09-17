using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Apha.PIMS.Api.UnitTests.Controllers.YearlyFinancialDataControllerTest
{
    public class YearlyFinancialDataControllerTests
    {
        private readonly IYearlyFinancialDataService _service;
        private readonly IMapper _mapper;
        private readonly YearlyFinancialDataController _controller;

        public YearlyFinancialDataControllerTests()
        {
            _service = Substitute.For<IYearlyFinancialDataService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new YearlyFinancialDataController(_service, _mapper);
        }

        #region GetAll

        [Fact]
        public async Task GetAll_WhenServiceReturnsData_ReturnsOkWithPaginatedResult()
        {
            // Arrange
            var project = "PRJ001";
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var queryParams = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var yearlyDatas = new List<YearlyFinancialDataDto>
            {
                new YearlyFinancialDataDto { Year = 2024, Project = project, BfBudget = 1000m },
                new YearlyFinancialDataDto { Year = 2025, Project = project, BfBudget = 1200m }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var paginatedResult = new PaginatedResult<YearlyFinancialDataDto>
            {
                Data = yearlyDatas,
                PaginationData = paginationDto
            };
            var paginationRes = new PaginationRes<YearlyFinancialDataRes>
            {
                Data = new List<YearlyFinancialDataRes>
                {
                    new YearlyFinancialDataRes { Year = 2024, Project = project, BfBudget = 1000m },
                    new YearlyFinancialDataRes { Year = 2025, Project = project, BfBudget = 1200m }
                },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 }
            };

            _mapper.Map<QueryParameters<string>>(query).Returns(queryParams);
            _service.GetAllAsync(project, queryParams).Returns(paginatedResult);
            _mapper.Map<PaginationRes<YearlyFinancialDataRes>>(paginatedResult).Returns(paginationRes);

            // Act
            var result = await _controller.GetAll(project, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            await _service.Received(1).GetAllAsync(project, queryParams);
        }

        #endregion

        #region GetByKey

        [Fact]
        public async Task GetByKey_WhenRecordExists_ReturnsOkWithData()
        {
            // Arrange
            var year = 2024;
            var project = "PRJ001";
            var yearlyData = new YearlyFinancialDataDto { Year = (short)year, Project = project, BfBudget = 1000m };
            var yearlyRes = new YearlyFinancialDataRes { Year = (short)year, Project = project, BfBudget = 1000m };

            _service.GetByKeyAsync((short)year, project).Returns(yearlyData);
            _mapper.Map<YearlyFinancialDataRes>(yearlyData).Returns(yearlyRes);

            // Act
            var result = await _controller.GetByKey(year, project);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            var returnedData = Assert.IsType<YearlyFinancialDataRes>(okResult.Value);
            Assert.Equal((short)year, returnedData.Year);

            await _service.Received(1).GetByKeyAsync((short)year, project);
            _mapper.Received(1).Map<YearlyFinancialDataRes>(yearlyData);
        }

        [Fact]
        public async Task GetByKey_WhenRecordDoesNotExist_ReturnsJsonSuccessResponseWithNullData()
        {
            // Arrange
            var year = 2024;
            var project = "PRJ999";
            _service.GetByKeyAsync((short)year, project).Returns((YearlyFinancialDataDto?)null);

            // Act
            var result = await _controller.GetByKey(year, project);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var apiResponse = Assert.IsType<Apha.Common.Contracts.ApiResponse<YearlyFinancialDataRes>>(jsonResult.Value);

            Assert.True(apiResponse.Success);
            Assert.Null(apiResponse.Data);
            Assert.NotNull(apiResponse.Meta);
            Assert.NotNull(apiResponse.Meta.CorrelationId);

            await _service.Received(1).GetByKeyAsync((short)year, project);
            _mapper.DidNotReceive().Map<YearlyFinancialDataRes>(Arg.Any<YearlyFinancialDataDto>());
        }

        #endregion

        #region Create

        [Fact]
        public async Task Create_WhenValidRequest_ReturnsCreatedAtActionWithData()
        {
            // Arrange
            var request = new YearlyFinancialDataReq { BfBudget = 1000m };
            var dto = new YearlyFinancialDataDto { Year = 2024, Project = "PRJ001", BfBudget = 1000m };
            var res = new YearlyFinancialDataRes { Year = 2024, Project = "PRJ001", BfBudget = 1000m };

            _mapper.Map<YearlyFinancialDataDto>(request).Returns(dto);
            _service.CreateAsync(Arg.Any<YearlyFinancialDataDto>()).Returns(dto);
            _mapper.Map<YearlyFinancialDataRes>(dto).Returns(res);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(YearlyFinancialDataController.GetByKey), createdResult.ActionName);
            var returnedData = Assert.IsType<YearlyFinancialDataRes>(createdResult.Value);
            Assert.Equal((short)2024, returnedData.Year);

            await _service.Received(1).CreateAsync(Arg.Any<YearlyFinancialDataDto>());
        }

        #endregion

        #region Update

        [Fact]
        public async Task Update_WhenValidRequest_ReturnsOkWithUpdatedData()
        {
            // Arrange
            var year = 2024;
            var project = "PRJ001";
            var request = new YearlyFinancialDataReq { BfBudget = 1500m };
            var dto = new YearlyFinancialDataDto { Year = (short)year, Project = project, BfBudget = 1500m };
            var res = new YearlyFinancialDataRes { Year = (short)year, Project = project, BfBudget = 1500m };

            _mapper.Map<YearlyFinancialDataDto>(request).Returns(dto);
            _service.UpdateAsync(Arg.Any<YearlyFinancialDataDto>()).Returns(dto);
            _mapper.Map<YearlyFinancialDataRes>(dto).Returns(res);

            // Act
            var result = await _controller.Update(year, project, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            var returnedData = Assert.IsType<YearlyFinancialDataRes>(okResult.Value);
            Assert.Equal((short)year, returnedData.Year);
            Assert.Equal(1500m, returnedData.BfBudget);

            await _service.Received(1).UpdateAsync(Arg.Is<YearlyFinancialDataDto>(d => d.Year == (short)year && d.Project == project));
        }

        #endregion

        #region Delete

        [Fact]
        public async Task Delete_WhenRecordExists_ReturnsOkWithSuccess()
        {
            // Arrange
            var year = 2024;
            var project = "PRJ001";
            _service.DeleteAsync((short)year, project).Returns(true);

            // Act
            var result = await _controller.Delete(year, project);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedValue = okResult.Value;
            var successProperty = returnedValue?.GetType().GetProperty("success");
            Assert.NotNull(successProperty);
            Assert.True((bool?)successProperty?.GetValue(returnedValue) ?? false);

            await _service.Received(1).DeleteAsync((short)year, project);
        }

        [Fact]
        public async Task Delete_WhenRecordNotFound_ReturnsOkWithFailure()
        {
            // Arrange
            var year = 2024;
            var project = "PRJ999";
            _service.DeleteAsync((short)year, project).Returns(false);

            // Act
            var result = await _controller.Delete(year, project);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedValue = okResult.Value;
            var successProperty = returnedValue?.GetType().GetProperty("success");
            Assert.NotNull(successProperty);
            Assert.False((bool?)successProperty?.GetValue(returnedValue) ?? true);
        }

        #endregion

        #region GetPactCosts

        [Fact]
        public async Task GetPactCosts_WhenDataExists_ReturnsOkWithCosts()
        {
            // Arrange
            var project = "PRJ001";
            var year = 2024;
            var costsList = new List<PactProjectYearCostsDto>
            {
                new PactProjectYearCostsDto { Project = project, Year = (short)year, Pay = 1000m, Tests = 500m }
            };
            var resList = new List<PactProjectYearCostsRes>
            {
                new PactProjectYearCostsRes { Project = project, Year = (short)year, Pay = 1000m, Tests = 500m }
            };

            _service.GetPactCostsAsync(project, (short)year).Returns((IReadOnlyList<PactProjectYearCostsDto>)costsList);
            _mapper.Map<IReadOnlyList<PactProjectYearCostsRes>>(costsList).Returns(resList.AsReadOnly());

            // Act
            var result = await _controller.GetPactCosts(project, year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedData = Assert.IsAssignableFrom<IReadOnlyList<PactProjectYearCostsRes>>(okResult.Value);
            Assert.Single(returnedData);

            await _service.Received(1).GetPactCostsAsync(project, (short)year);
        }

        #endregion

        #region GetSettingValueById

        [Fact]
        public async Task GetSettingValueById_WhenSettingExists_ReturnsOkWithValue()
        {
            // Arrange
            var id = "SettingId";
            var value = "SettingValue";
            _service.GetSettingValueByIdAsync(id).Returns(value);

            // Act
            var result = await _controller.GetSettingValueById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(value, okResult.Value);

            await _service.Received(1).GetSettingValueByIdAsync(id);
        }

        [Fact]
        public async Task GetSettingValueById_WhenSettingNotFound_ReturnsEmptyString()
        {
            // Arrange
            var id = "NonExistentId";
            _service.GetSettingValueByIdAsync(id).Returns((string?)null);

            // Act
            var result = await _controller.GetSettingValueById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(string.Empty, okResult.Value);
        }

        [Fact]
        public async Task GetSettingValueById_WhenIdIsNull_UsesEmptyString()
        {
            // Arrange
            var value = "SettingValue";
            _service.GetSettingValueByIdAsync(string.Empty).Returns(value);

            // Act
            var result = await _controller.GetSettingValueById(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(value, okResult.Value);

            await _service.Received(1).GetSettingValueByIdAsync(string.Empty);
        }

        #endregion
    }
}
