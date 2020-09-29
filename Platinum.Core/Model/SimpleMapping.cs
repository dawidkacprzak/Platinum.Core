namespace Platinum.Core.Model
{
    public class SimpleMapping
    {
        public string Parent { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public SimpleMapping(string name)
        {
            this.Name = name;
        }
        
        public SimpleMapping(string name, string parent)
        {
            this.Name = name;
            this.Parent = parent;
        }
        public SimpleMapping(string name, string parent,string type)
        {
            this.Name = name;
            this.Parent = parent;
            this.Type = type;
        }
    }
}