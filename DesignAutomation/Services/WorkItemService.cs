using Autodesk.Forge.DesignAutomation;
using Autodesk.Forge.DesignAutomation.Model;
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

        public async Task<WorkItemStatus> CreateWorkItemAsync(WorkItemRequest request)
        {
            var token = await _tokenService.GetTokenAsync();

            //key "string" phải khớp với các giá trị trong activity
            //value "IArgument" là các tham số truyền vào (xrefTreeArgument là loại tham số truyền file)
            var workItem = new WorkItem()
            {
                ActivityId = request.activityId,
                Arguments = new Dictionary<string, IArgument>() 
                {
                    { "rvtFile", new XrefTreeArgument() {
                        Url = $"urn:adsk.objects:os.object:{request.bucketKey}/{request.inputObjectKey}", //địa chỉ định danh tệp tin trên Oss
                        Verb = Verb.Get,
                        Headers = new Dictionary<string, string>() { { "Authorization", "Bearer " + token.access_token } }
                    } },
                    { "resultFile", new XrefTreeArgument() {
                        Url = $"urn:adsk.objects:os.object:{request.bucketKey}/{request.resultObjectKey}",
                        Verb = Verb.Put,
                        Headers = new Dictionary<string, string>() { { "Authorization", "Bearer " + token.access_token } }
                    } }

                    /*{ "InputDwg", new XrefTreeArgument() {
                        Url = $"urn:adsk.objects:os.object:{request.bucketKey}/{request.inputObjectKey}", //địa chỉ định danh tệp tin trên Oss
                        Verb = Verb.Get,
                        Headers = new Dictionary<string, string>() { { "Authorization", "Bearer " + token.access_token } }
                    } },
                    { "result", new XrefTreeArgument() {
                        Url = $"urn:adsk.objects:os.object:{request.bucketKey}/{request.resultObjectKey}",
                        Verb = Verb.Put,
                        Headers = new Dictionary<string, string>() { { "Authorization", "Bearer " + token.access_token } }
                    } }*/
                }
            };

            return await _daClient.CreateWorkItemAsync(workItem);
        }

        public async Task<WorkItemStatus> GetWorkItemStatusAsync(string id)
        {
            return await _daClient.GetWorkitemStatusAsync(id);
        }
    }
}