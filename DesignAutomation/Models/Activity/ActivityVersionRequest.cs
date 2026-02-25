using Autodesk.Forge.DesignAutomation.Model;

namespace DesignAutomation.Models.Activity
{
    public class ActivityVersionRequest
    {
        public string engine { get; set; }
        public List<string> appBundles { get; set; }
        public List<string> commandLine { get; set; }
        
        //public Dictionary<string, string> settings { get; set; } //dùng cho commandline khi làm việc với .dwg
        public Dictionary<string, ParameterRequest> parameters { get; set; }
    }
}
