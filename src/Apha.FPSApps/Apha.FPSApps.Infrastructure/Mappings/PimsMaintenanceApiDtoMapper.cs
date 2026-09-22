using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos.PIMS;
using Mapster;

namespace Apha.FPSApps.Infrastructure.Mappings
{
    public class PimsMaintenanceApiDtoMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<ReportRes, ReportDto>().TwoWays();
            config.NewConfig<ReportDto, ReportReq>().TwoWays();

            config.NewConfig<ReportGroupRes, ReportGroupDto>().TwoWays();
            config.NewConfig<ReportGroupDto, ReportGroupReq>().TwoWays();

            config.NewConfig<ReportGroupLinkRes, ReportGroupLinkDto>().TwoWays();
            config.NewConfig<ReportGroupLinkDto, ReportGroupLinkReq>().TwoWays();

            config.NewConfig<ProjectManagerRes, ProjectManagerDto>().TwoWays();
            config.NewConfig<ProjectManagerDto, ProjectManagerReq>().TwoWays();

            config.NewConfig<ProgramManagerLinkRes, ProgramManagerLinkDto>().TwoWays();
            config.NewConfig<ProgramManagerLinkDto, ProgramManagerLinkReq>().TwoWays();

            config.NewConfig<ProfitCentreManagerLinkRes, ProfitCentreManagerLinkDto>().TwoWays();
            config.NewConfig<ProfitCentreManagerLinkDto, ProfitCentreManagerLinkReq>().TwoWays();

            config.NewConfig<SettingRes, SettingDto>()
                .Map(dest => dest.SettingValue, src => src.Setting);
            config.NewConfig<SettingDto, SettingRes>()
                .Map(dest => dest.Setting, src => src.SettingValue);

            config.NewConfig<SettingDto, SettingReq>()
                .Map(dest => dest.Setting, src => src.SettingValue);
            config.NewConfig<SettingReq, SettingDto>()
                .Map(dest => dest.SettingValue, src => src.Setting);

            config.NewConfig<AccessUserRes, AccessUserDto>().TwoWays();
            config.NewConfig<AccessUserDto, AccessUserReq>().TwoWays();

            config.NewConfig<AccessLevelRes, AccessLevelDto>().TwoWays();

            config.NewConfig<AccessUserLevelRes, AccessUserLevelDto>().TwoWays();
            config.NewConfig<AccessUserLevelDto, AccessUserLevelReq>().TwoWays();

            config.NewConfig<AccessSystemRes, AccessSystemDto>();

            config.NewConfig<FrequencyRes, FrequencyDto>()
                .Map(dest => dest.Frequencyid, src => src.Frequencyid);
            config.NewConfig<FrequencyDto, FrequencyRes>()
                .Map(dest => dest.FrequencyValue, src => src.FrequencyValue);

            config.NewConfig<FrequencyDto, FrequencyReq>()
                .Map(dest => dest.FrequencyId, src => src.Frequencyid);
            config.NewConfig<FrequencyReq, FrequencyDto>()
                .Map(dest => dest.FrequencyValue, src => src.FrequencyValue);

            config.NewConfig<ReviewItemRes, ReviewItemDto>().TwoWays();
            config.NewConfig<ReviewItemDto, ReviewItemReq>().TwoWays();

            config.NewConfig<RadTrackProgRes, RadTrackProgDto>().TwoWays();
            config.NewConfig<RadTrackProgDto, RadTrackProgReq>().TwoWays();
        }
    }
}
