using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Platinum.AdminPanel.Model;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.ElasticIntegration;
using Platinum.Core.Finances;
using Platinum.Core.Model;

namespace Platinum.AdminPanel
{
    public class FetchDataController
    {
        public List<FetchingRow> GetRows()
        {
            List<FetchingRow> ret = new List<FetchingRow>();
            using (Dal db = new Dal())
            {
                using (DbDataReader reader = db.ExecuteReader(@"
                    WITH Processed(UserId, ProcessedCount) as
                    (
	                    SELECT WebApiUserId, COUNT(*) from allegroUrlFetchTask where Processed = 0
	                    group by WebApiUserId

                    ),
                    NotProcessed(UserId, NotProcessedCount) as
                    (
	                    SELECT WebApiUserId, COUNT(*) from allegroUrlFetchTask where Processed <> 0
	                    group by WebApiUserId
                    )

                    select WebApiUsers.Id,WebApiUsers.Login, ISNULL(Processed.ProcessedCount,0), isnull(NotProcessed.NotProcessedCount,0) from allegroUrlFetchTask 
                    left JOIN Processed on Processed.UserId = WebApiUserId
                    left JOIN NotProcessed on NotProcessed.UserId = WebApiUserId
                    INNER JOIN WebApiUsers on WebApiUsers.Id = WebApiUserId
                    GROUP BY WebApiUsers.Id,WebApiUsers.Login, Processed.ProcessedCount,NotProcessed.NotProcessedCount
                "))
                {
                    int index = 0;

                    while (reader.Read())
                    {
                        ret.Add(new FetchingRow()
                        {
                            UserId = reader.GetInt32(0),
                            Login = reader.GetString(1),
                            InProcessTasks = reader.GetInt32(3),
                            WaitingTasks = reader.GetInt32(2),
                            RowId = index++
                        });
                    }
                }
            }

            return ret;
        }

        public List<FetchingProcessedOffersRow> GetOffers(int userId)
        {
            List<FetchingProcessedOffersRow> ret = new List<FetchingProcessedOffersRow>();
            using (Dal db = new Dal())
            {
                using (DbDataReader reader = db.ExecuteReader($@"
                    WITH notProcessed (offerId) as (
	                    SELECT offers.Id as offerId FROM offers where WebApiUserId = {userId} AND Processed = 0
                    ),
                    processed (offerId) as (
	                    SELECT offers.Id as offerId FROM offers where WebApiUserId = {userId} AND Processed <> 0
                    )
                    select offers.WebsiteCategoryId,
                    offers.WebApiUserId,
                    COUNT(notProcessed.offerId) as NotProcessedOffers,
                    COUNT(processed.offerId) as ProcessedOffers,
                    ISNULL(websiteCategories.name,''),
                    PaidPlan.Id
                    FROM WebApiUserWebsiteCategory
                    INNER JOIN offers on offers.WebApiUserId = WebApiUserWebsiteCategory.WebApiUserId
                    LEFT JOIN notProcessed on notProcessed.offerId = offers.Id
                    LEFT JOIN processed on processed.offerId = offers.Id
                    LEFT JOIN websiteCategories ON websiteCategories.Id = offers.WebsiteCategoryId
                    LEFT JOIN PaidPlan on PaidPlan.Id = WebApiUserWebsiteCategory.PaidPlanId
                    WHERE offers.WebApiUserId = {userId}
                    group by offers.WebsiteCategoryId, OFFERS.WebApiUserId, websiteCategories.name, PaidPlan.Id
                "))
                {
                    while (reader.Read())
                    {
                        PaidPlan plan = new PaidPlan(reader.GetInt32(5));
                        long offersThisMonth =
                            ElasticController.Instance.GetIndexDocumentCountThisMonth(reader.GetInt32(0), userId);
                        long offersAllTime =
                            ElasticController.Instance.GetIndexDocumentCount(reader.GetInt32(0), userId);
                        ret.Add(new FetchingProcessedOffersRow()
                        {
                            CategoryId   = reader.GetInt32(0),
                            ProcessedOffersSql   = reader.GetInt32(3),
                            NotProcessedOffersSql   = reader.GetInt32(2),
                            WebApiUserId   = reader.GetInt32(1),
                            CategoryName = reader.GetString(4),
                            PaidPlan = plan.Name,
                            OffersThisMonth = offersThisMonth,
                            MonthPay = Math.Round(CalculatePriceController.GetPrice(offersThisMonth,offersAllTime,reader.GetInt32(5)),2),
                            MaxOffersInPlan = plan.MaxOffersInDb,
                            ProcessedOffersElastic = ElasticController.Instance.GetIndexDocumentCount(reader.GetInt32(0), userId),
                            AllTimePay = Math.Round(CalculatePriceController.GetPrice(offersThisMonth,offersAllTime,reader.GetInt32(5)),2),
                            MaxOffersPerMonth = plan.MaxProceedOffersInMonth
                        });
                    }
                }
            }

            foreach (var rows in ret)
            {
                rows.ProcessedOffersElastic = ElasticController.Instance.GetIndexDocumentCount(rows.CategoryId,userId);
            }
            
            return ret;
        }
    }
}