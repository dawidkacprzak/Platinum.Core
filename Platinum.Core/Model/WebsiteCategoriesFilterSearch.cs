namespace Platinum.Core.Model
{
    
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    public class WebsiteCategoriesFilterSearch
    {
        public int Id { get; set; }
        public int WebsiteCategoryId { get; set; }
        public string Argument { get; set; }
        public string Value { get; set; }
        public int SearchNumber { get; set; }
        public int TaskId { get; set; }
    }
}