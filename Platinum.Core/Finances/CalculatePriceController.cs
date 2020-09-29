using Platinum.Core.Model;

namespace Platinum.Core.Finances
{
    public static class CalculatePriceController
    {
        public static decimal GetPrice(long offerCount, int paidTypeId)
        {
            PaidPlan plan = new PaidPlan(paidTypeId);
            return (offerCount * plan.PricePer1ProcessedOffer) + ((offerCount/1000.0M) * plan.PricePer1000OffersInDatabase);
        }
        
        public static decimal GetStoragePrice(long offerCount, int paidTypeId)
        {
            PaidPlan plan = new PaidPlan(paidTypeId);
            return ((offerCount/1000.0M) * plan.PricePer1000OffersInDatabase);
        }
        
        public static decimal GetProceedPrice(long offerCount, int paidTypeId)
        {
            PaidPlan plan = new PaidPlan(paidTypeId);
            return (offerCount * plan.PricePer1ProcessedOffer);
        }
    }
}