using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Pagination;
using Apha.Costbook.DataAccess;
using Mapster;

namespace Apha.Costbook.Application.Mappings
{
	public class EntityMapper : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig(typeof(PaginationParameters<>), typeof(QueryParameters<>));
			config.NewConfig(typeof(QueryParameters<>), typeof(PaginationParameters<>));
			config.NewConfig(typeof(PagedData<>), typeof(PaginatedResult<>));
			config.NewConfig(typeof(PaginatedResult<>), typeof(PagedData<>));

			config.NewConfig<PaginationData, PaginationDto>().TwoWays();
			config.NewConfig<Project, ProjectDto>().TwoWays();
			config.NewConfig<Program, ProgramDto>().TwoWays();
			config.NewConfig<Customer, CustomerDto>().TwoWays();
			config.NewConfig<Disease, DiseaseDto>().TwoWays();
			config.NewConfig<Staff, StaffDto>().TwoWays();
			config.NewConfig<AccountGroup, AccountGroupDto>().TwoWays();

			config.NewConfig<FpsAccountCategory, AccountCategoryMaintenanceDto>()
				.Map(dest => dest.FpsYear, src => src.FpsYear ?? 0);

			config.NewConfig<AccountCategoryMaintenanceDto, FpsAccountCategory>()
				.Map(dest => dest.FpsYear, src => (int?)src.FpsYear);
		}
	}
}
