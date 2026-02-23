using DesignAutomation.Models.AppBundle;
using DesignAutomation.Services;
using Microsoft.AspNetCore.Mvc;

namespace DesignAutomation.Controllers
{
    [ApiController]
    [Route("api/appbundles")]
    public class AppBundleController : ControllerBase
    {
        private readonly AppBundleService _appBundleService;
        public AppBundleController(AppBundleService service)
        {
            _appBundleService = service;
        }
        [HttpPost]
        public async Task<IActionResult> Create(AppBundleRequest request)
        {
            var result = await _appBundleService.CreateAppBundleAsync(request);
            return Ok(result);
        }
        [HttpPost("{id}/versions")]
        public async Task<IActionResult> CreateVersion(string id, [FromForm] string engine, IFormFile fileZip)
        {
            var result = await _appBundleService.CreateVersionAsync(id, engine, fileZip);
            return Ok(result);
        }
        [HttpPost("{id}/aliases")]
        public async Task<IActionResult> CreateAlias(string id, AppBundleAliasRequest request)
        {
            var result = await _appBundleService.CreateAliasAsync(id, request);
            return Ok(result);
        }

        [HttpPatch("{id}/aliases/{aliasId}")]
        public async Task<IActionResult> UpdateAlias(string id, string aliasId, UpdateAliasRequest request)
        {
            await _appBundleService.UpdateAliasAsync(id, aliasId, request);
            return NoContent();
        }
    }
}
