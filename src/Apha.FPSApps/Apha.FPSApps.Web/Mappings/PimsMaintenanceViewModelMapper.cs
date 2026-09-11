using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Mapster;

namespace Apha.FPSApps.Web.Mappings
{
    public class PimsMaintenanceViewModelMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<ReportItem, ReportDto>().TwoWays();
            config.NewConfig<ReportGroupItem, ReportGroupDto>().TwoWays();
            config.NewConfig<RadTrackProgItem, RadTrackProgDto>().TwoWays();
            config.NewConfig<ProjectManagerItem, ProjectManagerDto>().TwoWays();
            config.NewConfig<ProgramManagerLinkItem, ProgramManagerLinkDto>().TwoWays();
            config.NewConfig<ProfitCentreManagerLinkItem, ProfitCentreManagerLinkDto>().TwoWays();
            config.NewConfig<SettingItem, SettingDto>().TwoWays();
            config.NewConfig<AccessUserItem, AccessUserDto>().TwoWays();

            config.NewConfig<AccessUserLevelItem, AccessUserLevelDto>()
                .Map(dest => dest.SystemId, src => src.SystemId)
                .Map(dest => dest.NtLogin, src => src.NtLogin)
                .Map(dest => dest.AccessLevelId, src => src.AccessLevelId);
            config.NewConfig<AccessUserLevelDto, AccessUserLevelItem>()
                .Ignore(dest => dest.AccessLevelName); // populated by controller

            // ── Other Tab ────────────────────────────────────────────────────────────
            config.NewConfig<FrequencyItem, FrequencyDto>().TwoWays();
            config.NewConfig<ReviewItemItem, ReviewItemDto>().TwoWays();
            config.NewConfig<RiskItem, RiskDto>().TwoWays();
            config.NewConfig<PublicationTypeItem, PublicationTypeDto>().TwoWays();
            config.NewConfig<OtherReportGroupItem, ReportGroupDto>().TwoWays();
        }
    }
}
