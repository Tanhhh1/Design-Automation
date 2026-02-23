namespace DesignAutomation.Models.WorkItem
{
    public class UploadRequest
    {
        public string BucketKey { get; set; }
        public string ObjectKey { get; set; }
        public IFormFile File { get; set; }
    }
}
