using Autodesk.Forge.DesignAutomation;
using Autodesk.Forge.DesignAutomation.Model;
using DesignAutomation.Models.AppBundle;
using System.Net.Http.Headers;

namespace DesignAutomation.Services
{
    public class AppBundleService
    {
        private readonly DesignAutomationClient _daClient;
        private readonly IHttpClientFactory _httpClientFactory; //dùng để tạo HttpClient, giúp gửi yêu cầu HTTP

        public AppBundleService(DesignAutomationClient daClient, IHttpClientFactory httpClientFactory)
        {
            _daClient = daClient;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<AppBundle> CreateAppBundleAsync(AppBundleRequest request) 
        {
            var appBundle = new AppBundle
            {
                Id = request.id,
                Engine = request.engine,
                Description = request.description
            };
            return await _daClient.CreateAppBundleAsync(appBundle);
        }

        public async Task<AppBundle> CreateVersionAsync(string id, string engine, IFormFile fileZip)
        {
            var newVersion = new AppBundle { Engine = engine };
            var versionResponse = await _daClient.CreateAppBundleVersionAsync(id, newVersion); //gọi API của Autodesk để tạo phiên bản mới
            await UploadStreamToS3Async(versionResponse.UploadParameters, fileZip); //gọi hàm để tải tệp zip lên S3
            return versionResponse;
        }

        private async Task UploadStreamToS3Async(UploadAppBundleParameters upload, IFormFile fileZip)
        {
            using var client = _httpClientFactory.CreateClient(); //tạo HttpClient để gửi yêu cầu HTTP
            using var formData = new MultipartFormDataContent(); //tạo 1 đối tượng để chứa dữ liệu biểu mẫu đa phần
            foreach (var kv in upload.FormData) //lặp qua từng cặp khóa-giá trị trong FormData
            {
                formData.Add(new StringContent(kv.Value), kv.Key); //thêm từng cặp khóa-giá trị vào formData
            }
            var fileStream = fileZip.OpenReadStream(); //mở luồng đọc từ tệp zip được tải lên
            var streamContent = new StreamContent(fileStream); //chuyển luồng thành nội dung có thể gửi đi
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream"); //đặt kiểu nội dung là nhị phân
            formData.Add(streamContent, "file", fileZip.FileName);
            await client.PostAsync(upload.EndpointURL, formData);
        }

        public async Task<Alias> CreateAliasAsync(string id, AppBundleAliasRequest request)
        {
            var alias = new Alias
            {
                Id = request.id,
                Version = request.version
            };
            return await _daClient.CreateAppBundleAliasAsync(id, alias);
        }

        public async Task UpdateAliasAsync(string id, string aliasId, UpdateAliasRequest request)
        {
            var aliasUpdate = new AliasPatch { Version = request.version };
            await _daClient.ModifyAppBundleAliasAsync(id, aliasId, aliasUpdate);
        }
    }
}