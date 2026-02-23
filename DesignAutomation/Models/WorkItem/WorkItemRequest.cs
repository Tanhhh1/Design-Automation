namespace DesignAutomation.Models.WorkItem
{
    public class WorkItemRequest
    {
        public string activityId { get; set; }
        public string bucketKey { get; set; }
        public string inputObjectKey { get; set; }
        public string resultObjectKey { get; set; }
    }
}
