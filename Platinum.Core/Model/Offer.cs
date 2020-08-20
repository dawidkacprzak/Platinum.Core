using System;
using System.Data.Common;
using Platinum.Core.Types;
using Platinum.Core.Types.Exceptions;

namespace Platinum.Core.Model
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    public class Offer
    {
        public int Id { get; set; }
        public int WebsiteId { get; set; }
        public string Uri { get; set; }
        public byte[] UriHash { get; set; }
        public DateTime CreatedDate { get; set; }
        public int WebsiteCategoryId { get; set; }
        public int Processed { get; set; }

        public Offer()
        {
#if RELEASE
throw new Exception("Cannot create empty offer in production");
#endif
            this.Id = 0;
        }

        public Offer(int id, int websiteId, string uri, byte[] uriHash, DateTime createdDate, int websiteCategoryId)
        {
            Id = id;
            WebsiteId = websiteId;
            Uri = uri;
            UriHash = uriHash;
            CreatedDate = createdDate;
            WebsiteCategoryId = websiteCategoryId;
        }

        public Offer(int id, int websiteId, string uri, byte[] uriHash, DateTime createdDate, int websiteCategoryId,
            int processed)
        {
            Id = id;
            WebsiteId = websiteId;
            Uri = uri;
            UriHash = uriHash;
            CreatedDate = createdDate;
            WebsiteCategoryId = websiteCategoryId;
            Processed = processed;
        }

        public Offer(IDal dal, int id)
        {
            using (DbDataReader reader = dal.ExecuteReader(
                $" SELECT Id,WebsiteId,UriHash,CreatedDate,WebsiteCategoryId,Uri,Processed FROM offers WITH(NOLOCK) where Id = {id}")
            )
            {
                if (!reader.HasRows)
                {
                    throw new DalException("Not found category with id: " + id);
                }

                while (reader.Read())
                {
                    this.Id = reader.GetInt32(0);
                    this.WebsiteId = reader.GetInt32(1);
                    this.UriHash = (byte[]) reader.GetValue(2);
                    this.CreatedDate = reader.GetDateTime(3);
                    this.WebsiteCategoryId = reader.GetInt32(4);
                    this.Uri = reader.GetString(5);
                    this.Processed = reader.GetInt32(6);
                }
            }
        }
    }
}