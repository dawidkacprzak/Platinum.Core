using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.ElasticIntegration;
using Platinum.Core.Model;

namespace Platinum.Core.Finances
{
    public class UserServicesController
    {
        public static async Task<List<UserServiceList>> GetUserServicePanelData(int userId)
        {
            List<UserServiceList> ret = new List<UserServiceList>();
            using (Dal db = new Dal())
            {
                using (DbDataReader reader = db.ExecuteReader($@"
                    SELECT wauwc.WebsiteCategoryId, wc.name, pp.Name, pp.Id as PaidPlanId FROM WebApiUserWebsiteCategory wauwc WITH(NOLOCK)
                    LEFT JOIN PaidPlan pp on pp.Id = wauwc.PaidPlanId
                    LEFT JOIN websiteCategories wc on wc.Id = wauwc.WebsiteCategoryId
                    where wauwc.WebApiUserId = {userId}
                "))
                {
                    while (reader.Read())
                    {
                        ret.Add(new UserServiceList()
                        {
                            CategoryId = reader.GetInt32(0),
                            Category = reader.GetString(1),
                            PaidPlanName = reader.GetString(2),
                            PaidPlanId =  reader.GetInt32(3)
                        });
                    }

                    for (int i = 0;i<ret.Count;i++)
                    {
                        long thisMonthDocuments =
                            ElasticController.Instance.GetIndexDocumentCountThisMonth(ret[i].CategoryId, userId);
                        ret[i].Income = Finances.CalculatePriceController.GetPrice(
                           thisMonthDocuments,ret[i].PaidPlanId);
                        ret[i].ProcessedOffersMonth = thisMonthDocuments;
                        ret[i].ProcessedOffersAllTime =
                            ElasticController.Instance.GetIndexDocumentCount(ret[i].CategoryId, userId);
                    }
                }
            }

            return ret;
        }
    }
}