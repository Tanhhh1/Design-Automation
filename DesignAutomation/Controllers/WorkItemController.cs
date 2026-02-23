using DesignAutomation.Models.WorkItem;
using DesignAutomation.Services;
using Microsoft.AspNetCore.Mvc;

namespace DesignAutomation.Controllers
{
    [ApiController]
    [Route("api/workitems")]
    public class WorkItemsController : ControllerBase
    {
        private readonly WorkItemService _service;

        public WorkItemsController(WorkItemService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateWorkItem(
            [FromBody] WorkItemRequest request)
        {
            var result = await _service.CreateWorkItemAsync(request);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStatus(string id)
        {
            var result = await _service.GetWorkItemStatusAsync(id);
            return Ok(result);
        }
    }
}
