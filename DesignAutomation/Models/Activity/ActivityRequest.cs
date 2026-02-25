using Autodesk.Forge.DesignAutomation.Model;

namespace DesignAutomation.Models.Activity
{
    public class ActivityRequest
    {
        public string id { get; set; }
        public string engine { get; set; }
        public List<string> appBundles { get; set; }
        public List<string> commandLine { get; set; }

        //public Dictionary<string, string> settings { get; set; } //dùng cho commandline khi làm việc với .dwg
        public Dictionary<string, ParameterRequest> parameters { get; set; }
    }

    public class ParameterRequest
    {
        public bool Zip { get; set; }
        public string LocalName { get; set; } //phải giống với localname trong file .zip được tải lên appbundle
        public bool Ondemand { get; set; }
        public string Verb { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
    }
}
