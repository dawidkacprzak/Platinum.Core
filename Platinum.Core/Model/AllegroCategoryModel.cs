using System.Diagnostics.CodeAnalysis;

namespace Platinum.Core.Model
{
    [ExcludeFromCodeCoverage]
    public class AllegroCategoryModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public bool leaf { get; set; }
    }
}