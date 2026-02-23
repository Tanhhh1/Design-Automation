namespace DesignAutomation.Models.AppBundle
{
    public class AppBundleAliasRequest
    {
        public string id { get; set; }
        public int version { get; set; }
    }

    public class UpdateAliasRequest
    {
        public int version { get; set; }
    }
}
