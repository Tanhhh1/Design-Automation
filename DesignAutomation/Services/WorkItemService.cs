using Autodesk.Forge.DesignAutomation;
using Autodesk.Forge.DesignAutomation.Model;
using Autodesk.Oss.Model;
using DesignAutomation.Models.WorkItem;

namespace DesignAutomation.Services
{
    public class WorkItemService
    {
        private readonly DesignAutomationClient _daClient;
        private readonly TokenService _tokenService;

        public WorkItemService(DesignAutomationClient daClient, TokenService tokenService)
        {
            _daClient = daClient;
            _tokenService = tokenService;
        }

        public async Task<WorkItemResponse> CreateWorkItemAsync(WorkItemRequest request)
        {
            var token = await _tokenService.GetTokenAsync();

            //key "string" phải khớp với các giá trị trong activity
            //value "IArgument" là các tham số truyền vào (xrefTreeArgument là loại tham số truyền file)
            var workItem = new WorkItem()
            {
                ActivityId = request.activityId,
                Arguments = new Dictionary<string, IArgument>() 
                {
                    { "Input", new XrefTreeArgument() {
                        Url = $"urn:adsk.objects:os.object:{request.bucketKey}/{request.inputObjectKey}",
                        Verb = Verb.Get,
                        Headers = new Dictionary<string, string>() { { "Authorization", "Bearer " + token.AccessToken } }
                    } },
                    { "Output", new XrefTreeArgument() {
                        Url = $"urn:adsk.objects:os.object:{request.bucketKey}/{request.resultObjectKey}",
                        Verb = Verb.Put,
                        Headers = new Dictionary<string, string>() { { "Authorization", "Bearer " + token.AccessToken } }
                    } }
                }
            };
            var result = await _daClient.CreateWorkItemAsync(workItem);
            return new WorkItemResponse
            {
                Id = result.Id,
                Status = result.Status.ToString(),
                Progress = result.Progress,
                ReportUrl = result.ReportUrl,
            };
        }

        public async Task<WorkItemResponse> GetWorkItemStatusAsync(string id)
        {
            var result = await _daClient.GetWorkitemStatusAsync(id);
            return new WorkItemResponse
            {
                Id = result.Id,
                Status = result.Status.ToString(),
                Progress = result.Progress,
                ReportUrl = result.ReportUrl,
            };
        }
    }
}