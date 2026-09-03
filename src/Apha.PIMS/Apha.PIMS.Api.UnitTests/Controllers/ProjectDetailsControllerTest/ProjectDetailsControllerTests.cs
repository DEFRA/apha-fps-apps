using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Api.UnitTests.Controllers.ProjectDetailsControllerTest
{
    public class ProjectDetailsControllerTests
    {
        private readonly IProjectDetailsService _service;
        private readonly IMapper _mapper;
        private readonly ProjectDetailsController _controller;

        public ProjectDetailsControllerTests()
        {
            _service = Substitute.For<IProjectDetailsService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new ProjectDetailsController(_service, _mapper);
        }

        #region GetPimsDetail

        [Fact]
        public async Task GetPimsDetail_WithValidProject_ReturnsOkResult_WithMappedDetail()
        {
            // Arrange
            var parentproject = "PP001";
            var detailDto = new ProjectDetailDto
            {
                Parentproject = parentproject,
                Version = "1.0",
                FileRef = "FR001",
                CustomerRef = "CR001",
                CostbookNumber = "CB001",
                Riskid = 1,
                UseProjectYears = true
            };
            var detailRes = new ProjectDetailRes
            {
                Parentproject = parentproject,
                Version = "1.0",
                FileRef = "FR001",
                CustomerRef = "CR001",
                CostbookNumber = "CB001",
                Riskid = 1,
                UseProjectYears = true
            };

            _service.GetPimsDetailAsync(parentproject).Returns(detailDto);
            _mapper.Map<ProjectDetailRes>(detailDto).Returns(detailRes);

            // Act
            var result = await _controller.GetPimsDetail(parentproject);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(detailRes, okResult.Value);

            await _service.Received(1).GetPimsDetailAsync(parentproject);
            _mapper.Received(1).Map<ProjectDetailRes>(detailDto);
        }

        

        [Fact]
        public async Task GetPimsDetail_WhenServiceReturnsNull_ReturnsJsonSuccessResponseWithNullData()
        {
            // Arrange
            var parentproject = "PP001";
            _service.GetPimsDetailAsync(parentproject).Returns((ProjectDetailDto?)null);

            // Act
            var result = await _controller.GetPimsDetail(parentproject);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var apiResponse = Assert.IsType<Apha.Common.Contracts.ApiResponse<ProjectDetailRes>>(jsonResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Null(apiResponse.Data);
            Assert.NotNull(apiResponse.Meta);
            _mapper.DidNotReceive().Map<ProjectDetailRes>(Arg.Any<ProjectDetailDto>());
        }

        [Fact]
        public async Task GetPimsDetail_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var parentproject = "PP001";
            _service.GetPimsDetailAsync(parentproject).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPimsDetail(parentproject));

            await _service.Received(1).GetPimsDetailAsync(parentproject);
            _mapper.DidNotReceive().Map<ProjectDetailRes>(Arg.Any<ProjectDetailDto>());
        }

        #endregion

        #region SavePimsDetail

        [Fact]
        public async Task SavePimsDetail_ReturnsOkResult_WithMappedDetail_AndSetsParentproject()
        {
            // Arrange
            var parentproject = "PP001";
            var request = new ProjectDetailReq
            {
                Version = "1.0",
                FileRef = "FR001",
                CustomerRef = "CR001",
                CostbookNumber = "CB001",
                Riskid = 2,
                UseProjectYears = false
            };
            var dto = new ProjectDetailDto
            {
                Version = "1.0",
                FileRef = "FR001",
                CustomerRef = "CR001",
                CostbookNumber = "CB001",
                Riskid = 2,
                UseProjectYears = false
            };
            var savedDto = new ProjectDetailDto
            {
                Parentproject = parentproject,
                Version = "1.0",
                FileRef = "FR001",
                CustomerRef = "CR001",
                CostbookNumber = "CB001",
                Riskid = 2,
                UseProjectYears = false
            };
            var savedRes = new ProjectDetailRes
            {
                Parentproject = parentproject,
                Version = "1.0",
                FileRef = "FR001",
                CustomerRef = "CR001",
                CostbookNumber = "CB001",
                Riskid = 2,
                UseProjectYears = false
            };

            _mapper.Map<ProjectDetailDto>(request).Returns(dto);
            _service.SavePimsDetailAsync(dto).Returns(savedDto);
            _mapper.Map<ProjectDetailRes>(savedDto).Returns(savedRes);

            // Act
            var result = await _controller.SavePimsDetail(parentproject, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(savedRes, okResult.Value);

            // Verify parentproject was set on the dto before service call
            Assert.Equal(parentproject, dto.Parentproject);

            _mapper.Received(1).Map<ProjectDetailDto>(request);
            await _service.Received(1).SavePimsDetailAsync(dto);
            _mapper.Received(1).Map<ProjectDetailRes>(savedDto);
        }

        [Fact]
        public async Task SavePimsDetail_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var parentproject = "PP001";
            var request = new ProjectDetailReq { Version = "1.0", FileRef = "FR001" };
            var dto = new ProjectDetailDto { Version = "1.0", FileRef = "FR001" };

            _mapper.Map<ProjectDetailDto>(request).Returns(dto);
            _service.SavePimsDetailAsync(dto).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.SavePimsDetail(parentproject, request));

            _mapper.Received(1).Map<ProjectDetailDto>(request);
            await _service.Received(1).SavePimsDetailAsync(dto);
            _mapper.DidNotReceive().Map<ProjectDetailRes>(Arg.Any<ProjectDetailDto>());
        }

        #endregion

        #region GetProposedProject

        [Fact]
        public async Task GetProposedProject_WithValidProject_ReturnsOkResult_WithMappedProject()
        {
            // Arrange
            var parentproject = "PP001";
            var proposedDto = new ProposedProjectDto
            {
                Id = 1,
                Parentproject = parentproject,
                Projecttitle = "TB Project",
                Program = "PROG1",
                Customer = "CUST1",
                Manager = "MGR1",
                Projectstatus = "Proposed",
                Disease = "TB"
            };
            var proposedRes = new ProposedProjectRes
            {
                Id = 1,
                Parentproject = parentproject,
                Projecttitle = "TB Project",
                Projectstatus = "Proposed",
                Disease = "TB"
            };

            _service.GetProposedProjectAsync(parentproject).Returns(proposedDto);
            _mapper.Map<ProposedProjectRes>(proposedDto).Returns(proposedRes);

            // Act
            var result = await _controller.GetProposedProject(parentproject);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(proposedRes, okResult.Value);

            await _service.Received(1).GetProposedProjectAsync(parentproject);
            _mapper.Received(1).Map<ProposedProjectRes>(proposedDto);
        }
       

        [Fact]
        public async Task GetProposedProject_WhenServiceReturnsNull_ReturnsJsonSuccessResponseWithNullData()
        {
            // Arrange
            var parentproject = "PP001";
            _service.GetProposedProjectAsync(parentproject).Returns((ProposedProjectDto?)null);

            // Act
            var result = await _controller.GetProposedProject(parentproject);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var apiResponse = Assert.IsType<Apha.Common.Contracts.ApiResponse<ProposedProjectRes>>(jsonResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Null(apiResponse.Data);
            Assert.NotNull(apiResponse.Meta);
            _mapper.DidNotReceive().Map<ProposedProjectRes>(Arg.Any<ProposedProjectDto>());
        }

        [Fact]
        public async Task GetProposedProject_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var parentproject = "PP001";
            _service.GetProposedProjectAsync(parentproject).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetProposedProject(parentproject));

            await _service.Received(1).GetProposedProjectAsync(parentproject);
            _mapper.DidNotReceive().Map<ProposedProjectRes>(Arg.Any<ProposedProjectDto>());
        }

        #endregion

        #region UpdateProposedProject

        [Fact]
        public async Task UpdateProposedProject_ReturnsOkResult_WithMappedProject_AndSetsParentproject()
        {
            // Arrange
            var parentproject = "PP001";
            var request = new ProposedProjectReq
            {
                Projecttitle  = "Updated TB Project",
                Program       = "PROG2",
                Customer      = "CUST2",
                Manager       = "MGR2",
                Projectstatus = "Active",
                Disease       = "TB",
                TransferTo    = null   // null → transferTo falls back to parentproject
            };
            var dto = new ProposedProjectDto
            {
                Projecttitle  = "Updated TB Project",
                Program       = "PROG2",
                Customer      = "CUST2",
                Manager       = "MGR2",
                Projectstatus = "Active",
                Disease       = "TB"
            };
            var updatedDto = new ProposedProjectDto
            {
                Id            = 5,
                Parentproject = parentproject,
                Projecttitle  = "Updated TB Project",
                Program       = "PROG2",
                Customer      = "CUST2",
                Manager       = "MGR2",
                Projectstatus = "Active",
                Disease       = "TB"
            };
            var updatedRes = new ProposedProjectRes
            {
                Id            = 5,
                Parentproject = parentproject,
                Projecttitle  = "Updated TB Project",
                Projectstatus = "Active",
                Disease       = "TB"
            };

            _mapper.Map<ProposedProjectDto>(request).Returns(dto);
            // transferTo = request.TransferTo ?? parentproject → "PP001"
            _service.UpdateProposedProjectAsync(dto, parentproject).Returns(updatedDto);
            _mapper.Map<ProposedProjectRes>(updatedDto).Returns(updatedRes);

            // Act
            var result = await _controller.UpdateProposedProject(parentproject, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(updatedRes, okResult.Value);

            // Verify parentproject was set on the dto before service call
            Assert.Equal(parentproject, dto.Parentproject);

            _mapper.Received(1).Map<ProposedProjectDto>(request);
            await _service.Received(1).UpdateProposedProjectAsync(dto, parentproject);
            _mapper.Received(1).Map<ProposedProjectRes>(updatedDto);
        }

        [Fact]
        public async Task UpdateProposedProject_WithTransferTo_PassesTransferToToService()
        {
            // Arrange
            var parentproject = "PP001";
            var transferTo    = "PP002";
            var request = new ProposedProjectReq
            {
                Projecttitle  = "Updated TB Project",
                Projectstatus = "Active",
                TransferTo    = transferTo   // explicit transfer target
            };
            var dto        = new ProposedProjectDto { Projecttitle = "Updated TB Project", Projectstatus = "Active" };
            var updatedDto = new ProposedProjectDto { Id = 5, Parentproject = transferTo, Projecttitle = "Updated TB Project" };
            var updatedRes = new ProposedProjectRes { Id = 5, Parentproject = transferTo, Projecttitle = "Updated TB Project" };

            _mapper.Map<ProposedProjectDto>(request).Returns(dto);
            _service.UpdateProposedProjectAsync(dto, transferTo).Returns(updatedDto);
            _mapper.Map<ProposedProjectRes>(updatedDto).Returns(updatedRes);

            // Act
            var result = await _controller.UpdateProposedProject(parentproject, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(updatedRes, okResult.Value);

            await _service.Received(1).UpdateProposedProjectAsync(dto, transferTo);
        }

        [Fact]
        public async Task UpdateProposedProject_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var parentproject = "PP001";
            var request = new ProposedProjectReq
            {
                Projecttitle  = "Updated Project",
                Projectstatus = "Active",
                TransferTo    = null
            };
            var dto = new ProposedProjectDto { Projecttitle = "Updated Project", Projectstatus = "Active" };

            _mapper.Map<ProposedProjectDto>(request).Returns(dto);
            // transferTo = request.TransferTo ?? parentproject → "PP001"
            _service.UpdateProposedProjectAsync(dto, parentproject).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.UpdateProposedProject(parentproject, request));

            _mapper.Received(1).Map<ProposedProjectDto>(request);
            await _service.Received(1).UpdateProposedProjectAsync(dto, parentproject);
            _mapper.DidNotReceive().Map<ProposedProjectRes>(Arg.Any<ProposedProjectDto>());
        }

        #endregion

        #region GetFpsProjectById

        [Fact]
        public async Task GetFpsProjectById_WithValidProject_ReturnsOkResult_WithMappedProject()
        {
            // Arrange
            var parentproject = "PP001";
            var projectDto = new ProjectDto
            {
                Parentproject = parentproject,
                Projecttitle = "FMD Survey",
                Disease = "FMD",
                Contract = "CON001",
                Projectstatus = "Active"
            };
            var projectRes = new ProjectRes
            {
                Parentproject = parentproject,
                Projecttitle = "FMD Survey",
                Disease = "FMD",
                Contract = "CON001",
                Projectstatus = "Active"
            };

            _service.GetFpsProjectByIdAsync(parentproject).Returns(projectDto);
            _mapper.Map<ProjectRes>(projectDto).Returns(projectRes);

            // Act
            var result = await _controller.GetFpsProjectById(parentproject);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(projectRes, okResult.Value);

            await _service.Received(1).GetFpsProjectByIdAsync(parentproject);
            _mapper.Received(1).Map<ProjectRes>(projectDto);
        }

        [Fact]
        public async Task GetFpsProjectById_WhenServiceReturnsNull_ReturnsJsonSuccessResponseWithNullData()
        {
            // Arrange
            var parentproject = "PP001";
            _service.GetFpsProjectByIdAsync(parentproject).Returns((ProjectDto?)null);

            // Act
            var result = await _controller.GetFpsProjectById(parentproject);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var apiResponse = Assert.IsType<Apha.Common.Contracts.ApiResponse<ProjectRes>>(jsonResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Null(apiResponse.Data);
            Assert.NotNull(apiResponse.Meta);
            _mapper.DidNotReceive().Map<ProjectRes>(Arg.Any<ProjectDto>());
        }

        [Fact]
        public async Task GetFpsProjectById_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var parentproject = "PP001";
            _service.GetFpsProjectByIdAsync(parentproject).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetFpsProjectById(parentproject));

            await _service.Received(1).GetFpsProjectByIdAsync(parentproject);
            _mapper.DidNotReceive().Map<ProjectRes>(Arg.Any<ProjectDto>());
        }

        #endregion
    }
}