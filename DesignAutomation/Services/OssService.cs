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

        /*Large-file upload:
        client -> S3
        dùng cho file lớn lên đến 5GB (hoặc 5TB nếu dùng Multipart)*/
        public async Task<string> GetUploadUrlAsync(string fileName)
        {
            var token = await _tokenService.GetTokenAsync();
            var createSignedResource = new CreateSignedResource { MinutesExpiration = 10 }; //tạo ra 1 đường dẫn có hiệu lực trong 10 phút, cho phép client tải lên trực tiếp đến S3 mà không cần đi qua server
            var response = await _ossClient.CreateSignedResourceAsync(
                _bucketKey,
                fileName,
                createSignedResource,
                access: Access.Write,
                accessToken: token.AccessToken);

            return response.SignedUrl;
        }

        /*Server-side upload: 
        client -> server -> S3
        dùng cho file nhỏ*/        
        public async Task UploadFileAsync(string fileName, Stream stream)
        {
            var token = await _tokenService.GetTokenAsync();
            await _ossClient.UploadObjectAsync(_bucketKey, fileName, stream, accessToken: token.AccessToken); //nhận 1 Stream từ controller, dùng UploadObjectAsync để tải lên S3 trực tiếp từ server.
        }

        public async Task<string> GetDownloadUrlAsync(string fileName)
        {
            var token = await _tokenService.GetTokenAsync();
            var createSignedResource = new CreateSignedResource { MinutesExpiration = 2 }; //tạo ra 1 đường dẫn tải xuống có hiệu lực trong 2 phút
            var response = await _ossClient.CreateSignedResourceAsync(_bucketKey, fileName, createSignedResource, access: Access.Read, accessToken: token.AccessToken);
            return response.SignedUrl;
        }

        private string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes).Replace("/", "_").Replace("+", "-").TrimEnd('=');
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