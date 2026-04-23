using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using System.Reflection.Emit;

namespace Apha.FPS.Application.Services
{
    
    public class StaffJobService : IStaffJobService
    {
        private readonly IStaffJobRepository _staffJobRepository;
        private readonly IMapper _mapper;
        
        public StaffJobService(IStaffJobRepository staffJobRepository, IMapper mapper)
        {
            _staffJobRepository = staffJobRepository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<StaffJobViewDto>> GetJobStaffCostAsync(QueryParameters<string> queryFilter, string jobCode)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(queryFilter);
            var staffJobViews = await _staffJobRepository.GetJobStaffCostAsync(filter, jobCode);
            return _mapper.Map<PaginatedResult<StaffJobViewDto>>(staffJobViews);
        }

        public async Task<decimal> GetTotalStaffCostAsync(string jobCode)
        {
            return await _staffJobRepository.GetTotalStaffCostAsync(jobCode);
        }

        public async Task<List<StaffWorkgroupLookupDto>> GetStaffWorkgroupLookup()
        {
            var staffWorkgroupLookup =  await _staffJobRepository.GetStaffWorkgroupLookup();
            return _mapper.Map<List<StaffWorkgroupLookupDto>>(staffWorkgroupLookup);
        }

        public async Task<decimal?> GetStaffChargeRate(string staffId, string jobcode)
        {
            var chargeRate = await _staffJobRepository.GetStaffChargeRate(staffId, jobcode);
            return chargeRate;
        }

        public async Task<StaffJobViewDto?> GetViewByStaffIdAsync(string staffId, string jobCode)
        {
            var staffWorkgroups = await _staffJobRepository.GetViewByStaffIdAsync(staffId, jobCode);
            return _mapper.Map<StaffJobViewDto>(staffWorkgroups);
        }

        public async Task<StaffJobDto?> GetByIdAsync(string staffId, string jobCode)
        {
            var staffWorkgroup = await _staffJobRepository.GetByIdAsync(staffId, jobCode);
            return _mapper.Map<StaffJobDto>(staffWorkgroup);
        }

        public async Task<StaffJobDto> AddAsync(StaffJobDto staffJob)
        {
            ArgumentNullException.ThrowIfNull(staffJob);
            ArgumentOutOfRangeException.ThrowIfNegative(staffJob.PlannedHours);
            var mapStaffJob = _mapper.Map<StaffJob>(staffJob);
            var staffWorkgroup = await _staffJobRepository.AddAsync(mapStaffJob);
            return _mapper.Map<StaffJobDto>(staffWorkgroup);
        }

        public async Task<StaffJobDto> UpdateAsync(StaffJobDto staffJob)
        {
            ArgumentNullException.ThrowIfNull(staffJob);
            ArgumentOutOfRangeException.ThrowIfNegative(staffJob.PlannedHours);
            var mapStaffJob = _mapper.Map<StaffJob>(staffJob);
            var staffWorkgroup = await _staffJobRepository.UpdateAsync(mapStaffJob);
            return _mapper.Map<StaffJobDto>(staffWorkgroup);
        }

        public async Task<bool> DeleteAsync(string staffId, string jobCode)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(jobCode);
            var isDeleted = await _staffJobRepository.DeleteAsync(staffId, jobCode);
            return isDeleted;
        }

    }
}
