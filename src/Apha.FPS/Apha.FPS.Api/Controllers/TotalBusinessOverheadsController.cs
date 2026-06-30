using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for managing Total Business Overheads.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/totalbusinessoverheads")]
    public class TotalBusinessOverheadsController : ControllerBase
    {
        private readonly ITotalBusinessOverheadsService _service;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TotalBusinessOverheadsController"/> class.
        /// </summary>
        /// <param name="service">The Total Business Overheads service.</param>
        /// <param name="mapper">The AutoMapper instance.</param>
        public TotalBusinessOverheadsController(
            ITotalBusinessOverheadsService service,
            IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Gets the Total Business Overheads for the current FPS year.
        /// </summary>
        /// <returns>The Total Business Overheads record.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var result = await _service.GetAsync();
            if (result == null)
                throw new KeyNotFoundException("Total Business Overheads not found for the current year.");
            return Ok(_mapper.Map<TotalBusinessOverheadsRes>(result));
        }

        /// <summary>
        /// Updates the Total Business Overheads for the current FPS year.
        /// </summary>
        /// <param name="req">The request data with updated value.</param>
        /// <returns>The updated Total Business Overheads record.</returns>
        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] TotalBusinessOverheadsReq req)
        {
            var dto = _mapper.Map<TotalBusinessOverheadsDto>(req);
            var result = await _service.UpdateAsync(dto);
            return Ok(_mapper.Map<TotalBusinessOverheadsRes>(result));
        }
    }
}
