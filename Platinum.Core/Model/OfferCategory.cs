using System;
using System.Data.Common;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Types;
using Platinum.Core.Types.Exceptions;

namespace Platinum.Core.Model
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    public class OfferCategory
    {
        public string CategoryUrl { get; }
        public string CategoryName { get; }
        public OfferWebsite OfferWebsite { get; }
        public int CategoryId { get; }

        public OfferCategory(OfferWebsite offerWebsite, int categoryId)
        {
            using (Dal db = new Dal(true))
            {
                using (DbDataReader reader =
                    db.ExecuteReader($"SELECT TOP 1 * FROM websiteCategories WHERE id = {categoryId}"))
                {
                    if (!reader.HasRows)
                    {
                        throw new DalException("Category cannot be found");
                    }
                    else
                    {
                        reader.Read();

                        this.CategoryUrl = reader.GetString(reader.GetOrdinal("routeUrl"));
                        this.CategoryName = reader.GetString(reader.GetOrdinal("name"));
                        this.CategoryId = reader.GetInt32(reader.GetOrdinal("Id"));
                    }
                }
            }

            this.OfferWebsite = offerWebsite;
        }

        /// <summary>
        /// Just for test purposes
        /// </summary>
        public OfferCategory(OfferWebsite offerWebsite, string categoryName)
        {
#if RELEASE
            throw new Exception("Cannot invoke test methods on prod. env.");
#endif

            this.CategoryUrl = categoryName;
            this.CategoryName = categoryName;
            this.OfferWebsite = offerWebsite;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(OfferCategory))
            {
                return false;
            }

            OfferCategory convertedCategory = (OfferCategory) obj;

            return convertedCategory.OfferWebsite.Equals(OfferWebsite) &&
                   convertedCategory.CategoryUrl.Equals(CategoryUrl);
        }

        protected bool Equals(OfferCategory other)
        {
            return CategoryUrl == other.CategoryUrl && CategoryName == other.CategoryName &&
                   OfferWebsite == other.OfferWebsite && CategoryId == other.CategoryId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CategoryUrl, CategoryName, (int) OfferWebsite, CategoryId);
        }
    }
}