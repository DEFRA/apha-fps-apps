using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos.DepartmentIncome;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Mappings
{
    public class FpsDepartmentIncomeApiDtoMapper : Profile
    {
        public FpsDepartmentIncomeApiDtoMapper()
        {
            // All 18 properties align by convention between DepartmentIncomeTimeRes and DepartmentIncomeTimeDto
            CreateMap<DepartmentIncomeTimeRes, DepartmentIncomeTimeDto>().ReverseMap();

            // All 14 properties align by convention between DepartmentIncomeTestRes and DepartmentIncomeTestDto
            CreateMap<DepartmentIncomeTestRes, DepartmentIncomeTestDto>().ReverseMap();

            // All 13 properties align by convention between DepartmentIncomeAnimalRes and DepartmentIncomeAnimalDto
            CreateMap<DepartmentIncomeAnimalRes, DepartmentIncomeAnimalDto>().ReverseMap();

            // All 8 properties align by convention between DepartmentIncomeAdditionalRes and DepartmentIncomeAdditionalDto
            CreateMap<DepartmentIncomeAdditionalRes, DepartmentIncomeAdditionalDto>().ReverseMap();

            // All 7 properties align by convention between DepartmentIncomeTotalsRes and DepartmentIncomeTotalsDto
            CreateMap<DepartmentIncomeTotalsRes, DepartmentIncomeTotalsDto>().ReverseMap();

            // Dedicated lookup DTO — must NOT be handled by CRUD entity mappers above
            // AccntsPeriod, MonthName, MonthNumber align by convention
            CreateMap<PeriodLookupRes, PeriodLookupDto>().ReverseMap();
        }
    }
}
