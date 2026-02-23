using Autodesk.Oss;
using Autodesk.Oss.Model;
using DesignAutomation.Services;

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

        public async Task<List<ObjectDetails>> GetFilesAsync()
        {
            var tokenResponse = await _tokenService.GetTokenAsync();
            var fileList = new List<ObjectDetails>();
            string nextToken = null;
            do
            {
                var objects = await _ossClient.GetObjectsAsync(_bucketKey, startAt: nextToken, accessToken: tokenResponse.access_token);
                if (objects.Items != null)
                {
                    foreach (var obj in objects.Items)
                    {
                        fileList.Add(new ObjectDetails
                        {
                            FileName = obj.ObjectKey,
                            Urn = Base64Encode(obj.ObjectId)
                        });
                    }
                }
                nextToken = objects.Next;
            } while (!string.IsNullOrEmpty(nextToken)); //nếu số lượng file trong bucket quá lớn, server sẽ trả về một nextToken. vòng lặp này đảm bảo code sẽ tiếp tục lấy dữ liệu cho đến khi không còn nextToken
            return fileList;
        }

        public async Task UploadFileAsync(string fileName, Stream stream)
        {
            var token = await _tokenService.GetTokenAsync();
            await _ossClient.UploadObjectAsync(_bucketKey, fileName, stream, accessToken: token.access_token);
        }

        public async Task<string> GetDownloadUrlAsync(string fileName)
        {
            var token = await _tokenService.GetTokenAsync();
            var createSignedResource = new CreateSignedResource { MinutesExpiration = 2 }; //tạo ra 1 đường dẫn tải xuống có hiệu lực trong 2 phút
            var response = await _ossClient.CreateSignedResourceAsync(_bucketKey, fileName, createSignedResource, access: Access.Read, accessToken: token.access_token);
            return response.SignedUrl;
        }

        private string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes).Replace("/", "_").Replace("+", "-").TrimEnd('=');
        }
    }

    public class ObjectDetails
    {
        public string FileName { get; set; }
        public string Urn { get; set; }
    }
}