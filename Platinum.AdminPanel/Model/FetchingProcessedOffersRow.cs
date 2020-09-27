namespace Platinum.AdminPanel.Model
{
    public class FetchingProcessedOffersRow
    {
        public int CategoryId { get; set; }
        public int WebApiUserId { get; set; }
        public int NotProcessedOffersSql { get; set; }
        public int ProcessedOffers { get; set; }
        public long ProcessedOffersElastic { get; set; }

        public string CategoryName { get; set; }
    }
}