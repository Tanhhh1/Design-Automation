using DesignAutomation.Services;
using Microsoft.AspNetCore.Mvc;


namespace DesignAutomation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OssController : ControllerBase
    {
        private readonly OssService _ossService;

        public OssController(OssService ossService)
        {
            _ossService = ossService;
        }

        [HttpGet("files")]
        public async Task<IActionResult> GetBucketFiles()
        {
            var files = await _ossService.GetFilesAsync();
            return Ok(files);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("File không hợp lệ.");
            using (var stream = file.OpenReadStream())
            {
                await _ossService.UploadFileAsync(file.FileName, stream);
            }
            return Ok(new { message = "Upload thành công!" });
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> Download(string fileName)
        {
            var url = await _ossService.GetDownloadUrlAsync(fileName);
            return Redirect(url);
        }
    }
}