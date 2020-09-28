namespace Platinum.AdminPanel.Model
{
    public class FetchingProcessedOffersRow
    {
        public int CategoryId { get; set; }
        public int WebApiUserId { get; set; }
        public int NotProcessedOffersSql { get; set; }
        public int ProcessedOffersSql { get; set; }
        public long ProcessedOffersElastic { get; set; }

        public string CategoryName { get; set; }
        public string PaidPlan { get; set; }
        public long MaxOffersInPlan { get; set; }
        public long MaxOffersPerMonth { get; set; }
        public long OffersThisMonth { get; set; }
        public decimal MonthPay { get; set; }
        public decimal AllTimePay { get; set; }
    }
}