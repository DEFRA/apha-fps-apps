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

        public async Task<PaginatedResult<AnimalCostDto>> GetAnimalActualsAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            PagedData<MyProjSubContract> paged = await _repository.GetAnimalActualsAsync(project, year, paging);
            return BuildResult(_mapper.Map<List<AnimalCostDto>>(paged.Data), paged.PaginationData);
        }

        public async Task<PaginatedResult<AnimalCostDto>> GetAnimalPlansAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            PagedData<MyProjectAnimalPlan> paged = await _repository.GetAnimalPlansAsync(project, year, paging);
            return BuildResult(_mapper.Map<List<AnimalCostDto>>(paged.Data), paged.PaginationData);
        }

        public async Task<PaginatedResult<TestCostDto>> GetTestPlansAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            PagedData<MyTlkpTestReqmt> paged = await _repository.GetTestPlansAsync(project, year, paging);
            List<TestCostDto> items = paged.Data.Select(t => new TestCostDto
            {
                Year       = t.Year,
                Buyer      = t.Buyer,
                TestCode   = t.Testcode,
                UnitPrice  = t.Unitprice,
                NoRequired = t.Norequired,
                Cost       = t.Norequired.HasValue && t.Unitprice.HasValue
                                 ? t.Unitprice.Value * (decimal)t.Norequired.Value
                                 : null
            }).ToList();
            return BuildResult(items, paged.PaginationData);
        }

        public async Task<PaginatedResult<TestCostDto>> GetTestActualsAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            PagedData<(MyMonthlyOutput Output, MyTlkpTestReqmt Reqmt)> paged =
                await _repository.GetTestActualsAsync(project, year, paging);
            List<TestCostDto> items = paged.Data.Select(x => new TestCostDto
            {
                Year      = x.Output.Year,
                Buyer     = x.Output.Buyer,
                TestCode  = x.Output.Testcode,
                UnitPrice = x.Reqmt.Unitprice,
                Month     = x.Output.Month,
                WorkGroup = x.Output.Workgroup,
                Volume    = x.Output.Volume,
                Charge    = x.Output.Volume.HasValue && x.Reqmt.Unitprice.HasValue
                                ? x.Reqmt.Unitprice.Value * (decimal)x.Output.Volume.Value
                                : null
            }).ToList();
            return BuildResult(items, paged.PaginationData);
        }

        private static PaginatedResult<TDto> BuildResult<TDto>(List<TDto> items, PaginationData pd)
        {
            return new PaginatedResult<TDto>(items, new PaginationDto
            {
                PageNumber   = pd.PageNumber,
                PageSize     = pd.PageSize,
                TotalPages   = pd.TotalPages,
                TotalRecords = pd.TotalRecords
            });
        }

        public async Task<PaginatedResult<StaffCostDto>> GetStaffPlansAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            PagedData<MyProjectStaffPlan> paged = await _repository.GetStaffPlansAsync(project, year, paging);
            List<StaffCostDto> items = paged.Data.Select(s => new StaffCostDto
            {
                Year         = s.Year,
                ParentProject = s.Parentproject,
                WgGrade      = s.Workgroupgrade,
                Name         = s.Name,
                PlannedHours = s.Plannedhours,
                Rate         = s.Rate,
                Cost         = s.Cost
            }).ToList();
            return BuildResult(items, paged.PaginationData);
        }

        public async Task<PaginatedResult<StaffCostDto>> GetStaffActualsAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            PagedData<MyTimeCostCalcs> paged = await _repository.GetStaffActualsAsync(project, year, paging);
            List<StaffCostDto> items = paged.Data.Select(s => new StaffCostDto
            {
                JobCode    = s.Jobcode,
                Name       = s.Name,
                WorkGroup  = s.Workgroup,
                GradeCode  = s.Gradecode,
                Month      = s.Month,
                Time       = s.Time,
                ChargeRate = s.Chargerate,
                ActualCost = s.Time.HasValue && s.Chargerate.HasValue
                                 ? Math.Round((decimal)s.Time.Value * s.Chargerate.Value, 2)
                                 : null
            }).ToList();
            return BuildResult(items, paged.PaginationData);
        }

        public async Task<ProjectYearDetailsDto> GetProjectYearDetailsAsync(string project, short year)
        {
            Projects? entity = await _repository.GetProjectYearDetailsAsync(project, year);
            return entity is null ? new ProjectYearDetailsDto() : _mapper.Map<ProjectYearDetailsDto>(entity);
        }

        public async Task<PaginatedResult<PactPayDto>> GetPactPayAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            PagedData<PactPayCalc> paged = await _repository.GetPactPayAsync(project, year, paging);
            return BuildResult(_mapper.Map<List<PactPayDto>>(paged.Data), paged.PaginationData);
        }
    }
}
