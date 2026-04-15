using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [Route("api/v{version:apiVersion}/testorproduct")]
    [ApiController]
    [ApiVersion("1.0")]
    public class TestorProductController : ControllerBase
    {
        private readonly ITestorProductService _testorProductService;

        public TestorProductController(ITestorProductService testorProductService)
        {
            _testorProductService = testorProductService ?? throw new ArgumentNullException(nameof(testorProductService));
        }

        [HttpGet]
        public async Task<ActionResult<List<TestorProductRes>>> GetAllTestorProductsAsync()
        {
            var items = await _testorProductService.GetAllTestorProductsAsync();
            return Ok(items.Select(i => new TestorProductRes
            {
                ItemCode = i.ItemCode,
                ItemDescription = i.ItemDescription
            }).ToList());
        }
    }
}
