namespace DesignAutomation.Models.Activity
{
    public class ActivityAliasRequest
    {
        public string id { get; set; }
        public int version { get; set; }
    }

    public class UpdateActivityAliasRequest
    {
        public int version { get; set; }
    }
}
