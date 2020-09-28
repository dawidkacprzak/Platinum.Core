using System;
using System.Data.Common;
using Platinum.Core.DatabaseIntegration;

namespace Platinum.Core.Model
{
    public class PaidPlan
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal PricePer1000OffersInDatabase { get; set; }
        public decimal PricePer1ProcessedOffer { get; set; }
        public int MaxOffersInDb { get; set; }
        public int MaxProceedOffersInMonth { get; set; }

        public PaidPlan(int Id)
        {
            using (Dal db = new Dal())
            {
                using(DbDataReader reader = db.ExecuteReader($"SELECT * FROM PaidPlan where Id = "+Id))
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            Id = reader.GetInt32(0);
                            Name = reader.GetString(1);
                            Description = reader.GetString(2);
                            PricePer1000OffersInDatabase = reader.GetDecimal(3);
                            PricePer1ProcessedOffer = reader.GetDecimal(4);
                            MaxOffersInDb = reader.GetInt32(5);
                            MaxProceedOffersInMonth = reader.GetInt32(6);
                        }
                        else
                        {
                            throw new Exception("Cannot load PaidPlan. Not found id: " + Id);
                        }
                    }
                }
            }
        }
    }
}