namespace Platinum.Core.Model
{
    public class WebsiteCategoriesFilterSearch
    {
        public int Id { get; set; }
        public int WebsiteCategoryId { get; set; }
        public string Argument { get; set; }
        public string Value { get; set; }
        public int SearchNumber { get; set; }
    }
}