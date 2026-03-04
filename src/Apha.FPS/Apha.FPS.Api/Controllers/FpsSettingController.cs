using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    [ApiController]
    [Route("api/setting")]
    public class FpsSettingController : ControllerBase
    {
        private readonly IFpsSettingService _fpsSettingService;
        private readonly IMapper _mapper;

        public FpsSettingController(
                        IFpsSettingService fpsSettingService,
                        IMapper mapper)
        {
            _fpsSettingService = fpsSettingService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var result = await _fpsSettingService.GetAllSettingsAsync();
            return Ok(_mapper.Map<List<FpsSettingRes>>(result));
        }
    }
}
