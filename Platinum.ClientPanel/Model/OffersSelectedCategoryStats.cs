using System.Collections.Generic;
using Platinum.Core.ElasticIntegration;
using Platinum.Core.Finances;
using Platinum.Core.Model;

namespace Platinum.ClientPanel.Model
{
    public class OffersSelectedCategoryStats
    {
        public long IndexedDocumentCount { get; set; }
        public long IndexedDocumentThisMonth { get; set; }
        public decimal IncomeForProceedOffersThisMonth { get; set; }
        public decimal IncomeForStorageThisMonth { get; set; }

        public int OffersRemains
        {
            get { return (int) PaidPlan.MaxOffersInDb - (int) IndexedDocumentCount; }
        }

        public int OffersRemainsThisMonth
        {
            get
            {
                int remains = PaidPlan.MaxProceedOffersInMonth - (int) IndexedDocumentThisMonth;
                if (remains > OffersRemains)
                {
                    return OffersRemains;
                }

                return remains;
            }
        }

        public decimal IncomeThisMonth { get; set; }
        public PaidPlan PaidPlan { get; set; }
        public WebApiUserWebsiteCategory webApiUserWebsiteCategory { get; set; }
        public List<string> FoundMappings { get; set; }
        public decimal DatabaseSize { get; set; }

        public OffersSelectedCategoryStats(int categoryId, int userId, int webApiUserWebsiteCategoryId)
        {
            webApiUserWebsiteCategory = new WebApiUserWebsiteCategory(webApiUserWebsiteCategoryId);
            PaidPlan = new PaidPlan(webApiUserWebsiteCategory.PaidPlanId);
            IndexedDocumentCount = ElasticController.Instance.GetIndexDocumentCount(categoryId, userId);
            IndexedDocumentThisMonth = ElasticController.Instance.GetIndexDocumentCountThisMonth(categoryId, userId);
            IncomeForStorageThisMonth =
                CalculatePriceController.GetStoragePrice(IndexedDocumentCount, webApiUserWebsiteCategory.PaidPlanId);
            IncomeForProceedOffersThisMonth =
                CalculatePriceController.GetProceedPrice(IndexedDocumentThisMonth,
                    webApiUserWebsiteCategory.PaidPlanId);
            IncomeThisMonth =
                CalculatePriceController.GetPrice(IndexedDocumentThisMonth, webApiUserWebsiteCategory.PaidPlanId);
            FoundMappings = ElasticController.Instance.GetIndexMappings(categoryId, userId);
        }
    }
}