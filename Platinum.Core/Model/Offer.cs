using System;

namespace Platinum.Core.Model
{
    public class Offer
    {
        public int Id { get; set; }
        public int WebsiteId { get; set; }
        public string Uri { get; set; }
        public byte[] UriHash  { get; set; }
        public DateTime CreatedDate { get; set; }
        public int WebsiteCategoryId { get; set; }
        
        public Offer(int id, int websiteId, string uri, byte[] uriHash, DateTime createdDate, int websiteCategoryId)
        {
            Id = id;
            WebsiteId = websiteId;
            Uri = uri;
            UriHash = uriHash;
            CreatedDate = createdDate;
            WebsiteCategoryId = websiteCategoryId;
        }
        
    }
}