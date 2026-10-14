using Apha.Common.Contracts.PACT;
using Apha.PACT.Api.Mappings;
using Apha.PACT.Application.Dtos;
using Mapster;
using MapsterMapper;

namespace Apha.PACT.Api.UnitTests.Mappings
{
    public class WorkGroupMaintenanceReqMappingTests
    {
        private readonly IMapper _mapper;

        public WorkGroupMaintenanceReqMappingTests()
        {
            var config = new TypeAdapterConfig();
            new RequestMapper().Register(config);
            _mapper = new Mapper(config);
        }

        [Fact]
        public void Map_WorkGroupMaintenanceReq_To_WorkGroupDto_MapsSendEmailCos90AndEmailRecipient()
        {
            var request = new WorkGroupMaintenanceReq
            {
                WorkGroupName = "WG1",
                ProfitCentre = "PC1",
                CostCentre = 123,
                Owner = "OWNER",
                Description = "Desc",
                CentralOverhead = 10.5m,
                SendEmail = 1,
                Cos90 = 1,
                EmailRecipient = "user@example.com"
            };

            var dto = _mapper.Map<WorkGroupDto>(request);

            Assert.Equal("WG1", dto.WorkGroupName);
            Assert.Equal("PC1", dto.ProfitCentre);
            Assert.Equal(123, dto.CostCentre);
            Assert.Equal("OWNER", dto.Owner);
            Assert.Equal("Desc", dto.Description);
            Assert.Equal(10.5m, dto.CentralOverhead);
            Assert.Equal((short)1, dto.SendEmail);
            Assert.Equal((short)1, dto.Cos90);
            Assert.Equal("user@example.com", dto.EmailRecipient);
        }

        [Fact]
        public void Map_WorkGroupMaintenanceReq_To_WorkGroupDto_NullNewFields_RemainNull()
        {
            var request = new WorkGroupMaintenanceReq
            {
                WorkGroupName = "WG1",
                ProfitCentre = "PC1"
            };

            var dto = _mapper.Map<WorkGroupDto>(request);

            Assert.Null(dto.SendEmail);
            Assert.Null(dto.Cos90);
            Assert.Null(dto.EmailRecipient);
        }

        [Fact]
        public void WorkGroupMaintenanceReq_NewProperties_GetAndSet()
        {
            var request = new WorkGroupMaintenanceReq
            {
                SendEmail = 0,
                Cos90 = 1,
                EmailRecipient = "a@b.com"
            };

            Assert.Equal((short)0, request.SendEmail);
            Assert.Equal((short)1, request.Cos90);
            Assert.Equal("a@b.com", request.EmailRecipient);
        }
    }
}
