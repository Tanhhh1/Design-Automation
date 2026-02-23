using DesignAutomation.Models.Activity;
using DesignAutomation.Services;
using Microsoft.AspNetCore.Mvc;

namespace DesignAutomation.Controllers
{
    [ApiController]
    [Route("api/activities")]
    public class ActivitiesController : ControllerBase
    {
        private readonly ActivityService _activityService;

        public ActivitiesController(ActivityService activityService)
        {
            _activityService = activityService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateActivity(ActivityRequest request)
        {
            var result = await _activityService.CreateActivityAsync(request);
            return Ok(result);
        }

        [HttpPost("{id}/aliases")]
        public async Task<IActionResult> CreateAlias(string id, ActivityAliasRequest request)
        {
            var result = await _activityService.CreateAliasAsync(id, request);
            return Ok(result);
        }

        [HttpPost("{id}/versions")]
        public async Task<IActionResult> CreateVersion(string id, ActivityVersionRequest request)
        {
            var result = await _activityService.CreateVersionAsync(id, request);
            return Ok(result);
        }

        [HttpPatch("{id}/aliases/{aliasId}")]
        public async Task<IActionResult> UpdateAlias(string id, string aliasId, UpdateActivityAliasRequest request)
        {
            await _activityService.UpdateAliasAsync(id, aliasId, request);
            return NoContent(); 
        }
    }
}
