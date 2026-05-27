using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;

namespace Apha.PIMS.Application.Services
{
    public class ProjectYearCostsService : IProjectYearCostsService
    {
        private readonly IProjectYearCostsRepository _repository;
        private readonly IMapper _mapper;

        public ProjectYearCostsService(IProjectYearCostsRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<AdditionalCostDto>> GetAdditionalActualsAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            PagedData<MyProjSubContract> paged = await _repository.GetAdditionalActualsAsync(project, year, paging);
            return BuildResult(_mapper.Map<List<AdditionalCostDto>>(paged.Data), paged.PaginationData);
        }

        public async Task<PaginatedResult<AdditionalCostDto>> GetAdditionalPlansAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            PagedData<MyTblAdditionalCosts> paged = await _repository.GetAdditionalPlansAsync(project, year, paging);
            return BuildResult(_mapper.Map<List<AdditionalCostDto>>(paged.Data), paged.PaginationData);
        }

        private static PaginatedResult<AdditionalCostDto> BuildResult(
            List<AdditionalCostDto> items, PaginationData pd)
        {
            return new PaginatedResult<AdditionalCostDto>(items, new PaginationDto
            {
                PageNumber   = pd.PageNumber,
                PageSize     = pd.PageSize,
                TotalPages   = pd.TotalPages,
                TotalRecords = pd.TotalRecords
            });
        }
    }
}
