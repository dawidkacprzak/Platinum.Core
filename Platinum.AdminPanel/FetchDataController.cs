using System.Collections.Generic;
using System.Data.Common;
using Platinum.AdminPanel.Model;
using Platinum.Core.DatabaseIntegration;

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
    }
}