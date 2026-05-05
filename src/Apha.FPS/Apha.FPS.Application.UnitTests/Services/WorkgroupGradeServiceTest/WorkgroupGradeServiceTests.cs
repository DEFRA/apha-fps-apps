using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;

namespace Apha.FPS.Application.UnitTests.Services.WorkgroupGradeServiceTest
{
    public class WorkgroupGradeServiceTests
    {
        private readonly IWorkgroupGradeRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly WorkgroupGradeService _sut;

        public WorkgroupGradeServiceTests()
        {
            _mockRepository = Substitute.For<IWorkgroupGradeRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new WorkgroupGradeService(_mockRepository, _mockMapper);
        }

        #region GetAllWorkgroupGradesPagedAsync

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_ValidQuery_ReturnsPaginatedResult()
        {
            var query = new QueryParameters<string>();
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<WorkgroupGrade>();
            var pagedResult = new PaginatedResult<WorkgroupGradeDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAllWorkgroupGradesPagedAsync(mappedParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<WorkgroupGradeDto>>(pagedData).Returns(pagedResult);

            var result = await _sut.GetAllWorkgroupGradesPagedAsync(query);

            result.Should().Be(pagedResult);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetAllWorkgroupGradesPagedAsync(mappedParams);
        }

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_NullQuery_ThrowsBusinessValidationErrorException()
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetAllWorkgroupGradesPagedAsync(null!));

            ex.Errors.Should().ContainSingle(e => e.Code == "WORKGROUPGRADE_INVALID_QUERY");
        }

        #endregion

        #region GetByWgGradeAsync

        [Fact]
        public async Task GetByWgGradeAsync_ValidCode_ReturnsMappedDto()
        {
            var entity = new WorkgroupGrade { WgGrade = "WG01" };
            var dto = new WorkgroupGradeDto { WgGrade = "WG01" };

            _mockRepository.GetByWgGradeAsync("WG01").Returns(entity);
            _mockMapper.Map<WorkgroupGradeDto>(entity).Returns(dto);

            var result = await _sut.GetByWgGradeAsync("WG01");

            result.Should().Be(dto);
            await _mockRepository.Received(1).GetByWgGradeAsync("WG01");
        }

        [Fact]
        public async Task GetByWgGradeAsync_NotFound_ReturnsNull()
        {
            _mockRepository.GetByWgGradeAsync("INVALID").Returns((WorkgroupGrade?)null);

            var result = await _sut.GetByWgGradeAsync("INVALID");

            result.Should().BeNull();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetByWgGradeAsync_NullOrEmpty_ThrowsBusinessValidationErrorException(string? wgGrade)
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetByWgGradeAsync(wgGrade!));

            ex.Errors.Should().ContainSingle(e => e.Code == "WORKGROUPGRADE_INVALID_CODE");
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ValidInput_ReturnsMappedDto()
        {
            var dto = new WorkgroupGradeDto { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };
            var entity = new WorkgroupGrade { WgGrade = "WG01" };
            var created = new WorkgroupGrade { WgGrade = "WG01" };
            var expected = new WorkgroupGradeDto { WgGrade = "WG01" };

            _mockMapper.Map<WorkgroupGrade>(dto).Returns(entity);
            _mockRepository.CreateAsync(entity).Returns(created);
            _mockMapper.Map<WorkgroupGradeDto>(created).Returns(expected);

            var result = await _sut.CreateAsync(dto);

            result.Should().Be(expected);
            _mockMapper.Received(1).Map<WorkgroupGrade>(dto);
            await _mockRepository.Received(1).CreateAsync(entity);
        }

        [Fact]
        public async Task CreateAsync_NullDto_ThrowsBusinessValidationErrorException()
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.CreateAsync(null!));

            ex.Errors.Should().ContainSingle(e => e.Code == "WORKGROUPGRADE_INVALID_DATA");
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ValidInput_ReturnsMappedDto()
        {
            var dto = new WorkgroupGradeDto { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" };
            var entity = new WorkgroupGrade { WgGrade = "WG01" };
            var updated = new WorkgroupGrade { WgGrade = "WG01" };
            var expected = new WorkgroupGradeDto { WgGrade = "WG01" };

            _mockMapper.Map<WorkgroupGrade>(dto).Returns(entity);
            _mockRepository.UpdateAsync(entity).Returns(updated);
            _mockMapper.Map<WorkgroupGradeDto>(updated).Returns(expected);

            var result = await _sut.UpdateAsync(dto);

            result.Should().Be(expected);
            _mockMapper.Received(1).Map<WorkgroupGrade>(dto);
            await _mockRepository.Received(1).UpdateAsync(entity);
        }

        [Fact]
        public async Task UpdateAsync_NullDto_ThrowsBusinessValidationErrorException()
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.UpdateAsync(null!));

            ex.Errors.Should().ContainSingle(e => e.Code == "WORKGROUPGRADE_INVALID_DATA");
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ValidCode_NoAssociations_ReturnsTrue()
        {
            _mockRepository.HasAssociatedStaffAsync("WG01").Returns(false);
            _mockRepository.DeleteAsync("WG01").Returns(true);

            var result = await _sut.DeleteAsync("WG01");

            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteAsync("WG01");
        }

        [Fact]
        public async Task DeleteAsync_HasAssociations_ThrowsBusinessValidationErrorException()
        {
            _mockRepository.HasAssociatedStaffAsync("WG01").Returns(true);

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.DeleteAsync("WG01"));

            ex.Errors.Should().ContainSingle(e => e.Code == "WORKGROUPGRADE_HAS_ASSOCIATIONS");
            await _mockRepository.DidNotReceive().DeleteAsync(Arg.Any<string>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task DeleteAsync_NullOrEmpty_ThrowsBusinessValidationErrorException(string? wgGrade)
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.DeleteAsync(wgGrade!));

            ex.Errors.Should().ContainSingle(e => e.Code == "WORKGROUPGRADE_INVALID_CODE");
        }

        #endregion

        #region GetAllPcGradesAsync

        [Fact]
        public async Task GetAllPcGradesAsync_ReturnsList()
        {
            var grades = new List<string> { "PC01", "PC02" };
            _mockRepository.GetAllPcGradesAsync().Returns(grades);

            var result = await _sut.GetAllPcGradesAsync();

            result.Should().BeEquivalentTo(grades);
            await _mockRepository.Received(1).GetAllPcGradesAsync();
        }

        #endregion

        #region GetAllGradeCodesAsync

        [Fact]
        public async Task GetAllGradeCodesAsync_ReturnsList()
        {
            var codes = new List<string> { "G01", "G02" };
            _mockRepository.GetAllGradeCodesAsync().Returns(codes);

            var result = await _sut.GetAllGradeCodesAsync();

            result.Should().BeEquivalentTo(codes);
            await _mockRepository.Received(1).GetAllGradeCodesAsync();
        }

        #endregion

        #region GetAllWorkgroupNamesAsync

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_ReturnsList()
        {
            var names = new List<string> { "IT", "HR" };
            _mockRepository.GetAllWorkgroupNamesAsync().Returns(names);

            var result = await _sut.GetAllWorkgroupNamesAsync();

            result.Should().BeEquivalentTo(names);
            await _mockRepository.Received(1).GetAllWorkgroupNamesAsync();
        }

        #endregion
    }
}
