using Autodesk.Oss;
using Autodesk.Oss.Model;
using ClosedXML.Excel;
using DesignAutomation.Models.WorkItem;

namespace DesignAutomation.Services
{
    public class OssService
    {
        private readonly string _bucketKey = "my-bucket-revit";
        private readonly TokenService _tokenService;
        private readonly OssClient _ossClient;

        public OssService(TokenService tokenService)
        {
            _tokenService = tokenService;
            _ossClient = new OssClient();
        }

        public async Task<List<ObjectDetails>> GetObjectsAsync()
        {
            var token = await _tokenService.GetTokenAsync();

            BucketObjects objects = await _ossClient.GetObjectsAsync(
                _bucketKey,
                accessToken: token.AccessToken
            );

            return objects.Items?.ToList() ?? new List<ObjectDetails>();
        }

        public async Task<ObjectDetails> UploadFileToOssAsync(string filePath, string objectKey, IProgress<int> progress = null)
        {
            var token = await _tokenService.GetTokenAsync();

            var response = await _ossClient.UploadObjectAsync(
                bucketKey: _bucketKey,
                objectKey: objectKey,
                sourceToUpload: filePath,
                accessToken: token.AccessToken,
                progress: progress
            );

            return response;
        }

        public async Task<Stream> DownloadFileAsync(string objectKey)
        {
            var token = await _tokenService.GetTokenAsync();

            return await _ossClient.DownloadObjectAsync(
                bucketKey: _bucketKey,
                objectKey: objectKey,
                accessToken: token.AccessToken
            );
        }

        public byte[] ExportToExcel(List<ElementDto> elements)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Revit Data");
                var headers = new HashSet<string> { "dbId", "Name" };
                foreach (var el in elements)
                {
                    if (el.Properties == null) continue;
                    foreach (var prop in el.Properties)
                        headers.Add(prop.DisplayName);
                }
                var headerList = headers.ToList();
                for (int i = 0; i < headerList.Count; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headerList[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                }
                int currentRow = 2;
                foreach (var el in elements)
                {
                    worksheet.Cell(currentRow, 1).Value = el.DbId;
                    worksheet.Cell(currentRow, 2).Value = el.Name;
                    if (el.Properties != null)
                    {
                        foreach (var prop in el.Properties)
                        {
                            int colIndex = headerList.IndexOf(prop.DisplayName) + 1;
                            worksheet.Cell(currentRow, colIndex).Value = prop.DisplayValue?.ToString();
                        }
                    }
                    currentRow++;
                }
                worksheet.Columns().AdjustToContents();
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}