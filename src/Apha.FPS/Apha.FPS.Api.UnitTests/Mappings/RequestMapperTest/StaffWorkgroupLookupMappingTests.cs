using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Mappings;
using Apha.FPS.Application.Dtos;
using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;

namespace Apha.FPS.Api.UnitTests.Mappings.RequestMapperTest
{
    public class StaffWorkgroupLookupMappingTests
    {
        private readonly IMapper _mapper;

        public StaffWorkgroupLookupMappingTests()
        {
            var config = new MapperConfiguration(
                cfg => cfg.AddProfile<RequestMapper>(),
                NullLoggerFactory.Instance);
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void Map_DtoToRes_MapsAllStaffingHourFields()
        {
            var dto = new StaffWorkgroupLookupDto
            {
                StaffID = "S001",
                Name = "John Doe",
                WorkGroupGrade = "WG01",
                HrsAvail = 100.5,
                HrsPaid = 150.25,
                Leave = 20.75,
                SickSpecial = 5.5
            };

            var res = _mapper.Map<StaffWorkgroupLookupRes>(dto);

            Assert.Equal("S001", res.StaffID);
            Assert.Equal("John Doe", res.Name);
            Assert.Equal("WG01", res.WorkGroupGrade);
            Assert.Equal(100.5, res.HrsAvail, 4);
            Assert.Equal(150.25, res.HrsPaid, 4);
            Assert.Equal(20.75, res.Leave, 4);
            Assert.Equal(5.5, res.SickSpecial, 4);
        }

        [Fact]
        public void Map_ResToDto_MapsAllStaffingHourFields()
        {
            var res = new StaffWorkgroupLookupRes
            {
                StaffID = "S002",
                Name = "Amy Smith",
                WorkGroupGrade = "WG02",
                HrsAvail = 80,
                HrsPaid = 120,
                Leave = 10,
                SickSpecial = 2
            };

            var dto = _mapper.Map<StaffWorkgroupLookupDto>(res);

            Assert.Equal("S002", dto.StaffID);
            Assert.Equal("Amy Smith", dto.Name);
            Assert.Equal("WG02", dto.WorkGroupGrade);
            Assert.Equal(80, dto.HrsAvail, 4);
            Assert.Equal(120, dto.HrsPaid, 4);
            Assert.Equal(10, dto.Leave, 4);
            Assert.Equal(2, dto.SickSpecial, 4);
        }

        [Fact]
        public void Map_DtoToRes_DefaultsHourFieldsToZero_WhenNotSet()
        {
            var dto = new StaffWorkgroupLookupDto { StaffID = "S003", Name = "Bob" };

            var res = _mapper.Map<StaffWorkgroupLookupRes>(dto);

            Assert.Equal(0, res.HrsPaid, 4);
            Assert.Equal(0, res.Leave, 4);
            Assert.Equal(0, res.SickSpecial, 4);
        }
    }
}
