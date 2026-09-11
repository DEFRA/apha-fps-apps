using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Services;
using Apha.PIMS.Application.Validation;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using MapsterMapper;
using NSubstitute;

namespace Apha.PIMS.Application.UnitTests.Services.ReportGroupLinkServiceTest
{
    public class ReportGroupLinkServiceTests
    {
        private readonly IReportGroupLinkRepository _repository;
        private readonly IReportRepository _reportRepository;
        private readonly IReportGroupRepository _reportGroupRepository;
        private readonly IMapper _mapper;
        private readonly ReportGroupLinkService _service;

        public ReportGroupLinkServiceTests()
        {
            _repository = Substitute.For<IReportGroupLinkRepository>();
            _reportRepository = Substitute.For<IReportRepository>();
            _reportGroupRepository = Substitute.For<IReportGroupRepository>();
            _mapper = Substitute.For<IMapper>();
            _service = new ReportGroupLinkService(_repository, _reportRepository, _reportGroupRepository, _mapper);
        }

        [Fact]
        public async Task CreateAsync_DuplicateLink_ThrowsNamesBasedMessage()
        {
            // Arrange
            var dto = new ReportGroupLinkDto { ReportId = 10, GroupId = 20 };
            _repository.ReportGroupLinkExistsAsync(10, 20).Returns(true);
            _reportRepository.GetReportByIdAsync(10).Returns(new Report { Id = 10, ReportName = "Annual Report" });
            _reportGroupRepository.GetReportGroupByIdAsync(20).Returns(new ReportGroup { GroupId = 20, Description = "Finance" });

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _service.CreateReportGroupLinkAsync(dto));

            // Assert
            Assert.Equal("REPORT_GROUP_LINK_DUPLICATE", ex.Errors[0].Code);
            Assert.Contains("Annual Report", ex.Errors[0].Message);
            Assert.Contains("Finance", ex.Errors[0].Message);
            await _repository.DidNotReceive().AddReportGroupLinkAsync(Arg.Any<ReportGroupLink>());
        }

        [Fact]
        public async Task DeleteAsync_MissingLink_ThrowsNamesBasedMessage()
        {
            // Arrange
            _repository.ReportGroupLinkExistsAsync(10, 20).Returns(false);
            _reportRepository.GetReportByIdAsync(10).Returns(new Report { Id = 10, ReportName = "Annual Report" });
            _reportGroupRepository.GetReportGroupByIdAsync(20).Returns(new ReportGroup { GroupId = 20, Description = "Finance" });

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _service.DeleteReportGroupLinkAsync(10, 20));

            // Assert
            Assert.Equal("REPORT_GROUP_LINK_NOT_FOUND", ex.Errors[0].Code);
            Assert.Contains("Annual Report", ex.Errors[0].Message);
            Assert.Contains("Finance", ex.Errors[0].Message);
        }
    }
}
