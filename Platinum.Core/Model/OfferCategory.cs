#nullable enable
using System;
using System.Text.RegularExpressions;
using Platinum.Core.Types;

namespace Platinum.Core.Model
{
    public class OfferCategory
    {
        public OfferCategory BaseOfferCategory { get; }
        public string CategoryUrl { get; protected set; }

        public OfferWebsite OfferWebsite { get; protected set; }

        public OfferCategory(OfferWebsite offerWebsite, string categoryUrl)
        {
            this.CategoryUrl = categoryUrl;
            this.OfferWebsite = offerWebsite;
        }

        public OfferCategory(OfferWebsite offerWebsite, OfferCategory baseOfferCategory, string categoryUrl)
        {
            this.BaseOfferCategory = baseOfferCategory;
            this.CategoryUrl = categoryUrl;
            this.OfferWebsite = offerWebsite;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != typeof(OfferCategory))
            {
                return false;
            }

            OfferCategory convertedCategory = (OfferCategory) obj;
            
            if (BaseOfferCategory != null && !BaseOfferCategory.OfferWebsite.Equals(convertedCategory.OfferWebsite))
            {
                return false;
            }

            return convertedCategory.OfferWebsite.Equals(OfferWebsite) &&
                   convertedCategory.CategoryUrl.Equals(CategoryUrl);
        }
    }
}