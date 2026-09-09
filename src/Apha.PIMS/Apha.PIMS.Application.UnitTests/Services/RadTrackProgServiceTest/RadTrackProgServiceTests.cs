using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Services;
using Apha.PIMS.Application.Validation;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using AutoMapper;
using NSubstitute;

namespace Apha.PIMS.Application.UnitTests.Services.RadTrackProgServiceTest
{
    public class RadTrackProgServiceTests
    {
        private readonly IRadTrackProgRepository _repository;
        private readonly IMapper _mapper;
        private readonly RadTrackProgService _service;

        public RadTrackProgServiceTests()
        {
            _repository = Substitute.For<IRadTrackProgRepository>();
            _mapper = Substitute.For<IMapper>();
            _service = new RadTrackProgService(_repository, _mapper);
        }

        private static RadTrackProgDto MakeDto(string program = "TEST001") => new() { Program = program, RadTrackProg = true, PublicationPrefix = "TP" };
        private static RadtrackProg MakeEntity(string program = "TEST001") => new() { Program = program, Radtrackprog = true, Publicationprefix = "TP" };

        [Fact]
        public async Task GetRadTrackProgByProgramAsync_Whitespace_ReturnsNull()
        {
            var result = await _service.GetRadTrackProgByProgramAsync("   ");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetRadTrackProgByProgramAsync_FindsUsingTrimmedCaseInsensitive()
        {
            var entity = MakeEntity("ADMIN");
            var dto = MakeDto("ADMIN");
            _repository.GetAllRadTrackProgsAsync().Returns(new List<RadtrackProg> { entity });
            _mapper.Map<RadTrackProgDto>(entity).Returns(dto);

            var result = await _service.GetRadTrackProgByProgramAsync("  admin  ");

            Assert.NotNull(result);
            Assert.Equal("ADMIN", result!.Program);
        }

        [Fact]
        public async Task CreateRadTrackProgAsync_NullProgram_ThrowsBusinessValidationErrorException()
        {
            var dto = MakeDto(null!);

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _service.CreateRadTrackProgAsync(dto));

            Assert.Equal("PROGRAM_REQUIRED", ex.Errors[0].Code);
        }

        [Fact]
        public async Task CreateRadTrackProgAsync_DuplicateProgram_ThrowsBusinessValidationErrorException()
        {
            var dto = MakeDto(" admin ");
            _repository.GetAllRadTrackProgsAsync().Returns(new List<RadtrackProg> { MakeEntity("ADMIN") });

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _service.CreateRadTrackProgAsync(dto));

            Assert.Equal("PROGRAM_DUPLICATE", ex.Errors[0].Code);
        }

        [Fact]
        public async Task DeleteRadTrackProgAsync_NullProgram_ThrowsBusinessValidationErrorException()
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _service.DeleteRadTrackProgAsync(null!));
            Assert.Equal("PROGRAM_REQUIRED", ex.Errors[0].Code);
        }

        [Fact]
        public async Task DeleteRadTrackProgAsync_NotFound_ThrowsBusinessValidationErrorException()
        {
            _repository.GetAllRadTrackProgsAsync().Returns(new List<RadtrackProg> { MakeEntity("KNOWN") });

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _service.DeleteRadTrackProgAsync("unknown"));

            Assert.Equal("PROGRAM_NOT_FOUND", ex.Errors[0].Code);
        }

        [Fact]
        public async Task RadTrackProgExistsAsync_NonExistent_ReturnsFalse()
        {
            _repository.GetAllRadTrackProgsAsync().Returns(new List<RadtrackProg> { MakeEntity("KNOWN") });

            var result = await _service.RadTrackProgExistsAsync("unknown");

            Assert.False(result);
        }
    }
}
