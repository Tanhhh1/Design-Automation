using DesignAutomation.Models.WorkItem;
using DesignAutomation.Services;
using Microsoft.AspNetCore.Mvc;
using static DesignAutomation.Services.OssService;


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
            using (var stream = file.OpenReadStream()) //mở luồng đọc từ tệp tin được tải lên từ client
                await _ossService.UploadFileAsync(file.FileName, stream); //gọi service để tải tệp lên S3
            return Ok(new { message = "Upload thành công!" });
        }

        [HttpGet("upload-url")]
        public async Task<IActionResult> GetUploadUrl([FromQuery] string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return BadRequest("File không hợp lệ."); //kiểm tra fileName từ URL mà client gửi lên
            var url = await _ossService.GetUploadUrlAsync(fileName); //gọi service để tạo một đường dẫn (Signed URL) có quyền GHI (Write)
            return Ok(new { uploadUrl = url });
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> Download(string fileName)
        {
            var url = await _ossService.GetDownloadUrlAsync(fileName);
            return Redirect(url);
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