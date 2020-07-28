#nullable enable
using System;
using System.Data.Common;
using System.Text.RegularExpressions;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Types;
using Platinum.Core.Types.Exceptions;

namespace Platinum.Core.Model
{
    public class OfferCategory
    {
        public string CategoryUrl { get; protected set; }
        public string CategoryName { get; protected set; }
        public OfferWebsite OfferWebsite { get; protected set; }

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
                    }
                }
            }

            this.OfferWebsite = offerWebsite;
        }

        /// <summary>
        /// Just for test purposes
        /// </summary>
        [Obsolete]
        public OfferCategory(OfferWebsite offerWebsite, string categoryName)
        {
#if RELEASE
            throw new Exception("Cannot invoke test methods on prod. env.");
#endif
            using (Dal db = new Dal(true))
            {
                this.CategoryUrl = categoryName;
                this.CategoryName = categoryName;
            }

            this.OfferWebsite = offerWebsite;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != typeof(OfferCategory))
            {
                return false;
            }

            OfferCategory convertedCategory = (OfferCategory) obj;

            return convertedCategory.OfferWebsite.Equals(OfferWebsite) &&
                   convertedCategory.CategoryUrl.Equals(CategoryUrl);
        }
    }
}