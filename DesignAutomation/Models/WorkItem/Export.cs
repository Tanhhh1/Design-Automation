namespace DesignAutomation.Models.WorkItem
{
    public class PropertyDto
    {
        public string DisplayName { get; set; }
        public object DisplayValue { get; set; }
    }

    public class ElementDto
    {
        public int DbId { get; set; }
        public string Name { get; set; }
        public List<PropertyDto> Properties { get; set; }
    }
}
