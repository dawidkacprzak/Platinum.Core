using System;
using System.Collections.Generic;
using System.Data.Common;
using Platinum.Core.DatabaseIntegration;

namespace Platinum.Core.Model
{
    public class WebApiUserWebsiteCategory
    {
        public int Id { get; set; }
        public int WebApiUserId { get; set; }
        public int WebsiteCategoryId { get; set; }
        public int PaidPlanId { get; set; }

        public string CategoryName { get; set; }

        public WebApiUserWebsiteCategory(int Id)
        {
            using (Dal db = new Dal())
            {
                using (DbDataReader reader = db.ExecuteReader($@"
                    SELECT WebApiUserWebsiteCategory.Id,WebApiUserId,WebApiUserWebsiteCategory.WebsiteCategoryId,PaidPlanId,websiteCategories.Name FROM WebApiUserWebsiteCategory WITH(NOLOCK)
                    LEFT JOIN websiteCategories on websiteCategories.Id = WebApiUserWebsiteCategory.WebsiteCategoryId
                    WHERE WebApiUserWebsiteCategory.Id = {Id}
                "))
                {
                    reader.Read();
                    if (reader.HasRows)
                    {
                        this.Id = Id;
                        this.WebApiUserId = reader.GetInt32(1);
                        this.WebsiteCategoryId = reader.GetInt32(2);
                        this.PaidPlanId = reader.GetInt32(3);
                        CategoryName = reader.GetString(4);
                    }
                    else
                    {
                        throw new Exception("Błąd podczas pobierania kategorii użytkownika.");
                    }
                }
            }
        }

        public static IEnumerable<WebApiUserWebsiteCategory> GetAllUserCategories(int userId)
        {
            using (Dal db = new Dal())
            {
                using (DbDataReader reader =
                    db.ExecuteReader(
                        $@"SELECT Id from WebApiUserWebsiteCategory WITH (NOLOCK) where WebApiUserId = {userId}"))
                {
                    while (reader.Read())
                    {
                        yield return new WebApiUserWebsiteCategory(reader.GetInt32(0));
                    }
                }
            }
        }
    }
}