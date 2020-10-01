namespace Platinum.Core.Model
{
    public class MappingApiElement
    {
        public string AttributeName { get; set; }
        public string Type { get; set; }
        public MappingApiElement(SimpleMapping simpleMapping)
        {
            if (string.IsNullOrEmpty(simpleMapping.Parent))
            {
                AttributeName = simpleMapping.Name;
            }
            else
            {
                AttributeName = simpleMapping.Parent+"."+simpleMapping.Name;
            }

            if (string.IsNullOrEmpty(simpleMapping.Type))
            {
                Type = "object";
            }
            else
            {
                Type = simpleMapping.Type;
            }
        }

        public override string ToString()
        {
            return AttributeName;
        }
    }
}