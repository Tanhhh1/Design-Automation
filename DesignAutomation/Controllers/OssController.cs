using DesignAutomation.Models.WorkItem;
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
            var files = await _ossService.GetObjectsAsync();
            return Ok(files);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var tempPath = Path.GetTempFileName();
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            var result = await _ossService.UploadFileToOssAsync(tempPath, file.FileName);
            return Ok(new { message = "Upload thành công!", objectId = result.ObjectId });
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> Download(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return BadRequest("Tên file không hợp lệ.");
            var stream = await _ossService.DownloadFileAsync(fileName);
            return new FileStreamResult(stream, "application/octet-stream") { FileDownloadName = fileName };
        }

        [HttpPost("excel")]
        public IActionResult ExportExcel([FromBody] List<ElementDto> elements)
        {
            if (elements == null || !elements.Any()) return BadRequest("Không có dữ liệu.");

            var fileBytes = _ossService.ExportToExcel(elements);
            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "BIM_Export.xlsx");
        }
    }
}