using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Application.Dtos;
using AutoMapper;

namespace Apha.Costbook.Application.Services
{
    public class StaffService : IStaffService
    {
        private readonly IStaffRepository _repo;
        private readonly IMapper _mapper;

        public StaffService(IStaffRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<StaffDto>> GetAllStaffAsync()
        {
            var staff = await _repo.GetAllStaffAsync();
            return _mapper.Map<List<StaffDto>>(staff);
        }
    }
}
