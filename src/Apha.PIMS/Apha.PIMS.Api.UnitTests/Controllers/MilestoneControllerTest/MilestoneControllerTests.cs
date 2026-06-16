using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Api.UnitTests.Controllers.MilestoneControllerTest
{
    public class MilestoneControllerTests
    {
        private readonly IMilestoneService _service;
        private readonly IMapper _mapper;
        private readonly MilestoneController _controller;

        public MilestoneControllerTests()
        {
            _service    = Substitute.For<IMilestoneService>();
            _mapper     = Substitute.For<IMapper>();
            _controller = new MilestoneController(_service, _mapper)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        #region GetAllMilestones

        [Fact]
        public async Task GetAllMilestones_ReturnsOkResult_WithMappedPaginatedResult()
        {
            // Arrange
            var parameters    = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string project = "PP001";

            var dtos = new List<MilestoneDto>
            {
                new() { Project = project, Number = "M1", DateDue = DateTime.Today.AddDays(10) },
                new() { Project = project, Number = "M2", DateDue = DateTime.Today.AddDays(20) }
            };
            var paginatedResult = new PaginatedResult<MilestoneDto>(dtos, new PaginationDto { TotalRecords = 2 });

            var resList = new List<MilestoneRes>
            {
                new() { Project = project, Number = "M1" },
                new() { Project = project, Number = "M2" }
            };
            var paginationRes = new PaginationRes<MilestoneRes>(resList, new Pagination { TotalRecords = 2 });

            _service.GetAllMilestonesAsync(parameters, project).Returns(paginatedResult);
            _mapper.Map<PaginationRes<MilestoneRes>>(paginatedResult).Returns(paginationRes);

            // Act
            var result = await _controller.GetAllMilestones(parameters, project);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paginationRes, okResult.Value);

            await _service.Received(1).GetAllMilestonesAsync(parameters, project);
            _mapper.Received(1).Map<PaginationRes<MilestoneRes>>(paginatedResult);
        }

        [Fact]
        public async Task GetAllMilestones_ReturnsOkResult_WithEmptyData()
        {
            // Arrange
            var parameters       = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string project = "PP001";

            var emptyResult  = new PaginatedResult<MilestoneDto>(new List<MilestoneDto>(), new PaginationDto());
            var emptyPageRes = new PaginationRes<MilestoneRes>();

            _service.GetAllMilestonesAsync(parameters, project).Returns(emptyResult);
            _mapper.Map<PaginationRes<MilestoneRes>>(emptyResult).Returns(emptyPageRes);

            // Act
            var result = await _controller.GetAllMilestones(parameters, project);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(emptyPageRes, okResult.Value);
        }

        [Fact]
        public async Task GetAllMilestones_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var parameters       = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string project = "PP001";

            _service.GetAllMilestonesAsync(parameters, project).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllMilestones(parameters, project));

            await _service.Received(1).GetAllMilestonesAsync(parameters, project);
            _mapper.DidNotReceive().Map<PaginationRes<MilestoneRes>>(Arg.Any<PaginatedResult<MilestoneDto>>());
        }

        #endregion

        #region GetMilestone

        [Fact]
        public async Task GetMilestone_ReturnsOkResult_WithMappedDto_WhenMilestoneExists()
        {
            // Arrange
            const string project = "PP001";
            const string number  = "M1";

            var dto = new MilestoneDto { Project = project, Number = number, DateDue = DateTime.Today.AddDays(10) };
            var res = new MilestoneRes { Project = project, Number = number };

            _service.GetMilestoneAsync(project, number).Returns(dto);
            _mapper.Map<MilestoneRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetMilestone(project, number);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, okResult.Value);

            await _service.Received(1).GetMilestoneAsync(project, number);
            _mapper.Received(1).Map<MilestoneRes>(dto);
        }

        [Fact]
        public async Task GetMilestone_ReturnsOkNullResult_WhenMilestoneNotFound()
        {
            // Arrange
            const string project = "PP001";
            const string number  = "UNKNOWN";

            _service.GetMilestoneAsync(project, number).Returns((MilestoneDto?)null);

            // Act
            var result = await _controller.GetMilestone(project, number);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);

            _mapper.DidNotReceive().Map<MilestoneRes>(Arg.Any<MilestoneDto>());
        }

        [Fact]
        public async Task GetMilestone_UrlDecodesNumber_BeforeCallingService()
        {
            // Arrange
            const string project       = "PP001";
            const string encodedNumber = "M%2F1";     // "M/1" URL-encoded
            const string decodedNumber = "M/1";

            var dto = new MilestoneDto { Project = project, Number = decodedNumber };
            var res = new MilestoneRes { Project = project, Number = decodedNumber };

            _service.GetMilestoneAsync(project, decodedNumber).Returns(dto);
            _mapper.Map<MilestoneRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetMilestone(project, encodedNumber);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _service.Received(1).GetMilestoneAsync(project, decodedNumber);
        }

        [Fact]
        public async Task GetMilestone_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            const string project = "PP001";
            const string number  = "M1";

            _service.GetMilestoneAsync(project, number).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetMilestone(project, number));

            await _service.Received(1).GetMilestoneAsync(project, number);
            _mapper.DidNotReceive().Map<MilestoneRes>(Arg.Any<MilestoneDto>());
        }

        #endregion

        #region SaveMilestone

        [Fact]
        public async Task SaveMilestone_ReturnsOkResult_WithMappedDto_AndSetsProject()
        {
            // Arrange
            const string project = "PP001";
            var request = new MilestoneReq
            {
                Number      = "M1",
                Description = "Test milestone",
                DateDue     = DateTime.Today.AddDays(30),
                IdType      = "D"
            };
            var dto = new MilestoneDto
            {
                Number      = "M1",
                Description = "Test milestone",
                DateDue     = DateTime.Today.AddDays(30),
                IdType      = "D"
            };
            var savedDto = new MilestoneDto { Project = project, Number = "M1" };
            var savedRes = new MilestoneRes { Project = project, Number = "M1" };

            _mapper.Map<MilestoneDto>(request).Returns(dto);
            _service.SaveMilestoneAsync(dto, Arg.Any<string?>()).Returns(savedDto);
            _mapper.Map<MilestoneRes>(savedDto).Returns(savedRes);

            // Act
            var result = await _controller.SaveMilestone(project, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(savedRes, okResult.Value);

            // Verify project was injected onto dto before service call
            Assert.Equal(project, dto.Project);

            _mapper.Received(1).Map<MilestoneDto>(request);
            await _service.Received(1).SaveMilestoneAsync(dto, Arg.Any<string?>());
            _mapper.Received(1).Map<MilestoneRes>(savedDto);
        }

        [Fact]
        public async Task SaveMilestone_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            const string project = "PP001";
            var request = new MilestoneReq { Number = "M1", DateDue = DateTime.Today.AddDays(30) };
            var dto     = new MilestoneDto { Number = "M1" };

            _mapper.Map<MilestoneDto>(request).Returns(dto);
            _service.SaveMilestoneAsync(dto, Arg.Any<string?>()).Throws(new Exception("Validation error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.SaveMilestone(project, request));

            _mapper.Received(1).Map<MilestoneDto>(request);
            await _service.Received(1).SaveMilestoneAsync(dto, Arg.Any<string?>());
            _mapper.DidNotReceive().Map<MilestoneRes>(Arg.Any<MilestoneDto>());
        }

        #endregion

        #region UpdateMilestone

        [Fact]
        public async Task UpdateMilestone_ReturnsOkResult_WithMappedDto_AndSetsProjectAndNumber()
        {
            // Arrange
            const string project = "PP001";
            const string number  = "M1";
            var request = new MilestoneReq
            {
                Description = "Updated milestone",
                DateDue     = DateTime.Today.AddDays(30),
                IdType      = "D"
            };
            var dto        = new MilestoneDto { Description = "Updated milestone" };
            var updatedDto = new MilestoneDto { Project = project, Number = number, Description = "Updated milestone" };
            var updatedRes = new MilestoneRes { Project = project, Number = number, Description = "Updated milestone" };

            _mapper.Map<MilestoneDto>(request).Returns(dto);
            _service.UpdateMilestoneAsync(dto, Arg.Any<string?>()).Returns(updatedDto);
            _mapper.Map<MilestoneRes>(updatedDto).Returns(updatedRes);

            // Act
            var result = await _controller.UpdateMilestone(project, number, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(updatedRes, okResult.Value);

            // Verify project and number were injected onto dto before service call
            Assert.Equal(project, dto.Project);
            Assert.Equal(number,  dto.Number);

            _mapper.Received(1).Map<MilestoneDto>(request);
            await _service.Received(1).UpdateMilestoneAsync(dto, Arg.Any<string?>());
            _mapper.Received(1).Map<MilestoneRes>(updatedDto);
        }

        [Fact]
        public async Task UpdateMilestone_UrlDecodesNumber_BeforeCallingService()
        {
            // Arrange
            const string project       = "PP001";
            const string encodedNumber = "M%2F1";
            const string decodedNumber = "M/1";

            var request    = new MilestoneReq { DateDue = DateTime.Today.AddDays(30) };
            var dto        = new MilestoneDto();
            var updatedDto = new MilestoneDto { Project = project, Number = decodedNumber };
            var updatedRes = new MilestoneRes { Project = project, Number = decodedNumber };

            _mapper.Map<MilestoneDto>(request).Returns(dto);
            _service.UpdateMilestoneAsync(dto, Arg.Any<string?>()).Returns(updatedDto);
            _mapper.Map<MilestoneRes>(updatedDto).Returns(updatedRes);

            // Act
            var result = await _controller.UpdateMilestone(project, encodedNumber, request);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(decodedNumber, dto.Number);
        }

        [Fact]
        public async Task UpdateMilestone_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            const string project = "PP001";
            const string number  = "M1";
            var request = new MilestoneReq { DateDue = DateTime.Today.AddDays(30) };
            var dto     = new MilestoneDto();

            _mapper.Map<MilestoneDto>(request).Returns(dto);
            _service.UpdateMilestoneAsync(dto, Arg.Any<string?>()).Throws(new Exception("Not found"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.UpdateMilestone(project, number, request));

            _mapper.Received(1).Map<MilestoneDto>(request);
            await _service.Received(1).UpdateMilestoneAsync(dto, Arg.Any<string?>());
            _mapper.DidNotReceive().Map<MilestoneRes>(Arg.Any<MilestoneDto>());
        }

        #endregion

        #region DeleteMilestone

        [Fact]
        public async Task DeleteMilestone_ReturnsOkWithSuccessTrue_WhenDeletedSuccessfully()
        {
            // Arrange
            const string project = "PP001";
            const string number  = "M1";

            _service.DeleteMilestoneAsync(project, number).Returns(true);

            // Act
            var result = await _controller.DeleteMilestone(project, number);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value    = Assert.IsType<object>(okResult.Value, exactMatch: false);
            Assert.NotNull(value);

            await _service.Received(1).DeleteMilestoneAsync(project, number);
        }

        [Fact]
        public async Task DeleteMilestone_ReturnsOkWithSuccessFalse_WhenNotFound()
        {
            // Arrange
            const string project = "PP001";
            const string number  = "UNKNOWN";

            _service.DeleteMilestoneAsync(project, number).Returns(false);

            // Act
            var result = await _controller.DeleteMilestone(project, number);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _service.Received(1).DeleteMilestoneAsync(project, number);
        }

        [Fact]
        public async Task DeleteMilestone_UrlDecodesNumber_BeforeCallingService()
        {
            // Arrange
            const string project       = "PP001";
            const string encodedNumber = "M%2F1";
            const string decodedNumber = "M/1";

            _service.DeleteMilestoneAsync(project, decodedNumber).Returns(true);

            // Act
            var result = await _controller.DeleteMilestone(project, encodedNumber);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _service.Received(1).DeleteMilestoneAsync(project, decodedNumber);
        }

        [Fact]
        public async Task DeleteMilestone_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            const string project = "PP001";
            const string number  = "M1";

            _service.DeleteMilestoneAsync(project, number).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.DeleteMilestone(project, number));

            await _service.Received(1).DeleteMilestoneAsync(project, number);
        }

        #endregion

        #region UpdateFormRequired

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task UpdateFormRequired_ReturnsOkWithSuccessResult(bool formRequired)
        {
            // Arrange
            const string parentProject = "PP001";

            _service.UpdateFormRequiredAsync(parentProject, formRequired).Returns(true);

            // Act
            var result = await _controller.UpdateFormRequired(parentProject, formRequired);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _service.Received(1).UpdateFormRequiredAsync(parentProject, formRequired);
        }

        [Fact]
        public async Task UpdateFormRequired_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            const string parentProject = "PP001";

            _service.UpdateFormRequiredAsync(parentProject, true).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.UpdateFormRequired(parentProject, true));

            await _service.Received(1).UpdateFormRequiredAsync(parentProject, true);
        }

        #endregion

        #region GetMilestoneTypes

        [Fact]
        public async Task GetMilestoneTypes_ReturnsOkResult_WithMappedList_WhenNoFilterProvided()
        {
            // Arrange
            var typeDtos = new List<MilestoneTypeDto>
            {
                new() { IdType = 'A', Type = "Alpha", MilestoneDeliverable = 'D' },
                new() { IdType = 'B', Type = "Beta",  MilestoneDeliverable = 'M' }
            };
            var typeResList = new List<MilestoneTypeRes>
            {
                new() { IdType = 'A', Type = "Alpha", MilestoneDeliverable = 'D' },
                new() { IdType = 'B', Type = "Beta",  MilestoneDeliverable = 'M' }
            };

            _service.GetMilestoneTypesAsync(null).Returns(typeDtos);
            _mapper.Map<List<MilestoneTypeRes>>(typeDtos).Returns(typeResList);

            // Act
            var result = await _controller.GetMilestoneTypes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(typeResList, okResult.Value);

            await _service.Received(1).GetMilestoneTypesAsync(null);
            _mapper.Received(1).Map<List<MilestoneTypeRes>>(typeDtos);
        }

        [Fact]
        public async Task GetMilestoneTypes_PassesFilterToService_WhenFilterProvided()
        {
            // Arrange
            const string filter = "M";
            var typeDtos    = new List<MilestoneTypeDto> { new() { IdType = 'B', Type = "Beta", MilestoneDeliverable = 'M' } };
            var typeResList = new List<MilestoneTypeRes> { new() { IdType = 'B', Type = "Beta", MilestoneDeliverable = 'M' } };

            _service.GetMilestoneTypesAsync(filter).Returns(typeDtos);
            _mapper.Map<List<MilestoneTypeRes>>(typeDtos).Returns(typeResList);

            // Act
            var result = await _controller.GetMilestoneTypes(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(typeResList, okResult.Value);

            await _service.Received(1).GetMilestoneTypesAsync(filter);
        }

        [Fact]
        public async Task GetMilestoneTypes_ReturnsOkResult_WithEmptyList()
        {
            // Arrange
            var emptyDtos    = new List<MilestoneTypeDto>();
            var emptyResList = new List<MilestoneTypeRes>();

            _service.GetMilestoneTypesAsync(null).Returns(emptyDtos);
            _mapper.Map<List<MilestoneTypeRes>>(emptyDtos).Returns(emptyResList);

            // Act
            var result = await _controller.GetMilestoneTypes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(emptyResList, okResult.Value);
        }

        [Fact]
        public async Task GetMilestoneTypes_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetMilestoneTypesAsync(null).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetMilestoneTypes());

            await _service.Received(1).GetMilestoneTypesAsync(null);
            _mapper.DidNotReceive().Map<List<MilestoneTypeRes>>(Arg.Any<List<MilestoneTypeDto>>());
        }

        #endregion

        #region GetAllMilestoneFormDates

        [Fact]
        public async Task GetAllMilestoneFormDates_ReturnsOkResult_WithMappedPaginatedResult()
        {
            // Arrange
            var parameters         = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string parent    = "PP001";

            var dtos = new List<MilestoneFormDatesDto>
            {
                new() { Year = 2024, ParentProject = parent },
                new() { Year = 2023, ParentProject = parent }
            };
            var paginatedResult = new PaginatedResult<MilestoneFormDatesDto>(dtos, new PaginationDto { TotalRecords = 2 });

            var resList      = new List<MilestoneFormDatesRes> { new() { Year = 2024, ParentProject = parent }, new() { Year = 2023, ParentProject = parent } };
            var paginationRes = new PaginationRes<MilestoneFormDatesRes>(resList, new Pagination { TotalRecords = 2 });

            _service.GetAllMilestoneFormDatesAsync(parameters, parent).Returns(paginatedResult);
            _mapper.Map<PaginationRes<MilestoneFormDatesRes>>(paginatedResult).Returns(paginationRes);

            // Act
            var result = await _controller.GetAllMilestoneFormDates(parameters, parent);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paginationRes, okResult.Value);

            await _service.Received(1).GetAllMilestoneFormDatesAsync(parameters, parent);
            _mapper.Received(1).Map<PaginationRes<MilestoneFormDatesRes>>(paginatedResult);
        }

        [Fact]
        public async Task GetAllMilestoneFormDates_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var parameters      = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string parent = "PP001";

            _service.GetAllMilestoneFormDatesAsync(parameters, parent).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllMilestoneFormDates(parameters, parent));

            await _service.Received(1).GetAllMilestoneFormDatesAsync(parameters, parent);
            _mapper.DidNotReceive().Map<PaginationRes<MilestoneFormDatesRes>>(Arg.Any<PaginatedResult<MilestoneFormDatesDto>>());
        }

        #endregion

        #region GetMilestoneFormDates

        [Fact]
        public async Task GetMilestoneFormDates_ReturnsOkResult_WithMappedDto_WhenExists()
        {
            // Arrange
            const string parent = "PP001";
            const short  year   = 2024;

            var dto = new MilestoneFormDatesDto { Year = year, ParentProject = parent, Jan = new DateTime(2024, 1, 31) };
            var res = new MilestoneFormDatesRes { Year = year, ParentProject = parent, Jan = new DateTime(2024, 1, 31) };

            _service.GetMilestoneFormDatesAsync(year, parent).Returns(dto);
            _mapper.Map<MilestoneFormDatesRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetMilestoneFormDates(parent, year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, okResult.Value);

            await _service.Received(1).GetMilestoneFormDatesAsync(year, parent);
            _mapper.Received(1).Map<MilestoneFormDatesRes>(dto);
        }

        [Fact]
        public async Task GetMilestoneFormDates_ReturnsNotFound_WhenNotFound()
        {
            // Arrange
            const string parent = "PP001";
            const short  year   = 2024;

            _service.GetMilestoneFormDatesAsync(year, parent).Returns((MilestoneFormDatesDto?)null);

            // Act
            var result = await _controller.GetMilestoneFormDates(parent, year);

            // Assert
            Assert.IsType<NotFoundResult>(result);

            _mapper.DidNotReceive().Map<MilestoneFormDatesRes>(Arg.Any<MilestoneFormDatesDto>());
        }

        [Fact]
        public async Task GetMilestoneFormDates_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            const string parent = "PP001";
            const short  year   = 2024;

            _service.GetMilestoneFormDatesAsync(year, parent).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetMilestoneFormDates(parent, year));

            await _service.Received(1).GetMilestoneFormDatesAsync(year, parent);
            _mapper.DidNotReceive().Map<MilestoneFormDatesRes>(Arg.Any<MilestoneFormDatesDto>());
        }

        #endregion

        #region SaveMilestoneFormDates

        [Fact]
        public async Task SaveMilestoneFormDates_ReturnsOkResult_WithMappedDto_AndSetsParentProject()
        {
            // Arrange
            const string parent = "PP001";
            var request = new MilestoneFormDatesReq
            {
                Year = 2024,
                Jan  = new DateTime(2024, 1, 31),
                Feb  = new DateTime(2024, 2, 28)
            };
            var dto = new MilestoneFormDatesDto
            {
                Year = 2024,
                Jan  = new DateTime(2024, 1, 31),
                Feb  = new DateTime(2024, 2, 28)
            };
            var savedDto = new MilestoneFormDatesDto { Year = 2024, ParentProject = parent };
            var savedRes = new MilestoneFormDatesRes { Year = 2024, ParentProject = parent };

            _mapper.Map<MilestoneFormDatesDto>(request).Returns(dto);
            _service.SaveMilestoneFormDatesAsync(dto).Returns(savedDto);
            _mapper.Map<MilestoneFormDatesRes>(savedDto).Returns(savedRes);

            // Act
            var result = await _controller.SaveMilestoneFormDates(parent, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(savedRes, okResult.Value);

            // Verify parentProject was injected onto dto before service call
            Assert.Equal(parent, dto.ParentProject);

            _mapper.Received(1).Map<MilestoneFormDatesDto>(request);
            await _service.Received(1).SaveMilestoneFormDatesAsync(dto);
            _mapper.Received(1).Map<MilestoneFormDatesRes>(savedDto);
        }

        [Fact]
        public async Task SaveMilestoneFormDates_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            const string parent = "PP001";
            var request = new MilestoneFormDatesReq { Year = 2024 };
            var dto     = new MilestoneFormDatesDto { Year = 2024 };

            _mapper.Map<MilestoneFormDatesDto>(request).Returns(dto);
            _service.SaveMilestoneFormDatesAsync(dto).Throws(new Exception("Validation error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.SaveMilestoneFormDates(parent, request));

            _mapper.Received(1).Map<MilestoneFormDatesDto>(request);
            await _service.Received(1).SaveMilestoneFormDatesAsync(dto);
            _mapper.DidNotReceive().Map<MilestoneFormDatesRes>(Arg.Any<MilestoneFormDatesDto>());
        }

        #endregion

        #region DeleteMilestoneFormDates

        [Fact]
        public async Task DeleteMilestoneFormDates_ReturnsOkWithSuccessTrue_WhenDeletedSuccessfully()
        {
            // Arrange
            const string parent = "PP001";
            const short  year   = 2024;

            _service.DeleteMilestoneFormDatesAsync(year, parent).Returns(true);

            // Act
            var result = await _controller.DeleteMilestoneFormDates(parent, year);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _service.Received(1).DeleteMilestoneFormDatesAsync(year, parent);
        }

        [Fact]
        public async Task DeleteMilestoneFormDates_ReturnsOkWithSuccessFalse_WhenNotFound()
        {
            // Arrange
            const string parent = "PP001";
            const short  year   = 9999;

            _service.DeleteMilestoneFormDatesAsync(year, parent).Returns(false);

            // Act
            var result = await _controller.DeleteMilestoneFormDates(parent, year);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _service.Received(1).DeleteMilestoneFormDatesAsync(year, parent);
        }

        [Fact]
        public async Task DeleteMilestoneFormDates_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            const string parent = "PP001";
            const short  year   = 2024;

            _service.DeleteMilestoneFormDatesAsync(year, parent).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.DeleteMilestoneFormDates(parent, year));

            await _service.Received(1).DeleteMilestoneFormDatesAsync(year, parent);
        }

        #endregion

        #region GetLogMilestones

        [Fact]
        public async Task GetLogMilestones_ReturnsOkResult_WithMappedPaginatedResult()
        {
            // Arrange
            var parameters       = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string project = "PP001";
            const string part1   = "M";
            const string part2   = "1";

            var dtos = new List<LogMilestoneDto>
            {
                new() { Project = project, Number = "M1", Description = "Log Entry 1" },
                new() { Project = project, Number = "M2", Description = "Log Entry 2" }
            };
            var paginatedResult = new PaginatedResult<LogMilestoneDto>(dtos, new PaginationDto { TotalRecords = 2 });

            var resList = new List<LogMilestoneRes>
            {
                new() { Project = project, Number = "M1", Description = "Log Entry 1" },
                new() { Project = project, Number = "M2", Description = "Log Entry 2" }
            };
            var paginationRes = new PaginationRes<LogMilestoneRes>(resList, new Pagination { TotalRecords = 2 });

            _service.GetLogMilestonesAsync(parameters, project, part1, part2).Returns(paginatedResult);
            _mapper.Map<PaginationRes<LogMilestoneRes>>(paginatedResult).Returns(paginationRes);

            // Act
            var result = await _controller.GetLogMilestones(parameters, project, part1, part2);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paginationRes, okResult.Value);

            await _service.Received(1).GetLogMilestonesAsync(parameters, project, part1, part2);
            _mapper.Received(1).Map<PaginationRes<LogMilestoneRes>>(paginatedResult);
        }

        [Fact]
        public async Task GetLogMilestones_ReturnsOkResult_WithEmptyData()
        {
            // Arrange
            var parameters  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var emptyResult = new PaginatedResult<LogMilestoneDto>(new List<LogMilestoneDto>(), new PaginationDto());
            var emptyRes    = new PaginationRes<LogMilestoneRes>();

            _service.GetLogMilestonesAsync(parameters, null, null, null).Returns(emptyResult);
            _mapper.Map<PaginationRes<LogMilestoneRes>>(emptyResult).Returns(emptyRes);

            // Act
            var result = await _controller.GetLogMilestones(parameters);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(emptyRes, okResult.Value);
        }

        [Fact]
        public async Task GetLogMilestones_WithNullOptionalParams_PassesNullsToService()
        {
            // Arrange
            var parameters  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var emptyResult = new PaginatedResult<LogMilestoneDto>(new List<LogMilestoneDto>(), new PaginationDto());
            var emptyRes    = new PaginationRes<LogMilestoneRes>();

            _service.GetLogMilestonesAsync(parameters, null, null, null).Returns(emptyResult);
            _mapper.Map<PaginationRes<LogMilestoneRes>>(emptyResult).Returns(emptyRes);

            // Act
            await _controller.GetLogMilestones(parameters, null, null, null);

            // Assert
            await _service.Received(1).GetLogMilestonesAsync(
                parameters,
                Arg.Is<string?>(p  => p  == null),
                Arg.Is<string?>(n1 => n1 == null),
                Arg.Is<string?>(n2 => n2 == null));
        }

        [Fact]
        public async Task GetLogMilestones_WithAllOptionalParams_PassesThemToService()
        {
            // Arrange
            var parameters       = new QueryParameters<string> { Page = 2, PageSize = 5 };
            const string project = "PP123";
            const string part1   = "M";
            const string part2   = "5";

            var paginatedResult = new PaginatedResult<LogMilestoneDto>(new List<LogMilestoneDto>(), new PaginationDto());
            var paginationRes   = new PaginationRes<LogMilestoneRes>();

            _service.GetLogMilestonesAsync(parameters, project, part1, part2).Returns(paginatedResult);
            _mapper.Map<PaginationRes<LogMilestoneRes>>(paginatedResult).Returns(paginationRes);

            // Act
            await _controller.GetLogMilestones(parameters, project, part1, part2);

            // Assert
            await _service.Received(1).GetLogMilestonesAsync(
                Arg.Is<QueryParameters<string>>(p => p.Page == 2 && p.PageSize == 5),
                Arg.Is<string?>(p  => p  == project),
                Arg.Is<string?>(n1 => n1 == part1),
                Arg.Is<string?>(n2 => n2 == part2));
        }

        [Fact]
        public async Task GetLogMilestones_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var parameters = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _service.GetLogMilestonesAsync(parameters, null, null, null)
                .Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetLogMilestones(parameters));

            await _service.Received(1).GetLogMilestonesAsync(parameters, null, null, null);
            _mapper.DidNotReceive().Map<PaginationRes<LogMilestoneRes>>(Arg.Any<PaginatedResult<LogMilestoneDto>>());
        }

        #endregion
    }
}

