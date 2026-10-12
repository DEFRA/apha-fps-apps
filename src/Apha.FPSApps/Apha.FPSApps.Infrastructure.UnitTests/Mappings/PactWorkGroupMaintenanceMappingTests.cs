using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Infrastructure.Mappings;
using Mapster;
using MapsterMapper;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Mappings
{
    public class PactWorkGroupMaintenanceMappingTests
    {
        private readonly IMapper _mapper;

        public PactWorkGroupMaintenanceMappingTests()
        {
            var config = new TypeAdapterConfig();
            config.NewConfig<WorkGroupDto, WorkGroupMaintenanceReq>().TwoWays();
            _mapper = new ServiceMapper(null!, config);
        }

        [Fact]
        public void Map_WorkGroupDto_To_WorkGroupMaintenanceReq_MapsSendEmailCos90AndEmailRecipient()
        {
            var dto = new WorkGroupDto
            {
                WorkGroupName = "WG1",
                ProfitCentre = "PC1",
                CostCentre = 123,
                Owner = "OWNER",
                Description = "Desc",
                CentralOverhead = 10.5m,
                SendEmail = 1,
                Cos90 = 0,
                EmailRecipient = "user@example.com"
            };

            var request = _mapper.Map<WorkGroupMaintenanceReq>(dto);

            Assert.Equal("WG1", request.WorkGroupName);
            Assert.Equal("PC1", request.ProfitCentre);
            Assert.Equal(123, request.CostCentre);
            Assert.Equal("OWNER", request.Owner);
            Assert.Equal("Desc", request.Description);
            Assert.Equal(10.5m, request.CentralOverhead);
            Assert.Equal((short)1, request.SendEmail);
            Assert.Equal((short)0, request.Cos90);
            Assert.Equal("user@example.com", request.EmailRecipient);
        }

        [Fact]
        public void Map_WorkGroupMaintenanceReq_To_WorkGroupDto_MapsSendEmailCos90AndEmailRecipient()
        {
            var request = new WorkGroupMaintenanceReq
            {
                WorkGroupName = "WG1",
                ProfitCentre = "PC1",
                SendEmail = 0,
                Cos90 = 1,
                EmailRecipient = "a@b.com"
            };

            var dto = _mapper.Map<WorkGroupDto>(request);

            Assert.Equal((short)0, dto.SendEmail);
            Assert.Equal((short)1, dto.Cos90);
            Assert.Equal("a@b.com", dto.EmailRecipient);
        }

        [Fact]
        public void PactApiDtoMapper_Register_MapsNewWorkGroupMaintenanceFields()
        {
            var config = new TypeAdapterConfig();
            new PactApiDtoMapper().Register(config);
            var mapper = new ServiceMapper(null!, config);

            var request = mapper.Map<WorkGroupMaintenanceReq>(new WorkGroupDto
            {
                WorkGroupName = "WG1",
                ProfitCentre = "PC1",
                SendEmail = 1,
                Cos90 = 1,
                EmailRecipient = "x@y.com"
            });

            Assert.Equal((short)1, request.SendEmail);
            Assert.Equal((short)1, request.Cos90);
            Assert.Equal("x@y.com", request.EmailRecipient);
        }
    }
}
