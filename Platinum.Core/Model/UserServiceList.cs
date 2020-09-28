namespace Platinum.Core.Model
{
    public class UserServiceList
    {
        public int CategoryId { get; set; }
        public string Category { get; set; }
        public long ProcessedOffersMonth { get; set; }
        public long ProcessedOffersAllTime { get; set; }
        public string PaidPlanName { get; set; }
        public int PaidPlanId { get; set; }
        public decimal Income { get; set; }
    }
}